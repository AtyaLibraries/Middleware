// <copyright file="ServiceCollectionExtensionsTests.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Errors.ProblemDetails.Abstractions;
using Atya.Errors.ProblemDetails.Options;
using Atya.Web.Middleware.Extensions;
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
            .AddAtyaWebMiddleware(options => options.IncludeTraceId = false)
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<AtyaProblemDetailsOptions>>().Value;

        options.IncludeTraceId.Should().BeFalse();
    }
}
