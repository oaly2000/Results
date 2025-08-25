using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OALY2000.Results.Exceptions;

namespace OALY2000.Results.AspNetCore;

public class ResultEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);

        if (result is UnsafeResult unsafeResult)
        {
            if (UnsafeResultAccessor.GetIsSuccess(unsafeResult)) return UnsafeResultAccessor.GetValue(unsafeResult);

            ref var error = ref UnsafeResultAccessor.GetError(unsafeResult);
            if (error is IKnownException knownException)
            {
                knownException.LogTo(context.HttpContext.RequestServices.GetRequiredService<ILogger<ResultEndpointFilter>>());
                return knownException.ToProblemHttpResult();
            }

            throw error;
        }

        return result;
    }
}

file static class UnsafeResultAccessor
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_value")]
    public static extern ref object GetValue(UnsafeResult unsafeResult);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_isSuccess")]
    public static extern ref bool GetIsSuccess(UnsafeResult unsafeResult);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_error")]
    public static extern ref Exception GetError(UnsafeResult unsafeResult);
}
