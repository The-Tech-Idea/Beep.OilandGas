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
/// DEV-02: the field development plan wizard, review and baseline history.
/// </summary>
/// <remarks>
/// The wizard saves step values into a draft; a submit locks the draft and lists any missing steps (AC-1). A review
/// decision records its rationale, a return names the step to correct (AC-3), and the author cannot review their own
/// plan (AC-4). An approval appends a baseline; a revision starts a new draft from the latest baseline and approving
/// it appends another baseline without changing the first (AC-2, AC-6). Costs are decimals with a currency (AC-5).
/// </remarks>
public sealed class FdpWizardPageView : OgPageBase
{
    public const string Route = "fdpwizard";

    private readonly OgApplication _application;
    private readonly IOgFdpWizard _wizard;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _detail;
    private readonly ComboBox _planPicker;
    private readonly TextBox _name;
    private readonly ComboBox _step;
    private readonly TextBox _scope;
    private readonly TextBox _reservoir;
    private readonly TextBox _wellCount;
    private readonly TextBox _facility;
    private readonly TextBox _cost;
    private readonly TextBox _currency;
    private readonly TextBox _risk;
    private readonly ComboBox _decision;
    private readonly TextBox _rationale;
    private readonly TextBox _returnedStep;
    private readonly TextBox _conditions;
    private readonly TextBox _reviewer;
    private readonly DataGrid _baselines;

    public FdpWizardPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _wizard = application.FdpWizard;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _planPicker = new ComboBox { MinWidth = 260, Margin = new Thickness(0, 0, 8, 0) };
        Named(_planPicker, "Development plan");
        _planPicker.SelectionChanged += (_, _) => ShowSelected();

        _name = Field("New plan name", 220);
        _step = new ComboBox
        {
            MinWidth = 200,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = Enumerable.Range(1, OgFdpWizardService.Steps.Count)
                .Select(step => $"{step.ToString(CultureInfo.InvariantCulture)}. {OgFdpWizardService.StepName(step)}")
                .ToList(),
            SelectedIndex = 0,
        };

        Named(_step, "Wizard step");

        _scope = Field("Scope", 260);
        _reservoir = Field("Reservoirs", 220);
        _wellCount = Field("Well count", 80);
        _facility = Field("Facilities and capacity", 220);
        _cost = Field("Cost", 100);
        _currency = Field("Currency", 70);
        _risk = Field("Risk and mitigations", 240);

        _decision = new ComboBox
        {
            MinWidth = 180,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = OgFdpDecision.Decisions,
            SelectedIndex = 0,
        };

        Named(_decision, "Review decision");

        _rationale = Field("Rationale", 260);
        _returnedStep = Field("Returned step (1-6)", 90);
        _conditions = Field("Conditions (one per line)", 220);
        _reviewer = Field("Reviewer", 130);
        _reviewer.Text = _application.Session.UserId;

        _baselines = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 200,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Version", "Version", 70),
                Text("Approved by", "ApprovedBy", 110),
                Text("Approved at", "ApprovedAt", 120),
                Text("Cost", "CostText", 110),
                Text("Conditions", "ConditionsText", 260),
            },
        };

        Named(_baselines, "Approved baselines");

        var planBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        planBar.Children.Add(Caption("Plan"));
        planBar.Children.Add(_planPicker);
        planBar.Children.Add(_name);
        planBar.Children.Add(Button("Create draft", (_, _) => _ = CreateAsync()));
        planBar.Children.Add(Button("Refresh", (_, _) => Load()));

        var wizard = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wizard.Children.Add(Caption("Step"));
        wizard.Children.Add(_step);
        wizard.Children.Add(_scope);
        wizard.Children.Add(_reservoir);
        wizard.Children.Add(Caption("Wells"));
        wizard.Children.Add(_wellCount);
        wizard.Children.Add(_facility);
        wizard.Children.Add(Caption("Cost"));
        wizard.Children.Add(_cost);
        wizard.Children.Add(_currency);
        wizard.Children.Add(_risk);
        wizard.Children.Add(Button("Save step", (_, _) => _ = SaveAsync()));
        wizard.Children.Add(Button("Submit", (_, _) => _ = SubmitAsync()));
        wizard.Children.Add(Button("Revise", (_, _) => _ = ReviseAsync()));
        wizard.Children.Add(Button("Retry", (_, _) => _ = RetryAsync()));

        var review = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        review.Children.Add(Caption("Review"));
        review.Children.Add(_decision);
        review.Children.Add(_rationale);
        review.Children.Add(_returnedStep);
        review.Children.Add(_conditions);
        review.Children.Add(_reviewer);
        review.Children.Add(Button("Record decision", (_, _) => _ = ReviewAsync()));

        var panel = new StackPanel();
        panel.Children.Add(planBar);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(wizard);
        panel.Children.Add(review);
        panel.Children.Add(_detail);
        panel.Children.Add(Caption("Baselines"));
        panel.Children.Add(_baselines);

        ShowContent(panel);

        Load();
    }

    public OgFdpWorkspace? Workspace { get; private set; }

    public OgFdpPlan? SelectedPlan { get; private set; }

    public OgFdpDraft? SelectedDraft => SelectedPlan?.Current;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string DetailText => _detail.Text;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _wizard
            .LoadAsync(session.FieldId, session.FieldName)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Workspace = null;
            SelectedPlan = null;
            _planPicker.ItemsSource = null;
            _baselines.ItemsSource = null;
            _header.Text = "Field development plan";
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The development plans could not be read.";
            ShowState(OgViewState.Failed, "The development plans could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Workspace = loaded.Value;
        var previous = SelectedPlan?.Id;

        _planPicker.ItemsSource = Workspace.Plans.ToList();
        _planPicker.SelectedItem = Workspace.Plans.FirstOrDefault(plan =>
            string.Equals(plan.Id, previous, StringComparison.OrdinalIgnoreCase)) ?? Workspace.Plans.FirstOrDefault();

        _header.Text =
            $"{Workspace.Plans.Count.ToString(CultureInfo.InvariantCulture)} plan(s) · " +
            $"{(string.IsNullOrWhiteSpace(session.FieldName) ? "(no field selected)" : session.FieldName)} · " +
            $"{Workspace.BaselineCount.ToString(CultureInfo.InvariantCulture)} baseline(s)";

        _notice.Text = Workspace.IsEmpty
            ? "No development plan is recorded for this field. Create a draft to begin."
            : "Select a plan; a submitted plan is read-only until it is reviewed.";

        ShowState(Workspace.IsEmpty ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    public async Task<bool> CreateAsync()
    {
        var session = _application.Session;

        var created = await _wizard
            .CreateDraftAsync(session.FieldId, session.FieldName, _name.Text, session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(created, "development.fdp.create", $"name={_name.Text.Trim()}");
    }

    public async Task<bool> SaveAsync()
    {
        if (SelectedDraft is not { } draft)
        {
            _notice.Text = "Select a plan first.";
            return false;
        }

        if (!int.TryParse(_wellCount.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var wells))
        {
            wells = 0;
        }

        if (!decimal.TryParse(_cost.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var cost))
        {
            cost = 0m;
        }

        var saved = await _wizard
            .SaveDraftAsync(
                draft.Id,
                new OgFdpDraftValues(
                    _scope.Text,
                    _reservoir.Text,
                    wells,
                    _facility.Text,
                    cost,
                    _currency.Text,
                    _risk.Text,
                    _step.SelectedIndex + 1),
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(saved, "development.fdp.save", $"step={_step.SelectedIndex + 1}");
    }

    public async Task<bool> SubmitAsync()
    {
        if (SelectedDraft is not { } draft)
        {
            _notice.Text = "Select a plan first.";
            return false;
        }

        var submitted = await _wizard.SubmitAsync(draft.Id, _application.Session.UserId).ConfigureAwait(true);

        return AfterWrite(submitted, "development.fdp.submit", $"draft={draft.Id}");
    }

    public async Task<bool> ReviewAsync()
    {
        if (SelectedDraft is not { } draft)
        {
            _notice.Text = "Select a plan first.";
            return false;
        }

        var conditions = _conditions.Text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var decision = new OgFdpDecision(
            _decision.SelectedItem as string ?? OgFdpDecision.Approve,
            _rationale.Text,
            int.TryParse(_returnedStep.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var step)
                ? step
                : null,
            conditions,
            _reviewer.Text);

        var reviewed = await _wizard.ReviewAsync(draft.Id, decision).ConfigureAwait(true);

        return AfterWrite(reviewed, "development.fdp.review", $"decision={decision.Decision}");
    }

    public async Task<bool> ReviseAsync()
    {
        if (SelectedDraft is not { } draft)
        {
            _notice.Text = "Select a plan first.";
            return false;
        }

        var revised = await _wizard.ReviseAsync(draft.Id, _application.Session.UserId).ConfigureAwait(true);

        return AfterWrite(revised, "development.fdp.revise", $"from={draft.Id}");
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _wizard.RetrySubmitAsync().ConfigureAwait(true);
        return AfterWrite(retried, "development.fdp.submit", "retry");
    }

    private bool AfterWrite(OgResult<OgFdpPlan> result, string verb, string detail)
    {
        _notice.Text = result.Success
            ? result.Message ?? "Saved."
            : result.Message ?? "The change was not applied.";

        if (result.Success)
        {
            _application.AuditLog.Add(verb, "development", SelectedPlan?.Id ?? string.Empty, "ok", detail);
        }

        // Reload either way: a refused submit still moves the wizard to the first missing step.
        Load();
        return result.Success;
    }

    private void ShowSelected()
    {
        SelectedPlan = _planPicker.SelectedItem as OgFdpPlan;
        var draft = SelectedDraft;

        if (draft is null)
        {
            _detail.Text = SelectedPlan is null ? "Select a plan." : "This plan has no draft.";
            _baselines.ItemsSource = null;
            return;
        }

        _step.SelectedIndex = Math.Clamp(draft.CurrentStep, 1, OgFdpWizardService.Steps.Count) - 1;
        _scope.Text = draft.Scope ?? string.Empty;
        _reservoir.Text = draft.Reservoir ?? string.Empty;
        _wellCount.Text = draft.WellCount.ToString(CultureInfo.InvariantCulture);
        _facility.Text = draft.Facility ?? string.Empty;
        _cost.Text = draft.CostAmount.ToString(CultureInfo.InvariantCulture);
        _currency.Text = draft.CostCurrency ?? string.Empty;
        _risk.Text = draft.Risk ?? string.Empty;
        _reviewer.Text = string.Equals(draft.Author, _application.Session.UserId, StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : _application.Session.UserId;

        _baselines.ItemsSource = SelectedPlan?.Baselines
            .Select(baseline => new
            {
                baseline.Version,
                baseline.ApprovedBy,
                ApprovedAt = baseline.ApprovedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                baseline.CostText,
                ConditionsText = baseline.Conditions.Count == 0 ? string.Empty : string.Join("; ", baseline.Conditions),
            })
            .ToList();

        var reviews = SelectedPlan?.ReviewsFor(draft.Id) ?? [];
        var missing = OgFdpWizardService.MissingSteps(draft, hasSchedule: true);

        _detail.Text = string.Join(
            Environment.NewLine,
            new[]
            {
                draft.Describe(),
                MissingText(missing),
                draft.ReturnText,
                reviews.Count == 0 ? null : "Reviews: " + string.Join(" · ", reviews.Select(review => review.Describe())),
                SelectedPlan is null || SelectedPlan.Baselines.Count == 0
                    ? "No baseline has been approved yet."
                    : SelectedPlan.Baselines[0].Describe(),
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

        ShowState(OgViewState.Ready, _notice.Text);
    }

    private static string MissingText(IReadOnlyList<int> missing) =>
        missing.Count == 0
            ? "All wizard steps have a value."
            : "Missing: " + string.Join(", ", missing.Select(step =>
                $"{step.ToString(CultureInfo.InvariantCulture)} ({OgFdpWizardService.StepName(step)})"));

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
