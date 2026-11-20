using ApiWeb.Data;
using ApiWeb.DTOs;
using ApiWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/atencionesclinicas")]
public sealed class AtencionesClinicasController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AtencionClinicaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AtencionClinicaDto>>> Listar(CancellationToken ct)
    {
        var registros = await db.AtencionesClinicas.AsNoTracking().OrderByDescending(a => a.Fecha)
            .ToListAsync(ct);
        return Ok(registros.Select(Map).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AtencionClinicaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AtencionClinicaDto>> Obtener(Guid id, CancellationToken ct)
    {
        var a = await db.AtencionesClinicas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return a is null ? NotFound() : Ok(Map(a));
    }

    [HttpGet("{id:guid}/tratamientos")]
    [ProducesResponseType(typeof(List<TratamientoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TratamientoDto>>> Tratamientos(Guid id, CancellationToken ct)
    {
        if (!await db.AtencionesClinicas.AnyAsync(a => a.Id == id, ct)) return NotFound();
        var tratamientos = await db.Tratamientos.AsNoTracking()
            .Where(t => t.AtencionClinicaId == id).OrderBy(t => t.Inicio).ToListAsync(ct);
        return Ok(tratamientos.Select(TratamientosController.Map).ToList());
    }

    [HttpPost]
    [ProducesResponseType(typeof(AtencionClinicaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AtencionClinicaDto>> Crear(AtencionClinicaCrearDto dto,
        CancellationToken ct)
    {
        var error = Validar(dto.Fecha, dto.Tipo, dto.Detalle);
        if (error is not null) return BadRequest(error);
        var ficha = await db.FichasBovinas.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == dto.FichaBovinaId, ct);
        if (ficha is null || ficha.FechaBaja is not null)
            return BadRequest("La ficha bovina no existe o está dada de baja.");
        if (!await ClinicaValidaciones.VeterinarioValidoAsync(db, dto.CuentaVeterinarioId,
                ficha.GranjaId, ct))
            return BadRequest("La cuenta debe estar activa y vinculada como veterinario a la granja.");
        var a = new AtencionClinica
        {
            FichaBovinaId = dto.FichaBovinaId,
            CuentaVeterinarioId = dto.CuentaVeterinarioId
        };
        Aplicar(a, dto.Fecha, dto.Tipo, dto.Detalle);
        db.AtencionesClinicas.Add(a);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Obtener), new { id = a.Id }, Map(a));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(Guid id, AtencionClinicaActualizarDto dto,
        CancellationToken ct)
    {
        var a = await db.AtencionesClinicas.FindAsync([id], ct);
        if (a is null) return NotFound();
        if (a.Anulado) return Conflict("La atención está anulada.");
        var error = Validar(dto.Fecha, dto.Tipo, dto.Detalle);
        if (error is not null) return BadRequest(error);
        Aplicar(a, dto.Fecha, dto.Tipo, dto.Detalle);
        a.Version++;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Anular(Guid id, CancellationToken ct)
    {
        var a = await db.AtencionesClinicas.FindAsync([id], ct);
        if (a is null) return NotFound();
        if (!a.Anulado)
        {
            if (await db.Tratamientos.AnyAsync(t => t.AtencionClinicaId == id && !t.Anulado, ct))
                return Conflict("Anule primero los tratamientos activos de esta atención.");
            a.Anulado = true;
            a.Version++;
            await db.SaveChangesAsync(ct);
        }
        return NoContent();
    }

    internal static AtencionClinicaDto Map(AtencionClinica a) => new(a.Id,
        a.FichaBovinaId, a.CuentaVeterinarioId, a.Fecha, a.Tipo, a.Detalle,
        a.Anulado, a.Version);

    private static string? Validar(DateTime fecha, TipoAtencion tipo, string detalle)
    {
        if (!ClinicaValidaciones.FechaValida(fecha)) return "La fecha es obligatoria.";
        if (!Enum.IsDefined(tipo)) return "El tipo de atención no es válido.";
        if (string.IsNullOrWhiteSpace(detalle)) return "El detalle es obligatorio.";
        return null;
    }

    private static void Aplicar(AtencionClinica a, DateTime fecha, TipoAtencion tipo,
        string detalle)
    {
        a.Fecha = fecha;
        a.Tipo = tipo;
        a.Detalle = detalle.Trim();
    }
}
