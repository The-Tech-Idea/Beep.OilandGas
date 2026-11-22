using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// LSE-02: the acquisition pipeline.
/// </summary>
/// <remarks>
/// One board over every acquisition opportunity: the stage columns come from the recorded history (an opportunity
/// with no stage is shown in an explicit unclassified column), the detail pane carries the diligence checklist,
/// the rights matrix and the negotiation log, and a stage change records actor, date and rationale. Open diligence
/// blocks the move out of due diligence (AC-2), and a decline is a recorded transition, so the history is kept
/// (AC-4). Money is shown with its currency (AC-5).
/// </remarks>
public sealed class LeasePipelinePageView : OgPageBase
{
    public const string Route = "leasepipeline";

    private readonly OgApplication _application;
    private readonly IOgLeasePipeline _pipeline;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _summary;
    private readonly TextBlock _detail;
    private readonly DataGrid _grid;

    private readonly TextBox _category;
    private readonly TextBox _question;
    private readonly ComboBox _outcome;
    private readonly TextBox _evidence;
    private readonly TextBox _counterparty;
    private readonly TextBox _round;
    private readonly TextBox _offer;
    private readonly TextBox _currency;
    private readonly TextBox _notes;
    private readonly ComboBox _targetStage;
    private readonly TextBox _rationale;

    public LeasePipelinePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _pipeline = application.LeasePipeline;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 8, 0, 8);

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 200,
            MaxHeight = 300,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Name", "Opportunity.Name", 170),
                Text("Stage", "StageLabel", 130),
                Text("Region", "Opportunity.Region", 80),
                Text("Acreage", "Opportunity.AcreageText", 120),
                Text("Asking price", "Opportunity.AskingPriceText", 160),
                Text("Owner", "Opportunity.Owner", 110),
                Text("Age", "Opportunity.AgeText", 70),
                Text("Diligence", "DiligenceText", 170),
            },
        };

        Named(_grid, "Acquisition pipeline");
        _grid.SelectionChanged += (_, _) => _ = ShowSelectedAsync();

        _category = Field("Diligence category", 120);
        _question = Field("Diligence question", 220);
        _evidence = Field("Evidence reference", 140);

        _outcome = new ComboBox
        {
            MinWidth = 100,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = OgDiligenceItem.Outcomes,
            SelectedIndex = 0,
        };

        Named(_outcome, "Diligence outcome");

        _counterparty = Field("Counterparty", 140);
        _round = Field("Round", 60);
        _offer = Field("Offer amount", 100);
        _currency = Field("Currency", 70);
        _notes = Field("Negotiation notes", 180);

        _targetStage = new ComboBox
        {
            MinWidth = 140,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = LeasePipelineStage.All,
        };

        Named(_targetStage, "Target stage");

        _rationale = Field("Rationale", 240);

        var actions = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actions.Children.Add(Button("Refresh", (_, _) => Load()));
        actions.Children.Add(Button("Retry", (_, _) => _ = RetryAsync()));

        var diligence = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        diligence.Children.Add(Caption("Diligence"));
        diligence.Children.Add(_category);
        diligence.Children.Add(_question);
        diligence.Children.Add(_outcome);
        diligence.Children.Add(_evidence);
        diligence.Children.Add(Button("Record", (_, _) => _ = RecordDiligenceAsync()));

        var negotiation = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        negotiation.Children.Add(Caption("Negotiation"));
        negotiation.Children.Add(_counterparty);
        negotiation.Children.Add(Caption("Round"));
        negotiation.Children.Add(_round);
        negotiation.Children.Add(Caption("Offer"));
        negotiation.Children.Add(_offer);
        negotiation.Children.Add(_currency);
        negotiation.Children.Add(_notes);
        negotiation.Children.Add(Button("Record round", (_, _) => _ = RecordNegotiationAsync()));

        var decision = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        decision.Children.Add(Caption("Stage change"));
        decision.Children.Add(_targetStage);
        decision.Children.Add(_rationale);
        decision.Children.Add(Button("Advance", (_, _) => _ = AdvanceAsync()));
        decision.Children.Add(Button("Decline", (_, _) => _ = DeclineAsync()));

        var panel = new StackPanel();
        panel.Children.Add(actions);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_summary);
        panel.Children.Add(_grid);
        panel.Children.Add(_detail);
        panel.Children.Add(diligence);
        panel.Children.Add(negotiation);
        panel.Children.Add(decision);

        ShowContent(panel);

        Load();
    }

    public OgPipelineSnapshot? Pipeline { get; private set; }

    public IReadOnlyList<OgPipelineCard> Cards => Pipeline?.Cards ?? [];

    public OgPipelineCard? Selected { get; private set; }

    public OgPipelineDetail? Detail { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string DetailText => _detail.Text;

    public string SummaryText => _summary.Text;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _pipeline
            .LoadAsync(session.FieldId, session.FieldName, DateTime.Today)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Pipeline = null;
            Selected = null;
            Detail = null;
            _grid.ItemsSource = null;
            _header.Text = "Acquisition pipeline";
            _summary.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The acquisition pipeline could not be read.";
            ShowState(OgViewState.Failed, "The acquisition pipeline could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Pipeline = loaded.Value;
        Selected = null;
        Detail = null;

        _grid.ItemsSource = Cards.ToList();

        _header.Text =
            $"{Pipeline.CardCount.ToString(CultureInfo.InvariantCulture)} opportunit(y/ies) · " +
            $"{(string.IsNullOrWhiteSpace(session.FieldName) ? "(no field selected)" : session.FieldName)} · " +
            $"as of {Pipeline.AsOf.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

        _summary.Text = Pipeline.IsEmpty
            ? "No acquisition opportunity is recorded for this field."
            : Pipeline.Summary;

        _notice.Text = Pipeline.IsEmpty
            ? "No opportunity is recorded for this field."
            : Pipeline.OpenDiligenceCount == 0
                ? "Diligence is complete on the shown opportunities."
                : $"{Pipeline.OpenDiligenceCount.ToString(CultureInfo.InvariantCulture)} open diligence item(s) " +
                  "across the shown opportunities; an open item blocks the move out of due diligence.";

        _detail.Text = "Select an opportunity for its diligence, rights, negotiation log and history.";
        ShowState(Pipeline.IsEmpty ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    /// <summary>AC-2: records or closes one diligence item.</summary>
    public async Task<bool> RecordDiligenceAsync()
    {
        if (Selected is not { } card)
        {
            _notice.Text = "Select an opportunity first.";
            return false;
        }

        var recorded = await _pipeline
            .RecordDiligenceAsync(
                card.Id,
                _category.Text,
                _question.Text,
                _outcome.SelectedItem as string,
                _evidence.Text,
                _application.Session.UserId)
            .ConfigureAwait(true);

        _notice.Text = recorded.Success
            ? recorded.Message ?? "Diligence item recorded."
            : recorded.Message ?? "The diligence item was not recorded.";

        if (recorded.Success)
        {
            _application.AuditLog.Add(
                "lease.diligence",
                "lease",
                card.Id,
                "ok",
                $"question={_question.Text.Trim()} outcome={_outcome.SelectedItem}");
            ShowDetail(recorded.Value);
        }

        return recorded.Success;
    }

    /// <summary>Records one negotiation round; the service refuses a round without a counterparty or currency.</summary>
    public async Task<bool> RecordNegotiationAsync()
    {
        if (Selected is not { } card)
        {
            _notice.Text = "Select an opportunity first.";
            return false;
        }

        var recorded = await _pipeline
            .RecordNegotiationAsync(
                card.Id,
                _counterparty.Text,
                Round(),
                Amount(_offer.Text),
                _currency.Text,
                _notes.Text,
                _application.Session.UserId)
            .ConfigureAwait(true);

        _notice.Text = recorded.Success
            ? recorded.Message ?? "Negotiation round recorded."
            : recorded.Message ?? "The negotiation round was not recorded.";

        if (recorded.Success)
        {
            _application.AuditLog.Add(
                "lease.negotiation",
                "lease",
                card.Id,
                "ok",
                $"counterparty={_counterparty.Text.Trim()} offer={_offer.Text.Trim()} {_currency.Text.Trim()}");
            ShowDetail(recorded.Value);
        }

        return recorded.Success;
    }

    /// <summary>AC-1/AC-2: advances one legal stage, recording actor, date and rationale.</summary>
    public async Task<bool> AdvanceAsync()
    {
        if (Selected is not { } card)
        {
            _notice.Text = "Select an opportunity first.";
            return false;
        }

        var moved = await _pipeline
            .AdvanceAsync(new OgPipelineDecision(
                card.Id,
                _targetStage.SelectedItem as string,
                _rationale.Text,
                _application.Session.UserId))
            .ConfigureAwait(true);

        return AfterDecision(moved);
    }

    /// <summary>AC-4: declines an opportunity; its history is kept.</summary>
    public async Task<bool> DeclineAsync()
    {
        if (Selected is not { } card)
        {
            _notice.Text = "Select an opportunity first.";
            return false;
        }

        var declined = await _pipeline
            .DeclineAsync(card.Id, _rationale.Text, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterDecision(declined);
    }

    /// <summary>Re-sends the stage change that was not acknowledged, unchanged.</summary>
    public async Task<bool> RetryAsync()
    {
        var retried = await _pipeline.RetryAsync(DateTime.Today).ConfigureAwait(true);
        return AfterDecision(retried);
    }

    private bool AfterDecision(OgResult<OgPipelineDetail> result)
    {
        _notice.Text = result.Success
            ? result.Message ?? "Stage changed."
            : result.Message ?? "The stage change was not applied.";

        if (result.Success)
        {
            _application.AuditLog.Add(
                "lease.pipeline.advance",
                "lease",
                Selected?.Id ?? string.Empty,
                "ok",
                $"target={_targetStage.SelectedItem} rationale={_rationale.Text.Trim()}");

            Load();
            ShowDetail(result.Value);
        }

        return result.Success;
    }

    private async Task ShowSelectedAsync()
    {
        var index = _grid.SelectedIndex;

        if (index < 0 || index >= Cards.Count)
        {
            return;
        }

        Selected = Cards[index];
        _targetStage.SelectedItem = LeasePipelineStage.Next(Selected.Opportunity.Stage);

        var loaded = await _pipeline
            .LoadDetailAsync(Selected.Id, DateTime.Today)
            .ConfigureAwait(true);

        if (!loaded.Success || loaded.Value is null)
        {
            _detail.Text = loaded.Message ?? "The opportunity could not be read.";
            return;
        }

        ShowDetail(loaded.Value);
    }

    private void ShowDetail(OgPipelineDetail? detail)
    {
        Detail = detail;

        if (detail is null)
        {
            return;
        }

        var lines = new List<string>
        {
            detail.Opportunity.Describe(),
            $"Stage {detail.Opportunity.StageLabel} since " +
            $"{detail.Opportunity.StageEnteredAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "unknown"} " +
            $"({detail.Opportunity.AgeText})",
            $"Diligence ({detail.OpenDiligenceCount.ToString(CultureInfo.InvariantCulture)} open of " +
            $"{detail.Diligence.Count.ToString(CultureInfo.InvariantCulture)}):",
        };

        lines.AddRange(detail.Diligence.Select(item => "  " + item.Describe()));

        lines.Add($"Rights ({detail.RightsSummary}):");
        lines.AddRange(detail.Rights.Select(right => "  " + right.Describe()));

        lines.Add($"Negotiation ({detail.Negotiations.Count.ToString(CultureInfo.InvariantCulture)} round(s)):");
        lines.AddRange(detail.Negotiations.Select(round => "  " + round.Describe()));

        lines.Add($"History ({detail.History.Count.ToString(CultureInfo.InvariantCulture)} transition(s)):");
        lines.AddRange(detail.History.Select(entry =>
            $"  {entry.Sequence.ToString(CultureInfo.InvariantCulture)} {entry.Stage} " +
            $"{entry.Outcome ?? "-"} by {entry.Actor ?? "(unknown)"} " +
            $"on {entry.DecidedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "unknown"} " +
            $"({entry.Rationale ?? "no rationale"})"));

        _detail.Text = string.Join(Environment.NewLine, lines);
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

    private int? Round() =>
        int.TryParse(_round.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
        && value > 0
            ? value
            : null;

    private static decimal Amount(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0m;

    private static ComboBox Picker(string name)
    {
        var picker = new ComboBox { MinWidth = 120, Margin = new Thickness(0, 0, 8, 0) };

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
