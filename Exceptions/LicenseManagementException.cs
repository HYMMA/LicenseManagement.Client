using System.Net;

namespace LicenseManagement.Client.Exceptions;

/// <summary>
/// Exception thrown when the License Management API returns an error.
/// </summary>
public class LicenseManagementException : Exception
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
    /// Creates a new LicenseManagementException.
    /// </summary>
    public LicenseManagementException(string message, HttpStatusCode statusCode, string? responseContent = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }

    /// <summary>
    /// Creates a new LicenseManagementException with an inner exception.
    /// </summary>
    public LicenseManagementException(string message, HttpStatusCode statusCode, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
