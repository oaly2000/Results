using System.Net;
using Microsoft.Extensions.Logging;

namespace OALY2000.Results.Exceptions;

public class UnfiledException : Exception, IKnownException
{
    public HttpStatusCode HttpStatus => HttpStatusCode.BadRequest;

    public LogLevel LogLevel => LogLevel.Warning;

    public string Code => "UNFILED_EXCEPTION";

    public string? Summary { get; set; }

    public UnfiledException() { }
    public UnfiledException(string message) : base(message) { }
    public UnfiledException(string message, Exception inner) : base(message, inner) { }
}