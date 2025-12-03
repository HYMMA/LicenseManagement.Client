using System.Text.Json.Serialization;

namespace LicenseManagement.Client.Requests;

/// <summary>
/// Request to register a new computer.
/// </summary>
public class RegisterComputerRequest
{
    /// <summary>
    /// The device identifier (typically MAC address or hardware ID).
    /// </summary>
    [JsonPropertyName("macAddress")]
    public string MacAddress { get; set; } = string.Empty;

    /// <summary>
    /// The friendly name of the computer.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
