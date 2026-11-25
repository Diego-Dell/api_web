using ApiWeb.Data;
using ApiWeb.DTOs;
using ApiWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/aplicaciones")]
public sealed class AplicacionesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AplicacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AplicacionDto>>> GetAll([FromQuery] Guid? fichaBovinaId,
        CancellationToken cancellationToken) =>
        // 1. Con fichaBovinaId lista solo las aplicaciones de ese animal.
        Ok(await db.Aplicaciones.AsNoTracking()
            .Where(x => fichaBovinaId == null || x.FichaBovinaId == fichaBovinaId)
            .OrderByDescending(x => x.FechaHoraReal)
            .Select(ToProjection()).ToListAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AplicacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AplicacionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var aplicacion = await db.Aplicaciones.AsNoTracking().Where(x => x.Id == id)
            .Select(ToProjection()).SingleOrDefaultAsync(cancellationToken);
        return aplicacion is null ? NotFound() : Ok(aplicacion);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AplicacionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AplicacionDto>> Create(CrearAplicacionDto dto, CancellationToken cancellationToken)
    {
        // 2. Quien aplica es siempre la persona del token.
        // RF-07: si el registro ya llegó en un envío anterior, se devuelve el mismo.
        if (dto.Id is Guid previo && previo != Guid.Empty &&
            await db.Aplicaciones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == previo, cancellationToken) is { } existente)
            return Ok(ToDto(existente));
        dto.CuentaTrabajadorId = User.CuentaId();
        var validated = await ValidarAplicacion(dto, null, cancellationToken);
        if (validated.Error is not null) return validated.Error;
        var now = DateTimesVetLink.ToUtc(dto.FechaHoraReal);
        var aplicacion = new Aplicacion
        {
            Id = dto.Id is Guid nuevo && nuevo != Guid.Empty ? nuevo : Guid.NewGuid(),
            FichaBovinaId = dto.FichaBovinaId,
            CuentaTrabajadorId = dto.CuentaTrabajadorId,
            TratamientoId = dto.TratamientoId,
            PrevencionId = dto.PrevencionId,
            FechaHoraReal = now,
            Dosis = dto.Dosis,
            Unidad = dto.Unidad.Trim(),
            Observacion = dto.Observacion?.Trim(),
            Tipo = validated.Tipo!,
            VersionIndicacion = validated.VersionIndicacion,
            Version = 1
        };
        // 3. La vacuna o desparasitación pasa a Realizada en la base (trigger prevencion_segun_aplicaciones).
        db.Aplicaciones.Add(aplicacion);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = aplicacion.Id }, ToDto(aplicacion));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    // 4. Corregir una aplicación: guarda los datos anteriores con el motivo (RF-05) y luego actualiza.
    public async Task<IActionResult> Update(Guid id, CorregirAplicacionDto dto, CancellationToken cancellationToken)
    {
        var aplicacion = await db.Aplicaciones.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (aplicacion is null) return NotFound();
        if (aplicacion.Anulado) return BadRequest(new { error = "No se puede modificar una aplicación anulada." });
        if (dto.Version is int vista && vista != aplicacion.Version)
            return Conflict(new { error = "El registro cambió mientras lo corregías. Revisá la versión actual antes de volver a guardar." });
        var fecha = DateTimesVetLink.ToUtc(dto.FechaHoraReal);
        if (fecha == default || fecha > DateTime.UtcNow.AddMinutes(5))
            return BadRequest(new { error = "La fecha real es obligatoria y no puede estar en el futuro." });

        db.CorreccionesCuidados.Add(new CorreccionCuidado
        {
            FichaBovinaId = aplicacion.FichaBovinaId,
            RegistroTipo = "aplicacion",
            RegistroId = aplicacion.Id,
            DatosAnteriores = System.Text.Json.JsonSerializer.Serialize(new
            {
                aplicacion.FechaHoraReal, aplicacion.Dosis, aplicacion.Unidad, aplicacion.Observacion, aplicacion.Version,
            }),
            Motivo = dto.Motivo.Trim(),
            CorregidoPor = User.CuentaId(),
        });
        aplicacion.FechaHoraReal = fecha;
        aplicacion.Dosis = dto.Dosis;
        aplicacion.Unidad = dto.Unidad.Trim();
        aplicacion.Observacion = dto.Observacion?.Trim();
        aplicacion.Version++;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var aplicacion = await db.Aplicaciones.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (aplicacion is null) return NotFound();
        if (!aplicacion.Anulado)
        {
            // 5. Si era la única aplicación de una prevención, la base la vuelve a Pendiente.
            aplicacion.Anulado = true;
            aplicacion.Version++;
            await db.SaveChangesAsync(cancellationToken);
        }
        return NoContent();
    }

    private async Task<ValidacionAplicacion> ValidarAplicacion(CrearAplicacionDto dto, Guid? aplicacionId,
        CancellationToken cancellationToken)
    {
        if ((dto.TratamientoId is null) == (dto.PrevencionId is null))
            return ValidacionAplicacion.ErrorResult("La aplicación debe referenciar exactamente un Tratamiento o una Prevención.");
        if (dto.FechaHoraReal == default || DateTimesVetLink.ToUtc(dto.FechaHoraReal) > DateTime.UtcNow.AddMinutes(5))
            return ValidacionAplicacion.ErrorResult("La fecha real es obligatoria y no puede estar en el futuro.");
        if (dto.Dosis <= 0) return ValidacionAplicacion.ErrorResult("La dosis debe ser positiva.");

        var ficha = await db.FichasBovinas.AsNoTracking()
            .Where(x => x.Id == dto.FichaBovinaId)
            .Select(x => new { x.GranjaId, x.FechaBaja })
            .SingleOrDefaultAsync(cancellationToken);
        if (ficha is null) return ValidacionAplicacion.ErrorResult("La ficha bovina indicada no existe.");
        if (ficha.FechaBaja is not null) return ValidacionAplicacion.ErrorResult("No se puede registrar una aplicación para una ficha dada de baja.");
        var administrador = Guid.TryParse(User.FindFirst("sub")?.Value, out var actor) &&
            await ClinicaValidaciones.EsAdministradorAsync(db, actor, cancellationToken);
        if (!administrador && !await db.Cuentas.AnyAsync(x => x.Id == dto.CuentaTrabajadorId && x.Activa, cancellationToken))
            return ValidacionAplicacion.ErrorResult("La cuenta del trabajador no existe o está inactiva.");
        var puedeAplicar = await db.Vinculaciones.AnyAsync(x => x.CuentaId == dto.CuentaTrabajadorId &&
            x.GranjaId == ficha.GranjaId && x.Activa &&
            (x.Rol == RolesVetLink.Trabajador || x.Rol == RolesVetLink.Veterinario), cancellationToken);
        if (!administrador && !puedeAplicar)
            return ValidacionAplicacion.ErrorResult("La cuenta necesita una vinculación activa como Trabajador o Veterinario en la granja de la ficha.");

        if (dto.TratamientoId is Guid tratamientoId)
        {
            var tratamiento = await db.Tratamientos.AsNoTracking()
                .Where(x => x.Id == tratamientoId)
                .Select(x => new { x.FichaBovinaId, x.Anulado, x.Version })
                .SingleOrDefaultAsync(cancellationToken);
            if (tratamiento is null) return ValidacionAplicacion.ErrorResult("El tratamiento indicado no existe.");
            if (tratamiento.FichaBovinaId != dto.FichaBovinaId)
                return ValidacionAplicacion.ErrorResult("El tratamiento no pertenece a la ficha bovina indicada.");
            if (tratamiento.Anulado) return ValidacionAplicacion.ErrorResult("No se puede aplicar un tratamiento anulado.");
            return new ValidacionAplicacion(null, null, "Tratamiento", tratamiento.Version);
        }

        var prevencion = await db.Prevenciones.SingleOrDefaultAsync(x => x.Id == dto.PrevencionId, cancellationToken);
        if (prevencion is null) return ValidacionAplicacion.ErrorResult("La prevención indicada no existe.");
        if (prevencion.FichaBovinaId != dto.FichaBovinaId)
            return ValidacionAplicacion.ErrorResult("La prevención no pertenece a la ficha bovina indicada.");
        if (prevencion.Estado == EstadoPrevencion.Cancelada)
            return ValidacionAplicacion.ErrorResult("No se puede aplicar una prevención cancelada.");
        // El diagrama permite varias aplicaciones por prevención (relación 1 a muchos).
        return new ValidacionAplicacion(null, prevencion, "Prevencion", prevencion.Version);
    }

    private sealed record ValidacionAplicacion(ActionResult? Error, Prevencion? Prevencion, string? Tipo, int VersionIndicacion)
    {
        public static ValidacionAplicacion ErrorResult(string mensaje) =>
            new(new BadRequestObjectResult(new { error = mensaje }), null, null, 0);
    }

    private static System.Linq.Expressions.Expression<Func<Aplicacion, AplicacionDto>> ToProjection() =>
        x => new AplicacionDto(x.Id, x.FichaBovinaId, x.CuentaTrabajadorId, x.TratamientoId,
            x.PrevencionId, x.FechaHoraReal, x.Dosis, x.Unidad, x.Observacion, x.Anulado,
            x.Tipo, x.VersionIndicacion, x.Version);

    private static AplicacionDto ToDto(Aplicacion x) => new(x.Id, x.FichaBovinaId, x.CuentaTrabajadorId,
        x.TratamientoId, x.PrevencionId, x.FechaHoraReal, x.Dosis, x.Unidad, x.Observacion,
        x.Anulado, x.Tipo, x.VersionIndicacion, x.Version);
}
