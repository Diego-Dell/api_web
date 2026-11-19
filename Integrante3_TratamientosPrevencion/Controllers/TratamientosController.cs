using ApiWeb.Data;
using ApiWeb.Integrante3_TratamientosPrevencion.Dtos;
using ApiWeb.Integrante3_TratamientosPrevencion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Integrante3_TratamientosPrevencion.Controllers;

[ApiController]
[Route("api/tratamientos")]
public class TratamientosController : ControllerBase
{
    private readonly AppDbContext _context;

    public TratamientosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/tratamientos/animal/{animalId}
    [HttpGet("animal/{animalId}")]
    public async Task<IActionResult> ListarPorAnimal(int animalId)
    {
        // TODO: Integrante 3 - Listar tratamientos activos e históricos del animal
        var lista = await _context.Tratamientos
            .Where(t => t.AnimalId == animalId)
            .ToListAsync();

        return Ok(lista);
    }

    // POST: api/tratamientos
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTratamientoDto dto)
    {
        // TODO: Integrante 3 - Tratamiento vinculado obligatoriamente a una atención
        var atencionExiste = await _context.AtencionesClinicas.AnyAsync(a => a.Id == dto.AtencionId && a.AnimalId == dto.AnimalId);
        if (!atencionExiste)
            return BadRequest("El tratamiento debe estar vinculado a una atención válida del mismo animal.");

        var tratamiento = new Tratamiento
        {
            AnimalId = dto.AnimalId,
            AtencionId = dto.AtencionId,
            Medicamento = dto.Medicamento,
            Dosis = dto.Dosis,
            UnidadDosis = dto.UnidadDosis,
            ViaAdministracion = dto.ViaAdministracion,
            Frecuencia = dto.Frecuencia,
            FechaInicio = dto.FechaInicio,
            DuracionDias = dto.DuracionDias,
            PeriodoRetiroDias = dto.PeriodoRetiroDias,
            Activo = true
        };

        _context.Tratamientos.Add(tratamiento);
        await _context.SaveChangesAsync();

        return Ok(tratamiento);
    }

    // PUT: api/tratamientos/{id}/modificar
    [HttpPut("{id}/modificar")]
    public async Task<IActionResult> Modificar(int id, [FromBody] ModificarTratamientoDto dto)
    {
        // TODO: Integrante 3 - Inmutabilidad:
        // Cerrar la indicación anterior con fecha_fin y motivo_fin. Nunca sobreescribir.
        var anterior = await _context.Tratamientos.FindAsync(id);
        if (anterior == null)
            return NotFound("Tratamiento anterior no encontrado.");

        anterior.Activo = false;
        anterior.FechaFin = dto.FechaFin;
        anterior.MotivoFin = dto.MotivoFin;

        // Crear la nueva indicación
        var nuevo = new Tratamiento
        {
            AnimalId = anterior.AnimalId,
            AtencionId = anterior.AtencionId,
            Medicamento = dto.NuevaIndicacion.Medicamento,
            Dosis = dto.NuevaIndicacion.Dosis,
            UnidadDosis = dto.NuevaIndicacion.UnidadDosis,
            ViaAdministracion = dto.NuevaIndicacion.ViaAdministracion,
            Frecuencia = dto.NuevaIndicacion.Frecuencia,
            FechaInicio = dto.NuevaIndicacion.FechaInicio,
            DuracionDias = dto.NuevaIndicacion.DuracionDias,
            PeriodoRetiroDias = dto.NuevaIndicacion.PeriodoRetiroDias,
            Activo = true,
            TratamientoAnteriorId = anterior.Id
        };

        _context.Tratamientos.Add(nuevo);
        await _context.SaveChangesAsync();

        return Ok(new { AnteriorCerrado = anterior, NuevoTratamiento = nuevo });
    }
}
