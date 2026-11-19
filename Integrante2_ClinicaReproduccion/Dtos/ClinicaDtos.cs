namespace ApiWeb.Integrante2_ClinicaReproduccion.Dtos;

public class RegistrarAtencionDto
{
    public int AnimalId { get; set; }

    public string TipoAtencion { get; set; } = string.Empty; // Diagnóstico, Control o Evolución

    public DateTime Fecha { get; set; }

    public string Detalle { get; set; } = string.Empty;

    public string RegistradoPor { get; set; } = string.Empty;
}

public class RegistrarReproduccionDto
{
    public int AnimalId { get; set; }

    public string Aptitud { get; set; } = "Sin evaluar";

    public string Prenez { get; set; } = "No determinada";

    public string? TipoEvento { get; set; }

    public DateTime? FechaEvento { get; set; }

    public DateTime? ProximoControl { get; set; }

    public DateTime? FechaProbableParto { get; set; }

    public string? Observaciones { get; set; }
}
