// <copyright file="AtyaWebMiddlewareOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

namespace Atya.Web.Middleware.Options;

/// <summary>
/// Configures Atya web middleware behavior.
/// </summary>
public sealed class AtyaWebMiddlewareOptions
{
    private string _correlationIdHeaderName = "X-Correlation-ID";

    /// <summary>
    /// Gets or sets the request and response header used for correlation ids.
    /// </summary>
    /// <exception cref="ArgumentException">The assigned value is empty or whitespace.</exception>
    public string CorrelationIdHeaderName
    {
        get => _correlationIdHeaderName;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            _correlationIdHeaderName = value.Trim();
        }
    }
}
