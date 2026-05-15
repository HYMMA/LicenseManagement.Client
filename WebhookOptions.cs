using Microsoft.Extensions.Options;

namespace LicenseManagement.Client;

/// <summary>
/// Configuration options for webhook signature verification.
/// </summary>
public sealed class WebhookOptions
{
    /// <summary>
    /// The configuration section name for binding from appsettings.
    /// </summary>
    public const string SectionName = "LicenseManagement:Webhook";

    /// <summary>
    /// Default maximum body size accepted by the webhook filter.
    /// </summary>
    public const int DefaultMaxBodyBytes = 1 * 1024 * 1024;

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
    /// Must be greater than zero and less than 1 hour. Default: 5 minutes.
    /// </summary>
    public TimeSpan? TimestampTolerance { get; set; }

    /// <summary>
    /// Maximum allowed size of an inbound webhook body, in bytes. Requests exceeding this limit are
    /// rejected with HTTP 413 before the body is read. Default: 1 MB.
    /// </summary>
    public int MaxBodyBytes { get; set; } = DefaultMaxBodyBytes;
}

/// <summary>
/// Validates <see cref="WebhookOptions"/> at startup so misconfiguration is caught early.
/// </summary>
internal sealed class WebhookOptionsValidator : IValidateOptions<WebhookOptions>
{
    private static readonly TimeSpan MaxAllowedTolerance = TimeSpan.FromHours(1);

    public ValidateOptionsResult Validate(string? name, WebhookOptions options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail($"{nameof(WebhookOptions)} is null.");

        if (options.TimestampTolerance is { } tolerance)
        {
            if (tolerance <= TimeSpan.Zero)
                return ValidateOptionsResult.Fail($"{nameof(WebhookOptions)}.{nameof(WebhookOptions.TimestampTolerance)} must be greater than zero.");
            if (tolerance > MaxAllowedTolerance)
                return ValidateOptionsResult.Fail($"{nameof(WebhookOptions)}.{nameof(WebhookOptions.TimestampTolerance)} must be less than 1 hour.");
        }

        if (options.MaxBodyBytes <= 0)
            return ValidateOptionsResult.Fail($"{nameof(WebhookOptions)}.{nameof(WebhookOptions.MaxBodyBytes)} must be greater than zero.");

        return ValidateOptionsResult.Success;
    }
}
