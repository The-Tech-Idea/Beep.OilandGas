using System.Text;
using System.Windows;
using System.Windows.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// DAT-06: the CSV import wizard - choose a file, pick the table, preview the mapping and validation, then commit
/// with a per-row outcome.
/// </summary>
/// <remarks>
/// Preview and commit are separate calls: the preview writes nothing and a row that fails it is never sent
/// (AC-5). The commit reports inserted/updated/rejected and can be cancelled between rows (AC-4).
/// </remarks>
public sealed class CsvImportPageView : OgPageBase
{
    public const string Route = "csvimport";

    private readonly OgApplication _application;
    private readonly IOgPpdmTableGateway _gateway;
    private readonly OgCsvImporter _importer;
    private readonly TextBox _file;
    private readonly TextBox _table;
    private readonly TextBox _delimiter;
    private readonly ComboBox _mode;
    private readonly TextBox _batch;
    private readonly ComboBox _encoding;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _mapping;
    private readonly ListBox _problems;

    private CancellationTokenSource? _cancellation;

    public CsvImportPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _gateway = application.TableGateway;
        _importer = new OgCsvImporter(_gateway);

        _file = new TextBox { MinWidth = 320, Margin = new Thickness(0, 0, 8, 0) };
        _table = new TextBox { MinWidth = 160, Margin = new Thickness(0, 0, 8, 0) };
        _delimiter = new TextBox { Width = 40, Text = ",", Margin = new Thickness(0, 0, 8, 0) };

        Named(_file, "CSV file path");
        Named(_table, "Target table");
        Named(_delimiter, "Delimiter");

        _mode = new ComboBox { MinWidth = 130, Margin = new Thickness(0, 0, 8, 0) };
        _mode.ItemsSource = new[] { "Upsert", "Insert only" };
        _mode.SelectedIndex = 0;
        Named(_mode, "Insert mode");

        _batch = new TextBox { Width = 70, Text = "500", Margin = new Thickness(0, 0, 8, 0) };
        Named(_batch, "Batch size");

        _encoding = new ComboBox { MinWidth = 110, Margin = new Thickness(0, 0, 8, 0) };
        _encoding.ItemsSource = new[] { "UTF-8", "Latin-1" };
        _encoding.SelectedIndex = 0;
        Named(_encoding, "File encoding");

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _mapping = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 180,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Columns =
            {
                Text("Source column", nameof(OgImportMappingRow.Source), 220),
                Text("Target field", nameof(OgImportMappingRow.Target), 220),
                Text("Required", nameof(OgImportMappingRow.Required), 80),
            },
        };

        Named(_mapping, "Column mapping");

        _problems = new ListBox { MaxHeight = 160, Margin = new Thickness(0, 0, 0, 8), IsTabStop = true };
        Named(_problems, "Rejected rows");

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Caption("File"));
        toolbar.Children.Add(_file);
        toolbar.Children.Add(Caption("Table"));
        toolbar.Children.Add(_table);
        toolbar.Children.Add(Caption("Delimiter"));
        toolbar.Children.Add(_delimiter);
        toolbar.Children.Add(Caption("Mode"));
        toolbar.Children.Add(_mode);
        toolbar.Children.Add(Caption("Batch"));
        toolbar.Children.Add(_batch);
        toolbar.Children.Add(Caption("Encoding"));
        toolbar.Children.Add(_encoding);
        toolbar.Children.Add(Button("Preview", (_, _) => Preview()));
        toolbar.Children.Add(Button("Import", async (_, _) => await ImportAsync()));
        toolbar.Children.Add(Button("Cancel", (_, _) => _cancellation?.Cancel()));

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_summary);
        panel.Children.Add(_notice);
        panel.Children.Add(_mapping);
        panel.Children.Add(_problems);

        ShowContent(panel);

        if (_application.Workbench.Pending is { } pending)
        {
            _table.Text = pending.TableName;
        }
    }

    public OgImportPlan? Plan { get; private set; }

    public OgImportResult? Result { get; private set; }

    public IReadOnlyList<OgImportMappingRow> Mapping { get; private set; } = [];

    public string SummaryText => _summary.Text;

    public string NoticeText => _notice.Text;

    public string? RejectionPath { get; private set; }

    /// <summary>Parses the file, suggests a mapping and validates every row - writing nothing.</summary>
    public bool Preview()
    {
        var table = _table.Text.Trim();

        if (table.Length == 0)
        {
            _notice.Text = "Name the target table before previewing.";
            return false;
        }

        try
        {
            if (!File.Exists(_file.Text.Trim()))
            {
                _notice.Text = $"'{_file.Text.Trim()}' does not exist.";
                return false;
            }

            var bytes = File.ReadAllBytes(_file.Text.Trim());
            var document = OgCsv.Parse(OgCsv.Decode(bytes, ChosenEncoding()), Delimiter());

            var form = _gateway.FormAsync(table).GetAwaiter().GetResult();

            if (!form.Success || form.Value is null)
            {
                _notice.Text = form.Message ?? "The target table's form could not be read.";
                ShowState(OgViewState.Failed, "The target table could not be read.", form.Reference);
                return false;
            }

            var mapping = OgCsv.SuggestMapping(document, form.Value);
            Plan = _importer.Preview(form.Value, document, mapping, bytes);

            Mapping = mapping
                .Select(pair => new OgImportMappingRow(
                    pair.Key,
                    pair.Value,
                    form.Value.Fields.Any(f => f.Column == pair.Value && f.Required)))
                .ToList();

            if (Plan.UnmappedTargetColumns.Count > 0)
            {
                Mapping = Mapping
                    .Concat(Plan.UnmappedTargetColumns.Select(column => new OgImportMappingRow(
                        "(not in file)",
                        column,
                        form.Value.Fields.Any(f => f.Column == column && f.Required))))
                    .ToList();
            }

            _mapping.ItemsSource = Mapping;
            _summary.Text = Plan.Describe();

            var problems = Plan.Rows
                .Where(row => !row.Acceptable)
                .SelectMany(row => row.Problems.Select(problem => $"line {row.LineNumber}: {problem.Message}"))
                .Take(50)
                .ToList();

            _problems.ItemsSource = problems;

            _notice.Text = Plan.Rejected == 0
                ? "Every row is valid. Import writes them; nothing has been written yet."
                : $"{Plan.Rejected} row(s) would be rejected. They will not be written.";

            ShowState(OgViewState.Ready, string.Empty);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reading the CSV file",
                "nothing was previewed",
                FailureSeverity.Error);

            _notice.Text = $"The file could not be read. Reference: {reference}";
            return false;
        }
    }

    /// <summary>Writes the acceptable rows and reports what happened to the rest.</summary>
    public async Task<bool> ImportAsync()
    {
        if (Plan is null && !Preview())
        {
            return false;
        }

        var form = _gateway.FormAsync(_table.Text.Trim()).GetAwaiter().GetResult();

        if (!form.Success || form.Value is null || Plan is null)
        {
            _notice.Text = form.Message ?? "The import cannot start.";
            return false;
        }

        _cancellation = new CancellationTokenSource();

        var result = await _importer
            .ImportAsync(
                form.Value,
                Plan,
                mode: _mode.SelectedIndex == 1 ? OgImportMode.InsertOnly : OgImportMode.Upsert,
                batchSize: int.TryParse(_batch.Text.Trim(), out var batch) && batch > 0 ? batch : 500,
                cancellationToken: _cancellation.Token)
            .ConfigureAwait(true);

        Result = result;

        _summary.Text =
            $"{result.Read} read · {result.Accepted} accepted · {result.Inserted} inserted · " +
            $"{result.Updated} updated · {result.Rejected} rejected · {result.Mode} · {result.Batches} batch(es)" +
            (result.Cancelled ? " · cancelled" : string.Empty);

        if (result.Rejections.Count > 0)
        {
            try
            {
                Directory.CreateDirectory(OgPaths.ExportsDirectory);

                var path = Path.Combine(
                    OgPaths.ExportsDirectory,
                    $"import-rejected-{DateTime.Now:yyyyMMdd-HHmmss}.csv");

                File.WriteAllBytes(path, OgCsvImporter.RejectionReport(result));
                RejectionPath = path;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                var reference = _application.Diagnostics.ReportHandled(
                    ex,
                    "writing the rejection report",
                    "the report is not on disk",
                    FailureSeverity.Degraded);

                _notice.Text = $"The rejection report could not be written. Reference: {reference}";
            }
        }

        _problems.ItemsSource = result.Rejections
            .Take(50)
            .Select(rejection => $"line {rejection.LineNumber} · {rejection.Column}: {rejection.Reason}")
            .ToList();

        _notice.Text = result.Cancelled
            ? "Cancelled: the rows before the cancel were written and are counted."
            : result.Rejected == 0
                ? "Every row was written. The file hash and mapping are recorded with the import."
                : $"{result.Rejected} row(s) were rejected; see the report.";

        _application.AuditLog.Add(
            "data.import",
            "table",
            result.TableName,
            result.Rejected == 0 && !result.Cancelled ? "ok" : "partial",
            $"file={result.FileHash[..Math.Min(16, result.FileHash.Length)]} inserted={result.Inserted} updated={result.Updated} rejected={result.Rejected} cancelled={result.Cancelled}");

        return result.Rejected == 0 && !result.Cancelled;
    }

    private OgCsvEncoding ChosenEncoding() =>
        _encoding.SelectedIndex == 1 ? OgCsvEncoding.Latin1 : OgCsvEncoding.Utf8;

    private char Delimiter()
    {
        var text = _delimiter.Text.Trim();
        return text.Length == 1 && text[0] != '"' ? text[0] : ',';
    }

    private static Button Button(string text, RoutedEventHandler onClick)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(10, 3, 10, 3),
            Margin = new Thickness(0, 0, 8, 0),
        };

        button.Click += onClick;
        return button;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
    };

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new System.Windows.Data.Binding(path),
        Width = new DataGridLength(width),
    };
}

/// <summary>One line of the mapping grid.</summary>
public sealed record OgImportMappingRow(string Source, string Target, bool Required);
