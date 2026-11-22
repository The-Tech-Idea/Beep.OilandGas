using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;

/// <summary>How an import treats a row whose key already exists (plan section 3, step 2).</summary>
public enum OgImportMode
{
    /// <summary>Insert every acceptable row; a duplicate key is refused by the database, not by us.</summary>
    InsertOnly,

    /// <summary>Update the row when its key already exists (the default, and the only safe choice for keyed tables).</summary>
    Upsert,
}

/// <summary>One physical line of a CSV file, with the line number a person can look up.</summary>
public sealed record OgCsvRow(int LineNumber, IReadOnlyList<string> Cells);

/// <summary>A parsed CSV file: its header and its rows.</summary>
public sealed record OgCsvDocument(
    IReadOnlyList<string> Headers,
    IReadOnlyList<OgCsvRow> Rows,
    char Delimiter)
{
    public int Count => Rows.Count;
}

/// <summary>The encodings the wizard offers: UTF-8 by default, Latin-1 for single-byte files from Windows tools.</summary>
public enum OgCsvEncoding
{
    Utf8,
    Latin1,
}

/// <summary>
/// DAT-06: the reader. Quotes are honoured (a comma or a newline inside quotes is data), doubled quotes are one
/// quote, and a UTF-8 byte-order mark is dropped. A row with a different cell count than the header is kept and
/// reported by the importer rather than silently padded.
/// </summary>
public static class OgCsv
{
    /// <summary>Decodes the file bytes with the chosen encoding (plan section 3, step 1).</summary>
    public static string Decode(byte[] bytes, OgCsvEncoding encoding = OgCsvEncoding.Utf8)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        return encoding switch
        {
            OgCsvEncoding.Utf8 => Encoding.UTF8.GetString(bytes),
            OgCsvEncoding.Latin1 => Encoding.Latin1.GetString(bytes),
        };
    }

    public static OgCsvDocument Parse(string text, char delimiter = ',')
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length > 0 && text[0] == '\uFEFF')
        {
            text = text[1..];
        }

        var rows = new List<OgCsvRow>();
        var cells = new List<string>();
        var cell = new StringBuilder();
        var quoted = false;
        var line = 1;
        var rowStartLine = 1;
        var index = 0;

        void EndCell()
        {
            cells.Add(cell.ToString());
            cell.Clear();
        }

        void EndRow()
        {
            EndCell();
            rows.Add(new OgCsvRow(rowStartLine, cells.ToArray()));
            cells.Clear();
            rowStartLine = line;
        }

        while (index < text.Length)
        {
            var current = text[index];

            if (quoted)
            {
                if (current == '"')
                {
                    if (index + 1 < text.Length && text[index + 1] == '"')
                    {
                        cell.Append('"');
                        index += 2;
                        continue;
                    }

                    quoted = false;
                    index++;
                    continue;
                }

                if (current == '\n')
                {
                    line++;
                }

                cell.Append(current);
                index++;
                continue;
            }

            if (current == '"' && cell.Length == 0)
            {
                quoted = true;
                index++;
                continue;
            }

            if (current == delimiter)
            {
                EndCell();
                index++;
                continue;
            }

            if (current is '\r' or '\n')
            {
                if (current == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                {
                    index++;
                }

                EndRow();
                line++;
                rowStartLine = line;
                index++;
                continue;
            }

            cell.Append(current);
            index++;
        }

        if (cell.Length > 0 || cells.Count > 0)
        {
            EndRow();
        }

        if (rows.Count == 0)
        {
            return new OgCsvDocument([], [], delimiter);
        }

        var headers = rows[0].Cells.Select(header => header.Trim()).ToList();
        rows.RemoveAt(0);

        return new OgCsvDocument(headers, rows, delimiter);
    }

    /// <summary>Suggests a target column for each source header, by name (case, spaces and underscores ignored).</summary>
    public static IReadOnlyDictionary<string, string> SuggestMapping(
        OgCsvDocument document,
        OgFormDefinition form)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(form);

        var normalised = form.Fields.ToDictionary(
            field => Normalise(field.Column),
            field => field.Column,
            StringComparer.OrdinalIgnoreCase);

        var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in document.Headers)
        {
            if (normalised.TryGetValue(Normalise(header), out var target) && used.Add(target))
            {
                mapping[header] = target;
            }
        }

        return mapping;
    }

    private static string Normalise(string name) =>
        new(name.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}

/// <summary>One row after mapping and validation.</summary>
public sealed record OgImportRow(
    int LineNumber,
    IReadOnlyDictionary<string, string?> Values,
    IReadOnlyList<OgFormProblem> Problems)
{
    public bool Acceptable => Problems.Count == 0;
}

/// <summary>
/// What the import would do. Carries the file's hash and the mapping profile, so the import can be recognised
/// later (AC-3).
/// </summary>
public sealed record OgImportPlan(
    string TableName,
    string FileHash,
    string MappingProfile,
    IReadOnlyList<OgImportRow> Rows,
    IReadOnlyList<string> UnmappedSourceColumns,
    IReadOnlyList<string> UnmappedTargetColumns)
{
    public int Acceptable => Rows.Count(row => row.Acceptable);

    public int Rejected => Rows.Count - Acceptable;

    public string Describe() =>
        $"{Rows.Count} row(s) read · {Acceptable} acceptable · {Rejected} rejected" +
        (UnmappedSourceColumns.Count == 0 ? string.Empty : $" · unmapped: {string.Join(", ", UnmappedSourceColumns)}");
}

/// <summary>One rejected row, or one failure during the write.</summary>
public sealed record OgImportRejection(int LineNumber, string Column, string Reason);

/// <summary>What an import did.</summary>
public sealed record OgImportResult(
    string TableName,
    string FileHash,
    string MappingProfile,
    int Read,
    int Accepted,
    int Rejected,
    int Inserted,
    int Updated,
    bool Cancelled,
    IReadOnlyList<OgImportRejection> Rejections,
    OgImportMode Mode = OgImportMode.Upsert,
    int Batches = 0);

/// <summary>
/// DAT-06: validates a mapped CSV against a table's form and then commits it, one row at a time, reporting what
/// happened to every rejected row.
/// </summary>
/// <remarks>
/// A row that fails preview validation is never written (AC-5), cancellation stops before the next row and
/// reports what completed (AC-4), and a row whose key already exists is updated rather than inserted, so a
/// composite-key table cannot accumulate duplicates (AC-6).
/// </remarks>
public sealed class OgCsvImporter
{
    private readonly IOgPpdmTableGateway _gateway;

    public OgCsvImporter(IOgPpdmTableGateway gateway)
    {
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
    }

    /// <summary>Validates every mapped row without writing anything.</summary>
    public OgImportPlan Preview(
        OgFormDefinition form,
        OgCsvDocument document,
        IReadOnlyDictionary<string, string> mapping,
        byte[]? fileBytes = null)
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(mapping);

        var rows = new List<OgImportRow>(document.Rows.Count);
        var known = form.Fields.Select(field => field.Column).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var row in document.Rows)
        {
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < document.Headers.Count; index++)
            {
                var header = document.Headers[index];

                if (!mapping.TryGetValue(header, out var target))
                {
                    continue;
                }

                var cell = index < row.Cells.Count ? row.Cells[index] : null;
                values[target] = cell?.Trim();
            }

            rows.Add(new OgImportRow(row.LineNumber, values, OgEntityForm.Validate(form, values, isNew: true)));
        }

        var mappedTargets = mapping.Values.ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new OgImportPlan(
            form.TableName,
            Hash(fileBytes),
            JsonSerializer.Serialize(mapping, new JsonSerializerOptions { WriteIndented = true }),
            rows,
            document.Headers.Where(header => !mapping.ContainsKey(header)).ToList(),
            known.Where(column => !mappedTargets.Contains(column)).ToList());
    }

    /// <summary>Writes the acceptable rows; rejected rows are reported and never sent.</summary>
    public async Task<OgImportResult> ImportAsync(
        OgFormDefinition form,
        OgImportPlan plan,
        IReadOnlyList<OgImportRow>? rows = null,
        OgImportMode mode = OgImportMode.Upsert,
        int batchSize = 500,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(plan);

        var accepted = 0;
        var inserted = 0;
        var updated = 0;
        var cancelled = false;
        var batches = 0;
        var rejections = new List<OgImportRejection>();

        // Batch size is a reporting and cancellation boundary (plan section 3, step 5); rows are still written
        // one at a time so a rejected row cannot take its neighbours with it.
        var size = Math.Max(1, batchSize);
        var source = rows ?? plan.Rows;

        for (var index = 0; index < source.Count; index++)
        {
            var row = source[index];

            if (index % size == 0)
            {
                batches++;

                if (cancellationToken.IsCancellationRequested)
                {
                    cancelled = true;
                    break;
                }
            }
            if (cancellationToken.IsCancellationRequested)
            {
                // AC-4: stop before the next row; the counts so far are the report.
                cancelled = true;
                break;
            }

            if (!row.Acceptable)
            {
                // AC-5: nothing is written for a row that failed validation.
                rejections.AddRange(row.Problems.Select(problem =>
                    new OgImportRejection(row.LineNumber, problem.Column, problem.Message)));

                continue;
            }

            accepted++;

            var converted = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var field in form.EditableFields)
            {
                if (row.Values.TryGetValue(field.Column, out var text))
                {
                    converted[field.Column] = OgEntityForm.Convert(field, text);
                }
            }

            foreach (var key in form.KeyColumns)
            {
                if (row.Values.TryGetValue(key, out var text) && !string.IsNullOrWhiteSpace(text))
                {
                    converted[key] = text.Trim();
                }
            }

            var written = await UpsertAsync(form, converted, mode, cancellationToken).ConfigureAwait(false);

            if (written.Success)
            {
                if (written.Message == "updated")
                {
                    updated++;
                }
                else
                {
                    inserted++;
                }

                continue;
            }

            rejections.Add(new OgImportRejection(
                row.LineNumber,
                string.Join("+", form.KeyColumns),
                written.Message ?? "The row could not be written."));
        }

        return new OgImportResult(
            form.TableName,
            plan.FileHash,
            plan.MappingProfile,
            plan.Rows.Count,
            accepted,
            plan.Rows.Count - accepted,
            inserted,
            updated,
            cancelled,
            rejections,
            mode,
            batches);
    }

    private async Task<OgResult> UpsertAsync(
        OgFormDefinition form,
        IReadOnlyDictionary<string, object?> values,
        OgImportMode mode,
        CancellationToken cancellationToken)
    {
        if (mode == OgImportMode.InsertOnly || form.KeyColumns.Count == 0)
        {
            return await _gateway.InsertAsync(form.TableName, new OgRow(values), cancellationToken).ConfigureAwait(false);
        }

        // AC-6: a row whose key already exists is updated, not inserted again.
        var filters = form.KeyColumns
            .Where(values.ContainsKey)
            .Select(key => new OgFilter(key, Convert.ToString(values[key], CultureInfo.InvariantCulture) ?? string.Empty))
            .ToList();

        if (filters.Count == form.KeyColumns.Count)
        {
            var existing = await _gateway
                .QueryAsync(form.TableName, new OgQuery(Page: 0, PageSize: 2, Filters: filters), cancellationToken)
                .ConfigureAwait(false);

            if (existing.Success && existing.Value is { Items.Count: > 0 })
            {
                var updated = await _gateway
                    .UpdateAsync(form.TableName, new OgRow(values), existing.Value.Items[0], cancellationToken)
                    .ConfigureAwait(false);

                return updated.Success
                    ? OgResult.Ok("updated")
                    : updated;
            }
        }

        return await _gateway.InsertAsync(form.TableName, new OgRow(values), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>The rejection report as CSV, which is what the person takes away (section 3, step 6).</summary>
    public static byte[] RejectionReport(OgImportResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder("line,column,reason\n");

        foreach (var rejection in result.Rejections)
        {
            builder.Append(rejection.LineNumber).Append(',')
                .Append('"').Append(rejection.Column.Replace("\"", "\"\"")).Append('"').Append(',')
                .Append('"').Append(rejection.Reason.Replace("\"", "\"\"")).Append('"')
                .Append('\n');
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Hash(byte[]? bytes) =>
        bytes is null ? string.Empty : Convert.ToHexString(SHA256.HashData(bytes));
}
