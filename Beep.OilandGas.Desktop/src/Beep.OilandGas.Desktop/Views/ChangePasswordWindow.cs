using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// SETUP-03 AC-4: the account the setup created was given a password by somebody else, so its owner replaces it
/// before the shell opens.
/// </summary>
/// <remarks>
/// This is the bootstrap-admin rule Keycloak documents: a provisional account may administer, but it exists only until
/// permanent access is established, and the application says so rather than letting the typed-at-setup password live
/// on. The window cannot be dismissed into the shell: cancelling ends the session.
/// </remarks>
public sealed class ChangePasswordWindow : Window
{
    private readonly OgApplication _application;
    private readonly PasswordBox _password = new() { MinWidth = 260, Margin = new Thickness(0, 2, 0, 10) };
    private readonly PasswordBox _confirm = new() { MinWidth = 260, Margin = new Thickness(0, 2, 0, 10) };
    private readonly TextBlock _status;
    private readonly Button _save;

    public ChangePasswordWindow(OgApplication application)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));

        Title = "Beep Oil & Gas — choose your password";
        Width = 480;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Application.Current?.TryFindResource("BeepBackgroundBrush") as Brush ?? Brushes.White;
        FlowDirection = application.Loc.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        _status = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0),
            Foreground = Application.Current?.TryFindResource("BeepDangerBrush") as Brush ?? Brushes.Firebrick,
        };

        _save = new Button
        {
            Content = "Save the new password",
            Padding = new Thickness(16, 6, 16, 6),
            MinWidth = 180,
            IsDefault = true,
            Margin = new Thickness(0, 8, 0, 0),
        };
        _save.Click += OnSave;

        Content = BuildContent();
    }

    private FrameworkElement BuildContent()
    {
        var panel = new StackPanel { Margin = new Thickness(20) };

        panel.Children.Add(new TextBlock
        {
            Text = "Choose your own password",
            FontWeight = FontWeights.SemiBold,
            FontSize = 16,
            Margin = new Thickness(0, 0, 0, 6),
            Foreground = Application.Current?.TryFindResource("BeepTextPrimaryBrush") as Brush ?? Brushes.Black,
        });

        panel.Children.Add(Caption(
            "This account was prepared by whoever set the installation up, so the password they typed is known to " +
            "them. Replace it before you continue."));

        panel.Children.Add(Caption(
            $"At least {OgPasswordPolicy.MinimumLength} characters and no more than {OgPasswordPolicy.MaximumLength}. " +
            "A sentence of several words is a good password; no punctuation tricks are required."));

        panel.Children.Add(Caption("New password"));
        panel.Children.Add(_password);
        panel.Children.Add(Caption("Confirm the new password"));
        panel.Children.Add(_confirm);
        panel.Children.Add(_save);
        panel.Children.Add(_status);

        var cancel = new Button
        {
            Content = "Sign out instead",
            Padding = new Thickness(16, 6, 16, 6),
            MinWidth = 140,
            Margin = new Thickness(0, 8, 0, 0),
        };
        cancel.Click += (_, _) =>
        {
            _application.SignOut();
            DialogResult = false;
        };

        panel.Children.Add(cancel);
        panel.Children.Add(Caption("The application does not open until the password has been replaced."));

        return panel;
    }

    private async void OnSave(object sender, RoutedEventArgs e)
    {
        _status.Text = string.Empty;

        if (!string.Equals(_password.Password, _confirm.Password, StringComparison.Ordinal))
        {
            _status.Text = "The two passwords do not match.";
            _confirm.Focus();
            return;
        }

        var problems = OgPasswordPolicy.Problems(_password.Password);

        if (problems.Count > 0)
        {
            _status.Text = string.Join(" ", problems);
            _password.Focus();
            return;
        }

        _save.IsEnabled = false;

        try
        {
            var changed = await _application.ChangePasswordAsync(_password.Password).ConfigureAwait(true);

            if (!changed.Success)
            {
                _status.Text = changed.Message ?? "The password was not changed.";
                return;
            }

            DialogResult = true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "changing the password after a setup",
                "the password was not changed",
                FailureSeverity.Error);

            _status.Text = $"The password was not changed. Reference: {reference}";
        }
        finally
        {
            _save.IsEnabled = true;
        }
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        FontSize = 11.5,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 0, 0, 6),
        Foreground = Application.Current?.TryFindResource("BeepTextSecondaryBrush") as Brush ?? Brushes.Gray,
    };
}
