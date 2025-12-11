using System.Security.Cryptography;
using System.Text;

namespace LicenseManagement.Client
{
    /// <summary>
    /// Utility class for validating webhook signatures
    /// </summary>
    public static class WebhookSignatureValidator
    {
        /// <summary>
        /// Default tolerance for timestamp validation (5 minutes)
        /// </summary>
        public static readonly TimeSpan DefaultTimestampTolerance = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Validates a webhook signature
        /// </summary>
        /// <param name="payload">The raw JSON payload body</param>
        /// <param name="signature">The signature from X-Webhook-Signature header (with "sha256=" prefix)</param>
        /// <param name="timestamp">The timestamp from X-Webhook-Timestamp header</param>
        /// <param name="secret">Your webhook signing secret</param>
        /// <param name="timestampTolerance">Optional tolerance for timestamp validation</param>
        /// <returns>True if the signature is valid</returns>
        public static bool ValidateSignature(
            string payload,
            string signature,
            string timestamp,
            string secret,
            TimeSpan? timestampTolerance = null)
        {
            if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signature) ||
                string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(secret))
            {
                return false;
            }

            // Validate timestamp to prevent replay attacks
            if (!ValidateTimestamp(timestamp, timestampTolerance ?? DefaultTimestampTolerance))
            {
                return false;
            }

            // Remove "sha256=" prefix if present
            var signatureValue = signature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
                ? signature.Substring(7)
                : signature;

            // Compute expected signature
            var expectedSignature = ComputeSignature(payload, secret, timestamp);

            // Constant-time comparison to prevent timing attacks
            return ConstantTimeEquals(signatureValue, expectedSignature);
        }

        /// <summary>
        /// Validates a webhook signature with a secondary secret (during key rotation)
        /// </summary>
        public static bool ValidateSignatureWithFallback(
            string payload,
            string signature,
            string timestamp,
            string primarySecret,
            string? secondarySecret,
            TimeSpan? timestampTolerance = null)
        {
            // Try primary secret first
            if (ValidateSignature(payload, signature, timestamp, primarySecret, timestampTolerance))
            {
                return true;
            }

            // Try secondary secret if available
            if (!string.IsNullOrEmpty(secondarySecret))
            {
                return ValidateSignature(payload, signature, timestamp, secondarySecret, timestampTolerance);
            }

            return false;
        }

        /// <summary>
        /// Computes the expected signature for a payload
        /// </summary>
        public static string ComputeSignature(string payload, string secret, string timestamp)
        {
            // Signature format: timestamp.payload
            var signedPayload = $"{timestamp}.{payload}";
            var key = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(signedPayload);

            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(payloadBytes);

#if NETSTANDARD2_0
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
#else
            return Convert.ToHexString(hash).ToLowerInvariant();
#endif
        }

        /// <summary>
        /// Validates that the timestamp is within tolerance
        /// </summary>
        public static bool ValidateTimestamp(string timestampString, TimeSpan tolerance)
        {
            if (!DateTime.TryParse(timestampString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var timestamp))
            {
                return false;
            }

            var now = DateTime.UtcNow;
            var difference = now - timestamp;

            return Math.Abs(difference.TotalSeconds) <= tolerance.TotalSeconds;
        }

        /// <summary>
        /// Constant-time string comparison to prevent timing attacks
        /// </summary>
        private static bool ConstantTimeEquals(string a, string b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            var result = 0;
            for (var i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }

    /// <summary>
    /// Extension methods for webhook signature validation
    /// </summary>
    public static class WebhookSignatureExtensions
    {
        /// <summary>
        /// Validates a webhook request from HTTP headers and body
        /// </summary>
        /// <param name="body">The raw request body</param>
        /// <param name="signatureHeader">Value of X-Webhook-Signature header</param>
        /// <param name="timestampHeader">Value of X-Webhook-Timestamp header</param>
        /// <param name="secret">Your webhook signing secret</param>
        /// <returns>True if valid</returns>
        public static bool IsValidWebhook(
            string body,
            string signatureHeader,
            string timestampHeader,
            string secret)
        {
            return WebhookSignatureValidator.ValidateSignature(body, signatureHeader, timestampHeader, secret);
        }
    }
}
