using LicenseManagement.Client.Models;

namespace LicenseManagement.Client.Requests
{
    /// <summary>
    /// Request to create a new webhook
    /// </summary>
    public class CreateWebhookRequest
    {
        /// <summary>
        /// The endpoint URL to send webhook events to (must be HTTPS)
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Array of event types to subscribe to (use "*" for all events)
        /// </summary>
        public string[] Events { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Optional description for the webhook
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Schema version for payloads (default: "v1")
        /// </summary>
        public string Version { get; set; } = "v1";

        /// <summary>
        /// Delivery options for the webhook
        /// </summary>
        public WebhookDeliveryOptions? DeliveryOptions { get; set; }
    }

    /// <summary>
    /// Request to update a webhook
    /// </summary>
    public class UpdateWebhookRequest
    {
        /// <summary>
        /// The endpoint URL to send webhook events to (must be HTTPS)
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Array of event types to subscribe to (use "*" for all events)
        /// </summary>
        public string[] Events { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Optional description for the webhook
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether the webhook is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Schema version for payloads
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Delivery options for the webhook
        /// </summary>
        public WebhookDeliveryOptions? DeliveryOptions { get; set; }
    }

    /// <summary>
    /// Request to rotate webhook secret
    /// </summary>
    public class RotateSecretRequest
    {
        /// <summary>
        /// If true, immediately invalidate the old secret.
        /// If false, keep both secrets valid during transition.
        /// </summary>
        public bool ImmediateRotation { get; set; } = false;
    }

    /// <summary>
    /// Request to replay a failed delivery
    /// </summary>
    public class ReplayDeliveryRequest
    {
        /// <summary>
        /// Optional: Override the target URL for this replay
        /// </summary>
        public string? TargetUrl { get; set; }
    }
}
