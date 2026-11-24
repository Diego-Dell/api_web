using ApiWeb.Data;
using ApiWeb.DTOs;
using ApiWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/invitaciones")]
public sealed class InvitacionesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InvitacionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InvitacionDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await db.Invitaciones.AsNoTracking().OrderByDescending(x => x.FechaVencimiento)
            .Select(ToProjection()).ToListAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InvitacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvitacionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var invitacion = await db.Invitaciones.AsNoTracking().Where(x => x.Id == id)
            .Select(ToProjection()).SingleOrDefaultAsync(cancellationToken);
        return invitacion is null ? NotFound() : Ok(invitacion);
    }

    [HttpPost]
    [ProducesResponseType(typeof(InvitacionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InvitacionDto>> Create(CrearInvitacionDto dto, CancellationToken cancellationToken)
    {
        var rolInvitado = RolesVetLink.Canonicalizar(dto.Rol);
        if (rolInvitado is null) return BadRequest(new { error = "Rol inválido. Use Propietario, Administrador, Veterinario o Trabajador." });
        var fechaVencimientoUtc = DateTimesVetLink.ToUtc(dto.FechaVencimiento);
        if (fechaVencimientoUtc <= DateTime.UtcNow)
            return BadRequest(new { error = "La fecha de vencimiento debe ser posterior a la fecha actual." });
        var validacion = await ValidarEmision(dto.GranjaId, dto.EmitidaPorCuentaId, cancellationToken);
        if (validacion is not null) return validacion;

        var invitacion = new Invitacion
        {
            GranjaId = dto.GranjaId,
            EmitidaPorCuentaId = dto.EmitidaPorCuentaId,
            Rol = rolInvitado,
            CorreoDestinatario = dto.CorreoDestinatario.Trim(),
            FechaVencimiento = fechaVencimientoUtc,
            Vigente = true,
            Aceptada = false
        };
        db.Invitaciones.Add(invitacion);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = invitacion.Id }, ToDto(invitacion));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, ActualizarInvitacionDto dto, CancellationToken cancellationToken)
    {
        var invitacion = await db.Invitaciones.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (invitacion is null) return NotFound();
        if (invitacion.Aceptada) return BadRequest(new { error = "No se puede modificar una invitación ya aceptada." });
        var rol = RolesVetLink.Canonicalizar(dto.Rol);
        if (rol is null) return BadRequest(new { error = "Rol inválido. Use Propietario, Administrador, Veterinario o Trabajador." });
        var fechaVencimientoUtc = DateTimesVetLink.ToUtc(dto.FechaVencimiento);
        if (dto.Vigente && fechaVencimientoUtc <= DateTime.UtcNow)
            return BadRequest(new { error = "La fecha de vencimiento debe ser futura para una invitación vigente." });
        var validacion = await ValidarEmision(dto.GranjaId, dto.EmitidaPorCuentaId, cancellationToken);
        if (validacion is not null) return validacion;

        invitacion.GranjaId = dto.GranjaId;
        invitacion.EmitidaPorCuentaId = dto.EmitidaPorCuentaId;
        invitacion.Rol = rol;
        invitacion.CorreoDestinatario = dto.CorreoDestinatario.Trim();
        invitacion.FechaVencimiento = fechaVencimientoUtc;
        invitacion.Vigente = dto.Vigente;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var invitacion = await db.Invitaciones.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (invitacion is null) return NotFound();
        invitacion.Vigente = false;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<ActionResult?> ValidarEmision(Guid granjaId, Guid cuentaEmisoraId, CancellationToken cancellationToken)
    {
        if (!await db.Granjas.AnyAsync(x => x.Id == granjaId, cancellationToken))
            return BadRequest(new { error = "La granja indicada no existe." });
        if (!await db.Cuentas.AnyAsync(x => x.Id == cuentaEmisoraId && x.Activa, cancellationToken))
            return BadRequest(new { error = "La cuenta emisora no existe o está inactiva." });
        if (await ClinicaValidaciones.EsAdministradorAsync(db, cuentaEmisoraId, cancellationToken)
            && await db.Granjas.AnyAsync(g => g.Id == granjaId && g.Confirmada, cancellationToken)) return null;
        var puedeInvitar = await db.Vinculaciones.AnyAsync(x => x.CuentaId == cuentaEmisoraId && x.GranjaId == granjaId &&
            x.Activa && (x.Rol == RolesVetLink.Propietario || x.Rol == RolesVetLink.Administrador), cancellationToken);
        return puedeInvitar ? null : BadRequest(new { error = "La cuenta emisora necesita una vinculación activa de Propietario o Administrador en esa granja." });
    }

    private static System.Linq.Expressions.Expression<Func<Invitacion, InvitacionDto>> ToProjection() =>
        x => new InvitacionDto(x.Id, x.GranjaId, x.EmitidaPorCuentaId, x.Rol, x.Vigente, x.Aceptada,
            x.CorreoDestinatario, x.FechaVencimiento);

    private static InvitacionDto ToDto(Invitacion x) => new(x.Id, x.GranjaId, x.EmitidaPorCuentaId, x.Rol,
        x.Vigente, x.Aceptada, x.CorreoDestinatario, x.FechaVencimiento);
}
