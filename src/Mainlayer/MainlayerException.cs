using System;
using System.Net;

namespace Mainlayer;

/// <summary>
/// Exception thrown when the Mainlayer API returns an error response.
/// </summary>
public sealed class MainlayerException : Exception
{
    /// <summary>Gets the HTTP status code returned by the API.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>Gets the machine-readable error code returned by the API, if any.</summary>
    public string? ErrorCode { get; }

    /// <summary>Gets the raw response body from the API, if available.</summary>
    public string? RawResponse { get; }

    /// <summary>
    /// Initialises a new instance of <see cref="MainlayerException"/>.
    /// </summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="errorCode">The machine-readable error code, if provided by the API.</param>
    /// <param name="rawResponse">The raw response body, if available.</param>
    public MainlayerException(
        string message,
        HttpStatusCode statusCode,
        string? errorCode = null,
        string? rawResponse = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        RawResponse = rawResponse;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"MainlayerException: {StatusCode} ({(int)StatusCode}) — {Message}" +
        (ErrorCode is not null ? $" [code: {ErrorCode}]" : string.Empty);
}
