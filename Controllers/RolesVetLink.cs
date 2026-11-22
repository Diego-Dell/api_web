namespace ApiWeb.Controllers;

internal static class RolesVetLink
{
    public const string Propietario = "Propietario";
    public const string Administrador = "Administrador";
    public const string Veterinario = "Veterinario";
    public const string Trabajador = "Trabajador";

    // 1. Supabase guarda dueno, veterinario y trabajador; el administrador técnico es una marca de la cuenta.
    private static readonly string[] RolesValidos = [Propietario, Veterinario, Trabajador];

    public static string? Canonicalizar(string? rol) =>
        RolesValidos.FirstOrDefault(x => string.Equals(x, rol?.Trim(), StringComparison.OrdinalIgnoreCase));

    public static bool PuedeInvitar(string rol) => rol is Propietario or Administrador;
    public static bool PuedeProgramarPrevencion(string rol) => rol == Veterinario;
    public static bool PuedeAplicar(string rol) => rol is Trabajador or Veterinario;
}
