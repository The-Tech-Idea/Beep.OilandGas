#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Web.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed partial class DrillingExecutionStartApiTests
{
    private static async Task ReconcileReportHistory(DrillingProgramApiTests.Fixture f, DrillingExecutionStart execution,
        DrillingServiceClient client, HttpClient http, Action<string> selection)
    {
        var db = f.Funding.Routing.Afe.Db; var native = f.Store.Store.Native;
        var basis = await client.GetDailyReportBasisAsync(execution.Basis.ReviewInstanceId);
        var recorded = new DrillingDailyReportInput(basis, Guid.NewGuid(), "ConfiguredExecutionRole", execution.SpudAtUtc,
            DateTimeOffset.UtcNow, 0, 125, execution.Basis.Source.Program.DEPTH_OUOM, null, null, null,
            "Actual competing site log 472", "Observed drilling recorded by the site recorder", null);
        native.AfterCommit = () => { if (native.Rows.OfType<DRILLING_DAILY_REPORT>().Any()) native.FailCommit = true; };
        await Assert.ThrowsAnyAsync<Exception>(() => client.RecordDailyReportAsync(recorded)); native.AfterCommit = null; native.FailCommit = false;
        Assert.Single(native.Rows.OfType<DRILLING_DAILY_REPORT>());
        var original = recorded with { IntentId = Guid.NewGuid(), Provenance = "My different retained site log 471", DepthEndMd = 120 };
        db.RemoveRange(await db.RoleClaims.Where(row => row.RoleId == "execution-role" && (row.ClaimValue == "Drilling.Create" || row.ClaimValue == "Drilling.Edit" || row.ClaimValue == "Drilling.ManageWellProgram")).ToArrayAsync());
        var grant = await db.Set<AppUserAssetAccess>().SingleAsync(row => row.UserId == "site-recorder"); grant.AccessLevel = "READ";
        db.Add(new AppPersona { Code = "REPORT_UNMAPPED", Name = "Historical report reader without recorder mapping" });
        (await db.Set<AppUserPersona>().SingleAsync(row => row.UserId == "site-recorder")).PersonaCode = "REPORT_UNMAPPED";
        await db.SaveChangesAsync(); await f.Select("site-recorder", "ConfiguredExecutionRole"); selection("site-recorder"); f.Well.ACTIVE_IND = "N";
        Assert.NotNull((await client.RecoverDailyReportAsync(recorded)).Recorded);
        var before = JsonSerializer.Serialize(native.Rows); var writes = native.Writes;
        var proof = await client.GetDailyReportConflictAsync(original); proof.Require(original);
        Assert.Equal(recorded.ReportId(), proof.Competing.DDR_ID); Assert.Equal(recorded.Provenance, proof.Competing.REPORT_PROVENANCE);
        Assert.Equal(125, proof.Competing.DEPTH_END_MD); Assert.Equal("site-recorder", proof.Competing.ROW_CREATED_BY);
        var confirmed = await client.ConfirmDailyReportConflictAsync(new(original, proof.Revision));
        Assert.Equal(proof.Revision, confirmed.Revision); Assert.True(confirmed.CheckedAtUtc >= proof.CheckedAtUtc);
        const string route = "/api/drilling/programs/execution/reports/conflict";
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsJsonAsync(route, recorded)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsJsonAsync(route + "/confirm", new DrillingDailyReportConflictAcknowledgement(original, new('F', 64)))).StatusCode);
        Assert.Equal(before, JsonSerializer.Serialize(native.Rows)); Assert.Equal(writes, native.Writes);
        grant.IsActive = false; await db.SaveChangesAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await http.PostAsJsonAsync(route, original)).StatusCode);
        grant.IsActive = true; await db.SaveChangesAsync(); Action? revoke = () => { grant.IsActive = false; db.SaveChanges(); };
        native.Source.Setup(s => s.GetEntityAsync("DRILLING_DAILY_REPORT", It.IsAny<List<AppFilter>>())).ReturnsAsync((string table, List<AppFilter> filters) =>
        {
            var action = revoke; revoke = null; action?.Invoke();
            return native.Rows.Where(row => row.GetType().Name == table && DrillingDailyReportNativeTests.Matches(row, filters))
                .Select(row => JsonSerializer.Deserialize(JsonSerializer.Serialize(row), row.GetType())!).ToArray();
        });
        Assert.Equal(HttpStatusCode.Forbidden, (await http.PostAsJsonAsync(route, original)).StatusCode); Assert.Null(revoke);
        grant.IsActive = true; await db.SaveChangesAsync();
        var orphan = CompanyInventorySourceReview.Copy(native.Rows.OfType<DRILLING_DAILY_REPORT>().Single());
        orphan.DDR_ID = original.ReportId(); orphan.DRILLING_OPERATION_ID = "retagged-original-operation"; native.Rows.Add(orphan);
        Assert.Equal(HttpStatusCode.Conflict, (await http.PostAsJsonAsync(route, original)).StatusCode);
        // Read-transaction rollback restores defensive copies. Remove the exact injected key, not its stale object reference.
        Assert.Equal(1, native.Rows.RemoveAll(row => row is DRILLING_DAILY_REPORT report && report.DDR_ID == orphan.DDR_ID && report.DRILLING_OPERATION_ID == orphan.DRILLING_OPERATION_ID));
        Assert.Equal(before, JsonSerializer.Serialize(native.Rows));
        var view = await db.RoleClaims.SingleAsync(row => row.RoleId == "execution-role" && row.ClaimValue == "Drilling.ViewDailyReports");
        db.Remove(view); await db.SaveChangesAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await http.PostAsJsonAsync(route, original)).StatusCode);
        view.Id = 0; db.RoleClaims.Add(view); await db.SaveChangesAsync();
        await f.Select("investor", "InvestmentReviewer");
        http.DefaultRequestHeaders.Remove("X-Test-Actor"); http.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");
        http.DefaultRequestHeaders.Add("X-Test-Actor", "investor"); http.DefaultRequestHeaders.Add("X-Test-Assigned-Role", "InvestmentReviewer");
        Assert.Equal(HttpStatusCode.Forbidden, (await http.PostAsJsonAsync(route, original)).StatusCode);
        await f.Select("foreign-reviewer", "ConfiguredReviewRole", new("tenant", "foreign"));
        http.DefaultRequestHeaders.Remove("X-Test-Actor"); http.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");
        http.DefaultRequestHeaders.Add("X-Test-Actor", "foreign-reviewer"); http.DefaultRequestHeaders.Add("X-Test-Assigned-Role", "ConfiguredReviewRole");
        var foreign = await http.PostAsJsonAsync(route, original); Assert.Equal(HttpStatusCode.Forbidden, foreign.StatusCode);
        Assert.DoesNotContain(recorded.Provenance, await foreign.Content.ReadAsStringAsync()); Assert.DoesNotContain(recorded.ReportId(), await foreign.Content.ReadAsStringAsync());
        http.DefaultRequestHeaders.Remove("X-Test-Auth");
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(route, original)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.PostAsJsonAsync(route + "/confirm", new DrillingDailyReportConflictAcknowledgement(original, proof.Revision))).StatusCode);
        Assert.Equal(before, JsonSerializer.Serialize(native.Rows)); Assert.Equal(writes, native.Writes);
    }
}
#endif
