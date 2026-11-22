using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Reporting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// RPT-01: the report catalogue, the parameter form a definition generates, the paginated preview, the template editor and the exports.
/// </summary>
/// <remarks>
/// A report is a definition plus a source that reads one table; the document is text with <c>{{Token}}</c> markers, rendered once and
/// paginated once, so the preview, the HTML, the table exports and the PDF are the same rendering (AC-1, AC-5). An unknown token is
/// named and nothing is rendered (AC-2); an empty result prints a sentence rather than an empty page (AC-3); the area's read permission
/// is the gate, and an unreadable permission source refuses rather than grants (AC-4); and a template save is a new version with the
/// default chosen explicitly (AC-6).
/// </remarks>
public sealed class ReportsPageView : OgPageBase
{
    public const string Route = "reports";

    private readonly OgApplication _application;
    private readonly IOgReports _reports;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _parameters;
    private readonly TextBlock _preview;
    private readonly TextBlock _tokens;
    private readonly TextBlock _templates;
    private readonly TextBlock _export;
    private readonly TextBlock _warnings;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _paramText;
    private readonly TextBox _templateBody;
    private readonly TextBox _templateName;
    private readonly TextBox _exportPath;
    private readonly ComboBox _templateChoice;
    private readonly ComboBox _format;
    private readonly CheckBox _makeDefault;
    private readonly DataGrid _reportGrid;
    private readonly DataGrid _tableGrid;
    private readonly Button _loadButton;
    private readonly Button _runButton;
    private readonly Button _pageButton;
    private readonly Button _saveTemplateButton;
    private readonly Button _exportButton;

    private IReadOnlyList<OgReportDefinition> _catalogue = [];
    private OgReportResult? _result;
    private int _page;

    public ReportsPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _reports = application.Reports;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.FontWeight = FontWeights.SemiBold;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _parameters = Body(string.Empty);
        _parameters.TextWrapping = TextWrapping.Wrap;
        _parameters.Margin = new Thickness(0, 0, 0, 4);

        _preview = Body(string.Empty);
        _preview.FontFamily = new System.Windows.Media.FontFamily("Consolas");
        _preview.TextWrapping = TextWrapping.NoWrap;
        _preview.Margin = new Thickness(0, 4, 0, 4);

        _tokens = Body(string.Empty);
        _tokens.TextWrapping = TextWrapping.Wrap;
        _tokens.Margin = new Thickness(0, 0, 0, 4);

        _templates = Body(string.Empty);
        _templates.TextWrapping = TextWrapping.Wrap;
        _templates.Margin = new Thickness(0, 0, 0, 4);

        _export = Body(string.Empty);
        _export.TextWrapping = TextWrapping.Wrap;
        _export.Margin = new Thickness(0, 4, 0, 4);

        _warnings = Body(string.Empty);
        _warnings.TextWrapping = TextWrapping.Wrap;
        _warnings.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _paramText = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 56,
            MaxHeight = 110,
            MinWidth = 460,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "One parameter per line: key = value - the form above lists the keys the report takes",
        };

        Named(_paramText, "Parameters");

        _templateBody = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 90,
            MaxHeight = 160,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "The template body: text with {{Token}} markers, and {{Table}} where the rows go",
        };

        Named(_templateBody, "Template body");

        _templateName = Field("Template name", 160);
        _exportPath = Field("Export path", 320);

        _templateChoice = new ComboBox { MinWidth = 200, Margin = new Thickness(0, 0, 8, 0) };
        Named(_templateChoice, "Template");

        _format = new ComboBox { MinWidth = 90, Margin = new Thickness(0, 0, 8, 0) };
        _format.ItemsSource = OgReportRules.Formats.ToList();
        _format.SelectedIndex = 0;
        Named(_format, "Export format");

        _makeDefault = new CheckBox { Content = "Make this the default", Margin = new Thickness(0, 0, 12, 0) };
        Named(_makeDefault, "Make this the default");

        _reportGrid = Grid("Reports", ("Key", "Key", 150), ("Name", "Name", 200), ("Area", "Area", 110),
            ("Permission", "Permission", 120));

        _reportGrid.SelectionChanged += (_, _) => ShowSelected();

        _tableGrid = Grid("Rows", ("Column", "Header", 180), ("Key", "Key", 120));

        _loadButton = Button("Load the catalogue", (_, _) => Load());
        _runButton = Button("Run", (_, _) => _ = RunAsync());
        _pageButton = Button("Next page", (_, _) => NextPage());
        _saveTemplateButton = Button("Save template", (_, _) => _ = SaveTemplateAsync());
        _exportButton = Button("Export", (_, _) => _ = ExportAsync());

        var catalogueBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        catalogueBar.Children.Add(_loadButton);
        catalogueBar.Children.Add(_runButton);

        var exportBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        exportBar.Children.Add(Caption("Template"));
        exportBar.Children.Add(_templateChoice);
        exportBar.Children.Add(Caption("Format"));
        exportBar.Children.Add(_format);
        exportBar.Children.Add(Caption("To"));
        exportBar.Children.Add(_exportPath);
        exportBar.Children.Add(_exportButton);
        exportBar.Children.Add(_pageButton);

        var templateBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        templateBar.Children.Add(Caption("Name"));
        templateBar.Children.Add(_templateName);
        templateBar.Children.Add(_makeDefault);
        templateBar.Children.Add(_saveTemplateButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(catalogueBar);
        panel.Children.Add(Caption("The catalogue"));
        panel.Children.Add(_reportGrid);
        panel.Children.Add(_parameters);
        panel.Children.Add(Caption("Parameters"));
        panel.Children.Add(_paramText);
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("Preview"));
        panel.Children.Add(_preview);
        panel.Children.Add(Caption("Rows"));
        panel.Children.Add(_tableGrid);
        panel.Children.Add(Caption("Tokens the template may use"));
        panel.Children.Add(_tokens);
        panel.Children.Add(exportBar);
        panel.Children.Add(_export);
        panel.Children.Add(Caption("Templates"));
        panel.Children.Add(_templates);
        panel.Children.Add(templateBar);
        panel.Children.Add(_templateBody);
        panel.Children.Add(_warnings);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Load the catalogue and choose a report.");
    }

    public IReadOnlyList<OgReportDefinition> Catalogue => _catalogue;

    public OgReportResult? Result => _result;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string ParametersText => _parameters.Text;

    public string PreviewText => _preview.Text;

    public string TokensText => _tokens.Text;

    public string TemplatesText => _templates.Text;

    public string ExportText => _export.Text;

    public string WarningsText => _warnings.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public int PageNumber => _page + 1;

    public int PageCount => _result?.PageCount ?? 0;

    public OgReportDefinition? ChosenReport => _reportGrid.SelectedItem as OgReportDefinition;

    public bool Load()
    {
        _header.Text = "Reporting · reports";

        var listed = _reports.CatalogueAsync().GetAwaiter().GetResult();

        if (!listed.Success || listed.Value is null)
        {
            LastFailed = true;
            _failure.Text = listed.Message ?? "The catalogue could not be read.";
            ShowState(OgViewState.Failed, "The catalogue is not shown.", listed.Reference);
            return false;
        }

        LastFailed = false;
        _catalogue = listed.Value;

        _reportGrid.ItemsSource = _catalogue.ToList();
        _reportGrid.SelectedItem = _catalogue.FirstOrDefault();

        var templates = _reports.TemplatesAsync().GetAwaiter().GetResult();

        ShowSelected();
        ShowTemplates(templates.Success ? templates.Value ?? [] : []);

        _refreshed.Text = _catalogue.Count.ToString(CultureInfo.InvariantCulture) + " report(s) in " +
                          _catalogue.Select(report => report.Area).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                              .ToString(CultureInfo.InvariantCulture) + " area(s)";

        _failure.Text = string.Empty;
        ShowState(_catalogue.Count > 0 ? OgViewState.Ready : OgViewState.Empty, _refreshed.Text);

        return _catalogue.Count > 0;
    }

    public async Task<bool> RunAsync()
    {
        var report = ChosenReport;

        if (report is null)
        {
            LastFailed = true;
            _failure.Text = "Select a report first; the catalogue runs one at a time.";
            return false;
        }

        var parameters = Parameters(_paramText.Text, out var problem);

        if (problem is not null)
        {
            LastFailed = true;
            _failure.Text = problem;
            return false;
        }

        var ran = await _reports
            .RunAsync(report.Key, parameters, _application.Permissions,
                _templateChoice.SelectedItem as string is "(generated layout)" or null
                    ? null
                    : _templateChoice.SelectedItem as string)
            .ConfigureAwait(true);

        if (!ran.Success || ran.Value is null)
        {
            LastFailed = true;
            _result = null;
            _preview.Text = string.Empty;
            _tableGrid.ItemsSource = null;
            _failure.Text = ran.Message ?? "The report was refused.";
            ShowState(OgViewState.Failed, "The report is not shown.", ran.Reference);
            return false;
        }

        LastFailed = false;
        _result = ran.Value;
        _page = 0;

        Apply(ran.Value);

        _failure.Text = string.Empty;
        _notice.Text = ran.Value.Empty
            ? "The report ran and found no data for these parameters."
            : "Report rendered.";

        return true;
    }

    public bool NextPage()
    {
        if (_result is null || _result.PageCount == 0)
        {
            return false;
        }

        _page = (_page + 1) % _result.PageCount;
        ShowPage();

        return true;
    }

    public async Task<bool> SaveTemplateAsync()
    {
        var saved = await _reports
            .SaveTemplateAsync(
                _templateName.Text?.Trim() ?? string.Empty,
                _templateBody.Text ?? string.Empty,
                _makeDefault.IsChecked == true,
                _application.Session.UserId)
            .ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _failure.Text = saved.Message ?? "The template was not saved.";
            return false;
        }

        LastFailed = false;

        _application.AuditLog.Add("reporting.template.write", "reporting", saved.Value.Name, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = "Template '" + saved.Value.Name + "' saved as version " +
                       saved.Value.Version.ToString(CultureInfo.InvariantCulture) +
                       (saved.Value.IsDefault ? " and made the default." : ".");

        var templates = await _reports.TemplatesAsync().ConfigureAwait(true);

        ShowTemplates(templates.Success ? templates.Value ?? [] : []);

        return true;
    }

    public async Task<bool> ExportAsync()
    {
        if (_result is null)
        {
            LastFailed = true;
            _failure.Text = "There is no report to export: run one first.";
            return false;
        }

        var format = _format.SelectedItem as string ?? OgReportRules.Formats[0];
        var path = string.IsNullOrWhiteSpace(_exportPath.Text)
            ? Path.Combine(OgPaths.ExportsDirectory,
                "report-" + _result.Key + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) +
                "." + format)
            : _exportPath.Text!.Trim();

        var exported = await _reports.ExportAsync(_result, format, path).ConfigureAwait(true);

        if (!exported.Success || exported.Value is null)
        {
            LastFailed = true;
            _export.Text = exported.Message ?? "The report was not written.";
            _failure.Text = exported.Message ?? "The report was not written.";
            ShowState(OgViewState.Failed, "The report is not exported.", exported.Reference);
            return false;
        }

        LastFailed = false;
        _exportPath.Text = exported.Value.Path;
        _export.Text = exported.Value.Message + Environment.NewLine + exported.Value.Summary;
        _failure.Text = string.Empty;

        _application.AuditLog.Add("reporting.export", "reporting", exported.Value.Path, "ok",
            _application.Session.UserId);

        _notice.Text = "Report written to " + exported.Value.Path + ".";

        return true;
    }

    /// <summary>
    /// The parameter form: one <c>key = value</c> line each, which is how a definition's parameters are entered without a control per
    /// report. A line that cannot be read is refused naming it.
    /// </summary>
    public static Dictionary<string, string> Parameters(string? text, out string? problem)
    {
        problem = null;

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in (text ?? string.Empty).Split('\n', StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var split = line.IndexOf('=');

            if (split <= 0)
            {
                problem = "The line '" + line + "' is not a parameter: write 'key = value'.";
                return parameters;
            }

            parameters[line[..split].Trim()] = line[(split + 1)..].Trim();
        }

        return parameters;
    }

    private void Apply(OgReportResult result)
    {
        _summary.Text = result.SummaryText;
        _tokens.Text = result.TokensText;
        _warnings.Text = result.WarningsText;
        _refreshed.Text = result.GeneratedText;

        _tableGrid.ItemsSource = result.Table.Columns.ToList();

        ShowPage();

        ShowState(OgViewState.Ready, result.SummaryText);
    }

    private void ShowPage()
    {
        if (_result is null)
        {
            _preview.Text = string.Empty;
            return;
        }

        var page = _result.Pages[Math.Clamp(_page, 0, _result.PageCount - 1)];

        _preview.Text = page.Text;
        _summary.Text = _result.SummaryText + " · page " +
                        page.Number.ToString(CultureInfo.InvariantCulture) + " of " +
                        _result.PageCount.ToString(CultureInfo.InvariantCulture);
    }

    private void ShowSelected()
    {
        if (_reportGrid.SelectedItem is not OgReportDefinition report)
        {
            return;
        }

        _parameters.Text = report.Describe() + Environment.NewLine + report.ParametersText;

        _templateName.Text = report.Key + "-default";

        // The parameters the report needs, offered as the lines to fill in.
        _paramText.Text = string.Join(Environment.NewLine,
            report.Parameters.Select(parameter =>
                parameter.Key + " = " + (parameter.DefaultValue ?? string.Empty)));
    }

    private void ShowTemplates(IReadOnlyList<OgReportTemplate> templates)
    {
        _templates.Text = templates.Count == 0
            ? "No template is stored, so reports are rendered with the generated layout - which says so at the top."
            : string.Join(Environment.NewLine, templates.Select(template => "· " + template.Describe()));

        var choice = new List<string> { "(generated layout)" };

        choice.AddRange(templates.Select(template => template.Name));

        _templateChoice.ItemsSource = choice;
        _templateChoice.SelectedIndex = templates.Count == 0 ? 0 : 1;

        var chosen = templates.FirstOrDefault(template => template.IsDefault);

        if (chosen is not null)
        {
            _templateBody.Text = chosen.Body;
        }
    }

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 180,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
        };

        foreach (var (header, path, width) in columns)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(path),
                Width = new DataGridLength(width),
            });
        }

        Named(grid, name);
        return grid;
    }

    private static TextBox Field(string name, double width)
    {
        var box = new TextBox
        {
            MinWidth = width,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = name,
        };

        Named(box, name);
        return box;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 6, 0, 0),
        FontSize = 11.5,
        FontWeight = FontWeights.SemiBold,
    };

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
}
