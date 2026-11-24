using System.Text;
using System.Text.Json.Serialization;
using ApiWeb.Data;
using ApiWeb.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 1. Datos de Supabase: variables de entorno en Render, appsettings.Development.json en local.
var supabaseUrl = (Environment.GetEnvironmentVariable("SUPABASE_URL")
    ?? builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException("Falta SUPABASE_URL")).TrimEnd('/');
var conexion = Conexion(Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("Supabase")
    ?? throw new InvalidOperationException("Falta DATABASE_URL"));

// 2. PostgreSQL de Supabase con sus tipos enumerados.
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<SesionSupabase>();
builder.Services.AddDbContext<AppDbContext>((servicios, opciones) => opciones
    .UseNpgsql(conexion, npgsql => npgsql
        .MapEnum<Etapa>("etapa")
        .MapEnum<TipoAtencion>("tipo_atencion")
        .MapEnum<TipoEvento>("tipo_evento")
        .MapEnum<Aptitud>("aptitud")
        .MapEnum<Prenez>("prenez")
        .MapEnum<TipoPrevencion>("tipo_prevencion")
        .MapEnum<EstadoPrevencion>("estado_prevencion")
        .MapEnum<RolGranja>("rol_granja")
        .MapEnum<RolInvitado>("rol_invitado"))
    .AddInterceptors(servicios.GetRequiredService<SesionSupabase>()));

// 3. Solo entra quien tiene una sesión de Supabase: se valida su token con las llaves públicas del proyecto.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opciones =>
{
    opciones.Authority = $"{supabaseUrl}/auth/v1";
    opciones.TokenValidationParameters.ValidAudience = "authenticated";
    opciones.MapInboundClaims = false;
    opciones.Events = new JwtBearerEvents
    {
        // 4. Guarda los datos del token para que la base aplique las políticas RLS de esa persona.
        OnTokenValidated = contexto =>
        {
            if (contexto.SecurityToken is JsonWebToken token)
                contexto.HttpContext.Items[SesionSupabase.ClaveDatosToken] =
                    Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(token.EncodedPayload));
            return Task.CompletedTask;
        },
    };
});
builder.Services.AddAuthorization(opciones => opciones.FallbackPolicy =
    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// 5. Los enumerados viajan como texto ("Vaca", "Pendiente"), no como números.
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    // 6. Swagger pide el token de Supabase ("Authorize") para probar los endpoints.
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token de acceso de Supabase (access_token de la sesión).",
    });
    opciones.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento)] = [],
    });
});
builder.Services.AddCors(opciones => opciones.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.Configure<ForwardedHeadersOptions>(opciones =>
{
    opciones.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    opciones.KnownIPNetworks.Clear();
    opciones.KnownProxies.Clear();
});

var app = builder.Build();

// 7. Render atiende HTTPS y reenvía la petición a la API.
app.UseForwardedHeaders();
app.UseExceptionHandler(errores => errores.Run(async contexto =>
{
    // 8. Traduce los rechazos de PostgreSQL a respuestas claras.
    var error = contexto.Features.Get<IExceptionHandlerFeature>()?.Error;
    var postgres = error as PostgresException ?? error?.InnerException as PostgresException;
    (int estado, string mensaje) = postgres?.SqlState switch
    {
        PostgresErrorCodes.InsufficientPrivilege => (403, "Tu rol no permite esta operación."),
        PostgresErrorCodes.UniqueViolation => (409, "Ya existe un registro con esos datos."),
        PostgresErrorCodes.ForeignKeyViolation => (400, "Uno de los registros relacionados no existe o no es de la misma granja."),
        PostgresErrorCodes.CheckViolation or PostgresErrorCodes.NotNullViolation => (400, "Hay datos obligatorios vacíos o fuera de rango."),
        PostgresErrorCodes.RaiseException => (400, postgres!.MessageText),
        _ => (500, "Error interno de la API."),
    };
    contexto.Response.StatusCode = estado;
    await contexto.Response.WriteAsJsonAsync(new { error = mensaje });
}));
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 9. Rutas públicas: estado del servicio y la página de inicio, que abre Swagger.
app.MapGet("/health", () => Results.Ok(new { estado = "ok" })).AllowAnonymous();

// 10. Revisa la conexión con Supabase y que el script de la API esté aplicado (sin mostrar datos).
app.MapGet("/health/base", async (AppDbContext db) =>
{
    try
    {
        await db.Database.ExecuteSqlRawAsync("select 1");
    }
    catch (Exception e)
    {
        // 11. Solo el tipo de fallo (código de PostgreSQL o clase de error), nunca la cadena de conexión.
        var causa = e is PostgresException p ? $"{p.SqlState} {p.MessageText}"
            : e.InnerException is PostgresException q ? $"{q.SqlState} {q.MessageText}"
            : $"{e.GetType().Name}: {(e.InnerException ?? e).GetType().Name}";
        return Results.Json(new { base_de_datos = "sin conexión", causa }, statusCode: 503);
    }
    try
    {
        // 12. Como "anon": si la columna existe falta permiso (42501); si no existe, falta el script (42703).
        await db.Database.ExecuteSqlRawAsync("select version from public.fichas_bovinas limit 0");
        return Results.Ok(new { base_de_datos = "conectada", script_api = "aplicado" });
    }
    catch (PostgresException e)
    {
        return Results.Ok(new
        {
            base_de_datos = "conectada",
            script_api = e.SqlState == PostgresErrorCodes.UndefinedColumn ? "falta aplicar 2026-11-22_api_web.sql" : "aplicado",
        });
    }
}).AllowAnonymous();
app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();

app.Run();

// 13. Acepta la cadena de Supabase en formato URI (postgresql://...) o clave=valor.
static string Conexion(string valor)
{
    valor = valor.Trim();
    var builder = new NpgsqlConnectionStringBuilder();
    if (valor.StartsWith("postgres://") || valor.StartsWith("postgresql://"))
    {
        // 14. Se separa a mano: la contraseña puede tener @ : / # ? sin codificar.
        var resto = valor[(valor.IndexOf("://") + 3)..];
        var arroba = resto.LastIndexOf('@');
        var credenciales = resto[..arroba];
        var servidor = resto[(arroba + 1)..];
        var dosPuntos = credenciales.IndexOf(':');
        var barra = servidor.IndexOf('/');
        var direccion = barra < 0 ? servidor : servidor[..barra];
        var baseDatos = barra < 0 ? "postgres" : servidor[(barra + 1)..].Split('?')[0];
        var puerto = direccion.LastIndexOf(':');
        builder.Host = puerto < 0 ? direccion : direccion[..puerto];
        builder.Port = puerto < 0 ? 5432 : int.Parse(direccion[(puerto + 1)..]);
        builder.Database = baseDatos.Length == 0 ? "postgres" : baseDatos;
        builder.Username = Uri.UnescapeDataString(dosPuntos < 0 ? credenciales : credenciales[..dosPuntos]);
        builder.Password = dosPuntos < 0 ? null : Desescapar(credenciales[(dosPuntos + 1)..]);
    }
    else builder.ConnectionString = valor;

    // 15. Supabase exige SSL; Kerberos (GSS) no se usa y su librería no está en la imagen.
    builder.SslMode = SslMode.Require;
    builder.GssEncryptionMode = GssEncryptionMode.Disable;
    return builder.ConnectionString;
}

// 16. Decodifica %XX solo si la contraseña venía codificada; si no, la deja igual.
static string Desescapar(string texto)
{
    try { return Uri.UnescapeDataString(texto); }
    catch (UriFormatException) { return texto; }
}
