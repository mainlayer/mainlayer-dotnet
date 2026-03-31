using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mainlayer.Internal;
using Mainlayer.Models;

namespace Mainlayer.Services;

/// <summary>
/// Provides operations for managing Mainlayer webhook registrations.
/// </summary>
public sealed class WebhookService
{
    private readonly MainlayerHttpClient _http;

    internal WebhookService(MainlayerHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Lists all webhooks registered on the authenticated account.
    /// </summary>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>A list of <see cref="Webhook"/> registrations.</returns>
    public Task<IReadOnlyList<Webhook>> ListAsync(CancellationToken ct = default) =>
        _http.GetAsync<IReadOnlyList<Webhook>>("/webhooks", ct);

    /// <summary>
    /// Registers a new webhook endpoint.
    /// </summary>
    /// <param name="request">The webhook URL and event subscriptions.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>The newly created <see cref="Webhook"/>.</returns>
    /// <exception cref="MainlayerException">Thrown when the request fails or validation errors occur.</exception>
    public Task<Webhook> CreateAsync(CreateWebhookRequest request, CancellationToken ct = default) =>
        _http.PostAsync<Webhook>("/webhooks", request, ct);

    /// <summary>
    /// Deletes a webhook registration.
    /// </summary>
    /// <param name="id">The identifier of the webhook to delete.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <exception cref="MainlayerException">Thrown when the webhook is not found or the request fails.</exception>
    public Task DeleteAsync(string id, CancellationToken ct = default) =>
        _http.DeleteAsync($"/webhooks/{id}", ct);
}
