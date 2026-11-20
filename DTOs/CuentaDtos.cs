using System.ComponentModel.DataAnnotations;

namespace ApiWeb.DTOs;

public sealed class CrearCuentaDto
{
    [Required, StringLength(100, MinimumLength = 3)] public string Identificador { get; set; } = string.Empty;
    [Required, StringLength(128, MinimumLength = 12)] public string Clave { get; set; } = string.Empty;
    [Required, StringLength(120, MinimumLength = 1)] public string Nombre { get; set; } = string.Empty;
}

public sealed class ActualizarCuentaDto
{
    [Required, StringLength(100, MinimumLength = 3)] public string Identificador { get; set; } = string.Empty;
    [StringLength(128, MinimumLength = 12)] public string? NuevaClave { get; set; }
    [Required, StringLength(120, MinimumLength = 1)] public string Nombre { get; set; } = string.Empty;
}

public sealed record CuentaDto(Guid Id, string Identificador, string Nombre, bool Activa, bool AdministradorTecnico);
