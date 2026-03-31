using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mainlayer;
using Mainlayer.Models;
using Xunit;

namespace Mainlayer.Tests;

/// <summary>
/// xUnit test suite for <see cref="MainlayerClient"/> using a stub
/// <see cref="HttpMessageHandler"/> — no live network calls are made.
/// </summary>
public sealed class MainlayerClientTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static MainlayerClient BuildClient(
        Func<HttpRequestMessage, HttpResponseMessage> handler,
        string apiKey = "ml_test_key")
    {
        var stub = new StubHandler(handler);
        var httpClient = new HttpClient(stub);
        var options = new MainlayerClientOptions
        {
            ApiKey = apiKey,
            MaxRetries = 0  // disable retries so tests run fast
        };
        return new MainlayerClient(httpClient, options);
    }

    private static HttpResponseMessage Json(object payload, HttpStatusCode status = HttpStatusCode.OK)
    {
        var body = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }

    private static HttpResponseMessage ErrorJson(
        string message,
        string? errorCode,
        HttpStatusCode status)
    {
        var payload = errorCode is null
            ? new { message }
            : new { message, error = errorCode };
        return Json(payload, status);
    }

    // -------------------------------------------------------------------------
    // Constructor validation
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_NullApiKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MainlayerClient(string.Empty));
    }

    [Fact]
    public void Constructor_WhitespaceApiKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MainlayerClient("   "));
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MainlayerClient(new HttpClient(), null!));
    }

    [Fact]
    public void Constructor_ValidApiKey_DoesNotThrow()
    {
        // Should not throw
        var client = BuildClient(_ => Json(new { }));
        Assert.NotNull(client);
    }

    // -------------------------------------------------------------------------
    // Resources
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Resources_ListAsync_ReturnsResources()
    {
        var now = DateTimeOffset.UtcNow;
        var expected = new[]
        {
            new { id = "res_1", slug = "weather-api", name = "Weather API",
                  description = "Live data", type = "api", fee_model = "pay_per_call",
                  price_usdc = 0.01m, created_at = now, updated_at = now },
            new { id = "res_2", slug = "stock-data", name = "Stock Data",
                  description = "EOD prices", type = "file", fee_model = "one_time",
                  price_usdc = 5.00m, created_at = now, updated_at = now }
        };

        var client = BuildClient(_ => Json(expected));
        var result = await client.Resources.ListAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("res_1", result[0].Id);
        Assert.Equal("res_2", result[1].Id);
    }

    [Fact]
    public async Task Resources_GetAsync_ReturnsResource()
    {
        var now = DateTimeOffset.UtcNow;
        var expected = new
        {
            id = "res_abc", slug = "my-api", name = "My API",
            description = "Desc", type = "api", fee_model = "pay_per_call",
            price_usdc = 0.05m, created_at = now, updated_at = now
        };

        HttpRequestMessage? captured = null;
        var client = BuildClient(req =>
        {
            captured = req;
            return Json(expected);
        });

        var resource = await client.Resources.GetAsync("res_abc");

        Assert.Equal("res_abc", resource.Id);
        Assert.Equal("my-api", resource.Slug);
        Assert.EndsWith("/resources/res_abc", captured!.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task Resources_CreateAsync_PostsAndReturnsNewResource()
    {
        var now = DateTimeOffset.UtcNow;
        HttpMethod? capturedMethod = null;
        var client = BuildClient(req =>
        {
            capturedMethod = req.Method;
            return Json(new
            {
                id = "res_new", slug = "new-api", type = "api",
                fee_model = "pay_per_call", price_usdc = 0.02m,
                created_at = now, updated_at = now
            });
        });

        var resource = await client.Resources.CreateAsync(new CreateResourceRequest
        {
            Slug = "new-api",
            Type = ResourceType.Api,
            FeeModel = FeeModel.PayPerCall,
            PriceUsdc = 0.02m
        });

        Assert.Equal(HttpMethod.Post, capturedMethod);
        Assert.Equal("res_new", resource.Id);
        Assert.Equal("new-api", resource.Slug);
    }

    [Fact]
    public async Task Resources_UpdateAsync_SendsPatchAndReturnsUpdated()
    {
        var now = DateTimeOffset.UtcNow;
        HttpMethod? capturedMethod = null;
        string? capturedPath = null;
        var client = BuildClient(req =>
        {
            capturedMethod = req.Method;
            capturedPath = req.RequestUri!.PathAndQuery;
            return Json(new
            {
                id = "res_abc", slug = "my-api", name = "Updated Name",
                type = "api", fee_model = "pay_per_call", price_usdc = 0.02m,
                created_at = now, updated_at = now
            });
        });

        var resource = await client.Resources.UpdateAsync("res_abc",
            new UpdateResourceRequest { Name = "Updated Name" });

        Assert.Equal(new HttpMethod("PATCH"), capturedMethod);
        Assert.Contains("res_abc", capturedPath);
        Assert.Equal("Updated Name", resource.Name);
    }

    [Fact]
    public async Task Resources_DeleteAsync_SendsDeleteRequest()
    {
        HttpMethod? capturedMethod = null;
        var client = BuildClient(req =>
        {
            capturedMethod = req.Method;
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        await client.Resources.DeleteAsync("res_abc");

        Assert.Equal(HttpMethod.Delete, capturedMethod);
    }

    [Fact]
    public async Task Resources_GetAsync_NotFound_ThrowsMainlayerException()
    {
        var client = BuildClient(_ =>
            ErrorJson("Resource not found", "resource_not_found", HttpStatusCode.NotFound));

        var ex = await Assert.ThrowsAsync<MainlayerException>(
            () => client.Resources.GetAsync("res_missing"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Equal("resource_not_found", ex.ErrorCode);
    }

    // -------------------------------------------------------------------------
    // Payments
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Payments_CreateAsync_ReturnsPayment()
    {
        var now = DateTimeOffset.UtcNow;
        HttpMethod? capturedMethod = null;
        var client = BuildClient(req =>
        {
            capturedMethod = req.Method;
            return Json(new
            {
                id = "pay_1", resource_id = "res_abc",
                payer_wallet = "wallet_xyz", amount_usdc = 0.05m,
                status = "confirmed", created_at = now
            });
        });

        var payment = await client.Payments.CreateAsync(new CreatePaymentRequest
        {
            ResourceId = "res_abc",
            PayerWallet = "wallet_xyz"
        });

        Assert.Equal(HttpMethod.Post, capturedMethod);
        Assert.Equal("pay_1", payment.Id);
        Assert.Equal("confirmed", payment.Status);
        Assert.Equal(0.05m, payment.AmountUsdc);
    }

    [Fact]
    public async Task Payments_ListAsync_ReturnsPaymentHistory()
    {
        var now = DateTimeOffset.UtcNow;
        var client = BuildClient(_ => Json(new[]
        {
            new { id = "pay_1", resource_id = "res_1", payer_wallet = "w1",
                  amount_usdc = 0.01m, status = "confirmed", created_at = now },
            new { id = "pay_2", resource_id = "res_2", payer_wallet = "w2",
                  amount_usdc = 5.00m, status = "confirmed", created_at = now }
        }));

        var payments = await client.Payments.ListAsync();

        Assert.Equal(2, payments.Count);
        Assert.Equal("pay_1", payments[0].Id);
        Assert.Equal("pay_2", payments[1].Id);
    }

    [Fact]
    public async Task Payments_CreateAsync_InsufficientFunds_ThrowsMainlayerException()
    {
        var client = BuildClient(_ =>
            ErrorJson("Insufficient funds", "insufficient_funds", HttpStatusCode.PaymentRequired));

        var ex = await Assert.ThrowsAsync<MainlayerException>(
            () => client.Payments.CreateAsync(new CreatePaymentRequest
            {
                ResourceId = "res_abc",
                PayerWallet = "wallet_broke"
            }));

        Assert.Equal(HttpStatusCode.PaymentRequired, ex.StatusCode);
        Assert.Equal("insufficient_funds", ex.ErrorCode);
    }

    // -------------------------------------------------------------------------
    // Entitlements
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Entitlements_CheckAsync_ReturnsHasAccessTrue()
    {
        string? capturedQuery = null;
        var client = BuildClient(req =>
        {
            capturedQuery = req.RequestUri!.Query;
            return Json(new
            {
                has_access = true, resource_id = "res_abc",
                payer_wallet = "wallet_xyz", expires_at = (DateTimeOffset?)null
            });
        });

        var entitlement = await client.Entitlements.CheckAsync("res_abc", "wallet_xyz");

        Assert.True(entitlement.HasAccess);
        Assert.Equal("res_abc", entitlement.ResourceId);
        Assert.Contains("resource_id=res_abc", capturedQuery);
        Assert.Contains("payer_wallet=wallet_xyz", capturedQuery);
    }

    [Fact]
    public async Task Entitlements_CheckAsync_ReturnsHasAccessFalse_WhenNotPaid()
    {
        var client = BuildClient(_ => Json(new
        {
            has_access = false, resource_id = "res_abc",
            payer_wallet = "wallet_nopay", expires_at = (DateTimeOffset?)null
        }));

        var entitlement = await client.Entitlements.CheckAsync("res_abc", "wallet_nopay");

        Assert.False(entitlement.HasAccess);
    }

    [Fact]
    public async Task Entitlements_CheckAsync_PopulatesExpiresAt_ForSubscriptions()
    {
        var expiry = DateTimeOffset.UtcNow.AddDays(30);
        var client = BuildClient(_ => Json(new
        {
            has_access = true, resource_id = "res_sub",
            payer_wallet = "wallet_xyz", expires_at = expiry
        }));

        var entitlement = await client.Entitlements.CheckAsync("res_sub", "wallet_xyz");

        Assert.True(entitlement.HasAccess);
        Assert.NotNull(entitlement.ExpiresAt);
    }

    // -------------------------------------------------------------------------
    // Discover
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Discover_SearchAsync_ReturnsResults()
    {
        var now = DateTimeOffset.UtcNow;
        string? capturedQuery = null;
        var client = BuildClient(req =>
        {
            capturedQuery = req.RequestUri!.Query;
            return Json(new
            {
                results = new[]
                {
                    new { id = "res_1", slug = "weather", type = "api",
                          fee_model = "pay_per_call", price_usdc = 0.01m,
                          created_at = now, updated_at = now }
                },
                total = 1
            });
        });

        var result = await client.Discover.SearchAsync(new SearchRequest
        {
            Query = "weather",
            Limit = 10
        });

        Assert.Single(result.Results);
        Assert.Equal(1, result.Total);
        Assert.Contains("weather", capturedQuery);
        Assert.Contains("limit=10", capturedQuery);
    }

    [Fact]
    public async Task Discover_SearchAsync_EmptyQuery_SendsNoQueryParam()
    {
        string? capturedQuery = null;
        var client = BuildClient(req =>
        {
            capturedQuery = req.RequestUri!.Query;
            return Json(new { results = Array.Empty<object>(), total = 0 });
        });

        await client.Discover.SearchAsync(new SearchRequest());

        // No query string or empty
        Assert.True(string.IsNullOrEmpty(capturedQuery) || !capturedQuery.Contains("q="));
    }

    // -------------------------------------------------------------------------
    // Analytics
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Analytics_GetAsync_ReturnsAnalytics()
    {
        var client = BuildClient(_ => Json(new
        {
            total_payments = 42L,
            total_revenue_usdc = 12.34m,
            unique_payers = 7L,
            by_resource = new[]
            {
                new { resource_id = "res_1", payment_count = 30L, revenue_usdc = 10.00m },
                new { resource_id = "res_2", payment_count = 12L, revenue_usdc = 2.34m }
            }
        }));

        var analytics = await client.Analytics.GetAsync();

        Assert.Equal(42L, analytics.TotalPayments);
        Assert.Equal(12.34m, analytics.TotalRevenueUsdc);
        Assert.Equal(7L, analytics.UniquePayers);
        Assert.Equal(2, analytics.ByResource.Count);
        Assert.Equal("res_1", analytics.ByResource[0].ResourceId);
    }

    // -------------------------------------------------------------------------
    // Webhooks
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Webhooks_CreateAsync_ReturnsWebhook()
    {
        var now = DateTimeOffset.UtcNow;
        HttpMethod? capturedMethod = null;
        var client = BuildClient(req =>
        {
            capturedMethod = req.Method;
            return Json(new
            {
                id = "wh_1",
                url = "https://example.com/hook",
                events = new[] { "payment.created" },
                created_at = now
            });
        });

        var webhook = await client.Webhooks.CreateAsync(new CreateWebhookRequest
        {
            Url = "https://example.com/hook",
            Events = new List<string> { "payment.created" }
        });

        Assert.Equal(HttpMethod.Post, capturedMethod);
        Assert.Equal("wh_1", webhook.Id);
        Assert.Equal("https://example.com/hook", webhook.Url);
        Assert.Single(webhook.Events);
    }

    [Fact]
    public async Task Webhooks_ListAsync_ReturnsWebhooks()
    {
        var now = DateTimeOffset.UtcNow;
        var client = BuildClient(_ => Json(new[]
        {
            new { id = "wh_1", url = "https://a.com/hook",
                  events = new[] { "payment.created" }, created_at = now },
            new { id = "wh_2", url = "https://b.com/hook",
                  events = new[] { "payment.created", "payment.failed" }, created_at = now }
        }));

        var webhooks = await client.Webhooks.ListAsync();

        Assert.Equal(2, webhooks.Count);
    }

    [Fact]
    public async Task Webhooks_DeleteAsync_SendsDeleteRequest()
    {
        HttpMethod? capturedMethod = null;
        string? capturedPath = null;
        var client = BuildClient(req =>
        {
            capturedMethod = req.Method;
            capturedPath = req.RequestUri!.PathAndQuery;
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        await client.Webhooks.DeleteAsync("wh_1");

        Assert.Equal(HttpMethod.Delete, capturedMethod);
        Assert.Contains("wh_1", capturedPath);
    }

    // -------------------------------------------------------------------------
    // API Keys
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ApiKeys_CreateAsync_ReturnsApiKeyWithRawValue()
    {
        var now = DateTimeOffset.UtcNow;
        var client = BuildClient(_ => Json(new
        {
            id = "key_1",
            name = "Production",
            key = "ml_live_abcdef123456",
            created_at = now
        }));

        var apiKey = await client.ApiKeys.CreateAsync(new CreateApiKeyRequest
        {
            Name = "Production"
        });

        Assert.Equal("key_1", apiKey.Id);
        Assert.Equal("Production", apiKey.Name);
        Assert.Equal("ml_live_abcdef123456", apiKey.Key);
    }

    // -------------------------------------------------------------------------
    // Error handling
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Request_Unauthorized_ThrowsMainlayerException()
    {
        var client = BuildClient(_ =>
            ErrorJson("Invalid API key", "unauthorized", HttpStatusCode.Unauthorized));

        var ex = await Assert.ThrowsAsync<MainlayerException>(
            () => client.Resources.ListAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal("unauthorized", ex.ErrorCode);
        Assert.Contains("Invalid API key", ex.Message);
    }

    [Fact]
    public async Task Request_ServerError_ThrowsMainlayerException()
    {
        var client = BuildClient(_ =>
            ErrorJson("Internal server error", "internal_error",
                HttpStatusCode.InternalServerError));

        var ex = await Assert.ThrowsAsync<MainlayerException>(
            () => client.Analytics.GetAsync());

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public async Task Request_NonJsonErrorBody_StillThrowsMainlayerException()
    {
        var client = BuildClient(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("Service Unavailable", Encoding.UTF8, "text/plain")
        });

        var ex = await Assert.ThrowsAsync<MainlayerException>(
            () => client.Resources.ListAsync());

        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
    }

    [Fact]
    public void MainlayerException_ToString_ContainsStatusAndMessage()
    {
        var ex = new MainlayerException("Resource not found", HttpStatusCode.NotFound, "not_found");
        var str = ex.ToString();

        Assert.Contains("404", str);
        Assert.Contains("Resource not found", str);
        Assert.Contains("not_found", str);
    }

    [Fact]
    public async Task Request_BearerTokenSentInAuthorizationHeader()
    {
        string? authHeader = null;
        var client = BuildClient(req =>
        {
            authHeader = req.Headers.Authorization?.ToString();
            return Json(Array.Empty<object>());
        }, apiKey: "ml_my_secret_key");

        await client.Resources.ListAsync();

        Assert.Equal("Bearer ml_my_secret_key", authHeader);
    }

    [Fact]
    public async Task Request_UserAgentHeaderIsSet()
    {
        string? userAgent = null;
        var client = BuildClient(req =>
        {
            userAgent = req.Headers.UserAgent.ToString();
            return Json(Array.Empty<object>());
        });

        await client.Resources.ListAsync();

        Assert.StartsWith("mainlayer-dotnet/", userAgent);
    }

    // -------------------------------------------------------------------------
    // Discover
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Discover_SearchAsync_ReturnsResources()
    {
        var now = DateTimeOffset.UtcNow;
        var client = BuildClient(_ => Json(new[]
        {
            new { id = "res_public_001", slug = "weather-api", name = "Weather API", description = "Real-time weather", type = "api", fee_model = "pay_per_call", price_usdc = 0.01m, created_at = now, updated_at = now }
        }));

        var results = await client.Discover.SearchAsync("weather");

        Assert.Single(results);
        Assert.Equal("res_public_001", results[0].Id);
        Assert.Equal("weather-api", results[0].Slug);
    }

    [Fact]
    public async Task Discover_QueryAsync_WithFilters_ReturnsFiltered()
    {
        var now = DateTimeOffset.UtcNow;
        var client = BuildClient(_ => Json(Array.Empty<object>()));

        var results = await client.Discover.QueryAsync(new DiscoverQuery
        {
            Query = "api",
            ResourceType = ResourceType.Api,
            FeeModel = FeeModel.PayPerCall,
            Limit = 10,
            Offset = 0
        });

        Assert.Empty(results);
    }

    // -------------------------------------------------------------------------
    // Entitlements
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Entitlements_CheckAsync_HasAccess_ReturnsTrue()
    {
        var client = BuildClient(_ => Json(new { has_access = true, expires_at = (DateTimeOffset?)null, credits_remaining = (int?)null }));

        var entitlement = await client.Entitlements.CheckAsync("res_abc123", "wallet_xyz");

        Assert.True(entitlement.HasAccess);
        Assert.Null(entitlement.ExpiresAt);
        Assert.Null(entitlement.CreditsRemaining);
    }

    [Fact]
    public async Task Entitlements_CheckAsync_NoAccess_ReturnsFalse()
    {
        var client = BuildClient(_ => Json(new { has_access = false, expires_at = (DateTimeOffset?)null, credits_remaining = (int?)null }));

        var entitlement = await client.Entitlements.CheckAsync("res_abc123", "wallet_xyz");

        Assert.False(entitlement.HasAccess);
    }

    [Fact]
    public async Task Entitlements_CheckAsync_WithExpiration_ReturnsExpiresAt()
    {
        var expiration = DateTimeOffset.UtcNow.AddDays(30);
        var client = BuildClient(_ => Json(new { has_access = true, expires_at = expiration, credits_remaining = (int?)null }));

        var entitlement = await client.Entitlements.CheckAsync("res_abc123", "wallet_xyz");

        Assert.True(entitlement.HasAccess);
        Assert.NotNull(entitlement.ExpiresAt);
        Assert.Equal(expiration.Date, entitlement.ExpiresAt?.Date);
    }

    [Fact]
    public async Task Entitlements_CheckAsync_PayPerCall_ReturnsCredits()
    {
        var client = BuildClient(_ => Json(new { has_access = true, expires_at = (DateTimeOffset?)null, credits_remaining = 500 }));

        var entitlement = await client.Entitlements.CheckAsync("res_abc123", "wallet_xyz");

        Assert.True(entitlement.HasAccess);
        Assert.Equal(500, entitlement.CreditsRemaining);
    }

    // -------------------------------------------------------------------------
    // Payments
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Payments_CreateAsync_ReturnsPayment()
    {
        var now = DateTimeOffset.UtcNow;
        var client = BuildClient(_ => Json(new { id = "pay_001", resource_id = "res_abc123", payer_wallet = "wallet_xyz", amount_usdc = 0.01m, status = "confirmed", created_at = now, updated_at = now }));

        var payment = await client.Payments.CreateAsync(new CreatePaymentRequest
        {
            ResourceId = "res_abc123",
            PayerWallet = "wallet_xyz"
        });

        Assert.Equal("pay_001", payment.Id);
        Assert.Equal("res_abc123", payment.ResourceId);
        Assert.Equal(0.01m, payment.AmountUsdc);
    }

    [Fact]
    public async Task Payments_ListAsync_ReturnsPayments()
    {
        var now = DateTimeOffset.UtcNow;
        var client = BuildClient(_ => Json(new[]
        {
            new { id = "pay_001", resource_id = "res_abc123", payer_wallet = "wallet_xyz", amount_usdc = 0.01m, status = "confirmed", created_at = now, updated_at = now }
        }));

        var payments = await client.Payments.ListAsync();

        Assert.Single(payments);
        Assert.Equal("pay_001", payments[0].Id);
    }

    // -------------------------------------------------------------------------
    // Analytics
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Analytics_GetAsync_ReturnsStats()
    {
        var client = BuildClient(_ => Json(new { total_payments = 42, total_revenue_usdc = 42.00m, unique_payers = 10, by_resource = Array.Empty<object>() }));

        var stats = await client.Analytics.GetAsync();

        Assert.Equal(42, stats.TotalPayments);
        Assert.Equal(42.00m, stats.TotalRevenueUsdc);
        Assert.Equal(10, stats.UniquePayers);
    }

    // -------------------------------------------------------------------------
    // API Keys
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ApiKeys_ListAsync_ReturnsKeys()
    {
        var now = DateTimeOffset.UtcNow;
        var client = BuildClient(_ => Json(new[]
        {
            new { id = "key_001", name = "Production", key = (string?)null, created_at = now }
        }));

        var keys = await client.ApiKeys.ListAsync();

        Assert.Single(keys);
        Assert.Equal("key_001", keys[0].Id);
        Assert.Equal("Production", keys[0].Name);
    }

    [Fact]
    public async Task ApiKeys_CreateAsync_ReturnsNewKey()
    {
        var now = DateTimeOffset.UtcNow;
        var client = BuildClient(_ => Json(new { id = "key_new", name = "Test Key", key = "ml_secret_key_123", created_at = now }));

        var newKey = await client.ApiKeys.CreateAsync(new CreateApiKeyRequest { Name = "Test Key" });

        Assert.Equal("key_new", newKey.Id);
        Assert.Equal("ml_secret_key_123", newKey.Key);
    }

    // -------------------------------------------------------------------------
    // Stub handler
    // -------------------------------------------------------------------------

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(_handler(request));
    }
}
