using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Net;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.App.Services.DataManagement;
using Beep.OilandGas.Client.Authentication;
using Beep.OilandGas.Client.Exceptions;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Moq;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class PpdmDataClientTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static readonly Dictionary<string, object> Entity = new() { ["WELL_ID"] = "well/one", ["WELL_NAME"] = "A" };
    private static HttpResponseMessage Json(object value, HttpStatusCode code = HttpStatusCode.OK) => new(code)
    { Content = new StringContent(JsonSerializer.Serialize(value, JsonOptions), Encoding.UTF8, "application/json") };

    [Fact]
    public async Task Crud_uses_the_actual_API_routes_envelopes_raw_entity_and_selected_company()
    {
        var calls = new List<(HttpMethod Method, string Path, string? Body)>();
        using var f = new Fixture(async (request, _) =>
        {
            calls.Add((request.Method, request.RequestUri!.PathAndQuery, request.Content is null ? null : await request.Content.ReadAsStringAsync()));
            Assert.Equal("tenant", request.Headers.GetValues(CompanyRequestHeaders.Tenant).Single());
            Assert.Equal("company", request.Headers.GetValues(CompanyRequestHeaders.Company).Single());
            if (request.Method == HttpMethod.Post && !request.RequestUri.AbsolutePath.EndsWith("insert"))
                return Json(new GetEntitiesResponse { Success = true, Count = 1, Entities = [Entity] });
            return Json(new GenericEntityResponse { Success = true, Message = "persisted", EntityData = Entity });
        });
        var list = await f.Data.GetEntitiesAsync("WELL", new() { Filters = [new AppFilter { FieldName = "WELL_ID", FilterValue = "well/one", Operator = "=" }] });
        Assert.True(list.Success); Assert.Equal(1, list.Count); Assert.Single(list.Entities);
        Assert.True((await f.Data.GetEntityAsync("WELL", "well/one")).Success);
        Assert.Equal("persisted", (await f.Data.InsertEntityAsync("WELL", Entity)).Message);
        Assert.True((await f.Data.UpdateEntityAsync("WELL", "well/one", Entity)).Success);
        Assert.True((await f.Data.DeleteEntityAsync("WELL", "well/one")).Success);
        Assert.Equal(new[] { HttpMethod.Post, HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete }, calls.Select(call => call.Method));
        Assert.Equal(new[] { "/api/ppdm39/data/WELL", "/api/ppdm39/data/WELL/well%2Fone", "/api/ppdm39/data/WELL/insert", "/api/ppdm39/data/WELL/well%2Fone", "/api/ppdm39/data/WELL/well%2Fone" }, calls.Select(call => call.Path));
        using var query = JsonDocument.Parse(calls[0].Body!); Assert.Equal("well/one", query.RootElement.GetProperty("filters")[0].GetProperty("filterValue").GetString());
        Assert.Equal(JsonValueKind.Null, query.RootElement.GetProperty("connectionName").ValueKind);
        foreach (var call in calls.Where(call => call.Method == HttpMethod.Put || call.Path.EndsWith("insert")))
        {
            using var body = JsonDocument.Parse(call.Body!);
            Assert.Equal("well/one", body.RootElement.GetProperty("WELL_ID").GetString());
            Assert.False(body.RootElement.TryGetProperty("entityData", out _));
            Assert.False(body.RootElement.TryGetProperty("connectionName", out _));
        }
    }

    [Theory]
    [InlineData("list")]
    [InlineData("insert")]
    [InlineData("delete")]
    public async Task A_failed_envelope_is_returned_as_failed_without_fabricated_entities_or_success(string operation)
    {
        using var f = new Fixture((_, _) => Task.FromResult(Json(operation == "list" ?
            (object)new GetEntitiesResponse { Success = false, ErrorMessage = "blocked" } :
            new GenericEntityResponse { Success = false, ErrorMessage = "blocked" })));
        if (operation == "list") { var result = await f.Data.GetEntitiesAsync("WELL", new()); Assert.False(result.Success); Assert.Empty(result.Entities); Assert.Equal("blocked", result.ErrorMessage); }
        else { var result = operation == "insert" ? await f.Data.InsertEntityAsync("WELL", Entity) : await f.Data.DeleteEntityAsync("WELL", "1"); Assert.False(result.Success); Assert.Null(result.EntityData); Assert.Equal("blocked", result.ErrorMessage); }
        Assert.Equal(1, f.Calls);
    }

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(409)]
    [InlineData(500)]
    public async Task Non_success_HTTP_keeps_the_API_status_and_response_and_is_not_retried(int status)
    {
        var response = "{\"message\":\"refused\",\"reference\":\"server-reference\"}";
        using var f = new Fixture((_, _) => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent(response) }));
        var error = await Assert.ThrowsAsync<ApiException>(() => f.Data.GetEntityAsync("WELL", "1"));
        Assert.Equal((HttpStatusCode)status, error.StatusCode); Assert.Equal(response, error.ResponseContent); Assert.Equal("refused", error.Message); Assert.Equal(1, f.Calls);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("[")]
    public async Task An_empty_null_or_malformed_success_response_is_an_error(string response)
    {
        using var f = new Fixture((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response) }));
        await Assert.ThrowsAsync<ApiClientException>(() => f.Data.GetEntityAsync("WELL", "1")); Assert.Equal(1, f.Calls);
    }

    [Fact]
    public async Task Missing_selection_or_cancellation_never_calls_authentication_or_HTTP()
    {
        var auth = new Mock<IAuthenticationProvider>(MockBehavior.Strict);
        using var f = new Fixture((_, _) => throw new InvalidOperationException("must not send"), auth.Object);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => f.Data.GetEntityAsync("WELL", "1", new CancellationToken(true)));
        f.Company.Clear(); await Assert.ThrowsAsync<InvalidOperationException>(() => f.Data.GetEntityAsync("WELL", "1"));
        Assert.Equal(0, f.Calls); auth.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task A_company_switch_during_token_lookup_cannot_adopt_the_new_company()
    {
        var waiting = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var auth = new Mock<IAuthenticationProvider>(); auth.Setup(provider => provider.GetAccessTokenAsync()).Returns(waiting.Task);
        using var f = new Fixture((_, _) => throw new InvalidOperationException("must not send"), auth.Object);
        var request = f.Data.InsertEntityAsync("WELL", Entity); Assert.False(request.IsCompleted);
        f.Select("other-company"); waiting.SetResult("token");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request); Assert.Equal(0, f.Calls);
    }

    [Fact]
    public async Task A_delayed_response_cannot_populate_a_new_company_even_if_the_handler_ignores_cancellation()
    {
        var waiting = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var f = new Fixture((_, _) => waiting.Task);
        var request = f.Data.GetEntitiesAsync("WELL", new()); Assert.False(request.IsCompleted);
        f.Select("other-company"); waiting.SetResult(Json(new GetEntitiesResponse { Success = true, Entities = [Entity] }));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => request); Assert.Equal(1, f.Calls);
    }

    [Theory]
    [InlineData("insert", false)]
    [InlineData("update", false)]
    [InlineData("delete", false)]
    [InlineData("insert", true)]
    [InlineData("update", true)]
    [InlineData("delete", true)]
    public async Task Uncertain_mutations_are_sent_once_for_connection_failures_and_timeouts(string operation, bool timeout)
    {
        using var f = new Fixture((_, _) => timeout ? throw new TaskCanceledException("timeout", new TimeoutException()) : throw new HttpRequestException("connection lost"));
        Task<GenericEntityResponse> Request() => operation switch { "insert" => f.Data.InsertEntityAsync("WELL", Entity), "update" => f.Data.UpdateEntityAsync("WELL", "1", Entity), "delete" => f.Data.DeleteEntityAsync("WELL", "1"), _ => throw new ArgumentException() };
        await Assert.ThrowsAnyAsync<ApiClientException>(Request); Assert.Equal(1, f.Calls);
    }

    [Fact]
    public async Task A_transient_GET_retries_with_new_messages_and_the_original_scope()
    {
        var messages = new List<HttpRequestMessage>();
        using var f = new Fixture((request, _) =>
        {
            messages.Add(request); Assert.Equal("company", request.Headers.GetValues(CompanyRequestHeaders.Company).Single());
            if (messages.Count == 1) throw new HttpRequestException("lost GET");
            return Task.FromResult(Json(new GenericEntityResponse { Success = true, EntityData = Entity }));
        });
        Assert.True((await f.Data.GetEntityAsync("WELL", "1")).Success); Assert.Equal(2, f.Calls); Assert.NotSame(messages[0], messages[1]);
    }

    [Fact]
    public async Task A_failed_GET_cannot_retry_after_a_company_switch()
    {
        Fixture? f = null;
        using (f = new((_, _) => { f!.Select("other-company"); throw new HttpRequestException("lost GET"); }))
        { await Assert.ThrowsAnyAsync<OperationCanceledException>(() => f.Data.GetEntityAsync("WELL", "1")); Assert.Equal(1, f.Calls); }
    }

    [Fact]
    public async Task Token_failures_are_explicit_and_cannot_reach_HTTP()
    {
        var auth = new Mock<IAuthenticationProvider>(); auth.Setup(provider => provider.GetAccessTokenAsync()).ThrowsAsync(new InvalidOperationException("token unavailable"));
        using var f = new Fixture((_, _) => throw new InvalidOperationException("must not send"), auth.Object);
        await Assert.ThrowsAsync<ApiAuthenticationException>(() => f.Data.GetEntityAsync("WELL", "1")); Assert.Equal(0, f.Calls);
    }

    [Fact]
    public async Task Existing_absolute_API_addresses_are_used_once_and_foreign_origins_are_refused()
    {
        using var f = new Fixture((request, _) => { Assert.Equal("https://api.test/api/ppdm39/data/WELL/1", request.RequestUri!.AbsoluteUri); return Task.FromResult(Json(new GenericEntityResponse { Success = true })); });
        Assert.True((await f.Transport.GetAsync<GenericEntityResponse>("https://api.test/api/ppdm39/data/WELL/1")).Success);
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Transport.GetAsync<GenericEntityResponse>("https://foreign.test/api/ppdm39/data/WELL/1")); Assert.Equal(1, f.Calls);
    }

    [Fact]
    public void Shared_CRUD_contracts_and_their_notifying_base_use_the_existing_model_owner()
    {
        var owner = typeof(ModelEntityBase).Assembly;
        Assert.Equal(owner, typeof(GetEntitiesRequest).Assembly); Assert.Equal(owner, typeof(GetEntitiesResponse).Assembly);
        Assert.Equal(owner, typeof(GenericEntityRequest).Assembly); Assert.Equal(owner, typeof(GenericEntityResponse).Assembly); Assert.Equal(owner, typeof(ModelEntityBase).Assembly);
        var value = new GenericEntityResponse(); var notified = false;
        value.PropertyChanged += (_, change) => notified |= change.PropertyName == nameof(GenericEntityResponse.Success);
        value.Success = true; Assert.True(notified);
    }

    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => callback(request, cancellationToken); }
    private sealed class Fixture : IDisposable
    {
        private readonly HttpClient http;
        public CompanyClientContext Company { get; } = new();
        public ClientApiTransport Transport { get; }
        public PpdmDataClient Data { get; }
        public int Calls { get; private set; }
        public Fixture(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler, IAuthenticationProvider? authentication = null)
        {
            Select("company");
            http = new(new Handler((request, token) => { Calls++; return handler(request, token); }));
            Transport = new(http, new() { ApiBaseUrl = "https://api.test", MaxRetries = 3, RetryDelay = TimeSpan.Zero, DefaultConnectionName = "wrong-global-default" }, Company, authentication);
            Data = new(Transport);
        }
        public void Select(string company) { var version = Company.Clear(); Assert.True(Company.TrySelect(version, "account", new(new("tenant", company), ["Admin"], []))); }
        public void Dispose() { http.Dispose(); Company.Dispose(); }
    }
}
