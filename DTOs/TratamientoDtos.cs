using System.ComponentModel.DataAnnotations;

namespace ApiWeb.DTOs;

public sealed class TratamientoCrearDto
{
    [Required] public Guid FichaBovinaId { get; set; }
    [Required] public Guid AtencionClinicaId { get; set; }
    [Required] public Guid CuentaVeterinarioId { get; set; }
    [Required] public string Medicamento { get; set; } = string.Empty;
    [Range(0.001, 999999999.999)] public decimal Dosis { get; set; }
    [Required, MaxLength(30)] public string Unidad { get; set; } = string.Empty;
    [Required, MaxLength(60)] public string Via { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Frecuencia { get; set; } = string.Empty;
    public DateOnly Inicio { get; set; }
    [Required, MaxLength(100)] public string Duracion { get; set; } = string.Empty;
    public DateOnly? FechaFin { get; set; }
    public string? MotivoFin { get; set; }
}

public sealed class TratamientoActualizarDto
{
    [Required] public string Medicamento { get; set; } = string.Empty;
    [Range(0.001, 999999999.999)] public decimal Dosis { get; set; }
    [Required, MaxLength(30)] public string Unidad { get; set; } = string.Empty;
    [Required, MaxLength(60)] public string Via { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Frecuencia { get; set; } = string.Empty;
    public DateOnly Inicio { get; set; }
    [Required, MaxLength(100)] public string Duracion { get; set; } = string.Empty;
    public DateOnly? FechaFin { get; set; }
    public string? MotivoFin { get; set; }
}

public sealed record TratamientoDto(Guid Id, Guid FichaBovinaId, Guid AtencionClinicaId,
    Guid CuentaVeterinarioId, string Medicamento, decimal Dosis, string Unidad, string Via,
    string Frecuencia, DateOnly Inicio, string Duracion, DateOnly? FechaFin, string? MotivoFin,
    bool Anulado, int Version);
