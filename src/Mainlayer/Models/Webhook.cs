using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mainlayer.Models;

/// <summary>
/// Represents a registered Mainlayer webhook endpoint.
/// </summary>
public sealed class Webhook
{
    /// <summary>Gets the unique identifier of the webhook.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the URL that Mainlayer will POST events to.</summary>
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    /// <summary>Gets the list of event types this webhook is subscribed to.</summary>
    [JsonPropertyName("events")]
    public IReadOnlyList<string> Events { get; init; } = [];

    /// <summary>Gets the UTC timestamp when the webhook was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Request body for registering a new webhook.
/// </summary>
public sealed class CreateWebhookRequest
{
    /// <summary>Gets or sets the HTTPS URL that Mainlayer will deliver events to.</summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>Gets or sets the event types to subscribe to (e.g. <c>"payment.created"</c>).</summary>
    [JsonPropertyName("events")]
    public IList<string> Events { get; set; } = new List<string>();
}
