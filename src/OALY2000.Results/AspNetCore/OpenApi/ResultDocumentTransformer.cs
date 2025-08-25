using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace OALY2000.Results.AspNetCore.OpenApi;

public class ResultDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var group in context.DescriptionGroups)
        {
            foreach (var item in group.Items)
            {
                foreach (var responseType in item.SupportedResponseTypes)
                {
                    var clrType = responseType.Type;
                    if (clrType is { } && clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Result<>))
                    {
                        var actualType = clrType.GetGenericArguments()[0];
                        responseType.Type = actualType;

                        // NOTE: temporary fix
                        if (context.ApplicationServices.GetService<IModelMetadataProvider>() is { } metadataProvider)
                        {
                            responseType.ModelMetadata = metadataProvider.GetMetadataForType(actualType);
                        }
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}

