using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One row of the register grid.</summary>
public sealed record OgLeaseRow(
    string Number,
    string Kind,
    string Name,
    string Acreage,
    string Region,
    string Operator,
    string Effective,
    string Expiry,
    string Royalty,
    string NetRevenue,
    string Status);

/// <summary>
/// LSE-01: one register over every lease type.
/// </summary>
/// <remarks>
/// The four lease tables are merged by the service, so a lease recorded only in a typed table is on the register
/// (AC-1). The acreage always shows a unit or says it is missing (AC-2). The state is derived from the dates and
/// labelled as such (AC-3). Linking a lease to a field records the actor and date on the row (AC-4). The export is
/// built from the rows the grid is showing (AC-5).
/// </remarks>
public sealed class LeaseRegisterPageView : OgPageBase
{
    public const string Route = "leases";

    private readonly OgApplication _application;
    private readonly IOgLeaseRegister _register;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _detail;
    private readonly ComboBox _kind;
    private readonly ComboBox _region;
    private readonly ComboBox _operator;
    private readonly ComboBox _status;
    private readonly TextBox _expiringWithin;
    private readonly TextBox _fieldToLink;
    private readonly DataGrid _grid;

    public LeaseRegisterPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _register = application.LeaseRegister;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;
        _detail = Body(string.Empty);

        _kind = Picker("Lease type filter");
        _region = Picker("Region filter");
        _operator = Picker("Operator filter");

        _status = Picker("Expiry state filter");
        _status.ItemsSource = LeaseStatus.All;

        _expiringWithin = new TextBox
        {
            MinWidth = 80,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = "Expiring within this many days",
        };

        Named(_expiringWithin, "Expiring within days");

        _fieldToLink = new TextBox
        {
            MinWidth = 140,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = "Field id to link the selected lease to",
        };

        Named(_fieldToLink, "Field id to link");

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 200,
            MaxHeight = 320,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Number", nameof(OgLeaseRow.Number), 140),
                Text("Type", nameof(OgLeaseRow.Kind), 110),
                Text("Name", nameof(OgLeaseRow.Name), 160),
                Text("Acreage", nameof(OgLeaseRow.Acreage), 130),
                Text("Region", nameof(OgLeaseRow.Region), 80),
                Text("Operator", nameof(OgLeaseRow.Operator), 110),
                Text("Effective", nameof(OgLeaseRow.Effective), 90),
                Text("Expiry", nameof(OgLeaseRow.Expiry), 90),
                Text("Royalty", nameof(OgLeaseRow.Royalty), 70),
                Text("NRI", nameof(OgLeaseRow.NetRevenue), 70),
                Text("State", nameof(OgLeaseRow.Status), 80),
            },
        };

        Named(_grid, "Lease register");
        _grid.SelectionChanged += (_, _) => ShowSelected();

        var filters = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        filters.Children.Add(Caption("Type"));
        filters.Children.Add(_kind);
        filters.Children.Add(Caption("Region"));
        filters.Children.Add(_region);
        filters.Children.Add(Caption("Operator"));
        filters.Children.Add(_operator);
        filters.Children.Add(Caption("State"));
        filters.Children.Add(_status);
        filters.Children.Add(Caption("Expiring within"));
        filters.Children.Add(_expiringWithin);
        filters.Children.Add(Button("Apply", (_, _) => Load()));
        filters.Children.Add(Button("Clear", (_, _) => ClearFilters()));

        var actions = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actions.Children.Add(Button("Refresh", (_, _) => Load()));
        actions.Children.Add(Button("Export CSV", (_, _) => ExportCsv()));
        actions.Children.Add(Caption("Link selected to field"));
        actions.Children.Add(_fieldToLink);
        actions.Children.Add(Button("Link", (_, _) => _ = LinkSelectedAsync()));

        var panel = new StackPanel();
        panel.Children.Add(actions);
        panel.Children.Add(filters);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_grid);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgLeaseRegister? Register { get; private set; }

    public IReadOnlyList<OgLeaseSummary> Leases => Register?.Leases ?? [];

    public OgLeaseSummary? Selected { get; private set; }

    public bool LastFailed { get; private set; }

    public string? ExportedPath { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string DetailText => _detail.Text;

    /// <summary>The filter the controls describe.</summary>
    public OgLeaseFilter Filter =>
        new(
            Kind: Empty(_kind.SelectedItem as string),
            Region: Empty(_region.SelectedItem as string),
            Operator: Empty(_operator.SelectedItem as string),
            Status: Empty(_status.SelectedItem as string),
            ExpiringWithinDays: Days(_expiringWithin.Text));

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _register
            .LoadAsync(session.FieldId, session.FieldName, Filter, DateTime.Today)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Register = null;
            Selected = null;
            _grid.ItemsSource = null;
            _header.Text = "Lease register";
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The lease register could not be read.";
            ShowState(OgViewState.Failed, "The lease register could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Register = loaded.Value;
        Selected = null;

        Fill(_kind, Register.Kinds);
        Fill(_region, Register.Regions);
        Fill(_operator, Register.Operators);

        _grid.ItemsSource = Leases
            .Select(lease => new OgLeaseRow(
                lease.LeaseNumber ?? lease.Id,
                lease.Kind,
                lease.Name ?? string.Empty,
                lease.AcreageText,
                lease.Region ?? string.Empty,
                lease.Operator ?? string.Empty,
                lease.EffectiveText,
                lease.ExpiryText,
                lease.RoyaltyText,
                lease.NetRevenueText,
                lease.StatusText))
            .ToList();

        _header.Text =
            $"{Register.Count.ToString(CultureInfo.InvariantCulture)} lease(s) · " +
            (Filter.HasFilters ? Filter.Describe() : "no filters") +
            $" · as of {Register.AsOf.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

        _notice.Text = Register.IsEmpty
            ? (Filter.HasFilters
                ? "No lease matches these filters. Clear them to see the whole portfolio."
                : "No lease is recorded for this field.")
            : Register.MissingAcreageUnitCount == 0
                ? "Every lease shows its acreage with a recorded unit."
                : $"{Register.MissingAcreageUnitCount.ToString(CultureInfo.InvariantCulture)} lease(s) have no " +
                  "acreage unit; the register says so instead of showing a bare number.";

        _detail.Text = "Select a lease for its identity and to link it to a field.";
        ShowState(Register.IsEmpty ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    /// <summary>AC-4: links the selected lease to a field, recording the actor and date.</summary>
    public async Task<bool> LinkSelectedAsync()
    {
        if (Selected is not { } lease)
        {
            _notice.Text = "Select a lease first.";
            return false;
        }

        var linked = await _register
            .LinkFieldAsync(lease.Id, _fieldToLink.Text, _application.Session.UserId)
            .ConfigureAwait(true);

        _notice.Text = linked.Success
            ? linked.Message ?? "Lease linked."
            : linked.Message ?? "The lease was not linked.";

        if (linked.Success)
        {
            _application.AuditLog.Add(
                "lease.link",
                "lease",
                lease.Id,
                "ok",
                $"field={_fieldToLink.Text.Trim()}");
            Load();
        }

        return linked.Success;
    }

    /// <summary>AC-5: writes exactly the rows the grid shows.</summary>
    public bool ExportCsv()
    {
        if (Register is null)
        {
            _notice.Text = "There is no register to export.";
            return false;
        }

        try
        {
            Directory.CreateDirectory(OgPaths.ExportsDirectory);

            var path = Path.Combine(
                OgPaths.ExportsDirectory,
                $"lease-register-{DateTime.Now:yyyyMMdd-HHmmss}.csv");

            File.WriteAllText(path, Register.ToCsv());
            ExportedPath = path;

            _notice.Text =
                $"{Register.Count.ToString(CultureInfo.InvariantCulture)} lease(s) exported to {path}.";
            _application.AuditLog.Add(
                "lease.export", "lease", "register", "ok", Path.GetFileName(path));
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting the lease register",
                "no file was written",
                TheTechIdeaWeb.Diagnostics.FailureSeverity.Error);

            _notice.Text = $"The register could not be exported. Reference: {reference}";
            return false;
        }
    }

    public bool ClearFilters()
    {
        _kind.SelectedItem = null;
        _region.SelectedItem = null;
        _operator.SelectedItem = null;
        _status.SelectedItem = null;
        _expiringWithin.Text = string.Empty;

        return Load();
    }

    private void ShowSelected()
    {
        var index = _grid.SelectedIndex;

        if (index < 0 || index >= Leases.Count)
        {
            return;
        }

        Selected = Leases[index];

        _detail.Text =
            $"{Selected.Describe()} · acreage {Selected.AcreageText} · royalty {Selected.RoyaltyText} · " +
            $"NRI {Selected.NetRevenueText} · state {Selected.StatusText} (from the dates) · " +
            $"recorded in {Selected.Source}" +
            (string.IsNullOrWhiteSpace(Selected.FieldId) ? string.Empty : $" · field {Selected.FieldId}") +
            (string.IsNullOrWhiteSpace(Selected.StoredStatus)
                ? string.Empty
                : $" · the row's stored status is '{Selected.StoredStatus}', which the register does not use");

        _fieldToLink.Text = Selected.FieldId ?? string.Empty;
    }

    private static void Fill(ComboBox picker, IReadOnlyList<string> values)
    {
        var current = picker.SelectedItem as string;

        picker.ItemsSource = values;
        picker.SelectedItem = values.FirstOrDefault(value =>
            string.Equals(value, current, StringComparison.OrdinalIgnoreCase));
    }

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? Days(string? text) =>
        int.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) && value >= 0
            ? value
            : null;

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
