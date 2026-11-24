using ApiWeb.Data;
using ApiWeb.DTOs;
using ApiWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/fichasbovinas")]
public sealed class FichasBovinasController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<FichaBovinaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FichaBovinaDto>>> Listar([FromQuery] Guid? granjaId, CancellationToken ct)
    {
        // 1. Con granjaId lista solo el ganado de esa granja; las políticas RLS ocultan las granjas ajenas.
        var fichas = await db.FichasBovinas.AsNoTracking()
            .Where(f => granjaId == null || f.GranjaId == granjaId)
            .OrderBy(f => f.Identificacion)
            .ToListAsync(ct);
        return Ok(fichas.Select(Map).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FichaBovinaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FichaBovinaDto>> Obtener(Guid id, CancellationToken ct)
    {
        var f = await db.FichasBovinas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return f is null ? NotFound() : Ok(Map(f));
    }

    [HttpGet("{id:guid}/atenciones")]
    [ProducesResponseType(typeof(List<AtencionClinicaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AtencionClinicaDto>>> Atenciones(Guid id, CancellationToken ct)
    {
        if (!await db.FichasBovinas.AnyAsync(f => f.Id == id, ct)) return NotFound();
        var atenciones = await db.AtencionesClinicas.AsNoTracking()
            .Where(a => a.FichaBovinaId == id).OrderBy(a => a.Fecha).ToListAsync(ct);
        return Ok(atenciones.Select(AtencionesClinicasController.Map).ToList());
    }

    [HttpPost]
    [ProducesResponseType(typeof(FichaBovinaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FichaBovinaDto>> Crear(FichaBovinaCrearDto dto, CancellationToken ct)
    {
        var error = Validar(dto.Identificacion, dto.Etapa, dto.Nacimiento);
        if (error is not null) return BadRequest(error);
        if (!await db.Granjas.AnyAsync(g => g.Id == dto.GranjaId, ct))
            return BadRequest("La granja indicada no existe.");
        if (await Duplicada(dto.GranjaId, dto.Identificacion.Trim(), null, ct))
            return BadRequest("La identificación ya existe en esta granja.");
        var f = new FichaBovina { GranjaId = dto.GranjaId };
        Aplicar(f, dto.Identificacion, dto.Nombre, dto.Raza, dto.Etapa,
            dto.Nacimiento, dto.NacimientoEstimado);
        db.FichasBovinas.Add(f);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        { return BadRequest("La identificación ya existe en esta granja."); }
        return CreatedAtAction(nameof(Obtener), new { id = f.Id }, Map(f));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(Guid id, FichaBovinaActualizarDto dto, CancellationToken ct)
    {
        var f = await db.FichasBovinas.FindAsync([id], ct);
        if (f is null) return NotFound();
        if (f.FechaBaja is not null) return Conflict("La ficha ya fue dada de baja.");
        var rol = await RolActual(f.GranjaId, ct);
        if (!PermisosFicha.PuedeEditar(rol, f.Etapa, dto.Etapa)) return Forbid();
        var error = Validar(dto.Identificacion, dto.Etapa, dto.Nacimiento);
        if (error is not null) return BadRequest(error);
        if (await Duplicada(f.GranjaId, dto.Identificacion.Trim(), id, ct))
            return BadRequest("La identificación ya existe en esta granja.");
        Aplicar(f, dto.Identificacion, dto.Nombre, dto.Raza, dto.Etapa,
            dto.Nacimiento, dto.NacimientoEstimado);
        f.Version++;
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        { return BadRequest("La identificación ya existe en esta granja."); }
        return NoContent();
    }

    // Solo actualiza la etapa: no recibe ni sobrescribe otros datos de la ficha.
    [HttpPatch("{id:guid}/etapa")]
    public async Task<IActionResult> ActualizarEtapa(Guid id, EtapaFichaDto dto, CancellationToken ct)
    {
        var f = await db.FichasBovinas.FindAsync([id], ct);
        if (f is null) return NotFound();
        if (f.FechaBaja is not null) return Conflict("La ficha ya fue dada de baja.");
        if (!PermisosFicha.PuedeCambiarEtapa(await RolActual(f.GranjaId, ct))) return Forbid();
        if (dto.Etapa is null || !Enum.IsDefined(dto.Etapa.Value)) return BadRequest("La etapa no es válida.");
        f.Etapa = dto.Etapa.Value;
        f.Version++;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<string?> RolActual(Guid granjaId, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var cuentaId)) return null;
        if (!await db.Cuentas.AnyAsync(c => c.Id == cuentaId && c.Activa, ct)) return null;
        if (await db.Cuentas.AnyAsync(c => c.Id == cuentaId && c.Activa && c.AdministradorTecnico, ct)
            && await db.Granjas.AnyAsync(g => g.Id == granjaId && g.Confirmada, ct))
            return RolesVetLink.Administrador;
        return await db.Vinculaciones.Where(v => v.GranjaId == granjaId && v.CuentaId == cuentaId && v.Activa)
            .Select(v => v.Rol).FirstOrDefaultAsync(ct);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DarDeBaja(Guid id, [FromQuery] string? motivo,
        [FromQuery] DateOnly? fecha, CancellationToken ct)
    {
        var f = await db.FichasBovinas.FindAsync([id], ct);
        if (f is null) return NotFound();
        // 2. La fecha de baja la elige quien la registra; no puede ser futura.
        if (fecha > DateOnly.FromDateTime(DateTime.UtcNow)) return BadRequest("La fecha de baja no puede ser futura.");
        if (f.FechaBaja is null)
        {
            f.FechaBaja = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
            f.MotivoBaja = string.IsNullOrWhiteSpace(motivo) ? "Baja registrada mediante API" : motivo.Trim();
            f.Version++;
            await db.SaveChangesAsync(ct);
        }
        return NoContent();
    }

    internal static FichaBovinaDto Map(FichaBovina f) => new(f.Id, f.GranjaId,
        f.Identificacion, f.Nombre, f.Raza, f.Etapa, f.Nacimiento,
        f.NacimientoEstimado, f.FechaBaja, f.MotivoBaja, f.Version);

    private async Task<bool> Duplicada(Guid granjaId, string identificacion, Guid? excluir,
        CancellationToken ct) => await db.FichasBovinas.AnyAsync(f => f.GranjaId == granjaId &&
        f.Identificacion == identificacion && f.Id != excluir, ct);

    private static string? Validar(string identificacion, Etapa etapa, DateOnly? nacimiento)
    {
        if (string.IsNullOrWhiteSpace(identificacion)) return "La identificación es obligatoria.";
        if (!Enum.IsDefined(etapa)) return "La etapa no es válida.";
        if (nacimiento > DateOnly.FromDateTime(DateTime.UtcNow))
            return "La fecha de nacimiento no puede ser futura.";
        return null;
    }

    private static void Aplicar(FichaBovina f, string identificacion, string? nombre,
        string? raza, Etapa etapa, DateOnly? nacimiento, bool estimado)
    {
        f.Identificacion = identificacion.Trim();
        f.Nombre = nombre?.Trim();
        f.Raza = raza?.Trim();
        f.Etapa = etapa;
        f.Nacimiento = nacimiento;
        f.NacimientoEstimado = estimado;
    }

    private static bool EsDuplicado(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
