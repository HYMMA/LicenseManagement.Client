namespace LicenseManagement.Client.Models;

/// <summary>
/// Represents a software product that can be licensed.
/// </summary>
public class Product
{
    /// <summary>
    /// The unique identifier (ULID) of the product.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The name of the product, unique per vendor.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// List of features this product includes.
    /// </summary>
    public List<string>? Features { get; set; }

    /// <summary>
    /// The vendor (maker) of this product.
    /// </summary>
    public Vendor? Vendor { get; set; }
}
