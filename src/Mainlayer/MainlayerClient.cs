using System;
using System.Net.Http;
using Mainlayer.Internal;
using Mainlayer.Services;

namespace Mainlayer;

/// <summary>
/// The primary entry point for interacting with the Mainlayer API.
/// </summary>
/// <remarks>
/// <para>
/// Create a single <see cref="MainlayerClient"/> instance per application lifetime and reuse it.
/// The client is thread-safe and manages its own <see cref="HttpClient"/> pool.
/// </para>
/// <para>
/// When using ASP.NET Core, prefer the <c>AddMainlayer()</c> extension method which integrates
/// with <c>IHttpClientFactory</c> for optimal connection management.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var client = new MainlayerClient("ml_your_api_key");
///
/// var resource = await client.Resources.CreateAsync(new CreateResourceRequest
/// {
///     Slug   = "my-weather-api",
///     Type   = ResourceType.Api,
///     FeeModel = FeeModel.PayPerCall,
///     PriceUsdc = 0.01m,
///     Description = "Real-time weather data"
/// });
/// </code>
/// </example>
public sealed class MainlayerClient
{
    private readonly MainlayerHttpClient _http;

    /// <summary>
    /// Gets the service for creating, listing, updating, and deleting resources.
    /// </summary>
    public ResourceService Resources { get; }

    /// <summary>
    /// Gets the service for initiating payments and listing payment history.
    /// </summary>
    public PaymentService Payments { get; }

    /// <summary>
    /// Gets the service for checking consumer entitlements (access control).
    /// </summary>
    public EntitlementService Entitlements { get; }

    /// <summary>
    /// Gets the service for discovering publicly listed resources.
    /// </summary>
    public DiscoverService Discover { get; }

    /// <summary>
    /// Gets the service for retrieving account analytics.
    /// </summary>
    public AnalyticsService Analytics { get; }

    /// <summary>
    /// Gets the service for managing webhook registrations.
    /// </summary>
    public WebhookService Webhooks { get; }

    /// <summary>
    /// Gets the service for managing API keys.
    /// </summary>
    public ApiKeyService ApiKeys { get; }

    /// <summary>
    /// Initialises a new <see cref="MainlayerClient"/> with the given API key and default options.
    /// </summary>
    /// <param name="apiKey">
    /// Your Mainlayer API key (starts with <c>ml_</c>).
    /// Obtain it from the Mainlayer dashboard.
    /// </param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKey"/> is null or empty.</exception>
    public MainlayerClient(string apiKey)
        : this(new MainlayerClientOptions { ApiKey = apiKey })
    {
    }

    /// <summary>
    /// Initialises a new <see cref="MainlayerClient"/> with explicit options.
    /// </summary>
    /// <param name="options">The client configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <c>options.ApiKey</c> is null or empty.</exception>
    public MainlayerClient(MainlayerClientOptions options)
        : this(new HttpClient(), options)
    {
    }

    /// <summary>
    /// Initialises a new <see cref="MainlayerClient"/> using a pre-configured <see cref="HttpClient"/>.
    /// Use this overload when integrating with <c>IHttpClientFactory</c>.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API requests.</param>
    /// <param name="options">The client configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <c>options.ApiKey</c> is null or empty.</exception>
    public MainlayerClient(HttpClient httpClient, MainlayerClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new ArgumentException(
                "An API key is required. Obtain one from the Mainlayer dashboard.",
                nameof(options));

        _http = new MainlayerHttpClient(httpClient, options);

        Resources = new ResourceService(_http);
        Payments = new PaymentService(_http);
        Entitlements = new EntitlementService(_http);
        Discover = new DiscoverService(_http);
        Analytics = new AnalyticsService(_http);
        Webhooks = new WebhookService(_http);
        ApiKeys = new ApiKeyService(_http);
    }
}
