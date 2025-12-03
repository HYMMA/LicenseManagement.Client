using System.Text.Json.Serialization;

namespace LicenseManagement.Client.Requests;

/// <summary>
/// Request to update a license (attach or detach a receipt).
/// </summary>
public class UpdateLicenseRequest
{
    /// <summary>
    /// The license ID (ULID with LIC_ prefix).
    /// </summary>
    [JsonPropertyName("license")]
    public string? License { get; set; }

    /// <summary>
    /// The receipt code to attach. Set to null to unregister the computer from the license.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }
}
