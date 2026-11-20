using ApiWeb.Data;
using ApiWeb.Integrante1_Fichas.Servicios;
using ApiWeb.Integrante4_CuidadosAplicaciones.Dtos;
using ApiWeb.Integrante4_CuidadosAplicaciones.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ApiWeb.Integrante4_CuidadosAplicaciones.Controllers;

[ApiController]
[Authorize]
[Route("api/aplicaciones")]
[Produces("application/json")]
public sealed class AplicacionesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly FichasServicio _fichas;

    public AplicacionesController(AppDbContext db, FichasServicio fichas)
    {
        _db = db;
        _fichas = fichas;
    }

    [HttpGet("animal/{fichaId:guid}")]
    [ProducesResponseType(typeof(List<AplicacionDto>), 200)]
    public async Task<IActionResult> ListarPorAnimal(Guid fichaId, [FromQuery] bool conAnuladas = false)
    {
        if (!await PuedeLeer(fichaId)) return NotFound();
        var q = _db.AplicacionesIndicaciones.AsNoTracking().Where(a => a.FichaId == fichaId);
        if (!conAnuladas) q = q.Where(a => !a.Anulado);
        var lista = await q.OrderByDescending(a => a.FechaHoraReal).ToListAsync();
        return Ok(lista.Select(AplicacionDto.Desde));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AplicacionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Obtener(Guid id)
    {
        var a = await _db.AplicacionesIndicaciones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (a is null || !await PuedeLeer(a.FichaId)) return NotFound();
        return Ok(AplicacionDto.Desde(a));
    }

    [HttpPost]
    [ProducesResponseType(typeof(AplicacionDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarAplicacionDto dto)
    {
        if (dto.FichaId == Guid.Empty || dto.Dosis <= 0 || string.IsNullOrWhiteSpace(dto.Unidad) ||
            (dto.TratamientoId.HasValue == dto.PrevencionId.HasValue))
            return BadRequest(new { error = "Indica ficha, dosis positiva, unidad y exactamente un tratamiento o prevención." });

        var ficha = await _db.FichasGanado.AsNoTracking().FirstOrDefaultAsync(f => f.Id == dto.FichaId);
        if (ficha is null) return NotFound(new { error = "Ficha no encontrada." });
        var cuenta = CuentaActual();
        if (cuenta is null || await _fichas.RolEnGranja(cuenta.Value, ficha.GranjaId) is null)
            return Forbid();
        if (ficha.FechaBaja is not null) return Conflict(new { error = "El animal está dado de baja." });

        if (dto.TratamientoId is { } tratamientoId)
        {
            var existe = await _db.Database.SqlQuery<bool>($"select exists(select 1 from public.tratamientos where id={tratamientoId} and ficha_id={dto.FichaId} and not anulado) as \"Value\"").SingleAsync();
            if (!existe) return BadRequest(new { error = "El tratamiento no existe, está anulado o corresponde a otro animal." });
        }
        if (dto.PrevencionId is { } prevencionId)
        {
            var existe = await _db.Database.SqlQuery<bool>($"select exists(select 1 from public.prevenciones where id={prevencionId} and ficha_id={dto.FichaId} and estado='pendiente'::public.estado_prevencion) as \"Value\"").SingleAsync();
            if (!existe) return BadRequest(new { error = "La prevención no existe, ya se realizó o corresponde a otro animal." });
        }

        var fecha = dto.FechaHoraReal?.ToUniversalTime() ?? DateTime.UtcNow;
        if (fecha > DateTime.UtcNow.AddMinutes(5))
            return BadRequest(new { error = "La fecha real no puede ser futura." });

        var a = new AplicacionIndicacion
        {
            FichaId = dto.FichaId, AplicadoPor = cuenta.Value,
            TratamientoId = dto.TratamientoId, PrevencionId = dto.PrevencionId,
            FechaHoraReal = fecha, Dosis = dto.Dosis, Unidad = dto.Unidad.Trim(),
            Observacion = Limpio(dto.Observacion)
        };
        _db.AplicacionesIndicaciones.Add(a);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation or PostgresErrorCodes.CheckViolation })
        {
            return BadRequest(new { error = "La indicación o la ficha ya no es válida." });
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { error = "La prevención ya tiene una aplicación activa." });
        }
        return CreatedAtAction(nameof(Obtener), new { id = a.Id }, AplicacionDto.Desde(a));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AplicacionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Corregir(Guid id, [FromBody] EditarAplicacionDto dto)
    {
        if (dto.Dosis <= 0 || string.IsNullOrWhiteSpace(dto.Unidad) ||
            string.IsNullOrWhiteSpace(dto.MotivoCorreccion))
            return BadRequest(new { error = "Dosis positiva, unidad y motivo son obligatorios." });
        var a = await _db.AplicacionesIndicaciones.FirstOrDefaultAsync(x => x.Id == id);
        if (a is null || !await PuedeLeer(a.FichaId)) return NotFound();
        var cuenta = CuentaActual();
        if (cuenta is null) return Forbid();
        if (a.Anulado) return Conflict(new { error = "La aplicación está anulada." });

        var unidad = dto.Unidad.Trim();
        var observacion = Limpio(dto.Observacion);
        if (a.Dosis == dto.Dosis && a.Unidad == unidad && a.Observacion == observacion)
            return BadRequest(new { error = "No hay cambios para guardar." });
        if (a.Dosis != dto.Dosis)
        {
            Auditar(a, "dosis", a.Dosis?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                dto.Dosis.ToString(System.Globalization.CultureInfo.InvariantCulture), dto.MotivoCorreccion, cuenta.Value);
            a.Dosis = dto.Dosis;
        }
        if (a.Unidad != unidad)
        {
            Auditar(a, "unidad", a.Unidad, unidad, dto.MotivoCorreccion, cuenta.Value);
            a.Unidad = unidad;
        }
        if (a.Observacion != observacion)
        {
            Auditar(a, "observacion", a.Observacion, observacion, dto.MotivoCorreccion, cuenta.Value);
            a.Observacion = observacion;
        }
        await _db.SaveChangesAsync();
        return Ok(AplicacionDto.Desde(a));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Anular(Guid id, [FromQuery] string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo)) return BadRequest(new { error = "El motivo es obligatorio." });
        var a = await _db.AplicacionesIndicaciones.FirstOrDefaultAsync(x => x.Id == id);
        if (a is null || !await PuedeLeer(a.FichaId) || a.Anulado) return NotFound();
        var cuenta = CuentaActual();
        if (cuenta is null) return Forbid();
        Auditar(a, "anulado", "false", "true", motivo, cuenta.Value);
        a.Anulado = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:guid}/correcciones")]
    public async Task<IActionResult> Historial(Guid id)
    {
        var a = await _db.AplicacionesIndicaciones.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (a is null || !await PuedeLeer(a.FichaId)) return NotFound();
        var lista = await _db.CorreccionesHistorial.AsNoTracking()
            .Where(c => c.Tabla == "aplicaciones" && c.RegistroId == id)
            .OrderBy(c => c.FechaHora).ToListAsync();
        return Ok(lista.Select(CorreccionDto.Desde));
    }

    private void Auditar(AplicacionIndicacion a, string campo, string? anterior, string? nuevo,
        string motivo, Guid cuenta) => _db.CorreccionesHistorial.Add(new CorreccionHistorial
        {
            Tabla = "aplicaciones", RegistroId = a.Id, Campo = campo,
            ValorAnterior = anterior, ValorNuevo = nuevo, Motivo = motivo.Trim(),
            CorregidoPor = cuenta, FechaHora = DateTime.UtcNow
        });

    private Guid? CuentaActual() =>
        Guid.TryParse(User.FindFirst("sub")?.Value, out var cuenta) ? cuenta : null;

    private async Task<bool> PuedeLeer(Guid fichaId)
    {
        var ficha = await _db.FichasGanado.AsNoTracking().FirstOrDefaultAsync(f => f.Id == fichaId);
        var cuenta = CuentaActual();
        return ficha is not null && cuenta is not null &&
            await _fichas.RolEnGranja(cuenta.Value, ficha.GranjaId) is not null;
    }

    private static string? Limpio(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
