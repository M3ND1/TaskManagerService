using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.Exceptions;

namespace TaskManager.Api.Exceptions.Handlers;

internal sealed class ConcurrencyConflictExceptionHandler(ILogger<ConcurrencyConflictExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<ConcurrencyConflictExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ConcurrencyConflictException concurrencyException)
        {
            return false;
        }

        _logger.LogWarning(exception, "Concurrency conflict: {Message}", concurrencyException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Concurrency Conflict",
            Detail = concurrencyException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
