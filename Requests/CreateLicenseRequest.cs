namespace LicenseManagement.Client.Requests;

/// <summary>
/// Request to create a new trial license.
/// </summary>
public sealed class CreateLicenseRequest
{
    /// <summary>
    /// The product ID (ULID with PRD_ prefix).
    /// </summary>
    public string Product { get; set; } = string.Empty;

    /// <summary>
    /// The computer ID (ULID with PC_ prefix).
    /// </summary>
    public string Computer { get; set; } = string.Empty;

    /// <summary>
    /// Optional metadata to attach to the license (key-value pairs).
    /// </summary>
    /// <remarks>Keys and values are limited to 100 characters each. Max 20 entries recommended.</remarks>
    public Dictionary<string, string>? Metadata { get; set; }
}
