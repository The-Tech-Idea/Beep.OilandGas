using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls.Notifications;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Services;
using TheTechIdea.Beep.OilandGas.Desktop.Views;

namespace TheTechIdea.Beep.OilandGas.Desktop.Shell;

/// <summary>
/// The one navigable surface. Navigation comes from the page catalog filtered by the two gates; the view
/// factory refuses any route the catalog does not contain. Toasts are added only by the pipeline.
/// </summary>
public partial class OgShellWindow : Window
{
    private readonly OgApplication _application;
    private readonly Action? _onClosed;
    private bool _restoringSelection;
    private readonly List<string> _history = [];

    /// <summary>FND-07 AC-7: where this role set lands, and why. The smoke run reads it.</summary>
    public OgLanding Landing { get; }

    /// <summary>
    /// The route actually selected after the window loads. <see cref="Landing"/> can only say where the shell
    /// intended to open; this says where it did, which is what the person sees.
    /// </summary>
    public string? LandedRoute => (NavList.SelectedItem as OgNavigationItem)?.Route;

    public OgShellWindow(OgApplication application, Action? onClosed = null)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
        _onClosed = onClosed;

        InitializeComponent();

        ContextText.Text = application.StatusLine;
        ModeText.Text = BuildModeLine(application);
        StatusText.Text = application.Session.CompanyName;

        // FND-07 AC-7: the RBAC role set decides where the shell opens. The route must also pass the same
        // guard the navigation uses, so the landing is never a page the shell would refuse.
        Landing = OgPersonaLanding.Resolve(
            application.Permissions.Roles,
            route => application.Navigation.CanAccess(route, application.Permissions));

        NotificationsButton.Content = application.Loc.Get("Shell.Notifications");
        HealthButton.Content = application.Loc.Get("Shell.DataPath");
        ApplyDirection();

        // DAT-03/04: a page can ask for another page (the tree opens the workbench).
        application.Pages.Handler = route => ShowRoute(route);

        application.Notifications.Raised += OnToastRaised;
        application.Loc.LanguageChanged += OnLanguageChanged;
        application.WellboreLifetime.EnvironmentChanged += OnEnvironmentChanged;
        application.Licence.Changed += OnLicenceChanged;
        Theme.OgSkins.ApplyTo(this);

        // REL-02: the licence decides whether the banner is needed and what it says.
        RefreshLicenceBanner();

        Loaded += OnLoaded;
        Closed += OnClosed;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    private void ApplyDirection() =>
        FlowDirection = _application.Loc.IsRightToLeft
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;

    /// <summary>
    /// The status chip: the deployment mode, the resolved access mode (FND-02) and who is signed in.
    /// Showing "not configured" here is deliberate — it is the difference between an empty list and a
    /// data path that does not exist.
    /// </summary>
    private static string BuildModeLine(OgApplication application) =>
        $"Mode: {application.Session.Mode}  ·  access: {application.Access.Mode}  ·  {application.Session.UserDisplayName}" +
        $"  ·  persona: {application.Session.PersonaCode}";

    private void OnNotificationsClick(object sender, RoutedEventArgs e)
    {
        var window = new NotificationCenterWindow(_application.Notifications, _application.Loc)
        {
            Owner = this,
        };

        window.Show();
    }

    /// <summary>FND-02: the data-path health window behind the status chip.</summary>
    private void OnHealthClick(object sender, RoutedEventArgs e)
    {
        var window = new ConnectionHealthWindow(_application) { Owner = this };
        window.Show();
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        ApplyDirection();
        NotificationsButton.Content = _application.Loc.Get("Shell.Notifications");
        ContextText.Text = _application.StatusLine;
        RefreshLicenceBanner();
        RebuildNavigation();
    }

    /// <summary>
    /// Rebuilds the navigation from the two gates and keeps the person on the same page when it is still
    /// allowed. Called after a language change and after a licence change (AC-6: no restart).
    /// </summary>
    private void RebuildNavigation()
    {
        var route = (NavList.SelectedItem as OgNavigationItem)?.Route;

        NavList.ItemsSource = _application.Navigation.Build(_application.Permissions);
        var restored = NavList.Items
            .OfType<OgNavigationItem>()
            .FirstOrDefault(item => string.Equals(item.Route, route, StringComparison.OrdinalIgnoreCase))
            ?? NavList.Items.OfType<OgNavigationItem>().FirstOrDefault();

        if (restored is null)
        {
            ContentHost.Content = null;
            StatusText.Text = _application.Loc.Get("State.Denied");
            return;
        }

        _restoringSelection = true;
        try
        {
            NavList.SelectedItem = restored;
        }
        finally
        {
            _restoringSelection = false;
        }

        ContentHost.Content = OgPageFactory.Create(_application, restored.Route);
    }

    private void OnLicenceChanged(object? sender, EventArgs e)
    {
        RefreshLicenceBanner();
        RebuildNavigation();
    }

    /// <summary>
    /// REL-02: a licence that is expired, in grace, or absent under enforcement is visible in the shell.
    /// A permissive installation with no licence says nothing, because nothing is being withheld.
    /// </summary>
    private void RefreshLicenceBanner()
    {
        var licence = _application.Licence.Current;

        var show = licence.IsExpired
                   || licence.IsInGrace
                   || (!licence.IsLicensed && licence.Enforcement == Core.OgLicenceEnforcement.Enforced);

        LicenceBanner.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        LicenceBannerText.Text = show ? licence.StatusText : string.Empty;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        NavList.ItemsSource = _application.Navigation.Build(_application.Permissions);

        var landing = NavList.Items
            .OfType<OgNavigationItem>()
            .FirstOrDefault(item => string.Equals(item.Route, Landing.Route, StringComparison.OrdinalIgnoreCase))
            ?? NavList.Items.OfType<OgNavigationItem>().FirstOrDefault();

        if (landing is not null)
        {
            NavList.SelectedItem = landing;
            NavList.ScrollIntoView(landing);

            // One audited row per start, so "why did it open here?" is answerable later.
            _application.AuditLog.Add(
                "landing",
                "page",
                landing.Route,
                "ok",
                $"{Landing.Because} persona={Landing.Persona} roles={string.Join(',', _application.Permissions.Roles)}");
        }
        else
        {
            StatusText.Text = _application.Loc.Get("State.Denied");
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _application.Notifications.Raised -= OnToastRaised;
        _application.Loc.LanguageChanged -= OnLanguageChanged;
        _application.WellboreLifetime.EnvironmentChanged -= OnEnvironmentChanged;
        _application.Licence.Changed -= OnLicenceChanged;

        // FND-01 AC-5: the shell is the application's main window, so closing it ends the process. The
        // action is injectable so a test can prove the rule without terminating its own process; the
        // production default is the real application shutdown. Dialogs closing never reach here, which is
        // why they cannot end the session (App.xaml is OnExplicitShutdown).
        (_onClosed ?? DefaultShutdown)();
    }

    private static void DefaultShutdown() => System.Windows.Application.Current?.Shutdown(0);

    /// <summary>
    /// REL-03 §6: the global keyboard map. Every entry works with no mouse, and F1 lists them so the map is
    /// discoverable.
    /// </summary>
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var modifiers = Keyboard.Modifiers;

        if (key == Key.K && modifiers.HasFlag(ModifierKeys.Control))
        {
            OpenPalette();
            e.Handled = true;
            return;
        }

        if (key == Key.Tab && modifiers.HasFlag(ModifierKeys.Control))
        {
            MovePage(modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1);
            e.Handled = true;
            return;
        }

        if (key == Key.F6)
        {
            CycleFocus();
            e.Handled = true;
            return;
        }

        if (key == Key.F1)
        {
            new ShortcutsWindow(_application) { Owner = this }.ShowDialog();
            e.Handled = true;
            return;
        }

        if (key == Key.Left && modifiers.HasFlag(ModifierKeys.Alt))
        {
            GoBack();
            e.Handled = true;
        }
    }

    private void OpenPalette()
    {
        var entries = _application.Navigation
            .Build(_application.Permissions)
            .Select(item => new Core.OgPaletteEntry(item.Route, item.Title, item.Group))
            .ToList();

        var palette = new Views.CommandPaletteWindow(_application, entries) { Owner = this };

        if (palette.ShowDialog() == true && palette.SelectedRoute is { } route)
        {
            NavigateTo(route);
        }
    }

    private void NavigateTo(string route)
    {
        var item = NavList.Items
            .OfType<OgNavigationItem>()
            .FirstOrDefault(candidate => string.Equals(candidate.Route, route, StringComparison.OrdinalIgnoreCase));

        if (item is not null)
        {
            NavList.SelectedItem = item;
            NavList.ScrollIntoView(item);
        }
    }

    private void MovePage(int delta)
    {
        if (NavList.Items.Count == 0)
        {
            return;
        }

        NavList.SelectedIndex = Core.OgCommandPalette.Move(
            Math.Max(0, NavList.SelectedIndex),
            delta,
            NavList.Items.Count);
    }

    private void GoBack()
    {
        if (_history.Count < 2)
        {
            return;
        }

        _history.RemoveAt(_history.Count - 1);
        var route = _history[^1];
        var item = NavList.Items
            .OfType<OgNavigationItem>()
            .FirstOrDefault(candidate => string.Equals(candidate.Route, route, StringComparison.OrdinalIgnoreCase));

        if (item is null)
        {
            return;
        }

        _restoringSelection = true;
        try
        {
            NavList.SelectedItem = item;
        }
        finally
        {
            _restoringSelection = false;
        }

        ContentHost.Content = OgPageFactory.Create(_application, route);
    }

    /// <summary>Shows a page by route, as if it had been chosen in the navigation. Returns false when the route is not offered.</summary>
    public bool ShowRoute(string route)
    {
        var item = NavList.Items
            .OfType<OgNavigationItem>()
            .FirstOrDefault(candidate => string.Equals(candidate.Route, route, StringComparison.OrdinalIgnoreCase));

        if (item is null)
        {
            return false;
        }

        NavList.SelectedItem = item;
        NavList.ScrollIntoView(item);
        return true;
    }

    /// <summary>F6 moves focus between the navigation and the page, so a keyboard user can leave a grid.</summary>
    private void CycleFocus()
    {
        if (NavList.IsKeyboardFocusWithin)
        {
            if (!FocusFirst(ContentHost))
            {
                ContentHost.Focus();
            }

            return;
        }

        NavList.Focus();
    }

    private static bool FocusFirst(DependencyObject root)
    {
        if (root is Control { Focusable: true, IsVisible: true } control && control.Focus())
        {
            return true;
        }

        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
        {
            if (FocusFirst(child))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// VSB-02: an environment reload or preset change can alter units and styling, so every open view is
    /// rebuilt rather than left showing data drawn under the previous configuration.
    /// </summary>
    private void OnEnvironmentChanged(object? sender, EventArgs e)
    {
        var route = (NavList.SelectedItem as OgNavigationItem)?.Route;
        if (string.IsNullOrWhiteSpace(route))
        {
            return;
        }

        ContentHost.Content = OgPageFactory.Create(_application, route);

        if (_application.WellboreLifetime.LastChangeSummary is { Length: > 0 } summary)
        {
            StatusText.Text = summary;
        }
    }

    private void OnNavigationSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_restoringSelection || NavList.SelectedItem is not OgNavigationItem item)
        {
            return;
        }

        if (!_application.Navigation.CanAccess(item.Route, _application.Permissions))
        {
            _application.Diagnostics.Add(
                "authz-denied",
                "page",
                item.Route,
                "refused",
                "navigation selection refused by the route guard");

            StatusText.Text = _application.Loc.Get("State.Denied");
            RestoreSelection();
            return;
        }

        var view = OgPageFactory.Create(_application, item.Route);
        if (view is null)
        {
            StatusText.Text = _application.Loc.Get("State.Empty");
            return;
        }

        ContentHost.Content = view;
        StatusText.Text = $"{item.Title}  ·  {_application.Session.CompanyName}";

        // REL-03: Alt+Left walks back through the pages visited in this session.
        if (_history.Count == 0 || _history[^1] != item.Route)
        {
            _history.Add(item.Route);
        }
    }

    private void RestoreSelection()
    {
        _restoringSelection = true;
        try
        {
            NavList.SelectedIndex = NavList.Items.Count > 0 ? 0 : -1;
            if (NavList.SelectedItem is OgNavigationItem fallback)
            {
                ContentHost.Content = OgPageFactory.Create(_application, fallback.Route);
            }
        }
        finally
        {
            _restoringSelection = false;
        }
    }

    private void OnToastRaised(object? sender, OgToast toast)
    {
        // The pipeline may publish from a background thread; the surface is UI-thread only.
        Dispatcher.BeginInvoke(() =>
        {
            var title = toast.Kind switch
            {
                OgToastKind.Success => "Done",
                OgToastKind.Warning => "Attention",
                OgToastKind.Failure => "Something went wrong",
                OgToastKind.Audit => "Recorded",
                OgToastKind.Info => "Information",
            };

            var text = _application.Loc.Get(toast.MessageKey, toast.Args ?? []);
            Toasts.Add(new ToastItem(toast, text, title));
        });
    }
}
