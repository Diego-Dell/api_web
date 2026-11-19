using ApiWeb.Data;
using ApiWeb.Integrante2_ClinicaReproduccion.Dtos;
using ApiWeb.Integrante2_ClinicaReproduccion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Integrante2_ClinicaReproduccion.Controllers;

[ApiController]
[Route("api/reproduccion")]
public class ReproduccionController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReproduccionController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/reproduccion/animal/{animalId}
    [HttpGet("animal/{animalId}")]
    public async Task<IActionResult> ListarEventos(int animalId)
    {
        // TODO: Integrante 2 - Retornar antecedentes y eventos reproductivos del animal
        var eventos = await _context.EventosReproduccion
            .Where(e => e.AnimalId == animalId)
            .ToListAsync();

        return Ok(eventos);
    }

    // POST: api/reproduccion
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarReproduccionDto dto)
    {
        // TODO: Integrante 2 - Registrar aptitud, preñez, parto o pérdida
        var evento = new EventoReproduccion
        {
            AnimalId = dto.AnimalId,
            Aptitud = dto.Aptitud,
            Prenez = dto.Prenez,
            TipoEvento = dto.TipoEvento,
            FechaEvento = dto.FechaEvento,
            ProximoControl = dto.ProximoControl,
            FechaProbableParto = dto.FechaProbableParto,
            Observaciones = dto.Observaciones
        };

        _context.EventosReproduccion.Add(evento);
        await _context.SaveChangesAsync();

        return Ok(evento);
    }
}
