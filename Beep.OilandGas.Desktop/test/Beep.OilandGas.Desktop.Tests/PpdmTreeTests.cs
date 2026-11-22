using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-03: the PPDM tree is built from the metadata registry, the search matches name and description, no row
/// count is read on the first open, a registry failure is a failure (never an empty tree), and a table the
/// policy restricts is marked rather than dropped.
/// </summary>
public sealed class PpdmTreeTests
{
    [Fact]
    public async Task The_node_count_equals_the_registry_count()
    {
        var registry = Registry(9);
        var builder = new OgPpdmTreeBuilder(registry, new Quiet());

        var built = await builder.BuildAsync(new OgTreeOptions());

        Assert.True(built.Success, built.Message);
        var root = built.Value!;
        var tables = OgPpdmSearch.MatchingTables(root, null);

        Assert.Equal(registry.Tables.Count, tables.Count);
        Assert.Equal(registry.Tables.Count, root.Count);
        Assert.All(root.ChildNodes, category => Assert.Equal(OgTreeNodeKind.Category, category.Kind));
    }

    [Fact]
    public async Task The_first_open_reads_no_row_counts()
    {
        var countsRead = 0;
        var builder = new OgPpdmTreeBuilder(
            Registry(5),
            new Quiet(),
            rowCount: (_, _) =>
            {
                countsRead++;
                return Task.FromResult<int?>(42);
            });

        var built = await builder.BuildAsync(new OgTreeOptions());

        Assert.True(built.Success);
        // AC-5: neither the build nor the summaries fetch a count.
        Assert.Equal(0, countsRead);
        Assert.Equal(0, builder.CountReads);
        Assert.All(OgPpdmSearch.MatchingTables(built.Value!, null), node => Assert.Null(node.Count));
    }

    [Fact]
    public async Task A_count_is_read_only_when_asked_for()
    {
        var builder = new OgPpdmTreeBuilder(Registry(3), new Quiet(), rowCount: (_, _) => Task.FromResult<int?>(17));

        var counted = await builder.CountRowsAsync("T_WELL_000");

        Assert.True(counted.Success);
        Assert.Equal(17, counted.Value);
        Assert.Equal(1, builder.CountReads);
    }

    [Fact]
    public async Task Search_matches_a_table_by_name()
    {
        var built = await new OgPpdmTreeBuilder(Registry(4), new Quiet()).BuildAsync(new OgTreeOptions());
        var root = built.Value!;

        var byName = OgPpdmSearch.MatchingTables(root, "T_WELL_0002");

        Assert.Single(byName);
        Assert.Equal("T_WELL_0002", byName[0].TableName);
    }

    [Fact]
    public async Task Search_matches_a_table_by_its_description()
    {
        var registry = new FakeRegistry();
        registry.Add("R_WELL_STATUS", "Status", "Well", subjectArea: "Well Status Codes");
        registry.Add("T_WELL", "Well", "Well", subjectArea: "Well Header");

        var built = await new OgPpdmTreeBuilder(registry, new Quiet()).BuildAsync(new OgTreeOptions());
        var root = built.Value!;

        var byDescription = OgPpdmSearch.MatchingTables(root, "status codes");

        Assert.Single(byDescription);
        Assert.Equal("R_WELL_STATUS", byDescription[0].TableName);
    }

    [Fact]
    public async Task Every_search_term_must_match_and_matching_is_case_insensitive()
    {
        var registry = new FakeRegistry();
        registry.Add("RA_WELL_STATUS_TYPE", "Ref", "Well", subjectArea: "Status reference");
        registry.Add("T_WELL", "Well", "Well", subjectArea: "Well header");

        var built = await new OgPpdmTreeBuilder(registry, new Quiet()).BuildAsync(new OgTreeOptions());
        var root = built.Value!;

        Assert.Single(OgPpdmSearch.MatchingTables(root, "WELL status"));
        Assert.Empty(OgPpdmSearch.MatchingTables(root, "well header status"));
        Assert.Equal(2, OgPpdmSearch.MatchingTables(root, "well").Count);
    }

    [Fact]
    public async Task A_registry_failure_is_reported_as_a_failure_not_an_empty_tree()
    {
        var reporter = new RecordingFailures();
        var builder = new OgPpdmTreeBuilder(new ThrowingRegistry(), reporter);

        var built = await builder.BuildAsync(new OgTreeOptions());

        // AC-4
        Assert.False(built.Success);
        Assert.Null(built.Value);
        Assert.Single(reporter.Reported);
        Assert.Contains("registry could not be read", built.Message!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task A_policy_restricted_table_is_marked_not_hidden()
    {
        var registry = new FakeRegistry();
        registry.Add("T_SECRET", "Secret", "Admin", subjectArea: "Restricted");
        registry.Add("T_WELL", "Well", "Well", subjectArea: "Well header");

        var builder = new OgPpdmTreeBuilder(registry, new Quiet(), canRead: name => name != "T_SECRET");
        var root = (await builder.BuildAsync(new OgTreeOptions())).Value!;

        var tables = OgPpdmSearch.MatchingTables(root, null);

        // AC-6: both are present; the restricted one says so.
        Assert.Equal(2, tables.Count);
        var secret = Assert.Single(tables, node => node.TableName == "T_SECRET");
        Assert.True(secret.PolicyRestricted);
        Assert.Contains("restricted", secret.PolicyNote!, StringComparison.OrdinalIgnoreCase);

        var well = Assert.Single(tables, node => node.TableName == "T_WELL");
        Assert.False(well.PolicyRestricted);
    }

    [Fact]
    public async Task A_table_summary_names_the_key_module_and_related_tables()
    {
        var registry = new FakeRegistry();
        registry.Add("T_WELL_DIRECTIONAL_SURVEY", "Well", "Well", subjectArea: "Directional Survey",
            primaryKey: "SURVEY_ID", related: ["T_WELL", "R_SURVEY_TOOL_TYPE"]);

        var builder = new OgPpdmTreeBuilder(registry, new Quiet());

        var summary = await builder.SummarizeAsync("t_well_directional_survey");

        Assert.True(summary.Success, summary.Message);
        var value = summary.Value!;

        Assert.Equal("T_WELL_DIRECTIONAL_SURVEY", value.TableName);
        Assert.Equal("Well", value.Module);
        Assert.Equal("SURVEY_ID", value.PrimaryKeyColumn);
        Assert.Equal(["SURVEY_ID"], value.KeyColumns);
        Assert.Equal(2, value.ForeignKeyCount);
        Assert.Contains("T_WELL", value.RelatedTables);
        Assert.Contains("T_WELL_DIRECTIONAL_SURVEY", value.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task An_unknown_table_summary_is_refused_by_name()
    {
        var summary = await new OgPpdmTreeBuilder(Registry(2), new Quiet()).SummarizeAsync("T_NOT_A_TABLE");

        Assert.False(summary.Success);
        Assert.Contains("T_NOT_A_TABLE", summary.Message!, StringComparison.Ordinal);
    }

    private static FakeRegistry Registry(int tables)
    {
        var registry = new FakeRegistry();

        for (var index = 0; index < tables; index++)
        {
            registry.Add(
                $"T_WELL_{index:0000}",
                "Well",
                index % 2 == 0 ? "Well" : "Drilling",
                subjectArea: $"Well table {index}");
        }

        return registry;
    }

    private sealed class FakeRegistry : IPPDMMetadataRepository
    {
        public Dictionary<string, PPDMTableMetadata> Tables { get; } = new(StringComparer.OrdinalIgnoreCase);

        public void Add(
            string table,
            string entity,
            string module,
            string? subjectArea = null,
            string? primaryKey = null,
            string[]? related = null)
        {
            var metadata = new PPDMTableMetadata
            {
                TableName = table,
                EntityTypeName = entity,
                Module = module,
                SubjectArea = subjectArea,
                PrimaryKeyColumn = primaryKey,
            };

            foreach (var referenced in related ?? [])
            {
                metadata.ForeignKeys.Add(new PPDMForeignKey
                {
                    ForeignKeyColumn = primaryKey ?? "ID",
                    ReferencedTable = referenced,
                });
            }

            Tables[table] = metadata;
        }

        public Task<Dictionary<string, PPDMTableMetadata>> GetAllMetadataAsync() =>
            Task.FromResult(Tables);

        public Task<PPDMTableMetadata> GetTableMetadataAsync(string tableName) =>
            Task.FromResult(Tables.TryGetValue(tableName, out var table) ? table : null!);

        public Task<IEnumerable<PPDMTableMetadata>> GetTablesByModuleAsync(string module) =>
            Task.FromResult(Tables.Values.Where(table => table.Module == module));

        public Task<IEnumerable<PPDMForeignKey>> GetForeignKeysAsync(string tableName) =>
            Task.FromResult<IEnumerable<PPDMForeignKey>>(
                Tables.TryGetValue(tableName, out var table) ? table.ForeignKeys : []);

        public Task<IEnumerable<PPDMTableMetadata>> GetReferencingTablesAsync(string tableName) =>
            Task.FromResult(Tables.Values.Where(table =>
                table.ForeignKeys.Any(key => string.Equals(key.ReferencedTable, tableName, StringComparison.OrdinalIgnoreCase))));

        public Task<string> GetPrimaryKeyColumnAsync(string tableName) =>
            Task.FromResult(Tables.TryGetValue(tableName, out var table) ? table.PrimaryKeyColumn : string.Empty);

        public Task<string> GetEntityTypeNameAsync(string tableName) =>
            Task.FromResult(Tables.TryGetValue(tableName, out var table) ? table.EntityTypeName : string.Empty);

        public Task<bool> HasCommonColumnsAsync(string tableName) => Task.FromResult(false);

        public Task<IEnumerable<string>> GetModulesAsync() =>
            Task.FromResult(Tables.Values.Select(table => table.Module).Distinct());

        public Task<IEnumerable<string>> GetPrimaryKeyColumnsAsync(string tableName) =>
            Task.FromResult<IEnumerable<string>>(
                Tables.TryGetValue(tableName, out var table) && !string.IsNullOrWhiteSpace(table.PrimaryKeyColumn)
                    ? [table.PrimaryKeyColumn]
                    : []);

        public Task<IEnumerable<PPDMTableMetadata>> GetTablesBySubjectAreaAsync(string subjectArea) =>
            Task.FromResult(Tables.Values.Where(table => table.SubjectArea == subjectArea));

        public Task<IEnumerable<PPDMTableMetadata>> GetTablesByPatternAsync(string pattern) =>
            Task.FromResult(Tables.Values.Where(table => table.TableName.Contains(pattern, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> IsCompositeKeyAsync(string tableName) => Task.FromResult(false);

        public Task RefreshMetadataAsync() => Task.CompletedTask;
    }

    private sealed class ThrowingRegistry : IPPDMMetadataRepository
    {
        public Task<Dictionary<string, PPDMTableMetadata>> GetAllMetadataAsync() =>
            throw new InvalidOperationException("registry offline");

        public Task<PPDMTableMetadata> GetTableMetadataAsync(string tableName) => throw new InvalidOperationException("registry offline");
        public Task<IEnumerable<PPDMTableMetadata>> GetTablesByModuleAsync(string module) => throw new InvalidOperationException("registry offline");
        public Task<IEnumerable<PPDMForeignKey>> GetForeignKeysAsync(string tableName) => throw new InvalidOperationException("registry offline");
        public Task<IEnumerable<PPDMTableMetadata>> GetReferencingTablesAsync(string tableName) => throw new InvalidOperationException("registry offline");
        public Task<string> GetPrimaryKeyColumnAsync(string tableName) => throw new InvalidOperationException("registry offline");
        public Task<string> GetEntityTypeNameAsync(string tableName) => throw new InvalidOperationException("registry offline");
        public Task<bool> HasCommonColumnsAsync(string tableName) => throw new InvalidOperationException("registry offline");
        public Task<IEnumerable<string>> GetModulesAsync() => throw new InvalidOperationException("registry offline");
        public Task<IEnumerable<string>> GetPrimaryKeyColumnsAsync(string tableName) => throw new InvalidOperationException("registry offline");
        public Task<IEnumerable<PPDMTableMetadata>> GetTablesBySubjectAreaAsync(string subjectArea) => throw new InvalidOperationException("registry offline");
        public Task<IEnumerable<PPDMTableMetadata>> GetTablesByPatternAsync(string pattern) => throw new InvalidOperationException("registry offline");
        public Task<bool> IsCompositeKeyAsync(string tableName) => throw new InvalidOperationException("registry offline");
        public Task RefreshMetadataAsync() => throw new InvalidOperationException("registry offline");
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }

    private sealed class RecordingFailures : IFailureReporter
    {
        public List<string> Reported { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Reported.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
