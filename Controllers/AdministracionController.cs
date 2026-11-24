using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using ApiWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/administracion")]
public sealed class AdministracionController(AppDbContext db,
    IConfiguration configuracion, IHttpClientFactory clientes) : ControllerBase
{
    private Guid CuentaId => Guid.TryParse(User.FindFirst("sub")?.Value, out var id) ? id : Guid.Empty;
    private Task<bool> Autorizado(CancellationToken ct) =>
        db.Cuentas.AnyAsync(c => c.Id == CuentaId && c.Activa && c.AdministradorTecnico, ct);

    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen(CancellationToken ct)
    {
        if (!await Autorizado(ct)) return Forbid();
        var pendientes = await db.Database.SqlQuery<int>($"select count(*)::int as \"Value\" from public.granjas where not confirmada and motivo_rechazo is null").SingleAsync(ct);
        return Ok(new { pendientes, granjasActivas = await db.Granjas.CountAsync(g => g.Confirmada, ct), cuentas = await db.Cuentas.CountAsync(ct) });
    }

    [HttpGet("cuentas")]
    public async Task<IActionResult> Cuentas([FromQuery] string? buscar, CancellationToken ct)
    {
        if (!await Autorizado(ct)) return Forbid();
        var consulta = db.Cuentas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim().ToLower();
            consulta = consulta.Where(c => c.Nombre.ToLower().Contains(texto) || c.Identificador.ToLower().Contains(texto));
        }
        return Ok(await consulta.OrderBy(c => c.Nombre).Take(100)
            .Select(c => new { c.Id, c.Nombre, correo = c.Identificador, c.Activa, c.AdministradorTecnico }).ToListAsync(ct));
    }

    [HttpGet("cuentas/{id:guid}")]
    public async Task<IActionResult> Cuenta(Guid id, CancellationToken ct)
    {
        if (!await Autorizado(ct)) return Forbid();
        var cuenta = await db.Cuentas.AsNoTracking().Where(c => c.Id == id)
            .Select(c => new { c.Id, c.Nombre, correo = c.Identificador, c.Activa, c.AdministradorTecnico }).SingleOrDefaultAsync(ct);
        if (cuenta is null) return NotFound();
        var accesos = await db.Vinculaciones.AsNoTracking().Where(v => v.CuentaId == id)
            .Select(v => new { v.Id, v.GranjaId, granja = v.Granja.Nombre, v.Rol, v.Activa }).ToListAsync(ct);
        return Ok(new { cuenta, accesos });
    }

    [HttpPut("cuentas/{id:guid}")]
    public async Task<IActionResult> GuardarCuenta(Guid id, CuentaAdministrativaDto dto, CancellationToken ct)
    {
        if (!await Autorizado(ct)) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Nombre)) return BadRequest(new { error = "El nombre es obligatorio." });
        if (id == CuentaId && !dto.Activa) return BadRequest(new { error = "No podés desactivar tu propia cuenta administradora." });
        var cuenta = await db.Cuentas.SingleOrDefaultAsync(c => c.Id == id, ct);
        if (cuenta is null) return NotFound();
        cuenta.Nombre = dto.Nombre.Trim();
        cuenta.Activa = dto.Activa;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPatch("vinculaciones/{id:guid}")]
    public async Task<IActionResult> EstadoAcceso(Guid id, EstadoAccesoDto dto, CancellationToken ct)
    {
        if (!await Autorizado(ct)) return Forbid();
        var acceso = await db.Vinculaciones.SingleOrDefaultAsync(v => v.Id == id, ct);
        if (acceso is null) return NotFound();
        acceso.Activa = dto.Activa;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("granjas")]
    public async Task<IActionResult> Granjas(CancellationToken ct)
    {
        if (!await Autorizado(ct)) return Forbid();
        return Ok(await db.Database.SqlQuery<GranjaAdministrativaDto>($"""
            select id as "Id", nombre as "Nombre", hectareas as "Hectareas",
              runsa as "Runsa", documentos_terreno as "DocumentosTerreno",
              confirmada as "Confirmada", motivo_rechazo as "MotivoRechazo"
            from public.granjas where motivo_rechazo is null order by nombre
            """).ToListAsync(ct));
    }

    [HttpPost("granjas/{id:guid}/confirmar")]
    public async Task<IActionResult> Confirmar(Guid id, CancellationToken ct)
    {
        if (!await Autorizado(ct)) return Forbid();
        await db.Database.ExecuteSqlInterpolatedAsync($"select public.confirmar_granja({id})", ct);
        return NoContent();
    }

    [HttpPost("granjas/{id:guid}/rechazar")]
    public async Task<IActionResult> Rechazar(Guid id, RechazoGranjaDto dto, CancellationToken ct)
    {
        if (!await Autorizado(ct)) return Forbid();
        await db.Database.ExecuteSqlInterpolatedAsync($"select public.rechazar_granja({id}, {dto.Motivo})", ct);
        return NoContent();
    }

    [HttpGet("granjas/{id:guid}/miembros")]
    public async Task<IActionResult> Miembros(Guid id, CancellationToken ct)
    {
        if (!await Autorizado(ct)) return Forbid();
        return Ok(await db.Vinculaciones.AsNoTracking().Where(v => v.GranjaId == id)
            .Select(v => new { v.Id, v.CuentaId, nombre = v.Cuenta.Nombre,
                correo = v.Cuenta.Identificador, v.Rol, v.Activa }).ToListAsync(ct));
    }

    [HttpPost("cuentas/{id:guid}/recuperacion")]
    public async Task<IActionResult> Recuperar(Guid id, CancellationToken ct)
    {
        if (!await Autorizado(ct)) return Forbid();
        var cuenta = await db.Cuentas.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, ct);
        if (cuenta is null) return NotFound();
        if (!cuenta.Activa) return BadRequest(new { error = "Activá la cuenta antes de solicitar la recuperación." });
        var url = configuracion["SUPABASE_URL"]?.TrimEnd('/');
        var clave = configuracion["SUPABASE_PUBLISHABLE_KEY"];
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(clave))
            return StatusCode(503, new { error = "Falta configurar la recuperación en Render." });
        using var cliente = clientes.CreateClient();
        cliente.DefaultRequestHeaders.Add("apikey", clave);
        var redireccion = Uri.EscapeDataString("vetlink://restablecer");
        using var respuesta = await cliente.PostAsJsonAsync($"{url}/auth/v1/recover?redirect_to={redireccion}",
            new { email = cuenta.Identificador }, ct);
        if (!respuesta.IsSuccessStatusCode)
            return StatusCode(502, new { error = "Supabase no aceptó la solicitud de recuperación. Revisá el correo y el límite de envíos." });
        return Ok(new { mensaje = "Solicitud de recuperación aceptada. Revisá el correo de la cuenta." });
    }
}

public sealed record CuentaAdministrativaDto([Required, StringLength(120)] string Nombre, bool Activa);
public sealed record EstadoAccesoDto(bool Activa);
public sealed record RechazoGranjaDto([Required, StringLength(500, MinimumLength = 1)] string Motivo);
public sealed record GranjaAdministrativaDto(Guid Id, string Nombre, decimal Hectareas,
    string? Runsa, string? DocumentosTerreno, bool Confirmada, string? MotivoRechazo);
