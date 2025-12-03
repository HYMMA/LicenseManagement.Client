namespace LicenseManagement.Client.Models;

/// <summary>
/// Represents a computer/device that has software installed.
/// </summary>
public class Computer
{
    /// <summary>
    /// The unique identifier (ULID) of the computer.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The device identifier (typically MAC address or hardware ID).
    /// </summary>
    public string MacAddress { get; set; } = string.Empty;

    /// <summary>
    /// The friendly name of the computer as seen by users.
    /// </summary>
    public string? Name { get; set; }
}
