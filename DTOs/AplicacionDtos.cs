using System.ComponentModel.DataAnnotations;

namespace ApiWeb.DTOs;

public class CrearAplicacionDto
{
    // RF-07: id generado en el dispositivo; un reintento con el mismo id no duplica el registro.
    public Guid? Id { get; set; }
    public Guid FichaBovinaId { get; set; }
    public Guid CuentaTrabajadorId { get; set; }
    public Guid? TratamientoId { get; set; }
    public Guid? PrevencionId { get; set; }
    public DateTime FechaHoraReal { get; set; }
    [Range(typeof(decimal), "0.000001", "999999999.999")] public decimal Dosis { get; set; }
    [Required, StringLength(30, MinimumLength = 1)] public string Unidad { get; set; } = string.Empty;
    [StringLength(1000)] public string? Observacion { get; set; }
}

public sealed class ActualizarAplicacionDto : CrearAplicacionDto
{
}

public sealed record AplicacionDto(Guid Id, Guid FichaBovinaId, Guid CuentaTrabajadorId,
    Guid? TratamientoId, Guid? PrevencionId, DateTime FechaHoraReal, decimal Dosis,
    string Unidad, string? Observacion, bool Anulado, string Tipo, int VersionIndicacion, int Version);
