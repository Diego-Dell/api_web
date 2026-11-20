namespace ApiWeb.Integrante4_CuidadosAplicaciones.Models;

public sealed class ObservacionCuidado
{
    public Guid Id { get; set; }
    public Guid FichaId { get; set; }
    public Guid RegistradoPor { get; set; }
    public DateTime FechaHora { get; set; }
    public string Observacion { get; set; } = string.Empty;
    public string AccionRealizada { get; set; } = string.Empty;
    public bool Anulado { get; set; }
}
