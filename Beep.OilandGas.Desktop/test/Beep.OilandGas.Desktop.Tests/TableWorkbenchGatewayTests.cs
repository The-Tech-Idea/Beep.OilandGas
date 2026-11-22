using System.Text;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-04 AC-1, AC-2, AC-5 and AC-7 against a real SQLite table: paging is bounded, filtering and sorting happen
/// in the database, a failed read is a failure, and the export carries the filtered rows only.
/// </summary>
/// <remarks>
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>): it provisions a local database through BeepDM, as the desktop does,
/// and reads 500 rows through the gateway. The paging arithmetic is cheap; the host is what takes a moment.
/// </remarks>
public sealed class TableWorkbenchGatewayTests
{
    private const string ConnectionName = "dat04-workbench";
    private const int TotalRows = 500;

    [IntegrationFact]
    public async Task Paging_filtering_sorting_and_export_respect_the_bound_query()
    {        var root = Path.Combine(OgPaths.DataRoot, "dat04-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailures();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDat04"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "workbench.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        var created = await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "workbench.db"));

        Assert.True(created.Success, created.Message);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);

        Seed(database);

        var metadata = new FakeMetadata();
        var gateway = new OgPpdmTableGateway(() => source, metadata, reporter);

        // AC-1: one page, never the whole table.
        var first = await gateway.QueryAsync("T_SMOKE_ROW", new OgQuery(Page: 0, PageSize: 200));

        Assert.True(first.Success, first.Message);
        Assert.Equal(200, first.Value!.Items.Count);
        Assert.Equal(TotalRows, first.Value.Total);
        Assert.True(first.Value.HasMore);

        // AC-2: filtering and sorting are in the predicate, and paging still holds.
        var filtered = await gateway.QueryAsync(
            "T_SMOKE_ROW",
            new OgQuery(Page: 0, PageSize: 200, Filters: [new OgFilter("BAND", "A")]));

        Assert.True(filtered.Success, filtered.Message);
        Assert.Equal(TotalRows / 2, filtered.Value!.Total);
        Assert.All(filtered.Value.Items, row => Assert.Equal("A", row.Values["BAND"]));

        var sorted = await gateway.QueryAsync(
            "T_SMOKE_ROW",
            new OgQuery(Page: 0, PageSize: 5, Sort: [new OgSort("ID", Descending: true)]));

        Assert.True(sorted.Success, sorted.Message);
        Assert.Equal(TotalRows, Convert.ToInt64(sorted.Value!.Items[0].Values["ID"]));
        Assert.Equal(TotalRows - 1, Convert.ToInt64(sorted.Value.Items[1].Values["ID"]));

        var thirdPage = await gateway.QueryAsync("T_SMOKE_ROW", new OgQuery(Page: 2, PageSize: 200));

        Assert.True(thirdPage.Success);
        Assert.Equal(100, thirdPage.Value!.Items.Count);
        Assert.False(thirdPage.Value.HasMore);

        // AC-1 enforced by the gateway: an oversized page request is clamped, not honoured.
        var oversized = await gateway.QueryAsync("T_SMOKE_ROW", new OgQuery(Page: 0, PageSize: 10_000));

        Assert.True(oversized.Success);
        Assert.Equal(OgPpdmTableGateway.MaximumPageSize, oversized.Value!.Items.Count);

        // AC-7: the export carries the filtered rows, not the whole table.
        var exported = await gateway.ExportAsync(
            "T_SMOKE_ROW",
            new OgQuery(Page: 0, PageSize: 200, Filters: [new OgFilter("BAND", "A")]));

        Assert.True(exported.Success, exported.Message);
        var text = Encoding.UTF8.GetString(exported.Value!);
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal((TotalRows / 2) + 2, lines.Length);      // header + filtered rows + summary line
        Assert.Contains($"of {TotalRows / 2} matching", text, StringComparison.Ordinal);
        Assert.Contains(",A,", text, StringComparison.Ordinal);

        // AC-5: a failed read is a failure with a reference, never an empty page.
        var failed = await gateway.QueryAsync("NO_SUCH_TABLE_FOR_SMOKE", new OgQuery());

        Assert.False(failed.Success);
        Assert.Empty(failed.Value?.Items ?? []);
        Assert.False(string.IsNullOrWhiteSpace(failed.Reference));

        // The identifier guard refuses a name that is not a plain table name before SQL is built.
        var injected = await gateway.QueryAsync("T_SMOKE_ROW; DROP TABLE T_SMOKE_ROW", new OgQuery());

        Assert.False(injected.Success);
        Assert.Equal(OgPpdmTableGateway.CodeInvalidIdentifier, injected.Reference);
    }

    [IntegrationFact]
    public async Task An_edit_round_trips_preserves_untouched_columns_and_detects_a_conflict()
    {        var root = Path.Combine(OgPaths.DataRoot, "dat04w-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailures();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDat04Write"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "write.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest("dat04-write", Path.Combine(root, "Data"), "write.db"))).Success);

        var source = host.Editor.GetDataSource("dat04-write");
        Assert.NotNull(source);
        Seed(database);

        var gateway = new OgPpdmTableGateway(() => source, new FakeMetadata(), reporter);

        var read = await gateway.QueryAsync("T_SMOKE_ROW", new OgQuery(Page: 0, PageSize: 1, Sort: [new OgSort("ID")]));
        Assert.True(read.Success, read.Message);
        var original = read.Value!.Items[0];
        Assert.Equal("B", original.Values["BAND"]);          // ID 1 is odd → B

        // AC-3: change one column; the others must be untouched.
        var edited = new OgRow(new Dictionary<string, object?>(original.Values, StringComparer.OrdinalIgnoreCase)
        {
            ["BAND"] = "Z",
        });

        var updated = await gateway.UpdateAsync("T_SMOKE_ROW", edited, original);
        Assert.True(updated.Success, updated.Message);

        var reread = await gateway.QueryAsync("T_SMOKE_ROW", new OgQuery(Page: 0, PageSize: 1, Sort: [new OgSort("ID")]));
        var row = reread.Value!.Items[0];
        Assert.Equal("Z", row.Values["BAND"]);
        Assert.Equal(original.Values["DEPTH"], row.Values["DEPTH"]);
        Assert.Equal(1L, Convert.ToInt64(row.Values["ID"]));

        // AC-4: the same edit again, with the stale original, no longer matches the row.
        var stale = await gateway.UpdateAsync("T_SMOKE_ROW", edited, original);

        Assert.False(stale.Success);
        Assert.Equal(OgPpdmTableGateway.CodeConflict, stale.Reference);

        // AC-4: deleting with stale values is refused too, and the row is still there.
        var staleDelete = await gateway.DeleteAsync("T_SMOKE_ROW", original);
        Assert.False(staleDelete.Success);
        Assert.Equal(OgPpdmTableGateway.CodeConflict, staleDelete.Reference);
        Assert.Equal(TotalRows, (await gateway.QueryAsync("T_SMOKE_ROW", new OgQuery())).Value!.Total);

        // Insert, then delete the inserted row with its own values.
        var inserted = new OgRow(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["ID"] = TotalRows + 1,
            ["BAND"] = "C",
            ["DEPTH"] = 99.5,
        });

        Assert.True((await gateway.InsertAsync("T_SMOKE_ROW", inserted)).Success);
        Assert.Equal(TotalRows + 1, (await gateway.QueryAsync("T_SMOKE_ROW", new OgQuery())).Value!.Total);
        Assert.True((await gateway.DeleteAsync("T_SMOKE_ROW", inserted)).Success);
        Assert.Equal(TotalRows, (await gateway.QueryAsync("T_SMOKE_ROW", new OgQuery())).Value!.Total);
    }

    [IntegrationFact]
    public async Task A_row_policy_predicate_filters_reads_and_is_visible()
    {        var root = Path.Combine(OgPaths.DataRoot, "dat04p-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailures();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDat04Policy"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "policy.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest("dat04-policy", Path.Combine(root, "Data"), "policy.db"))).Success);

        var source = host.Editor.GetDataSource("dat04-policy");
        Assert.NotNull(source);
        Seed(database);

        var gateway = new OgPpdmTableGateway(
            () => source,
            new FakeMetadata(),
            reporter,
            rowFilter: _ => [new AppFilter { FieldName = "BAND", Operator = "=", FilterValue = "A" }],
            policyNote: () => "Only BAND A rows are visible to your role.");

        // AC-6: the predicate is applied in the query, so the total is the policy total, not the table total.
        var read = await gateway.QueryAsync("T_SMOKE_ROW", new OgQuery(Page: 0, PageSize: 200));

        Assert.True(read.Success, read.Message);
        Assert.Equal(TotalRows / 2, read.Value!.Total);
        Assert.All(read.Value.Items, row => Assert.Equal("A", row.Values["BAND"]));
        Assert.Contains("BAND A", gateway.PolicyNote, StringComparison.Ordinal);
    }
    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using (var create = connection.CreateCommand())
        {
            create.CommandText =
                "CREATE TABLE T_SMOKE_ROW (ID INTEGER NOT NULL PRIMARY KEY, BAND TEXT NOT NULL, DEPTH REAL NOT NULL);";
            create.ExecuteNonQuery();
        }

        using var transaction = connection.BeginTransaction();
        using var insert = connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = "INSERT INTO T_SMOKE_ROW (ID, BAND, DEPTH) VALUES ($id, $band, $depth);";

        var id = insert.Parameters.Add("$id", SqliteType.Integer);
        var band = insert.Parameters.Add("$band", SqliteType.Text);
        var depth = insert.Parameters.Add("$depth", SqliteType.Real);

        for (var index = 1; index <= TotalRows; index++)
        {
            id.Value = index;
            band.Value = index % 2 == 0 ? "A" : "B";
            depth.Value = index / 10.0;
            insert.ExecuteNonQuery();
        }

        transaction.Commit();
    }
    private sealed class FakeMetadata : IPPDMMetadataRepository
    {
        public Task<Dictionary<string, PPDMTableMetadata>> GetAllMetadataAsync() =>
            Task.FromResult<Dictionary<string, PPDMTableMetadata>>(new(StringComparer.OrdinalIgnoreCase));

        public Task<PPDMTableMetadata> GetTableMetadataAsync(string tableName) => Task.FromResult<PPDMTableMetadata>(null!);
        public Task<IEnumerable<PPDMTableMetadata>> GetTablesByModuleAsync(string module) => Task.FromResult<IEnumerable<PPDMTableMetadata>>([]);
        public Task<IEnumerable<PPDMForeignKey>> GetForeignKeysAsync(string tableName) => Task.FromResult<IEnumerable<PPDMForeignKey>>([]);
        public Task<IEnumerable<PPDMTableMetadata>> GetReferencingTablesAsync(string tableName) => Task.FromResult<IEnumerable<PPDMTableMetadata>>([]);
        public Task<string> GetPrimaryKeyColumnAsync(string tableName) => Task.FromResult("ID");
        public Task<string> GetEntityTypeNameAsync(string tableName) => Task.FromResult("T_SMOKE_ROW");
        public Task<bool> HasCommonColumnsAsync(string tableName) => Task.FromResult(false);
        public Task<IEnumerable<string>> GetModulesAsync() => Task.FromResult<IEnumerable<string>>([]);
        public Task<IEnumerable<string>> GetPrimaryKeyColumnsAsync(string tableName) => Task.FromResult<IEnumerable<string>>(["ID"]);
        public Task<IEnumerable<PPDMTableMetadata>> GetTablesBySubjectAreaAsync(string subjectArea) => Task.FromResult<IEnumerable<PPDMTableMetadata>>([]);
        public Task<IEnumerable<PPDMTableMetadata>> GetTablesByPatternAsync(string pattern) => Task.FromResult<IEnumerable<PPDMTableMetadata>>([]);
        public Task<bool> IsCompositeKeyAsync(string tableName) => Task.FromResult(false);
        public Task RefreshMetadataAsync() => Task.CompletedTask;
    }

    private sealed class RecordingFailures : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
