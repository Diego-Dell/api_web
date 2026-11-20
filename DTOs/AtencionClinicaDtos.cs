using System.ComponentModel.DataAnnotations;
using ApiWeb.Models;

namespace ApiWeb.DTOs;

public sealed class AtencionClinicaCrearDto
{
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
