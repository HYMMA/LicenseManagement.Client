using System.Text.Json.Serialization;

namespace LicenseManagement.Client.Requests;

/// <summary>
/// Request to create a new receipt (purchase record).
/// </summary>
public class CreateReceiptRequest
{
    /// <summary>
    /// The number of license seats (computers that can use this receipt).
    /// </summary>
    [JsonPropertyName("qty")]
    public int Qty { get; set; }

    /// <summary>
    /// The product ID (ULID with PRD_ prefix).
    /// </summary>
    [JsonPropertyName("product")]
    public string Product { get; set; } = string.Empty;

    /// <summary>
    /// Optional receipt code. If not provided, one will be generated.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// The email address of the customer.
    /// </summary>
    [JsonPropertyName("buyerEmail")]
    public string BuyerEmail { get; set; } = string.Empty;

    /// <summary>
    /// When this receipt expires. For subscription products, this is the renewal date.
    /// </summary>
    [JsonPropertyName("expires")]
    public DateTime? Expires { get; set; }
}
