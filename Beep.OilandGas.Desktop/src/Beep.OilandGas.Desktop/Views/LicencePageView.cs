using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Services;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// REL-02: the licence page — what the installation is entitled to, how it was verified, and the two actions
/// that change it (import a file, switch enforcement).
/// </summary>
/// <remarks>
/// Importing is refused unless the signature verifies, so a bad file can never degrade a working installation.
/// Enforcement is a visible setting with the trade written beside it, because the commercial terms are not yet
/// decided and an operator needs to know which behaviour they are choosing.
/// </remarks>
public sealed class LicencePageView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _grid;
    private readonly TextBox _path = new() { MinWidth = 420, Margin = new Thickness(0, 0, 8, 0) };
    private readonly ComboBox _enforcement = new() { MinWidth = 120, Margin = new Thickness(0, 0, 8, 0) };

    public LicencePageView(OgApplication application)
        : base(application, "licence")
    {
        _application = application;

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _grid = new DataGrid
        {
            IsTabStop = true, // REL-03 AC-1: a read-only grid must be reachable by Tab.
            AutoGenerateColumns = false,
            IsReadOnly = true,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Margin = new Thickness(0, 8, 0, 8),
            Columns =
            {
                Text("Field", nameof(OgLicenceRow.Field), 140),
                Star("Value", nameof(OgLicenceRow.Value)),
            },
        };

        _enforcement.ItemsSource = Enum.GetValues<OgLicenceEnforcement>();
        _enforcement.SelectionChanged += (_, _) =>
        {
            if (_enforcement.SelectedItem is OgLicenceEnforcement chosen
                && chosen != _application.Licence.Current.Enforcement)
            {
                ApplyEnforcement(chosen);
            }
        };

        var import = new Button
        {
            Content = "Import licence",
            Padding = new Thickness(12, 4, 12, 4),
            Margin = new Thickness(0, 0, 8, 0),
        };
        import.Click += (_, _) => Import(_path.Text);

        var reload = new Button
        {
            Content = "Re-read file",
            Padding = new Thickness(12, 4, 12, 4),
        };
        reload.Click += (_, _) =>
        {
            _application.Licence.Reload();
            Refresh();
        };

        var pathRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        pathRow.Children.Add(Caption("Licence file"));
        pathRow.Children.Add(_path);
        pathRow.Children.Add(import);
        pathRow.Children.Add(reload);

        var enforcementRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        enforcementRow.Children.Add(Caption("Enforcement"));
        enforcementRow.Children.Add(_enforcement);
        enforcementRow.Children.Add(Body(
            "Permissive reports a licence problem but blocks nothing; Enforced withholds ungranted modules " +
            "and blocks writes after expiry while reads continue."));

        var panel = new StackPanel();
        panel.Children.Add(_summary);
        panel.Children.Add(_notice);
        panel.Children.Add(_grid);
        panel.Children.Add(pathRow);
        panel.Children.Add(enforcementRow);

        Named(_grid, "Licence facts");
        Named(_path, "Licence file path");
        Named(_enforcement, "Enforcement");
        ShowContent(panel);

        _path.Text = Path.Combine(AppContext.BaseDirectory, "Security", "Samples", "sample-licence.json");
        Refresh();
    }

    /// <summary>The facts currently shown; the smoke run and tests read these.</summary>
    public IReadOnlyList<OgLicenceRow> Rows { get; private set; } = [];

    public string SummaryText => _summary.Text;

    /// <summary>Installs a licence file. Returns false with the reason shown when the signature does not verify.</summary>
    public bool Import(string path)
    {
        try
        {
            var result = _application.Licence.Import(path);

            Refresh();

            if (!result.Success)
            {
                _notice.Text = $"{result.Message}  ({result.Reference})";
                return false;
            }

            _notice.Text = result.Message ?? "The licence was imported.";
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "importing a licence",
                "the licence was not imported",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The licence could not be imported.", reference);
            return false;
        }
    }

    /// <summary>Switches enforcement; audited by the service.</summary>
    public bool ApplyEnforcement(OgLicenceEnforcement enforcement)
    {
        try
        {
            var result = _application.Licence.SetEnforcement(enforcement);
            Refresh();
            _notice.Text = result.Message ?? _application.Licence.Current.StatusText;
            return result.Success;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "changing the licence enforcement",
                "enforcement is unchanged",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "Enforcement could not be changed.", reference);
            return false;
        }
    }

    private void Refresh()
    {
        var licence = _application.Licence.Current;

        Rows = OgLicenceRows.From(licence, _application.Licence.LicencePath);
        _grid.ItemsSource = Rows;

        _summary.Text = licence.StatusText;
        _notice.Text = licence.Error ?? string.Empty;

        _enforcement.SelectedItem = licence.Enforcement;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
    };

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };

    private static DataGridTextColumn Star(string header, string path) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
    };
}
