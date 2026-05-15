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
/// <item>The request body does not exceed <see cref="WebhookOptions.MaxBodyBytes"/> (default 1 MB)</item>
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
public sealed class EnsureIsFromLicenseManagementAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Creates a new instance of the EnsureIsFromLicenseManagementAttribute.
    /// </summary>
    public EnsureIsFromLicenseManagementAttribute() : base(typeof(EnsureIsFromLicenseManagementFilter))
    {
    }

    private sealed class EnsureIsFromLicenseManagementFilter : IAsyncResourceFilter
    {
        private const int BufferThresholdBytes = 81920;

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
            var opts = _options.Value;
            var maxBodyBytes = opts.MaxBodyBytes > 0 ? opts.MaxBodyBytes : WebhookOptions.DefaultMaxBodyBytes;

            // Reject oversized bodies before reading anything off the wire.
            if (request.ContentLength is long len && len > maxBodyBytes)
            {
                _logger?.LogWarning(
                    "Webhook request rejected: body of {ContentLength} bytes exceeds limit of {Limit} bytes",
                    len, maxBodyBytes);
                context.Result = new StatusCodeResult(StatusCodes.Status413PayloadTooLarge);
                return;
            }

            // Bound the buffer so a chunked body without Content-Length cannot exhaust memory or
            // spill an arbitrary amount to disk.
            request.EnableBuffering(bufferThreshold: BufferThresholdBytes, bufferLimit: maxBodyBytes);

            var signature = request.Headers["X-Webhook-Signature"].FirstOrDefault();
            // Prefer Unix-seconds timestamp; fall back to legacy ISO header for backward compatibility
            // with vendors still on the previous webapp release (see WEBAPP-FIXES-2026-05-15).
            var timestamp = request.Headers["X-Webhook-Timestamp"].FirstOrDefault();
            if (string.IsNullOrEmpty(timestamp))
            {
                timestamp = request.Headers["X-Webhook-Timestamp-ISO"].FirstOrDefault();
            }

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

            string body;
            using (var reader = new StreamReader(request.Body, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync().ConfigureAwait(false);
                request.Body.Position = 0;
            }

            if (body.Length > maxBodyBytes)
            {
                _logger?.LogWarning(
                    "Webhook request rejected: streamed body of {Length} bytes exceeds limit of {Limit} bytes",
                    body.Length, maxBodyBytes);
                context.Result = new StatusCodeResult(StatusCodes.Status413PayloadTooLarge);
                return;
            }

            var secret = opts.Secret;
            var secondarySecret = opts.SecondarySecret;
            var tolerance = opts.TimestampTolerance ?? WebhookSignatureValidator.DefaultTimestampTolerance;

            if (string.IsNullOrEmpty(secret))
            {
                _logger?.LogError("Webhook configuration error: No webhook secret configured");
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

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
            await next().ConfigureAwait(false);
        }
    }
}
#endif
