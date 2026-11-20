namespace ApiWeb.Integrante4_CuidadosAplicaciones.Models;

public class CorreccionHistorial
{
    public int Id { get; set; }

    public string Entidad { get; set; } = string.Empty; // Ej: Ficha, Tratamiento, Cuidado

    public int EntidadId { get; set; }

    public string CampoModificado { get; set; } = string.Empty;

    public string ValorAnterior { get; set; } = string.Empty;

    public string ValorNuevo { get; set; } = string.Empty;

    public string MotivoCorreccion { get; set; } = string.Empty;

    public DateTime FechaCorreccion { get; set; } = DateTime.UtcNow;

    public string UsuarioResponsable { get; set; } = string.Empty;
}
