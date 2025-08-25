using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OALY2000.Results.Exceptions;

public interface IKnownException
{
    HttpStatusCode HttpStatus { get; }

    LogLevel LogLevel { get; }

    string Code { get; }

    string? Summary { get; }

    string Message { get; }

    ProblemDetails ToProblemDetails() => new()
    {
        Status = (int)HttpStatus,
        Type = Code,
        Title = Summary,
        Detail = Message
    };

    ProblemHttpResult ToProblemHttpResult() => TypedResults.Problem(statusCode: (int)HttpStatus, type: Code, title: Summary, detail: Message);

    void LogTo(ILogger logger)
    {
        if (LogLevel == LogLevel.None || LogLevel == LogLevel.Trace) return;

        logger.Log(LogLevel, this as Exception, "A known exception({Code}) occurred: {Summary}", Code, Summary);
    }
}
