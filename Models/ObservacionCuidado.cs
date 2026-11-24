namespace ApiWeb.Models;

// 1. Lo que el trabajador observó y el cuidado que realizó (tabla observaciones_cuidados).
public class ObservacionCuidado
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FichaBovinaId { get; set; }
    public Guid RegistradoPor { get; set; }
    public DateTime FechaHora { get; set; }
    public string Observado { get; set; } = string.Empty;
    public string CuidadoRealizado { get; set; } = string.Empty;
    public bool Anulado { get; set; }
    public int Version { get; set; } = 1;
}

// 2. Corrección de una aplicación u observación: guarda los datos anteriores y el motivo (tabla correcciones_cuidados).
public class CorreccionCuidado
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FichaBovinaId { get; set; }
    public string RegistroTipo { get; set; } = string.Empty;
    public Guid RegistroId { get; set; }
    public string DatosAnteriores { get; set; } = "{}";
    public string Motivo { get; set; } = string.Empty;
    public Guid CorregidoPor { get; set; }
    public DateTime CorregidoEn { get; set; } = DateTime.UtcNow;
}
