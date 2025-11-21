using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Exceptions.Custom;

namespace TaskManager.Api.Exceptions.Handlers;

internal sealed class ForbiddenExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ForbiddenExceptionHandler> _logger;
    public ForbiddenExceptionHandler(ILogger<ForbiddenExceptionHandler> logger)
    {
        _logger = logger;
    }
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ForbiddenException forbiddenException)
        {
            return false;
        }

        _logger.LogWarning(forbiddenException, "Forbidden access: {Message}", forbiddenException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = forbiddenException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

