using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace LicenseManagement.Client;

/// <summary>
/// Utility class for validating webhook signatures (HMAC-SHA256).
/// </summary>
public static class WebhookSignatureValidator
{
    /// <summary>
    /// Default tolerance for timestamp validation (5 minutes).
    /// </summary>
    public static readonly TimeSpan DefaultTimestampTolerance = TimeSpan.FromMinutes(5);

    private const string Sha256Prefix = "sha256=";

    /// <summary>
    /// Validates a webhook signature against the timestamp + payload using HMAC-SHA256.
    /// </summary>
    /// <param name="payload">The raw JSON payload body.</param>
    /// <param name="signature">The signature from <c>X-Webhook-Signature</c> (with or without "sha256=" prefix).</param>
    /// <param name="timestamp">
    /// The timestamp from <c>X-Webhook-Timestamp</c>. Accepts Unix seconds (Stripe / GitHub convention)
    /// or ISO-8601. ISO-8601 strings without an explicit offset are treated as UTC.
    /// </param>
    /// <param name="secret">Your webhook signing secret.</param>
    /// <param name="timestampTolerance">Optional tolerance for timestamp validation.</param>
    /// <returns>True if the signature is valid.</returns>
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

        if (!ValidateTimestamp(timestamp, timestampTolerance ?? DefaultTimestampTolerance))
        {
            return false;
        }

        var signatureValue = signature.StartsWith(Sha256Prefix, StringComparison.OrdinalIgnoreCase)
            ? signature.Substring(Sha256Prefix.Length)
            : signature;

        if (!TryParseHex(signatureValue, out var providedBytes))
        {
            return false;
        }

        var expectedBytes = ComputeSignatureBytes(payload, secret, timestamp);
        return CryptographicEquals(expectedBytes, providedBytes);
    }

    /// <summary>
    /// Validates a webhook signature with a secondary secret (during key rotation).
    /// </summary>
    public static bool ValidateSignatureWithFallback(
        string payload,
        string signature,
        string timestamp,
        string primarySecret,
        string? secondarySecret,
        TimeSpan? timestampTolerance = null)
    {
        if (ValidateSignature(payload, signature, timestamp, primarySecret, timestampTolerance))
        {
            return true;
        }

        if (!string.IsNullOrEmpty(secondarySecret))
        {
            return ValidateSignature(payload, signature, timestamp, secondarySecret!, timestampTolerance);
        }

        return false;
    }

    /// <summary>
    /// Computes the expected signature for a payload as a lowercase hex string.
    /// </summary>
    public static string ComputeSignature(string payload, string secret, string timestamp)
    {
        var hash = ComputeSignatureBytes(payload, secret, timestamp);
#if NETSTANDARD2_0
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
#else
        return Convert.ToHexString(hash).ToLowerInvariant();
#endif
    }

    private static byte[] ComputeSignatureBytes(string payload, string secret, string timestamp)
    {
        var signedPayload = $"{timestamp}.{payload}";
        var key = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(signedPayload);

        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(payloadBytes);
    }

    /// <summary>
    /// Validates that the timestamp is within the configured tolerance window.
    /// Accepts Unix seconds first; falls back to ISO-8601 with <c>AssumeUniversal</c> so that
    /// timestamps lacking an explicit offset are not silently treated as local time.
    /// </summary>
    internal static bool ValidateTimestamp(string timestampString, TimeSpan tolerance)
    {
        if (!TryParseTimestamp(timestampString, out var timestamp))
        {
            return false;
        }

        var diff = DateTimeOffset.UtcNow - timestamp;
        return diff.Duration() <= tolerance;
    }

    internal static bool TryParseTimestamp(string timestampString, out DateTimeOffset timestamp)
    {
        // Stripe / GitHub convention: Unix seconds.
        if (long.TryParse(timestampString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixSeconds))
        {
            timestamp = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            return true;
        }

        // Legacy ISO-8601 path. AssumeUniversal prevents Kind=Unspecified strings from being
        // silently treated as local time.
        return DateTimeOffset.TryParse(
            timestampString,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out timestamp);
    }

    private static bool TryParseHex(string hex, out byte[] bytes)
    {
        if (hex.Length % 2 != 0)
        {
            bytes = Array.Empty<byte>();
            return false;
        }

#if NETSTANDARD2_0
        bytes = new byte[hex.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            var hi = FromHexNibble(hex[i * 2]);
            var lo = FromHexNibble(hex[i * 2 + 1]);
            if (hi < 0 || lo < 0)
            {
                bytes = Array.Empty<byte>();
                return false;
            }
            bytes[i] = (byte)((hi << 4) | lo);
        }
        return true;
#else
        try
        {
            bytes = Convert.FromHexString(hex);
            return true;
        }
        catch (FormatException)
        {
            bytes = Array.Empty<byte>();
            return false;
        }
#endif
    }

#if NETSTANDARD2_0
    private static int FromHexNibble(char c) => c switch
    {
        >= '0' and <= '9' => c - '0',
        >= 'a' and <= 'f' => c - 'a' + 10,
        >= 'A' and <= 'F' => c - 'A' + 10,
        _ => -1
    };
#endif

    private static bool CryptographicEquals(byte[] a, byte[] b)
    {
#if NETSTANDARD2_0
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (var i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }
        return diff == 0;
#else
        return CryptographicOperations.FixedTimeEquals(a, b);
#endif
    }
}

/// <summary>
/// Extension methods for webhook signature validation.
/// </summary>
public static class WebhookSignatureExtensions
{
    /// <summary>
    /// Validates a webhook request from HTTP headers and body.
    /// </summary>
    /// <param name="body">The raw request body.</param>
    /// <param name="signatureHeader">Value of <c>X-Webhook-Signature</c> header.</param>
    /// <param name="timestampHeader">Value of <c>X-Webhook-Timestamp</c> header.</param>
    /// <param name="secret">Your webhook signing secret.</param>
    /// <param name="timestampTolerance">Optional tolerance window override.</param>
    /// <returns>True if valid.</returns>
    public static bool IsValidWebhook(
        string body,
        string signatureHeader,
        string timestampHeader,
        string secret,
        TimeSpan? timestampTolerance = null)
    {
        return WebhookSignatureValidator.ValidateSignature(body, signatureHeader, timestampHeader, secret, timestampTolerance);
    }
}
