using System.Text.Json.Serialization;

namespace LicenseManagement.Client.Requests;

/// <summary>
/// Request to update an existing receipt.
/// </summary>
public class UpdateReceiptRequest
{
    /// <summary>
    /// The receipt ID (ULID with RCT_ prefix).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Updated number of license seats.
    /// </summary>
    [JsonPropertyName("qty")]
    public int? Qty { get; set; }

    /// <summary>
    /// Updated expiration date.
    /// </summary>
    [JsonPropertyName("expires")]
    public DateTime? Expires { get; set; }

    /// <summary>
    /// Updated buyer email address.
    /// </summary>
    [JsonPropertyName("buyerEmail")]
    public string? BuyerEmail { get; set; }
}
