namespace ApiWeb.Integrante4_CuidadosAplicaciones.Dtos;

public class RegistrarCuidadoDto
{
    public int AnimalId { get; set; }

    public string LoQueVioElTrabajador { get; set; } = string.Empty;

    public string LoQueHizoElTrabajador { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    public string TrabajadorNombre { get; set; } = string.Empty;
}

public class RegistrarAplicacionDto
{
    public int AnimalId { get; set; }

    public int? TratamientoId { get; set; }

    public int? PlanPrevencionId { get; set; }

    public decimal DosisAplicada { get; set; }

    public string Unidad { get; set; } = string.Empty;

    public DateTime FechaAplicacion { get; set; }

    public string AplicadoPor { get; set; } = string.Empty;

    public string? Observaciones { get; set; }
}

public class RegistrarCorreccionDto
{
    public string Entidad { get; set; } = string.Empty;

    public int EntidadId { get; set; }

    public string CampoModificado { get; set; } = string.Empty;

    public string ValorAnterior { get; set; } = string.Empty;

    public string ValorNuevo { get; set; } = string.Empty;

    public string MotivoCorreccion { get; set; } = string.Empty;

    public string UsuarioResponsable { get; set; } = string.Empty;
}
