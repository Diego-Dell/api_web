using ApiWeb.Data;
using ApiWeb.DTOs;
using ApiWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/eventosreproductivos")]
public sealed class EventosReproductivosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<EventoReproductivoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EventoReproductivoDto>>> Listar(CancellationToken ct)
    {
        var registros = await db.EventosReproductivos.AsNoTracking()
            .OrderByDescending(e => e.Fecha).ToListAsync(ct);
        return Ok(registros.Select(Map).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventoReproductivoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventoReproductivoDto>> Obtener(Guid id, CancellationToken ct)
    {
        var e = await db.EventosReproductivos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? NotFound() : Ok(Map(e));
    }

    [HttpPost]
    [ProducesResponseType(typeof(EventoReproductivoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventoReproductivoDto>> Crear(EventoReproductivoCrearDto dto,
        CancellationToken ct)
    {
        // RF-07: si el registro ya llegó en un envío anterior, se devuelve el mismo.
        if (dto.Id is Guid previo && previo != Guid.Empty &&
            await db.EventosReproductivos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == previo, ct) is { } existente)
            return Ok(Map(existente));
        dto.CuentaVeterinarioId = User.CuentaId();
        var error = Validar(dto.Fecha, dto.Tipo, dto.Aptitud, dto.Prenez,
            dto.ProximoControl, dto.PartoProbable, dto.NumeroParto);
        if (error is not null) return BadRequest(error);
        var ficha = await db.FichasBovinas.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == dto.FichaBovinaId, ct);
        if (ficha is null || ficha.FechaBaja is not null)
            return BadRequest("La ficha bovina no existe o está dada de baja.");
        if (!await ClinicaValidaciones.VeterinarioValidoAsync(db, dto.CuentaVeterinarioId,
                ficha.GranjaId, ct))
            return BadRequest("La cuenta debe estar activa y vinculada como veterinario a la granja.");
        var e = new EventoReproductivo
        {
            Id = dto.Id is Guid nuevo && nuevo != Guid.Empty ? nuevo : Guid.NewGuid(),
            FichaBovinaId = dto.FichaBovinaId,
            CuentaVeterinarioId = dto.CuentaVeterinarioId
        };
        Aplicar(e, dto.Fecha, dto.Tipo, dto.Aptitud, dto.Prenez,
            dto.ProximoControl, dto.PartoProbable, dto.NumeroParto, dto.Detalle);
        db.EventosReproductivos.Add(e);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Obtener), new { id = e.Id }, Map(e));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(Guid id, EventoReproductivoActualizarDto dto,
        CancellationToken ct)
    {
        var e = await db.EventosReproductivos.FindAsync([id], ct);
        if (e is null) return NotFound();
        if (e.Anulado) return Conflict("El evento está anulado.");
        if (dto.Version is int vista && vista != e.Version) return Conflict(new { error = "El registro cambió mientras lo corregías. Revisá la versión actual antes de volver a guardar." });
        var error = Validar(dto.Fecha, dto.Tipo, dto.Aptitud, dto.Prenez,
            dto.ProximoControl, dto.PartoProbable, dto.NumeroParto);
        if (error is not null) return BadRequest(error);
        Aplicar(e, dto.Fecha, dto.Tipo, dto.Aptitud, dto.Prenez,
            dto.ProximoControl, dto.PartoProbable, dto.NumeroParto, dto.Detalle);
        e.Version++;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(Guid id, CancellationToken ct)
    {
        var e = await db.EventosReproductivos.FindAsync([id], ct);
        if (e is null) return NotFound();
        if (!e.Anulado)
        {
            e.Anulado = true;
            e.Version++;
            await db.SaveChangesAsync(ct);
        }
        return NoContent();
    }

    private static EventoReproductivoDto Map(EventoReproductivo e) => new(e.Id,
        e.FichaBovinaId, e.CuentaVeterinarioId, e.Fecha, e.Tipo, e.Aptitud,
        e.Prenez, e.ProximoControl, e.PartoProbable, e.NumeroParto, e.Detalle,
        e.Anulado, e.Version);

    private static string? Validar(DateTime fecha, TipoEvento tipo, Aptitud? aptitud,
        Prenez? prenez, DateOnly? proximoControl, DateOnly? partoProbable, int? numeroParto)
    {
        if (!ClinicaValidaciones.FechaValida(fecha)) return "La fecha es obligatoria.";
        if (!Enum.IsDefined(tipo) ||
            (aptitud.HasValue && !Enum.IsDefined(aptitud.Value)) ||
            (prenez.HasValue && !Enum.IsDefined(prenez.Value)))
            return "Uno de los valores reproductivos no es válido.";
        if (numeroParto is <= 0) return "El número de parto debe ser positivo.";
        var dia = DateOnly.FromDateTime(fecha);
        if (proximoControl < dia || partoProbable < dia)
            return "Las fechas previstas no pueden ser anteriores al evento.";
        if (prenez == Prenez.Vacia && partoProbable.HasValue)
            return "No puede indicar parto probable si la hembra está vacía.";
        return null;
    }

    private static void Aplicar(EventoReproductivo e, DateTime fecha, TipoEvento tipo,
        Aptitud? aptitud, Prenez? prenez, DateOnly? proximoControl,
        DateOnly? partoProbable, int? numeroParto, string? detalle)
    {
        e.Fecha = fecha;
        e.Tipo = tipo;
        e.Aptitud = aptitud;
        e.Prenez = prenez;
        e.ProximoControl = proximoControl;
        e.PartoProbable = partoProbable;
        e.NumeroParto = numeroParto;
        e.Detalle = detalle?.Trim();
    }
}
