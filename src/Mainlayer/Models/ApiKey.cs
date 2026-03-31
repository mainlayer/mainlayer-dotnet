using System;
using System.Text.Json.Serialization;

namespace Mainlayer.Models;

/// <summary>
/// Represents a Mainlayer API key.
/// </summary>
public sealed class ApiKey
{
    /// <summary>Gets the unique identifier of the API key.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the human-readable name of the API key.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the raw key value. This is only populated on creation and is never returned again.
    /// Store it securely.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    /// <summary>Gets the UTC timestamp when the key was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Request body for creating a new API key.
/// </summary>
public sealed class CreateApiKeyRequest
{
    /// <summary>Gets or sets the human-readable name to identify this key.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
