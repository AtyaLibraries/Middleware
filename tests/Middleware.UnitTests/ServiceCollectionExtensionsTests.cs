// <copyright file="ServiceCollectionExtensionsTests.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Errors.ProblemDetails.Abstractions;
using Atya.Errors.ProblemDetails.Options;
using Atya.Web.Middleware.Extensions;
using Atya.Web.Middleware.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Atya.Web.Middleware.UnitTests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAtyaWebMiddleware_Should_Throw_When_Services_Is_Null()
    {
        var act = () => ServiceCollectionExtensions.AddAtyaWebMiddleware(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddAtyaWebMiddleware_Should_Register_Exception_And_Result_Mappers()
    {
        using var provider = new ServiceCollection()
            .AddAtyaWebMiddleware()
            .BuildServiceProvider();

        provider.GetRequiredService<IExceptionToProblemDetailsMapper>().Should().NotBeNull();
        provider.GetRequiredService<IResultToProblemDetailsMapper>().Should().NotBeNull();
    }

    [Fact]
    public void AddAtyaWebMiddleware_Should_Apply_ProblemDetails_Configuration()
    {
        using var provider = new ServiceCollection()
            .AddAtyaWebMiddleware(configureProblemDetails: options => options.IncludeTraceId = false)
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<AtyaProblemDetailsOptions>>().Value;

        options.IncludeTraceId.Should().BeFalse();
    }

    [Fact]
    public void AddAtyaWebMiddleware_Should_Support_Non_Ambiguous_Compile_Shapes()
    {
        using var defaultProvider = new ServiceCollection()
            .AddAtyaWebMiddleware()
            .BuildServiceProvider();
        using var middlewareProvider = new ServiceCollection()
            .AddAtyaWebMiddleware(m => { })
            .BuildServiceProvider();
        using var problemDetailsProvider = new ServiceCollection()
            .AddAtyaWebMiddleware(configureProblemDetails: p => { })
            .BuildServiceProvider();

        defaultProvider.GetRequiredService<IOptions<AtyaWebMiddlewareOptions>>().Value.Should().NotBeNull();
        middlewareProvider.GetRequiredService<IOptions<AtyaWebMiddlewareOptions>>().Value.Should().NotBeNull();
        problemDetailsProvider.GetRequiredService<IOptions<AtyaProblemDetailsOptions>>().Value.Should().NotBeNull();
    }
}
