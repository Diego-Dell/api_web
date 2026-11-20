using System.ComponentModel.DataAnnotations;

namespace ApiWeb.Models;

public class Invitacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GranjaId { get; set; }
    public Granja Granja { get; set; } = null!;
    public Guid EmitidaPorCuentaId { get; set; }
    public Cuenta EmitidaPor { get; set; } = null!;
    [Required, MaxLength(25)] public string Rol { get; set; } = string.Empty;
    public bool Vigente { get; set; } = true;
    public bool Aceptada { get; set; }
    [Required, EmailAddress, MaxLength(254)] public string CorreoDestinatario { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
}
