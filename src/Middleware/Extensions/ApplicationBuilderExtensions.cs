// <copyright file="ApplicationBuilderExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Errors.ProblemDetails.Extensions;
using Atya.Foundation.Guards;
using Atya.Web.Middleware.Internal;
using Microsoft.AspNetCore.Builder;

namespace Atya.Web.Middleware.Extensions;

/// <summary>
/// Application builder extensions for Atya web middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the HTTP-edge middleware that writes Atya ProblemDetails responses for mapped exceptions.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The same application builder instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
    public static IApplicationBuilder UseAtyaWebMiddleware(this IApplicationBuilder app)
    {
        Guard.AgainstNull(app);

        app.UseMiddleware<AtyaCorrelationIdMiddleware>();
        app.UseAtyaProblemDetails();

        return app;
    }
}
