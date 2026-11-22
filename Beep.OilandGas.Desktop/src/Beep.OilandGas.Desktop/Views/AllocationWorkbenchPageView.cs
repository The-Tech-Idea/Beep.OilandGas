using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// PRD-04: the allocation workbench - measured volumes turned into allocated volumes per well, with the method, factor
/// and residual visible so the numbers can be defended.
/// </summary>
/// <remarks>
/// Every row shows its method and residual, and a row with no basis reads "unallocated" rather than zero (AC-1). The run
/// is flagged when the allocated total is not the measured total within tolerance, and it cannot be committed until that
/// is acknowledged (AC-2). A manual override needs a reason and is recorded with it (AC-3). A commit is idempotent: the
/// same run is recognised by its inputs hash and is never posted twice (AC-4). A preview writes nothing (AC-5) and every
/// run carries the hash of its inputs (AC-6).
/// </remarks>
public sealed class AllocationWorkbenchPageView : OgPageBase
{
    public const string Route = "allocation";

    private readonly OgApplication _application;
    private readonly IOgAllocationWorkbench _workbench;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _residual;
    private readonly TextBlock _basis;
    private readonly TextBlock _history;
    private readonly TextBlock _detail;
    private readonly TextBox _period;
    private readonly ComboBox _method;
    private readonly ComboBox _facility;
    private readonly TextBox _tolerance;
    private readonly CheckBox _acknowledge;
    private readonly DataGrid _rows;
    private readonly ComboBox _overrideWell;
    private readonly TextBox _overrideValue;
    private readonly TextBox _overrideReason;

    private readonly Dictionary<string, OgAllocationOverride> _overrides = new(StringComparer.OrdinalIgnoreCase);

    public AllocationWorkbenchPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _workbench = application.AllocationWorkbench;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _residual = Body(string.Empty);
        _residual.TextWrapping = TextWrapping.Wrap;
        _residual.Margin = new Thickness(0, 4, 0, 4);

        _basis = Body(string.Empty);
        _basis.TextWrapping = TextWrapping.Wrap;
        _basis.Margin = new Thickness(0, 0, 0, 4);

        _history = Body(string.Empty);
        _history.TextWrapping = TextWrapping.Wrap;
        _history.Margin = new Thickness(0, 0, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _period = Field("Period yyyy-MM-dd", 120);
        _period.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        _method = new ComboBox
        {
            MinWidth = 170,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = AllocationRules.Methods,
        };

        Named(_method, "Allocation method");
        _method.SelectedIndex = 0;

        _facility = new ComboBox { MinWidth = 190, Margin = new Thickness(0, 0, 8, 0) };
        Named(_facility, "Facility whose measurement is the total (optional)");

        _tolerance = Field("Tolerance percent", 100);
        _tolerance.Text = AllocationRules.DefaultTolerancePercent.ToString(CultureInfo.InvariantCulture);

        _acknowledge = new CheckBox
        {
            Content = "Acknowledge a residual beyond tolerance",
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };

        Named(_acknowledge, "Acknowledge the residual");

        _rows = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 120,
            MaxHeight = 210,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Well", "NameText", 130),
                Text("Measured", "MeasuredText", 120),
                Text("Allocated", "AllocatedText", 120),
                Text("Factor", "FactorText", 90),
                Text("Residual", "ResidualText", 110),
                Text("Method", "Method", 130),
                Text("Status", "StatusText", 100),
            },
        };

        Named(_rows, "Allocation rows");

        _overrideWell = new ComboBox { MinWidth = 150, Margin = new Thickness(0, 0, 8, 0) };
        Named(_overrideWell, "Well to override");

        _overrideValue = Field("Override allocated volume", 130);
        _overrideReason = Field("Override reason", 200);

        var run = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        run.Children.Add(Caption("Period"));
        run.Children.Add(_period);
        run.Children.Add(_method);
        run.Children.Add(_facility);
        run.Children.Add(Caption("Tolerance %"));
        run.Children.Add(_tolerance);
        run.Children.Add(Button("Preview", (_, _) => _ = PreviewAsync()));
        run.Children.Add(Button("Commit", (_, _) => _ = CommitAsync()));
        run.Children.Add(Button("Retry commit", (_, _) => _ = RetryAsync()));

        var overrideBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        overrideBar.Children.Add(Caption("Manual override"));
        overrideBar.Children.Add(_overrideWell);
        overrideBar.Children.Add(_overrideValue);
        overrideBar.Children.Add(_overrideReason);
        overrideBar.Children.Add(Button("Apply override", (_, _) => _ = ApplyOverrideAsync()));
        overrideBar.Children.Add(Button("Clear overrides", (_, _) => _ = ClearOverridesAsync()));

        var panel = new StackPanel();
        panel.Children.Add(run);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("Residual"));
        panel.Children.Add(_residual);
        panel.Children.Add(_acknowledge);
        panel.Children.Add(Caption("Allocation rows"));
        panel.Children.Add(_rows);
        panel.Children.Add(_detail);
        panel.Children.Add(overrideBar);
        panel.Children.Add(Caption("Basis per well"));
        panel.Children.Add(_basis);
        panel.Children.Add(Caption("Committed runs"));
        panel.Children.Add(_history);

        ShowContent(panel);

        Load();
    }

    public OgAllocationWorkspace? Workspace { get; private set; }

    public OgAllocationRun? Run => Workspace?.Run;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string ResidualText => _residual.Text;

    public string BasisText => _basis.Text;

    public string HistoryText => _history.Text;

    public string DetailText => _detail.Text;

    public bool Acknowledged => _acknowledge.IsChecked == true;

    public IReadOnlyCollection<OgAllocationOverride> Overrides => _overrides.Values;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _workbench
            .LoadAsync(
                session.FieldId,
                session.FieldName,
                (_facility.SelectedItem as OgAllocationFacility)?.Id,
                Date(_period.Text),
                _method.SelectedItem as string,
                Tolerance(),
                session.UserId)
            .GetAwaiter()
            .GetResult();

        return Show(loaded);
    }

    public async Task<bool> PreviewAsync()
    {
        _overrides.Clear();

        var previewed = await _workbench
            .PreviewAsync(Inputs(), _application.Session.FieldName)
            .ConfigureAwait(true);

        return Show(previewed);
    }

    public async Task<bool> ApplyOverrideAsync()
    {
        if (_overrideWell.SelectedItem is not OgAllocationRow row)
        {
            _notice.Text = "Choose the well to override.";
            return false;
        }

        if (!decimal.TryParse(_overrideValue.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture,
                out var allocated))
        {
            _notice.Text = "Enter the allocated volume the override should carry.";
            return false;
        }

        // AC-3: the reason travels with the override; the service refuses without it.
        _overrides[row.WellId] = new OgAllocationOverride(row.WellId, allocated, _overrideReason.Text);

        var previewed = await _workbench
            .PreviewAsync(Inputs(), _application.Session.FieldName)
            .ConfigureAwait(true);

        return Show(previewed);
    }

    public async Task<bool> ClearOverridesAsync()
    {
        _overrides.Clear();

        var previewed = await _workbench
            .PreviewAsync(Inputs(), _application.Session.FieldName)
            .ConfigureAwait(true);

        return Show(previewed);
    }

    public async Task<bool> CommitAsync()
    {
        var committed = await _workbench
            .CommitAsync(
                Inputs(),
                _acknowledge.IsChecked == true,
                _application.Session.UserId,
                _application.Session.FieldName)
            .ConfigureAwait(true);

        var shown = Show(committed);

        if (committed.Success)
        {
            _application.AuditLog.Add(
                "production.allocation.commit",
                "production",
                committed.Value?.Run?.Id ?? "(no run)",
                "ok",
                _application.Session.UserId);
        }

        return shown;
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _workbench
            .RetryCommitAsync(_acknowledge.IsChecked == true, _application.Session.FieldName)
            .ConfigureAwait(true);

        return Show(retried);
    }

    private OgAllocationInputs Inputs() =>
        new(
            _application.Session.FieldId,
            (_facility.SelectedItem as OgAllocationFacility)?.Id,
            Date(_period.Text) ?? DateTime.Today,
            _method.SelectedItem as string ?? AllocationRules.MeasuredShare,
            Tolerance(),
            _overrides.Values.ToList());

    private decimal Tolerance() =>
        decimal.TryParse(_tolerance.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : AllocationRules.DefaultTolerancePercent;

    private bool Show(OgResult<OgAllocationWorkspace> result)
    {
        if (!result.Success || result.Value is null)
        {
            LastFailed = true;
            Workspace = null;
            _rows.ItemsSource = null;
            _overrideWell.ItemsSource = null;
            _header.Text = "Allocation";
            _residual.Text = string.Empty;
            _basis.Text = string.Empty;
            _history.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = result.Message ?? "The allocation workbench could not be read.";
            ShowState(OgViewState.Failed, "The allocation workbench could not be read.", result.Reference);
            return false;
        }

        LastFailed = false;
        Workspace = result.Value;

        var workspace = result.Value;
        var run = workspace.Run;

        _facility.ItemsSource = workspace.Facilities.ToList();

        if (run is null)
        {
            _rows.ItemsSource = null;
            _overrideWell.ItemsSource = null;
            _header.Text = workspace.FieldName;
            _residual.Text = "no run is computed";
            _basis.Text = "no run is computed";
            _history.Text = workspace.HistoryText;
            _detail.Text = string.Empty;
            _notice.Text = "Choose a period and method, then preview a run.";
            ShowState(OgViewState.Empty, _notice.Text);
            return true;
        }

        _rows.ItemsSource = run.Rows.ToList();
        _overrideWell.ItemsSource = run.Rows.ToList();
        _overrideWell.SelectedIndex = run.Rows.Count > 0 ? 0 : -1;

        _header.Text = $"{workspace.FieldName} · {run.Summary}";

        _notice.Text = run.IsCommitted
            ? $"Committed by {run.CommittedBy ?? "(unknown)"} on " +
              $"{run.CommittedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "unknown"} · " +
              $"hash {Short(run.Hash)}"
            : run.IsBeyondTolerance
                ? "This run is not committed and cannot be committed until its residual is acknowledged."
                : "Preview only: nothing has been written. Commit writes the run.";

        _residual.Text =
            $"measured {run.TotalMeasuredText} · allocated {run.TotalAllocatedText} · residual {run.ResidualText} · " +
            $"tolerance {run.ToleranceText} · measured from {run.MeasuredSource} · {run.StateText}";

        _basis.Text = run.BasisText;
        _history.Text = workspace.HistoryText;

        _detail.Text = string.Join(
            Environment.NewLine,
            new[]
            {
                run.FlagsText,
                $"hash {run.Hash} (a hash of the inputs; the same run is never posted twice)",
                _overrides.Count == 0
                    ? "No override is applied."
                    : $"{_overrides.Count.ToString(CultureInfo.InvariantCulture)} override(s) applied to the preview.",
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

        ShowState(run.IsCommitted ? OgViewState.Ready : OgViewState.Ready, _notice.Text);
        return true;
    }

    private static string Short(string hash) => hash.Length <= 12 ? hash : hash[..12];

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
