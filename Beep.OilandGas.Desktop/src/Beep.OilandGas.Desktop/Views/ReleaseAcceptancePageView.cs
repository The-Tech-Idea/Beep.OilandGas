using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Release;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// REL-05: the release acceptance checklist - thirty items, five computed from artefacts that exist and twenty-five attested by a person
/// with evidence - and the evidence pack.
/// </summary>
/// <remarks>
/// The page refuses to claim readiness on the strength of a build that compiled: an item with no artefact and no attestation is
/// <b>NotRun</b>, a check that fails stays failed, and an expired waiver reverts to a failure rather than quietly disappearing. Attesting
/// needs <c>release.attest</c> and waiving needs <c>release.waive</c> from a different person, which is the feature's own separation.
/// </remarks>
public sealed class ReleaseAcceptancePageView : OgPageBase
{
    public const string Route = "releaseacceptance";

    private readonly OgApplication _application;
    private readonly IOgReleaseAcceptance _release;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _blockers;
    private readonly TextBlock _plan;
    private readonly TextBlock _pack;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _evidence;
    private readonly TextBox _approver;
    private readonly TextBox _reason;
    private readonly TextBox _expires;
    private readonly TextBox _planFolder;
    private readonly TextBox _zipPath;
    private readonly ComboBox _attestItem;
    private readonly ComboBox _waiveItem;
    private readonly DataGrid _itemGrid;
    private readonly Button _evaluateButton;
    private readonly Button _attestButton;
    private readonly Button _waiveButton;
    private readonly Button _exportButton;

    private OgAcceptanceReport? _report;

    public ReleaseAcceptancePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _release = application.ReleaseAcceptance;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.FontWeight = FontWeights.SemiBold;
        _summary.FontSize = 14;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _blockers = Body(string.Empty);
        _blockers.TextWrapping = TextWrapping.Wrap;
        _blockers.Margin = new Thickness(0, 0, 0, 4);

        _plan = Body(string.Empty);
        _plan.TextWrapping = TextWrapping.Wrap;
        _plan.Margin = new Thickness(0, 0, 0, 4);

        _pack = Body(string.Empty);
        _pack.TextWrapping = TextWrapping.Wrap;
        _pack.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _evidence = new TextBox
        {
            MinWidth = 380,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = "The log, screenshot or test run that shows the item - a path under the plan folder, or a sentence naming it",
        };

        Named(_evidence, "Evidence");

        _approver = Field("Approver", 140);
        _reason = Field("Reason", 260);
        _expires = Field("Expires (yyyy-MM-dd)", 130);

        _expires.Text = DateTime.Now.AddMonths(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        _planFolder = Field("Plan folder", 340);
        _planFolder.Text = OgReleaseLocations.PlanFolder() ?? string.Empty;

        _zipPath = Field("Pack file", 340);
        _zipPath.Text = OgReleaseLocations.DefaultEvidencePackPath;

        _attestItem = new ComboBox { MinWidth = 460, Margin = new Thickness(0, 0, 8, 0) };
        _waiveItem = new ComboBox { MinWidth = 460, Margin = new Thickness(0, 0, 8, 0) };

        Named(_attestItem, "Item to attest");
        Named(_waiveItem, "Item to waive");

        _itemGrid = Grid("Release checklist", ("#", "NumberText", 40), ("Area", "Category", 120),
            ("Item", "Text", 400), ("Source", "Kind", 90), ("State", "StateText", 80),
            ("Detail", "Detail", 320), ("Evidence", "EvidenceText", 260), ("Attested by", "AttestedBy", 110));

        _evaluateButton = Button("Evaluate", (_, _) => Load());
        _attestButton = Button("Attest", (_, _) => _ = AttestAsync());
        _waiveButton = Button("Waive", (_, _) => _ = WaiveAsync());
        _exportButton = Button("Export the evidence pack", (_, _) => _ = ExportAsync());

        var evaluateBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        evaluateBar.Children.Add(_evaluateButton);
        evaluateBar.Children.Add(Caption("Attesting needs release.attest; waiving needs release.waive from a different person."));

        var attestBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        attestBar.Children.Add(Caption("Item"));
        attestBar.Children.Add(_attestItem);
        attestBar.Children.Add(Caption("Evidence"));
        attestBar.Children.Add(_evidence);
        attestBar.Children.Add(_attestButton);

        var waiveBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        waiveBar.Children.Add(Caption("Item"));
        waiveBar.Children.Add(_waiveItem);
        waiveBar.Children.Add(Caption("Approver"));
        waiveBar.Children.Add(_approver);
        waiveBar.Children.Add(Caption("Reason"));
        waiveBar.Children.Add(_reason);
        waiveBar.Children.Add(Caption("Expires"));
        waiveBar.Children.Add(_expires);
        waiveBar.Children.Add(_waiveButton);

        var packBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        packBar.Children.Add(Caption("Plan folder"));
        packBar.Children.Add(_planFolder);
        packBar.Children.Add(Caption("Pack file"));
        packBar.Children.Add(_zipPath);
        packBar.Children.Add(_exportButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(evaluateBar);
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("What blocks the release"));
        panel.Children.Add(_blockers);
        panel.Children.Add(Caption("Where the evidence is"));
        panel.Children.Add(_plan);
        panel.Children.Add(Caption("The checklist"));
        panel.Children.Add(_itemGrid);
        panel.Children.Add(Caption("Attest an item"));
        panel.Children.Add(attestBar);
        panel.Children.Add(Caption("Waive an item"));
        panel.Children.Add(waiveBar);
        panel.Children.Add(Caption("Evidence pack"));
        panel.Children.Add(packBar);
        panel.Children.Add(_pack);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Evaluate the checklist to see what a release still needs.");
    }

    public OgAcceptanceReport? Report => _report;

    public bool LastFailed { get; private set; }

    public bool Ready => _report?.Ready ?? false;

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string BlockerText => _blockers.Text;

    public string PlanText => _plan.Text;

    public string PackText => _pack.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public IReadOnlyList<OgAcceptanceItem> Items => _report?.Items ?? [];

    public bool Load()
    {
        _header.Text = "Administration · release acceptance";

        var evaluated = _release.EvaluateAsync().GetAwaiter().GetResult();

        if (!evaluated.Success || evaluated.Value is null)
        {
            LastFailed = true;
            _report = null;
            _itemGrid.ItemsSource = null;
            _summary.Text = string.Empty;
            _blockers.Text = string.Empty;
            _failure.Text = evaluated.Message ?? "The checklist could not be evaluated.";
            ShowState(OgViewState.Failed, "The checklist is not shown.", evaluated.Reference);
            return false;
        }

        LastFailed = false;
        _report = evaluated.Value;

        _summary.Text = evaluated.Value.SummaryText;
        _blockers.Text = evaluated.Value.BlockerText;
        _plan.Text = evaluated.Value.PlanText;
        _refreshed.Text = evaluated.Value.EvaluatedText;
        _failure.Text = string.Empty;

        _itemGrid.ItemsSource = evaluated.Value.Items.ToList();

        // Both item pickers offer the same thirty items, so an attestation and a waiver cannot name different lists.
        var choices = evaluated.Value.Items
            .Select(item => item.NumberText + " " + item.Text)
            .ToList();

        var keepAttest = _attestItem.SelectedIndex;
        var keepWaive = _waiveItem.SelectedIndex;

        _attestItem.ItemsSource = choices;
        _waiveItem.ItemsSource = choices;
        _attestItem.SelectedIndex = keepAttest >= 0 && keepAttest < choices.Count ? keepAttest : 0;
        _waiveItem.SelectedIndex = keepWaive >= 0 && keepWaive < choices.Count ? keepWaive : 0;

        if (string.IsNullOrWhiteSpace(_planFolder.Text))
        {
            _planFolder.Text = evaluated.Value.PlanFolder ?? string.Empty;
        }

        ShowState(evaluated.Value.Ready ? OgViewState.Ready : OgViewState.Failed, evaluated.Value.SummaryText,
            evaluated.Value.Ready ? null : evaluated.Value.BlockerText);

        return evaluated.Value.Ready;
    }

    public async Task<bool> AttestAsync()
    {
        var key = KeyOf(_attestItem);

        if (key is null)
        {
            LastFailed = true;
            _failure.Text = "Choose the item to attest.";
            return false;
        }

        var attested = await _release
            .AttestAsync(key, _evidence.Text ?? string.Empty, _application.Session.UserId, _application.Permissions)
            .ConfigureAwait(true);

        if (!attested.Success || attested.Value is null)
        {
            LastFailed = true;
            _failure.Text = attested.Message ?? "The attestation was refused.";
            ShowState(OgViewState.Failed, "The item is not attested.", attested.Reference);
            return false;
        }

        LastFailed = false;
        _failure.Text = string.Empty;

        _application.AuditLog.Add("release.attest", "release", key, "ok", _application.Session.UserId);

        _notice.Text = "Item " + attested.Value.NumberText + " is " + attested.Value.StateText + ".";

        Load();

        return attested.Value.State == OgAcceptanceState.Pass;
    }

    public async Task<bool> WaiveAsync()
    {
        var key = KeyOf(_waiveItem);

        if (key is null)
        {
            LastFailed = true;
            _failure.Text = "Choose the item to waive.";
            return false;
        }

        if (!DateTime.TryParse(_expires.Text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var expires))
        {
            LastFailed = true;
            _failure.Text = "The expiry is not a date: write it as yyyy-MM-dd.";
            return false;
        }

        var waived = await _release
            .WaiveAsync(key, _approver.Text ?? string.Empty, _reason.Text ?? string.Empty, expires.ToUniversalTime(),
                _application.Session.UserId, _application.Permissions)
            .ConfigureAwait(true);

        if (!waived.Success || waived.Value is null)
        {
            LastFailed = true;
            _failure.Text = waived.Message ?? "The waiver was refused.";
            ShowState(OgViewState.Failed, "The item is not waived.", waived.Reference);
            return false;
        }

        LastFailed = false;
        _failure.Text = string.Empty;

        _application.AuditLog.Add("release.waive", "release", key, "ok", _application.Session.UserId);

        _notice.Text = "Item " + waived.Value.NumberText + " is " + waived.Value.StateText +
                       (waived.Value.Waiver is { } waiver ? " - " + waiver.Describe() : string.Empty);

        Load();

        return waived.Value.State == OgAcceptanceState.Waived;
    }

    public async Task<bool> ExportAsync()
    {
        var exported = await _release
            .ExportEvidenceAsync(_planFolder.Text?.Trim() ?? string.Empty, _zipPath.Text?.Trim() ?? string.Empty,
                _application.Session.UserId)
            .ConfigureAwait(true);

        if (!exported.Success || exported.Value is null)
        {
            LastFailed = true;
            _pack.Text = exported.Message ?? "The evidence pack was not written.";
            _failure.Text = exported.Message ?? "The evidence pack was not written.";
            return false;
        }

        LastFailed = false;
        _zipPath.Text = exported.Value.Path;
        _pack.Text = exported.Value.Describe() + Environment.NewLine + exported.Value.MissingText;
        _failure.Text = string.Empty;

        _application.AuditLog.Add("release.evidence.pack", "release", exported.Value.Path, "ok",
            _application.Session.UserId);

        _notice.Text = "Evidence pack written to " + exported.Value.Path + ".";

        return exported.Value.Missing.Count == 0;
    }

    /// <summary>The item key behind a picker entry - the number is the stable part, so the text can be reworded.</summary>
    private string? KeyOf(ComboBox picker) =>
        picker.SelectedIndex >= 0 && _report is not null && picker.SelectedIndex < _report.Items.Count
            ? _report.Items[picker.SelectedIndex].Key
            : null;

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 200,
            MaxHeight = 420,
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
