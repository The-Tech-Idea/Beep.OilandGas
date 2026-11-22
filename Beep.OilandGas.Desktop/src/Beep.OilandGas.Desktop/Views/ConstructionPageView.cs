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
/// DEV-08: facility construction progress and the handover to operations.
/// </summary>
/// <remarks>
/// A milestone cannot complete without evidence (AC-1); overdue is computed from the planned date, never stored (AC-2);
/// an unresolved prior submission hides the create and edit controls and says why (AC-3); a handover records recipient,
/// date, conditions and the signature it was signed under (AC-4); recorded equipment is shown with the sentence that it
/// does not prove installation, testing or acceptance (AC-5); and every write revalidates the facility, so a facility
/// that changed underneath the screen is refused rather than written to (AC-6).
/// </remarks>
public sealed class ConstructionPageView : OgPageBase
{
    public const string Route = "construction";

    private readonly OgApplication _application;
    private readonly IOgConstruction _construction;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _summary;
    private readonly TextBlock _equipmentNote;
    private readonly TextBlock _upstream;
    private readonly TextBlock _handover;
    private readonly TextBlock _detail;
    private readonly ComboBox _facilityPicker;
    private readonly DataGrid _milestones;
    private readonly DataGrid _evidence;
    private readonly DataGrid _equipment;
    private readonly WrapPanel _createBar;
    private readonly WrapPanel _updateBar;
    private readonly WrapPanel _evidenceBar;
    private readonly WrapPanel _handoverBar;

    private readonly ComboBox _milestoneName;
    private readonly TextBox _newPlannedDate;
    private readonly TextBox _plannedDate;
    private readonly TextBox _notes;

    private readonly TextBox _evidenceDocument;
    private readonly TextBox _evidenceReference;
    private readonly TextBox _evidenceDate;
    private readonly TextBox _evidenceReviewer;
    private readonly ComboBox _evidenceOutcome;

    private readonly TextBox _actualDate;
    private readonly TextBox _handedOverTo;
    private readonly TextBox _handedOverAt;
    private readonly TextBox _conditions;
    private readonly TextBox _signature;
    private readonly TextBox _signedBy;

    public ConstructionPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _construction = application.Construction;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _equipmentNote = Body(string.Empty);
        _equipmentNote.TextWrapping = TextWrapping.Wrap;
        _equipmentNote.Margin = new Thickness(0, 4, 0, 8);

        _upstream = Body(string.Empty);
        _upstream.TextWrapping = TextWrapping.Wrap;
        _upstream.Margin = new Thickness(0, 0, 0, 8);

        _handover = Body(string.Empty);
        _handover.TextWrapping = TextWrapping.Wrap;
        _handover.Margin = new Thickness(0, 4, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _facilityPicker = new ComboBox { MinWidth = 260, Margin = new Thickness(0, 0, 8, 0) };
        Named(_facilityPicker, "Facility");
        _facilityPicker.SelectionChanged += (_, _) => SelectFacility();

        _milestones = new DataGrid
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
                Text("Seq", "Sequence", 45),
                Text("Milestone", "NameText", 110),
                Text("State", "StateText", 90),
                Text("Planned", "PlannedText", 100),
                Text("Actual", "ActualText", 100),
                Text("Overdue", "OverdueText", 80),
                Text("Evidence", "EvidenceText", 110),
            },
        };

        Named(_milestones, "Construction milestones");
        _milestones.SelectionChanged += (_, _) => ShowSelectedMilestone();

        _evidence = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 80,
            MaxHeight = 140,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Document", "Document", 170),
                Text("Reference", "Reference", 170),
                Text("Date", "EvidenceDateText", 95),
                Text("Reviewer", "Reviewer", 100),
                Text("Outcome", "OutcomeText", 120),
            },
        };

        Named(_evidence, "Milestone evidence");

        _equipment = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 70,
            MaxHeight = 130,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Equipment", "EquipmentId", 150),
                Text("State", "StateText", 90),
                Text("Installed", "InstallText", 100),
                Text("Removed", "RemoveText", 100),
                Text("Use", "Use", 160),
            },
        };

        Named(_equipment, "Recorded equipment");

        _milestoneName = new ComboBox
        {
            MinWidth = 150,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = ConstructionRules.MilestoneNames,
            SelectedIndex = 0,
        };

        Named(_milestoneName, "Milestone name");

        _newPlannedDate = Field("New milestone planned date yyyy-MM-dd", 140);
        _plannedDate = Field("Planned date yyyy-MM-dd", 120);
        _notes = Field("Milestone note", 200);
        _actualDate = Field("Actual date yyyy-MM-dd", 120);

        _evidenceDocument = Field("Evidence document", 170);
        _evidenceReference = Field("Evidence reference", 170);
        _evidenceDate = Field("Evidence date yyyy-MM-dd", 110);
        _evidenceReviewer = Field("Evidence reviewer", 120);

        _evidenceOutcome = new ComboBox
        {
            MinWidth = 170,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = OgMilestoneEvidence.Outcomes,
            SelectedIndex = 0,
        };

        Named(_evidenceOutcome, "Evidence outcome");

        _handedOverTo = Field("Hand over to", 130);
        _handedOverAt = Field("Handover date yyyy-MM-dd", 120);
        _conditions = Field("Handover conditions (one per line)", 220);
        _signature = Field("Acceptance signature", 170);
        _signedBy = Field("Signed by", 120);
        _signedBy.Text = _application.Session.UserId;

        _createBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        _createBar.Children.Add(Caption("New milestone"));
        _createBar.Children.Add(_milestoneName);
        _createBar.Children.Add(_newPlannedDate);
        _createBar.Children.Add(Button("Create milestone", (_, _) => _ = CreateMilestoneAsync()));

        _updateBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        _updateBar.Children.Add(Caption("Selected milestone"));
        _updateBar.Children.Add(_plannedDate);
        _updateBar.Children.Add(_notes);
        _updateBar.Children.Add(Button("Save milestone", (_, _) => _ = UpdateMilestoneAsync()));
        _updateBar.Children.Add(_actualDate);
        _updateBar.Children.Add(Button("Complete on evidence", (_, _) => _ = CompleteMilestoneAsync()));

        _evidenceBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        _evidenceBar.Children.Add(Caption("Evidence"));
        _evidenceBar.Children.Add(_evidenceDocument);
        _evidenceBar.Children.Add(_evidenceReference);
        _evidenceBar.Children.Add(_evidenceDate);
        _evidenceBar.Children.Add(_evidenceReviewer);
        _evidenceBar.Children.Add(_evidenceOutcome);
        _evidenceBar.Children.Add(Button("Record evidence", (_, _) => _ = AddEvidenceAsync()));

        _handoverBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        _handoverBar.Children.Add(Caption("Handover"));
        _handoverBar.Children.Add(_handedOverTo);
        _handoverBar.Children.Add(_handedOverAt);
        _handoverBar.Children.Add(_conditions);
        _handoverBar.Children.Add(_signature);
        _handoverBar.Children.Add(_signedBy);
        _handoverBar.Children.Add(Button("Record handover", (_, _) => _ = HandoverAsync()));

        var top = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        top.Children.Add(Caption("Facility"));
        top.Children.Add(_facilityPicker);
        top.Children.Add(Button("Refresh", (_, _) => Load()));

        var panel = new StackPanel();
        panel.Children.Add(top);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("Milestone tracker"));
        panel.Children.Add(_milestones);
        panel.Children.Add(_createBar);
        panel.Children.Add(_updateBar);
        panel.Children.Add(Caption("Milestone evidence"));
        panel.Children.Add(_evidence);
        panel.Children.Add(_evidenceBar);
        panel.Children.Add(Caption("Recorded equipment"));
        panel.Children.Add(_equipment);
        panel.Children.Add(_equipmentNote);
        panel.Children.Add(Caption("Upstream well handovers"));
        panel.Children.Add(_upstream);
        panel.Children.Add(Caption("Handover to operations"));
        panel.Children.Add(_handover);
        panel.Children.Add(_handoverBar);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgConstructionWorkspace? Workspace { get; private set; }

    public OgConstructionFacility? Selected { get; private set; }

    public OgConstructionMilestone? SelectedMilestone { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string SummaryText => _summary.Text;

    public string EquipmentText => _equipmentNote.Text;

    public string UpstreamText => _upstream.Text;

    public string HandoverText => _handover.Text;

    public string DetailText => _detail.Text;

    /// <summary>AC-3: the create and edit bars are hidden while a submission is unresolved.</summary>
    public bool EditControlsVisible => _createBar.Visibility == Visibility.Visible;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _construction
            .LoadAsync(session.FieldId, session.FieldName, Selected?.Id, DateTime.Today)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Workspace = null;
            Selected = null;
            SelectedMilestone = null;
            _milestones.ItemsSource = null;
            _evidence.ItemsSource = null;
            _equipment.ItemsSource = null;
            _facilityPicker.ItemsSource = null;
            _header.Text = "Construction";
            _summary.Text = string.Empty;
            _equipmentNote.Text = string.Empty;
            _upstream.Text = string.Empty;
            _handover.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The construction progress could not be read.";
            ShowState(OgViewState.Failed, "The construction progress could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Workspace = loaded.Value;

        _facilityPicker.ItemsSource = Workspace.Facilities.ToList();
        _facilityPicker.SelectedIndex = Workspace.Facilities.ToList()
            .FindIndex(facility => facility.Id == Workspace.Selected?.Id);

        Selected = Workspace.Selected;

        _milestones.ItemsSource = Selected?.Milestones.ToList();
        _evidence.ItemsSource = null;
        _equipment.ItemsSource = Selected?.Equipment.ToList();
        _equipmentNote.Text = Selected?.EquipmentText ?? string.Empty;
        _upstream.Text = Workspace.UpstreamText;

        _header.Text = Selected is { } facility
            ? $"{facility.Name} · {facility.Kind ?? "(no type)"} · {facility.StateText} · {facility.CompletionText} · " +
              $"{facility.OverdueCount(Workspace.AsOf).ToString(CultureInfo.InvariantCulture)} overdue"
            : "Construction";

        _summary.Text = Selected is { } chosen
            ? $"{Workspace.Summary} · budget {chosen.BudgetText} · spend {chosen.SpendText} · {chosen.VarianceText} · " +
              $"capacity {chosen.CapacityText} · plan v{chosen.PlanVersion.ToString(CultureInfo.InvariantCulture)}"
            : Workspace.Summary;

        _notice.Text = Selected is { } current
            ? current.UnresolvedReason
            : Workspace.IsEmpty
                ? "No facility investment is recorded for this field; add one in the plan before tracking construction."
                : "Choose a facility.";

        _handover.Text = Selected is { } target ? target.HandoverText : string.Empty;

        _handoverBar.Visibility = Selected is { IsHandedOver: false } && Selected.CanHandover
            ? Visibility.Visible
            : Visibility.Collapsed;

        // AC-3: no create or edit while a submission is unresolved, or once the facility is handed over.
        var canEdit = Selected is { CanEdit: true };
        _createBar.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        _updateBar.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        _evidenceBar.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;

        _detail.Text = Selected is { } shown
            ? string.Join(
                Environment.NewLine,
                new[]
                {
                    shown.Describe(),
                    "Milestones: " + (shown.Milestones.Count == 0
                        ? "none tracked"
                        : string.Join(" · ", shown.Milestones.Select(milestone =>
                            $"{milestone.NameText} {milestone.StateText} ({milestone.PlanText(Workspace.AsOf)})"))),
                    shown.UnresolvedReason is { } unresolved
                        ? "Editing is hidden: " + unresolved
                        : "Editing is available.",
                    shown.CanHandover
                        ? "Handover is available: every milestone is complete."
                        : "Handover: " + string.Join("; ", shown.HandoverGaps),
                }.Where(part => !string.IsNullOrWhiteSpace(part)))
            : string.Empty;

        ShowState(Workspace.IsEmpty ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    public async Task<bool> CreateMilestoneAsync()
    {
        if (Selected is not { } facility)
        {
            _notice.Text = "Choose a facility first.";
            return false;
        }

        var created = await _construction
            .CreateMilestoneAsync(
                facility.Id,
                new OgMilestoneValues(_milestoneName.SelectedItem as string, Date(_newPlannedDate.Text)),
                _application.Session.UserId,
                facility.PlanVersion)
            .ConfigureAwait(true);

        return AfterWrite(created, "development.construction.milestone.create", facility.Id);
    }

    public async Task<bool> UpdateMilestoneAsync()
    {
        if (SelectedMilestone is not { } milestone)
        {
            _notice.Text = "Select a milestone first.";
            return false;
        }

        var saved = await _construction
            .UpdateMilestoneAsync(
                milestone.Id,
                new OgMilestoneUpdate(Date(_plannedDate.Text), _notes.Text),
                _application.Session.UserId,
                Selected?.PlanVersion)
            .ConfigureAwait(true);

        return AfterWrite(saved, "development.construction.milestone.update", milestone.Id);
    }

    public async Task<bool> AddEvidenceAsync()
    {
        if (SelectedMilestone is not { } milestone)
        {
            _notice.Text = "Select a milestone first.";
            return false;
        }

        var added = await _construction
            .AddEvidenceAsync(
                milestone.Id,
                new OgMilestoneEvidenceValues(
                    _evidenceDocument.Text,
                    _evidenceReference.Text,
                    Date(_evidenceDate.Text),
                    _evidenceReviewer.Text,
                    _evidenceOutcome.SelectedItem as string),
                _application.Session.UserId,
                Selected?.PlanVersion)
            .ConfigureAwait(true);

        return AfterWrite(added, "development.construction.evidence", milestone.Id);
    }

    public async Task<bool> CompleteMilestoneAsync()
    {
        if (SelectedMilestone is not { } milestone)
        {
            _notice.Text = "Select a milestone first.";
            return false;
        }

        var completed = await _construction
            .CompleteMilestoneAsync(
                milestone.Id,
                Date(_actualDate.Text),
                _application.Session.UserId,
                Selected?.PlanVersion)
            .ConfigureAwait(true);

        return AfterWrite(completed, "development.construction.milestone.complete", milestone.Id);
    }

    public async Task<bool> HandoverAsync()
    {
        if (Selected is not { } facility)
        {
            _notice.Text = "Choose a facility first.";
            return false;
        }

        var conditions = _conditions.Text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var handedOver = await _construction
            .HandoverAsync(
                facility.Id,
                new OgFacilityHandoverValues(
                    _handedOverTo.Text,
                    Date(_handedOverAt.Text),
                    conditions,
                    _signature.Text,
                    _signedBy.Text),
                _application.Session.UserId,
                facility.PlanVersion)
            .ConfigureAwait(true);

        return AfterWrite(handedOver, "development.construction.handover", facility.Id);
    }

    private void SelectFacility()
    {
        if (_facilityPicker.SelectedItem is not OgConstructionFacility facility)
        {
            return;
        }

        var reloaded = _construction
            .LoadAsync(_application.Session.FieldId, _application.Session.FieldName, facility.Id, DateTime.Today)
            .GetAwaiter()
            .GetResult();

        if (reloaded.Success)
        {
            Selected = reloaded.Value?.Selected ?? facility;
        }

        Load();
    }

    private void ShowSelectedMilestone()
    {
        if (_milestones.SelectedItem is not OgConstructionMilestone milestone)
        {
            return;
        }

        SelectedMilestone = milestone;

        _plannedDate.Text = milestone.PlannedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        _notes.Text = milestone.Notes ?? string.Empty;
        _actualDate.Text = milestone.ActualDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

        _evidence.ItemsSource = milestone.Evidence.ToList();

        _detail.Text = string.Join(
            Environment.NewLine,
            new[]
            {
                milestone.Describe(),
                milestone.CompletionText,
                milestone.Evidence.Count == 0
                    ? "No evidence is recorded for this milestone."
                    : "Latest evidence: " + milestone.LatestEvidence?.Describe(),
            });
    }

    private bool AfterWrite(OgResult<OgConstructionWorkspace> result, string verb, string subject)
    {
        _notice.Text = result.Success
            ? result.Message ?? "Saved."
            : result.Message ?? "The change was not applied.";

        if (result.Success)
        {
            _application.AuditLog.Add(verb, "development", subject, "ok", _application.Session.UserId);
        }

        Load();
        return result.Success;
    }

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
