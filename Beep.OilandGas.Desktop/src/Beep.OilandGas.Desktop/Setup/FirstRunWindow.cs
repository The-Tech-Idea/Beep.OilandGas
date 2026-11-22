using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;
using TheTechIdea.Beep.OilandGas.Desktop.Services;
using TheTechIdea.Beep.Wpf.Data.Integrated.Controls;
using TheTechIdea.Beep.Wpf.Data.Integrated.Services;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Setup;

/// <summary>
/// FND-07: the first-run wizard. It records the company and creates the one administrator this installation
/// will have; nothing else opens until it succeeds.
/// </summary>
/// <remarks>
/// <para>
/// The six steps mirror the ERP's setup: welcome and deployment, database, company, the database modules, the
/// administrator, and a review that performs the work. What changes is the content: a country is one of the
/// wellbore environment's own presets, a unit system is one the environment accepts, the modules are the PPDM
/// module set, and the company record is stored where the deployment says it belongs.
/// </para>
/// <para>
/// Two deployments are offered. <b>This workstation</b> keeps a local database file and the company record beside
/// it. <b>Company database</b> uses a server database, and the company record, the module set and the OilandGasTools
/// environment are written into that database (<c>SYSTEM_SETUP_RECORD</c>), so every user of it reads the same
/// settings. A workstation joining an existing company database adopts its record instead of writing a second one.
/// </para>
/// </remarks>
public sealed class FirstRunWindow : Window
{
    private readonly OgApplication _application;
    /// <summary>
    /// The step rail: completed, current and upcoming, in one list. Nielsen Norman's wizard guidance asks for exactly
    /// this - show the steps, highlight the current one, and keep the order - and a plain list of labelled rows is
    /// readable by a screen reader, which a tab strip of glyphs is not.
    /// </summary>
    private readonly ListBox _rail = new()
    {
        Width = 230,
        Margin = new Thickness(0, 12, 16, 0),
        HorizontalContentAlignment = HorizontalAlignment.Stretch,
    };

    /// <summary>The step's content; one page at a time, like the tab it replaces.</summary>
    private readonly ContentControl _stepHost = new()
    {
        Margin = new Thickness(0, 12, 0, 0),
        HorizontalContentAlignment = HorizontalAlignment.Stretch,
        VerticalContentAlignment = VerticalAlignment.Stretch,
    };

    private readonly List<FrameworkElement> _pages = [];
    private int _stepIndex;
    private bool _refreshingRail;

    // Welcome.
    private readonly RadioButton _personalMode = new()
    {
        Content = "This workstation only — a local database file",
        GroupName = "Deployment",
        Margin = new Thickness(0, 8, 0, 0),
    };

    private readonly RadioButton _companyMode = new()
    {
        Content = "A company database — one database and one set of settings for every user",
        GroupName = "Deployment",
        Margin = new Thickness(0, 6, 0, 0),
    };

    private readonly TextBox _licencePath = new() { MinWidth = 320 };
    private readonly TextBlock _licenceStatus;
    private readonly CheckBox _authorised = new()
    {
        Content = "I am authorised to initialise this database, and I have a backup of it.",
        Margin = new Thickness(0, 16, 0, 0),
    };

    // Database.
    private BeepWpfConnectionCatalog? _catalog;
    private WpfDataManagementContext? _managementContext;
    private readonly StackPanel _databasePanel = new();
    private readonly TextBlock _databaseStatus;
    private readonly ComboBox _provider = new() { MinWidth = 160, Margin = new Thickness(0, 2, 8, 8) };
    private readonly TextBox _host = new() { MinWidth = 200, Margin = new Thickness(0, 2, 8, 8) };
    private readonly TextBox _port = new() { Width = 80, Margin = new Thickness(0, 2, 8, 8) };
    private readonly TextBox _database = new() { MinWidth = 160, Margin = new Thickness(0, 2, 8, 8) };
    private readonly TextBox _userId = new() { MinWidth = 140, Margin = new Thickness(0, 2, 8, 8) };
    private readonly PasswordBox _password = new() { MinWidth = 140, Margin = new Thickness(0, 2, 8, 8) };
    private readonly CheckBox _windowsAuth = new() { Content = "Windows authentication", Margin = new Thickness(0, 2, 12, 8) };
    private readonly CheckBox _trustCertificate = new() { Content = "Trust the server certificate", Margin = new Thickness(0, 2, 0, 8) };
    private bool _catalogBuilt;

    // Company.
    private readonly TextBox _companyName = new() { MinWidth = 240 };
    private readonly TextBox _companyCode = new() { MinWidth = 140 };
    private readonly ComboBox _country = new() { MinWidth = 240 };
    private readonly TextBox _currency = new() { Width = 80 };
    private readonly ComboBox _culture = new() { MinWidth = 160 };
    private readonly ComboBox _timeZone = new() { MinWidth = 260 };
    private readonly ComboBox _fiscalMonth = new() { MinWidth = 160 };
    private readonly ComboBox _companyType = new() { MinWidth = 220 };
    private readonly ComboBox _unitSystem = new() { MinWidth = 220 };
    private readonly TextBox _fieldName = new() { MinWidth = 220 };
    private readonly TextBlock _companyStatus;

    // Modules and reference data.
    private readonly StackPanel _moduleList = new();
    private readonly CheckBox _includeDemo = new()
    {
        Content = "Include the demonstration dataset (clearly labelled sample wells, leases and operations)",
        Margin = new Thickness(0, 12, 0, 0),
    };

    private readonly TextBlock _moduleStatus;

    /// <summary>
    /// SETUP-06 AC-5: the reference ("code") tables the plan will seed, named before anything is applied. This is the
    /// LOV set the SystemAdmin reviews; it is read from the same module metadata that produces the plan, so it cannot
    /// disagree with what the apply later does.
    /// </summary>
    private readonly StackPanel _referenceList = new();

    private TextBlock _referenceStatus = new();
    private readonly Button _readPlan;
    private readonly List<string> _plannedModules = [];
    private readonly List<string> _selectedModules = [];

    // Administrator.
    private readonly TextBox _adminUser = new() { MinWidth = 240 };
    private readonly TextBox _adminDisplayName = new() { MinWidth = 240 };
    private readonly PasswordBox _adminPassword = new() { MinWidth = 240 };
    private readonly PasswordBox _adminConfirm = new() { MinWidth = 240 };
    private readonly TextBlock _adminNote;

    // Review.
    private readonly TextBlock _summary;
    private readonly ProgressBar _progress = new() { Height = 6, IsIndeterminate = false, Margin = new Thickness(0, 12, 0, 8) };
    private readonly TextBlock _initializeStatus;
    private readonly Button _initialize;

    // Footer.
    private readonly TextBlock _footerStatus;
    private readonly Button _back;
    private readonly Button _next;

    private string? _selectedConnectionName;
    private string _companyRevision = string.Empty;
    private string _toolsRevision = string.Empty;
    private string _modulesRevision = string.Empty;
    private bool _adoptedExistingRecord;
    private bool _initializing;

    /// <summary>The furthest step the person has reached; the order is enforced against it.</summary>
    private int _furthestStep;

    public FirstRunWindow(OgApplication application)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));

        Title = "Beep Oil & Gas — setup";
        Width = 1080;
        Height = 820;
        MinWidth = 940;
        MinHeight = 720;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Brush("BeepBackgroundBrush", Brushes.White);
        FlowDirection = application.Loc.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        _licenceStatus = Caption(string.Empty);
        _databaseStatus = Caption(string.Empty);
        _companyStatus = Caption(string.Empty);
        _moduleStatus = Caption(string.Empty);
        _adminNote = Caption(string.Empty);
        _summary = Caption(string.Empty);
        _initializeStatus = Caption(string.Empty);
        _footerStatus = Caption(string.Empty);

        var browse = ActionButton("Browse…");
        browse.Click += OnBrowseLicence;

        var readPlan = ActionButton("Read the module plan");
        readPlan.Click += OnReadModulePlan;
        _readPlan = readPlan;

        var licenceRow = new StackPanel { Orientation = Orientation.Horizontal };
        licenceRow.Children.Add(_licencePath);
        licenceRow.Children.Add(browse);

        foreach (var step in new[]
                 {
                     BuildWelcomeStep(licenceRow),
                     BuildDatabaseStep(),
                     BuildCompanyStep(),
                     BuildModulesStep(),
                     BuildAdministratorStep(),
                     BuildReviewStep(),
                 })
        {
            }

        _rail.SelectionChanged += OnRailSelectionChanged;

        _back = ActionButton("Back");
        _back.Click += (_, _) =>
        {
            if (_stepIndex > 0)
            {
                SetStep(_stepIndex - 1);
            }
        };

        _next = ActionButton("Next");
        _next.Click += OnNext;

        var cancel = ActionButton("Cancel");
        cancel.Click += (_, _) =>
        {
            DialogResult = false;
            Close();
        };

        _initialize = ActionButton("Create the installation");
        _initialize.IsDefault = true;
        _initialize.Click += OnInitialize;

        Content = BuildLayout();
        Prefill();
        SetStep(0);
        Loaded += OnLoaded;
    }

    // ------------------------------------------------------------------ layout

    private FrameworkElement BuildLayout()
    {
        var root = new Grid { Margin = new Thickness(24) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
        header.Children.Add(new TextBlock
        {
            Text = "Set up Beep Oil & Gas",
            FontSize = 26,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brush("BeepTextPrimaryBrush", Brushes.Black),
        });
        header.Children.Add(Caption(
            "Six steps: where the data lives, the company, the modules this database holds, and the administrator " +
            "who runs it. Nothing opens until the last step succeeds."));
        root.Children.Add(header);

        var body = new Grid();
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        body.Children.Add(_rail);

        Grid.SetColumn(_stepHost, 1);
        body.Children.Add(_stepHost);

        Grid.SetRow(body, 1);
        root.Children.Add(body);

        var footer = new Grid { Margin = new Thickness(0, 16, 0, 0) };
        footer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var status = _footerStatus;
        status.VerticalAlignment = VerticalAlignment.Center;
        status.Margin = new Thickness(0, 0, 16, 0);
        footer.Children.Add(status);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        buttons.Children.Add(_back);
        buttons.Children.Add(_next);
        buttons.Children.Add(_initialize);
        buttons.Children.Add(ActionButton("Cancel", OnCancel));

        Grid.SetColumn(buttons, 1);
        footer.Children.Add(buttons);

        Grid.SetRow(footer, 2);
        root.Children.Add(footer);

        return root;
    }

    // ------------------------------------------------------------------ step 0

    private FrameworkElement BuildWelcomeStep(FrameworkElement licenceRow)
    {
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(Heading("Welcome"));
        panel.Children.Add(Caption(
            "This installation has no company yet. Choose where its data lives, then record the company and the " +
            "administrator. A company database keeps one company record and one set of Oil & Gas settings for " +
            "every user who connects to it."));

        panel.Children.Add(Heading("Where does the data live?"));
        panel.Children.Add(_personalMode);
        panel.Children.Add(Caption(
            $"A database file on this computer, at {OgPaths.LocalDatabasePath}. Its company record and settings " +
            "live beside it, on this computer only."));

        panel.Children.Add(_companyMode);
        panel.Children.Add(Caption(
            "A database on a server (SQL Server, PostgreSQL, Oracle or whatever this build has a driver for). The " +
            "company record, the PPDM module set and the Oil & Gas environment settings are stored in that " +
            "database, so every user reads the same settings. Only an administrator can change them."));

        panel.Children.Add(Heading("Licence (optional)"));
        panel.Children.Add(Caption(
            "Import a signed licence file to unlock the modules it names. Without one the product runs unlicensed " +
            "and says so."));
        panel.Children.Add(licenceRow);
        panel.Children.Add(_licenceStatus);

        panel.Children.Add(_authorised);

        return Tab("Welcome", panel);
    }

    // ------------------------------------------------------------------ step 1

    private FrameworkElement BuildDatabaseStep()
    {
        _databasePanel.Margin = new Thickness(20);
        _databasePanel.Children.Add(Heading("Database"));
        _databasePanel.Children.Add(Caption(
            "Use the connection list below for an existing connection, or enter a server's details to register a " +
            "new one. A driver that is missing can be downloaded without leaving the wizard."));

        var actions = new WrapPanel { Margin = new Thickness(0, 8, 0, 8) };
        actions.Children.Add(ActionButton("Connection wizard…", OnConnectionWizard));
        actions.Children.Add(ActionButton("Download driver…", OnDownloadDriver));
        actions.Children.Add(ActionButton("Download data source…", OnDownloadDataSource));
        actions.Children.Add(ActionButton("Test the selected connection", OnTestConnection));
        _databasePanel.Children.Add(actions);

        // The catalogue control is created when this step is first shown: creating it starts BeepDM, which is
        // the expensive part of setup and must not delay the Welcome step.
        _databasePanel.Children.Add(new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brush("BeepBorderBrush", Brushes.LightGray),
            CornerRadius = new CornerRadius(6),
            Height = 300,
            Child = new TextBlock
            {
                Text = "The connection list is prepared when this step is opened…",
                Margin = new Thickness(12),
                Foreground = Brush("BeepTextSecondaryBrush", Brushes.Gray),
            },
            Name = "CatalogHost",
        });

        _databasePanel.Children.Add(Heading("Or enter a server"));
        _databasePanel.Children.Add(Caption(
            "The provider list is the drivers this installation actually found. The password is stored by the " +
            "connection store with the host's own encryption; it is never written as text."));

        var grid = new Grid { Margin = new Thickness(0, 4, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        AddLabelled(grid, 0, 0, "Provider", _provider);
        AddLabelled(grid, 0, 1, "Host", _host);
        AddLabelled(grid, 0, 2, "Port", _port);
        AddLabelled(grid, 0, 3, "Database", _database);
        AddLabelled(grid, 1, 0, "User", _userId);

        var passwordLabel = new StackPanel { Margin = new Thickness(0, 0, 8, 8) };
        passwordLabel.Children.Add(Caption("Password"));
        passwordLabel.Children.Add(_password);
        Grid.SetRow(passwordLabel, 1);
        Grid.SetColumn(passwordLabel, 1);
        grid.Children.Add(passwordLabel);

        var options = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
        options.Children.Add(_windowsAuth);
        options.Children.Add(_trustCertificate);
        Grid.SetRow(options, 2);
        Grid.SetColumnSpan(options, 4);
        grid.Children.Add(options);

        var register = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };
        register.Children.Add(ActionButton("Register and test this server", OnRegisterServer));
        Grid.SetRow(register, 2);
        Grid.SetColumn(register, 2);
        Grid.SetColumnSpan(register, 2);
        register.HorizontalAlignment = HorizontalAlignment.Right;
        grid.Children.Add(register);

        _databasePanel.Children.Add(grid);
        _databasePanel.Children.Add(_databaseStatus);

        return Tab("Database", new ScrollViewer { Content = _databasePanel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
    }

    // ------------------------------------------------------------------ step 2

    private FrameworkElement BuildCompanyStep()
    {
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(Heading("Company"));
        panel.Children.Add(Caption(
            "These values are the installation's company record. In a company database they are stored there, " +
            "once, and every workstation that joins reads them; an administrator may change them later."));

        var grid = new Grid { Margin = new Thickness(0, 12, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        for (var row = 0; row < 6; row++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        AddLabelled(grid, 0, 0, "Company name", _companyName);
        AddLabelled(grid, 0, 2, "Company code", _companyCode);
        AddLabelled(grid, 1, 0, "Country", _country);
        AddLabelled(grid, 1, 2, "Base currency (ISO)", _currency);
        AddLabelled(grid, 2, 0, "Interface language", _culture);
        AddLabelled(grid, 2, 2, "Time zone", _timeZone);
        AddLabelled(grid, 3, 0, "Fiscal year starts", _fiscalMonth);
        AddLabelled(grid, 3, 2, "Company type", _companyType);
        AddLabelled(grid, 4, 0, "Unit system", _unitSystem);
        AddLabelled(grid, 4, 2, "Default field or asset", _fieldName);

        panel.Children.Add(grid);
        panel.Children.Add(Caption(
            "The country and unit system come from the Oil & Gas environment's own presets, so the units, " +
            "printing and reference lists they select are ones this build actually ships."));

        var adopt = ActionButton("Read what this database already contains");
        adopt.HorizontalAlignment = HorizontalAlignment.Left;
        adopt.Margin = new Thickness(0, 8, 0, 0);
        adopt.Click += OnAdoptExisting;
        panel.Children.Add(adopt);
        panel.Children.Add(_companyStatus);

        return Tab("Company", new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
    }

    // ------------------------------------------------------------------ step 3

    private FrameworkElement BuildModulesStep()
    {
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(Heading("PPDM modules and reference lists"));
        panel.Children.Add(Caption(
            "The module set and its reference lists (the PPDM code tables and their lists of values) live in the " +
            "database, so every user sees the same lists. Reading the plan touches nothing; applying it in the " +
            "last step creates the schema and seeds the data."));

        _readPlan.HorizontalAlignment = HorizontalAlignment.Left;
        _readPlan.Margin = new Thickness(0, 8, 0, 8);
        panel.Children.Add(_readPlan);

        panel.Children.Add(new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brush("BeepBorderBrush", Brushes.LightGray),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            MinHeight = 180,
            Child = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = _moduleList,
            },
        });

        panel.Children.Add(_includeDemo);
        panel.Children.Add(Caption(
            "The demonstration dataset needs an explicit confirmation and is recorded as skipped when it is not " +
            "asked for. Reference lists are seeded either way."));

        _referenceStatus = Caption(
            "Read the module plan to review the reference lists (the PPDM code tables) this database will be seeded with.");

        panel.Children.Add(Heading("Reference lists"));
        panel.Children.Add(_referenceStatus);
        panel.Children.Add(new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brush("BeepBorderBrush", Brushes.LightGray),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            MaxHeight = 200,
            Child = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = _referenceList,
            },
        });

        panel.Children.Add(_moduleStatus);

        return Tab("Modules", new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
    }

    // ------------------------------------------------------------------ step 4

    private FrameworkElement BuildAdministratorStep()
    {
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(Heading("Administrator"));
        panel.Children.Add(Caption(
            "The first administrator for this installation. The password is stored as a hash, never in clear " +
            $"text, and must be at least {Core.OgPasswordPolicy.MinimumLength} characters — a sentence of several " +
            "words is a good password, and no punctuation tricks are required."));

        var grid = new Grid { Margin = new Thickness(0, 12, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        for (var row = 0; row < 4; row++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        AddLabelled(grid, 0, 0, "Full name", _adminDisplayName);
        AddLabelled(grid, 1, 0, "User name", _adminUser);
        AddLabelled(grid, 2, 0, "Password", _adminPassword);
        AddLabelled(grid, 3, 0, "Confirm password", _adminConfirm);

        panel.Children.Add(grid);
        panel.Children.Add(_adminNote);

        return Tab("Administrator", new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
    }

    // ------------------------------------------------------------------ step 5

    private FrameworkElement BuildReviewStep()
    {
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(Heading("Review"));
        panel.Children.Add(_summary);
        panel.Children.Add(_progress);
        panel.Children.Add(_initializeStatus);
        panel.Children.Add(_initialize);

        return Tab("Review", panel);
    }

    // ------------------------------------------------------------------ state

    private OgDeploymentMode Mode => _companyMode.IsChecked == true
        ? OgDeploymentMode.CompanyDatabase
        : OgDeploymentMode.Personal;

    private void Prefill()
    {
        // There is no setup file to read. On a re-run the deployment and the connection are read back from the
        // installation's own database, so a half-finished setup does not make the person answer the Database step
        // again - and a workstation already pointed at a server is not offered the personal path.
        var installation = _application.InstalledDatabase;

        _companyMode.Checked += (_, _) => UpdateAdministratorNote();
        _personalMode.Checked += (_, _) => UpdateAdministratorNote();

        if (installation is { ConnectionConfigured: true })
        {
            _companyMode.IsChecked = !installation.IsPersonal;
            _personalMode.IsChecked = installation.IsPersonal;
            _selectedConnectionName = installation.ConnectionName;
        }
        else
        {
            _personalMode.IsChecked = true;
        }

        _companyName.Text = string.Empty;
        _companyCode.Text = string.Empty;
        _currency.Text = "USD";
        _fieldName.Text = string.Empty;

        _country.ItemsSource = OgSetupCatalog.Countries;
        _country.SelectedItem = OgSetupCatalog.Find(OgSetupCatalog.Countries, "US")
                                ?? OgSetupCatalog.Countries.FirstOrDefault();
        _country.SelectionChanged += OnCountryChanged;

        _culture.ItemsSource = OgSetupCatalog.Cultures;
        _culture.SelectedItem = OgSetupCatalog.Find(OgSetupCatalog.Cultures, _application.Loc.CultureName)
                                ?? OgSetupCatalog.Cultures.FirstOrDefault();

        _timeZone.ItemsSource = OgSetupCatalog.TimeZones;
        _timeZone.SelectedItem = OgSetupCatalog.Find(OgSetupCatalog.TimeZones, TimeZoneInfo.Local.Id)
                                 ?? OgSetupCatalog.TimeZones.FirstOrDefault();

        _fiscalMonth.ItemsSource = OgSetupCatalog.FiscalMonths;
        _fiscalMonth.SelectedItem = FindFiscal(1);

        _companyType.ItemsSource = OgSetupCatalog.CompanyTypes;
        _companyType.SelectedItem = OgSetupCatalog.Find(OgSetupCatalog.CompanyTypes, "OPERATOR");

        _unitSystem.ItemsSource = OgSetupCatalog.UnitSystems;
        _unitSystem.SelectedItem = OgSetupCatalog.Find(OgSetupCatalog.UnitSystems, "oilfield");

        _adminUser.Text = string.Empty;

        UpdateAdministratorNote();

        _authorised.IsChecked = false;
        _databaseStatus.Text = string.Empty;
        _licenceStatus.Text = "No licence file selected.";
        UpdateSummary();
    }

    /// <summary>
    /// Opens the wizard where an interrupted run stopped: the progress record in the database says which step was
    /// last accepted, and the rail shows the steps before it as done.
    /// </summary>
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateSummary();

        try
        {
            var installation = _application.InstalledDatabase;

            if (installation is not { ConnectionConfigured: true })
            {
                return;
            }

            var progress = await _application
                .SharedSettings(installation.ConnectionName)
                .ReadSetupProgressAsync()
                .ConfigureAwait(true);

            if (!progress.Success || progress.Value < 0)
            {
                return;
            }

            var lastCompleted = Math.Min(progress.Value, OgSetupSteps.Count - 1);

            _furthestStep = lastCompleted;
            SetStep(OgSetupSteps.ResumeIndex(lastCompleted));

            _footerStatus.Text =
                $"Resumed: step {lastCompleted + 1} was completed before the setup stopped.";
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reading how far the setup had got",
                "the wizard starts from the first step",
                FailureSeverity.Degraded);

            _footerStatus.Text = $"The setup progress could not be read. Reference: {reference}";
        }
    }

    /// <summary>Records the step the person just accepted, so an interruption resumes after it.</summary>
    private async Task RecordProgressAsync(int completedStep)
    {
        var installation = _application.InstalledDatabase;

        if (installation is not { ConnectionConfigured: true })
        {
            return;
        }

        try
        {
            var written = await _application
                .SharedSettings(installation.ConnectionName)
                .WriteSetupProgressAsync(completedStep, _application.Session.UserId)
                .ConfigureAwait(true);

            if (!written.Success)
            {
                // The setup itself is not blocked by this: the person can still finish, they would only start again.
                _footerStatus.Text = written.Message ?? "The setup progress was not recorded.";
            }
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "recording how far the setup has got",
                "an interruption would start the setup again",
                FailureSeverity.Degraded);

            _footerStatus.Text = $"The setup progress was not recorded. Reference: {reference}";
        }
    }

    /// <summary>Where the administrator is created depends on the deployment, so the note follows the choice.</summary>
    private void UpdateAdministratorNote()
    {
        if (_companyMode.IsChecked == true)
        {
            _adminNote.Text =
                "This administrator is created in the company database, with the administrator role the security " +
                "module seeds there. Every later user signs in against that database; nothing about the company is " +
                "kept on this computer.";

            _adminPassword.IsEnabled = true;
            _adminConfirm.IsEnabled = true;
        }
        else
        {
            _adminNote.Text = string.Empty;
            _adminPassword.IsEnabled = true;
            _adminConfirm.IsEnabled = true;
        }
    }

    private OgSetupChoice? FindFiscal(int month) =>
        OgSetupCatalog.FiscalMonths.FirstOrDefault(choice =>
            string.Equals(choice.Code, month.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));

    private void OnCountryChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_country.SelectedItem is not OgSetupChoice country)
        {
            return;
        }

        // Suggest the country's currency, without claiming it is the company's: a person may report in another.
        var suggestion = CurrencyFor(country.Code);

        if (!string.IsNullOrWhiteSpace(suggestion))
        {
            _currency.Text = suggestion!;
        }

        var preset = OgSetupCatalog.UnitSystems.FirstOrDefault();

        if (preset is not null && CountryUnits(country.Code) is { Length: > 0 } units
            && OgSetupCatalog.Find(OgSetupCatalog.UnitSystems, units) is { } match)
        {
            _unitSystem.SelectedItem = match;
        }

        UpdateSummary();
    }

    /// <summary>The currency the country's regional data names, as a suggestion only.</summary>
    private static string? CurrencyFor(string countryCode)
    {
        var culture = CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .FirstOrDefault(item => item.Name.EndsWith("-" + countryCode, StringComparison.OrdinalIgnoreCase));

        return culture is null ? null : new RegionInfo(culture.Name).ISOCurrencySymbol;
    }

    /// <summary>The unit system the environment's own country preset names.</summary>
    private static string? CountryUnits(string countryCode)
    {
        if (!CountryPresets.TryGet(countryCode, out var json))
        {
            return null;
        }

        var environment = WellboreEnvironment.FromJson(json);

        return environment.Units.System;
    }

    /// <summary>Moves to a step. Every navigation - the rail, Back, Next, a resume - goes through here.</summary>
    private void SetStep(int index)
    {
        _stepIndex = Math.Clamp(index, 0, Math.Max(0, _pages.Count - 1));

        if (_pages.Count > 0)
        {
            _stepHost.Content = _pages[_stepIndex];
        }

        RefreshRail();
        UpdateFooter();
        UpdateSummary();
    }

    /// <summary>Redraws the rail's labels from the furthest step reached.</summary>
    private void RefreshRail()
    {
        _refreshingRail = true;

        try
        {
            _rail.ItemsSource = Enumerable
                .Range(0, _pages.Count)
                .Select(index => OgSetupSteps.LabelFor(_furthestStep, index))
                .ToList();

            _rail.SelectedIndex = _stepIndex;
        }
        finally
        {
            _refreshingRail = false;
        }
    }

    private void OnRailSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshingRail || _rail.SelectedIndex < 0 || _rail.SelectedIndex == _stepIndex)
        {
            return;
        }

        var target = _rail.SelectedIndex;

        // The research is explicit: a wizard enforces its order. A step further than the next one is refused.
        if (!OgSetupSteps.MaySelect(_furthestStep, target))
        {
            _refreshingRail = true;
            try
            {
                _rail.SelectedIndex = _stepIndex;
            }
            finally
            {
                _refreshingRail = false;
            }

            _footerStatus.Text = "Complete the current step before moving on.";
            return;
        }

        _furthestStep = Math.Max(_furthestStep, target);
        SetStep(target);

        if (target == 1 && !_catalogBuilt)
        {
            BuildCatalog();
        }

        if (target == 3 && _plannedModules.Count == 0 && _readPlan.IsEnabled)
        {
            OnReadModulePlan(this, new RoutedEventArgs());
        }
    }

    private async void OnNext(object sender, RoutedEventArgs e)
    {
        var accepted = _stepIndex;

        if (!ValidateStep(accepted))
        {
            return;
        }

        // The step is accepted: record it before moving, so a crash between the two loses nothing.
        await RecordProgressAsync(accepted).ConfigureAwait(true);

        if (accepted < _pages.Count - 1)
        {
            _furthestStep = Math.Max(_furthestStep, accepted + 1);
            SetStep(accepted + 1);
        }
    }

    private void UpdateFooter()
    {
        var index = _stepIndex;
        _back.IsEnabled = index > 0;

        // "Next" with no information scent is the pattern the guidance warns against; the label names the step.
        _next.Content = index < _pages.Count - 1
            ? $"Next: {OgSetupSteps.NameOf(index + 1)}"
            : "Next";

        _next.Visibility = index < _pages.Count - 1 ? Visibility.Visible : Visibility.Collapsed;
        _initialize.Visibility = index == _pages.Count - 1 ? Visibility.Visible : Visibility.Collapsed;
        _footerStatus.Text = index == _pages.Count - 1
            ? "The last step creates the database schema, the settings and the administrator."
            : string.Empty;
    }

    // ------------------------------------------------------------------ validation

    private bool ValidateStep(int index)
    {
        switch (index)
        {
            case 0:
                if (Mode == OgDeploymentMode.CompanyDatabase && _authorised.IsChecked != true)
                {
                    return Fail("Confirm that you are authorised to initialise this database and have a backup.");
                }

                return true;

            case 1:
                if (string.IsNullOrWhiteSpace(_selectedConnectionName))
                {
                    return Fail("Choose a connection, or register a server, before continuing.");
                }

                return true;

            case 2:
                if (_companyName.Text.Trim().Length < 2)
                {
                    return Fail("A company name of at least two characters is required.");
                }

                var code = _companyCode.Text.Trim();

                if (code.Length is < 2 or > 20
                    || code.Any(character => !char.IsLetterOrDigit(character) && character is not ('-' or '_')))
                {
                    return Fail("The company code must be 2–20 letters, digits, '-' or '_'.");
                }

                if (_country.SelectedItem is not OgSetupChoice
                    || _culture.SelectedItem is not OgSetupChoice
                    || _timeZone.SelectedItem is not OgSetupChoice
                    || _fiscalMonth.SelectedItem is not OgSetupChoice
                    || _companyType.SelectedItem is not OgSetupChoice
                    || _unitSystem.SelectedItem is not OgSetupChoice)
                {
                    return Fail("Country, language, time zone, fiscal month, company type and unit system are all required.");
                }

                var currency = _currency.Text.Trim();

                if (currency.Length != 3 || currency.Any(character => character is < 'A' or > 'Z'))
                {
                    return Fail("The base currency is a three-letter ISO code, in upper case (for example USD).");
                }

                return true;

            case 3:
                return true;

            case 4:
                if (string.IsNullOrWhiteSpace(_adminUser.Text))
                {
                    return Fail("A user name is required for the administrator.");
                }

                var problems = Core.OgPasswordPolicy.Problems(_adminPassword.Password);

                if (problems.Count > 0)
                {
                    return Fail(string.Join(" ", problems));
                }

                if (!string.Equals(_adminPassword.Password, _adminConfirm.Password, StringComparison.Ordinal))
                {
                    return Fail("The two passwords do not match.");
                }

                return true;

            default:
                return true;
        }
    }

    private bool Fail(string message)
    {
        _footerStatus.Text = message;
        return false;
    }

    // ------------------------------------------------------------------ database step

    private void BuildCatalog()
    {
        try
        {
            var host = _application.Services.GetRequiredService<OgBeepLifetime>().Host;

            _managementContext = new WpfDataManagementContext(_application.DataServices, host.Beep);
            _catalog = new BeepWpfConnectionCatalog { Height = 280 };
            _catalog.InitializeTemplate(_application.DataServices, _managementContext);
            _catalog.Initialize(_managementContext, new WpfConnectionCatalog(_managementContext));
            _catalog.SelectedConnectionChanged += (_, connection) =>
            {
                _selectedConnectionName = connection?.ConnectionName;
                UpdateSummary();
            };

            if (_databasePanel.Children.OfType<Border>().FirstOrDefault() is { } border)
            {
                border.Child = _catalog;
            }

            _catalogBuilt = true;
            _ = LoadProvidersAsync();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "preparing the connection list",
                "the connection list is not available; a server can still be registered below",
                FailureSeverity.Error);

            _databaseStatus.Text = $"The connection list could not be prepared. Reference: {reference}";
        }
    }

    private async Task LoadProvidersAsync()
    {
        var providers = await _application.Connections.ProvidersAsync().ConfigureAwait(true);

        if (!providers.Success || providers.Value is null)
        {
            _provider.ItemsSource = Array.Empty<OgSetupChoice>();
            _databaseStatus.Text = providers.Message ?? "The installed drivers could not be listed.";
            return;
        }

        _provider.ItemsSource = providers.Value
            .Where(candidate => !candidate.CreateLocal || candidate.Type.Length > 0)
            .Select(candidate => new OgSetupChoice(
                candidate.Type,
                candidate.DriverName is { Length: > 0 } driver ? $"{candidate.Type} ({driver})" : candidate.Type))
            .ToList();

        _provider.SelectedItem = ((IReadOnlyList<OgSetupChoice>)_provider.ItemsSource)
            .FirstOrDefault(choice => !string.Equals(choice.Code, "Sqlite", StringComparison.OrdinalIgnoreCase));
    }

    private void OnConnectionWizard(object sender, RoutedEventArgs e)
    {
        if (!_catalogBuilt || _catalog is null)
        {
            _databaseStatus.Text = "The connection list is still being prepared.";
            return;
        }

        var dialog = new TheTechIdea.Beep.Wpf.Data.Integrated.Dialogs.BeepWpfConnectionDialog(_application.DataServices)
        {
            Owner = this,
        };

        if (dialog.ShowDialog() != true || dialog.Result is null)
        {
            return;
        }

        _catalog.ReloadConnections();
        _selectedConnectionName = dialog.Result.ConnectionName;
        _databaseStatus.Text = $"Connection '{dialog.Result.ConnectionName}' saved.";
        UpdateSummary();
    }

    private void OnDownloadDriver(object sender, RoutedEventArgs e)
    {
        try
        {
            new TheTechIdea.Beep.Wpf.Data.Integrated.Dialogs.BeepWpfDriverDownloadDialog(_application.DataServices)
            {
                Owner = this,
            }.ShowDialog();

            _databaseStatus.Text = "Driver download finished. Read the module plan again after a driver is installed.";
            _ = LoadProvidersAsync();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "downloading a database driver",
                "no driver was installed",
                FailureSeverity.Error);

            _databaseStatus.Text = $"The driver download did not finish. Reference: {reference}";
        }
    }

    private void OnDownloadDataSource(object sender, RoutedEventArgs e)
    {
        try
        {
            new TheTechIdea.Beep.Wpf.Data.Integrated.Dialogs.BeepWpfDataSourceDownloadDialog(_application.DataServices)
            {
                Owner = this,
            }.ShowDialog();

            _databaseStatus.Text = "Data source download finished.";
            _ = LoadProvidersAsync();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "downloading a data source",
                "no data source was installed",
                FailureSeverity.Error);

            _databaseStatus.Text = $"The data source download did not finish. Reference: {reference}";
        }
    }

    private async void OnTestConnection(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedConnectionName))
        {
            _databaseStatus.Text = "Choose a connection first.";
            return;
        }

        _databaseStatus.Text = $"Testing '{_selectedConnectionName}'…";
        var result = await _application.Connections.TestAsync(_selectedConnectionName).ConfigureAwait(true);

        _databaseStatus.Text = result.Success
            ? $"'{_selectedConnectionName}' opened."
            : result.Message;

        UpdateSummary();
    }

    private async void OnRegisterServer(object sender, RoutedEventArgs e)
    {
        if (_provider.SelectedItem is not OgSetupChoice provider)
        {
            _databaseStatus.Text = "Choose the database product this server runs.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_host.Text) || string.IsNullOrWhiteSpace(_database.Text))
        {
            _databaseStatus.Text = "A host and a database name are required.";
            return;
        }

        if (!int.TryParse(_port.Text.Trim(), out var port) || port is < 0 or > 65535)
        {
            _databaseStatus.Text = "The port must be a number between 0 and 65535 (0 lets the driver use its default).";
            return;
        }

        var name = _companyMode.IsChecked == true
            ? string.IsNullOrWhiteSpace(_companyCode.Text) ? "CompanyDatabase" : _companyCode.Text.Trim()
            : "CompanyDatabase";

        _databaseStatus.Text = $"Registering '{name}'…";

        var result = await _application.Connections
            .EnsureRemoteAsync(new OgRemoteDatabaseRequest(
                Name: name,
                Provider: provider.Code,
                Host: _host.Text.Trim(),
                Port: port,
                Database: _database.Text.Trim(),
                UserId: _userId.Text.Trim(),
                Password: _password.Password,
                Schema: null,
                UseWindowsAuthentication: _windowsAuth.IsChecked == true,
                TrustServerCertificate: _trustCertificate.IsChecked == true))
            .ConfigureAwait(true);

        if (!result.Success)
        {
            _databaseStatus.Text = result.Message ?? "The server was not registered.";
            return;
        }

        _selectedConnectionName = name;
        _password.Clear();
        _catalog?.ReloadConnections();
        _databaseStatus.Text = result.Value?.Detail ?? $"'{name}' is registered and was opened.";
        UpdateSummary();
    }

    // ------------------------------------------------------------------ company step

    private async void OnAdoptExisting(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedConnectionName))
        {
            _companyStatus.Text = "Choose the company database first (the Database step).";
            return;
        }

        _companyStatus.Text = "Reading the database…";

        try
        {
            var read = await _application.SharedSettings(_selectedConnectionName).ReadAsync().ConfigureAwait(true);

            if (!read.Success || read.Value is null)
            {
                _companyStatus.Text = read.Message ?? "The shared settings could not be read.";
                return;
            }

            var snapshot = read.Value;
            _companyRevision = snapshot.InstallationRevision;
            _toolsRevision = snapshot.ToolsRevision;
            _modulesRevision = snapshot.ModulesRevision;

            if (snapshot.Installation is not { } company)
            {
                _adoptedExistingRecord = false;
                _companyStatus.Text =
                    "This database has no company record yet, so this step will create it. " +
                    $"({snapshot.Modules.Count} recorded module(s).)";
                return;
            }

            _adoptedExistingRecord = true;
            _companyName.Text = company.CompanyName;
            _companyCode.Text = company.CompanyCode;
            _currency.Text = company.BaseCurrencyCode;
            _fieldName.Text = company.DefaultFieldName ?? string.Empty;

            Select(_country, OgSetupCatalog.Countries, company.CountryCode);
            Select(_culture, OgSetupCatalog.Cultures, company.CultureName);
            Select(_timeZone, OgSetupCatalog.TimeZones, company.TimeZoneId);
            Select(_companyType, OgSetupCatalog.CompanyTypes, company.CompanyTypeCode);
            Select(_unitSystem, OgSetupCatalog.UnitSystems, company.UnitSystemCode);
            Select(_fiscalMonth, OgSetupCatalog.FiscalMonths, company.FiscalYearStartMonth.ToString(CultureInfo.InvariantCulture));

            _companyStatus.Text =
                $"This database is already set up for '{company.CompanyName}' ({company.CompanyCode}), recorded " +
                $"{company.RecordedAtUtc:yyyy-MM-dd} by {company.RecordedBy}. The wizard adopts that record and will " +
                "not write a second company.";

            UpdateSummary();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reading the company record from the database",
                "the wizard is still working from the values on screen",
                FailureSeverity.Error);

            _companyStatus.Text = $"The company record could not be read. Reference: {reference}";
        }
    }

    private static void Select(ComboBox combo, IReadOnlyList<OgSetupChoice> choices, string? code)
    {
        if (OgSetupCatalog.Find(choices, code) is { } match)
        {
            combo.SelectedItem = match;
        }
    }

    // ------------------------------------------------------------------ modules step

    private async void OnReadModulePlan(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedConnectionName))
        {
            _moduleStatus.Text = "Choose the connection first (the Database step).";
            return;
        }

        _readPlan.IsEnabled = false;
        _moduleList.Children.Clear();
        _referenceList.Children.Clear();
        _plannedModules.Clear();
        _selectedModules.Clear();

        try
        {
            var runner = CreateModuleRunner();
            var plan = await runner.PlanAsync().ConfigureAwait(true);

            if (!plan.Success || plan.Value is null)
            {
                _moduleStatus.Text = plan.Message ?? "The module plan could not be read.";
                return;
            }

            foreach (var module in plan.Value.Modules.OrderBy(item => item.Order))
            {
                var check = new CheckBox
                {
                    Content = module.Describe(),
                    IsChecked = !module.RequiresConfirmation && !module.AlreadyCompleted,
                    IsEnabled = !module.AlreadyCompleted,
                    Margin = new Thickness(0, 0, 0, 6),
                    Tag = module.ModuleId,
                };

                check.Checked += (_, _) => ToggleModule(module.ModuleId, true);
                check.Unchecked += (_, _) => ToggleModule(module.ModuleId, false);

                _moduleList.Children.Add(check);
                _plannedModules.Add(module.ModuleId);

                if (check.IsChecked == true)
                {
                    _selectedModules.Add(module.ModuleId);
                }
            }

            _moduleStatus.Text = plan.Value.IsComplete
                ? $"Nothing to apply: all {plan.Value.Modules.Count} module(s) are already in this database."
                : $"{plan.Value.Modules.Count} module(s) planned, {plan.Value.EntityTypeCount} type(s) in total.";

            ShowReferenceLists(plan.Value);
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reading the PPDM module plan",
                "the module list stays empty and setup can still record the company",
                FailureSeverity.Error);

            _moduleStatus.Text = $"The module plan could not be read. Reference: {reference}";
        }
        finally
        {
            _readPlan.IsEnabled = true;
        }
    }

    /// <summary>
    /// Names the reference tables the plan will seed, grouped by module, so the SystemAdmin can review the LOV set
    /// before applying. An empty set is stated, not left blank - "none" is information too.
    /// </summary>
    private void ShowReferenceLists(OgSetupPlan plan)
    {
        _referenceList.Children.Clear();

        var named = plan.Modules
            .Where(module => module.ReferenceLists.Count > 0)
            .ToList();

        if (named.Count == 0)
        {
            _referenceStatus.Text = plan.ReferenceLists.Count == 0
                ? "The planned modules seed no reference lists of their own."
                : $"The plan seeds {plan.ReferenceLists.Count} reference list(s).";

            return;
        }

        foreach (var module in named)
        {
            _referenceList.Children.Add(new TextBlock
            {
                Text = $"{module.ModuleName} ({module.ModuleId}) - {module.ReferenceLists.Count} list(s)",
                FontWeight = FontWeights.SemiBold,
                FontSize = 11.5,
                Margin = new Thickness(0, 4, 0, 2),
                Foreground = Application.Current?.TryFindResource("BeepTextPrimaryBrush") as Brush ?? Brushes.Black,
            });

            foreach (var table in module.ReferenceLists)
            {
                _referenceList.Children.Add(new TextBlock
                {
                    Text = "  " + table,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 11,
                    Foreground = Application.Current?.TryFindResource("BeepTextSecondaryBrush") as Brush ?? Brushes.Gray,
                });
            }
        }

        _referenceStatus.Text =
            $"{plan.ReferenceLists.Count} reference list(s) will be seeded, in {named.Count} module(s). " +
            "They are reviewed here; the apply creates them.";
    }

    private void ToggleModule(string moduleId, bool selected)
    {
        _selectedModules.RemoveAll(id => string.Equals(id, moduleId, StringComparison.OrdinalIgnoreCase));

        if (selected)
        {
            _selectedModules.Add(moduleId);
        }

        UpdateSummary();
    }

    private OgModuleSetupRunner CreateModuleRunner(string? connectionName = null)
    {
        var host = _application.Services.GetRequiredService<OgBeepLifetime>().Host;
        var failures = _application.Services.GetRequiredService<IFailureReporter>();

        return new OgModuleSetupRunner(
            new OgSetupComposition(host, failures, connectionName ?? _selectedConnectionName!),
            new OgSetupStateStore(OgPaths.ModuleSetupStatePath),
            failures,
            _application.SetupState?.AdministratorUserName ?? OgPaths.SystemUserId,
            () => OgPaths.LocalDatabasePath);
    }

    // ------------------------------------------------------------------ review and initialize

    private void UpdateSummary()
    {
        var mode = Mode == OgDeploymentMode.CompanyDatabase
            ? "Company database — settings shared by every user"
            : "This workstation only";

        var country = (_country.SelectedItem as OgSetupChoice)?.DisplayName ?? "—";
        var units = (_unitSystem.SelectedItem as OgSetupChoice)?.DisplayName ?? "—";
        var company = string.IsNullOrWhiteSpace(_companyName.Text) ? "(not entered yet)" : _companyName.Text.Trim();

        var modules = _selectedModules.Count == 0
            ? "none selected"
            : string.Join(", ", _selectedModules);

        _summary.Text =
            $"Deployment: {mode}{Environment.NewLine}" +
            $"Connection: {(_selectedConnectionName ?? "(not chosen yet)")}{Environment.NewLine}" +
            $"Company: {company}{Environment.NewLine}" +
            $"Country: {country}{Environment.NewLine}" +
            $"Units: {units}{Environment.NewLine}" +
            $"Base currency: {(_currency.Text.Trim().Length == 0 ? "—" : _currency.Text.Trim())}{Environment.NewLine}" +
            $"Modules to apply: {modules}{Environment.NewLine}" +
            (_adoptedExistingRecord
                ? "This database already has a company record; it will be adopted, not replaced." + Environment.NewLine
                : string.Empty) +
            $"Data root on this computer: {OgPaths.DataRoot}";
    }

    private async void OnInitialize(object sender, RoutedEventArgs e)
    {
        if (_initializing)
        {
            return;
        }

        foreach (var step in new[] { 0, 1, 2, 4 })
        {
            if (ValidateStep(step))
            {
                continue;
            }

            SetStep(step);
            _initializeStatus.Text = _footerStatus.Text;
            return;
        }

        var mode = Mode;

        if (string.IsNullOrWhiteSpace(_selectedConnectionName))
        {
            _initializeStatus.Text = "Choose a connection before creating the installation.";
            return;
        }

        _initializing = true;
        _initialize.IsEnabled = false;
        _progress.IsIndeterminate = true;

        try
        {
            // 1. Licence, if one was chosen. The import decides; the wizard never assumes it is valid.
            var licence = _licencePath.Text.Trim();

            if (licence.Length > 0)
            {
                _initializeStatus.Text = "Importing the licence…";
                var imported = _application.Licence.Import(licence);

                if (!imported.Success)
                {
                    _initializeStatus.Text = imported.Message ?? "The licence was refused.";
                    return;
                }

                _licenceStatus.Text = _application.Licence.Current.StatusText;
            }

            // 2. The data path.
            string connectionName;

            if (mode == OgDeploymentMode.Personal)
            {
                // A personal installation is one local file. An existing local file the person chose is adopted;
                // otherwise the installation's own file is created at the documented path.
                var listed = await _application.Connections.ListAsync().ConfigureAwait(true);

                var chosen = listed.Value?.FirstOrDefault(candidate =>
                    string.Equals(candidate.Name, _selectedConnectionName, StringComparison.OrdinalIgnoreCase));

                if (chosen is { IsFile: true }
                    && !string.Equals(chosen.Name, OgPaths.ConnectionName, StringComparison.OrdinalIgnoreCase))
                {
                    _initializeStatus.Text = $"Adopting '{chosen.Name}' as this installation's local database…";
                    var adopted = await _application.Connections
                        .AdoptAsync(chosen.Name, OgPaths.ConnectionName)
                        .ConfigureAwait(true);

                    if (!adopted.Success)
                    {
                        _initializeStatus.Text = adopted.Message ?? "The chosen local database could not be adopted.";
                        return;
                    }
                }
                else
                {
                    _initializeStatus.Text = "Creating the local database…";

                    var local = await _application.Connections
                        .EnsureLocalAsync(new OgLocalDatabaseRequest(
                            OgPaths.ConnectionName,
                            OgPaths.LocalDatabaseDirectory,
                            OgPaths.ConnectionName + ".db"))
                        .ConfigureAwait(true);

                    if (!local.Success)
                    {
                        _initializeStatus.Text = local.Message ?? "The local database could not be created.";
                        return;
                    }
                }

                connectionName = OgPaths.ConnectionName;
            }
            else
            {
                _initializeStatus.Text = $"Adopting '{_selectedConnectionName}' as this installation's connection…";

                var adopted = await _application.Connections
                    .AdoptAsync(_selectedConnectionName, OgPaths.ConnectionName)
                    .ConfigureAwait(true);

                if (!adopted.Success)
                {
                    _initializeStatus.Text = adopted.Message ?? "The chosen connection could not be adopted.";
                    return;
                }

                _initializeStatus.Text = $"Opening '{OgPaths.ConnectionName}'…";
                var tested = await _application.Connections.TestAsync(OgPaths.ConnectionName).ConfigureAwait(true);

                if (!tested.Success)
                {
                    _initializeStatus.Text = tested.Message ?? "The company database did not open.";
                    return;
                }

                connectionName = OgPaths.ConnectionName;
            }

            var administrator = _adminUser.Text.Trim();
            var displayName = string.IsNullOrWhiteSpace(_adminDisplayName.Text)
                ? administrator
                : _adminDisplayName.Text.Trim();

            // One creation path for both deployments. The difference is only where the database is: a local file
            // created here, or a server registered in the Database step. The company record, the module set, the
            // environment and the administrator are rows in that database either way — nothing is written to a
            // settings file.
            _initializeStatus.Text = "Creating the company in the database…";

            var identity = _application.DatabaseIdentity(connectionName);

            // The security module is always applied: it creates USER/ROLE and seeds the roles the administrator
            // needs. The person's own choices are added to it.
            var modules = _selectedModules
                .Append("SECURITY")
                .Where(module => !string.IsNullOrWhiteSpace(module))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var creation = new OgCompanyCreation(
                _application.SharedSettings(connectionName),
                identity,
                async (moduleIds, confirmation, progress) =>
                    await CreateModuleRunner(connectionName)
                        .ApplyAsync(confirmation, moduleIds, progress)
                        .ConfigureAwait(true),
                _application.Diagnostics,
                _application.AuditLog);

            var created = await creation.CreateAsync(
                    new OgCompanyCreationRequest(
                        ConnectionName: connectionName,
                        Company: BuildCompanyDocument(),
                        EnvironmentDocument: BuildEnvironmentDocument() ?? string.Empty,
                        ModuleIds: modules,
                        IncludeDemoData: _includeDemo.IsChecked == true,
                        AdministratorUserName: administrator,
                        AdministratorDisplayName: displayName,
                        AdministratorPassword: _adminPassword.Password,
                        Actor: administrator,
                        RequirePasswordChange: true),
                    new Progress<OgSetupProgress>(report =>
                        _initializeStatus.Text = $"{report.Phase}: {report.ModuleId} ({report.Completed}/{report.Total})"))
                .ConfigureAwait(true);

            if (!created.Success || created.Value is null)
            {
                _initializeStatus.Text = created.Message ?? "The company was not created.";
                return;
            }

            administrator = created.Value.AdministratorUserName ?? administrator;

            // The installation is now whatever the database says: this is read back, not remembered.
            var probed = await _application.ProbeInstallationAsync().ConfigureAwait(true);

            if (!probed.Success || probed.Value is null)
            {
                _initializeStatus.Text = probed.Message ?? "The installation could not be re-read after setup.";
                return;
            }

            if (probed.Value.Company is not null)
            {
                _application.AdoptCompany(probed.Value.Company);
            }

            _initializeStatus.Text = created.Value.Message;

            // The environment is the document in the database; the application has already loaded it. There is no
            // machine record left to write.
            // The installation exists: there is nothing to resume, and a stale record would open the next setup at
            // the review step.
            await _application
                .SharedSettings(connectionName)
                .ClearSetupProgressAsync(administrator)
                .ConfigureAwait(true);

            _initializeStatus.Text = "The installation is ready.";
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "creating the installation",
                "the installation was not created",
                FailureSeverity.Critical);

            _initializeStatus.Text = $"The installation could not be created. Reference: {reference}";
        }
        finally
        {
            _initializing = false;
            _progress.IsIndeterminate = false;
            _initialize.IsEnabled = true;
        }
    }

    /// <summary>The company record the form describes, as the settings document stores it.</summary>
    private Data.Settings.OgInstallationSettings BuildCompanyDocument()
    {
        var actor = _adminUser.Text.Trim();

        return new Data.Settings.OgInstallationSettings(
            CompanyName: _companyName.Text.Trim(),
            CompanyCode: _companyCode.Text.Trim(),
            CountryCode: (_country.SelectedItem as OgSetupChoice)?.Code ?? string.Empty,
            CultureName: (_culture.SelectedItem as OgSetupChoice)?.Code ?? string.Empty,
            TimeZoneId: (_timeZone.SelectedItem as OgSetupChoice)?.Code ?? string.Empty,
            BaseCurrencyCode: _currency.Text.Trim(),
            FiscalYearStartMonth: int.TryParse((_fiscalMonth.SelectedItem as OgSetupChoice)?.Code, out var month) ? month : 1,
            CompanyTypeCode: (_companyType.SelectedItem as OgSetupChoice)?.Code ?? string.Empty,
            UnitSystemCode: (_unitSystem.SelectedItem as OgSetupChoice)?.Code ?? string.Empty,
            DefaultFieldName: _fieldName.Text.Trim(),
            RecordedBy: string.IsNullOrWhiteSpace(actor) ? OgPaths.SystemUserId : actor,
            RecordedAtUtc: DateTimeOffset.UtcNow);
    }

    /// <summary>The environment document this installation shares: the chosen country preset and unit system.</summary>
    private string? BuildEnvironmentDocument() =>
        OgSetupCatalog.BuildEnvironmentDocument(
            (_country.SelectedItem as OgSetupChoice)?.Code,
            (_unitSystem.SelectedItem as OgSetupChoice)?.Code);

    // ------------------------------------------------------------------ licence

    private void OnBrowseLicence(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Licence files (*.json)|*.json|All files (*.*)|*.*",
            CheckFileExists = true,
            Title = "Choose a licence file",
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        _licencePath.Text = dialog.FileName;

        try
        {
            var read = Security.OgLicenceVerifier.Verify(
                File.ReadAllText(dialog.FileName),
                Security.OgLicenceVerifier.Trusted);

            _licenceStatus.Text = read.IsAuthenticated
                ? $"Signed licence for {read.Claims!.Customer}; edition {read.Claims.Edition}."
                : read.Reason ?? "The file is not a signed licence, so it grants nothing.";
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reading the licence file",
                "the licence is not imported",
                FailureSeverity.Error);

            _licenceStatus.Text = $"The licence file could not be read. Reference: {reference}";
        }
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>
    /// A step's page. The rail carries the name, so the page is only its content; the header argument is kept so the
    /// call sites read as "Tab("Company", panel)" and a future reader still sees which step it is.
    /// </summary>
    private static FrameworkElement Tab(string header, FrameworkElement content)
    {
        _ = header;
        return content;
    }

    private Button ActionButton(string text, RoutedEventHandler? click = null)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(14, 5, 14, 5),
            Margin = new Thickness(0, 0, 8, 0),
            MinWidth = 96,
        };

        if (click is not null)
        {
            button.Click += click;
        }

        return button;
    }

    private static void AddLabelled(Grid grid, int row, int column, string label, FrameworkElement field)
    {
        var cell = new StackPanel { Margin = new Thickness(0, 0, 16, 8) };
        cell.Children.Add(Caption(label));
        cell.Children.Add(field);

        Grid.SetRow(cell, row);
        Grid.SetColumn(cell, column);
        grid.Children.Add(cell);
    }

    private static TextBlock Heading(string text) => new()
    {
        Text = text,
        FontWeight = FontWeights.SemiBold,
        FontSize = 16,
        Margin = new Thickness(0, 14, 0, 4),
        Foreground = (Brush?)Application.Current?.TryFindResource("BeepTextPrimaryBrush") ?? Brushes.Black,
    };

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        FontSize = 11.5,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 0, 0, 6),
        Foreground = (Brush?)Application.Current?.TryFindResource("BeepTextSecondaryBrush") ?? Brushes.Gray,
    };

    private static Brush Brush(string key, Brush fallback) =>
        Application.Current?.TryFindResource(key) as Brush ?? fallback;
}
