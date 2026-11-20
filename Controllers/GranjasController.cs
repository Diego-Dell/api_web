using ApiWeb.Data;
using ApiWeb.DTOs;
using ApiWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/granjas")]
public sealed class GranjasController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<GranjaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GranjaDto>>> Listar(CancellationToken ct)
    {
        var granjas = await db.Granjas.AsNoTracking().OrderBy(g => g.Nombre)
            .Select(g => new GranjaDto(g.Id, g.Nombre, g.Hectareas, g.Trabajadores,
                g.DocumentosTerreno, g.RUNSA, g.Confirmada)).ToListAsync(ct);
        return Ok(granjas);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GranjaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GranjaDto>> Obtener(Guid id, CancellationToken ct)
    {
        var g = await db.Granjas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return g is null ? NotFound() : Ok(Map(g));
    }

    [HttpGet("{id:guid}/fichasbovinas")]
    [ProducesResponseType(typeof(List<FichaBovinaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<FichaBovinaDto>>> Fichas(Guid id, CancellationToken ct)
    {
        if (!await db.Granjas.AnyAsync(g => g.Id == id, ct)) return NotFound();
        var fichas = await db.FichasBovinas.AsNoTracking().Where(f => f.GranjaId == id)
            .OrderBy(f => f.Identificacion).ToListAsync(ct);
        return Ok(fichas.Select(FichasBovinasController.Map).ToList());
    }

    [HttpPost]
    [ProducesResponseType(typeof(GranjaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GranjaDto>> Crear(GranjaGuardarDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest("El nombre es obligatorio.");
        var g = new Granja();
        Aplicar(g, dto);
        db.Granjas.Add(g);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Obtener), new { id = g.Id }, Map(g));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(Guid id, GranjaGuardarDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest("El nombre es obligatorio.");
        var g = await db.Granjas.FindAsync([id], ct);
        if (g is null) return NotFound();
        Aplicar(g, dto);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        var g = await db.Granjas.FindAsync([id], ct);
        if (g is null) return NotFound();
        if (await db.FichasBovinas.AnyAsync(f => f.GranjaId == id, ct) ||
            await db.Vinculaciones.AnyAsync(v => v.GranjaId == id, ct) ||
            await db.Invitaciones.AnyAsync(i => i.GranjaId == id, ct))
            return Conflict("La granja tiene registros relacionados y no puede eliminarse.");
        db.Granjas.Remove(g);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Conflict("La granja tiene registros relacionados."); }
        return NoContent();
    }

    private static GranjaDto Map(Granja g) => new(g.Id, g.Nombre, g.Hectareas,
        g.Trabajadores, g.DocumentosTerreno, g.RUNSA, g.Confirmada);

    private static void Aplicar(Granja g, GranjaGuardarDto dto)
    {
        g.Nombre = dto.Nombre.Trim();
        g.Hectareas = dto.Hectareas;
        g.Trabajadores = dto.Trabajadores;
        g.DocumentosTerreno = dto.DocumentosTerreno?.Trim();
        g.RUNSA = dto.RUNSA?.Trim();
        g.Confirmada = dto.Confirmada;
    }
}
