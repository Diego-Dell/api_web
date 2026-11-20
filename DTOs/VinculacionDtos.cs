using System.ComponentModel.DataAnnotations;

namespace ApiWeb.DTOs;

public sealed class CrearVinculacionDto
{
    public Guid CuentaId { get; set; }
    public Guid GranjaId { get; set; }
    [Required, StringLength(25)] public string Rol { get; set; } = string.Empty;
}

public sealed class ActualizarVinculacionDto
{
    public Guid CuentaId { get; set; }
    public Guid GranjaId { get; set; }
    [Required, StringLength(25)] public string Rol { get; set; } = string.Empty;
    public bool Activa { get; set; }
}

public sealed record VinculacionDto(Guid Id, Guid CuentaId, Guid GranjaId, string Rol, bool Activa);
