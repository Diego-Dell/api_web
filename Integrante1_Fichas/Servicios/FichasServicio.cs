using ApiWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Integrante1_Fichas.Servicios;

/// <summary>
/// 1. Reúne las reglas de acceso a las fichas; los demás módulos lo usan para saber si un animal admite eventos.
/// </summary>
/// <remarks>
/// La API se conecta a Postgres con un usuario que no pasa por las políticas RLS de Supabase,
/// así que aquí se repite la regla de rol_en_granja.
/// </remarks>
public class FichasServicio
{
    public const string Dueno = "dueno";
    public const string Veterinario = "veterinario";
    public const string Trabajador = "trabajador";

    private readonly AppDbContext _context;

    public FichasServicio(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 2. Devuelve el rol de la cuenta en una granja confirmada, o null si no tiene acceso.
    /// </summary>
    public Task<string?> RolEnGranja(Guid cuentaId, Guid granjaId) =>
        _context.Database.SqlQuery<string>($"""
            select v.rol::text as "Value"
            from public.vinculaciones v
            join public.cuentas c on c.id = v.cuenta_id
            join public.granjas g on g.id = v.granja_id
            where v.cuenta_id = {cuentaId} and v.granja_id = {granjaId}
              and v.activa and c.activa and g.confirmada
            """).FirstOrDefaultAsync();

    /// <summary>3. RF-02: el dueño y el trabajador gestionan los datos generales.</summary>
    public static bool PuedeGestionar(string? rol) => rol is Dueno or Trabajador;

    /// <summary>4. Solo el dueño da de baja un animal.</summary>
    public static bool PuedeDarDeBaja(string? rol) => rol == Dueno;

    /// <summary>
    /// 5. Indica si el animal existe y sigue activo; un animal dado de baja no admite nuevos eventos.
    /// </summary>
    public Task<bool> AdmiteEventos(Guid fichaId) =>
        _context.FichasGanado.AnyAsync(f => f.Id == fichaId && f.FechaBaja == null);
}
