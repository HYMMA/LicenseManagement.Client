using System.Net;
using System.Text;
using LicenseManagement.Client;
using LicenseManagement.Client.Models;
using LicenseManagement.Client.Requests;
using Microsoft.Extensions.Options;

namespace LicenseManagement.Client.Tests;

/// <summary>
/// Regression tests for the C-2 / M-4 idempotency-key support:
///   * Create* methods accept an optional idempotencyKey
///   * When supplied, it is forwarded as Idempotency-Key header on the POST
///   * When omitted, no header is sent (server picks its own behavior)
/// </summary>
public class IdempotencyKeyTests
{
    private static (LicenseManagementClient client, List<HttpRequestMessage> requests) CreateClient(
        Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var captured = new List<HttpRequestMessage>();
        var handler = new CapturingHandler(req =>
        {
            captured.Add(req);
            return responder(req);
        });
        var http = new HttpClient(handler);
        var options = Options.Create(new LicenseManagementClientOptions
        {
            BaseUrl = "https://example.test",
            ApiKey = "test-key",
            TimeoutSeconds = 30
        });
        return (new LicenseManagementClient(http, options), captured);
    }

    private static HttpResponseMessage JsonOk(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    [Fact]
    public async Task CreateLicenseAsync_With_IdempotencyKey_Forwards_Header()
    {
        var (client, requests) = CreateClient(_ => JsonOk("{\"id\":\"LIC_1\"}"));

        await client.CreateLicenseAsync(new CreateLicenseRequest { Product = "PRD_1", Computer = "PC_1" }, "deterministic-key-1");

        var post = requests.Single(r => r.Method == HttpMethod.Post);
        Assert.True(post.Headers.Contains("Idempotency-Key"));
        Assert.Equal("deterministic-key-1", post.Headers.GetValues("Idempotency-Key").Single());
    }

    [Fact]
    public async Task CreateLicenseAsync_Without_IdempotencyKey_Sends_No_Header()
    {
        var (client, requests) = CreateClient(_ => JsonOk("{\"id\":\"LIC_1\"}"));

        await client.CreateLicenseAsync(new CreateLicenseRequest { Product = "PRD_1", Computer = "PC_1" });

        var post = requests.Single(r => r.Method == HttpMethod.Post);
        Assert.False(post.Headers.Contains("Idempotency-Key"));
    }

    [Fact]
    public async Task CreateReceiptAsync_With_IdempotencyKey_Forwards_Header()
    {
        var (client, requests) = CreateClient(_ => JsonOk("{\"id\":\"RCT_1\",\"code\":\"abc\"}"));

        await client.CreateReceiptAsync(new CreateReceiptRequest
        {
            Code = "abc",
            BuyerEmail = "x@y.test",
            Product = "PRD_1",
            Qty = 1
        }, "receipt-key-7");

        var post = requests.Single(r => r.Method == HttpMethod.Post);
        Assert.Equal("receipt-key-7", post.Headers.GetValues("Idempotency-Key").Single());
    }

    [Fact]
    public async Task CreateProductAsync_With_IdempotencyKey_Forwards_Header()
    {
        var (client, requests) = CreateClient(_ => JsonOk("{\"id\":\"PRD_1\",\"name\":\"A\"}"));

        await client.CreateProductAsync(new CreateProductRequest { Name = "A" }, "product-key");

        var post = requests.Single(r => r.Method == HttpMethod.Post);
        Assert.Equal("product-key", post.Headers.GetValues("Idempotency-Key").Single());
    }

    [Fact]
    public async Task RegisterComputerAsync_With_IdempotencyKey_Forwards_Header()
    {
        var (client, requests) = CreateClient(_ => JsonOk("{\"id\":\"PC_1\",\"macAddress\":\"x\"}"));

        await client.RegisterComputerAsync(new RegisterComputerRequest { MacAddress = "x", Name = "y" }, "comp-key");

        var post = requests.Single(r => r.Method == HttpMethod.Post);
        Assert.Equal("comp-key", post.Headers.GetValues("Idempotency-Key").Single());
    }

    [Fact]
    public async Task CreateWebhookAsync_With_IdempotencyKey_Forwards_Header()
    {
        var (client, requests) = CreateClient(_ => JsonOk("{\"id\":\"WH_1\",\"secret\":\"s\",\"url\":\"https://x\",\"events\":[]}"));

        await client.CreateWebhookAsync(new CreateWebhookRequest { Url = "https://x", Events = new[] { "*" } }, "wh-key");

        var post = requests.Single(r => r.Method == HttpMethod.Post);
        Assert.Equal("wh-key", post.Headers.GetValues("Idempotency-Key").Single());
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
        public CapturingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_responder(request));
    }
}
