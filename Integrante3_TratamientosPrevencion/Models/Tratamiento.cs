namespace ApiWeb.Integrante3_TratamientosPrevencion.Models;

public class Tratamiento
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    // Vinculado obligatoriamente a una atención clínica
    public int AtencionId { get; set; }

    public string Medicamento { get; set; } = string.Empty;

    public decimal Dosis { get; set; }

    public string UnidadDosis { get; set; } = string.Empty; // mg, ml, etc.

    public string ViaAdministracion { get; set; } = string.Empty; // oral, intramuscular, etc.

    public string Frecuencia { get; set; } = string.Empty; // cada 8 horas, diario, etc.

    public DateTime FechaInicio { get; set; }

    public int DuracionDias { get; set; }

    public int? PeriodoRetiroDias { get; set; }

    // Campos de versionado e inmutabilidad:
    public bool Activo { get; set; } = true;

    public DateTime? FechaFin { get; set; }

    public string? MotivoFin { get; set; }

    public int? TratamientoAnteriorId { get; set; }
}
