using System.Windows.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// DAT-01: the connection list, a connection test, and provisioning of the local working database.
/// Nothing on this page opens a database directly — it calls <see cref="IOgConnectionCatalog"/>.
/// </summary>
public sealed class ConnectionsView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly DataGrid _grid;
    private readonly TextBlock _detail;

    public ConnectionsView(OgApplication application)
        : base(application, "connections")
    {
        _application = application;

        _grid = new DataGrid
        {
            IsTabStop = true, // REL-03 AC-1: a read-only grid must be reachable by Tab.
            AutoGenerateColumns = true,
            IsReadOnly = true,
            ItemsSource = new List<OgConnectionInfo>(),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Margin = new Thickness(0, 8, 0, 0),
        };

        _detail = Body(string.Empty);

        var create = new Button
        {
            Content = application.Loc.Get("Connections.CreateLocal"),
            Padding = new Thickness(14, 6, 14, 6),
            Margin = new Thickness(0, 0, 8, 0),
        };
        create.Click += OnCreateLocalAsync;

        var test = new Button
        {
            Content = application.Loc.Get("Connections.Test"),
            Padding = new Thickness(14, 6, 14, 6),
            IsEnabled = false,
        };
        test.Click += OnTestSelectedAsync;

        _grid.SelectionChanged += (_, _) => test.IsEnabled = _grid.SelectedItem is OgConnectionInfo;

        var bar = new StackPanel { Orientation = Orientation.Horizontal };
        bar.Children.Add(create);
        bar.Children.Add(test);

        var panel = new DockPanel();
        DockPanel.SetDock(bar, Dock.Top);
        panel.Children.Add(bar);
        DockPanel.SetDock(_detail, Dock.Top);
        panel.Children.Add(_detail);
        panel.Children.Add(_grid);

        Named(_grid, "Connections");
        ShowContent(panel);

        Loaded += OnLoadedAsync;
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e) => await RefreshAsync();

    private async Task RefreshAsync()
    {
        try
        {
            var listed = await _application.Connections.ListAsync();
            if (!listed.Success)
            {
                ShowState(OgViewState.Failed, listed.Message ?? "The connection list could not be read.", listed.Reference);
                return;
            }

            var items = listed.Value ?? [];
            _grid.ItemsSource = items;
            _detail.Text = items.Count == 0
                ? _application.Loc.Get("Connections.Empty")
                : _application.Loc.Get("Connections.Count", items.Count);
            ShowState(OgViewState.Ready, string.Empty);
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "listing the configured connections",
                "no connections are shown",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The connection list could not be read.", reference);
        }
    }

    private async void OnCreateLocalAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            var request = new OgLocalDatabaseRequest(
                _application.Session.ConnectionName,
                Bootstrapping.OgPaths.LocalDatabaseDirectory,
                _application.Session.ConnectionName + ".db");

            var created = await _application.Connections.EnsureLocalAsync(request);

            if (!created.Success || created.Value is null)
            {
                _application.Notifications.Publish(new Diagnostics.OgDiagnosticEvent(
                    created.Reference ?? "local-database-not-created",
                    FailureSeverity.Error,
                    "creating the local database"));

                ShowState(OgViewState.Failed, created.Message ?? "The local database was not created.", created.Reference);
                return;
            }

            _application.Notifications.Success("Connections.Created", created.Value.Name);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "creating the local database",
                "no local database was created",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The local database could not be created.", reference);
        }
    }

    private async void OnTestSelectedAsync(object sender, RoutedEventArgs e)
    {
        if (_grid.SelectedItem is not OgConnectionInfo selected)
        {
            return;
        }

        try
        {
            var tested = await _application.Connections.TestAsync(selected.Name);

            if (!tested.Success || tested.Value is null)
            {
                ShowState(OgViewState.Failed, tested.Message ?? "The connection test failed.", tested.Reference);
                return;
            }

            _detail.Text = _application.Loc.Get(
                "Connections.TestResult",
                tested.Value.State.ToString(),
                tested.Value.Detail ?? string.Empty);

            if (tested.Value.State == OgConnectionState.Ok)
            {
                _application.Notifications.Success("Connections.Ok", tested.Value.Name);
            }
            else
            {
                _application.Notifications.Warn("Connections.NotOk", tested.Value.Name, tested.Value.State.ToString());
            }

            await RefreshAsync();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                $"testing the connection '{selected.Name}'",
                "the connection was not proven usable",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The connection test failed.", reference);
        }
    }
}
