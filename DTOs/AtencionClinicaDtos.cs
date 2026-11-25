using System.ComponentModel.DataAnnotations;
using ApiWeb.Models;

namespace ApiWeb.DTOs;

public sealed class AtencionClinicaCrearDto
{
    // RF-07: id generado en el dispositivo; un reintento con el mismo id no duplica el registro.
    public Guid? Id { get; set; }
    [Required] public Guid FichaBovinaId { get; set; }
    [Required] public Guid CuentaVeterinarioId { get; set; }
    public DateTime Fecha { get; set; }
    public TipoAtencion Tipo { get; set; }
    [Required] public string Detalle { get; set; } = string.Empty;
}

public sealed class AtencionClinicaActualizarDto
{
    public DateTime Fecha { get; set; }
    public TipoAtencion Tipo { get; set; }
    [Required] public string Detalle { get; set; } = string.Empty;
}

public sealed record AtencionClinicaDto(Guid Id, Guid FichaBovinaId, Guid CuentaVeterinarioId,
    DateTime Fecha, TipoAtencion Tipo, string Detalle, bool Anulado, int Version);
