using System.Globalization;
using Beep.OilandGas.ProductionAccounting.Constants;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Economics;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ECO-02: AFE management - raise, submit, approve, supplement, commit and close an authorisation for expenditure, with the
/// budget/committed/actual variance beside it.
/// </summary>
/// <remarks>
/// A revision never mutates the approved version: the supplement writes a new row that points at it (AC-1, SG-09-4). A
/// commitment above the remaining amount is refused (AC-2), the author cannot approve (AC-3), remaining is amount less
/// committed exactly (AC-4), a closed AFE is read-only (AC-5), and a supplement is inert until it is itself approved
/// (AC-6). The transition list is printed from the owner's own lifecycle, so no state is invented.
/// </remarks>
public sealed class AfePageView : OgPageBase
{
    public const string Route = "afe";

    private readonly OgApplication _application;
    private readonly IOgAfe _afe;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _lifecycle;
    private readonly TextBlock _summary;
    private readonly TextBlock _status;
    private readonly TextBlock _variance;
    private readonly TextBlock _commitments;
    private readonly TextBlock _revisions;
    private readonly TextBlock _detail;
    private readonly TextBox _title;
    private readonly ComboBox _kind;
    private readonly TextBox _entity;
    private readonly TextBox _amount;
    private readonly ComboBox _currency;
    private readonly TextBox _description;
    private readonly TextBox _supplementAmount;
    private readonly TextBox _supplementReason;
    private readonly TextBox _commitReference;
    private readonly TextBox _commitAmount;
    private readonly TextBox _closeNote;
    private readonly DataGrid _afes;
    private readonly DataGrid _commitmentGrid;
    private readonly DataGrid _revisionGrid;
    private readonly Button _raiseButton;
    private readonly Button _submitButton;
    private readonly Button _approveButton;
    private readonly Button _supplementButton;
    private readonly Button _commitButton;
    private readonly Button _closeButton;

    private OgAfeWorkspace? _workspace;
    private OgAfe? _selected;

    public AfePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _afe = application.Afes;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 4, 0, 4);

        _lifecycle = Body(string.Empty);
        _lifecycle.TextWrapping = TextWrapping.Wrap;
        _lifecycle.Margin = new Thickness(0, 0, 0, 6);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _status = Body(string.Empty);
        _status.TextWrapping = TextWrapping.Wrap;
        _status.Margin = new Thickness(0, 0, 0, 4);

        _variance = Body(string.Empty);
        _variance.TextWrapping = TextWrapping.Wrap;
        _variance.Margin = new Thickness(0, 0, 0, 4);

        _commitments = Body(string.Empty);
        _commitments.TextWrapping = TextWrapping.Wrap;
        _commitments.Margin = new Thickness(0, 0, 0, 4);

        _revisions = Body(string.Empty);
        _revisions.TextWrapping = TextWrapping.Wrap;
        _revisions.Margin = new Thickness(0, 0, 0, 6);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _title = Field("Title", 220);
        _entity = Field("Well or entity", 130);
        _amount = Field("Amount", 120);
        _description = Field("Scope", 280);

        _supplementAmount = Field("Supplement amount", 130);
        _supplementReason = Field("Supplement reason", 260);
        _commitReference = Field("Commitment reference", 160);
        _commitAmount = Field("Commitment amount", 150);
        _closeNote = Field("Closure note", 240);

        _kind = Picker("Kind", 130);
        _currency = Picker("Currency", 100);

        _afes = new DataGrid
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
                Text("Number", "Number", 120),
                Text("Rev", "RevisionNo", 60),
                Text("Title", "Title", 190),
                Text("Kind", "Kind", 90),
                Text("Amount", "AmountText", 140),
                Text("Committed", "Committed", 110),
                Text("Remaining", "RemainingText", 140),
                Text("Status", "Status", 100),
                Text("Approved by", "Approver", 110),
            },
        };

        Named(_afes, "AFEs");
        _afes.SelectionChanged += (_, _) => SelectAfe();

        _commitmentGrid = new DataGrid
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
                Text("Reference", "Reference", 180),
                Text("Amount", "Amount", 120),
                Text("Currency", "Currency", 90),
                Text("By", "Actor", 110),
                Text("On", "CommittedAt", 150),
            },
        };

        Named(_commitmentGrid, "Commitments on the selected revision");

        _revisionGrid = new DataGrid
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
                Text("Rev", "RevisionNo", 60),
                Text("Amount", "Amount", 120),
                Text("Status", "StatusText", 260),
                Text("Approver", "Approver", 120),
            },
        };

        Named(_revisionGrid, "Revisions of the selected AFE");

        _raiseButton = Button("Raise", (_, _) => _ = RaiseAsync());
        _submitButton = Button("Submit", (_, _) => _ = SubmitAsync());
        _submitButton.IsEnabled = false;
        _approveButton = Button("Approve", (_, _) => _ = ApproveAsync());
        _approveButton.IsEnabled = false;
        _supplementButton = Button("Raise supplement", (_, _) => _ = SupplementAsync());
        _supplementButton.IsEnabled = false;
        _commitButton = Button("Record commitment", (_, _) => _ = CommitAsync());
        _commitButton.IsEnabled = false;
        _closeButton = Button("Close", (_, _) => _ = CloseAsync());
        _closeButton.IsEnabled = false;

        var raiseBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        raiseBar.Children.Add(Caption("Title"));
        raiseBar.Children.Add(_title);
        raiseBar.Children.Add(Caption("Kind"));
        raiseBar.Children.Add(_kind);
        raiseBar.Children.Add(Caption("Entity"));
        raiseBar.Children.Add(_entity);

        var amountBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        amountBar.Children.Add(Caption("Amount"));
        amountBar.Children.Add(_amount);
        amountBar.Children.Add(Caption("Currency"));
        amountBar.Children.Add(_currency);
        amountBar.Children.Add(Caption("Scope"));
        amountBar.Children.Add(_description);
        amountBar.Children.Add(_raiseButton);

        var submitBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        submitBar.Children.Add(_submitButton);
        submitBar.Children.Add(_approveButton);

        var supplementBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        supplementBar.Children.Add(Caption("Supplement"));
        supplementBar.Children.Add(_supplementAmount);
        supplementBar.Children.Add(Caption("Reason"));
        supplementBar.Children.Add(_supplementReason);
        supplementBar.Children.Add(_supplementButton);

        var commitBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        commitBar.Children.Add(Caption("Commit to"));
        commitBar.Children.Add(_commitReference);
        commitBar.Children.Add(Caption("Amount"));
        commitBar.Children.Add(_commitAmount);
        commitBar.Children.Add(_commitButton);

        var closeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        closeBar.Children.Add(Caption("Close note"));
        closeBar.Children.Add(_closeNote);
        closeBar.Children.Add(_closeButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("Raise an AFE"));
        panel.Children.Add(raiseBar);
        panel.Children.Add(amountBar);
        panel.Children.Add(Caption("Submit and approve"));
        panel.Children.Add(submitBar);
        panel.Children.Add(Caption("Supplement"));
        panel.Children.Add(supplementBar);
        panel.Children.Add(Caption("Commitment"));
        panel.Children.Add(commitBar);
        panel.Children.Add(Caption("Closure"));
        panel.Children.Add(closeBar);
        panel.Children.Add(_missing);
        panel.Children.Add(_lifecycle);
        panel.Children.Add(Caption("AFEs"));
        panel.Children.Add(_afes);
        panel.Children.Add(Caption("Selected"));
        panel.Children.Add(_summary);
        panel.Children.Add(_status);
        panel.Children.Add(Caption("Variance"));
        panel.Children.Add(_variance);
        panel.Children.Add(Caption("Commitments"));
        panel.Children.Add(_commitments);
        panel.Children.Add(_commitmentGrid);
        panel.Children.Add(Caption("Revisions"));
        panel.Children.Add(_revisions);
        panel.Children.Add(_revisionGrid);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _title, _entity, _amount, _description, _supplementAmount, _supplementReason,
                     _commitReference, _commitAmount, _closeNote,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _kind.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgAfeWorkspace? Workspace => _workspace;

    public OgAfe? Afe => _selected;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string LifecycleText => _lifecycle.Text;

    public string SummaryText => _summary.Text;

    public string StatusText => _status.Text;

    public string VarianceText => _variance.Text;

    public string CommitmentsText => _commitments.Text;

    public string RevisionsText => _revisions.Text;

    public string DetailText => _detail.Text;

    public bool CanRaise => AfeRules.Missing(Entry()).Count == 0;

    public bool CanSubmit => _selected is { IsClosed: false } afe &&
                              string.Equals(afe.Status, AfeLifecycle.Draft,
                                  StringComparison.OrdinalIgnoreCase);

    public bool CanApprove => _selected is { IsClosed: false } afe &&
                               string.Equals(afe.Status, AfeLifecycle.Submitted,
                                   StringComparison.OrdinalIgnoreCase);

    public bool CanSupplement => _selected is { IsApproved: true, IsClosed: false };

    public bool CanCommit => _selected is { IsApproved: true, IsClosed: false };

    public bool CanClose => _selected is { IsApproved: true, IsClosed: false };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _afe.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _afes.ItemsSource = null;
            _header.Text = "AFE";
            _notice.Text = loaded.Message ?? "The AFEs could not be read.";
            ShowState(OgViewState.Failed, "The AFEs could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> RaiseAsync()
    {
        var entry = Entry();

        if (AfeRules.Missing(entry).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var raised = await _afe.RaiseAsync(entry, _application.Session.UserId).ConfigureAwait(true);

        if (!raised.Success || raised.Value is null)
        {
            LastFailed = true;
            _notice.Text = raised.Message ?? "The AFE was not raised.";
            ShowState(OgViewState.Failed, "The AFE was not raised.", raised.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(raised.Value);

        _application.AuditLog.Add(
            "economics.afe.raise",
            "economics",
            raised.Value.Selected?.Number ?? "(no number)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Raised " + (raised.Value.Selected?.Describe() ?? "the AFE") + ".";
        RefreshEnabled();
        return true;
    }

    public async Task<bool> SubmitAsync()
    {
        if (_selected is not { } afe)
        {
            _notice.Text = "Select the AFE to submit.";
            return false;
        }

        return await RunAsync(() => _afe.SubmitAsync(afe.Id, _application.Session.UserId),
            "economics.afe.submit", afe.Number, "Submitted").ConfigureAwait(true);
    }

    public async Task<bool> ApproveAsync()
    {
        if (_selected is not { } afe)
        {
            _notice.Text = "Select the AFE to approve.";
            return false;
        }

        return await RunAsync(() => _afe.ApproveAsync(afe.Id, _application.Session.UserId),
            "economics.afe.approve", afe.Number, "Approved").ConfigureAwait(true);
    }

    public async Task<bool> SupplementAsync()
    {
        if (_selected is not { } afe)
        {
            _notice.Text = "Select the approved AFE to supplement.";
            return false;
        }

        var amount = Parse(_supplementAmount.Text);

        if (amount is null)
        {
            _notice.Text = "A supplement needs its amount.";
            return false;
        }

        return await RunAsync(
            () => _afe.SupplementAsync(afe.Id, amount.Value, _supplementReason.Text ?? string.Empty,
                _application.Session.UserId),
            "economics.afe.supplement", afe.Number, "Supplement raised").ConfigureAwait(true);
    }

    public async Task<bool> CommitAsync()
    {
        if (_selected is not { } afe)
        {
            _notice.Text = "Select the AFE to commit against.";
            return false;
        }

        var amount = Parse(_commitAmount.Text);

        if (amount is null)
        {
            _notice.Text = "A commitment needs its amount.";
            return false;
        }

        return await RunAsync(
            () => _afe.CommitAsync(afe.Id, _commitReference.Text ?? string.Empty, amount.Value,
                _application.Session.UserId),
            "economics.afe.commit", afe.Number, "Commitment recorded").ConfigureAwait(true);
    }

    public async Task<bool> CloseAsync()
    {
        if (_selected is not { } afe)
        {
            _notice.Text = "Select the AFE to close.";
            return false;
        }

        return await RunAsync(
            () => _afe.CloseAsync(afe.Id, _closeNote.Text ?? string.Empty, _application.Session.UserId),
            "economics.afe.close", afe.Number, "Closed").ConfigureAwait(true);
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgAfeWorkspace>>> action,
        string auditAction,
        string subject,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _notice.Text = outcome.Message ?? "The AFE could not be moved.";
            ShowState(OgViewState.Failed, "The AFE could not be moved.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(outcome.Value);

        _application.AuditLog.Add(auditAction, "economics", subject, "ok", _application.Session.UserId);

        _notice.Text = done + ". " + outcome.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    private void Apply(OgAfeWorkspace workspace)
    {
        _workspace = workspace;

        _kind.ItemsSource = workspace.Kinds.ToList();
        _kind.SelectedIndex = _kind.SelectedIndex < 0 ? 0 : _kind.SelectedIndex;

        _currency.ItemsSource = workspace.Currencies.ToList();
        _currency.SelectedIndex = _currency.SelectedIndex < 0 ? 0 : _currency.SelectedIndex;

        _afes.ItemsSource = workspace.Afes.ToList();

        _header.Text = "Economics · AFE";
        _lifecycle.Text = "Lifecycle: " + workspace.LifecycleText;
        _notice.Text = workspace.HistoryFailed
            ? "The AFEs are unavailable: " + workspace.HistoryReason + "."
            : workspace.SummaryText;

        _selected = workspace.Selected;
        ShowSelected();
        RefreshEnabled();
    }

    private void ShowSelected()
    {
        if (_selected is not { } afe)
        {
            _summary.Text = "no AFE is selected";
            _status.Text = string.Empty;
            _variance.Text = string.Empty;
            _commitments.Text = string.Empty;
            _revisions.Text = string.Empty;
            _commitmentGrid.ItemsSource = null;
            _revisionGrid.ItemsSource = null;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text = afe.Describe();
        _status.Text = "Status: " + afe.StatusText + " · " + afe.ApprovalText + " · " + afe.ClosureText +
                       " · raised by " + (afe.Author ?? "(unknown)");
        _variance.Text = afe.VarianceText;
        _commitments.Text = afe.CommitmentsText;
        _revisions.Text = afe.RevisionsText;
        _commitmentGrid.ItemsSource = afe.Commitments.ToList();
        _revisionGrid.ItemsSource = afe.Revisions.ToList();
        _detail.Text = afe.Description ?? string.Empty;
    }

    private void SelectAfe()
    {
        if (_afes.SelectedItem is not OgAfe selected)
        {
            return;
        }

        _selected = selected;
        ShowSelected();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var missing = AfeRules.Missing(Entry());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The AFE needs " + string.Join(", ", missing) + ".";

        _raiseButton.IsEnabled = missing.Count == 0;
        _submitButton.IsEnabled = CanSubmit;
        _approveButton.IsEnabled = CanApprove;
        _supplementButton.IsEnabled = CanSupplement;
        _commitButton.IsEnabled = CanCommit;
        _closeButton.IsEnabled = CanClose;
    }

    private OgAfeEntry Entry() =>
        new(
            Empty(_title.Text),
            _kind.SelectedItem as string,
            Empty(_entity.Text),
            Parse(_amount.Text),
            _currency.SelectedItem as string,
            Empty(_description.Text));

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal? Parse(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
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
