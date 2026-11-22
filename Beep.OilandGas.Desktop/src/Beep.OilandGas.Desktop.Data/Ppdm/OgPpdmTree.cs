using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;

/// <summary>What a node in the PPDM tree is.</summary>
public enum OgTreeNodeKind
{
    Root,
    Category,
    Table,
    BusinessProcess,
}

/// <summary>
/// One node of the PPDM tree (DAT-03). <see cref="Count"/> is null until a count has been asked for, so the
/// first open never runs one query per table (AC-5).
/// </summary>
public sealed record OgTreeNode(
    string Key,
    string Title,
    OgTreeNodeKind Kind,
    string? TableName = null,
    string? Description = null,
    int? Count = null,
    bool CountPending = false,
    bool PolicyRestricted = false,
    string? PolicyNote = null,
    IReadOnlyList<OgTreeNode>? Children = null)
{
    public IReadOnlyList<OgTreeNode> ChildNodes => Children ?? [];
}

/// <summary>What to include when the tree is built.</summary>
/// <param name="OnlyModules">Restrict to these module names; null or empty means every module.</param>
public sealed record OgTreeOptions(
    IReadOnlyList<string>? OnlyModules = null,
    bool IncludeBusinessProcessRoot = false);

/// <summary>The summary shown when a table node is selected.</summary>
public sealed record OgTableSummary(
    string TableName,
    string EntityTypeName,
    string Module,
    string? SubjectArea,
    string? PrimaryKeyColumn,
    IReadOnlyList<string> KeyColumns,
    IReadOnlyList<string> RelatedTables,
    int ForeignKeyCount,
    int? RowCount,
    bool RowCountPending)
{
    public string Describe() =>
        $"{TableName} · module {Module} · key {(KeyColumns.Count == 0 ? "(none recorded)" : string.Join("+", KeyColumns))} · " +
        $"{ForeignKeyCount} FK · {RelatedTables.Count} related table(s)";
}

/// <summary>
/// DAT-03: the PPDM tree browser's data side. Nodes come from the metadata registry, which is the one source of
/// table/category/relationship information; row counts are only fetched when asked for, one table at a time.
/// </summary>
public interface IOgPpdmTreeBuilder
{
    Task<OgResult<OgTreeNode>> BuildAsync(OgTreeOptions options, CancellationToken cancellationToken = default);

    Task<OgResult<OgTableSummary>> SummarizeAsync(string tableName, CancellationToken cancellationToken = default);

    /// <summary>Fills in one table's row count, for the node the person actually opened.</summary>
    Task<OgResult<int>> CountRowsAsync(string tableName, CancellationToken cancellationToken = default);
}

/// <summary>
/// The builder over <see cref="IPPDMMetadataRepository"/>. It performs no database read for the tree itself and
/// no count for any node until <see cref="CountRowsAsync"/> is called, which is what keeps the first open cheap
/// with ~2700 entity types in the model.
/// </summary>
public sealed class OgPpdmTreeBuilder : IOgPpdmTreeBuilder
{
    /// <summary>Code returned when the registry cannot be read (AC-4).</summary>
    public const string CodeMetadataUnavailable = "ppdm-metadata-unavailable";

    private readonly IPPDMMetadataRepository _metadata;
    private readonly IFailureReporter _failures;
    private readonly Func<string, CancellationToken, Task<int?>>? _rowCount;
    private readonly Func<string, bool>? _canRead;

    /// <param name="metadata">The table registry.</param>
    /// <param name="rowCount">Optional row-count reader; null means counts are simply unavailable.</param>
    /// <param name="canRead">
    /// Optional policy probe: false marks a table as restricted rather than dropping it silently (AC-6).
    /// </param>
    public OgPpdmTreeBuilder(
        IPPDMMetadataRepository metadata,
        IFailureReporter failures,
        Func<string, CancellationToken, Task<int?>>? rowCount = null,
        Func<string, bool>? canRead = null)
    {
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _rowCount = rowCount;
        _canRead = canRead;
    }

    /// <summary>How many times a row count has been read; the page and tests assert the first open is zero.</summary>
    public int CountReads { get; private set; }

    public async Task<OgResult<OgTreeNode>> BuildAsync(
        OgTreeOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        Dictionary<string, PPDMTableMetadata> metadata;

        try
        {
            metadata = await _metadata.GetAllMetadataAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the PPDM table registry",
                "the PPDM tree is not shown",
                FailureSeverity.Error);

            return OgResult<OgTreeNode>.Failed(
                "The PPDM table registry could not be read, so the tree cannot be shown.",
                reference ?? CodeMetadataUnavailable);
        }

        if (metadata is null || metadata.Count == 0)
        {
            return OgResult<OgTreeNode>.Failed(
                "The PPDM table registry is empty, so the tree cannot be shown.",
                CodeMetadataUnavailable);
        }

        var wanted = options.OnlyModules is { Count: > 0 }
            ? new HashSet<string>(options.OnlyModules, StringComparer.OrdinalIgnoreCase)
            : null;

        var groups = metadata.Values
            .Where(table => wanted is null || wanted.Contains(ModuleOf(table)))
            .GroupBy(ModuleOf, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

        var categories = new List<OgTreeNode>();

        foreach (var group in groups)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tables = group
                .OrderBy(table => table.TableName, StringComparer.OrdinalIgnoreCase)
                .Select(table => ToNode(table))
                .ToList();

            categories.Add(new OgTreeNode(
                Key: $"category:{group.Key}",
                Title: group.Key,
                Kind: OgTreeNodeKind.Category,
                Count: tables.Count,
                Children: tables));
        }

        var root = new OgTreeNode(
            Key: "ppdm",
            Title: "PPDM",
            Kind: OgTreeNodeKind.Root,
            Count: categories.Sum(category => category.Count ?? 0),
            Children: categories);

        return OgResult<OgTreeNode>.Ok(root);
    }

    public async Task<OgResult<OgTableSummary>> SummarizeAsync(
        string tableName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("A summary needs a table name.", nameof(tableName));
        }

        Dictionary<string, PPDMTableMetadata> metadata;

        try
        {
            metadata = await _metadata.GetAllMetadataAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the PPDM table registry for a table summary",
                "no table summary is shown",
                FailureSeverity.Error);

            return OgResult<OgTableSummary>.Failed(
                "The PPDM table registry could not be read, so no summary is available.",
                reference ?? CodeMetadataUnavailable);
        }

        if (!metadata.TryGetValue(tableName, out var table))
        {
            // A case-insensitive fallback: the tree is built from the same keys, but a hand-typed name may differ.
            table = metadata.Values.FirstOrDefault(candidate =>
                string.Equals(candidate.TableName, tableName, StringComparison.OrdinalIgnoreCase));
        }

        if (table is null)
        {
            return OgResult<OgTableSummary>.Failed(
                $"'{tableName}' is not a table in the PPDM registry.",
                "ppdm-table-unknown");
        }

        var related = table.ForeignKeys
            .Select(key => key.ReferencedTable)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var keys = string.IsNullOrWhiteSpace(table.PrimaryKeyColumn)
            ? []
            : new[] { table.PrimaryKeyColumn };

        return OgResult<OgTableSummary>.Ok(new OgTableSummary(
            table.TableName,
            table.EntityTypeName,
            ModuleOf(table),
            table.SubjectArea,
            table.PrimaryKeyColumn,
            keys,
            related,
            table.ForeignKeys.Count,
            RowCount: null,
            RowCountPending: _rowCount is not null));
    }

    public async Task<OgResult<int>> CountRowsAsync(
        string tableName,
        CancellationToken cancellationToken = default)
    {
        if (_rowCount is null)
        {
            return OgResult<int>.Failed(
                "Row counts are not available for this connection.",
                "ppdm-rowcount-unavailable");
        }

        CountReads++;
        var count = await _rowCount(tableName, cancellationToken).ConfigureAwait(false);

        return count is { } value
            ? OgResult<int>.Ok(value)
            : OgResult<int>.Failed(
                $"The row count for '{tableName}' could not be read.",
                "ppdm-rowcount-unavailable");
    }

    private OgTreeNode ToNode(PPDMTableMetadata table)
    {
        var restricted = _canRead?.Invoke(table.TableName) == false;

        return new OgTreeNode(
            Key: $"table:{table.TableName}",
            Title: table.TableName,
            Kind: OgTreeNodeKind.Table,
            TableName: table.TableName,
            Description: table.SubjectArea,
            PolicyRestricted: restricted,
            PolicyNote: restricted
                ? "Rows in this table are restricted by your role; the table is listed so it is not silently missing."
                : null);
    }

    private static string ModuleOf(PPDMTableMetadata table) =>
        string.IsNullOrWhiteSpace(table.Module) ? "(unclassified)" : table.Module;
}

/// <summary>
/// DAT-03 AC-2: the tree filter. A term matches a table's name or its description (subject area), and every
/// term must match, so two words narrow rather than widen.
/// </summary>
public static class OgPpdmSearch
{
    public static IReadOnlyList<OgTreeNode> Filter(OgTreeNode node, string? query)
    {
        ArgumentNullException.ThrowIfNull(node);

        var terms = (query ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (terms.Length == 0)
        {
            return [node];
        }

        return FilterNode(node, terms);
    }

    /// <summary>The tables that match, flattened - what the page puts in the results list.</summary>
    public static IReadOnlyList<OgTreeNode> MatchingTables(OgTreeNode root, string? query)
    {
        ArgumentNullException.ThrowIfNull(root);

        var terms = (query ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return Flatten(root)
            .Where(node => node.Kind == OgTreeNodeKind.Table)
            .Where(node => terms.Length == 0 || Matches(node, terms))
            .ToList();
    }

    private static List<OgTreeNode> FilterNode(OgTreeNode node, string[] terms)
    {
        if (node.Kind == OgTreeNodeKind.Table && Matches(node, terms))
        {
            return [node];
        }

        var kept = new List<OgTreeNode>();

        foreach (var child in node.ChildNodes)
        {
            if (child.Kind == OgTreeNodeKind.Table)
            {
                if (Matches(child, terms))
                {
                    kept.Add(child);
                }

                continue;
            }

            var nested = FilterNode(child, terms);

            if (nested.Count > 0)
            {
                kept.Add(child with { Children = nested, Count = nested.Count });
            }
        }

        return kept;
    }

    private static bool Matches(OgTreeNode node, string[] terms) =>
        terms.All(term =>
            node.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
            || (node.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));

    private static IEnumerable<OgTreeNode> Flatten(OgTreeNode node)
    {
        yield return node;

        foreach (var child in node.ChildNodes)
        {
            foreach (var nested in Flatten(child))
            {
                yield return nested;
            }
        }
    }
}
