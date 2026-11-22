using ApiWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Controllers;

internal static class ClinicaValidaciones
{
    internal static async Task<bool> VeterinarioValidoAsync(AppDbContext db, Guid cuentaId,
        Guid granjaId, CancellationToken ct) =>
        await db.Cuentas.AnyAsync(c => c.Id == cuentaId && c.Activa, ct) &&
        await db.Vinculaciones.AnyAsync(v => v.CuentaId == cuentaId && v.GranjaId == granjaId &&
            v.Activa && v.Rol == RolesVetLink.Veterinario, ct);

    internal static bool FechaValida(DateTime fecha) => fecha != default;
}
