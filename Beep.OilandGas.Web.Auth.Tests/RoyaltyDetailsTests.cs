using System.Net;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class RoyaltyDetailsTests
{
    private static AccountingServiceClient Client(HttpClient http) => new(new ApiClient(http, NullLogger<ApiClient>.Instance), NullLogger<AccountingServiceClient>.Instance);
    private static HttpResponseMessage Json(object? data) => new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(data)) };
    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => send(request, token);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "session has expired")]
    [InlineData(HttpStatusCode.Forbidden, "do not have access")]
    [InlineData(HttpStatusCode.NotFound, "could not be found")]
    [InlineData(HttpStatusCode.InternalServerError, "Please retry")]
    public async Task ApiStatusReachesPageWithoutBecomingEmptyRecords(HttpStatusCode code, string message)
    {
        using var http = new HttpClient(new Handler((_, _) => Task.FromResult(new HttpResponseMessage(code) { Content = new StringContent("private server details") }))) { BaseAddress = new("https://api.example") };
        using var state = new RoyaltyDetailsState(Client(http));
        await state.LoadAsync("one");
        Assert.Contains(message, state.Error);
        Assert.DoesNotContain("private server", state.Error);
        Assert.Null(state.Calculation); Assert.Null(state.Payments); Assert.False(state.Loading);
    }

    [Theory]
    [InlineData("details")]
    [InlineData("payments")]
    [InlineData("review")]
    public async Task NullPayloadIsNotASuccessfulEmptyResource(string resource)
    {
        string? path = null;
        using var http = new HttpClient(new Handler((request, _) => { path = request.RequestUri!.AbsolutePath; return Task.FromResult(Json(null)); })) { BaseAddress = new("https://api.example") };
        var client = Client(http);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => {
            if (resource == "details") await client.GetRoyaltyAsync("id with space");
            else if (resource == "payments") await client.GetRoyaltyPaymentsAsync("id with space");
            else await client.GetRoyaltyPostingReviewAsync("id with space");
        });
        Assert.Contains("id%20with%20space", path);
    }

    [Fact]
    public async Task FailedPaymentAndDeniedReviewKeepTheObligationVisible()
    {
        using var http = new HttpClient(new Handler((request, _) => Task.FromResult(
            request.RequestUri!.AbsolutePath.Contains("service/calculations") ? Json(new ROYALTY_CALCULATION { ROYALTY_CALCULATION_ID = "one", ROYALTY_AMOUNT = 50 }) :
            new HttpResponseMessage(request.RequestUri.AbsolutePath.EndsWith("payments") ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.Forbidden) { Content = new StringContent("") }))) { BaseAddress = new("https://api.example") };
        using var state = new RoyaltyDetailsState(Client(http));
        await state.LoadAsync("one");
        Assert.NotNull(state.Calculation); Assert.Null(state.Error); Assert.NotNull(state.PaymentError); Assert.Null(state.Payments);
        await state.LoadReviewAsync();
        Assert.Contains("do not have access", state.ReviewError);
        Assert.NotNull(state.Calculation); Assert.Null(state.Review); Assert.False(state.Reviewing);
    }

    [Fact]
    public async Task LateResponseCannotReplaceAnotherRoyalty()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var http = new HttpClient(new Handler((request, _) => {
            if (request.RequestUri!.AbsolutePath.EndsWith("/old")) { entered.TrySetResult(); return release.Task; }
            return Task.FromResult(request.RequestUri.AbsolutePath.EndsWith("payments") ? Json(Array.Empty<ROYALTY_PAYMENT>()) : Json(new ROYALTY_CALCULATION { ROYALTY_CALCULATION_ID = "new" }));
        })) { BaseAddress = new("https://api.example") };
        using var state = new RoyaltyDetailsState(Client(http));
        var old = state.LoadAsync("old");
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await state.LoadAsync("new");
        release.SetResult(Json(new ROYALTY_CALCULATION { ROYALTY_CALCULATION_ID = "old" }));
        await old;
        Assert.Equal("new", state.Calculation!.ROYALTY_CALCULATION_ID);
        Assert.Empty(state.Payments!); Assert.Null(state.Error);
    }

    [Fact]
    public async Task ReviewIsExplicitAndDisposeClearsVisibleData()
    {
        var reviews = 0;
        using var http = new HttpClient(new Handler((request, _) => {
            var path = request.RequestUri!.AbsolutePath;
            if (path.EndsWith("posting-review")) { reviews++; return Task.FromResult(Json(Array.Empty<RoyaltyPostingReview>())); }
            return Task.FromResult(path.EndsWith("payments") ? Json(Array.Empty<ROYALTY_PAYMENT>()) : Json(new ROYALTY_CALCULATION { ROYALTY_CALCULATION_ID = "one" }));
        })) { BaseAddress = new("https://api.example") };
        var state = new RoyaltyDetailsState(Client(http));
        await state.LoadAsync("one"); Assert.Equal(0, reviews);
        await state.LoadReviewAsync(); Assert.Equal(1, reviews); Assert.Empty(state.Review!);
        state.Dispose(); Assert.Null(state.Calculation); Assert.Null(state.Payments); Assert.Null(state.Review);
        await state.LoadReviewAsync(); Assert.Equal(1, reviews);
    }

    [Fact]
    public async Task PaymentHistoryRoundTripsCanonicalStatusThroughWebJson()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var payload = JsonSerializer.Serialize(new[] { new ROYALTY_PAYMENT { ROYALTY_PAYMENT_ID = "payment", STATUS = "PENDING", NET_PAYMENT_AMOUNT = 42m } }, options);
        using var http = new HttpClient(new Handler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(payload) }))) { BaseAddress = new("https://api.example") };
        var payment = Assert.Single(await Client(http).GetRoyaltyPaymentsAsync("one"));
        Assert.Equal("PENDING", payment.STATUS);
        Assert.Equal(42m, payment.NET_PAYMENT_AMOUNT);
    }

    [Fact]
    public void WebsiteRoyaltyRoutesRequireAuthentication()
    {
        Assert.NotEmpty(typeof(Beep.OilandGas.Web.Pages.PPDM39.Accounting.RoyaltyDetails).GetCustomAttributes(typeof(AuthorizeAttribute), true));
        Assert.NotEmpty(typeof(Beep.OilandGas.Web.Pages.PPDM39.Accounting.Royalties).GetCustomAttributes(typeof(AuthorizeAttribute), true));
    }
}
