using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiWeb.Filtros;

// 1. Todo error de la API sale como { "error": "..." } para que la app muestre el motivo real.
public sealed class ErroresUniformes : IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext contexto)
    {
        if (contexto.Result is ObjectResult { Value: string mensaje } r && (r.StatusCode ?? 200) >= 400)
            contexto.Result = new ObjectResult(new { error = mensaje }) { StatusCode = r.StatusCode };
    }

    public void OnResultExecuted(ResultExecutedContext contexto) { }

    // 2. Datos inválidos del modelo: se devuelve el primer mensaje legible.
    public static IActionResult ModeloInvalido(ActionContext contexto)
    {
        var mensaje = contexto.ModelState.Values.SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Revisá los datos enviados." : e.ErrorMessage)
            .FirstOrDefault() ?? "Revisá los datos enviados.";
        return new BadRequestObjectResult(new { error = mensaje });
    }
}
