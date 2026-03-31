namespace Mainlayer.Internal;

/// <summary>
/// Type alias kept for forward-compatibility.
/// The implementation lives in <see cref="MainlayerHttpClient"/>.
/// </summary>
internal static class HttpClientWrapper
{
    /// <summary>
    /// Creates a configured <see cref="MainlayerHttpClient"/> instance.
    /// </summary>
    internal static MainlayerHttpClient Create(
        System.Net.Http.HttpClient httpClient,
        MainlayerClientOptions options) =>
        new MainlayerHttpClient(httpClient, options);
}
