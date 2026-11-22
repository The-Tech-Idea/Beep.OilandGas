using System.Windows;
using System.Windows.Controls;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Setup;
using TheTechIdea.Beep.OilandGas.Desktop.Theme;
using TheTechIdea.Beep.Wpf.Data.Integrated.Dialogs;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;
using TheTechIdea.Beep.WPF.Controls.Styling;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// FND-04, FND-05 and VSB-02: the design-system picker, the interface language, where the application
/// writes, and the wellbore environment with its findings and a reload action.
/// </summary>
public sealed class SettingsView : OgPageBase
{
    private const int MaxFindingsShown = 8;

    private readonly OgApplication _application;
    private readonly TextBlock _skinState;
    private readonly TextBlock _environmentSummary;
    private readonly TextBlock _environmentFindings;
    private readonly TextBlock _companySummary;
    private readonly TextBlock _companyState;
    private readonly Button _editCompany;
    private readonly Button _editEnvironment;
    private readonly Button _backupNow;
    private readonly Button _restoreBackup;
    private readonly TextBlock _backupSummary;
    private bool _suppressSkinChange;

    public SettingsView(OgApplication application)
        : base(application, "settings")
    {
        _application = application;

        var panel = new StackPanel();
        panel.Children.Add(Heading(application.Loc.Get("Settings.Appearance")));

        panel.Children.Add(Caption(application.Loc.Get("Settings.Skin")));
        var skinPicker = new ComboBox
        {
            ItemsSource = OgSkins.All,
            SelectedItem = OgSkins.All.FirstOrDefault(option => option.Style == OgSkins.Current),
            MinWidth = 260,
            Margin = new Thickness(0, 2, 0, 6),
        };
        skinPicker.SelectionChanged += OnSkinChanged;
        panel.Children.Add(skinPicker);

        _skinState = Caption($"Active: {OgSkins.CurrentDisplayName} ({OgSkins.All.Count} available)");
        panel.Children.Add(_skinState);

        panel.Children.Add(Heading(application.Loc.Get("Settings.Language")));
        var languagePicker = new ComboBox
        {
            ItemsSource = application.Loc.Cultures,
            DisplayMemberPath = nameof(OgCultureOption.DisplayName),
            SelectedItem = application.Loc.Cultures.FirstOrDefault(option =>
                string.Equals(option.Name, application.Loc.CultureName, StringComparison.OrdinalIgnoreCase)),
            MinWidth = 260,
            Margin = new Thickness(0, 2, 0, 12),
        };
        languagePicker.SelectionChanged += OnLanguageChanged;
        panel.Children.Add(languagePicker);

        // Company settings: read by everyone, changed only by an administrator with the role. In a company
        // database the change goes to the shared record, so it reaches every user's next start.
        panel.Children.Add(Heading("Company settings"));

        _companySummary = Caption(string.Empty);
        _companyState = Caption(string.Empty);
        panel.Children.Add(_companySummary);

        var companyActions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 6) };
        _editCompany = new Button
        {
            Content = "Edit company settings…",
            Padding = new Thickness(12, 5, 12, 5),
            IsEnabled = CanEditCompany,
        };
        _editCompany.Click += OnEditCompany;
        companyActions.Children.Add(_editCompany);
        panel.Children.Add(companyActions);
        panel.Children.Add(_companyState);

        if (!CanEditCompany)
        {
            _companyState.Text =
                "Your roles do not include Admin.ConfigureSystem, which changing the company settings requires.";
        }

        // VSB-02: the environment, its findings and a reload action.
        panel.Children.Add(Heading(application.Loc.Get("Settings.Environment")));

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 8) };
        var reload = new Button
        {
            Content = application.Loc.Get("Settings.ReloadEnvironment"),
            Padding = new Thickness(12, 5, 12, 5),
            Margin = new Thickness(0, 0, 8, 0),
        };
        reload.Click += OnReloadEnvironment;

        var checkResources = new Button
        {
            Content = application.Loc.Get("Settings.CheckResources"),
            Padding = new Thickness(12, 5, 12, 5),
        };
        checkResources.Click += OnCheckResources;

        actions.Children.Add(reload);
        actions.Children.Add(checkResources);

        // VSB-02 AC-4: a country preset can be applied and is labelled as a preset.
        var presetPicker = new ComboBox
        {
            ItemsSource = CountryPresets.Codes,
            SelectedItem = CountryPresets.Codes.FirstOrDefault(),
            MinWidth = 90,
            Margin = new Thickness(12, 0, 8, 0),
        };
        var applyPreset = new Button
        {
            Content = application.Loc.Get("Settings.ApplyPreset"),
            Padding = new Thickness(12, 5, 12, 5),
        };
        applyPreset.Click += (_, _) => ApplyPreset(presetPicker.SelectedItem as string);

        actions.Children.Add(presetPicker);
        actions.Children.Add(applyPreset);

        // SETUP-10 AC-7: the shared environment is one document; the SystemAdmin edits it and it is written with the
        // revision it read, so every user's next start picks it up.
        var editEnvironment = new Button
        {
            Content = "Edit the shared environment…",
            Padding = new Thickness(12, 5, 12, 5),
            Margin = new Thickness(8, 0, 0, 0),
            IsEnabled = CanEditCompany,
        };
        editEnvironment.Click += OnEditEnvironment;
        _editEnvironment = editEnvironment;

        actions.Children.Add(editEnvironment);
        panel.Children.Add(actions);

        _environmentSummary = Caption(string.Empty);
        _environmentFindings = Caption(string.Empty);
        panel.Children.Add(_environmentSummary);
        panel.Children.Add(_environmentFindings);

        panel.Children.Add(Heading(application.Loc.Get("Settings.Storage")));
        panel.Children.Add(LocalBody($"Data root: {OgPaths.DataRoot}"));
        panel.Children.Add(LocalBody($"Failure store: {OgPaths.FailureStorePath}"));
        panel.Children.Add(LocalBody($"Wellbore environment: {OgPaths.WellboreEnvironmentPath}"));
        panel.Children.Add(LocalBody($"Logs: {OgPaths.LogDirectory}"));
        panel.Children.Add(LocalBody($"Inside the data root: {OgPaths.IsWritableLocation(OgPaths.FailureStorePath)}"));
        panel.Children.Add(LocalBody("Nothing is written beside the executable."));

        // SETUP-12: the installation's database, copied and restored. A server database says so and points at its DBA.
        panel.Children.Add(Heading("Backup and restore"));

        _backupSummary = Caption(string.Empty);
        panel.Children.Add(_backupSummary);

        var backupActions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 6) };

        _backupNow = new Button
        {
            Content = "Back up now…",
            Padding = new Thickness(12, 5, 12, 5),
            Margin = new Thickness(0, 0, 8, 0),
            IsEnabled = CanEditCompany,
        };
        _backupNow.Click += OnBackupNow;

        _restoreBackup = new Button
        {
            Content = "Restore a backup…",
            Padding = new Thickness(12, 5, 12, 5),
            IsEnabled = CanEditCompany,
        };
        _restoreBackup.Click += OnRestoreBackup;

        backupActions.Children.Add(_backupNow);
        backupActions.Children.Add(_restoreBackup);
        panel.Children.Add(backupActions);

        Named(skinPicker, "Skin");
        Named(languagePicker, "Language");
        Named(presetPicker, "Regional preset");
        ShowContent(panel);
        RefreshCompany();
        RefreshEnvironment();
        RefreshBackups();
    }

    /// <summary>
    /// Only an account the database allows prepares the installation's settings: the system administrator, or a role
    /// that carries <c>Admin.ConfigureSystem</c>. Everyone else reads.
    /// </summary>
    private bool CanEditCompany => _application.Permissions.CanConfigureSystem;

    /// <summary>
    /// Shows the company record and where it comes from — this workstation's own setup file, or the company
    /// database every user shares.
    /// </summary>
    private void RefreshCompany()
    {
        var state = _application.SetupState;

        if (state is null)
        {
            _companySummary.Text = "This installation has no company record yet.";
            return;
        }

        var source = state.UsesCompanyDatabase
            ? $"Company database '{state.ConnectionName}'"
            : "This workstation's setup file";

        var lastRead = state.SharedSettingsLoadedAtUtc is { } at
            ? $" · read at {at.ToLocalTime():g}"
            : string.Empty;

        _companySummary.Text =
            $"{state.CompanyName} ({state.CompanyCode}){Environment.NewLine}" +
            $"Country {Display(state.CountryCode, OgSetupCatalog.Countries)} · " +
            $"units {Display(state.UnitSystemCode, OgSetupCatalog.UnitSystems)} · " +
            $"currency {state.BaseCurrencyCode} · " +
            $"fiscal year starts {state.FiscalYearStartMonth}{Environment.NewLine}" +
            $"Source: {source}{lastRead}";

        if (_application.Settings is { Success: false } failure && state.UsesCompanyDatabase)
        {
            _companyState.Text = failure.Message ?? "The shared settings could not be read at startup.";
        }
        else if (!CanEditCompany)
        {
            _companyState.Text =
                "Your roles do not include Admin.ConfigureSystem, which changing the company settings requires.";
        }
        else
        {
            _companyState.Text = state.UsesCompanyDatabase
                ? "A change is written to the company database, so every user reads it at their next start."
                : "A change is written to this workstation's own record.";
        }
    }

    private static string Display(string? code, IReadOnlyList<OgSetupChoice> choices) =>
        string.IsNullOrWhiteSpace(code)
            ? "not recorded"
            : OgSetupCatalog.Find(choices, code)?.DisplayName ?? code!;

    private async void OnEditCompany(object sender, RoutedEventArgs e)
    {
        var state = _application.SetupState;

        if (state is null || !CanEditCompany)
        {
            return;
        }

        var window = new OgCompanySettingsWindow(
            _application,
            new Data.Settings.OgInstallationSettings(
                state.CompanyName,
                state.CompanyCode,
                state.CountryCode,
                state.CultureName,
                state.TimeZoneId,
                state.BaseCurrencyCode,
                state.FiscalYearStartMonth,
                state.CompanyTypeCode,
                state.UnitSystemCode,
                state.DefaultFieldName,
                state.AdministratorUserName,
                state.CompletedAtUtc))
        {
            Owner = Window.GetWindow(this),
        };

        if (window.ShowDialog() != true || window.Result is not { } edited)
        {
            return;
        }

        _editCompany.IsEnabled = false;

        try
        {
            var written = await SaveCompanyAsync(state, edited, window.ChangesEnvironment).ConfigureAwait(true);

            if (!written.Success)
            {
                _companyState.Text = written.Message;
                return;
            }

            _application.Notifications.Success("Settings.CompanySaved");
            _companyState.Text = written.Message ?? "The company settings were saved.";
            RefreshCompany();
            RefreshEnvironment();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "saving the company settings",
                "the previous company record is still in force",
                FailureSeverity.Error);

            _companyState.Text = $"The company settings were not saved. Reference: {reference}";
        }
        finally
        {
            _editCompany.IsEnabled = CanEditCompany;
        }
    }

    /// <summary>Shows whether this installation can be backed up here, and how many copies exist.</summary>
    private async void RefreshBackups()
    {
        try
        {
            var backup = _application.Backup;

            if (!backup.CanBackUp)
            {
                _backupSummary.Text =
                    "This installation's database is on a server; its backup and restore are the database administrator's to perform.";
                _backupNow.IsEnabled = false;
                _restoreBackup.IsEnabled = false;
                return;
            }

            var listed = await backup.ListAsync().ConfigureAwait(true);

            if (!listed.Success || listed.Value is null)
            {
                _backupSummary.Text = listed.Message ?? "The backups could not be listed.";
                return;
            }

            _backupSummary.Text = listed.Value.Count == 0
                ? $"No backup has been taken. Copies are written to {backup.BackupDirectory}."
                : $"{listed.Value.Count} backup(s) in {backup.BackupDirectory}. The newest is "
                  + $"{listed.Value[0].TakenUtc.ToLocalTime():g} ({listed.Value[0].Bytes / 1024} KB).";
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "listing the installation's backups",
                "no backups are shown",
                FailureSeverity.Degraded);

            _backupSummary.Text = $"The backups could not be listed. Reference: {reference}";
        }
    }

    private async void OnBackupNow(object sender, RoutedEventArgs e)
    {
        try
        {
            _backupNow.IsEnabled = false;

            var taken = await _application.Backup
                .BackupAsync(string.IsNullOrWhiteSpace(_application.Session.UserId)
                    ? OgPaths.SystemUserId
                    : _application.Session.UserId)
                .ConfigureAwait(true);

            _backupSummary.Text = taken.Success && taken.Value is not null
                ? $"Backup taken: {Path.GetFileName(taken.Value.Path)} ({taken.Value.Bytes / 1024} KB)."
                : taken.Message ?? "The backup was not taken.";

            if (taken.Success)
            {
                _application.Notifications.Success("Settings.BackupTaken");
            }
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "taking a backup",
                "no backup was taken",
                FailureSeverity.Error);

            _backupSummary.Text = $"The backup was not taken. Reference: {reference}";
        }
        finally
        {
            _backupNow.IsEnabled = CanEditCompany;
            RefreshBackups();
        }
    }

    private async void OnRestoreBackup(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Choose a backup to restore",
            Filter = "Database backups (*.db)|*.db|All files (*.*)|*.*",
            CheckFileExists = true,
            InitialDirectory = _application.Backup.BackupDirectory,
        };

        if (dialog.ShowDialog(Window.GetWindow(this)) != true)
        {
            return;
        }

        // Restoring overwrites the live database: the confirmation names the file and says so. The product's own alert
        // dialog is used - MessageBox is reserved for the one startup failure that has no window to toast into.
        var confirm = new WpfAlertDialog
        {
            Owner = Window.GetWindow(this),
        };

        confirm.Configure(
            "Restore a backup",
            $"Restore '{Path.GetFileName(dialog.FileName)}' over this installation's database?\n\n"
            + "Everything recorded since that backup is lost.",
            AlertStyle.Caution,
            button1Text: "Restore",
            button2Text: "Cancel",
            button3Text: null);

        confirm.ShowDialog();

        if (confirm.Result != AlertResult.Button1)
        {
            return;
        }

        try
        {
            _restoreBackup.IsEnabled = false;

            var restored = await _application.Backup
                .RestoreAsync(
                    dialog.FileName,
                    string.IsNullOrWhiteSpace(_application.Session.UserId) ? OgPaths.SystemUserId : _application.Session.UserId)
                .ConfigureAwait(true);

            _backupSummary.Text = restored.Success
                ? restored.Message ?? "The database was restored; sign in again to read it."
                : restored.Message ?? "The database was not restored.";

            if (restored.Success)
            {
                _application.Notifications.Success("Settings.DatabaseRestored");
            }
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "restoring a backup",
                "the database was not restored",
                FailureSeverity.Critical);

            _backupSummary.Text = $"The database was not restored. Reference: {reference}";
        }
        finally
        {
            _restoreBackup.IsEnabled = CanEditCompany;
            RefreshBackups();
        }
    }

    /// <summary>
    /// Edits the shared environment document and writes it with the revision that was read, then applies it to the
    /// running application. The save writes the audit row; the page only reports it.
    /// </summary>
    private async void OnEditEnvironment(object sender, RoutedEventArgs e)
    {
        var state = _application.SetupState;

        if (state is null || !CanEditCompany)
        {
            return;
        }

        try
        {
            var shared = _application.SharedSettings(state.ConnectionName);
            var read = await shared.ReadAsync().ConfigureAwait(true);

            if (!read.Success || read.Value is null)
            {
                _environmentSummary.Text = read.Message ?? "The shared environment could not be read.";
                return;
            }

            var dialog = new OgEnvironmentDialog(_application, read.Value.ToolsEnvironmentJson)
            {
                Owner = Window.GetWindow(this),
            };

            if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.DocumentJson))
            {
                return;
            }

            var written = await shared
                .WriteToolsEnvironmentAsync(
                    dialog.DocumentJson!,
                    read.Value.ToolsRevision,
                    string.IsNullOrWhiteSpace(_application.Session.UserId)
                        ? OgPaths.SystemUserId
                        : _application.Session.UserId)
                .ConfigureAwait(true);

            if (!written.Success)
            {
                _environmentSummary.Text = written.Message ?? "The shared environment was not saved.";
                return;
            }

            var applied = _application.WellboreLifetime.LoadSharedDocument(dialog.DocumentJson);

            _environmentSummary.Text = applied.Success
                ? "The shared environment was saved and applied; every user reads it at their next start."
                : applied.Message ?? "The environment was saved but could not be applied.";

            _application.Notifications.Success("Settings.EnvironmentSaved");
            RefreshEnvironment();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "editing the shared environment",
                "the previous environment is still active",
                FailureSeverity.Error);

            _environmentSummary.Text = $"The shared environment was not changed. Reference: {reference}";
        }
    }

    /// <summary>
    /// Writes the edited company record where the deployment keeps it: the shared record in a company database
    /// (revision-checked, so a second administrator cannot overwrite a change they did not see), or this
    /// workstation's own record otherwise.
    /// </summary>
    private async Task<Core.OgResult> SaveCompanyAsync(
        Setup.OgSetupState state,
        Data.Settings.OgInstallationSettings edited,
        bool environmentChanged)
    {
        var actor = string.IsNullOrWhiteSpace(_application.Session.UserId)
            ? Bootstrapping.OgPaths.SystemUserId
            : _application.Session.UserId;

        var record = edited with
        {
            RecordedBy = string.IsNullOrWhiteSpace(edited.RecordedBy) ? actor : edited.RecordedBy,
            RecordedAtUtc = DateTimeOffset.UtcNow,
        };

        if (state.UsesCompanyDatabase)
        {
            var shared = _application.SharedSettings(state.ConnectionName);
            var read = await shared.ReadAsync().ConfigureAwait(true);

            if (!read.Success || read.Value is null)
            {
                return Core.OgResult.Failed(
                    read.Message ?? "The shared settings could not be read, so nothing was written.",
                    read.Reference ?? Data.Settings.OgSharedSetupCodes.ReadFailed);
            }

            var written = await shared
                .WriteInstallationAsync(record, read.Value.InstallationRevision, actor)
                .ConfigureAwait(true);

            if (!written.Success)
            {
                return Core.OgResult.Failed(
                    written.Message ?? "The company record was not written.",
                    written.Reference ?? Data.Settings.OgSharedSetupCodes.WriteFailed);
            }

            if (environmentChanged && read.Value.ToolsEnvironmentJson is { Length: > 0 })
            {
                var document = BuildEnvironmentDocument(record.CountryCode, record.UnitSystemCode);

                if (document is not null)
                {
                    var environment = await shared
                        .WriteToolsEnvironmentAsync(document, read.Value.ToolsRevision, actor)
                        .ConfigureAwait(true);

                    if (!environment.Success)
                    {
                        return Core.OgResult.Failed(
                            environment.Message ?? "The company record was saved but the shared environment was not.",
                            environment.Reference ?? Data.Settings.OgSharedSetupCodes.WriteFailed);
                    }
                }
            }

            _application.AdoptCompany(record);

            if (environmentChanged)
            {
                _application.WellboreLifetime.LoadSharedDocument(
                    Setup.OgSetupCatalog.BuildEnvironmentDocument(record.CountryCode, record.UnitSystemCode));
            }

            return Core.OgResult.Ok(
                $"Saved to the company database '{state.ConnectionName}'; every user reads it at their next start.");
        }

        _application.AdoptCompany(record);

        if (environmentChanged && record.CountryCode.Length == 2)
        {
            _application.WellboreLifetime.ApplyCountryPreset(record.CountryCode);
        }

        return Core.OgResult.Ok("Saved to this workstation's record.");
    }

    /// <summary>The environment document for a country and unit system, resolved before it is stored.</summary>
    private static string? BuildEnvironmentDocument(string countryCode, string unitSystem) =>
        Setup.OgSetupCatalog.BuildEnvironmentDocument(countryCode, unitSystem);

    private void RefreshEnvironment()
    {
        try
        {
            var snapshot = _application.Environment.Current();
            var availability = _application.Environment.ToolboxAvailability();
            var usable = availability.Count(pair => pair.Value.Count == 0);

            var source = snapshot.UsingDefaults
                ? _application.Loc.Get("Settings.EnvironmentDefaults")
                : snapshot.UsingCountryPreset
                    ? $"{_application.Loc.Get("Settings.CountryPreset")}: {snapshot.CountryCode}"
                    : _application.Loc.Get("Settings.EnvironmentFile", snapshot.Path ?? string.Empty);

            _environmentSummary.Text =
                source
                + "   |   "
                + _application.Loc.Get(
                    "Settings.EnvironmentSummary",
                    string.IsNullOrWhiteSpace(snapshot.CountryCode) ? "—" : snapshot.CountryCode,
                    snapshot.UnitCount,
                    snapshot.RegisteredToolboxes.Count,
                    snapshot.MissingToolboxes.Count)
                + "   |   "
                + _application.Loc.Get("Settings.ToolboxesUsable", usable, availability.Count)
                + (_application.WellboreLifetime.LastChangeSummary is { Length: > 0 } change
                    ? Environment.NewLine + change
                    : string.Empty);

            var findings = snapshot.Findings.Count;
            _environmentFindings.Text = findings == 0
                ? _application.Loc.Get("Settings.EnvironmentFindings", 0)
                : _application.Loc.Get("Settings.EnvironmentFindings", findings)
                  + Environment.NewLine
                  + string.Join(
                      Environment.NewLine,
                      snapshot.Findings
                          .Take(MaxFindingsShown)
                          .Select(finding => $"[{finding.Severity}] {finding.Source}: {finding.Message}"));
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reading the wellbore environment findings",
                "the environment section shows no findings",
                FailureSeverity.Error);

            _environmentSummary.Text = "The environment could not be read.";
            _environmentFindings.Text = $"Reference: {reference}";
        }
    }

    private void ApplyPreset(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return;
        }

        try
        {
            var result = _application.WellboreLifetime.ApplyCountryPreset(countryCode);

            if (!result.Success)
            {
                _application.Notifications.Publish(new Diagnostics.OgDiagnosticEvent(
                    result.Reference ?? "country-preset-failed",
                    FailureSeverity.Error,
                    $"applying the country preset {countryCode}"));

                ShowState(OgViewState.Failed, result.Message ?? "The preset was not applied.", result.Reference);
                return;
            }

            _application.Notifications.Success("Settings.PresetApplied", countryCode);
            ShowState(OgViewState.Ready, string.Empty);
            RefreshEnvironment();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                $"applying the country preset {countryCode}",
                "the previous environment is still active",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The preset was not applied.", reference);
        }
    }

    private void OnReloadEnvironment(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = _application.ReloadWellboreEnvironment();

            if (!result.Success)
            {
                _application.Notifications.Publish(new Diagnostics.OgDiagnosticEvent(
                    result.Reference ?? "environment-reload-failed",
                    FailureSeverity.Error,
                    "reloading the wellbore environment"));

                ShowState(OgViewState.Failed, result.Message ?? "The environment was not reloaded.", result.Reference);
                return;
            }

            _application.Notifications.Success("Settings.EnvironmentReloaded");
            ShowState(OgViewState.Ready, string.Empty);
            RefreshEnvironment();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reloading the wellbore environment",
                "the previous environment is still active",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The environment was not reloaded.", reference);
        }
    }

    private void OnCheckResources(object sender, RoutedEventArgs e)
    {
        try
        {
            var problems = _application.Environment.CheckResources();

            _application.Notifications.Success("Settings.ResourcesChecked", problems.Count);

            _environmentFindings.Text = problems.Count == 0
                ? _application.Loc.Get("Settings.ResourcesChecked", 0)
                : _application.Loc.Get("Settings.ResourcesChecked", problems.Count)
                  + Environment.NewLine
                  + string.Join(Environment.NewLine, problems.Take(MaxFindingsShown).Select(problem => problem.Message));
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "checking the toolbox resources",
                "the resource check produced no result",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The toolbox resources could not be checked.", reference);
        }
    }

    private static TextBlock Heading(string text) => new()
    {
        Text = text,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 12, 0, 4),
    };

    private void OnSkinChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSkinChange || sender is not ComboBox { SelectedItem: OgSkinOption option })
        {
            return;
        }

        try
        {
            OgSkins.Apply(option.Style);
            _skinState.Text = $"Active: {OgSkins.CurrentDisplayName} ({OgSkins.All.Count} available)";
            _application.Notifications.Success("Settings.SkinApplied", OgSkins.CurrentDisplayName);
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                $"applying the design system '{option.DisplayName}'",
                "the previous design system is still active",
                FailureSeverity.Error);

            _suppressSkinChange = true;
            try
            {
                if (sender is ComboBox picker)
                {
                    picker.SelectedItem = OgSkins.All.FirstOrDefault(item => item.Style == OgSkins.Current);
                }
            }
            finally
            {
                _suppressSkinChange = false;
            }

            ShowState(OgViewState.Failed, "The design system could not be applied.", reference);
        }
    }

    private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox { SelectedItem: OgCultureOption option })
        {
            return;
        }

        _application.Loc.SetCulture(option.Name);

        // The shell owns re-applying direction and rebuilding titles; it is listening to LanguageChanged.
        var window = Window.GetWindow(this);
        if (window is not null)
        {
            window.FlowDirection = _application.Loc.IsRightToLeft
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;
        }
    }

    private static TextBlock LocalBody(string text) => new()
    {
        Text = text,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 0, 0, 4),
        Foreground = (System.Windows.Media.Brush?)Application.Current?.TryFindResource("BeepTextPrimaryBrush")
                     ?? System.Windows.Media.Brushes.Black,
    };

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        TextWrapping = TextWrapping.Wrap,
        FontSize = 11.5,
        Margin = new Thickness(0, 0, 0, 8),
        Foreground = (System.Windows.Media.Brush?)Application.Current?.TryFindResource("BeepTextSecondaryBrush")
                     ?? System.Windows.Media.Brushes.Gray,
    };
}
