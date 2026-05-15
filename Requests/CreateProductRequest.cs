namespace LicenseManagement.Client.Requests;

/// <summary>
/// Request to create a new product.
/// </summary>
public sealed class CreateProductRequest
{
    /// <summary>
    /// The name of the product (must be unique per vendor).
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
