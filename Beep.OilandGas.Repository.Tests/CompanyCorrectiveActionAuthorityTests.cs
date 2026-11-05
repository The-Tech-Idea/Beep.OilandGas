using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

// Actual incident and CA owners, app registry/profile/eligibility/policy/Beep repositories; explicit recording provider.
public sealed class CompanyCorrectiveActionAuthorityTests
{
    internal const string Plan = "recorded-project-73";
    private static readonly DateTime RecordedDate = new(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
    public static IEnumerable<object[]> Tables => new[] { typeof(PROJECT), typeof(PROJECT_STEP), typeof(PROJECT_STATUS), typeof(PROJECT_STEP_BA) }.Select(type => new object[] { type.Name });
    private static Type Entity(string name) => typeof(PROJECT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + name)!;
    internal static RepositoryUserProfileService Profile(Fixture f)
    { f.Http.HttpContext = f.Request; return new(f.Db, new RepositoryApplicationAuthorizationReader(f.Gate), new UpperInvariantLookupNormalizer(), f.Account, f.Gate, f.Service); }
    internal static CorrectiveActionService Service(Fixture f)
    {
        var profile = Profile(f);
        var incident = new HSEIncidentService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, profile, f.Policy, NullLogger<HSEIncidentService>.Instance);
        return new(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, profile, f.Policy, incident, new PPDMCompanyCorrectiveActionCommands(f.Gate, profile, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy), NullLogger<CorrectiveActionService>.Instance);
    }
    internal static async Task<Fixture> Ready(bool inherit = true, string level = "READ", Fixture? fixture = null)
    {
        var f = fixture ?? await Fixture.Create();
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.View });
        await f.Db.SaveChangesAsync(); await f.Grant("FIELD", "A", level, inherit: inherit);
        f.Rows.AddRange([new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" }, new FIELD { FIELD_ID = "B", ACTIVE_IND = "Y" },
            new R_HSE_COMP_ROLE { COMPONENT_ROLE = HseReferenceCodes.FieldContext.PrimaryRole, ACTIVE_IND = "Y" },
            new R_HSE_INCIDENT_COMP_TYPE { COMPONENT_TYPE = HseReferenceCodes.FieldContext.ComponentType, ACTIVE_IND = "Y" },
            new R_HSE_RESPONSE_TYPE { ACTION_TYPE = "recorded-action-response", ACTIVE_IND = "Y" },
            new R_PROJECT_TYPE { PROJECT_TYPE = "CORRECTIVE_ACTION", ACTIVE_IND = "Y" }, new R_PROJ_STEP_TYPE { STEP_TYPE = "recorded-action-type", ACTIVE_IND = "Y" },
            new R_PROJECT_STATUS_TYPE { STATUS_TYPE = "CA_STEP", PROJECT_TYPE = "CORRECTIVE_ACTION", ACTIVE_IND = "Y" },
            new R_PROJECT_STATUS { STATUS_TYPE = "CA_STEP", STATUS = "OPEN", ACTIVE_IND = "Y" }, new R_PROJECT_STATUS { STATUS_TYPE = "CA_STEP", STATUS = "COMPLETED", ACTIVE_IND = "Y" },
            new R_PROJECT_BA_ROLE { ROLE = "RESPONSIBLE", ACTIVE_IND = "Y" }]);
        Seed(f, "local", "A", Plan); Seed(f, "foreign", "B", "foreign-project");
        var profile = Profile(f); await profile.SetCurrentFieldSelectionAsync(new() { FieldId = "A", Context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext });
        return f;
    }
    private static void Seed(Fixture f, string incident, string field, string project)
    {
        f.Rows.AddRange([new HSE_INCIDENT { INCIDENT_ID = incident, ACTIVE_IND = "Y", INCIDENT_DATE = RecordedDate },
            new HSE_INCIDENT_COMPONENT { INCIDENT_ID = incident, COMPONENT_OBS_NO = 1, FIELD_ID = field, COMPONENT_ROLE = HseReferenceCodes.FieldContext.PrimaryRole, COMPONENT_TYPE = HseReferenceCodes.FieldContext.ComponentType, ACTIVE_IND = "Y" },
            new HSE_INCIDENT_DETAIL { INCIDENT_ID = incident, DETAIL_OBS_NO = 1, DETAIL_TYPE = "PRIMARY", INCIDENT_SEVERITY_ID = "TIER_2", INCIDENT_TYPE_ID = "recorded-incident-type", REMARK = "recorded description", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_RESPONSE { INCIDENT_ID = incident, RESPONSE_OBS_NO = 1, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = "REPORTED", EFFECTIVE_DATE = RecordedDate, ACTIVE_IND = "Y" },
            new HSE_INCIDENT_RESPONSE { INCIDENT_ID = incident, RESPONSE_OBS_NO = 2, RESPONSE_TYPE = "recorded-action-response", PROJECT_ID = project, EFFECTIVE_DATE = RecordedDate, ACTIVE_IND = "Y" },
            new PROJECT { PROJECT_ID = project, PROJECT_TYPE = "CORRECTIVE_ACTION", PROJECT_NUM = "unrelated-recorded-number", ACTIVE_IND = "Y" },
            new PROJECT_STEP { PROJECT_ID = project, STEP_ID = project + "-actual-step", STEP_SEQ_NO = 1, STEP_TYPE = "recorded-action-type", STEP_NAME = "recorded action", DUE_DATE = RecordedDate.AddDays(10), ACTIVE_IND = "Y" },
            new PROJECT_STATUS { PROJECT_ID = project, STATUS_ID = "recorded-status", STEP_ID = project + "-actual-step", STATUS_TYPE = "CA_STEP", STATUS = "OPEN", EFFECTIVE_DATE = RecordedDate, ACTIVE_IND = "Y" },
            new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = incident + "-person", ACTIVE_IND = "Y" },
            new PROJECT_BA { PROJECT_ID = project, BUSINESS_ASSOCIATE_ID = incident + "-person", ACTIVE_IND = "Y" },
            new PROJECT_BA_ROLE { PROJECT_ID = project, BUSINESS_ASSOCIATE_ID = incident + "-person", ROLE = "RESPONSIBLE", ROLE_SEQ_NO = 7, ACTIVE_IND = "Y" },
            new PROJECT_STEP_BA { PROJECT_ID = project, BUSINESS_ASSOCIATE_ID = incident + "-person", ROLE = "RESPONSIBLE", ROLE_SEQ_NO = 7, STEP_ID = project + "-actual-step", STEP_BA_OBS_NO = 11, ACTIVE_IND = "Y" }]);
    }
    private static PROJECT_STEP Step(Fixture f) => f.Rows.OfType<PROJECT_STEP>().Single(row => row.PROJECT_ID == Plan);
    private static PROJECT_STATUS Status(Fixture f) => f.Rows.OfType<PROJECT_STATUS>().Single(row => row.PROJECT_ID == Plan);
    private static HSE_INCIDENT_RESPONSE Link(Fixture f) => f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(row => row.PROJECT_ID == Plan);
    private static PROJECT_STEP_BA Assignment(Fixture f) => f.Rows.OfType<PROJECT_STEP_BA>().Single(row => row.PROJECT_ID == Plan);
    [Theory]
    [InlineData("large-field")]
    [InlineData("unrelated-malformed")]
    [InlineData("foreign-link")]
    [InlineData("duplicate-link")]
    [InlineData("changed-link")]
    [InlineData("retired-field")]
    [InlineData("missing-status")]
    [InlineData("foreign-provider-row")]
    [InlineData("contradictory-project")]
    public async Task ActualCorrectiveActionReadStaysBoundToRecordedProjectAndIncident(string scenario)
    {
        using var f = await Ready(); var profile = Profile(f);
        _ = await profile.GetCurrentFieldSelectionAsync();
        for (var i = 0; i < 300; i++) f.Rows.Add(new HSE_INCIDENT_COMPONENT
        {
            INCIDENT_ID = "unrelated-" + i, COMPONENT_OBS_NO = 1, FIELD_ID = "A", ACTIVE_IND = "Y",
            COMPONENT_ROLE = HseReferenceCodes.FieldContext.PrimaryRole, COMPONENT_TYPE = HseReferenceCodes.FieldContext.ComponentType
        });
        if (scenario == "unrelated-malformed") f.Rows.OfType<HSE_INCIDENT_COMPONENT>().First(row => row.INCIDENT_ID.StartsWith("unrelated-", StringComparison.Ordinal)).INCIDENT_ID = "";
        if (scenario == "foreign-link") Link(f).PROJECT_ID = "foreign-project";
        if (scenario == "duplicate-link") f.Rows.Add(new HSE_INCIDENT_RESPONSE
        {
            INCIDENT_ID = "foreign", RESPONSE_OBS_NO = 3, PROJECT_ID = Plan, RESPONSE_TYPE = "recorded-action-response",
            ACTIVE_IND = "Y", EFFECTIVE_DATE = RecordedDate
        });
        if (scenario == "retired-field") f.Rows.OfType<FIELD>().Single(row => row.FIELD_ID == "A").ACTIVE_IND = "N";
        if (scenario == "missing-status") f.Rows.Remove(Status(f));
        if (scenario == "changed-link") f.AfterRead = () =>
        {
            var last = f.Source.Invocations.LastOrDefault(call => call.Method.Name == "GetEntityAsync");
            if (last?.Arguments[0] as string == nameof(PROJECT_STEP)) Link(f).INCIDENT_ID = "foreign";
            return Task.CompletedTask;
        };
        if (scenario == "foreign-provider-row")
            f.Source.Setup(source => source.GetEntityAsync(nameof(PROJECT_STEP), It.IsAny<List<AppFilter>>()))
                .ReturnsAsync(new object[] { new PROJECT_STEP { PROJECT_ID = "foreign-project", STEP_ID = "foreign-step", STEP_SEQ_NO = 1, ACTIVE_IND = "Y" } });
        f.Source.Invocations.Clear(); f.Queries.Clear();
        if (scenario is "large-field" or "unrelated-malformed")
        {
            var action = Assert.Single(await Service(f).GetCAStatusAsync("local"));
            Assert.Equal(1, action.StepSeq); Assert.Equal("recorded action", action.Description); Assert.Equal("local-person", action.ResponsiblePerson);
        }
        else if (scenario == "contradictory-project")
            await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_STEP)).GetAsync([
                new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = Plan },
                new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = "foreign-project" }]));
        else await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetCAStatusAsync("local"));
        var primaryReads = f.Source.Invocations.Where(call => call.Method.Name == "GetEntityAsync" && call.Arguments[0] as string == nameof(HSE_INCIDENT_COMPONENT)).ToArray();
        Assert.All(primaryReads, call => Assert.Contains((List<AppFilter>)call.Arguments[1], filter => filter.FieldName == "INCIDENT_ID" && filter.Operator == "=" && filter.FilterValue is "local" or "foreign"));
        Assert.Empty(f.Writes);
    }
    [Theory] [MemberData(nameof(Tables))]
    public async Task Canonical_CA_rows_require_actual_response_link_and_eligible_inherited_incident_FIELD(string table)
    {
        using var f = await Ready(); var row = Assert.Single(await f.Repository(Entity(table)).GetAsync(new()));
        Assert.Equal(Plan, row.GetType().GetProperty("PROJECT_ID")!.GetValue(row));
        Assert.Equal("READ", await f.Policy.GetEffectiveAccessLevelAsync(Entity(table), table, row)); Assert.Empty(f.Writes);
    }
    [Theory] [MemberData(nameof(Tables))]
    public async Task Noninherited_FIELD_grant_cannot_authorize_linked_CA_rows(string table)
    {
        using var f = await Ready(false); Assert.Empty(await f.Repository(Entity(table)).GetAsync(new()));
        f.Source.Verify(source => source.GetEntityAsync(table, It.IsAny<List<AppFilter>>()), Times.Never);
    }
    [Fact]
    public async Task Actual_CA_owner_reuses_actual_project_and_reports_recorded_evidence_without_ID_or_number_inference()
    {
        using var f = await Ready(); var service = Service(f);
        Assert.Equal(Plan, (await service.GetCAPlanAsync("local")).PlanId);
        var action = Assert.Single(await service.GetCAStatusAsync("local"));
        Assert.Equal("recorded action", action.Description); Assert.Equal(1, action.StepSeq); Assert.Equal("recorded-action-type", action.CAType);
        Assert.Equal("OPEN", action.Status); Assert.Equal(RecordedDate.AddDays(10), action.DueDate); Assert.Equal("local-person", action.ResponsiblePerson);
        Assert.True(action.IsOverdue); Assert.Null(action.CompletedDate); Assert.False(await service.AllCAsMeetDeadlineAsync("local"));
        Assert.All(f.Connections, value => Assert.Equal("selected-tenant", value)); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData(9, true)] [InlineData(10, true)] [InlineData(11, false)]
    public async Task Deadline_result_compares_actual_completion_against_recorded_due_date(int days, bool onTime)
    {
        using var f = await Ready(); Step(f).ACTUAL_END_DATE = RecordedDate.AddDays(days); Status(f).STATUS = "COMPLETED"; Status(f).EFFECTIVE_DATE = RecordedDate.AddDays(days);
        var service = Service(f); var row = Assert.Single(await service.GetCAStatusAsync("local")); Assert.Equal(!onTime, row.IsOverdue);
        Assert.Equal(onTime, await service.AllCAsMeetDeadlineAsync("local")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("foreign")] [InlineData("duplicate")] [InlineData("inactive")] [InlineData("expired")] [InlineData("future")] [InlineData("unknown-zone")] [InlineData("missing-code")] [InlineData("wrong-type")] [InlineData("extra-incident")]
    public async Task Invalid_or_ambiguous_incident_project_links_refuse_CA_interpretation(string fault)
    {
        using var f = await Ready(); var link = Link(f);
        switch (fault)
        {
            case "foreign": link.PROJECT_ID = "foreign-project"; break;
            case "duplicate": case "extra-incident": f.Rows.Add(new HSE_INCIDENT_RESPONSE { INCIDENT_ID = fault == "duplicate" ? "local" : "foreign", RESPONSE_OBS_NO = 3, RESPONSE_TYPE = link.RESPONSE_TYPE, PROJECT_ID = Plan, ACTIVE_IND = "Y" }); break;
            case "inactive": link.ACTIVE_IND = "N"; break;
            case "expired": link.EXPIRY_DATE = DateTime.UtcNow.AddDays(-1); break;
            case "future": link.EFFECTIVE_DATE = DateTime.UtcNow.AddDays(1); break;
            case "unknown-zone": link.EFFECTIVE_DATE = new(2020, 1, 1); break;
            case "missing-code": f.Rows.RemoveAll(row => row is R_HSE_RESPONSE_TYPE); break;
            case "wrong-type": f.Rows.OfType<PROJECT>().Single(row => row.PROJECT_ID == Plan).PROJECT_TYPE = "HAZOP"; break;
        }
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetCAStatusAsync("local")); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("REPORTED")]
    [InlineData("UNDER_INVESTIGATION")]
    [InlineData("PENDING_RCA")]
    [InlineData("PENDING_CORRECTIVE_ACTION")]
    [InlineData("PENDING_CLOSE_OUT")]
    [InlineData("CLOSED")]
    [InlineData("CANCELLED")]
    public async Task Confirmed_plan_absence_is_readable_only_before_corrective_work_and_never_establishes_completion(string state)
    {
        using var f = await Ready(); f.Rows.Remove(Link(f));
        // Keep unlinked real and name-derived projects: neither may be inferred as this incident's plan.
        f.Rows.Add(new PROJECT { PROJECT_ID = "CA-local", PROJECT_NUM = "local", PROJECT_TYPE = "CORRECTIVE_ACTION", ACTIVE_IND = "Y" });
        f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(row => row.INCIDENT_ID == "local" && row.RESPONSE_TYPE == "STATUS").RESPONSE_RESULT = state;
        var service = Service(f);
        if (state is "REPORTED" or "UNDER_INVESTIGATION" or "PENDING_RCA")
            Assert.Empty(await service.GetCAStatusAsync("local"));
        else await Assert.ThrowsAsync<RefusalException>(() => service.GetCAStatusAsync("local"));
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetCAPlanAsync("local"));
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).AllCAsMeetDeadlineAsync("local"));
        Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("due-missing")] [InlineData("due-zone")] [InlineData("status-missing")] [InlineData("status-date")] [InlineData("status-tie")] [InlineData("orphan-status")] [InlineData("completion-missing")] [InlineData("completion-future")] [InlineData("completion-zone")] [InlineData("completion-after-status")] [InlineData("open-completed")] [InlineData("name")] [InlineData("fractional-sequence")] [InlineData("large-sequence")] [InlineData("zero-sequence")] [InlineData("participant-missing")] [InlineData("participant-duplicate")]
    public async Task Missing_or_ambiguous_recorded_action_evidence_cannot_become_defaults_or_success(string fault)
    {
        using var f = await Ready(); var step = Step(f); var status = Status(f);
        switch (fault)
        {
            case "due-missing": step.DUE_DATE = null; break;
            case "due-zone": step.DUE_DATE = new(2026, 1, 2); break;
            case "status-missing": f.Rows.Remove(status); break;
            case "status-date": status.EFFECTIVE_DATE = null; break;
            case "status-tie": f.Rows.Add(new PROJECT_STATUS { PROJECT_ID = Plan, STATUS_ID = "second", STEP_ID = step.STEP_ID, STATUS_TYPE = status.STATUS_TYPE, STATUS = status.STATUS, EFFECTIVE_DATE = status.EFFECTIVE_DATE, ACTIVE_IND = "Y" }); break;
            case "orphan-status": status.STEP_ID = "foreign-project-actual-step"; break;
            case "completion-missing": status.STATUS = "COMPLETED"; break;
            case "completion-future": status.STATUS = "COMPLETED"; step.ACTUAL_END_DATE = DateTime.UtcNow.AddDays(1); break;
            case "completion-after-status": status.STATUS = "COMPLETED"; step.ACTUAL_END_DATE = RecordedDate.AddDays(1); break;
            case "completion-zone": status.STATUS = "COMPLETED"; step.ACTUAL_END_DATE = new(2026, 1, 3); break;
            case "open-completed": step.ACTUAL_END_DATE = RecordedDate; break;
            case "name": step.STEP_NAME = " "; break;
            case "fractional-sequence": step.STEP_SEQ_NO = 1.5m; break;
            case "large-sequence": step.STEP_SEQ_NO = (decimal)int.MaxValue + 1; break;
            case "zero-sequence": step.STEP_SEQ_NO = 0; break;
            case "participant-missing": f.Rows.Remove(Assignment(f)); break;
            case "participant-duplicate": var row = Assignment(f); f.Rows.Add(new PROJECT_STEP_BA { PROJECT_ID = Plan, STEP_ID = row.STEP_ID, BUSINESS_ASSOCIATE_ID = row.BUSINESS_ASSOCIATE_ID, ROLE = row.ROLE, ROLE_SEQ_NO = row.ROLE_SEQ_NO, STEP_BA_OBS_NO = 12, ACTIVE_IND = "Y" }); break;
        }
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetCAStatusAsync("local")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("role-sequence")] [InlineData("role-inactive")] [InlineData("participant")] [InlineData("associate")] [InlineData("vocabulary")] [InlineData("foreign-step")] [InlineData("fractional-observation")]
    public async Task Raw_assignment_requires_complete_actual_project_participant_and_role_relationship(string fault)
    {
        using var f = await Ready();
        switch (fault)
        {
            case "role-sequence": Assignment(f).ROLE_SEQ_NO = 8; break;
            case "role-inactive": f.Rows.OfType<PROJECT_BA_ROLE>().Single(row => row.PROJECT_ID == Plan).ACTIVE_IND = "N"; break;
            case "participant": f.Rows.RemoveAll(row => row is PROJECT_BA value && value.PROJECT_ID == Plan); break;
            case "associate": f.Rows.OfType<BUSINESS_ASSOCIATE>().Single(row => row.BUSINESS_ASSOCIATE_ID == "local-person").ACTIVE_IND = "N"; break;
            case "vocabulary": f.Rows.RemoveAll(row => row is R_PROJECT_BA_ROLE); break;
            case "foreign-step": Assignment(f).STEP_ID = "foreign-project-actual-step"; break;
            case "fractional-observation": Assignment(f).STEP_BA_OBS_NO = 11.5m; break;
        }
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_STEP_BA)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("reparent")] [InlineData("revoke")] [InlineData("permission")]
    public async Task Relationship_or_authority_changes_during_provider_read_refuse_CA_rows(string fault)
    {
        using var f = await Ready();
        f.Source.Setup(source => source.GetEntityAsync("PROJECT_STEP_BA", It.IsAny<List<AppFilter>>())).Returns(async () =>
        {
            if (fault == "reparent") Link(f).INCIDENT_ID = "foreign";
            else if (fault == "revoke") { (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive = false; await f.Db.SaveChangesAsync(); }
            else { f.Db.RoleClaims.Remove(await f.Db.RoleClaims.SingleAsync(row => row.ClaimValue == Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.View)); await f.Db.SaveChangesAsync(); }
            return new object[] { Assignment(f) };
        });
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_STEP_BA)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Provider_ignoring_CA_project_predicates_cannot_return_foreign_actions()
    {
        using var f = await Ready(); f.Source.Setup(source => source.GetEntityAsync("PROJECT_STEP", It.IsAny<List<AppFilter>>())).ReturnsAsync(f.Rows.OfType<PROJECT_STEP>().Cast<object>().ToArray());
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_STEP)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("PROJECT_STEP_BA", "keys")] [InlineData("HSE_INCIDENT_RESPONSE", "project-link")] [InlineData("PROJECT", "type-link")]
    public async Task Metadata_drift_refuses_actual_complete_identity_and_relationship(string table, string fault)
    {
        using var f = await Ready(); var definition = await f.Metadata.Object.GetTableMetadataAsync(table);
        if (fault == "keys") definition.PrimaryKeyColumn = "PROJECT_ID,STEP_ID";
        else if (fault == "type-link") definition.ForeignKeys.Single(key => key.ForeignKeyColumn == "PROJECT_TYPE" && key.ReferencedTable == "R_PROJECT_TYPE").ReferencedPrimaryKey = "OTHER";
        else definition.ForeignKeys.Single(key => key.ForeignKeyColumn == "PROJECT_ID" && key.ReferencedTable == "PROJECT").ReferencedPrimaryKey = "OTHER";
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_STEP_BA)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData(PPDMRowMutation.Insert)] [InlineData(PPDMRowMutation.Update)] [InlineData(PPDMRowMutation.Delete)]
    public async Task Generic_CA_writes_remain_fenced_pending_reviewed_transactional_commands(PPDMRowMutation operation)
    {
        using var f = await Ready(); var row = Assignment(f);
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(typeof(PROJECT_STEP_BA), "PROJECT_STEP_BA", row, operation == PPDMRowMutation.Insert ? null : row, operation, "actor")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("response-type")] [InlineData("status-code")] [InlineData("step-type")] [InlineData("status-parent")]
    public async Task Approved_reference_identity_and_parent_type_remain_required_for_CA_evidence(string fault)
    {
        using var f = await Ready();
        if (fault == "response-type") Link(f).RESPONSE_TYPE = "unrecorded";
        else if (fault == "status-code") Status(f).STATUS = "unrecorded";
        else if (fault == "step-type") Step(f).STEP_TYPE = "unrecorded";
        else f.Rows.OfType<R_PROJECT_STATUS_TYPE>().Single().PROJECT_TYPE = "HAZOP";
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetCAStatusAsync("local")); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Two_distinct_valid_CA_plans_for_one_incident_require_explicit_resolution()
    {
        using var f = await Ready(); f.Rows.Add(new PROJECT { PROJECT_ID = "second-plan", PROJECT_TYPE = "CORRECTIVE_ACTION", ACTIVE_IND = "Y" });
        f.Rows.Add(new HSE_INCIDENT_RESPONSE { INCIDENT_ID = "local", RESPONSE_OBS_NO = 3, PROJECT_ID = "second-plan", RESPONSE_TYPE = Link(f).RESPONSE_TYPE, ACTIVE_IND = "Y" });
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetCAStatusAsync("local")); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Reported_provider_error_cannot_become_recorded_CA_evidence()
    {
        using var f = await Ready(); var error = new TheTechIdea.Beep.ConfigUtil.ErrorsInfo { Flag = TheTechIdea.Beep.ConfigUtil.Errors.Ok };
        f.Source.SetupGet(source => source.ErrorObject).Returns(error);
        f.Source.Setup(source => source.GetEntityAsync("PROJECT_STEP_BA", It.IsAny<List<AppFilter>>())).ReturnsAsync(() =>
        { error.Flag = TheTechIdea.Beep.ConfigUtil.Errors.Failed; return new object[] { Assignment(f) }; });
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Repository(typeof(PROJECT_STEP_BA)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task CA_project_with_recorded_FIELD_component_does_not_require_unrelated_HAZOP_reference_codes()
    {
        using var f = await Ready();
        f.Rows.Add(new PROJECT_COMPONENT { PROJECT_ID = Plan, COMPONENT_OBS_NO = 1, FIELD_ID = "A", FIELD_SOURCE = HseReferenceCodes.Hazop.FieldContextSource, SOURCE = HseReferenceCodes.Hazop.FieldContextSource, ACTIVE_IND = "Y" });
        Assert.Equal(Plan, (await Service(f).GetCAPlanAsync("local")).PlanId); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Response_identity_change_during_reference_review_cannot_retain_a_project_predicate()
    {
        using var f = await Ready(); var changed = false;
        f.Source.Setup(source => source.GetEntityAsync("R_HSE_RESPONSE_TYPE", It.IsAny<List<AppFilter>>())).ReturnsAsync(() =>
        {
            if (!changed) { changed = true; Link(f).RESPONSE_OBS_NO = 9; }
            return f.Rows.OfType<R_HSE_RESPONSE_TYPE>().Cast<object>().ToArray();
        });
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_STEP_BA)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Requested_incident_cannot_follow_a_plan_relinked_to_another_authorized_incident()
    {
        using var f = await Ready(); await f.Grant("FIELD", "B", inherit: true);
        f.Source.Setup(source => source.GetEntityAsync("PROJECT_STEP_BA", It.IsAny<List<AppFilter>>())).ReturnsAsync(() =>
        {
            var link = Link(f); link.INCIDENT_ID = "foreign"; link.RESPONSE_OBS_NO = 3;
            return new object[] { Assignment(f) };
        });
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetCAStatusAsync("local")); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Incident_response_candidate_inventory_must_remain_bounded()
    {
        using var f = await Ready();
        for (var i = 3; i < 260; i++) f.Rows.Add(new HSE_INCIDENT_RESPONSE { INCIDENT_ID = "local", RESPONSE_OBS_NO = i, RESPONSE_TYPE = "recorded-action-response", ACTIVE_IND = "Y" });
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_STEP_BA)).GetAsync(new())); Assert.Empty(f.Writes);
        f.Source.Verify(source => source.GetEntityAsync("PROJECT_STEP_BA", It.IsAny<List<AppFilter>>()), Times.Never);
    }
    [Fact]
    public async Task Empty_recorded_plan_cannot_establish_completion()
    {
        using var f = await Ready(); f.Rows.RemoveAll(row => row is PROJECT_STEP step && step.PROJECT_ID == Plan || row is PROJECT_STATUS status && status.PROJECT_ID == Plan || row is PROJECT_STEP_BA assignment && assignment.PROJECT_ID == Plan);
        Assert.Empty(await Service(f).GetCAStatusAsync("local")); await Assert.ThrowsAsync<RefusalException>(() => Service(f).AllCAsMeetDeadlineAsync("local")); Assert.Empty(f.Writes);
    }
}
