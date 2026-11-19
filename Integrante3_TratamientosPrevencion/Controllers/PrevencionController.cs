using ApiWeb.Data;
using ApiWeb.Integrante3_TratamientosPrevencion.Dtos;
using ApiWeb.Integrante3_TratamientosPrevencion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Integrante3_TratamientosPrevencion.Controllers;

[ApiController]
[Route("api/prevencion")]
public class PrevencionController : ControllerBase
{
    private readonly AppDbContext _context;

    public PrevencionController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/prevencion/animal/{animalId}
    [HttpGet("animal/{animalId}")]
    public async Task<IActionResult> ListarPorAnimal(int animalId)
    {
        // TODO: Integrante 3 - Listar planes de prevención del animal
        var lista = await _context.PlanesPrevencion
            .Where(p => p.AnimalId == animalId)
            .OrderBy(p => p.FechaProgramada)
            .ToListAsync();

        return Ok(lista);
    }

    // POST: api/prevencion
    [HttpPost]
    public async Task<IActionResult> Programar([FromBody] ProgramarPrevencionDto dto)
    {
        // TODO: Integrante 3 - Programar prevención; estado inicial "Pendiente"
        var plan = new PlanPrevencion
        {
            AnimalId = dto.AnimalId,
            Tipo = dto.Tipo,
            Nombre = dto.Nombre,
            FechaProgramada = dto.FechaProgramada,
            Estado = "Pendiente"
        };

        _context.PlanesPrevencion.Add(plan);
        await _context.SaveChangesAsync();

        return Ok(plan);
    }

    // PUT: api/prevencion/{id}/completar
    [HttpPut("{id}/completar")]
    public async Task<IActionResult> Completar(int id)
    {
        // TODO: Integrante 3 - Pasar de "Pendiente" a "Realizado"
        var plan = await _context.PlanesPrevencion.FindAsync(id);
        if (plan == null)
            return NotFound("Plan de prevención no encontrado.");

        plan.Estado = "Realizado";
        plan.FechaRealizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(plan);
    }
}
