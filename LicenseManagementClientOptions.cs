namespace LicenseManagement.Client;

/// <summary>
/// Configuration options for the License Management client.
/// </summary>
public class LicenseManagementClientOptions
{
    /// <summary>
    /// The master API key for authenticating with the License Management API.
    /// Format: MST_{vendorUlid}_{secret}
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The base URL of the License Management API.
    /// Default: https://license-management.com/api
    /// </summary>
    public string BaseUrl { get; set; } = "https://license-management.com/api";

    /// <summary>
    /// Timeout for HTTP requests in seconds.
    /// Default: 30 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
