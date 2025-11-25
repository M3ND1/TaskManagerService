using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.Exceptions;

namespace TaskManager.Api.Exceptions.Handlers;

internal sealed class DatabaseOperationExceptionHandler(ILogger<DatabaseOperationExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<DatabaseOperationExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DatabaseOperationException databaseException)
        {
            return false;
        }

        _logger.LogError(exception, "Database operation failed: {Message}", databaseException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Database Operation Failed",
            Detail = "An error occurred while performing a database operation. Please try again later."
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
