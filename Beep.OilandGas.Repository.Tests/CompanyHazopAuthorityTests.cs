using System.Globalization;
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

// Actual HAZOP owner, profile/app registry/eligibility/policy/canonical repository; Beep is an explicit recording provider.
public sealed class CompanyHazopAuthorityTests
{
    public static IEnumerable<object[]> Tables => new[] { typeof(PROJECT), typeof(PROJECT_COMPONENT), typeof(PROJECT_STEP), typeof(PROJECT_CONDITION), typeof(PROJECT_STATUS) }.Select(type => new object[] { type.Name });
    private static Type Entity(string name) => typeof(PROJECT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + name)!;
    private static RepositoryUserProfileService Profile(Fixture f)
    { f.Http.HttpContext = f.Request; return new(f.Db, new RepositoryApplicationAuthorizationReader(f.Gate), new UpperInvariantLookupNormalizer(), f.Account, f.Gate, f.Service); }
    private static HAZOPService Service(Fixture f) => new(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, Profile(f), f.Policy, NullLogger<HAZOPService>.Instance);
    private static async Task<Fixture> Ready(bool inherit = true)
    {
        var f = await Fixture.Create();
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.View });
        await f.Db.SaveChangesAsync(); await f.Grant("FIELD", "A", inherit: inherit);
        f.Rows.AddRange([new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" }, new FIELD { FIELD_ID = "B", ACTIVE_IND = "Y" },
            new R_PROJECT_TYPE { PROJECT_TYPE = "HAZOP", ACTIVE_IND = "Y" }, new R_PROJ_STEP_TYPE { STEP_TYPE = "HAZOP_NODE", ACTIVE_IND = "Y" },
            new R_PROJECT_CONDITION { PROJECT_TYPE = "HAZOP", CONDITION_TYPE = "test-guide-word", ACTIVE_IND = "Y" }]);
        foreach (var type in new[] { "HAZOP_DEVIATION", "HAZOP_STUDY" })
        {
            f.Rows.Add(new R_PROJECT_STATUS_TYPE { STATUS_TYPE = type, PROJECT_TYPE = "HAZOP", ACTIVE_IND = "Y" });
            foreach (var code in type == "HAZOP_STUDY" ? new[] { "DRAFT" } : new[] { "OPEN", "ACTION_REQUIRED", "CLOSED" })
                f.Rows.Add(new R_PROJECT_STATUS { STATUS_TYPE = type, STATUS = code, ACTIVE_IND = "Y" });
        }
        Seed(f, "local", "A"); Seed(f, "foreign", "B");
        Seed(f, "CA-local", "A"); f.Rows.OfType<PROJECT>().Single(row => row.PROJECT_ID == "CA-local").PROJECT_TYPE = "CORRECTIVE_ACTION";
        var profile = Profile(f); await profile.SetCurrentFieldSelectionAsync(new() { FieldId = "A", Context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext });
        return f;
    }
    private static void Seed(Fixture f, string id, string field)
    {
        f.Rows.AddRange([new PROJECT { PROJECT_ID = id, PROJECT_TYPE = "HAZOP", PROJECT_NAME = "recorded study", ACTIVE_IND = "Y" },
            new PROJECT_COMPONENT { PROJECT_ID = id, COMPONENT_OBS_NO = 1, FIELD_ID = field, FIELD_SOURCE = HseReferenceCodes.Hazop.FieldContextSource, SOURCE = HseReferenceCodes.Hazop.FieldContextSource, ACTIVE_IND = "Y" },
            new PROJECT_STEP { PROJECT_ID = id, STEP_ID = id + "-step1", STEP_SEQ_NO = 1, STEP_TYPE = "HAZOP_NODE", STEP_NAME = "recorded node one", REMARK = "recorded safeguard", ACTIVE_IND = "Y" },
            new PROJECT_STEP { PROJECT_ID = id, STEP_ID = id + "-step2", STEP_SEQ_NO = 2, STEP_TYPE = "HAZOP_NODE", STEP_NAME = "recorded node two", REMARK = "recorded safeguard two", ACTIVE_IND = "Y" },
            new PROJECT_STATUS { PROJECT_ID = id, STATUS_ID = id + "-STUDY", STATUS_TYPE = "HAZOP_STUDY", STATUS = "DRAFT", ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow.AddDays(-1) }]);
        for (var i = 1; i <= 3; i++)
        {
            var step = id + (i == 3 ? "-step2" : "-step1");
            f.Rows.Add(new PROJECT_CONDITION { PROJECT_ID = id, CONDITION_OBS_NO = i, PROJECT_TYPE = "HAZOP", SOURCE = step,
                CONDITION_TYPE = "test-guide-word", REMARK = "{\"Deviation\":\"recorded deviation\",\"Consequence\":\"recorded consequence\"}", ACTIVE_IND = "Y" });
            f.Rows.Add(new PROJECT_STATUS { PROJECT_ID = id, STATUS_ID = step + "-DEV-" + i.ToString("000", CultureInfo.InvariantCulture), STATUS_TYPE = "HAZOP_DEVIATION", STEP_ID = step,
                STATUS = new[] { "OPEN", "ACTION_REQUIRED", "CLOSED" }[i - 1], ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow.AddDays(-1) });
        }
    }
    [Theory] [MemberData(nameof(Tables))]
    public async Task Canonical_project_family_reads_only_recorded_HAZOP_rows_in_an_eligible_inherited_FIELD(string table)
    {
        using var f = await Ready(); var rows = (await f.Repository(Entity(table)).GetAsync(new())).ToArray();
        Assert.NotEmpty(rows); Assert.All(rows, row => Assert.Equal("local", row.GetType().GetProperty("PROJECT_ID")!.GetValue(row)));
        Assert.Equal(table switch { "PROJECT_STEP" => 2, "PROJECT_CONDITION" => 3, "PROJECT_STATUS" => 4, _ => 1 }, rows.Length);
        Assert.Equal("READ", await f.Policy.GetEffectiveAccessLevelAsync(Entity(table), table, rows[0]));
        Assert.Null(await f.Policy.GetEffectiveAccessLevelAsync(Entity(table), table, rows[0], includeInherited: false));
        Assert.All(f.Connections, connection => Assert.Equal("selected-tenant", connection)); Assert.Empty(f.Writes);
    }
    [Theory] [MemberData(nameof(Tables))]
    public async Task Noninherited_FIELD_grants_cannot_authorize_HAZOP_projects(string table)
    {
        using var f = await Ready(false); Assert.Empty(await f.Repository(Entity(table)).GetAsync(new()));
        f.Source.Verify(source => source.GetEntityAsync(table, It.IsAny<List<AppFilter>>()), Times.Never); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Actual_studies_nodes_deviations_and_summary_read_recorded_facts_without_status_or_name_defaults()
    {
        using var f = await Ready(); var service = Service(f); var studies = await service.GetStudiesAsync("A"); var summary = Assert.Single(studies);
        Assert.Equal("local", summary.StudyId); Assert.Equal("recorded study", summary.StudyName); Assert.Equal(2, summary.TotalNodes); Assert.Equal(3, summary.TotalDeviations);
        Assert.Equal(1, summary.OpenDeviations); Assert.Equal(1, summary.ActionRequired); Assert.Equal(1, summary.Closed);
        var nodes = await service.GetNodesAsync("local"); Assert.Equal("recorded node one", nodes[0].NodeName); Assert.Equal("recorded safeguard", nodes[0].Safeguard);
        Assert.Equal(new[] { "OPEN", "ACTION_REQUIRED", "CLOSED" }, nodes.SelectMany(node => node.Deviations).Select(row => row.Status));
        Assert.All(nodes.SelectMany(node => node.Deviations), row => Assert.Equal("recorded consequence", row.Consequence)); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("duplicate")] [InlineData("other-field")] [InlineData("other-origin-field")] [InlineData("wrong-source")] [InlineData("expired")] [InlineData("future")] [InlineData("unknown-zone")] [InlineData("inactive")] [InlineData("missing-parent")] [InlineData("inactive-parent")] [InlineData("wrong-project-type")] [InlineData("missing-reference")]
    public async Task Ambiguous_or_unavailable_FIELD_and_project_evidence_never_authorizes_child_payload(string fault)
    {
        using var f = await Ready(); var context = f.Rows.OfType<PROJECT_COMPONENT>().Single(row => row.PROJECT_ID == "local");
        switch (fault)
        {
            case "duplicate": case "other-field": case "other-origin-field": f.Rows.Add(new PROJECT_COMPONENT { PROJECT_ID = "local", COMPONENT_OBS_NO = 2, FIELD_ID = fault == "duplicate" ? "A" : "B", FIELD_SOURCE = fault == "other-origin-field" ? "other-origin" : context.FIELD_SOURCE, SOURCE = context.SOURCE, ACTIVE_IND = "Y" }); break;
            case "wrong-source": context.SOURCE = "other-origin"; break;
            case "expired": context.EXPIRY_DATE = DateTime.UtcNow.AddDays(-1); break;
            case "future": context.EFFECTIVE_DATE = DateTime.UtcNow.AddDays(1); break;
            case "unknown-zone": context.EFFECTIVE_DATE = new(2020, 1, 1); break;
            case "inactive": context.ACTIVE_IND = "N"; break;
            case "missing-parent": f.Rows.RemoveAll(row => row is PROJECT project && project.PROJECT_ID == "local"); break;
            case "inactive-parent": f.Rows.OfType<PROJECT>().Single(row => row.PROJECT_ID == "local").ACTIVE_IND = "N"; break;
            case "wrong-project-type": f.Rows.OfType<PROJECT>().Single(row => row.PROJECT_ID == "local").PROJECT_TYPE = "other-type"; break;
            case "missing-reference": f.Rows.RemoveAll(row => row is R_PROJECT_TYPE); break;
        }
        var repo = f.Repository(typeof(PROJECT_STEP));
        if (fault is "expired" or "future" or "inactive" or "missing-parent" or "inactive-parent" or "wrong-project-type") Assert.Empty(await repo.GetAsync(new()));
        else await Assert.ThrowsAsync<RefusalException>(() => repo.GetAsync(new()));
        f.Source.Verify(source => source.GetEntityAsync("PROJECT_STEP", It.IsAny<List<AppFilter>>()), Times.Never); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("missing-status")] [InlineData("orphan-status")] [InlineData("wrong-status-step")] [InlineData("unknown-status")] [InlineData("undated-status")] [InlineData("future-status")] [InlineData("orphan-condition")] [InlineData("case-mismatched-condition")] [InlineData("fractional-condition")] [InlineData("large-condition")] [InlineData("fractional-node")] [InlineData("duplicate-node-sequence")] [InlineData("missing-node-name")] [InlineData("missing-study-name")]
    public async Task Inconsistent_recorded_graphs_cannot_be_reported_as_complete_studies(string fault)
    {
        using var f = await Ready(); var condition = f.Rows.OfType<PROJECT_CONDITION>().First(row => row.PROJECT_ID == "local"); var step = f.Rows.OfType<PROJECT_STEP>().First(row => row.PROJECT_ID == "local");
        var status = f.Rows.OfType<PROJECT_STATUS>().First(row => row.PROJECT_ID == "local" && row.STATUS_TYPE == "HAZOP_DEVIATION");
        switch (fault)
        {
            case "missing-status": f.Rows.Remove(status); break;
            case "orphan-status": status.STATUS_ID = "other-deviation"; break;
            case "wrong-status-step": status.STEP_ID = "foreign-step1"; break;
            case "unknown-status": status.STATUS = "unreviewed"; break;
            case "undated-status": status.EFFECTIVE_DATE = null; break;
            case "future-status": status.EFFECTIVE_DATE = DateTime.UtcNow.AddDays(1); break;
            case "orphan-condition": condition.SOURCE = "foreign-step1"; break;
            case "case-mismatched-condition": condition.SOURCE = "LOCAL-STEP1"; break;
            case "fractional-condition": condition.CONDITION_OBS_NO = 1.5m; break;
            case "large-condition": condition.CONDITION_OBS_NO = (decimal)int.MaxValue + 1; break;
            case "fractional-node": step.STEP_SEQ_NO = 1.5m; break;
            case "duplicate-node-sequence": f.Rows.OfType<PROJECT_STEP>().Last(row => row.PROJECT_ID == "local").STEP_SEQ_NO = 1; break;
            case "missing-node-name": step.STEP_NAME = ""; break;
            case "missing-study-name": f.Rows.OfType<PROJECT>().Single(row => row.PROJECT_ID == "local").PROJECT_NAME = ""; break;
        }
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetStudiesAsync("A")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("{}")] [InlineData("{\"Deviation\":\"recorded\"}")] [InlineData("{\"Deviation\":null,\"Consequence\":\"recorded\"}")] [InlineData("{\"Deviation\":12,\"Consequence\":\"recorded\"}")] [InlineData("{\"Deviation\":\"recorded\",\"Consequence\":\" \"}")] [InlineData("{\"Deviation\":\"recorded\",\"Deviation\":\"second\",\"Consequence\":\"recorded\"}")] [InlineData("{\"deviation\":\"recorded\",\"Consequence\":\"recorded\"}")] [InlineData("{\"Deviation\":\"recorded\",\"Consequence\":\"recorded\",\"Other\":\"extra\"}")] [InlineData("[]")]
    public async Task Missing_or_conflicting_payload_contents_cannot_invent_a_deviation_or_consequence(string payload)
    {
        using var f = await Ready(); f.Rows.OfType<PROJECT_CONDITION>().First(row => row.PROJECT_ID == "local").REMARK = payload;
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetNodesAsync("local")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("orphan-condition")] [InlineData("orphan-status")] [InlineData("inactive-node")]
    public async Task Raw_child_reads_require_the_complete_actual_project_and_node_relationship(string fault)
    {
        using var f = await Ready(); var type = typeof(PROJECT_CONDITION);
        if (fault == "orphan-condition") f.Rows.OfType<PROJECT_CONDITION>().First(row => row.PROJECT_ID == "local").SOURCE = "foreign-step1";
        else if (fault == "orphan-status") { type = typeof(PROJECT_STATUS); f.Rows.OfType<PROJECT_STATUS>().First(row => row.PROJECT_ID == "local" && row.STATUS_TYPE == "HAZOP_DEVIATION").STEP_ID = "foreign-step1"; }
        else f.Rows.OfType<PROJECT_STEP>().First(row => row.PROJECT_ID == "local").ACTIVE_IND = "N";
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(type).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("PROJECT_CONDITION")] [InlineData("HSE_INCIDENT")]
    public async Task An_asset_grant_and_Administrator_role_cannot_replace_explicit_app_HSE_view_permission(string table)
    {
        using var f = await Ready(); var claim = await f.Db.RoleClaims.SingleAsync(row => row.ClaimValue == "HSE.View"); f.Db.RoleClaims.Remove(claim); await f.Db.SaveChangesAsync();
        await f.Select("company");
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(Entity(table)).GetAsync(new())); Assert.Empty(f.Writes);
        f.Source.Verify(source => source.GetEntityAsync(table, It.IsAny<List<AppFilter>>()), Times.Never);
    }
    [Fact]
    public async Task Revoking_HSE_view_permission_during_a_child_query_refuses_the_returned_rows()
    {
        using var f = await Ready(); f.Source.Setup(source => source.GetEntityAsync("PROJECT_CONDITION", It.IsAny<List<AppFilter>>())).Returns(async () =>
        {
            var claim = await f.Db.RoleClaims.SingleAsync(row => row.ClaimValue == "HSE.View"); f.Db.RoleClaims.Remove(claim); await f.Db.SaveChangesAsync();
            return f.Rows.OfType<PROJECT_CONDITION>().Where(row => row.PROJECT_ID == "local").Cast<object>().ToArray();
        });
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_CONDITION)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task An_Administrators_permission_cannot_substitute_for_a_subjects_company_HSE_permission()
    {
        using var f = await Ready();
        f.Db.Users.Add(new OilGasUser { Id = "reader", UserName = "reader", IsActive = true });
        f.Db.Roles.Add(new IdentityRole { Id = "reader-role", Name = "Reader", NormalizedName = "READER" });
        f.Db.UserRoles.Add(new IdentityUserRole<string> { UserId = "reader", RoleId = "reader-role" });
        f.Db.Add(new AppCompanyMembership { UserId = "reader", TenantId = "tenant", BusinessAssociateId = "company", GrantedByUserId = "actor" });
        f.Db.Add(new AppUserRoleExtension { UserId = "reader", RoleId = "reader-role", TenantId = "tenant", BusinessAssociateId = "company" });
        var database = await f.Gate.ResolveAsync();
        f.Db.Add(new AppUserAssetAccess { Id = "reader-field", UserId = "reader", OrganizationId = "company", AssetType = "FIELD", AssetId = "A", DatabaseScope = database.Fingerprint, AccessLevel = "READ", Inherit = true, IsActive = true });
        await f.Db.SaveChangesAsync();
        var policy = new CompanyPPDMRowAccessPolicy(f.Gate, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Metadata.Object, f.Eligibility, f.Editor.Object, f.Columns.Object, f.Defaults.Object, Microsoft.Extensions.Options.Options.Create(f.EligibilityConfiguration), "reader");
        await Assert.ThrowsAsync<RefusalException>(() => policy.GetReadConjunctionsAsync(typeof(PROJECT_CONDITION), "PROJECT_CONDITION")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("failed")] [InlineData("ignored")]
    public async Task Provider_failure_or_ignored_reference_predicates_cannot_supply_HAZOP_vocabulary(string fault)
    {
        using var f = await Ready();
        if (fault == "failed")
        {
            var error = new TheTechIdea.Beep.ConfigUtil.ErrorsInfo { Flag = TheTechIdea.Beep.ConfigUtil.Errors.Ok }; f.Source.SetupGet(source => source.ErrorObject).Returns(error);
            f.Source.Setup(source => source.GetEntityAsync("PROJECT_STEP", It.IsAny<List<AppFilter>>())).ReturnsAsync(() => { error.Flag = TheTechIdea.Beep.ConfigUtil.Errors.Failed; return f.Rows.OfType<PROJECT_STEP>().Where(row => row.PROJECT_ID == "local").Cast<object>().ToArray(); });
            await Assert.ThrowsAsync<InvalidOperationException>(() => f.Repository(typeof(PROJECT_STEP)).GetAsync(new()));
        }
        else
        {
            f.Source.Setup(source => source.GetEntityAsync("R_PROJECT_TYPE", It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[] { new R_PROJECT_TYPE { PROJECT_TYPE = "other-project", ACTIVE_IND = "Y" } });
            await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_STEP)).GetAsync(new()));
        }
        Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("node")] [InlineData("guide-word")] [InlineData("status-type")] [InlineData("status")] [InlineData("other-project-status-type")]
    public async Task Recorded_reference_codes_must_match_the_exact_HAZOP_parent_vocabulary(string fault)
    {
        using var f = await Ready();
        if (fault == "node") f.Rows.RemoveAll(row => row is R_PROJ_STEP_TYPE);
        else if (fault == "guide-word") f.Rows.RemoveAll(row => row is R_PROJECT_CONDITION);
        else if (fault == "status-type") f.Rows.RemoveAll(row => row is R_PROJECT_STATUS_TYPE);
        else if (fault == "status") f.Rows.RemoveAll(row => row is R_PROJECT_STATUS);
        else f.Rows.OfType<R_PROJECT_STATUS_TYPE>().Single(row => row.STATUS_TYPE == "HAZOP_DEVIATION").PROJECT_TYPE = "other-project";
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetNodesAsync("local")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("reparent")] [InlineData("revoke")]
    public async Task Current_authority_changes_during_a_child_query_refuse_the_returned_rows(string change)
    {
        using var f = await Ready(); f.Source.Setup(source => source.GetEntityAsync("PROJECT_CONDITION", It.IsAny<List<AppFilter>>())).Returns(async () =>
        {
            if (change == "reparent") f.Rows.OfType<PROJECT_COMPONENT>().Single(row => row.PROJECT_ID == "local").FIELD_ID = "B";
            else { (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive = false; await f.Db.SaveChangesAsync(); }
            return f.Rows.OfType<PROJECT_CONDITION>().Where(row => row.PROJECT_ID == "local").Cast<object>().ToArray();
        });
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_CONDITION)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Provider_ignoring_project_predicates_cannot_return_another_FIELDs_conditions()
    {
        using var f = await Ready(); f.Source.Setup(source => source.GetEntityAsync("PROJECT_CONDITION", It.IsAny<List<AppFilter>>())).ReturnsAsync(f.Rows.OfType<PROJECT_CONDITION>().Cast<object>().ToArray());
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_CONDITION)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Even_another_granted_FIELD_cannot_replace_the_current_study_FIELD()
    {
        using var f = await Ready(); await f.Grant("FIELD", "B", inherit: true);
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetNodesAsync("foreign"));
        f.Source.Verify(source => source.GetEntityAsync("PROJECT_CONDITION", It.IsAny<List<AppFilter>>()), Times.Never); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("key")] [InlineData("parent")]
    public async Task Canonical_project_keys_and_relationships_are_required_before_child_payload_access(string fault)
    {
        using var f = await Ready(); var definition = await f.Metadata.Object.GetTableMetadataAsync("PROJECT_STEP");
        if (fault == "key") definition.PrimaryKeyColumn = "PROJECT_ID";
        else definition.ForeignKeys.Single(key => key.ForeignKeyColumn == "PROJECT_ID" && key.ReferencedTable == "PROJECT").ReferencedPrimaryKey = "OTHER";
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_STEP)).GetAsync(new())); Assert.Empty(f.Writes);
        f.Source.Verify(source => source.GetEntityAsync("PROJECT_STEP", It.IsAny<List<AppFilter>>()), Times.Never);
    }
    [Theory] [InlineData(PPDMRowMutation.Insert)] [InlineData(PPDMRowMutation.Update)] [InlineData(PPDMRowMutation.Delete)]
    public async Task Generic_project_writes_remain_fenced_until_reviewed_durable_commands_exist(PPDMRowMutation operation)
    {
        using var f = await Ready(); foreach (var row in f.Rows.Where(row => row.GetType().GetProperty("PROJECT_ID")?.GetValue(row) as string == "local").ToArray())
            await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(row.GetType(), row.GetType().Name, row, operation == PPDMRowMutation.Insert ? null : row, operation, "actor"));
        Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("missing-node")] [InlineData("missing-condition")] [InlineData("other-node")]
    public async Task Missing_or_foreign_deviation_targets_cannot_be_reported_as_a_successful_update(string fault)
    {
        using var f = await Ready(); var node = fault == "missing-node" ? 99 : 1; var condition = fault == "missing-condition" ? 99 : fault == "other-node" ? 3 : 1;
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).UpdateDeviationStatusAsync("local", node, condition, "CLOSED", "actor")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("GetCAPlanAsync")] [InlineData("AddCorrectiveActionAsync")] [InlineData("AssignResponsiblePersonAsync")] [InlineData("SetDueDateAsync")] [InlineData("RecordCompletionAsync")] [InlineData("GetCAStatusAsync")] [InlineData("AllCAsMeetDeadlineAsync")]
    public async Task Explicit_mocked_incident_input_cannot_turn_an_ID_matching_HAZOP_project_into_a_corrective_action_plan(string operation)
    {
        using var f = await Ready(); Seed(f, "CA-incident", "A"); f.Rows.OfType<PROJECT>().Single(row => row.PROJECT_ID == "CA-incident").PROJECT_NUM = "incident";
        var incident = new Mock<Beep.OilandGas.Models.Core.Interfaces.IHSEService>();
        incident.Setup(owner => owner.GetByIdAsync("incident")).ReturnsAsync(new Beep.OilandGas.Models.Data.HSE.HSEIncidentRecord { IncidentId = "incident", FieldId = "A" });
        var owner = new CorrectiveActionService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, Profile(f), f.Policy, incident.Object, new PPDMCompanyCorrectiveActionCommands(f.Gate, Profile(f), new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy), NullLogger<CorrectiveActionService>.Instance);
        Func<Task> result = operation switch
        {
            "GetCAPlanAsync" => () => owner.GetCAPlanAsync("incident"),
            "AddCorrectiveActionAsync" => () => owner.AddCorrectiveActionAsync("incident", new(Guid.Parse("969bc148-1300-438b-9fb5-4df4599140ca"), "recorded action", "recorded-type", DateTime.UtcNow.AddDays(1), "recorded-participant"), "actor"),
            "AssignResponsiblePersonAsync" => () => owner.AssignResponsiblePersonAsync("incident", 1, "recorded-participant", "actor"),
            "SetDueDateAsync" => () => owner.SetDueDateAsync("incident", 1, DateTime.UtcNow.AddDays(1), "actor"),
            "RecordCompletionAsync" => () => owner.RecordCompletionAsync("incident", 1, "recorded completion", "actor"),
            "GetCAStatusAsync" => () => owner.GetCAStatusAsync("incident"),
            _ => () => owner.AllCAsMeetDeadlineAsync("incident")
        };
        await Assert.ThrowsAsync<RefusalException>(result); Assert.Empty(f.Writes);
        f.Source.Verify(source => source.GetEntityAsync("PROJECT_STEP", It.IsAny<List<AppFilter>>()), Times.Never);
    }
    [Theory] [InlineData(0, 1, "OPEN")] [InlineData(1, 0, "CLOSED")] [InlineData(1, 1, "open")] [InlineData(1, 1, "unknown")]
    public async Task Invalid_requested_status_or_identity_cannot_select_a_different_deviation(int node, int condition, string status)
    {
        using var f = await Ready(); var error = await Assert.ThrowsAsync<RefusalException>(() => Service(f).UpdateDeviationStatusAsync("local", node, condition, status, "actor"));
        Assert.Equal(RefusalKind.Invalid, error.Kind); Assert.Empty(f.Writes);
        f.Source.Verify(source => source.GetEntityAsync("PROJECT_CONDITION", It.IsAny<List<AppFilter>>()), Times.Never);
    }
}
