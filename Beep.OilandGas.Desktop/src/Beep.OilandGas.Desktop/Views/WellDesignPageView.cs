using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// DEV-03: the design of each planned development well.
/// </summary>
/// <remarks>
/// A design is created from a scheduled well, so it is scoped to that well's plan and field (AC-1). A target depth
/// without its unit is refused (AC-2). The casing check verifies the design's inputs and records a result linked to
/// the version (AC-3); an incomplete design cannot be approved and names what is missing (AC-4); a cost revision
/// appends a version and retains the old one (AC-5). The schematic preview is disabled with the reason until a casing
/// string exists, and the burst/collapse arithmetic is ENG-02's, not this screen's.
/// </remarks>
public sealed class WellDesignPageView : OgPageBase
{
    public const string Route = "welldesign";

    private readonly OgApplication _application;
    private readonly IOgWellDesign _designs;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _state;
    private readonly TextBlock _detail;
    private readonly ComboBox _schedule;
    private readonly ComboBox _well;
    private readonly DataGrid _grid;
    private readonly DataGrid _casing;

    private readonly TextBox _objective;
    private readonly TextBox _formation;
    private readonly TextBox _targetDepth;
    private readonly TextBox _depthUnit;
    private readonly TextBox _trajectory;
    private readonly TextBox _completion;
    private readonly TextBox _risk;
    private readonly TextBox _cost;
    private readonly TextBox _currency;
    private readonly TextBox _spudStart;
    private readonly TextBox _spudEnd;

    private readonly TextBox _casingName;
    private readonly TextBox _casingOd;
    private readonly TextBox _casingWeight;
    private readonly TextBox _casingGrade;
    private readonly TextBox _casingDepth;
    private readonly TextBox _casingUnit;
    private readonly TextBox _revisionReason;

    public WellDesignPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _designs = application.WellDesign;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _state = Body(string.Empty);
        _state.TextWrapping = TextWrapping.Wrap;

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _schedule = new ComboBox { MinWidth = 240, Margin = new Thickness(0, 0, 8, 0) };
        Named(_schedule, "Scheduled well to design");

        _well = new ComboBox { MinWidth = 240, Margin = new Thickness(0, 0, 8, 0) };
        Named(_well, "Development well");
        _well.SelectionChanged += (_, _) => ShowSelected();

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 170,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Well", "Name", 150),
                Text("Version", "Version", 70),
                Text("State", "StateText", 90),
                Text("Target", "TargetText", 120),
                Text("Cost", "CostText", 120),
                Text("Casing", "CasingCount", 70),
                Text("Design complete", "MissingText", 240),
            },
        };

        Named(_grid, "Development wells");
        _grid.SelectionChanged += (_, _) => ShowSelected();

        _casing = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 160,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Order", "Order", 60),
                Text("String", "Name", 110),
                Text("OD", "Od", 70),
                Text("Weight", "Weight", 70),
                Text("Grade", "Grade", 80),
                Text("Set depth", "SetDepthText", 130),
            },
        };

        Named(_casing, "Casing scheme");

        _objective = Field("Objective", 240);
        _formation = Field("Target formation", 150);
        _targetDepth = Field("Target depth", 90);
        _depthUnit = Field("Depth unit", 70);
        _trajectory = Field("Trajectory intent", 200);
        _completion = Field("Completion concept", 200);
        _risk = Field("Design risks", 220);
        _cost = Field("Estimated cost", 100);
        _currency = Field("Cost currency", 70);
        _spudStart = Field("Spud window start yyyy-MM-dd", 130);
        _spudEnd = Field("Spud window end yyyy-MM-dd", 130);

        _casingName = Field("Casing string name", 120);
        _casingOd = Field("Outside diameter", 80);
        _casingWeight = Field("Weight", 70);
        _casingGrade = Field("Steel grade", 80);
        _casingDepth = Field("Set depth", 90);
        _casingUnit = Field("Set depth unit", 70);
        _revisionReason = Field("Revision reason", 200);

        var create = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        create.Children.Add(Caption("Scheduled well"));
        create.Children.Add(_schedule);
        create.Children.Add(Button("Create design", (_, _) => _ = CreateAsync()));
        create.Children.Add(Button("Refresh", (_, _) => Load()));

        var design = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        design.Children.Add(Caption("Design"));
        design.Children.Add(_objective);
        design.Children.Add(_formation);
        design.Children.Add(_targetDepth);
        design.Children.Add(_depthUnit);
        design.Children.Add(_trajectory);
        design.Children.Add(_completion);
        design.Children.Add(_risk);
        design.Children.Add(_cost);
        design.Children.Add(_currency);
        design.Children.Add(_spudStart);
        design.Children.Add(_spudEnd);
        design.Children.Add(Button("Save design", (_, _) => _ = SaveAsync()));
        design.Children.Add(Button("Approve", (_, _) => _ = ApproveAsync()));

        var revision = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        revision.Children.Add(Caption("Cost revision"));
        revision.Children.Add(_revisionReason);
        revision.Children.Add(Button("Save as new version", (_, _) => _ = ReviseAsync()));

        var casing = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        casing.Children.Add(Caption("Casing"));
        casing.Children.Add(_casingName);
        casing.Children.Add(_casingOd);
        casing.Children.Add(_casingWeight);
        casing.Children.Add(_casingGrade);
        casing.Children.Add(_casingDepth);
        casing.Children.Add(_casingUnit);
        casing.Children.Add(Button("Add string", (_, _) => _ = AddCasingAsync()));
        casing.Children.Add(Button("Check casing design", (_, _) => _ = RunCheckAsync()));

        var panel = new StackPanel();
        panel.Children.Add(create);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_well);
        panel.Children.Add(_grid);
        panel.Children.Add(design);
        panel.Children.Add(revision);
        panel.Children.Add(Caption("Casing scheme"));
        panel.Children.Add(_casing);
        panel.Children.Add(casing);
        panel.Children.Add(_state);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgWellDesignWorkspace? Workspace { get; private set; }

    public OgDevelopmentWell? Selected { get; private set; }

    public OgWellDesignDetail? Detail { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string StateText => _state.Text;

    public string DetailText => _detail.Text;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _designs
            .LoadAsync(session.FieldId, session.FieldName, null)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Workspace = null;
            Selected = null;
            Detail = null;
            _grid.ItemsSource = null;
            _casing.ItemsSource = null;
            _schedule.ItemsSource = null;
            _well.ItemsSource = null;
            _header.Text = "Well design";
            _state.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The well designs could not be read.";
            ShowState(OgViewState.Failed, "The well designs could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Workspace = loaded.Value;
        var previous = Selected?.Id;

        _schedule.ItemsSource = Workspace.UndesignedSchedules.ToList();
        _schedule.SelectedIndex = Workspace.UndesignedSchedules.Count > 0 ? 0 : -1;

        _well.ItemsSource = Workspace.Wells.ToList();
        _well.SelectedItem = Workspace.Wells.FirstOrDefault(well =>
            string.Equals(well.Id, previous, StringComparison.OrdinalIgnoreCase)) ?? Workspace.Wells.FirstOrDefault();

        _grid.ItemsSource = Workspace.Wells.ToList();

        _header.Text =
            $"{Workspace.Wells.Count.ToString(CultureInfo.InvariantCulture)} designed well(s) · " +
            $"{Workspace.UndesignedSchedules.Count.ToString(CultureInfo.InvariantCulture)} awaiting design · " +
            $"{(string.IsNullOrWhiteSpace(session.FieldName) ? "(no field selected)" : session.FieldName)}";

        _notice.Text = Workspace.IsEmpty
            ? Workspace.Schedules.Count == 0
                ? "No scheduled well is recorded for this field; add one in the plan before designing it."
                : "No design is recorded yet. Choose a scheduled well and create its design."
            : $"{Workspace.IncompleteWells.Count.ToString(CultureInfo.InvariantCulture)} design(s) are incomplete; " +
              "an incomplete design blocks drilling program authoring.";

        ShowState(Workspace.IsEmpty ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    public async Task<bool> CreateAsync()
    {
        if (_schedule.SelectedItem is not OgDesignSchedule schedule)
        {
            _notice.Text = "Choose a scheduled well that has no design.";
            return false;
        }

        var created = await _designs
            .CreateAsync(schedule.Id, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(created, "development.welldesign.create", schedule.Id);
    }

    public async Task<bool> SaveAsync()
    {
        if (Selected is not { } well)
        {
            _notice.Text = "Select a design first.";
            return false;
        }

        var saved = await _designs
            .SaveAsync(well.Id, ReadValues(), _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(saved, "development.welldesign.save", well.Id);
    }

    public async Task<bool> ApproveAsync()
    {
        if (Selected is not { } well)
        {
            _notice.Text = "Select a design first.";
            return false;
        }

        var approved = await _designs
            .ApproveAsync(well.Id, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(approved, "development.welldesign.approve", well.Id);
    }

    public async Task<bool> ReviseAsync()
    {
        if (Selected is not { } well)
        {
            _notice.Text = "Select a design first.";
            return false;
        }

        var revised = await _designs
            .ReviseAsync(well.Id, ReadValues(), _revisionReason.Text, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(revised, "development.welldesign.revise", well.Id);
    }

    public async Task<bool> AddCasingAsync()
    {
        if (Selected is not { } well)
        {
            _notice.Text = "Select a design first.";
            return false;
        }

        var added = await _designs
            .AddCasingStringAsync(
                well.Id,
                new OgCasingStringRequest(
                    _casingName.Text,
                    Decimal(_casingOd.Text),
                    Decimal(_casingWeight.Text),
                    _casingGrade.Text,
                    Decimal(_casingDepth.Text),
                    string.IsNullOrWhiteSpace(_casingUnit.Text) ? well.DepthUnit : _casingUnit.Text),
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(added, "development.welldesign.casing", well.Id);
    }

    public async Task<bool> RunCheckAsync()
    {
        if (Selected is not { } well)
        {
            _notice.Text = "Select a design first.";
            return false;
        }

        var checkedDesign = await _designs
            .RunCasingCheckAsync(well.Id, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(checkedDesign, "development.welldesign.casingcheck", well.Id);
    }

    private OgWellDesignValues ReadValues() =>
        new(
            _objective.Text,
            _formation.Text,
            decimal.TryParse(_targetDepth.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var depth)
                ? depth
                : null,
            Empty(_depthUnit.Text),
            _trajectory.Text,
            _completion.Text,
            _risk.Text,
            decimal.TryParse(_cost.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var cost)
                ? cost
                : null,
            Empty(_currency.Text),
            Date(_spudStart.Text),
            Date(_spudEnd.Text));

    private bool AfterWrite(OgResult<OgWellDesignDetail> result, string verb, string subject)
    {
        _notice.Text = result.Success
            ? result.Message ?? "Saved."
            : result.Message ?? "The change was not applied.";

        if (result.Success)
        {
            _application.AuditLog.Add(verb, "development", subject, "ok", _application.Session.UserId);
        }

        // A refused approve or check still returns the design with its findings, so show them either way.
        if (result.Value is not null)
        {
            Selected = result.Value.Current;
            ShowDetail(result.Value);
        }

        Load();

        if (result.Value is not null)
        {
            ShowDetail(result.Value);
        }

        return result.Success;
    }

    private void ShowSelected()
    {
        if (_well.SelectedItem is OgDevelopmentWell well)
        {
            Selected = well;
            Fill(well);
            _casing.ItemsSource = well.Casing
                .Select(item => new
                {
                    item.Order,
                    item.Name,
                    item.Od,
                    item.Weight,
                    Grade = item.Grade ?? string.Empty,
                    SetDepthText = $"{item.SetDepth.ToString("0.##", CultureInfo.InvariantCulture)} " +
                                   (item.DepthUnit ?? "(unit not recorded)"),
                })
                .ToList();

            ShowDetail(new OgWellDesignDetail(
                well,
                Workspace?.Wells.Where(row =>
                    string.Equals(row.ScheduleId, well.ScheduleId, StringComparison.OrdinalIgnoreCase)).ToList() ?? [],
                well.LastCheck));
        }
    }

    private void Fill(OgDevelopmentWell well)
    {
        _objective.Text = well.Objective ?? string.Empty;
        _formation.Text = well.TargetFormation ?? string.Empty;
        _targetDepth.Text = well.TargetDepth?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _depthUnit.Text = well.DepthUnit ?? string.Empty;
        _trajectory.Text = well.TrajectoryIntent ?? string.Empty;
        _completion.Text = well.CompletionConcept ?? string.Empty;
        _risk.Text = well.Risk ?? string.Empty;
        _cost.Text = well.EstimatedCost?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _currency.Text = well.Currency ?? string.Empty;
        _spudStart.Text = well.SpudWindowStart?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        _spudEnd.Text = well.SpudWindowEnd?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        _casingUnit.Text = well.DepthUnit ?? string.Empty;
    }

    private void ShowDetail(OgWellDesignDetail detail)
    {
        Detail = detail;

        var well = detail.Current;

        _state.Text =
            $"Design complete: {(well.CanAuthorProgram ? "yes" : "no")} · " +
            (well.CanAuthorProgram
                ? "a drilling program may be authored from this design"
                : well.MissingText);

        _detail.Text = string.Join(
            Environment.NewLine,
            new[]
            {
                well.Describe(),
                $"Objective: {well.Objective ?? "(not recorded)"}",
                $"Target: {well.TargetFormation ?? "(no formation)"} at {well.TargetText}",
                $"Trajectory intent: {well.TrajectoryText}",
                $"Completion: {well.CompletionConcept ?? "(not recorded)"}",
                $"Risk: {well.Risk ?? "(not recorded)"}",
                $"Spud window: {well.SpudWindowText}",
                well.IsApproved
                    ? $"Approved by {well.ApprovedBy ?? "(unknown)"} on " +
                      $"{well.ApprovedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "unknown"}"
                    : "Not approved.",
                well.CasingPreviewReason,
                detail.LastCheck is null
                    ? "No casing check has been recorded for this version."
                    : "Casing check: " + detail.LastCheck.Describe(),
                detail.Versions.Count <= 1
                    ? null
                    : "Versions: " + string.Join(" · ", detail.Versions.Select(version =>
                        $"v{version.Version.ToString(CultureInfo.InvariantCulture)} {version.StateText}")) +
                      (well.RevisionReason is null ? string.Empty : $" · this version: {well.RevisionReason}"),
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static decimal Decimal(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0m;

    private static DateTime? Date(string? text) =>
        DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : null;

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
