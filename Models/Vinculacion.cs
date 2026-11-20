using System.ComponentModel.DataAnnotations;

namespace ApiWeb.Models;

public class Vinculacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CuentaId { get; set; }
    public Cuenta Cuenta { get; set; } = null!;
    public Guid GranjaId { get; set; }
    public Granja Granja { get; set; } = null!;
    [Required, MaxLength(25)] public string Rol { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
}
