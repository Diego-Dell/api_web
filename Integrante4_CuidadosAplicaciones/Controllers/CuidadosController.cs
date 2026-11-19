using ApiWeb.Data;
using ApiWeb.Integrante4_CuidadosAplicaciones.Dtos;
using ApiWeb.Integrante4_CuidadosAplicaciones.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Integrante4_CuidadosAplicaciones.Controllers;

[ApiController]
[Route("api/cuidados")]
public class CuidadosController : ControllerBase
{
    private readonly AppDbContext _context;

    public CuidadosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/cuidados/animal/{animalId}
    [HttpGet("animal/{animalId}")]
    public async Task<IActionResult> ListarPorAnimal(int animalId)
    {
        // TODO: Integrante 4 - Listar observaciones y cuidados del animal
        var lista = await _context.ObservacionesCuidados
            .Where(c => c.AnimalId == animalId)
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();

        return Ok(lista);
    }

    // POST: api/cuidados
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarCuidadoDto dto)
    {
        // TODO: Integrante 4 - Registrar lo que vio el trabajador y lo que hizo
        var cuidado = new ObservacionCuidado
        {
            AnimalId = dto.AnimalId,
            LoQueVioElTrabajador = dto.LoQueVioElTrabajador,
            LoQueHizoElTrabajador = dto.LoQueHizoElTrabajador,
            Fecha = dto.Fecha,
            TrabajadorNombre = dto.TrabajadorNombre
        };

        _context.ObservacionesCuidados.Add(cuidado);
        await _context.SaveChangesAsync();

        return Ok(cuidado);
    }

    // POST: api/cuidados/correccion
    [HttpPost("correccion")]
    public async Task<IActionResult> RegistrarCorreccion([FromBody] RegistrarCorreccionDto dto)
    {
        // TODO: Integrante 4 - Guardar corrección con el dato anterior y el motivo
        var correccion = new CorreccionHistorial
        {
            Entidad = dto.Entidad,
            EntidadId = dto.EntidadId,
            CampoModificado = dto.CampoModificado,
            ValorAnterior = dto.ValorAnterior,
            ValorNuevo = dto.ValorNuevo,
            MotivoCorreccion = dto.MotivoCorreccion,
            UsuarioResponsable = dto.UsuarioResponsable,
            FechaCorreccion = DateTime.UtcNow
        };

        _context.CorreccionesHistorial.Add(correccion);
        await _context.SaveChangesAsync();

        return Ok(correccion);
    }
}
