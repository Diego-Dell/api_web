using ApiWeb.Data;
using ApiWeb.Integrante2_ClinicaReproduccion.Dtos;
using ApiWeb.Integrante2_ClinicaReproduccion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Integrante2_ClinicaReproduccion.Controllers;

[ApiController]
[Route("api/atenciones")]
public class AtencionesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AtencionesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/atenciones/animal/{animalId}
    [HttpGet("animal/{animalId}")]
    public async Task<IActionResult> ListarPorAnimal(int animalId)
    {
        // TODO: Integrante 2 - Retornar lista de atenciones del animal
        var atenciones = await _context.AtencionesClinicas
            .Where(a => a.AnimalId == animalId)
            .OrderByDescending(a => a.Fecha)
            .ToListAsync();

        return Ok(atenciones);
    }

    // POST: api/atenciones
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarAtencionDto dto)
    {
        // TODO: Integrante 2 - Registrar atención médica (diagnóstico, control o evolución)
        var atencion = new AtencionClinica
        {
            AnimalId = dto.AnimalId,
            TipoAtencion = dto.TipoAtencion,
            Fecha = dto.Fecha,
            Detalle = dto.Detalle,
            RegistradoPor = dto.RegistradoPor
        };

        _context.AtencionesClinicas.Add(atencion);
        await _context.SaveChangesAsync();

        return Ok(atencion);
    }
}
