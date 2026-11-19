namespace ApiWeb.Integrante4_CuidadosAplicaciones.Models;

public class ObservacionCuidado
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    public string LoQueVioElTrabajador { get; set; } = string.Empty;

    public string LoQueHizoElTrabajador { get; set; } = string.Empty;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public string TrabajadorNombre { get; set; } = string.Empty;
}
