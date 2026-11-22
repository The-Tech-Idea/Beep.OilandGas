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
/// DEV-04: how the field is drained - pools, well counts, facility capacity and the investment the plan depends on.
/// </summary>
/// <remarks>
/// A rate is compared with a capacity only when the units agree, and the units that did not agree are named rather
/// than converted (AC-1). A shortfall is its own number (AC-2). A facility is shown as planned, in construction,
/// operating or abandoned, and only an operating one is offered to the operational pages (AC-3). Pools and facility
/// investments both show the plan version they belong to (AC-4). The allocation preview sums recoverable volumes only
/// where the pools share a unit.
/// </remarks>
public sealed class PoolsFacilitiesPageView : OgPageBase
{
    public const string Route = "poolsfacilities";

    private readonly OgApplication _application;
    private readonly IOgPoolsFacilities _plan;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _capacity;
    private readonly TextBlock _allocation;
    private readonly DataGrid _pools;
    private readonly DataGrid _facilities;

    private readonly TextBox _poolName;
    private readonly TextBox _poolId;
    private readonly TextBox _reservoir;
    private readonly TextBox _drive;
    private readonly TextBox _ooip;
    private readonly TextBox _ooipUnit;
    private readonly TextBox _recovery;
    private readonly TextBox _plannedWells;
    private readonly ComboBox _facilityPicker;

    private readonly TextBox _facilityName;
    private readonly TextBox _facilityId;
    private readonly TextBox _facilityKind;
    private readonly TextBox _facilityPhase;
    private readonly TextBox _capacityValue;
    private readonly TextBox _capacityUnit;
    private readonly TextBox _investment;
    private readonly TextBox _currency;
    private readonly TextBox _startDate;
    private readonly TextBox _completionDate;

    public PoolsFacilitiesPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _plan = application.PoolsFacilities;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _capacity = Body(string.Empty);
        _capacity.TextWrapping = TextWrapping.Wrap;
        _capacity.Margin = new Thickness(0, 4, 0, 4);

        _allocation = Body(string.Empty);
        _allocation.TextWrapping = TextWrapping.Wrap;
        _allocation.Margin = new Thickness(0, 0, 0, 8);

        _pools = new DataGrid
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
                Text("Pool", "Name", 150),
                Text("Reservoir", "Reservoir", 130),
                Text("Drive", "DriveMechanism", 120),
                Text("OOIP", "OoipText", 160),
                Text("RF", "RecoveryText", 70),
                Text("Recoverable", "RecoverableText", 160),
                Text("Wells", "PlannedWells", 70),
                Text("Facility", "FacilityId", 110),
                Text("Plan version", "PlanVersion", 90),
            },
        };

        Named(_pools, "Pools");
        _pools.SelectionChanged += (_, _) => ShowSelectedPool();

        _facilities = new DataGrid
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
                Text("Facility", "Name", 150),
                Text("Type", "Kind", 100),
                Text("Phase", "Phase", 100),
                Text("State", "StateText", 110),
                Text("Capacity", "CapacityText", 140),
                Text("Investment", "InvestmentText", 120),
                Text("In service", "InServiceText", 100),
                Text("Plan version", "PlanVersion", 90),
            },
        };

        Named(_facilities, "Facilities");
        _facilities.SelectionChanged += (_, _) => ShowSelectedFacility();

        _poolName = Field("Pool name", 130);
        _poolId = Field("PPDM pool id", 100);
        _reservoir = Field("Reservoir", 120);
        _drive = Field("Drive mechanism", 120);
        _ooip = Field("OOIP", 90);
        _ooipUnit = Field("OOIP unit", 80);
        _recovery = Field("Recovery factor 0-1", 90);
        _plannedWells = Field("Planned wells", 80);

        _facilityPicker = new ComboBox { MinWidth = 160, Margin = new Thickness(0, 0, 8, 0) };
        Named(_facilityPicker, "Facility the pool produces to");

        _facilityName = Field("Facility name", 130);
        _facilityId = Field("PPDM facility id", 110);
        _facilityKind = Field("Facility type", 100);
        _facilityPhase = Field("Phase", 90);
        _capacityValue = Field("Capacity", 90);
        _capacityUnit = Field("Capacity unit", 80);
        _investment = Field("Investment", 90);
        _currency = Field("Currency", 70);
        _startDate = Field("Planned start yyyy-MM-dd", 120);
        _completionDate = Field("In-service yyyy-MM-dd", 120);

        var poolBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        poolBar.Children.Add(Caption("Pool"));
        poolBar.Children.Add(_poolName);
        poolBar.Children.Add(_poolId);
        poolBar.Children.Add(_reservoir);
        poolBar.Children.Add(_drive);
        poolBar.Children.Add(Caption("OOIP"));
        poolBar.Children.Add(_ooip);
        poolBar.Children.Add(_ooipUnit);
        poolBar.Children.Add(Caption("RF"));
        poolBar.Children.Add(_recovery);
        poolBar.Children.Add(Caption("Wells"));
        poolBar.Children.Add(_plannedWells);
        poolBar.Children.Add(_facilityPicker);
        poolBar.Children.Add(Button("Save pool", (_, _) => _ = SavePoolAsync()));

        var facilityBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        facilityBar.Children.Add(Caption("Facility"));
        facilityBar.Children.Add(_facilityName);
        facilityBar.Children.Add(_facilityId);
        facilityBar.Children.Add(_facilityKind);
        facilityBar.Children.Add(_facilityPhase);
        facilityBar.Children.Add(Caption("Capacity"));
        facilityBar.Children.Add(_capacityValue);
        facilityBar.Children.Add(_capacityUnit);
        facilityBar.Children.Add(Caption("Investment"));
        facilityBar.Children.Add(_investment);
        facilityBar.Children.Add(_currency);
        facilityBar.Children.Add(_startDate);
        facilityBar.Children.Add(_completionDate);
        facilityBar.Children.Add(Button("Save facility", (_, _) => _ = SaveFacilityAsync()));

        var actions = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actions.Children.Add(Button("Refresh", (_, _) => Load()));

        var panel = new StackPanel();
        panel.Children.Add(actions);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("Pools"));
        panel.Children.Add(_pools);
        panel.Children.Add(poolBar);
        panel.Children.Add(Caption("Facilities"));
        panel.Children.Add(_facilities);
        panel.Children.Add(facilityBar);
        panel.Children.Add(Caption("Capacity check"));
        panel.Children.Add(_capacity);
        panel.Children.Add(Caption("Allocation preview"));
        panel.Children.Add(_allocation);

        ShowContent(panel);

        Load();
    }

    public OgPoolsFacilitiesSnapshot? Snapshot { get; private set; }

    public OgPoolPlan? SelectedPool { get; private set; }

    public OgFacilityPlan? SelectedFacility { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string CapacityText => _capacity.Text;

    public string AllocationText => _allocation.Text;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _plan
            .LoadAsync(session.FieldId, session.FieldName, null, DateTime.Today)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Snapshot = null;
            SelectedPool = null;
            SelectedFacility = null;
            _pools.ItemsSource = null;
            _facilities.ItemsSource = null;
            _facilityPicker.ItemsSource = null;
            _header.Text = "Pools and facilities";
            _capacity.Text = string.Empty;
            _allocation.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The pool and facility plan could not be read.";
            ShowState(OgViewState.Failed, "The pool and facility plan could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Snapshot = loaded.Value;
        SelectedPool = null;
        SelectedFacility = null;

        _pools.ItemsSource = Snapshot.Pools.ToList();
        _facilities.ItemsSource = Snapshot.Facilities.ToList();
        _facilityPicker.ItemsSource = Snapshot.Facilities
            .Where(facility => !string.IsNullOrWhiteSpace(facility.FacilityId))
            .ToList();
        _facilityPicker.SelectedIndex = Snapshot.Facilities.Count > 0 ? 0 : -1;

        _header.Text = Snapshot.HasPlan
            ? $"Pools and facilities · {Snapshot.PlanName ?? Snapshot.PlanId} · plan v" +
              $"{Snapshot.PlanVersion.ToString(CultureInfo.InvariantCulture)} · {Snapshot.Summary}"
            : "Pools and facilities";

        _notice.Text = !Snapshot.HasPlan
            ? "No development plan is recorded for this field; create one in the plan wizard first."
            : Snapshot.IsEmpty
                ? "No pool or facility is recorded for this plan."
                : Snapshot.Capacity.HasShortfall
                    ? "At least one planned rate exceeds the plan's capacity; the shortfall is shown as its own number."
                    : Snapshot.PoolsWithoutUnit > 0
                        ? $"{Snapshot.PoolsWithoutUnit.ToString(CultureInfo.InvariantCulture)} pool(s) have an OOIP " +
                          "without a unit, so their recoverable volume is not computed."
                        : "Capacity and allocation are read from the plan's own records.";

        _capacity.Text = Snapshot.Capacity.Text;
        _allocation.Text = Snapshot.Allocation.Text;

        ShowState(Snapshot.HasPlan && !Snapshot.IsEmpty ? OgViewState.Ready : OgViewState.Empty, _notice.Text);
        return true;
    }

    /// <summary>AC-4: the pool row carries the plan and the plan version it belongs to.</summary>
    public async Task<bool> SavePoolAsync()
    {
        if (Snapshot?.PlanId is not { } planId)
        {
            _notice.Text = "No development plan is selected.";
            return false;
        }

        var facility = _facilityPicker.SelectedItem as OgFacilityPlan;

        var saved = await _plan
            .SavePoolAsync(
                planId,
                new OgPoolValues(
                    _poolName.Text,
                    _poolId.Text,
                    _reservoir.Text,
                    _drive.Text,
                    decimal.TryParse(_ooip.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ooip)
                        ? ooip
                        : null,
                    _ooipUnit.Text,
                    decimal.TryParse(_recovery.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var factor)
                        ? factor
                        : 0m,
                    int.TryParse(_plannedWells.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var wells)
                        ? wells
                        : 0,
                    facility?.FacilityId),
                _application.Session.UserId,
                DateTime.Today)
            .ConfigureAwait(true);

        return AfterWrite(saved, "development.pools.save", _poolName.Text.Trim());
    }

    /// <summary>AC-3: the facility row carries the plan version, and its state is derived, not entered.</summary>
    public async Task<bool> SaveFacilityAsync()
    {
        if (Snapshot?.PlanId is not { } planId)
        {
            _notice.Text = "No development plan is selected.";
            return false;
        }

        var saved = await _plan
            .SaveFacilityAsync(
                planId,
                new OgFacilityValues(
                    _facilityId.Text,
                    _facilityName.Text,
                    _facilityKind.Text,
                    _facilityPhase.Text,
                    decimal.TryParse(_capacityValue.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var capacity)
                        ? capacity
                        : null,
                    _capacityUnit.Text,
                    decimal.TryParse(_investment.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var investment)
                        ? investment
                        : null,
                    _currency.Text,
                    Date(_startDate.Text),
                    Date(_completionDate.Text)),
                _application.Session.UserId,
                DateTime.Today)
            .ConfigureAwait(true);

        return AfterWrite(saved, "development.facilities.save", _facilityName.Text.Trim());
    }

    private bool AfterWrite(OgResult<OgPoolsFacilitiesSnapshot> result, string verb, string subject)
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

    private void ShowSelectedPool()
    {
        if (_pools.SelectedItem is not OgPoolPlan pool)
        {
            return;
        }

        SelectedPool = pool;

        _poolName.Text = pool.Name;
        _poolId.Text = pool.PoolId ?? string.Empty;
        _reservoir.Text = pool.Reservoir ?? string.Empty;
        _drive.Text = pool.DriveMechanism ?? string.Empty;
        _ooip.Text = pool.Ooip?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _ooipUnit.Text = pool.OoipUnit ?? string.Empty;
        _recovery.Text = pool.RecoveryFactor.ToString(CultureInfo.InvariantCulture);
        _plannedWells.Text = pool.PlannedWells.ToString(CultureInfo.InvariantCulture);

        _facilityPicker.SelectedItem = Snapshot?.Facilities.FirstOrDefault(facility =>
            string.Equals(facility.FacilityId, pool.FacilityId, StringComparison.OrdinalIgnoreCase));

        _notice.Text = pool.Describe();
    }

    private void ShowSelectedFacility()
    {
        if (_facilities.SelectedItem is not OgFacilityPlan facility)
        {
            return;
        }

        SelectedFacility = facility;

        _facilityName.Text = facility.Name;
        _facilityId.Text = facility.FacilityId ?? string.Empty;
        _facilityKind.Text = facility.Kind ?? string.Empty;
        _facilityPhase.Text = facility.Phase ?? string.Empty;
        _capacityValue.Text = facility.Capacity?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _capacityUnit.Text = facility.CapacityUnit ?? string.Empty;
        _investment.Text = facility.Investment?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _currency.Text = facility.Currency ?? string.Empty;
        _startDate.Text = facility.StartDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        _completionDate.Text = facility.CompletionDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

        _notice.Text = facility.Describe() +
                       (facility.ExcludedFromOperationalPages
                           ? " · excluded from the operational pages until it operates"
                           : string.Empty);
    }

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
