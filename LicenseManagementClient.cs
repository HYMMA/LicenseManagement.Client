using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LicenseManagement.Client.Exceptions;
using LicenseManagement.Client.Models;
using LicenseManagement.Client.Requests;
using Microsoft.Extensions.Options;

namespace LicenseManagement.Client;

/// <summary>
/// HTTP client implementation for the License Management API.
/// </summary>
public class LicenseManagementClient : ILicenseManagementClient
{
    private readonly HttpClient _httpClient;
    private readonly LicenseManagementClientOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    // HTTP 422 Unprocessable Entity (not available in .NET Standard 2.0)
    private const int UnprocessableEntityStatusCode = 422;

    /// <summary>
    /// Creates a new LicenseManagementClient.
    /// </summary>
    public LicenseManagementClient(HttpClient httpClient, IOptions<LicenseManagementClientOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    #region Licenses

    /// <inheritdoc />
    public async Task<License?> GetLicenseAsync(string productId, string computerId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"license?product={Uri.EscapeDataString(productId)}&computer={Uri.EscapeDataString(computerId)}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return null;

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<License>(JsonOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<License> CreateLicenseAsync(CreateLicenseRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("license", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response);

        // Get the created license from the Location header
        if (response.Headers.Location != null)
        {
            var getResponse = await _httpClient.GetAsync(response.Headers.Location, cancellationToken);
            await EnsureSuccessAsync(getResponse);
            return (await getResponse.Content.ReadFromJsonAsync<License>(JsonOptions, cancellationToken))!;
        }

        return (await response.Content.ReadFromJsonAsync<License>(JsonOptions, cancellationToken))!;
    }

    /// <inheritdoc />
    public async Task UpdateLicenseAsync(UpdateLicenseRequest request, CancellationToken cancellationToken = default)
    {
        var response = await PatchAsJsonAsync("license", request, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    #endregion

    #region Receipts

    /// <inheritdoc />
    public async Task<Receipt?> GetReceiptAsync(string code, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"receipt?code={Uri.EscapeDataString(code)}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return null;

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<Receipt>(JsonOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Receipt> CreateReceiptAsync(CreateReceiptRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("receipt", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response);

        // Get the created receipt from the Location header
        if (response.Headers.Location != null)
        {
            var getResponse = await _httpClient.GetAsync(response.Headers.Location, cancellationToken);
            await EnsureSuccessAsync(getResponse);
            return (await getResponse.Content.ReadFromJsonAsync<Receipt>(JsonOptions, cancellationToken))!;
        }

        return (await response.Content.ReadFromJsonAsync<Receipt>(JsonOptions, cancellationToken))!;
    }

    /// <inheritdoc />
    public async Task UpdateReceiptAsync(UpdateReceiptRequest request, CancellationToken cancellationToken = default)
    {
        var response = await PatchAsJsonAsync("receipt", request, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    /// <inheritdoc />
    public async Task<string> GenerateReceiptCodeAsync(string productName, string email, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"receipt/code?product={Uri.EscapeDataString(productName)}&email={Uri.EscapeDataString(email)}", cancellationToken);
        await EnsureSuccessAsync(response);
#if NETSTANDARD2_0
        return await response.Content.ReadAsStringAsync();
#else
        return await response.Content.ReadAsStringAsync(cancellationToken);
#endif
    }

    #endregion

    #region Products

    /// <inheritdoc />
    public async Task<Product?> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"product?product={Uri.EscapeDataString(productId)}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return null;

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<Product>(JsonOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("product/all", cancellationToken);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<IEnumerable<Product>>(JsonOptions, cancellationToken))!;
    }

    /// <inheritdoc />
    public async Task<Product> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("product", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response);

        // Get the created product from the Location header
        if (response.Headers.Location != null)
        {
            var getResponse = await _httpClient.GetAsync(response.Headers.Location, cancellationToken);
            await EnsureSuccessAsync(getResponse);
            return (await getResponse.Content.ReadFromJsonAsync<Product>(JsonOptions, cancellationToken))!;
        }

        return (await response.Content.ReadFromJsonAsync<Product>(JsonOptions, cancellationToken))!;
    }

    #endregion

    #region Computers

    /// <inheritdoc />
    public async Task<Computer?> GetComputerAsync(string macAddress, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"computer?macAddress={Uri.EscapeDataString(macAddress)}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return null;

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<Computer>(JsonOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Computer> RegisterComputerAsync(RegisterComputerRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("computer", request, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response);

        // Get the created computer from the Location header
        if (response.Headers.Location != null)
        {
            var getResponse = await _httpClient.GetAsync(response.Headers.Location, cancellationToken);
            await EnsureSuccessAsync(getResponse);
            return (await getResponse.Content.ReadFromJsonAsync<Computer>(JsonOptions, cancellationToken))!;
        }

        return (await response.Content.ReadFromJsonAsync<Computer>(JsonOptions, cancellationToken))!;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Computer>> GetComputersAsync(string receiptCode, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"computer/all?receiptCode={Uri.EscapeDataString(receiptCode)}", cancellationToken);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<IEnumerable<Computer>>(JsonOptions, cancellationToken))!;
    }

    #endregion

    #region Signing Keys

    /// <inheritdoc />
    public async Task<string> GetPublicKeyAsync(string format = "xml", CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"signingkey?format={Uri.EscapeDataString(format)}", cancellationToken);
        await EnsureSuccessAsync(response);
#if NETSTANDARD2_0
        return await response.Content.ReadAsStringAsync();
#else
        return await response.Content.ReadAsStringAsync(cancellationToken);
#endif
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Sends a PATCH request with JSON body (cross-platform compatible).
    /// </summary>
    private async Task<HttpResponseMessage> PatchAsJsonAsync<T>(string requestUri, T value, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
        {
            Content = content
        };
        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

#if NETSTANDARD2_0
        var content = await response.Content.ReadAsStringAsync();
#else
        var content = await response.Content.ReadAsStringAsync();
#endif
        var statusCode = (int)response.StatusCode;
        var message = statusCode switch
        {
            400 => "Invalid request parameters",
            401 => "Invalid or missing API key",
            403 => "Access denied",
            404 => "Resource not found",
            409 => "Resource already exists",
            UnprocessableEntityStatusCode => "Unable to process request",
            _ => $"API request failed with status {response.StatusCode}"
        };

        throw new LicenseManagementException(message, response.StatusCode, content);
    }

    #endregion
}
