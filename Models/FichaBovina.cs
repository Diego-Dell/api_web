using System.ComponentModel.DataAnnotations;

namespace ApiWeb.Models;

public class FichaBovina
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GranjaId { get; set; }
    public Granja Granja { get; set; } = null!;
    [Required, MaxLength(50)] public string Identificacion { get; set; } = string.Empty;
    [MaxLength(120)] public string? Nombre { get; set; }
    [MaxLength(120)] public string? Raza { get; set; }
    public Etapa Etapa { get; set; }
    public DateOnly? Nacimiento { get; set; }
    public bool NacimientoEstimado { get; set; }
    public DateOnly? FechaBaja { get; set; }
    public string? MotivoBaja { get; set; }
    public int Version { get; set; } = 1;
    public ICollection<AtencionClinica> Atenciones { get; set; } = new List<AtencionClinica>();
    public ICollection<EventoReproductivo> EventosReproductivos { get; set; } = new List<EventoReproductivo>();
    public ICollection<Prevencion> Prevenciones { get; set; } = new List<Prevencion>();
    public ICollection<Aplicacion> Aplicaciones { get; set; } = new List<Aplicacion>();
}
