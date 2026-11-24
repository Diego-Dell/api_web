using ApiWeb.Data;
using ApiWeb.DTOs;
using ApiWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/prevenciones")]
public sealed class PrevencionesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PrevencionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PrevencionDto>>> GetAll([FromQuery] Guid? fichaBovinaId,
        CancellationToken cancellationToken) =>
        // 1. Con fichaBovinaId lista solo la prevención de ese animal.
        Ok(await db.Prevenciones.AsNoTracking()
            .Where(x => fichaBovinaId == null || x.FichaBovinaId == fichaBovinaId)
            .OrderBy(x => x.FechaProgramada)
            .Select(ToProjection()).ToListAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PrevencionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrevencionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var prevencion = await db.Prevenciones.AsNoTracking().Where(x => x.Id == id)
            .Select(ToProjection()).SingleOrDefaultAsync(cancellationToken);
        return prevencion is null ? NotFound() : Ok(prevencion);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PrevencionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrevencionDto>> Create(CrearPrevencionDto dto, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(dto.Tipo)) return BadRequest(new { error = "El tipo de prevención no es válido." });

        // 2. La función de Supabase toma al veterinario del token y deja la prevención Pendiente.
        var id = (Guid)(await db.LlamarAsync("select public.programar_prevencion(@ficha, @tipo, @nombre, @fecha)",
            cancellationToken,
            ("ficha", dto.FichaBovinaId, NpgsqlDbType.Uuid), ("tipo", dto.Tipo.ToString(), NpgsqlDbType.Text),
            ("nombre", dto.Nombre, NpgsqlDbType.Text), ("fecha", dto.FechaProgramada, NpgsqlDbType.Date)))!;
        var prevencion = await db.Prevenciones.AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ToDto(prevencion));
    }

    // 3. Completar un control: pasa de Pendiente a Realizado y conserva el registro.
    [HttpPost("{id:guid}/completar-control")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CompletarControl(Guid id, CompletarControlDto dto, CancellationToken cancellationToken)
    {
        await db.LlamarAsync("select public.completar_control_preventivo(@id, @version, @fecha)", cancellationToken,
            ("id", id, NpgsqlDbType.Uuid), ("version", dto.Version, NpgsqlDbType.Integer),
            ("fecha", DateTimesVetLink.ToUtc(dto.FechaRealizacion), NpgsqlDbType.TimestampTz));
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, ActualizarPrevencionDto dto, CancellationToken cancellationToken)
    {
        var prevencion = await db.Prevenciones.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (prevencion is null) return NotFound();
        if (prevencion.Estado == EstadoPrevencion.Cancelada && dto.Estado != EstadoPrevencion.Cancelada)
            return BadRequest(new { error = "Una prevención cancelada no se puede reactivar." });
        var validacion = await ValidarResponsable(dto.FichaBovinaId, dto.CuentaVeterinarioId, dto.Tipo, dto.Nombre,
            dto.FechaProgramada, cancellationToken);
        if (validacion is not null) return validacion;
        validacion = ValidarEstado(dto.Estado, dto.FechaRealizacion);
        if (validacion is not null) return validacion;

        var tieneAplicacionesActivas = await db.Aplicaciones.AnyAsync(x => x.PrevencionId == id && !x.Anulado, cancellationToken);
        if (tieneAplicacionesActivas &&
            (dto.FichaBovinaId != prevencion.FichaBovinaId || dto.Estado != EstadoPrevencion.Realizada))
            return BadRequest(new { error = "La ficha o el estado no se pueden cambiar mientras existan aplicaciones activas asociadas." });

        prevencion.FichaBovinaId = dto.FichaBovinaId;
        // Se conserva el autor original; la auditoría identifica a quien corrige.
        prevencion.Tipo = dto.Tipo;
        prevencion.Nombre = dto.Nombre.Trim();
        prevencion.FechaProgramada = dto.FechaProgramada;
        prevencion.Estado = dto.Estado;
        prevencion.FechaRealizacion = dto.FechaRealizacion is null
            ? null
            : DateTimesVetLink.ToUtc(dto.FechaRealizacion.Value);
        prevencion.Version++;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var prevencion = await db.Prevenciones.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (prevencion is null) return NotFound();
        if (prevencion.Estado == EstadoPrevencion.Realizada)
            return BadRequest(new { error = "Una prevención realizada se conserva como antecedente y no se puede cancelar." });
        if (prevencion.Estado != EstadoPrevencion.Cancelada)
        {
            prevencion.Estado = EstadoPrevencion.Cancelada;
            prevencion.Version++;
            await db.SaveChangesAsync(cancellationToken);
        }
        return NoContent();
    }

    private async Task<ActionResult?> ValidarResponsable(Guid fichaBovinaId, Guid cuentaVeterinarioId,
        TipoPrevencion tipo, string nombre, DateOnly fechaProgramada, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(tipo)) return BadRequest(new { error = "El tipo de prevención no es válido." });
        if (string.IsNullOrWhiteSpace(nombre)) return BadRequest(new { error = "El nombre es obligatorio." });
        if (fechaProgramada == default) return BadRequest(new { error = "La fecha programada es obligatoria." });

        var ficha = await db.FichasBovinas.AsNoTracking()
            .Where(x => x.Id == fichaBovinaId)
            .Select(x => new { x.GranjaId, x.FechaBaja })
            .SingleOrDefaultAsync(cancellationToken);
        if (ficha is null) return BadRequest(new { error = "La ficha bovina indicada no existe." });
        if (ficha.FechaBaja is not null) return BadRequest(new { error = "No se puede programar prevención para una ficha dada de baja." });
        if (Guid.TryParse(User.FindFirst("sub")?.Value, out var actor) &&
            await ClinicaValidaciones.EsAdministradorAsync(db, actor, cancellationToken)) return null;
        if (!await db.Cuentas.AnyAsync(x => x.Id == cuentaVeterinarioId && x.Activa, cancellationToken))
            return BadRequest(new { error = "La cuenta veterinaria no existe o está inactiva." });
        var tieneRolVeterinario = await db.Vinculaciones.AnyAsync(x => x.CuentaId == cuentaVeterinarioId &&
            x.GranjaId == ficha.GranjaId && x.Activa && x.Rol == RolesVetLink.Veterinario, cancellationToken);
        if (!tieneRolVeterinario)
            return BadRequest(new { error = "La cuenta debe tener una vinculación activa como Veterinario en la granja de la ficha." });
        return null;
    }

    private static ActionResult? ValidarEstado(EstadoPrevencion estado, DateTime? fechaRealizacion)
    {
        if (!Enum.IsDefined(estado)) return new BadRequestObjectResult(new { error = "El estado de prevención no es válido." });
        if (estado == EstadoPrevencion.Realizada && fechaRealizacion is null)
            return new BadRequestObjectResult(new { error = "Una prevención realizada requiere fecha de realización." });
        if (estado != EstadoPrevencion.Realizada && fechaRealizacion is not null)
            return new BadRequestObjectResult(new { error = "Solo las prevenciones realizadas pueden tener fecha de realización." });
        return null;
    }

    private static System.Linq.Expressions.Expression<Func<Prevencion, PrevencionDto>> ToProjection() =>
        x => new PrevencionDto(x.Id, x.FichaBovinaId, x.CuentaVeterinarioId, x.Tipo, x.Nombre,
            x.FechaProgramada, x.Estado, x.FechaRealizacion, x.Version);

    private static PrevencionDto ToDto(Prevencion x) => new(x.Id, x.FichaBovinaId, x.CuentaVeterinarioId,
        x.Tipo, x.Nombre, x.FechaProgramada, x.Estado, x.FechaRealizacion, x.Version);
}
