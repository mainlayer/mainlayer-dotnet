using System.Threading;
using System.Threading.Tasks;
using Mainlayer.Internal;
using Mainlayer.Models;

namespace Mainlayer.Services;

/// <summary>
/// Provides operations for managing Mainlayer API keys.
/// </summary>
public sealed class ApiKeyService
{
    private readonly MainlayerHttpClient _http;

    internal ApiKeyService(MainlayerHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Creates a new API key.
    /// </summary>
    /// <param name="request">The API key creation parameters.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>
    /// The newly created <see cref="ApiKey"/>. The <see cref="ApiKey.Key"/> property
    /// contains the raw key value — store it securely. It will not be returned again.
    /// </returns>
    /// <exception cref="MainlayerException">Thrown when the request fails.</exception>
    public Task<ApiKey> CreateAsync(CreateApiKeyRequest request, CancellationToken ct = default) =>
        _http.PostAsync<ApiKey>("/api-keys", request, ct);
}
