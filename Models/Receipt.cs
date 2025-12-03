namespace LicenseManagement.Client.Models;

/// <summary>
/// Represents a purchase receipt that grants license seats.
/// </summary>
public class Receipt
{
    /// <summary>
    /// The unique identifier (ULID) of the receipt.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The receipt code - a hash of email and transaction ID used to identify the purchase.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The number of license seats included in this receipt.
    /// </summary>
    public int Qty { get; set; }

    /// <summary>
    /// The product this receipt grants a license for.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// The email address of the customer who purchased the product.
    /// </summary>
    public string BuyerEmail { get; set; } = string.Empty;

    /// <summary>
    /// The expiration date of this receipt. For subscription products, this is the renewal date.
    /// </summary>
    public DateTime? Expires { get; set; }
}
