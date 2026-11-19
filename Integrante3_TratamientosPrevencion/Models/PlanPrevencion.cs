namespace ApiWeb.Integrante3_TratamientosPrevencion.Models;

public class PlanPrevencion
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    // Tipo: Vacuna, Desparasitación, Control general
    public string Tipo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaProgramada { get; set; }

    // Estado: "Pendiente" o "Realizado"
    public string Estado { get; set; } = "Pendiente";

    public DateTime? FechaRealizacion { get; set; }
}
