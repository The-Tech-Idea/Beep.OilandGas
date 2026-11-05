using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Data.HSE;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.Report;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;
using ReadTests = Beep.OilandGas.Repository.Tests.CompanyCorrectiveActionAuthorityTests;

namespace Beep.OilandGas.Repository.Tests;

// Actual owner/command/policy/registry/Beep repository and transaction lane; explicit persisting test provider.
public sealed class CompanyCorrectiveActionCommandTests
{
    internal static async Task<Fixture> Ready(string level = "WRITE", bool manage = true, Fixture? fixture = null)
    {
        var f = await ReadTests.Ready(level: level, fixture: fixture);
        if (manage) { f.Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = PermissionConstants.HSE.ManageIncidents }); await f.Db.SaveChangesAsync(); }
        f.EnableCorrectiveActionTransactions(); return f;
    }
    [Theory]
    [InlineData("read")][InlineData("foreign")][InlineData("ambiguous")][InlineData("changed-link")][InlineData("stale-selection")]
    public async Task NativeIncidentPlanResolutionPreservesReadAuthorityAndExactUniqueLinks(string scenario)
    {
        using var f = await Ready("READ", false); var profile = ReadTests.Profile(f);
        var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        if (scenario == "ambiguous") f.Rows.AddRange([new PROJECT { PROJECT_ID = "another-plan", PROJECT_TYPE = "CORRECTIVE_ACTION", ACTIVE_IND = "Y" }, new HSE_INCIDENT_RESPONSE { INCIDENT_ID = "local", RESPONSE_OBS_NO = 9, RESPONSE_TYPE = "recorded-action-response", PROJECT_ID = "another-plan", ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow.AddHours(-1) }]);
        if (scenario == "changed-link") f.AfterRead = () => { f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(r => r.PROJECT_ID == ReadTests.Plan).PROJECT_ID = "changed-plan"; return Task.CompletedTask; };
        if (scenario == "stale-selection")
        {
            await f.Grant("FIELD", "B", "READ", inherit: true);
            await profile.SetCurrentFieldSelectionAsync(new() { FieldId = "B", Context = context });
        }
        if (scenario == "read")
        {
            var result = await commands.ResolveIncidentPlanAsync(context, "local", "actor"); Assert.Equal(new CAPlanResolution(context, "local", ReadTests.Plan), result);
            Assert.DoesNotContain("HSE.ManageIncidents", (await f.Gate.RefreshCompanyAsync()).Permissions);
        }
        else await Assert.ThrowsAnyAsync<Exception>(() => commands.ResolveIncidentPlanAsync(context, scenario == "foreign" ? "foreign" : "local", "actor"));
        Assert.Empty(f.Writes);
    }
    private sealed class StoredFieldProbe(RepositoryUserProfileService profile) : ICompanyFieldSelectionService, ICompanyFieldSelectionAuthority
    {
        public int Checks;
        public Task<List<FieldListItem>> GetAvailableFieldsAsync(string? connectionName = null) => throw new InvalidOperationException("An authority check cannot enumerate available FIELD data.");
        public Task<FieldResponse> GetCurrentFieldSelectionAsync() => throw new InvalidOperationException("Use the stored preference plus the exact native FIELD authority.");
        public Task<SetActiveFieldResponse> SetCurrentFieldSelectionAsync(SetActiveFieldRequest request) => throw new InvalidOperationException("An authority check cannot change the FIELD preference.");
        public Task RequireStoredSelectionAsync(FieldSelectionReview review) { Checks++; return profile.RequireStoredSelectionAsync(review); }
    }
    [Theory]
    [InlineData("corrective")][InlineData("incident")][InlineData("stale-selection")][InlineData("field-retired")][InlineData("permission-revoked")]
    public async Task HseAuthorityUsesStoredPreferenceAndFreshBoundedNativeEvidence(string scenario)
    {
        using var f = await CompanyIncidentCommandTests.Ready(); CompanyIncidentCommandTests.EnableReportOptionProjections(f);
        var profile = ReadTests.Profile(f); var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!; var probe = new StoredFieldProbe(profile);
        if (scenario == "stale-selection")
        {
            await f.Grant("FIELD", "B", "WRITE", inherit: true);
            await profile.SetCurrentFieldSelectionAsync(new() { FieldId = "B", Context = context });
        }
        if (scenario == "permission-revoked") { f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(c => c.ClaimValue == "HSE.ManageIncidents")); await f.Db.SaveChangesAsync(); }
        if (scenario == "field-retired") f.AfterRead = () =>
        {
            if (f.Source.Invocations.Any(call => call.Method.Name == "GetEntityAsync" && call.Arguments[0] as string == "PROJECT")) f.Rows.OfType<FIELD>().Single(r => r.FIELD_ID == "A").ACTIVE_IND = "N";
            return Task.CompletedTask;
        };
        if (scenario == "incident")
        {
            var commands = new PPDMCompanyIncidentCommands(f.Gate, probe, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
            Assert.True(ProcessDefinitionRevision.IsValid(await commands.ReviewWorkflowSourceRevisionAsync(context, "local")));
        }
        else
        {
            var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, probe, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
            if (scenario == "corrective") (await commands.ReviewVerificationAsync(context, "local", ReadTests.Plan, "actor")).Require();
            else await Assert.ThrowsAnyAsync<Exception>(() => commands.ReviewVerificationAsync(context, "local", ReadTests.Plan, "actor"));
        }
        Assert.True(probe.Checks > 0); Assert.Empty(f.Writes);
    }
    private static PROJECT_STEP Step(Fixture f) => f.Rows.OfType<PROJECT_STEP>().Single(row => row.PROJECT_ID == ReadTests.Plan);
    [Fact]
    public async Task UnknownNativeIncidentStateCannotBeReviewedForClosure()
    {
        using var f = await Ready();
#if BEEP_SECURE_CONNECTIONS
        f.NativeProjectionEnabled = true;
#endif
        f.Source.Setup(x => x.GetEntityStructure(It.IsAny<TheTechIdea.Beep.DataBase.EntityStructure>(), true)).Returns(() => new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object).ConvertToEntityStructure(typeof(HSE_INCIDENT_RESPONSE)));
        var profile = ReadTests.Profile(f); var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        var status = new HSE_INCIDENT_RESPONSE { INCIDENT_ID = "local", RESPONSE_OBS_NO = 100, ACTIVE_IND = "Y", RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = "REPORTED", EFFECTIVE_DATE = DateTime.UtcNow.AddHours(-1) }; f.Rows.Add(status);
        Assert.True(ProcessDefinitionRevision.IsValid(await commands.ReviewNativeClosureRevisionAsync(context, "local", ReadTests.Plan)));
        status.RESPONSE_RESULT = "UNRECOGNIZED";
        await Assert.ThrowsAsync<RefusalException>(() => commands.ReviewNativeClosureRevisionAsync(context, "local", ReadTests.Plan)); Assert.Empty(f.Writes);
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("ledger-failure")]
    [InlineData("lost-commit")]
    [InlineData("changed-status")]
    [InlineData("revoked-role")]
    [InlineData("resolution")]
    [InlineData("resolution-drift")]
    [InlineData("resolution-cancelled")]
    public async Task NativeIncidentClosureRequiresTerminalApprovalAndReconcilesOneClosedStatus(string mode)
    {
        using var f = await Ready();
#if BEEP_SECURE_CONNECTIONS
        f.NativeProjectionEnabled = true;
        f.Source.As<TheTechIdea.Beep.DataBase.IOwnedEntityProjectionDataSource>()
            .Setup(x => x.ReadEntityProjectionAsync(It.IsAny<string>(), It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .Returns(async (string table, List<TheTechIdea.Beep.Report.AppFilter> filters, IReadOnlyList<string> requested, CancellationToken _) =>
                new TheTechIdea.Beep.DataBase.EntityReadResult(new TheTechIdea.Beep.ConfigUtil.ErrorsInfo { Flag = TheTechIdea.Beep.ConfigUtil.Errors.Ok },
                    ((IEnumerable<object>)await f.Source.Object.GetEntityAsync(table, filters)).Select(row => (object)row.GetType().GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && requested.Contains(p.Name, StringComparer.OrdinalIgnoreCase)).ToDictionary(p => p.Name, p => p.GetValue(row)!))));
#endif
        var profile = ReadTests.Profile(f); var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        await commands.RecordCompletionAsync(context, "local", ReadTests.Plan, 1, "Valve inspected and tested", "actor");
        var completed = f.Rows.OfType<PROJECT_STATUS>().Single(r => r.PROJECT_ID == ReadTests.Plan && r.STATUS == "COMPLETED"); completed.ROW_CREATED_BY = completed.ROW_CHANGED_BY = "operator"; Step(f).ROW_CHANGED_BY = "operator";
        var nativeMetadata = Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.HseIncidentNativeMetadata.AddTo(new());
        foreach (var type in new[] { typeof(HSE_CA_VERIFICATION), typeof(HSE_INCIDENT_CLOSURE) })
        {
            var name = type.Name; f.Metadata.Setup(x => x.GetTableMetadataAsync(name)).ReturnsAsync(nativeMetadata[name]);
            f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(name)).ReturnsAsync([nativeMetadata[name].PrimaryKeyColumn]);
        }
        f.Source.Setup(x => x.GetEntityStructure(It.IsAny<TheTechIdea.Beep.DataBase.EntityStructure>(), true)).Returns((TheTechIdea.Beep.DataBase.EntityStructure request, bool _) =>
        {
            var type = request.EntityName == nameof(HSE_INCIDENT_CLOSURE) ? typeof(HSE_INCIDENT_CLOSURE) : request.EntityName == nameof(HSE_CA_VERIFICATION) ? typeof(HSE_CA_VERIFICATION) : typeof(HSE_INCIDENT_RESPONSE);
            var shape = new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object).ConvertToEntityStructure(type);
            foreach (var field in shape.Fields) { field.IsKey = type == typeof(HSE_INCIDENT_RESPONSE) ? field.FieldName is "INCIDENT_ID" or "RESPONSE_OBS_NO" : field.FieldName == type.Name + "_ID"; if (field.Fieldtype == typeof(string).FullName) field.Size = field.Size1 = 65536; }
            return shape;
        });
        var verification = new CAVerificationDecision(Guid.NewGuid(), await commands.ReviewVerificationAsync(context, "local", ReadTests.Plan, "actor"), "VERIFIED", "Independent test confirmed the repair is effective."); await commands.RecordVerificationAsync(verification);
        f.Rows.Add(new HSE_INCIDENT_RESPONSE { INCIDENT_ID = "local", RESPONSE_OBS_NO = 100, ACTIVE_IND = "Y", RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = "REPORTED", EFFECTIVE_DATE = DateTime.UtcNow.AddHours(-1), SOURCE = "Beep.OilandGas.HSE" });
        var evidence = await commands.ReviewClosureAsync(context, "local", ReadTests.Plan, "actor");
        var review = new CompanyHseClosureReview(evidence, new("lifecycle", "binding"), "instance", "step", "INC_CLOSURE", "HSEOfficer", new string('A', 64), await commands.ReviewNativeClosureRevisionAsync(context, "local", ReadTests.Plan));
        var original = new CompanyHseClosureCommand(Guid.NewGuid(), review, "All incident evidence reviewed and final closure approved.");
        var lifecycle = new CompanyHseClosureReceipt(original.IntentId, context.Company, "actor", "instance", "step", "INC_CLOSURE", "history", "approval", null, ["completed-task"], [], new string('B', 64));
        var confirmed = 0; Task Confirm() { PPDMDataSourceTransaction.RequireIdle(f.Source.Object); confirmed++; return Task.CompletedTask; }
        var before = f.Writes.Count;
        IncidentClosureResolution? resolution = null;
        if (mode.StartsWith("resolution", StringComparison.Ordinal))
        {
            var changed = f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(r => r.RESPONSE_OBS_NO == 100); changed.REMARK = "A recorded native history note changed after terminal approval.";
            if (mode == "resolution-cancelled") { changed.RESPONSE_RESULT = "CANCELLED"; await Assert.ThrowsAnyAsync<Exception>(() => commands.ReviewClosureResolutionAsync(context, original, lifecycle)); Assert.Equal(before, f.Writes.Count); return; }
            var sourceReview = await commands.ReviewClosureResolutionAsync(context, original, lifecycle); sourceReview.RequireAgainst(original, lifecycle);
            Assert.Contains(sourceReview.Responses, r => r.Observation == 100 && r.Reason == changed.REMARK);
            resolution = new(Guid.NewGuid(), sourceReview, "Explicitly reviewed the native history change and confirmed closure."); resolution.Require();
            if (mode == "resolution-drift") { changed.REMARK = "A second change after the resolution review."; await Assert.ThrowsAnyAsync<Exception>(() => commands.ResolveIncidentClosureAsync(original, lifecycle, resolution, Confirm)); Assert.Equal(before, f.Writes.Count); return; }
        }
        if (mode == "revoked-role")
        {
            f.AfterLeaseWrite = row => { if (row is HSE_INCIDENT_RESPONSE status && status.RESPONSE_RESULT == "CLOSED") { f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(c => c.ClaimValue == PermissionConstants.HSE.ManageIncidents)); f.Db.SaveChanges(); } };
            await Assert.ThrowsAnyAsync<Exception>(() => commands.RecordIncidentClosedAsync(original, lifecycle, Confirm));
            Assert.Empty(f.Rows.OfType<HSE_INCIDENT_CLOSURE>()); Assert.DoesNotContain(f.Rows.OfType<HSE_INCIDENT_RESPONSE>(), r => r.RESPONSE_RESULT == "CLOSED"); return;
        }
        if (mode == "changed-status") { f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(r => r.RESPONSE_OBS_NO == 100).REMARK = "changed"; await Assert.ThrowsAnyAsync<Exception>(() => commands.RecordIncidentClosedAsync(original, lifecycle, Confirm)); Assert.Equal(before, f.Writes.Count); return; }
        if (mode == "ledger-failure")
        {
            f.LeaseFailWriteNumber = before + 2;
            await Assert.ThrowsAnyAsync<Exception>(() => commands.RecordIncidentClosedAsync(original, lifecycle, Confirm));
            Assert.Empty(f.Rows.OfType<HSE_INCIDENT_CLOSURE>()); Assert.DoesNotContain(f.Rows.OfType<HSE_INCIDENT_RESPONSE>(), r => r.RESPONSE_RESULT == "CLOSED"); f.LeaseFailWriteNumber = 0;
        }
        if (mode == "lost-commit")
        {
            f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true;
            await Assert.ThrowsAnyAsync<Exception>(() => commands.RecordIncidentClosedAsync(original, lifecycle, Confirm)); f.LeaseCommitFailed = false;
        }
        var result = resolution is null ? await commands.RecordIncidentClosedAsync(original, lifecycle, Confirm) : await commands.ResolveIncidentClosureAsync(original, lifecycle, resolution, Confirm); result.Require(); var writes = f.Writes.Count;
        if (resolution is not null)
        {
            Assert.Equal(JsonSerializer.Serialize(resolution), JsonSerializer.Serialize(result.Resolution));
            Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(await commands.ResolveIncidentClosureAsync(original, lifecycle, resolution, Confirm))); Assert.Equal(writes, f.Writes.Count);
            await Assert.ThrowsAnyAsync<Exception>(() => commands.ResolveIncidentClosureAsync(original, lifecycle, resolution with { IntentId = Guid.NewGuid() }, Confirm)); Assert.Equal(writes, f.Writes.Count);
        }
        Assert.Single(f.Rows.OfType<HSE_INCIDENT_CLOSURE>()); Assert.Single(f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Where(r => r.RESPONSE_RESULT == "CLOSED"));
        Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(await commands.RecordIncidentClosedAsync(original, lifecycle, Confirm))); Assert.Equal(writes, f.Writes.Count); Assert.True(confirmed >= 3);
        var locked = await Assert.ThrowsAsync<RefusalException>(() => commands.ReviewActionOptionsAsync(context, "local", ReadTests.Plan, "actor")); Assert.Contains("cannot accept corrective-action changes", locked.Message); Assert.Equal(writes, f.Writes.Count);
        await Assert.ThrowsAnyAsync<Exception>(() => commands.RecordIncidentClosedAsync(original with { IntentId = Guid.NewGuid() }, lifecycle with { IntentId = Guid.NewGuid() }, Confirm)); Assert.Equal(writes, f.Writes.Count);
    }
    internal static AddCARequest Addition() => new(Guid.Parse("a54f10ec-c1dd-4b8c-9c9d-dedb553b6c5d"), "Explicit corrective action", "recorded-action-type", new DateTime(2027, 1, 12, 0, 0, 0, DateTimeKind.Utc), "recorded-target");
    [Theory]
    [InlineData("valid")]
    [InlineData("self")]
    [InlineData("stale")]
    [InlineData("rejected")]
    public async Task VerificationDecisionRecordsOneOriginalAndRefusesChangedEvidence(string mode)
    {
        using var f = await Ready();
#if BEEP_SECURE_CONNECTIONS
        f.NativeProjectionEnabled = true;
#endif
        var profile = ReadTests.Profile(f); var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile,
            new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        await commands.RecordCompletionAsync(context, "local", ReadTests.Plan, 1, "Valve inspected and tested", "actor");
        if (mode != "self")
        {
            var completed = f.Rows.OfType<PROJECT_STATUS>().Single(row => row.PROJECT_ID == ReadTests.Plan && row.STATUS == "COMPLETED");
            completed.ROW_CREATED_BY = "other-operator"; completed.ROW_CHANGED_BY = "other-operator"; Step(f).ROW_CHANGED_BY = "other-operator";
        }
        var review = await commands.ReviewVerificationAsync(context, "local", ReadTests.Plan, "actor");
        var decision = new CAVerificationDecision(Guid.NewGuid(), review, mode == "rejected" ? "REJECTED" : "VERIFIED", "Independent inspection documented the repair outcome.");
        if (mode == "self") { Assert.Throws<InvalidOperationException>(() => decision.Require()); return; }
        var nativeMetadata = Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.HseIncidentNativeMetadata.AddTo(new());
        f.Metadata.Setup(x => x.GetTableMetadataAsync(nameof(HSE_CA_VERIFICATION))).ReturnsAsync(nativeMetadata[nameof(HSE_CA_VERIFICATION)]);
        f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(nameof(HSE_CA_VERIFICATION))).ReturnsAsync([nameof(HSE_CA_VERIFICATION.HSE_CA_VERIFICATION_ID)]);
        f.Source.Setup(x => x.GetEntityStructure(It.IsAny<TheTechIdea.Beep.DataBase.EntityStructure>(), true)).Returns((TheTechIdea.Beep.DataBase.EntityStructure requested, bool _) =>
        {
            var schema = new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object).ConvertToEntityStructure(typeof(HSE_CA_VERIFICATION));
            foreach (var field in schema.Fields) { field.IsKey = field.FieldName == nameof(HSE_CA_VERIFICATION.HSE_CA_VERIFICATION_ID); if (field.Fieldtype == typeof(string).FullName) { field.Size = 16384; field.Size1 = 16384; } }
            return schema;
        });
        if (mode == "stale") Step(f).DUE_DATE = Step(f).DUE_DATE!.Value.AddDays(1);
        var before = f.Writes.Count;
        if (mode == "stale") { await Assert.ThrowsAnyAsync<Exception>(() => commands.RecordVerificationAsync(decision)); Assert.Equal(before, f.Writes.Count); return; }
        var result = await commands.RecordVerificationAsync(decision); Assert.Equal(JsonSerializer.Serialize(decision), JsonSerializer.Serialize(result));
        Assert.Equal(before + 1, f.Writes.Count);
        Assert.Equal(JsonSerializer.Serialize(decision), JsonSerializer.Serialize(await commands.RecordVerificationAsync(decision))); Assert.Equal(before + 1, f.Writes.Count);
        await Assert.ThrowsAsync<RefusalException>(() => commands.RecordVerificationAsync(decision with { IntentId = Guid.NewGuid() })); Assert.Equal(before + 1, f.Writes.Count);
        var closure = await commands.ReviewClosureAsync(context, "local", ReadTests.Plan, "actor"); closure.Require();
        Assert.Equal(mode != "rejected", closure.VerificationSatisfied); Assert.Single(closure.Decisions); Assert.Equal(before + 1, f.Writes.Count);
        Step(f).DUE_DATE = Step(f).DUE_DATE!.Value.AddDays(1);
        Assert.False((await commands.ReviewClosureAsync(context, "local", ReadTests.Plan, "actor")).VerificationSatisfied);
        Assert.Equal(before + 1, f.Writes.Count);
        f.Rows.OfType<HSE_CA_VERIFICATION>().Single().COMMAND_JSON += " ";
        await Assert.ThrowsAsync<InvalidOperationException>(() => commands.ReviewClosureAsync(context, "local", ReadTests.Plan, "actor"));
    }
    [Theory]
    [InlineData("open")]
    [InlineData("own")]
    [InlineData("independent")]
    public async Task VerificationReviewUsesRecordedCompletionAndIndependentActor(string mode)
    {
        using var f = await Ready();
        var profile = ReadTests.Profile(f); var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile,
            new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        if (mode != "open")
        {
            await commands.RecordCompletionAsync(context, "local", ReadTests.Plan, 1, "Valve inspected and tested", "actor");
            if (mode == "independent")
            {
                var completed = f.Rows.OfType<PROJECT_STATUS>().Single(row => row.PROJECT_ID == ReadTests.Plan && row.STATUS == "COMPLETED");
                completed.ROW_CREATED_BY = "other-operator"; completed.ROW_CHANGED_BY = "other-operator";
                Step(f).ROW_CHANGED_BY = "other-operator";
            }
        }
        var before = f.Writes.Count;
        var result = await commands.ReviewVerificationAsync(context, "local", ReadTests.Plan, "actor"); result.Require();
        var action = Assert.Single(result.Actions); Assert.Equal(mode == "open" ? "OPEN" : "COMPLETED", action.Status);
        Assert.Equal(mode == "independent", result.CanReviewIndependently);
        if (mode != "open") Assert.Equal("Valve inspected and tested", action.CompletionEvidence);
        Assert.Equal(before, f.Writes.Count);
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("changed-due")]
    [InlineData("replaced-step")]
    public async Task CompletionUsesTheReviewedActionAndReconcilesTheSameResult(string mode)
    {
        using var f = await Ready(); Target(f); var request = Addition();
        var profile = ReadTests.Profile(f); var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile,
            new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        await commands.AddCorrectiveActionAsync(context, "local", ReadTests.Plan, request, "actor");
        var step = f.Rows.OfType<PROJECT_STEP>().Single(row => row.STEP_ID == request.ActionId.ToString("N"));
        var review = await commands.ReviewCompletionAsync(context, "local", ReadTests.Plan, checked((int)step.STEP_SEQ_NO), "actor");
        Assert.Equal(step.STEP_ID, review.StepId); Assert.Equal(request.ResponsibleBaId, review.ResponsiblePerson);
        if (mode == "changed-due") step.DUE_DATE = request.DueDate.AddDays(1);
        if (mode == "replaced-step") review = review with { StepId = "different-step" };
        var before = f.Writes.Count;
        if (mode == "valid")
        {
            await commands.RecordReviewedCompletionAsync(review, "Confirmed work", "actor");
            var after = f.Writes.Count; Assert.Equal(before + 2, after);
            await commands.RecordReviewedCompletionAsync(review, "Confirmed work", "actor"); Assert.Equal(after, f.Writes.Count);
            await Assert.ThrowsAsync<RefusalException>(() => commands.RecordReviewedCompletionAsync(review with { DueDate = review.DueDate.AddDays(1) }, "Confirmed work", "actor"));
            Assert.Equal(after, f.Writes.Count);
        }
        else { await Assert.ThrowsAsync<RefusalException>(() => commands.RecordReviewedCompletionAsync(review, "Confirmed work", "actor")); Assert.Equal(before, f.Writes.Count); }
    }
    [Theory]
    [InlineData("changed-vocabulary")]
    [InlineData("changed-intent")]
    [InlineData("incomplete")]
    [InlineData("absent")]
    [InlineData("completed")]
    [InlineData("tampered-completion")]
    [InlineData("changed-due")]
    [InlineData("reassigned")]
    [InlineData("tampered-assignment")]
    public async Task ActionRecoveryReadsOnlyTheExactRecordedCreationEvenIfChoicesChanged(string mode)
    {
        using var f = await Ready(); Target(f); var request = Addition();
        var profile = ReadTests.Profile(f); var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile,
            new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        if (mode != "absent") await commands.AddCorrectiveActionAsync(context, "local", ReadTests.Plan, request, "actor");
        if (mode is "completed" or "tampered-completion")
        {
            var action = f.Rows.OfType<PROJECT_STEP>().Single(row => row.STEP_ID == request.ActionId.ToString("N"));
            await commands.RecordCompletionAsync(context, "local", ReadTests.Plan, checked((int)action.STEP_SEQ_NO), "Completed field work", "actor");
            if (mode == "tampered-completion") f.Rows.OfType<PROJECT_STATUS>().Single(row => row.STEP_ID == action.STEP_ID && row.STATUS == "COMPLETED").REMARK = "Altered";
        }
        if (mode is "reassigned" or "tampered-assignment")
        {
            Target(f, "replacement", 24);
            var action = f.Rows.OfType<PROJECT_STEP>().Single(row => row.STEP_ID == request.ActionId.ToString("N"));
            await commands.AssignResponsiblePersonAsync(context, "local", ReadTests.Plan, checked((int)action.STEP_SEQ_NO), "replacement", "actor");
            if (mode == "tampered-assignment") f.Rows.OfType<PROJECT_STEP_BA>().Single(row => row.STEP_ID == action.STEP_ID && row.BUSINESS_ASSOCIATE_ID == "replacement").STEP_BA_OBS_NO = 5;
        }
        if (mode == "changed-due")
        {
            var action = f.Rows.OfType<PROJECT_STEP>().Single(row => row.STEP_ID == request.ActionId.ToString("N"));
            await commands.SetDueDateAsync(context, "local", ReadTests.Plan, checked((int)action.STEP_SEQ_NO), request.DueDate.AddDays(1), "actor");
        }
        var writes = f.Writes.Count;
        if (mode == "changed-vocabulary")
        {
            f.Rows.OfType<R_PROJ_STEP_TYPE>().Single(row => row.STEP_TYPE == request.CAType).EXPIRY_DATE = DateTime.UtcNow.AddDays(-1);
            f.Rows.OfType<PROJECT_BA_ROLE>().Single(row => row.BUSINESS_ASSOCIATE_ID == request.ResponsibleBaId).EXPIRY_DATE = DateTime.UtcNow.AddDays(-1);
        }
        if (mode == "incomplete") f.Rows.RemoveAll(row => row is PROJECT_STEP_BA assignment && assignment.STEP_ID == request.ActionId.ToString("N"));
        if (mode == "changed-intent") request = request with { CADescription = "Different intent" };
        if (mode is "changed-intent" or "incomplete" or "tampered-completion" or "changed-due" or "tampered-assignment") await Assert.ThrowsAsync<RefusalException>(() => commands.FindRecordedActionAsync(context, "local", ReadTests.Plan, request, "actor"));
        else Assert.Equal(mode == "absent" ? null : request.ActionId.ToString("N"), await commands.FindRecordedActionAsync(context, "local", ReadTests.Plan, request, "actor"));
        Assert.Equal(writes, f.Writes.Count);
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("ambiguous")]
    [InlineData("revoked")]
    [InlineData("expired-type")]
    [InlineData("expired-role")]
    [InlineData("expired-member")]
    [InlineData("missing-role-reference")]
    public async Task ActionOptionsUseCurrentPlanReferencesAndResponsibleMembers(string mode)
    {
        using var f = await Ready(manage: mode != "revoked"); Target(f);
        var profile = ReadTests.Profile(f); var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile,
            new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
        if (mode == "ambiguous") f.Rows.Add(new PROJECT_BA_ROLE { PROJECT_ID = ReadTests.Plan, BUSINESS_ASSOCIATE_ID = "recorded-target", ROLE = "RESPONSIBLE", ROLE_SEQ_NO = 24, ACTIVE_IND = "Y" });
        if (mode == "expired-type") f.Rows.OfType<R_PROJ_STEP_TYPE>().Single(row => row.STEP_TYPE == "recorded-action-type").EXPIRY_DATE = DateTime.UtcNow.AddDays(-1);
        if (mode == "expired-role") f.Rows.OfType<PROJECT_BA_ROLE>().Single(row => row.BUSINESS_ASSOCIATE_ID == "recorded-target").EXPIRY_DATE = DateTime.UtcNow.AddDays(-1);
        if (mode == "expired-member") f.Rows.OfType<PROJECT_BA>().Single(row => row.BUSINESS_ASSOCIATE_ID == "recorded-target").EXPIRY_DATE = DateTime.UtcNow.AddDays(-1);
        if (mode == "missing-role-reference") f.Rows.RemoveAll(row => row is R_PROJECT_BA_ROLE);
        if (mode is "valid" or "expired-type" or "expired-role")
        {
            var result = await commands.ReviewActionOptionsAsync(context, "local", ReadTests.Plan, "actor"); result.Require();
            if (mode == "expired-type") Assert.DoesNotContain("recorded-action-type", result.ActionTypes);
            else Assert.Contains("recorded-action-type", result.ActionTypes);
            if (mode == "expired-role") Assert.DoesNotContain("recorded-target", result.Participants);
            else Assert.Contains("recorded-target", result.Participants);
            Assert.DoesNotContain("foreign-person", result.Participants); Assert.Empty(f.Writes);
        }
        else await Assert.ThrowsAnyAsync<Exception>(() => commands.ReviewActionOptionsAsync(context, "local", ReadTests.Plan, "actor"));
    }
    [Fact]
    public async Task Addition_atomically_records_the_step_open_status_actual_participant_and_stable_retry_intent()
    {
        using var f = await Ready(); Target(f); var request = Addition();
        var id = await ReadTests.Service(f).AddCorrectiveActionAsync("local", request, "actor");
        Assert.Equal(request.ActionId.ToString("N"), id); Assert.Equal(3, f.Writes.Count);
        var step = Assert.Single(f.Rows.OfType<PROJECT_STEP>(), row => row.STEP_ID == id);
        Assert.Equal(2, step.STEP_SEQ_NO); Assert.Equal(request.DueDate, step.DUE_DATE); Assert.Equal("actor", step.ROW_CREATED_BY);
        var status = Assert.Single(f.Rows.OfType<PROJECT_STATUS>(), row => row.STEP_ID == id);
        Assert.Equal("OPEN", status.STATUS); Assert.Equal(id, status.STATUS_ID); Assert.Null(status.DEFINED_BY_BA_ID);
        var assignment = Assert.Single(f.Rows.OfType<PROJECT_STEP_BA>(), row => row.STEP_ID == id);
        Assert.Equal(23, assignment.ROLE_SEQ_NO); Assert.Equal(1, assignment.STEP_BA_OBS_NO); Assert.Equal(request.ResponsibleBaId, assignment.BUSINESS_ASSOCIATE_ID);
        Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions);
        Assert.Equal(id, await ReadTests.Service(f).AddCorrectiveActionAsync("local", request, "actor")); Assert.Equal(3, f.Writes.Count);
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AddCorrectiveActionAsync("local", request with { CADescription = "Different intent" }, "actor")); Assert.Equal(3, f.Writes.Count);
    }
    [Fact]
    public async Task Addition_allocation_preserves_inactive_history_and_does_not_reuse_its_sequence()
    {
        using var f = await Ready(); Target(f);
        f.Rows.Add(new PROJECT_STEP { PROJECT_ID = ReadTests.Plan, STEP_ID = "inactive-history", STEP_SEQ_NO = 41, ACTIVE_IND = "N" });
        var id = await ReadTests.Service(f).AddCorrectiveActionAsync("local", Addition(), "actor");
        Assert.Equal(42, f.Rows.OfType<PROJECT_STEP>().Single(row => row.STEP_ID == id).STEP_SEQ_NO);
        Assert.Contains(f.Rows.OfType<PROJECT_STEP>(), row => row.STEP_ID == "inactive-history" && row.ACTIVE_IND == "N");
    }
    [Theory] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public async Task Addition_rolls_back_all_records_when_any_of_the_three_writes_fails(int failedWrite)
    {
        using var f = await Ready(); Target(f); f.LeaseFailWriteNumber = failedWrite; var id = Addition().ActionId.ToString("N");
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadTests.Service(f).AddCorrectiveActionAsync("local", Addition(), "actor"));
        Assert.DoesNotContain(f.Rows.OfType<PROJECT_STEP>(), row => row.STEP_ID == id);
        Assert.DoesNotContain(f.Rows.OfType<PROJECT_STATUS>(), row => row.STEP_ID == id);
        Assert.DoesNotContain(f.Rows.OfType<PROJECT_STEP_BA>(), row => row.STEP_ID == id);
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); Assert.Null(f.LeaseActiveTransaction);
    }
    [Fact]
    public async Task Addition_reconciles_the_same_durable_effect_after_lost_commit_acknowledgment()
    {
        using var f = await Ready(); Target(f); f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadTests.Service(f).AddCorrectiveActionAsync("local", Addition(), "actor")); Assert.Equal(3, f.Writes.Count);
        f.LeaseCommitFailed = false;
        Assert.Equal(Addition().ActionId.ToString("N"), await ReadTests.Service(f).AddCorrectiveActionAsync("local", Addition(), "actor")); Assert.Equal(3, f.Writes.Count);
    }
    [Theory] [InlineData("id")] [InlineData("date-zone")] [InlineData("description")] [InlineData("long-description")] [InlineData("type")] [InlineData("participant")]
    public async Task Addition_requires_explicit_bounded_inputs_before_transaction(string fault)
    {
        using var f = await Ready(); Target(f); var request = Addition();
        request = fault switch { "id" => request with { ActionId = Guid.Empty }, "date-zone" => request with { DueDate = DateTime.SpecifyKind(request.DueDate, DateTimeKind.Unspecified) },
            "description" => request with { CADescription = " " }, "long-description" => request with { CADescription = new string('a', 129) }, "type" => request with { CAType = " recorded-action-type" }, _ => request with { ResponsibleBaId = "" } };
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AddCorrectiveActionAsync("local", request, "actor")); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("step-reference")] [InlineData("status-reference")] [InlineData("role-reference")] [InlineData("missing-participant")] [InlineData("expired-role")] [InlineData("duplicate-role")] [InlineData("wrong-role-sequence")]
    public async Task Addition_proves_every_actual_reference_and_participant_before_the_first_write(string fault)
    {
        using var f = await Ready(); Target(f);
        var role = f.Rows.OfType<PROJECT_BA_ROLE>().Single(row => row.BUSINESS_ASSOCIATE_ID == "recorded-target");
        if (fault == "step-reference") f.Rows.RemoveAll(row => row is R_PROJ_STEP_TYPE);
        if (fault == "status-reference") f.Rows.RemoveAll(row => row is R_PROJECT_STATUS reference && reference.STATUS == "OPEN");
        if (fault == "role-reference") f.Rows.RemoveAll(row => row is R_PROJECT_BA_ROLE);
        if (fault == "missing-participant") f.Rows.RemoveAll(row => row is PROJECT_BA member && member.BUSINESS_ASSOCIATE_ID == "recorded-target");
        if (fault == "expired-role") role.EXPIRY_DATE = DateTime.UtcNow.AddDays(-1);
        if (fault == "duplicate-role") f.Rows.Add(new PROJECT_BA_ROLE { PROJECT_ID = role.PROJECT_ID, BUSINESS_ASSOCIATE_ID = role.BUSINESS_ASSOCIATE_ID, ROLE = role.ROLE, ROLE_SEQ_NO = 24, ACTIVE_IND = "Y" });
        if (fault == "wrong-role-sequence") role.ROLE_SEQ_NO = 23.5m;
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AddCorrectiveActionAsync("local", Addition(), "actor")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("duplicate-sequence")] [InlineData("fractional-sequence")] [InlineData("exhausted-sequence")] [InlineData("inactive-intent")] [InlineData("bounded-inventory")]
    public async Task Addition_refuses_ambiguous_or_exhausted_historical_allocation(string fault)
    {
        using var f = await Ready(); Target(f);
        var historical = new PROJECT_STEP { PROJECT_ID = ReadTests.Plan, STEP_ID = "history", STEP_SEQ_NO = 2, ACTIVE_IND = "N" };
        if (fault == "duplicate-sequence") historical.STEP_SEQ_NO = 1;
        if (fault == "fractional-sequence") historical.STEP_SEQ_NO = 1.5m;
        if (fault == "exhausted-sequence") historical.STEP_SEQ_NO = int.MaxValue;
        if (fault == "inactive-intent") historical.STEP_ID = Addition().ActionId.ToString("N");
        f.Rows.Add(historical);
        if (fault == "bounded-inventory") for (var i = 3; i < 258; i++) f.Rows.Add(new PROJECT_STEP { PROJECT_ID = ReadTests.Plan, STEP_ID = "history-" + i, STEP_SEQ_NO = i, ACTIVE_IND = "N" });
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AddCorrectiveActionAsync("local", Addition(), "actor")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("orphan")] [InlineData("status-key-collision")] [InlineData("wrong-readback")]
    public async Task Addition_refuses_child_history_collisions_or_provider_OK_without_the_recorded_effect(string fault)
    {
        using var f = await Ready(); Target(f); var id = Addition().ActionId.ToString("N");
        if (fault != "wrong-readback") f.Rows.Add(new PROJECT_STATUS { PROJECT_ID = ReadTests.Plan, STATUS_ID = id, STEP_ID = fault == "orphan" ? id : "some-other-action", ACTIVE_IND = "N" });
        else f.AfterLeaseWrite = row => { if (row is PROJECT_STATUS status) status.REMARK = "provider changed intent"; };
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AddCorrectiveActionAsync("local", Addition(), "actor"));
        Assert.DoesNotContain(f.Rows.OfType<PROJECT_STEP>(), row => row.STEP_ID == id);
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }

    [Theory] [InlineData("role-drift")] [InlineData("status-extra")] [InlineData("permission-drift")]
    public async Task Addition_rolls_back_when_captured_relationship_or_complete_effect_changes_during_write(string fault)
    {
        using var f = await Ready(); Target(f); var changed = false; var id = Addition().ActionId.ToString("N");
        f.AfterLeaseWrite = row => {
            if (changed) return; changed = true;
            if (fault == "role-drift") f.Rows.OfType<PROJECT_BA_ROLE>().Single(role => role.BUSINESS_ASSOCIATE_ID == "recorded-target").ROLE_SEQ_NO = 24;
            if (fault == "status-extra") f.Rows.Add(new PROJECT_STATUS { PROJECT_ID = ReadTests.Plan, STATUS_ID = "extra-effect", STEP_ID = id, ACTIVE_IND = "N" });
            if (fault == "permission-drift") { f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(claim => claim.ClaimValue == PermissionConstants.HSE.ManageIncidents)); f.Db.SaveChanges(); }
        };
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AddCorrectiveActionAsync("local", Addition(), "actor"));
        Assert.DoesNotContain(f.Rows.OfType<PROJECT_STEP>(), row => row.STEP_ID == id); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Theory] [InlineData("permission")] [InlineData("read")] [InlineData("foreign-incident")] [InlineData("actor")]
    public async Task Addition_independently_requires_current_app_and_field_write_authority(string fault)
    {
        using var f = await Ready(fault == "read" ? "READ" : "WRITE", fault != "permission"); Target(f);
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AddCorrectiveActionAsync(fault == "foreign-incident" ? "foreign" : "local", Addition(), fault == "actor" ? "caller-actor" : "actor"));
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Fact]
    public async Task Actual_due_date_command_persists_recorded_target_and_audit_then_confirms_after_owned_commit()
    {
        using var f = await Ready(); var due = DateTime.UtcNow.AddDays(7); var service = ReadTests.Service(f);
        await service.SetDueDateAsync("local", 1, due, "actor");
        Assert.Equal(due, Step(f).DUE_DATE); Assert.Equal("actor", Step(f).ROW_CHANGED_BY); Assert.Equal(DateTimeKind.Utc, Step(f).ROW_CHANGED_DATE!.Value.Kind);
        Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions); Assert.Single(f.Writes); Assert.Null(f.LeaseActiveTransaction);
        Assert.Equal(due, Assert.Single(await ReadTests.Service(f).GetCAStatusAsync("local")).DueDate);
        await ReadTests.Service(f).SetDueDateAsync("local", 1, due, "actor"); Assert.Single(f.Writes); Assert.Equal(new[] { "begin", "commit", "begin", "commit" }, f.LeaseTransactions);
    }
    [Fact]
    public async Task Actual_completion_atomically_persists_step_and_status_and_retries_the_same_recorded_outcome()
    {
        using var f = await Ready(); Step(f).DUE_DATE = DateTime.UtcNow.AddDays(1);
        await ReadTests.Service(f).RecordCompletionAsync("local", 1, "recorded completion evidence", "actor");
        var status = Assert.Single(f.Rows.OfType<PROJECT_STATUS>(), row => row.PROJECT_ID == ReadTests.Plan && row.STATUS == "COMPLETED");
        Assert.Equal(Step(f).ACTUAL_END_DATE, status.EFFECTIVE_DATE); Assert.Equal("actor", status.ROW_CREATED_BY); Assert.Null(status.DEFINED_BY_BA_ID);
        Assert.Equal("recorded completion evidence", Step(f).REMARK); Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions); Assert.Equal(2, f.Writes.Count);
        Assert.True(await ReadTests.Service(f).AllCAsMeetDeadlineAsync("local"));
        await ReadTests.Service(f).RecordCompletionAsync("local", 1, "recorded completion evidence", "actor"); Assert.Equal(2, f.Writes.Count);
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).RecordCompletionAsync("local", 1, "different evidence", "actor")); Assert.Equal(2, f.Writes.Count);
    }
    [Theory] [InlineData("permission")] [InlineData("read-grant")] [InlineData("no-inheritance")] [InlineData("read-entitlement")]
    public async Task Current_view_or_noninherited_write_authority_cannot_replace_command_permission_and_inherited_entitled_write(string fault)
    {
        using var f = await Ready(fault == "read-grant" ? "READ" : "WRITE", fault != "permission");
        if (fault == "no-inheritance") { (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).Inherit = false; await f.Db.SaveChangesAsync(); }
        if (fault == "read-entitlement") f.Rows.OfType<ENTITLEMENT>().Single().ACCESS_COND_CODE = "test-read";
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).SetDueDateAsync("local", 1, DateTime.UtcNow.AddDays(2), "actor")); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("due-zone")] [InlineData("notes-missing")] [InlineData("sequence")]
    public async Task Invalid_command_inputs_cannot_begin_a_transaction(string fault)
    {
        using var f = await Ready(); var service = ReadTests.Service(f);
        await Assert.ThrowsAsync<RefusalException>(() => fault == "notes-missing" ? service.RecordCompletionAsync("local", 1, " ", "actor") : service.SetDueDateAsync("local", fault == "sequence" ? 0 : 1, fault == "due-zone" ? new(2026, 1, 1) : DateTime.UtcNow, "actor"));
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("no-op-begin")] [InlineData("failed-begin")]
    public async Task Unowned_or_failed_provider_begin_cannot_write(string fault)
    {
        using var f = await Ready(); f.LeaseBeginNoOp = fault == "no-op-begin"; f.LeaseBeginFailed = fault == "failed-begin";
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadTests.Service(f).SetDueDateAsync("local", 1, DateTime.UtcNow.AddDays(2), "actor")); Assert.Empty(f.Writes); Assert.Null(f.LeaseActiveTransaction);
    }
    [Theory] [InlineData(1)] [InlineData(2)]
    public async Task Either_completion_write_failure_rolls_back_the_compound_command(int failedWrite)
    {
        using var f = await Ready(); f.LeaseFailWriteNumber = failedWrite;
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadTests.Service(f).RecordCompletionAsync("local", 1, "recorded completion", "actor"));
        Assert.Null(Step(f).ACTUAL_END_DATE); Assert.DoesNotContain(f.Rows.OfType<PROJECT_STATUS>(), row => row.STATUS == "COMPLETED");
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); Assert.Null(f.LeaseActiveTransaction);
    }
    [Theory] [InlineData("no-op-update")] [InlineData("wrong-readback")]
    public async Task Provider_OK_without_exact_persisted_due_date_evidence_rolls_back(string fault)
    {
        using var f = await Ready(); var original = Step(f).DUE_DATE;
        if (fault == "no-op-update") f.Source.Setup(source => source.UpdateEntity("PROJECT_STEP", It.IsAny<object>())).Returns(new TheTechIdea.Beep.ConfigUtil.ErrorsInfo { Flag = TheTechIdea.Beep.ConfigUtil.Errors.Ok });
        else f.AfterLeaseWrite = row => { if (row is PROJECT_STEP step) step.DUE_DATE = DateTime.UtcNow.AddYears(1); };
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).SetDueDateAsync("local", 1, DateTime.UtcNow.AddDays(2), "actor"));
        Assert.Equal(original, Step(f).DUE_DATE); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Fact]
    public async Task Revocation_after_the_first_completion_write_refuses_and_rolls_back_before_status_insert()
    {
        using var f = await Ready(); f.AfterLeaseWrite = row => {
            if (row is PROJECT_STEP) { f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(claim => claim.ClaimValue == PermissionConstants.HSE.ManageIncidents)); f.Db.SaveChanges(); }
        };
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).RecordCompletionAsync("local", 1, "recorded evidence", "actor"));
        Assert.Null(Step(f).ACTUAL_END_DATE); Assert.Single(f.Writes); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Fact]
    public async Task Weaker_provider_isolation_cannot_authorize_a_corrective_action_write()
    {
        using var f = await Ready(); f.TransactionIsolation = System.Data.IsolationLevel.ReadCommitted;
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadTests.Service(f).SetDueDateAsync("local", 1, DateTime.UtcNow.AddDays(2), "actor"));
        Assert.Empty(f.Writes); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Fact]
    public async Task Lost_completion_commit_acknowledgment_requires_recorded_reconciliation_and_does_not_duplicate_a_retry()
    {
        using var f = await Ready(); f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadTests.Service(f).RecordCompletionAsync("local", 1, "recorded evidence", "actor"));
        Assert.NotNull(Step(f).ACTUAL_END_DATE); Assert.Single(f.Rows.OfType<PROJECT_STATUS>(), row => row.STATUS == "COMPLETED");
        Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions); Assert.Equal(2, f.Writes.Count);
        f.LeaseCommitFailed = false;
        await ReadTests.Service(f).RecordCompletionAsync("local", 1, "recorded evidence", "actor"); Assert.Equal(2, f.Writes.Count);
    }
    [Theory] [InlineData("missing")] [InlineData("unspecified")]
    public async Task Explicit_due_date_command_can_record_the_missing_or_replace_an_uninterpretable_due_date(string fault)
    {
        using var f = await Ready(); Step(f).DUE_DATE = fault == "missing" ? null : new DateTime(2026, 1, 1);
        var requested = DateTime.UtcNow.AddDays(3);
        await ReadTests.Service(f).SetDueDateAsync("local", 1, requested, "actor");
        Assert.Equal(requested, Step(f).DUE_DATE); Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions);
    }
    [Fact]
    public async Task Commit_failure_before_effect_rolls_back_and_does_not_report_success()
    {
        using var f = await Ready(); f.LeaseCommitFailed = true; var before = Step(f).DUE_DATE;
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadTests.Service(f).SetDueDateAsync("local", 1, DateTime.UtcNow.AddDays(3), "actor"));
        Assert.Equal(before, Step(f).DUE_DATE); Assert.Equal(new[] { "begin", "commit", "rollback" }, f.LeaseTransactions);
    }
    [Theory] [InlineData("link-key")] [InlineData("participant")] [InlineData("grant")] [InlineData("entitlement")]
    public async Task Relationship_or_write_authority_changes_after_the_first_write_roll_back_completion(string change)
    {
        using var f = await Ready();
        f.AfterLeaseWrite = row => {
            if (row is not PROJECT_STEP) return;
            if (change == "link-key") f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(value => value.PROJECT_ID == ReadTests.Plan).RESPONSE_OBS_NO = 9;
            else if (change == "participant") f.Rows.OfType<PROJECT_STEP_BA>().Single(value => value.PROJECT_ID == ReadTests.Plan).STEP_BA_OBS_NO = 12;
            else if (change == "entitlement") f.Rows.OfType<ENTITLEMENT>().Single().ACCESS_COND_CODE = "test-read";
            else { var grant = f.Db.Set<AppUserAssetAccess>().Single(); grant.AssetId = "B"; f.Db.SaveChanges(); }
        };
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).RecordCompletionAsync("local", 1, "recorded completion", "actor"));
        Assert.Null(Step(f).ACTUAL_END_DATE); Assert.DoesNotContain(f.Rows.OfType<PROJECT_STATUS>(), row => row.STATUS == "COMPLETED");
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Fact]
    public async Task Generic_HSE_mutations_stay_fenced_even_for_a_current_command_manager()
    {
        using var f = await Ready(); var row = Step(f);
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_STEP)).UpdateAsync(row, "actor")); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    private static PROJECT_STEP_BA Responsible(Fixture f) => f.Rows.OfType<PROJECT_STEP_BA>().Single(row => row.PROJECT_ID == ReadTests.Plan && row.ACTIVE_IND == "Y");
    internal static void Target(Fixture f, string id = "recorded-target", decimal sequence = 23)
    {
        f.Rows.AddRange([new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = id, ACTIVE_IND = "Y" },
            new PROJECT_BA { PROJECT_ID = ReadTests.Plan, BUSINESS_ASSOCIATE_ID = id, ACTIVE_IND = "Y" },
            new PROJECT_BA_ROLE { PROJECT_ID = ReadTests.Plan, BUSINESS_ASSOCIATE_ID = id, ROLE = "RESPONSIBLE", ROLE_SEQ_NO = sequence, ACTIVE_IND = "Y" }]);
    }
    [Fact]
    public async Task Actual_assignment_retires_the_old_full_key_and_inserts_the_recorded_target_role_with_atomic_audit_and_retry()
    {
        using var f = await Ready(); Target(f);
        var before = Responsible(f); var beforeGuid = before.PPDM_GUID;
        await ReadTests.Service(f).AssignResponsiblePersonAsync("local", 1, "recorded-target", "actor");
        var current = Responsible(f);
        Assert.Equal("recorded-target", current.BUSINESS_ASSOCIATE_ID); Assert.Equal(23, current.ROLE_SEQ_NO); Assert.Equal(12, current.STEP_BA_OBS_NO);
        Assert.Equal("actor", current.ROW_CREATED_BY); Assert.Equal("actor", current.ROW_CHANGED_BY); Assert.Equal(DateTimeKind.Utc, current.EFFECTIVE_DATE!.Value.Kind);
        var retired = Assert.Single(f.Rows.OfType<PROJECT_STEP_BA>(), row => row.PROJECT_ID == ReadTests.Plan && row.ACTIVE_IND == "N");
        Assert.Equal("local-person", retired.BUSINESS_ASSOCIATE_ID); Assert.Equal(7, retired.ROLE_SEQ_NO); Assert.Equal(11, retired.STEP_BA_OBS_NO); Assert.Equal(beforeGuid, retired.PPDM_GUID);
        Assert.Equal("actor", retired.ROW_CHANGED_BY); Assert.Null(retired.ROW_CREATED_BY);
        Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions); Assert.Equal(2, f.Writes.Count);
        Assert.Equal("recorded-target", Assert.Single(await ReadTests.Service(f).GetCAStatusAsync("local")).ResponsiblePerson);
        await ReadTests.Service(f).AssignResponsiblePersonAsync("local", 1, "recorded-target", "actor");
        Assert.Equal(2, f.Writes.Count); Assert.Equal(new[] { "begin", "commit", "begin", "commit" }, f.LeaseTransactions);
    }
    [Fact]
    public async Task First_assignment_uses_recorded_role_sequence_and_all_history_for_observation_allocation()
    {
        using var f = await Ready(); Target(f); var historic = Responsible(f); historic.ACTIVE_IND = "N"; historic.STEP_BA_OBS_NO = 41;
        await ReadTests.Service(f).AssignResponsiblePersonAsync("local", 1, "recorded-target", "actor");
        Assert.Equal(42, Responsible(f).STEP_BA_OBS_NO); Assert.Equal(23, Responsible(f).ROLE_SEQ_NO); Assert.Single(f.Writes);
        Assert.Single(f.Rows.OfType<PROJECT_STEP_BA>(), row => row.PROJECT_ID == ReadTests.Plan && row.ACTIVE_IND == "N");
    }
    [Theory] [InlineData(1)] [InlineData(2)]
    public async Task Either_assignment_write_failure_rolls_back_without_changing_primary_keys(int failedWrite)
    {
        using var f = await Ready(); Target(f); f.LeaseFailWriteNumber = failedWrite;
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadTests.Service(f).AssignResponsiblePersonAsync("local", 1, "recorded-target", "actor"));
        Assert.Equal("local-person", Responsible(f).BUSINESS_ASSOCIATE_ID); Assert.Equal(7, Responsible(f).ROLE_SEQ_NO); Assert.Equal(11, Responsible(f).STEP_BA_OBS_NO);
        Assert.Single(f.Rows.OfType<PROJECT_STEP_BA>(), row => row.PROJECT_ID == ReadTests.Plan); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Theory] [InlineData("missing-ba")] [InlineData("missing-member")] [InlineData("missing-role")] [InlineData("ambiguous-role")] [InlineData("expired-role")] [InlineData("foreign-member")] [InlineData("fractional-role")]
    public async Task Assignment_requires_one_actual_current_target_participant_role_before_begin(string fault)
    {
        using var f = await Ready(); Target(f); var role = f.Rows.OfType<PROJECT_BA_ROLE>().Single(row => row.BUSINESS_ASSOCIATE_ID == "recorded-target");
        if (fault == "missing-ba") f.Rows.RemoveAll(row => row is BUSINESS_ASSOCIATE ba && ba.BUSINESS_ASSOCIATE_ID == "recorded-target");
        if (fault == "missing-member") f.Rows.RemoveAll(row => row is PROJECT_BA member && member.BUSINESS_ASSOCIATE_ID == "recorded-target");
        if (fault == "missing-role") f.Rows.Remove(role);
        if (fault == "ambiguous-role") f.Rows.Add(new PROJECT_BA_ROLE { PROJECT_ID = ReadTests.Plan, BUSINESS_ASSOCIATE_ID = "recorded-target", ROLE = "RESPONSIBLE", ROLE_SEQ_NO = 24, ACTIVE_IND = "Y" });
        if (fault == "expired-role") role.EXPIRY_DATE = DateTime.UtcNow.AddDays(-1);
        if (fault == "foreign-member") f.Rows.OfType<PROJECT_BA>().Single(row => row.BUSINESS_ASSOCIATE_ID == "recorded-target").PROJECT_ID = "foreign-project";
        if (fault == "fractional-role") role.ROLE_SEQ_NO = 2.5m;
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AssignResponsiblePersonAsync("local", 1, "recorded-target", "actor")); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("role")] [InlineData("permission")] [InlineData("state")] [InlineData("history")]
    public async Task Assignment_drift_after_retirement_refuses_and_rolls_back_before_confirmation(string fault)
    {
        using var f = await Ready(); Target(f);
        f.AfterLeaseWrite = row => {
            if (row is not PROJECT_STEP_BA { ACTIVE_IND: "N" }) return;
            if (fault == "role") f.Rows.OfType<PROJECT_BA_ROLE>().Single(value => value.BUSINESS_ASSOCIATE_ID == "recorded-target").ROLE_SEQ_NO = 24;
            if (fault == "permission") { f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(claim => claim.ClaimValue == PermissionConstants.HSE.ManageIncidents)); f.Db.SaveChanges(); }
            if (fault == "state") f.Rows.OfType<PROJECT_STATUS>().Single(value => value.PROJECT_ID == ReadTests.Plan).STATUS = "COMPLETED";
            if (fault == "history") f.Rows.OfType<PROJECT_STEP_BA>().Single(value => value.PROJECT_ID == ReadTests.Plan && value.ACTIVE_IND == "N").REMARK = "concurrent history rewrite";
        };
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AssignResponsiblePersonAsync("local", 1, "recorded-target", "actor"));
        Assert.Equal("local-person", Responsible(f).BUSINESS_ASSOCIATE_ID); Assert.DoesNotContain(f.Rows.OfType<PROJECT_STEP_BA>(), row => row.BUSINESS_ASSOCIATE_ID == "recorded-target"); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Theory] [InlineData("no-op")] [InlineData("wrong-readback")]
    public async Task Assignment_provider_OK_requires_exact_full_saved_history_and_active_row(string fault)
    {
        using var f = await Ready(); Target(f);
        if (fault == "no-op") f.Source.Setup(source => source.UpdateEntity("PROJECT_STEP_BA", It.IsAny<object>())).Returns(new TheTechIdea.Beep.ConfigUtil.ErrorsInfo { Flag = TheTechIdea.Beep.ConfigUtil.Errors.Ok });
        else f.AfterLeaseWrite = row => { if (row is PROJECT_STEP_BA { ACTIVE_IND: "Y" } assignment) assignment.REMARK = "provider mismatch"; };
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AssignResponsiblePersonAsync("local", 1, "recorded-target", "actor"));
        Assert.Equal("local-person", Responsible(f).BUSINESS_ASSOCIATE_ID); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Fact]
    public async Task Lost_assignment_commit_acknowledgment_reconciles_recorded_effect_without_another_history_row()
    {
        using var f = await Ready(); Target(f); f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReadTests.Service(f).AssignResponsiblePersonAsync("local", 1, "recorded-target", "actor"));
        Assert.Equal("recorded-target", Responsible(f).BUSINESS_ASSOCIATE_ID); Assert.Equal(2, f.Writes.Count);
        f.LeaseCommitFailed = false; await ReadTests.Service(f).AssignResponsiblePersonAsync("local", 1, "recorded-target", "actor");
        Assert.Equal(2, f.Writes.Count); Assert.Equal(2, f.Rows.OfType<PROJECT_STEP_BA>().Count(row => row.PROJECT_ID == ReadTests.Plan));
    }
    [Theory] [InlineData("read")] [InlineData("permission")] [InlineData("completed")] [InlineData("identity")] [InlineData("inactive-key-duplicate")]
    public async Task Assignment_request_or_current_authority_cannot_admit_an_unreviewed_command(string fault)
    {
        using var f = await Ready(fault == "read" ? "READ" : "WRITE", fault != "permission"); Target(f);
        if (fault == "completed") { Step(f).ACTUAL_END_DATE = DateTime.UtcNow.AddDays(-1); var state = f.Rows.OfType<PROJECT_STATUS>().Single(row => row.PROJECT_ID == ReadTests.Plan); state.STATUS = "COMPLETED"; state.EFFECTIVE_DATE = Step(f).ACTUAL_END_DATE; }
        if (fault == "inactive-key-duplicate") { var old = Responsible(f); f.Rows.Add(new PROJECT_STEP_BA { PROJECT_ID = old.PROJECT_ID, STEP_ID = old.STEP_ID, BUSINESS_ASSOCIATE_ID = old.BUSINESS_ASSOCIATE_ID, ROLE = old.ROLE, ROLE_SEQ_NO = old.ROLE_SEQ_NO, STEP_BA_OBS_NO = old.STEP_BA_OBS_NO, ACTIVE_IND = "N" }); }
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AssignResponsiblePersonAsync("local", 1, fault == "identity" ? " recorded-target" : "recorded-target", "actor")); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Fact]
    public async Task Requested_role_payload_change_during_policy_validation_cannot_retain_stale_participant_evidence()
    {
        using var f = await Ready(); Target(f); var changed = false;
        f.AfterRead = () => {
            var query = f.Queries.Last();
            if (!changed && query.Any(filter => filter.FieldName == "ROLE") && query.Any(filter => filter.FieldName == "BUSINESS_ASSOCIATE_ID" && filter.FilterValue == "recorded-target") && !query.Any(filter => filter.FieldName == "ROLE_SEQ_NO"))
            { changed = true; f.Rows.OfType<PROJECT_BA_ROLE>().Single(row => row.BUSINESS_ASSOCIATE_ID == "recorded-target").REMARK = "changed recorded role evidence"; }
            return Task.CompletedTask;
        };
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AssignResponsiblePersonAsync("local", 1, "recorded-target", "actor"));
        Assert.True(changed); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("inventory-limit")] [InlineData("allocation-limit")]
    public async Task Responsible_assignment_history_or_allocation_limit_cannot_become_an_invented_identity(string fault)
    {
        using var f = await Ready(); Target(f); var original = Responsible(f);
        if (fault == "allocation-limit") original.STEP_BA_OBS_NO = int.MaxValue;
        else for (var index = 0; index < 257; index++) f.Rows.Add(new PROJECT_STEP_BA { PROJECT_ID = original.PROJECT_ID, STEP_ID = original.STEP_ID,
            BUSINESS_ASSOCIATE_ID = "historical-" + index, ROLE = "RESPONSIBLE", ROLE_SEQ_NO = 7, STEP_BA_OBS_NO = index + 20, ACTIVE_IND = "N" });
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AssignResponsiblePersonAsync("local", 1, "recorded-target", "actor"));
        Assert.Empty(f.Writes); Assert.Equal("local-person", Responsible(f).BUSINESS_ASSOCIATE_ID);
        Assert.Equal(fault == "inventory-limit" ? Array.Empty<string>() : new[] { "begin", "rollback" }, f.LeaseTransactions);
    }
    [Fact]
    public async Task Shared_transaction_owner_cannot_be_asserted_outside_an_owned_command()
    { using var f = await ReadTests.Ready(); Assert.Throws<InvalidOperationException>(() => PPDMDataSourceTransaction.RequireOwned(f.Source.Object)); }
    [Fact]
    public async Task Withheld_restricted_case_prevents_linked_CA_read_and_addition_before_any_transaction_or_write()
    {
        using var f = await Ready(); Target(f); CompanyHseRowAccessTests.Restrict(f, true);
        await CompanyHseRowAccessTests.ReviewPermissions(f); await CompanyHseRowAccessTests.Bind(f);
        f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row => row.ClaimValue == "HSE.ViewRestricted").ToArrayAsync()); await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(PROJECT_STEP)).GetAsync(new()));
        await f.Select("company");
        Assert.Empty(await f.Repository(typeof(PROJECT_STEP)).GetAsync(new()));
        await Assert.ThrowsAsync<RefusalException>(() => ReadTests.Service(f).AddCorrectiveActionAsync("local", Addition(), "actor"));
        Assert.Empty(f.LeaseTransactions); Assert.Empty(f.Writes);
    }

}
