using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mainlayer.Models;

/// <summary>
/// A summary of revenue earned by the authenticated vendor over a given period.
/// </summary>
public sealed class Revenue
{
    /// <summary>Gets the total revenue earned in USD.</summary>
    [JsonPropertyName("total_usd")]
    public decimal TotalUsd { get; init; }

    /// <summary>Gets the number of completed payments that contributed to this revenue.</summary>
    [JsonPropertyName("payment_count")]
    public long PaymentCount { get; init; }

    /// <summary>Gets the start of the reporting period.</summary>
    [JsonPropertyName("period_start")]
    public DateTimeOffset PeriodStart { get; init; }

    /// <summary>Gets the end of the reporting period.</summary>
    [JsonPropertyName("period_end")]
    public DateTimeOffset PeriodEnd { get; init; }

    /// <summary>Gets per-resource revenue breakdowns within this period.</summary>
    [JsonPropertyName("by_resource")]
    public IReadOnlyList<ResourceRevenue> ByResource { get; init; } = [];
}

/// <summary>
/// Revenue contribution from a single resource within a reporting period.
/// </summary>
public sealed class ResourceRevenue
{
    /// <summary>Gets the resource identifier.</summary>
    [JsonPropertyName("resource_id")]
    public string ResourceId { get; init; } = string.Empty;

    /// <summary>Gets the revenue earned from this resource in USD.</summary>
    [JsonPropertyName("revenue_usd")]
    public decimal RevenueUsd { get; init; }

    /// <summary>Gets the number of payments for this resource.</summary>
    [JsonPropertyName("payment_count")]
    public long PaymentCount { get; init; }
}
