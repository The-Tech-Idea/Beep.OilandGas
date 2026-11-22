using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Work;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// WRK-02: one work order - its lines, its cost, its transitions and its gates.
/// </summary>
/// <remarks>
/// Only the transitions the owner's own graph reports are offered (AC-1); the actual cost is the sum of the labour and material
/// lines and is never typed (AC-2); execution start is refused without a valid permit (AC-3); completion needs recorded evidence
/// (AC-4); the submitter cannot approve or complete (AC-5); and every line shows the actor and date it was recorded with (AC-6).
/// </remarks>
public sealed class WorkOrderDetailPageView : OgPageBase
{
    public const string Route = "workorder";

    private readonly OgApplication _application;
    private readonly IOgWorkOrderDetail _detail;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _checklist;
    private readonly TextBlock _permit;
    private readonly TextBlock _evidence;
    private readonly TextBlock _cost;
    private readonly TextBlock _labour;
    private readonly TextBlock _materials;
    private readonly TextBlock _notice;
    private readonly TextBlock _failure;
    private readonly TextBlock _refreshed;
    private readonly TextBox _order;
    private readonly TextBox _note;
    private readonly TextBox _person;
    private readonly TextBox _duty;
    private readonly TextBox _hours;
    private readonly TextBox _rate;
    private readonly TextBox _currency;
    private readonly TextBox _item;
    private readonly TextBox _quantity;
    private readonly ComboBox _target;
    private readonly Button _loadButton;
    private readonly Button _transitionButton;
    private readonly Button _labourButton;
    private readonly Button _reserveButton;

    private OgWorkOrderDetail? _detailValue;

    public WorkOrderDetailPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _detail = application.WorkOrderDetail;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _checklist = Body(string.Empty);
        _checklist.TextWrapping = TextWrapping.Wrap;
        _checklist.Margin = new Thickness(0, 0, 0, 4);

        _permit = Body(string.Empty);
        _permit.TextWrapping = TextWrapping.Wrap;

        _evidence = Body(string.Empty);
        _evidence.TextWrapping = TextWrapping.Wrap;

        _cost = Body(string.Empty);
        _cost.TextWrapping = TextWrapping.Wrap;
        _cost.Margin = new Thickness(0, 4, 0, 4);

        _labour = Body(string.Empty);
        _labour.TextWrapping = TextWrapping.Wrap;
        _labour.Margin = new Thickness(0, 4, 0, 4);

        _materials = Body(string.Empty);
        _materials.TextWrapping = TextWrapping.Wrap;
        _materials.Margin = new Thickness(0, 0, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _order = Field("Work order id", 150);
        _note = Field("Transition note", 200);
        _person = Field("Person", 110);
        _duty = Field("Duty", 110);
        _hours = Field("Quantity", 80);
        _rate = Field("Rate", 90);
        _currency = Field("Currency", 80);
        _item = Field("Inventory item id", 140);
        _quantity = Field("Quantity", 90);

        _target = new ComboBox { MinWidth = 160, Margin = new Thickness(0, 0, 8, 0) };
        Named(_target, "Transition target");

        _loadButton = Button("Load", (_, _) => Load());
        _transitionButton = Button("Apply transition", (_, _) => _ = TransitionAsync());
        _labourButton = Button("Add labour", (_, _) => _ = AddLabourAsync());
        _reserveButton = Button("Reserve material", (_, _) => _ = ReserveAsync());

        var loadBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        loadBar.Children.Add(Caption("Order"));
        loadBar.Children.Add(_order);
        loadBar.Children.Add(_loadButton);

        var transitionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        transitionBar.Children.Add(Caption("Transition"));
        transitionBar.Children.Add(_target);
        transitionBar.Children.Add(Caption("Note"));
        transitionBar.Children.Add(_note);
        transitionBar.Children.Add(_transitionButton);

        var labourBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        labourBar.Children.Add(_person);
        labourBar.Children.Add(_duty);
        labourBar.Children.Add(Caption("Qty"));
        labourBar.Children.Add(_hours);
        labourBar.Children.Add(Caption("Rate"));
        labourBar.Children.Add(_rate);
        labourBar.Children.Add(_currency);
        labourBar.Children.Add(_labourButton);

        var materialBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        materialBar.Children.Add(Caption("Item"));
        materialBar.Children.Add(_item);
        materialBar.Children.Add(Caption("Qty"));
        materialBar.Children.Add(_quantity);
        materialBar.Children.Add(_reserveButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(loadBar);
        panel.Children.Add(Caption("Order"));
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("Gates"));
        panel.Children.Add(_checklist);
        panel.Children.Add(_permit);
        panel.Children.Add(_evidence);
        panel.Children.Add(Caption("Transitions"));
        panel.Children.Add(transitionBar);
        panel.Children.Add(_failure);
        panel.Children.Add(Caption("Actual cost"));
        panel.Children.Add(_cost);
        panel.Children.Add(Caption("Materials"));
        panel.Children.Add(_materials);
        panel.Children.Add(materialBar);
        panel.Children.Add(Caption("Labour"));
        panel.Children.Add(_labour);
        panel.Children.Add(labourBar);
        panel.Children.Add(_notice);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Enter the work order id and load it.");
    }

    public OgWorkOrderDetail? Detail => _detailValue;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string ChecklistText => _checklist.Text;

    public string PermitText => _permit.Text;

    public string EvidenceText => _evidence.Text;

    public string CostText => _cost.Text;

    public string LabourText => _labour.Text;

    public string MaterialText => _materials.Text;

    public string FailureText => _failure.Text;

    public string RefreshText => _refreshed.Text;

    public bool Load()
    {
        _header.Text = "Work · work order";

        var orderId = _order.Text?.Trim();

        if (string.IsNullOrWhiteSpace(orderId))
        {
            _detailValue = null;
            _summary.Text = string.Empty;
            _checklist.Text = string.Empty;
            _permit.Text = string.Empty;
            _evidence.Text = string.Empty;
            _cost.Text = string.Empty;
            _labour.Text = string.Empty;
            _materials.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _target.ItemsSource = null;
            _notice.Text = "Enter the work order id.";
            _failure.Text = string.Empty;
            return false;
        }

        var loaded = _detail.LoadAsync(orderId, DateTime.Today).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _detailValue = null;
            _summary.Text = string.Empty;
            _checklist.Text = string.Empty;
            _permit.Text = string.Empty;
            _evidence.Text = string.Empty;
            _cost.Text = string.Empty;
            _labour.Text = string.Empty;
            _materials.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _target.ItemsSource = null;
            _notice.Text = string.Empty;
            _failure.Text = loaded.Message ?? "The work order could not be read.";
            ShowState(OgViewState.Failed, "The work order is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public Task<bool> TransitionAsync()
    {
        if (_target.SelectedItem is not string target)
        {
            _failure.Text = "There is no available transition to apply.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _detail.TransitionAsync(
                OrderId(), target, _note.Text ?? string.Empty, _application.Session.UserId, DateTime.Today),
            "work.submit",
            "Order moved to " + target);
    }

    public Task<bool> AddLabourAsync()
    {
        var hours = Number(_hours.Text) ?? 0m;
        var to = DateTime.UtcNow;
        var from = to.AddHours(-(double)hours);

        return RunAsync(() => _detail.AddLabourAsync(
                OrderId(),
                _person.Text ?? string.Empty,
                _duty.Text ?? string.Empty,
                from,
                to,
                hours,
                "HR",
                // An empty rate is passed as -1, which the service refuses rather than costing the line at zero.
                Number(_rate.Text) ?? -1m,
                _currency.Text ?? string.Empty,
                _application.Session.UserId,
                DateTime.Today),
            "work.execute",
            "Labour recorded");
    }

    public Task<bool> ReserveAsync() =>
        RunAsync(() => _detail.ReserveMaterialAsync(
                OrderId(),
                _item.Text ?? string.Empty,
                Number(_quantity.Text) ?? 0m,
                "EA",
                string.Empty,
                _application.Session.UserId,
                DateTime.Today),
            "work.execute",
            "Material reserved");

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgWorkOrderDetail>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The work order command was refused.";
            ShowState(OgViewState.Failed, "The work order command was refused.", outcome.Reference);
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add(auditAction, "work", outcome.Value.Id, "ok", _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = done + ".";

        return true;
    }

    private void Apply(OgWorkOrderDetail detail)
    {
        _detailValue = detail;

        _header.Text = "Work · " + detail.Number;
        _summary.Text = detail.Number + " · " + detail.Kind + " · " +
                        (string.IsNullOrWhiteSpace(detail.State) ? "no recorded state" : detail.State.ToLowerInvariant()) +
                        (string.IsNullOrWhiteSpace(detail.AssetId) ? string.Empty : " · " + detail.AssetId) +
                        (string.IsNullOrWhiteSpace(detail.Assignee) ? " · unassigned" : " · " + detail.Assignee) +
                        (detail.DueAt is { } due
                            ? " · due " + due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                            : string.Empty);

        _checklist.Text = detail.ChecklistText;
        _permit.Text = detail.Permit.Sentence;
        _evidence.Text = detail.Evidence.Sentence;
        _cost.Text = detail.CostText;
        _labour.Text = detail.LabourText;
        _materials.Text = detail.MaterialText;

        _refreshed.Text = detail.ReadText + " · " + detail.TransitionText;

        _target.ItemsSource = detail.AvailableTransitions.ToList();
        _target.SelectedIndex = detail.AvailableTransitions.Count > 0 ? 0 : -1;

        _failure.Text = string.Empty;

        ShowState(detail.IsComplete ? OgViewState.Ready : OgViewState.Empty, detail.Number);
    }

    private string OrderId() => _order.Text?.Trim() ?? string.Empty;

    private static decimal? Number(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;

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
        Margin = new Thickness(0, 6, 0, 0),
        FontSize = 11.5,
        FontWeight = FontWeights.SemiBold,
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
}
