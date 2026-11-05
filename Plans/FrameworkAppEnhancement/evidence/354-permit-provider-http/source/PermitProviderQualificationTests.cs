#if RECORDED_APPROVAL_HTTP_FOCUSED
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PermitsAndApplications.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

/// <summary>Plan 354 physical schema gate. Does not qualify API/OIDC or company authorization.</summary>
public sealed class PermitProviderQualificationTests
{
    [Fact]
    public Task SetupInstalledSqliteMigratesPermitApplicationLedger() => WithInstalledLedgerAsync((_, _) => Task.CompletedTask);

    internal async Task WithInstalledLedgerAsync(Func<IDMEEditor, IDataSource, Task> qualify, string? retainedDirectory = null, bool installSchema = true)
    {
        var directory = retainedDirectory ?? Path.Combine(Path.GetTempPath(), "oilgas-permit-provider-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "permits.db");
        if (!installSchema) Assert.True(File.Exists(path), "Recovery must open an existing physical permit database.");
        var config = new Mock<IConfigEditor>();
        var driver = ConnectionHelper.CreateSQLiteConfig();
        var classes = new List<AssemblyClassDefinition>();
        var connection = new ConnectionProperties { ConnectionName = "permit-provider", DatabaseType = DataSourceType.SqlLite,
            ConnectionString = "Data Source=" + path, Database = path, GuidID = Guid.NewGuid().ToString() };
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        config.SetupProperty(x => x.DataConnections, new List<ConnectionProperties> { connection });
        config.SetupGet(x => x.DataTypesMap).Returns(TheTechIdea.Beep.Helpers.DataTypesHelpers.DatabaseTypeMappingRepository.GenerateSQLiteDataTypesMapping());
        var storage = new TheTechIdea.Beep.Services.Persistence.FileMigrationExecutionStorage(directory, new TheTechIdea.Beep.JsonLoaderService.JsonLoader());
        config.As<IMigrationExecutionStorageProvider>().Setup(x => x.CaptureMigrationExecutionStorage()).Returns(storage);
        var log = new Mock<IDMLogger>();
        var messages = new List<string>();
        log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
        var queries = new TheTechIdea.Beep.ConfigUtil.Managers.QueryManager(log.Object, Mock.Of<IJsonLoader>(), string.Empty);
        config.Setup(x => x.GetSql(It.IsAny<Sqlcommandtype>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<QuerySqlRepo>>(), It.IsAny<DataSourceType>()))
            .Returns((Sqlcommandtype kind, string table, string schema, string filter, List<QuerySqlRepo> catalog, DataSourceType provider) => queries.GetSql(kind, table, schema ?? string.Empty, filter, provider));
        var errors = new ErrorsInfo();
        var util = new TheTechIdea.Beep.Utils.Util(log.Object, errors, config.Object);
        using var handler = new AssemblyHandler(config.Object, errors, log.Object, util);
        using var editor = new DMEEditor(log.Object, util, errors, config.Object, handler);
        util.DME = editor;
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["SetupDrivers:Connectors:SQLiteDataSource:Package"] = "TheTechIdea.Beep.SqliteDatasourceCore",
            ["SetupDrivers:Connectors:SQLiteDataSource:Version"] = "1.0.59",
            ["SetupDrivers:Providers:SQLiteDataSource:Package"] = "Stub.System.Data.SQLite.Core.NetStandard",
            ["SetupDrivers:Providers:SQLiteDataSource:Version"] = "1.0.119"
        });
        var feed = Environment.GetEnvironmentVariable("OILGAS_TEST_PERMIT_DRIVER_FEED");
        if (!string.IsNullOrWhiteSpace(feed))
            settings.AddInMemoryCollection(new Dictionary<string, string?> { ["SetupDrivers:PackageSources:0"] = feed });
        var configuration = settings.Build();
        IDataSource? source = null;
        try
        {
            // Use the wizard's package installer, without manually scanning a connector assembly.
            var installer = new SetupDriverService(editor, configuration, NullLogger<SetupDriverService>.Instance);
            var authorityChecks = 0;
            var reviewed = Assert.Single(installer.Catalog());
            var installed = await installer.InstallAsync(driver.GuidID, reviewed, () => { authorityChecks++; return Task.CompletedTask; }, default);
            Assert.True(installed.ConnectorLoaded && installed.ProviderLoaded && installed.Loaded);
            Assert.True(authorityChecks >= 2);
            using (var physical = (IDbConnection)Activator.CreateInstance(handler.GetType(driver.DbConnectionType))!)
            { physical.ConnectionString = connection.ConnectionString; physical.Open(); }
            source = editor.GetDataSource(connection.ConnectionName);
            Assert.NotNull(source);
            Assert.Equal(ConnectionState.Open, source.Openconnection());
            if (installSchema)
            {
            var migration = new MigrationManager(editor, source) { ExecutionTargetIdentity = ModuleDatabaseTarget.Key(editor, connection) };
            Type[] types = [typeof(PERMIT_APPLICATION), typeof(PERMIT_STATUS_HISTORY), typeof(PERMIT_APPLICATION_COMMAND)];
            var plan = migration.BuildMigrationPlanForTypes(types, detectRelationships: false);
            Assert.Equal(3, plan.Operations.Count(x => x.Kind == MigrationPlanOperationKind.CreateEntity));
            migration.ApproveMigrationPlan(plan, "permit-provider-qualification");
            var applied = migration.ExecuteMigrationPlan(plan);
            Assert.True(applied.Success, applied.Message + " | " + errors.Message);
            foreach (var type in types)
            {
                Assert.True(source.CheckEntityExist(type.Name));
                var shape = source.GetEntityStructure(new EntityStructure { EntityName = type.Name, DatasourceEntityName = type.Name }, true);
                Assert.NotNull(shape);
                var key = Assert.Single(shape.Fields.Where(field => field.IsKey));
                Assert.Equal(type.Name + "_ID", key.FieldName, ignoreCase: true);
                foreach (var name in new[] { "OWNER_TENANT_ID", "OWNER_COMPANY_ID", "FIELD_ID" }.Where(name => type != typeof(PERMIT_STATUS_HISTORY)))
                    Assert.Single(shape.Fields, field => field.FieldName.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            var header = source.GetEntityStructure("PERMIT_APPLICATION", true);
            foreach (var name in new[] { "STATUS", "SUBMITTED_BY_USER_ID", "FACILITY_TYPE" })
                Assert.Single(header.Fields, field => field.FieldName.Equals(name, StringComparison.OrdinalIgnoreCase));
            var replay = migration.BuildMigrationPlanForTypes(types, detectRelationships: false);
            // Beep retains one explicit UpToDate operation per unchanged entity.
            Assert.Equal(types.Length, replay.Operations.Count);
            Assert.All(replay.Operations, operation => Assert.Equal(MigrationPlanOperationKind.UpToDate, operation.Kind));
            }
            await qualify(editor, source);
        }
        catch (Exception error)
        {
            throw new InvalidOperationException("Permit provider gate failed: " + string.Join(" | ", messages.TakeLast(12)), error);
        }
        finally
        {
            source?.Closeconnection();
            // Keep this isolated database/checkpoint directory as diagnostic evidence.
        }
    }

    [Fact]
    public Task InstalledPermitProviderRollsBackThenRecoversExactNativeSubmission() => WithInstalledLedgerAsync(async (editor, source) =>
    {
        var rowType = source.GetEntityType(nameof(PERMIT_APPLICATION));
        var underscoreType = DMTypeBuilder.GetOrCreateType(editor, "Beep.permit_provider", null, nameof(PERMIT_APPLICATION), source.GetEntityStructure(nameof(PERMIT_APPLICATION), true).Fields);
        Assert.NotEqual(rowType, underscoreType);
        var fixture = CreateNativeFixture(editor, source);
        fixture.InterruptReceipt = true;
        var service = fixture.Service;
        var seed = new PERMIT_APPLICATION { PERMIT_APPLICATION_ID = "physical-permit", OWNER_TENANT_ID = "tenant", OWNER_COMPANY_ID = "company",
            FIELD_ID = "field-a", APPLICANT_ID = "Recorded applicant", ACTIVE_IND = "Y" };
        new CommonColumnHandler().PrepareForInsert(seed, "author");
        Assert.Equal(Errors.Ok, source.InsertEntity(nameof(PERMIT_APPLICATION), seed).Flag);
        var review = await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);
        var original = new PermitApplicationCommand(Guid.NewGuid(), review, "SUBMIT", "Reviewed the recorded filing before submission");
        Assert.Null(await service.RecoverCommandAsync(original));
        var interruptedCommand = await Assert.ThrowsAnyAsync<Exception>(() => service.RecordCommandAsync(original));
        Assert.True(fixture.Interrupted, "The failure must occur after the header and history writes, at receipt preparation. Actual: " + interruptedCommand);
        PPDMDataSourceTransaction.RequireIdle(source);
        var rolledBack = await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);
        Assert.Equal(review, rolledBack);
        Assert.Null(await service.RecoverCommandAsync(original));
        fixture.InterruptReceipt = false;
        var saved = await service.RecordCommandAsync(original);
        saved.Require();
        Assert.Equal(original, saved.Original);
        Assert.Equal(Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Submitted, saved.ReadApplication().STATUS);
        fixture.AllowWrites = false;
        Assert.Equal(saved, await service.RecoverCommandAsync(original));
        Assert.Equal(saved, await service.RecordCommandAsync(original));
        var current = await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);
        Assert.Single(current.ReadHistory());
        Assert.Single(await fixture.ReceiptRepository.GetAsync([]));
        PPDMDataSourceTransaction.RequireIdle(source);
    });

    [Fact]
    public async Task InstalledPermitProviderRecoversUnacknowledgedSubmissionAcrossProcesses()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-permit-restart-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var commit = await RunWorkerAsync("commit", directory, 73);
        var originalBytes = await File.ReadAllBytesAsync(Path.Combine(directory, "original.json"));
        var databaseHash = SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(directory, "permits.db")));
        var recovered = await RunWorkerAsync("recover", directory, 0);
        Assert.Equal(databaseHash, SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(directory, "permits.db"))));
        var replayed = await RunWorkerAsync("replay", directory, 0);
        Assert.Equal(databaseHash, SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(directory, "permits.db"))));
        Assert.Equal(originalBytes, await File.ReadAllBytesAsync(Path.Combine(directory, "original.json")));
        Assert.Equal(3, new[] { commit.ProcessId, recovered.ProcessId, replayed.ProcessId }.Distinct().Count());
        Assert.DoesNotContain(Environment.ProcessId, new[] { commit.ProcessId, recovered.ProcessId, replayed.ProcessId });
        Assert.Null(commit.OutcomeHash);
        Assert.False(string.IsNullOrEmpty(recovered.OutcomeHash));
        Assert.Equal(recovered.OutcomeHash, replayed.OutcomeHash);
        Assert.Equal(commit.IntentId, recovered.IntentId);
        Assert.Equal(commit.IntentId, replayed.IntentId);
        Assert.All(new[] { recovered, replayed }, report =>
        {
            Assert.Equal(1, report.HistoryCount);
            Assert.Equal(1, report.ReceiptCount);
        });
    }

    private sealed record RestartReport(int ProcessId, Guid IntentId, string? OutcomeHash, int HistoryCount, int ReceiptCount);

    private static Task<RestartReport> RunWorkerAsync(string stage, string directory, int expectedExit) => RunWorkerAsync<RestartReport>(stage, directory, expectedExit);

    private static async Task<T> RunWorkerAsync<T>(string stage, string directory, int expectedExit) where T : class
    {
        var worker = Path.Combine(AppContext.BaseDirectory, "Beep.OilandGas.PermitProviderWorker.dll");
        Assert.True(File.Exists(worker), "Build tools/PermitProviderWorker and copy its DLL into the focused test output before this gate.");
        var tests = typeof(PermitProviderQualificationTests).Assembly.Location;
        var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        foreach (var argument in new[] { "exec", "--runtimeconfig", Path.ChangeExtension(tests, ".runtimeconfig.json"), "--depsfile", Path.ChangeExtension(tests, ".deps.json"), worker, stage, directory })
            start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("The permit qualification worker could not start.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        try { await process.WaitForExitAsync(deadline.Token); }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw;
        }
        await File.WriteAllTextAsync(Path.Combine(directory, stage + ".stdout.log"), await stdout);
        await File.WriteAllTextAsync(Path.Combine(directory, stage + ".stderr.log"), await stderr);
        Assert.True(process.ExitCode == expectedExit, $"Permit {stage} worker exited {process.ExitCode}: {await stderr} {await stdout}");
        return JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(Path.Combine(directory, stage + ".json")))
            ?? throw new InvalidOperationException("The permit worker report is absent.");
    }

    // Entry point for the separate qualification executable; this is not an API or production authorization path.
    public static Task RunRestartStageAsync(string stage, string directory)
    {
        if (stage is not ("commit" or "recover" or "replay")) throw new ArgumentException("Unknown permit qualification stage.", nameof(stage));
        return new PermitProviderQualificationTests().WithInstalledLedgerAsync(async (editor, source) =>
        {
            var fixture = CreateNativeFixture(editor, source);
            var service = fixture.Service;
            var originalPath = Path.Combine(directory, "original.json");
            PermitApplicationCommand original;
            if (stage == "commit")
            {
                var seed = new PERMIT_APPLICATION { PERMIT_APPLICATION_ID = "restart-permit", OWNER_TENANT_ID = "tenant", OWNER_COMPANY_ID = "company",
                    FIELD_ID = "field-a", APPLICANT_ID = "Restart qualification applicant", ACTIVE_IND = "Y" };
                new CommonColumnHandler().PrepareForInsert(seed, "author");
                Assert.Equal(Errors.Ok, source.InsertEntity(nameof(PERMIT_APPLICATION), seed).Flag);
                original = new(Guid.NewGuid(), await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID), "SUBMIT", "Retained before transport; response deliberately withheld");
                await File.WriteAllTextAsync(originalPath, JsonSerializer.Serialize(original));
                // Do not pass the returned result to the caller. Only fresh-process recovery can confirm it.
                await service.RecordCommandAsync(original);
                await File.WriteAllTextAsync(Path.Combine(directory, "commit.json"), JsonSerializer.Serialize(new RestartReport(Environment.ProcessId, original.IntentId, null, 0, 0)));
            }
            else
            {
                original = JsonSerializer.Deserialize<PermitApplicationCommand>(await File.ReadAllTextAsync(originalPath))
                    ?? throw new InvalidOperationException("The retained permit original is absent.");
                original.Require();
                fixture.AllowWrites = false;
                var saved = stage == "recover" ? await service.RecoverCommandAsync(original) : await service.RecordCommandAsync(original);
                Assert.NotNull(saved);
                saved.Require();
                Assert.Equal(original, saved.Original);
                Assert.Equal(Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Submitted, saved.ReadApplication().STATUS);
                var current = await service.ReviewSourceAsync(original.Review.ReadApplication().PERMIT_APPLICATION_ID);
                var receipts = (await fixture.ReceiptRepository.GetAsync([])).ToArray();
                Assert.Single(receipts);
                Assert.Equal(saved, Assert.IsType<PERMIT_APPLICATION_COMMAND>(receipts[0]).Restore(original.Review.Context));
                var report = new RestartReport(Environment.ProcessId, original.IntentId, ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(saved)), current.ReadHistory().Count, receipts.Length);
                await File.WriteAllTextAsync(Path.Combine(directory, stage + ".json"), JsonSerializer.Serialize(report));
            }
            PPDMDataSourceTransaction.RequireIdle(source);
        }, directory, installSchema: stage == "commit");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InstalledPermitProviderSerializesConcurrentOriginalsAcrossProcesses(bool sameOriginal)
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-permit-race-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        await RunWorkerAsync<RaceReport>(sameOriginal ? "race-prepare-same" : "race-prepare-conflict", directory, 0);
        var aBytes = await File.ReadAllBytesAsync(Path.Combine(directory, "a.original.json"));
        var bBytes = await File.ReadAllBytesAsync(Path.Combine(directory, "b.original.json"));
        var a = RunWorkerAsync<RaceReport>("race-a", directory, 0);
        var b = RunWorkerAsync<RaceReport>("race-b", directory, 0);
        RaceReport[] contenders;
        try
        {
            await WaitForFilesAsync(directory, "a.ready", "b.ready");
            await File.WriteAllTextAsync(Path.Combine(directory, "release"), "Both originals reached the pre-transaction barrier.");
            contenders = await Task.WhenAll(a, b);
        }
        catch (Exception primary)
        {
            await File.WriteAllTextAsync(Path.Combine(directory, "release"), "Release owned workers for diagnostic cleanup.");
            try { await Task.WhenAll(a, b); }
            catch (Exception cleanup) { throw new AggregateException("Permit race failed; both workers were awaited.", primary, cleanup); }
            throw;
        }
        var databaseHash = SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(directory, "permits.db")));
        var audit = await RunWorkerAsync<RaceReport>("race-audit", directory, 0);
        Assert.Equal(databaseHash, SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(directory, "permits.db"))));
        Assert.Equal(aBytes, await File.ReadAllBytesAsync(Path.Combine(directory, "a.original.json")));
        Assert.Equal(bBytes, await File.ReadAllBytesAsync(Path.Combine(directory, "b.original.json")));
        Assert.NotEqual(contenders[0].ProcessId, contenders[1].ProcessId);
        Assert.DoesNotContain(audit.ProcessId, contenders.Select(report => report.ProcessId));
        Assert.Equal(1, audit.HistoryCount);
        Assert.Equal(1, audit.ReceiptCount);
        var held = new[] { "a", "b" }.Select(role => JsonSerializer.Deserialize<OwnedWindow>(File.ReadAllText(Path.Combine(directory, role + ".owned.json")))!).OrderBy(window => window.StartTicks).ToArray();
        Assert.All(held, window => Assert.Equal(IsolationLevel.Serializable.ToString(), window.Isolation));
        Assert.True(held[0].ReleasedTicks <= held[1].StartTicks, "Owned serializable transaction windows overlapped across processes.");
        if (sameOriginal)
        {
            Assert.All(contenders, report => { Assert.Equal("recorded", report.Decision); Assert.Equal(audit.OutcomeHash, report.OutcomeHash); });
            Assert.Equal(contenders[0].IntentId, contenders[1].IntentId);
        }
        else
        {
            var winner = Assert.Single(contenders, report => report.Decision == "recorded");
            var loser = Assert.Single(contenders, report => report.Decision == "source-conflict");
            Assert.NotEqual(winner.IntentId, loser.IntentId);
            Assert.Equal(winner.IntentId, audit.IntentId);
            Assert.Equal(winner.OutcomeHash, audit.OutcomeHash);
            Assert.Null(loser.OutcomeHash);
        }
    }

    private sealed record RaceReport(int ProcessId, Guid IntentId, string Decision, string? OutcomeHash, int HistoryCount, int ReceiptCount);
    private sealed record OwnedWindow(int ProcessId, string Isolation, long StartTicks, long ReleasedTicks);

    private static async Task WaitForFilesAsync(string directory, params string[] names)
    {
        var timer = Stopwatch.StartNew();
        while (names.Any(name => !File.Exists(Path.Combine(directory, name))))
        {
            if (timer.Elapsed > TimeSpan.FromSeconds(30)) throw new TimeoutException("Permit race barrier was not reached: " + string.Join(", ", names));
            await Task.Delay(25);
        }
    }

    public static Task RunRaceStageAsync(string stage, string directory)
    {
        if (stage is not ("race-prepare-same" or "race-prepare-conflict" or "race-a" or "race-b" or "race-audit"))
            throw new ArgumentException("Unknown permit race qualification stage.", nameof(stage));
        var prepare = stage.StartsWith("race-prepare-", StringComparison.Ordinal);
        return new PermitProviderQualificationTests().WithInstalledLedgerAsync(async (editor, source) =>
        {
            var fixture = CreateNativeFixture(editor, source);
            var service = fixture.Service;
            if (prepare)
            {
                var seed = new PERMIT_APPLICATION { PERMIT_APPLICATION_ID = "race-permit", OWNER_TENANT_ID = "tenant", OWNER_COMPANY_ID = "company",
                    FIELD_ID = "field-a", APPLICANT_ID = "Concurrent filing applicant", ACTIVE_IND = "Y" };
                new CommonColumnHandler().PrepareForInsert(seed, "author");
                Assert.Equal(Errors.Ok, source.InsertEntity(nameof(PERMIT_APPLICATION), seed).Flag);
                var a = new PermitApplicationCommand(Guid.NewGuid(), await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID), "SUBMIT", "Reviewed shared source before concurrent delivery");
                var b = stage == "race-prepare-same" ? a : a with { IntentId = Guid.NewGuid(), Reason = "Separately reviewed the same frozen source" };
                await File.WriteAllTextAsync(Path.Combine(directory, "a.original.json"), JsonSerializer.Serialize(a));
                await File.WriteAllTextAsync(Path.Combine(directory, "b.original.json"), JsonSerializer.Serialize(b));
                await File.WriteAllTextAsync(Path.Combine(directory, stage + ".json"), JsonSerializer.Serialize(new RaceReport(Environment.ProcessId, a.IntentId, "prepared", null, 0, 0)));
            }
            else if (stage is "race-a" or "race-b")
            {
                var role = stage == "race-a" ? "a" : "b";
                var other = role == "a" ? "b" : "a";
                var original = JsonSerializer.Deserialize<PermitApplicationCommand>(await File.ReadAllTextAsync(Path.Combine(directory, role + ".original.json")))!;
                original.Require();
                fixture.BeforeCommand = async () =>
                {
                    PPDMDataSourceTransaction.RequireIdle(source);
                    await File.WriteAllTextAsync(Path.Combine(directory, role + ".ready"), Environment.ProcessId.ToString());
                    await WaitForFilesAsync(directory, "release");
                    await File.WriteAllTextAsync(Path.Combine(directory, role + ".attempting"), Environment.ProcessId.ToString());
                };
                var guarded = false;
                fixture.CommandGuard = async () =>
                {
                    PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
                    if (guarded) return;
                    guarded = true;
                    var start = DateTimeOffset.UtcNow.UtcTicks;
                    await WaitForFilesAsync(directory, other + ".attempting");
                    await Task.Delay(250); // Hold a verified transaction while both workers attempt entry.
                    await File.WriteAllTextAsync(Path.Combine(directory, role + ".owned.json"), JsonSerializer.Serialize(new OwnedWindow(Environment.ProcessId, IsolationLevel.Serializable.ToString(), start, DateTimeOffset.UtcNow.UtcTicks)));
                };
                RaceReport report;
                try
                {
                    var saved = await service.RecordCommandAsync(original);
                    saved.Require();
                    Assert.Equal(original, saved.Original);
                    report = new(Environment.ProcessId, original.IntentId, "recorded", ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(saved)), 0, 0);
                }
                catch (RefusalException refusal) when (refusal.Kind == RefusalKind.Conflict)
                {
                    Assert.Null(await service.RecoverCommandAsync(original));
                    report = new(Environment.ProcessId, original.IntentId, "source-conflict", null, 0, 0);
                }
                await File.WriteAllTextAsync(Path.Combine(directory, stage + ".json"), JsonSerializer.Serialize(report));
            }
            else
            {
                fixture.AllowWrites = false;
                var a = JsonSerializer.Deserialize<PermitApplicationCommand>(await File.ReadAllTextAsync(Path.Combine(directory, "a.original.json")))!;
                var b = JsonSerializer.Deserialize<PermitApplicationCommand>(await File.ReadAllTextAsync(Path.Combine(directory, "b.original.json")))!;
                var receipts = (await fixture.ReceiptRepository.GetAsync([])).ToArray();
                var receipt = Assert.IsType<PERMIT_APPLICATION_COMMAND>(Assert.Single(receipts));
                var saved = receipt.Restore(a.Review.Context);
                Assert.True(saved.Original == a || saved.Original == b);
                Assert.Equal(saved, await service.RecoverCommandAsync(saved.Original));
                if (a.IntentId != b.IntentId) Assert.Null(await service.RecoverCommandAsync(saved.Original == a ? b : a));
                var current = await service.ReviewSourceAsync(saved.Original.Review.ReadApplication().PERMIT_APPLICATION_ID);
                Assert.Equal(Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Submitted, current.ReadApplication().STATUS);
                await File.WriteAllTextAsync(Path.Combine(directory, stage + ".json"), JsonSerializer.Serialize(new RaceReport(Environment.ProcessId, saved.Original.IntentId, "audited", ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(saved)), current.ReadHistory().Count, receipts.Length)));
            }
            PPDMDataSourceTransaction.RequireIdle(source);
        }, directory, installSchema: prepare);
    }

    private sealed class NativeFixture
    {
        public bool AllowWrites = true;
        public bool InterruptReceipt;
        public bool Interrupted;
        public Func<Task>? BeforeCommand;
        public Func<Task>? CommandGuard;
        public PermitApplicationLifecycleService Service = null!;
        public PPDMGenericRepository ReceiptRepository = null!;
    }

    private static NativeFixture CreateNativeFixture(IDMEEditor editor, IDataSource source)
    {
        var scope = new PermitApplicationDataScope(new(new("tenant", "company"), new("core", "core-binding"), "author", "selection", "field-a"),
            new(source.DatasourceName, "permit-binding"));
        var metadata = new Mock<IPPDMMetadataRepository>();
        foreach (var type in new[] { typeof(PERMIT_APPLICATION), typeof(PERMIT_STATUS_HISTORY), typeof(PERMIT_APPLICATION_COMMAND) })
        {
            var key = type.Name + "_ID";
            metadata.Setup(x => x.GetTableMetadataAsync(type.Name)).ReturnsAsync(new PPDMTableMetadata { TableName = type.Name, EntityTypeName = type.FullName!, PrimaryKeyColumn = key });
            metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync([key]);
            metadata.Setup(x => x.GetPrimaryKeyColumnAsync(type.Name)).ReturnsAsync(key);
        }
        var defaults = new Mock<IPPDM39DefaultsRepository>();
        defaults.Setup(x => x.GetActiveIndicatorYes()).Returns("Y");
        defaults.Setup(x => x.FormatIdForTable(It.IsAny<string>(), It.IsAny<object>())).Returns((string _, object id) => Convert.ToString(id, System.Globalization.CultureInfo.InvariantCulture)!);
        var access = new Mock<ICompanyPermitApplicationDataAccess>();
        access.Setup(x => x.RequireReadScopeAsync()).ReturnsAsync(scope);
        access.Setup(x => x.ForRead(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(new ActivePermitFixtureReads(scope));
        var fixture = new NativeFixture();
        access.Setup(x => x.WithCommandAuthorityAsync(It.IsAny<PermitApplicationCommand>(), It.IsAny<Func<Func<Task>, Task<bool>>>()))
            .Returns(async (PermitApplicationCommand _, Func<Func<Task>, Task<bool>> execute) =>
            {
                if (!fixture.AllowWrites) throw new UnauthorizedAccessException("Fixture author permission removed.");
                if (fixture.BeforeCommand is not null) await fixture.BeforeCommand();
                return await execute(async () => { if (fixture.CommandGuard is not null) await fixture.CommandGuard(); });
            });
        var actualColumns = new CommonColumnHandler();
        var columns = new Mock<ICommonColumnHandler>();
        columns.Setup(x => x.PrepareForUpdate(It.IsAny<IPPDMEntity>(), It.IsAny<string>())).Callback((IPPDMEntity row, string actor) => actualColumns.PrepareForUpdate(row, actor));
        columns.Setup(x => x.PrepareForInsert(It.IsAny<IPPDMEntity>(), It.IsAny<string>())).Callback((IPPDMEntity row, string actor) =>
        {
            actualColumns.PrepareForInsert(row, actor);
            if (row is PERMIT_APPLICATION_COMMAND && fixture.InterruptReceipt) { fixture.Interrupted = true; throw new InvalidOperationException("Controlled interruption before receipt persistence."); }
        });
        fixture.Service = new PermitApplicationLifecycleService(editor, columns.Object, defaults.Object, metadata.Object, access.Object);
        fixture.ReceiptRepository = new PPDMGenericRepository(editor, actualColumns, defaults.Object, metadata.Object, typeof(PERMIT_APPLICATION_COMMAND), source.DatasourceName,
            nameof(PERMIT_APPLICATION_COMMAND), rowAccess: new ActivePermitFixtureReads(scope));
        return fixture;
    }

    // This isolated storage fixture supplies read authority; actual API/RBAC is qualified separately.
    private sealed class ActivePermitFixtureReads(PermitApplicationDataScope scope) : IPPDMRowAccessPolicy
    {
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table) =>
            Task.FromResult<IReadOnlyList<IReadOnlyList<AppFilter>>>([[new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }]]);
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table) => Task.FromResult<IReadOnlyList<AppFilter>>([]);
        public Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows)
        {
            Assert.All(rows, row =>
            {
                Assert.Equal(type, row.GetType());
                Assert.Equal("Y", ((IPPDMEntity)row).ACTIVE_IND);
                if (row is PERMIT_APPLICATION application)
                {
                    Assert.Equal(scope.Field.Company.TenantId, application.OWNER_TENANT_ID);
                    Assert.Equal(scope.Field.Company.BusinessAssociateId, application.OWNER_COMPANY_ID);
                    Assert.Equal(scope.Field.FieldId, application.FIELD_ID);
                }
            });
            return Task.CompletedTask;
        }
        public Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool inherited = true) => Task.FromResult<string?>("READ");
        public Task ValidateMutationAsync(Type type, string table, object row, object? previous, PPDMRowMutation mutation, string? actor) => throw new UnauthorizedAccessException("Read-only fixture policy.");
    }
}
#endif
