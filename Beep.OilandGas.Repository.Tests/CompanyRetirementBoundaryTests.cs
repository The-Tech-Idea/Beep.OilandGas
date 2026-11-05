using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Decommissioning;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

// Actual complete retirement service/mapping/profile/registry/row policy; recording Beep provider is explicit.
public sealed class CompanyRetirementBoundaryTests
{
    private static RepositoryUserProfileService Profile(Fixture f)
    { f.Http.HttpContext = f.Request; return new(f.Db, new RepositoryApplicationAuthorizationReader(f.Gate), new UpperInvariantLookupNormalizer(), f.Account, f.Gate, f.Service); }
    private static PPDMDecommissioningService Phase(Fixture f, ICompanyFieldSelectionService profile) =>
        new(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, new PPDMMappingService(), profile, f.Policy);
    private static async Task Save(RepositoryUserProfileService profile) => await profile.SetCurrentFieldSelectionAsync(new() { FieldId = "A", Context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext });
    private static async Task<Fixture> Ready()
    {
        var f = await Fixture.Create(); await f.Grant("FIELD", "A"); await f.Grant("FIELD", "B");
        f.Rows.AddRange([new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" }, new FIELD { FIELD_ID = "B", ACTIVE_IND = "Y" }]); return f;
    }
    private static Task Invoke(PPDMDecommissioningService service, string operation, string field = "A", string actor = "actor") => operation switch {
        "wells" => service.GetAbandonedWellsForFieldAsync(field), "well" => service.GetWellAbandonmentForFieldAsync(field, "record"),
        "facilities" => service.GetDecommissionedFacilitiesForFieldAsync(field), "facility" => service.GetFacilityDecommissioningForFieldAsync(field, "record"),
        "restorations" => service.GetEnvironmentalRestorationsForFieldAsync(field), "costs" => service.GetDecommissioningCostsForFieldAsync(field),
        "estimate" => service.EstimateCostsForFieldAsync(field), "abandon" => service.AbandonWellForFieldAsync(field, "well", new(), actor),
        "retire-facility" => service.DecommissionFacilityForFieldAsync(field, "facility", new(), actor),
        "create-restoration" => service.CreateEnvironmentalRestorationForFieldAsync(field, new(), actor),
        "plan" => service.PlanDecommissioningAsync(field, "well", new(), actor), "execute" => service.ExecuteDecommissioningAsync(field, "well", "record", [], actor),
        "verify" => service.VerifyDecommissioningAsync(field, "well", "record", actor, true, actor), _ => throw new ArgumentOutOfRangeException(nameof(operation)) };
    public static IEnumerable<object[]> FieldOperations => new[] { "wells", "well", "facilities", "facility", "restorations", "costs", "estimate", "abandon", "retire-facility", "create-restoration", "plan", "execute", "verify" }.Select(operation => new object[] { operation });
    [Theory] [MemberData(nameof(FieldOperations))]
    public async Task Every_actual_phase_operation_refuses_a_FIELD_different_from_the_reviewed_selection(string operation)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); var service = Phase(f, profile); f.Queries.Clear();
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(service, operation, "B"));
        Assert.Empty(f.Writes); Assert.All(f.Queries, filters => Assert.Contains(filters, filter => filter.FieldName == "FIELD_ID" && filter.FilterValue == "A"));
    }
    [Theory] [InlineData("abandon")] [InlineData("retire-facility")] [InlineData("create-restoration")] [InlineData("plan")] [InlineData("execute")] [InlineData("verify")]
    public async Task Every_mutation_requires_the_reviewed_application_actor(string operation)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); var service = Phase(f, profile); f.Queries.Clear();
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(service, operation, actor: "external-subject"));
        Assert.Empty(f.Writes); Assert.All(f.Queries, filters => Assert.Contains(filters, filter => filter.FieldName == "FIELD_ID" && filter.FilterValue == "A"));
    }
    [Theory] [InlineData("wells")] [InlineData("facilities")] [InlineData("restorations")] [InlineData("costs")]
    public async Task Missing_actual_module_metadata_is_unavailable_not_an_empty_or_alternate_portfolio(string operation)
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile);
        var refusal = await Assert.ThrowsAsync<RefusalException>(() => Invoke(Phase(f, profile), operation));
        Assert.Equal(RefusalKind.Conflict, refusal.Kind); Assert.Empty(f.Writes);
        f.Source.Verify(x => x.GetEntityAsync("FACILITY_STATUS", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
        f.Source.Verify(x => x.GetEntityAsync("WELL_ACTIVITY", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
    }
    [Fact]
    public async Task An_unselected_FIELD_cannot_be_inferred_from_the_route()
    {
        using var f = await Ready(); var profile = Profile(f);
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Phase(f, profile), "wells")); Assert.Empty(f.Queries); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task The_actual_WELL_prerequisite_uses_only_the_reviewed_database_and_required_row_policy()
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); await f.Grant("WELL", "well");
        f.Rows.Add(new WELL { UWI = "well", ASSIGNED_FIELD = "A", OPERATOR = "company" });
        f.Connections.Clear(); f.Queries.Clear();
        var failure = await Assert.ThrowsAsync<RefusalException>(() => Invoke(Phase(f, profile), "abandon"));
        Assert.Equal(RefusalKind.Conflict, failure.Kind); // The authoritative extension metadata is absent; no schema is guessed.
        Assert.All(f.Connections, connection => Assert.Equal("selected-tenant", connection));
        Assert.Contains(f.Queries, filters => filters.Any(filter => filter.FieldName == "UWI" && filter.FilterValue == "well") && filters.Any(filter => filter.FieldName == "OPERATOR" && filter.FilterValue == "company"));
        Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task A_provider_that_ignores_company_predicates_cannot_supply_a_retirement_WELL()
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); await f.Grant("WELL", "well");
        var foreign = new WELL { UWI = "well", ASSIGNED_FIELD = "A", OPERATOR = "foreign-company" };
        f.Source.Setup(x => x.GetEntityAsync("WELL", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new object[] { foreign });
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Phase(f, profile), "abandon")); Assert.Empty(f.Writes);
        f.Source.Verify(x => x.GetEntityAsync("WELL", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Once);
    }
    [Fact]
    public async Task A_request_cannot_adopt_a_new_profile_revision_after_its_first_phase_attempt()
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); var phase = Phase(f, profile);
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(phase, "wells"));
        await profile.UpdateUserPreferencesAsync("actor", "{\"density\":\"changed\"}");
        f.Queries.Clear(); await Assert.ThrowsAsync<RefusalException>(() => Invoke(phase, "abandon"));
        Assert.Empty(f.Writes); Assert.All(f.Queries, filters => Assert.Contains(filters, filter => filter.FieldName == "FIELD_ID"));
    }
    [Fact]
    public async Task Loss_of_FIELD_entitlement_is_refused_before_phase_provider_work()
    {
        using var f = await Ready(); var profile = Profile(f); await Save(profile); var phase = Phase(f, profile);
        f.Rows.OfType<ENT_COMPONENT>().Single(row => row.FIELD_ID == "A").ACTIVE_IND = "N";
        f.Queries.Clear(); await Assert.ThrowsAsync<RefusalException>(() => Invoke(phase, "abandon")); Assert.Empty(f.Writes);
        f.Source.Verify(x => x.GetEntityAsync("WELL", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
    }
}
