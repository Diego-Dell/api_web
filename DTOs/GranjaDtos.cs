using System.ComponentModel.DataAnnotations;

namespace ApiWeb.DTOs;

public sealed class GranjaGuardarDto
{
    [Required, MaxLength(120)] public string Nombre { get; set; } = string.Empty;
    [Range(0, 9999999999.99)] public decimal Hectareas { get; set; }
    [Range(0, int.MaxValue)] public int Trabajadores { get; set; }
    public string? DocumentosTerreno { get; set; }
    [MaxLength(40)] public string? RUNSA { get; set; }
    public bool Confirmada { get; set; }
}

public sealed record GranjaDto(Guid Id, string Nombre, decimal Hectareas, int Trabajadores,
    string? DocumentosTerreno, string? RUNSA, bool Confirmada);
