using System.ComponentModel.DataAnnotations;

namespace ApiWeb.Models;

public class Prevencion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FichaBovinaId { get; set; }
    public FichaBovina FichaBovina { get; set; } = null!;
    public Guid CuentaVeterinarioId { get; set; }
    public Cuenta CuentaVeterinario { get; set; } = null!;
    public TipoPrevencion Tipo { get; set; }
    [Required] public string Nombre { get; set; } = string.Empty;
    public DateOnly FechaProgramada { get; set; }
    public EstadoPrevencion Estado { get; set; } = EstadoPrevencion.Pendiente;
    public DateTime? FechaRealizacion { get; set; }
    public int Version { get; set; } = 1;
    public ICollection<Aplicacion> Aplicaciones { get; set; } = new List<Aplicacion>();
}
