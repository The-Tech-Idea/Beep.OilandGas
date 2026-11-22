using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Beep.OilandGas.LifeCycle.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Economics;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ECO-03: the investment review queue - where a proposal waits for a decision, and the handoff that turns an approval into
/// one AFE.
/// </summary>
/// <remarks>
/// A decision records its actor, date, rationale and conditions (AC-1, SG-09-5). A decision on stale economics is refused
/// (AC-2), the submitter cannot decide (AC-3), a handoff creates exactly one AFE (AC-4) that carries the conditions
/// (AC-5), and a lost acknowledgement does not duplicate it (AC-6).
/// </remarks>
public sealed class InvestmentReviewPageView : OgPageBase
{
    public const string Route = "investmentreview";

    private readonly OgApplication _application;
    private readonly IOgInvestmentReview _review;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _summary;
    private readonly TextBlock _detail;
    private readonly TextBlock _economics;
    private readonly TextBlock _decision;
    private readonly TextBlock _conditions;
    private readonly TextBlock _handoff;
    private readonly TextBox _title;
    private readonly TextBox _entity;
    private readonly TextBox _amount;
    private readonly TextBox _due;
    private readonly TextBox _economicsId;
    private readonly TextBox _npv;
    private readonly TextBox _risks;
    private readonly TextBox _rationale;
    private readonly TextBox _conditionList;
    private readonly TextBox _amendAmount;
    private readonly ComboBox _class;
    private readonly ComboBox _currency;
    private readonly ComboBox _decisionPicker;
    private readonly DataGrid _queue;
    private readonly Button _submitButton;
    private readonly Button _decideButton;
    private readonly Button _amendButton;
    private readonly Button _reviewButton;
    private readonly Button _handoffButton;

    private OgInvestmentWorkspace? _workspace;
    private OgInvestmentProposal? _selected;

    public InvestmentReviewPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _review = application.InvestmentReview;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 4, 0, 4);

        _reference = Body(string.Empty);
        _reference.TextWrapping = TextWrapping.Wrap;
        _reference.Margin = new Thickness(0, 0, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 0, 0, 4);

        _economics = Body(string.Empty);
        _economics.TextWrapping = TextWrapping.Wrap;
        _economics.Margin = new Thickness(0, 0, 0, 4);

        _decision = Body(string.Empty);
        _decision.TextWrapping = TextWrapping.Wrap;
        _decision.Margin = new Thickness(0, 0, 0, 4);

        _conditions = Body(string.Empty);
        _conditions.TextWrapping = TextWrapping.Wrap;
        _conditions.Margin = new Thickness(0, 0, 0, 4);

        _handoff = Body(string.Empty);
        _handoff.TextWrapping = TextWrapping.Wrap;
        _handoff.Margin = new Thickness(0, 0, 0, 6);

        _title = Field("Title", 220);
        _entity = Field("Well or entity", 130);
        _amount = Field("Amount", 120);
        _due = Field("Decision due yyyy-MM-dd", 170);
        _economicsId = Field("Economics id", 160);
        _npv = Field("Economics NPV", 130);
        _risks = Field("Risks", 260);
        _rationale = Field("Rationale", 280);
        _conditionList = Field("Conditions, one per line", 320);
        _conditionList.AcceptsReturn = true;
        _conditionList.MinHeight = 54;
        _conditionList.TextWrapping = TextWrapping.Wrap;
        _amendAmount = Field("Amended amount", 130);

        _class = Picker("Class", 130);
        _currency = Picker("Currency", 100);
        _decisionPicker = Picker("Decision", 160);

        _queue = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 130,
            MaxHeight = 210,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Proposal", "Title", 210),
                Text("Entity", "EntityId", 110),
                Text("Amount", "AmountText", 150),
                Text("Class", "Class", 90),
                Text("State", "State", 100),
                Text("Submitted by", "SubmittedBy", 110),
                Text("Age", "AgeText", 80),
                Text("Due", "DueText", 130),
            },
        };

        Named(_queue, "Investment proposals awaiting review");
        _queue.SelectionChanged += (_, _) => SelectProposal();

        _submitButton = Button("Submit", (_, _) => _ = SubmitAsync());
        _decideButton = Button("Record decision", (_, _) => _ = DecideAsync());
        _decideButton.IsEnabled = false;
        _amendButton = Button("Amend amount", (_, _) => _ = AmendAsync());
        _amendButton.IsEnabled = false;
        _reviewButton = Button("Re-review", (_, _) => _ = ReviewAsync());
        _reviewButton.IsEnabled = false;
        _handoffButton = Button("Hand off to AFE", (_, _) => _ = HandoffAsync());
        _handoffButton.IsEnabled = false;

        var submitBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        submitBar.Children.Add(Caption("Title"));
        submitBar.Children.Add(_title);
        submitBar.Children.Add(Caption("Entity"));
        submitBar.Children.Add(_entity);
        submitBar.Children.Add(Caption("Amount"));
        submitBar.Children.Add(_amount);
        submitBar.Children.Add(_currency);

        var submitBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        submitBar2.Children.Add(Caption("Class"));
        submitBar2.Children.Add(_class);
        submitBar2.Children.Add(Caption("Due"));
        submitBar2.Children.Add(_due);
        submitBar2.Children.Add(Caption("Economics"));
        submitBar2.Children.Add(_economicsId);
        submitBar2.Children.Add(Caption("NPV"));
        submitBar2.Children.Add(_npv);
        submitBar2.Children.Add(_submitButton);

        var riskBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        riskBar.Children.Add(Caption("Risks"));
        riskBar.Children.Add(_risks);

        var decisionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        decisionBar.Children.Add(Caption("Decision"));
        decisionBar.Children.Add(_decisionPicker);
        decisionBar.Children.Add(Caption("Rationale"));
        decisionBar.Children.Add(_rationale);
        decisionBar.Children.Add(_decideButton);

        var conditionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        conditionBar.Children.Add(Caption("Conditions"));
        conditionBar.Children.Add(_conditionList);

        var amendBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        amendBar.Children.Add(Caption("Amended amount"));
        amendBar.Children.Add(_amendAmount);
        amendBar.Children.Add(_amendButton);
        amendBar.Children.Add(_reviewButton);

        var handoffBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        handoffBar.Children.Add(_handoffButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Submit a proposal"));
        panel.Children.Add(submitBar);
        panel.Children.Add(submitBar2);
        panel.Children.Add(riskBar);
        panel.Children.Add(Caption("Decide"));
        panel.Children.Add(decisionBar);
        panel.Children.Add(conditionBar);
        panel.Children.Add(Caption("Amend and review"));
        panel.Children.Add(amendBar);
        panel.Children.Add(Caption("Handoff"));
        panel.Children.Add(handoffBar);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Queue"));
        panel.Children.Add(_queue);
        panel.Children.Add(Caption("Selected proposal"));
        panel.Children.Add(_summary);
        panel.Children.Add(_detail);
        panel.Children.Add(Caption("Economics"));
        panel.Children.Add(_economics);
        panel.Children.Add(Caption("Decision"));
        panel.Children.Add(_decision);
        panel.Children.Add(Caption("Conditions"));
        panel.Children.Add(_conditions);
        panel.Children.Add(Caption("Handoff"));
        panel.Children.Add(_handoff);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _title, _entity, _amount, _due, _economicsId, _npv, _risks, _rationale, _conditionList,
                     _amendAmount,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _class.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgInvestmentWorkspace? Workspace => _workspace;

    public OgInvestmentProposal? Proposal => _selected;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string SummaryText => _summary.Text;

    public string DetailText => _detail.Text;

    public string EconomicsText => _economics.Text;

    public string DecisionText => _decision.Text;

    public string ConditionsText => _conditions.Text;

    public string HandoffText => _handoff.Text;

    public bool CanSubmit => InvestmentReviewRules.Missing(Entry()).Count == 0;

    public bool CanDecide => _selected is { IsQueued: true } proposal &&
                             proposal.HasEconomics &&
                             !proposal.EconomicsStale;

    public bool CanAmend => _selected is { IsQueued: true };

    public bool CanReview => _selected is { IsQueued: true, EconomicsStale: true };

    public bool CanHandOff => _selected is { IsDecided: true, IsHandedOff: false } proposal && proposal.Authorises;

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _review.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _queue.ItemsSource = null;
            _header.Text = "Investment review";
            _notice.Text = loaded.Message ?? "The queue could not be read.";
            ShowState(OgViewState.Failed, "The queue could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public Task<bool> SubmitAsync() =>
        RunAsync(() => _review.SubmitAsync(Entry(), _application.Session.UserId),
            "economics.investment.submit", "Submitted");

    public Task<bool> DecideAsync()
    {
        if (_selected is not { } proposal)
        {
            _notice.Text = "Select the proposal to decide.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _review.DecideAsync(
                proposal.Id,
                _decisionPicker.SelectedItem as string ?? string.Empty,
                _rationale.Text ?? string.Empty,
                _conditionList.Text ?? string.Empty,
                _application.Session.UserId),
            "economics.investment.decide",
            "Decision recorded");
    }

    public Task<bool> AmendAsync()
    {
        if (_selected is not { } proposal)
        {
            _notice.Text = "Select the proposal to amend.";
            return Task.FromResult(false);
        }

        var amount = Parse(_amendAmount.Text);

        if (amount is null)
        {
            _notice.Text = "An amendment needs its amount.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _review.AmendAsync(proposal.Id, amount.Value, _application.Session.UserId),
            "economics.investment.amend",
            "Amended - the review is now stale until it is reviewed again");
    }

    public Task<bool> ReviewAsync()
    {
        if (_selected is not { } proposal)
        {
            _notice.Text = "Select the proposal to review.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _review.ReviewAsync(proposal.Id, _application.Session.UserId),
            "economics.investment.review",
            "Re-reviewed at the current amount");
    }

    public Task<bool> HandoffAsync()
    {
        if (_selected is not { } proposal)
        {
            _notice.Text = "Select the proposal to hand off.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _review.HandoffAsync(proposal.Id, _application.Session.UserId),
            "economics.investment.handoff",
            "Handed off as one AFE");
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgInvestmentWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _notice.Text = outcome.Message ?? "The action was refused.";
            ShowState(OgViewState.Failed, "The action was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(outcome.Value);

        _application.AuditLog.Add(
            auditAction,
            "economics",
            outcome.Value.Selected?.Title ?? "(no proposal)",
            "ok",
            _application.Session.UserId);

        _notice.Text = done + ". " + outcome.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    private void Apply(OgInvestmentWorkspace workspace)
    {
        _workspace = workspace;

        _class.ItemsSource = workspace.Classes.ToList();
        _class.SelectedIndex = _class.SelectedIndex < 0 ? 0 : _class.SelectedIndex;

        _currency.ItemsSource = workspace.Currencies.ToList();
        _currency.SelectedIndex = _currency.SelectedIndex < 0 ? 0 : _currency.SelectedIndex;

        _decisionPicker.ItemsSource = workspace.Decisions.ToList();
        _decisionPicker.SelectedIndex = _decisionPicker.SelectedIndex < 0 ? 0 : _decisionPicker.SelectedIndex;

        _queue.ItemsSource = workspace.Queue.ToList();

        _header.Text = "Economics · investment review";
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The queue is unavailable: " + workspace.HistoryReason + "."
            : workspace.SummaryText;

        _selected = workspace.Selected;
        ShowSelected();
        RefreshEnabled();
    }

    private void ShowSelected()
    {
        if (_selected is not { } proposal)
        {
            _summary.Text = "no proposal is selected";
            _detail.Text = string.Empty;
            _economics.Text = string.Empty;
            _decision.Text = string.Empty;
            _conditions.Text = string.Empty;
            _handoff.Text = string.Empty;
            return;
        }

        _summary.Text = proposal.Describe();
        _detail.Text =
            $"submitted by {proposal.SubmittedBy ?? "(unknown)"} on " +
            $"{proposal.SubmittedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "an unrecorded date"}" +
            " · " + proposal.DueText + " · risks " + (proposal.Risks ?? "not recorded");
        _economics.Text = proposal.EconomicsText;
        _decision.Text = proposal.DecisionText + " · " + proposal.RationaleText;
        _conditions.Text = proposal.ConditionsText;
        _handoff.Text = proposal.HandoffText;
    }

    private void SelectProposal()
    {
        if (_queue.SelectedItem is not OgInvestmentProposal selected)
        {
            return;
        }

        _selected = selected;
        ShowSelected();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var missing = InvestmentReviewRules.Missing(Entry());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The proposal needs " + string.Join(", ", missing) + ".";

        _submitButton.IsEnabled = missing.Count == 0;
        _decideButton.IsEnabled = CanDecide;
        _amendButton.IsEnabled = CanAmend;
        _reviewButton.IsEnabled = CanReview;
        _handoffButton.IsEnabled = CanHandOff;
    }

    private OgInvestmentEntry Entry() =>
        new(
            Empty(_title.Text),
            Empty(_entity.Text),
            Parse(_amount.Text),
            _currency.SelectedItem as string,
            _class.SelectedItem as string,
            Date(_due.Text),
            Empty(_economicsId.Text),
            Parse(_npv.Text),
            Empty(_risks.Text));

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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

    private static ComboBox Picker(string name, double width)
    {
        var picker = new ComboBox
        {
            MinWidth = width,
            Margin = new Thickness(0, 0, 8, 0),
        };

        Named(picker, name);
        return picker;
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
