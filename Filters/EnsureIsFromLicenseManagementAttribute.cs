#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LicenseManagement.Client.Filters;

/// <summary>
/// Filter attribute that validates incoming webhook requests are from License Management.
/// Verifies the HMAC-SHA256 signature using the configured webhook secret.
/// </summary>
/// <remarks>
/// This attribute validates:
/// <list type="bullet">
/// <item>The X-Webhook-Signature header contains a valid HMAC-SHA256 signature</item>
/// <item>The X-Webhook-Timestamp header contains a timestamp within the tolerance window (default 5 minutes)</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// [HttpPost("webhook")]
/// [EnsureIsFromLicenseManagement]
/// public async Task&lt;IActionResult&gt; HandleWebhook([FromBody] WebhookPayload payload)
/// {
///     // Request is verified - process the webhook
///     return Ok();
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class EnsureIsFromLicenseManagementAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Creates a new instance of the EnsureIsFromLicenseManagementAttribute.
    /// </summary>
    public EnsureIsFromLicenseManagementAttribute() : base(typeof(EnsureIsFromLicenseManagementFilter))
    {
    }

    private class EnsureIsFromLicenseManagementFilter : IAsyncResourceFilter
    {
        private readonly IOptions<WebhookOptions> _options;
        private readonly ILogger<EnsureIsFromLicenseManagementFilter>? _logger;

        public EnsureIsFromLicenseManagementFilter(
            IOptions<WebhookOptions> options,
            ILogger<EnsureIsFromLicenseManagementFilter>? logger = null)
        {
            _options = options;
            _logger = logger;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var request = context.HttpContext.Request;

            // Enable buffering so we can read the body multiple times
            request.EnableBuffering();

            // Get the signature and timestamp headers
            var signature = request.Headers["X-Webhook-Signature"].FirstOrDefault();
            var timestamp = request.Headers["X-Webhook-Timestamp"].FirstOrDefault();

            if (string.IsNullOrEmpty(signature))
            {
                _logger?.LogWarning("Webhook request rejected: Missing X-Webhook-Signature header");
                context.Result = new UnauthorizedObjectResult(new { error = "Missing signature header" });
                return;
            }

            if (string.IsNullOrEmpty(timestamp))
            {
                _logger?.LogWarning("Webhook request rejected: Missing X-Webhook-Timestamp header");
                context.Result = new UnauthorizedObjectResult(new { error = "Missing timestamp header" });
                return;
            }

            // Read the request body
            string body;
            using (var reader = new StreamReader(request.Body, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0; // Reset for downstream handlers
            }

            // Get secrets from options
            var secret = _options.Value.Secret;
            var secondarySecret = _options.Value.SecondarySecret;
            var tolerance = _options.Value.TimestampTolerance ?? WebhookSignatureValidator.DefaultTimestampTolerance;

            if (string.IsNullOrEmpty(secret))
            {
                _logger?.LogError("Webhook configuration error: No webhook secret configured");
                context.Result = new StatusCodeResult(500);
                return;
            }

            // Validate the signature (with fallback for key rotation)
            bool isValid;
            if (!string.IsNullOrEmpty(secondarySecret))
            {
                isValid = WebhookSignatureValidator.ValidateSignatureWithFallback(
                    body, signature, timestamp, secret, secondarySecret, tolerance);
            }
            else
            {
                isValid = WebhookSignatureValidator.ValidateSignature(
                    body, signature, timestamp, secret, tolerance);
            }

            if (!isValid)
            {
                _logger?.LogWarning("Webhook request rejected: Invalid signature");
                context.Result = new UnauthorizedObjectResult(new { error = "Invalid webhook signature" });
                return;
            }

            _logger?.LogDebug("Webhook signature verified successfully");
            await next();
        }
    }
}
#endif
