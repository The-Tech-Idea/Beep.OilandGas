using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyCorrectiveActionPlanCommandTests
{
    internal static CreateCAPlanRequest Request() => new(Guid.Parse("34f270c0-5a08-48b3-9d35-41334f649089"), "Recorded incident action plan", "CA-2026-017", "recorded-action-response", 17, 4, "local-person", new string('A', 64));
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Fixture, CAPlanCreationReview> Reviews = new();
    private static string Id => Request().PlanId.ToString("N");
    internal static async Task<Fixture> Ready()
    {
        var f = await CompanyIncidentCommandTests.Ready();
        f.Rows.RemoveAll(row => row is HSE_INCIDENT_RESPONSE response && response.INCIDENT_ID == "local" && response.PROJECT_ID == CompanyCorrectiveActionAuthorityTests.Plan ||
            row.GetType().GetProperty("PROJECT_ID")?.GetValue(row) as string == CompanyCorrectiveActionAuthorityTests.Plan);
        return f;
    }
    private static async Task<string> Create(Fixture f, CreateCAPlanRequest? request = null, string incident = "local", string actor = "actor")
    {
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        var review = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        request ??= Request();
        if (request.ExpectedRevision == new string('A',64))
        {
            if (!Reviews.TryGetValue(f, out var catalog))
            {
                catalog = await commands.ReviewPlanCreationAsync(review, incident);
                Reviews.Add(f, catalog);
            }
            request = request with { ExpectedRevision = catalog.Revision };
        }
        return await commands.CreatePlanAsync(review, incident, request, actor);
    }
    [Fact]
    public async Task Creation_records_one_atomic_project_link_participant_and_role_then_replays_without_writes()
    {
        using var f = await Ready();
        Assert.Equal(Id, await Create(f)); Assert.Equal(4, f.Writes.Count); Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions);
        Assert.Equal(new[] { typeof(PROJECT), typeof(PROJECT_BA), typeof(PROJECT_BA_ROLE), typeof(HSE_INCIDENT_RESPONSE) }, f.Writes.Select(row => row.GetType()));
        var project = Assert.Single(f.Rows.OfType<PROJECT>(), row => row.PROJECT_ID == Id);
        Assert.Equal("CORRECTIVE_ACTION", project.PROJECT_TYPE); Assert.Equal(Request().PlanName, project.PROJECT_NAME); Assert.Equal(Request().PlanNumber, project.PROJECT_NUM);
        var role = Assert.Single(f.Rows.OfType<PROJECT_BA_ROLE>(), row => row.PROJECT_ID == Id); Assert.Equal(1, role.ROLE_SEQ_NO); Assert.Equal("RESPONSIBLE", role.ROLE); Assert.Equal("local-person", role.BUSINESS_ASSOCIATE_ID);
        var link = Assert.Single(f.Rows.OfType<HSE_INCIDENT_RESPONSE>(), row => row.PROJECT_ID == Id); Assert.Equal("local", link.INCIDENT_ID); Assert.Equal(2, link.RESPONSE_OBS_NO);
        Assert.Equal("actor", project.ROW_CREATED_BY); Assert.Equal("actor", link.ROW_CHANGED_BY); Assert.Equal(project.EFFECTIVE_DATE, link.EFFECTIVE_DATE);
        Assert.Equal(Id, (await CompanyCorrectiveActionAuthorityTests.Service(f).GetCAPlanAsync("local")).PlanId);
        Assert.Equal(Id, await Create(f)); Assert.Equal(4, f.Writes.Count); Assert.Null(f.LeaseActiveTransaction);
    }
    [Fact]
    public async Task Creation_allocates_after_inactive_response_history_without_changing_it()
    {
        using var f = await Ready(); f.Rows.Add(new HSE_INCIDENT_RESPONSE { INCIDENT_ID = "local", RESPONSE_OBS_NO = 41, ACTIVE_IND = "N", RESPONSE_TYPE = "recorded-action-response", REMARK = "historical evidence" });
        await Create(f); Assert.Equal(42, f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(row => row.PROJECT_ID == Id).RESPONSE_OBS_NO);
        Assert.Equal("historical evidence", f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(row => row.INCIDENT_ID == "local" && row.RESPONSE_OBS_NO == 41).REMARK);
    }
    [Theory] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public async Task Every_failed_compound_write_rolls_back_the_whole_effect(int write)
    {
        using var f = await Ready(); f.LeaseFailWriteNumber = write;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Create(f));
        Assert.DoesNotContain(f.Rows, row => row.GetType().GetProperty("PROJECT_ID")?.GetValue(row) as string == Id);
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); Assert.Null(f.LeaseActiveTransaction);
    }
    [Fact]
    public async Task Lost_commit_acknowledgment_reconciles_the_same_unchanged_effect_without_duplicate_writes()
    {
        using var f = await Ready(); f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Create(f)); Assert.Equal(4, f.Writes.Count);
        f.LeaseCommitFailed = false; Assert.Equal(Id, await Create(f)); Assert.Equal(4, f.Writes.Count);
        await Assert.ThrowsAsync<RefusalException>(() => Create(f, Request() with { PlanNumber = "changed-intent" })); Assert.Equal(4, f.Writes.Count);
    }
    [Theory] [InlineData("id")] [InlineData("name")] [InlineData("number")] [InlineData("status-response")] [InlineData("party")] [InlineData("role")]
    public async Task Incomplete_or_inferred_inputs_are_refused_before_transaction(string fault)
    {
        using var f = await Ready(); var request = fault switch {
            "id" => Request() with { PlanId = Guid.Empty }, "name" => Request() with { PlanName = " " }, "number" => Request() with { PlanNumber = new string('a', 129) },
            "status-response" => Request() with { ResponseType = "STATUS" }, "party" => Request() with { PartyObservation = 17.5m }, _ => Request() with { RoleObservation = -1 } };
        await Assert.ThrowsAsync<RefusalException>(() => Create(f, request)); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("response-type")] [InlineData("project-type")] [InlineData("responsible-role")] [InlineData("missing-party")] [InlineData("wrong-role-key")] [InlineData("app-user-ba")] [InlineData("expired-ba")]
    public async Task Creation_requires_actual_current_reference_and_full_incident_participant_evidence(string fault)
    {
        using var f = await Ready(); var request = Request();
        if (fault == "response-type") f.Rows.RemoveAll(row => row is R_HSE_RESPONSE_TYPE reference && reference.ACTION_TYPE == request.ResponseType);
        if (fault == "project-type") f.Rows.RemoveAll(row => row is R_PROJECT_TYPE);
        if (fault == "responsible-role") f.Rows.RemoveAll(row => row is R_PROJECT_BA_ROLE);
        if (fault == "missing-party") f.Rows.RemoveAll(row => row is HSE_INCIDENT_BA);
        if (fault == "wrong-role-key") request = request with { RoleObservation = 5 };
        if (fault == "app-user-ba") request = request with { ResponsibleBaId = "actor" };
        if (fault == "expired-ba") f.Rows.OfType<BUSINESS_ASSOCIATE>().Single(row => row.BUSINESS_ASSOCIATE_ID == "local-person").EXPIRY_DATE = DateTime.UtcNow.AddDays(-1);
        await Assert.ThrowsAsync<RefusalException>(() => Create(f, request)); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("foreign")] [InlineData("closed")] [InlineData("read-only")] [InlineData("permission")] [InlineData("restriction")]
    public async Task Current_company_case_state_write_permission_and_restrictions_are_independent_gates(string fault)
    {
        using var f = await Ready();
        if (fault == "closed") f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(row => row.INCIDENT_ID == "local" && row.RESPONSE_TYPE == "STATUS").RESPONSE_RESULT = IncidentState.Closed;
        if (fault == "read-only") { var grant = await f.Db.Set<AppUserAssetAccess>().SingleAsync(row => row.AssetId == "A"); grant.AccessLevel = "READ"; await f.Db.SaveChangesAsync(); }
        if (fault == "permission") { f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row => row.ClaimValue == PermissionConstants.HSE.ManageIncidents).ToArrayAsync()); await f.Db.SaveChangesAsync(); }
        if (fault == "restriction") CompanyHseRowAccessTests.Restrict(f, true);
        await Assert.ThrowsAsync<RefusalException>(() => Create(f, incident: fault == "foreign" ? "foreign" : "local")); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("orphan")] [InlineData("foreign-link")] [InlineData("existing-plan")] [InlineData("fractional-history")] [InlineData("duplicate-history")] [InlineData("exhausted-history")]
    public async Task Existing_graph_collisions_and_invalid_response_allocation_are_refused(string fault)
    {
        using var f = await Ready();
        if (fault == "orphan") f.Rows.Add(new PROJECT_BA { PROJECT_ID = Id, BUSINESS_ASSOCIATE_ID = "local-person", ACTIVE_IND = "N" });
        if (fault == "foreign-link") f.Rows.Add(new HSE_INCIDENT_RESPONSE { INCIDENT_ID = "foreign", RESPONSE_OBS_NO = 31, PROJECT_ID = Id, ACTIVE_IND = "Y" });
        if (fault == "existing-plan") f.Rows.Add(new HSE_INCIDENT_RESPONSE { INCIDENT_ID = "local", RESPONSE_OBS_NO = 31, PROJECT_ID = "some-other-project", ACTIVE_IND = "Y" });
        if (fault is "fractional-history" or "duplicate-history" or "exhausted-history") f.Rows.Add(new HSE_INCIDENT_RESPONSE { INCIDENT_ID = "local", RESPONSE_OBS_NO = fault == "fractional-history" ? 1.5m : fault == "duplicate-history" ? 1 : int.MaxValue, ACTIVE_IND = "N" });
        await Assert.ThrowsAsync<RefusalException>(() => Create(f)); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("reference-change")] [InlineData("provider-substitution")] [InlineData("history-change")]
    public async Task Evidence_drift_during_insertion_rolls_back_the_compound_plan(string fault)
    {
        using var f = await Ready(); var changed = false;
        f.AfterLeaseWrite = row => {
            if (changed) return; changed = true;
            if (fault == "reference-change") f.Rows.OfType<R_PROJECT_BA_ROLE>().Single().REMARK = "changed interpretation";
            if (fault == "provider-substitution") ((PROJECT)row).PROJECT_NAME = "provider substituted";
            if (fault == "history-change") f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(value => value.INCIDENT_ID == "local").REMARK = "changed surrounding history";
        };
        await Assert.ThrowsAsync<RefusalException>(() => Create(f)); Assert.True(changed);
        Assert.DoesNotContain(f.Rows, row => row.GetType().GetProperty("PROJECT_ID")?.GetValue(row) as string == Id); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Theory] [InlineData("no-op-begin")] [InlineData("isolation")]
    public async Task A_real_owned_serializable_transaction_is_required_before_writes(string fault)
    {
        using var f = await Ready(); f.LeaseBeginNoOp = fault == "no-op-begin"; if (fault == "isolation") f.TransactionIsolation = System.Data.IsolationLevel.ReadCommitted;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Create(f)); Assert.Empty(f.Writes); Assert.Null(f.LeaseActiveTransaction);
    }
    [Fact]
    public async Task Later_child_history_and_another_creation_identity_are_never_automatically_replayed()
    {
        using var f = await Ready(); await Create(f);
        await Assert.ThrowsAsync<RefusalException>(() => Create(f, Request() with { PlanId = Guid.NewGuid() }));
        f.Rows.Add(new PROJECT_STEP { PROJECT_ID = Id, STEP_ID = "later-action", STEP_SEQ_NO = 1, ACTIVE_IND = "Y" });
        await Assert.ThrowsAsync<RefusalException>(() => Create(f)); Assert.Equal(4, f.Writes.Count);
    }
    [Fact]
    public async Task Inventory_requires_the_exact_canonical_child_key_definition_before_transaction()
    {
        using var f = await Ready();
        f.Metadata.Setup(value => value.GetTableMetadataAsync("PROJECT_STEP")).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata { EntityTypeName = "PROJECT_STEP", PrimaryKeyColumn = "PROJECT_ID" });
        await Assert.ThrowsAsync<RefusalException>(() => Create(f)); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Fact]
    public async Task Provider_OK_without_a_participant_effect_rolls_back_all_saved_plan_rows()
    {
        using var f = await Ready();
        f.Source.Setup(value => value.InsertEntity("PROJECT_BA", It.IsAny<object>())).Returns(new TheTechIdea.Beep.ConfigUtil.ErrorsInfo { Flag = TheTechIdea.Beep.ConfigUtil.Errors.Ok });
        await Assert.ThrowsAsync<RefusalException>(() => Create(f));
        Assert.DoesNotContain(f.Rows, row => row.GetType().GetProperty("PROJECT_ID")?.GetValue(row) as string == Id); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Fact]
    public async Task A_one_shot_stored_effect_change_after_its_first_read_is_detected_before_commit()
    {
        using var f = await Ready(); var changed = false;
        f.AfterRead = () => {
            if (!changed && f.Writes.Count == 4 && f.Queries.Last().Any(filter => filter.FieldName == "PROJECT_ID" && filter.FilterValue == Id)) {
                changed = true; f.Rows.OfType<PROJECT>().Single(row => row.PROJECT_ID == Id).REMARK = "changed stored effect";
            }
            return Task.CompletedTask;
        };
        await Assert.ThrowsAsync<RefusalException>(() => Create(f)); Assert.True(changed);
        Assert.DoesNotContain(f.Rows, row => row.GetType().GetProperty("PROJECT_ID")?.GetValue(row) as string == Id); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Theory] [InlineData("revision")] [InlineData("selected-reference")] [InlineData("unselected-reference")] [InlineData("participant")] [InlineData("history")]
    public async Task Catalog_revision_refuses_changed_review_evidence_before_any_transaction(string fault)
    {
        using var f = await Ready(); var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!; var catalog = await commands.ReviewPlanCreationAsync(context, "local");
        var request = Request() with { ExpectedRevision = catalog.Revision };
        if (fault == "revision") request = request with { ExpectedRevision = new string('B',64) };
        if (fault == "selected-reference") f.Rows.OfType<R_PROJECT_BA_ROLE>().Single().REMARK = "Changed reference interpretation";
        if (fault == "unselected-reference") f.Rows.Add(new R_HSE_RESPONSE_TYPE { ACTION_TYPE = "retired-other-response", ACTIVE_IND = "N" });
        if (fault == "participant") f.Rows.OfType<HSE_INCIDENT_BA>().First(row => row.INCIDENT_ID == "local").REMARK = "Changed recorded participant interpretation";
        if (fault == "history") f.Rows.Add(new HSE_INCIDENT_RESPONSE { INCIDENT_ID = "local", RESPONSE_OBS_NO = 30, ACTIVE_IND = "N", RESPONSE_TYPE = "recorded-action-response" });
        await Assert.ThrowsAsync<RefusalException>(() => commands.CreatePlanAsync(context, "local", request, "actor"));
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData(1)] [InlineData(4)]
    public async Task Catalog_change_during_compound_insertion_rolls_back_the_entire_effect(int write)
    {
        using var f = await Ready();
        f.AfterLeaseWrite = _ => { if (f.Writes.Count == write) f.Rows.Add(new R_HSE_RESPONSE_TYPE { ACTION_TYPE = "new-retired-code", ACTIVE_IND = "N" }); };
        await Assert.ThrowsAsync<RefusalException>(() => Create(f));
        Assert.DoesNotContain(f.Rows, row => row.GetType().GetProperty("PROJECT_ID")?.GetValue(row) as string == Id);
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Theory] [InlineData("")] [InlineData("short")] [InlineData("lowercase")]
    public async Task Catalog_revision_is_mandatory_and_canonical_before_reads_or_writes(string invalid)
    {
        using var f = await Ready(); var request = Request() with { ExpectedRevision = invalid == "lowercase" ? new string('a',64) : invalid };
        await Assert.ThrowsAsync<RefusalException>(() => Create(f, request)); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }

    [Theory] [InlineData(false, "grant")] [InlineData(false, "permission")] [InlineData(true, "grant")] [InlineData(true, "permission")]
    public async Task Catalog_capture_cannot_keep_write_authority_revoked_during_prepare_or_closing_confirmation(bool closing, string authority)
    {
        using var f = await Ready(); var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!; var catalog = await commands.ReviewPlanCreationAsync(context, "local");
        var request = Request() with { ExpectedRevision = catalog.Revision }; var changed = false;
        f.AfterRead = async () => {
            if (changed || f.Writes.Count != (closing ? 4 : 0) || f.Queries.Last().Count != 1 || !f.Queries.Last().Any(filter => filter.FieldName == "ACTIVE_IND" && filter.FilterValue == "N")) return;
            changed = true;
            if (authority == "grant") (await f.Db.Set<AppUserAssetAccess>().SingleAsync(row => row.AssetId == "A")).AccessLevel = "READ";
            else f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row => row.ClaimValue == PermissionConstants.HSE.ManageIncidents).ToArrayAsync());
            await f.Db.SaveChangesAsync();
        };
        await Assert.ThrowsAsync<RefusalException>(() => commands.CreatePlanAsync(context, "local", request, "actor")); Assert.True(changed);
        Assert.DoesNotContain(f.Rows, row => row.GetType().GetProperty("PROJECT_ID")?.GetValue(row) as string == Id);
        if (closing) Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); else Assert.Empty(f.Writes);
    }

    [Theory] [InlineData("grant")] [InlineData("permission")]
    public async Task Prepared_plan_mutation_closes_write_authority_revoked_during_selected_reference_reads(string authority)
    {
        using var f = await Ready(); var changed = false;
        f.AfterRead = async () => {
            if (changed || f.Writes.Count != 1 || !f.Queries.Last().Any(filter => filter.FieldName == "ACTION_TYPE" && filter.FilterValue == Request().ResponseType)) return;
            changed = true;
            if (authority == "grant") (await f.Db.Set<AppUserAssetAccess>().SingleAsync(row => row.AssetId == "A")).AccessLevel = "READ";
            else f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row => row.ClaimValue == PermissionConstants.HSE.ManageIncidents).ToArrayAsync());
            await f.Db.SaveChangesAsync();
        };
        await Assert.ThrowsAsync<RefusalException>(() => Create(f)); Assert.True(changed);
        Assert.Single(f.Writes);
        Assert.DoesNotContain(f.Rows, row => row.GetType().GetProperty("PROJECT_ID")?.GetValue(row) as string == Id);
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }

}
