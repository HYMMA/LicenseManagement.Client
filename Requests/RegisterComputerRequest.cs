namespace LicenseManagement.Client.Requests;

/// <summary>
/// Request to register a new computer.
/// </summary>
public sealed class RegisterComputerRequest
{
    /// <summary>
    /// The device identifier (typically MAC address or hardware ID).
    /// </summary>
    public string MacAddress { get; set; } = string.Empty;

    /// <summary>
    /// The friendly name of the computer.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
