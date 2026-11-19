namespace ApiWeb.Integrante1_Fichas.Models;

public class FichaGanado
{
    public int Id { get; set; }

    public int GranjaId { get; set; }

    public string Identificacion { get; set; } = string.Empty;

    public string? Nombre { get; set; }

    public string? Raza { get; set; }

    public string Etapa { get; set; } = string.Empty;

    public DateTime FechaNacimiento { get; set; }

    public bool EsFechaEstimada { get; set; }

    public bool DadoDeBaja { get; set; } = false;

    public DateTime? FechaBaja { get; set; }

    public string? MotivoBaja { get; set; }
}
