// <copyright file="ApplicationBuilderExtensionsTests.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Web.Middleware.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
        using var provider = new ServiceCollection()
            .AddAtyaWebMiddleware()
            .BuildServiceProvider();
        var app = new ApplicationBuilder(provider);

        var result = app.UseAtyaWebMiddleware();

        result.Should().BeSameAs(app);
    }
}
