// <copyright file="ApplicationBuilderExtensionsTests.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using System.Diagnostics;
using Atya.Errors.ProblemDetails.Abstractions;
using Atya.Foundation.Results;
using Atya.Web.Middleware.Correlation;
using Atya.Web.Middleware.Extensions;
using Atya.Web.Middleware.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using AspNetProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Atya.Web.Middleware.UnitTests;

public sealed class ApplicationBuilderExtensionsTests
{
    [Fact]
    public void UseAtyaWebMiddleware_Should_Throw_When_App_Is_Null()
    {
        var act = () => Extensions.ApplicationBuilderExtensions.UseAtyaWebMiddleware(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UseAtyaWebMiddleware_Should_Return_The_Same_App_Instance()
    {
        using var provider = CreateProvider();
        var app = new ApplicationBuilder(provider);

        var result = app.UseAtyaWebMiddleware();

        result.Should().BeSameAs(app);
    }

    [Fact]
    public async Task UseAtyaWebMiddleware_Should_Accept_And_Echo_Request_Correlation_Header()
    {
        using var provider = CreateProvider();
        var app = new ApplicationBuilder(provider);
        AtyaCorrelationIdFeature? feature = null;

        app.UseAtyaWebMiddleware();
        app.Run(context =>
        {
            feature = context.Features.Get<AtyaCorrelationIdFeature>();

            return Task.CompletedTask;
        });

        var httpContext = CreateHttpContext(provider);
        httpContext.Request.Headers["X-Correlation-ID"] = "incoming-id";

        await app.Build().Invoke(httpContext);

        feature.Should().NotBeNull();
        feature!.CorrelationId.Should().Be("incoming-id");
        httpContext.Response.Headers["X-Correlation-ID"].ToString().Should().Be("incoming-id");
    }

    [Fact]
    public async Task UseAtyaWebMiddleware_Should_Generate_And_Echo_Correlation_Header_When_Missing()
    {
        using var provider = CreateProvider();
        var app = new ApplicationBuilder(provider);
        AtyaCorrelationIdFeature? feature = null;

        app.UseAtyaWebMiddleware();
        app.Run(context =>
        {
            feature = context.Features.Get<AtyaCorrelationIdFeature>();

            return Task.CompletedTask;
        });

        var httpContext = CreateHttpContext(provider);

        await app.Build().Invoke(httpContext);

        feature.Should().NotBeNull();
        feature!.CorrelationId.Should().NotBeNullOrWhiteSpace();
        httpContext.Response.Headers["X-Correlation-ID"].ToString().Should().Be(feature.CorrelationId);
    }

    [Fact]
    public async Task UseAtyaWebMiddleware_Should_Use_Custom_Correlation_Header_Name()
    {
        using var provider = CreateProvider(options => options.CorrelationIdHeaderName = "X-Request-ID");
        var app = new ApplicationBuilder(provider);

        app.UseAtyaWebMiddleware();
        app.Run(static _ => Task.CompletedTask);

        var httpContext = CreateHttpContext(provider);
        httpContext.Request.Headers["X-Request-ID"] = "request-id";

        await app.Build().Invoke(httpContext);

        httpContext.Response.Headers["X-Request-ID"].ToString().Should().Be("request-id");
    }

    [Fact]
    public async Task UseAtyaWebMiddleware_Should_Stamp_Correlation_Id_Into_ProblemDetails_Extensions()
    {
        using var provider = CreateProvider();
        var app = new ApplicationBuilder(provider);
        AspNetProblemDetails? problemDetails = null;

        app.UseAtyaWebMiddleware();
        app.Run(context =>
        {
            var mapper = context.RequestServices.GetRequiredService<IResultToProblemDetailsMapper>();
            problemDetails = mapper.Map(
                Result.Failure("customers.conflict", "Customer already exists.", ErrorKind.Conflict),
                context);

            return Task.CompletedTask;
        });

        var httpContext = CreateHttpContext(provider);
        httpContext.Request.Headers["X-Correlation-ID"] = "correlation-123";

        await app.Build().Invoke(httpContext);

        problemDetails.Should().NotBeNull();
        problemDetails!.Extensions["correlationId"].Should().Be("correlation-123");
    }

    [Fact]
    public void AtyaWebMiddlewareOptions_Should_Reject_Blank_Correlation_Header_Name()
    {
        var options = new AtyaWebMiddlewareOptions();

        var act = () => options.CorrelationIdHeaderName = " ";

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AtyaCorrelationIdFeature_Should_Reject_Blank_Correlation_Id()
    {
        var act = () => new AtyaCorrelationIdFeature(" ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddAtyaWebMiddleware_Should_Register_Custom_Correlation_Header_Name()
    {
        using var provider = CreateProvider(options => options.CorrelationIdHeaderName = "X-Request-ID");

        provider.GetRequiredService<IOptions<AtyaWebMiddlewareOptions>>()
            .Value
            .CorrelationIdHeaderName
            .Should()
            .Be("X-Request-ID");
    }

    private static ServiceProvider CreateProvider(Action<AtyaWebMiddlewareOptions>? configure = null)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddMetrics()
            .AddSingleton<DiagnosticListener>(new DiagnosticListener("Atya.Web.Middleware.UnitTests"))
            .AddAtyaWebMiddleware(configure);

        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext CreateHttpContext(IServiceProvider provider)
    {
        return new DefaultHttpContext
        {
            RequestServices = provider,
            Response =
            {
                Body = new MemoryStream()
            }
        };
    }
}
