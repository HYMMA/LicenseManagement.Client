using System.Net;
using System.Text;
using LicenseManagement.Client;
using LicenseManagement.Client.Exceptions;
using Microsoft.Extensions.Options;

namespace LicenseManagement.Client.Tests;

/// <summary>
/// Regression tests for the H-3 / M-5 fixes:
///   * Non-2xx responses surface URL + method + problem+json detail
///   * X-Correlation-Id header is captured into the exception
/// </summary>
public class ProblemDetailsTests
{
    private static LicenseManagementClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new StubHandler(responder);
        return new LicenseManagementClient(
            new HttpClient(handler),
            Options.Create(new LicenseManagementClientOptions
            {
                BaseUrl = "https://example.test",
                ApiKey = "test-key",
                TimeoutSeconds = 30
            }));
    }

    [Fact]
    public async Task GetReceiptAsync_404_Includes_Method_Url_And_ProblemDetail()
    {
        var client = CreateClient(_ =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(
                    "{\"type\":\"https://license-management.com/errors/not-found\",\"title\":\"Not Found\",\"detail\":\"receipt with code abc does not exist\"}",
                    Encoding.UTF8, "application/problem+json")
            };
            return resp;
        });

        var ex = await Assert.ThrowsAsync<LicenseManagementException>(() => client.GetReceiptAsync("abc"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("Resource not found for GET", ex.Message);
        Assert.Contains("receipt", ex.Message);
        Assert.Contains("receipt with code abc does not exist", ex.Message);
    }

    [Fact]
    public async Task Failure_With_Correlation_Id_Header_Surfaces_On_Exception()
    {
        var client = CreateClient(_ =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/problem+json")
            };
            resp.Headers.Add("X-Correlation-Id", "00-trace123-span456-01");
            return resp;
        });

        var ex = await Assert.ThrowsAsync<LicenseManagementException>(() => client.GetReceiptAsync("abc"));

        Assert.Equal("00-trace123-span456-01", ex.CorrelationId);
        Assert.Contains("correlationId=00-trace123-span456-01", ex.Message);
    }

    [Fact]
    public async Task Failure_Without_ProblemDetails_Body_Still_Includes_Url()
    {
        // Audit H-3 minimum: when the server doesn't return problem+json, the SDK still embeds
        // method + URL in the message.
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("oops", Encoding.UTF8, "text/plain")
        });

        var ex = await Assert.ThrowsAsync<LicenseManagementException>(() => client.GetReceiptAsync("abc"));

        Assert.Contains("API request failed", ex.Message);
        Assert.Contains("GET", ex.Message);
        Assert.Contains("receipt", ex.Message);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = _responder(request);
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }
}
