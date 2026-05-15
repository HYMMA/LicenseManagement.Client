using System.Net;

namespace LicenseManagement.Client.Exceptions;

/// <summary>
/// Exception thrown when the License Management API returns an error.
/// </summary>
public sealed class LicenseManagementException : Exception
{
    /// <summary>
    /// The HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// The raw response content from the API.
    /// </summary>
    public string? ResponseContent { get; }

    /// <summary>
    /// Correlation ID from the <c>X-Correlation-Id</c> response header, when present. Surface this
    /// to end users for support escalation — the same identifier appears in server logs.
    /// </summary>
    public string? CorrelationId { get; }

    /// <summary>
    /// Creates a new LicenseManagementException.
    /// </summary>
    public LicenseManagementException(string message, HttpStatusCode statusCode, string? responseContent = null, string? correlationId = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Creates a new LicenseManagementException with an inner exception, preserving the response
    /// content and correlation ID for diagnostics.
    /// </summary>
    public LicenseManagementException(
        string message,
        HttpStatusCode statusCode,
        Exception innerException,
        string? responseContent = null,
        string? correlationId = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
        CorrelationId = correlationId;
    }
}
