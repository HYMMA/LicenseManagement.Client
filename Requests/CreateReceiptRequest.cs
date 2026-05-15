namespace LicenseManagement.Client.Requests;

/// <summary>
/// Request to create a new receipt (purchase record).
/// </summary>
public sealed class CreateReceiptRequest
{
    /// <summary>
    /// The number of license seats (computers that can use this receipt).
    /// </summary>
    public int Qty { get; set; }

    /// <summary>
    /// The product ID (ULID with PRD_ prefix).
    /// </summary>
    public string Product { get; set; } = string.Empty;

    /// <summary>
    /// Optional receipt code. If not provided, one will be generated.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// The email address of the customer.
    /// </summary>
    public string BuyerEmail { get; set; } = string.Empty;

    /// <summary>
    /// When this receipt expires. For subscription products, this is the renewal date.
    /// </summary>
    public DateTime? Expires { get; set; }
}
