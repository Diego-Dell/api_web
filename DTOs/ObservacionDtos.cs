using System.ComponentModel.DataAnnotations;

namespace ApiWeb.DTOs;

// 1. Registrar una observación; la cuenta sale del token (CuentaTrabajadorId se acepta pero no se usa).
public sealed class CrearObservacionDto
{
    public Guid FichaBovinaId { get; set; }
    public Guid? CuentaTrabajadorId { get; set; }
    public DateTime FechaHora { get; set; }
    [Required, StringLength(1000, MinimumLength = 1)] public string Observado { get; set; } = string.Empty;
    [Required, StringLength(1000, MinimumLength = 1)] public string CuidadoRealizado { get; set; } = string.Empty;
}

// 2. Corregir una observación propia: datos nuevos y motivo obligatorio.
public sealed class CorregirObservacionDto
{
    public DateTime FechaHora { get; set; }
    [Required, StringLength(1000, MinimumLength = 1)] public string Observado { get; set; } = string.Empty;
    [Required, StringLength(1000, MinimumLength = 1)] public string CuidadoRealizado { get; set; } = string.Empty;
    [Required, StringLength(500, MinimumLength = 3)] public string Motivo { get; set; } = string.Empty;
}

// 3. Corregir una aplicación: fecha, dosis, unidad u observación, con motivo obligatorio.
public sealed class CorregirAplicacionDto
{
    public DateTime FechaHoraReal { get; set; }
    [Range(typeof(decimal), "0.000001", "999999999.999")] public decimal Dosis { get; set; }
    [Required, StringLength(30, MinimumLength = 1)] public string Unidad { get; set; } = string.Empty;
    [StringLength(1000)] public string? Observacion { get; set; }
    [Required, StringLength(500, MinimumLength = 3)] public string Motivo { get; set; } = string.Empty;
}

public sealed record ObservacionDto(Guid Id, Guid FichaBovinaId, Guid RegistradoPor, DateTime FechaHora,
    string Observado, string CuidadoRealizado, bool Anulado, int Version);

public sealed record CorreccionDto(Guid Id, Guid FichaBovinaId, string RegistroTipo, Guid RegistroId,
    string DatosAnteriores, string Motivo, Guid CorregidoPor, DateTime CorregidoEn);
