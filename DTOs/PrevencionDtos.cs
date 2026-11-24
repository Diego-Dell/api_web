using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ApiWeb.Models;

namespace ApiWeb.DTOs;

public sealed class CrearPrevencionDto
{
    public Guid FichaBovinaId { get; set; }
    public Guid CuentaVeterinarioId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter)), EnumDataType(typeof(TipoPrevencion))]
    public TipoPrevencion Tipo { get; set; }
    [Required, StringLength(200, MinimumLength = 1)] public string Nombre { get; set; } = string.Empty;
    public DateOnly FechaProgramada { get; set; }
}

public sealed class ActualizarPrevencionDto
{
    public Guid FichaBovinaId { get; set; }
    public Guid CuentaVeterinarioId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter)), EnumDataType(typeof(TipoPrevencion))]
    public TipoPrevencion Tipo { get; set; }
    [Required, StringLength(200, MinimumLength = 1)] public string Nombre { get; set; } = string.Empty;
    public DateOnly FechaProgramada { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter)), EnumDataType(typeof(EstadoPrevencion))]
    public EstadoPrevencion Estado { get; set; }
    public DateTime? FechaRealizacion { get; set; }
}

// 1. Completar un control preventivo: versión vista y fecha de realización.
public sealed class CompletarControlDto
{
    public int Version { get; set; }
    public DateTime FechaRealizacion { get; set; }
}

public sealed record PrevencionDto(Guid Id, Guid FichaBovinaId, Guid CuentaVeterinarioId,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] TipoPrevencion Tipo,
    string Nombre, DateOnly FechaProgramada,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] EstadoPrevencion Estado,
    DateTime? FechaRealizacion, int Version);
