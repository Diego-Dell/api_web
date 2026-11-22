using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ApiWeb.Data;

// 1. Cada conexión toma la identidad de quien llama: rol "authenticated" y los datos de su token.
//    Así PostgreSQL aplica las mismas políticas RLS que usa la app con Supabase.
public sealed class SesionSupabase(IHttpContextAccessor http) : DbConnectionInterceptor
{
    public const string ClaveDatosToken = "supabase.jwt.claims";

    public override void ConnectionOpened(DbConnection conexion, ConnectionEndEventData datos)
    {
        using var comando = Preparar(conexion);
        comando.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(DbConnection conexion, ConnectionEndEventData datos,
        CancellationToken ct = default)
    {
        await using var comando = Preparar(conexion);
        await comando.ExecuteNonQueryAsync(ct);
    }

    // 2. Sin token válido la conexión queda como "anon", que no ve datos privados.
    private DbCommand Preparar(DbConnection conexion)
    {
        var claims = http.HttpContext?.Items[ClaveDatosToken] as string;
        var comando = conexion.CreateCommand();
        comando.CommandText = "select set_config('request.jwt.claims', @claims, false), set_config('role', @rol, false)";
        Agregar(comando, "claims", claims ?? "{}");
        Agregar(comando, "rol", claims is null ? "anon" : "authenticated");
        return comando;
    }

    private static void Agregar(DbCommand comando, string nombre, string valor)
    {
        var parametro = comando.CreateParameter();
        parametro.ParameterName = nombre;
        parametro.Value = valor;
        comando.Parameters.Add(parametro);
    }
}
