using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Security;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// SETUP-05 AC-9 and SETUP-10 AC-6: the SystemAdmin's account directory. Every user of the installation's database
/// is listed with the roles it holds; the administrator creates accounts, resets a password, enables or disables an
/// account, and grants or revokes a role.
/// </summary>
/// <remarks>
/// The page is gated on the administrator role because that is the authority the database granted at sign-in, not
/// because a button is hidden: a non-administrator sees the page's refusal and no controls.
/// </remarks>
public sealed class UsersView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly StackPanel _rows = new();
    private readonly TextBlock _status;
    private readonly ComboBox _rolePicker = new() { MinWidth = 200, Margin = new Thickness(0, 2, 8, 8) };

    public UsersView(OgApplication application)
        : base(application, "users")
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));

        // The database decides: the administrator role, or the permission granted through a role
        // (Admin.ManageUsers). A role alone would be too coarse to express "may administer accounts".
        if (!_application.Permissions.CanManageUsers)
        {
            ShowState(
                OgViewState.Denied,
                "Your roles do not include Admin.ManageUsers, which account administration requires.",
                "users-denied");

            return;
        }

        _status = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize = 11.5,
            Margin = new Thickness(0, 8, 0, 0),
            Foreground = Application.Current?.TryFindResource("BeepTextSecondaryBrush") as Brush ?? Brushes.Gray,
        };

        var panel = new StackPanel();
        panel.Children.Add(Heading("Accounts"));
        panel.Children.Add(Caption(
            "The accounts of this installation's database. A new or reset password is provisional: the person is " +
            "asked to choose their own at the next sign-in. The last active administrator cannot be disabled or " +
            "lose its administrator role."));

        var actions = new WrapPanel { Margin = new Thickness(0, 6, 0, 10) };
        actions.Children.Add(Button("New account…", OnNewAccount));
        actions.Children.Add(Button("Refresh", (_, _) => Reload()));
        actions.Children.Add(_rolePicker);
        panel.Children.Add(actions);

        panel.Children.Add(_rows);
        panel.Children.Add(_status);

        Named(_rolePicker, "Roles");
        ShowContent(panel);

        Reload();
    }

    /// <summary>Reads the accounts and the roles, and redraws the list.</summary>
    private async void Reload()
    {
        try
        {
            var administration = Administration();

            var roles = await administration.ListRolesAsync().ConfigureAwait(true);

            if (roles.Success && roles.Value is not null)
            {
                _rolePicker.ItemsSource = roles.Value;
                _rolePicker.SelectedItem = roles.Value.FirstOrDefault();
            }

            var listed = await administration.ListAsync().ConfigureAwait(true);

            if (!listed.Success || listed.Value is null)
            {
                ShowState(OgViewState.Failed, listed.Message ?? "The accounts could not be read.", listed.Reference);
                return;
            }

            _rows.Children.Clear();

            foreach (var user in listed.Value)
            {
                _rows.Children.Add(Row(user));
            }

            _status.Text = $"{listed.Value.Count} account(s).";
            ShowState(OgViewState.Ready, string.Empty);
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reading the account directory",
                "no accounts are shown",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The accounts could not be read.", reference);
        }
    }

    private FrameworkElement Row(OgManagedUser user)
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var details = new StackPanel();
        details.Children.Add(new TextBlock
        {
            Text = $"{user.DisplayName} ({user.UserName})",
            FontWeight = FontWeights.SemiBold,
            Foreground = Application.Current?.TryFindResource("BeepTextPrimaryBrush") as Brush ?? Brushes.Black,
        });

        details.Children.Add(Caption(
            (user.IsActive ? "active" : "disabled")
            + " · roles: " + (user.Roles.Count == 0 ? "none" : string.Join(", ", user.Roles))
            + (user.IsLocked ? " · locked until " + user.LockedUntil!.Value.ToLocalTime().ToString("HH:mm") : string.Empty)
            + (user.MustChangePassword ? " · must choose a password at next sign-in" : string.Empty)));

        row.Children.Add(details);

        var buttons = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };

        if (user.IsLocked)
        {
            buttons.Children.Add(Button("Unlock", async (_, _) =>
                await ApplyAsync(
                    () => Administration().ClearLockoutAsync(user.UserName, _application.Session.UserId),
                    "The account is unlocked.").ConfigureAwait(true)));
        }

        buttons.Children.Add(Button("Reset password…", async (_, _) =>
        {
            var password = OgAccountDialog.AskPassword(
                Window.GetWindow(this),
                $"Set a provisional password for '{user.UserName}'",
                "The person must choose their own at the next sign-in.");

            if (password is null)
            {
                return;
            }

            await ApplyAsync(
                () => Administration().ResetPasswordAsync(user.UserName, password, _application.Session.UserId),
                "The password was reset.").ConfigureAwait(true);
        }));

        buttons.Children.Add(Button(user.IsActive ? "Disable" : "Enable", async (_, _) =>
            await ApplyAsync(
                () => Administration().SetActiveAsync(user.UserName, !user.IsActive, _application.Session.UserId),
                user.IsActive ? "The account is disabled." : "The account is active.").ConfigureAwait(true)));

        buttons.Children.Add(Button("Grant role", async (_, _) =>
        {
            if (_rolePicker.SelectedItem is not string role)
            {
                return;
            }

            await ApplyAsync(
                () => Administration().GrantRoleAsync(user.UserName, role, _application.Session.UserId),
                $"'{role}' granted.").ConfigureAwait(true);
        }));

        buttons.Children.Add(Button("Revoke role", async (_, _) =>
        {
            if (_rolePicker.SelectedItem is not string role)
            {
                return;
            }

            await ApplyAsync(
                () => Administration().RevokeRoleAsync(user.UserName, role, _application.Session.UserId),
                $"'{role}' revoked.").ConfigureAwait(true);
        }));

        Grid.SetColumn(buttons, 1);
        row.Children.Add(buttons);

        return row;
    }

    private async void OnNewAccount(object sender, RoutedEventArgs e)
    {
        var request = OgAccountDialog.AskNewAccount(Window.GetWindow(this), (IReadOnlyList<string>?)_rolePicker.ItemsSource);

        if (request is null)
        {
            return;
        }

        await ApplyAsync(
            async () =>
            {
                var created = await Administration()
                    .CreateAsync(
                        request.UserName,
                        request.DisplayName,
                        request.Password,
                        request.Roles,
                        _application.Session.UserId)
                    .ConfigureAwait(true);

                return created.Success
                    ? OgResult.Ok($"'{request.UserName}' created; it must choose a password at the next sign-in.")
                    : OgResult.Failed(created.Message ?? "The account was not created.", created.Reference ?? "user-create-failed");
            },
            "The account was created.").ConfigureAwait(true);
    }

    /// <summary>Runs one change, reports its outcome, and redraws.</summary>
    private async Task ApplyAsync(Func<Task<OgResult>> change, string success)
    {
        try
        {
            var result = await change().ConfigureAwait(true);

            if (!result.Success)
            {
                _application.Notifications.Warn(result.Message ?? "The change was refused.", nameof(UsersView));
                _status.Text = result.Message ?? "The change was refused.";
                return;
            }

            _status.Text = result.Message ?? success;
            _application.Notifications.Success(result.Message ?? success);
            Reload();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "changing an account",
                "the account was not changed",
                FailureSeverity.Error);

            _status.Text = $"The change did not complete. Reference: {reference}";
        }
    }

    /// <summary>The administration service over this installation's database.</summary>
    private IOgUserAdministration Administration() =>
        new BeepDmUserAdministration(
            new BeepDmDatabaseIdentity(
                _application.Services.GetRequiredService<Services.OgBeepLifetime>().Host,
                _application.Diagnostics,
                _application.SetupState?.ConnectionName ?? OgPaths.ConnectionName),
            _application.Diagnostics);

    private static Button Button(string text, RoutedEventHandler click)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(10, 4, 10, 4),
            Margin = new Thickness(0, 0, 6, 4),
            MinWidth = 96,
        };

        button.Click += click;
        return button;
    }

    private static TextBlock Heading(string text) => new()
    {
        Text = text,
        FontSize = 20,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 0, 0, 6),
        Foreground = Application.Current?.TryFindResource("BeepTextPrimaryBrush") as Brush ?? Brushes.Black,
    };

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        FontSize = 11.5,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 0, 0, 6),
        Foreground = Application.Current?.TryFindResource("BeepTextSecondaryBrush") as Brush ?? Brushes.Gray,
    };
}
