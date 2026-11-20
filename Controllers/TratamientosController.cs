using ApiWeb.Data;
using ApiWeb.DTOs;
using ApiWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/tratamientos")]
public sealed class TratamientosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<TratamientoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TratamientoDto>>> Listar(CancellationToken ct)
    {
        var registros = await db.Tratamientos.AsNoTracking().OrderByDescending(t => t.Inicio)
            .ToListAsync(ct);
        return Ok(registros.Select(Map).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TratamientoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TratamientoDto>> Obtener(Guid id, CancellationToken ct)
    {
        var t = await db.Tratamientos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return t is null ? NotFound() : Ok(Map(t));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TratamientoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TratamientoDto>> Crear(TratamientoCrearDto dto,
        CancellationToken ct)
    {
        var error = Validar(dto.Medicamento, dto.Dosis, dto.Unidad, dto.Via,
            dto.Frecuencia, dto.Inicio, dto.Duracion, dto.FechaFin);
        if (error is not null) return BadRequest(error);
        var atencion = await db.AtencionesClinicas.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == dto.AtencionClinicaId, ct);
        if (atencion is null || atencion.Anulado || atencion.FichaBovinaId != dto.FichaBovinaId)
            return BadRequest("La atención debe existir, estar vigente y corresponder al mismo animal.");
        var ficha = await db.FichasBovinas.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == dto.FichaBovinaId, ct);
        if (ficha is null || ficha.FechaBaja is not null)
            return BadRequest("La ficha bovina no existe o está dada de baja.");
        if (!await ClinicaValidaciones.VeterinarioValidoAsync(db, dto.CuentaVeterinarioId,
                ficha.GranjaId, ct))
            return BadRequest("La cuenta debe estar activa y vinculada como veterinario a la granja.");
        var t = new Tratamiento
        {
            FichaBovinaId = dto.FichaBovinaId,
            AtencionClinicaId = dto.AtencionClinicaId,
            CuentaVeterinarioId = dto.CuentaVeterinarioId
        };
        Aplicar(t, dto.Medicamento, dto.Dosis, dto.Unidad, dto.Via,
            dto.Frecuencia, dto.Inicio, dto.Duracion, dto.FechaFin, dto.MotivoFin);
        db.Tratamientos.Add(t);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Obtener), new { id = t.Id }, Map(t));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(Guid id, TratamientoActualizarDto dto,
        CancellationToken ct)
    {
        var t = await db.Tratamientos.FindAsync([id], ct);
        if (t is null) return NotFound();
        if (t.Anulado) return Conflict("El tratamiento está anulado.");
        var error = Validar(dto.Medicamento, dto.Dosis, dto.Unidad, dto.Via,
            dto.Frecuencia, dto.Inicio, dto.Duracion, dto.FechaFin);
        if (error is not null) return BadRequest(error);
        Aplicar(t, dto.Medicamento, dto.Dosis, dto.Unidad, dto.Via,
            dto.Frecuencia, dto.Inicio, dto.Duracion, dto.FechaFin, dto.MotivoFin);
        t.Version++;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(Guid id, CancellationToken ct)
    {
        var t = await db.Tratamientos.FindAsync([id], ct);
        if (t is null) return NotFound();
        if (!t.Anulado)
        {
            t.Anulado = true;
            t.Version++;
            await db.SaveChangesAsync(ct);
        }
        return NoContent();
    }

    internal static TratamientoDto Map(Tratamiento t) => new(t.Id, t.FichaBovinaId,
        t.AtencionClinicaId, t.CuentaVeterinarioId, t.Medicamento, t.Dosis,
        t.Unidad, t.Via, t.Frecuencia, t.Inicio, t.Duracion, t.FechaFin,
        t.MotivoFin, t.Anulado, t.Version);

    private static string? Validar(string medicamento, decimal dosis, string unidad,
        string via, string frecuencia, DateOnly inicio, string duracion, DateOnly? fin)
    {
        if (string.IsNullOrWhiteSpace(medicamento) || string.IsNullOrWhiteSpace(unidad) ||
            string.IsNullOrWhiteSpace(via) || string.IsNullOrWhiteSpace(frecuencia) ||
            string.IsNullOrWhiteSpace(duracion)) return "Complete los datos obligatorios del tratamiento.";
        if (dosis <= 0) return "La dosis debe ser positiva.";
        if (inicio == default) return "La fecha de inicio es obligatoria.";
        if (fin < inicio) return "La fecha de fin no puede ser anterior al inicio.";
        return null;
    }

    private static void Aplicar(Tratamiento t, string medicamento, decimal dosis,
        string unidad, string via, string frecuencia, DateOnly inicio, string duracion,
        DateOnly? fin, string? motivoFin)
    {
        t.Medicamento = medicamento.Trim();
        t.Dosis = dosis;
        t.Unidad = unidad.Trim();
        t.Via = via.Trim();
        t.Frecuencia = frecuencia.Trim();
        t.Inicio = inicio;
        t.Duracion = duracion.Trim();
        t.FechaFin = fin;
        t.MotivoFin = motivoFin?.Trim();
    }
}
