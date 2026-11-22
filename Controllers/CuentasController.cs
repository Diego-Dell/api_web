using ApiWeb.Data;
using ApiWeb.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

// 1. Las cuentas se crean al registrarse en la app (Supabase Auth); la API las consulta y edita el nombre.
[ApiController]
[Route("api/cuentas")]
public sealed class CuentasController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CuentaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CuentaDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await db.Cuentas.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new CuentaDto(x.Id, x.Identificador, x.Nombre, x.Activa, x.AdministradorTecnico))
            .ToListAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CuentaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CuentaDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cuenta = await db.Cuentas.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new CuentaDto(x.Id, x.Identificador, x.Nombre, x.Activa, x.AdministradorTecnico))
            .SingleOrDefaultAsync(cancellationToken);
        return cuenta is null ? NotFound() : Ok(cuenta);
    }

    // 2. Cada persona edita su propio nombre (política cuentas_editar).
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, ActualizarCuentaDto dto, CancellationToken cancellationToken)
    {
        var cuenta = await db.Cuentas.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cuenta is null) return NotFound();
        cuenta.Nombre = dto.Nombre.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
