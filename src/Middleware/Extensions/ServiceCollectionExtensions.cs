// <copyright file="ServiceCollectionExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Errors.ProblemDetails.Extensions;
using Atya.Errors.ProblemDetails.Options;
using Atya.Foundation.Guards;
using Atya.Web.Middleware.Internal;
using Atya.Web.Middleware.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Atya.Web.Middleware.Extensions;

/// <summary>
/// Service collection extensions for Atya web middleware.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the HTTP-edge services for Atya exception and Result ProblemDetails mapping.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="configure">An optional callback for customizing ProblemDetails options.</param>
    /// <returns>The same service collection instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddAtyaWebMiddleware(
        this IServiceCollection services,
        Action<AtyaProblemDetailsOptions>? configure = null)
    {
        return AddAtyaWebMiddleware(services, configureMiddleware: null, configure);
    }

    /// <summary>
    /// Registers the HTTP-edge services for Atya correlation, exception, and Result ProblemDetails mapping.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="configureMiddleware">An optional callback for customizing web middleware options.</param>
    /// <param name="configureProblemDetails">An optional callback for customizing ProblemDetails options.</param>
    /// <returns>The same service collection instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddAtyaWebMiddleware(
        this IServiceCollection services,
        Action<AtyaWebMiddlewareOptions>? configureMiddleware,
        Action<AtyaProblemDetailsOptions>? configureProblemDetails = null)
    {
        Guard.AgainstNull(services);

        services.AddOptions<AtyaWebMiddlewareOptions>();
        if (configureMiddleware is not null)
        {
            services.Configure(configureMiddleware);
        }

        services.AddAtyaProblemDetails(configureProblemDetails);
        services.Configure<AtyaProblemDetailsOptions>(options =>
        {
            var fallbackAccessor = options.CorrelationIdAccessor;
            options.CorrelationIdAccessor = httpContext =>
                AtyaCorrelationIdAccessor.GetCorrelationId(httpContext) ?? fallbackAccessor(httpContext);
        });

        return services;
    }
}
