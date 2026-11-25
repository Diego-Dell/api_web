using System.Globalization;
using System.Text.RegularExpressions;

namespace ApiWeb.Controllers;

// 1. Reglas comunes de dosis: unidades permitidas y total de dosis de un tratamiento.
public static partial class IndicacionesVetLink
{
    public static readonly string[] Unidades = ["ml", "mg", "g", "UI", "mg/kg", "ml/kg", "comprimido", "bolo", "pomo"];

    public static string? UnidadValida(string? unidad) =>
        Unidades.FirstOrDefault(u => string.Equals(u, unidad?.Trim(), StringComparison.OrdinalIgnoreCase));

    // 2. "Cada" en horas ("24 h") y "Duración" en días ("5 días"); "Dosis única" equivale a 1.
    public static int? TotalDosis(string? frecuencia, string? duracion)
    {
        if (frecuencia is not null && frecuencia.Contains("única", StringComparison.OrdinalIgnoreCase)) return 1;
        var horas = Horas(frecuencia, 1);
        var dias = Horas(duracion, 24);
        if (horas is not > 0 || dias is not > 0) return null;
        return Math.Max(1, (int)Math.Ceiling(dias.Value / horas.Value));
    }

    public static bool FormatoCada(string? texto) => CadaRegex().IsMatch(texto?.Trim() ?? "") || (texto?.Contains("única", StringComparison.OrdinalIgnoreCase) ?? false);
    public static bool FormatoDuracion(string? texto) => DuracionRegex().IsMatch(texto?.Trim() ?? "");

    private static double? Horas(string? texto, double porDefecto)
    {
        var m = NumeroRegex().Match(texto ?? "");
        if (!m.Success) return null;
        var n = double.Parse(m.Groups[1].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
        var u = m.Groups[2].Value.ToLowerInvariant();
        return u.StartsWith('h') ? n : u.StartsWith('d') ? n * 24 : u.StartsWith('s') ? n * 24 * 7 : n * porDefecto;
    }

    [GeneratedRegex(@"(\d+(?:[.,]\d+)?)\s*([a-záéíóú]*)", RegexOptions.IgnoreCase)]
    private static partial Regex NumeroRegex();
    [GeneratedRegex(@"^\d{1,3}\s*h$", RegexOptions.IgnoreCase)]
    private static partial Regex CadaRegex();
    [GeneratedRegex(@"^\d{1,3}\s*d[ií]as?$", RegexOptions.IgnoreCase)]
    private static partial Regex DuracionRegex();
}
