using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyFieldSelectionTests
{
    private static RepositoryUserProfileService Profile(Fixture f, IPPDM39DataService? fields = null, Beep.OilandGas.Repository.RepositoryDbContext? context = null)
    {
        f.Http.HttpContext = f.Request;
        return new(context ?? f.Db, new RepositoryApplicationAuthorizationReader(f.Gate),
            new UpperInvariantLookupNormalizer(), f.Account, f.Gate, fields ?? f.Service);
    }
    private static async Task<Fixture> Ready()
    {
        var f = await Fixture.Create(); await f.Grant("FIELD", "A"); await f.Grant("FIELD", "B");
        f.Rows.AddRange([new FIELD { FIELD_ID = "A", FIELD_NAME = "Name<recorded>", ACTIVE_IND = "Y" }, new FIELD { FIELD_ID = "B", FIELD_NAME = "B", ACTIVE_IND = "Y" },
            new FIELD { FIELD_ID = "foreign", ACTIVE_IND = "Y" }]); f.Queries.Clear(); return f;
    }
    private static async Task<SetActiveFieldRequest> Intent(RepositoryUserProfileService service, string id = "A") => new() { FieldId = id, Context = (await service.GetCurrentFieldSelectionAsync()).SelectionContext };

    [Fact]
    public async Task First_selection_has_a_reviewed_empty_state_and_only_current_granted_FIELDs_are_listed()
    {
        using var f = await Ready(); var service = Profile(f);
        var current = await service.GetCurrentFieldSelectionAsync(); Assert.NotNull(current.SelectionContext); Assert.Null(current.SelectionContext.FieldId);
        Assert.False(current.SelectionAvailable); Assert.Equal("", current.FieldId); Assert.Empty(await f.Db.Set<AppUserExtension>().ToArrayAsync());
        var rows = await service.GetAvailableFieldsAsync(); Assert.Equal(new[] { "A", "B" }, rows.Select(row => row.FieldId)); Assert.Equal("Name<recorded>", rows[0].FieldName);
        Assert.DoesNotContain(rows, row => row.FieldId == "foreign"); Assert.All(f.Connections, name => Assert.Equal("selected-tenant", name)); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task A_saved_selection_survives_a_fresh_service_and_keeps_unrelated_preferences_and_the_actual_actor()
    {
        using var f = await Ready(); var service = Profile(f); await service.UpdateUserPreferencesAsync("actor", "{\"density\":\"compact\"}");
        var intent = await Intent(service); var receipt = await service.SetCurrentFieldSelectionAsync(intent);
        Assert.True(receipt.Success); Assert.Equal("A", receipt.Context!.FieldId); Assert.NotEqual(intent.Context!.Revision, receipt.Context.Revision);
        var current = await Profile(f).GetCurrentFieldSelectionAsync(); Assert.True(current.SelectionAvailable); Assert.Equal(receipt.Context, current.SelectionContext); Assert.Equal("Name<recorded>", current.FieldName);
        var metadata = await f.Db.Set<AppUserExtension>().AsNoTracking().SingleAsync(); Assert.Equal("actor", metadata.ChangedBy);
        var json = JsonNode.Parse(metadata.PreferencesJson!)!.AsObject(); Assert.Equal("compact", json["density"]!.GetValue<string>());
        Assert.Equal(receipt.Context.Revision, (await f.Db.Users.AsNoTracking().SingleAsync()).ConcurrencyStamp); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("actor")] [InlineData("company")] [InlineData("database")] [InlineData("revision")] [InlineData("current-field")]
    [InlineData("missing-context")] [InlineData("connection")] [InlineData("field")]
    public async Task Stale_or_forged_selection_assertions_cannot_change_the_profile(string fault)
    {
        using var f = await Ready(); var service = Profile(f); var intent = await Intent(service); var review = intent.Context!;
        intent.Context = fault switch { "actor" => review with { ActorUserId = "other" }, "company" => review with { Company = new("other", "company") },
            "database" => review with { Database = new("other", review.Database.Fingerprint) }, "revision" => review with { Revision = "stale" },
            "current-field" => review with { FieldId = "B" }, "missing-context" => null, _ => review };
        if (fault == "connection") intent.ConnectionName = "PPDM39"; if (fault == "field") intent.FieldId = "foreign";
        await Assert.ThrowsAsync<RefusalException>(() => service.SetCurrentFieldSelectionAsync(intent)); Assert.Empty(await f.Db.Set<AppUserExtension>().ToArrayAsync()); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task A_saved_preference_does_not_confer_revoked_FIELD_authority_and_an_available_replacement_can_be_selected()
    {
        using var f = await Ready(); var service = Profile(f); await service.SetCurrentFieldSelectionAsync(await Intent(service));
        var grant = await f.Db.Set<AppUserAssetAccess>().SingleAsync(x => x.AssetId == "A"); grant.IsActive = false; await f.Db.SaveChangesAsync();
        var current = await Profile(f).GetCurrentFieldSelectionAsync(); Assert.False(current.SelectionAvailable); Assert.Equal("", current.FieldId);
        Assert.Equal("A", current.SelectionContext!.FieldId); Assert.Null(current.Field); Assert.Equal("B", Assert.Single(await Profile(f).GetAvailableFieldsAsync()).FieldId);
        Assert.Equal("B", (await Profile(f).SetCurrentFieldSelectionAsync(new() { FieldId = "B", Context = current.SelectionContext })).FieldId);
    }
    [Fact]
    public async Task General_profile_changes_preserve_server_owned_selections_and_invalidate_old_selection_reviews()
    {
        using var f = await Ready(); var service = Profile(f); await service.SetCurrentFieldSelectionAsync(await Intent(service)); var old = await Intent(service, "B");
        await service.UpdateUserPreferencesAsync("actor", "{\"density\":\"comfortable\"}");
        Assert.Equal("A", (await service.GetCurrentFieldSelectionAsync()).FieldId);
        await Assert.ThrowsAsync<RefusalException>(() => service.SetCurrentFieldSelectionAsync(old));
        var before = (await f.Db.Set<AppUserExtension>().AsNoTracking().SingleAsync()).PreferencesJson;
        await Assert.ThrowsAsync<RefusalException>(() => service.UpdateUserPreferencesAsync("actor", "{\"OilGasFieldSelections\":[]}"));
        Assert.Equal(before, (await f.Db.Set<AppUserExtension>().AsNoTracking().SingleAsync()).PreferencesJson);
    }
    [Theory] [InlineData("grant")] [InlineData("entitlement")] [InlineData("inactive-field")] [InlineData("account")]
    public async Task Live_authority_is_required_at_selection_save(string fault)
    {
        using var f = await Ready(); var service = Profile(f); var intent = await Intent(service);
        if (fault == "grant") (await f.Db.Set<AppUserAssetAccess>().SingleAsync(x => x.AssetId == "A")).IsActive = false;
        if (fault == "entitlement") f.Rows.OfType<ENT_COMPONENT>().Single(row => row.FIELD_ID == "A").ACTIVE_IND = "N";
        if (fault == "inactive-field") f.Rows.OfType<FIELD>().Single(row => row.FIELD_ID == "A").ACTIVE_IND = "N";
        if (fault == "account") (await f.Db.Users.SingleAsync()).IsActive = false;
        await f.Db.SaveChangesAsync(); await Assert.ThrowsAsync<RefusalException>(() => service.SetCurrentFieldSelectionAsync(intent)); Assert.Empty(await f.Db.Set<AppUserExtension>().ToArrayAsync());
    }
    [Theory] [InlineData("null")] [InlineData("failed")] [InlineData("count")] [InlineData("duplicate")] [InlineData("identity")] [InlineData("inactive")]
    public async Task Malformed_catalogs_are_not_confirmed_empty_lists(string fault)
    {
        using var f = await Ready(); var fields = new Mock<IPPDM39DataService>();
        var result = new GetEntitiesResponse { Success = true, Count = 1, Entities = [new() { ["FIELD_ID"] = "A", ["ACTIVE_IND"] = "Y" }] };
        if (fault == "failed") result.Success = false; if (fault == "count") result.Count = 0;
        if (fault == "duplicate") { result.Entities.Add(result.Entities[0]); result.Count = 2; }
        if (fault == "identity") result.Entities[0]["FIELD_ID"] = " A"; if (fault == "inactive") result.Entities[0]["ACTIVE_IND"] = "N";
        fields.Setup(x => x.GetEntitiesAsync("FIELD", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>(), "selected-tenant")).ReturnsAsync(fault == "null" ? null! : result);
        await Assert.ThrowsAsync<RefusalException>(() => Profile(f, fields.Object).GetAvailableFieldsAsync()); Assert.Empty(await f.Db.Set<AppUserExtension>().ToArrayAsync());
    }
    [Fact]
    public async Task A_saved_selection_retains_its_database_scope_and_refuses_reusing_an_old_review()
    {
        using var f = await Ready(); var service = Profile(f); var intent = await Intent(service); await service.SetCurrentFieldSelectionAsync(intent);
        var stored = (await f.Db.Set<AppUserExtension>().SingleAsync()).PreferencesJson;
        var selection = JsonSerializer.Deserialize<List<FieldSelectionReview>>(JsonNode.Parse(stored!)!["OilGasFieldSelections"]!.ToJsonString(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!.Single();
        Assert.Equal((await f.Gate.ResolveAsync()).Fingerprint, selection.Database.Fingerprint);
        await Assert.ThrowsAsync<RefusalException>(() => Profile(f).SetCurrentFieldSelectionAsync(intent)); Assert.Equal(stored, (await f.Db.Set<AppUserExtension>().AsNoTracking().SingleAsync()).PreferencesJson);
    }

    private static CompanyPPDMRowAccessTests.Context Fresh(Fixture f) => new(new DbContextOptionsBuilder<CompanyPPDMRowAccessTests.Context>().UseSqlite(f.Db.Database.GetDbConnection()).Options);

    [Fact]
    public async Task A_saved_selection_is_materialized_by_a_fresh_registry_context()
    {
        using var f = await Ready(); var owner = Profile(f); var receipt = await owner.SetCurrentFieldSelectionAsync(await Intent(owner));
        using var fresh = Fresh(f); var current = await Profile(f, context: fresh).GetCurrentFieldSelectionAsync();
        Assert.True(current.SelectionAvailable); Assert.Equal(receipt.Context, current.SelectionContext); Assert.Equal("A", current.FieldId);
    }

    [Fact]
    public async Task Concurrent_general_preferences_win_without_a_partial_FIELD_save()
    {
        using var f = await Ready(); var owner = Profile(f); await owner.UpdateUserPreferencesAsync("actor", "{\"density\":\"before\"}");
        var intent = await Intent(owner); using var other = Fresh(f); var competing = Profile(f, context: other); var called = false;
        f.AfterRead = async () => { if (called) return; called = true; await competing.UpdateUserPreferencesAsync("actor", "{\"density\":\"concurrent\"}"); };
        var error = await Assert.ThrowsAsync<RefusalException>(() => owner.SetCurrentFieldSelectionAsync(intent)); Assert.Equal(RefusalKind.Conflict, error.Kind);
        using var readback = Fresh(f); var root = JsonNode.Parse((await readback.Set<AppUserExtension>().SingleAsync()).PreferencesJson!)!.AsObject();
        Assert.Equal("concurrent", root["density"]!.GetValue<string>()); Assert.False(root.ContainsKey("OilGasFieldSelections"));
        Assert.NotEqual(intent.Context!.Revision, (await readback.Users.SingleAsync()).ConcurrencyStamp);
    }

    [Theory] [InlineData("array")] [InlineData("ambiguous")] [InlineData("capacity")]
    public async Task Existing_profile_structure_and_capacity_are_preserved_on_refusal(string fault)
    {
        using var f = await Ready(); var owner = Profile(f);
        var json = fault switch { "array" => "[]", "ambiguous" => "{\"density\":1,\"density\":2}", _ => "{\"retained\":\"" + new string('x', 3960) + "\"}" };
        f.Db.Add(new AppUserExtension { UserId = "actor", PreferencesJson = json }); await f.Db.SaveChangesAsync();
        if (fault == "capacity") { var intent = await Intent(owner); await Assert.ThrowsAsync<RefusalException>(() => owner.SetCurrentFieldSelectionAsync(intent)); }
        else await Assert.ThrowsAsync<RefusalException>(() => owner.GetCurrentFieldSelectionAsync());
        using var readback = Fresh(f); Assert.Equal(json, (await readback.Set<AppUserExtension>().SingleAsync()).PreferencesJson);
    }

    [Fact]
    public async Task Company_and_database_snapshot_changes_do_not_adopt_an_old_preference_or_review()
    {
        using var f = await Ready(); var owner = Profile(f); var receipt = await owner.SetCurrentFieldSelectionAsync(await Intent(owner));
        await f.Select("other-company"); await f.Grant("FIELD", "B"); var next = Profile(f); var empty = await next.GetCurrentFieldSelectionAsync();
        Assert.Null(empty.SelectionContext!.FieldId); Assert.False(empty.SelectionAvailable);
        await Assert.ThrowsAsync<RefusalException>(() => next.SetCurrentFieldSelectionAsync(new() { FieldId = "B", Context = receipt.Context }));
        await next.SetCurrentFieldSelectionAsync(new() { FieldId = "B", Context = empty.SelectionContext });
        // Model an actual binding review change, rather than incidentally generating another connection GUID.
        (await f.Db.ModuleDatabases.SingleAsync()).ConcurrencyStamp = "reviewed-database-revision-2";
        await f.Db.SaveChangesAsync();
        await f.Select("company"); var original = await Profile(f).GetCurrentFieldSelectionAsync();
        Assert.NotEqual(receipt.Context!.Database, original.SelectionContext!.Database);
        Assert.False(original.SelectionAvailable); Assert.Null(original.SelectionContext.FieldId); Assert.Equal("", original.FieldId);
        Assert.Equal(2, JsonNode.Parse((await f.Db.Set<AppUserExtension>().AsNoTracking().SingleAsync()).PreferencesJson!)!["OilGasFieldSelections"]!.AsArray().Count);
    }
    [Fact]
    public async Task Returning_to_a_company_on_the_same_reviewed_database_restores_only_its_own_FIELD_preference()
    {
        using var f = await Ready(); var owner = Profile(f); var first = await owner.SetCurrentFieldSelectionAsync(await Intent(owner));
        await f.Select("other-company"); await f.Grant("FIELD", "B"); var other = Profile(f); var otherReview = (await other.GetCurrentFieldSelectionAsync()).SelectionContext!;
        await other.SetCurrentFieldSelectionAsync(new() { FieldId = "B", Context = otherReview });
        await f.Select("company"); var original = await Profile(f).GetCurrentFieldSelectionAsync();
        Assert.Equal(first.Context!.Database, original.SelectionContext!.Database); Assert.True(original.SelectionAvailable); Assert.Equal("A", original.FieldId);
        await Assert.ThrowsAsync<RefusalException>(() => Profile(f).SetCurrentFieldSelectionAsync(new() { FieldId = "A", Context = otherReview }));
    }

}
