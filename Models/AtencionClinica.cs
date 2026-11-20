using System.ComponentModel.DataAnnotations;

namespace ApiWeb.Models;

public class AtencionClinica
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FichaBovinaId { get; set; }
    public FichaBovina FichaBovina { get; set; } = null!;
    public Guid CuentaVeterinarioId { get; set; }
    public Cuenta CuentaVeterinario { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public TipoAtencion Tipo { get; set; }
    [Required] public string Detalle { get; set; } = string.Empty;
    public bool Anulado { get; set; }
    public int Version { get; set; } = 1;
    public ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();
}
