using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text.Json;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Web.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed partial class DrillingExecutionStartApiTests
{
    private static async Task InspectReportPages(DrillingProgramApiTests.Fixture f, DrillingExecutionStart execution,
        DrillingServiceClient client, HttpClient http, Action<string> selection)
    {
        var db = f.Funding.Routing.Afe.Db; var native = f.Store.Store.Native;
        var basis = await client.GetDailyReportBasisAsync(execution.Basis.ReviewInstanceId);
        var first = new DrillingDailyReportInput(basis, Guid.NewGuid(), "ConfiguredExecutionRole", execution.SpudAtUtc,
            DateTimeOffset.UtcNow, 0, 125, execution.Basis.Source.Program.DEPTH_OUOM, null, null, null,
            "Actual first report recorded through API", "Observed source for checked history pages", null);
        var recorded = (await client.RecordDailyReportAsync(first)).Recorded!;
        var nextBasis = basis with { NextNumber = 2, PreviousReportId = recorded.DDR_ID, PreviousReportRevision = recorded.REPORT_REVISION,
            PreviousPeriodEndUtc = first.PeriodEndUtc, CheckedAtUtc = DateTimeOffset.UtcNow };
        nextBasis = nextBasis with { Revision = nextBasis.ContentRevision() };
        var seed = first with { Basis = nextBasis, PeriodStartUtc = first.PeriodEndUtc, PeriodEndUtc = DateTimeOffset.UtcNow };
        // Controlled persisted-provider population after one real API record; no claim that these fixtures were recorded over HTTP.
        var later = DrillingDailyReportPageNativeTests.Series(seed, 26); native.Rows.AddRange(later);
        db.RemoveRange(await db.RoleClaims.Where(row => row.RoleId == "execution-role" && (row.ClaimValue == "Drilling.Create" || row.ClaimValue == "Drilling.Edit" || row.ClaimValue == "Drilling.ManageWellProgram")).ToArrayAsync());
        db.Add(new AppPersona { Code = "PAGED_READ_ONLY", Name = "Unmapped report history reader" });
        (await db.Set<AppUserPersona>().SingleAsync(row => row.UserId == "site-recorder")).PersonaCode = "PAGED_READ_ONLY";
        var grant = await db.Set<AppUserAssetAccess>().SingleAsync(row => row.UserId == "site-recorder"); grant.AccessLevel = "READ";
        await db.SaveChangesAsync(); await f.Select("site-recorder", "ConfiguredExecutionRole"); selection("site-recorder"); f.Well.ACTIVE_IND = "N";
        var before = JsonSerializer.Serialize(native.Rows); var writes = native.Writes;
        var page = await client.GetDailyReportPageAsync(execution.Basis.ReviewInstanceId, 0, 10);
        Assert.Equal(27, page.History.Count); Assert.Equal(27, page.History.Head!.Number); Assert.Equal(10, page.Reports.Count);
        Assert.Equal(recorded.DDR_ID, page.Reports[0].DDR_ID); Assert.Equal("site-recorder", page.Reports[0].ROW_CREATED_BY);
        Assert.DoesNotContain("REPORT_ORIGINAL_JSON", JsonSerializer.Serialize(page));
        var second = await client.GetDailyReportPageAsync(execution.Basis.ReviewInstanceId, 10, 10, page.History.Revision);
        var last = await client.GetDailyReportPageAsync(execution.Basis.ReviewInstanceId, 20, 10, page.History.Revision);
        Assert.Equal(11, second.Reports[0].REPORT_NUMBER); Assert.Equal(27, last.Reports[^1].REPORT_NUMBER); Assert.False(last.HasNext);
        Assert.Equal(page.History.Revision, last.History.Revision); Assert.Equal(before, JsonSerializer.Serialize(native.Rows)); Assert.Equal(writes, native.Writes);
        var route = "/api/drilling/programs/execution/reports?reviewId=" + Uri.EscapeDataString(execution.Basis.ReviewInstanceId);
        Assert.Equal(HttpStatusCode.Conflict, (await http.GetAsync(route + "&size=51")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await http.GetAsync(route + "&after=1")).StatusCode);
        grant.IsActive = false; await db.SaveChangesAsync(); Assert.Equal(HttpStatusCode.Forbidden, (await http.GetAsync(route)).StatusCode);
        grant.IsActive = true; await db.SaveChangesAsync(); Action? revoke = () => { grant.IsActive = false; db.SaveChanges(); };
        native.Source.Setup(s => s.GetEntityAsync("DRILLING_DAILY_REPORT", It.IsAny<List<AppFilter>>())).ReturnsAsync((string table, List<AppFilter> filters) =>
        {
            if (filters.Any(x => x.FieldName == "REPORT_NUMBER" && x.Operator == ">=")) { var action = revoke; revoke = null; action?.Invoke(); }
            return native.Rows.Where(row => row is DRILLING_DAILY_REPORT && DrillingDailyReportNativeTests.Matches(row, filters))
                .Select(row => CompanyInventorySourceReview.Copy((DRILLING_DAILY_REPORT)row)).ToArray();
        });
        Assert.Equal(HttpStatusCode.Forbidden, (await http.GetAsync(route)).StatusCode); Assert.Null(revoke);
        grant.IsActive = true; await db.SaveChangesAsync();
        var lifecycle = f.Funding.Routing.Catalog.Workflow.Native;
        var approvedStep = lifecycle.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_STEP_INSTANCE>()
            .Single(row => row.PROCESS_INSTANCE_ID == execution.Basis.ReviewInstanceId);
        var stepId = approvedStep.PROCESS_STEP_INSTANCE_ID; var approvedStatus = approvedStep.STATUS;
        revoke = () => lifecycle.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_STEP_INSTANCE>()
            .Single(row => row.PROCESS_STEP_INSTANCE_ID == stepId).STATUS = "PENDING";
        var changedProgram = await http.GetAsync(route);
        Assert.Equal(HttpStatusCode.Conflict, changedProgram.StatusCode); Assert.Null(revoke);
        Assert.DoesNotContain(recorded.DDR_ID, await changedProgram.Content.ReadAsStringAsync());
        // A refused native read can restore defensive copies; restore by exact key.
        lifecycle.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_STEP_INSTANCE>()
            .Single(row => row.PROCESS_STEP_INSTANCE_ID == stepId).STATUS = approvedStatus;
        Assert.Equal(before, JsonSerializer.Serialize(native.Rows)); Assert.Equal(writes, native.Writes);
        var head = later[^1]; var original = DrillingDailyReportInput.FromRecord(head);
        var appendedBasis = original.Basis with { NextNumber = 28, PreviousReportId = head.DDR_ID, PreviousReportRevision = head.REPORT_REVISION,
            PreviousPeriodEndUtc = original.PeriodEndUtc, CheckedAtUtc = DateTimeOffset.UtcNow };
        appendedBasis = appendedBasis with { Revision = appendedBasis.ContentRevision() };
        var appended = original with { Basis = appendedBasis, IntentId = Guid.NewGuid(), PeriodStartUtc = original.PeriodEndUtc, PeriodEndUtc = DateTimeOffset.UtcNow };
        native.Rows.Add(appended.ToRecord(DateTime.UtcNow)); before = JsonSerializer.Serialize(native.Rows);
        Assert.Equal(HttpStatusCode.Conflict, (await http.GetAsync(route + "&after=20&size=10&historyRevision=" + page.History.Revision)).StatusCode);
        Assert.Equal(28, (await client.GetDailyReportPageAsync(execution.Basis.ReviewInstanceId, 0, 10)).History.Count);
        var view = await db.RoleClaims.SingleAsync(row => row.RoleId == "execution-role" && row.ClaimValue == "Drilling.ViewDailyReports");
        db.Remove(view); await db.SaveChangesAsync(); Assert.Equal(HttpStatusCode.Forbidden, (await http.GetAsync(route)).StatusCode);
        view.Id = 0; db.RoleClaims.Add(view); await db.SaveChangesAsync();
        await f.Select("foreign-reviewer", "ConfiguredReviewRole", new("tenant", "foreign"));
        http.DefaultRequestHeaders.Remove("X-Test-Actor"); http.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");
        http.DefaultRequestHeaders.Add("X-Test-Actor", "foreign-reviewer"); http.DefaultRequestHeaders.Add("X-Test-Assigned-Role", "ConfiguredReviewRole");
        var foreign = await http.GetAsync(route); Assert.Equal(HttpStatusCode.Forbidden, foreign.StatusCode);
        Assert.DoesNotContain(recorded.DDR_ID, await foreign.Content.ReadAsStringAsync()); Assert.DoesNotContain(first.Provenance, await foreign.Content.ReadAsStringAsync());
        http.DefaultRequestHeaders.Remove("X-Test-Auth"); Assert.Equal(HttpStatusCode.Unauthorized, (await http.GetAsync(route)).StatusCode);
        Assert.Equal(before, JsonSerializer.Serialize(native.Rows)); Assert.Equal(writes, native.Writes);
    }
}
#endif
