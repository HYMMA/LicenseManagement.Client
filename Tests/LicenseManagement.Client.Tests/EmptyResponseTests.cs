using System.Net;
using System.Text;
using LicenseManagement.Client;
using LicenseManagement.Client.Models;
using Microsoft.Extensions.Options;

namespace LicenseManagement.Client.Tests;

/// <summary>
/// Regression tests for the bug where Get* collection methods threw
/// System.Text.Json.JsonException on HTTP 204 No Content or 200 with empty body.
/// </summary>
public class EmptyResponseTests
{
    private static LicenseManagementClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new StubHandler(responder);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new LicenseManagementClientOptions
        {
            BaseUrl = "https://example.test",
            ApiKey = "test-key",
            TimeoutSeconds = 30
        });
        return new LicenseManagementClient(httpClient, options);
    }

    private static HttpResponseMessage NoContent() => new(HttpStatusCode.NoContent);

    private static HttpResponseMessage OkEmpty()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
        };
        return response;
    }

    [Fact]
    public async Task GetComputersAsync_Returns_Empty_On_204_NoContent()
    {
        var client = CreateClient(_ => NoContent());

        var result = await client.GetComputersAsync("RCP_123");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetComputersAsync_Returns_Empty_On_200_With_Empty_Body()
    {
        var client = CreateClient(_ => OkEmpty());

        var result = await client.GetComputersAsync("RCP_123");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetComputersAsync_Returns_Items_On_200_With_Json_Array()
    {
        var json = "[{\"id\":\"PC_1\",\"macAddress\":\"AA:BB:CC:DD:EE:FF\",\"name\":\"Dev box\"}]";
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = (await client.GetComputersAsync("RCP_123")).ToList();

        Assert.Single(result);
        Assert.Equal("PC_1", result[0].Id);
    }

    [Fact]
    public async Task GetProductsAsync_Returns_Empty_On_204_NoContent()
    {
        var client = CreateClient(_ => NoContent());

        var result = await client.GetProductsAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProductsAsync_Returns_Empty_On_200_With_Empty_Body()
    {
        var client = CreateClient(_ => OkEmpty());

        var result = await client.GetProductsAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReceiptsAsync_Returns_Empty_On_204_NoContent()
    {
        var client = CreateClient(_ => NoContent());

        var result = await client.GetReceiptsAsync("buyer@example.com", "PRD_1");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReceiptsAsync_Returns_Empty_On_200_With_Empty_Body()
    {
        var client = CreateClient(_ => OkEmpty());

        var result = await client.GetReceiptsAsync("buyer@example.com", "PRD_1");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWebhooksAsync_Returns_Empty_On_204_NoContent()
    {
        var client = CreateClient(_ => NoContent());

        var result = await client.GetWebhooksAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWebhooksAsync_Returns_Empty_On_200_With_Empty_Body()
    {
        var client = CreateClient(_ => OkEmpty());

        var result = await client.GetWebhooksAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWebhookDeliveriesAsync_Returns_Empty_On_204_NoContent()
    {
        var client = CreateClient(_ => NoContent());

        var result = await client.GetWebhookDeliveriesAsync("WH_1");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetComputerAsync_Returns_Null_On_200_With_Empty_Body()
    {
        var client = CreateClient(_ => OkEmpty());

        var result = await client.GetComputerAsync("AA:BB:CC:DD:EE:FF");

        Assert.Null(result);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }
}
