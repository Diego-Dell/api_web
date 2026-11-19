using ApiWeb.Data;
using ApiWeb.Integrante1_Fichas.Dtos;
using ApiWeb.Integrante1_Fichas.Models;
using ApiWeb.Integrante1_Fichas.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ApiWeb.Integrante1_Fichas.Controllers;

/// <summary>
/// 1. Fichas del ganado: lista, detalle, crear, editar y dar de baja (RF-02).
/// </summary>
/// <remarks>
/// Todas las rutas piden el token de Supabase de la persona en la cabecera Authorization: Bearer.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/fichas")]
[Produces("application/json")]
public class FichasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly FichasServicio _fichas;

    public FichasController(AppDbContext context, FichasServicio fichas)
    {
        _context = context;
        _fichas = fichas;
    }

    /// <summary>2. Lista el ganado de la granja; sin conBajas solo devuelve los animales activos.</summary>
    [HttpGet("granja/{granjaId:guid}")]
    [ProducesResponseType<List<FichaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorGranja(Guid granjaId, [FromQuery] bool conBajas = false)
    {
        // 2.1. Cualquier miembro activo de la granja consulta las fichas.
        if (await RolActual(granjaId) is null)
            return Prohibido("No tienes acceso a esta granja.");

        var consulta = _context.FichasGanado.AsNoTracking().Where(f => f.GranjaId == granjaId);
        if (!conBajas)
            consulta = consulta.Where(f => f.FechaBaja == null);

        var lista = await consulta.OrderBy(f => f.Identificacion).ToListAsync();
        return Ok(lista.Select(FichaDto.Desde));
    }

    /// <summary>3. Devuelve el detalle de una ficha, también si el animal está dado de baja.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<FichaDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerDetalle(Guid id)
    {
        // 3.1. Sin acceso a la granja se responde igual que si no existiera, para no revelar fichas ajenas.
        var ficha = await _context.FichasGanado.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
        if (ficha is null || await RolActual(ficha.GranjaId) is null)
            return NoEncontrada();

        return Ok(FichaDto.Desde(ficha));
    }

    /// <summary>4. Crea una ficha en la granja indicada.</summary>
    [HttpPost]
    [ProducesResponseType<FichaDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] CrearFichaDto dto)
    {
        // 4.1. Solo el dueño y el trabajador gestionan fichas.
        var rol = await RolActual(dto.GranjaId);
        if (rol is null)
            return Prohibido("No tienes acceso a esta granja.");
        if (!FichasServicio.PuedeGestionar(rol))
            return Prohibido("Tu rol no permite gestionar fichas.");

        // 4.2. Valida los datos y que la identificación no se repita en esta granja.
        var datos = Validar(dto.Identificacion, dto.Etapa, dto.Nacimiento, out var error);
        if (datos is null)
            return Invalido(error);
        if (await IdentificacionUsada(dto.GranjaId, datos.Value.Identificacion, null))
            return IdentificacionRepetida();

        var ficha = new FichaGanado
        {
            GranjaId = dto.GranjaId,
            Identificacion = datos.Value.Identificacion,
            Nombre = Limpio(dto.Nombre),
            Raza = Limpio(dto.Raza),
            Etapa = datos.Value.Etapa,
            Nacimiento = dto.Nacimiento,
            NacimientoEstimado = dto.Nacimiento is not null && dto.NacimientoEstimado,
        };

        // 4.3. Guarda; si otra petición usó la identificación al mismo tiempo, la base lo rechaza.
        _context.FichasGanado.Add(ficha);
        if (!await Guardar())
            return IdentificacionRepetida();

        return CreatedAtAction(nameof(ObtenerDetalle), new { id = ficha.Id }, FichaDto.Desde(ficha));
    }

    /// <summary>5. Edita los datos generales; el id no cambia, así los registros del animal siguen vinculados.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<FichaDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Editar(Guid id, [FromBody] EditarFichaDto dto)
    {
        var ficha = await _context.FichasGanado.FirstOrDefaultAsync(f => f.Id == id);
        var rol = ficha is null ? null : await RolActual(ficha.GranjaId);
        if (ficha is null || rol is null)
            return NoEncontrada();

        // 5.1. Solo el dueño y el trabajador editan, y nunca un animal dado de baja.
        if (!FichasServicio.PuedeGestionar(rol))
            return Prohibido("Tu rol no permite gestionar fichas.");
        if (ficha.DadoDeBaja)
            return Conflicto("No se puede editar un animal dado de baja.");

        // 5.2. Valida los datos; la identificación puede quedar igual, pero no repetir la de otro animal.
        var datos = Validar(dto.Identificacion, dto.Etapa, dto.Nacimiento, out var error);
        if (datos is null)
            return Invalido(error);
        if (await IdentificacionUsada(ficha.GranjaId, datos.Value.Identificacion, ficha.Id))
            return IdentificacionRepetida();

        ficha.Identificacion = datos.Value.Identificacion;
        ficha.Nombre = Limpio(dto.Nombre);
        ficha.Raza = Limpio(dto.Raza);
        ficha.Etapa = datos.Value.Etapa;
        ficha.Nacimiento = dto.Nacimiento;
        ficha.NacimientoEstimado = dto.Nacimiento is not null && dto.NacimientoEstimado;

        if (!await Guardar())
            return IdentificacionRepetida();

        return Ok(FichaDto.Desde(ficha));
    }

    /// <summary>6. Da de baja al animal con fecha y motivo; la ficha y su historial se conservan.</summary>
    [HttpPost("{id:guid}/baja")]
    [ProducesResponseType<FichaDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> DarDeBaja(Guid id, [FromBody] DarBajaDto dto)
    {
        var ficha = await _context.FichasGanado.FirstOrDefaultAsync(f => f.Id == id);
        var rol = ficha is null ? null : await RolActual(ficha.GranjaId);
        if (ficha is null || rol is null)
            return NoEncontrada();

        // 6.1. Solo el dueño da de baja, una sola vez.
        if (!FichasServicio.PuedeDarDeBaja(rol))
            return Prohibido("Solo el dueño puede dar de baja un animal.");
        if (ficha.DadoDeBaja)
            return Conflicto("El animal ya está dado de baja.");

        // 6.2. La fecha no puede ser futura ni anterior al nacimiento, y el motivo es obligatorio.
        var motivo = Limpio(dto.Motivo);
        if (dto.FechaBaja is not { } fecha)
            return Invalido("La fecha de baja es obligatoria.");
        if (fecha > Hoy())
            return Invalido("La fecha de baja no puede ser futura.");
        if (ficha.Nacimiento is { } nacimiento && fecha < nacimiento)
            return Invalido("La fecha de baja no puede ser anterior al nacimiento.");
        if (motivo is null)
            return Invalido("El motivo de la baja es obligatorio.");

        ficha.FechaBaja = fecha;
        ficha.MotivoBaja = motivo;
        await _context.SaveChangesAsync();

        return Ok(FichaDto.Desde(ficha));
    }

    /// <summary>7. Lee la cuenta del token de Supabase y busca su rol en la granja.</summary>
    private async Task<string?> RolActual(Guid granjaId)
    {
        var sub = User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var cuentaId) ? await _fichas.RolEnGranja(cuentaId, granjaId) : null;
    }

    /// <summary>8. Comprueba los datos comunes a crear y editar.</summary>
    private static (string Identificacion, EtapaFicha Etapa)? Validar(
        string? identificacion, string? etapa, DateOnly? nacimiento, out string error)
    {
        error = string.Empty;
        var limpia = Limpio(identificacion);
        if (limpia is null)
            error = "La identificación es obligatoria.";
        else if (!Enum.TryParse<EtapaFicha>(etapa, ignoreCase: true, out var valor) || !Enum.IsDefined(valor))
            error = "La etapa debe ser ternera, vaquilla o vaca.";
        else if (nacimiento > Hoy())
            error = "La fecha de nacimiento no puede ser futura.";
        else
            return (limpia, valor);
        return null;
    }

    private Task<bool> IdentificacionUsada(Guid granjaId, string identificacion, Guid? excepto) =>
        _context.FichasGanado.AnyAsync(f =>
            f.GranjaId == granjaId && f.Identificacion == identificacion && f.Id != excepto);

    /// <summary>9. Guarda los cambios; devuelve false si la base rechazó una identificación repetida.</summary>
    private async Task<bool> Guardar()
    {
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return false;
        }
    }

    private static string? Limpio(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static DateOnly Hoy() => DateOnly.FromDateTime(DateTime.UtcNow);

    private BadRequestObjectResult Invalido(string mensaje) => BadRequest(new { error = mensaje });

    private ObjectResult Prohibido(string mensaje) => StatusCode(StatusCodes.Status403Forbidden, new { error = mensaje });

    private NotFoundObjectResult NoEncontrada() => NotFound(new { error = "Ficha no encontrada." });

    private ConflictObjectResult Conflicto(string mensaje) => Conflict(new { error = mensaje });

    private ConflictObjectResult IdentificacionRepetida() =>
        Conflicto("Ya existe un animal con esta identificación en esta granja.");
}
