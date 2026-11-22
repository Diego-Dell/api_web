using System.ComponentModel.DataAnnotations;

namespace ApiWeb.DTOs;

// 1. Solo el nombre se edita desde la API; correo y contraseña los maneja Supabase Auth.
public sealed class ActualizarCuentaDto
{
    [Required, StringLength(120, MinimumLength = 1)] public string Nombre { get; set; } = string.Empty;
}

public sealed record CuentaDto(Guid Id, string Identificador, string? Nombre, bool Activa, bool AdministradorTecnico);
