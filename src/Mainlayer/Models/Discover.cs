using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mainlayer.Models;

/// <summary>
/// Parameters for searching the public Mainlayer resource directory.
/// </summary>
public sealed class SearchRequest
{
    /// <summary>Gets or sets the full-text search query.</summary>
    public string? Query { get; set; }

    /// <summary>Gets or sets an optional type filter.</summary>
    public ResourceType? Type { get; set; }

    /// <summary>Gets or sets an optional fee model filter.</summary>
    public FeeModel? FeeModel { get; set; }

    /// <summary>Gets or sets the maximum number of results to return. Defaults to 20.</summary>
    public int? Limit { get; set; }
}

/// <summary>
/// The result of a resource discovery search.
/// </summary>
public sealed class SearchResult
{
    /// <summary>Gets the list of matching resources.</summary>
    [JsonPropertyName("results")]
    public IReadOnlyList<Resource> Results { get; init; } = [];

    /// <summary>Gets the total number of matching resources (before the limit was applied).</summary>
    [JsonPropertyName("total")]
    public long Total { get; init; }
}
