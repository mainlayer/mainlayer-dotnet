using System.Threading;
using System.Threading.Tasks;
using Mainlayer.Internal;
using Mainlayer.Models;

namespace Mainlayer.Services;

/// <summary>
/// Provides access to aggregated analytics for your Mainlayer account.
/// </summary>
public sealed class AnalyticsService
{
    private readonly MainlayerHttpClient _http;

    internal AnalyticsService(MainlayerHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Returns aggregated analytics for the authenticated account,
    /// including total revenue, payment counts, and per-resource breakdowns.
    /// </summary>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>An <see cref="Analytics"/> snapshot.</returns>
    public Task<Analytics> GetAsync(CancellationToken ct = default) =>
        _http.GetAsync<Analytics>("/analytics", ct);
}
