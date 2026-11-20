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
    public async Task<ActionResult<IReadOnlyList<AplicacionDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await db.Aplicaciones.AsNoTracking().OrderByDescending(x => x.FechaHoraReal)
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
        var validated = await ValidarAplicacion(dto, null, cancellationToken);
        if (validated.Error is not null) return validated.Error;
        var now = DateTimesVetLink.ToUtc(dto.FechaHoraReal);
        var aplicacion = new Aplicacion
        {
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
        if (validated.Prevencion is not null)
        {
            validated.Prevencion.Estado = EstadoPrevencion.Realizada;
            validated.Prevencion.FechaRealizacion = now;
            validated.Prevencion.Version++;
        }
        db.Aplicaciones.Add(aplicacion);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = aplicacion.Id }, ToDto(aplicacion));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, ActualizarAplicacionDto dto, CancellationToken cancellationToken)
    {
        var aplicacion = await db.Aplicaciones.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (aplicacion is null) return NotFound();
        if (aplicacion.Anulado) return BadRequest(new { error = "No se puede modificar una aplicación anulada." });

        var validated = await ValidarAplicacion(dto, id, cancellationToken);
        if (validated.Error is not null) return validated.Error;
        var oldPrevencionId = aplicacion.PrevencionId;
        var oldPrevencion = oldPrevencionId is null
            ? null
            : await db.Prevenciones.SingleOrDefaultAsync(x => x.Id == oldPrevencionId, cancellationToken);

        aplicacion.FichaBovinaId = dto.FichaBovinaId;
        aplicacion.CuentaTrabajadorId = dto.CuentaTrabajadorId;
        aplicacion.TratamientoId = dto.TratamientoId;
        aplicacion.PrevencionId = dto.PrevencionId;
        aplicacion.FechaHoraReal = DateTimesVetLink.ToUtc(dto.FechaHoraReal);
        aplicacion.Dosis = dto.Dosis;
        aplicacion.Unidad = dto.Unidad.Trim();
        aplicacion.Observacion = dto.Observacion?.Trim();
        aplicacion.Tipo = validated.Tipo!;
        aplicacion.VersionIndicacion = validated.VersionIndicacion;
        aplicacion.Version++;

        if (oldPrevencion is not null && oldPrevencion.Id != dto.PrevencionId)
        {
            var quedanAplicaciones = await db.Aplicaciones.AnyAsync(x => x.Id != id &&
                x.PrevencionId == oldPrevencion.Id && !x.Anulado, cancellationToken);
            if (!quedanAplicaciones && oldPrevencion.Estado != EstadoPrevencion.Cancelada)
            {
                oldPrevencion.Estado = EstadoPrevencion.Pendiente;
                oldPrevencion.FechaRealizacion = null;
                oldPrevencion.Version++;
            }
        }
        if (validated.Prevencion is not null)
        {
            validated.Prevencion.Estado = EstadoPrevencion.Realizada;
            validated.Prevencion.FechaRealizacion = aplicacion.FechaHoraReal;
            validated.Prevencion.Version++;
        }
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
            aplicacion.Anulado = true;
            aplicacion.Version++;
            if (aplicacion.PrevencionId is Guid prevencionId)
            {
                var prevencion = await db.Prevenciones.SingleOrDefaultAsync(x => x.Id == prevencionId, cancellationToken);
                var quedanAplicaciones = await db.Aplicaciones.AnyAsync(x => x.Id != id &&
                    x.PrevencionId == prevencionId && !x.Anulado, cancellationToken);
                if (prevencion is not null && !quedanAplicaciones && prevencion.Estado != EstadoPrevencion.Cancelada)
                {
                    prevencion.Estado = EstadoPrevencion.Pendiente;
                    prevencion.FechaRealizacion = null;
                    prevencion.Version++;
                }
            }
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
        if (!await db.Cuentas.AnyAsync(x => x.Id == dto.CuentaTrabajadorId && x.Activa, cancellationToken))
            return ValidacionAplicacion.ErrorResult("La cuenta del trabajador no existe o está inactiva.");
        var puedeAplicar = await db.Vinculaciones.AnyAsync(x => x.CuentaId == dto.CuentaTrabajadorId &&
            x.GranjaId == ficha.GranjaId && x.Activa &&
            (x.Rol == RolesVetLink.Trabajador || x.Rol == RolesVetLink.Veterinario), cancellationToken);
        if (!puedeAplicar)
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
