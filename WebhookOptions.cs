namespace LicenseManagement.Client;

/// <summary>
/// Configuration options for webhook signature verification.
/// </summary>
public class WebhookOptions
{
    /// <summary>
    /// The configuration section name for binding from appsettings.
    /// </summary>
    public const string SectionName = "LicenseManagement:Webhook";

    /// <summary>
    /// The primary webhook signing secret.
    /// This is provided when you create a webhook and should be stored securely.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// The secondary webhook signing secret, used during secret rotation.
    /// When rotating secrets, both the old and new secrets are valid during the transition period.
    /// </summary>
    public string? SecondarySecret { get; set; }

    /// <summary>
    /// The tolerance for timestamp validation.
    /// Requests with timestamps outside this window will be rejected to prevent replay attacks.
    /// Default: 5 minutes.
    /// </summary>
    public TimeSpan? TimestampTolerance { get; set; }
}
