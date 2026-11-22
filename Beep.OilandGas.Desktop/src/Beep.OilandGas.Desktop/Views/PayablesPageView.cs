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
/// ACC-05: payables and procurement - the order, the goods received, the supplier invoice, the three-way match, and the
/// payment run that settles the lot with one journal.
/// </summary>
/// <remarks>
/// The match refuses a receipt above the order (AC-1), turns a price outside the recorded tolerance into an exception rather
/// than a silent pass (AC-2), refuses a duplicate supplier invoice number (AC-3), refuses a commitment above the AFE
/// remaining (AC-4), pays with exactly one journal (AC-5) and refuses the author as approver of either the exception or the
/// run (AC-6). The tolerances are printed from the recorded policy, so the screen shows the rule it applied.
/// </remarks>
public sealed class PayablesPageView : OgPageBase
{
    public const string Route = "payables";

    private readonly OgApplication _application;
    private readonly IOgPayables _payables;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _policy;
    private readonly TextBlock _reference;
    private readonly TextBlock _summary;
    private readonly TextBlock _approval;
    private readonly TextBlock _orderLines;
    private readonly TextBlock _invoiceMoney;
    private readonly TextBlock _match;
    private readonly TextBlock _exceptions;
    private readonly TextBlock _runState;
    private readonly TextBlock _detail;
    private readonly TextBox _orderVendor;
    private readonly TextBox _orderDate;
    private readonly TextBox _orderDelivery;
    private readonly TextBox _orderAfe;
    private readonly TextBox _orderDescription;
    private readonly TextBox _orderLinesText;
    private readonly TextBox _receiveQuantity;
    private readonly TextBox _invoiceVendor;
    private readonly TextBox _invoiceNumber;
    private readonly TextBox _invoiceDate;
    private readonly TextBox _invoiceDue;
    private readonly TextBox _invoiceTax;
    private readonly TextBox _invoiceOrder;
    private readonly TextBox _invoiceAfe;
    private readonly TextBox _invoiceLinesText;
    private readonly TextBox _runDate;
    private readonly TextBox _runReason;
    private readonly ComboBox _orderCurrency;
    private readonly ComboBox _invoiceCurrency;
    private readonly DataGrid _orders;
    private readonly DataGrid _orderLineGrid;
    private readonly DataGrid _invoices;
    private readonly DataGrid _invoiceLineGrid;
    private readonly DataGrid _runGrid;
    private readonly Button _raiseButton;
    private readonly Button _approveOrderButton;
    private readonly Button _receiveButton;
    private readonly Button _captureButton;
    private readonly Button _approveExceptionButton;
    private readonly Button _proposeRunButton;
    private readonly Button _approveRunButton;
    private readonly Button _returnRunButton;
    private readonly Button _postRunButton;

    private OgPayablesWorkspace? _workspace;
    private OgPurchaseOrder? _order;
    private OgApInvoice? _invoice;
    private OgPaymentRun? _run;

    public PayablesPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _payables = application.Payables;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 4, 0, 4);

        _policy = Body(string.Empty);
        _policy.TextWrapping = TextWrapping.Wrap;
        _policy.Margin = new Thickness(0, 4, 0, 4);

        _reference = Body(string.Empty);
        _reference.TextWrapping = TextWrapping.Wrap;
        _reference.Margin = new Thickness(0, 0, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _approval = Body(string.Empty);
        _approval.TextWrapping = TextWrapping.Wrap;
        _approval.Margin = new Thickness(0, 0, 0, 4);

        _orderLines = Body(string.Empty);
        _orderLines.TextWrapping = TextWrapping.Wrap;
        _orderLines.Margin = new Thickness(0, 0, 0, 4);

        _invoiceMoney = Body(string.Empty);
        _invoiceMoney.TextWrapping = TextWrapping.Wrap;
        _invoiceMoney.Margin = new Thickness(0, 0, 0, 4);

        _match = Body(string.Empty);
        _match.TextWrapping = TextWrapping.Wrap;
        _match.Margin = new Thickness(0, 4, 0, 4);

        _exceptions = Body(string.Empty);
        _exceptions.TextWrapping = TextWrapping.Wrap;
        _exceptions.Margin = new Thickness(0, 0, 0, 4);

        _runState = Body(string.Empty);
        _runState.TextWrapping = TextWrapping.Wrap;
        _runState.Margin = new Thickness(0, 4, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _orderVendor = Field("Vendor id", 120);
        _orderDate = Field("Order date yyyy-MM-dd", 160);
        _orderDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _orderDelivery = Field("Expected delivery yyyy-MM-dd", 180);
        _orderAfe = Field("AFE id", 120);
        _orderDescription = Field("Order description", 260);
        _receiveQuantity = Field("Receive quantity", 130);

        _invoiceVendor = Field("Vendor id", 120);
        _invoiceNumber = Field("Supplier invoice number", 170);
        _invoiceDate = Field("Invoice date yyyy-MM-dd", 160);
        _invoiceDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _invoiceDue = Field("Due date yyyy-MM-dd", 150);
        _invoiceTax = Field("Tax", 90);
        _invoiceOrder = Field("Order id", 120);
        _invoiceAfe = Field("AFE id", 120);

        _runDate = Field("Run date yyyy-MM-dd", 150);
        _runDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _runReason = Field("Return reason", 240);

        _orderCurrency = Picker("Order currency", 110);
        _invoiceCurrency = Picker("Invoice currency", 110);

        _orderLinesText = LinesBox("Order lines: description, quantity, unit, unit price");
        _invoiceLinesText = LinesBox("Invoice lines: description, quantity, unit, unit price, order line id");

        _orders = new DataGrid
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
                Text("Vendor", "VendorId", 110),
                Text("Total", "Total", 120),
                Text("Currency", "Currency", 90),
                Text("State", "State", 100),
                Text("Author", "Author", 100),
            },
        };

        Named(_orders, "Purchase orders");
        _orders.SelectionChanged += (_, _) => SelectOrder();

        _orderLineGrid = new DataGrid
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
                Text("Description", "Description", 220),
                Text("Ordered", "Quantity", 100),
                Text("Unit", "Unit", 70),
                Text("Price", "UnitPrice", 100),
                Text("Received", "Received", 100),
                Text("Invoiced", "Invoiced", 100),
                Text("Open", "Open", 90),
            },
        };

        Named(_orderLineGrid, "Order lines");

        _invoices = new DataGrid
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
                Text("Vendor", "VendorId", 110),
                Text("Total", "Total", 120),
                Text("Balance", "Balance", 110),
                Text("Match", "MatchState", 150),
                Text("Policy", "PolicyId", 110),
            },
        };

        Named(_invoices, "Supplier invoices");
        _invoices.SelectionChanged += (_, _) => SelectInvoice();

        _invoiceLineGrid = new DataGrid
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
                Text("Description", "Description", 220),
                Text("Quantity", "Quantity", 100),
                Text("Unit", "Unit", 70),
                Text("Price", "UnitPrice", 100),
                Text("Total", "LineTotal", 110),
                Text("Order line", "PoLineItemId", 130),
            },
        };

        Named(_invoiceLineGrid, "Invoice lines and the order line each matches");

        _runGrid = new DataGrid
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
                Text("Number", "Number", 120),
                Text("Date", "Date", 100),
                Text("Total", "Total", 120),
                Text("Currency", "Currency", 90),
                Text("State", "Status", 100),
                Text("Proposed by", "Author", 110),
            },
        };

        Named(_runGrid, "Payment runs");
        _runGrid.SelectionChanged += (_, _) => SelectRun();

        _raiseButton = Button("Raise order", (_, _) => _ = RaiseAsync());
        _approveOrderButton = Button("Approve order", (_, _) => _ = ApproveOrderAsync());
        _approveOrderButton.IsEnabled = false;
        _receiveButton = Button("Book in goods", (_, _) => _ = ReceiveAsync());
        _receiveButton.IsEnabled = false;
        _captureButton = Button("Capture invoice", (_, _) => _ = CaptureAsync());
        _approveExceptionButton = Button("Approve exception", (_, _) => _ = ApproveExceptionAsync());
        _approveExceptionButton.IsEnabled = false;
        _proposeRunButton = Button("Propose run", (_, _) => _ = ProposeRunAsync());
        _approveRunButton = Button("Approve run", (_, _) => _ = ApproveRunAsync());
        _approveRunButton.IsEnabled = false;
        _returnRunButton = Button("Return run", (_, _) => _ = ReturnRunAsync());
        _returnRunButton.IsEnabled = false;
        _postRunButton = Button("Post run", (_, _) => _ = PostRunAsync());
        _postRunButton.IsEnabled = false;

        var orderBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        orderBar.Children.Add(Caption("Vendor"));
        orderBar.Children.Add(_orderVendor);
        orderBar.Children.Add(Caption("Date"));
        orderBar.Children.Add(_orderDate);
        orderBar.Children.Add(Caption("Delivery"));
        orderBar.Children.Add(_orderDelivery);
        orderBar.Children.Add(_orderCurrency);

        var orderBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        orderBar2.Children.Add(Caption("AFE"));
        orderBar2.Children.Add(_orderAfe);
        orderBar2.Children.Add(Caption("Description"));
        orderBar2.Children.Add(_orderDescription);
        orderBar2.Children.Add(_raiseButton);

        var orderActionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        orderActionBar.Children.Add(_approveOrderButton);
        orderActionBar.Children.Add(Caption("Receive"));
        orderActionBar.Children.Add(_receiveQuantity);
        orderActionBar.Children.Add(_receiveButton);

        var invoiceBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        invoiceBar.Children.Add(Caption("Vendor"));
        invoiceBar.Children.Add(_invoiceVendor);
        invoiceBar.Children.Add(Caption("Number"));
        invoiceBar.Children.Add(_invoiceNumber);
        invoiceBar.Children.Add(Caption("Date"));
        invoiceBar.Children.Add(_invoiceDate);
        invoiceBar.Children.Add(Caption("Due"));
        invoiceBar.Children.Add(_invoiceDue);

        var invoiceBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        invoiceBar2.Children.Add(Caption("Tax"));
        invoiceBar2.Children.Add(_invoiceTax);
        invoiceBar2.Children.Add(_invoiceCurrency);
        invoiceBar2.Children.Add(Caption("Order"));
        invoiceBar2.Children.Add(_invoiceOrder);
        invoiceBar2.Children.Add(Caption("AFE"));
        invoiceBar2.Children.Add(_invoiceAfe);
        invoiceBar2.Children.Add(_captureButton);

        var exceptionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        exceptionBar.Children.Add(_approveExceptionButton);

        var runBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        runBar.Children.Add(Caption("Run date"));
        runBar.Children.Add(_runDate);
        runBar.Children.Add(_proposeRunButton);
        runBar.Children.Add(_approveRunButton);
        runBar.Children.Add(_postRunButton);

        var returnBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        returnBar.Children.Add(Caption("Reason"));
        returnBar.Children.Add(_runReason);
        returnBar.Children.Add(_returnRunButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_policy);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Raise a purchase order"));
        panel.Children.Add(orderBar);
        panel.Children.Add(orderBar2);
        panel.Children.Add(Caption("Order lines"));
        panel.Children.Add(_orderLinesText);
        panel.Children.Add(Caption("Approve and receive"));
        panel.Children.Add(orderActionBar);
        panel.Children.Add(Caption("Capture a supplier invoice"));
        panel.Children.Add(invoiceBar);
        panel.Children.Add(invoiceBar2);
        panel.Children.Add(Caption("Invoice lines"));
        panel.Children.Add(_invoiceLinesText);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Match exception"));
        panel.Children.Add(exceptionBar);
        panel.Children.Add(Caption("Payment run"));
        panel.Children.Add(runBar);
        panel.Children.Add(returnBar);
        panel.Children.Add(_runState);
        panel.Children.Add(Caption("Orders"));
        panel.Children.Add(_orders);
        panel.Children.Add(Caption("Selected order"));
        panel.Children.Add(_summary);
        panel.Children.Add(_approval);
        panel.Children.Add(_orderLines);
        panel.Children.Add(_orderLineGrid);
        panel.Children.Add(Caption("Invoices"));
        panel.Children.Add(_invoices);
        panel.Children.Add(Caption("Selected invoice"));
        panel.Children.Add(_invoiceMoney);
        panel.Children.Add(Caption("Three-way match"));
        panel.Children.Add(_match);
        panel.Children.Add(_exceptions);
        panel.Children.Add(_invoiceLineGrid);
        panel.Children.Add(Caption("Payment runs"));
        panel.Children.Add(_runGrid);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _orderVendor, _orderDate, _orderDelivery, _orderAfe, _orderDescription, _orderLinesText,
                     _receiveQuantity, _invoiceVendor, _invoiceNumber, _invoiceDate, _invoiceDue, _invoiceTax,
                     _invoiceOrder, _invoiceAfe, _invoiceLinesText, _runDate, _runReason,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _orderCurrency.SelectionChanged += (_, _) => RefreshEnabled();
        _invoiceCurrency.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgPayablesWorkspace? Workspace => _workspace;

    public OgPurchaseOrder? Order => _order;

    public OgApInvoice? Invoice => _invoice;

    public OgPaymentRun? Run => _run;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string PolicyText => _policy.Text;

    public string ReferenceText => _reference.Text;

    public string SummaryText => _summary.Text;

    public string ApprovalText => _approval.Text;

    public string OrderLinesText => _orderLines.Text;

    public string InvoiceMoneyText => _invoiceMoney.Text;

    public string MatchText => _match.Text;

    public string ExceptionsText => _exceptions.Text;

    public string RunStateText => _runState.Text;

    public string DetailText => _detail.Text;

    public bool CanRaise => PayableRules.OrderMissing(OrderInputs()).Count == 0;

    public bool CanApproveOrder => _order is { IsDraft: true };

    public bool CanReceive => _order is { AcceptsReceipts: true } &&
                              _orderLineGrid.SelectedItem is OgOrderLine &&
                              Parse(_receiveQuantity.Text) is > 0m;

    public bool CanCapture => PayableRules.InvoiceMissing(InvoiceInputs()).Count == 0;

    public bool CanApproveException => _invoice is { NeedsExceptionApproval: true };

    public bool CanApproveRun => _run is { IsDraft: true };

    public bool CanReturnRun => _run is { IsDraft: true };

    public bool CanPostRun => _run is { IsApproved: true };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    private OgPayableContext Context => OgPayableContext.FromSession(_application.Session);

    public bool Load()
    {
        var loaded = _payables.LoadAsync(Context).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _orders.ItemsSource = null;
            _invoices.ItemsSource = null;
            _runGrid.ItemsSource = null;
            _header.Text = "Payables";
            _notice.Text = loaded.Message ?? "The payables ledger could not be read.";
            ShowState(OgViewState.Failed, "The payables ledger could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public Task<bool> RaiseAsync() =>
        RunAsync(() => _payables.RaiseOrderAsync(OrderInputs(), Context, _application.Session.UserId),
            "accounting.payable.write", "Order raised");

    public Task<bool> ApproveOrderAsync()
    {
        if (_order is not { } order)
        {
            _notice.Text = "Select the order to approve.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _payables.ApproveOrderAsync(order.Id, _application.Session.UserId),
            "accounting.payable.approve", "Order approved");
    }

    public Task<bool> ReceiveAsync()
    {
        if (_order is not { } order)
        {
            _notice.Text = "Select the order to receive against.";
            return Task.FromResult(false);
        }

        if (_orderLineGrid.SelectedItem is not OgOrderLine line)
        {
            _notice.Text = "Select the order line being received.";
            return Task.FromResult(false);
        }

        var quantity = Parse(_receiveQuantity.Text);

        if (quantity is null)
        {
            _notice.Text = "A receipt needs its quantity.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _payables.ReceiveAsync(order.Id, line.Id, quantity.Value, _application.Session.UserId),
            "accounting.payable.receipt",
            "Goods booked in");
    }

    public Task<bool> CaptureAsync() =>
        RunAsync(() => _payables.CaptureAsync(InvoiceInputs(), Context, _application.Session.UserId),
            "accounting.payable.write", "Invoice captured");

    public Task<bool> ApproveExceptionAsync()
    {
        if (_invoice is not { } invoice)
        {
            _notice.Text = "Select the invoice whose exception is approved.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _payables.ApproveExceptionAsync(invoice.Id, _application.Session.UserId),
            "accounting.payable.approve", "Match exception approved");
    }

    public Task<bool> ProposeRunAsync()
    {
        var date = Date(_runDate.Text);

        if (date is null)
        {
            _notice.Text = "A run needs its date.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _payables.ProposeRunAsync(date.Value, Context, _application.Session.UserId),
            "accounting.payable.pay", "Run proposed");
    }

    public Task<bool> ApproveRunAsync()
    {
        if (_run is not { } run)
        {
            _notice.Text = "Select the run to approve.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _payables.ReviewRunAsync(run.Id, approve: true, reason: string.Empty,
                actor: _application.Session.UserId),
            "accounting.payable.pay",
            "Run approved - it may now be posted");
    }

    public Task<bool> ReturnRunAsync()
    {
        if (_run is not { } run)
        {
            _notice.Text = "Select the run to return.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _payables.ReviewRunAsync(run.Id, approve: false, reason: _runReason.Text ?? string.Empty,
                actor: _application.Session.UserId),
            "accounting.payable.pay",
            "Run returned to the proposer");
    }

    public Task<bool> PostRunAsync()
    {
        if (_run is not { } run)
        {
            _notice.Text = "Select the run to post.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _payables.PostRunAsync(run.Id, Context, _application.Session.UserId),
            "accounting.payable.pay", "Run posted - one journal written");
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgPayablesWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _notice.Text = outcome.Message ?? "The payables command was refused.";
            ShowState(OgViewState.Failed, "The payables command was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(outcome.Value);

        _application.AuditLog.Add(
            auditAction,
            "accounting",
            outcome.Value.SelectedInvoice?.Number ?? outcome.Value.SelectedOrder?.Number ??
            outcome.Value.SelectedRun?.Number ?? "(payables)",
            "ok",
            _application.Session.UserId);

        _notice.Text = done + ". " + outcome.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    private void Apply(OgPayablesWorkspace workspace)
    {
        _workspace = workspace;

        _orderCurrency.ItemsSource = workspace.Currencies.ToList();
        _orderCurrency.SelectedIndex = _orderCurrency.SelectedIndex < 0 ? 0 : _orderCurrency.SelectedIndex;

        _invoiceCurrency.ItemsSource = workspace.Currencies.ToList();
        _invoiceCurrency.SelectedIndex = _invoiceCurrency.SelectedIndex < 0 ? 0 : _invoiceCurrency.SelectedIndex;

        _orders.ItemsSource = workspace.Orders.ToList();
        _invoices.ItemsSource = workspace.Invoices.ToList();
        _runGrid.ItemsSource = workspace.Runs.ToList();

        _header.Text = "Accounting · payables";
        _policy.Text = "Matching policy: " + workspace.PolicyText;
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The payables ledger is unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText + " · " + workspace.CommittedText;

        _order = workspace.SelectedOrder;
        _invoice = workspace.SelectedInvoice;
        _run = workspace.SelectedRun;

        ShowOrder();
        ShowInvoice();
        ShowRun();
        RefreshEnabled();
    }

    private void ShowOrder()
    {
        if (_order is not { } order)
        {
            _summary.Text = "no order is selected";
            _approval.Text = string.Empty;
            _orderLines.Text = string.Empty;
            _orderLineGrid.ItemsSource = null;
            return;
        }

        _summary.Text = order.Describe();
        _approval.Text = "Approval: " + order.ApprovalText +
                         (string.IsNullOrWhiteSpace(order.AfeId) ? string.Empty : " · commits against AFE " + order.AfeId);
        _orderLines.Text = order.LinesText;
        _orderLineGrid.ItemsSource = order.Lines.ToList();
    }

    private void ShowInvoice()
    {
        if (_invoice is not { } invoice)
        {
            _invoiceMoney.Text = "no invoice is selected";
            _match.Text = string.Empty;
            _exceptions.Text = string.Empty;
            _invoiceLineGrid.ItemsSource = null;
            return;
        }

        _invoiceMoney.Text = invoice.Describe() + " · " + invoice.TotalsText + " · " + invoice.BalanceText;
        _match.Text = "Match: " + invoice.MatchText;
        _exceptions.Text = invoice.ExceptionsText;
        _invoiceLineGrid.ItemsSource = invoice.Lines.ToList();
    }

    private void ShowRun()
    {
        if (_run is not { } run)
        {
            _runState.Text = "no run is selected";
            _detail.Text = string.Empty;
            return;
        }

        _runState.Text = run.Describe() + " · " + run.JournalText + " · pays " + run.InvoicesText;
        _detail.Text = run.ReturnReason is { Length: > 0 }
            ? "returned: " + run.ReturnReason
            : string.IsNullOrWhiteSpace(run.ApprovedBy)
                ? "not approved yet"
                : "approved by " + run.ApprovedBy + " on " +
                  (run.ApprovedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date");
    }

    private void SelectOrder()
    {
        if (_orders.SelectedItem is not OgPurchaseOrder selected)
        {
            return;
        }

        _order = selected;
        ShowOrder();
        RefreshEnabled();
    }

    private void SelectInvoice()
    {
        if (_invoices.SelectedItem is not OgApInvoice selected)
        {
            return;
        }

        _invoice = selected;
        ShowInvoice();
        RefreshEnabled();
    }

    private void SelectRun()
    {
        if (_runGrid.SelectedItem is not OgPaymentRun selected)
        {
            return;
        }

        _run = selected;
        ShowRun();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var orderMissing = PayableRules.OrderMissing(OrderInputs());
        var invoiceMissing = PayableRules.InvoiceMissing(InvoiceInputs());

        var messages = new List<string>();

        if (orderMissing.Count > 0)
        {
            messages.Add("The order needs " + string.Join(", ", orderMissing) + ".");
        }

        if (invoiceMissing.Count > 0)
        {
            messages.Add("The invoice needs " + string.Join(", ", invoiceMissing) + ".");
        }

        _missing.Text = string.Join(" ", messages);

        _raiseButton.IsEnabled = orderMissing.Count == 0;
        _approveOrderButton.IsEnabled = CanApproveOrder;
        _receiveButton.IsEnabled = CanReceive;
        _captureButton.IsEnabled = invoiceMissing.Count == 0;
        _approveExceptionButton.IsEnabled = CanApproveException;
        _proposeRunButton.IsEnabled = Date(_runDate.Text) is not null;
        _approveRunButton.IsEnabled = CanApproveRun;
        _returnRunButton.IsEnabled = CanReturnRun;
        _postRunButton.IsEnabled = CanPostRun;
    }

    private OgOrderInputs OrderInputs() =>
        new(
            Empty(_orderVendor.Text),
            Date(_orderDate.Text),
            Date(_orderDelivery.Text),
            _orderCurrency.SelectedItem as string,
            Empty(_orderAfe.Text),
            Empty(_orderDescription.Text),
            OrderLines());

    private OgSupplierInvoiceInputs InvoiceInputs() =>
        new(
            Empty(_invoiceVendor.Text),
            Empty(_invoiceNumber.Text),
            Date(_invoiceDate.Text),
            Date(_invoiceDue.Text),
            _invoiceCurrency.SelectedItem as string,
            Parse(_invoiceTax.Text),
            Empty(_invoiceOrder.Text),
            Empty(_invoiceAfe.Text),
            null,
            InvoiceLines());

    /// <summary>Reads the order editor: <c>description, quantity, unit, unit price</c> per line.</summary>
    private IReadOnlyList<OgOrderLineInput> OrderLines()
    {
        var lines = new List<OgOrderLineInput>();
        var sequence = 1;

        foreach (var row in Rows(_orderLinesText.Text))
        {
            var parts = row.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 4)
            {
                continue;
            }

            lines.Add(new OgOrderLineInput(
                string.IsNullOrWhiteSpace(parts[0]) ? "line " + sequence.ToString(CultureInfo.InvariantCulture) : parts[0],
                Parse(parts[1]),
                string.IsNullOrWhiteSpace(parts[2]) ? null : parts[2],
                Parse(parts[3])));

            sequence++;
        }

        return lines;
    }

    /// <summary>Reads the invoice editor: <c>description, quantity, unit, unit price, order line id</c>.</summary>
    private IReadOnlyList<OgInvoiceLineInput> InvoiceLines()
    {
        var lines = new List<OgInvoiceLineInput>();
        var sequence = 1;

        foreach (var row in Rows(_invoiceLinesText.Text))
        {
            var parts = row.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 4)
            {
                continue;
            }

            lines.Add(new OgInvoiceLineInput(
                string.IsNullOrWhiteSpace(parts[0]) ? "line " + sequence.ToString(CultureInfo.InvariantCulture) : parts[0],
                Parse(parts[1]),
                string.IsNullOrWhiteSpace(parts[2]) ? null : parts[2],
                Parse(parts[3]),
                parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]) ? parts[4] : null));

            sequence++;
        }

        return lines;
    }

    private static IEnumerable<string> Rows(string? text) =>
        (text ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(row => !row.StartsWith('#'));

    private static TextBox LinesBox(string name)
    {
        var box = new TextBox
        {
            AcceptsReturn = true,
            IsTabStop = true,
            MinHeight = 62,
            MaxHeight = 120,
            Margin = new Thickness(0, 0, 0, 6),
            TextWrapping = TextWrapping.NoWrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = name,
        };

        Named(box, name);
        box.Text = "# " + name;

        return box;
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
