namespace ApiWeb.Integrante3_TratamientosPrevencion.Dtos;

public class CrearTratamientoDto
{
    public int AnimalId { get; set; }

    public int AtencionId { get; set; }

    public string Medicamento { get; set; } = string.Empty;

    public decimal Dosis { get; set; }

    public string UnidadDosis { get; set; } = string.Empty;

    public string ViaAdministracion { get; set; } = string.Empty;

    public string Frecuencia { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public int DuracionDias { get; set; }

    public int? PeriodoRetiroDias { get; set; }
}

public class ModificarTratamientoDto
{
    public DateTime FechaFin { get; set; }

    public string MotivoFin { get; set; } = string.Empty;

    // Nueva indicación que reemplaza la anterior
    public CrearTratamientoDto NuevaIndicacion { get; set; } = null!;
}

public class ProgramarPrevencionDto
{
    public int AnimalId { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaProgramada { get; set; }
}
