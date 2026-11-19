using ApiWeb.Integrante1_Fichas.Models;

namespace ApiWeb.Integrante1_Fichas.Dtos;

/// <summary>1. Datos para crear una ficha en una granja.</summary>
public class CrearFichaDto
{
    public Guid GranjaId { get; set; }

    public string Identificacion { get; set; } = string.Empty;

    public string? Nombre { get; set; }

    public string? Raza { get; set; }

    /// <summary>1.1. Acepta ternera, vaquilla o vaca.</summary>
    public string Etapa { get; set; } = string.Empty;

    public DateOnly? Nacimiento { get; set; }

    public bool NacimientoEstimado { get; set; }
}

/// <summary>2. Datos generales que se pueden editar; la granja y la baja no cambian por aquí.</summary>
public class EditarFichaDto
{
    public string Identificacion { get; set; } = string.Empty;

    public string? Nombre { get; set; }

    public string? Raza { get; set; }

    /// <summary>1.1. Acepta ternera, vaquilla o vaca.</summary>
    public string Etapa { get; set; } = string.Empty;

    public DateOnly? Nacimiento { get; set; }

    public bool NacimientoEstimado { get; set; }
}

/// <summary>3. Fecha y motivo de la baja.</summary>
public class DarBajaDto
{
    public DateOnly? FechaBaja { get; set; }

    public string Motivo { get; set; } = string.Empty;
}

/// <summary>4. Ficha tal como la devuelve la API.</summary>
public record FichaDto(
    Guid Id,
    Guid GranjaId,
    string Identificacion,
    string? Nombre,
    string? Raza,
    string Etapa,
    DateOnly? Nacimiento,
    bool NacimientoEstimado,
    bool DadoDeBaja,
    DateOnly? FechaBaja,
    string? MotivoBaja)
{
    public static FichaDto Desde(FichaGanado f) => new(
        f.Id,
        f.GranjaId,
        f.Identificacion,
        f.Nombre,
        f.Raza,
        f.Etapa.ToString().ToLowerInvariant(),
        f.Nacimiento,
        f.NacimientoEstimado,
        f.DadoDeBaja,
        f.FechaBaja,
        f.MotivoBaja);
}
