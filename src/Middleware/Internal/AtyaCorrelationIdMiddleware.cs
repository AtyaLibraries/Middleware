// <copyright file="AtyaCorrelationIdMiddleware.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Foundation.Guards;
using Atya.Web.Middleware.Correlation;
using Atya.Web.Middleware.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Atya.Web.Middleware.Internal;

internal sealed class AtyaCorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptions<AtyaWebMiddlewareOptions> _options;

    public AtyaCorrelationIdMiddleware(RequestDelegate next, IOptions<AtyaWebMiddlewareOptions> options)
    {
        _next = Guard.AgainstNull(next);
        _options = Guard.AgainstNull(options);
    }

    public Task InvokeAsync(HttpContext httpContext)
    {
        Guard.AgainstNull(httpContext);

        var headerName = _options.Value.CorrelationIdHeaderName;
        var correlationId = ResolveCorrelationId(httpContext, headerName);

        httpContext.Features.Set(new AtyaCorrelationIdFeature(correlationId));
        httpContext.Response.Headers[headerName] = correlationId;

        return _next(httpContext);
    }

    private static string ResolveCorrelationId(HttpContext httpContext, string headerName)
    {
        if (httpContext.Request.Headers.TryGetValue(headerName, out var values))
        {
            var candidate = values.FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value));
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate.Trim();
            }
        }

        return Guid.NewGuid().ToString("N");
    }
}
