using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Beep.OilandGas.Accounting.Calculations;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ACC-04: receivables - the ageing, the open invoices, a cash receipt allocated across them, and the one journal the
/// posting writes.
/// </summary>
/// <remarks>
/// The allocation total either equals the receipt or leaves unapplied cash shown explicitly (AC-1); the ageing buckets come
/// from due dates (AC-2); a partial payment leaves exactly the remainder (AC-3); an over-allocation is refused with the
/// excess (AC-4); posting writes one journal (AC-5); and the author cannot approve their own receipt (AC-6).
/// </remarks>
public sealed class ReceivablesPageView : OgPageBase
{
    public const string Route = "receivables";

    private readonly OgApplication _application;
    private readonly IOgReceivable _receivable;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _ageing;
    private readonly TextBlock _unapplied;
    private readonly TextBlock _summary;
    private readonly TextBlock _allocationText;
    private readonly TextBlock _journal;
    private readonly TextBlock _detail;
    private readonly TextBox _customer;
    private readonly TextBox _date;
    private readonly TextBox _amount;
    private readonly TextBox _referenceNumber;
    private readonly TextBox _bank;
    private readonly TextBox _allocations;
    private readonly TextBox _reviewReason;
    private readonly ComboBox _currency;
    private readonly DataGrid _receipts;
    private readonly DataGrid _openInvoices;
    private readonly DataGrid _ageingGrid;
    private readonly DataGrid _allocationGrid;
    private readonly Button _recordButton;
    private readonly Button _postButton;
    private readonly Button _approveButton;
    private readonly Button _returnButton;

    private OgReceivableWorkspace? _workspace;
    private OgReceipt? _selected;

    public ReceivablesPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _receivable = application.Receivables;

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

        _ageing = Body(string.Empty);
        _ageing.TextWrapping = TextWrapping.Wrap;
        _ageing.Margin = new Thickness(0, 4, 0, 4);

        _unapplied = Body(string.Empty);
        _unapplied.TextWrapping = TextWrapping.Wrap;
        _unapplied.Margin = new Thickness(0, 0, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _allocationText = Body(string.Empty);
        _allocationText.TextWrapping = TextWrapping.Wrap;
        _allocationText.Margin = new Thickness(0, 0, 0, 4);

        _journal = Body(string.Empty);
        _journal.TextWrapping = TextWrapping.Wrap;
        _journal.Margin = new Thickness(0, 0, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _customer = Field("Customer id", 120);
        _date = Field("Receipt date yyyy-MM-dd", 160);
        _date.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _amount = Field("Amount", 110);
        _referenceNumber = Field("Bank reference", 150);
        _bank = Field("Bank account", 130);
        _reviewReason = Field("Review reason", 240);

        _allocations = new TextBox
        {
            AcceptsReturn = true,
            IsTabStop = true,
            MinHeight = 60,
            MaxHeight = 120,
            Margin = new Thickness(0, 0, 0, 6),
            TextWrapping = TextWrapping.NoWrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "invoice id, amount - one line per invoice the cash settles",
        };

        Named(_allocations, "Allocations, one per invoice");
        _allocations.Text = "# invoice id, amount";

        _currency = Picker("Currency", 100);

        _receipts = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 180,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Number", "Number", 120),
                Text("Date", "Date", 100),
                Text("Customer", "CustomerId", 110),
                Text("Amount", "Amount", 110),
                Text("Unapplied", "Unapplied", 100),
                Text("State", "State", 100),
                Text("Author", "Author", 100),
            },
        };

        Named(_receipts, "Cash receipts");
        _receipts.SelectionChanged += (_, _) => SelectReceipt();

        _openInvoices = new DataGrid
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
                Text("Invoice", "Number", 120),
                Text("Customer", "CustomerId", 110),
                Text("Due", "DueDate", 110),
                Text("Balance", "Balance", 110),
                Text("Currency", "Currency", 90),
                Text("Bucket", "Bucket", 90),
            },
        };

        Named(_openInvoices, "Open invoices");
        _openInvoices.SelectionChanged += (_, _) => PickInvoice();

        _ageingGrid = new DataGrid
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
                Text("Customer", "CustomerId", 130),
                Text("Current", "Current", 110),
                Text("30", "Days30", 110),
                Text("60", "Days60", 110),
                Text("90+", "Days90", 110),
                Text("Total", "Total", 120),
            },
        };

        Named(_ageingGrid, "Ageing by customer");

        _allocationGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 80,
            MaxHeight = 130,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Invoice", "InvoiceId", 260),
                Text("Amount", "Amount", 130),
            },
        };

        Named(_allocationGrid, "Allocations on the selected receipt");

        _recordButton = Button("Record receipt", (_, _) => _ = RecordAsync());
        _postButton = Button("Post cash", (_, _) => _ = PostAsync());
        _postButton.IsEnabled = false;
        _approveButton = Button("Approve", (_, _) => _ = ApproveAsync());
        _approveButton.IsEnabled = false;
        _returnButton = Button("Return", (_, _) => _ = ReturnAsync());
        _returnButton.IsEnabled = false;

        var receiptBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        receiptBar.Children.Add(Caption("Customer"));
        receiptBar.Children.Add(_customer);
        receiptBar.Children.Add(Caption("Date"));
        receiptBar.Children.Add(_date);
        receiptBar.Children.Add(Caption("Amount"));
        receiptBar.Children.Add(_amount);
        receiptBar.Children.Add(_currency);

        var bankBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        bankBar.Children.Add(Caption("Reference"));
        bankBar.Children.Add(_referenceNumber);
        bankBar.Children.Add(Caption("Bank"));
        bankBar.Children.Add(_bank);
        bankBar.Children.Add(_recordButton);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_postButton);

        var reviewBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        reviewBar.Children.Add(Caption("Review reason"));
        reviewBar.Children.Add(_reviewReason);
        reviewBar.Children.Add(_approveButton);
        reviewBar.Children.Add(_returnButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Receipt"));
        panel.Children.Add(receiptBar);
        panel.Children.Add(bankBar);
        panel.Children.Add(Caption("Allocations"));
        panel.Children.Add(_allocations);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Posting"));
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Review"));
        panel.Children.Add(reviewBar);
        panel.Children.Add(Caption("Ageing"));
        panel.Children.Add(_ageing);
        panel.Children.Add(_ageingGrid);
        panel.Children.Add(Caption("Open invoices"));
        panel.Children.Add(_openInvoices);
        panel.Children.Add(Caption("Receipts"));
        panel.Children.Add(_receipts);
        panel.Children.Add(Caption("Selected receipt"));
        panel.Children.Add(_summary);
        panel.Children.Add(_allocationText);
        panel.Children.Add(_unapplied);
        panel.Children.Add(_journal);
        panel.Children.Add(_allocationGrid);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _customer, _date, _amount, _referenceNumber, _bank, _allocations, _reviewReason,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        Load();
    }

    public OgReceivableWorkspace? Workspace => _workspace;

    public OgReceipt? Receipt => _selected;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string AgeingText => _ageing.Text;

    public string UnappliedText => _unapplied.Text;

    public string SummaryText => _summary.Text;

    public string AllocationText => _allocationText.Text;

    public string JournalText => _journal.Text;

    public string DetailText => _detail.Text;

    public bool CanRecord => ReceivableRules.Missing(Inputs()).Count == 0;

    public bool CanPost => _selected is { IsDraft: true };

    public bool CanReview => _selected is { IsPosted: true } receipt && !receipt.IsApproved;

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    private OgRevenueContext Context => OgRevenueContext.FromSession(_application.Session);

    public bool Load()
    {
        var loaded = _receivable.LoadAsync(Context).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _receipts.ItemsSource = null;
            _header.Text = "Receivables";
            _notice.Text = loaded.Message ?? "The receipts could not be read.";
            ShowState(OgViewState.Failed, "The receipts could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public Task<bool> RecordAsync() =>
        RunAsync(() => _receivable.RecordAsync(Inputs(), Context, _application.Session.UserId),
            "accounting.receivable.write", "Receipt recorded");

    public Task<bool> PostAsync()
    {
        if (_selected is not { } receipt)
        {
            _notice.Text = "Select the receipt to post.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _receivable.PostAsync(receipt.Id, Context, _application.Session.UserId),
            "accounting.receivable.post", "Cash posted - one journal written");
    }

    public Task<bool> ApproveAsync() => ReviewAsync(true);

    public Task<bool> ReturnAsync() => ReviewAsync(false);

    private Task<bool> ReviewAsync(bool approve)
    {
        if (_selected is not { } receipt)
        {
            _notice.Text = "Select the receipt to review.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _receivable.ReviewAsync(receipt.Id, approve, _reviewReason.Text ?? string.Empty,
                _application.Session.UserId),
            approve ? "accounting.receivable.approve" : "accounting.receivable.return",
            approve ? "Approved" : "Returned to the author");
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgReceivableWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _notice.Text = outcome.Message ?? "The receivables command was refused.";
            ShowState(OgViewState.Failed, "The receivables command was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(outcome.Value);

        _application.AuditLog.Add(
            auditAction,
            "accounting",
            outcome.Value.Selected?.Number ?? "(receipt)",
            "ok",
            _application.Session.UserId);

        _notice.Text = done + ". " + outcome.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    private void Apply(OgReceivableWorkspace workspace)
    {
        _workspace = workspace;

        _currency.ItemsSource = workspace.Currencies.ToList();
        _currency.SelectedIndex = _currency.SelectedIndex < 0 ? 0 : _currency.SelectedIndex;

        _receipts.ItemsSource = workspace.Receipts.ToList();
        _openInvoices.ItemsSource = workspace.OpenInvoices.ToList();
        _ageingGrid.ItemsSource = workspace.Ageing.ToList();

        _header.Text = "Accounting · receivables";
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The receipts are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;
        _ageing.Text = workspace.AgeingSummaryText;
        _unapplied.Text = workspace.UnappliedText;

        _selected = workspace.Selected;
        ShowReceipt();
        RefreshEnabled();
    }

    private void ShowReceipt()
    {
        if (_selected is not { } receipt)
        {
            _summary.Text = "no receipt is selected";
            _allocationText.Text = string.Empty;
            _journal.Text = string.Empty;
            _allocationGrid.ItemsSource = null;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text = receipt.Describe() + " · " + receipt.StateText;
        _allocationText.Text = receipt.AllocationText;
        _journal.Text = receipt.JournalText;
        _allocationGrid.ItemsSource = receipt.Allocations.ToList();
        _detail.Text = string.IsNullOrWhiteSpace(receipt.ReturnReason)
            ? "reference " + (receipt.Reference ?? "(none)") + " · bank " + (receipt.BankAccount ?? "(none)")
            : "returned: " + receipt.ReturnReason;
    }

    private void SelectReceipt()
    {
        if (_receipts.SelectedItem is not OgReceipt selected)
        {
            return;
        }

        _selected = selected;
        ShowReceipt();
        RefreshEnabled();
    }

    /// <summary>Puts the selected open invoice on the next allocation line, so the id is not retyped.</summary>
    private void PickInvoice()
    {
        if (_openInvoices.SelectedItem is not OgOpenInvoice invoice)
        {
            return;
        }

        var lines = (_allocations.Text ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !line.StartsWith('#'))
            .ToList();

        var amount = invoice.Balance.ToString("0.00", CultureInfo.InvariantCulture);
        lines.Add(invoice.Id + ", " + amount);

        _allocations.Text = "# invoice id, amount" + Environment.NewLine + string.Join(Environment.NewLine, lines);
    }

    private void RefreshEnabled()
    {
        var missing = ReceivableRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The receipt needs " + string.Join(", ", missing) + ".";

        _recordButton.IsEnabled = missing.Count == 0;
        _postButton.IsEnabled = CanPost;
        _approveButton.IsEnabled = CanReview;
        _returnButton.IsEnabled = CanReview;
    }

    private OgReceiptInputs Inputs() =>
        new(
            Empty(_customer.Text),
            Date(_date.Text),
            Parse(_amount.Text),
            _currency.SelectedItem as string,
            Empty(_referenceNumber.Text),
            Empty(_bank.Text),
            Allocations());

    /// <summary>Reads the allocation editor: <c>invoice id, amount</c> per line.</summary>
    private IReadOnlyList<OgAllocationInput> Allocations()
    {
        var allocations = new List<OgAllocationInput>();

        foreach (var row in (_allocations.Text ?? string.Empty)
                     .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (row.StartsWith('#'))
            {
                continue;
            }

            var parts = row.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 2)
            {
                continue;
            }

            allocations.Add(new OgAllocationInput(
                string.IsNullOrWhiteSpace(parts[0]) ? null : parts[0],
                Parse(parts[1])));
        }

        return allocations;
    }

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
