using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// FND-07: sign-in. A refused attempt shows the identity service's own sentence; the audit trail records
/// every attempt, and the shell is only created after this returns true.
/// </summary>
public sealed class SignInWindow : Window
{
    private readonly OgApplication _application;
    /// <summary>
    /// The account picker. Editable, so a typed name is still accepted: the list saves a person on a shared
    /// workstation from guessing the spelling, it does not restrict who may try.
    /// </summary>
    private readonly ComboBox _userName = new()
    {
        MinWidth = 260,
        IsEditable = true,
        IsTextSearchEnabled = true,
        Margin = new Thickness(0, 2, 0, 10),
    };

    private readonly TextBlock _directoryState;
    private readonly PasswordBox _password = new() { MinWidth = 260, Margin = new Thickness(0, 2, 0, 10) };
    private readonly TextBlock _status;

    public SignInWindow(OgApplication application)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));

        Title = "Beep Oil & Gas — sign in";
        Width = 440;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = (Brush?)Application.Current?.TryFindResource("BeepBackgroundBrush") ?? Brushes.White;
        FlowDirection = application.Loc.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        _directoryState = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize = 11,
            Margin = new Thickness(0, 0, 0, 6),
            Foreground = Application.Current?.TryFindResource("BeepTextSecondaryBrush") as Brush ?? Brushes.Gray,
        };

        _status = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0),
            Foreground = (Brush?)Application.Current?.TryFindResource("BeepDangerBrush") ?? Brushes.Firebrick,
        };

        Content = BuildContent();
    }

    private FrameworkElement BuildContent()
    {
        var panel = new StackPanel { Margin = new Thickness(20) };

        // Before sign-in a company deployment knows the server, not the company: naming a company here would mean
        // reading it from this workstation's file, which is exactly what a company installation must not do.
        var state = _application.SetupState;
        var heading = state?.UsesCompanyDatabase == true
            ? $"Sign in to {(string.IsNullOrWhiteSpace(state.DataConnectionName) ? "the company database" : state.DataConnectionName)}"
            : state?.CompanyName ?? "Beep Oil & Gas";

        panel.Children.Add(new TextBlock
        {
            Text = heading,
            FontWeight = FontWeights.SemiBold,
            FontSize = 15,
            Margin = new Thickness(0, 0, 0, 12),
        });

        panel.Children.Add(Caption("User name"));
        panel.Children.Add(_userName);
        panel.Children.Add(_directoryState);
        panel.Children.Add(Caption("Password"));
        panel.Children.Add(_password);

        var signIn = new Button
        {
            Content = "Sign in",
            Padding = new Thickness(16, 6, 16, 6),
            MinWidth = 140,
            IsDefault = true,
            Margin = new Thickness(0, 10, 0, 0),
        };
        signIn.Click += OnSignIn;

        // The directory is read when the window is shown: the installation's database is already open by then (the
        // startup probe reached it), and a failure here only means the names are not offered.
        Loaded += OnLoaded;

        panel.Children.Add(signIn);
        panel.Children.Add(_status);
        panel.Children.Add(Caption($"Data root: {Bootstrapping.OgPaths.DataRoot}"));

        return panel;
    }

    /// <summary>Offers the user names of accounts that can sign in; a typed name still works.</summary>
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var names = await _application.DatabaseIdentity().ListUserNamesAsync().ConfigureAwait(true);

            if (!names.Success || names.Value is null)
            {
                _directoryState.Text = "The account names could not be listed; type the user name.";
                return;
            }

            if (names.Value.Count == 0)
            {
                _directoryState.Text = "No account exists in this database yet.";
                return;
            }

            _userName.ItemsSource = names.Value;
            _userName.Text = names.Value.Count == 1 ? names.Value[0] : string.Empty;
            _directoryState.Text = $"{names.Value.Count} account(s) in this database.";
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reading the user names for the sign-in picker",
                "the sign-in window offers no names",
                FailureSeverity.Degraded);

            _directoryState.Text = $"The account names could not be listed. Reference: {reference}";
        }
    }

    private async void OnSignIn(object sender, RoutedEventArgs e)
    {
        _status.Text = string.Empty;

        try
        {
            // A company deployment authenticates against the company database's own account directory; a personal
            // one against this workstation's credential file. The window does not choose: the deployment does.
            var result = await _application
                .SignInAsync(_userName.Text?.Trim() ?? string.Empty, _password.Password)
                .ConfigureAwait(true);

            if (!result.Success)
            {
                // The identity service's refusal sentence; never an exception message.
                _status.Text = result.Message ?? "Sign-in failed.";
                _password.Clear();
                _password.Focus();
                return;
            }

            DialogResult = true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "signing in",
                "the session was not created",
                FailureSeverity.Error);

            _status.Text = $"Sign-in did not complete. Reference: {reference}";
        }
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        FontSize = 11.5,
        Foreground = (Brush?)Application.Current?.TryFindResource("BeepTextSecondaryBrush") ?? Brushes.Gray,
    };
}
