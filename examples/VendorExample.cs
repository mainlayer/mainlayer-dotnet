/// <summary>
/// Demonstrates how a vendor (seller) uses the Mainlayer .NET SDK to:
///   1. Create a monetised resource
///   2. Register a webhook to receive payment notifications
///   3. Check analytics / revenue
///
/// Run from the project root:
///   dotnet run --project examples/Examples.csproj -- vendor
/// </summary>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mainlayer;
using Mainlayer.Models;

// ---------------------------------------------------------------------------
// Bootstrap
// ---------------------------------------------------------------------------

var apiKey = Environment.GetEnvironmentVariable("MAINLAYER_API_KEY")
             ?? throw new InvalidOperationException(
                 "Set the MAINLAYER_API_KEY environment variable before running this example.");

var client = new MainlayerClient(apiKey);

Console.WriteLine("=== Mainlayer Vendor Example ===\n");

// ---------------------------------------------------------------------------
// 1. Create a monetised API resource
// ---------------------------------------------------------------------------

Console.WriteLine("Creating resource...");

var resource = await client.Resources.CreateAsync(new CreateResourceRequest
{
    Slug        = "weather-forecast-v1",
    Type        = ResourceType.Api,
    FeeModel    = FeeModel.PayPerCall,
    PriceUsdc   = 0.01m,
    Name        = "Weather Forecast API",
    Description = "Real-time global weather forecasts. Pay once per query."
});

Console.WriteLine($"  Resource created: {resource.Id}  slug={resource.Slug}  price={resource.PriceUsdc} USD");

// ---------------------------------------------------------------------------
// 2. List all resources owned by this account
// ---------------------------------------------------------------------------

Console.WriteLine("\nListing owned resources...");

var resources = await client.Resources.ListAsync();
foreach (var r in resources)
{
    Console.WriteLine($"  [{r.Type}] {r.Slug} — {r.FeeModel} @ {r.PriceUsdc} USD");
}

// ---------------------------------------------------------------------------
// 3. Register a webhook to receive payment.created events
// ---------------------------------------------------------------------------

Console.WriteLine("\nRegistering webhook...");

var webhook = await client.Webhooks.CreateAsync(new CreateWebhookRequest
{
    Url    = "https://myapp.example.com/mainlayer/webhook",
    Events = new List<string> { "payment.created" }
});

Console.WriteLine($"  Webhook registered: {webhook.Id}  url={webhook.Url}");

// ---------------------------------------------------------------------------
// 4. Pull analytics
// ---------------------------------------------------------------------------

Console.WriteLine("\nFetching analytics...");

var analytics = await client.Analytics.GetAsync();

Console.WriteLine($"  Total payments : {analytics.TotalPayments}");
Console.WriteLine($"  Total revenue  : {analytics.TotalRevenueUsdc:F4} USD");
Console.WriteLine($"  Unique payers  : {analytics.UniquePayers}");

if (analytics.ByResource.Count > 0)
{
    Console.WriteLine("  Per-resource breakdown:");
    foreach (var item in analytics.ByResource)
    {
        Console.WriteLine($"    {item.ResourceId}: {item.PaymentCount} payments, {item.RevenueUsdc:F4} USD");
    }
}

// ---------------------------------------------------------------------------
// 5. Clean up — delete the resource created in this example
// ---------------------------------------------------------------------------

Console.WriteLine($"\nCleaning up resource {resource.Id}...");
await client.Resources.DeleteAsync(resource.Id);
Console.WriteLine("  Done.");

Console.WriteLine("\n=== Vendor example complete ===");
