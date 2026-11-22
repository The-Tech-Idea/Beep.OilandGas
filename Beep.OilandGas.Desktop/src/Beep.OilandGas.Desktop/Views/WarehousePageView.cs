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
/// ACC-06: the warehouse - items from receipt to issue to return, with what each document holds and the ledger entry every
/// value change produces.
/// </summary>
/// <remarks>
/// Available is on hand less reserved, exactly (AC-1); an issue above availability is refused naming the shortfall (AC-2); a
/// reservation names the document holding it (AC-3); a return has to state its restock decision (AC-4); a value change posts
/// a linked journal (AC-5); and stock never goes below zero (AC-6).
/// </remarks>
public sealed class WarehousePageView : OgPageBase
{
    public const string Route = "warehouse";

    private readonly OgApplication _application;
    private readonly IOgWarehouse _warehouse;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _owner;
    private readonly TextBlock _reference;
    private readonly TextBlock _summary;
    private readonly TextBlock _availability;
    private readonly TextBlock _holds;
    private readonly TextBlock _link;
    private readonly TextBlock _detail;
    private readonly TextBox _receiveQuantity;
    private readonly TextBox _receiveCost;
    private readonly TextBox _receiveReference;
    private readonly TextBox _receiveLocation;
    private readonly TextBox _reserveQuantity;
    private readonly TextBox _holder;
    private readonly TextBox _issueQuantity;
    private readonly TextBox _issueReference;
    private readonly TextBox _releaseReason;
    private readonly TextBox _returnQuantity;
    private readonly TextBox _returnReference;
    private readonly TextBox _countedQuantity;
    private readonly TextBox _adjustReason;
    private readonly TextBox _marketCost;
    private readonly TextBox _revalueReason;
    private readonly TextBox _assetId;
    private readonly ComboBox _condition;
    private readonly ComboBox _restockDecision;
    private readonly ComboBox _holdingKind;
    private readonly ComboBox _assetType;
    private readonly DataGrid _items;
    private readonly DataGrid _movementGrid;
    private readonly DataGrid _reservationGrid;
    private readonly Button _receiveButton;
    private readonly Button _reserveButton;
    private readonly Button _issueButton;
    private readonly Button _releaseButton;
    private readonly Button _returnButton;
    private readonly Button _adjustButton;
    private readonly Button _revalueButton;
    private readonly Button _linkButton;

    private OgWarehouseWorkspace? _workspace;
    private OgStockItem? _item;
    private OgReservation? _reservation;

    public WarehousePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _warehouse = application.Warehouse;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 4, 0, 4);

        _owner = Body(string.Empty);
        _owner.TextWrapping = TextWrapping.Wrap;
        _owner.Margin = new Thickness(0, 4, 0, 4);

        _reference = Body(string.Empty);
        _reference.TextWrapping = TextWrapping.Wrap;
        _reference.Margin = new Thickness(0, 0, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _availability = Body(string.Empty);
        _availability.TextWrapping = TextWrapping.Wrap;
        _availability.Margin = new Thickness(0, 0, 0, 4);

        _holds = Body(string.Empty);
        _holds.TextWrapping = TextWrapping.Wrap;
        _holds.Margin = new Thickness(0, 0, 0, 4);

        _link = Body(string.Empty);
        _link.TextWrapping = TextWrapping.Wrap;
        _link.Margin = new Thickness(0, 0, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _receiveQuantity = Field("Receive quantity", 120);
        _receiveCost = Field("Unit cost", 100);
        _receiveReference = Field("Receive reference", 140);
        _receiveLocation = Field("Bin / location", 120);
        _reserveQuantity = Field("Reserve quantity", 120);
        _holder = Field("Holding document", 150);
        _issueQuantity = Field("Issue quantity", 110);
        _issueReference = Field("Issue reference", 140);
        _releaseReason = Field("Release reason", 200);
        _returnQuantity = Field("Return quantity", 120);
        _returnReference = Field("Return reference", 150);
        _countedQuantity = Field("Counted quantity", 130);
        _adjustReason = Field("Adjust reason", 190);
        _marketCost = Field("Market unit cost", 130);
        _revalueReason = Field("Revalue reason", 190);
        _assetId = Field("Asset / equipment id", 150);

        _condition = Picker("Condition", 140);
        _restockDecision = Picker("Restock decision", 150);
        _holdingKind = Picker("Holding kind", 130);
        _assetType = Picker("Link type", 110);

        _items = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 120,
            MaxHeight = 200,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Code", "Code", 110),
                Text("Description", "Description", 190),
                Text("Unit", "Unit", 60),
                Text("On hand", "OnHand", 90),
                Text("Reserved", "Reserved", 90),
                Text("Available", "Available", 90),
                Text("Value", "Value", 110),
                Text("Currency", "Currency", 80),
                Text("Location", "Location", 100),
            },
        };

        Named(_items, "Stock items");
        _items.SelectionChanged += (_, _) => SelectItem();

        _movementGrid = new DataGrid
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
                Text("Kind", "Kind", 100),
                Text("Item", "ItemCode", 100),
                Text("Quantity", "Quantity", 100),
                Text("Value change", "ValueChange", 110),
                Text("Date", "Date", 100),
                Text("Reference", "Reference", 130),
                Text("Condition", "Condition", 100),
                Text("Decision", "RestockDecision", 100),
                Text("Journal", "JournalId", 110),
            },
        };

        Named(_movementGrid, "Stock movements");

        _reservationGrid = new DataGrid
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
                Text("Item", "ItemCode", 100),
                Text("Held by", "Holder", 150),
                Text("Kind", "HoldingKind", 110),
                Text("Quantity", "Quantity", 100),
                Text("State", "State", 90),
                Text("Reserved", "ReservedAt", 130),
            },
        };

        Named(_reservationGrid, "Reservations");
        _reservationGrid.SelectionChanged += (_, _) => SelectReservation();

        _receiveButton = Button("Receive", (_, _) => _ = ReceiveAsync());
        _reserveButton = Button("Reserve", (_, _) => _ = ReserveAsync());
        _issueButton = Button("Issue", (_, _) => _ = IssueAsync());
        _releaseButton = Button("Release hold", (_, _) => _ = ReleaseAsync());
        _returnButton = Button("Return to store", (_, _) => _ = ReturnAsync());
        _adjustButton = Button("Adjust count", (_, _) => _ = AdjustAsync());
        _revalueButton = Button("Revalue", (_, _) => _ = RevalueAsync());
        _linkButton = Button("Link", (_, _) => _ = LinkAsync());

        var receiveBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        receiveBar.Children.Add(Caption("Qty"));
        receiveBar.Children.Add(_receiveQuantity);
        receiveBar.Children.Add(Caption("Cost"));
        receiveBar.Children.Add(_receiveCost);
        receiveBar.Children.Add(Caption("Ref"));
        receiveBar.Children.Add(_receiveReference);
        receiveBar.Children.Add(Caption("Condition"));
        receiveBar.Children.Add(_condition);
        receiveBar.Children.Add(Caption("Decision"));
        receiveBar.Children.Add(_restockDecision);
        receiveBar.Children.Add(Caption("Bin"));
        receiveBar.Children.Add(_receiveLocation);
        receiveBar.Children.Add(_receiveButton);

        var reserveBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        reserveBar.Children.Add(Caption("Qty"));
        reserveBar.Children.Add(_reserveQuantity);
        reserveBar.Children.Add(Caption("Held by"));
        reserveBar.Children.Add(_holder);
        reserveBar.Children.Add(Caption("Kind"));
        reserveBar.Children.Add(_holdingKind);
        reserveBar.Children.Add(_reserveButton);

        var issueBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        issueBar.Children.Add(Caption("Qty"));
        issueBar.Children.Add(_issueQuantity);
        issueBar.Children.Add(Caption("Ref"));
        issueBar.Children.Add(_issueReference);
        issueBar.Children.Add(_issueButton);
        issueBar.Children.Add(Caption("Release"));
        issueBar.Children.Add(_releaseReason);
        issueBar.Children.Add(_releaseButton);

        var returnBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        returnBar.Children.Add(Caption("Qty"));
        returnBar.Children.Add(_returnQuantity);
        returnBar.Children.Add(Caption("Condition and decision use the fields above · Ref"));
        returnBar.Children.Add(_returnReference);
        returnBar.Children.Add(_returnButton);

        var countBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        countBar.Children.Add(Caption("Counted"));
        countBar.Children.Add(_countedQuantity);
        countBar.Children.Add(Caption("Reason"));
        countBar.Children.Add(_adjustReason);
        countBar.Children.Add(_adjustButton);

        var revalueBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        revalueBar.Children.Add(Caption("Market cost"));
        revalueBar.Children.Add(_marketCost);
        revalueBar.Children.Add(Caption("Reason"));
        revalueBar.Children.Add(_revalueReason);
        revalueBar.Children.Add(_revalueButton);

        var linkBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        linkBar.Children.Add(Caption("Type"));
        linkBar.Children.Add(_assetType);
        linkBar.Children.Add(Caption("Id"));
        linkBar.Children.Add(_assetId);
        linkBar.Children.Add(_linkButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_owner);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Items"));
        panel.Children.Add(_items);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Receive"));
        panel.Children.Add(receiveBar);
        panel.Children.Add(Caption("Reserve for a document"));
        panel.Children.Add(reserveBar);
        panel.Children.Add(Caption("Issue and release"));
        panel.Children.Add(issueBar);
        panel.Children.Add(Caption("Return from a work order"));
        panel.Children.Add(returnBar);
        panel.Children.Add(Caption("Count adjustment"));
        panel.Children.Add(countBar);
        panel.Children.Add(Caption("Valuation"));
        panel.Children.Add(revalueBar);
        panel.Children.Add(Caption("Link to an asset"));
        panel.Children.Add(linkBar);
        panel.Children.Add(Caption("Selected item"));
        panel.Children.Add(_summary);
        panel.Children.Add(_availability);
        panel.Children.Add(_holds);
        panel.Children.Add(_link);
        panel.Children.Add(Caption("Reservations"));
        panel.Children.Add(_reservationGrid);
        panel.Children.Add(Caption("Movements"));
        panel.Children.Add(_movementGrid);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _receiveQuantity, _receiveCost, _receiveReference, _receiveLocation, _reserveQuantity, _holder,
                     _issueQuantity, _issueReference, _releaseReason, _returnQuantity, _returnReference,
                     _countedQuantity, _adjustReason, _marketCost, _revalueReason, _assetId,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _condition.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgWarehouseWorkspace? Workspace => _workspace;

    public OgStockItem? Item => _item;

    public OgReservation? Reservation => _reservation;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string OwnerText => _owner.Text;

    public string ReferenceText => _reference.Text;

    public string SummaryText => _summary.Text;

    public string AvailabilityText => _availability.Text;

    public string HoldsText => _holds.Text;

    public string LinkText => _link.Text;

    public string DetailText => _detail.Text;

    public bool CanReceive => _item is not null && Parse(_receiveQuantity.Text) is > 0m;

    public bool CanReserve => _item is not null &&
                              Parse(_reserveQuantity.Text) is > 0m &&
                              !string.IsNullOrWhiteSpace(_holder.Text);

    public bool CanIssue => _item is not null && Parse(_issueQuantity.Text) is > 0m;

    public bool CanRelease => _reservation is { HoldsStock: true } && !string.IsNullOrWhiteSpace(_releaseReason.Text);

    public bool CanReturn => _item is not null &&
                             Parse(_returnQuantity.Text) is > 0m &&
                             _restockDecision.SelectedItem is not null &&
                             _condition.SelectedItem is not null;

    public bool CanAdjust => _item is not null &&
                             Parse(_countedQuantity.Text) is not null &&
                             !string.IsNullOrWhiteSpace(_adjustReason.Text);

    public bool CanRevalue => _item is not null &&
                              Parse(_marketCost.Text) is not null &&
                              !string.IsNullOrWhiteSpace(_revalueReason.Text);

    public bool CanLink => _item is not null &&
                           _assetType.SelectedItem is not null &&
                           !string.IsNullOrWhiteSpace(_assetId.Text);

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    private OgWarehouseContext Context => OgWarehouseContext.FromSession(_application.Session);

    public bool Load()
    {
        var loaded = _warehouse.LoadAsync(Context).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _items.ItemsSource = null;
            _header.Text = "Warehouse";
            _notice.Text = loaded.Message ?? "The warehouse could not be read.";
            ShowState(OgViewState.Failed, "The warehouse could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public Task<bool> ReceiveAsync()
    {
        if (_item is not { } item)
        {
            _notice.Text = "Select the item being received.";
            return Task.FromResult(false);
        }

        var quantity = Parse(_receiveQuantity.Text);

        if (quantity is null)
        {
            _notice.Text = "A receipt needs its quantity.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _warehouse.ReceiveAsync(
                item.Id,
                quantity.Value,
                Parse(_receiveCost.Text),
                Empty(_receiveReference.Text),
                _condition.SelectedItem as string,
                _restockDecision.SelectedItem as string,
                Empty(_receiveLocation.Text),
                Context,
                _application.Session.UserId),
            "accounting.stock.write",
            "Goods received");
    }

    public Task<bool> ReserveAsync()
    {
        if (_item is not { } item)
        {
            _notice.Text = "Select the item being reserved.";
            return Task.FromResult(false);
        }

        var quantity = Parse(_reserveQuantity.Text);

        if (quantity is null)
        {
            _notice.Text = "A reservation needs its quantity.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _warehouse.ReserveAsync(
                item.Id,
                quantity.Value,
                _holder.Text ?? string.Empty,
                _holdingKind.SelectedItem as string,
                _application.Session.UserId),
            "accounting.stock.write",
            "Stock reserved");
    }

    public Task<bool> IssueAsync()
    {
        if (_item is not { } item)
        {
            _notice.Text = "Select the item being issued.";
            return Task.FromResult(false);
        }

        var quantity = Parse(_issueQuantity.Text);

        if (quantity is null)
        {
            _notice.Text = "An issue needs its quantity.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _warehouse.IssueAsync(
                item.Id,
                quantity.Value,
                Empty(_issueReference.Text),
                _reservation?.HoldsStock == true ? _reservation.Id : null,
                Context,
                _application.Session.UserId),
            "accounting.stock.write",
            "Stock issued");
    }

    public Task<bool> ReleaseAsync()
    {
        if (_reservation is not { } reservation)
        {
            _notice.Text = "Select the reservation to release.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _warehouse.ReleaseAsync(
                reservation.Id,
                _releaseReason.Text ?? string.Empty,
                _application.Session.UserId),
            "accounting.stock.write",
            "Reservation released");
    }

    public Task<bool> ReturnAsync()
    {
        if (_item is not { } item)
        {
            _notice.Text = "Select the item being returned.";
            return Task.FromResult(false);
        }

        var quantity = Parse(_returnQuantity.Text);

        if (quantity is null)
        {
            _notice.Text = "A return needs its quantity.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _warehouse.ReturnAsync(
                item.Id,
                quantity.Value,
                _condition.SelectedItem as string,
                _restockDecision.SelectedItem as string,
                Empty(_returnReference.Text),
                Context,
                _application.Session.UserId),
            "accounting.stock.write",
            "Return recorded");
    }

    public Task<bool> AdjustAsync()
    {
        if (_item is not { } item)
        {
            _notice.Text = "Select the item being counted.";
            return Task.FromResult(false);
        }

        var counted = Parse(_countedQuantity.Text);

        if (counted is null)
        {
            _notice.Text = "An adjustment needs the counted quantity.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _warehouse.AdjustAsync(
                item.Id,
                counted.Value,
                _adjustReason.Text ?? string.Empty,
                Context,
                _application.Session.UserId),
            "accounting.stock.adjust",
            "Count adjusted");
    }

    public Task<bool> RevalueAsync()
    {
        if (_item is not { } item)
        {
            _notice.Text = "Select the item being revalued.";
            return Task.FromResult(false);
        }

        var market = Parse(_marketCost.Text);

        if (market is null)
        {
            _notice.Text = "A revaluation needs the market unit cost.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _warehouse.RevalueAsync(
                item.Id,
                market.Value,
                _revalueReason.Text ?? string.Empty,
                Context,
                _application.Session.UserId),
            "accounting.stock.adjust",
            "Stock revalued");
    }

    public Task<bool> LinkAsync()
    {
        if (_item is not { } item)
        {
            _notice.Text = "Select the item being linked.";
            return Task.FromResult(false);
        }

        return RunAsync(
            () => _warehouse.LinkAsync(
                item.Id,
                _assetType.SelectedItem as string ?? string.Empty,
                _assetId.Text ?? string.Empty,
                _application.Session.UserId),
            "accounting.stock.write",
            "Item linked");
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgWarehouseWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _notice.Text = outcome.Message ?? "The warehouse command was refused.";
            ShowState(OgViewState.Failed, "The warehouse command was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(outcome.Value);

        _application.AuditLog.Add(
            auditAction,
            "accounting",
            outcome.Value.Selected?.Code ?? "(stock)",
            "ok",
            _application.Session.UserId);

        _notice.Text = done + ". " + outcome.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    private void Apply(OgWarehouseWorkspace workspace)
    {
        _workspace = workspace;

        _condition.ItemsSource = workspace.Conditions.ToList();
        _condition.SelectedIndex = _condition.SelectedIndex < 0 ? 0 : _condition.SelectedIndex;

        _restockDecision.ItemsSource = workspace.RestockDecisions.ToList();
        _restockDecision.SelectedIndex = _restockDecision.SelectedIndex < 0 ? 0 : _restockDecision.SelectedIndex;

        _holdingKind.ItemsSource = WarehouseRules.HoldingKinds.ToList();
        _holdingKind.SelectedIndex = _holdingKind.SelectedIndex < 0 ? 0 : _holdingKind.SelectedIndex;

        _assetType.ItemsSource = WarehouseRules.AssetTypes.ToList();
        _assetType.SelectedIndex = _assetType.SelectedIndex < 0 ? 0 : _assetType.SelectedIndex;

        _items.ItemsSource = workspace.Items.ToList();
        _movementGrid.ItemsSource = workspace.Movements.ToList();
        _reservationGrid.ItemsSource = workspace.Reservations.ToList();

        _header.Text = "Accounting · warehouse";
        _owner.Text = "Stock quantity owner: " + workspace.OwnerText;
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The warehouse is unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText + " · " + workspace.TotalValueText;

        _item = workspace.Selected;

        // Keep the reservation selection on the selected item's holds when there was none.
        _reservation = workspace.Selected is { } selected
            ? selected.Holds.FirstOrDefault(hold => hold.HoldsStock) ?? selected.Holds.FirstOrDefault()
            : null;

        ShowItem();
        RefreshEnabled();
    }

    private void ShowItem()
    {
        if (_item is not { } item)
        {
            _summary.Text = "no item is selected";
            _availability.Text = string.Empty;
            _holds.Text = string.Empty;
            _link.Text = string.Empty;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text = item.Describe();
        _availability.Text =
            $"on hand {Number(item.OnHand)} {item.Unit} · reserved {Number(item.Reserved)} · " +
            $"available {item.AvailabilityText} · value {item.ValueText}";
        _holds.Text = item.HeldText;
        _link.Text = item.LinkText;
        _detail.Text = string.Join(Environment.NewLine, item.Holds.Select(hold => hold.Describe()));
    }

    private void SelectItem()
    {
        if (_items.SelectedItem is not OgStockItem selected)
        {
            return;
        }

        _item = selected;
        _reservation = selected.Holds.FirstOrDefault(hold => hold.HoldsStock) ?? selected.Holds.FirstOrDefault();

        _reservationGrid.ItemsSource = selected.Holds.ToList();

        ShowItem();
        RefreshEnabled();
    }

    private void SelectReservation()
    {
        if (_reservationGrid.SelectedItem is not OgReservation selected)
        {
            return;
        }

        _reservation = selected;
        _detail.Text = selected.Describe();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var missing = new List<string>();

        if (_item is null)
        {
            missing.Add("select a stock item first");
        }

        _missing.Text = string.Join(" ", missing);
        _receiveButton.IsEnabled = CanReceive;
        _reserveButton.IsEnabled = CanReserve;
        _issueButton.IsEnabled = CanIssue;
        _releaseButton.IsEnabled = CanRelease;
        _returnButton.IsEnabled = CanReturn;
        _adjustButton.IsEnabled = CanAdjust;
        _revalueButton.IsEnabled = CanRevalue;
        _linkButton.IsEnabled = CanLink;
    }

    private static string Number(decimal value) =>
        value.ToString("#,0.####", CultureInfo.InvariantCulture);

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
        TextWrapping = TextWrapping.Wrap,
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
