using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mainlayer.Extensions;

/// <summary>
/// Extension methods for registering the Mainlayer SDK with the ASP.NET Core dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="MainlayerClient"/> as a singleton service with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add Mainlayer to.</param>
    /// <param name="configure">A delegate to configure <see cref="MainlayerClientOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddMainlayer(options =>
    /// {
    ///     options.ApiKey = builder.Configuration["Mainlayer:ApiKey"]!;
    /// });
    ///
    /// // Then inject MainlayerClient anywhere:
    /// public class PaymentController(MainlayerClient mainlayer) { ... }
    /// </code>
    /// </example>
    public static IServiceCollection AddMainlayer(
        this IServiceCollection services,
        Action<MainlayerClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);

        services.AddHttpClient<MainlayerClient>((serviceProvider, httpClient) =>
        {
            var opts = serviceProvider
                .GetRequiredService<IOptions<MainlayerClientOptions>>()
                .Value;

            // HttpClient is configured inside MainlayerHttpClient, but we set the
            // timeout here so IHttpClientFactory can pool correctly.
            httpClient.Timeout = opts.Timeout;
        });

        services.AddSingleton(serviceProvider =>
        {
            var opts = serviceProvider
                .GetRequiredService<IOptions<MainlayerClientOptions>>()
                .Value;

            var httpClientFactory = serviceProvider
                .GetRequiredService<IHttpClientFactory>();

            var httpClient = httpClientFactory.CreateClient(nameof(MainlayerClient));
            return new MainlayerClient(httpClient, opts);
        });

        return services;
    }
}
