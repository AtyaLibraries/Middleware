// <copyright file="ResultHttpExtensionsTests.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using System.Text;
using Atya.Errors.ProblemDetails.Abstractions;
using Atya.Foundation.Results;
using Atya.Web.Middleware.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using AspNetProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Atya.Web.Middleware.UnitTests;

public sealed class ResultHttpExtensionsTests
{
    [Fact]
    public async Task ToHttpResult_Should_Return_NoContent_When_Result_Succeeds()
    {
        var httpContext = CreateHttpContext();
        var mapper = new StubResultToProblemDetailsMapper(CreateProblemDetails(StatusCodes.Status409Conflict));

        var httpResult = Result.Success().ToHttpResult(httpContext, mapper);

        await httpResult.ExecuteAsync(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        mapper.MapResultCallCount.Should().Be(0);
    }

    [Fact]
    public async Task ToHttpResult_Should_Write_ProblemDetails_When_Result_Fails()
    {
        var httpContext = CreateHttpContext();
        var mapper = new StubResultToProblemDetailsMapper(CreateProblemDetails(StatusCodes.Status409Conflict));
        var result = Result.Failure("customers.conflict", "Customer already exists.", ErrorKind.Conflict);

        var httpResult = result.ToHttpResult(httpContext, mapper);

        await httpResult.ExecuteAsync(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        httpContext.Response.ContentType.Should().StartWith("application/problem+json");
        mapper.MapResultCallCount.Should().Be(1);

        var body = await ReadBodyAsync(httpContext);
        body.Should().Contain("Customer already exists.");
        body.Should().Contain("customers.conflict");
    }

    [Fact]
    public async Task ToHttpResult_Should_Return_Ok_When_Typed_Result_Succeeds()
    {
        var httpContext = CreateHttpContext();
        var mapper = new StubResultToProblemDetailsMapper(CreateProblemDetails(StatusCodes.Status404NotFound));

        var httpResult = Result.Success("customer-42").ToHttpResult(httpContext, mapper);

        await httpResult.ExecuteAsync(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        mapper.MapTypedResultCallCount.Should().Be(0);

        var body = await ReadBodyAsync(httpContext);
        body.Should().Contain("customer-42");
    }

    [Fact]
    public async Task ToHttpResult_Should_Write_ProblemDetails_When_Typed_Result_Fails()
    {
        var httpContext = CreateHttpContext();
        var mapper = new StubResultToProblemDetailsMapper(CreateProblemDetails(StatusCodes.Status404NotFound));
        var result = Result.Failure<string>("customers.not_found", "Customer was not found.", ErrorKind.NotFound);

        var httpResult = result.ToHttpResult(httpContext, mapper);

        await httpResult.ExecuteAsync(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        mapper.MapTypedResultCallCount.Should().Be(1);
    }

    [Fact]
    public async Task ToHttpResult_Should_Write_ProblemDetails_When_Error_Is_Provided()
    {
        var httpContext = CreateHttpContext();
        var mapper = new StubResultToProblemDetailsMapper(CreateProblemDetails(StatusCodes.Status400BadRequest));
        var error = new Error("validation.required", "Email is required.", "email", kind: ErrorKind.Validation);

        var httpResult = error.ToHttpResult(httpContext, mapper);

        await httpResult.ExecuteAsync(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        mapper.MapErrorCallCount.Should().Be(1);
    }

    [Fact]
    public void ToHttpResult_Should_Throw_When_Result_Is_Null()
    {
        var httpContext = CreateHttpContext();
        var mapper = new StubResultToProblemDetailsMapper(CreateProblemDetails(StatusCodes.Status400BadRequest));

        var act = () => ResultHttpExtensions.ToHttpResult((Result)null!, httpContext, mapper);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToHttpResult_Should_Throw_When_Typed_Result_Is_Null()
    {
        var httpContext = CreateHttpContext();
        var mapper = new StubResultToProblemDetailsMapper(CreateProblemDetails(StatusCodes.Status400BadRequest));

        var act = () => ResultHttpExtensions.ToHttpResult((Result<string>)null!, httpContext, mapper);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToHttpResult_Should_Throw_When_Error_Is_Null()
    {
        var httpContext = CreateHttpContext();
        var mapper = new StubResultToProblemDetailsMapper(CreateProblemDetails(StatusCodes.Status400BadRequest));

        var act = () => ResultHttpExtensions.ToHttpResult((Error)null!, httpContext, mapper);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToHttpResult_Should_Throw_When_HttpContext_Is_Null()
    {
        var mapper = new StubResultToProblemDetailsMapper(CreateProblemDetails(StatusCodes.Status400BadRequest));

        var act = () => Result.Success().ToHttpResult(null!, mapper);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToHttpResult_Should_Throw_When_Mapper_Is_Null()
    {
        var httpContext = CreateHttpContext();

        var act = () => Result.Success().ToHttpResult(httpContext, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private static AspNetProblemDetails CreateProblemDetails(int statusCode)
    {
        var problemDetails = new AspNetProblemDetails
        {
            Status = statusCode,
            Title = "Mapped title",
            Type = "urn:test:mapped",
            Detail = statusCode == StatusCodes.Status409Conflict ? "Customer already exists." : "Mapped detail.",
            Instance = "/customers/42"
        };

        problemDetails.Extensions["errorCode"] =
            statusCode == StatusCodes.Status409Conflict ? "customers.conflict" : "mapped.error";

        return problemDetails;
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider(),
            Response =
            {
                Body = new MemoryStream()
            }
        };
    }

    private static async Task<string> ReadBodyAsync(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8);

        return await reader.ReadToEndAsync();
    }

    private sealed class StubResultToProblemDetailsMapper : IResultToProblemDetailsMapper
    {
        private readonly AspNetProblemDetails _problemDetails;

        public StubResultToProblemDetailsMapper(AspNetProblemDetails problemDetails)
        {
            _problemDetails = problemDetails;
        }

        public int MapResultCallCount { get; private set; }

        public int MapTypedResultCallCount { get; private set; }

        public int MapErrorCallCount { get; private set; }

        public AspNetProblemDetails Map(Result result, HttpContext httpContext)
        {
            MapResultCallCount++;

            return _problemDetails;
        }

        public AspNetProblemDetails Map<TValue>(Result<TValue> result, HttpContext httpContext)
        {
            MapTypedResultCallCount++;

            return _problemDetails;
        }

        public AspNetProblemDetails Map(Error resultError, HttpContext httpContext)
        {
            MapErrorCallCount++;

            return _problemDetails;
        }
    }
}
