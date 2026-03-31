using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mainlayer.Internal;
using Mainlayer.Models;

namespace Mainlayer.Services;

/// <summary>
/// Provides operations for initiating and listing Mainlayer payments.
/// </summary>
public sealed class PaymentService
{
    private readonly MainlayerHttpClient _http;

    internal PaymentService(MainlayerHttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Initiates a payment for a resource on behalf of a payer wallet.
    /// </summary>
    /// <param name="request">The payment details.</param>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>The created <see cref="Payment"/> record.</returns>
    /// <exception cref="MainlayerException">Thrown when the request fails or the resource does not exist.</exception>
    public Task<Payment> CreateAsync(CreatePaymentRequest request, CancellationToken ct = default) =>
        _http.PostAsync<Payment>("/pay", request, ct);

    /// <summary>
    /// Lists all payments received by the authenticated account.
    /// </summary>
    /// <param name="ct">An optional cancellation token.</param>
    /// <returns>A list of <see cref="Payment"/> records.</returns>
    public Task<IReadOnlyList<Payment>> ListAsync(CancellationToken ct = default) =>
        _http.GetAsync<IReadOnlyList<Payment>>("/payments", ct);
}
