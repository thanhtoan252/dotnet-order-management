using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace OrderManagement.Api.Middleware;

public class GlobalExceptionHandler(IWebHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Concurrency conflict",
                "The resource was modified by another request. Refresh the data and retry."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                exception.Message)
        };

        if (exception is DbUpdateConcurrencyException)
        {
            Log.Warning("Concurrency conflict on {Path}", httpContext.Request.Path);
        }
        else
        {
            Log.Error(exception,
                "Exception {ExceptionType} handled. StatusCode: {StatusCode}. Path: {Path}",
                exception.GetType().Name, statusCode, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            StatusCode = statusCode,
            Title = title,
            Detail = detail,
            TraceId = httpContext.TraceIdentifier,
            StackTrace = env.IsDevelopment() ? exception.StackTrace : null
        }, cancellationToken);

        return true;
    }
}