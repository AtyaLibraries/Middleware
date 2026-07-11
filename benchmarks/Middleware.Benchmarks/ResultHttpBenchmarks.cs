// <copyright file="ResultHttpBenchmarks.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Errors.ProblemDetails.Abstractions;
using Atya.Foundation.Results;
using Atya.Web.Middleware.Extensions;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using AspNetProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Atya.Web.Middleware.Benchmarks;

/// <summary>
/// Benchmarks Result-to-HTTP conversion at the endpoint boundary.
/// </summary>
[MemoryDiagnoser]
public class ResultHttpBenchmarks
{
    private readonly DefaultHttpContext _httpContext = new();
    private readonly Result _success = Result.Success();
    private readonly Result _failure = Result.Failure("customers.conflict", "Customer already exists.", ErrorKind.Conflict);
    private readonly IResultToProblemDetailsMapper _mapper = new BenchmarkResultToProblemDetailsMapper();

    /// <summary>
    /// Converts a successful untyped Result.
    /// </summary>
    /// <returns>The generated HTTP result.</returns>
    [Benchmark(Baseline = true)]
    public IResult ConvertSuccess() => _success.ToHttpResult(_httpContext, _mapper);

    /// <summary>
    /// Converts a failed untyped Result.
    /// </summary>
    /// <returns>The generated HTTP result.</returns>
    [Benchmark]
    public IResult ConvertFailure() => _failure.ToHttpResult(_httpContext, _mapper);

    private sealed class BenchmarkResultToProblemDetailsMapper : IResultToProblemDetailsMapper
    {
        public AspNetProblemDetails Map(Result result, HttpContext httpContext) => CreateProblemDetails();

        public AspNetProblemDetails Map<TValue>(Result<TValue> result, HttpContext httpContext) => CreateProblemDetails();

        public AspNetProblemDetails Map(Error resultError, HttpContext httpContext) => CreateProblemDetails();

        private static AspNetProblemDetails CreateProblemDetails()
        {
            return new AspNetProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Type = "urn:atya:problem-type:conflict",
                Detail = "Customer already exists.",
                Instance = "/customers/42"
            };
        }
    }
}
