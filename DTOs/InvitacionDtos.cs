using System.ComponentModel.DataAnnotations;

namespace ApiWeb.DTOs;

public sealed class CrearInvitacionDto
{
    public Guid GranjaId { get; set; }
    public Guid EmitidaPorCuentaId { get; set; }
    [Required, StringLength(25)] public string Rol { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(254)] public string CorreoDestinatario { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
}

public sealed class ActualizarInvitacionDto
{
    public Guid GranjaId { get; set; }
    public Guid EmitidaPorCuentaId { get; set; }
    [Required, StringLength(25)] public string Rol { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(254)] public string CorreoDestinatario { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public bool Vigente { get; set; }
}

public sealed record InvitacionDto(Guid Id, Guid GranjaId, Guid? EmitidaPorCuentaId, string Rol,
    bool Vigente, bool Aceptada, string CorreoDestinatario, DateTime? FechaVencimiento);
