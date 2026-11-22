using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// PRD-06: the production engineer's workspace - which wells underperform, why, what could be done, and the decision that
/// follows from it.
/// </summary>
/// <remarks>
/// The diagnosis is assembled from what the earlier features recorded, so it is evidence rather than a second opinion.
/// With no evidence at all it says "insufficient data" and names what is missing (AC-1). Every option states its basis and
/// a gain carries its unit (AC-2, AC-3). A decision records its rationale and owner (AC-4), and a work order carries the
/// decision it came from and cannot be raised twice (AC-5). Conflicting evidence is shown on both sides and nothing is
/// resolved automatically.
/// </remarks>
public sealed class InterventionsPageView : OgPageBase
{
    public const string Route = "interventions";

    private readonly OgApplication _application;
    private readonly IOgInterventions _interventions;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _diagnosis;
    private readonly TextBlock _evidence;
    private readonly TextBlock _missing;
    private readonly TextBlock _confidence;
    private readonly TextBlock _methods;
    private readonly TextBlock _detail;
    private readonly DataGrid _wells;
    private readonly DataGrid _decisions;
    private readonly DataGrid _optionGrid;

    private readonly ComboBox _methodPicker;
    private readonly TextBox _gain;
    private readonly TextBox _gainUnit;
    private readonly TextBox _cost;
    private readonly TextBox _currency;
    private readonly ComboBox _riskPicker;
    private readonly ComboBox _basisPicker;
    private readonly TextBox _basisText;

    private readonly ComboBox _chosenPicker;
    private readonly TextBox _rationale;
    private readonly TextBox _owner;
    private readonly TextBox _targetDate;

    private readonly TextBox _workOrderRef;

    private readonly List<OgInterventionOptionValues> _options = [];

    private OgInterventionWell? _selectedWell;
    private OgInterventionDecision? _selectedDecision;

    public InterventionsPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _interventions = application.Interventions;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _diagnosis = Body(string.Empty);
        _diagnosis.TextWrapping = TextWrapping.Wrap;
        _diagnosis.Margin = new Thickness(0, 4, 0, 4);

        _evidence = Body(string.Empty);
        _evidence.TextWrapping = TextWrapping.Wrap;
        _evidence.Margin = new Thickness(0, 0, 0, 4);

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 0, 0, 4);

        _confidence = Body(string.Empty);
        _confidence.TextWrapping = TextWrapping.Wrap;
        _confidence.Margin = new Thickness(0, 0, 0, 8);

        _methods = Body(string.Empty);
        _methods.TextWrapping = TextWrapping.Wrap;
        _methods.Margin = new Thickness(0, 0, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _wells = new DataGrid
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
                Text("Well", "NameText", 140),
                Text("Status", "StatusText", 110),
                Text("Rate", "RateText", 110),
                Text("Average", "AverageText", 150),
                Text("Change", "ChangeText", 110),
                Text("Diagnosis", "DiagnosisText", 260),
                Text("Confidence", "ConfidenceText", 120),
            },
        };

        Named(_wells, "Wells and their diagnosis");
        _wells.SelectionChanged += (_, _) => SelectWell();

        _decisions = new DataGrid
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
                Text("Well", "NameText", 130),
                Text("Chosen", "ChosenText", 170),
                Text("Owner", "Owner", 110),
                Text("Target", "TargetText", 100),
                Text("State", "StateText", 90),
                Text("Work order", "WorkOrderRef", 140),
            },
        };

        Named(_decisions, "Recorded decisions");
        _decisions.SelectionChanged += (_, _) => SelectDecision();

        _optionGrid = new DataGrid
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
                Text("Method", "MethodText", 150),
                Text("Gain", "GainText", 120),
                Text("Cost", "CostText", 120),
                Text("Risk", "RiskText", 80),
                Text("Basis", "BasisDescription", 240),
            },
        };

        Named(_optionGrid, "Candidate options");

        _methodPicker = new ComboBox { MinWidth = 150, Margin = new Thickness(0, 0, 8, 0) };
        Named(_methodPicker, "Option method");

        _gain = Field("Estimated gain", 100);
        _gainUnit = Field("Gain unit", 80);
        _cost = Field("Estimated cost", 100);
        _currency = Field("Currency", 70);
        _currency.Text = "USD";

        _riskPicker = new ComboBox
        {
            MinWidth = 100,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = InterventionRules.Risks,
        };

        Named(_riskPicker, "Option risk");
        _riskPicker.SelectedIndex = 1;

        _basisPicker = new ComboBox
        {
            MinWidth = 110,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = InterventionRules.Bases,
        };

        Named(_basisPicker, "Option basis");
        _basisPicker.SelectedIndex = 0;

        _basisText = Field("Basis evidence (which test, analogue or model)", 240);

        _chosenPicker = new ComboBox { MinWidth = 150, Margin = new Thickness(0, 0, 8, 0) };
        Named(_chosenPicker, "Chosen option");

        _rationale = Field("Rationale", 220);
        _owner = Field("Owner", 110);
        _owner.Text = _application.Session.UserId;

        _targetDate = Field("Target date yyyy-MM-dd", 120);
        _targetDate.Text = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        _workOrderRef = Field("Work order reference", 160);

        var top = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        top.Children.Add(Button("Refresh", (_, _) => Load()));

        var optionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        optionBar.Children.Add(Caption("Candidate"));
        optionBar.Children.Add(_methodPicker);
        optionBar.Children.Add(_gain);
        optionBar.Children.Add(_gainUnit);
        optionBar.Children.Add(_cost);
        optionBar.Children.Add(_currency);
        optionBar.Children.Add(_riskPicker);
        optionBar.Children.Add(_basisPicker);
        optionBar.Children.Add(_basisText);
        optionBar.Children.Add(Button("Add option", (_, _) => AddOption()));
        optionBar.Children.Add(Button("Remove selected", (_, _) => RemoveOption()));

        var decisionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        decisionBar.Children.Add(Caption("Decision"));
        decisionBar.Children.Add(_chosenPicker);
        decisionBar.Children.Add(_rationale);
        decisionBar.Children.Add(_owner);
        decisionBar.Children.Add(_targetDate);
        decisionBar.Children.Add(Button("Record decision", (_, _) => _ = RecordAsync()));

        var workOrderBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        workOrderBar.Children.Add(Caption("Work order"));
        workOrderBar.Children.Add(_workOrderRef);
        workOrderBar.Children.Add(Button("Create work order", (_, _) => _ = CreateWorkOrderAsync()));
        workOrderBar.Children.Add(Button("Retry", (_, _) => _ = RetryAsync()));

        var panel = new StackPanel();
        panel.Children.Add(top);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("Wells"));
        panel.Children.Add(_wells);
        panel.Children.Add(Caption("Diagnosis"));
        panel.Children.Add(_diagnosis);
        panel.Children.Add(Caption("Evidence"));
        panel.Children.Add(_evidence);
        panel.Children.Add(Caption("Confidence"));
        panel.Children.Add(_confidence);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Candidate options"));
        panel.Children.Add(optionBar);
        panel.Children.Add(_optionGrid);
        panel.Children.Add(decisionBar);
        panel.Children.Add(Caption("Decisions"));
        panel.Children.Add(_decisions);
        panel.Children.Add(workOrderBar);
        panel.Children.Add(Caption("Method reference"));
        panel.Children.Add(_methods);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgInterventionWorkspace? Workspace { get; private set; }

    public OgInterventionWell? SelectedWell => _selectedWell;

    public OgInterventionDecision? SelectedDecision => _selectedDecision;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string DiagnosisText => _diagnosis.Text;

    public string EvidenceText => _evidence.Text;

    public string MissingText => _missing.Text;

    public string ConfidenceText => _confidence.Text;

    public string MethodsText => _methods.Text;

    public string DetailText => _detail.Text;

    public bool MethodReferenceMissing => Workspace?.MethodReferenceMissing ?? false;

    public IReadOnlyList<OgInterventionOptionValues> Options => _options;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _interventions
            .LoadAsync(session.FieldId, session.FieldName, _selectedWell?.WellId, DateTime.Today)
            .GetAwaiter()
            .GetResult();

        return Show(loaded);
    }

    public void AddOption()
    {
        if (_methodPicker.SelectedItem is not string method)
        {
            _notice.Text = MethodReferenceMissing
                ? Workspace?.MethodsText ?? "No intervention method is recorded."
                : "Choose the method of the option first.";
            return;
        }

        if (_options.Any(option =>
                string.Equals(option.Method, method, StringComparison.OrdinalIgnoreCase)))
        {
            _notice.Text = $"'{method}' is already a candidate; a decision chooses between different methods.";
            return;
        }

        var basis = _basisPicker.SelectedItem as string ?? InterventionRules.BasisTest;

        if (string.IsNullOrWhiteSpace(_basisText.Text))
        {
            _notice.Text =
                $"The option '{method}' must name the evidence behind its {basis.ToLowerInvariant()} basis " +
                "(which test, which analogue well, which model).";
            return;
        }

        var gain = Parse(_gain.Text);

        if (gain is not null && string.IsNullOrWhiteSpace(_gainUnit.Text))
        {
            _notice.Text = $"The estimated gain of '{method}' needs the unit it is measured in.";
            return;
        }

        _options.Add(new OgInterventionOptionValues(
            method,
            gain,
            _gainUnit.Text,
            Parse(_cost.Text),
            _currency.Text,
            _riskPicker.SelectedItem as string,
            basis,
            _basisText.Text));

        RefreshOptions();
        _notice.Text = $"'{method}' added as a candidate.";
    }

    public void RemoveOption()
    {
        if (_optionGrid.SelectedItem is not OgInterventionOptionValues selected)
        {
            _notice.Text = "Select a candidate to remove.";
            return;
        }

        _options.Remove(selected);
        RefreshOptions();
        _notice.Text = $"'{selected.Method}' removed.";
    }

    public async Task<bool> RecordAsync()
    {
        if (_selectedWell is not { } well)
        {
            _notice.Text = "Select the well the decision is about.";
            return false;
        }

        var recorded = await _interventions
            .RecordDecisionAsync(
                new OgInterventionDecisionValues(
                    _application.Session.FieldId,
                    well.WellId,
                    _options.ToList(),
                    _chosenPicker.SelectedItem as string,
                    _rationale.Text,
                    _owner.Text,
                    Date(_targetDate.Text)),
                _application.Session.UserId)
            .ConfigureAwait(true);

        var shown = Show(recorded);

        if (recorded.Success)
        {
            _application.AuditLog.Add(
                "production.intervention.decision",
                "production",
                well.WellId,
                "ok",
                _application.Session.UserId);

            _options.Clear();
            RefreshOptions();
        }

        return shown;
    }

    public async Task<bool> CreateWorkOrderAsync()
    {
        if (_selectedDecision is not { } decision)
        {
            _notice.Text = "Select the decision the work order is raised from.";
            return false;
        }

        var created = await _interventions
            .CreateWorkOrderAsync(decision.Id, _workOrderRef.Text, _application.Session.UserId)
            .ConfigureAwait(true);

        var shown = Show(created);

        if (created.Success)
        {
            _application.AuditLog.Add(
                "production.intervention.workorder",
                "production",
                decision.Id,
                "ok",
                _application.Session.UserId);
        }

        return shown;
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _interventions
            .RetryWorkOrderAsync(_workOrderRef.Text, _application.Session.UserId)
            .ConfigureAwait(true);

        return Show(retried);
    }

    private void SelectWell()
    {
        if (_wells.SelectedItem is not OgInterventionWell well)
        {
            return;
        }

        _selectedWell = well;
        ShowDiagnosis(well);

        var loaded = _interventions
            .LoadAsync(_application.Session.FieldId, _application.Session.FieldName, well.WellId, DateTime.Today)
            .GetAwaiter()
            .GetResult();

        if (loaded.Success && loaded.Value is not null)
        {
            Workspace = loaded.Value;
            _decisions.ItemsSource = loaded.Value.Decisions.ToList();
        }
    }

    private void SelectDecision()
    {
        if (_decisions.SelectedItem is not OgInterventionDecision decision)
        {
            return;
        }

        _selectedDecision = decision;

        if (!decision.HasWorkOrder && string.IsNullOrWhiteSpace(_workOrderRef.Text))
        {
            _workOrderRef.Text = "WO-" + decision.Id[..Math.Min(8, decision.Id.Length)].ToUpperInvariant();
        }

        _detail.Text = decision.Describe() +
                       Environment.NewLine +
                       "Options: " + decision.OptionsText;
    }

    private void ShowDiagnosis(OgInterventionWell well)
    {
        _diagnosis.Text = well.Describe();
        _evidence.Text = well.EvidenceText;
        _missing.Text = "Missing: " + well.MissingText;
        _confidence.Text =
            $"Confidence {well.ConfidenceText} · {well.FlagsText}" +
            (well.HasConflicts ? " · conflicting evidence is shown on both sides" : string.Empty);
    }

    private void RefreshOptions()
    {
        _optionGrid.ItemsSource = null;
        _optionGrid.ItemsSource = _options.ToList();

        var methods = _options
            .Select(option => option.Method)
            .Where(method => !string.IsNullOrWhiteSpace(method))
            .Select(method => method!)
            .ToList();

        _chosenPicker.ItemsSource = methods;
        _chosenPicker.SelectedIndex = methods.Count > 0 ? 0 : -1;
    }

    private bool Show(OgResult<OgInterventionWorkspace> result)
    {
        if (!result.Success || result.Value is null)
        {
            LastFailed = true;
            Workspace = null;
            _selectedWell = null;
            _selectedDecision = null;
            _wells.ItemsSource = null;
            _decisions.ItemsSource = null;
            _optionGrid.ItemsSource = null;
            _methodPicker.ItemsSource = null;
            _header.Text = "Interventions";
            _diagnosis.Text = string.Empty;
            _evidence.Text = string.Empty;
            _missing.Text = string.Empty;
            _confidence.Text = string.Empty;
            _methods.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = result.Message ?? "The intervention workspace could not be read.";
            ShowState(OgViewState.Failed, "The intervention workspace could not be read.", result.Reference);
            return false;
        }

        LastFailed = false;
        Workspace = result.Value;

        var workspace = result.Value;

        _wells.ItemsSource = workspace.Wells.ToList();
        _decisions.ItemsSource = workspace.Decisions.ToList();
        _methodPicker.ItemsSource = workspace.Methods.ToList();
        _methodPicker.SelectedIndex = workspace.Methods.Count > 0 ? 0 : -1;

        _header.Text = $"{workspace.FieldName} · {workspace.Summary}";

        _notice.Text = workspace.MethodReferenceMissing
            ? workspace.MethodsText
            : workspace.Wells.Count == 0
                ? "No well is recorded for this field."
                : $"{workspace.UnderperformingCount.ToString(CultureInfo.InvariantCulture)} well(s) are flagged; " +
                  "select a well to read its diagnosis.";

        _methods.Text = workspace.MethodsText;

        _selectedWell = workspace.Selected;
        _wells.SelectedItem = _selectedWell;

        if (_selectedWell is { } well)
        {
            ShowDiagnosis(well);
        }

        if (workspace.Decisions.Count > 0)
        {
            _selectedDecision = workspace.Decisions[0];
            _decisions.SelectedItem = _selectedDecision;
        }

        ShowState(workspace.Wells.Count == 0 ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    private static decimal? Parse(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

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
