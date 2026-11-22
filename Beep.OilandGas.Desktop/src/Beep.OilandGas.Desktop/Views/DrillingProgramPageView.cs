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
/// DEV-05: authoring a drilling program and having it independently reviewed.
/// </summary>
/// <remarks>
/// A program is created for a designed well; the submission is refused while the well design or a hole section is
/// incomplete (AC-1 and DEV-03's gate), and the refusal names the gaps. The three design checks exist from creation as
/// "not run", so a check nobody ran can never read as passed (AC-2). The author cannot review their own program
/// (AC-3). An approved version is never edited: a revision appends a new version and the old one stays as it was
/// (AC-4). Depths and diameters carry their units (AC-5), and the total is the sum of the section costs (AC-6).
/// </remarks>
public sealed class DrillingProgramPageView : OgPageBase
{
    public const string Route = "drillingprogram";

    private readonly OgApplication _application;
    private readonly IOgDrillingPrograms _programs;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _checks;
    private readonly TextBlock _detail;
    private readonly ComboBox _wellPicker;
    private readonly ComboBox _programPicker;
    private readonly DataGrid _grid;
    private readonly DataGrid _sections;

    private readonly TextBox _name;
    private readonly TextBox _objective;
    private readonly TextBox _md;
    private readonly TextBox _tvd;
    private readonly TextBox _depthUnit;
    private readonly TextBox _duration;
    private readonly TextBox _currency;

    private readonly TextBox _sequence;
    private readonly TextBox _sectionName;
    private readonly TextBox _holeSize;
    private readonly TextBox _holeSizeUnit;
    private readonly TextBox _top;
    private readonly TextBox _bottom;
    private readonly TextBox _sectionDepthUnit;
    private readonly TextBox _casingOd;
    private readonly TextBox _casingOdUnit;
    private readonly TextBox _mudWeight;
    private readonly TextBox _mudUnit;
    private readonly TextBox _sectionDays;
    private readonly TextBox _sectionCost;

    private readonly ComboBox _checkName;
    private readonly ComboBox _checkStatus;
    private readonly TextBox _checkReference;
    private readonly ComboBox _decision;
    private readonly TextBox _comments;
    private readonly TextBox _reviewer;

    public DrillingProgramPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _programs = application.DrillingPrograms;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _checks = Body(string.Empty);
        _checks.TextWrapping = TextWrapping.Wrap;
        _checks.Margin = new Thickness(0, 4, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _wellPicker = new ComboBox { MinWidth = 260, Margin = new Thickness(0, 0, 8, 0) };
        Named(_wellPicker, "Designed well");

        _programPicker = new ComboBox { MinWidth = 240, Margin = new Thickness(0, 0, 8, 0) };
        Named(_programPicker, "Drilling program");
        _programPicker.SelectionChanged += (_, _) => ShowSelectedProgram();

        _grid = new DataGrid
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
                Text("Program", "Name", 160),
                Text("Well", "WellId", 110),
                Text("Version", "Version", 70),
                Text("State", "StateText", 100),
                Text("Sections", "SectionCount", 70),
                Text("Total cost", "TotalCostText", 150),
                Text("Checks", "ChecksSummary", 220),
            },
        };

        Named(_grid, "Drilling programs");
        _grid.SelectionChanged += (_, _) =>
        {
            if (_grid.SelectedItem is OgDrillProgram program)
            {
                var index = Programs.ToList().FindIndex(row => row.Id == program.Id);

                if (index >= 0)
                {
                    _programPicker.SelectedIndex = index;
                }
            }
        };

        _sections = new DataGrid
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
                Text("Seq", "Sequence", 50),
                Text("Section", "Name", 110),
                Text("Hole", "HoleText", 100),
                Text("Interval", "DepthText", 150),
                Text("Mud", "MudText", 100),
                Text("Days", "DurationText", 80),
                Text("Cost", "CostText", 100),
            },
        };

        Named(_sections, "Hole sections");

        _name = Field("Program name", 160);
        _objective = Field("Objective", 220);
        _md = Field("Planned MD", 90);
        _tvd = Field("Planned TVD", 90);
        _depthUnit = Field("Depth unit (m/ft)", 90);
        _duration = Field("Planned duration (days)", 100);
        _currency = Field("Currency", 70);

        _sequence = Field("Sequence", 60);
        _sectionName = Field("Section name", 120);
        _holeSize = Field("Hole size", 80);
        _holeSizeUnit = Field("Hole unit", 70);
        _top = Field("Top", 70);
        _bottom = Field("Bottom", 80);
        _sectionDepthUnit = Field("Depth unit", 70);
        _casingOd = Field("Casing OD", 80);
        _casingOdUnit = Field("Casing unit", 70);
        _mudWeight = Field("Mud weight", 80);
        _mudUnit = Field("Mud unit", 70);
        _sectionDays = Field("Days", 60);
        _sectionCost = Field("Cost", 80);

        _checkName = new ComboBox
        {
            MinWidth = 140,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = OgDrillProgramService.CheckNames,
            SelectedIndex = 0,
        };

        Named(_checkName, "Design check");

        _checkStatus = new ComboBox
        {
            MinWidth = 110,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = OgDrillProgramService.CheckStatuses,
            SelectedIndex = 0,
        };

        Named(_checkStatus, "Check status");

        _checkReference = Field("Check reference", 140);

        _decision = new ComboBox
        {
            MinWidth = 180,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = OgDrillDecision.Decisions,
            SelectedIndex = 0,
        };

        Named(_decision, "Review decision");

        _comments = Field("Review comments", 220);
        _reviewer = Field("Reviewer", 140);
        _reviewer.Text = _application.Session.UserId;

        var create = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        create.Children.Add(Caption("Designed well"));
        create.Children.Add(_wellPicker);
        create.Children.Add(Button("Create program", (_, _) => _ = CreateAsync()));
        create.Children.Add(Button("Refresh", (_, _) => Load()));

        var header = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        header.Children.Add(Caption("Program"));
        header.Children.Add(_name);
        header.Children.Add(_objective);
        header.Children.Add(Caption("MD"));
        header.Children.Add(_md);
        header.Children.Add(Caption("TVD"));
        header.Children.Add(_tvd);
        header.Children.Add(_depthUnit);
        header.Children.Add(Caption("Days"));
        header.Children.Add(_duration);
        header.Children.Add(_currency);
        header.Children.Add(Button("Save header", (_, _) => _ = SaveHeaderAsync()));
        header.Children.Add(Button("Submit", (_, _) => _ = SubmitAsync()));
        header.Children.Add(Button("Retry submit", (_, _) => _ = RetryAsync()));

        var sections = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        sections.Children.Add(Caption("Section"));
        sections.Children.Add(_sequence);
        sections.Children.Add(_sectionName);
        sections.Children.Add(_holeSize);
        sections.Children.Add(_holeSizeUnit);
        sections.Children.Add(_top);
        sections.Children.Add(_bottom);
        sections.Children.Add(_sectionDepthUnit);
        sections.Children.Add(_casingOd);
        sections.Children.Add(_casingOdUnit);
        sections.Children.Add(_mudWeight);
        sections.Children.Add(_mudUnit);
        sections.Children.Add(_sectionDays);
        sections.Children.Add(_sectionCost);
        sections.Children.Add(Button("Save section", (_, _) => _ = SaveSectionAsync()));

        var checks = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        checks.Children.Add(Caption("Check"));
        checks.Children.Add(_checkName);
        checks.Children.Add(_checkStatus);
        checks.Children.Add(_checkReference);
        checks.Children.Add(Button("Record check", (_, _) => _ = RecordCheckAsync()));

        var review = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        review.Children.Add(Caption("Review"));
        review.Children.Add(_decision);
        review.Children.Add(_comments);
        review.Children.Add(_reviewer);
        review.Children.Add(Button("Record decision", (_, _) => _ = ReviewAsync()));
        review.Children.Add(Button("Revise", (_, _) => _ = ReviseAsync()));

        var panel = new StackPanel();
        panel.Children.Add(create);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_programPicker);
        panel.Children.Add(_grid);
        panel.Children.Add(header);
        panel.Children.Add(Caption("Hole sections"));
        panel.Children.Add(_sections);
        panel.Children.Add(sections);
        panel.Children.Add(Caption("Design checks"));
        panel.Children.Add(_checks);
        panel.Children.Add(checks);
        panel.Children.Add(review);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgDrillProgramWorkspace? Workspace { get; private set; }

    public IReadOnlyList<OgDrillProgram> Programs => Workspace?.Programs ?? [];

    public OgDrillProgram? Selected { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string ChecksText => _checks.Text;

    public string DetailText => _detail.Text;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _programs
            .LoadAsync(session.FieldId, session.FieldName)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Workspace = null;
            Selected = null;
            _grid.ItemsSource = null;
            _sections.ItemsSource = null;
            _wellPicker.ItemsSource = null;
            _programPicker.ItemsSource = null;
            _header.Text = "Drilling program";
            _checks.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The drilling programs could not be read.";
            ShowState(OgViewState.Failed, "The drilling programs could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Workspace = loaded.Value;
        var previous = Selected?.Id;

        _wellPicker.ItemsSource = Workspace.DesigndWells.ToList();
        _wellPicker.SelectedIndex = Workspace.DesigndWells.Count > 0 ? 0 : -1;

        _grid.ItemsSource = Programs.ToList();
        _programPicker.ItemsSource = Programs.ToList();
        _programPicker.SelectedItem = Programs.FirstOrDefault(program =>
            string.Equals(program.Id, previous, StringComparison.OrdinalIgnoreCase)) ?? Programs.FirstOrDefault();

        _header.Text =
            $"{Programs.Count.ToString(CultureInfo.InvariantCulture)} program(s) · " +
            $"{Workspace.DesigndWells.Count.ToString(CultureInfo.InvariantCulture)} designed well(s) · " +
            $"{(string.IsNullOrWhiteSpace(session.FieldName) ? "(no field selected)" : session.FieldName)}";

        _notice.Text = Workspace.DesigndWells.Count == 0
            ? "No well has a design yet; design a development well before authoring its program."
            : Programs.Count == 0
                ? "No drilling program is recorded for this field. Choose a designed well and create one."
                : "A submitted program is read-only; a revision creates a new version.";

        ShowState(Programs.Count == 0 ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    public async Task<bool> CreateAsync()
    {
        if (_wellPicker.SelectedItem is not OgWellDesignOption well)
        {
            _notice.Text = "Choose a designed well.";
            return false;
        }

        var created = await _programs
            .CreateAsync(
                _application.Session.CompanyId,
                well.DesignId,
                ReadHeaderValues(),
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(created, "development.drillingprogram.create", well.WellId);
    }

    public async Task<bool> SaveHeaderAsync()
    {
        if (Selected is not { } program)
        {
            _notice.Text = "Select a program first.";
            return false;
        }

        var saved = await _programs
            .SaveHeaderAsync(program.Id, ReadHeaderValues(), _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(saved, "development.drillingprogram.header", program.Id);
    }

    public async Task<bool> SaveSectionAsync()
    {
        if (Selected is not { } program)
        {
            _notice.Text = "Select a program first.";
            return false;
        }

        var saved = await _programs
            .SaveSectionAsync(
                program.Id,
                new OgDrillSectionValues(
                    Integer(_sequence.Text, 1),
                    _sectionName.Text,
                    Decimal(_holeSize.Text),
                    _holeSizeUnit.Text,
                    Decimal(_top.Text),
                    Decimal(_bottom.Text),
                    _sectionDepthUnit.Text,
                    Decimal(_casingOd.Text),
                    _casingOdUnit.Text,
                    Decimal(_mudWeight.Text),
                    _mudUnit.Text,
                    Decimal(_sectionDays.Text),
                    Decimal(_sectionCost.Text)),
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(saved, "development.drillingprogram.section", program.Id);
    }

    public async Task<bool> RecordCheckAsync()
    {
        if (Selected is not { } program)
        {
            _notice.Text = "Select a program first.";
            return false;
        }

        var recorded = await _programs
            .RecordCheckAsync(
                program.Id,
                _checkName.SelectedItem as string ?? OgDrillProgramService.CheckNames[0],
                _checkStatus.SelectedItem as string ?? OgDrillCheck.NotRun,
                _checkReference.Text,
                null,
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(recorded, "development.drillingprogram.check", program.Id);
    }

    public async Task<bool> SubmitAsync()
    {
        if (Selected is not { } program)
        {
            _notice.Text = "Select a program first.";
            return false;
        }

        var submitted = await _programs.SubmitAsync(program.Id, _application.Session.UserId).ConfigureAwait(true);

        return AfterWrite(submitted, "development.drillingprogram.submit", program.Id);
    }

    public async Task<bool> ReviewAsync()
    {
        if (Selected is not { } program)
        {
            _notice.Text = "Select a program first.";
            return false;
        }

        var reviewed = await _programs
            .ReviewAsync(
                program.Id,
                new OgDrillDecision(
                    _decision.SelectedItem as string ?? OgDrillDecision.Approve,
                    _comments.Text,
                    _reviewer.Text))
            .ConfigureAwait(true);

        return AfterWrite(reviewed, "development.drillingprogram.review", program.Id);
    }

    public async Task<bool> ReviseAsync()
    {
        if (Selected is not { } program)
        {
            _notice.Text = "Select a program first.";
            return false;
        }

        var revised = await _programs.ReviseAsync(program.Id, _application.Session.UserId).ConfigureAwait(true);

        return AfterWrite(revised, "development.drillingprogram.revise", program.Id);
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _programs.RetrySubmitAsync().ConfigureAwait(true);
        return AfterWrite(retried, "development.drillingprogram.submit", "retry");
    }

    private OgDrillProgramValues ReadHeaderValues() =>
        new(
            _name.Text,
            _objective.Text,
            Decimal(_md.Text),
            Decimal(_tvd.Text),
            _depthUnit.Text,
            Decimal(_duration.Text),
            _currency.Text);

    private bool AfterWrite(OgResult<OgDrillProgram> result, string verb, string subject)
    {
        _notice.Text = result.Success
            ? result.Message ?? "Saved."
            : result.Message ?? "The change was not applied.";

        if (result.Success)
        {
            _application.AuditLog.Add(verb, "development", subject, "ok", _application.Session.UserId);
        }

        if (result.Value is not null)
        {
            Selected = result.Value;
            ShowProgram(result.Value);
        }

        Load();

        if (result.Value is not null)
        {
            var index = Programs.ToList().FindIndex(program =>
                string.Equals(program.Id, result.Value.Id, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                _programPicker.SelectedIndex = index;
            }

            ShowProgram(result.Value);
        }

        return result.Success;
    }

    private void ShowSelectedProgram()
    {
        if (_programPicker.SelectedItem is OgDrillProgram program)
        {
            Selected = program;
            ShowProgram(program);
        }
    }

    private void ShowProgram(OgDrillProgram program)
    {
        _name.Text = program.Name ?? string.Empty;
        _objective.Text = program.Objective ?? string.Empty;
        _md.Text = program.MeasuredDepth?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _tvd.Text = program.TrueVerticalDepth?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _depthUnit.Text = program.DepthUnit ?? string.Empty;
        _duration.Text = program.DurationDays?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _currency.Text = program.Currency ?? string.Empty;
        _sequence.Text = (program.Sections.Count + 1).ToString(CultureInfo.InvariantCulture);

        _sections.ItemsSource = program.Sections.ToList();
        _checks.Text = program.ChecksText;
        _reviewer.Text = string.Equals(program.PreparedBy, _application.Session.UserId, StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : _application.Session.UserId;

        _detail.Text = string.Join(
            Environment.NewLine,
            new[]
            {
                program.Describe(),
                $"Checks: {program.ChecksText}",
                program.CanSubmit
                    ? "Submittable: the design and every section are complete."
                    : "Not submittable: " + string.Join("; ", program.SubmissionGaps
                        .Concat(program.DesignMissing.Count == 0
                            ? []
                            : ["design missing " + string.Join(", ", program.DesignMissing)])),
                VersionHistory(program),
                program.Reviews.Count == 0
                    ? "No review is recorded for this version."
                    : "Reviews: " + string.Join(" · ", program.Reviews.Select(review => review.Describe())),
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    /// <summary>The version history of the program's lineage (the feature's "Version history" panel).</summary>
    private string VersionHistory(OgDrillProgram program)
    {
        var history = _programs.LoadVersionsAsync(program.LineageId).GetAwaiter().GetResult();

        return history.Success && history.Value is { Count: > 1 }
            ? "Versions: " + string.Join(" · ", history.Value.Select(version =>
                $"v{version.Version.ToString(CultureInfo.InvariantCulture)} {version.StateText} " +
                $"({version.TotalCostText})"))
            : "Versions: this is the only version.";
    }

    private static decimal? Decimal(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : null;

    private static int Integer(string? text, int fallback) =>
        int.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback;

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
