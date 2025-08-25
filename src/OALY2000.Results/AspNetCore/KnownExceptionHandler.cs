using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OALY2000.Results.Exceptions;

namespace OALY2000.Results.AspNetCore;

public class KnownExceptionHandler(ILogger<KnownExceptionHandler> logger, IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is IKnownException knownException)
        {
            knownException.LogTo(logger);

            httpContext.Response.StatusCode = (int)knownException.HttpStatus;

            return await problemDetailsService.TryWriteAsync(new()
            {
                Exception = exception,
                HttpContext = httpContext,
                ProblemDetails = knownException.ToProblemDetails()
            });
        }

        logger.LogError(exception, "An unhandled exception({Name}) occurred: {Message}", exception.GetType().Name, exception.Message);

        return false;
    }
}
