using System.ComponentModel.DataAnnotations;

namespace ApiWeb.Models;

public class Granja
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(120)] public string Nombre { get; set; } = string.Empty;
    public decimal Hectareas { get; set; }
    public int Trabajadores { get; set; }
    public string? DocumentosTerreno { get; set; }
    [MaxLength(40)] public string? RUNSA { get; set; }
    public bool Confirmada { get; set; }
    public ICollection<Vinculacion> Vinculaciones { get; set; } = new List<Vinculacion>();
    public ICollection<Invitacion> Invitaciones { get; set; } = new List<Invitacion>();
    public ICollection<FichaBovina> FichasBovinas { get; set; } = new List<FichaBovina>();
}
