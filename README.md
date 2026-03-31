# Mainlayer .NET SDK

Official .NET SDK for [Mainlayer](https://mainlayer.xyz) — payment infrastructure for AI agents.

## Installation

```bash
dotnet add package Mainlayer
```

## Quick Start

```csharp
using Mainlayer;
using Mainlayer.Models;

var client = new MainlayerClient("ml_your_api_key");

// Create a monetised resource
var resource = await client.Resources.CreateAsync(new CreateResourceRequest
{
    Slug        = "weather-api",
    Type        = ResourceType.Api,
    FeeModel    = FeeModel.PayPerCall,
    PriceUsdc   = 0.01m,
    Name        = "Weather Forecast API",
    Description = "Real-time weather data for AI agents."
});

// Pay for a resource
var payment = await client.Payments.CreateAsync(new CreatePaymentRequest
{
    ResourceId  = resource.Id,
    PayerWallet = "wallet_abc123"
});

// Check whether a wallet has access
var entitlement = await client.Entitlements.CheckAsync(resource.Id, "wallet_abc123");
if (entitlement.HasAccess)
{
    Console.WriteLine("Access granted.");
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
    BaseUrl        = new Uri("https://api.mainlayer.xyz"), // default
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

## Requirements

- .NET 8.0 or .NET 9.0

## License

MIT
