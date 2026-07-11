// <copyright file="AtyaCorrelationIdAccessor.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Web.Middleware.Correlation;
using Microsoft.AspNetCore.Http;

namespace Atya.Web.Middleware.Internal;

internal static class AtyaCorrelationIdAccessor
{
    public static string? GetCorrelationId(HttpContext httpContext)
    {
        return httpContext.Features.Get<AtyaCorrelationIdFeature>()?.CorrelationId;
    }
}
