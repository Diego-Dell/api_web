using System.ComponentModel.DataAnnotations;

namespace ApiWeb.Models;

public class EventoReproductivo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FichaBovinaId { get; set; }
    public FichaBovina FichaBovina { get; set; } = null!;
    public Guid CuentaVeterinarioId { get; set; }
    public Cuenta CuentaVeterinario { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public TipoEvento Tipo { get; set; }
    public Aptitud? Aptitud { get; set; }
    public Prenez? Prenez { get; set; }
    public DateOnly? ProximoControl { get; set; }
    public DateOnly? PartoProbable { get; set; }
    public int? NumeroParto { get; set; }
    public string? Detalle { get; set; }
    public bool Anulado { get; set; }
    public int Version { get; set; } = 1;
}
