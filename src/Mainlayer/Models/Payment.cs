using System;
using System.Text.Json.Serialization;

namespace Mainlayer.Models;

/// <summary>
/// Represents a payment transaction processed through Mainlayer.
/// </summary>
public sealed class Payment
{
    /// <summary>Gets the unique identifier of the payment.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the identifier of the resource that was paid for.</summary>
    [JsonPropertyName("resource_id")]
    public string ResourceId { get; init; } = string.Empty;

    /// <summary>Gets the wallet address of the payer.</summary>
    [JsonPropertyName("payer_wallet")]
    public string PayerWallet { get; init; } = string.Empty;

    /// <summary>Gets the amount charged in USDC.</summary>
    [JsonPropertyName("amount_usdc")]
    public decimal AmountUsdc { get; init; }

    /// <summary>Gets the current status of the payment.</summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the payment was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Request body for initiating a Mainlayer payment.
/// </summary>
public sealed class CreatePaymentRequest
{
    /// <summary>Gets or sets the identifier of the resource to pay for.</summary>
    [JsonPropertyName("resource_id")]
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the wallet address of the payer.</summary>
    [JsonPropertyName("payer_wallet")]
    public string PayerWallet { get; set; } = string.Empty;
}
