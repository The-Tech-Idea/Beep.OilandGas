using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Repository.Models;
#if RECORDED_APPROVAL_HTTP_FOCUSED
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;

namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CurrentFieldLifecycleGateTests
{
    private sealed record HseRaceInput(ConnectionProperties Core, ConnectionProperties Lifecycle,
        CompanyHseClosureCommand Original, CompanyHseClosureReceipt? Approval, string Scenario = "same-original");
    private sealed record HseRaceReport(int ProcessId, string Stage, string Status, string? Error, IncidentClosedReceipt? Receipt);

    private static async Task<IncidentClosedReceipt> RaceInstalledHseClosureAsync(CompanyPPDMRowAccessTests.Fixture f,
        IDataSource core, IDataSource lifecycle, CompanyHseClosureCommand original, CompanyHseClosureReceipt approval,
        ConnectionProperties coreTarget, ConnectionProperties lifecycleTarget, PPDMCompanyCorrectiveActionCommands commands,
        Func<Task> confirmLifecycle, string directory)
    {
        PPDMDataSourceTransaction.RequireIdle(core); PPDMDataSourceTransaction.RequireIdle(lifecycle);
        Assert.Empty((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
        await WriteHseWorkerInputAsync(f, directory, new(coreTarget, lifecycleTarget, original, approval));
        var a = RunHseRaceWorkerAsync("a", directory); var b = RunHseRaceWorkerAsync("b", directory);
        // Await both workers even when one fails; never leave an active writer behind after a failed assertion.
        var workers = await Task.WhenAll(a, b);
        Assert.NotEqual(workers[0].ProcessId, workers[1].ProcessId);
        Assert.All(workers, report => Assert.True(report.Status is "confirmed" or "contention", report.Error));
        Assert.Contains(workers, report => report.Status == "confirmed");
        Assert.Single((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
        var statuses = (await core.GetEntityAsync(nameof(HSE_INCIDENT_RESPONSE), [new() { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = "local" }])).Cast<object>().ToArray();
        Assert.Single(statuses, row => (string?)row.GetType().GetProperty("RESPONSE_RESULT")!.GetValue(row) == "CLOSED");
        var confirmed = workers.First(report => report.Receipt is not null).Receipt!; confirmed.Require();
        foreach (var report in workers.Where(report => report.Receipt is not null))
            Assert.Equal(JsonSerializer.Serialize(confirmed), JsonSerializer.Serialize(report.Receipt));
        var beforeCore = HseRaceHash(coreTarget.Database); var beforeLifecycle = HseRaceHash(lifecycleTarget.Database);
        var recovered = await commands.RecordIncidentClosedAsync(original, approval, confirmLifecycle); recovered.Require();
        Assert.Equal(JsonSerializer.Serialize(confirmed), JsonSerializer.Serialize(recovered));
        Assert.Equal(beforeCore, HseRaceHash(coreTarget.Database)); Assert.Equal(beforeLifecycle, HseRaceHash(lifecycleTarget.Database));
        await File.WriteAllTextAsync(Path.Combine(directory, "hse-race-observations.json"), JsonSerializer.Serialize(new
        {
            workers, BothEnteredBeforeMutation = File.Exists(Path.Combine(directory, "a.ready")) && File.Exists(Path.Combine(directory, "b.ready")),
            NativeReceiptCount = 1, ClosedStatusCount = 1, NativeHashBeforeReconcile = beforeCore, NativeHashAfterReconcile = HseRaceHash(coreTarget.Database),
            LifecycleHashBeforeReconcile = beforeLifecycle, LifecycleHashAfterReconcile = HseRaceHash(lifecycleTarget.Database)
        }));
        return recovered;
    }
    private static async Task<IncidentClosedReceipt> RaceInstalledFinalApprovalsAsync(CompanyPPDMRowAccessTests.Fixture f,
        IDataSource core, IDataSource lifecycle, CompanyHseClosureCommand original,
        ConnectionProperties coreTarget, ConnectionProperties lifecycleTarget, string directory, bool differentAccounts)
    {
        PPDMDataSourceTransaction.RequireIdle(core); PPDMDataSourceTransaction.RequireIdle(lifecycle);
        Assert.Empty((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
        if (differentAccounts) await ArrangeOtherHseApproverAsync(f);
        var approvalsBefore = (await lifecycle.GetEntityAsync(nameof(PROCESS_APPROVAL), [])).Cast<object>().Count();
        await WriteHseWorkerInputAsync(f, directory, new(coreTarget, lifecycleTarget, original, null, differentAccounts ? "competing-account-approvals" : "competing-approvals"));
        var a = RunHseRaceWorkerAsync("approve-a", directory); var b = RunHseRaceWorkerAsync("approve-b", directory);
        var reports = await Task.WhenAll(a, b); Assert.NotEqual(reports[0].ProcessId, reports[1].ProcessId);
        Assert.All(reports, report => Assert.True(report.Status is "confirmed" or "contention" or "competing-original", report.Error));
        var winner = Assert.Single(reports, report => report.Status == "confirmed"); Assert.NotNull(winner.Receipt); winner.Receipt.Require();
        Assert.All(reports.Where(report => report.Status != "confirmed"), report => Assert.Null(report.Receipt));
        var originals = new[] { "approve-a", "approve-b" }.Select(stage => JsonSerializer.Deserialize<CompanyHseClosureCommand>(File.ReadAllText(Path.Combine(directory, stage + ".original.json")))!).ToArray();
        Assert.NotEqual(originals[0].IntentId, originals[1].IntentId);
        if (differentAccounts) Assert.NotEqual(originals[0].Review.Context.ActorUserId, originals[1].Review.Context.ActorUserId);
        Assert.Single(originals, candidate => candidate.IntentId == winner.Receipt.Original.IntentId);
        Assert.Equal(approvalsBefore + 1, (await lifecycle.GetEntityAsync(nameof(PROCESS_APPROVAL), [])).Cast<object>().Count());
        Assert.Single((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
        var raceCore = HseRaceHash(coreTarget.Database); var raceLifecycle = HseRaceHash(lifecycleTarget.Database);
        HseRaceReport? winnerRecovery = null;
        if (differentAccounts)
        {
            // Recover under the actual winner, regardless of which account won. Keep the live registry image unchanged.
            await File.WriteAllTextAsync(Path.Combine(directory, "hse-race-input.json"), JsonSerializer.Serialize(new HseRaceInput(coreTarget, lifecycleTarget, winner.Receipt.Original, winner.Receipt.Lifecycle, "competing-account-approvals")));
            winnerRecovery = await RunHseRaceWorkerAsync("recover", directory); Assert.Equal("confirmed", winnerRecovery.Status);
            Assert.Equal(JsonSerializer.Serialize(winner.Receipt), JsonSerializer.Serialize(winnerRecovery.Receipt));
            Assert.Equal(raceCore, HseRaceHash(coreTarget.Database)); Assert.Equal(raceLifecycle, HseRaceHash(lifecycleTarget.Database));
        }
        await File.WriteAllTextAsync(Path.Combine(directory, "hse-competing-approval-observations.json"), JsonSerializer.Serialize(new
        {
            reports, winnerRecovery, OriginalIds = originals.Select(candidate => candidate.IntentId).ToArray(), Actors = originals.Select(candidate => candidate.Review.Context.ActorUserId).ToArray(), Winner = winner.Receipt.Original.IntentId,
            BothBeforeMutation = File.Exists(Path.Combine(directory, "approve-a.ready")) && File.Exists(Path.Combine(directory, "approve-b.ready")),
            ApprovalsBefore = approvalsBefore, ApprovalsAfter = (await lifecycle.GetEntityAsync(nameof(PROCESS_APPROVAL), [])).Cast<object>().Count(),
            NativeHashAfterRace = raceCore, LifecycleHashAfterRace = raceLifecycle, NativeHashAfterWinnerRecovery = HseRaceHash(coreTarget.Database), LifecycleHashAfterWinnerRecovery = HseRaceHash(lifecycleTarget.Database)
        }));
        return winner.Receipt;
    }
    private static async Task WriteHseWorkerInputAsync(CompanyPPDMRowAccessTests.Fixture f, string directory, HseRaceInput input)
    {
        await File.WriteAllTextAsync(Path.Combine(directory, "hse-race-input.json"), JsonSerializer.Serialize(input));
        // Copy only the existing controlled identity/registry fixture; native schemas still use Beep Migration.
        using var snapshot = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "foundation-snapshot.db") }.ConnectionString);
        snapshot.Open(); ((SqliteConnection)f.Db.Database.GetDbConnection()).BackupDatabase(snapshot);
    }
    private static async Task<IncidentClosedReceipt> RevokeInstalledHseClosureAuthorityAsync(CompanyPPDMRowAccessTests.Fixture f,
        IDataSource core, IDataSource lifecycle, CompanyHseClosureCommand original, CompanyHseClosureReceipt approval,
        ConnectionProperties coreTarget, ConnectionProperties lifecycleTarget, PPDMCompanyCorrectiveActionCommands commands,
        Func<Task> confirmLifecycle, string directory)
    {
        PPDMDataSourceTransaction.RequireIdle(core); PPDMDataSourceTransaction.RequireIdle(lifecycle);
        Assert.Empty((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
        await WriteHseWorkerInputAsync(f, directory, new(coreTarget, lifecycleTarget, original, approval, "revoke-authority"));
        var beforeCore = HseRaceHash(coreTarget.Database); var beforeLifecycle = HseRaceHash(lifecycleTarget.Database);
        var pending = RunHseRaceWorkerAsync("a", directory); var revoking = RunHseRaceWorkerAsync("revoke", directory);
        var reports = await Task.WhenAll(pending, revoking);
        Assert.NotEqual(reports[0].ProcessId, reports[1].ProcessId);
        Assert.Equal("authority-refused", reports[0].Status); Assert.Null(reports[0].Receipt);
        Assert.Equal("revoked", reports[1].Status);
        Assert.Empty((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
        var refusedCore = HseRaceHash(coreTarget.Database); var refusedLifecycle = HseRaceHash(lifecycleTarget.Database);
        Assert.Equal(beforeCore, refusedCore); Assert.Equal(beforeLifecycle, refusedLifecycle);
        var restored = await RunHseRaceWorkerAsync("restore", directory); Assert.Equal("restored", restored.Status);
        var confirmed = await RunHseRaceWorkerAsync("recover", directory); Assert.Equal("confirmed", confirmed.Status); Assert.NotNull(confirmed.Receipt);
        var afterCore = HseRaceHash(coreTarget.Database); var afterLifecycle = HseRaceHash(lifecycleTarget.Database);
        var recovered = await commands.RecordIncidentClosedAsync(original, approval, confirmLifecycle); recovered.Require();
        Assert.Equal(JsonSerializer.Serialize(confirmed.Receipt), JsonSerializer.Serialize(recovered));
        Assert.Equal(afterCore, HseRaceHash(coreTarget.Database)); Assert.Equal(afterLifecycle, HseRaceHash(lifecycleTarget.Database));
        Assert.Single((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
        await File.WriteAllTextAsync(Path.Combine(directory, "hse-authority-observations.json"), JsonSerializer.Serialize(new
        {
            reports, restored, confirmed, AuthorityChangedAfterApprovalConfirmation = File.Exists(Path.Combine(directory, "a.ready")),
            NativeHashBeforeRefusal = beforeCore, NativeHashAfterRefusal = refusedCore, LifecycleHashBeforeRefusal = beforeLifecycle, LifecycleHashAfterRefusal = refusedLifecycle,
            NativeHashBeforeReconcile = afterCore, NativeHashAfterReconcile = HseRaceHash(coreTarget.Database),
            LifecycleHashBeforeReconcile = afterLifecycle, LifecycleHashAfterReconcile = HseRaceHash(lifecycleTarget.Database)
        }));
        return recovered;
    }
    private static async Task ArrangeOtherHseApproverAsync(CompanyPPDMRowAccessTests.Fixture f)
    {
        // Arrange another fully authorized account in the controlled identity fixture before its shared image is opened.
        f.Db.Users.Add(new OilGasUser { Id = "other-approver", UserName = "other-approver", IsActive = true });
        foreach (var role in new[] { "admin", "provider-hse" })
            f.Db.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<string> { UserId = "other-approver", RoleId = role });
        await f.Db.SaveChangesAsync(); await f.Select("company", "other-approver");
        f.Request.User = new(new System.Security.Claims.ClaimsIdentity([new("party_id", "other-approver")], "test"));
        await f.Grant("FIELD", "A", "DELETE", inherit: true);
        var otherProfile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var otherContext = (await otherProfile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        Assert.Equal("other-approver", otherContext.ActorUserId);
        await otherProfile.SetCurrentFieldSelectionAsync(new() { FieldId = "A", Context = otherContext });
        await f.Select("company"); f.Request.User = new(new System.Security.Claims.ClaimsIdentity([new("party_id", "actor")], "test")); f.Http.HttpContext = f.Request;
    }
    private static async Task<IncidentClosedReceipt> IsolateInstalledHseClosureOriginalAsync(CompanyPPDMRowAccessTests.Fixture f,
        IDataSource core, IDataSource lifecycle, CompanyHseClosureCommand original, CompanyHseClosureReceipt approval,
        ConnectionProperties coreTarget, ConnectionProperties lifecycleTarget, string directory)
    {
        PPDMDataSourceTransaction.RequireIdle(core); PPDMDataSourceTransaction.RequireIdle(lifecycle);
        Assert.Empty((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
        await ArrangeOtherHseApproverAsync(f);
        await WriteHseWorkerInputAsync(f, directory, new(coreTarget, lifecycleTarget, original, approval, "original-isolation"));
        var beforeCore = HseRaceHash(coreTarget.Database); var beforeLifecycle = HseRaceHash(lifecycleTarget.Database);
        var foreign = await RunHseRaceWorkerAsync("foreign", directory);
        var conflict = await RunHseRaceWorkerAsync("conflict", directory);
        Assert.Equal("foreign-refused", foreign.Status); Assert.Equal("conflict-refused", conflict.Status);
        Assert.Null(foreign.Receipt); Assert.Null(conflict.Receipt); Assert.NotEqual(foreign.ProcessId, conflict.ProcessId);
        var refusedCore = HseRaceHash(coreTarget.Database); var refusedLifecycle = HseRaceHash(lifecycleTarget.Database);
        Assert.Equal(beforeCore, refusedCore); Assert.Equal(beforeLifecycle, refusedLifecycle);
        Assert.Empty((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
        var recovery = await RunHseRaceWorkerAsync("recover", directory); Assert.Equal("confirmed", recovery.Status); Assert.NotNull(recovery.Receipt);
        await File.WriteAllTextAsync(Path.Combine(directory, "hse-isolation-observations.json"), JsonSerializer.Serialize(new
        {
            foreign, conflict, recovery, NativeHashBeforeRefusal = beforeCore, NativeHashAfterRefusal = refusedCore,
            LifecycleHashBeforeRefusal = beforeLifecycle, LifecycleHashAfterRefusal = refusedLifecycle,
            NativeHashAfterRecovery = HseRaceHash(coreTarget.Database), LifecycleHashAfterRecovery = HseRaceHash(lifecycleTarget.Database)
        }));
        return recovery.Receipt;
    }
    private static async Task WaitForHseFileAsync(string directory, string name)
    {
        var watch = Stopwatch.StartNew();
        while (!File.Exists(Path.Combine(directory, name)))
        {
            if (watch.Elapsed > TimeSpan.FromSeconds(90)) throw new TimeoutException("The selected HSE barrier was not reached: " + name);
            await Task.Delay(50);
        }
    }
    private static async Task RunHseAuthorityRegistryStageAsync(string stage, string directory)
    {
        if (stage == "revoke") await WaitForHseFileAsync(directory, "a.ready");
        var options = new DbContextOptionsBuilder<CompanyPPDMRowAccessTests.Context>().UseSqlite(
            new SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "foundation-snapshot.db"), Mode = SqliteOpenMode.ReadWrite }.ConnectionString).Options;
        await using var registry = new CompanyPPDMRowAccessTests.Context(options);
        var permissions = await registry.RoleClaims.Where(c => c.RoleId == "admin" && c.ClaimType == "permission" && c.ClaimValue == PermissionConstants.HSE.ManageIncidents).ToArrayAsync();
        if (stage == "revoke")
        {
            Assert.NotEmpty(permissions); registry.RoleClaims.RemoveRange(permissions); await registry.SaveChangesAsync();
            await File.WriteAllTextAsync(Path.Combine(directory, "authority.revoked"), DateTimeOffset.UtcNow.ToString("O"));
        }
        else
        {
            Assert.Empty(permissions);
            registry.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = PermissionConstants.HSE.ManageIncidents });
            await registry.SaveChangesAsync();
        }
        await File.WriteAllTextAsync(Path.Combine(directory, stage + ".report.json"), JsonSerializer.Serialize(new HseRaceReport(Environment.ProcessId, stage, stage == "revoke" ? "revoked" : "restored", null, null)));
    }
    private static string HseRaceHash(string path)
    {
        using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(file));
    }
    private static async Task<HseRaceReport> RunHseRaceWorkerAsync(string stage, string directory)
    {
        var worker = Path.Combine(AppContext.BaseDirectory, "Beep.OilandGas.HseClosureProviderWorker.dll");
        Assert.True(File.Exists(worker), "Build tools/HseClosureProviderWorker and copy its DLL to the focused output before this selected gate.");
        var tests = typeof(CurrentFieldLifecycleGateTests).Assembly.Location;
        var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        foreach (var argument in new[] { "exec", "--runtimeconfig", Path.ChangeExtension(tests, ".runtimeconfig.json"), "--depsfile", Path.ChangeExtension(tests, ".deps.json"), worker, stage, directory }) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("The selected HSE closure worker could not start.");
        var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
        using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        try { await process.WaitForExitAsync(deadline.Token); }
        catch (OperationCanceledException) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); throw; }
        await File.WriteAllTextAsync(Path.Combine(directory, stage + ".stdout.log"), await stdout);
        await File.WriteAllTextAsync(Path.Combine(directory, stage + ".stderr.log"), await stderr);
        Assert.True(process.ExitCode == 0, $"HSE {stage} worker exited {process.ExitCode}: {await stderr} {await stdout}");
        return JsonSerializer.Deserialize<HseRaceReport>(await File.ReadAllTextAsync(Path.Combine(directory, stage + ".report.json")))!;
    }

    // A separate executable invokes this helper; it is not another test selection or a production endpoint.
    public static async Task RunHseClosureRaceStageAsync(string stage, string directory)
    {
        if (stage is not ("a" or "b" or "recover" or "revoke" or "restore" or "foreign" or "conflict" or "approve-a" or "approve-b")) throw new ArgumentException("Unknown HSE closure worker stage.", nameof(stage));
        if (stage is "revoke" or "restore") { await RunHseAuthorityRegistryStageAsync(stage, directory); return; }
        var input = JsonSerializer.Deserialize<HseRaceInput>(await File.ReadAllTextAsync(Path.Combine(directory, "hse-race-input.json")))!;
        input.Original.Require(); input.Approval?.Require(input.Original);
        await new PermitProviderQualificationTests().WithInstalledProviderAsync(async (editor, core) =>
        {
            using var f = await CompanyIncidentCommandTests.Ready(await CompanyPPDMRowAccessTests.Fixture.Create(input.Core));
            if (input.Scenario is "revoke-authority" or "original-isolation" or "competing-approvals" or "competing-account-approvals") await f.UseExistingRegistryAsync(Path.Combine(directory, "foundation-snapshot.db"), stage == "foreign" || input.Scenario == "competing-account-approvals" && stage == "approve-b" ? "other-approver" : input.Original.Review.Context.ActorUserId);
            else
            {
                using var snapshot = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "foundation-snapshot.db"), Mode = SqliteOpenMode.ReadOnly }.ConnectionString);
                snapshot.Open(); snapshot.BackupDatabase((SqliteConnection)f.Db.Database.GetDbConnection()); f.Db.ChangeTracker.Clear();
                await f.Select("company");
            }
            var nativeMetadata = HseIncidentNativeMetadata.AddTo(new());
            foreach (var type in new[] { typeof(HSE_CA_VERIFICATION), typeof(HSE_INCIDENT_CLOSURE), typeof(HSE_INCIDENT_RCA_INTENT) })
            {
                f.Metadata.Setup(x => x.GetTableMetadataAsync(type.Name)).ReturnsAsync(nativeMetadata[type.Name]);
                f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync([nativeMetadata[type.Name].PrimaryKeyColumn]);
            }
            foreach (var type in new[] { typeof(PROCESS_DEFINITION), typeof(PROCESS_INSTANCE), typeof(PROCESS_STEP_INSTANCE), typeof(PROCESS_HISTORY), typeof(PROCESS_APPROVAL), typeof(CROSS_PERSONA_TASK) })
            {
                var key = type == typeof(CROSS_PERSONA_TASK) ? "CROSS_TASK_ID" : type.Name + "_ID";
                f.Metadata.Setup(x => x.GetTableMetadataAsync(type.Name)).ReturnsAsync(new PPDMTableMetadata { TableName = type.Name, EntityTypeName = type.FullName!, PrimaryKeyColumn = key });
                f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync([key]);
            }
            f.Editor.Setup(x => x.GetDataSource(core.DatasourceName)).Returns(core); f.Editor.Setup(x => x.OpenDataSource(core.DatasourceName)).Returns(ConnectionState.Open);
            var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
            var current = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
            Assert.Equal(input.Original.Review.Context.Database, current.Database); Assert.Equal(input.Original.Review.Context.Company, current.Company);
            editor.ConfigEditor.DataConnections.Add(input.Lifecycle);
            var lifecycle = editor.GetDataSource(input.Lifecycle.ConnectionName); Assert.NotNull(lifecycle); Assert.Equal(ConnectionState.Open, lifecycle.Openconnection());
            try
            {
                var columns = new CommonColumnHandler();
                var incidents = new PPDMCompanyIncidentCommands(f.Gate, profile, editor, columns, f.Defaults.Object, f.Metadata.Object, f.Policy);
                var commands = new PPDMCompanyCorrectiveActionCommands(f.Gate, profile, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), editor, columns, f.Defaults.Object, f.Metadata.Object, f.Policy);
                var personas = new Mock<IRolePersonaReader>(); personas.Setup(x => x.GetActivePersonasAsync("HSEOfficer")).ReturnsAsync(["HSE"]);
                var process = new PPDMProcessService(editor, columns, f.Defaults.Object, f.Metadata.Object, lifecycle.DatasourceName, NullLogger<PPDMProcessService>.Instance, personaReader: personas.Object);
                async Task Role(string role)
                {
                    Assert.Equal("HSEOfficer", role); var access = await f.Gate.RefreshCompanyAsync();
                    if (!access.Roles.Contains(role) || !access.Permissions.Contains(PermissionConstants.Workflow.Approve)) throw new UnauthorizedAccessException("Current HSE closure role is required.");
                }
                async Task Cause(CompanyHseClosureCommand _)
                {
                    PPDMDataSourceTransaction.RequireIdle(core);
                    var expected = input.Original.Review.RecordedCause; Assert.NotNull(expected);
                    Assert.Equal(JsonSerializer.Serialize(expected), JsonSerializer.Serialize(Assert.Single(await incidents.ReadSharedRcaAsync(current, "local"))));
                }
                if (stage is "approve-a" or "approve-b")
                {
                    Assert.True(input.Scenario is "competing-approvals" or "competing-account-approvals"); Assert.Null(input.Approval);
                    var competing = stage == "approve-a" ? input.Original : input.Original with { IntentId = Guid.NewGuid() };
                    if (input.Scenario == "competing-account-approvals")
                    {
                        var access = await f.Gate.RefreshCompanyAsync(); Assert.Contains("HSEOfficer", access.Roles);
                        Assert.Contains(PermissionConstants.Workflow.Approve, access.Permissions); Assert.Contains(PermissionConstants.HSE.ManageIncidents, access.Permissions);
                        var plan = input.Original.Review.Evidence.Actions.PlanId;
                        var evidence = await commands.ReviewClosureAsync(current, "local", plan, current.ActorUserId);
                        var nativeRevision = await commands.ReviewNativeClosureRevisionAsync(current, "local", plan);
                        var ownReview = await process.ReviewClosureStageAsync(evidence, input.Original.Review.LifecycleDatabase, nativeRevision, Role);
                        Assert.Equal(current.ActorUserId, ownReview.Context.ActorUserId);
                        competing = new(Guid.NewGuid(), ownReview, input.Original.Reason);
                    }
                    competing.Require();
                    await File.WriteAllTextAsync(Path.Combine(directory, stage + ".original.json"), JsonSerializer.Serialize(competing));
                    var firstSource = true;
                    async Task PendingSource()
                    {
                        await Cause(competing);
                        if (!firstSource) return; firstSource = false;
                        PPDMDataSourceTransaction.RequireIdle(core); PPDMDataSourceTransaction.RequireIdle(lifecycle);
                        Assert.Empty((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
                        var pending = await process.GetProcessInstanceAsync(competing.Review.InstanceId); Assert.NotNull(pending);
                        Assert.Equal("INC_CLOSURE", pending.CurrentStepId); Assert.NotEqual(Beep.OilandGas.Models.Processes.ProcessStatus.COMPLETED, pending.Status);
                        await File.WriteAllTextAsync(Path.Combine(directory, stage + ".ready"), competing.IntentId.ToString());
                        await WaitForHseFileAsync(directory, "approve-a.ready"); await WaitForHseFileAsync(directory, "approve-b.ready");
                    }
                    IncidentClosedReceipt? raceResult = null; string raceStatus = "confirmed"; string? raceError = null;
                    try
                    {
                        var approved = await process.CompleteClosureStageAsync(competing, Role, PendingSource); approved.Require(competing);
                        async Task ConfirmApproved()
                        {
                            PPDMDataSourceTransaction.RequireIdle(core);
                            var recorded = await process.CompleteClosureStageAsync(competing, Role, () => Cause(competing));
                            Assert.Equal(JsonSerializer.Serialize(approved), JsonSerializer.Serialize(recorded));
                        }
                        raceResult = await commands.RecordIncidentClosedAsync(competing, approved, ConfirmApproved); raceResult.Require();
                    }
                    catch (InvalidOperationException error) when (error.Message == "This stage already has another recorded original.") { raceStatus = "competing-original"; raceError = error.ToString(); }
                    catch (Exception error) when (HseProviderContention(error)) { raceStatus = "contention"; raceError = error.ToString(); }
                    PPDMDataSourceTransaction.RequireIdle(core); PPDMDataSourceTransaction.RequireIdle(lifecycle);
                    await File.WriteAllTextAsync(Path.Combine(directory, stage + ".report.json"), JsonSerializer.Serialize(new HseRaceReport(Environment.ProcessId, stage, raceStatus, raceError, raceResult)));
                    return;
                }
                var priorApproval = input.Approval ?? throw new InvalidOperationException("The selected native worker requires a persisted lifecycle approval.");
                if (stage is "foreign" or "conflict")
                {
                    var access = await f.Gate.RefreshCompanyAsync();
                    Assert.Contains("HSEOfficer", access.Roles); Assert.Contains(PermissionConstants.HSE.ManageIncidents, access.Permissions);
                    Assert.Contains(PermissionConstants.Workflow.Approve, access.Permissions); Assert.Equal("A", current.FieldId);
                    Exception refused;
                    if (stage == "foreign")
                    {
                        Assert.Equal("other-approver", current.ActorUserId); Assert.NotEqual(input.Original.Review.Context.ActorUserId, current.ActorUserId);
                        var callback = false;
                        refused = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => commands.RecordIncidentClosedAsync(input.Original, priorApproval, () => { callback = true; return Task.CompletedTask; }));
                        Assert.False(callback); Assert.Equal("The native closure original belongs to another scope.", refused.Message);
                    }
                    else
                    {
                        var competing = input.Original with { IntentId = Guid.NewGuid() }; competing.Require();
                        refused = await Assert.ThrowsAsync<InvalidOperationException>(() => process.CompleteClosureStageAsync(competing, Role, () => Cause(competing)));
                        Assert.Equal("This stage already has another recorded original.", refused.Message);
                    }
                    PPDMDataSourceTransaction.RequireIdle(core); PPDMDataSourceTransaction.RequireIdle(lifecycle);
                    await File.WriteAllTextAsync(Path.Combine(directory, stage + ".report.json"), JsonSerializer.Serialize(new HseRaceReport(Environment.ProcessId, stage, stage + "-refused", refused.ToString(), null)));
                    return;
                }
                var first = true;
                async Task Confirm()
                {
                    PPDMDataSourceTransaction.RequireIdle(core);
                    var recorded = await process.RecoverClosureStageAsync(current, input.Original.Review.LifecycleDatabase, "local", Role, Cause); Assert.NotNull(recorded);
                    Assert.Equal(JsonSerializer.Serialize(input.Original), JsonSerializer.Serialize(recorded.Original));
                    Assert.Equal(JsonSerializer.Serialize(priorApproval), JsonSerializer.Serialize(recorded.Receipt));
                    if (!first || stage == "recover") return; first = false;
                    Assert.Empty((await core.GetEntityAsync(nameof(HSE_INCIDENT_CLOSURE), [])).Cast<object>());
                    await File.WriteAllTextAsync(Path.Combine(directory, stage + ".ready"), DateTimeOffset.UtcNow.ToString("O"));
                    if (input.Scenario == "revoke-authority") { await WaitForHseFileAsync(directory, "authority.revoked"); return; }
                    var timer = Stopwatch.StartNew();
                    while (!File.Exists(Path.Combine(directory, "a.ready")) || !File.Exists(Path.Combine(directory, "b.ready")))
                    {
                        if (timer.Elapsed > TimeSpan.FromSeconds(90)) throw new TimeoutException("Both HSE workers must reach lifecycle confirmation before native mutation.");
                        await Task.Delay(50);
                    }
                }
                IncidentClosedReceipt? result = null; string? errorText = null; var status = "confirmed";
                try { result = await commands.RecordIncidentClosedAsync(input.Original, priorApproval, Confirm); result.Require(); }
                catch (RefusalException error) when (input.Scenario == "revoke-authority" && stage == "a" && error.Kind == RefusalKind.Forbidden) { status = "authority-refused"; errorText = error.ToString(); }
                catch (Exception error) when (input.Scenario == "same-original" && HseProviderContention(error)) { status = "contention"; errorText = error.ToString(); }
                await File.WriteAllTextAsync(Path.Combine(directory, stage + ".report.json"), JsonSerializer.Serialize(new HseRaceReport(Environment.ProcessId, stage, status, errorText, result)));
                PPDMDataSourceTransaction.RequireIdle(core); PPDMDataSourceTransaction.RequireIdle(lifecycle);
            }
            finally { lifecycle.Closeconnection(); lifecycle.Dispose(); }
        }, directory, requireExisting: true, connectionName: input.Core.ConnectionName, configuredConnection: input.Core);
    }
    private static bool HseProviderContention(Exception error)
    {
        for (Exception? current = error; current is not null; current = current.InnerException)
            if (current.Message.Contains("database is locked", StringComparison.OrdinalIgnoreCase) || current.Message.Contains("database table is locked", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}
#endif
