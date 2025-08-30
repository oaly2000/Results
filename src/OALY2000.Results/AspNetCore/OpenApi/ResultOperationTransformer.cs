using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace OALY2000.Results.AspNetCore.OpenApi;

public class ResultOperationTransformer(IOptions<JsonOptions> jsonOptions, IOptionsMonitor<OpenApiOptions> openApiOptionsMonitor) : IOpenApiOperationTransformer
{
    JsonSerializerOptions JsonSerializerOptions => jsonOptions.Value.SerializerOptions;
    OpenApiOptions OpenApiOptions => openApiOptionsMonitor.Get("v1");

    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var responseType in context.Description.SupportedResponseTypes)
        {
            var clrType = responseType.Type;
            if (clrType is { } && clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var actualType = clrType.GetGenericArguments()[0];

                var key = actualType == typeof(string) ? "text/plain" : "application/json";

                IOpenApiSchema schema;
                if (actualType.HasElementType)
                {
                    var _schema = await context.GetOrCreateSchemaAsync(actualType, cancellationToken: cancellationToken);
                    var referenceId = OpenApiOptions.CreateSchemaReferenceId(JsonTypeInfo.CreateJsonTypeInfo(actualType.GetElementType()!, JsonSerializerOptions))!;
                    _schema.Items = new OpenApiSchemaReference(referenceId);
                    schema = _schema;
                }
                else if (actualType.IsPrimitive || actualType == typeof(string))
                {
                    schema = await context.GetOrCreateSchemaAsync(actualType, cancellationToken: cancellationToken);
                }
                else
                {
                    schema = new OpenApiSchemaReference(OpenApiOptions.CreateSchemaReferenceId(JsonTypeInfo.CreateJsonTypeInfo(actualType, JsonSerializerOptions))!);
                }

                operation.Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Content = new Dictionary<string, OpenApiMediaType>()
                        {
                            [key] = new OpenApiMediaType { Schema = schema }
                        }
                    }
                };

                if (context.Document?.Components?.Schemas is { } schemas)
                {
                    foreach (var t in new[] { clrType, typeof(Exception), typeof(MethodBase) })
                    {
                        var referenceId = OpenApiOptions.CreateSchemaReferenceId(JsonTypeInfo.CreateJsonTypeInfo(t, JsonSerializerOptions))!;
                        schemas.Remove(referenceId);
                    }
                }
            }
        }
    }
}
