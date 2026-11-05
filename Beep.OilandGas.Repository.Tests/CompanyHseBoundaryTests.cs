using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using System.Reflection;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.HSE;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

// Complete actual HSE implementations, profile and policy; Beep provider and explicitly mocked incident input are named boundaries.
public sealed class CompanyHseBoundaryTests
{
    private static readonly Type[] Owners = [typeof(HSEIncidentService), typeof(RCAService), typeof(BarrierManagementService), typeof(CorrectiveActionService), typeof(HAZOPService), typeof(HSEKPIService), typeof(PPDMHSEService)];
    private static RepositoryUserProfileService Profile(Fixture f)
    { f.Http.HttpContext = f.Request; return new(f.Db, new RepositoryApplicationAuthorizationReader(f.Gate), new UpperInvariantLookupNormalizer(), f.Account, f.Gate, f.Service); }
    private static HSEIncidentService Incident(Fixture f, ICompanyFieldSelectionService profile) => new(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, profile, f.Policy, NullLogger<HSEIncidentService>.Instance);
    private static object Owner(string name, Fixture f, ICompanyFieldSelectionService profile, string field = "A", IHSEService? incidents = null) => name switch
    {
        nameof(HSEIncidentService) => Incident(f, profile),
        nameof(RCAService) => new RCAService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, profile, f.Policy, incidents ?? Incident(f, profile), NullLogger<RCAService>.Instance),
        nameof(BarrierManagementService) => new BarrierManagementService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, profile, f.Policy, incidents ?? Incident(f, profile), NullLogger<BarrierManagementService>.Instance),
        nameof(CorrectiveActionService) => new CorrectiveActionService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, profile, f.Policy, incidents ?? Incident(f, profile), new PPDMCompanyCorrectiveActionCommands(f.Gate, profile, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy), NullLogger<CorrectiveActionService>.Instance),
        nameof(HAZOPService) => new HAZOPService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, profile, f.Policy, NullLogger<HAZOPService>.Instance),
        nameof(HSEKPIService) => new HSEKPIService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, profile, f.Policy, NullLogger<HSEKPIService>.Instance),
        _ => new PPDMHSEService(field, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, profile, f.Policy, new PPDMCompanyCorrectiveActionCommands(f.Gate, profile, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy))
    };
    private static IEnumerable<MethodInfo> Operations => Owners.SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    public static IEnumerable<object[]> AllOperations => Operations.Select(m => new object[] { m.DeclaringType!.Name, m.Name });
    public static IEnumerable<object[]> ActorOperations => Operations.Where(m => m.GetParameters().Any(p => p.Name == "userId")).Select(m => new object[] { m.DeclaringType!.Name, m.Name });
    public static IEnumerable<object[]> ExplicitFieldOperations => Operations.Where(m => m.GetParameters().Any(p => p.Name == "fieldId")).Select(m => new object[] { m.DeclaringType!.Name, m.Name });
    public static IEnumerable<object[]> FacadeOperations => Operations.Where(m => m.DeclaringType == typeof(PPDMHSEService)).Select(m => new object[] { m.Name });
    private static Task Invoke(object owner, string name, string field = "A", string? actor = "actor")
    {
        var method = owner.GetType().GetMethod(name)!;
        var args = method.GetParameters().Select(p => p.Name == "fieldId" ? field : p.Name == "userId" ? actor : p.ParameterType == typeof(DateRangeFilter) ? new DateRangeFilter(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31)) : p.ParameterType == typeof(string) ? "incident" : p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null).ToArray();
        return (Task)method.Invoke(owner, args)!;
    }
    private static async Task<Fixture> Ready(bool selected = true)
    {
        var f = await Fixture.Create();
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.View });
        await f.Db.SaveChangesAsync(); await f.Grant("FIELD", "A"); await f.Grant("FIELD", "B"); f.Rows.AddRange([new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" }, new FIELD { FIELD_ID = "B", ACTIVE_IND = "Y" }]);
        if (selected) { var profile = Profile(f); await profile.SetCurrentFieldSelectionAsync(new() { FieldId = "A", Context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext }); }
        return f;
    }
    [Theory] [MemberData(nameof(AllOperations))]
    public async Task Every_actual_HSE_operation_requires_a_current_FIELD(string owner, string operation)
    {
        using var f = await Ready(false); var profile = Profile(f);
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Owner(owner, f, profile), operation)); Assert.Empty(f.Writes);
        f.Source.Verify(x => x.GetEntityAsync("HSE_INCIDENT", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
    }
    [Theory] [MemberData(nameof(ActorOperations))]
    public async Task Supplied_actors_cannot_replace_the_actual_profile_account(string owner, string operation)
    {
        using var f = await Ready(); var profile = Profile(f);
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Owner(owner, f, profile), operation, actor: "external-subject")); Assert.Empty(f.Writes);
    }
    [Theory] [MemberData(nameof(ActorOperations))]
    public async Task Required_actors_cannot_be_missing(string owner, string operation)
    {
        using var f = await Ready(false); var profile = Profile(f);
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Owner(owner, f, profile), operation, actor: null)); Assert.Empty(f.Writes);
    }
    [Theory] [MemberData(nameof(ExplicitFieldOperations))]
    public async Task Direct_FIELD_parameters_cannot_override_selection(string owner, string operation)
    {
        using var f = await Ready(); var profile = Profile(f);
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Owner(owner, f, profile), operation, field: "B")); Assert.Empty(f.Writes);
    }
    [Theory] [MemberData(nameof(FacadeOperations))]
    public async Task Every_facade_operation_refuses_a_different_captured_FIELD(string operation)
    {
        using var f = await Ready(); var profile = Profile(f);
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Owner(nameof(PPDMHSEService), f, profile, "B"), operation)); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Incident_and_study_creation_cannot_rewrite_submitted_FIELD(bool facade)
    {
        using var f = await Ready(); var profile = Profile(f);
        var incident = new ReportIncidentRequest("B", IncidentType.Injury, 1, new DateTime(2026, 1, 1), "test", "recorded test", "test", "SET", new string('A', 64));
        var study = new CreateHAZOPStudyRequest { FieldId = "B", StudyName = "test", Scope = "test" };
        if (facade) { var service = (PPDMHSEService)Owner(nameof(PPDMHSEService), f, profile); await Assert.ThrowsAsync<RefusalException>(() => service.ReportIncidentAsync(incident, "actor")); await Assert.ThrowsAsync<RefusalException>(() => service.CreateStudyAsync(study, "actor")); }
        else { await Assert.ThrowsAsync<RefusalException>(() => Incident(f, profile).ReportIncidentAsync(incident, "actor")); await Assert.ThrowsAsync<RefusalException>(() => ((HAZOPService)Owner(nameof(HAZOPService), f, profile)).CreateStudyAsync(study, "actor")); }
        Assert.Equal("B", incident.FieldId); Assert.Equal("B", study.FieldId); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData(nameof(RCAService), "GetCauseChainAsync", "B")] [InlineData(nameof(RCAService), "GetCauseChainAsync", "a")]
    [InlineData(nameof(BarrierManagementService), "GetBarriersForIncidentAsync", "B")] [InlineData(nameof(BarrierManagementService), "GetBarriersForIncidentAsync", "a")]
    [InlineData(nameof(CorrectiveActionService), "GetCAStatusAsync", "B")] [InlineData(nameof(CorrectiveActionService), "GetCAStatusAsync", "a")]
    public async Task Incident_input_boundary_cannot_authorize_another_FIELD_or_case_alias(string owner, string operation, string field)
    {
        using var f = await Ready(); var profile = Profile(f); var incident = new Mock<IHSEService>(MockBehavior.Strict);
        incident.Setup(x => x.GetByIdAsync("incident")).ReturnsAsync(new HSEIncidentRecord { IncidentId = "incident", FieldId = field });
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Owner(owner, f, profile, incidents: incident.Object), operation)); Assert.Empty(f.Writes);
        incident.Verify(x => x.GetByIdAsync("incident"), Times.Once);
        f.Source.Verify(x => x.GetEntityAsync("HSE_INCIDENT_CAUSE", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
    }
    [Fact]
    public async Task Incident_input_revision_change_is_rechecked_before_child_storage()
    {
        using var f = await Ready(); var profile = Profile(f); var incident = new Mock<IHSEService>(MockBehavior.Strict);
        incident.Setup(x => x.GetByIdAsync("incident")).Returns(async () => { await profile.UpdateUserPreferencesAsync("actor", "{\"density\":\"changed\"}"); return new HSEIncidentRecord { IncidentId = "incident", FieldId = "A" }; });
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Owner(nameof(RCAService), f, profile, incidents: incident.Object), "GetCauseChainAsync")); Assert.Empty(f.Writes);
        f.Source.Verify(x => x.GetEntityAsync("HSE_INCIDENT_CAUSE", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
    }
    [Theory] [InlineData("GetExposureHoursAsync")] [InlineData("GetKPIsAsync")] [InlineData("GetTRIRAsync")] [InlineData("GetTierRateTrendAsync")]
    public async Task Unavailable_exposure_does_not_become_zero_or_an_invented_denominator(string operation)
    {
        using var f = await Ready(); var profile = Profile(f); var service = (HSEKPIService)Owner(nameof(HSEKPIService), f, profile); var range = new DateRangeFilter(new(2026, 1, 1), new(2026, 1, 31));
        Task result = operation switch { "GetExposureHoursAsync" => service.GetExposureHoursAsync("A", range), "GetKPIsAsync" => service.GetKPIsAsync("A", range), "GetTRIRAsync" => service.GetTRIRAsync("A", range), _ => service.GetTierRateTrendAsync("A", 1) };
        var error = await Assert.ThrowsAsync<RefusalException>(() => result); Assert.Contains("exposure", error.Message); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData(nameof(HSEIncidentService), "HSE_INCIDENT_COMPONENT")] [InlineData(nameof(HAZOPService), "PROJECT_COMPONENT")]
    public async Task Actual_canonical_sources_require_the_existing_row_policy(string owner, string table)
    {
        using var f = await Ready(); var profile = Profile(f);
        if (owner == nameof(HSEIncidentService)) Assert.Empty(await Incident(f, profile).GetFieldIncidentsAsync("A", null));
        else Assert.Empty(await ((HAZOPService)Owner(owner, f, profile)).GetStudiesAsync("A"));
        Assert.Empty(f.Writes);
        f.Source.Verify(x => x.GetEntityAsync(table, It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
    }
    [Theory] [InlineData(nameof(HSEIncidentService), "BuildRepoAsync", "HSE_INCIDENT")] [InlineData(nameof(RCAService), "BuildRepoAsync", "HSE_INCIDENT_CAUSE")] [InlineData(nameof(BarrierManagementService), "BuildComponentRepoAsync", "HSE_INCIDENT_COMPONENT")]
    public async Task Actual_repository_factories_resolve_canonical_types_and_the_reviewed_database(string owner, string factory, string table)
    {
        using var f = await Ready(); var profile = Profile(f); var instance = Owner(owner, f, profile);
        var method = instance.GetType().GetMethod(factory, BindingFlags.Instance | BindingFlags.NonPublic)!;
        var repo = await (Task<PPDMGenericRepository>)method.Invoke(instance, owner == nameof(HSEIncidentService) ? new object[] { table } : null)!;
        Assert.Equal(typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + table), repo.EntityType); Assert.Equal(table, repo.TableName); Assert.Equal("selected-tenant", typeof(PPDMGenericRepository).GetField("_connectionName", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(repo));
        Assert.Empty(await repo.GetAsync(new())); Assert.All(f.Connections, c => Assert.Equal("selected-tenant", c)); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData(null)] [InlineData("")] [InlineData("unparsed recorded text")] [InlineData("{")]
    public void Malformed_or_missing_HAZOP_payload_cannot_invent_consequence(string? text)
    {
        var method = typeof(HAZOPService).GetMethod("ParseConditionPayload", BindingFlags.Static | BindingFlags.NonPublic)!;
        var error = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, [new PROJECT_CONDITION { REMARK = text! }])); Assert.IsType<RefusalException>(error.InnerException);
    }
    [Theory] [InlineData(0)] [InlineData(5)] [InlineData(-1)]
    public void Out_of_range_tiers_cannot_be_clamped_into_another_severity(int tier)
    {
        var method = typeof(HSEIncidentService).GetMethod("MapTierToSeverityId", BindingFlags.Static | BindingFlags.NonPublic)!;
        Assert.IsType<RefusalException>(Assert.Throws<TargetInvocationException>(() => method.Invoke(null, [tier])).InnerException);
    }
    [Theory] [InlineData(null)] [InlineData("unknown")] [InlineData("tier_1")]
    public void Missing_or_unreviewed_severity_cannot_become_tier_four(string? severity)
    {
        var method = typeof(HSEIncidentService).GetMethod("MapSeverityIdToTier", BindingFlags.Static | BindingFlags.NonPublic)!;
        Assert.IsType<RefusalException>(Assert.Throws<TargetInvocationException>(() => method.Invoke(null, [severity])).InnerException);
    }
}
