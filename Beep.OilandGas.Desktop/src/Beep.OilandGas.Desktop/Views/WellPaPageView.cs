using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Decommissioning;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// DEC-02: a well's plug-and-abandonment programme - its barriers, their evidence, its leak paths and its closure.
/// </summary>
/// <remarks>
/// Completion is refused without evidence for every required barrier (AC-1); the required set comes from the owner's
/// permanent-abandonment template, not from this view (AC-2); an open leak path blocks completion (AC-3); a depth is recorded
/// and shown with its unit (AC-4); the schematic notice says the schematic is a visual aid and not the evidence (AC-5); and a
/// closure records who, when and what was verified (AC-6).
/// </remarks>
public sealed class WellPaPageView : OgPageBase
{
    public const string Route = "wellpa";

    private readonly OgApplication _application;
    private readonly IOgWellPa _wellPa;

    private readonly TextBlock _header;
    private readonly TextBlock _status;
    private readonly TextBlock _notice;
    private readonly TextBlock _checklist;
    private readonly TextBlock _schematic;
    private readonly TextBlock _failure;
    private readonly TextBlock _refreshed;
    private readonly TextBox _well;
    private readonly TextBox _plannedDate;
    private readonly TextBox _barrierEnvelope;
    private readonly TextBox _barrierType;
    private readonly TextBox _barrierTop;
    private readonly TextBox _barrierBase;
    private readonly TextBox _barrierUnit;
    private readonly TextBox _verification;
    private readonly TextBox _evidence;
    private readonly TextBox _leakDescription;
    private readonly TextBox _leakHow;
    private readonly TextBox _rationale;
    private readonly DataGrid _barrierGrid;
    private readonly DataGrid _requiredGrid;
    private readonly DataGrid _leakGrid;
    private readonly Button _loadButton;
    private readonly Button _planButton;
    private readonly Button _verifyButton;
    private readonly Button _addLeakButton;
    private readonly Button _addressLeakButton;
    private readonly Button _closeButton;

    private OgWellPa? _programme;
    private OgAbandonmentBarrier? _barrier;
    private OgLeakPath? _leakPath;

    public WellPaPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _wellPa = application.WellPa;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _status = Body(string.Empty);
        _status.TextWrapping = TextWrapping.Wrap;
        _status.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _checklist = Body(string.Empty);
        _checklist.TextWrapping = TextWrapping.Wrap;
        _checklist.Margin = new Thickness(0, 4, 0, 4);

        _schematic = Body(string.Empty);
        _schematic.TextWrapping = TextWrapping.Wrap;
        _schematic.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;
        _refreshed.Margin = new Thickness(0, 0, 0, 4);

        _well = Field("Well", 140);
        _plannedDate = Field("Planned date yyyy-MM-dd", 160);
        _barrierEnvelope = Field("Envelope", 100);
        _barrierType = Field("Barrier type", 140);
        _barrierTop = Field("Top depth", 90);
        _barrierBase = Field("Base depth", 90);
        _barrierUnit = Field("Depth unit", 90);
        _verification = Field("Verification method", 150);
        _evidence = Field("Evidence reference", 200);
        _leakDescription = Field("Leak path", 220);
        _leakHow = Field("How it was addressed", 220);
        _rationale = Field("Closure rationale", 220);

        _barrierGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 190,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Envelope", "Envelope", 100),
                Text("Type", "BarrierType", 130),
                Text("Depth", "DepthText", 150),
                Text("Evidence", "EvidenceRef", 160),
                Text("Verified by", "VerifiedBy", 110),
                Text("Counts", "IsVerifiedWithEvidence", 70),
            },
        };

        Named(_barrierGrid, "Barriers");
        _barrierGrid.SelectionChanged += (_, _) => SelectBarrier();

        _requiredGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Envelope", "Envelope", 110),
                Text("Required type", "BarrierType", 160),
                Text("Conditional", "Conditional", 90),
            },
        };

        Named(_requiredGrid, "Required by the owner's template");

        _leakGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Leak path", "Description", 240),
                Text("Status", "Status", 100),
                Text("Addressed how", "AddressedHow", 200),
                Text("By", "AddressedBy", 100),
            },
        };

        Named(_leakGrid, "Leak paths");
        _leakGrid.SelectionChanged += (_, _) => SelectLeakPath();

        _loadButton = Button("Load", (_, _) => Load());
        _planButton = Button("Plan programme", (_, _) => _ = PlanAsync());
        _verifyButton = Button("Verify barrier with evidence", (_, _) => _ = VerifyAsync());
        _addLeakButton = Button("Add leak path", (_, _) => _ = AddLeakAsync());
        _addressLeakButton = Button("Address leak path", (_, _) => _ = AddressLeakAsync());
        _closeButton = Button("Complete and close", (_, _) => _ = CloseAsync());

        var loadBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        loadBar.Children.Add(Caption("Well"));
        loadBar.Children.Add(_well);
        loadBar.Children.Add(_loadButton);
        loadBar.Children.Add(Caption("Planned date"));
        loadBar.Children.Add(_plannedDate);
        loadBar.Children.Add(_planButton);

        var barrierBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        barrierBar.Children.Add(_barrierEnvelope);
        barrierBar.Children.Add(_barrierType);
        barrierBar.Children.Add(_barrierTop);
        barrierBar.Children.Add(_barrierBase);
        barrierBar.Children.Add(_barrierUnit);

        var verifyBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        verifyBar.Children.Add(Caption("Method"));
        verifyBar.Children.Add(_verification);
        verifyBar.Children.Add(Caption("Evidence"));
        verifyBar.Children.Add(_evidence);
        verifyBar.Children.Add(_verifyButton);

        var leakBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        leakBar.Children.Add(Caption("Leak path"));
        leakBar.Children.Add(_leakDescription);
        leakBar.Children.Add(_addLeakButton);
        leakBar.Children.Add(Caption("Addressed how"));
        leakBar.Children.Add(_leakHow);
        leakBar.Children.Add(_addressLeakButton);

        var closeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        closeBar.Children.Add(Caption("Rationale"));
        closeBar.Children.Add(_rationale);
        closeBar.Children.Add(_closeButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_status);
        panel.Children.Add(_notice);
        panel.Children.Add(loadBar);
        panel.Children.Add(Caption("Completion checklist"));
        panel.Children.Add(_checklist);
        panel.Children.Add(_failure);
        panel.Children.Add(Caption("Barriers"));
        panel.Children.Add(_barrierGrid);
        panel.Children.Add(barrierBar);
        panel.Children.Add(verifyBar);
        panel.Children.Add(Caption("Required by the owner's template"));
        panel.Children.Add(_requiredGrid);
        panel.Children.Add(Caption("Leak paths"));
        panel.Children.Add(_leakGrid);
        panel.Children.Add(leakBar);
        panel.Children.Add(Caption("Close"));
        panel.Children.Add(closeBar);
        panel.Children.Add(Caption("Schematic"));
        panel.Children.Add(_schematic);

        ShowContent(panel);

        _schematic.Text = OgWellPa.SchematicNotice;
        _plannedDate.Text = DateTime.Today.AddMonths(3).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _barrierUnit.Text = "m";
        _barrierEnvelope.Text = "Primary";
        _barrierType.Text = "CEMENT_PLUG";

        ShowState(OgViewState.Empty, "Enter the well and load its programme.");
        RefreshEnabled();
    }

    public OgWellPa? Programme => _programme;

    public OgAbandonmentBarrier? Barrier => _barrier;

    public OgLeakPath? LeakPath => _leakPath;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string StatusText => _status.Text;

    public string ChecklistText => _checklist.Text;

    public string SchematicText => _schematic.Text;

    public string FailureText => _failure.Text;

    public string RefreshedText => _refreshed.Text;

    public bool Load()
    {
        var wellId = _well.Text?.Trim();

        _header.Text = "Decommissioning · well P&A";

        if (string.IsNullOrWhiteSpace(wellId))
        {
            _programme = null;
            _barrierGrid.ItemsSource = null;
            _requiredGrid.ItemsSource = null;
            _leakGrid.ItemsSource = null;
            _status.Text = string.Empty;
            _checklist.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _notice.Text = "Enter the well whose abandonment programme you are opening.";
            _failure.Text = string.Empty;
            RefreshEnabled();
            return false;
        }

        var loaded = _wellPa
            .LoadAsync(OgHseScope.FromSession(_application.Session), wellId)
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _programme = null;
            _barrierGrid.ItemsSource = null;
            _requiredGrid.ItemsSource = null;
            _leakGrid.ItemsSource = null;
            _status.Text = string.Empty;
            _checklist.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _notice.Text = string.Empty;
            _failure.Text = loaded.Message ?? "The abandonment programme could not be read.";
            ShowState(OgViewState.Failed, "The abandonment programme is not shown.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public Task<bool> PlanAsync()
    {
        var planned = Date(_plannedDate.Text);

        if (planned is null)
        {
            _failure.Text = "A programme needs its planned date.";
            return Task.FromResult(false);
        }

        var envelope = string.IsNullOrWhiteSpace(_barrierEnvelope.Text) ? "Primary" : _barrierEnvelope.Text!;
        var type = string.IsNullOrWhiteSpace(_barrierType.Text) ? "CEMENT_PLUG" : _barrierType.Text!;

        var barrier = new OgBarrierInput(
            envelope,
            type,
            null,
            Number(_barrierTop.Text),
            Number(_barrierBase.Text),
            string.IsNullOrWhiteSpace(_barrierUnit.Text) ? null : _barrierUnit.Text,
            string.IsNullOrWhiteSpace(_verification.Text) ? null : _verification.Text);

        return RunAsync(() => _wellPa.PlanAsync(
                OgHseScope.FromSession(_application.Session),
                WellId(),
                planned.Value,
                [barrier],
                _application.Session.UserId),
            "decommissioning.wellpa.write",
            "Programme planned");
    }

    public Task<bool> VerifyAsync()
    {
        if (_barrier is not { } barrier)
        {
            _failure.Text = "Select the barrier to verify.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _wellPa.VerifyBarrierAsync(
                OgHseScope.FromSession(_application.Session),
                WellId(),
                barrier.Id,
                _verification.Text ?? string.Empty,
                _evidence.Text ?? string.Empty,
                _application.Session.UserId),
            "decommissioning.wellpa.write",
            "Barrier verified");
    }

    public Task<bool> AddLeakAsync() =>
        RunAsync(() => _wellPa.AddLeakPathAsync(
                OgHseScope.FromSession(_application.Session),
                WellId(),
                _leakDescription.Text ?? string.Empty,
                _application.Session.UserId),
            "decommissioning.wellpa.write",
            "Leak path recorded");

    public Task<bool> AddressLeakAsync()
    {
        if (_leakPath is not { } path)
        {
            _failure.Text = "Select the leak path to address.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _wellPa.AddressLeakPathAsync(
                OgHseScope.FromSession(_application.Session),
                WellId(),
                path.Id,
                _leakHow.Text ?? string.Empty,
                _application.Session.UserId),
            "decommissioning.wellpa.write",
            "Leak path addressed");
    }

    public Task<bool> CloseAsync() =>
        RunAsync(() => _wellPa.CloseAsync(
                OgHseScope.FromSession(_application.Session),
                WellId(),
                _rationale.Text ?? string.Empty,
                _application.Session.UserId),
            "decommissioning.wellpa.close",
            "Programme completed");

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgWellPa>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The abandonment command was refused.";
            ShowState(OgViewState.Failed, "The abandonment command was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add(auditAction, "decommissioning", outcome.Value.WellId, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = done + ".";

        return true;
    }

    private void Apply(OgWellPa programme)
    {
        _programme = programme;

        _status.Text = programme.StatusText;
        _checklist.Text = programme.ChecklistText;

        _barrierGrid.ItemsSource = programme.Barriers.ToList();
        _requiredGrid.ItemsSource = programme.Required.ToList();
        _leakGrid.ItemsSource = programme.LeakPaths.ToList();

        _barrier = programme.Barriers.FirstOrDefault();
        _barrierGrid.SelectedItem = _barrier;

        _leakPath = programme.LeakPaths.FirstOrDefault();
        _leakGrid.SelectedItem = _leakPath;

        _refreshed.Text = programme.ReadText +
                          (programme.PlannedDate is { } planned
                              ? " · " + programme.PlannedText
                              : " · " + programme.PlannedText);

        _notice.Text = programme.VerifiedBarriers.ToString(CultureInfo.InvariantCulture) + " of " +
                       programme.Barriers.Count.ToString(CultureInfo.InvariantCulture) +
                       " barrier(s) verified with evidence · " + programme.OpenLeakPaths.Count +
                       " leak path(s) open";

        _failure.Text = string.Empty;

        RefreshEnabled();

        ShowState(programme.IsComplete ? OgViewState.Ready : OgViewState.Empty, _notice.Text);
    }

    private void SelectBarrier()
    {
        if (_barrierGrid.SelectedItem is not OgAbandonmentBarrier selected)
        {
            return;
        }

        _barrier = selected;
        RefreshEnabled();
    }

    private void SelectLeakPath()
    {
        if (_leakGrid.SelectedItem is not OgLeakPath selected)
        {
            return;
        }

        _leakPath = selected;
        RefreshEnabled();
    }

    private string WellId() => _well.Text?.Trim() ?? string.Empty;

    private void RefreshEnabled()
    {
        var loaded = _programme is not null;

        _planButton.IsEnabled = true;
        _verifyButton.IsEnabled = loaded && _barrier is not null;
        _addLeakButton.IsEnabled = loaded;
        _addressLeakButton.IsEnabled = loaded && _leakPath is { IsOpen: true };
        _closeButton.IsEnabled = loaded && !_programme!.IsComplete;
    }

    private static decimal? Number(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;

    private static DateTime? Date(string? text) =>
        DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : null;

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
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
        TextWrapping = TextWrapping.Wrap,
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

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };
}
