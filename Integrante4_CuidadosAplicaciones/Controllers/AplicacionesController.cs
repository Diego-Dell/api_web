using ApiWeb.Data;
using ApiWeb.Integrante4_CuidadosAplicaciones.Dtos;
using ApiWeb.Integrante4_CuidadosAplicaciones.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Integrante4_CuidadosAplicaciones.Controllers;

[ApiController]
[Route("api/aplicaciones")]
public class AplicacionesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AplicacionesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/aplicaciones/animal/{animalId}
    [HttpGet("animal/{animalId}")]
    public async Task<IActionResult> ListarPorAnimal(int animalId)
    {
        // TODO: Integrante 4 - Listar aplicaciones de tratamientos y prevención
        var lista = await _context.AplicacionesIndicaciones
            .Where(a => a.AnimalId == animalId)
            .OrderByDescending(a => a.FechaAplicacion)
            .ToListAsync();

        return Ok(lista);
    }

    // POST: api/aplicaciones
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarAplicacionDto dto)
    {
        // TODO: Integrante 4 - Validación: Dosis positiva (> 0)
        if (dto.DosisAplicada <= 0)
            return BadRequest("La dosis aplicada debe ser positiva (mayor a 0).");

        var aplicacion = new AplicacionIndicacion
        {
            AnimalId = dto.AnimalId,
            TratamientoId = dto.TratamientoId,
            PlanPrevencionId = dto.PlanPrevencionId,
            DosisAplicada = dto.DosisAplicada,
            Unidad = dto.Unidad,
            FechaAplicacion = dto.FechaAplicacion,
            AplicadoPor = dto.AplicadoPor,
            Observaciones = dto.Observaciones
        };

        _context.AplicacionesIndicaciones.Add(aplicacion);

        // Si la aplicación corresponde a una vacuna o desparasitación preventiva, pasar a Realizada:
        if (dto.PlanPrevencionId.HasValue)
        {
            var plan = await _context.PlanesPrevencion.FindAsync(dto.PlanPrevencionId.Value);
            if (plan != null)
            {
                plan.Estado = "Realizado";
                plan.FechaRealizacion = dto.FechaAplicacion;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(aplicacion);
    }
}
