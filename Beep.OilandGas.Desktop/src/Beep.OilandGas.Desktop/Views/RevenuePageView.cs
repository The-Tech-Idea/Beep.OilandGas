using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Beep.OilandGas.Accounting.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ACC-03: revenue - sold volumes turned into a sales invoice, its one ledger entry, and the credit notes that reduce what
/// the buyer still owes.
/// </summary>
/// <remarks>
/// A volume without a price is blocked naming the volume, a currency that disagrees with the contract is refused, the tax
/// comes from the owning project's calculator (AC-5), issuing writes exactly one journal (AC-2) that a retry cannot
/// duplicate (AC-6), each line keeps the volume it came from (AC-3), the author cannot approve (AC-4), and a credit note
/// reduces the balance by exactly its amount (AC-1, SG-10-5).
/// </remarks>
public sealed class RevenuePageView : OgPageBase
{
    public const string Route = "revenue";

    private readonly OgApplication _application;
    private readonly IOgRevenue _revenue;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _summary;
    private readonly TextBlock _money;
    private readonly TextBlock _balance;
    private readonly TextBlock _journal;
    private readonly TextBlock _creditText;
    private readonly TextBlock _detail;
    private readonly TextBox _buyer;
    private readonly TextBox _contract;
    private readonly TextBox _contractCurrency;
    private readonly TextBox _date;
    private readonly TextBox _dueDate;
    private readonly TextBox _taxRate;
    private readonly TextBox _lines;
    private readonly TextBox _creditAmount;
    private readonly TextBox _creditReason;
    private readonly TextBox _reviewReason;
    private readonly ComboBox _currency;
    private readonly DataGrid _invoices;
    private readonly DataGrid _lineGrid;
    private readonly DataGrid _creditGrid;
    private readonly Button _buildButton;
    private readonly Button _issueButton;
    private readonly Button _creditButton;
    private readonly Button _approveButton;
    private readonly Button _returnButton;
    private readonly Button _deleteButton;

    private OgRevenueWorkspace? _workspace;
    private OgSalesInvoice? _selected;

    public RevenuePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _revenue = application.Revenue;

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

        _money = Body(string.Empty);
        _money.TextWrapping = TextWrapping.Wrap;
        _money.Margin = new Thickness(0, 0, 0, 4);

        _balance = Body(string.Empty);
        _balance.TextWrapping = TextWrapping.Wrap;
        _balance.Margin = new Thickness(0, 0, 0, 4);

        _journal = Body(string.Empty);
        _journal.TextWrapping = TextWrapping.Wrap;
        _journal.Margin = new Thickness(0, 0, 0, 4);

        _creditText = Body(string.Empty);
        _creditText.TextWrapping = TextWrapping.Wrap;
        _creditText.Margin = new Thickness(0, 0, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _buyer = Field("Buyer id", 120);
        _contract = Field("Contract id", 120);
        _contractCurrency = Field("Contract currency", 130);
        _date = Field("Invoice date yyyy-MM-dd", 160);
        _date.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _dueDate = Field("Due date yyyy-MM-dd", 150);
        _taxRate = Field("Tax rate", 90);
        _creditAmount = Field("Credit amount", 120);
        _creditReason = Field("Credit reason", 240);
        _reviewReason = Field("Review reason", 240);

        _currency = Picker("Currency", 100);

        _lines = new TextBox
        {
            AcceptsReturn = true,
            IsTabStop = true,
            MinHeight = 70,
            MaxHeight = 140,
            Margin = new Thickness(0, 0, 0, 6),
            TextWrapping = TextWrapping.NoWrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "commodity, quantity, unit, unit price, volume reference - one line per sold volume",
        };

        Named(_lines, "Invoice lines, one per sold volume");
        _lines.Text = "# commodity, quantity, unit, unit price, volume reference";

        _invoices = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 120,
            MaxHeight = 190,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Number", "Number", 110),
                Text("Date", "Date", 100),
                Text("Buyer", "BuyerId", 110),
                Text("Total", "Total", 120),
                Text("Balance", "BalanceDue", 110),
                Text("State", "State", 100),
                Text("Author", "Author", 100),
            },
        };

        Named(_invoices, "Sales invoices");
        _invoices.SelectionChanged += (_, _) => SelectInvoice();

        _lineGrid = new DataGrid
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
                Text("Line", "LineNumber", 60),
                Text("Commodity", "Commodity", 110),
                Text("Quantity", "Quantity", 100),
                Text("Unit", "Unit", 70),
                Text("Price", "UnitPrice", 100),
                Text("Net", "Net", 100),
                Text("Tax", "Tax", 90),
                Text("Volume", "VolumeReference", 140),
            },
        };

        Named(_lineGrid, "Invoice lines and the volume each came from");

        _creditGrid = new DataGrid
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
                Text("Number", "Number", 110),
                Text("Date", "Date", 100),
                Text("Amount", "Amount", 110),
                Text("Status", "Status", 90),
                Text("Reason", "Reason", 240),
            },
        };

        Named(_creditGrid, "Credit notes against the invoice");

        _buildButton = Button("Build draft", (_, _) => _ = BuildAsync());
        _issueButton = Button("Issue", (_, _) => _ = IssueAsync());
        _issueButton.IsEnabled = false;
        _creditButton = Button("Issue credit note", (_, _) => _ = CreditAsync());
        _creditButton.IsEnabled = false;
        _approveButton = Button("Approve", (_, _) => _ = ApproveAsync());
        _approveButton.IsEnabled = false;
        _returnButton = Button("Return", (_, _) => _ = ReturnAsync());
        _returnButton.IsEnabled = false;
        _deleteButton = Button("Delete draft", (_, _) => _ = DeleteAsync());
        _deleteButton.IsEnabled = false;

        var buyerBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        buyerBar.Children.Add(Caption("Buyer"));
        buyerBar.Children.Add(_buyer);
        buyerBar.Children.Add(Caption("Contract"));
        buyerBar.Children.Add(_contract);
        buyerBar.Children.Add(Caption("Contract currency"));
        buyerBar.Children.Add(_contractCurrency);
        buyerBar.Children.Add(Caption("Invoice currency"));
        buyerBar.Children.Add(_currency);

        var dateBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        dateBar.Children.Add(Caption("Date"));
        dateBar.Children.Add(_date);
        dateBar.Children.Add(Caption("Due"));
        dateBar.Children.Add(_dueDate);
        dateBar.Children.Add(Caption("Tax rate"));
        dateBar.Children.Add(_taxRate);
        dateBar.Children.Add(_buildButton);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_issueButton);
        actionBar.Children.Add(_deleteButton);

        var creditBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        creditBar.Children.Add(Caption("Credit"));
        creditBar.Children.Add(_creditAmount);
        creditBar.Children.Add(Caption("Reason"));
        creditBar.Children.Add(_creditReason);
        creditBar.Children.Add(_creditButton);

        var reviewBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        reviewBar.Children.Add(Caption("Review reason"));
        reviewBar.Children.Add(_reviewReason);
        reviewBar.Children.Add(_approveButton);
        reviewBar.Children.Add(_returnButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Invoice"));
        panel.Children.Add(buyerBar);
        panel.Children.Add(dateBar);
        panel.Children.Add(Caption("Sold volumes"));
        panel.Children.Add(_lines);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Issue"));
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Credit note"));
        panel.Children.Add(creditBar);
        panel.Children.Add(Caption("Review"));
        panel.Children.Add(reviewBar);
        panel.Children.Add(Caption("Invoices"));
        panel.Children.Add(_invoices);
        panel.Children.Add(Caption("Selected invoice"));
        panel.Children.Add(_summary);
        panel.Children.Add(_money);
        panel.Children.Add(_balance);
        panel.Children.Add(_journal);
        panel.Children.Add(Caption("Lines"));
        panel.Children.Add(_lineGrid);
        panel.Children.Add(Caption("Credits"));
        panel.Children.Add(_creditText);
        panel.Children.Add(_creditGrid);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _buyer, _contract, _contractCurrency, _date, _dueDate, _taxRate, _lines, _creditAmount,
                     _creditReason, _reviewReason,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        Load();
    }

    public OgRevenueWorkspace? Workspace => _workspace;

    public OgSalesInvoice? Invoice => _selected;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string SummaryText => _summary.Text;

    public string MoneySummaryText => _money.Text;

    public string BalanceText => _balance.Text;

    public string JournalText => _journal.Text;

    public string CreditsText => _creditText.Text;

    public string DetailText => _detail.Text;

    public bool CanBuild => RevenueRules.Missing(Inputs()).Count == 0;

    public bool CanIssue => _selected is { IsDraft: true, HasJournal: false };

    public bool CanCredit => _selected is { IsIssued: true } invoice && invoice.BalanceDue > 0m;

    public bool CanReview => _selected is { IsIssued: true } invoice &&
                             !string.Equals(invoice.State, RevenueCodes.InvoiceApproved,
                                 StringComparison.OrdinalIgnoreCase);

    public bool CanDelete => _selected is { CanDelete: true };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    private OgRevenueContext Context => OgRevenueContext.FromSession(_application.Session);

    public bool Load()
    {
        var loaded = _revenue.LoadAsync(Context).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _invoices.ItemsSource = null;
            _header.Text = "Revenue";
            _notice.Text = loaded.Message ?? "The invoices could not be read.";
            ShowState(OgViewState.Failed, "The invoices could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public Task<bool> BuildAsync() =>
        RunAsync(() => _revenue.BuildAsync(Inputs(), Context, _application.Session.UserId),
            "accounting.revenue.write", "Draft built");

    public Task<bool> IssueAsync()
    {
        if (_selected is not { } invoice)
        {
            _notice.Text = "Select the invoice to issue.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _revenue.IssueAsync(invoice.Id, Context, _application.Session.UserId),
            "accounting.revenue.issue", "Issued - one ledger entry written");
    }

    public Task<bool> CreditAsync()
    {
        if (_selected is not { } invoice)
        {
            _notice.Text = "Select the invoice to credit.";
            return Task.FromResult(false);
        }

        var amount = Parse(_creditAmount.Text);

        if (amount is null)
        {
            _notice.Text = "A credit note needs its amount.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _revenue.CreditAsync(invoice.Id, amount.Value, _creditReason.Text ?? string.Empty,
                _application.Session.UserId),
            "accounting.revenue.credit", "Credit note issued");
    }

    public Task<bool> ApproveAsync() => ReviewAsync(true);

    public Task<bool> ReturnAsync() => ReviewAsync(false);

    private Task<bool> ReviewAsync(bool approve)
    {
        if (_selected is not { } invoice)
        {
            _notice.Text = "Select the invoice to review.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _revenue.ReviewAsync(invoice.Id, approve, _reviewReason.Text ?? string.Empty,
                _application.Session.UserId),
            approve ? "accounting.revenue.approve" : "accounting.revenue.return",
            approve ? "Approved" : "Returned to the author");
    }

    public Task<bool> DeleteAsync()
    {
        if (_selected is not { } invoice)
        {
            _notice.Text = "Select the draft to delete.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _revenue.DeleteAsync(invoice.Id, _application.Session.UserId),
            "accounting.revenue.delete", "Draft deleted");
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgRevenueWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _notice.Text = outcome.Message ?? "The revenue command was refused.";
            ShowState(OgViewState.Failed, "The revenue command was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(outcome.Value);

        _application.AuditLog.Add(
            auditAction,
            "accounting",
            outcome.Value.Selected?.Number ?? "(invoice)",
            "ok",
            _application.Session.UserId);

        _notice.Text = done + ". " + outcome.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    private void Apply(OgRevenueWorkspace workspace)
    {
        _workspace = workspace;

        _currency.ItemsSource = workspace.Currencies.ToList();
        _currency.SelectedIndex = _currency.SelectedIndex < 0 ? 0 : _currency.SelectedIndex;

        _invoices.ItemsSource = workspace.Invoices.ToList();

        _header.Text = "Accounting · revenue";
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The invoices are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText + " · " + workspace.OutstandingText;

        _selected = workspace.Selected;
        ShowInvoice();
        RefreshEnabled();
    }

    private void ShowInvoice()
    {
        if (_selected is not { } invoice)
        {
            _summary.Text = "no invoice is selected";
            _money.Text = string.Empty;
            _balance.Text = string.Empty;
            _journal.Text = string.Empty;
            _creditText.Text = string.Empty;
            _lineGrid.ItemsSource = null;
            _creditGrid.ItemsSource = null;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text = invoice.Describe() + " · " + invoice.StateText;
        _money.Text = invoice.TotalsText;
        _balance.Text = invoice.BalanceText;
        _journal.Text = invoice.JournalText;
        _creditText.Text = invoice.CreditsText;
        _lineGrid.ItemsSource = invoice.Lines.ToList();
        _creditGrid.ItemsSource = invoice.CreditNotes.ToList();
        _detail.Text = string.IsNullOrWhiteSpace(invoice.ReturnReason)
            ? "built by " + (invoice.Author ?? "(unknown)")
            : "returned: " + invoice.ReturnReason;
    }

    private void SelectInvoice()
    {
        if (_invoices.SelectedItem is not OgSalesInvoice selected)
        {
            return;
        }

        _selected = selected;
        ShowInvoice();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var missing = RevenueRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The invoice needs " + string.Join(", ", missing) + ".";

        _buildButton.IsEnabled = missing.Count == 0;
        _issueButton.IsEnabled = CanIssue;
        _creditButton.IsEnabled = CanCredit;
        _approveButton.IsEnabled = CanReview;
        _returnButton.IsEnabled = CanReview;
        _deleteButton.IsEnabled = CanDelete;
    }

    private OgInvoiceInputs Inputs() =>
        new(
            Empty(_buyer.Text),
            Empty(_contract.Text),
            Empty(_contractCurrency.Text),
            Date(_date.Text),
            Date(_dueDate.Text),
            _currency.SelectedItem as string,
            Parse(_taxRate.Text),
            Lines());

    /// <summary>Reads the line editor: <c>commodity, quantity, unit, unit price, volume reference</c> per line.</summary>
    private IReadOnlyList<OgSalesLineInput> Lines()
    {
        var lines = new List<OgSalesLineInput>();

        foreach (var row in (_lines.Text ?? string.Empty)
                     .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (row.StartsWith('#'))
            {
                continue;
            }

            var parts = row.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 4)
            {
                continue;
            }

            lines.Add(new OgSalesLineInput(
                string.IsNullOrWhiteSpace(parts[0]) ? null : parts[0],
                null,
                Parse(parts[1]),
                string.IsNullOrWhiteSpace(parts[2]) ? null : parts[2],
                Parse(parts[3]),
                parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]) ? parts[4] : null));
        }

        return lines;
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
