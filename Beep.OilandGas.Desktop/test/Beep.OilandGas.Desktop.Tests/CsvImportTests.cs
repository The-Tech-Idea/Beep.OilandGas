using System.Diagnostics;
using System.Text;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-06: the reader honours quotes and newlines, mapping is suggested from the header, every rejected row
/// reports line/column/reason, nothing is written for an invalid row, cancelling stops the run, the file hash and
/// mapping profile travel with the import, and a key that already exists is updated rather than duplicated.
/// </summary>
public sealed class CsvImportTests
{
    private static OgFormDefinition Form() => OgEntityForm.Build(
        "T_WELL",
        [
            new("WELL_ID", typeof(string), AllowNull: false, MaxLength: 20),
            new("WELL_NAME", typeof(string), AllowNull: false, MaxLength: 40),
            new("DEPTH_MD", typeof(decimal?), AllowNull: true, Scale: 2),
            new("SPUD_DATE", typeof(DateTime?), AllowNull: true),
        ],
        ["WELL_ID"]);

    private static OgFormDefinition CompositeForm() => OgEntityForm.Build(
        "T_WELL",
        [
            new("WELL_ID", typeof(string), AllowNull: false, MaxLength: 20),
            new("WELL_NAME", typeof(string), AllowNull: false, MaxLength: 40),
            new("DEPTH_MD", typeof(decimal?), AllowNull: true, Scale: 2),
            new("SPUD_DATE", typeof(DateTime?), AllowNull: false),
        ],
        ["WELL_ID", "SPUD_DATE"]);

    [Fact]
    public void The_reader_honours_quotes_embedded_newlines_and_a_bom()
    {
        var csv = "\uFEFFWELL_ID,WELL_NAME,DEPTH_MD\n" +
                  "W-1,\"Alpha, North\",10.5\n" +
                  "W-2,\"Line\nbreak\",11.25\n" +
                  "W-3,\"Quote \"\"inside\"\"\",12\n";

        var document = OgCsv.Parse(csv);

        Assert.Equal(["WELL_ID", "WELL_NAME", "DEPTH_MD"], document.Headers);
        Assert.Equal(3, document.Count);
        Assert.Equal("Alpha, North", document.Rows[0].Cells[1]);
        Assert.Equal("Line\nbreak", document.Rows[1].Cells[1]);
        Assert.Equal("Quote \"inside\"", document.Rows[2].Cells[1]);

        // The line number is where the row starts, so a person can find it in an editor.
        Assert.Equal(2, document.Rows[0].LineNumber);
        Assert.Equal(3, document.Rows[1].LineNumber);
    }

    [Fact]
    public void A_row_with_fewer_cells_is_kept_and_validated_rather_than_padded()
    {
        var document = OgCsv.Parse("A,B\n1\n2,3,4\n");

        Assert.Equal(2, document.Count);
        Assert.Single(document.Rows[0].Cells);
        Assert.Equal(3, document.Rows[1].Cells.Count);
    }

    [Fact]
    public void Mapping_is_suggested_ignoring_case_spaces_and_underscores()
    {
        var document = OgCsv.Parse("Well Id,Well Name,depth md,EXTRA\n1,Alpha,10,zzz\n");
        var mapping = OgCsv.SuggestMapping(document, Form());

        Assert.Equal("WELL_ID", mapping["Well Id"]);
        Assert.Equal("WELL_NAME", mapping["Well Name"]);
        Assert.Equal("DEPTH_MD", mapping["depth md"]);
        Assert.False(mapping.ContainsKey("EXTRA"));
    }

    [Fact]
    public void Every_rejected_row_reports_its_line_its_column_and_the_reason()
    {
        var document = OgCsv.Parse("WELL_ID,WELL_NAME,DEPTH_MD\n1,,\n2,Bravo,not-a-number\n");
        var form = Form();

        var plan = new OgCsvImporter(new CountingGateway()).Preview(
            form,
            document,
            OgCsv.SuggestMapping(document, form),
            Encoding.UTF8.GetBytes("x"));

        Assert.Equal(2, plan.Rows.Count);
        Assert.Equal(0, plan.Acceptable);
        Assert.Equal(2, plan.Rejected);

        var first = plan.Rows[0].Problems.Single();
        Assert.Equal("WELL_NAME", first.Column);
        Assert.Contains("Well Name", first.Message, StringComparison.Ordinal);

        var second = plan.Rows[1].Problems.Single();
        Assert.Equal("DEPTH_MD", second.Column);
        Assert.Contains("number", second.Message, StringComparison.OrdinalIgnoreCase);

        // The plan keeps the hashes so the import can be recognised later (AC-3).
        Assert.Equal(64, plan.FileHash.Length);
        Assert.Contains("WELL_NAME", plan.MappingProfile, StringComparison.Ordinal);
        // The CSV has no SPUD_DATE column, so that target stays unmapped and is named.
        Assert.Contains("SPUD_DATE", plan.UnmappedTargetColumns);
    }

    [Fact]
    public async Task A_thousand_rows_import_with_correct_accepted_and_rejected_counts()
    {
        var builder = new StringBuilder("WELL_ID,WELL_NAME,DEPTH_MD\n");

        for (var index = 0; index < 1_000; index++)
        {
            // Every tenth row has a name that is too long, so the rejected count is exact.
            var name = index % 10 == 0 ? new string('x', 41) : $"W{index}";
            builder.Append($"W-{index},{name},{index / 10.0}\n");
        }

        var document = OgCsv.Parse(builder.ToString());
        var form = Form();
        var gateway = new CountingGateway();
        var importer = new OgCsvImporter(gateway);
        var plan = importer.Preview(form, document, OgCsv.SuggestMapping(document, form));

        var result = await importer.ImportAsync(form, plan);

        // AC-1
        Assert.Equal(1_000, result.Read);
        Assert.Equal(900, result.Accepted);
        Assert.Equal(100, result.Rejected);
        Assert.Equal(900, result.Inserted);
        Assert.Equal(900, gateway.Inserts);
        Assert.Equal(100, result.Rejections.Count);
        Assert.All(result.Rejections, rejection => Assert.Equal("WELL_NAME", rejection.Column));
    }

    [Fact]
    public async Task No_row_is_written_when_its_preview_validation_failed()
    {
        var document = OgCsv.Parse("WELL_ID,WELL_NAME\n1,\n2,Bravo\n");
        var form = Form();
        var gateway = new CountingGateway();
        var importer = new OgCsvImporter(gateway);
        var plan = importer.Preview(form, document, OgCsv.SuggestMapping(document, form));

        var result = await importer.ImportAsync(form, plan);

        // AC-5: only the valid row reached the gateway.
        Assert.Equal(1, gateway.Inserts);
        Assert.Equal(1, result.Inserted);
        Assert.Single(result.Rejections);
        Assert.Equal(2, result.Rejections[0].LineNumber);
    }

    [Fact]
    public async Task Cancelling_stops_before_the_next_row_and_reports_what_completed()
    {
        var builder = new StringBuilder("WELL_ID,WELL_NAME\n");

        for (var index = 0; index < 50; index++)
        {
            builder.Append($"W-{index},Name{index}\n");
        }

        var document = OgCsv.Parse(builder.ToString());
        var form = Form();
        var gateway = new CountingGateway();
        var importer = new OgCsvImporter(gateway);
        var plan = importer.Preview(form, document, OgCsv.SuggestMapping(document, form));

        using var cancellation = new CancellationTokenSource();

        // Cancel from the tenth inserted row onward.
        gateway.AfterInsert = count => { if (count == 10) cancellation.Cancel(); };

        var result = await importer.ImportAsync(form, plan, cancellationToken: cancellation.Token);

        // AC-4: the run stopped and says so; the rows before the cancel are counted.
        Assert.True(result.Cancelled);
        Assert.Equal(10, result.Inserted);
        Assert.Equal(50, result.Read);
        Assert.Equal(10, gateway.Inserts);
    }

    [Fact]
    public async Task A_row_whose_key_exists_is_updated_not_duplicated()
    {
        var document = OgCsv.Parse("WELL_ID,WELL_NAME,SPUD_DATE\nW-1,Alpha,2026-01-01\n");
        var form = CompositeForm();
        var gateway = new CountingGateway
        {
            Existing = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["WELL_ID"] = "W-1",
                ["SPUD_DATE"] = new DateTime(2026, 1, 1),
                ["WELL_NAME"] = "Old",
            },
        };

        var importer = new OgCsvImporter(gateway);
        var plan = importer.Preview(form, document, OgCsv.SuggestMapping(document, form));

        var result = await importer.ImportAsync(form, plan);

        // AC-6: the composite key matched, so this is an update, and no insert was attempted.
        Assert.Equal(0, result.Inserted);
        Assert.Equal(1, result.Updated);
        Assert.Equal(1, gateway.Updates);
        Assert.Equal(0, gateway.Inserts);
    }

    [Fact]
    public void The_rejection_report_is_csv_naming_the_line_and_column()
    {
        var result = new OgImportResult(
            "T_WELL", "hash", "{}", 3, 2, 1, 2, 0, false,
            [new OgImportRejection(4, "WELL_NAME", "Well Name is required.")]);

        var text = Encoding.UTF8.GetString(OgCsvImporter.RejectionReport(result));

        Assert.StartsWith("line,column,reason", text, StringComparison.Ordinal);
        Assert.Contains("4,\"WELL_NAME\",\"Well Name is required.\"", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Insert_only_mode_ignores_an_existing_key()
    {
        var document = OgCsv.Parse("WELL_ID,WELL_NAME,SPUD_DATE\nW-1,Alpha,2026-01-01\n");
        var form = CompositeForm();
        var gateway = new CountingGateway
        {
            Existing = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["WELL_ID"] = "W-1",
                ["SPUD_DATE"] = new DateTime(2026, 1, 1),
            },
        };

        var importer = new OgCsvImporter(gateway);
        var plan = importer.Preview(form, document, OgCsv.SuggestMapping(document, form));

        var result = await importer.ImportAsync(form, plan, mode: OgImportMode.InsertOnly);

        // The caller asked for inserts, so an existing key is not looked up.
        Assert.Equal(1, result.Inserted);
        Assert.Equal(0, result.Updated);
        Assert.Equal(1, gateway.Inserts);
        Assert.Equal(0, gateway.Updates);
        Assert.Equal(OgImportMode.InsertOnly, result.Mode);
    }

    [Fact]
    public async Task The_batch_size_decides_how_many_batches_are_reported()
    {
        var builder = new StringBuilder("WELL_ID,WELL_NAME\n");

        for (var index = 0; index < 250; index++)
        {
            builder.Append($"W-{index},Name{index}\n");
        }

        var document = OgCsv.Parse(builder.ToString());
        var form = Form();
        var gateway = new CountingGateway();
        var importer = new OgCsvImporter(gateway);
        var plan = importer.Preview(form, document, OgCsv.SuggestMapping(document, form));

        var result = await importer.ImportAsync(form, plan, batchSize: 100);

        Assert.Equal(250, result.Inserted);
        Assert.Equal(3, result.Batches);
    }

    [Theory]
    [InlineData("caf\u00E9", OgCsvEncoding.Utf8)]
    [InlineData("caf\u00E9", OgCsvEncoding.Latin1)]
    public void The_encoding_override_decodes_single_byte_text(string expected, OgCsvEncoding encoding)
    {
        var bytes = encoding == OgCsvEncoding.Utf8
            ? Encoding.UTF8.GetBytes(expected)
            : Encoding.Latin1.GetBytes(expected);

        Assert.Equal(expected, OgCsv.Decode(bytes, encoding));

        // The same single-byte file read as UTF-8 is not the same text - which is why the choice exists.
        if (encoding == OgCsvEncoding.Latin1)
        {
            Assert.NotEqual(expected, OgCsv.Decode(bytes, OgCsvEncoding.Utf8));
        }
    }
    private sealed class CountingGateway : IOgPpdmTableGateway
    {
        public int Inserts { get; private set; }

        public int Updates { get; private set; }

        public IReadOnlyDictionary<string, object?>? Existing { get; set; }

        public Action<int>? BeforeInsert { get; set; }

        public Action<int>? AfterInsert { get; set; }

        public Task<OgResult> InsertAsync(string table, OgRow row, CancellationToken cancellationToken = default)
        {
            BeforeInsert?.Invoke(Inserts + 1);

            Inserts++;
            AfterInsert?.Invoke(Inserts);
            return Task.FromResult(OgResult.Ok("inserted"));
        }

        public Task<OgResult> UpdateAsync(string table, OgRow row, OgRow original, CancellationToken cancellationToken = default)
        {
            Updates++;
            return Task.FromResult(OgResult.Ok("updated"));
        }

        public Task<OgResult> DeleteAsync(string table, OgRow row, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult.Ok("deleted"));

        public Task<OgResult<OgPage<OgRow>>> QueryAsync(string table, OgQuery query, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgPage<OgRow>>.Ok(
                Existing is null
                    ? new OgPage<OgRow>([], 0, query.PageSize, 0, 20_000)
                    : new OgPage<OgRow>([new OgRow(Existing)], 0, query.PageSize, 1, 20_000)));

        public Task<OgResult<IReadOnlyList<string>>> KeyColumnsAsync(string table, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<IReadOnlyList<string>>.Ok(["WELL_ID"]));

        public Task<OgResult<byte[]>> ExportAsync(string table, OgQuery query, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<byte[]>.Ok([]));

        public Task<OgResult<OgFormDefinition>> FormAsync(string table, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgFormDefinition>.Ok(Form()));
    }
}
