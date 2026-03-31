using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Mainlayer.Internal;

/// <summary>
/// Internal HTTP helper that wraps <see cref="HttpClient"/> with authentication,
/// serialisation, and retry logic.
/// </summary>
internal sealed class MainlayerHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly MainlayerClientOptions _options;

    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };

    public MainlayerHttpClient(HttpClient httpClient, MainlayerClientOptions options)
    {
        _httpClient = httpClient;
        _options = options;

        _httpClient.BaseAddress = options.BaseUrl;
        _httpClient.Timeout = options.Timeout;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.ApiKey);
        _httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add(
            "User-Agent", $"mainlayer-dotnet/{ThisAssembly.Version}");
    }

    public Task<T> GetAsync<T>(string path, CancellationToken ct = default) =>
        SendWithRetryAsync<T>(HttpMethod.Get, path, body: null, ct);

    public Task<T> PostAsync<T>(string path, object? body, CancellationToken ct = default) =>
        SendWithRetryAsync<T>(HttpMethod.Post, path, body, ct);

    public Task<T> PatchAsync<T>(string path, object? body, CancellationToken ct = default) =>
        SendWithRetryAsync<T>(HttpMethod.Patch, path, body, ct);

    public Task DeleteAsync(string path, CancellationToken ct = default) =>
        SendWithRetryAsync<object?>(HttpMethod.Delete, path, body: null, ct, expectBody: false);

    // -------------------------------------------------------------------------

    private async Task<T> SendWithRetryAsync<T>(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken ct,
        bool expectBody = true)
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            using var request = BuildRequest(method, path, body);
            HttpResponseMessage response;

            try
            {
                response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new MainlayerException(
                    "The request timed out.", HttpStatusCode.RequestTimeout);
            }

            if (response.IsSuccessStatusCode)
            {
                if (!expectBody) return default!;
                var result = await response.Content
                    .ReadFromJsonAsync<T>(JsonOptions, ct)
                    .ConfigureAwait(false);
                return result!;
            }

            var shouldRetry = ShouldRetry(response.StatusCode) &&
                              attempt <= _options.MaxRetries;

            if (shouldRetry)
            {
                var delay = CalculateDelay(attempt, response);
                await Task.Delay(delay, ct).ConfigureAwait(false);
                continue;
            }

            await ThrowApiExceptionAsync(response, ct).ConfigureAwait(false);
            throw new InvalidOperationException("Unreachable"); // satisfy compiler
        }
    }

    private HttpRequestMessage BuildRequest(HttpMethod method, string path, object? body)
    {
        var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }
        return request;
    }

    private static bool ShouldRetry(HttpStatusCode status) =>
        status == HttpStatusCode.TooManyRequests ||
        (int)status >= 500;

    private TimeSpan CalculateDelay(int attempt, HttpResponseMessage response)
    {
        // Honour Retry-After if the server supplies it.
        if (response.Headers.RetryAfter?.Delta is { } delta)
            return delta;

        // Exponential back-off: 500 ms, 1 s, 2 s, …
        return TimeSpan.FromMilliseconds(
            _options.RetryBaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
    }

    private static async Task ThrowApiExceptionAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        string raw;
        try
        {
            raw = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            raw = string.Empty;
        }

        string? errorCode = null;
        string message = response.ReasonPhrase ?? "An API error occurred.";

        if (!string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    message = msg.GetString() ?? message;
                if (doc.RootElement.TryGetProperty("error", out var err))
                    errorCode = err.GetString();
                else if (doc.RootElement.TryGetProperty("code", out var code))
                    errorCode = code.GetString();
            }
            catch (JsonException) { /* use defaults */ }
        }

        throw new MainlayerException(message, response.StatusCode, errorCode, raw);
    }
}
