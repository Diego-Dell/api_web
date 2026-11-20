namespace ApiWeb.Integrante4_CuidadosAplicaciones.Models;

public sealed class CorreccionHistorial
{
    public Guid Id { get; set; }
    public string Tabla { get; set; } = string.Empty;
    public Guid RegistroId { get; set; }
    public string Campo { get; set; } = string.Empty;
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public Guid CorregidoPor { get; set; }
    public DateTime FechaHora { get; set; }
}
