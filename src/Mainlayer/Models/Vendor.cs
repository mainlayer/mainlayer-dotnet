using System;
using System.Text.Json.Serialization;

namespace Mainlayer.Models;

/// <summary>
/// Represents the authenticated vendor (seller) account associated with an API key.
/// </summary>
public sealed class Vendor
{
    /// <summary>Gets the unique identifier of the vendor account.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the display name of the vendor.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the email address associated with the vendor account.</summary>
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the vendor account was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
}
