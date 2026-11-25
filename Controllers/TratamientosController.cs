using ApiWeb.Data;
using ApiWeb.DTOs;
using ApiWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using System.Text.Json;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/tratamientos")]
public sealed class TratamientosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<TratamientoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TratamientoDto>>> Listar([FromQuery] Guid? fichaBovinaId, CancellationToken ct)
    {
        // 1. Con fichaBovinaId lista solo las indicaciones de ese animal.
        var registros = await db.Tratamientos.AsNoTracking()
            .Where(t => fichaBovinaId == null || t.FichaBovinaId == fichaBovinaId)
            .OrderByDescending(t => t.Inicio)
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
            dto.Frecuencia, dto.Inicio, dto.Duracion, null);
        if (error is not null) return BadRequest(new { error });
        // RF-07: un reintento del celular devuelve la misma indicación vigente en vez de duplicarla.
        var veterinario = User.CuentaId();
        var previa = await db.Tratamientos.AsNoTracking().FirstOrDefaultAsync(x =>
            x.FichaBovinaId == dto.FichaBovinaId && x.AtencionClinicaId == dto.AtencionClinicaId &&
            x.CuentaVeterinarioId == veterinario && x.Medicamento == dto.Medicamento.Trim() &&
            x.Inicio == dto.Inicio && x.FechaFin == null && !x.Anulado, ct);
        if (previa is not null) return Ok(Map(previa));

        // 2. La función de Supabase valida veterinario activo, atención del mismo animal y datos.
        var id = await GuardarIndicacion(dto.FichaBovinaId, dto.AtencionClinicaId, dto.Medicamento, dto.Dosis,
            dto.Unidad, dto.Via, dto.Frecuencia, dto.Inicio, dto.Duracion, dto.Retiro, null, null, null, null, ct);
        var t = await db.Tratamientos.AsNoTracking().FirstAsync(x => x.Id == id, ct);
        return CreatedAtAction(nameof(Obtener), new { id }, Map(t));
    }

    // 3. Cierra la indicación vigente (con su versión, fecha y motivo) y crea la nueva; la anterior se conserva.
    [HttpPost("{id:guid}/reemplazar")]
    [ProducesResponseType(typeof(TratamientoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TratamientoDto>> Reemplazar(Guid id, TratamientoReemplazarDto dto,
        CancellationToken ct)
    {
        var anterior = await db.Tratamientos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (anterior is null) return NotFound();
        var error = Validar(dto.Medicamento, dto.Dosis, dto.Unidad, dto.Via,
            dto.Frecuencia, dto.Inicio, dto.Duracion, null);
        if (error is not null) return BadRequest(new { error });
        var nueva = await GuardarIndicacion(anterior.FichaBovinaId, anterior.AtencionClinicaId, dto.Medicamento,
            dto.Dosis, dto.Unidad, dto.Via, dto.Frecuencia, dto.Inicio, dto.Duracion, dto.Retiro,
            id, dto.Version, dto.FechaFinAnterior, dto.MotivoFinAnterior, ct);
        var t = await db.Tratamientos.AsNoTracking().FirstAsync(x => x.Id == nueva, ct);
        return CreatedAtAction(nameof(Obtener), new { id = nueva }, Map(t));
    }

    // 4. Finaliza la indicación con fecha y motivo; queda como antecedente.
    [HttpPost("{id:guid}/finalizar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Finalizar(Guid id, TratamientoFinalizarDto dto, CancellationToken ct)
    {
        await db.LlamarAsync("select public.finalizar_indicacion_tratamiento(@id, @version, @fin, @motivo)", ct,
            ("id", id, NpgsqlDbType.Uuid), ("version", dto.Version, NpgsqlDbType.Integer),
            ("fin", dto.FechaFin, NpgsqlDbType.Date), ("motivo", dto.MotivoFin, NpgsqlDbType.Text));
        return NoContent();
    }

    private async Task<Guid> GuardarIndicacion(Guid ficha, Guid atencion, string medicamento, decimal dosis,
        string unidad, string via, string frecuencia, DateOnly inicio, string duracion, string? retiro,
        Guid? anterior, int? version, DateOnly? fechaFin, string? motivo, CancellationToken ct)
    {
        var datos = JsonSerializer.Serialize(new
        {
            medicamento, dosis, unidad = IndicacionesVetLink.UnidadValida(unidad) ?? unidad, via, frecuencia,
            inicio = inicio.ToString("yyyy-MM-dd"), duracion, retiro,
        });
        var resultado = await db.LlamarAsync(
            "select public.guardar_indicacion_tratamiento(@ficha, @atencion, @datos, @anterior, @version, @fin, @motivo)", ct,
            ("ficha", ficha, NpgsqlDbType.Uuid), ("atencion", atencion, NpgsqlDbType.Uuid),
            ("datos", datos, NpgsqlDbType.Jsonb), ("anterior", anterior, NpgsqlDbType.Uuid),
            ("version", version, NpgsqlDbType.Integer), ("fin", fechaFin, NpgsqlDbType.Date),
            ("motivo", motivo, NpgsqlDbType.Text));
        return (Guid)resultado!;
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
        t.MotivoFin, t.Anulado, t.Version, t.Retiro, t.AnteriorId);

    private static string? Validar(string medicamento, decimal dosis, string unidad,
        string via, string frecuencia, DateOnly inicio, string duracion, DateOnly? fin)
    {
        if (string.IsNullOrWhiteSpace(medicamento) || string.IsNullOrWhiteSpace(unidad) ||
            string.IsNullOrWhiteSpace(via) || string.IsNullOrWhiteSpace(frecuencia) ||
            string.IsNullOrWhiteSpace(duracion)) return "Complete los datos obligatorios del tratamiento.";
        if (dosis <= 0) return "La dosis debe ser positiva.";
        // F1/F4: unidad de la lista y "Cada"/"Duración" en formato que permita calcular el total de dosis.
        if (IndicacionesVetLink.UnidadValida(unidad) is null)
            return $"La unidad debe ser una de: {string.Join(", ", IndicacionesVetLink.Unidades)}.";
        if (!IndicacionesVetLink.FormatoCada(frecuencia)) return "Indicá cada cuántas horas, por ejemplo \"24 h\".";
        if (!IndicacionesVetLink.FormatoDuracion(duracion)) return "Indicá la duración en días, por ejemplo \"5 días\".";
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
        t.Unidad = IndicacionesVetLink.UnidadValida(unidad) ?? unidad.Trim();
        t.Via = via.Trim();
        t.Frecuencia = frecuencia.Trim();
        t.Inicio = inicio;
        t.Duracion = duracion.Trim();
        t.FechaFin = fin;
        t.MotivoFin = motivoFin?.Trim();
    }
}
