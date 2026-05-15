using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using LicenseManagement.Client;

namespace LicenseManagement.Client.Tests;

/// <summary>
/// Regression tests for the C-3 / H-1 webhook signature fixes:
///   * Unix-seconds timestamps are accepted (Stripe / GitHub convention)
///   * ISO-8601 timestamps without explicit offset are NOT silently treated as local time
///   * Signature comparison happens on raw HMAC bytes (FixedTimeEquals on net6+)
/// </summary>
public class WebhookSignatureValidatorTests
{
    private const string Secret = "whsec_test_secret_12345";
    private const string Payload = """{"event":"license.created","data":{"id":"LIC_01J0"}}""";

    private static string Sign(string payload, string secret, string timestamp)
    {
        var signed = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signed));
        return "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
    }

    [Fact]
    public void ValidateSignature_Accepts_UnixSeconds_Timestamp()
    {
        var unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var sig = Sign(Payload, Secret, unix);

        Assert.True(WebhookSignatureValidator.ValidateSignature(Payload, sig, unix, Secret));
    }

    [Fact]
    public void ValidateSignature_Accepts_ISO8601_With_Offset()
    {
        var iso = DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        var sig = Sign(Payload, Secret, iso);

        Assert.True(WebhookSignatureValidator.ValidateSignature(Payload, sig, iso, Secret));
    }

    [Fact]
    public void ValidateSignature_Rejects_Stale_UnixSeconds()
    {
        var staleUnix = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 600).ToString(CultureInfo.InvariantCulture);
        var sig = Sign(Payload, Secret, staleUnix);

        Assert.False(WebhookSignatureValidator.ValidateSignature(Payload, sig, staleUnix, Secret));
    }

    [Fact]
    public void ValidateSignature_AssumeUniversal_Closes_LocalTime_Loophole()
    {
        // C-3 regression: an ISO-8601 timestamp without an explicit offset must be treated as UTC,
        // not as the server's local time. A timestamp formed from a non-UTC clock should still be
        // anchored on the UTC instant of "now" — so signing with a NaiveLocalNow string and
        // validating against UTC must succeed only when the underlying moment is current.
        var nowUtc = DateTimeOffset.UtcNow;
        var unspecifiedIso = nowUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        var sig = Sign(Payload, Secret, unspecifiedIso);

        // Should be accepted: AssumeUniversal makes the unspecified-Kind string anchor at UTC,
        // which matches "now".
        Assert.True(WebhookSignatureValidator.ValidateSignature(Payload, sig, unspecifiedIso, Secret));
    }

    [Fact]
    public void ValidateSignature_Tampered_Body_Rejected()
    {
        var unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var sig = Sign(Payload, Secret, unix);

        Assert.False(WebhookSignatureValidator.ValidateSignature(Payload + "tampered", sig, unix, Secret));
    }

    [Fact]
    public void ValidateSignature_Wrong_Secret_Rejected()
    {
        var unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var sig = Sign(Payload, "different-secret", unix);

        Assert.False(WebhookSignatureValidator.ValidateSignature(Payload, sig, unix, Secret));
    }

    [Fact]
    public void ValidateSignature_NonHex_Signature_Rejected()
    {
        // H-1 regression: the comparator must be byte-based; a junk-non-hex signature must not
        // throw or fall back to char-by-char comparison against the hex string.
        var unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

        Assert.False(WebhookSignatureValidator.ValidateSignature(Payload, "sha256=zzzzznothex", unix, Secret));
    }

    [Fact]
    public void IsValidWebhook_Honours_Caller_Tolerance()
    {
        // L-5 regression: the convenience extension previously ignored configured tolerance.
        var staleUnix = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1200).ToString(CultureInfo.InvariantCulture);
        var sig = Sign(Payload, Secret, staleUnix);

        // Default 5 min: rejected.
        Assert.False(WebhookSignatureExtensions.IsValidWebhook(Payload, sig, staleUnix, Secret));

        // Widened to 30 min: accepted.
        Assert.True(WebhookSignatureExtensions.IsValidWebhook(Payload, sig, staleUnix, Secret, TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void ValidateSignature_Accepts_Signature_Without_Prefix()
    {
        var unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var sig = Sign(Payload, Secret, unix).Substring("sha256=".Length);

        Assert.True(WebhookSignatureValidator.ValidateSignature(Payload, sig, unix, Secret));
    }
}
