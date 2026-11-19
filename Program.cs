using ApiWeb.Data;
using ApiWeb.Integrante1_Fichas.Models;
using ApiWeb.Integrante1_Fichas.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// Configuración de Conexión a Base de Datos (Supabase / Render)
// =============================================================================
var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("SupabaseConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

string connectionString = rawConnectionString ?? "Host=localhost;Port=5432;Database=VetLinkDb;Username=postgres;Password=postgres";

// Conversión para URL en formato postgres:// o postgresql:// (la que copia Supabase en Connect)
if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':', 2);
    // El constructor escapa la contraseña aunque traiga caracteres como ; o =.
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

builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Integrante 1: la columna etapa usa el enum etapa de Postgres.
    options.UseNpgsql(connectionString, npgsql => npgsql.MapEnum<EtapaFicha>("etapa"));
});

// Integrante 1: reglas de acceso a las fichas, también disponibles para los demás módulos.
builder.Services.AddScoped<FichasServicio>();

// =============================================================================
// Autenticación con los tokens de Supabase Auth
// =============================================================================
// La app envía el token de la sesión en Authorization: Bearer; aquí se valida con las claves públicas del proyecto.
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

// =============================================================================
// Servicios de Controladores y Swagger
// =============================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de CORS para frontend / Render
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

// =============================================================================
// Pipeline HTTP
// =============================================================================
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // También disponible en producción para pruebas en Render
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// =============================================================================
// Mapeo de Controladores y Endpoints
// =============================================================================
app.MapControllers();

// Endpoint de verificación (útil para Render Health Check)
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.Run();
