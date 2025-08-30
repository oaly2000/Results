# Results

### Installation

![NuGet Version](https://img.shields.io/nuget/v/OALY2000.Results)

### Basic usage

The library defines implicit operators to convert from `T` to `Result<T>` and from `Exception` to `Result<T>`.

To simplify the usage of result pattern, LINQ syntax is available.

```cs
Result<int> number1 = 10;
Result<int> number2 = 0;
Result<int> invalideNumber = new Exception("Invalid number");

var result =
    from n1 in number1
    from n2 in number2
    from n3 in invalideNumber
    select n1 + n2 + n3;

if (result.IsSuccess)
{
    Console.WriteLine(result.Value);
}
else throw result.Error;
```

### AspNetCore Integration

An interface `IKnownException` is defined to provide a common way to handle exceptions. It will handle loggings、error responses. To avoid duplicated loggings, the default exception handler middleware logger should be disabled.

```diff
"LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning",
+   "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware": "None"
},
```

Then, modify Program.cs:

```csharp
builder.Services.AddExceptionHandler<KnownExceptionHandler>();
// ...
app.UseExceptionHandler();
```

Returning a `Result<T>` from a minimal api endpoint(not AOT) is also supported. The `ResultEndpointFilter` depends on `IKnownException` to handle exceptions, so above configuration should be added before adding the filter.

```csharp
var api = app.MapGroup("/api").AddEndpointFilter<ResultEndpointFilter>();
```

The actual response type is `T` when successful, and `ProblemDetails` when failed.

### OpenApi Integration

After add `ResultEndpointFilter`, we should configures openapi document generation.

```csharp
builder.Services.AddOpenApi(options => options.AddOperationTransformer<ResultOperationTransformer>());
```
