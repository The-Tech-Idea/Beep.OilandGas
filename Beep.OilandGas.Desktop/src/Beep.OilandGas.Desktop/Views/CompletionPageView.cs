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
/// DEV-07: completion tests and the independent acceptance that hands the well to production.
/// </summary>
/// <remarks>
/// A test records its type, interval and result with the result's unit (AC-5). A submission needs at least one test and
/// one evidence item; evidence naming another well is refused (AC-2). Acceptance needs at least one PASS test (AC-1) and
/// the author cannot accept their own submission (AC-3). An acceptance carries its conditions and handover recipient on
/// the record production reads (AC-4), and a lost acknowledgement is retried without writing a second acceptance (AC-6).
/// </remarks>
public sealed class CompletionPageView : OgPageBase
{
    public const string Route = "completion";

    private readonly OgApplication _application;
    private readonly IOgWellCompletion _completion;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _summary;
    private readonly TextBlock _handover;
    private readonly TextBlock _detail;
    private readonly ComboBox _wellPicker;
    private readonly ComboBox _submissionPicker;
    private readonly DataGrid _tests;
    private readonly DataGrid _evidence;

    private readonly TextBox _testType;
    private readonly TextBox _top;
    private readonly TextBox _bottom;
    private readonly TextBox _depthUnit;
    private readonly ComboBox _testStatus;
    private readonly TextBox _result;
    private readonly TextBox _resultUnit;
    private readonly TextBox _performedBy;
    private readonly TextBox _evidenceRef;

    private readonly TextBox _submissionSummary;
    private readonly TextBox _evidenceDocument;
    private readonly TextBox _evidenceWell;
    private readonly TextBox _evidenceReference;

    private readonly ComboBox _decision;
    private readonly TextBox _rationale;
    private readonly TextBox _conditions;
    private readonly TextBox _handoverTo;
    private readonly TextBox _reviewer;

    public CompletionPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _completion = application.Completion;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _handover = Body(string.Empty);
        _handover.TextWrapping = TextWrapping.Wrap;
        _handover.Margin = new Thickness(0, 0, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _wellPicker = new ComboBox { MinWidth = 200, Margin = new Thickness(0, 0, 8, 0) };
        Named(_wellPicker, "Well to complete");
        _wellPicker.SelectionChanged += (_, _) => SelectWell();

        _submissionPicker = new ComboBox { MinWidth = 220, Margin = new Thickness(0, 0, 8, 0) };
        Named(_submissionPicker, "Completion submission");
        _submissionPicker.SelectionChanged += (_, _) => ShowSelectedSubmission();

        _tests = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 180,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Type", "TestType", 130),
                Text("Interval", "DepthText", 150),
                Text("Status", "StatusText", 70),
                Text("Result", "ResultText", 140),
                Text("By", "PerformedBy", 100),
                Text("Evidence", "EvidenceRef", 140),
            },
        };

        Named(_tests, "Completion tests");

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
                Text("Document", "Document", 180),
                Text("Reference", "Reference", 200),
                Text("Well", "WellId", 120),
            },
        };

        Named(_evidence, "Completion evidence");

        _testType = Field("Test type", 140);
        _testType.ToolTip = "Test type, for example PRESSURE_TEST, INFLOW_TEST, CBL or LEAK_OFF";

        _top = Field("Top depth", 80);
        _bottom = Field("Base depth", 80);
        _depthUnit = Field("Depth unit (m/ft)", 90);

        _testStatus = new ComboBox
        {
            MinWidth = 90,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = OgCompletionTest.Statuses,
            SelectedIndex = 0,
        };

        Named(_testStatus, "Test status");

        _result = Field("Result", 100);
        _resultUnit = Field("Result unit", 80);
        _performedBy = Field("Performed by", 110);
        _evidenceRef = Field("Evidence reference", 140);

        _submissionSummary = Field("Submission summary", 240);
        _evidenceDocument = Field("Evidence document", 140);
        _evidenceWell = Field("Evidence well", 110);
        _evidenceReference = Field("Evidence reference", 140);

        _decision = new ComboBox
        {
            MinWidth = 190,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = OgCompletionDecision.Decisions,
            SelectedIndex = 0,
        };

        Named(_decision, "Completion decision");

        _rationale = Field("Rationale", 200);
        _conditions = Field("Conditions (one per line)", 200);
        _handoverTo = Field("Hand over to", 120);
        _reviewer = Field("Reviewer", 130);
        _reviewer.Text = _application.Session.UserId;

        var well = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        well.Children.Add(Caption("Well"));
        well.Children.Add(_wellPicker);
        well.Children.Add(Button("Refresh", (_, _) => Load()));

        var test = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        test.Children.Add(Caption("Test"));
        test.Children.Add(_testType);
        test.Children.Add(_top);
        test.Children.Add(_bottom);
        test.Children.Add(_depthUnit);
        test.Children.Add(_testStatus);
        test.Children.Add(_result);
        test.Children.Add(_resultUnit);
        test.Children.Add(_performedBy);
        test.Children.Add(_evidenceRef);
        test.Children.Add(Button("Record test", (_, _) => _ = RecordTestAsync()));

        var submission = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        submission.Children.Add(Caption("Submission"));
        submission.Children.Add(_submissionSummary);
        submission.Children.Add(Button("Create", (_, _) => _ = CreateSubmissionAsync()));
        submission.Children.Add(Button("Submit", (_, _) => _ = SubmitAsync()));
        submission.Children.Add(Button("Retry decision", (_, _) => _ = RetryAsync()));

        var evidence = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        evidence.Children.Add(Caption("Evidence"));
        evidence.Children.Add(_evidenceDocument);
        evidence.Children.Add(_evidenceWell);
        evidence.Children.Add(_evidenceReference);
        evidence.Children.Add(Button("Attach", (_, _) => _ = AddEvidenceAsync()));

        var review = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        review.Children.Add(Caption("Decision"));
        review.Children.Add(_decision);
        review.Children.Add(_rationale);
        review.Children.Add(_conditions);
        review.Children.Add(_handoverTo);
        review.Children.Add(_reviewer);
        review.Children.Add(Button("Record decision", (_, _) => _ = DecideAsync()));

        var panel = new StackPanel();
        panel.Children.Add(well);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("Tests"));
        panel.Children.Add(_tests);
        panel.Children.Add(test);
        panel.Children.Add(_submissionPicker);
        panel.Children.Add(submission);
        panel.Children.Add(Caption("Evidence"));
        panel.Children.Add(_evidence);
        panel.Children.Add(evidence);
        panel.Children.Add(review);
        panel.Children.Add(Caption("Handover"));
        panel.Children.Add(_handover);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgCompletionWorkspace? Workspace { get; private set; }

    public OgCompletionSubmission? Selected { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string SummaryText => _summary.Text;

    public string HandoverText => _handover.Text;

    public string DetailText => _detail.Text;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _completion
            .LoadAsync(session.FieldId, session.FieldName, Workspace?.SelectedWellId)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Workspace = null;
            Selected = null;
            _tests.ItemsSource = null;
            _evidence.ItemsSource = null;
            _wellPicker.ItemsSource = null;
            _submissionPicker.ItemsSource = null;
            _header.Text = "Well completion";
            _summary.Text = string.Empty;
            _handover.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The completion could not be read.";
            ShowState(OgViewState.Failed, "The completion could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Workspace = loaded.Value;

        _wellPicker.ItemsSource = Workspace.Wells.ToList();
        _wellPicker.SelectedIndex = Workspace.Wells.ToList().FindIndex(well => well.WellId == Workspace.SelectedWellId);

        _tests.ItemsSource = Workspace.Tests.ToList();
        _submissionPicker.ItemsSource = Workspace.Submissions.ToList();

        var index = Workspace.Selected is null
            ? -1
            : Workspace.Submissions.ToList().FindIndex(row => row.Id == Workspace.Selected.Id);

        _submissionPicker.SelectedIndex = index;

        Selected = Workspace.Selected;

        _evidence.ItemsSource = Workspace.Evidence.ToList();
        _evidenceWell.Text = Workspace.SelectedWellId ?? string.Empty;

        _header.Text = Workspace.SelectedWellId is { } wellId
            ? $"Well {wellId} · {Workspace.TestsText} · " +
              $"{(Workspace.CanAccept ? "acceptance possible" : "no passing test")}"
            : "Well completion";

        _summary.Text = Workspace.Selected is { } selected
            ? $"{selected.Describe()} · evidence {Workspace.Evidence.Count.ToString(CultureInfo.InvariantCulture)} item(s)"
            : Workspace.SubmissionGaps.Count == 0
                ? "No submission exists; create one for this well."
                : "No submission exists. Before one can be submitted: " + string.Join("; ", Workspace.SubmissionGaps);

        _notice.Text = Workspace.IsEmpty
            ? "No drilling execution is recorded for this field, so there is no well to complete."
            : Workspace.SubmissionGaps.Count == 0
                ? "A submission needs one test and one evidence item; acceptance needs a PASS test."
                : "Missing for a submission: " + string.Join("; ", Workspace.SubmissionGaps);

        _handover.Text = Workspace.Handover is { } handover
            ? handover.Describe()
            : "No handover: the completion has not been accepted.";

        _detail.Text = Workspace.Selected is { } current && current.Conditions.Count > 0
            ? "Conditions: " + string.Join("; ", current.Conditions)
            : "No conditions recorded on this submission.";

        ShowState(Workspace.IsEmpty ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    public async Task<bool> RecordTestAsync()
    {
        if (Workspace?.SelectedWellId is not { } wellId)
        {
            _notice.Text = "Select a well first.";
            return false;
        }

        var recorded = await _completion
            .RecordTestAsync(
                wellId,
                new OgCompletionTestValues(
                    _testType.Text,
                    Decimal(_top.Text),
                    Decimal(_bottom.Text),
                    _depthUnit.Text,
                    _testStatus.SelectedItem as string,
                    _result.Text,
                    _resultUnit.Text,
                    _performedBy.Text,
                    DateTime.Today,
                    _evidenceRef.Text),
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(recorded, "development.completion.test", wellId);
    }

    public async Task<bool> CreateSubmissionAsync()
    {
        if (Workspace?.SelectedWellId is not { } wellId)
        {
            _notice.Text = "Select a well first.";
            return false;
        }

        var created = await _completion
            .CreateSubmissionAsync(wellId, _submissionSummary.Text, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(created, "development.completion.submission", wellId);
    }

    public async Task<bool> AddEvidenceAsync()
    {
        if (Selected is not { } submission)
        {
            _notice.Text = "Create or select a submission first.";
            return false;
        }

        var added = await _completion
            .AddEvidenceAsync(
                submission.Id,
                _evidenceWell.Text,
                _evidenceDocument.Text,
                _evidenceReference.Text,
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(added, "development.completion.evidence", submission.Id);
    }

    public async Task<bool> SubmitAsync()
    {
        if (Selected is not { } submission)
        {
            _notice.Text = "Create or select a submission first.";
            return false;
        }

        var submitted = await _completion
            .SubmitAsync(submission.Id, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(submitted, "development.completion.submit", submission.Id);
    }

    public async Task<bool> DecideAsync()
    {
        if (Selected is not { } submission)
        {
            _notice.Text = "Create or select a submission first.";
            return false;
        }

        var conditions = _conditions.Text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var decided = await _completion
            .DecideAsync(
                submission.Id,
                new OgCompletionDecision(
                    _decision.SelectedItem as string ?? OgCompletionDecision.Accept,
                    _rationale.Text,
                    conditions,
                    _handoverTo.Text,
                    DateTime.Today),
                _reviewer.Text)
            .ConfigureAwait(true);

        return AfterWrite(decided, "development.completion.decision", submission.Id);
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _completion.RetryDecisionAsync().ConfigureAwait(true);
        return AfterWrite(retried, "development.completion.decision", "retry");
    }

    private void SelectWell()
    {
        if (_wellPicker.SelectedItem is not OgCompletionWell well)
        {
            return;
        }

        Workspace = _completion
            .LoadAsync(_application.Session.FieldId, _application.Session.FieldName, well.WellId)
            .GetAwaiter()
            .GetResult()
            .Value ?? Workspace;

        Load();
    }

    private void ShowSelectedSubmission()
    {
        Selected = _submissionPicker.SelectedItem as OgCompletionSubmission;

        if (Selected is null)
        {
            return;
        }

        _submissionSummary.Text = Selected.Summary ?? string.Empty;
        _reviewer.Text = string.Equals(Selected.Author, _application.Session.UserId, StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : _application.Session.UserId;
        _conditions.Text = string.Join(Environment.NewLine, Selected.Conditions);
        _handoverTo.Text = Selected.HandoverTo ?? string.Empty;
        _detail.Text = Selected.Describe();
    }

    private bool AfterWrite(OgResult<OgCompletionWorkspace> result, string verb, string subject)
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

    private static decimal? Decimal(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : null;

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
