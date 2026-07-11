<h1 align="center">Atya.Web.Middleware</h1>

<p align="center"><i>ASP.NET Core HTTP-edge adapters for Atya exceptions, Results, and RFC 9457 ProblemDetails.</i></p>

<p align="center">
  <a href="https://www.nuget.org/packages/Atya.Web.Middleware"><img src="https://img.shields.io/nuget/v/Atya.Web.Middleware?style=for-the-badge&logo=nuget&logoColor=white&label=NuGet&color=512BD4" alt="NuGet Version"></a>
  <a href="https://www.nuget.org/packages/Atya.Web.Middleware"><img src="https://img.shields.io/nuget/dt/Atya.Web.Middleware?style=for-the-badge&logo=nuget&logoColor=white&label=Downloads&color=512BD4" alt="NuGet Downloads"></a>
  <img src="https://img.shields.io/badge/.NET%2010.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="Target Framework">
  <a href="LICENSE"><img src="https://img.shields.io/github/license/AtyaLibraries/Middleware?style=for-the-badge&color=512BD4" alt="License"></a>
  <a href="https://github.com/AtyaLibraries/Middleware/actions"><img src="https://img.shields.io/github/actions/workflow/status/AtyaLibraries/Middleware/ci.yml?branch=development&style=for-the-badge&logo=githubactions&logoColor=white&label=Build&color=512BD4" alt="Build"></a>
</p>

---

## Overview

`Atya.Web.Middleware` is the ASP.NET Core adoption surface for Atya error handling. It wires the
`Atya.Errors.ProblemDetails` exception handler into the request pipeline and provides endpoint-boundary
helpers that convert `Result`, `Result<T>`, and `Error` values into ASP.NET Core `IResult` responses.

The package does not own RFC 9457 mapping rules. Exception taxonomy mapping and Result/Error mapping stay
inside `Atya.Errors.ProblemDetails`; this package calls those mappers exactly once at the HTTP edge.

## Features

- **Exception pipeline hook** - call `UseAtyaWebMiddleware()` to write mapped ProblemDetails responses for Atya exception taxonomy failures.
- **Result boundary helpers** - convert `Result`, `Result<T>`, or `Error` values to HTTP responses through `IResultToProblemDetailsMapper`.
- **Validation details passthrough** - validation targets and child details ride through `Error.Target` and `Error.Details`.
- **Small dependency surface** - built on ASP.NET Core abstractions plus Atya Guards, Exceptions, and ProblemDetails.

## Installation

**.NET CLI**
```bash
dotnet add package Atya.Web.Middleware
```

**Package Manager**
```powershell
Install-Package Atya.Web.Middleware
```

**PackageReference**
```xml
<PackageReference Include="Atya.Web.Middleware" Version="1.0.0" />
```

## Quick Start

```csharp
using Atya.Foundation.Results;
using Atya.Web.Middleware.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAtyaWebMiddleware();

var app = builder.Build();

app.UseAtyaWebMiddleware();

app.MapGet(
    "/customers/{id}",
    (string id, HttpContext httpContext) =>
    {
        Result<string> result = id == "42"
            ? Result.Success("Ada Lovelace")
            : Result.Failure<string>(
                "customers.not_found",
                "Customer was not found.",
                ErrorKind.NotFound);

        return result.ToHttpResult(httpContext);
    });

app.Run();
```

## Feature Tour

### Register the HTTP-edge services

```csharp
using Atya.Web.Middleware.Extensions;

builder.Services.AddAtyaWebMiddleware(
    configureMiddleware: options =>
    {
        options.CorrelationIdHeaderName = "X-Correlation-ID";
    },
    configureProblemDetails: options =>
{
    options.IncludeTraceId = true;
});
```

`AddAtyaWebMiddleware` registers the correlation middleware options and delegates RFC 9457 mapping to
`AddAtyaProblemDetails`, which registers the exception and Result mappers owned by
`Atya.Errors.ProblemDetails`.

### Add the exception middleware

```csharp
using Atya.Web.Middleware.Extensions;

app.UseAtyaWebMiddleware();
```

This accepts an incoming correlation id from the configured header, generates one when the request does not
provide it, echoes the value on the response, and composes the Atya ProblemDetails exception handler into
the ASP.NET Core pipeline. The same correlation id is exposed to `Atya.Errors.ProblemDetails`, so mapped
ProblemDetails responses include it in the configured correlation extension.

### Return Results from endpoints

```csharp
using Atya.Foundation.Results;
using Atya.Web.Middleware.Extensions;

static IResult GetCustomer(HttpContext httpContext)
{
    Result<string> result = Result.Failure<string>(
        "customers.not_found",
        "Customer was not found.",
        ErrorKind.NotFound);

    return result.ToHttpResult(httpContext);
}
```

Success is part of the public API contract: successful untyped `Result` values become `204 No Content`;
successful `Result<TValue>` values become `200 OK` with the JSON success value. Failures delegate to
`IResultToProblemDetailsMapper`. Overloads that accept an explicit mapper are available for advanced tests
and manually wired endpoints; the `HttpContext`-only overloads resolve the mapper from `RequestServices`.

### Return validation details through Error

```csharp
using Atya.Foundation.Results;

var error = new Error(
    "customers.invalid",
    "Customer is invalid.",
    target: null,
    details:
    [
        new Error("customers.email.required", "Email is required.", "email", kind: ErrorKind.Validation),
        new Error("customers.email.format", "Email is invalid.", "email", kind: ErrorKind.Validation)
    ],
    kind: ErrorKind.Validation);
```

No separate validation mapping dependency is required. `Atya.Errors.ProblemDetails` reads validation targets
and details from the Result/Error model.

## Error Codes

This package does not define error codes. It preserves the codes carried by `Atya.Foundation.Results.Error`
and the Atya exception taxonomy when those values are mapped by `Atya.Errors.ProblemDetails`.

## Why These Dependencies

- `Atya.Errors.ProblemDetails` owns RFC 9457 mapping and the ASP.NET Core exception handler.
- `Atya.Errors.Exceptions` supplies the exception taxonomy consumed by ProblemDetails.
- `Atya.Foundation.Guards` validates public entry points consistently across Atya packages.

## Compatibility

Targets `net10.0`.

## Testing

```bash
dotnet test
```

## Benchmarks

Performance benchmarks live in `benchmarks/`. Run them from the benchmark project:

```bash
dotnet run -c Release --project benchmarks/Middleware.Benchmarks/Middleware.Benchmarks.csproj
```

## Links

- NuGet: https://www.nuget.org/packages/Atya.Web.Middleware
- Repository: https://github.com/AtyaLibraries/Middleware
- Samples: https://github.com/AtyaLibraries/Middleware/tree/development/samples
- License: https://github.com/AtyaLibraries/Middleware/blob/development/LICENSE
- Atya Libraries: https://github.com/AtyaLibraries

## License

Released under the MIT license. See [LICENSE](LICENSE) for details.
