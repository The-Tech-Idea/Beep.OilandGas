using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Net;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyAssetGrantUiTests
{
    private static void Select(CompanyClientContext company, bool administrator = true, string actor = "actor", CompanyScope? scope = null) =>
        Assert.True(company.TrySelect(company.Clear(), actor, new(scope ?? new("tenant", "company"), administrator ? ["Administrator"] : [], [])));
    private static string Explain(Exception failure, string operation, string sentence) => failure is RefusalException refusal ? refusal.Sentence : sentence + " (reference test)";
    private static AssetAccess Row(string user = "subject", string type = "FIELD", string id = "asset", string organization = "company", string level = "READ", bool inherit = true) =>
        new() { UserId = user, AssetType = type, AssetId = id, OrganizationId = organization, AccessLevel = level, Inherit = inherit, Active = true };
    private static AssetGrantAdministrationState State(CompanyClientContext company, Func<string, CancellationToken, Task<List<AssetAccess>?>> load,
        Func<string, GrantAccessRequest, CancellationToken, Task<bool>>? grant = null, Func<string, RevokeAccessRequest, CancellationToken, Task<bool>>? revoke = null) =>
        new(company, load, grant ?? ((_, _, _) => throw new InvalidOperationException("Unexpected grant")), revoke ?? ((_, _, _) => throw new InvalidOperationException("Unexpected revoke")), Explain,
            Mock.Of<Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService>());

    [Theory]
    [InlineData("company")] [InlineData("member")] [InlineData("blank")] [InlineData("padded")] [InlineData("long")] [InlineData("control")]
    public async Task Missing_authority_or_invalid_subject_cannot_request_or_mutate_grants(string reason)
    {
        using var company = new CompanyClientContext(); if (reason != "company") Select(company, reason != "member"); var calls = 0;
        using var state = State(company, (_, _) => { calls++; return Task.FromResult<List<AssetAccess>?>([]); });
        state.SelectUser(reason switch { "blank" => " ", "padded" => " subject ", "long" => new string('a', 129), "control" => "sub\nject", _ => "subject" });
        Assert.False(state.CanLoad); await state.LoadAsync(); await state.GrantAsync(); await state.RevokeAsync(Row());
        Assert.Equal(0, calls); Assert.NotNull(state.Error); Assert.False(state.Loaded); Assert.False(state.CanEdit); Assert.False(state.Busy);
    }

    [Fact]
    public async Task Subject_and_company_are_encoded_in_a_direct_only_read_and_empty_is_confirmed()
    {
        using var company = new CompanyClientContext(); Select(company, scope: new("tenant", "company / north")); string? path = null;
        using var state = State(company, (url, _) => { path = url; return Task.FromResult<List<AssetAccess>?>([]); });
        state.SelectUser("subject / one"); await state.LoadAsync();
        Assert.Equal("/api/AccessControl/user/subject%20%2F%20one/assets?includeInherited=false&organizationId=company%20%2F%20north", path);
        Assert.True(state.Loaded); Assert.True(state.CanEdit); Assert.Empty(state.Grants); Assert.Null(state.Error);
    }

    [Theory]
    [InlineData("null")] [InlineData("null-row")] [InlineData("subject")] [InlineData("organization")] [InlineData("active")]
    [InlineData("facility")] [InlineData("qualifier")] [InlineData("area")] [InlineData("id")] [InlineData("level")] [InlineData("duplicate")] [InlineData("bound")]
    public async Task Malformed_or_scope_mismatched_direct_results_never_enable_editing(string reason)
    {
        using var company = new CompanyClientContext(); Select(company); List<AssetAccess>? rows = [Row()];
        switch (reason)
        {
            case "null": rows = null; break; case "null-row": rows!.Add(null!); break;
            case "subject": rows![0].UserId = "other"; break; case "organization": rows![0].OrganizationId = "other"; break;
            case "active": rows![0].Active = false; break; case "facility": rows![0].AssetType = "FACILITY"; rows[0].FacilityType = null; break;
            case "qualifier": rows![0].FacilityType = "gas"; break; case "area": rows![0].AssetType = "AREA"; break;
            case "id": rows![0].AssetId = " "; break; case "level": rows![0].AccessLevel = "ADMIN"; break;
            case "duplicate": rows!.Add(Row()); break; default: rows = Enumerable.Range(0, 4097).Select(index => Row(id: index.ToString())).ToList(); break;
        }
        using var state = State(company, (_, _) => Task.FromResult(rows)); state.SelectUser("subject"); await state.LoadAsync();
        Assert.Empty(state.Grants); Assert.NotNull(state.Error); Assert.False(state.Loaded); Assert.False(state.CanEdit); Assert.False(state.Busy);
    }

    [Theory]
    [InlineData("FIELD", true)] [InlineData("WELL", false)] [InlineData("POOL", false)]
    public async Task Supported_grant_sends_exact_shared_contract_and_confirms_the_requested_readback(string type, bool inherit)
    {
        using var company = new CompanyClientContext(); Select(company); var reads = 0; var writes = 0;
        using var state = State(company, (_, _) => Task.FromResult<List<AssetAccess>?>(++reads == 1 ? [] : [Row(type: type, level: "DELETE", inherit: inherit)]),
            (path, request, _) =>
            {
                writes++; Assert.Equal("/api/AccessControl/grant-access", path); Assert.Equal("subject", request.UserId); Assert.Equal("company", request.OrganizationId);
                Assert.Equal(type, request.AssetType); Assert.Equal("asset", request.AssetId); Assert.Equal("DELETE", request.AccessLevel); Assert.Equal(inherit, request.Inherit);
                Assert.Empty(request.ROW_CREATED_BY); return Task.FromResult(true);
            });
        state.SelectUser("subject"); await state.LoadAsync(); state.SelectAssetType(type); state.AssetId = "asset"; state.AccessLevel = "DELETE";
        await state.GrantAsync(); Assert.Equal(1, writes); Assert.Equal(2, reads); Assert.NotNull(state.Confirmation); Assert.Null(state.Error);
        Assert.True(state.CanRevoke(Assert.Single(state.Grants))); Assert.False(state.CanRevoke(Row(type: type))); Assert.False(state.Busy);
    }

    [Theory]
    [InlineData("facility")] [InlineData("area")] [InlineData("id")] [InlineData("padded")] [InlineData("control")] [InlineData("level")] [InlineData("inherit")]
    public async Task Invalid_or_unsupported_grants_make_no_write_and_keep_the_confirmed_list(string reason)
    {
        using var company = new CompanyClientContext(); Select(company); var row = Row();
        using var state = State(company, (_, _) => Task.FromResult<List<AssetAccess>?>([row])); state.SelectUser("subject"); await state.LoadAsync(); state.AssetId = "asset";
        switch (reason)
        {
            case "facility": state.AssetType = "FACILITY"; break; case "area": state.AssetType = "AREA"; break;
            case "id": state.AssetId = ""; break; case "padded": state.AssetId = " asset "; break; case "control": state.AssetId = "bad\nasset"; break;
            case "level": state.AccessLevel = "ADMIN"; break; default: state.AssetType = "WELL"; state.Inherit = true; break;
        }
        await state.GrantAsync(); Assert.Same(row, Assert.Single(state.Grants)); Assert.NotNull(state.Error); Assert.True(state.CanEdit); Assert.Null(state.Confirmation);
    }

    [Fact]
    public async Task Revoke_uses_the_confirmed_subject_and_key_then_requires_its_absence_in_the_new_list()
    {
        using var company = new CompanyClientContext(); Select(company); var row = Row(); var reads = 0;
        using var state = State(company, (_, _) => Task.FromResult<List<AssetAccess>?>(++reads == 1 ? [row] : []), revoke: (path, request, _) =>
        {
            Assert.Equal("/api/AccessControl/revoke-access", path); Assert.Equal("subject", request.UserId); Assert.Equal("FIELD", request.AssetType); Assert.Equal("asset", request.AssetId); return Task.FromResult(true);
        });
        state.SelectUser("subject"); await state.LoadAsync(); await state.RevokeAsync(row);
        Assert.Empty(state.Grants); Assert.NotNull(state.Confirmation); Assert.True(state.CanEdit); Assert.False(state.CanRevoke(row));
    }

    [Theory]
    [InlineData("user")] [InlineData("company")] [InlineData("type")] [InlineData("id")] [InlineData("level")] [InlineData("inherit")] [InlineData("active")] [InlineData("qualifier")]
    public async Task Mutated_confirmed_rows_cannot_be_revoked(string part)
    {
        using var company = new CompanyClientContext(); Select(company); var row = Row();
        using var state = State(company, (_, _) => Task.FromResult<List<AssetAccess>?>([row])); state.SelectUser("subject"); await state.LoadAsync(); Assert.True(state.CanRevoke(row));
        switch (part)
        {
            case "user": row.UserId = "other"; break; case "company": row.OrganizationId = "other"; break; case "type": row.AssetType = "WELL"; break;
            case "id": row.AssetId = "other"; break; case "level": row.AccessLevel = "DELETE"; break; case "inherit": row.Inherit = false; break;
            case "active": row.Active = false; break; default: row.FacilityType = "gas"; break;
        }
        Assert.False(state.CanRevoke(row)); await state.RevokeAsync(row); Assert.Null(state.Confirmation);
    }

    [Theory]
    [InlineData("false")] [InlineData("write-error")] [InlineData("read-error")] [InlineData("null")] [InlineData("wrong-level")] [InlineData("wrong-inherit")] [InlineData("missing-grant")] [InlineData("still-revoked-key")]
    public async Task Unknown_or_unconfirmed_changes_clear_state_and_require_reload_before_retry(string reason)
    {
        using var company = new CompanyClientContext(); Select(company); var row = Row(); var reads = 0; var writes = 0;
        Task<bool> Write()
        {
            writes++; if (reason == "write-error") throw new HttpRequestException("internal details"); return Task.FromResult(reason != "false");
        }
        using var state = State(company, (_, _) =>
        {
            if (++reads == 1) return Task.FromResult<List<AssetAccess>?>(reason == "still-revoked-key" ? [row] : []);
            if (reason == "read-error") throw new HttpRequestException("internal details");
            return Task.FromResult<List<AssetAccess>?>(reason == "null" ? null : reason == "missing-grant" ? [] :
                [Row(level: reason == "wrong-level" ? "READ" : "WRITE", inherit: reason != "wrong-inherit")]);
        }, (_, _, _) => Write(), (_, _, _) => Write());
        state.SelectUser("subject"); await state.LoadAsync(); state.AssetId = "asset"; state.AccessLevel = "WRITE";
        if (reason == "still-revoked-key") await state.RevokeAsync(row); else await state.GrantAsync();
        await state.GrantAsync(); Assert.Equal(1, writes); Assert.Empty(state.Grants); Assert.False(state.Loaded); Assert.False(state.CanEdit);
        Assert.False(state.CanRevoke(row)); Assert.False(state.Busy); Assert.Null(state.Confirmation); Assert.Contains("Reload before", state.Error); Assert.DoesNotContain("internal details", state.Error);
    }

    [Theory]
    [InlineData("company")] [InlineData("same-company")] [InlineData("actor")] [InlineData("subject")] [InlineData("dispose")] [InlineData("reload")]
    public async Task Changes_cancel_inflight_reads_and_late_results_cannot_enable_editing(string change)
    {
        using var company = new CompanyClientContext(); Select(company); var reads = 0; CancellationToken seen = default;
        var old = new TaskCompletionSource<List<AssetAccess>?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var state = State(company, (_, token) => { seen = token; return ++reads == 1 ? old.Task : Task.FromResult<List<AssetAccess>?>([]); });
        state.SelectUser("subject"); var first = state.LoadAsync(); var oldToken = seen;
        if (change == "subject") state.SelectUser("other"); else if (change == "dispose") state.Dispose(); else if (change == "reload") await state.LoadAsync();
        else Select(company, actor: change == "actor" ? "other-actor" : "actor", scope: change == "company" ? new("other-tenant", "other-company") : null);
        Assert.True(oldToken.IsCancellationRequested); old.SetResult([Row()]); await first;
        Assert.Empty(state.Grants); Assert.Null(state.Error); Assert.False(state.Busy); Assert.Equal(change == "reload", state.Loaded);
        if (change is "company" or "same-company" or "actor") Assert.Empty(state.UserId);
    }

    [Theory]
    [InlineData("subject", false)] [InlineData("subject", true)] [InlineData("company", false)] [InlineData("company", true)]
    [InlineData("actor", false)] [InlineData("actor", true)] [InlineData("dispose", false)] [InlineData("dispose", true)] [InlineData("reload", false)] [InlineData("reload", true)]
    public async Task Changes_during_write_or_readback_cannot_publish_old_success(string change, bool duringReadback)
    {
        using var company = new CompanyClientContext(); Select(company); var reads = 0; var writes = 0; CancellationToken seen = default;
        var oldWrite = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var oldRead = new TaskCompletionSource<List<AssetAccess>?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var state = State(company, (_, token) =>
        {
            reads++; if (duringReadback && reads == 2) { seen = token; return oldRead.Task; } return Task.FromResult<List<AssetAccess>?>([]);
        }, (_, request, token) => { writes++; Assert.Equal("subject", request.UserId); seen = token; return duringReadback ? Task.FromResult(true) : oldWrite.Task; });
        state.SelectUser("subject"); await state.LoadAsync(); state.AssetId = "asset"; var write = state.GrantAsync();
        Assert.True(state.Busy); Assert.False(state.CanEdit); await state.GrantAsync(); Assert.Equal(1, writes);
        if (change == "subject") state.SelectUser("other"); else if (change == "dispose") state.Dispose(); else if (change == "reload") await state.LoadAsync();
        else Select(company, actor: change == "actor" ? "other-actor" : "actor", scope: change == "company" ? new("other", "other") : null);
        Assert.True(seen.IsCancellationRequested);
        if (duringReadback) oldRead.SetResult([Row()]); else oldWrite.SetResult(true); await write;
        Assert.Null(state.Confirmation); Assert.Null(state.Error); Assert.Empty(state.Grants); Assert.False(state.Busy); Assert.Equal(change == "reload", state.Loaded);
    }

    [Fact]
    public async Task Failed_reload_or_changed_subject_removes_previous_revoke_targets()
    {
        using var company = new CompanyClientContext(); Select(company); var row = Row(); var reads = 0;
        using var state = State(company, (_, _) => ++reads == 1 ? Task.FromResult<List<AssetAccess>?>([row]) : throw new HttpRequestException());
        state.SelectUser("subject"); await state.LoadAsync(); Assert.True(state.CanRevoke(row)); await state.LoadAsync(); Assert.False(state.CanRevoke(row)); Assert.Empty(state.Grants);
        Assert.NotNull(state.Error); state.SelectUser("other"); Assert.Null(state.Error); Assert.False(state.CanEdit);
    }

    [Theory]
    [InlineData("FIELD")] [InlineData("WELL")] [InlineData("POOL")]
    public async Task Actual_editor_admission_registry_policy_and_PPDM_reads_confirm_grant_update_and_revoke(string type)
    {
        using var f = await Fixture.Create(); f.Editor.Setup(editor => editor.OpenDataSource("selected-tenant")).Returns(ConnectionState.Open);
        f.Rows.Add(new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" });
        f.Rows.Add(type switch { "WELL" => new WELL { UWI = "asset", ACTIVE_IND = "Y", OPERATOR = "company" },
            "POOL" => new POOL { POOL_ID = "asset", ACTIVE_IND = "Y", BUSINESS_ASSOCIATE_ID = "company" }, _ => new FIELD { FIELD_ID = "asset", ACTIVE_IND = "Y" } });
        if (type == "FIELD") f.AddFieldEntitlement("asset");
        var service = CompanyAssetFacadeTests.Service(f, admission: new PPDMAssetGrantTargetVerifier(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Eligibility));
        using var company = new CompanyClientContext(); Select(company, scope: f.Gate.Company.Scope);
        using var state = State(company, async (_, _) => await service.GetUserAccessibleAssetsAsync("actor", organizationId: "company", includeInherited: false),
            (_, request, _) => service.GrantAssetAccessAsync(request.UserId, request.AssetId, request.AssetType, request.AccessLevel, request.Inherit, request.OrganizationId, request.FacilityType),
            (_, request, _) => service.RevokeAssetAccessAsync(request.UserId, request.AssetId, request.AssetType, request.FacilityType));
        state.SelectUser("actor"); await state.LoadAsync(); state.SelectAssetType(type); state.AssetId = "asset"; state.AccessLevel = "WRITE";
        await state.GrantAsync(); Assert.NotNull(state.Confirmation); Assert.Null(state.Error); var row = Assert.Single(state.Grants); Assert.Equal("WRITE", row.AccessLevel);
        var stored = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync(); Assert.Equal("actor", stored.CreatedBy); Assert.Equal("company", stored.OrganizationId);
        state.AccessLevel = "DELETE"; await state.GrantAsync(); Assert.Equal("DELETE", Assert.Single(state.Grants).AccessLevel);
        await state.RevokeAsync(Assert.Single(state.Grants)); Assert.NotNull(state.Confirmation); Assert.Empty(state.Grants); Assert.Null(state.Error);
        Assert.False((await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync()).IsActive); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Actual_store_refuses_a_revoked_administrator_despite_the_stale_UI_selection()
    {
        using var f = await Fixture.Create(); var service = CompanyAssetFacadeTests.Service(f); using var company = new CompanyClientContext(); Select(company, scope: f.Gate.Company.Scope);
        using var state = State(company, async (_, _) => await service.GetUserAccessibleAssetsAsync("actor", organizationId: "company", includeInherited: false),
            (_, request, _) => service.GrantAssetAccessAsync(request.UserId, request.AssetId, request.AssetType, request.AccessLevel, request.Inherit, request.OrganizationId));
        state.SelectUser("actor"); await state.LoadAsync(); state.AssetId = "asset";
        var membership = await f.Db.Set<AppCompanyMembership>().SingleAsync(); membership.EffectiveToUtc = DateTime.UtcNow.AddSeconds(-1); await f.Db.SaveChangesAsync();
        await state.GrantAsync(); Assert.Null(state.Confirmation); Assert.NotNull(state.Error); Assert.False(state.CanEdit); Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToListAsync());
    }

    [Fact]
    public async Task Actual_company_transport_carries_current_headers_and_shared_JSON_for_grant_readback()
    {
        using var company = new CompanyClientContext(); Select(company); var options = new JsonSerializerOptions(JsonSerializerDefaults.Web); var seen = new List<(HttpMethod Method, string Path)>();
        using var http = new HttpClient(new Handler(async request =>
        {
            Assert.Equal("tenant", request.Headers.GetValues(CompanyRequestHeaders.Tenant).Single()); Assert.Equal("company", request.Headers.GetValues(CompanyRequestHeaders.Company).Single());
            seen.Add((request.Method, request.RequestUri!.PathAndQuery)); object response;
            if (request.Method == HttpMethod.Post)
            {
                var body = JsonSerializer.Deserialize<GrantAccessRequest>(await request.Content!.ReadAsStringAsync(), options)!;
                Assert.Equal("subject", body.UserId); Assert.Equal("company", body.OrganizationId); Assert.Equal("FIELD", body.AssetType); Assert.Equal("asset", body.AssetId);
                Assert.Equal("WRITE", body.AccessLevel); response = true;
            }
            else response = seen.Count == 1 ? new List<AssetAccess>() : new List<AssetAccess> { Row(level: "WRITE") };
            return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(response, options)) };
        })) { BaseAddress = new("https://api.test") };
        async Task<List<AssetAccess>?> Read(string path, CancellationToken token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path); company.Prepare(request);
            using var response = await company.SendAsync(http, request, token);
            var result = JsonSerializer.Deserialize<List<AssetAccess>>(await response.Content.ReadAsStringAsync(token), options); company.EnsureCurrent(response); return result;
        }
        async Task<bool> Write(string path, GrantAccessRequest value, CancellationToken token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = new StringContent(JsonSerializer.Serialize(value, options)) }; company.Prepare(request);
            using var response = await company.SendAsync(http, request, token);
            var result = JsonSerializer.Deserialize<bool>(await response.Content.ReadAsStringAsync(token), options); company.EnsureCurrent(response); return result;
        }
        using var state = State(company, Read, Write); state.SelectUser("subject"); await state.LoadAsync(); state.AssetId = "asset"; state.AccessLevel = "WRITE"; await state.GrantAsync();
        Assert.NotNull(state.Confirmation); Assert.Null(state.Error); Assert.Equal("subject", Assert.Single(state.Grants).UserId);
        Assert.Equal(new[] { (HttpMethod.Get, "/api/AccessControl/user/subject/assets?includeInherited=false&organizationId=company"),
            (HttpMethod.Post, "/api/AccessControl/grant-access"), (HttpMethod.Get, "/api/AccessControl/user/subject/assets?includeInherited=false&organizationId=company") }, seen);
        Assert.Empty(http.DefaultRequestHeaders);
    }

    [Theory]
    [InlineData("load", false)] [InlineData("load", true)] [InlineData("write", false)] [InlineData("write", true)] [InlineData("readback", false)] [InlineData("readback", true)]
    public async Task Late_failures_do_not_resolve_failure_services_after_disposal_or_subject_change(string phase, bool dispose)
    {
        using var company = new CompanyClientContext(); Select(company); var reads = 0;
        var oldRead = new TaskCompletionSource<List<AssetAccess>?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var oldWrite = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var state = new AssetGrantAdministrationState(company, (_, _) =>
        {
            reads++; return phase == "load" || phase == "readback" && reads == 2 ? oldRead.Task : Task.FromResult<List<AssetAccess>?>([]);
        }, (_, _, _) => phase == "write" ? oldWrite.Task : Task.FromResult(true), (_, _, _) => throw new InvalidOperationException(),
            (_, _, _) => throw new ObjectDisposedException("Circuit failure services"), Mock.Of<Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService>());
        state.SelectUser("subject"); Task pending;
        if (phase == "load") pending = state.LoadAsync();
        else { await state.LoadAsync(); state.AssetId = "asset"; pending = state.GrantAsync(); }
        if (dispose) state.Dispose(); else state.SelectUser("other");
        if (phase == "write") oldWrite.SetException(new HttpRequestException()); else oldRead.SetException(new HttpRequestException());
        await pending; Assert.Null(state.Error); Assert.Null(state.Confirmation); Assert.Empty(state.Grants); Assert.False(state.Busy); Assert.False(state.CanEdit);
    }

    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => send(request);
    }
}
