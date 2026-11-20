namespace ApiWeb.Integrante4_CuidadosAplicaciones.Models;

public class AplicacionIndicacion
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    // Vinculado a Tratamiento o Prevención
    public int? TratamientoId { get; set; }

    public int? PlanPrevencionId { get; set; }

    // Dosis aplicada (debe ser positiva > 0)
    public decimal DosisAplicada { get; set; }

    public string Unidad { get; set; } = string.Empty;

    public DateTime FechaAplicacion { get; set; } = DateTime.UtcNow;

    public string AplicadoPor { get; set; } = string.Empty;

    public string? Observaciones { get; set; }
}
