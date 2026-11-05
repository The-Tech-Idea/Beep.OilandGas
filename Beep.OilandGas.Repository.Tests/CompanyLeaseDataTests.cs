using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LeaseAcquisition.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyLeaseDataTests
{
    private static readonly DateTime Start = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task An_actor_with_both_FIELD_grants_cannot_read_or_write_a_B_lease_through_A()
    {
        using var f = await Ready(); await f.Grant("FIELD", "B", "WRITE"); f.Rows.Add(new FIELD { FIELD_ID = "B", ACTIVE_IND = "Y" }); AddLease(f, id: "lease-b", field: "B");
        var service = Service(f);
        await Assert.ThrowsAsync<RefusalException>(() => service.EvaluateLeaseAsync("lease-b", "A"));
        await Assert.ThrowsAsync<RefusalException>(() => service.UpdateLeaseStatusAsync("lease-b", "INACTIVE", "actor", "A"));
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
        Assert.Equal("B", (await service.EvaluateLeaseAsync("lease-b", "B")).FieldId);
        await service.UpdateLeaseStatusAsync("lease-b", "INACTIVE", "actor", "B");
        Assert.Equal("INACTIVE", (await service.EvaluateLeaseAsync("lease-b", "B")).Status); Assert.Equal(3, f.Writes.Count);
    }

    [Fact]
    public async Task The_actual_controller_uses_the_live_company_FIELD_gate_and_canonical_service()
    {
        using var f = await Ready(); var controller = new Beep.OilandGas.ApiService.Controllers.Operations.LeaseAcquisitionController(Service(f), Access(f));
        var result = await controller.EvaluateLease("A", "lease");
        Assert.Equal("A", Assert.IsType<LeaseSummary>(Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result).Value).FieldId);
        await controller.UpdateLeaseStatus("A", "lease", new() { Status = "INACTIVE" }); Assert.Equal(3, f.Writes.Count);
        f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync()); await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<RefusalException>(() => controller.UpdateLeaseStatus("A", "lease", new() { Status = "ACTIVE" }));
        Assert.Equal(3, f.Writes.Count);
    }

    [Theory]
    [InlineData("command")] [InlineData("context")] [InlineData("company")] [InlineData("database")]
    [InlineData("account")] [InlineData("field")] [InlineData("subtype")] [InlineData("component")]
    [InlineData("status-type")] [InlineData("zone")]
    public async Task Creation_intent_must_match_current_authority_and_interpretation_before_any_write(string fault)
    {
        using var f = await Ready(); var request = Request(f); var context = request.Context!;
        request.Context = fault switch {
            "context" => null, "company" => context with { Company = new("other", "other") },
            "database" => context with { DatabaseScopeFingerprint = "other" }, "account" => context with { ActorUserId = "other" },
            "field" => context with { FieldId = "B" }, "subtype" => context with { LandRightSubtype = "other" },
            "component" => context with { CompanyComponentType = "other" }, "status-type" => context with { StatusType = "other" },
            "zone" => context with { DateZoneId = "Asia/Kuwait" }, _ => context };
        if (fault == "command") request.CommandId = Guid.Empty;
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).CreateLeaseAcquisitionAsync(request, "actor"));
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).ReconcileLeaseCreationAsync(request, "actor"));
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
        Assert.DoesNotContain(f.LeaseQueries, query => query.Table.StartsWith("LAND_", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Creation_context_requires_current_Administrator_and_FIELD_WRITE()
    {
        using var f = await Ready(); var context = await Service(f).GetCreationContextAsync("A");
        Assert.Equal(Contexts.GetValue(f, _ => throw new Exception()), context);
        f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync()); await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetCreationContextAsync("A")); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Absent_creation_is_read_only_and_does_not_require_a_transaction_capable_driver()
    {
        using var f = await Ready(); f.LeaseBeginFailed = true; var request = Request(f);
        var receipt = await Service(f).ReconcileLeaseCreationAsync(request, "actor");
        Assert.Equal("NotRecorded", receipt.State); Assert.Null(receipt.LeaseId); Assert.Equal(request.CommandId, receipt.CommandId);
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }

    [Fact]
    public async Task Matching_committed_replay_returns_the_original_id_without_another_write_or_transaction()
    {
        using var f = await Ready(); var request = Request(f); var service = Service(f);
        var id = await service.CreateLeaseAcquisitionAsync(request, "actor"); f.LeaseBeginFailed = true;
        Assert.Equal(request.CommandId.ToString("N"), id);
        Assert.Equal(id, await Service(f).CreateLeaseAcquisitionAsync(request, "actor"));
        var receipt = await Service(f).ReconcileLeaseCreationAsync(request, "actor");
        Assert.Equal("Recorded", receipt.State); Assert.Equal(id, receipt.LeaseId); Assert.Equal(request.Context, receipt.Context);
        Assert.Equal(5, f.Writes.Count); Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions);
    }

    [Theory]
    [InlineData("name")] [InlineData("number")] [InlineData("date")] [InlineData("expiry")] [InlineData("term")] [InlineData("interest")]
    public async Task Reusing_a_recorded_command_with_different_business_values_is_a_conflict(string fault)
    {
        using var f = await Ready(); var request = Request(f); await Service(f).CreateLeaseAcquisitionAsync(request, "actor");
        if (fault == "name") request.LeaseName = "changed";
        if (fault == "number") request.LeaseNumber = "changed";
        if (fault == "date") request.EffectiveDate = Start.AddDays(1);
        if (fault == "expiry") request.ExpirationDate = Start.AddYears(50);
        if (fault == "term") request.PrimaryTermMonths = 24;
        if (fault == "interest") request.CalculatedInterest = .5m;
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).CreateLeaseAcquisitionAsync(request, "actor"));
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).ReconcileLeaseCreationAsync(request, "actor"));
        Assert.Equal(5, f.Writes.Count); Assert.Equal(2, f.LeaseTransactions.Count);
    }

    [Theory]
    [InlineData("LAND_RIGHT")] [InlineData("LAND_AGREEMENT")] [InlineData("LAND_STATUS")]
    [InlineData("LAND_RIGHT_FIELD")] [InlineData("LAND_RIGHT_COMPONENT")]
    public async Task Partial_registration_never_becomes_NotRecorded_or_a_replacement_insert(string table)
    {
        using var f = await Ready(); var request = Request(f); var id = await Service(f).CreateLeaseAcquisitionAsync(request, "actor");
        f.Rows.RemoveAll(row => row.GetType().Name == table && row.GetType().GetProperty("LAND_RIGHT_ID")!.GetValue(row) as string == id);
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).ReconcileLeaseCreationAsync(request, "actor"));
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).CreateLeaseAcquisitionAsync(request, "actor"));
        Assert.Equal(5, f.Writes.Count); Assert.Equal(2, f.LeaseTransactions.Count);
    }

    [Theory]
    [InlineData("LAND_RIGHT")] [InlineData("LAND_AGREEMENT")] [InlineData("LAND_STATUS")]
    [InlineData("LAND_RIGHT_FIELD")] [InlineData("LAND_RIGHT_COMPONENT")]
    public async Task Reconciliation_requires_the_original_actor_on_every_record(string table)
    {
        using var f = await Ready(); var request = Request(f); var id = await Service(f).CreateLeaseAcquisitionAsync(request, "actor");
        var row = f.Rows.Single(row => row.GetType().Name == table && row.GetType().GetProperty("LAND_RIGHT_ID")!.GetValue(row) as string == id);
        row.GetType().GetProperty("ROW_CREATED_BY")!.SetValue(row, "other");
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).ReconcileLeaseCreationAsync(request, "actor")); Assert.Equal(5, f.Writes.Count);
    }

    [Fact]
    public async Task Uncertain_committed_creation_can_be_reconciled_without_resending_a_write()
    {
        using var f = await Ready(); var request = Request(f); f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(f).CreateLeaseAcquisitionAsync(request, "actor"));
        var receipt = await Service(f).ReconcileLeaseCreationAsync(request, "actor");
        Assert.Equal("Recorded", receipt.State); Assert.Equal(request.CommandId.ToString("N"), receipt.LeaseId);
        Assert.Equal(5, f.Writes.Count); Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions);
    }

    [Fact]
    public async Task Confirmed_rollback_can_retry_the_same_intent_and_id_explicitly()
    {
        using var f = await Ready(); var request = Request(f); f.LeaseFailWriteNumber = 2;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(f).CreateLeaseAcquisitionAsync(request, "actor"));
        Assert.Equal("NotRecorded", (await Service(f).ReconcileLeaseCreationAsync(request, "actor")).State);
        f.LeaseFailWriteNumber = 0; f.Writes.Clear();
        Assert.Equal(request.CommandId.ToString("N"), await Service(f).CreateLeaseAcquisitionAsync(request, "actor"));
        Assert.Equal(5, f.Writes.Count); Assert.Equal(new[] { "begin", "rollback", "begin", "commit" }, f.LeaseTransactions);
    }

    [Fact]
    public async Task Original_registration_remains_reconcilable_after_an_operational_status_change()
    {
        using var f = await Ready(); var request = Request(f); var service = Service(f);
        var id = await service.CreateLeaseAcquisitionAsync(request, "actor"); await service.UpdateLeaseStatusAsync(id, "INACTIVE", "actor", "A");
        Assert.Equal("INACTIVE", (await service.EvaluateLeaseAsync(id, "A")).Status);
        Assert.Equal("Recorded", (await service.ReconcileLeaseCreationAsync(request, "actor")).State);
        Assert.Equal(id, await service.CreateLeaseAcquisitionAsync(request, "actor")); Assert.Equal(8, f.Writes.Count);
    }

    [Fact]
    public async Task Mutable_request_changes_during_authority_resolution_cannot_change_the_frozen_intent()
    {
        using var f = await Ready(); var request = Request(f); var id = request.CommandId.ToString("N");
        f.AfterLeaseRead = () => { request.CommandId = Guid.NewGuid(); request.LeaseName = "changed"; request.CalculatedInterest = .9m; return Task.CompletedTask; };
        Assert.Equal(id, await Service(f).CreateLeaseAcquisitionAsync(request, "actor"));
        var right = f.Rows.OfType<LAND_RIGHT>().Single(row => row.LAND_RIGHT_ID == id);
        Assert.Equal("Explicit lease", right.REMARK); Assert.Equal(.25m, right.CALCULATED_INTEREST);
    }
    private static PPDMCompanyLeaseDataAccess Access(Fixture f) => new(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy, Options.Create(f.EligibilityConfiguration));
    private static ILeaseAcquisitionService Service(Fixture f) => new LeaseAcquisitionService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, Access(f));
    private static async Task<Fixture> Ready(string level = "WRITE")
    {
        var f = await Fixture.Create(); await f.Grant("FIELD", "A", level); f.Rows.Add(new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" });
        var rules = f.EligibilityConfiguration.Companies.Single(); rules.LeaseLandRightSubtype = "test-lease"; rules.LeaseCompanyComponentType = "test-company-data"; rules.LeaseStatusType = "test-operation";
        f.Rows.AddRange([new R_LAND_COMPONENT_TYPE { LAND_COMPONENT_TYPE = "test-company-data", ACTIVE_IND = "Y" },
            new R_LAND_AGREE_TYPE { LAND_AGREE_TYPE = "test-agreement", ACTIVE_IND = "Y" },
            new R_LAND_RIGHT_STATUS { STATUS_TYPE = "test-operation", LAND_RIGHT_STATUS = "ACTIVE", ACTIVE_IND = "Y" },
            new R_LAND_RIGHT_STATUS { STATUS_TYPE = "test-operation", LAND_RIGHT_STATUS = "INACTIVE", ACTIVE_IND = "Y" }]);
        AddLease(f); f.EnableLeaseTransactions(); var scope = await Access(f).ResolveAsync();
        Contexts.Add(f, new(scope.Company, scope.Database.Fingerprint, scope.ActorUserId, "A", scope.LandRightSubtype, scope.CompanyComponentType, scope.StatusType, scope.DateZoneId)); f.LeaseQueries.Clear(); return f;
    }
    private static void AddLease(Fixture f, string subtype = "test-lease", string company = "company", string id = "lease", string field = "A") =>
        f.Rows.AddRange([new LAND_RIGHT_COMPONENT { LAND_RIGHT_SUBTYPE = subtype, LAND_RIGHT_ID = id, COMPONENT_OBS_NO = 4, BUSINESS_ASSOCIATE_ID = company,
                FIELD_ID = field, LAND_COMPONENT_TYPE = "test-company-data", ACTIVE_IND = "Y", EFFECTIVE_DATE = Start },
            new LAND_RIGHT_FIELD { LAND_RIGHT_SUBTYPE = subtype, LAND_RIGHT_ID = id, FIELD_ID = field, ACTIVE_IND = "Y", EFFECTIVE_DATE = Start },
            new LAND_RIGHT { LAND_RIGHT_SUBTYPE = subtype, LAND_RIGHT_ID = id, ACTIVE_IND = "Y", EFFECTIVE_DATE = Start, CASE_SERIAL_NUM = "recorded-number", REMARK = "Recorded lease", CALCULATED_INTEREST = .375m },
            new LAND_AGREEMENT { LAND_RIGHT_SUBTYPE = subtype, LAND_RIGHT_ID = id, ACTIVE_IND = "Y", EFFECTIVE_DATE = Start, LAND_AGREE_TYPE = "test-agreement" },
            new LAND_STATUS { LAND_RIGHT_SUBTYPE = subtype, LAND_RIGHT_ID = id, STATUS_TYPE = "test-operation", LAND_RIGHT_STATUS = "ACTIVE", STATUS_SEQ_NO = 2,
                ACTIVE_IND = "Y", EFFECTIVE_DATE = Start, EFFECTIVE_TERM = "12", EFFECTIVE_TERM_OUOM = "MONTH" }]);
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Fixture, LeaseCreationContext> Contexts = new();
    private static CreateLeaseAcquisition Request(Fixture f) => new() { CommandId = Guid.NewGuid(), Context = Contexts.GetValue(f, _ => throw new InvalidOperationException("Missing reviewed test context")), PropertyId = "A", LeaseNumber = "new-number", LeaseName = "Explicit lease", LeaseType = "test-agreement", EffectiveDate = Start, CalculatedInterest = .25m, PrimaryTermMonths = 12 };

    [Fact]
    public async Task Canonical_owner_returns_stored_status_and_exact_company_field_composite_identity_without_an_opportunity_recommendation()
    {
        using var f = await Ready(); AddLease(f, company: "other", id: "foreign"); AddLease(f, subtype: "other-subtype");
        var service = Service(f); var row = Assert.Single(await service.GetAvailableLeasesAsync(new() { ["FieldId"] = "A" }));
        Assert.Equal("lease", row.LeaseId); Assert.Equal("test-lease", row.LandRightSubtype); Assert.Equal("UTC", row.DateZoneId);
        Assert.Equal("ACTIVE", row.Status); Assert.Equal("A", row.FieldId); Assert.Equal(.375m, row.CalculatedInterest); Assert.Null(row.WorkingInterest); Assert.Equal(12, row.PrimaryTermMonths);
        Assert.Equal("ACTIVE", (await service.EvaluateLeaseAsync("lease", "A")).Status); Assert.Empty(f.Writes);
        Assert.All(f.LeaseQueries.Where(query => query.Table.StartsWith("LAND_", StringComparison.Ordinal)), query => Assert.Contains(query.Filters, filter => filter.FieldName == "LAND_RIGHT_SUBTYPE" && filter.FilterValue == "test-lease"));
        Assert.All(f.LeaseQueries.Where(query => query.Table is "LAND_RIGHT" or "LAND_AGREEMENT" or "LAND_STATUS"), query => Assert.Contains(query.Filters, filter => filter.FieldName == "LAND_RIGHT_ID" && filter.FilterValue == "lease"));
    }
    [Theory]
    [InlineData("company")] [InlineData("inactive")] [InlineData("expired")] [InlineData("component-type")] [InlineData("subtype")]
    public async Task Noncurrent_or_other_company_associations_cannot_return_lease_records(string fault)
    {
        using var f = await Ready(); var link = f.Rows.OfType<LAND_RIGHT_COMPONENT>().Single();
        if (fault == "company") link.BUSINESS_ASSOCIATE_ID = "other";
        if (fault == "inactive") link.ACTIVE_IND = "N";
        if (fault == "expired") link.EXPIRY_DATE = Start.AddDays(1);
        if (fault == "component-type") link.LAND_COMPONENT_TYPE = "other";
        if (fault == "subtype") link.LAND_RIGHT_SUBTYPE = "other";
        Assert.Empty(await Service(f).GetAvailableLeasesAsync(new() { ["FieldId"] = "A" })); Assert.Empty(f.Writes);
    }
    [Theory]
    [InlineData("no-resource")] [InlineData("raw-column")] [InlineData("conflicting-field")] [InlineData("lease-only")]
    public async Task Broad_or_arbitrary_query_filters_never_open_a_business_repository(string fault)
    {
        using var f = await Ready(); var filters = fault switch { "no-resource" => new Dictionary<string, string>(), "lease-only" => new() { ["LeaseId"] = "lease" }, "raw-column" => new() { ["FieldId"] = "A", ["LAND_RIGHT_ID"] = "lease" }, _ => new() { ["FieldId"] = "A", ["PropertyId"] = "B" } };
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetAvailableLeasesAsync(filters)); Assert.Empty(f.LeaseQueries);
    }
    [Theory]
    [InlineData("field-grant")] [InlineData("field-entitlement")] [InlineData("inactive-field")]
    public async Task Missing_real_FIELD_access_prevents_lease_association_reads(string fault)
    {
        using var f = await Ready();
        if (fault == "field-grant") (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive = false;
        if (fault == "field-entitlement") f.Rows.OfType<ENTITLEMENT>().Single().ACTIVE_IND = "N";
        if (fault == "inactive-field") f.Rows.OfType<FIELD>().Single().ACTIVE_IND = "N";
        await f.Db.SaveChangesAsync(); await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetAvailableLeasesAsync(new() { ["FieldId"] = "A" }));
        Assert.DoesNotContain(f.LeaseQueries, query => query.Table == "LAND_RIGHT_COMPONENT"); Assert.Empty(f.Writes);
    }
    [Theory]
    [InlineData("missing-field-link")] [InlineData("duplicate-component")] [InlineData("wrong-linked-field")]
    [InlineData("duplicate-right")] [InlineData("term-unit")] [InlineData("term-value")] [InlineData("fractional-key")]
    public async Task Incomplete_ambiguous_or_malformed_land_evidence_is_an_explicit_error(string fault)
    {
        using var f = await Ready();
        if (fault == "missing-field-link") f.Rows.RemoveAll(row => row is LAND_RIGHT_FIELD);
        if (fault == "duplicate-component") f.Rows.Add(f.Rows.OfType<LAND_RIGHT_COMPONENT>().Single());
        if (fault == "wrong-linked-field") f.Rows.OfType<LAND_RIGHT_FIELD>().Single().FIELD_ID = "B";
        if (fault == "duplicate-right") f.Rows.Add(f.Rows.OfType<LAND_RIGHT>().Single());
        if (fault == "term-unit") f.Rows.OfType<LAND_STATUS>().Single().EFFECTIVE_TERM_OUOM = "DAY";
        if (fault == "term-value") f.Rows.OfType<LAND_STATUS>().Single().EFFECTIVE_TERM = "unknown";
        if (fault == "fractional-key") f.Rows.OfType<LAND_RIGHT_COMPONENT>().Single().COMPONENT_OBS_NO = 4.5m;
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).EvaluateLeaseAsync("lease", "A")); Assert.Empty(f.Writes);
    }
    [Theory]
    [InlineData("configuration")] [InlineData("code")] [InlineData("metadata")]
    public async Task Missing_reviewed_configuration_reference_or_metadata_never_infers_lease_meaning(string fault)
    {
        using var f = await Ready();
        if (fault == "configuration") f.EligibilityConfiguration.Companies.Single().LeaseCompanyComponentType = "";
        if (fault == "code") f.Rows.OfType<R_LAND_COMPONENT_TYPE>().Single().ACTIVE_IND = "N";
        if (fault == "metadata") f.Metadata.Setup(value => value.GetTableMetadataAsync("LAND_RIGHT")).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata { EntityTypeName = "LAND_RIGHT", PrimaryKeyColumn = "LAND_RIGHT_ID" });
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetAvailableLeasesAsync(new() { ["FieldId"] = "A" })); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Provider_that_ignores_company_or_subtype_predicates_cannot_return_foreign_leases()
    {
        using var f = await Ready(); AddLease(f, company: "other", id: "foreign"); f.IgnoreLeaseFilters = true;
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetAvailableLeasesAsync(new() { ["FieldId"] = "A" })); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Missing_status_is_not_inferred_from_the_active_record_flag()
    {
        using var f = await Ready(); f.Rows.RemoveAll(row => row is LAND_STATUS); var row = await Service(f).EvaluateLeaseAsync("lease", "A");
        Assert.Equal("", row.Status); Assert.Null(row.PrimaryTermMonths); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Creation_uses_current_actor_and_explicit_company_FIELD_links_without_a_title_record()
    {
        using var f = await Ready(); var id = await Service(f).CreateLeaseAcquisitionAsync(Request(f), "actor"); Assert.True(Guid.TryParseExact(id, "N", out _)); Assert.Equal(5, f.Writes.Count);
        var component = Assert.Single(f.Writes.OfType<LAND_RIGHT_COMPONENT>()); Assert.Equal("company", component.BUSINESS_ASSOCIATE_ID); Assert.Equal("A", component.FIELD_ID); Assert.Equal(id, component.LAND_RIGHT_ID);
        var link = Assert.Single(f.Writes.OfType<LAND_RIGHT_FIELD>()); Assert.Equal("test-lease", link.LAND_RIGHT_SUBTYPE); Assert.Equal(id, link.LAND_RIGHT_ID);
        Assert.Empty(f.Writes.OfType<LAND_TITLE>()); f.Columns.Verify(value => value.PrepareForInsert(It.IsAny<IPPDMEntity>(), "actor"), Times.Exactly(5));
    }
    [Theory]
    [InlineData("actor")] [InlineData("missing-actor")] [InlineData("empty-actor")] [InlineData("field-level")] [InlineData("administrator")] [InlineData("code")] [InlineData("default-date")] [InlineData("missing-calculated-interest")]
    public async Task Creation_cannot_write_without_current_resource_actor_role_and_explicit_evidence(string fault)
    {
        using var f = await Ready(fault == "field-level" ? "READ" : "WRITE"); var request = Request(f);
        if (fault == "administrator") { f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync()); await f.Db.SaveChangesAsync(); }
        if (fault == "code") request.LeaseType = "unreviewed";
        if (fault == "default-date") request.EffectiveDate = default;
        if (fault == "missing-calculated-interest") request.CalculatedInterest = null;
        var actor = fault switch { "actor" => "spoofed", "missing-actor" => null!, "empty-actor" => "", _ => "actor" };
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).CreateLeaseAcquisitionAsync(request, actor)); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Status_command_updates_only_the_complete_company_lease_identity_and_appends_actual_status_history()
    {
        using var f = await Ready(); AddLease(f, subtype: "other-subtype"); await Service(f).UpdateLeaseStatusAsync("lease", "INACTIVE", "actor", "A");
        Assert.Equal(3, f.Writes.Count); Assert.All(f.Writes, row => Assert.Equal("test-lease", row.GetType().GetProperty("LAND_RIGHT_SUBTYPE")!.GetValue(row)));
        var status = Assert.Single(f.Writes.OfType<LAND_STATUS>()); Assert.Equal(3, status.STATUS_SEQ_NO); Assert.Equal("INACTIVE", status.LAND_RIGHT_STATUS); Assert.Equal("test-operation", status.STATUS_TYPE);
        Assert.Equal("Y", f.Rows.OfType<LAND_RIGHT>().Single(row => row.LAND_RIGHT_SUBTYPE == "other-subtype").ACTIVE_IND);
    }
    [Theory]
    [InlineData("grant")] [InlineData("membership")] [InlineData("configuration")]
    public async Task Authority_or_interpretation_changed_during_lease_reads_prevents_return(string fault)
    {
        using var f = await Ready(); var changed = false;
        f.AfterLeaseRead = async () => {
            if (changed || f.LeaseQueries.Last().Table != "LAND_RIGHT") return; changed = true;
            if (fault == "grant") (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive = false;
            if (fault == "membership") (await f.Db.Set<AppCompanyMembership>().SingleAsync()).EffectiveToUtc = DateTime.UtcNow.AddSeconds(-1);
            if (fault == "configuration") f.EligibilityConfiguration.Companies.Single().LeaseLandRightSubtype = "changed";
            await f.Db.SaveChangesAsync();
        };
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).EvaluateLeaseAsync("lease", "A")); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Reported_provider_failure_remains_an_error_before_any_lease_write()
    {
        using var f = await Ready(); f.Source.SetupGet(value => value.ErrorObject).Returns(new ErrorsInfo { Flag = Errors.Failed });
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(f).GetAvailableLeasesAsync(new() { ["FieldId"] = "A" })); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("company")] [InlineData("field")] [InlineData("link")] [InlineData("code")]
    public async Task Association_or_reference_revoked_during_evidence_read_cannot_return_a_lease(string fault)
    {
        using var f = await Ready(); var changed = false;
        f.AfterLeaseRead = () => {
            if (!changed && f.LeaseQueries.Last().Table == "LAND_RIGHT") {
                changed = true;
                if (fault == "company") f.Rows.OfType<LAND_RIGHT_COMPONENT>().Single().BUSINESS_ASSOCIATE_ID = "other";
                if (fault == "field") f.Rows.OfType<LAND_RIGHT_COMPONENT>().Single().FIELD_ID = "B";
                if (fault == "link") f.Rows.OfType<LAND_RIGHT_FIELD>().Single().ACTIVE_IND = "N";
                if (fault == "code") f.Rows.OfType<R_LAND_COMPONENT_TYPE>().Single().ACTIVE_IND = "N";
            }
            return Task.CompletedTask;
        };
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).EvaluateLeaseAsync("lease", "A")); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("subtype")] [InlineData("id")] [InlineData("company")]
    [InlineData("field")] [InlineData("observation")] [InlineData("status-sequence")]
    public async Task Mutation_preparation_cannot_change_a_reviewed_complete_key_or_company_FIELD(string fault)
    {
        using var f = await Ready(); var target = fault switch { "company" or "field" or "observation" => typeof(LAND_RIGHT_COMPONENT), "status-sequence" => typeof(LAND_STATUS), _ => typeof(LAND_RIGHT) };
        f.Columns.Setup(value => value.PrepareForInsert(It.IsAny<IPPDMEntity>(), "actor")).Callback((IPPDMEntity entity, string _) => {
            if (entity.GetType() != target) return;
            if (fault == "subtype") ((LAND_RIGHT)entity).LAND_RIGHT_SUBTYPE = "other";
            if (fault == "id") ((LAND_RIGHT)entity).LAND_RIGHT_ID = "other";
            if (fault == "company") ((LAND_RIGHT_COMPONENT)entity).BUSINESS_ASSOCIATE_ID = "other";
            if (fault == "field") ((LAND_RIGHT_COMPONENT)entity).FIELD_ID = "B";
            if (fault == "observation") ((LAND_RIGHT_COMPONENT)entity).COMPONENT_OBS_NO = 99;
            if (fault == "status-sequence") ((LAND_STATUS)entity).STATUS_SEQ_NO = 99;
        });
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).CreateLeaseAcquisitionAsync(Request(f), "actor"));
        Assert.DoesNotContain(f.Writes, row => row.GetType() == target);
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
        Assert.Single(f.Rows.OfType<LAND_RIGHT>()); // Earlier attempted inserts were rolled back.
    }

    [Theory]
    [InlineData("association")] [InlineData("link")] [InlineData("id")]
    public async Task Status_preparation_rechecks_live_association_and_exact_update_identity_before_provider_write(string fault)
    {
        using var f = await Ready();
        f.Columns.Setup(value => value.PrepareForUpdate(It.IsAny<IPPDMEntity>(), "actor")).Callback((IPPDMEntity entity, string _) => {
            if (fault == "association") f.Rows.OfType<LAND_RIGHT_COMPONENT>().Single().ACTIVE_IND = "N";
            if (fault == "link") f.Rows.OfType<LAND_RIGHT_FIELD>().Single().ACTIVE_IND = "N";
            if (fault == "id") ((LAND_RIGHT)entity).LAND_RIGHT_ID = "other";
        });
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).UpdateLeaseStatusAsync("lease", "INACTIVE", "actor", "A")); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("working")] [InlineData("net")] [InlineData("royalty")] [InlineData("mineral")]
    [InlineData("surface")] [InlineData("agency")] [InlineData("federal")] [InlineData("indian")]
    public async Task Unsupported_economic_owner_or_government_terms_are_never_silently_discarded(string fault)
    {
        using var f = await Ready(); var request = Request(f);
        if (fault == "working") request.WorkingInterest = .1m;
        if (fault == "net") request.NetRevenueInterest = .1m;
        if (fault == "royalty") request.RoyaltyRate = .1m;
        if (fault == "mineral") request.MineralOwnerBaId = "owner";
        if (fault == "surface") request.SurfaceOwnerBaId = "owner";
        if (fault == "agency") request.GovernmentAgency = "agency";
        if (fault == "federal") request.IsFederal = false;
        if (fault == "indian") request.IsIndian = false;
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).CreateLeaseAcquisitionAsync(request, "actor")); Assert.Empty(f.Writes); Assert.Empty(f.LeaseQueries);
    }

    [Theory]
    [InlineData("future")] [InlineData("inactive")] [InlineData("expired")]
    public async Task Noncurrent_status_history_does_not_replace_the_recorded_current_status(string fault)
    {
        using var f = await Ready(); f.Rows.Add(new LAND_STATUS { LAND_RIGHT_SUBTYPE = "test-lease", LAND_RIGHT_ID = "lease", STATUS_TYPE = "test-operation",
            LAND_RIGHT_STATUS = "INACTIVE", STATUS_SEQ_NO = 3, ACTIVE_IND = fault == "inactive" ? "N" : "Y",
            EFFECTIVE_DATE = fault == "future" ? DateTime.UtcNow.AddDays(1) : Start.AddDays(1), EXPIRY_DATE = fault == "expired" ? Start.AddDays(2) : null });
        Assert.Equal("ACTIVE", (await Service(f).EvaluateLeaseAsync("lease", "A")).Status); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Reference_revoked_during_status_preparation_cannot_insert_a_status_record()
    {
        using var f = await Ready();
        f.Columns.Setup(value => value.PrepareForInsert(It.IsAny<IPPDMEntity>(), "actor")).Callback((IPPDMEntity _, string _) =>
            f.Rows.OfType<R_LAND_RIGHT_STATUS>().Single(row => row.LAND_RIGHT_STATUS == "INACTIVE").ACTIVE_IND = "N");
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).UpdateLeaseStatusAsync("lease", "INACTIVE", "actor", "A"));
        Assert.Empty(f.Writes.OfType<LAND_STATUS>()); Assert.Equal(2, f.Writes.Count);
    }

    [Fact]
    public async Task Status_command_preserves_the_current_recorded_term_instead_of_future_history()
    {
        using var f = await Ready(); f.Rows.Add(new LAND_STATUS { LAND_RIGHT_SUBTYPE = "test-lease", LAND_RIGHT_ID = "lease", STATUS_TYPE = "test-operation",
            LAND_RIGHT_STATUS = "ACTIVE", STATUS_SEQ_NO = 7, ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow.AddDays(1), EFFECTIVE_TERM = "999", EFFECTIVE_TERM_OUOM = "DAY" });
        await Service(f).UpdateLeaseStatusAsync("lease", "INACTIVE", "actor", "A");
        var added = Assert.Single(f.Writes.OfType<LAND_STATUS>()); Assert.Equal(8, added.STATUS_SEQ_NO); Assert.Equal("12", added.EFFECTIVE_TERM); Assert.Equal("MONTH", added.EFFECTIVE_TERM_OUOM);
    }

    [Fact]
    public async Task An_explicit_zero_calculated_interest_remains_recorded_without_implying_financial_working_interest()
    {
        using var f = await Ready(); f.Rows.OfType<LAND_RIGHT>().Single().CALCULATED_INTEREST = 0;
        var row = await Service(f).EvaluateLeaseAsync("lease", "A"); Assert.Equal(0, row.CalculatedInterest); Assert.Null(row.WorkingInterest);
        var request = Request(f); request.CalculatedInterest = 0;
        await Service(f).CreateLeaseAcquisitionAsync(request, "actor"); Assert.Equal(0, Assert.Single(f.Writes.OfType<LAND_RIGHT>()).CALCULATED_INTEREST);
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public async Task Failed_creation_write_rolls_back_every_earlier_record_without_retry(int write)
    {
        using var f = await Ready(); f.LeaseFailWriteNumber = write;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(f).CreateLeaseAcquisitionAsync(Request(f), "actor"));
        Assert.Equal(write, f.Writes.Count); Assert.Single(f.Rows.OfType<LAND_RIGHT>()); Assert.Single(f.Rows.OfType<LAND_AGREEMENT>());
        Assert.Single(f.Rows.OfType<LAND_STATUS>()); Assert.Single(f.Rows.OfType<LAND_RIGHT_FIELD>()); Assert.Single(f.Rows.OfType<LAND_RIGHT_COMPONENT>());
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); Assert.Null(f.LeaseActiveTransaction);
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public async Task Failed_status_write_preserves_header_agreement_and_history(int write)
    {
        using var f = await Ready(); f.LeaseFailWriteNumber = write;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(f).UpdateLeaseStatusAsync("lease", "INACTIVE", "actor", "A"));
        Assert.Equal("Y", f.Rows.OfType<LAND_RIGHT>().Single().ACTIVE_IND); Assert.Equal("Y", f.Rows.OfType<LAND_AGREEMENT>().Single().ACTIVE_IND);
        Assert.Equal("ACTIVE", Assert.Single(f.Rows.OfType<LAND_STATUS>()).LAND_RIGHT_STATUS);
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); Assert.Equal(write, f.Writes.Count);
    }

    [Theory]
    [InlineData("no-op")] [InlineData("failed")] [InlineData("borrowed")]
    public async Task Unverified_or_borrowed_transaction_never_reaches_a_provider_write(string fault)
    {
        using var f = await Ready(); f.LeaseBeginNoOp = fault == "no-op"; f.LeaseBeginFailed = fault == "failed";
        if (fault == "borrowed") f.LeaseActiveTransaction = Mock.Of<System.Data.IDbTransaction>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(f).CreateLeaseAcquisitionAsync(Request(f), "actor"));
        Assert.Empty(f.Writes); Assert.DoesNotContain("commit", f.LeaseTransactions); Assert.DoesNotContain("rollback", f.LeaseTransactions);
        Assert.Equal(fault == "borrowed" ? 0 : 1, f.LeaseTransactions.Count);
    }

    [Fact]
    public async Task A_driver_without_owned_transaction_evidence_is_refused_before_begin()
    {
        var source = new Mock<TheTechIdea.Beep.IDataSource>(MockBehavior.Strict); var ran = false;
        await Assert.ThrowsAsync<DataSourceTransactionCapabilityException>(() => PPDMDataSourceTransaction.ExecuteAsync(source.Object, () => { ran = true; return Task.CompletedTask; }));
        Assert.Throws<DataSourceTransactionCapabilityException>(()=>PPDMDataSourceTransaction.RequireIdle(source.Object));
        Assert.False(ran); source.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("number")] [InlineData("interest")] [InlineData("actor")] [InlineData("date")] [InlineData("missing")]
    public async Task Mismatched_persistence_readback_rolls_back_the_whole_command(string fault)
    {
        using var f = await Ready(); f.AfterLeaseWrite = stored => {
            if (stored is not LAND_RIGHT right) return;
            if (fault == "number") right.CASE_SERIAL_NUM = "other";
            if (fault == "interest") right.CALCULATED_INTEREST = .75m;
            if (fault == "actor") right.ROW_CREATED_BY = "other";
            if (fault == "date") right.EFFECTIVE_DATE = Start.AddHours(1);
            if (fault == "missing") f.Rows.Remove(stored);
        };
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).CreateLeaseAcquisitionAsync(Request(f), "actor"));
        Assert.Single(f.Rows.OfType<LAND_RIGHT>()); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }

    [Fact]
    public async Task Failure_with_unconfirmed_rollback_is_explicit_and_never_reported_as_success()
    {
        using var f = await Ready(); f.LeaseFailWriteNumber = 2; f.LeaseRollbackFailed = true;
        var failure = await Assert.ThrowsAsync<AggregateException>(() => Service(f).CreateLeaseAcquisitionAsync(Request(f), "actor"));
        Assert.Equal(2, failure.InnerExceptions.Count); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
        Assert.DoesNotContain("commit", f.LeaseTransactions); Assert.NotNull(f.LeaseActiveTransaction);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task A_commit_failure_never_retries_and_preserves_uncertainty_if_the_driver_already_committed(bool alreadyCommitted)
    {
        using var f = await Ready(); f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = alreadyCommitted;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(f).CreateLeaseAcquisitionAsync(Request(f), "actor"));
        Assert.Equal(5, f.Writes.Count); Assert.Equal(alreadyCommitted ? 2 : 1, f.Rows.OfType<LAND_RIGHT>().Count());
        Assert.Equal(alreadyCommitted ? new[] { "begin", "commit" } : new[] { "begin", "commit", "rollback" }, f.LeaseTransactions);
    }

    [Fact]
    public async Task Success_requires_committed_records_and_a_fresh_second_readback()
    {
        using var f = await Ready(); var confirmed = false;
        f.AfterLeaseRead = () => {
            if (f.LeaseTransactions.LastOrDefault() == "commit" && f.LeaseQueries.Last().Table == "LAND_RIGHT") confirmed = true;
            return Task.CompletedTask;
        };
        var id = await Service(f).CreateLeaseAcquisitionAsync(Request(f), "actor");
        Assert.Equal(id, f.Rows.OfType<LAND_RIGHT>().Single(row => row.LAND_RIGHT_ID != "lease").LAND_RIGHT_ID);
        Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions); Assert.True(confirmed); Assert.Null(f.LeaseActiveTransaction);
    }

    [Fact]
    public async Task Post_commit_readback_failure_does_not_claim_success_or_roll_back_an_already_committed_command()
    {
        using var f = await Ready(); var changed = false;
        f.AfterLeaseRead = () => {
            if (!changed && f.LeaseTransactions.LastOrDefault() == "commit") {
                changed = true; f.Rows.OfType<LAND_RIGHT>().Single(row => row.LAND_RIGHT_ID != "lease").CASE_SERIAL_NUM = "changed-after-commit";
            }
            return Task.CompletedTask;
        };
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).CreateLeaseAcquisitionAsync(Request(f), "actor"));
        Assert.Equal(2, f.Rows.OfType<LAND_RIGHT>().Count()); Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions);
    }

    [Fact]
    public async Task Normal_repository_reads_wait_for_an_owned_command_on_the_same_cached_source()
    {
        using var f = await Ready(); var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = PPDMDataSourceTransaction.ExecuteAsync(f.Source.Object, async () => { started.SetResult(); await release.Task; });
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var repository = new PPDMGenericRepository(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, typeof(LAND_RIGHT), "selected-tenant", "LAND_RIGHT");
        var read = repository.GetAsync([new() { FieldName = "LAND_RIGHT_ID", Operator = "=", FilterValue = "lease" }]);
        try { Assert.False(read.IsCompleted); Assert.DoesNotContain(f.LeaseQueries, query => query.Table == "LAND_RIGHT"); }
        finally { release.TrySetResult(); }
        await command.WaitAsync(TimeSpan.FromSeconds(5)); Assert.Single(await read.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task Nested_commands_cannot_commit_the_outer_command_transaction()
    {
        using var f = await Ready();
        await Assert.ThrowsAsync<InvalidOperationException>(() => PPDMDataSourceTransaction.ExecuteAsync(f.Source.Object,
            () => PPDMDataSourceTransaction.ExecuteAsync(f.Source.Object, () => Task.CompletedTask)));
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Server_UTC_status_time_is_stored_in_the_reviewed_company_date_zone()
    {
        using var f = await Ready(); f.EligibilityConfiguration.Companies.Single().TimeZoneId = "Asia/Kuwait";
        var before = DateTime.UtcNow; await Service(f).UpdateLeaseStatusAsync("lease", "INACTIVE", "actor", "A"); var after = DateTime.UtcNow;
        var recorded = Assert.Single(f.Writes.OfType<LAND_STATUS>()).EFFECTIVE_DATE!.Value;
        Assert.Equal(DateTimeKind.Unspecified, recorded.Kind);
        Assert.InRange(TimeZoneInfo.ConvertTimeToUtc(recorded, TimeZoneInfo.FindSystemTimeZoneById("Asia/Kuwait")), before, after);
    }

    [Theory]
    [InlineData("role")] [InlineData("grant")] [InlineData("configuration")]
    public async Task Authority_changed_after_the_last_write_prevents_commit_and_rolls_back(string fault)
    {
        using var f = await Ready(); f.AfterLeaseWrite = _ => {
            if (f.Writes.Count != 5) return;
            if (fault == "role") f.Db.UserRoles.Remove(f.Db.UserRoles.Single());
            if (fault == "grant") f.Db.Set<AppUserAssetAccess>().Single().IsActive = false;
            if (fault == "configuration") f.EligibilityConfiguration.Companies.Single().LeaseLandRightSubtype = "changed";
            f.Db.SaveChanges();
        };
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).CreateLeaseAcquisitionAsync(Request(f), "actor"));
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); Assert.Single(f.Rows.OfType<LAND_RIGHT>());
    }

    [Fact]
    public async Task A_datasource_replaced_during_the_command_cannot_redirect_a_write_or_readback()
    {
        using var f = await Ready(); var replacement = new Mock<TheTechIdea.Beep.IDataSource>();
        replacement.SetupGet(value => value.DatasourceName).Returns(f.Source.Object.DatasourceName);
        replacement.SetupGet(value => value.Dataconnection).Returns(f.Source.Object.Dataconnection);
        f.AfterLeaseWrite = _ => f.Editor.Setup(value => value.GetDataSource("selected-tenant")).Returns(replacement.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(f).CreateLeaseAcquisitionAsync(Request(f), "actor"));
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); Assert.Single(f.Writes); Assert.Single(f.Rows.OfType<LAND_RIGHT>());
        replacement.Verify(value => value.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()), Times.Never);
        replacement.Verify(value => value.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Unconfirmed_rollback_cannot_expose_its_uncommitted_records_to_a_later_repository_read()
    {
        using var f = await Ready(); f.LeaseFailWriteNumber = 2; f.LeaseRollbackFailed = true;
        await Assert.ThrowsAsync<AggregateException>(() => Service(f).CreateLeaseAcquisitionAsync(Request(f), "actor"));
        var repository = new PPDMGenericRepository(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, typeof(LAND_RIGHT), "selected-tenant", "LAND_RIGHT");
        f.LeaseQueries.Clear();
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetAsync([new() { FieldName = "LAND_RIGHT_ID", Operator = "=", FilterValue = "lease" }]));
        Assert.Empty(f.LeaseQueries); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
}
