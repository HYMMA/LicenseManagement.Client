using System.Text.Json.Serialization;

namespace LicenseManagement.Client.Models
{
    /// <summary>
    /// Represents a webhook configuration
    /// </summary>
    public class Webhook
    {
        /// <summary>
        /// Unique identifier for the webhook (ULID)
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The endpoint URL to send webhook events to
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Array of event types this webhook is subscribed to
        /// </summary>
        public string[] Events { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Optional description for the webhook
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether the webhook is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Schema version for payloads (e.g., "v1")
        /// </summary>
        public string Version { get; set; } = "v1";

        /// <summary>
        /// Number of consecutive failures
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// Last successful delivery timestamp
        /// </summary>
        public DateTime? LastSuccess { get; set; }

        /// <summary>
        /// Last failure timestamp
        /// </summary>
        public DateTime? LastFailure { get; set; }

        /// <summary>
        /// Circuit breaker state: closed, open, half-open
        /// </summary>
        public string CircuitState { get; set; } = "closed";

        /// <summary>
        /// Delivery options for the webhook
        /// </summary>
        public WebhookDeliveryOptions? DeliveryOptions { get; set; }

        /// <summary>
        /// When the webhook was created
        /// </summary>
        public DateTime Created { get; set; }
    }

    /// <summary>
    /// Webhook created response (includes secret)
    /// </summary>
    public class WebhookCreated
    {
        /// <summary>
        /// Unique identifier for the webhook
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The webhook signing secret (only returned on creation)
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// The endpoint URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Subscribed event types
        /// </summary>
        public string[] Events { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Schema version
        /// </summary>
        public string Version { get; set; } = "v1";
    }

    /// <summary>
    /// Webhook delivery options
    /// </summary>
    public class WebhookDeliveryOptions
    {
        /// <summary>
        /// Maximum deliveries per minute (null = no limit)
        /// </summary>
        public int? RateLimitPerMinute { get; set; }

        /// <summary>
        /// Whether to batch events
        /// </summary>
        public bool BatchDelivery { get; set; }

        /// <summary>
        /// Maximum batch size
        /// </summary>
        public int MaxBatchSize { get; set; } = 100;

        /// <summary>
        /// Maximum time to wait for batch (seconds)
        /// </summary>
        public int BatchTimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// Webhook delivery record
    /// </summary>
    public class WebhookDelivery
    {
        /// <summary>
        /// Unique identifier for the delivery
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Event type that was delivered
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Delivery status: pending, success, failed, retrying, dead_letter
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// HTTP response status code
        /// </summary>
        public int? ResponseStatusCode { get; set; }

        /// <summary>
        /// Response body (may be truncated)
        /// </summary>
        public string? ResponseBody { get; set; }

        /// <summary>
        /// Request duration in milliseconds
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// Number of retry attempts
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Maximum retry attempts
        /// </summary>
        public int MaxRetryAttempts { get; set; }

        /// <summary>
        /// Next retry time (if retrying)
        /// </summary>
        public DateTime? NextRetry { get; set; }

        /// <summary>
        /// Trace ID for distributed tracing
        /// </summary>
        public string? TraceId { get; set; }

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Whether this is a replay of a previous delivery
        /// </summary>
        public bool IsReplay { get; set; }

        /// <summary>
        /// When the delivery was created
        /// </summary>
        public DateTime Created { get; set; }
    }

    /// <summary>
    /// Detailed webhook delivery information
    /// </summary>
    public class WebhookDeliveryDetail : WebhookDelivery
    {
        /// <summary>
        /// Full JSON payload sent
        /// </summary>
        public string? Payload { get; set; }

        /// <summary>
        /// HTTP headers sent (JSON)
        /// </summary>
        public string? RequestHeaders { get; set; }

        /// <summary>
        /// Original delivery ID if this is a replay
        /// </summary>
        public string? OriginalDeliveryId { get; set; }
    }

    /// <summary>
    /// Webhook health status
    /// </summary>
    public class WebhookHealth
    {
        /// <summary>
        /// Webhook ID
        /// </summary>
        public string WebhookId { get; set; } = string.Empty;

        /// <summary>
        /// Health status: healthy, degraded, unhealthy, disabled
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Whether the webhook is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Circuit breaker state
        /// </summary>
        public string CircuitState { get; set; } = "closed";

        /// <summary>
        /// Number of consecutive failures
        /// </summary>
        public int ConsecutiveFailures { get; set; }

        /// <summary>
        /// Pending deliveries count
        /// </summary>
        public int PendingDeliveries { get; set; }

        /// <summary>
        /// Dead letter count
        /// </summary>
        public int DeadLetterCount { get; set; }

        /// <summary>
        /// Last successful delivery
        /// </summary>
        public DateTime? LastSuccessAt { get; set; }

        /// <summary>
        /// Last failed delivery
        /// </summary>
        public DateTime? LastFailureAt { get; set; }

        /// <summary>
        /// Last error message
        /// </summary>
        public string? LastErrorMessage { get; set; }
    }

    /// <summary>
    /// Webhook delivery statistics
    /// </summary>
    public class WebhookStats
    {
        /// <summary>
        /// Webhook ID
        /// </summary>
        public string WebhookId { get; set; } = string.Empty;

        /// <summary>
        /// Total number of deliveries
        /// </summary>
        public int TotalDeliveries { get; set; }

        /// <summary>
        /// Successful deliveries
        /// </summary>
        public int SuccessfulDeliveries { get; set; }

        /// <summary>
        /// Failed deliveries
        /// </summary>
        public int FailedDeliveries { get; set; }

        /// <summary>
        /// Pending retries
        /// </summary>
        public int PendingRetries { get; set; }

        /// <summary>
        /// Dead letter count
        /// </summary>
        public int DeadLetterCount { get; set; }

        /// <summary>
        /// Success rate percentage
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// First-try success rate percentage
        /// </summary>
        public double FirstTrySuccessRate { get; set; }

        /// <summary>
        /// Average latency in milliseconds
        /// </summary>
        public double AverageLatencyMs { get; set; }

        /// <summary>
        /// Last delivery time
        /// </summary>
        public DateTime? LastDeliveryAt { get; set; }

        /// <summary>
        /// Circuit breaker state
        /// </summary>
        public string CircuitState { get; set; } = "closed";
    }

    /// <summary>
    /// Secret rotation response
    /// </summary>
    public class WebhookSecretRotated
    {
        /// <summary>
        /// The new webhook secret
        /// </summary>
        public string NewSecret { get; set; } = string.Empty;

        /// <summary>
        /// Whether there's a secondary secret still valid
        /// </summary>
        public bool HasSecondarySecret { get; set; }

        /// <summary>
        /// Message about the rotation
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Available webhook event types
    /// </summary>
    public class WebhookEventTypes
    {
        /// <summary>
        /// List of all available event types
        /// </summary>
        public string[] Events { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Event type descriptions
        /// </summary>
        public Dictionary<string, string> Descriptions { get; set; } = new();
    }
}
