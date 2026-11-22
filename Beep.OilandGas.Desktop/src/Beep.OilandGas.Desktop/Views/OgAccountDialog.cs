using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>What the new-account dialog collected.</summary>
public sealed record OgNewAccountRequest(
    string UserName,
    string DisplayName,
    string Password,
    IReadOnlyList<string> Roles);

/// <summary>
/// The two small prompts the account page needs: a provisional password, and a new account.
/// </summary>
/// <remarks>
/// Both follow the same rule as the setup: the password is typed by an administrator, so the account is told to
/// choose its own at the next sign-in. The dialog checks the shared password policy before it returns, so the person
/// is not left guessing after a round trip.
/// </remarks>
public static class OgAccountDialog
{
    /// <summary>A provisional password, or null when the administrator cancelled.</summary>
    public static string? AskPassword(Window? owner, string title, string description)
    {
        var password = NewPasswordBox();
        var confirm = NewPasswordBox();
        var status = NewStatus();

        var panel = Panel(title, description);
        panel.Children.Add(Caption("Password"));
        panel.Children.Add(password);
        panel.Children.Add(Caption("Confirm the password"));
        panel.Children.Add(confirm);
        panel.Children.Add(status);

        var window = Window(owner, title, panel, out var save, out _);

        save.Click += (_, _) =>
        {
            status.Text = string.Empty;

            if (!string.Equals(password.Password, confirm.Password, StringComparison.Ordinal))
            {
                status.Text = "The two passwords do not match.";
                return;
            }

            var problems = OgPasswordPolicy.Problems(password.Password);

            if (problems.Count > 0)
            {
                status.Text = string.Join(" ", problems);
                return;
            }

            window.Tag = password.Password;
            window.DialogResult = true;
        };

        return window.ShowDialog() == true ? window.Tag as string : null;
    }

    /// <summary>A new account, or null when the administrator cancelled.</summary>
    public static OgNewAccountRequest? AskNewAccount(Window? owner, IReadOnlyList<string>? roles)
    {
        var userName = new TextBox { MinWidth = 240, Margin = new Thickness(0, 2, 0, 8) };
        var displayName = new TextBox { MinWidth = 240, Margin = new Thickness(0, 2, 0, 8) };
        var password = NewPasswordBox();
        var confirm = NewPasswordBox();
        var role = new ComboBox { MinWidth = 240, Margin = new Thickness(0, 2, 0, 8) };
        var status = NewStatus();

        role.ItemsSource = roles ?? [];
        role.SelectedItem = (roles ?? []).FirstOrDefault(candidate =>
            string.Equals(candidate, "Viewer", StringComparison.OrdinalIgnoreCase)) ?? (roles ?? []).FirstOrDefault();

        var panel = Panel(
            "New account",
            "The account is created in this installation's database with the role chosen here. Its password is " +
            "provisional: the person chooses their own at the next sign-in.");

        panel.Children.Add(Caption("User name"));
        panel.Children.Add(userName);
        panel.Children.Add(Caption("Full name"));
        panel.Children.Add(displayName);
        panel.Children.Add(Caption("Provisional password"));
        panel.Children.Add(password);
        panel.Children.Add(Caption("Confirm the password"));
        panel.Children.Add(confirm);
        panel.Children.Add(Caption("Role"));
        panel.Children.Add(role);
        panel.Children.Add(status);

        var window = Window(owner, "New account", panel, out var save, out _);

        save.Click += (_, _) =>
        {
            status.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(userName.Text))
            {
                status.Text = "A user name is required.";
                userName.Focus();
                return;
            }

            if (!string.Equals(password.Password, confirm.Password, StringComparison.Ordinal))
            {
                status.Text = "The two passwords do not match.";
                confirm.Focus();
                return;
            }

            var problems = OgPasswordPolicy.Problems(password.Password);

            if (problems.Count > 0)
            {
                status.Text = string.Join(" ", problems);
                password.Focus();
                return;
            }

            window.Tag = new OgNewAccountRequest(
                userName.Text.Trim(),
                string.IsNullOrWhiteSpace(displayName.Text) ? userName.Text.Trim() : displayName.Text.Trim(),
                password.Password,
                role.SelectedItem is string chosen && !string.IsNullOrWhiteSpace(chosen) ? [chosen] : []);

            window.DialogResult = true;
        };

        return window.ShowDialog() == true ? window.Tag as OgNewAccountRequest : null;
    }

    private static Window Window(
        Window? owner,
        string title,
        FrameworkElement content,
        out Button save,
        out Button cancel)
    {
        var window = new Window
        {
            Title = title,
            Width = 470,
            SizeToContent = SizeToContent.Height,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = owner is null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
            Background = Application.Current?.TryFindResource("BeepBackgroundBrush") as Brush ?? Brushes.White,
        };

        if (owner is not null)
        {
            window.Owner = owner;
            window.FlowDirection = owner.FlowDirection;
        }

        save = new Button
        {
            Content = "Save",
            IsDefault = true,
            Padding = new Thickness(16, 5, 16, 5),
            MinWidth = 100,
            Margin = new Thickness(0, 6, 8, 0),
        };

        cancel = new Button
        {
            Content = "Cancel",
            IsCancel = true,
            Padding = new Thickness(16, 5, 16, 5),
            MinWidth = 100,
            Margin = new Thickness(0, 6, 0, 0),
        };

        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        buttons.Children.Add(save);
        buttons.Children.Add(cancel);

        if (content is StackPanel panel)
        {
            panel.Children.Add(buttons);
        }

        window.Content = content;
        return window;
    }

    private static StackPanel Panel(string title, string description)
    {
        var panel = new StackPanel { Margin = new Thickness(20) };

        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.SemiBold,
            FontSize = 15,
            Margin = new Thickness(0, 0, 0, 6),
            Foreground = Application.Current?.TryFindResource("BeepTextPrimaryBrush") as Brush ?? Brushes.Black,
        });

        panel.Children.Add(Caption(description));
        return panel;
    }

    private static PasswordBox NewPasswordBox() => new() { MinWidth = 240, Margin = new Thickness(0, 2, 0, 8) };

    private static TextBlock NewStatus() => new()
    {
        TextWrapping = TextWrapping.Wrap,
        FontSize = 11.5,
        Margin = new Thickness(0, 4, 0, 4),
        Foreground = Application.Current?.TryFindResource("BeepDangerBrush") as Brush ?? Brushes.Firebrick,
    };

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        FontSize = 11.5,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 0, 0, 4),
        Foreground = Application.Current?.TryFindResource("BeepTextSecondaryBrush") as Brush ?? Brushes.Gray,
    };
}
