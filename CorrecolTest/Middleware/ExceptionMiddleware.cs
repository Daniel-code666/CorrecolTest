using System.Diagnostics;
using CorrecolTest.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CorrecolTest.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // El consumidor cerró la conexión; no intentar escribir una respuesta.
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted) throw;
            var (status, title) = ex switch
            {
                ValidationException => (400, "Datos inválidos"),
                NotFoundException => (404, "Recurso no encontrado"),
                ConflictException => (409, "Conflicto"),
                BadHttpRequestException => (400, "Solicitud inválida"),
                _ => (500, "Error interno")
            };
            if (status == 500) logger.LogError(ex, "Error no controlado. TraceId: {TraceId}", context.TraceIdentifier);
            else logger.LogWarning("Solicitud rechazada con estado {Status}: {Message}", status, ex.Message);

            context.Response.Clear();
            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = ex is AppException ? ex.Message : status == 500
                    ? "Ocurrió un error inesperado al procesar la solicitud." : "La solicitud no es válida.",
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
            await Results.Problem(problem).ExecuteAsync(context);
        }
    }
}

