using System.ComponentModel.DataAnnotations;

namespace ApiWeb.DTOs;

// 1. El autor es siempre la persona del token; CuentaVeterinarioId se acepta pero no se usa.
public sealed class TratamientoCrearDto
{
    [Required] public Guid FichaBovinaId { get; set; }
    [Required] public Guid AtencionClinicaId { get; set; }
    public Guid? CuentaVeterinarioId { get; set; }
    [Required] public string Medicamento { get; set; } = string.Empty;
    [Range(0.001, 999999999.999)] public decimal Dosis { get; set; }
    [Required, MaxLength(30)] public string Unidad { get; set; } = string.Empty;
    [Required, MaxLength(60)] public string Via { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Frecuencia { get; set; } = string.Empty;
    public DateOnly Inicio { get; set; }
    [Required, MaxLength(100)] public string Duracion { get; set; } = string.Empty;
    [MaxLength(500)] public string? Retiro { get; set; }
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

// 2. Reemplazar: cierra la indicación vigente y crea la nueva en una sola operación.
public sealed class TratamientoReemplazarDto
{
    [Required] public string Medicamento { get; set; } = string.Empty;
    [Range(0.001, 999999999.999)] public decimal Dosis { get; set; }
    [Required, MaxLength(30)] public string Unidad { get; set; } = string.Empty;
    [Required, MaxLength(60)] public string Via { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Frecuencia { get; set; } = string.Empty;
    public DateOnly Inicio { get; set; }
    [Required, MaxLength(100)] public string Duracion { get; set; } = string.Empty;
    [MaxLength(500)] public string? Retiro { get; set; }
    public int Version { get; set; }
    public DateOnly FechaFinAnterior { get; set; }
    [Required, MaxLength(500)] public string MotivoFinAnterior { get; set; } = string.Empty;
}

// 3. Finalizar: registra fecha y motivo de cierre sin borrar la indicación.
public sealed class TratamientoFinalizarDto
{
    public int Version { get; set; }
    public DateOnly FechaFin { get; set; }
    [Required, MaxLength(500)] public string MotivoFin { get; set; } = string.Empty;
}

public sealed record TratamientoDto(Guid Id, Guid FichaBovinaId, Guid AtencionClinicaId,
    Guid CuentaVeterinarioId, string Medicamento, decimal Dosis, string Unidad, string Via,
    string Frecuencia, DateOnly Inicio, string Duracion, DateOnly? FechaFin, string? MotivoFin,
    bool Anulado, int Version, string? Retiro, Guid? AnteriorId);
