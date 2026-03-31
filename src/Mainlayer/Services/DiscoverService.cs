using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mainlayer.Internal;
using Mainlayer.Models;

namespace Mainlayer.Services;

/// <summary>
/// Provides discovery of publicly listed Mainlayer resources.
/// </summary>
public sealed class DiscoverService
{
    private readonly MainlayerHttpClient _http;

    internal DiscoverService(MainlayerHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Searches the public Mainlayer resource directory.
    /// </summary>
    /// <param name="request">The search parameters.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>A <see cref="SearchResult"/> containing matching resources.</returns>
    public Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken ct = default)
    {
        var qs = BuildQueryString(request);
        return _http.GetAsync<SearchResult>($"/discover{qs}", ct);
    }

    private static string BuildQueryString(SearchRequest request)
    {
        var sb = new StringBuilder("?");

        if (!string.IsNullOrWhiteSpace(request.Query))
            sb.Append($"q={Uri.EscapeDataString(request.Query)}&");

        if (request.Type.HasValue)
            sb.Append($"type={request.Type.Value.ToString().ToLowerInvariant()}&");

        if (request.FeeModel.HasValue)
        {
            var feeModel = request.FeeModel.Value switch
            {
                FeeModel.OneTime => "one_time",
                FeeModel.Subscription => "subscription",
                FeeModel.PayPerCall => "pay_per_call",
                _ => request.FeeModel.Value.ToString().ToLowerInvariant()
            };
            sb.Append($"fee_model={feeModel}&");
        }

        if (request.Limit.HasValue)
            sb.Append($"limit={request.Limit.Value}&");

        // Remove trailing '?' or '&'
        return sb.Length == 1 ? string.Empty : sb.ToString().TrimEnd('&');
    }
}
