using ApiWeb.Data;
using ApiWeb.DTOs;
using ApiWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/vinculaciones")]
public sealed class VinculacionesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VinculacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VinculacionDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await db.Vinculaciones.AsNoTracking().OrderBy(x => x.GranjaId).ThenBy(x => x.CuentaId)
            .Select(ToProjection()).ToListAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VinculacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VinculacionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var vinculacion = await db.Vinculaciones.AsNoTracking().Where(x => x.Id == id)
            .Select(ToProjection()).SingleOrDefaultAsync(cancellationToken);
        return vinculacion is null ? NotFound() : Ok(vinculacion);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VinculacionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VinculacionDto>> Create(CrearVinculacionDto dto, CancellationToken cancellationToken)
    {
        var rol = RolesVetLink.Canonicalizar(dto.Rol);
        if (rol is null) return BadRequest(new { error = "Rol inválido. Use Propietario, Administrador, Veterinario o Trabajador." });
        var referencias = await ValidarReferencias(dto.CuentaId, dto.GranjaId, cancellationToken);
        if (referencias is not null) return referencias;
        if (await ExisteVinculacionActiva(dto.CuentaId, dto.GranjaId, null, cancellationToken))
            return Conflict(new { error = "La cuenta ya tiene una vinculación activa con esa granja." });

        var vinculacion = new Vinculacion
        {
            CuentaId = dto.CuentaId,
            GranjaId = dto.GranjaId,
            Rol = rol,
            Activa = true
        };
        db.Vinculaciones.Add(vinculacion);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { error = "La cuenta ya tiene una vinculación activa con esa granja." });
        }
        var response = ToDto(vinculacion);
        return CreatedAtAction(nameof(GetById), new { id = vinculacion.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, ActualizarVinculacionDto dto, CancellationToken cancellationToken)
    {
        var vinculacion = await db.Vinculaciones.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (vinculacion is null) return NotFound();
        var rol = RolesVetLink.Canonicalizar(dto.Rol);
        if (rol is null) return BadRequest(new { error = "Rol inválido. Use Propietario, Administrador, Veterinario o Trabajador." });
        var referencias = await ValidarReferencias(dto.CuentaId, dto.GranjaId, cancellationToken);
        if (referencias is not null) return referencias;
        if (dto.Activa && await ExisteVinculacionActiva(dto.CuentaId, dto.GranjaId, id, cancellationToken))
            return Conflict(new { error = "La cuenta ya tiene otra vinculación activa con esa granja." });

        vinculacion.CuentaId = dto.CuentaId;
        vinculacion.GranjaId = dto.GranjaId;
        vinculacion.Rol = rol;
        vinculacion.Activa = dto.Activa;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { error = "La cuenta ya tiene otra vinculación activa con esa granja." });
        }
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var vinculacion = await db.Vinculaciones.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (vinculacion is null) return NotFound();
        vinculacion.Activa = false;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<ActionResult?> ValidarReferencias(Guid cuentaId, Guid granjaId, CancellationToken cancellationToken)
    {
        if (!await db.Cuentas.AnyAsync(x => x.Id == cuentaId && x.Activa, cancellationToken))
            return BadRequest(new { error = "La cuenta no existe o está inactiva." });
        if (!await db.Granjas.AnyAsync(x => x.Id == granjaId, cancellationToken))
            return BadRequest(new { error = "La granja indicada no existe." });
        return null;
    }

    private Task<bool> ExisteVinculacionActiva(Guid cuentaId, Guid granjaId, Guid? excluirId, CancellationToken cancellationToken) =>
        db.Vinculaciones.AnyAsync(x => x.Activa && x.CuentaId == cuentaId && x.GranjaId == granjaId &&
            (excluirId == null || x.Id != excluirId), cancellationToken);

    private static System.Linq.Expressions.Expression<Func<Vinculacion, VinculacionDto>> ToProjection() =>
        x => new VinculacionDto(x.Id, x.CuentaId, x.GranjaId, x.Rol, x.Activa);

    private static VinculacionDto ToDto(Vinculacion x) => new(x.Id, x.CuentaId, x.GranjaId, x.Rol, x.Activa);
}
