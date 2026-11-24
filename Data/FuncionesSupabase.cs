using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace ApiWeb.Data;

// 1. Llama funciones de Supabase (guardar_indicacion_tratamiento, programar_prevencion...)
//    con la misma conexión de EF, así corren como la persona del token.
public static class FuncionesSupabase
{
    public static async Task<object?> LlamarAsync(this AppDbContext db, string sql, CancellationToken ct,
        params (string Nombre, object? Valor, NpgsqlDbType Tipo)[] parametros)
    {
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await using var comando = (NpgsqlCommand)db.Database.GetDbConnection().CreateCommand();
            comando.CommandText = sql;
            foreach (var (nombre, valor, tipo) in parametros)
                comando.Parameters.Add(new NpgsqlParameter(nombre, tipo) { Value = valor ?? DBNull.Value });
            var resultado = await comando.ExecuteScalarAsync(ct);
            return resultado is DBNull ? null : resultado;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    // 2. Cuenta de quien llama, tomada del token validado (nunca del cuerpo del pedido).
    public static Guid CuentaId(this ClaimsPrincipal usuario) =>
        Guid.TryParse(usuario.FindFirst("sub")?.Value, out var id) ? id : Guid.Empty;
}
