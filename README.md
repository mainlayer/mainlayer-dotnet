# Mainlayer .NET SDK

Official .NET SDK for [Mainlayer](https://mainlayer.fr) — payment infrastructure for AI agents.

[![NuGet](https://img.shields.io/nuget/v/Mainlayer.svg)](https://www.nuget.org/packages/Mainlayer)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Mainlayer.svg)](https://www.nuget.org/packages/Mainlayer)
[![CI](https://github.com/mainlayer/mainlayer-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/mainlayer/mainlayer-dotnet/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Installation

```bash
dotnet add package Mainlayer
```

Or via NuGet Package Manager:
```bash
Install-Package Mainlayer
```

## Quick Start

```csharp
using Mainlayer;
using Mainlayer.Models;

// Initialize the client with your API key
var client = new MainlayerClient("ml_your_api_key");

// Create a monetised resource (async/await pattern)
var resource = await client.Resources.CreateAsync(new CreateResourceRequest
{
    Slug        = "weather-api",
    Type        = ResourceType.Api,
    FeeModel    = FeeModel.PayPerCall,
    PriceUsdc   = 0.01m,
    Name        = "Weather Forecast API",
    Description = "Real-time weather data for AI agents."
});

Console.WriteLine($"Resource created: {resource.Id}");

// Initiate a payment
var payment = await client.Payments.CreateAsync(new CreatePaymentRequest
{
    ResourceId  = resource.Id,
    PayerWallet = "wallet_abc123"
});

Console.WriteLine($"Payment initiated: {payment.Id}");

// Check consumer entitlement (access control)
var entitlement = await client.Entitlements.CheckAsync(resource.Id, "wallet_abc123");
if (entitlement.HasAccess)
{
    Console.WriteLine("Access granted!");
}
else
{
    Console.WriteLine("Access denied.");
}

// Discover public resources
var resources = await client.Discover.SearchAsync("weather", limit: 10);
foreach (var r in resources)
{
    Console.WriteLine($"Found: {r.Name} ({r.PriceUsdc} USDC)");
}
```

## Features

- **Resources** — create, list, update, and delete monetised APIs, files, and endpoints
- **Payments** — initiate payments and retrieve payment history
- **Entitlements** — check consumer access in real time
- **Discover** — search the public Mainlayer resource directory
- **Analytics** — aggregated revenue and payment statistics
- **Webhooks** — register endpoints for real-time payment events
- **API Keys** — programmatically manage API keys

## ASP.NET Core Integration

```csharp
// Program.cs
builder.Services.AddMainlayer(options =>
{
    options.ApiKey = builder.Configuration["Mainlayer:ApiKey"]!;
});

// Controller or minimal API
app.MapGet("/check", async (MainlayerClient mainlayer, string resourceId, string wallet) =>
{
    var entitlement = await mainlayer.Entitlements.CheckAsync(resourceId, wallet);
    return entitlement.HasAccess ? Results.Ok() : Results.Forbid();
});
```

## Configuration

```csharp
var client = new MainlayerClient(new MainlayerClientOptions
{
    ApiKey         = "ml_your_api_key",
    BaseUrl        = new Uri("https://api.mainlayer.fr"), // default
    MaxRetries     = 3,                                    // default
    RetryBaseDelay = TimeSpan.FromMilliseconds(500),       // default
    Timeout        = TimeSpan.FromSeconds(30)              // default
});
```

Transient failures (HTTP 429, 5xx) are retried with exponential back-off and respect the `Retry-After` response header.

## Error Handling

```csharp
try
{
    var resource = await client.Resources.GetAsync("res_missing");
}
catch (MainlayerException ex)
{
    Console.WriteLine($"Status : {(int)ex.StatusCode} {ex.StatusCode}");
    Console.WriteLine($"Code   : {ex.ErrorCode}");
    Console.WriteLine($"Message: {ex.Message}");
}
```

## Complete API Reference

### Resources

Manage monetised resources (APIs, files, endpoints, pages).

```csharp
// List all resources owned by your account
var resources = await client.Resources.ListAsync();

// Get a single resource by ID
var resource = await client.Resources.GetAsync("res_abc123");

// Get a public resource without authentication
var publicResource = await client.Resources.GetPublicAsync("res_abc123");

// Update a resource
var updated = await client.Resources.UpdateAsync("res_abc123", new UpdateResourceRequest
{
    Name = "Updated Name",
    PriceUsdc = 0.05m
});

// Delete a resource
await client.Resources.DeleteAsync("res_abc123");
```

### Payments

Initiate and track payments for resources.

```csharp
// Create a payment
var payment = await client.Payments.CreateAsync(new CreatePaymentRequest
{
    ResourceId = "res_abc123",
    PayerWallet = "wallet_xyz"
});

// List all payments
var payments = await client.Payments.ListAsync();

// Get payment details
var paymentDetail = await client.Payments.GetAsync(payment.Id);
```

### Entitlements

Check consumer access to resources in real-time.

```csharp
// Check if a wallet has access
var entitlement = await client.Entitlements.CheckAsync("res_abc123", "wallet_xyz");

if (entitlement.HasAccess)
{
    // Serve the resource
    Console.WriteLine("User has access");

    if (entitlement.ExpiresAt.HasValue)
    {
        Console.WriteLine($"Access expires at: {entitlement.ExpiresAt}");
    }

    if (entitlement.CreditsRemaining.HasValue)
    {
        Console.WriteLine($"Credits left: {entitlement.CreditsRemaining}");
    }
}
```

### Discovery

Search the public Mainlayer resource directory.

```csharp
// Simple keyword search
var results = await client.Discover.SearchAsync("weather api", limit: 10);

// Advanced search with filters
var filtered = await client.Discover.QueryAsync(new DiscoverQuery
{
    Query = "nlp",
    ResourceType = ResourceType.Api,
    FeeModel = FeeModel.PayPerCall,
    Limit = 20,
    Offset = 0
});

foreach (var result in filtered)
{
    Console.WriteLine($"{result.Name}: {result.Description}");
}
```

### Analytics

Retrieve account statistics and revenue data.

```csharp
// Get all-time analytics
var stats = await client.Analytics.GetAsync();

Console.WriteLine($"Total payments: {stats.TotalPayments}");
Console.WriteLine($"Total revenue: {stats.TotalRevenueUsdc:F2} USDC");
Console.WriteLine($"Unique payers: {stats.UniquePayers}");

// Get analytics for a date range
var filtered = await client.Analytics.GetAsync(new AnalyticsQuery
{
    StartDate = "2024-01-01",
    EndDate = "2024-12-31"
});

foreach (var item in filtered.ByResource)
{
    Console.WriteLine($"{item.ResourceId}: {item.PaymentCount} payments, {item.RevenueUsdc:F2} USDC");
}
```

### Webhooks

Register endpoints to receive real-time payment and entitlement notifications.

```csharp
// Register a webhook
var webhook = await client.Webhooks.CreateAsync(new CreateWebhookRequest
{
    Url = "https://myapp.example.com/webhooks/mainlayer",
    Events = new List<string> { "payment.created", "entitlement.expired" }
});

// List all webhooks
var webhooks = await client.Webhooks.ListAsync();

// Delete a webhook
await client.Webhooks.DeleteAsync(webhook.Id);

// Get webhook secret for signature verification
var secret = await client.Webhooks.GetSecretAsync("res_abc123");
```

### API Keys

Programmatically manage API keys for your account.

```csharp
// List all API keys
var keys = await client.ApiKeys.ListAsync();

// Create a new API key
var newKey = await client.ApiKeys.CreateAsync(new CreateApiKeyRequest
{
    Name = "Production Key"
});

Console.WriteLine($"New key created: {newKey.Id}");
Console.WriteLine($"Secret: {newKey.Key}"); // Store this securely!

// Delete an API key
await client.ApiKeys.DeleteAsync("key_abc123");
```

## Documentation

For more details, visit https://docs.mainlayer.fr

## Requirements

- .NET 8.0 or later
- .NET Framework 4.7.2 or later (for legacy projects)

## Error Handling

The SDK provides comprehensive error handling through the `MainlayerException` class:

```csharp
try
{
    var resource = await client.Resources.GetAsync("res_missing");
}
catch (MainlayerException ex)
{
    Console.WriteLine($"HTTP {(int)ex.StatusCode}: {ex.StatusCode}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
    Console.WriteLine($"Message: {ex.Message}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
```

## Thread Safety

The `MainlayerClient` is thread-safe and should be created once and reused across your application.
It internally manages HTTP connection pooling and is designed to be used as a singleton in ASP.NET Core applications.

## License

MIT
