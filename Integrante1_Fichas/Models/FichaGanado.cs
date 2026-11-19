namespace ApiWeb.Integrante1_Fichas.Models;

/// <summary>
/// 1. Representa una fila de la tabla fichas_bovinas de Supabase, la misma que usa la app móvil.
/// </summary>
public class FichaGanado
{
    public Guid Id { get; set; }

    public Guid GranjaId { get; set; }

    /// <summary>2. Arete u otro código; es único dentro de cada granja.</summary>
    public string Identificacion { get; set; } = string.Empty;

    public string? Nombre { get; set; }

    public string? Raza { get; set; }

    public EtapaFicha Etapa { get; set; }

    public DateOnly? Nacimiento { get; set; }

    public bool NacimientoEstimado { get; set; }

    public DateOnly? FechaBaja { get; set; }

    public string? MotivoBaja { get; set; }

    /// <summary>3. Un animal dado de baja se sigue consultando, pero no admite nuevos eventos.</summary>
    public bool DadoDeBaja => FechaBaja is not null;
}

/// <summary>
/// 4. Etapas del enum etapa de Postgres; Npgsql las guarda en minúsculas (ternera, vaquilla, vaca).
/// </summary>
public enum EtapaFicha
{
    Ternera,
    Vaquilla,
    Vaca,
}
