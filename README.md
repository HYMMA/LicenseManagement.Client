# LicenseManagement.Client

[![Build and Test](https://github.com/HYMMA/LicenseManagement.Client/actions/workflows/build.yml/badge.svg)](https://github.com/HYMMA/LicenseManagement.Client/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/LicenseManagement.Client.svg)](https://www.nuget.org/packages/LicenseManagement.Client)
[![NuGet Downloads](https://img.shields.io/nuget/dt/LicenseManagement.Client.svg)](https://www.nuget.org/packages/LicenseManagement.Client)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Server-side SDK for [license-management.com](https://license-management.com) - A license management platform for software vendors.

## Features

- **License Management** - Create, retrieve, and update licenses
- **Receipt Management** - Manage purchase records and subscriptions
- **Computer Tracking** - Register and track end-user devices
- **Product Management** - Create and manage software products
- **Webhook Support** - Full webhook lifecycle management with signature validation
- **Multi-Framework** - Supports .NET Standard 2.0, .NET 6.0, and .NET 8.0

## Installation

```bash
dotnet add package LicenseManagement.Client
```

## Quick Start

### Basic Setup

```csharp
using LicenseManagement.Client;
using Microsoft.Extensions.DependencyInjection;

// Register the client in your DI container
services.AddLicenseManagementClient(options =>
{
    options.ApiKey = "MST_01ARZ3..."; // Your master API key
    options.BaseUrl = "https://license-management.com/api";
});
```

### Using the Client

```csharp
public class LicenseService
{
    private readonly ILicenseManagementClient _client;

    public LicenseService(ILicenseManagementClient client)
    {
        _client = client;
    }

    public async Task<License?> GetLicenseAsync(string productId, string computerId)
    {
        return await _client.GetLicenseAsync(productId, computerId);
    }

    public async Task<Receipt> CreateReceiptAsync(string productId, string buyerEmail, int qty)
    {
        return await _client.CreateReceiptAsync(new CreateReceiptRequest
        {
            Product = productId,
            BuyerEmail = buyerEmail,
            Qty = qty,
            Expires = DateTime.UtcNow.AddYears(1)
        });
    }
}
```

## API Reference

### Licenses

| Method | Description |
|--------|-------------|
| `GetLicenseAsync(productId, computerId)` | Get a license by product and computer |
| `CreateLicenseAsync(request)` | Create a new trial license |
| `UpdateLicenseAsync(request)` | Update a license (attach/detach receipt code) |

### Receipts

| Method | Description |
|--------|-------------|
| `GetReceiptAsync(code)` | Get a receipt by code |
| `CreateReceiptAsync(request)` | Create a new receipt |
| `UpdateReceiptAsync(request)` | Update receipt (qty, expires, email) |
| `GenerateReceiptCodeAsync(productId, email)` | Generate a receipt code |

### Products

| Method | Description |
|--------|-------------|
| `GetProductAsync(productId)` | Get a product by ID |
| `GetProductsAsync()` | Get all products for vendor |
| `CreateProductAsync(request)` | Create a new product |

### Computers

| Method | Description |
|--------|-------------|
| `GetComputerAsync(macAddress)` | Get a computer by MAC address |
| `RegisterComputerAsync(request)` | Register a new computer |
| `GetComputersAsync(receiptCode)` | Get all computers for a receipt |

### Signing Keys

| Method | Description |
|--------|-------------|
| `GetPublicKeyAsync(format)` | Get vendor's public signing key (xml/pem) |

### Webhooks

| Method | Description |
|--------|-------------|
| `GetWebhooksAsync()` | List all webhooks |
| `CreateWebhookAsync(request)` | Create a new webhook |
| `UpdateWebhookAsync(webhookId, request)` | Update webhook configuration |
| `DeleteWebhookAsync(webhookId)` | Delete a webhook |
| `RotateWebhookSecretAsync(webhookId, immediate)` | Rotate signing secret |
| `GetWebhookHealthAsync(webhookId)` | Get webhook health status |
| `GetWebhookStatsAsync(webhookId)` | Get delivery statistics |
| `ReplayWebhookDeliveryAsync(webhookId, deliveryId)` | Replay a failed delivery |

## Webhook Signature Validation

For ASP.NET Core applications (.NET 6.0+), use the built-in filter:

```csharp
// Register webhook validation
services.AddLicenseManagementWebhooks(options =>
{
    options.Secret = "whsec_...";
    options.TimestampTolerance = TimeSpan.FromMinutes(5);
});

// Apply to controller
[HttpPost("webhook")]
[EnsureIsFromLicenseManagement]
public async Task<IActionResult> HandleWebhook([FromBody] WebhookPayload payload)
{
    // Signature is automatically validated
    return Ok();
}
```

## Error Handling

```csharp
try
{
    var license = await _client.GetLicenseAsync(productId, computerId);
}
catch (LicenseManagementException ex)
{
    Console.WriteLine($"Error: {ex.StatusCode} - {ex.Message}");
    // Handle specific status codes: 400, 401, 403, 404, 409, 422
}
```

## Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `ApiKey` | Master API key (MST_...) | Required |
| `BaseUrl` | API base URL | `https://license-management.com/api` |
| `Timeout` | HTTP request timeout | 30 seconds |

## Requirements

- .NET Standard 2.0+ / .NET 6.0+ / .NET 8.0+
- Master API key from license-management.com

## License

MIT - See [LICENSE](LICENSE) for details.

## Related Packages

- [LicenseManagement.EndUser](https://www.nuget.org/packages/LicenseManagement.EndUser) - End-user SDK for license validation
- [LicenseManagement.EndUser.Wpf](https://www.nuget.org/packages/LicenseManagement.EndUser.Wpf) - WPF components for license UI
