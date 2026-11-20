using System.ComponentModel.DataAnnotations;

namespace ApiWeb.Models;

public class Cuenta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(100)] public string Identificador { get; set; } = string.Empty;
    [Required, MaxLength(500)] public string ClaveHash { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
    public bool AdministradorTecnico { get; set; }
    [Required, MaxLength(120)] public string Nombre { get; set; } = string.Empty;
    public ICollection<Vinculacion> Vinculaciones { get; set; } = new List<Vinculacion>();
    public ICollection<Invitacion> InvitacionesEmitidas { get; set; } = new List<Invitacion>();
}
