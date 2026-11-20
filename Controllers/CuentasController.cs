using ApiWeb.Data;
using ApiWeb.DTOs;
using ApiWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/cuentas")]
public sealed class CuentasController(AppDbContext db) : ControllerBase
{
    private static readonly PasswordHasher<Cuenta> PasswordHasher = new();

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

    [HttpPost]
    [ProducesResponseType(typeof(CuentaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CuentaDto>> Create(CrearCuentaDto dto, CancellationToken cancellationToken)
    {
        var identificador = dto.Identificador.Trim();
        if (await db.Cuentas.AnyAsync(x => x.Identificador == identificador, cancellationToken))
            return Conflict(new { error = "Ya existe una cuenta con ese identificador." });

        var cuenta = new Cuenta
        {
            Identificador = identificador,
            Nombre = dto.Nombre.Trim(),
            Activa = true,
            AdministradorTecnico = false
        };
        cuenta.ClaveHash = PasswordHasher.HashPassword(cuenta, dto.Clave);
        db.Cuentas.Add(cuenta);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { error = "El identificador de cuenta ya está en uso." });
        }

        var response = ToDto(cuenta);
        return CreatedAtAction(nameof(GetById), new { id = cuenta.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, ActualizarCuentaDto dto, CancellationToken cancellationToken)
    {
        var cuenta = await db.Cuentas.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cuenta is null) return NotFound();

        var identificador = dto.Identificador.Trim();
        if (await db.Cuentas.AnyAsync(x => x.Id != id && x.Identificador == identificador, cancellationToken))
            return Conflict(new { error = "Ya existe una cuenta con ese identificador." });

        cuenta.Identificador = identificador;
        cuenta.Nombre = dto.Nombre.Trim();
        if (!string.IsNullOrWhiteSpace(dto.NuevaClave))
            cuenta.ClaveHash = PasswordHasher.HashPassword(cuenta, dto.NuevaClave);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { error = "El identificador de cuenta ya está en uso." });
        }
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var cuenta = await db.Cuentas.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cuenta is null) return NotFound();
        cuenta.Activa = false;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static CuentaDto ToDto(Cuenta cuenta) =>
        new(cuenta.Id, cuenta.Identificador, cuenta.Nombre, cuenta.Activa, cuenta.AdministradorTecnico);
}
