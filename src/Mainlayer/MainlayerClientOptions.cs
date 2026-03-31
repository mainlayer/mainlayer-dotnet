using System;

namespace Mainlayer;

/// <summary>
/// Configuration options for <see cref="MainlayerClient"/>.
/// </summary>
public sealed class MainlayerClientOptions
{
    /// <summary>
    /// Gets or sets the Mainlayer API key used to authenticate requests.
    /// </summary>
    /// <remarks>
    /// Obtain your API key from the Mainlayer dashboard. Store it in a secret manager or
    /// environment variable — never hard-code it in source files.
    /// </remarks>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL of the Mainlayer API.
    /// Defaults to <c>https://api.mainlayer.fr</c>.
    /// </summary>
    public Uri BaseUrl { get; set; } = new Uri("https://api.mainlayer.fr");

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for transient failures (429 / 5xx).
    /// Defaults to <c>3</c>. Set to <c>0</c> to disable retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay for exponential back-off between retries.
    /// Defaults to <c>500 ms</c>.
    /// </summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets or sets the HTTP request timeout.
    /// Defaults to <c>30 seconds</c>.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
