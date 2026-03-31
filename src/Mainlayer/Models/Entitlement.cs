using System;
using System.Text.Json.Serialization;

namespace Mainlayer.Models;

/// <summary>
/// Represents an entitlement check result — whether a wallet has access to a resource.
/// </summary>
public sealed class Entitlement
{
    /// <summary>Gets whether the payer wallet has access to the specified resource.</summary>
    [JsonPropertyName("has_access")]
    public bool HasAccess { get; init; }

    /// <summary>Gets the identifier of the resource that was checked.</summary>
    [JsonPropertyName("resource_id")]
    public string ResourceId { get; init; } = string.Empty;

    /// <summary>Gets the wallet address that was checked.</summary>
    [JsonPropertyName("payer_wallet")]
    public string PayerWallet { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the entitlement expires, if applicable.</summary>
    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; init; }
}
