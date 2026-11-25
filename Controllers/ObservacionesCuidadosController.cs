using System.Text.Json;
using ApiWeb.Data;
using ApiWeb.DTOs;
using ApiWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

// 1. RF-05: observaciones y cuidados del trabajador; las correcciones conservan el dato anterior y el motivo.
[ApiController]
[Route("api/observacionescuidados")]
public sealed class ObservacionesCuidadosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ObservacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ObservacionDto>>> Listar([FromQuery] Guid? fichaBovinaId,
        CancellationToken ct) =>
        Ok(await db.ObservacionesCuidados.AsNoTracking()
            .Where(x => fichaBovinaId == null || x.FichaBovinaId == fichaBovinaId)
            .OrderByDescending(x => x.FechaHora)
            .Select(x => new ObservacionDto(x.Id, x.FichaBovinaId, x.RegistradoPor, x.FechaHora,
                x.Observado, x.CuidadoRealizado, x.Anulado, x.Version))
            .ToListAsync(ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ObservacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ObservacionDto>> Obtener(Guid id, CancellationToken ct)
    {
        var o = await db.ObservacionesCuidados.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return o is null ? NotFound() : Ok(Map(o));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ObservacionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ObservacionDto>> Crear(CrearObservacionDto dto, CancellationToken ct)
    {
        // RF-07: si el registro ya llegó en un envío anterior, se devuelve el mismo.
        if (dto.Id is Guid previo && previo != Guid.Empty &&
            await db.ObservacionesCuidados.AsNoTracking().FirstOrDefaultAsync(x => x.Id == previo, ct) is { } existente)
            return Ok(Map(existente));
        var fecha = DateTimesVetLink.ToUtc(dto.FechaHora == default ? DateTime.UtcNow : dto.FechaHora);
        if (fecha > DateTime.UtcNow.AddMinutes(5))
            return BadRequest(new { error = "La fecha de observación no puede estar en el futuro." });
        var o = new ObservacionCuidado
        {
            Id = dto.Id is Guid nuevo && nuevo != Guid.Empty ? nuevo : Guid.NewGuid(),
            FichaBovinaId = dto.FichaBovinaId,
            RegistradoPor = User.CuentaId(),
            FechaHora = fecha,
            Observado = dto.Observado.Trim(),
            CuidadoRealizado = dto.CuidadoRealizado.Trim(),
        };
        db.ObservacionesCuidados.Add(o);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Obtener), new { id = o.Id }, Map(o));
    }

    // 2. Corregir: guarda la versión anterior con el motivo y luego actualiza.
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Corregir(Guid id, CorregirObservacionDto dto, CancellationToken ct)
    {
        var o = await db.ObservacionesCuidados.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return NotFound();
        if (o.Anulado) return BadRequest(new { error = "No se puede corregir una observación anulada." });
        if (dto.Version is int vista && vista != o.Version)
            return Conflict(new { error = "El registro cambió mientras lo corregías. Revisá la versión actual antes de volver a guardar." });
        db.CorreccionesCuidados.Add(new CorreccionCuidado
        {
            FichaBovinaId = o.FichaBovinaId,
            RegistroTipo = "observacion",
            RegistroId = o.Id,
            DatosAnteriores = JsonSerializer.Serialize(new { o.FechaHora, o.Observado, o.CuidadoRealizado, o.Version }),
            Motivo = dto.Motivo.Trim(),
            CorregidoPor = User.CuentaId(),
        });
        o.FechaHora = DateTimesVetLink.ToUtc(dto.FechaHora);
        o.Observado = dto.Observado.Trim();
        o.CuidadoRealizado = dto.CuidadoRealizado.Trim();
        o.Version++;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // 3. Anular: el registro queda como antecedente.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(Guid id, CancellationToken ct)
    {
        var o = await db.ObservacionesCuidados.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return NotFound();
        if (!o.Anulado)
        {
            o.Anulado = true;
            o.Version++;
            await db.SaveChangesAsync(ct);
        }
        return NoContent();
    }

    // 4. Historial de correcciones de una aplicación u observación.
    [HttpGet("/api/correcciones")]
    [ProducesResponseType(typeof(IReadOnlyList<CorreccionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CorreccionDto>>> Correcciones([FromQuery] Guid? registroId,
        [FromQuery] Guid? fichaBovinaId, CancellationToken ct) =>
        Ok(await db.CorreccionesCuidados.AsNoTracking()
            .Where(x => (registroId == null || x.RegistroId == registroId) &&
                        (fichaBovinaId == null || x.FichaBovinaId == fichaBovinaId))
            .OrderByDescending(x => x.CorregidoEn)
            .Select(x => new CorreccionDto(x.Id, x.FichaBovinaId, x.RegistroTipo, x.RegistroId,
                x.DatosAnteriores, x.Motivo, x.CorregidoPor, x.CorregidoEn))
            .ToListAsync(ct));

    private static ObservacionDto Map(ObservacionCuidado x) => new(x.Id, x.FichaBovinaId, x.RegistradoPor,
        x.FechaHora, x.Observado, x.CuidadoRealizado, x.Anulado, x.Version);
}
