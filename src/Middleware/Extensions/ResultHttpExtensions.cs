// <copyright file="ResultHttpExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>

using Atya.Errors.ProblemDetails.Abstractions;
using Atya.Foundation.Guards;
using Atya.Foundation.Results;
using Microsoft.AspNetCore.Http;
using AspNetProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Atya.Web.Middleware.Extensions;

/// <summary>
/// Converts Atya Result and Error values into ASP.NET Core HTTP results at the endpoint boundary.
/// </summary>
public static class ResultHttpExtensions
{
    /// <summary>
    /// Converts an untyped Result into an HTTP result.
    /// </summary>
    /// <param name="result">The Result to convert.</param>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="mapper">The mapper that owns RFC 9457 ProblemDetails translation.</param>
    /// <returns>A 204 response for success, or a mapped ProblemDetails response for failure.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="result"/>, <paramref name="httpContext"/>, or <paramref name="mapper"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static IResult ToHttpResult(
        this Result result,
        HttpContext httpContext,
        IResultToProblemDetailsMapper mapper)
    {
        Guard.AgainstNull(result);
        Guard.AgainstNull(httpContext);
        Guard.AgainstNull(mapper);

        return result.IsSuccess
            ? Results.NoContent()
            : ToProblemHttpResult(mapper.Map(result, httpContext));
    }

    /// <summary>
    /// Converts a typed Result into an HTTP result.
    /// </summary>
    /// <typeparam name="TValue">The success value type.</typeparam>
    /// <param name="result">The Result to convert.</param>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="mapper">The mapper that owns RFC 9457 ProblemDetails translation.</param>
    /// <returns>A 200 response with the success value, or a mapped ProblemDetails response for failure.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="result"/>, <paramref name="httpContext"/>, or <paramref name="mapper"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static IResult ToHttpResult<TValue>(
        this Result<TValue> result,
        HttpContext httpContext,
        IResultToProblemDetailsMapper mapper)
    {
        Guard.AgainstNull(result);
        Guard.AgainstNull(httpContext);
        Guard.AgainstNull(mapper);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblemHttpResult(mapper.Map(result, httpContext));
    }

    /// <summary>
    /// Converts an Error into an HTTP ProblemDetails result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="mapper">The mapper that owns RFC 9457 ProblemDetails translation.</param>
    /// <returns>A mapped ProblemDetails response.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="error"/>, <paramref name="httpContext"/>, or <paramref name="mapper"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static IResult ToHttpResult(
        this Error error,
        HttpContext httpContext,
        IResultToProblemDetailsMapper mapper)
    {
        Guard.AgainstNull(error);
        Guard.AgainstNull(httpContext);
        Guard.AgainstNull(mapper);

        return ToProblemHttpResult(mapper.Map(error, httpContext));
    }

    private static IResult ToProblemHttpResult(AspNetProblemDetails problemDetails)
    {
        Guard.AgainstNull(problemDetails);

        return Results.Problem(
            detail: problemDetails.Detail,
            instance: problemDetails.Instance,
            statusCode: problemDetails.Status,
            title: problemDetails.Title,
            type: problemDetails.Type,
            extensions: problemDetails.Extensions);
    }
}
