using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Net;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Authentication;
using Beep.OilandGas.Client.Exceptions;
using Beep.OilandGas.Models.Data;
using Moq;
using Xunit;
using AccessApi = Beep.OilandGas.Client.App.Services.AccessControl.AccessControlService;

namespace Beep.OilandGas.Repository.Tests;

public sealed class AccessControlClientTests
{
    private static HttpResponseMessage Json(object value, HttpStatusCode status = HttpStatusCode.OK) => Raw(JsonSerializer.Serialize(value, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), status);
    private static HttpResponseMessage Raw(string value, HttpStatusCode status = HttpStatusCode.OK) => new(status)
        { Content = new StringContent(value, Encoding.UTF8, "application/json") };
    private static AssetAccess Asset(string type = "WELL") => new() { UserId = "subject", AssetType = type, AssetId = "asset", FacilityType = type == "FACILITY" ? "gas" : null, AccessLevel = "READ", OrganizationId = "company", Active = true };

    [Fact]
    public async Task Every_active_operation_uses_the_canonical_controller_verb_route_contract_and_company_transport()
    {
        var calls = new List<(HttpMethod Method, string Path, string? Body)>();
        var auth = new Mock<IAuthenticationProvider>(); auth.Setup(value => value.GetAccessTokenAsync()).ReturnsAsync("token");
        using var f = new Fixture(async (request, _) =>
        {
            calls.Add((request.Method, request.RequestUri!.PathAndQuery, request.Content is null ? null : await request.Content.ReadAsStringAsync()));
            Assert.Equal("tenant", Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Tenant)));
            Assert.Equal("company", Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Company)));
            Assert.Equal("Bearer", request.Headers.Authorization?.Scheme); Assert.Equal("token", request.Headers.Authorization?.Parameter);
            Assert.DoesNotContain("connectionName", request.RequestUri.Query);
            if (request.Method == HttpMethod.Post) return Json(new AccessCheckResponse { HasAccess = true, AccessLevel = "READ" });
            if (request.RequestUri.AbsolutePath.EndsWith("/assets")) return Json(new[] { Asset("FACILITY") });
            if (request.RequestUri.AbsolutePath.EndsWith("/roles")) return Json(new[] { "Reader" });
            if (request.RequestUri.AbsolutePath.Contains("/permission/")) return Json(false);
            return Json(new UserProfile { UserId = "subject", Roles = ["Reader"], PreferredLayout = "custom" });
        }, auth.Object);
        Assert.True(await f.Access.CheckAssetAccessAsync("subject", "plant/one", "facility", facilityType: "gas", requiredPermission: "facility.read"));
        Assert.Equal("gas", Assert.Single(await f.Access.GetUserAccessibleAssetsAsync("subject", "FACILITY", includeInherited: false, organizationId: "company")).FacilityType);
        Assert.Equal(new[] { "Reader" }, await f.Access.GetUserRolesAsync("subject"));
        Assert.False(await f.Access.HasPermissionAsync("subject", "permit/read?x=1"));
        Assert.Equal("custom", (await f.Access.GetUserProfileAsync("subject")).PreferredLayout);
        Assert.Equal(new[] { HttpMethod.Post, HttpMethod.Get, HttpMethod.Get, HttpMethod.Get, HttpMethod.Get }, calls.Select(value => value.Method));
        Assert.Equal(new[] { "/api/AccessControl/check-access", "/api/AccessControl/user/subject/assets?includeInherited=false&assetType=FACILITY&organizationId=company",
            "/api/AccessControl/user/subject/roles", "/api/AccessControl/user/subject/permission/permit%2Fread%3Fx%3D1", "/api/UserProfile/subject" }, calls.Select(value => value.Path));
        using var body = JsonDocument.Parse(calls[0].Body!);
        Assert.Equal("subject", body.RootElement.GetProperty("userId").GetString()); Assert.Equal("plant/one", body.RootElement.GetProperty("assetId").GetString());
        Assert.Equal("FACILITY", body.RootElement.GetProperty("assetType").GetString()); Assert.Equal("gas", body.RootElement.GetProperty("facilityType").GetString());
        Assert.Equal("facility.read", body.RootElement.GetProperty("requiredPermission").GetString());
        Assert.False(body.RootElement.TryGetProperty("connectionName", out _)); auth.Verify(value => value.GetAccessTokenAsync(), Times.Exactly(5));
    }

    [Theory]
    [InlineData("assets")] [InlineData("roles")] [InlineData("profile")] [InlineData("permission")]
    public async Task User_and_permission_route_segments_are_encoded_without_changing_the_API_origin(string operation)
    {
        using var f = new Fixture((request, _) =>
        {
            Assert.Equal("api.test", request.RequestUri!.Host); Assert.Contains("subject%2Fone%3Fx%3D1", request.RequestUri.PathAndQuery);
            return Task.FromResult(operation switch { "assets" => Json(Array.Empty<AssetAccess>()), "roles" => Json(Array.Empty<string>()), "profile" => Json(new UserProfile { UserId = "subject/one?x=1" }), _ => Json(false) });
        });
        await Read(f.Access, operation, "subject/one?x=1"); Assert.Equal(1, f.Calls);
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public async Task Actual_false_decisions_are_distinct_from_invalid_or_missing_responses(bool allowed)
    {
        using var f = new Fixture((request, _) => Task.FromResult(request.Method == HttpMethod.Post ? Json(new AccessCheckResponse { HasAccess = allowed, AccessLevel = allowed ? "READ" : null }) : Json(allowed)));
        Assert.Equal(allowed, await f.Access.CheckAssetAccessAsync("subject", "asset", "WELL"));
        Assert.Equal(allowed, await f.Access.HasPermissionAsync("subject", "read"));
    }

    [Theory]
    [InlineData("check", "")] [InlineData("check", "null")] [InlineData("check", "{}")] [InlineData("check", "true")] [InlineData("check", "[")]
    [InlineData("check", "{\"hasAccess\":true}")]
    [InlineData("permission", "")] [InlineData("permission", "null")] [InlineData("permission", "{}")] [InlineData("permission", "\"true\"")]
    [InlineData("assets", "")] [InlineData("assets", "null")] [InlineData("assets", "{}")] [InlineData("assets", "[")]
    [InlineData("roles", "null")] [InlineData("roles", "[null]")] [InlineData("profile", "null")] [InlineData("profile", "{}")] [InlineData("profile", "[")]
    public async Task Missing_malformed_or_inconsistent_success_payloads_remain_explicit_errors(string operation, string response)
    {
        using var f = new Fixture((_, _) => Task.FromResult(Raw(response)));
        await Assert.ThrowsAnyAsync<ApiClientException>(() => Read(f.Access, operation)); Assert.Equal(1, f.Calls);
    }

    [Theory]
    [InlineData("subject")] [InlineData("inactive")] [InlineData("type")] [InlineData("level")] [InlineData("company")] [InlineData("foreign-company")] [InlineData("facility-key")]
    public async Task Accessible_asset_payloads_must_match_the_requested_subject_and_complete_reviewed_identity(string problem)
    {
        var value = Asset(problem == "facility-key" ? "FACILITY" : "WELL");
        if (problem == "subject") value.UserId = "other";
        if (problem == "inactive") value.Active = false;
        if (problem == "type") value.AssetType = "UNKNOWN";
        if (problem == "level") value.AccessLevel = "OWNER";
        if (problem == "company") value.OrganizationId = null;
        if (problem == "foreign-company") value.OrganizationId = "other";
        if (problem == "facility-key") value.FacilityType = null;
        using var f = new Fixture((_, _) => Task.FromResult(Json(new[] { value })));
        await Assert.ThrowsAsync<ApiClientException>(() => f.Access.GetUserAccessibleAssetsAsync("subject"));
    }

    [Fact]
    public async Task A_response_cannot_silently_discard_an_explicit_organization_assertion()
    {
        using var f = new Fixture((_, _) => Task.FromResult(Json(new[] { Asset() })));
        await Assert.ThrowsAsync<ApiClientException>(() => f.Access.GetUserAccessibleAssetsAsync("subject", organizationId: "other"));
    }

    [Fact]
    public async Task A_profile_cannot_silently_substitute_another_subject()
    {
        using var f = new Fixture((_, _) => Task.FromResult(Json(new UserProfile { UserId = "other" })));
        await Assert.ThrowsAsync<ApiClientException>(() => f.Access.GetUserProfileAsync("subject"));
    }

    [Theory]
    [InlineData("check", HttpStatusCode.Unauthorized)] [InlineData("check", HttpStatusCode.Forbidden)]
    [InlineData("assets", HttpStatusCode.Forbidden)] [InlineData("profile", HttpStatusCode.NotFound)]
    [InlineData("roles", HttpStatusCode.InternalServerError)] [InlineData("permission", HttpStatusCode.Conflict)]
    [InlineData("stored", HttpStatusCode.Forbidden)] [InlineData("retire", HttpStatusCode.Conflict)] [InlineData("history", HttpStatusCode.Unauthorized)]
    public async Task HTTP_refusals_and_errors_do_not_become_grants_denials_or_default_profiles(string operation, HttpStatusCode status)
    {
        using var f = new Fixture((_, _) => Task.FromResult(Json(new { error = "refused" }, status)));
        var error = await Assert.ThrowsAsync<ApiException>(() => Read(f.Access, operation)); Assert.Equal(status, error.StatusCode); Assert.Equal(1, f.Calls);
    }

    [Theory]
    [InlineData("facility-key")] [InlineData("extra-facility-key")] [InlineData("user")] [InlineData("asset")] [InlineData("type")] [InlineData("permission")] [InlineData("list-type")]
    public async Task Invalid_inputs_refuse_before_token_lookup_or_HTTP(string problem)
    {
        var auth = new Mock<IAuthenticationProvider>(MockBehavior.Strict); using var f = new Fixture((_, _) => throw new InvalidOperationException("must not send"), auth.Object);
        Task Request() => problem switch
        {
            "facility-key" => f.Access.CheckAssetAccessAsync("subject", "asset", "FACILITY"),
            "extra-facility-key" => f.Access.CheckAssetAccessAsync("subject", "asset", "WELL", facilityType: "gas"),
            "user" => f.Access.GetUserRolesAsync(" "), "asset" => f.Access.CheckAssetAccessAsync("subject", " ", "WELL"),
            "type" => f.Access.CheckAssetAccessAsync("subject", "asset", "UNKNOWN"),
            "permission" => f.Access.HasPermissionAsync("subject", " "), _ => f.Access.GetUserAccessibleAssetsAsync("subject", "UNKNOWN")
        };
        await Assert.ThrowsAnyAsync<ArgumentException>(Request); Assert.Equal(0, f.Calls); auth.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("check")] [InlineData("assets")] [InlineData("roles")] [InlineData("permission")] [InlineData("profile")]
    [InlineData("stored")] [InlineData("retire")] [InlineData("history")]
    public async Task Missing_company_selection_refuses_every_operation_before_authentication_or_HTTP(string operation)
    {
        var auth = new Mock<IAuthenticationProvider>(MockBehavior.Strict); using var f = new Fixture((_, _) => throw new InvalidOperationException("must not send"), auth.Object);
        f.Company.Clear(); await Assert.ThrowsAsync<InvalidOperationException>(() => Read(f.Access, operation)); Assert.Equal(0, f.Calls); auth.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("check")] [InlineData("assets")] [InlineData("roles")] [InlineData("permission")] [InlineData("profile")]
    [InlineData("stored")] [InlineData("retire")] [InlineData("history")]
    public async Task Caller_cancellation_refuses_every_operation_before_authentication_or_HTTP(string operation)
    {
        var auth = new Mock<IAuthenticationProvider>(MockBehavior.Strict); using var f = new Fixture((_, _) => throw new InvalidOperationException("must not send"), auth.Object);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Read(f.Access, operation, token: new CancellationToken(true))); Assert.Equal(0, f.Calls); auth.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("check")] [InlineData("assets")] [InlineData("roles")] [InlineData("permission")] [InlineData("profile")]
    [InlineData("stored")] [InlineData("retire")] [InlineData("history")]
    public async Task A_company_switch_during_token_lookup_cannot_adopt_the_new_company(string operation)
    {
        var waiting = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var auth = new Mock<IAuthenticationProvider>(); auth.Setup(value => value.GetAccessTokenAsync()).Returns(waiting.Task);
        using var f = new Fixture((_, _) => throw new InvalidOperationException("must not send"), auth.Object);
        var pending = Read(f.Access, operation); Assert.False(pending.IsCompleted); f.Select("other"); waiting.SetResult("token");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending); Assert.Equal(0, f.Calls);
    }

    [Theory]
    [InlineData("check")] [InlineData("assets")] [InlineData("roles")] [InlineData("permission")] [InlineData("profile")]
    [InlineData("stored")] [InlineData("retire")] [InlineData("history")]
    public async Task A_delayed_response_cannot_populate_a_different_company_even_if_the_handler_ignores_cancellation(string operation)
    {
        var waiting = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var f = new Fixture((_, _) => waiting.Task);
        var pending = Read(f.Access, operation); Assert.False(pending.IsCompleted); f.Select("other"); waiting.SetResult(Raw("null"));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending); Assert.Equal(1, f.Calls);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Access_check_POSTs_are_single_attempts_for_connection_failures_and_timeouts(bool timeout)
    {
        using var f = new Fixture((_, _) => timeout ? throw new TaskCanceledException("timeout", new TimeoutException()) : throw new HttpRequestException("lost"));
        await Assert.ThrowsAnyAsync<ApiClientException>(() => f.Access.CheckAssetAccessAsync("subject", "asset", "WELL")); Assert.Equal(1, f.Calls);
    }

    [Fact]
    public async Task Access_GET_retries_use_fresh_messages_and_preserve_the_original_company()
    {
        var messages = new List<HttpRequestMessage>();
        using var f = new Fixture((request, _) =>
        {
            messages.Add(request); Assert.Equal("company", Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Company)));
            if (messages.Count == 1) throw new HttpRequestException("transient"); return Task.FromResult(Json(new[] { "Reader" }));
        });
        Assert.Equal(new[] { "Reader" }, await f.Access.GetUserRolesAsync("subject")); Assert.Equal(2, f.Calls); Assert.NotSame(messages[0], messages[1]);
    }

    [Fact]
    public void SDK_services_and_shared_access_contracts_use_their_existing_owners()
    {
        var owner = typeof(ModelEntityBase).Assembly;
        var sdkOwner=typeof(Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService).Assembly;
        Assert.Equal("Beep.OilandGas.Client",sdkOwner.GetName().Name);
        Assert.NotEqual(owner,sdkOwner);
        Assert.Equal(owner, typeof(Beep.OilandGas.Models.Core.Interfaces.IAccessControlService).Assembly);
        Assert.Equal(owner, typeof(AssetAccess).Assembly); Assert.Equal(owner, typeof(AccessCheckRequest).Assembly);
        Assert.Equal(owner, typeof(AccessCheckResponse).Assembly); Assert.Equal(owner, typeof(UserProfile).Assembly);
    }

    [Theory]
    [InlineData("FIELD")] [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task Actual_client_and_facade_round_trip_shared_contracts_and_current_PPDM_visibility(string table)
    {
        using var domain = await CompanyPPDMRowAccessTests.Fixture.Create();
        await domain.Grant("FIELD", "A", "READ", true);
        object row = table switch
        {
            "FIELD" => new Beep.OilandGas.PPDM39.Models.FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" },
            "WELL" => new Beep.OilandGas.PPDM39.Models.WELL { UWI = "asset", ASSIGNED_FIELD = "A", OPERATOR = "company", ACTIVE_IND = "Y" },
            "POOL" => new Beep.OilandGas.PPDM39.Models.POOL { POOL_ID = "asset", FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" },
            _ => new Beep.OilandGas.PPDM39.Models.FACILITY { FACILITY_ID = "asset", FACILITY_TYPE = "gas", PRIMARY_FIELD_ID = "A", CURRENT_OPERATOR = "company", ACTIVE_IND = "Y" }
        };
        domain.Rows.Add(row); var backend = CompanyAssetFacadeTests.Service(domain);
        using var f = new Fixture(async (request, _) =>
        {
            Assert.Equal("company", Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Company)));
            if (request.Method == HttpMethod.Post)
            {
                Assert.Equal("/api/AccessControl/check-access", request.RequestUri!.AbsolutePath);
                var input = JsonSerializer.Deserialize<AccessCheckRequest>(await request.Content!.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                return Json(await backend.CheckAssetAccessAsync(input.UserId, input.AssetId, input.AssetType, input.RequiredPermission, input.FacilityType));
            }
            Assert.Equal("/api/AccessControl/user/actor/assets", request.RequestUri!.AbsolutePath);
            return Json(await backend.GetUserAccessibleAssetsAsync("actor", table));
        });
        var id = table == "FIELD" ? "A" : "asset";
        Assert.True(await f.Access.CheckAssetAccessAsync("actor", id, table, facilityType: table == "FACILITY" ? "gas" : null));
        Assert.Equal(id, Assert.Single(await f.Access.GetUserAccessibleAssetsAsync("actor", table)).AssetId);
        row.GetType().GetProperty("ACTIVE_IND")!.SetValue(row, "N");
        Assert.False(await f.Access.CheckAssetAccessAsync("actor", id, table, facilityType: table == "FACILITY" ? "gas" : null));
        Assert.Empty(await f.Access.GetUserAccessibleAssetsAsync("actor", table));
        Assert.Equal(4, f.Calls); Assert.Empty(domain.Writes);
    }

    [Fact]
    public async Task Actual_SDK_and_facade_preserve_direct_facility_qualifiers_through_shared_JSON()
    {
        using var domain = await CompanyPPDMRowAccessTests.Fixture.Create(); await domain.Grant("FACILITY", "plant", facilityType: "gas");
        domain.Rows.AddRange([new Beep.OilandGas.PPDM39.Models.FACILITY { FACILITY_ID = "plant", FACILITY_TYPE = "gas", CURRENT_OPERATOR = "company", ACTIVE_IND = "Y" },
            new Beep.OilandGas.PPDM39.Models.FACILITY { FACILITY_ID = "plant", FACILITY_TYPE = "oil", CURRENT_OPERATOR = "company", ACTIVE_IND = "Y" }]);
        var backend = CompanyAssetFacadeTests.Service(domain);
        using var f = new Fixture(async (request, _) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                var input = JsonSerializer.Deserialize<AccessCheckRequest>(await request.Content!.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                return Json(await backend.CheckAssetAccessAsync(input.UserId, input.AssetId, input.AssetType, input.RequiredPermission, input.FacilityType));
            }
            return Json(await backend.GetUserAccessibleAssetsAsync("actor", "FACILITY", includeInherited: false));
        });
        Assert.Equal("gas", Assert.Single(await f.Access.GetUserAccessibleAssetsAsync("actor", "FACILITY", includeInherited: false)).FacilityType);
        Assert.True(await f.Access.CheckAssetAccessAsync("actor", "plant", "FACILITY", facilityType: "gas"));
        Assert.False(await f.Access.CheckAssetAccessAsync("actor", "plant", "FACILITY", facilityType: "oil")); Assert.Empty(domain.Writes);
    }

    [Theory]
    [InlineData("READ")] [InlineData("WRITE")] [InlineData("DELETE")]
    public async Task Actual_SDK_round_trip_preserves_the_PPDM_entitlement_cap_and_loses_access_after_entitlement_revocation(string level)
    {
        using var domain = await CompanyPPDMRowAccessTests.Fixture.Create(); await domain.Grant("FIELD", "A", "DELETE", true);
        domain.Rows.OfType<Beep.OilandGas.PPDM39.Models.ENTITLEMENT>().Single().ACCESS_COND_CODE = "test-" + level.ToLowerInvariant();
        domain.Rows.Add(new Beep.OilandGas.PPDM39.Models.FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" });
        var backend = CompanyAssetFacadeTests.Service(domain);
        using var f = new Fixture(async (request, _) => {
            Assert.Equal("tenant", Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Tenant)));
            Assert.Equal("company", Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Company)));
            if (request.Method == HttpMethod.Post) {
                var input = JsonSerializer.Deserialize<AccessCheckRequest>(await request.Content!.ReadAsStringAsync(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                return Json(await backend.CheckAssetAccessAsync(input.UserId, input.AssetId, input.AssetType));
            }
            return Json(await backend.GetUserAccessibleAssetsAsync("actor", "FIELD"));
        });
        Assert.Equal(level, Assert.Single(await f.Access.GetUserAccessibleAssetsAsync("actor", "FIELD")).AccessLevel);
        Assert.True(await f.Access.CheckAssetAccessAsync("actor", "A", "FIELD"));
        domain.Rows.OfType<Beep.OilandGas.PPDM39.Models.ENT_COMPONENT>().Single().ACTIVE_IND = "N";
        Assert.Empty(await f.Access.GetUserAccessibleAssetsAsync("actor", "FIELD"));
        Assert.False(await f.Access.CheckAssetAccessAsync("actor", "A", "FIELD")); Assert.Empty(domain.Writes);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Retirement_POST_never_retries_an_unconfirmed_write(bool timeout)
    {
        using var f = new Fixture((_, _) => timeout ? throw new TaskCanceledException("timeout", new TimeoutException()) : throw new HttpRequestException("lost"));
        await Assert.ThrowsAnyAsync<ApiClientException>(() => f.Access.RetireStoredGrantAsync("grant", new("reviewed-revision", "Reviewed"))); Assert.Equal(1, f.Calls);
    }

    private static async Task Read(AccessApi client, string operation, string user = "subject", CancellationToken token = default)
    {
        switch (operation)
        {
            case "check": await client.CheckAssetAccessAsync(user, "asset", "WELL", token); break;
            case "assets": await client.GetUserAccessibleAssetsAsync(user, cancellationToken: token); break;
            case "roles": await client.GetUserRolesAsync(user, token); break;
            case "permission": await client.HasPermissionAsync(user, "read", token); break;
            case "profile": await client.GetUserProfileAsync(user, token); break;
            case "stored": await client.ReviewStoredGrantsAsync(user, cancellationToken: token); break;
            case "retire": await client.RetireStoredGrantAsync("grant", new("reviewed-revision", "Reviewed"), token); break;
            case "history": await client.GetGrantRetirementsAsync("grant", cancellationToken: token); break;
            default: throw new ArgumentException();
        }
    }
    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => callback(request, token); }
    private sealed class Fixture : IDisposable
    {
        private readonly HttpClient http;
        public CompanyClientContext Company { get; } = new();
        public AccessApi Access { get; }
        public int Calls { get; private set; }
        public Fixture(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler, IAuthenticationProvider? authentication = null)
        {
            Select("company"); http = new(new Handler((request, token) => { Calls++; return handler(request, token); }));
            Access = new(new ClientApiTransport(http, new() { ApiBaseUrl = "https://api.test", MaxRetries = 3, RetryDelay = TimeSpan.Zero,
                DefaultConnectionName = "unrelated-default" }, Company, authentication));
        }
        public void Select(string company) { var version = Company.Clear(); Assert.True(Company.TrySelect(version, "account", new(new("tenant", company), ["Reader"], []))); }
        public void Dispose() { http.Dispose(); Company.Dispose(); }
    }
}
