# LicenseManagement.Client

Server-side SDK for [license-management.com](https://license-management.com) - A license management platform for software vendors.

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

- `GetLicenseAsync(productId, computerId)` - Get a license by product and computer
- `CreateLicenseAsync(request)` - Create a new trial license
- `UpdateLicenseAsync(request)` - Update a license (attach receipt code)

### Receipts

- `GetReceiptAsync(code)` - Get a receipt by code
- `CreateReceiptAsync(request)` - Create a new receipt
- `UpdateReceiptAsync(request)` - Update a receipt
- `GenerateReceiptCodeAsync(productId)` - Generate a receipt code

### Products

- `GetProductAsync(productId)` - Get a product by ID
- `GetProductsAsync()` - Get all products
- `CreateProductAsync(request)` - Create a new product

### Computers

- `GetComputerAsync(macAddress)` - Get a computer by MAC address
- `RegisterComputerAsync(request)` - Register a new computer
- `GetComputersAsync(receiptCode)` - Get all computers for a receipt

### Signing Keys

- `GetPublicKeyAsync(format)` - Get the vendor's public signing key

## Error Handling

```csharp
try
{
    var license = await _client.GetLicenseAsync(productId, computerId);
}
catch (LicenseManagementException ex)
{
    Console.WriteLine($"Error: {ex.StatusCode} - {ex.Message}");
}
```

## License

MIT
