using System.Net;
using System.Text;
using LicenseManagement.Client;
using Microsoft.Extensions.Options;

namespace LicenseManagement.Client.Tests;

/// <summary>
/// Regression tests for the C-1 fix: ResetReceiptCodeAsync now performs the void+create
/// atomically on the server via POST {receiptId}/rotate-code.
/// </summary>
public class ResetReceiptCodeTests
{
    [Fact]
    public async Task ResetReceiptCodeAsync_Calls_Server_Rotate_Endpoint_With_Idempotency_Key()
    {
        var captured = new List<HttpRequestMessage>();
        var handler = new CapturingHandler(req =>
        {
            captured.Add(req);
            if (req.Method == HttpMethod.Get && req.RequestUri!.AbsolutePath.EndsWith("/receipt"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"id\":\"RCT_01J0\",\"code\":\"old-code\",\"qty\":3,\"buyerEmail\":\"x@y.test\"}",
                        Encoding.UTF8, "application/json")
                };
            }
            if (req.Method == HttpMethod.Post && req.RequestUri!.AbsolutePath.EndsWith("/rotate-code"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"id\":\"RCT_01J0\",\"code\":\"NEW-CODE-XYZ\",\"qty\":3,\"buyerEmail\":\"x@y.test\"}",
                        Encoding.UTF8, "application/json")
                };
            }
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var client = new LicenseManagementClient(
            new HttpClient(handler),
            Options.Create(new LicenseManagementClientOptions
            {
                BaseUrl = "https://example.test",
                ApiKey = "test-key",
                TimeoutSeconds = 30
            }));

        var newCode = await client.ResetReceiptCodeAsync("old-code");

        Assert.Equal("NEW-CODE-XYZ", newCode);

        // Should be exactly two requests: lookup + rotate. No void/generate-code/create.
        Assert.Equal(2, captured.Count);
        Assert.Equal(HttpMethod.Get, captured[0].Method);
        Assert.Contains("/receipt", captured[0].RequestUri!.AbsolutePath);
        Assert.Equal(HttpMethod.Post, captured[1].Method);
        Assert.EndsWith("/rotate-code", captured[1].RequestUri!.AbsolutePath);

        // Idempotency key forwarded so a retried network call cannot duplicate the rotation.
        Assert.True(captured[1].Headers.Contains("Idempotency-Key"));
    }

    [Fact]
    public async Task ResetReceiptCodeAsync_Throws_When_Receipt_Not_Found()
    {
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                "{\"type\":\"https://license-management.com/errors/not-found\",\"title\":\"Not Found\",\"detail\":\"No receipt with that code\"}",
                Encoding.UTF8, "application/problem+json")
        });

        var client = new LicenseManagementClient(
            new HttpClient(handler),
            Options.Create(new LicenseManagementClientOptions
            {
                BaseUrl = "https://example.test",
                ApiKey = "test-key",
                TimeoutSeconds = 30
            }));

        var ex = await Assert.ThrowsAsync<Exceptions.LicenseManagementException>(
            () => client.ResetReceiptCodeAsync("missing"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("Resource not found", ex.Message);
        // H-3 fix: error message contains method + URL for diagnostics.
        Assert.Contains("GET", ex.Message);
        Assert.Contains("receipt", ex.Message);
        // Problem+json detail is surfaced.
        Assert.Contains("No receipt with that code", ex.Message);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
        public CapturingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = _responder(request);
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }
}
