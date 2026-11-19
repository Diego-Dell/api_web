namespace ApiWeb.Integrante2_ClinicaReproduccion.Models;

public class AtencionClinica
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    public string TipoAtencion { get; set; } = string.Empty; // Diagnóstico, Control o Evolución

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public string Detalle { get; set; } = string.Empty;

    public string RegistradoPor { get; set; } = string.Empty; // Rol: Veterinario o Dueño
}
