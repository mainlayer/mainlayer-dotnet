using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mainlayer.Internal;
using Mainlayer.Models;

namespace Mainlayer.Services;

/// <summary>
/// Provides CRUD operations for Mainlayer resources.
/// </summary>
public sealed class ResourceService
{
    private readonly MainlayerHttpClient _http;

    internal ResourceService(MainlayerHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Returns all resources owned by the authenticated account.
    /// </summary>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>A list of <see cref="Resource"/> objects.</returns>
    public Task<IReadOnlyList<Resource>> ListAsync(CancellationToken ct = default) =>
        _http.GetAsync<IReadOnlyList<Resource>>("/resources", ct);

    /// <summary>
    /// Retrieves a single resource by its identifier.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>The <see cref="Resource"/> with the given <paramref name="id"/>.</returns>
    /// <exception cref="MainlayerException">Thrown when the resource is not found or the request fails.</exception>
    public Task<Resource> GetAsync(string id, CancellationToken ct = default) =>
        _http.GetAsync<Resource>($"/resources/{id}", ct);

    /// <summary>
    /// Retrieves a resource without authentication using its public identifier.
    /// Useful for displaying resource details to unauthenticated users.
    /// </summary>
    /// <param name="id">The public resource identifier.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>The public <see cref="Resource"/>.</returns>
    public Task<Resource> GetPublicAsync(string id, CancellationToken ct = default) =>
        _http.GetAsync<Resource>($"/resources/public/{id}", ct);

    /// <summary>
    /// Creates a new resource.
    /// </summary>
    /// <param name="request">The resource creation parameters.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>The newly created <see cref="Resource"/>.</returns>
    /// <exception cref="MainlayerException">Thrown when the request fails or validation errors occur.</exception>
    public Task<Resource> CreateAsync(CreateResourceRequest request, CancellationToken ct = default) =>
        _http.PostAsync<Resource>("/resources", request, ct);

    /// <summary>
    /// Partially updates an existing resource.
    /// </summary>
    /// <param name="id">The identifier of the resource to update.</param>
    /// <param name="request">The fields to update.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>The updated <see cref="Resource"/>.</returns>
    /// <exception cref="MainlayerException">Thrown when the resource is not found or the request fails.</exception>
    public Task<Resource> UpdateAsync(
        string id,
        UpdateResourceRequest request,
        CancellationToken ct = default) =>
        _http.PatchAsync<Resource>($"/resources/{id}", request, ct);

    /// <summary>
    /// Permanently deletes a resource.
    /// </summary>
    /// <param name="id">The identifier of the resource to delete.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <exception cref="MainlayerException">Thrown when the resource is not found or the request fails.</exception>
    public Task DeleteAsync(string id, CancellationToken ct = default) =>
        _http.DeleteAsync($"/resources/{id}", ct);
}
