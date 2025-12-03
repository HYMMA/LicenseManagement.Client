namespace LicenseManagement.Client.Models;

/// <summary>
/// Represents a software vendor in the license management system.
/// </summary>
public class Vendor
{
    /// <summary>
    /// The unique identifier (ULID) of the vendor.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The official name of the vendor.
    /// </summary>
    public string? Name { get; set; }
}
