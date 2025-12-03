using System.Text.Json.Serialization;

namespace LicenseManagement.Client.Requests;

/// <summary>
/// Request to create a new trial license.
/// </summary>
public class CreateLicenseRequest
{
    /// <summary>
    /// The product ID (ULID with PRD_ prefix).
    /// </summary>
    [JsonPropertyName("product")]
    public string Product { get; set; } = string.Empty;

    /// <summary>
    /// The computer ID (ULID with PC_ prefix).
    /// </summary>
    [JsonPropertyName("computer")]
    public string Computer { get; set; } = string.Empty;
}
