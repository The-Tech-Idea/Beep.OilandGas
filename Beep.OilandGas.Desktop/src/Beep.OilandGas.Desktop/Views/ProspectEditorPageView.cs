using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One row of the read-only stage history.</summary>
public sealed record OgProspectHistoryRow(string Stage, string Order, string Started, string Ended, string Outcome);

/// <summary>
/// EXP-03: the prospect editor - identity, trap, reservoir, source, volume, risk, economics and history.
/// </summary>
/// <remarks>
/// The chance is produced only from recorded element probabilities, and a missing element shows "incomplete" with
/// no number (AC-1, AC-2). The volume percentiles carry the recorded unit and are flagged when they cross (AC-3).
/// A submitted prospect is read-only and is revised into a new version instead of edited (AC-4). Every risk save
/// records its assessor and date (AC-5). The economic summary is read-only and the evaluation link is offered only
/// when the ECO-01 page can be opened (AC-6).
/// </remarks>
public sealed class ProspectEditorPageView : OgPageBase
{
    public const string Route = "prospect";

    private readonly Bootstrapping.OgApplication _application;
    private readonly IOgProspectEditor _editor;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _chance;
    private readonly TextBlock _detail;
    private readonly TextBlock _trap;
    private readonly TextBlock _reservoir;
    private readonly TextBlock _source;
    private readonly TextBlock _volume;
    private readonly TextBlock _economics;
    private readonly TextBlock _riskNote;
    private readonly TextBox _name;
    private readonly TextBox _play;
    private readonly TextBox _operator;
    private readonly TextBox _description;
    private readonly TextBox _assessor;
    private readonly TextBox _assessedAt;
    private readonly TextBox _revisionId;
    private readonly Dictionary<string, TextBox> _probabilities = new(StringComparer.OrdinalIgnoreCase);
    private readonly DataGrid _history;
    private readonly Button _revise;

    public ProspectEditorPageView(Bootstrapping.OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _editor = application.ProspectEditor;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _chance = Body(string.Empty);
        _chance.FontSize = 13;
        _detail = Body(string.Empty);
        _trap = Body(string.Empty);
        _reservoir = Body(string.Empty);
        _source = Body(string.Empty);
        _volume = Body(string.Empty);
        _economics = Body(string.Empty);
        _riskNote = Body(string.Empty);

        _name = Input("Prospect name");
        _play = Input("Play identifier");
        _operator = Input("Operator (business associate id)");
        _description = Input("Description");

        Named(_name, "Prospect name");
        Named(_play, "Play identifier");
        Named(_operator, "Operator business associate id");
        Named(_description, "Prospect description");

        _assessor = Input("Assessor");
        _assessor.Text = application.Session.UserId;
        Named(_assessor, "Risk assessor");

        _assessedAt = Input("Assessment date (yyyy-MM-dd)");
        _assessedAt.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        Named(_assessedAt, "Assessment date");

        _revisionId = Input("New prospect id for the revision");
        Named(_revisionId, "New prospect id");

        _revise = Button("Revise to a new version", (_, _) => _ = ReviseAsync());
        _revise.IsEnabled = false;

        _history = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 200,
            MinHeight = 80,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            Columns =
            {
                Text("Stage", nameof(OgProspectHistoryRow.Stage), 160),
                Text("Seq", nameof(OgProspectHistoryRow.Order), 60),
                Text("Started", nameof(OgProspectHistoryRow.Started), 100),
                Text("Ended", nameof(OgProspectHistoryRow.Ended), 100),
                Text("Outcome", nameof(OgProspectHistoryRow.Outcome), 120),
            },
        };

        Named(_history, "Prospect stage history");

        var tabs = new TabControl { Margin = new Thickness(0, 6, 0, 8), MinHeight = 300, IsTabStop = true };
        Named(tabs, "Prospect sections");

        tabs.Items.Add(Tab("Identity",
            Row(Caption("Name"), _name),
            Row(Caption("Play"), _play),
            Row(Caption("Operator"), _operator),
            Row(Caption("Description"), _description),
            _detail,
            Row(Button("Save identity", (_, _) => _ = SaveIdentityAsync()))));

        tabs.Items.Add(Tab("Trap", _trap));
        tabs.Items.Add(Tab("Reservoir", _reservoir));
        tabs.Items.Add(Tab("Source", _source));
        tabs.Items.Add(Tab("Volume", _volume));
        tabs.Items.Add(Tab("Risk", RiskPanel()));
        tabs.Items.Add(Tab("Economics", EconomicsPanel()));
        tabs.Items.Add(Tab("History", _history));

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        toolbar.Children.Add(Button("Refresh", (_, _) => Load()));
        toolbar.Children.Add(Button("Back to board", (_, _) => _application.Pages.Navigate(ProspectBoardPageView.Route)));
        toolbar.Children.Add(Caption("Revision id"));
        toolbar.Children.Add(_revisionId);
        toolbar.Children.Add(_revise);

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_chance);
        panel.Children.Add(tabs);

        ShowContent(panel);

        OpenPending();
    }

    /// <summary>The prospect last read, or null.</summary>
    public OgProspectDetail? Detail { get; private set; }

    public bool LastFailed { get; private set; }

    /// <summary>AC-4: true when the prospect is submitted and therefore read-only.</summary>
    public bool IsLocked => Detail?.IsLocked ?? false;

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string ChanceText => _chance.Text;

    public string VolumeText => _volume.Text;

    public string EconomicsText => _economics.Text;

    public string TrapText => _trap.Text;

    public string ReservoirText => _reservoir.Text;

    public string SourceText => _source.Text;

    public IReadOnlyList<OgProspectHistoryRow> HistoryRows { get; private set; } = [];

    /// <summary>Opens the prospect the board selected.</summary>
    public bool OpenPending()
    {
        var selected = _application.ProspectContext.Current;

        return selected is not null && Open(selected.Id);
    }

    /// <summary>Loads one prospect.</summary>
    public bool Open(string? prospectId)
    {
        if (string.IsNullOrWhiteSpace(prospectId))
        {
            Detail = null;
            _header.Text = "No prospect selected.";
            _notice.Text = "Choose a card on the prospect board first.";
            ShowState(OgViewState.Empty, "No prospect selected.");
            return false;
        }

        var loaded = _editor.LoadAsync(prospectId.Trim()).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            Detail = null;
            LastFailed = true;
            _history.ItemsSource = null;
            _header.Text = $"Prospect {prospectId}";
            _notice.Text = loaded.Message ?? "The prospect could not be read.";
            ShowState(OgViewState.Failed, "The prospect could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Detail = loaded.Value;
        Apply(Detail);
        return true;
    }

    public bool Load() => Open(Detail?.Identity.Id ?? _application.ProspectContext.Current?.Id);

    /// <summary>AC-4: saves the identity only while the prospect is not submitted.</summary>
    public async Task<bool> SaveIdentityAsync()
    {
        if (Detail is not { } detail)
        {
            _notice.Text = "No prospect is open.";
            return false;
        }

        var saved = await _editor
            .SaveIdentityAsync(
                detail.Identity,
                _name.Text,
                _play.Text,
                _operator.Text,
                _description.Text,
                _application.Session.UserId)
            .ConfigureAwait(true);

        _notice.Text = saved.Success
            ? saved.Message ?? "Prospect saved."
            : saved.Message ?? "The prospect was not saved.";

        if (saved.Success)
        {
            _application.AuditLog.Add(
                "prospect.identity.save", "prospect", detail.Identity.Id, "ok", _name.Text.Trim());
            Open(detail.Identity.Id);
        }

        return saved.Success;
    }

    /// <summary>AC-1, AC-2, AC-5: saves the element probabilities with their assessor and date.</summary>
    public async Task<bool> SaveRiskAsync()
    {
        if (Detail is not { } detail)
        {
            _notice.Text = "No prospect is open.";
            return false;
        }

        var elements = OgProspectChance.RequiredElements
            .Select(element => new OgProspectRiskElement(
                element,
                Number(_probabilities.TryGetValue(element, out var box) ? box.Text : null),
                null,
                _assessor.Text,
                Date(_assessedAt.Text)))
            .ToList();

        var saved = await _editor
            .SaveRiskAsync(detail.Identity.Id, elements, _assessor.Text, Date(_assessedAt.Text) ?? default)
            .ConfigureAwait(true);

        _notice.Text = saved.Success
            ? saved.Message ?? "Risk saved."
            : saved.Message ?? "The risk was not saved.";

        if (saved.Success)
        {
            _application.AuditLog.Add(
                "prospect.risk.save", "prospect", detail.Identity.Id, "ok", $"assessor={_assessor.Text.Trim()}");
            Open(detail.Identity.Id);
        }

        return saved.Success;
    }

    /// <summary>AC-4: a submitted prospect becomes a new draft version.</summary>
    public async Task<bool> ReviseAsync()
    {
        if (Detail is not { } detail)
        {
            _notice.Text = "No prospect is open.";
            return false;
        }

        var revised = await _editor
            .ReviseAsync(detail.Identity.Id, _revisionId.Text, _application.Session.UserId)
            .ConfigureAwait(true);

        _notice.Text = revised.Success
            ? $"Revision {_revisionId.Text.Trim()} created as a new draft."
            : revised.Message ?? "The revision was not created.";

        if (!revised.Success || revised.Value is null)
        {
            return false;
        }

        _application.AuditLog.Add(
            "prospect.revise",
            "prospect",
            revised.Value.Identity.Id,
            "ok",
            $"revision of {detail.Identity.Id} ({detail.Identity.Status})");

        _application.ProspectContext.Select(
            new OgProspectSelection(revised.Value.Identity.Id, revised.Value.Identity.Name));

        Detail = revised.Value;
        Apply(revised.Value);
        return true;
    }

    private UIElement RiskPanel()
    {
        var panel = new StackPanel();

        panel.Children.Add(_riskNote);

        foreach (var element in OgProspectChance.RequiredElements)
        {
            var box = Input($"{element} risk probability (0-1)");
            Named(box, $"{element} risk probability");
            _probabilities[element] = box;

            var rationale = Body(string.Empty);
            Named(rationale, $"{element} risk rationale");
            rationale.FontSize = 11.5;
            rationale.Margin = new Thickness(8, 0, 0, 0);
            _rationales[element] = rationale;

            panel.Children.Add(Row(Caption(element), box, rationale));
        }

        panel.Children.Add(Row(
            Caption("Assessor"), _assessor,
            Caption("Assessed"), _assessedAt));

        panel.Children.Add(Row(Button("Save risk", (_, _) => _ = SaveRiskAsync())));

        return panel;
    }

    private readonly Dictionary<string, TextBlock> _rationales = new(StringComparer.OrdinalIgnoreCase);

    private UIElement EconomicsPanel()
    {
        var panel = new StackPanel();
        panel.Children.Add(_economics);

        var route = OgProspectEconomicSummary.EvaluationRoute;

        if (_application.Navigation.CanAccess(route, _application.Permissions))
        {
            panel.Children.Add(Row(Button(
                "Open economic evaluation",
                (_, _) => _application.Pages.Navigate(route))));
        }
        else
        {
            panel.Children.Add(Body(
                "The economic evaluation page is not available in this build; it arrives with ECO-01."));
        }

        return panel;
    }

    private void Apply(OgProspectDetail detail)
    {
        _header.Text =
            $"{detail.Identity.Name} · {detail.Identity.Status}" +
            (string.IsNullOrWhiteSpace(detail.Identity.PlayName) ? string.Empty : $" · {detail.Identity.PlayName}") +
            (string.IsNullOrWhiteSpace(detail.Identity.FieldId) ? string.Empty : $" · {detail.Identity.FieldId}");

        _chance.Text = detail.RiskComplete
            ? $"Chance of success {detail.ChanceText}, from the recorded element probabilities."
            : $"Chance is incomplete: {string.Join(", ", detail.MissingElements)} not recorded yet. No number is shown.";

        _detail.Text =
            $"Id {detail.Identity.Id} · play {detail.Identity.PlayId ?? "-"} · type {detail.Identity.Type ?? "-"} · " +
            $"operator {detail.Identity.OperatorId ?? "-"} · status {detail.Identity.Status}" +
            (detail.IsLocked ? " · submitted, so read-only; revise it to a new version." : string.Empty);

        _name.Text = detail.Identity.Name;
        _play.Text = detail.Identity.PlayId ?? string.Empty;
        _operator.Text = detail.Identity.OperatorId ?? string.Empty;
        _description.Text = detail.Identity.Description ?? string.Empty;

        _trap.Text =
            $"Type {detail.Trap.TrapType ?? "-"} · closure {Number(detail.Trap.ClosureArea)} " +
            $"{detail.Trap.ClosureAreaUnit ?? string.Empty}".TrimEnd() +
            $" · seal {detail.Trap.SealQuality ?? "-"} · confidence {detail.Trap.Confidence ?? "-"}" +
            (string.IsNullOrWhiteSpace(detail.Trap.Description)
                ? string.Empty
                : Environment.NewLine + detail.Trap.Description);

        _reservoir.Text =
            $"Reservoir {detail.Reservoir.Name ?? "-"} · unit {detail.Reservoir.StratUnitId ?? "-"} · " +
            $"lithology {detail.Reservoir.Lithology ?? "-"} · porosity {Number(detail.Reservoir.Porosity)} · " +
            $"permeability {Number(detail.Reservoir.Permeability)} · net pay {Number(detail.Reservoir.NetPay)} " +
            $"{detail.Reservoir.NetPayUnit ?? string.Empty}".TrimEnd() +
            $" · fluid {detail.Reservoir.FluidType ?? "-"}";

        _source.Text =
            $"Source unit {detail.Source.StratUnitId ?? "-"} · TOC {Number(detail.Source.Toc)} · " +
            $"maturity {detail.Source.MaturityStage ?? "-"} · migration {Number(detail.Source.MigrationDistance)} " +
            $"{detail.Source.MigrationDistanceUnit ?? string.Empty}".TrimEnd();

        _volume.Text =
            $"{detail.Volume.Describe()}" +
            (detail.Volume.HasUnit ? string.Empty : " · the unit is not recorded, so the volumes cannot be compared") +
            (detail.Volume.IsOrdered ? string.Empty : " · the percentiles are out of order (P90 must not exceed P50, nor P50 P10)");

        _economics.Text =
            (detail.Economics.HasContent
                ? $"Scenario {detail.Economics.Scenario ?? "-"} · evaluated " +
                  $"{detail.Economics.EvaluatedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "-"} · " +
                  $"by {detail.Economics.Evaluator ?? "-"} · EMV {Number(detail.Economics.Emv)} · " +
                  $"NPV {Number(detail.Economics.Npv)} {detail.Economics.Currency ?? string.Empty}".TrimEnd()
                : "No economic evaluation is recorded for this prospect.") +
            " (read-only here)";

        _riskNote.Text =
            "Probabilities are the risk of failure for each element (0-1). The chance is the product of their " +
            "complements, and it is shown only when all five are recorded. The rationale is the assessment's shared " +
            "note, because the table records one rationale per assessment, not one per element.";

        foreach (var element in detail.Risk)
        {
            if (_probabilities.TryGetValue(element.Element, out var box))
            {
                box.Text = element.Probability?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            }

            if (_rationales.TryGetValue(element.Element, out var rationale))
            {
                rationale.Text =
                    $"{element.ProbabilityText} · {element.Assessor ?? "no assessor"} · " +
                    $"{element.AssessedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "no date"}";
            }
        }

        if (detail.Risk.Count > 0 && detail.Risk[0].Assessor is { } assessor)
        {
            _assessor.Text = assessor;
        }

        if (detail.Risk.Count > 0 && detail.Risk[0].AssessedAt is { } assessed)
        {
            _assessedAt.Text = assessed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        HistoryRows = detail.History
            .Select(row => new OgProspectHistoryRow(
                row.Stage,
                row.Order.ToString(CultureInfo.InvariantCulture),
                row.StartedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                row.EndedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                row.Outcome ?? string.Empty))
            .ToList();

        _history.ItemsSource = HistoryRows;

        _revise.IsEnabled = detail.IsLocked;

        _notice.Text = detail.IsLocked
            ? "This prospect is submitted: it is read-only, and a revision creates a new draft version."
            : "Editing a draft: save the identity or the risk assessment.";

        ShowState(OgViewState.Ready, _notice.Text);
    }

    private static TabItem Tab(string header, params object[] content)
    {
        var panel = new StackPanel { Margin = new Thickness(8) };

        foreach (var item in content)
        {
            if (item is UIElement element)
            {
                panel.Children.Add(element);
            }
        }

        return new TabItem
        {
            Header = header,
            Content = new ScrollViewer { Content = panel, Focusable = false },
        };
    }

    private static WrapPanel Row(params object[] items)
    {
        var row = new WrapPanel { Margin = new Thickness(0, 2, 0, 2) };

        foreach (var item in items)
        {
            if (item is UIElement element)
            {
                row.Children.Add(element);
            }
        }

        return row;
    }

    private static TextBox Input(string toolTip) =>
        new() { MinWidth = 200, Margin = new Thickness(0, 0, 8, 0), ToolTip = toolTip };

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

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
        MinWidth = 90,
    };

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };

    private static string Number(decimal? value) =>
        value?.ToString("0.###", CultureInfo.InvariantCulture) ?? "-";

    private static decimal? Number(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : null;

    private static DateTime? Date(string? text) =>
        DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value.Date
            : null;
}
