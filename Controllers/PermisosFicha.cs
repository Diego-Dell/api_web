using ApiWeb.Models;

namespace ApiWeb.Controllers;

public static class PermisosFicha
{
    // El rol pertenece a la granja de la ficha, nunca al cuerpo de la solicitud.
    public static bool PuedeCambiarEtapa(string? rol) => rol is RolesVetLink.Propietario or RolesVetLink.Veterinario or RolesVetLink.Administrador;
    public static bool PuedeEditar(string? rol, Etapa anterior, Etapa nueva) =>
        rol is RolesVetLink.Propietario or RolesVetLink.Administrador || (rol == RolesVetLink.Trabajador && anterior == nueva);
}
