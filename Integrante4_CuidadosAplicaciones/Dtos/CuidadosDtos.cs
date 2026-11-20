using System.ComponentModel.DataAnnotations;
using ApiWeb.Integrante4_CuidadosAplicaciones.Models;

namespace ApiWeb.Integrante4_CuidadosAplicaciones.Dtos;

public sealed class RegistrarCuidadoDto
{
    public Guid FichaId { get; set; }
    [Required] public string Observacion { get; set; } = string.Empty;
    [Required] public string AccionRealizada { get; set; } = string.Empty;
    public DateTime? FechaHora { get; set; }
}

public sealed class EditarCuidadoDto
{
    [Required] public string Observacion { get; set; } = string.Empty;
    [Required] public string AccionRealizada { get; set; } = string.Empty;
    [Required] public string MotivoCorreccion { get; set; } = string.Empty;
}

public sealed class RegistrarAplicacionDto
{
    public Guid FichaId { get; set; }
    public Guid? TratamientoId { get; set; }
    public Guid? PrevencionId { get; set; }
    [Range(typeof(decimal), "0.0000001", "999999999")] public decimal Dosis { get; set; }
    [Required] public string Unidad { get; set; } = string.Empty;
    public DateTime? FechaHoraReal { get; set; }
    public string? Observacion { get; set; }
}

public sealed class EditarAplicacionDto
{
    [Range(typeof(decimal), "0.0000001", "999999999")] public decimal Dosis { get; set; }
    [Required] public string Unidad { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    [Required] public string MotivoCorreccion { get; set; } = string.Empty;
}

public sealed record CuidadoDto(Guid Id, Guid FichaId, Guid RegistradoPor, DateTime FechaHora,
    string Observacion, string AccionRealizada, bool Anulado)
{
    public static CuidadoDto Desde(ObservacionCuidado c) =>
        new(c.Id, c.FichaId, c.RegistradoPor, c.FechaHora, c.Observacion, c.AccionRealizada, c.Anulado);
}

public sealed record AplicacionDto(Guid Id, Guid FichaId, Guid AplicadoPor, Guid? TratamientoId,
    Guid? PrevencionId, DateTime FechaHoraReal, decimal? Dosis, string? Unidad,
    string? Observacion, bool Anulado)
{
    public static AplicacionDto Desde(AplicacionIndicacion a) =>
        new(a.Id, a.FichaId, a.AplicadoPor, a.TratamientoId, a.PrevencionId,
            a.FechaHoraReal, a.Dosis, a.Unidad, a.Observacion, a.Anulado);
}

public sealed record CorreccionDto(Guid Id, string Tabla, Guid RegistroId, string Campo,
    string? ValorAnterior, string? ValorNuevo, string Motivo, Guid CorregidoPor, DateTime FechaHora)
{
    public static CorreccionDto Desde(CorreccionHistorial c) =>
        new(c.Id, c.Tabla, c.RegistroId, c.Campo, c.ValorAnterior, c.ValorNuevo,
            c.Motivo, c.CorregidoPor, c.FechaHora);
}
