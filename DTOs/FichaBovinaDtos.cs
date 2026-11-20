using System.ComponentModel.DataAnnotations;
using ApiWeb.Models;

namespace ApiWeb.DTOs;

public sealed class FichaBovinaCrearDto
{
    [Required] public Guid GranjaId { get; set; }
    [Required, MaxLength(50)] public string Identificacion { get; set; } = string.Empty;
    [MaxLength(120)] public string? Nombre { get; set; }
    [MaxLength(120)] public string? Raza { get; set; }
    public Etapa Etapa { get; set; }
    public DateOnly? Nacimiento { get; set; }
    public bool NacimientoEstimado { get; set; }
}

public sealed class FichaBovinaActualizarDto
{
    [Required, MaxLength(50)] public string Identificacion { get; set; } = string.Empty;
    [MaxLength(120)] public string? Nombre { get; set; }
    [MaxLength(120)] public string? Raza { get; set; }
    public Etapa Etapa { get; set; }
    public DateOnly? Nacimiento { get; set; }
    public bool NacimientoEstimado { get; set; }
}

public sealed record FichaBovinaDto(Guid Id, Guid GranjaId, string Identificacion, string? Nombre,
    string? Raza, Etapa Etapa, DateOnly? Nacimiento, bool NacimientoEstimado,
    DateOnly? FechaBaja, string? MotivoBaja, int Version);

public sealed class BajaFichaDto
{
    [Required, MinLength(3)] public string Motivo { get; set; } = string.Empty;
}
