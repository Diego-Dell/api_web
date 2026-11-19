using ApiWeb.Data;
using ApiWeb.Integrante1_Fichas.Models;
using ApiWeb.Integrante1_Fichas.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Lee la conexión a la base de datos (Supabase o Render); DATABASE_URL tiene prioridad.
var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("SupabaseConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

string connectionString = rawConnectionString ?? "Host=localhost;Port=5432;Database=VetLinkDb;Username=postgres;Password=postgres";

// 2. Convierte la URL postgres:// o postgresql:// (la que copia Supabase en Connect) al formato de Npgsql.
if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':', 2);

    // 2.1. El constructor escapa la contraseña aunque traiga caracteres como ; o =.
    connectionString = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo.Length > 1 ? userInfo[1] : string.Empty),
        SslMode = Npgsql.SslMode.Require,
    }.ConnectionString;
}

// 3. Registra el contexto de datos; la columna etapa de las fichas usa el enum etapa de Postgres.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql => npgsql.MapEnum<EtapaFicha>("etapa"));
});

// 4. Registra las reglas de acceso a las fichas, también disponibles para los demás módulos.
builder.Services.AddScoped<FichasServicio>();

// 5. Valida el token de Supabase Auth que la app envía en Authorization: Bearer, con las claves públicas del proyecto.
var supabaseUrl = (Environment.GetEnvironmentVariable("SUPABASE_URL") ?? builder.Configuration["Supabase:Url"])?.TrimEnd('/')
    ?? throw new InvalidOperationException("Falta la URL de Supabase (Supabase:Url o SUPABASE_URL).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{supabaseUrl}/auth/v1";
        options.RequireHttpsMetadata = supabaseUrl.StartsWith("https://");
        options.MapInboundClaims = false;
        options.TokenValidationParameters.ValidAudience = "authenticated";
    });
builder.Services.AddAuthorization();

// 6. Registra los controladores y Swagger.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 7. Permite llamadas desde cualquier origen (frontend o Render).
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 8. Arma el pipeline HTTP.
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // 8.1. Swagger también queda disponible en producción para probar en Render.
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// 9. Mapea los controladores de los cuatro integrantes.
app.MapControllers();

// 10. Responde la verificación de salud que usa Render.
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.Run();
