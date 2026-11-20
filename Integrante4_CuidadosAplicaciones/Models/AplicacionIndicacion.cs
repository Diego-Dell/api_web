namespace ApiWeb.Integrante4_CuidadosAplicaciones.Models;

public sealed class AplicacionIndicacion
{
    public Guid Id { get; set; }
    public Guid FichaId { get; set; }
    public Guid AplicadoPor { get; set; }
    public Guid? TratamientoId { get; set; }
    public Guid? PrevencionId { get; set; }
    public DateTime FechaHoraReal { get; set; }
    public decimal? Dosis { get; set; }
    public string? Unidad { get; set; }
    public string? Observacion { get; set; }
    public bool Anulado { get; set; }
}
