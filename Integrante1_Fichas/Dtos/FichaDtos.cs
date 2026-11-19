namespace ApiWeb.Integrante1_Fichas.Dtos;

public class CrearFichaDto
{
    public int GranjaId { get; set; }

    public string Identificacion { get; set; } = string.Empty;

    public string? Nombre { get; set; }

    public string? Raza { get; set; }

    public string Etapa { get; set; } = string.Empty;

    public DateTime FechaNacimiento { get; set; }

    public bool EsFechaEstimada { get; set; }
}

public class EditarFichaDto
{
    public string Identificacion { get; set; } = string.Empty;

    public string? Nombre { get; set; }

    public string? Raza { get; set; }

    public string Etapa { get; set; } = string.Empty;

    public DateTime FechaNacimiento { get; set; }

    public bool EsFechaEstimada { get; set; }
}

public class DarBajaDto
{
    public DateTime FechaBaja { get; set; }

    public string Motivo { get; set; } = string.Empty;
}
