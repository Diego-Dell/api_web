using System.ComponentModel.DataAnnotations;
using ApiWeb.Models;

namespace ApiWeb.DTOs;

public sealed class EventoReproductivoCrearDto
{
    [Required] public Guid FichaBovinaId { get; set; }
    [Required] public Guid CuentaVeterinarioId { get; set; }
    public DateTime Fecha { get; set; }
    public TipoEvento Tipo { get; set; }
    public Aptitud? Aptitud { get; set; }
    public Prenez? Prenez { get; set; }
    public DateOnly? ProximoControl { get; set; }
    public DateOnly? PartoProbable { get; set; }
    public int? NumeroParto { get; set; }
    public string? Detalle { get; set; }
}

public sealed class EventoReproductivoActualizarDto
{
    public DateTime Fecha { get; set; }
    public TipoEvento Tipo { get; set; }
    public Aptitud? Aptitud { get; set; }
    public Prenez? Prenez { get; set; }
    public DateOnly? ProximoControl { get; set; }
    public DateOnly? PartoProbable { get; set; }
    public int? NumeroParto { get; set; }
    public string? Detalle { get; set; }
}

public sealed record EventoReproductivoDto(Guid Id, Guid FichaBovinaId, Guid CuentaVeterinarioId,
    DateTime Fecha, TipoEvento Tipo, Aptitud? Aptitud, Prenez? Prenez, DateOnly? ProximoControl,
    DateOnly? PartoProbable, int? NumeroParto, string? Detalle, bool Anulado, int Version);
