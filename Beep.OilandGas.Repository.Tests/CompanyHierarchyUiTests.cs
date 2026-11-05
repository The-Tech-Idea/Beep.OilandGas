using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Services;
using Moq;
using System.Net;
using System.Text.Json;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyHierarchyUiTests
{
    [Theory][InlineData(false)][InlineData(true)]
    public async Task Explicit_order_category_keeps_current_revision_and_requires_exact_saved_content(bool mismatch)
    {
        using var company=new CompanyClientContext();Select(company);var revision=Guid.NewGuid().ToString();var reads=0;
        var current=new[]{"FIELD","WELL","POOL","FACILITY"}.Select(type=>new HierarchyConfig{OrganizationId="company",AssetType=type,HierarchyLevel=type=="FIELD"?0:1,
            ParentLevel=type=="FIELD"?null:0,LevelName=type,Active=true,Revision=revision}).ToList();
        using var state=new HierarchyConfigurationState(company,(_,_)=>{reads++;return Task.FromResult<List<HierarchyConfig>?>(current);},Explain,(_,request,_)=>
        {
            Assert.Equal(5,request.Count);Assert.All(request,row=>Assert.Equal(revision,row.Revision));
            Assert.Null(request.Single(row=>row.AssetType=="WORK_ORDER").ParentLevel);
            var next=Guid.NewGuid().ToString();current=request.Select(row=>new HierarchyConfig{OrganizationId=row.OrganizationId,AssetType=row.AssetType,HierarchyLevel=row.HierarchyLevel,
                ParentLevel=row.ParentLevel,LevelName=row.LevelName,Active=row.Active,Revision=next}).ToList();
            if(mismatch)current.Single(row=>row.AssetType=="WORK_ORDER").LevelName="Different saved label";
            return Task.FromResult(true);
        });
        state.AddOrderCategory();Assert.Empty(state.Configuration);await state.LoadAsync();Assert.True(state.CanAddOrderCategory);
        state.AddOrderCategory();Assert.False(state.CanAddOrderCategory);Assert.Equal(5,state.Configuration.Count);
        state.SetVisible(state.Configuration.Single(row=>row.AssetType=="FIELD"),false);
        var order=state.Configuration.Single(row=>row.AssetType=="WORK_ORDER");Assert.True(order.Active);
        state.SetDisplayName(order,"Recorded orders");await state.SaveAsync();Assert.Equal(2,reads);
        if(mismatch){Assert.Null(state.Confirmation);Assert.False(state.CanSave);Assert.NotNull(state.Error);}
        else{Assert.NotNull(state.Confirmation);Assert.Null(state.Error);Assert.Equal("Recorded orders",state.Configuration.Single(row=>row.AssetType=="WORK_ORDER").LevelName);}
    }
    [Fact]
    public async Task Order_selection_preserves_all_field_components_and_refuses_mutated_relationships()
    {
        using var company=new CompanyClientContext();Select(company);
        var order=new AssetHierarchyNode{AssetId="order",AssetType="WORK_ORDER",UserHasAccess=true,
            WorkOrderComponents=[new("order","a","east","",""),new("order","b","west","","")]};
        using var state=new AssetHierarchyState(company,(_,_)=>Task.FromResult<AssetHierarchyNode?>(new(){AssetId="company",AssetType="ORGANIZATION",Children=[order]}),Explain);
        await state.LoadAsync();Assert.Null(state.Error);Assert.True(state.CanSelect(order));
        order.WorkOrderComponents[1]=new("order","b","other","","");Assert.False(state.CanSelect(order));
        company.Clear();Assert.Null(state.Hierarchy);Assert.False(state.CanSelect(order));
    }

    [Theory][InlineData("parent")][InlineData("missing")][InlineData("duplicate")]
    public async Task Unconfirmed_order_scope_never_becomes_selectable(string fault)
    {
        using var company=new CompanyClientContext();Select(company);
        var order=new AssetHierarchyNode{AssetId="order",AssetType="WORK_ORDER",UserHasAccess=true,WorkOrderComponents=[new("order","a","east","","")]};
        if(fault=="parent"){order.ParentAssetId="east";order.ParentAssetType="FIELD";}
        if(fault=="missing")order.WorkOrderComponents.Clear();
        if(fault=="duplicate")order.WorkOrderComponents.Add(order.WorkOrderComponents[0]);
        using var state=new AssetHierarchyState(company,(_,_)=>Task.FromResult<AssetHierarchyNode?>(new(){AssetId="company",AssetType="ORGANIZATION",Children=[order]}),Explain);
        await state.LoadAsync();Assert.NotNull(state.Error);Assert.Null(state.Hierarchy);Assert.False(state.CanSelect(order));
    }
    private static readonly CompanyScope A = new("tenant", "company");
    private static readonly CompanyScope B = new("other-tenant", "other");
    private static void Select(CompanyClientContext company, CompanyScope? scope = null, string user = "actor", bool admin = true) =>
        Assert.True(company.TrySelect(company.Clear(), user, new(scope ?? A, admin ? ["Administrator"] : [], [])));
    private static string Explain(Exception failure, string operation, string sentence) =>
        failure is RefusalException refusal ? refusal.Sentence : sentence + " (reference test)";
    private static AssetHierarchyNode Field(string id = "A") => new() { AssetId = id, AssetType = "FIELD", ParentAssetId = "company", ParentAssetType = "ORGANIZATION", UserHasAccess = true };
    private static AssetHierarchyNode Facility(string type = "gas", string id = "facility") => new() { AssetId = id, AssetType = "FACILITY", FacilityType = type, ParentAssetId = "A", ParentAssetType = "FIELD", UserHasAccess = true };
    private static AssetHierarchyNode Root(params AssetHierarchyNode[] children) => new() { AssetId = "company", AssetType = "ORGANIZATION", Children = children.ToList() };

    [Fact]
    public async Task Missing_company_never_requests_business_data_and_does_not_invent_an_identity()
    {
        using var company = new CompanyClientContext(); var calls = 0;
        using var state = new AssetHierarchyState(company, (_, _) => { calls++; return Task.FromResult<AssetHierarchyNode?>(Root()); }, Explain);
        await state.LoadAsync(); Assert.True(state.NeedsCompany); Assert.False(state.Loading); Assert.Null(state.Hierarchy); Assert.Null(state.Error); Assert.Equal(0, calls);
    }

    [Fact]
    public async Task Confirmed_facilities_keep_both_key_parts_and_only_current_business_nodes_are_selectable()
    {
        using var company = new CompanyClientContext(); Select(company); var gas = Facility(); var oil = Facility("oil");
        var field = Field(); field.Children = [gas, oil]; var root = Root(field); string? path = null;
        using var state = new AssetHierarchyState(company, (url, _) => { path = url; return Task.FromResult<AssetHierarchyNode?>(root); }, Explain);
        await state.LoadAsync(); Assert.Same(root, state.Hierarchy); Assert.Null(state.Error);
        Assert.Equal("/api/AssetHierarchy/user/actor?organizationId=company", path);
        Assert.False(state.CanSelect(root)); Assert.True(state.CanSelect(field)); Assert.True(state.CanSelect(gas)); Assert.True(state.CanSelect(oil));
        Assert.False(state.CanSelect(Facility())); company.Clear();
        Assert.Null(state.Hierarchy); Assert.False(state.CanSelect(gas)); Assert.True(state.NeedsCompany);
    }

    [Fact]
    public async Task Complete_root_key_and_api_resolved_account_are_encoded_in_the_existing_route()
    {
        using var company = new CompanyClientContext(); Select(company, new("tenant", "company / north"), "account / one"); string? path = null;
        var leaf = Facility("gas / sour", "facility / west");
        using var state = new AssetHierarchyState(company, (url, _) => { path = url; return Task.FromResult<AssetHierarchyNode?>(leaf); }, Explain);
        await state.LoadAsync(rootAssetId: leaf.AssetId, rootAssetType: "facility", rootFacilityType: leaf.FacilityType);
        Assert.Equal("/api/AssetHierarchy/user/account%20%2F%20one?organizationId=company%20%2F%20north&rootAssetId=facility%20%2F%20west&rootAssetType=FACILITY&rootFacilityType=gas%20%2F%20sour", path);
        Assert.True(state.CanSelect(leaf));
    }

    [Theory]
    [InlineData("organization")] [InlineData("blank-organization")] [InlineData("partial-id")] [InlineData("partial-type")]
    [InlineData("unknown")] [InlineData("facility-missing")] [InlineData("facility-blank")] [InlineData("well-qualified")]
    public async Task Invalid_scope_assertions_or_incomplete_root_keys_fail_without_a_request(string reason)
    {
        using var company = new CompanyClientContext(); Select(company); var calls = 0;
        using var state = new AssetHierarchyState(company, (_, _) => { calls++; return Task.FromResult<AssetHierarchyNode?>(Root()); }, Explain);
        await state.LoadAsync(reason == "organization" ? "other" : reason == "blank-organization" ? " " : null,
            reason == "partial-type" ? null : "asset", reason == "partial-id" ? null : reason == "unknown" ? "BLOCK" : reason == "well-qualified" ? "WELL" : "FACILITY",
            reason == "well-qualified" ? "gas" : reason == "facility-blank" ? " " : null);
        Assert.NotNull(state.Error); Assert.Null(state.Hierarchy); Assert.False(state.Loading); Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("null")] [InlineData("wrong-company")] [InlineData("duplicate")] [InlineData("missing-facility-type")]
    [InlineData("qualified-well")] [InlineData("unknown-type")] [InlineData("unconfirmed")] [InlineData("null-child")]
    [InlineData("null-children")] [InlineData("recursive")] [InlineData("bad-parent")] [InlineData("invented-chain")]
    [InlineData("company-grant")]
    public async Task Invalid_hierarchy_responses_cannot_be_shown_or_selected(string reason)
    {
        using var company = new CompanyClientContext(); Select(company); var field = Field(); var leaf = Facility(); field.Children = [leaf];
        AssetHierarchyNode? root = Root(field);
        switch (reason)
        {
            case "null": root = null; break;
            case "wrong-company": root.AssetId = "other"; break;
            case "duplicate": field.Children.Add(Facility()); break;
            case "missing-facility-type": leaf.FacilityType = null; break;
            case "qualified-well": leaf.AssetType = "WELL"; break;
            case "unknown-type": leaf.AssetType = "BLOCK"; break;
            case "unconfirmed": leaf.UserHasAccess = false; break;
            case "null-child": field.Children.Add(null!); break;
            case "null-children": leaf.Children = null!; break;
            case "recursive": leaf.Children.Add(root); break;
            case "bad-parent": leaf.ParentAssetId = "B"; break;
            case "invented-chain": leaf.Children = [new() { AssetId = "well", AssetType = "WELL", UserHasAccess = true }]; break;
            case "company-grant": root.UserHasAccess = true; break;
        }
        using var state = new AssetHierarchyState(company, (_, _) => Task.FromResult(root), Explain);
        await state.LoadAsync(); Assert.NotNull(state.Error); Assert.Null(state.Hierarchy); Assert.False(state.CanSelect(leaf)); Assert.False(state.Loading);
    }

    [Theory]
    [InlineData("id")] [InlineData("type")] [InlineData("facility-type")]
    public async Task A_root_response_must_match_the_complete_requested_identity(string part)
    {
        using var company = new CompanyClientContext(); Select(company); var leaf = Facility();
        if (part == "id") leaf.AssetId = "other"; if (part == "type") { leaf.AssetType = "WELL"; leaf.FacilityType = null; } if (part == "facility-type") leaf.FacilityType = "oil";
        using var state = new AssetHierarchyState(company, (_, _) => Task.FromResult<AssetHierarchyNode?>(leaf), Explain);
        await state.LoadAsync(rootAssetId: "facility", rootAssetType: "FACILITY", rootFacilityType: "gas"); Assert.Null(state.Hierarchy); Assert.NotNull(state.Error);
    }

    [Theory]
    [InlineData("clear")] [InlineData("switch")] [InlineData("same-scope")] [InlineData("account")] [InlineData("dispose")]
    public async Task Selection_changes_cancel_inflight_reads_and_discard_late_responses(string change)
    {
        using var company = new CompanyClientContext(); Select(company);
        var response = new TaskCompletionSource<AssetHierarchyNode?>(TaskCreationOptions.RunContinuationsAsynchronously); CancellationToken seen = default;
        using var state = new AssetHierarchyState(company, (_, token) => { seen = token; return response.Task; }, Explain);
        var load = state.LoadAsync(); Assert.True(state.Loading);
        if (change == "clear") company.Clear(); else if (change == "dispose") state.Dispose(); else Select(company, change == "switch" ? B : A, change == "account" ? "other-user" : "actor");
        Assert.True(seen.IsCancellationRequested); Assert.Null(state.Hierarchy);
        response.SetResult(Root(Field())); await load; Assert.Null(state.Hierarchy); Assert.Null(state.Error); Assert.False(state.Loading);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Superseded_root_load_cannot_replace_the_new_result_or_error(bool failure)
    {
        using var company = new CompanyClientContext(); Select(company); var old = new TaskCompletionSource<AssetHierarchyNode?>(TaskCreationOptions.RunContinuationsAsynchronously); var calls = 0;
        using var state = new AssetHierarchyState(company, (_, _) => ++calls == 1 ? old.Task : Task.FromResult<AssetHierarchyNode?>(Facility("oil")), Explain);
        var first = state.LoadAsync(rootAssetId: "facility", rootAssetType: "FACILITY", rootFacilityType: "gas");
        await state.LoadAsync(rootAssetId: "facility", rootAssetType: "FACILITY", rootFacilityType: "oil");
        if (failure) old.SetException(new HttpRequestException()); else old.SetResult(Facility()); await first;
        Assert.Equal("oil", state.Hierarchy!.FacilityType); Assert.Null(state.Error); Assert.False(state.Loading);
    }

    [Fact]
    public async Task Failed_reload_removes_previously_confirmed_nodes_and_reports_the_failure()
    {
        using var company = new CompanyClientContext(); Select(company); var leaf = Facility(); var calls = 0; var reports = new List<Exception>();
        using var state = new AssetHierarchyState(company, (_, _) => ++calls == 1 ? Task.FromResult<AssetHierarchyNode?>(Root(leaf)) : throw new HttpRequestException("internal provider detail"),
            (failure, operation, sentence) => { reports.Add(failure); return Explain(failure, operation, sentence); });
        await state.LoadAsync(); Assert.True(state.CanSelect(leaf)); await state.LoadAsync();
        Assert.Null(state.Hierarchy); Assert.False(state.CanSelect(leaf)); Assert.Single(reports); Assert.DoesNotContain("internal provider", state.Error); Assert.Contains("reference", state.Error);
    }

    [Fact]
    public async Task Only_successful_empty_company_container_is_an_empty_result()
    {
        using var company = new CompanyClientContext(); Select(company);
        using var state = new AssetHierarchyState(company, (_, _) => Task.FromResult<AssetHierarchyNode?>(Root()), Explain);
        await state.LoadAsync(); Assert.Empty(state.Hierarchy!.Children); Assert.Null(state.Error); Assert.False(state.NeedsCompany);
    }

    [Theory]
    [InlineData("id")] [InlineData("type")] [InlineData("facility-type")] [InlineData("parent-id")] [InlineData("parent-type")] [InlineData("access")]
    public async Task Selection_requires_the_complete_confirmed_identity_even_if_a_consumer_mutates_the_shared_node(string part)
    {
        using var company = new CompanyClientContext(); Select(company); var leaf = Facility();
        using var state = new AssetHierarchyState(company, (_, _) => Task.FromResult<AssetHierarchyNode?>(Root(leaf)), Explain);
        await state.LoadAsync(); Assert.True(state.CanSelect(leaf));
        switch (part)
        {
            case "id": leaf.AssetId = "other"; break; case "type": leaf.AssetType = "WELL"; break;
            case "facility-type": leaf.FacilityType = "oil"; break; case "parent-id": leaf.ParentAssetId = "B"; break;
            case "parent-type": leaf.ParentAssetType = "POOL"; break; default: leaf.UserHasAccess = false; break;
        }
        Assert.False(state.CanSelect(leaf));
    }

    [Theory]
    [InlineData("missing-company")] [InlineData("member")]
    public async Task Configuration_requires_selected_company_admin_without_making_a_request(string reason)
    {
        using var company = new CompanyClientContext(); if (reason == "member") Select(company, admin: false); var calls = 0;
        using var state = new HierarchyConfigurationState(company, (_, _) => { calls++; return Task.FromResult<List<HierarchyConfig>?>([]); }, Explain, (_, _, _) => throw new InvalidOperationException("Unexpected configuration save"));
        await state.LoadAsync(); Assert.NotNull(state.Error); Assert.False(state.Loaded); Assert.False(state.Loading); Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("null")] [InlineData("wrong-company")] [InlineData("duplicate")] [InlineData("null-row")] [InlineData("negative-level")]
    public async Task Configuration_identity_or_missing_response_errors_are_not_empty_success(string reason)
    {
        using var company = new CompanyClientContext(); Select(company); List<HierarchyConfig>? rows = [new() { OrganizationId = "company", HierarchyLevel = 0 }];
        if (reason == "null") rows = null; if (reason == "wrong-company") rows![0].OrganizationId = "other";
        if (reason == "duplicate") rows!.Add(new() { OrganizationId = "company", HierarchyLevel = 0 }); if (reason == "null-row") rows!.Add(null!); if (reason == "negative-level") rows![0].HierarchyLevel = -1;
        using var state = new HierarchyConfigurationState(company, (_, _) => Task.FromResult(rows), Explain, (_, _, _) => throw new InvalidOperationException("Unexpected configuration save"));
        await state.LoadAsync(); Assert.NotNull(state.Error); Assert.Empty(state.Configuration); Assert.False(state.Loaded);
    }

    [Fact]
    public async Task Configuration_route_uses_selected_company_and_successful_empty_results_remain_explicit()
    {
        using var company = new CompanyClientContext(); Select(company, new("tenant", "company / north")); string? path = null;
        using var state = new HierarchyConfigurationState(company, (url, _) => { path = url; return Task.FromResult<List<HierarchyConfig>?>([]); }, Explain, (_, _, _) => throw new InvalidOperationException("Unexpected configuration save"));
        await state.LoadAsync(); Assert.Equal("/api/AssetHierarchy/organization/company%20%2F%20north/config", path); Assert.True(state.Loaded); Assert.Empty(state.Configuration); Assert.Null(state.Error);
    }

    [Theory]
    [InlineData("clear")] [InlineData("switch")] [InlineData("dispose")] [InlineData("reload")]
    public async Task Configuration_cancellation_or_newer_loads_cannot_publish_late_records(string change)
    {
        using var company = new CompanyClientContext(); Select(company); var calls = 0; CancellationToken token = default;
        var old = new TaskCompletionSource<List<HierarchyConfig>?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var state = new HierarchyConfigurationState(company, (_, cancellation) => { token = cancellation; return ++calls == 1 ? old.Task : Task.FromResult<List<HierarchyConfig>?>([]); }, Explain, (_, _, _) => throw new InvalidOperationException("Unexpected configuration save"));
        var first = state.LoadAsync(); var firstToken = token;
        if (change == "clear") company.Clear(); if (change == "switch") Select(company, B); if (change == "dispose") state.Dispose(); if (change == "reload") await state.LoadAsync();
        Assert.True(firstToken.IsCancellationRequested); old.SetResult([new() { OrganizationId = "company", HierarchyLevel = 0 }]); await first;
        Assert.Empty(state.Configuration); Assert.Null(state.Error); Assert.Equal(change == "reload", state.Loaded); Assert.False(state.Loading);
    }

    [Fact]
    public async Task Actual_hierarchy_policy_and_configuration_store_reach_the_web_states()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        _ = CompanyAssetFacadeTests.Service(f); f.Rows.AddRange([new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" },
            new FACILITY { FACILITY_ID = "facility", FACILITY_TYPE = "gas", PRIMARY_FIELD_ID = "A", CURRENT_OPERATOR = "company", ACTIVE_IND = "Y" },
            new FACILITY { FACILITY_ID = "foreign", FACILITY_TYPE = "gas", PRIMARY_FIELD_ID = "B", CURRENT_OPERATOR = "other", ACTIVE_IND = "Y" }]);
        var store = new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>());
        var server = new AssetHierarchyService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, () => f.Gate.ResolveAsync(),
            subject => new CompanyPPDMRowAccessPolicy(f.Gate, store, f.Metadata.Object, f.Eligibility, f.Editor.Object, f.Columns.Object, f.Defaults.Object, Microsoft.Extensions.Options.Options.Create(f.EligibilityConfiguration), subject), subject => f.Gate.ResolveHierarchyCompanyAsync(subject), new RepositoryHierarchyConfigurationStore(f.Db, f.Gate), order => CompanyWorkOrderFieldReview.ComponentsAsync(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Eligibility, order));
        using var company = new CompanyClientContext(); Select(company, f.Gate.Company.Scope);
        using var tree = new AssetHierarchyState(company, (path, _) => server.GetAssetHierarchyForUserAsync("actor", "company"), Explain);
        await tree.LoadAsync(); var field = Assert.Single(tree.Hierarchy!.Children); var facility = Assert.Single(field.Children);
        Assert.Equal("facility", facility.AssetId); Assert.Equal("gas", facility.FacilityType); Assert.True(tree.CanSelect(facility)); Assert.Null(tree.Error);
        using var configuration = new HierarchyConfigurationState(company, async (_, _) => await server.GetHierarchyConfigAsync("company"), Explain, (_, _, _) => throw new InvalidOperationException("Unexpected configuration save"));
        await configuration.LoadAsync(); Assert.True(configuration.Loaded); Assert.Empty(configuration.Configuration); Assert.Null(configuration.Error); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task State_route_uses_actual_company_transport_headers_and_scoped_response_guard()
    {
        using var company = new CompanyClientContext(); Select(company); var seen = new List<string>();
        using var http = new HttpClient(new Handler(request =>
        {
            Assert.Equal("tenant", request.Headers.GetValues(CompanyRequestHeaders.Tenant).Single());
            Assert.Equal("company", request.Headers.GetValues(CompanyRequestHeaders.Company).Single()); seen.Add(request.RequestUri!.PathAndQuery);
            return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(Root(Field()))) };
        })) { BaseAddress = new("https://api.test") };
        using var state = new AssetHierarchyState(company, async (path, token) =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path); company.Prepare(request);
            using var response = await company.SendAsync(http, request, token);
            var result = JsonSerializer.Deserialize<AssetHierarchyNode>(await response.Content.ReadAsStringAsync(token)); company.EnsureCurrent(response); return result;
        }, Explain);
        await state.LoadAsync(); Assert.Single(state.Hierarchy!.Children); Assert.Equal("/api/AssetHierarchy/user/actor?organizationId=company", Assert.Single(seen)); Assert.Empty(http.DefaultRequestHeaders);
    }

    private static List<HierarchyConfig> ConfigRows(string? revision = null, string id = "company") =>
        new[] { "FIELD", "WELL", "POOL", "FACILITY" }.Select(type => new HierarchyConfig
        {
            OrganizationId = id, AssetType = type, HierarchyLevel = type == "FIELD" ? 0 : 1,
            ParentLevel = type == "FIELD" ? null : 0, LevelName = type, Active = true, Revision = revision
        }).ToList();

    [Fact]
    public async Task Empty_configuration_requires_explicit_draft_and_save_reloads_the_server_revision()
    {
        using var company = new CompanyClientContext(); Select(company); var reads = 0; var writes = 0;
        var newRevision = Guid.NewGuid().ToString("D");
        List<HierarchyConfig>? saved=null;
        using var state = new HierarchyConfigurationState(company, (_, _) => Task.FromResult<List<HierarchyConfig>?>(++reads == 1 ? [] : saved), Explain,
            (path, rows, _) =>
            {
                writes++; Assert.Equal("/api/AssetHierarchy/organization/company/config", path);
                Assert.All(rows, row => { Assert.Null(row.Revision); Assert.Equal("company", row.OrganizationId); });
                Assert.Equal("حقول", rows.Single(row => row.AssetType == "FIELD").LevelName);
                saved=rows.Select(row=>new HierarchyConfig{OrganizationId=row.OrganizationId,AssetType=row.AssetType,HierarchyLevel=row.HierarchyLevel,
                    ParentLevel=row.ParentLevel,LevelName=row.LevelName,Active=row.Active,Revision=newRevision}).ToList();
                return Task.FromResult(true);
            });
        state.BeginDraft(); Assert.Empty(state.Configuration);
        await state.LoadAsync(); Assert.False(state.CanSave); state.BeginDraft(); Assert.True(state.CanSave); Assert.Equal(0, writes);
        state.SetDisplayName(state.Configuration.Single(row => row.AssetType == "FIELD"), "حقول");
        await state.SaveAsync(); Assert.Equal(1, writes); Assert.Equal(2, reads); Assert.NotNull(state.Confirmation);
        Assert.All(state.Configuration, row => Assert.Equal(newRevision, row.Revision)); Assert.False(state.Saving); Assert.True(state.CanSave);
        state.SetDisplayName(state.Configuration.Single(row => row.AssetType == "FIELD"), "Fields revised"); Assert.Null(state.Confirmation); Assert.Equal(1, writes);
    }

    [Fact]
    public async Task Save_uses_confirmed_revision_and_company_even_if_mutable_dto_identity_is_changed()
    {
        using var company = new CompanyClientContext(); Select(company); var oldRevision = Guid.NewGuid().ToString("D"); var nextRevision = Guid.NewGuid().ToString("D"); var reads = 0;
        using var state = new HierarchyConfigurationState(company, (_, _) => Task.FromResult<List<HierarchyConfig>?>(ConfigRows(++reads == 1 ? oldRevision : nextRevision)), Explain,
            (_, rows, _) => { Assert.All(rows, row => { Assert.Equal(oldRevision, row.Revision); Assert.Equal("company", row.OrganizationId); }); return Task.FromResult(true); });
        await state.LoadAsync(); foreach (var row in state.Configuration) { row.Revision = Guid.NewGuid().ToString("D"); row.OrganizationId = "foreign"; }
        await state.SaveAsync(); Assert.NotNull(state.Confirmation); Assert.All(state.Configuration, row => Assert.Equal(nextRevision, row.Revision));
    }

    [Theory]
    [InlineData("blank")] [InlineData("long")] [InlineData("control")]
    public async Task Invalid_display_names_remain_editable_without_sending_a_write(string reason)
    {
        using var company = new CompanyClientContext(); Select(company); var writes = 0;
        using var state = new HierarchyConfigurationState(company, (_, _) => Task.FromResult<List<HierarchyConfig>?>(ConfigRows(Guid.NewGuid().ToString("D"))), Explain,
            (_, _, _) => { writes++; return Task.FromResult(true); });
        await state.LoadAsync(); state.Configuration[0].LevelName = reason == "blank" ? " " : reason == "long" ? new string('a', 129) : "bad\nname";
        await state.SaveAsync(); Assert.Equal(0, writes); Assert.NotNull(state.Error); Assert.True(state.CanSave); Assert.False(state.Saving); Assert.Null(state.Confirmation);
    }

    [Fact]
    public async Task Hiding_fields_hides_children_and_children_cannot_be_enabled_without_fields()
    {
        using var company = new CompanyClientContext(); Select(company);
        using var state = new HierarchyConfigurationState(company, (_, _) => Task.FromResult<List<HierarchyConfig>?>(ConfigRows(Guid.NewGuid().ToString("D"))), Explain,
            (_, _, _) => throw new InvalidOperationException());
        await state.LoadAsync(); var field = state.Configuration.Single(row => row.AssetType == "FIELD"); var well = state.Configuration.Single(row => row.AssetType == "WELL");
        state.SetVisible(field, false); Assert.All(state.Configuration, row => Assert.False(row.Active));
        state.SetVisible(well, true); Assert.False(well.Active); state.SetVisible(field, true); Assert.False(well.Active);
        state.SetVisible(well, true); Assert.True(well.Active); Assert.False(state.Configuration.Single(row => row.AssetType == "POOL").Active);
    }

    [Theory]
    [InlineData("false")] [InlineData("write-error")] [InlineData("refresh-error")] [InlineData("same-revision")] [InlineData("empty-refresh")] [InlineData("foreign-refresh")]
    public async Task Unconfirmed_write_or_readback_requires_refresh_before_any_further_save(string reason)
    {
        using var company = new CompanyClientContext(); Select(company); var revision = Guid.NewGuid().ToString("D"); var reads = 0; var writes = 0;
        using var state = new HierarchyConfigurationState(company, (_, _) =>
        {
            if (++reads == 1) return Task.FromResult<List<HierarchyConfig>?>(ConfigRows(revision));
            if (reason == "refresh-error") throw new HttpRequestException("internal detail");
            return Task.FromResult<List<HierarchyConfig>?>(reason == "empty-refresh" ? [] : ConfigRows(revision, reason == "foreign-refresh" ? "other" : "company"));
        }, Explain, (_, _, _) => { writes++; if (reason == "write-error") throw new HttpRequestException("internal detail"); return Task.FromResult(reason != "false"); });
        await state.LoadAsync(); await state.SaveAsync(); await state.SaveAsync();
        Assert.Equal(1, writes); Assert.False(state.Loaded); Assert.False(state.CanSave); Assert.Empty(state.Configuration); Assert.False(state.Saving);
        Assert.Null(state.Confirmation); Assert.Contains("Refresh before editing", state.Error); Assert.DoesNotContain("internal detail", state.Error);
    }

    [Theory]
    [InlineData("clear")] [InlineData("switch")] [InlineData("same-scope")] [InlineData("account")] [InlineData("dispose")] [InlineData("reload")]
    public async Task Inflight_save_is_single_and_late_result_cannot_confirm_a_changed_selection(string change)
    {
        using var company = new CompanyClientContext(); Select(company); var revision = Guid.NewGuid().ToString("D"); var writes = 0; var reads = 0; CancellationToken seen = default;
        var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var state = new HierarchyConfigurationState(company, (_, _) => { reads++; return Task.FromResult<List<HierarchyConfig>?>(ConfigRows(revision)); }, Explain,
            (_, _, token) => { writes++; seen = token; return result.Task; });
        await state.LoadAsync(); var save = state.SaveAsync(); Assert.True(state.Saving); Assert.False(state.CanSave); await state.SaveAsync(); Assert.Equal(1, writes);
        state.SetDisplayName(state.Configuration[0], "blocked edit"); Assert.NotEqual("blocked edit", state.Configuration[0].LevelName);
        if (change == "clear") company.Clear(); else if (change == "dispose") state.Dispose(); else if (change == "reload") await state.LoadAsync();
        else Select(company, change == "switch" ? B : A, change == "account" ? "other-user" : "actor");
        Assert.True(seen.IsCancellationRequested); result.SetResult(true); await save;
        Assert.Null(state.Confirmation); Assert.Null(state.Error); Assert.False(state.Saving); Assert.Equal(change == "reload", state.Loaded); Assert.Equal(change == "reload" ? 2 : 1, reads);
    }

    [Theory]
    [InlineData("revision")] [InlineData("types")] [InlineData("control")] [InlineData("hidden-parent")]
    public async Task Malformed_persisted_configuration_is_not_an_editable_confirmation(string reason)
    {
        using var company = new CompanyClientContext(); Select(company); var rows = ConfigRows(Guid.NewGuid().ToString("D"));
        if (reason == "revision") rows[1].Revision = Guid.NewGuid().ToString("D");
        if (reason == "types") rows[1].AssetType = "BLOCK";
        if (reason == "control") rows[1].LevelName = "bad\nname";
        if (reason == "hidden-parent") rows[0].Active = false;
        using var state = new HierarchyConfigurationState(company, (_, _) => Task.FromResult<List<HierarchyConfig>?>(rows), Explain, (_, _, _) => throw new InvalidOperationException());
        await state.LoadAsync(); Assert.False(state.Loaded); Assert.False(state.CanSave); Assert.NotNull(state.Error); Assert.Empty(state.Configuration);
    }

    [Fact]
    public async Task Actual_configuration_editor_persists_then_reads_the_canonical_store_and_hierarchy()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true); _ = CompanyAssetFacadeTests.Service(f);
        f.Rows.AddRange([new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" }, new POOL { POOL_ID = "pool", FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" }]);
        var store = new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>());
        var server = new AssetHierarchyService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, () => f.Gate.ResolveAsync(),
            subject => new CompanyPPDMRowAccessPolicy(f.Gate, store, f.Metadata.Object, f.Eligibility, f.Editor.Object, f.Columns.Object, f.Defaults.Object, Microsoft.Extensions.Options.Options.Create(f.EligibilityConfiguration), subject), subject => f.Gate.ResolveHierarchyCompanyAsync(subject), new RepositoryHierarchyConfigurationStore(f.Db, f.Gate), order => CompanyWorkOrderFieldReview.ComponentsAsync(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Eligibility, order));
        using var company = new CompanyClientContext(); Select(company, f.Gate.Company.Scope);
        using var state = new HierarchyConfigurationState(company, async (_, _) => await server.GetHierarchyConfigAsync("company"), Explain,
            (_, rows, _) => server.UpdateHierarchyConfigAsync("company", rows));
        await state.LoadAsync(); state.BeginDraft(); state.Configuration.Single(row => row.AssetType == "FIELD").LevelName = "حقول";
        state.SetVisible(state.Configuration.Single(row => row.AssetType == "POOL"), false); await state.SaveAsync();
        Assert.NotNull(state.Confirmation); Assert.All(state.Configuration, row => Assert.True(Guid.TryParseExact(row.Revision, "D", out _)));
        var root = await server.GetAssetHierarchyForUserAsync("actor", "company"); var field = Assert.Single(root!.Children);
        Assert.Equal("حقول", field.LevelName); Assert.Empty(field.Children);
        Assert.True(await server.ValidateAccessAsync("actor", [new() { AssetId = "pool", AssetType = "POOL" }])); Assert.Empty(f.Writes);
    }

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => Task.FromResult(send(request));
    }
}
