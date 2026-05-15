namespace LicenseManagement.Client.Requests;

/// <summary>
/// Request to update an existing receipt.
/// </summary>
public sealed class UpdateReceiptRequest
{
    /// <summary>
    /// The receipt ID (ULID with RCT_ prefix).
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Updated number of license seats.
    /// </summary>
    public int? Qty { get; set; }

    /// <summary>
    /// Updated expiration date.
    /// </summary>
    public DateTime? Expires { get; set; }

    /// <summary>
    /// Updated buyer email address.
    /// </summary>
    public string? BuyerEmail { get; set; }
}
