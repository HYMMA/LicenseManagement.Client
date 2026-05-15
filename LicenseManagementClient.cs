using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LicenseManagement.Client.Exceptions;
using LicenseManagement.Client.Models;
using LicenseManagement.Client.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LicenseManagement.Client;

/// <summary>
/// HTTP client implementation for the License Management API.
/// </summary>
public sealed class LicenseManagementClient : ILicenseManagementClient
{
    private readonly HttpClient _httpClient;
    private readonly LicenseManagementClientOptions _options;
    private readonly ILogger<LicenseManagementClient>? _logger;

    internal static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    // HTTP 422 Unprocessable Entity (not available in .NET Standard 2.0)
    private const int UnprocessableEntityStatusCode = 422;

    /// <summary>
    /// Creates a new LicenseManagementClient.
    /// </summary>
    /// <remarks>
    /// HTTP configuration (BaseAddress, Timeout, default headers, User-Agent) should be applied via
    /// <c>AddLicenseManagementClient</c> in dependency injection. The constructor no longer mutates
    /// the supplied <see cref="HttpClient"/> so callers can compose their own handlers and headers.
    /// </remarks>
    public LicenseManagementClient(
        HttpClient httpClient,
        IOptions<LicenseManagementClientOptions> options,
        ILogger<LicenseManagementClient>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        // BaseAddress is configured via DI in ServiceCollectionExtensions. When the client is
        // instantiated directly (tests, ad-hoc usage), fall back to the configured options so the
        // class is still usable without DI plumbing.
        if (_httpClient.BaseAddress is null && !string.IsNullOrEmpty(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        }
        if (!_httpClient.DefaultRequestHeaders.Contains("X-API-KEY") && !string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _options.ApiKey);
        }
    }

    #region Licenses

    /// <inheritdoc />
    public async Task<License?> GetLicenseAsync(string productId, string computerId, CancellationToken cancellationToken = default)
    {
        var url = $"license?product={Escape(productId)}&computer={Escape(computerId)}";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrDefaultAsync<License>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<License> CreateLicenseAsync(CreateLicenseRequest request, CancellationToken cancellationToken = default)
        => CreateLicenseAsync(request, idempotencyKey: null, cancellationToken);

    /// <inheritdoc />
    public Task<License> CreateLicenseAsync(CreateLicenseRequest request, string? idempotencyKey, CancellationToken cancellationToken = default)
        => CreateAndFetchAsync<License>("license", request, idempotencyKey, cancellationToken);

    /// <inheritdoc />
    public async Task UpdateLicenseAsync(UpdateLicenseRequest request, CancellationToken cancellationToken = default)
    {
        var response = await PatchAsJsonAsync("license", request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Receipts

    /// <inheritdoc />
    public async Task<Receipt?> GetReceiptAsync(string code, CancellationToken cancellationToken = default)
    {
        var url = $"receipt?code={Escape(code)}";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrDefaultAsync<Receipt>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<Receipt> CreateReceiptAsync(CreateReceiptRequest request, CancellationToken cancellationToken = default)
        => CreateReceiptAsync(request, idempotencyKey: null, cancellationToken);

    /// <inheritdoc />
    public Task<Receipt> CreateReceiptAsync(CreateReceiptRequest request, string? idempotencyKey, CancellationToken cancellationToken = default)
        => CreateAndFetchAsync<Receipt>("receipt", request, idempotencyKey, cancellationToken);

    /// <inheritdoc />
    public async Task UpdateReceiptAsync(UpdateReceiptRequest request, CancellationToken cancellationToken = default)
    {
        var response = await PatchAsJsonAsync("receipt", request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<string> GenerateReceiptCodeAsync(string productName, string email, CancellationToken cancellationToken = default)
    {
        var url = $"receipt/code?product={Escape(productName)}&email={Escape(email)}";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadAsStringAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<string> ResetReceiptCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var existingReceipt = await GetReceiptAsync(code, cancellationToken).ConfigureAwait(false)
            ?? throw new LicenseManagementException(
                $"Receipt not found with code '{code}'",
                HttpStatusCode.NotFound,
                $"No receipt exists with code: {code}");

        if (string.IsNullOrEmpty(existingReceipt.Id))
        {
            throw new LicenseManagementException(
                "Receipt is missing its identifier; cannot rotate code",
                (HttpStatusCode)UnprocessableEntityStatusCode,
                responseContent: null);
        }

        // Server-side endpoint performs void+create atomically inside a TransactionScope (C-1 fix).
        var url = $"receipt/{Escape(existingReceipt.Id)}/rotate-code";
        var idempotencyKey = $"reset-receipt-{existingReceipt.Id}-{Guid.NewGuid():N}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Idempotency-Key", idempotencyKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        var rotated = await response.Content.ReadFromJsonAsync<Receipt>(JsonOptions, cancellationToken).ConfigureAwait(false);
        if (rotated is null || string.IsNullOrEmpty(rotated.Code))
        {
            throw new LicenseManagementException(
                $"Server returned an empty body for POST {url}",
                response.StatusCode,
                responseContent: null);
        }

        return rotated.Code;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Receipt>> GetReceiptsAsync(string buyerEmail, string productId, CancellationToken cancellationToken = default)
    {
        var url = $"receipt/all?buyerEmail={Escape(buyerEmail)}&product={Escape(productId)}";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrEmptyAsync<Receipt>(response, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Products

    /// <inheritdoc />
    public async Task<Product?> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        var url = $"product?product={Escape(productId)}";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrDefaultAsync<Product>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("product/all", cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrEmptyAsync<Product>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<Product> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        => CreateProductAsync(request, idempotencyKey: null, cancellationToken);

    /// <inheritdoc />
    public Task<Product> CreateProductAsync(CreateProductRequest request, string? idempotencyKey, CancellationToken cancellationToken = default)
        => CreateAndFetchAsync<Product>("product", request, idempotencyKey, cancellationToken);

    #endregion

    #region Computers

    /// <inheritdoc />
    public async Task<Computer?> GetComputerAsync(string macAddress, CancellationToken cancellationToken = default)
    {
        var url = $"computer?macAddress={Escape(macAddress)}";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrDefaultAsync<Computer>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<Computer> RegisterComputerAsync(RegisterComputerRequest request, CancellationToken cancellationToken = default)
        => RegisterComputerAsync(request, idempotencyKey: null, cancellationToken);

    /// <inheritdoc />
    public Task<Computer> RegisterComputerAsync(RegisterComputerRequest request, string? idempotencyKey, CancellationToken cancellationToken = default)
        => CreateAndFetchAsync<Computer>("computer", request, idempotencyKey, cancellationToken);

    /// <inheritdoc />
    public async Task<IEnumerable<Computer>> GetComputersAsync(string receiptCode, CancellationToken cancellationToken = default)
    {
        var url = $"computer/all?receiptCode={Escape(receiptCode)}";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrEmptyAsync<Computer>(response, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Signing Keys

    /// <inheritdoc />
    public async Task<string> GetPublicKeyAsync(string format = "xml", CancellationToken cancellationToken = default)
    {
        var url = $"signingkey?format={Escape(format)}";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadAsStringAsync(response, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Webhooks

    /// <inheritdoc />
    public async Task<IEnumerable<Webhook>> GetWebhooksAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("webhook", cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrEmptyAsync<Webhook>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Webhook?> GetWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"webhook/{Escape(webhookId)}", cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrDefaultAsync<Webhook>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<WebhookCreated> CreateWebhookAsync(CreateWebhookRequest request, CancellationToken cancellationToken = default)
        => CreateWebhookAsync(request, idempotencyKey: null, cancellationToken);

    /// <inheritdoc />
    public async Task<WebhookCreated> CreateWebhookAsync(CreateWebhookRequest request, string? idempotencyKey, CancellationToken cancellationToken = default)
    {
        using var httpRequest = BuildJsonRequest(HttpMethod.Post, "webhook", request, idempotencyKey);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrThrowAsync<WebhookCreated>(response, "POST webhook", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateWebhookAsync(string webhookId, UpdateWebhookRequest request, CancellationToken cancellationToken = default)
    {
        using var httpRequest = BuildJsonRequest(HttpMethod.Put, $"webhook/{Escape(webhookId)}", request, idempotencyKey: null);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"webhook/{Escape(webhookId)}", cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WebhookSecretRotated> RotateWebhookSecretAsync(string webhookId, bool immediateRotation = false, CancellationToken cancellationToken = default)
    {
        var body = new { ImmediateRotation = immediateRotation };
        using var httpRequest = BuildJsonRequest(HttpMethod.Post, $"webhook/{Escape(webhookId)}/rotate-secret", body, idempotencyKey: null);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrThrowAsync<WebhookSecretRotated>(response, $"POST webhook/{webhookId}/rotate-secret", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CompleteSecretRotationAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"webhook/{Escape(webhookId)}/complete-rotation", null, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WebhookDelivery>> GetWebhookDeliveriesAsync(string webhookId, int limit = WebhookConstants.DefaultDeliveryLimit, int offset = 0, string? status = null, CancellationToken cancellationToken = default)
    {
        var url = $"webhook/{Escape(webhookId)}/deliveries?limit={limit}&offset={offset}";
        if (!string.IsNullOrEmpty(status))
        {
            url += $"&status={Escape(status!)}";
        }

        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrEmptyAsync<WebhookDelivery>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WebhookDeliveryDetail?> GetWebhookDeliveryAsync(string webhookId, string deliveryId, CancellationToken cancellationToken = default)
    {
        var url = $"webhook/{Escape(webhookId)}/deliveries/{Escape(deliveryId)}";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrDefaultAsync<WebhookDeliveryDetail>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WebhookDelivery> ReplayWebhookDeliveryAsync(string webhookId, string deliveryId, string? targetUrl = null, CancellationToken cancellationToken = default)
    {
        var body = new { TargetUrl = targetUrl };
        var url = $"webhook/{Escape(webhookId)}/deliveries/{Escape(deliveryId)}/replay";
        using var httpRequest = BuildJsonRequest(HttpMethod.Post, url, body, idempotencyKey: null);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrThrowAsync<WebhookDelivery>(response, $"POST {url}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WebhookHealth> GetWebhookHealthAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        var url = $"webhook/{Escape(webhookId)}/health";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrThrowAsync<WebhookHealth>(response, $"GET {url}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WebhookStats> GetWebhookStatsAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        var url = $"webhook/{Escape(webhookId)}/stats";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrThrowAsync<WebhookStats>(response, $"GET {url}", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<WebhookEventTypes> GetWebhookEventTypesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("webhook/events", cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonOrThrowAsync<WebhookEventTypes>(response, "GET webhook/events", cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task TestWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"webhook/{Escape(webhookId)}/test", null, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Helpers

    private async Task<T> CreateAndFetchAsync<T>(string path, object body, string? idempotencyKey, CancellationToken cancellationToken)
    {
        using var httpRequest = BuildJsonRequest(HttpMethod.Post, path, body, idempotencyKey);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        // POST may return the resource directly. If it returns a Location header, follow it.
        if (response.Headers.Location is { } location)
        {
            using var getResponse = await _httpClient.GetAsync(location, cancellationToken).ConfigureAwait(false);
            await EnsureSuccessAsync(getResponse, cancellationToken).ConfigureAwait(false);
            return await ReadJsonOrThrowAsync<T>(getResponse, $"GET {location}", cancellationToken).ConfigureAwait(false);
        }

        return await ReadJsonOrThrowAsync<T>(response, $"POST {path}", cancellationToken).ConfigureAwait(false);
    }

    private HttpRequestMessage BuildJsonRequest<T>(HttpMethod method, string requestUri, T body, string? idempotencyKey)
    {
        var request = new HttpRequestMessage(method, requestUri)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        if (idempotencyKey is not null)
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        return request;
    }

    /// <summary>
    /// Deserializes a JSON response body, returning default(T) when the response is 204 No Content
    /// or has an empty body. Guards against System.Text.Json throwing on empty input.
    /// </summary>
    private static async Task<T?> ReadJsonOrDefaultAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
            return default;

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deserializes a JSON array response body, returning an empty sequence when the response is
    /// 204 No Content, has an empty body, or deserializes to null.
    /// </summary>
    private static async Task<IEnumerable<T>> ReadJsonOrEmptyAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
            return Enumerable.Empty<T>();

        return (await response.Content.ReadFromJsonAsync<IEnumerable<T>>(JsonOptions, cancellationToken).ConfigureAwait(false))
            ?? Enumerable.Empty<T>();
    }

    private static async Task<T> ReadJsonOrThrowAsync<T>(HttpResponseMessage response, string context, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
        {
            throw new LicenseManagementException(
                $"Server returned an empty body for {context}",
                response.StatusCode,
                responseContent: null);
        }

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken).ConfigureAwait(false);
        return result ?? throw new LicenseManagementException(
            $"Server returned a null body for {context}",
            response.StatusCode,
            responseContent: null);
    }

    private static async Task<string> ReadAsStringAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
#if NETSTANDARD2_0
        _ = cancellationToken; // Best-effort: ReadAsStringAsync(CancellationToken) is unavailable on netstandard2.0.
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
    }

    /// <summary>
    /// Sends a PATCH request with JSON body (cross-platform compatible).
    /// </summary>
    private async Task<HttpResponseMessage> PatchAsJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        using var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static string Escape(string value) => Uri.EscapeDataString(value);

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await ReadAsStringAsync(response, cancellationToken).ConfigureAwait(false);
        var method = response.RequestMessage?.Method?.Method ?? "?";
        var requestUri = response.RequestMessage?.RequestUri?.ToString() ?? "?";
        var correlationId = response.Headers.TryGetValues("X-Correlation-Id", out var ids)
            ? ids.FirstOrDefault()
            : null;

        // The webapp returns RFC 7807 problem+json bodies; surface the detail when present.
        var problemDetail = TryGetProblemDetail(response, content);

        var statusCode = (int)response.StatusCode;
        var baseReason = statusCode switch
        {
            400 => "Invalid request parameters",
            401 => "Authentication failed",
            403 => "Access denied",
            404 => "Resource not found",
            409 => "Resource already exists",
            UnprocessableEntityStatusCode => "Unable to process request",
            413 => "Request body too large",
            429 => "Rate limit exceeded",
            _ => $"API request failed with status {response.StatusCode}"
        };

        var message = problemDetail is { Length: > 0 }
            ? $"{baseReason} for {method} {requestUri}: {problemDetail}"
            : $"{baseReason} for {method} {requestUri}";

        if (correlationId is { Length: > 0 })
            message += $" [correlationId={correlationId}]";

        _logger?.LogWarning(
            "License Management API request failed: {Method} {Uri} returned {StatusCode} (correlationId={CorrelationId})",
            method, requestUri, statusCode, correlationId ?? "(none)");

        throw new LicenseManagementException(message, response.StatusCode, content, correlationId);
    }

    private static string? TryGetProblemDetail(HttpResponseMessage response, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (!string.Equals(mediaType, "application/problem+json", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return null;
            if (doc.RootElement.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                return detail.GetString();
            if (doc.RootElement.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                return title.GetString();
        }
        catch (JsonException)
        {
            // Body wasn't JSON after all; fall through.
        }
        return null;
    }

    #endregion
}

/// <summary>
/// Compile-time constants for webhook delivery API defaults.
/// </summary>
internal static class WebhookConstants
{
    public const int DefaultDeliveryLimit = 50;
}
