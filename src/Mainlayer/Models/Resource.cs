using System;
using System.Text.Json.Serialization;

namespace Mainlayer.Models;

/// <summary>
/// The type of a Mainlayer resource.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResourceType
{
    /// <summary>An API endpoint resource.</summary>
    [JsonStringEnumMemberName("api")]
    Api,

    /// <summary>A file resource.</summary>
    [JsonStringEnumMemberName("file")]
    File,

    /// <summary>An endpoint resource.</summary>
    [JsonStringEnumMemberName("endpoint")]
    Endpoint,

    /// <summary>A page resource.</summary>
    [JsonStringEnumMemberName("page")]
    Page
}

/// <summary>
/// The billing model for a Mainlayer resource.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FeeModel
{
    /// <summary>Charge once per purchase.</summary>
    [JsonStringEnumMemberName("one_time")]
    OneTime,

    /// <summary>Charge on a recurring subscription basis.</summary>
    [JsonStringEnumMemberName("subscription")]
    Subscription,

    /// <summary>Charge per individual call or usage.</summary>
    [JsonStringEnumMemberName("pay_per_call")]
    PayPerCall
}

/// <summary>
/// Represents a Mainlayer resource — a monetised API, file, endpoint, or page.
/// </summary>
public sealed class Resource
{
    /// <summary>Gets the unique identifier of the resource.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the URL-friendly slug for the resource.</summary>
    [JsonPropertyName("slug")]
    public string Slug { get; init; } = string.Empty;

    /// <summary>Gets the human-readable name of the resource.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Gets the description of the resource.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Gets the resource type.</summary>
    [JsonPropertyName("type")]
    public ResourceType Type { get; init; }

    /// <summary>Gets the fee model used to bill consumers.</summary>
    [JsonPropertyName("fee_model")]
    public FeeModel FeeModel { get; init; }

    /// <summary>Gets the price in USDC.</summary>
    [JsonPropertyName("price_usdc")]
    public decimal PriceUsdc { get; init; }

    /// <summary>Gets the UTC timestamp when the resource was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Gets the UTC timestamp when the resource was last updated.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Request body for creating a new resource.
/// </summary>
public sealed class CreateResourceRequest
{
    /// <summary>Gets or sets the URL-friendly slug. Must be unique within your account.</summary>
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    /// <summary>Gets or sets the resource type.</summary>
    [JsonPropertyName("type")]
    public ResourceType Type { get; set; }

    /// <summary>Gets or sets the price in USDC.</summary>
    [JsonPropertyName("price_usdc")]
    public decimal PriceUsdc { get; set; }

    /// <summary>Gets or sets the billing model.</summary>
    [JsonPropertyName("fee_model")]
    public FeeModel FeeModel { get; set; }

    /// <summary>Gets or sets an optional human-readable name.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>Gets or sets an optional description.</summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }
}

/// <summary>
/// Request body for partially updating an existing resource.
/// </summary>
public sealed class UpdateResourceRequest
{
    /// <summary>Gets or sets the updated description.</summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>Gets or sets the updated name.</summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>Gets or sets the updated price in USDC.</summary>
    [JsonPropertyName("price_usdc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? PriceUsdc { get; set; }

    /// <summary>Gets or sets the updated fee model.</summary>
    [JsonPropertyName("fee_model")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FeeModel? FeeModel { get; set; }
}
