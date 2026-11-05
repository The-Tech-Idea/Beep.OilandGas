using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Reflection;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

// Complete actual phase/mapping/profile/policy sources; provider records are an explicit boundary.
public sealed partial class CompanyProductionBoundaryTests
{
    private static RepositoryUserProfileService Profile(Fixture f)
    { f.Http.HttpContext = f.Request; return new(f.Db, new RepositoryApplicationAuthorizationReader(f.Gate), new UpperInvariantLookupNormalizer(), f.Account, f.Gate, f.Service); }
    private static PPDMProductionService Phase(Fixture f, ICompanyFieldSelectionService profile) => new(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, new PPDMMappingService(), profile, f.Policy);
    private static async Task Save(RepositoryUserProfileService profile) => await profile.SetCurrentFieldSelectionAsync(new() { FieldId = "A", Context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext });
    private static async Task<Fixture> Ready()
    {
        var f = await Fixture.Create(); await f.Grant("FIELD", "A"); await f.Grant("FIELD", "B");
        f.Rows.AddRange([new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" }, new FIELD { FIELD_ID = "B", ACTIVE_IND = "Y" }]); return f;
    }
    private static MethodInfo[] Operations => typeof(PPDMProductionService).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
    public static IEnumerable<object[]> FieldOperations => Operations.Where(m => m.GetParameters().Any(p => p.Name == "fieldId")).Select(m => new object[] { m.Name });
    public static IEnumerable<object[]> ActorOperations => Operations.Where(m => m.GetParameters().Any(p => p.Name == "userId")).Select(m => new object[] { m.Name });
    public static IEnumerable<object[]> RequiredActorOperations => Operations.Where(m => m.GetParameters().Any(p => p.Name == "userId" && !p.HasDefaultValue)).Select(m => new object[] { m.Name });
    private static Task Invoke(PPDMProductionService service, string name, string field = "A", string? actor = "actor")
    {
        var method = Operations.Single(m => m.Name == name);
        var arguments = method.GetParameters().Select(p => p.Name == "fieldId" ? field : p.Name == "userId" ? actor : p.HasDefaultValue ? p.DefaultValue : p.ParameterType == typeof(string) ? "well" : p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null).ToArray();
        return (Task)method.Invoke(service, arguments)!;
    }
    [Theory] [MemberData(nameof(FieldOperations))]
    public async Task Every_FIELD_operation_requires_the_reviewed_selection(string operation)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); f.Queries.Clear();
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Phase(f, profile), operation, "B"));
        Assert.Empty(f.Writes); Assert.All(f.Queries, filters => Assert.Contains(filters, filter => filter.FieldName == "FIELD_ID" && filter.FilterValue == "A"));
    }
    [Theory] [MemberData(nameof(ActorOperations))]
    public async Task Supplied_actor_assertions_cannot_override_the_actual_profile_account(string operation)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); f.Queries.Clear();
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Phase(f, profile), operation, actor: "external-subject"));
        Assert.Empty(f.Writes); f.Source.Verify(x => x.GetEntityAsync("WELL", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
    }
    [Theory] [MemberData(nameof(RequiredActorOperations))]
    public async Task Required_mutation_actors_cannot_be_null(string operation)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile);
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Phase(f, profile), operation, actor: null)); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Company_FIELD_catalog_reads_do_not_invent_or_require_a_current_preference()
    {
        using var f = await Ready(); var profile = Profile(f); var phase = Phase(f, profile);
        var fields = await phase.GetFieldsAsync(); Assert.Equal(new[] { "A", "B" }, fields.Select(row => row.FIELD_ID).Order().ToArray());
        Assert.False((await profile.GetCurrentFieldSelectionAsync()).SelectionAvailable); Assert.Empty(f.Writes);
        Assert.All(f.Connections, connection => Assert.Equal("selected-tenant", connection));
    }
    [Fact]
    public async Task Company_POOL_reads_apply_actual_company_and_asset_predicates_without_a_FIELD_preference()
    {
        using var f = await Ready(); var profile = Profile(f); await f.Grant("POOL", "pool");
        f.Rows.AddRange([new POOL { POOL_ID = "pool", FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = "company" }, new POOL { POOL_ID = "foreign", FIELD_ID = "B", BUSINESS_ASSOCIATE_ID = "another" }]);
        var pools = await Phase(f, profile).GetPoolsAsync(); Assert.Equal("pool", Assert.Single(pools).POOL_ID);
        Assert.All(f.Connections, connection => Assert.Equal("selected-tenant", connection)); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task A_successful_catalog_read_cannot_keep_an_old_review_after_preference_revision_changes()
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); var phase = Phase(f, profile); await phase.GetFieldsAsync();
        await profile.UpdateUserPreferencesAsync("actor", "{\"density\":\"changed\"}");
        await Assert.ThrowsAsync<RefusalException>(() => phase.GetPoolsAsync()); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Caller_changes_during_the_actual_provider_read_are_refused_before_success_returns()
    {
        using var f = await Ready(); var profile = Profile(f); var phase = Phase(f, profile); // No selected FIELD avoids profile raw FIELD reads before the phase read.
        var changed = false;
        f.AfterRead = async () => { if (!changed) { changed = true; await profile.UpdateUserPreferencesAsync("actor", "{\"density\":\"changed\"}"); } };
        await Assert.ThrowsAsync<RefusalException>(() => phase.GetFieldsAsync()); Assert.True(changed); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task An_authorized_WELL_in_another_FIELD_cannot_supply_calculation_inputs()
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); await f.Grant("WELL", "well");
        f.Rows.Add(new WELL { UWI = "well", ASSIGNED_FIELD = "B", OPERATOR = "company", ACTIVE_IND = "Y" });
        await Assert.ThrowsAsync<RefusalException>(() => Phase(f, profile).AnalyzeChokeFlowAsync("A", "well")); Assert.Empty(f.Writes);
        f.Source.Verify(x => x.GetEntityAsync("WELL_EQUIPMENT", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
    }
    private static object[] StorageArguments(string name) => name switch {
        "StoreChokeFlowResultsAsync" => ["well", new CHOKE_FLOW_RESULT(), "UPHOLE", "actor"],
        "StoreChokeSizingResultsAsync" => ["well", new CHOKE_PROPERTIES(), 1m, "UPHOLE", "actor"],
        "StoreSuckerRodLoadResultsAsync" => ["well", new SUCKER_ROD_LOAD_RESULT(), "actor"],
        _ => ["well", new SUCKER_ROD_FLOW_RATE_POWER_RESULT(), "actor"] };
    private static Task Store(PPDMProductionService phase, string name) => (Task)typeof(PPDMProductionService).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(phase, StorageArguments(name))!;
    [Theory] [InlineData("StoreChokeFlowResultsAsync")] [InlineData("StoreChokeSizingResultsAsync")] [InlineData("StoreSuckerRodLoadResultsAsync")] [InlineData("StoreSuckerRodPowerResultsAsync")]
    public async Task Missing_storage_metadata_cannot_be_reported_as_a_saved_analysis(string method)
    {
        using var f = await Ready(); var profile = Profile(f); f.Metadata.Setup(x => x.GetTableMetadataAsync("WELL_EQUIPMENT")).ReturnsAsync((Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata)null!);
        await Assert.ThrowsAsync<RefusalException>(() => Store(Phase(f, profile), method)); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("StoreChokeFlowResultsAsync")] [InlineData("StoreChokeSizingResultsAsync")] [InlineData("StoreSuckerRodLoadResultsAsync")] [InlineData("StoreSuckerRodPowerResultsAsync")]
    public async Task Actual_storage_failures_propagate_instead_of_returning_unsaved_success(string method)
    {
        using var f = await Ready(); var profile = Profile(f); var failure = new InvalidOperationException("recording metadata failure");
        f.Metadata.Setup(x => x.GetTableMetadataAsync("WELL_EQUIPMENT")).ThrowsAsync(failure);
        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(() => Store(Phase(f, profile), method))); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Equipment_read_failure_cannot_be_replaced_with_a_uniform_rod_string()
    {
        using var f = await Ready(); var profile = Profile(f); var failure = new InvalidOperationException("recording metadata failure");
        f.Metadata.Setup(x => x.GetTableMetadataAsync("WELL_EQUIPMENT")).ThrowsAsync(failure);
        var phase = Phase(f, profile); var method = typeof(PPDMProductionService).GetMethod("GetSuckerRodStringAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(() => (Task)method.Invoke(phase, ["well", new SUCKER_ROD_SYSTEM_PROPERTIES()])!));
        Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("OptimizeProductionAsync",false)] [InlineData("OptimizeProductionAsync",true)]
    [InlineData("PlanEnhancedRecoveryAsync",false)] [InlineData("PlanEnhancedRecoveryAsync",true)]
    [InlineData("ExecuteEnhancedRecoveryAsync",false)] [InlineData("ExecuteEnhancedRecoveryAsync",true)]
    public async Task LegacyProductionEventsCannotWriteFacilityStatusOrReturnUnsavedSuccess(string operation,bool missingMetadata)
    {
        using var f=await Ready();var profile=Profile(f);await Save(profile);
        if(missingMetadata)f.Metadata.Setup(value=>value.GetTableMetadataAsync("FACILITY_STATUS")).ReturnsAsync((Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata)null!);
        var phase=Phase(f,profile);f.Metadata.Invocations.Clear();f.Writes.Clear();
        var refusal=await Assert.ThrowsAsync<RefusalException>(()=>Invoke(phase,operation));
        Assert.Contains("facility operating statuses cannot be used as event records",refusal.Message);
        f.Metadata.Verify(value=>value.GetTableMetadataAsync("FACILITY_STATUS"),Times.Never);
        f.Source.Verify(value=>value.GetEntityAsync("FACILITY_STATUS",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()),Times.Never);
        Assert.Empty(f.Writes);
    }
}
