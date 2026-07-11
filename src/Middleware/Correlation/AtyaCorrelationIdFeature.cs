// <copyright file="AtyaCorrelationIdFeature.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

namespace Atya.Web.Middleware.Correlation;

/// <summary>
/// Exposes the correlation id accepted or generated for the current HTTP request.
/// </summary>
public sealed class AtyaCorrelationIdFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtyaCorrelationIdFeature"/> class.
    /// </summary>
    /// <param name="correlationId">The accepted or generated correlation id.</param>
    /// <exception cref="ArgumentException"><paramref name="correlationId"/> is empty or whitespace.</exception>
    public AtyaCorrelationIdFeature(string correlationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        CorrelationId = correlationId;
    }

    /// <summary>
    /// Gets the accepted or generated correlation id.
    /// </summary>
    public string CorrelationId { get; }
}
