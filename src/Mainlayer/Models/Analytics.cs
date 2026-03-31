using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mainlayer.Models;

/// <summary>
/// Contains aggregated analytics data for your Mainlayer account.
/// </summary>
public sealed class Analytics
{
    /// <summary>Gets the total number of payments received.</summary>
    [JsonPropertyName("total_payments")]
    public long TotalPayments { get; init; }

    /// <summary>Gets the total revenue earned in USDC.</summary>
    [JsonPropertyName("total_revenue_usdc")]
    public decimal TotalRevenueUsdc { get; init; }

    /// <summary>Gets the total number of unique payers.</summary>
    [JsonPropertyName("unique_payers")]
    public long UniquePayers { get; init; }

    /// <summary>Gets per-resource breakdown of payment statistics.</summary>
    [JsonPropertyName("by_resource")]
    public IReadOnlyList<ResourceAnalytics> ByResource { get; init; } = [];
}

/// <summary>
/// Analytics data for a single resource.
/// </summary>
public sealed class ResourceAnalytics
{
    /// <summary>Gets the resource identifier.</summary>
    [JsonPropertyName("resource_id")]
    public string ResourceId { get; init; } = string.Empty;

    /// <summary>Gets the number of payments for this resource.</summary>
    [JsonPropertyName("payment_count")]
    public long PaymentCount { get; init; }

    /// <summary>Gets the total revenue for this resource in USDC.</summary>
    [JsonPropertyName("revenue_usdc")]
    public decimal RevenueUsdc { get; init; }
}
