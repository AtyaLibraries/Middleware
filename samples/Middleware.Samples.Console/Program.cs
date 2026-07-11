using Atya.Errors.ProblemDetails.Abstractions;
using Atya.Foundation.Results;
using Atya.Web.Middleware.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Atya.Web.Middleware.Samples.ConsoleApp;

/// <summary>
/// Runs the sample console application.
/// </summary>
public static class Program
{
    /// <summary>
    /// Demonstrates converting a failed Result at the HTTP boundary.
    /// </summary>
    public static void Main()
    {
        using var provider = new ServiceCollection()
            .AddAtyaWebMiddleware()
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Path = "/customers/42"
            }
        };
        var mapper = provider.GetRequiredService<IResultToProblemDetailsMapper>();
        var result = Result.Failure<string>(
            "customers.not_found",
            "Customer was not found.",
            ErrorKind.NotFound);

        var response = result.ToHttpResult(httpContext, mapper);

        Console.WriteLine(response.GetType().Name);
    }
}
