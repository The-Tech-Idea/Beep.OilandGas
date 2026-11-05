using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Reflection;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Development;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.CompressorAnalysis.Data;

using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

// Complete actual phase/mapping/profile/policy sources; provider records are an explicit boundary.
public sealed class CompanyDevelopmentBoundaryTests
{
    private static RepositoryUserProfileService Profile(Fixture f)
    { f.Http.HttpContext = f.Request; return new(f.Db, new RepositoryApplicationAuthorizationReader(f.Gate), new UpperInvariantLookupNormalizer(), f.Account, f.Gate, f.Service); }
    private static PPDMDevelopmentService Phase(Fixture f, ICompanyFieldSelectionService profile) => new(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, new PPDMMappingService(), profile, f.Policy);
    private static async Task Save(RepositoryUserProfileService profile) => await profile.SetCurrentFieldSelectionAsync(new() { FieldId = "A", Context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext });
    private static async Task<Fixture> Ready()
    {
        var f = await Fixture.Create(); await f.Grant("FIELD", "A"); await f.Grant("FIELD", "B");
        f.Rows.AddRange([new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" }, new FIELD { FIELD_ID = "B", ACTIVE_IND = "Y" }]); return f;
    }
    private static MethodInfo[] Operations => typeof(PPDMDevelopmentService).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
    public static IEnumerable<object[]> FieldOperations => Operations.Where(m => m.GetParameters().Any(p => p.Name == "fieldId")).Select(m => new object[] { m.Name });
    public static IEnumerable<object[]> ActorOperations => Operations.Where(m => m.GetParameters().Any(p => p.Name == "userId")).Select(m => new object[] { m.Name });
    public static IEnumerable<object[]> RequiredActorOperations => Operations.Where(m => m.GetParameters().Any(p => p.Name == "userId" && !p.HasDefaultValue)).Select(m => new object[] { m.Name });
    private static Task Invoke(PPDMDevelopmentService service, string name, string field = "A", string? actor = "actor")
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
    [Theory] [InlineData("POOL")] [InlineData("WELL")] [InlineData("FACILITY")]
    public async Task Actual_FIELD_asset_reads_keep_only_selected_FIELD_and_company_records(string table)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile);
        await f.Grant("POOL", "pool"); await f.Grant("POOL", "other-pool");
        await f.Grant("WELL", "well"); await f.Grant("WELL", "other-well");
        await f.Grant("FACILITY", "facility", facilityType: "PLANT"); await f.Grant("FACILITY", "other-facility", facilityType: "PLANT");
        f.Rows.AddRange([new POOL { POOL_ID = "pool", FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = "company" }, new POOL { POOL_ID = "other-pool", FIELD_ID = "B", BUSINESS_ASSOCIATE_ID = "company" },
            new WELL { UWI = "well", ASSIGNED_FIELD = "A", OPERATOR = "company", CURRENT_CLASS = "DEVELOPMENT", ACTIVE_IND = "Y" }, new WELL { UWI = "other-well", ASSIGNED_FIELD = "B", OPERATOR = "company", CURRENT_CLASS = "DEVELOPMENT", ACTIVE_IND = "Y" },
            new FACILITY { FACILITY_ID = "facility", FACILITY_TYPE = "PLANT", PRIMARY_FIELD_ID = "A", CURRENT_OPERATOR = "company", ACTIVE_IND = "Y" }, new FACILITY { FACILITY_ID = "other-facility", FACILITY_TYPE = "PLANT", PRIMARY_FIELD_ID = "B", CURRENT_OPERATOR = "company", ACTIVE_IND = "Y" }]);
        var phase = Phase(f, profile);
        if (table == "POOL") Assert.Equal("pool", Assert.Single(await phase.GetPoolsForFieldAsync("A")).POOL_ID);
        else if (table == "WELL") Assert.Equal("well", Assert.Single(await phase.GetDevelopmentWellsForFieldAsync("A")).UWI);
        else Assert.Equal("facility", Assert.Single(await phase.GetFacilitiesForFieldAsync("A")).FACILITY_ID);
        Assert.All(f.Connections, c => Assert.Equal("selected-tenant", c)); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task ActualDevelopmentFacilityListShowsPartnerOperatorOnlyThroughInheritedSelectedField()
    {
        using var f=await Ready();await f.Grant("FIELD","A","READ",inherit:true);var profile=Profile(f);await Save(profile);
        f.Rows.AddRange([new FACILITY{FACILITY_ID="shared",FACILITY_TYPE="gas",PRIMARY_FIELD_ID="A",CURRENT_OPERATOR="partner",ACTIVE_IND="Y"},new FACILITY{FACILITY_ID="shared",FACILITY_TYPE="oil",PRIMARY_FIELD_ID="B",CURRENT_OPERATOR="partner",ACTIVE_IND="Y"}]);
        var rows=await Phase(f,profile).GetFacilitiesForFieldAsync("A");var visible=Assert.Single(rows);Assert.Equal("gas",visible.FACILITY_TYPE);Assert.Equal("partner",visible.CURRENT_OPERATOR);Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task No_FIELD_preference_cannot_become_an_implicit_development_request()
    {
        using var f = await Ready(); var profile = Profile(f);
        await Assert.ThrowsAsync<RefusalException>(() => Phase(f, profile).GetDevelopmentWellsForFieldAsync("A")); Assert.Empty(f.Writes);
        f.Source.Verify(x => x.GetEntityAsync("WELL", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
    }
    [Fact]
    public async Task A_completed_read_cannot_adopt_a_later_profile_revision()
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); var phase = Phase(f, profile); await phase.GetPoolsForFieldAsync("A");
        await profile.UpdateUserPreferencesAsync("actor", "{\"density\":\"changed\"}");
        await Assert.ThrowsAsync<RefusalException>(() => phase.GetDevelopmentWellsForFieldAsync("A")); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Provider_callback_revision_change_cannot_return_success()
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); var phase = Phase(f, profile); await phase.GetPoolsForFieldAsync("A"); var changed = false;
        await f.Grant("WELL", "well");
        f.Source.Setup(x => x.GetEntityAsync("WELL", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).Returns(async () => { changed = true; await profile.UpdateUserPreferencesAsync("actor", "{\"density\":\"changed\"}"); return (IEnumerable<object>)Array.Empty<object>(); });
        await Assert.ThrowsAsync<RefusalException>(() => phase.GetDevelopmentWellsForFieldAsync("A")); Assert.True(changed); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("AssignRigToWellAsync")] [InlineData("AnalyzeGasLiftPotentialAsync")]
    public async Task A_WELL_in_another_FIELD_cannot_receive_a_rig_or_supply_analysis_inputs(string method)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); await f.Grant("WELL", "well", "WRITE");
        f.Rows.Add(new WELL { UWI = "well", ASSIGNED_FIELD = "B", OPERATOR = "company", ACTIVE_IND = "Y" });
        f.Source.Setup(x => x.GetEntityAsync("WELL", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((IEnumerable<object>)f.Rows.OfType<WELL>().Cast<object>().ToArray());
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Phase(f, profile), method)); Assert.Empty(f.Writes);
        f.Source.Verify(x => x.GetEntityAsync("WELL_TUBULAR", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Missing_or_ambiguous_facility_cannot_supply_compressor_inputs(bool ambiguous)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile);
        if (ambiguous) { foreach (var type in new[] { "PLANT", "COMPRESSOR" }) { await f.Grant("FACILITY", "well", facilityType: type); f.Rows.Add(new FACILITY { FACILITY_ID = "well", FACILITY_TYPE = type, PRIMARY_FIELD_ID = "A", CURRENT_OPERATOR = "company", ACTIVE_IND = "Y" }); } }
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Phase(f, profile), "AnalyzeCompressorPowerAsync")); Assert.Empty(f.Writes);
        f.Source.Verify(x => x.GetEntityAsync("FACILITY_EQUIPMENT", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
    }
    [Theory] [InlineData("GetPoolsForFieldAsync", "POOL")] [InlineData("GetDevelopmentWellsForFieldAsync", "WELL")] [InlineData("GetFacilitiesForFieldAsync", "FACILITY")] [InlineData("GetPipelinesForFieldAsync", "PIPELINE")]
    public async Task Missing_read_metadata_is_unavailable_not_empty_success(string method, string table)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); f.Metadata.Setup(x => x.GetTableMetadataAsync(table)).ReturnsAsync((Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata)null!);
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Phase(f, profile), method)); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task A_provider_ignoring_the_POOL_predicate_cannot_return_another_FIELD()
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); await f.Grant("POOL", "pool");
        var other = new POOL { POOL_ID = "pool", FIELD_ID = "B", BUSINESS_ASSOCIATE_ID = "company" };
        f.Source.Setup(x => x.GetEntityAsync("POOL", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((IEnumerable<object>)new object[] { other });
        await Assert.ThrowsAsync<RefusalException>(() => Phase(f, profile).GetPoolsForFieldAsync("A")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("AnalyzePipelineCapacityAsync")] [InlineData("AnalyzePipelineFlowAsync")]
    public async Task Missing_pipeline_ownership_policy_refuses_before_calculation(string method)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile);
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Phase(f, profile), method)); Assert.Empty(f.Writes);
        f.Source.Verify(x => x.GetEntityAsync("PIPELINE", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
    }
    [Fact]
    public async Task WELL_membership_change_between_validation_and_rig_lookup_cannot_mutate_another_FIELD()
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); await f.Grant("WELL", "well", "WRITE");
        var original = new WELL { UWI = "well", ASSIGNED_FIELD = "A", OPERATOR = "company", ACTIVE_IND = "Y" };
        var changed = new WELL { UWI = "well", ASSIGNED_FIELD = "B", OPERATOR = "company", ACTIVE_IND = "Y" };
        f.Source.SetupSequence(x => x.GetEntityAsync("WELL", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((IEnumerable<object>)new object[] { original }).ReturnsAsync((IEnumerable<object>)new object[] { changed });
        await Assert.ThrowsAsync<RefusalException>(() => Phase(f, profile).AssignRigToWellAsync("A", "well", "rig", "actor")); Assert.Empty(f.Writes);
        f.Source.Verify(x => x.GetEntityAsync("WELL", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Exactly(2)); Assert.Null(changed.STATUS_TYPE);
    }
    [Fact]
    public async Task Requested_POOL_identity_cannot_be_replaced_by_another_authorized_POOL()
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); await f.Grant("POOL", "other");
        var row = new POOL { POOL_ID = "other", FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = "company" };
        f.Source.Setup(x => x.GetEntityAsync("POOL", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((IEnumerable<object>)new object[] { row });
        await Assert.ThrowsAsync<RefusalException>(() => Phase(f, profile).GetPoolForFieldAsync("A", "requested")); Assert.Empty(f.Writes);
    }
    private static object[] StorageArguments(string name) => name switch {
        "StoreGasLiftPotentialResultsAsync" => ["well", new GAS_LIFT_WELL_PROPERTIES(), "actor"],
        "StoreGasLiftValveDesignAsync" => ["well", new GAS_LIFT_VALVE_DESIGN_RESULT(), "actor"],
        "StoreGasLiftValveSpacingAsync" => ["well", new GasLiftValveSpacingResult(), "EQUAL_DEPTH", "actor"],
        "StoreCompressorPowerResultsAsync" => ["facility", new COMPRESSOR_POWER_RESULT(), "CENTRIFUGAL", "actor"],
        _ => ["facility", new COMPRESSOR_PRESSURE_RESULT(), "actor"] };
    public static IEnumerable<object[]> EquipmentStores => new[] { "StoreGasLiftPotentialResultsAsync", "StoreGasLiftValveDesignAsync", "StoreGasLiftValveSpacingAsync", "StoreCompressorPowerResultsAsync", "StoreCompressorPressureResultsAsync" }.Select(name => new object[] { name, name.Contains("GasLift") ? "WELL_EQUIPMENT" : "FACILITY_EQUIPMENT" });
    private static Task Store(PPDMDevelopmentService phase, string name) => (Task)typeof(PPDMDevelopmentService).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(phase, StorageArguments(name))!;
    [Theory] [MemberData(nameof(EquipmentStores))]
    public async Task Missing_storage_metadata_cannot_return_a_saved_analysis(string method, string table)
    {
        using var f = await Ready(); var profile = Profile(f); f.Metadata.Setup(x => x.GetTableMetadataAsync(table)).ReturnsAsync((Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata)null!);
        await Assert.ThrowsAsync<RefusalException>(() => Store(Phase(f, profile), method)); Assert.Empty(f.Writes);
    }
    [Theory] [MemberData(nameof(EquipmentStores))]
    public async Task Actual_storage_failures_keep_the_original_error(string method, string table)
    {
        using var f = await Ready(); var profile = Profile(f); var error = new InvalidOperationException("recording metadata failure"); f.Metadata.Setup(x => x.GetTableMetadataAsync(table)).ThrowsAsync(error);
        Assert.Same(error, await Assert.ThrowsAsync<InvalidOperationException>(() => Store(Phase(f, profile), method))); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("StorePipelineCapacityResultsAsync")] [InlineData("StorePipelineFlowResultsAsync")]
    public async Task Pipeline_entity_validation_failure_cannot_be_suppressed_as_saved_success(string method)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); var phase = Phase(f, profile);
        object result = method.Contains("Capacity") ? new Beep.OilandGas.Models.Data.PipelineAnalysis.PIPELINE_CAPACITY_RESULT() : new Beep.OilandGas.Models.Data.PipelineAnalysis.PIPELINE_FLOW_ANALYSIS_RESULT();
        var actual = typeof(PPDMDevelopmentService).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!;
        var error = await Assert.ThrowsAsync<ArgumentException>(() => (Task)actual.Invoke(phase, ["pipeline", result, "GAS", "actor"])!); Assert.Equal("entity", error.ParamName); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("DesignWellAsync", "WELL_ACTIVITY")]
    public async Task Missing_plan_storage_cannot_return_an_unpersisted_plan(string method, string table)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); f.Metadata.Setup(x => x.GetTableMetadataAsync(table)).ReturnsAsync((Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata)null!);
        var phase = Phase(f, profile); Task command = phase.DesignWellAsync("A", "plan", new CreateWellPlan(), "actor");
        await Assert.ThrowsAsync<RefusalException>(() => command); Assert.Empty(f.Writes);
    }
}
