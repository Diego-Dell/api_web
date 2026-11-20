using ApiWeb.Data;
using ApiWeb.Integrante1_Fichas.Servicios;
using ApiWeb.Integrante4_CuidadosAplicaciones.Dtos;
using ApiWeb.Integrante4_CuidadosAplicaciones.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Integrante4_CuidadosAplicaciones.Controllers;

[ApiController]
[Authorize]
[Route("api/cuidados")]
[Produces("application/json")]
public sealed class CuidadosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly FichasServicio _fichas;

    public CuidadosController(AppDbContext db, FichasServicio fichas)
    {
        _db = db;
        _fichas = fichas;
    }

    [HttpGet("animal/{fichaId:guid}")]
    [ProducesResponseType(typeof(List<CuidadoDto>), 200)]
    public async Task<IActionResult> ListarPorAnimal(Guid fichaId, [FromQuery] bool conAnulados = false)
    {
        var ficha = await _db.FichasGanado.AsNoTracking().FirstOrDefaultAsync(f => f.Id == fichaId);
        if (ficha is null || await Rol(ficha.GranjaId) is null) return NotFound();
        var q = _db.ObservacionesCuidados.AsNoTracking().Where(c => c.FichaId == fichaId);
        if (!conAnulados) q = q.Where(c => !c.Anulado);
        return Ok((await q.OrderByDescending(c => c.FechaHora).ToListAsync()).Select(CuidadoDto.Desde));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CuidadoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Obtener(Guid id)
    {
        var cuidado = await _db.ObservacionesCuidados.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (cuidado is null || !await PuedeLeer(cuidado.FichaId)) return NotFound();
        return Ok(CuidadoDto.Desde(cuidado));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CuidadoDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarCuidadoDto dto)
    {
        if (dto.FichaId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Observacion) ||
            string.IsNullOrWhiteSpace(dto.AccionRealizada))
            return BadRequest(new { error = "Ficha, observación y acción realizada son obligatorias." });

        var (ficha, rol, cuenta) = await Acceso(dto.FichaId);
        if (ficha is null) return NotFound(new { error = "Ficha no encontrada." });
        if (rol is null || cuenta is null) return Forbid();
        if (rol is not (FichasServicio.Dueno or FichasServicio.Trabajador))
            return StatusCode(403, new { error = "Tu rol no permite registrar cuidados." });
        if (ficha.FechaBaja is not null) return Conflict(new { error = "El animal está dado de baja." });

        var fecha = dto.FechaHora?.ToUniversalTime() ?? DateTime.UtcNow;
        if (fecha > DateTime.UtcNow.AddMinutes(5))
            return BadRequest(new { error = "La fecha no puede ser futura." });

        var cuidado = new ObservacionCuidado
        {
            FichaId = dto.FichaId,
            RegistradoPor = cuenta.Value,
            FechaHora = fecha,
            Observacion = dto.Observacion.Trim(),
            AccionRealizada = dto.AccionRealizada.Trim()
        };
        _db.ObservacionesCuidados.Add(cuidado);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Obtener), new { id = cuidado.Id }, CuidadoDto.Desde(cuidado));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CuidadoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Corregir(Guid id, [FromBody] EditarCuidadoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Observacion) || string.IsNullOrWhiteSpace(dto.AccionRealizada) ||
            string.IsNullOrWhiteSpace(dto.MotivoCorreccion))
            return BadRequest(new { error = "Observación, acción y motivo son obligatorios." });
        var cuidado = await _db.ObservacionesCuidados.FirstOrDefaultAsync(c => c.Id == id);
        if (cuidado is null) return NotFound();
        var (_, rol, cuenta) = await Acceso(cuidado.FichaId);
        if (rol is null || cuenta is null) return NotFound();
        if (rol is not (FichasServicio.Dueno or FichasServicio.Trabajador))
            return StatusCode(403, new { error = "Tu rol no permite corregir cuidados." });
        if (cuidado.Anulado) return Conflict(new { error = "El cuidado está anulado." });

        var nuevaObservacion = dto.Observacion.Trim();
        var nuevaAccion = dto.AccionRealizada.Trim();
        if (cuidado.Observacion == nuevaObservacion && cuidado.AccionRealizada == nuevaAccion)
            return BadRequest(new { error = "No hay cambios para guardar." });
        if (cuidado.Observacion != nuevaObservacion)
        {
            Auditar(cuidado, "observacion", cuidado.Observacion, nuevaObservacion, dto.MotivoCorreccion, cuenta.Value);
            cuidado.Observacion = nuevaObservacion;
        }
        if (cuidado.AccionRealizada != nuevaAccion)
        {
            Auditar(cuidado, "accion_realizada", cuidado.AccionRealizada, nuevaAccion, dto.MotivoCorreccion, cuenta.Value);
            cuidado.AccionRealizada = nuevaAccion;
        }
        await _db.SaveChangesAsync();
        return Ok(CuidadoDto.Desde(cuidado));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Anular(Guid id, [FromQuery] string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo)) return BadRequest(new { error = "El motivo es obligatorio." });
        var cuidado = await _db.ObservacionesCuidados.FirstOrDefaultAsync(c => c.Id == id);
        if (cuidado is null) return NotFound();
        var (_, rol, cuenta) = await Acceso(cuidado.FichaId);
        if (rol is null || cuenta is null) return NotFound();
        if (rol is not (FichasServicio.Dueno or FichasServicio.Trabajador)) return Forbid();
        if (cuidado.Anulado) return NotFound();
        Auditar(cuidado, "anulado", "false", "true", motivo, cuenta.Value);
        cuidado.Anulado = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:guid}/correcciones")]
    public async Task<IActionResult> Historial(Guid id)
    {
        var cuidado = await _db.ObservacionesCuidados.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (cuidado is null || !await PuedeLeer(cuidado.FichaId)) return NotFound();
        var lista = await _db.CorreccionesHistorial.AsNoTracking()
            .Where(c => c.Tabla == "cuidados" && c.RegistroId == id)
            .OrderBy(c => c.FechaHora).ToListAsync();
        return Ok(lista.Select(CorreccionDto.Desde));
    }

    private void Auditar(ObservacionCuidado c, string campo, string? anterior, string? nuevo,
        string motivo, Guid cuenta) => _db.CorreccionesHistorial.Add(new CorreccionHistorial
        {
            Tabla = "cuidados", RegistroId = c.Id, Campo = campo, ValorAnterior = anterior,
            ValorNuevo = nuevo, Motivo = motivo.Trim(), CorregidoPor = cuenta, FechaHora = DateTime.UtcNow
        });

    private async Task<bool> PuedeLeer(Guid fichaId)
    {
        var ficha = await _db.FichasGanado.AsNoTracking().FirstOrDefaultAsync(f => f.Id == fichaId);
        return ficha is not null && await Rol(ficha.GranjaId) is not null;
    }

    private async Task<(ApiWeb.Integrante1_Fichas.Models.FichaGanado? ficha, string? rol, Guid? cuenta)> Acceso(Guid fichaId)
    {
        var ficha = await _db.FichasGanado.AsNoTracking().FirstOrDefaultAsync(f => f.Id == fichaId);
        if (ficha is null) return (null, null, null);
        var sub = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(sub, out var cuenta)) return (ficha, null, null);
        return (ficha, await _fichas.RolEnGranja(cuenta, ficha.GranjaId), cuenta);
    }

    private async Task<string?> Rol(Guid granjaId)
    {
        var sub = User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var cuenta) ? await _fichas.RolEnGranja(cuenta, granjaId) : null;
    }
}
