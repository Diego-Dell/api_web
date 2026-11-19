namespace ApiWeb.Integrante2_ClinicaReproduccion.Models;

public class EventoReproduccion
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    // Aptitud: Sin evaluar / Todavía no apta / Apta
    public string Aptitud { get; set; } = "Sin evaluar";

    // Preñez: No determinada / No gestante / Confirmada
    public string Prenez { get; set; } = "No determinada";

    // Parto o pérdida
    public string? TipoEvento { get; set; }

    public DateTime? FechaEvento { get; set; }

    public DateTime? ProximoControl { get; set; }

    public DateTime? FechaProbableParto { get; set; }

    public string? Observaciones { get; set; }
}
