using System.Text.Json.Serialization;

namespace LicenseManagement.Client.Requests;

/// <summary>
/// Request to create a new product.
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// The name of the product (must be unique per vendor).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
