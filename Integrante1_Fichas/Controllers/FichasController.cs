using ApiWeb.Data;
using ApiWeb.Integrante1_Fichas.Dtos;
using ApiWeb.Integrante1_Fichas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Integrante1_Fichas.Controllers;

[ApiController]
[Route("api/fichas")]
public class FichasController : ControllerBase
{
    private readonly AppDbContext _context;

    public FichasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/fichas/granja/{granjaId}
    [HttpGet("granja/{granjaId}")]
    public async Task<IActionResult> ListarPorGranja(int granjaId)
    {
        // TODO: Integrante 1 - Retornar lista del ganado de la granja actual
        var lista = await _context.FichasGanado
            .Where(f => f.GranjaId == granjaId)
            .ToListAsync();

        return Ok(lista);
    }

    // GET: api/fichas/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerDetalle(int id)
    {
        // TODO: Integrante 1 - Retornar detalle de la ficha
        var ficha = await _context.FichasGanado.FindAsync(id);
        if (ficha == null)
            return NotFound("Ficha de ganado no encontrada.");

        return Ok(ficha);
    }

    // POST: api/fichas
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearFichaDto dto)
    {
        // TODO: Integrante 1 - Validaciones:
        // 1. Identificación no vacía
        // 2. No repetida en la misma granja
        if (string.IsNullOrWhiteSpace(dto.Identificacion))
            return BadRequest("La identificación es obligatoria.");

        var existe = await _context.FichasGanado
            .AnyAsync(f => f.GranjaId == dto.GranjaId && f.Identificacion == dto.Identificacion);

        if (existe)
            return BadRequest("Ya existe un animal con esta identificación en esta granja.");

        var ficha = new FichaGanado
        {
            GranjaId = dto.GranjaId,
            Identificacion = dto.Identificacion,
            Nombre = dto.Nombre,
            Raza = dto.Raza,
            Etapa = dto.Etapa,
            FechaNacimiento = dto.FechaNacimiento,
            EsFechaEstimada = dto.EsFechaEstimada
        };

        _context.FichasGanado.Add(ficha);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerDetalle), new { id = ficha.Id }, ficha);
    }

    // PUT: api/fichas/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarFichaDto dto)
    {
        // TODO: Integrante 1 - Editar ficha existente
        var ficha = await _context.FichasGanado.FindAsync(id);
        if (ficha == null)
            return NotFound("Ficha no encontrada.");

        if (ficha.DadoDeBaja)
            return BadRequest("No se puede editar un animal que ha sido dado de baja.");

        ficha.Identificacion = dto.Identificacion;
        ficha.Nombre = dto.Nombre;
        ficha.Raza = dto.Raza;
        ficha.Etapa = dto.Etapa;
        ficha.FechaNacimiento = dto.FechaNacimiento;
        ficha.EsFechaEstimada = dto.EsFechaEstimada;

        await _context.SaveChangesAsync();
        return Ok(ficha);
    }

    // POST: api/fichas/{id}/baja
    [HttpPost("{id}/baja")]
    public async Task<IActionResult> DarDeBaja(int id, [FromBody] DarBajaDto dto)
    {
        // TODO: Integrante 1 - Dar de baja con fecha y motivo
        var ficha = await _context.FichasGanado.FindAsync(id);
        if (ficha == null)
            return NotFound("Ficha no encontrada.");

        ficha.DadoDeBaja = true;
        ficha.FechaBaja = dto.FechaBaja;
        ficha.MotivoBaja = dto.Motivo;

        await _context.SaveChangesAsync();
        return Ok("Animal dado de baja exitosamente.");
    }
}
