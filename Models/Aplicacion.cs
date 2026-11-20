using System.ComponentModel.DataAnnotations;

namespace ApiWeb.Models;

public class Aplicacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FichaBovinaId { get; set; }
    public FichaBovina FichaBovina { get; set; } = null!;
    public Guid CuentaTrabajadorId { get; set; }
    public Cuenta CuentaTrabajador { get; set; } = null!;
    public Guid? TratamientoId { get; set; }
    public Tratamiento? Tratamiento { get; set; }
    public Guid? PrevencionId { get; set; }
    public Prevencion? Prevencion { get; set; }
    public DateTime FechaHoraReal { get; set; }
    public decimal Dosis { get; set; }
    [Required, MaxLength(30)] public string Unidad { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public bool Anulado { get; set; }
    [Required, MaxLength(50)] public string Tipo { get; set; } = string.Empty;
    public int VersionIndicacion { get; set; }
    public int Version { get; set; } = 1;
}
