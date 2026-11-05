#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Web.Services;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed partial class DrillingExecutionStartApiTests
{
    private static async Task CorrectObservedReport(DrillingProgramApiTests.Fixture f, DrillingExecutionStart execution,
        DrillingServiceClient client, HttpClient http, Action<string> selection, bool completionReport = false, bool recipientEvidence = false, bool routingEvidence = false, bool submissionEvidence = false, bool decisionEvidence = false,bool acceptanceEvidence=false)
    {
        var db = f.Funding.Routing.Afe.Db; var native = f.Store.Store.Native;
        var basis = await client.GetDailyReportBasisAsync(execution.Basis.ReviewInstanceId);
        var routingPath = "/api/drilling/programs/execution/completion/review?reviewId=" + Uri.EscapeDataString(execution.Basis.ReviewInstanceId);
        if (routingEvidence) Assert.Equal(HttpStatusCode.Conflict, (await http.GetAsync(routingPath)).StatusCode);
        var first = new DrillingDailyReportInput(basis, Guid.NewGuid(), "ConfiguredExecutionRole", execution.SpudAtUtc,
            DateTimeOffset.UtcNow, 0, 125, execution.Basis.Source.Program.DEPTH_OUOM, null, null, null,
            "Original signed site measurement reference", "Original observed drilling activity", null);
        var recorded = (await client.RecordDailyReportAsync(first)).Recorded!;
        var nextBasis = basis with { NextNumber = 2, PreviousReportId = recorded.DDR_ID, PreviousReportRevision = recorded.REPORT_REVISION,
            PreviousPeriodEndUtc = first.PeriodEndUtc, CheckedAtUtc = DateTimeOffset.UtcNow };
        nextBasis = nextBasis with { Revision = nextBasis.ContentRevision() };
        // One later canonical persisted-provider fixture, following the actual API-recorded original.
        var second = (first with { Basis = nextBasis, IntentId = Guid.NewGuid(), PeriodStartUtc = first.PeriodEndUtc,
            PeriodEndUtc = DateTimeOffset.UtcNow, Provenance = "Later persisted site measurement" }).ToRecord(DateTime.UtcNow);
        native.Rows.Add(second); var before = JsonSerializer.Serialize(native.Rows.OfType<DRILLING_DAILY_REPORT>());
        var current = await client.GetDailyReportBasisAsync(execution.Basis.ReviewInstanceId);
        var target = completionReport ? second : recorded;
        var correction = (completionReport ? DrillingDailyReportInput.FromRecord(second) : first) with { Basis = current, IntentId = Guid.NewGuid(), DepthEndMd = 126,
            Summary = "Corrected observed activity reconciled against the signed log",
            Correction = new(target.DDR_ID, target.REPORT_REVISION, target.REPORT_NUMBER, completionReport ? "Explicit completion report against the latest observed period" : "Signed log corrected the earlier hole MD and operation description"),
            Completion = completionReport ? DrillingCompletionReportNativeTests.Evidence() : null };
        if (completionReport) correction = correction with { Completion = correction.Completion! with { Tests = [correction.Completion.Tests.Single() with { ReportedResult = acceptanceEvidence?"REPORTED_PASS":"REPORTED_FAIL" }],OutstandingItems=acceptanceEvidence?[]:correction.Completion.OutstandingItems } };
        native.AfterCommit = () => { if (native.Rows.OfType<DRILLING_DAILY_REPORT>().Count() == 3) native.FailCommit = true; };
        await Assert.ThrowsAnyAsync<Exception>(() => client.RecordDailyReportAsync(correction)); native.AfterCommit = null; native.FailCommit = false;
        Assert.Equal(before, JsonSerializer.Serialize(native.Rows.OfType<DRILLING_DAILY_REPORT>().Where(row => row.REPORT_NUMBER < 3)));
        Assert.Equal(3, native.Rows.OfType<DRILLING_DAILY_REPORT>().Count()); var writes = native.Writes;
        DrillingCompletionReviewStart? completionReview = null;
        if (routingEvidence)
        {
            var routing = await client.GetCompletionRoutingAsync(execution.Basis.ReviewInstanceId);
            var option = Assert.Single(routing.Options); Assert.True(option.Ready); Assert.Equal(2, option.Stages.Count);
            Assert.All(option.Stages, stage => Assert.Equal("completion-inspector", Assert.Single(stage.Authority.Recipients).UserId));
            Assert.Equal(correction.ReportId(), routing.Source.ReportedCompletion!.ReportId);
            Assert.Equal(acceptanceEvidence?"REPORTED_PASS":"REPORTED_FAIL", routing.Source.ReportedCompletion.Report.Tests.Single().ReportedResult);
            var recipientGrant = await db.Set<AppUserAssetAccess>().SingleAsync(row => row.Id == "completion-inspection-field");
            recipientGrant.IsActive = false; await db.SaveChangesAsync();
            var blocked = await client.GetCompletionRoutingAsync(execution.Basis.ReviewInstanceId);
            Assert.False(Assert.Single(blocked.Options).Ready); Assert.All(blocked.Options.Single().Stages, stage => Assert.Empty(stage.Authority.Recipients));
            recipientGrant.IsActive = true; await db.SaveChangesAsync(); Assert.Equal(writes, native.Writes);
            if (submissionEvidence) completionReview = await SubmitObservedCompletion(f, client, http, routing);
        }
        db.RemoveRange(await db.RoleClaims.Where(row => row.RoleId == "execution-role" && (row.ClaimValue == "Drilling.Create" || row.ClaimValue == "Drilling.Edit" || row.ClaimValue == "Drilling.ManageWellProgram")).ToArrayAsync());
        db.Add(new AppPersona { Code = "CORRECTION_READ_ONLY", Name = "Unmapped correction recovery reader" });
        (await db.Set<AppUserPersona>().SingleAsync(row => row.UserId == "site-recorder")).PersonaCode = "CORRECTION_READ_ONLY";
        var grant = await db.Set<AppUserAssetAccess>().SingleAsync(row => row.UserId == "site-recorder"); grant.AccessLevel = "READ";
        await db.SaveChangesAsync(); await f.Select("site-recorder", "ConfiguredExecutionRole"); selection("site-recorder"); f.Well.ACTIVE_IND = "N";
        grant.IsActive=false;await db.SaveChangesAsync();
        Assert.Equal(HttpStatusCode.Forbidden,(await http.PostAsJsonAsync("/api/drilling/programs/execution/reports/recover",correction)).StatusCode);Assert.Equal(writes,native.Writes);
        grant.IsActive=true;await db.SaveChangesAsync();await f.Select("site-recorder","ConfiguredExecutionRole");selection("site-recorder");
        var role=await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="site-recorder" && row.RoleId=="execution-role");var revokedDuringRecovery=false;
        native.AfterCommit=()=>{if(!revokedDuringRecovery){revokedDuringRecovery=true;role.ApprovalStatus="Revoked";db.SaveChanges();}};
        try
        {
            Assert.Equal(HttpStatusCode.Forbidden,(await http.PostAsJsonAsync("/api/drilling/programs/execution/reports/recover",correction)).StatusCode);
            Assert.True(revokedDuringRecovery);Assert.Equal(writes,native.Writes);
        }
        finally{native.AfterCommit=null;role.ApprovalStatus="Approved";await db.SaveChangesAsync();await f.Select("site-recorder","ConfiguredExecutionRole");selection("site-recorder");}
        var recovered = await client.RecoverDailyReportAsync(correction); recovered.Require(correction);
        Assert.NotNull(recovered.Recorded); Assert.Equal(correction.Correction, DrillingDailyReportInput.FromRecord(recovered.Recorded!).Correction);
        Assert.Equal(writes, native.Writes); var page = await client.GetDailyReportPageAsync(execution.Basis.ReviewInstanceId);
        Assert.Equal(3, page.History.Count); Assert.Equal(2, page.History.ObservedPeriodCount);
        Assert.Equal(recovered.ReportId, page.Reports[completionReport ? 1 : 0].SupersededByReportId); Assert.Equal(correction.Correction.Reason, page.Reports[2].Correction!.Reason);
        Assert.Equal(second.PERIOD_END_UTC, page.History.Head!.LatestPeriodEndUtc.UtcDateTime);
        if (completionReport)
        {
            Assert.Equal("COMPLETION_REPORTED", page.History.ProgressState); Assert.Equal(recovered.ReportId, page.History.ReportedCompletion!.ReportId);
            Assert.Equal(acceptanceEvidence?"REPORTED_PASS":"REPORTED_FAIL", page.History.ReportedCompletion.Report.Tests.Single().ReportedResult);if(acceptanceEvidence)Assert.Empty(page.History.ReportedCompletion.Report.OutstandingItems);else Assert.NotEmpty(page.History.ReportedCompletion.Report.OutstandingItems);
            Assert.Equal("STARTED", page.History.Execution.Recorded!.STATUS);
        }
        else Assert.Null(page.History.ReportedCompletion);
        if (routingEvidence)
        {
            var readOnly = await client.GetCompletionRoutingAsync(execution.Basis.ReviewInstanceId);
            Assert.Empty(readOnly.SenderRoles); Assert.Empty(readOnly.Options); Assert.Equal(recovered.ReportId, readOnly.Source.ReportedCompletion!.ReportId);
            if (completionReview is not null)
            {
                Assert.Equal(completionReview.InstanceId(),readOnly.ExistingReviewInstanceId);
                await RecoverObservedCompletionAndOpenInbox(f,client,http,selection,completionReview,decisionEvidence,acceptanceEvidence);
            }
        }
        if (recipientEvidence)
        {
            var lifecycle = f.Funding.Routing.Catalog.Workflow.Native;
            var assignment = lifecycle.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_INSTANCE>().Single(row => row.ENTITY_TYPE == "DRILLING_OPERATION");
            await f.Select("execution-recipient", "ConfiguredReportingRole");
            http.DefaultRequestHeaders.Remove("X-Test-Actor"); http.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");
            http.DefaultRequestHeaders.Add("X-Test-Actor", "execution-recipient"); http.DefaultRequestHeaders.Add("X-Test-Assigned-Role", "ConfiguredReportingRole"); selection("execution-recipient");
            var assigned = await client.GetAssignedExecutionAsync(assignment.PROCESS_INSTANCE_ID);
            var recipientPage = await client.GetDailyReportPageAsync(execution.Basis.ReviewInstanceId);
            Assert.Equal("execution-recipient", recipientPage.History.Execution.Basis.Scope().ActorUserId);
            Assert.Equal(recovered.ReportId, recipientPage.History.ReportedCompletion!.ReportId);
            Assert.Equal("site-recorder", recipientPage.History.ReportedCompletion.RecorderUserId);
            Assert.Equal(assigned.TaskId, lifecycle.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.CROSS_PERSONA_TASK>().Single(row => row.PROCESS_INSTANCE_ID == assigned.InstanceId).CROSS_TASK_ID);
            Assert.All(lifecycle.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.CROSS_PERSONA_TASK>().Where(row => row.PROCESS_INSTANCE_ID == assigned.InstanceId), row => Assert.Equal("PENDING", row.TASK_STATUS));
            await f.Select("site-recorder", "ConfiguredExecutionRole");
            http.DefaultRequestHeaders.Remove("X-Test-Actor"); http.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");
            http.DefaultRequestHeaders.Add("X-Test-Actor", "site-recorder"); http.DefaultRequestHeaders.Add("X-Test-Assigned-Role", "ConfiguredExecutionRole"); selection("site-recorder");
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await http.PostAsJsonAsync("/api/drilling/programs/execution/reports", correction)).StatusCode);
        grant.IsActive = false; await db.SaveChangesAsync();
        if(completionReview is not null) Assert.Equal(HttpStatusCode.Forbidden,(await http.PostAsJsonAsync("/api/drilling/programs/execution/completion/review/recover",completionReview)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await http.PostAsJsonAsync("/api/drilling/programs/execution/reports/recover", correction)).StatusCode);
        grant.IsActive = true; await db.SaveChangesAsync();
        await f.Select("foreign-reviewer", "ConfiguredReviewRole", new("tenant", "foreign"));
        http.DefaultRequestHeaders.Remove("X-Test-Actor"); http.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");
        http.DefaultRequestHeaders.Add("X-Test-Actor", "foreign-reviewer"); http.DefaultRequestHeaders.Add("X-Test-Assigned-Role", "ConfiguredReviewRole");
        var foreign = await http.GetAsync("/api/drilling/programs/execution/reports?reviewId=" + Uri.EscapeDataString(execution.Basis.ReviewInstanceId));
        if(completionReview is not null)
        {
            var refused=await http.GetAsync("/api/drilling/programs/execution/completion/assigned?processId="+Uri.EscapeDataString(completionReview.InstanceId()));
            Assert.Equal(HttpStatusCode.Forbidden,refused.StatusCode);Assert.DoesNotContain(completionReview.Reason,await refused.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.Forbidden,(await http.PostAsJsonAsync("/api/drilling/programs/execution/completion/review/recover",completionReview)).StatusCode);
        }
        if (routingEvidence)
        {
            var refused = await http.GetAsync(routingPath); Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
            Assert.DoesNotContain("completion-inspector", await refused.Content.ReadAsStringAsync());
        }
        Assert.Equal(HttpStatusCode.Forbidden, foreign.StatusCode); Assert.DoesNotContain(correction.Correction.Reason, await foreign.Content.ReadAsStringAsync());
        http.DefaultRequestHeaders.Remove("X-Test-Auth");
        if(completionReview is not null)
        {
            Assert.Equal(HttpStatusCode.Unauthorized,(await http.GetAsync("/api/drilling/programs/execution/completion/assigned?processId="+Uri.EscapeDataString(completionReview.InstanceId()))).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized,(await http.PostAsJsonAsync("/api/drilling/programs/execution/completion/review/recover",completionReview)).StatusCode);
        }
        if (routingEvidence) Assert.Equal(HttpStatusCode.Unauthorized, (await http.GetAsync(routingPath)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync("/api/drilling/programs/execution/reports/recover", correction)).StatusCode);
        Assert.Equal(writes, native.Writes); Assert.Equal(before, JsonSerializer.Serialize(native.Rows.OfType<DRILLING_DAILY_REPORT>().Where(row => row.REPORT_NUMBER < 3)));
    }
}
#endif
