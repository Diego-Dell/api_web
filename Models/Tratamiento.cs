using System.ComponentModel.DataAnnotations;

namespace ApiWeb.Models;

public class Tratamiento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FichaBovinaId { get; set; }
    public FichaBovina FichaBovina { get; set; } = null!;
    public Guid AtencionClinicaId { get; set; }
    public AtencionClinica AtencionClinica { get; set; } = null!;
    public Guid CuentaVeterinarioId { get; set; }
    public Cuenta CuentaVeterinario { get; set; } = null!;
    [Required] public string Medicamento { get; set; } = string.Empty;
    public decimal Dosis { get; set; }
    [Required, MaxLength(30)] public string Unidad { get; set; } = string.Empty;
    [Required, MaxLength(60)] public string Via { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Frecuencia { get; set; } = string.Empty;
    public DateOnly Inicio { get; set; }
    [Required, MaxLength(100)] public string Duracion { get; set; } = string.Empty;
    public DateOnly? FechaFin { get; set; }
    public string? MotivoFin { get; set; }
    public bool Anulado { get; set; }
    public int Version { get; set; } = 1;
    public ICollection<Aplicacion> Aplicaciones { get; set; } = new List<Aplicacion>();
}
