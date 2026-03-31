using System.Threading;
using System.Threading.Tasks;
using Mainlayer.Internal;
using Mainlayer.Models;

namespace Mainlayer.Services;

/// <summary>
/// Provides access-control checks using Mainlayer entitlements.
/// </summary>
public sealed class EntitlementService
{
    private readonly MainlayerHttpClient _http;

    internal EntitlementService(MainlayerHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Checks whether a given wallet address has access to a resource.
    /// </summary>
    /// <param name="resourceId">The resource identifier to check.</param>
    /// <param name="payerWallet">The wallet address of the user requesting access.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>
    /// An <see cref="Entitlement"/> with <see cref="Entitlement.HasAccess"/> set to
    /// <c>true</c> if the wallet is entitled to use the resource.
    /// </returns>
    /// <exception cref="MainlayerException">Thrown when the request fails.</exception>
    public Task<Entitlement> CheckAsync(
        string resourceId,
        string payerWallet,
        CancellationToken ct = default) =>
        _http.GetAsync<Entitlement>(
            $"/entitlements/check?resource_id={Uri.EscapeDataString(resourceId)}&payer_wallet={Uri.EscapeDataString(payerWallet)}",
            ct);
}
