/// <summary>
/// Demonstrates how a buyer (consumer / AI agent) uses the Mainlayer .NET SDK to:
///   1. Discover publicly listed resources
///   2. Pay for a resource
///   3. Check entitlement before accessing gated content
///
/// Run from the project root:
///   dotnet run --project examples/Examples.csproj -- buyer
/// </summary>

using System;
using System.Threading.Tasks;
using Mainlayer;
using Mainlayer.Models;

// ---------------------------------------------------------------------------
// Bootstrap
// ---------------------------------------------------------------------------

var apiKey = Environment.GetEnvironmentVariable("MAINLAYER_API_KEY")
             ?? throw new InvalidOperationException(
                 "Set the MAINLAYER_API_KEY environment variable before running this example.");

// The wallet address that represents this buyer / AI agent.
var myWallet = Environment.GetEnvironmentVariable("MAINLAYER_PAYER_WALLET")
               ?? "wallet_demo_abc123";

var client = new MainlayerClient(apiKey);

Console.WriteLine("=== Mainlayer Buyer Example ===\n");

// ---------------------------------------------------------------------------
// 1. Discover available resources
// ---------------------------------------------------------------------------

Console.WriteLine("Discovering resources (query: \"weather\")...");

var searchResult = await client.Discover.SearchAsync(new SearchRequest
{
    Query = "weather",
    Limit = 5
});

Console.WriteLine($"  Found {searchResult.Total} matching resource(s).");

if (searchResult.Results.Count == 0)
{
    Console.WriteLine("  No results found. Make sure the vendor example has been run first.");
    return;
}

foreach (var r in searchResult.Results)
{
    Console.WriteLine($"  [{r.Id}] {r.Slug} — {r.FeeModel} @ {r.PriceUsdc} USD");
}

// Pick the first result for this demo
var target = searchResult.Results[0];

// ---------------------------------------------------------------------------
// 2. Check entitlement before paying (expect false)
// ---------------------------------------------------------------------------

Console.WriteLine($"\nChecking entitlement for resource '{target.Id}' (before payment)...");

var before = await client.Entitlements.CheckAsync(target.Id, myWallet);
Console.WriteLine($"  Has access: {before.HasAccess}");

// ---------------------------------------------------------------------------
// 3. Pay for the resource
// ---------------------------------------------------------------------------

if (!before.HasAccess)
{
    Console.WriteLine($"\nPaying for resource '{target.Id}'...");

    var payment = await client.Payments.CreateAsync(new CreatePaymentRequest
    {
        ResourceId  = target.Id,
        PayerWallet = myWallet
    });

    Console.WriteLine($"  Payment created: {payment.Id}  status={payment.Status}  amount={payment.AmountUsdc} USD");
}

// ---------------------------------------------------------------------------
// 4. Check entitlement again (expect true)
// ---------------------------------------------------------------------------

Console.WriteLine($"\nChecking entitlement for resource '{target.Id}' (after payment)...");

var after = await client.Entitlements.CheckAsync(target.Id, myWallet);
Console.WriteLine($"  Has access: {after.HasAccess}");

if (after.ExpiresAt.HasValue)
    Console.WriteLine($"  Expires at: {after.ExpiresAt:O}");

// ---------------------------------------------------------------------------
// 5. List payment history
// ---------------------------------------------------------------------------

Console.WriteLine("\nPayment history for this account:");

var payments = await client.Payments.ListAsync();
foreach (var p in payments)
{
    Console.WriteLine($"  {p.Id}  resource={p.ResourceId}  amount={p.AmountUsdc} USD  status={p.Status}");
}

Console.WriteLine("\n=== Buyer example complete ===");
