using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using TheTechIdea.Beep.OilandGas.Desktop.Setup;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// Edits the installation's company record — the values the first-run wizard wrote.
/// </summary>
/// <remarks>
/// <para>
/// The window only collects and checks the values; the settings page decides where they are stored (the
/// workstation's own record or the company database's <c>INSTALLATION</c> record) and reports the outcome. That
/// keeps one place responsible for the write, its revision check and its audit.
/// </para>
/// <para>
/// It is opened by the settings page only for an administrator, and only the settings page is allowed to save:
/// this window has no idea whether the signed-in person is allowed, which is exactly the order the privilege
/// check must happen in.
/// </para>
/// </remarks>
public sealed class OgCompanySettingsWindow : Window
{
    private readonly TextBox _name = new() { MinWidth = 260 };
    private readonly TextBox _code = new() { MinWidth = 160 };
    private readonly ComboBox _country = new() { MinWidth = 260 };
    private readonly TextBox _currency = new() { Width = 90 };
    private readonly ComboBox _culture = new() { MinWidth = 180 };
    private readonly ComboBox _timeZone = new() { MinWidth = 280 };
    private readonly ComboBox _fiscalMonth = new() { MinWidth = 180 };
    private readonly ComboBox _companyType = new() { MinWidth = 240 };
    private readonly ComboBox _unitSystem = new() { MinWidth = 240 };
    private readonly TextBox _field = new() { MinWidth = 240 };
    private readonly TextBlock _status;
    private readonly string _originalCountry;
    private readonly string _originalUnits;

    public OgCompanySettingsWindow(OgApplication application, OgInstallationSettings current)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(current);

        Title = "Company settings";
        Width = 760;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Application.Current?.TryFindResource("BeepBackgroundBrush") as Brush ?? Brushes.White;
        FlowDirection = application.Loc.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        _status = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0),
            Foreground = Application.Current?.TryFindResource("BeepDangerBrush") as Brush ?? Brushes.Firebrick,
        };

        _originalCountry = current.CountryCode;
        _originalUnits = current.UnitSystemCode;

        _name.Text = current.CompanyName;
        _code.Text = current.CompanyCode;
        _currency.Text = current.BaseCurrencyCode;

        _country.ItemsSource = OgSetupCatalog.Countries;
        _country.SelectedItem = OgSetupCatalog.Find(OgSetupCatalog.Countries, current.CountryCode)
                                ?? OgSetupCatalog.Countries.FirstOrDefault();

        _culture.ItemsSource = OgSetupCatalog.Cultures;
        _culture.SelectedItem = OgSetupCatalog.Find(OgSetupCatalog.Cultures, current.CultureName)
                                ?? OgSetupCatalog.Cultures.FirstOrDefault();

        _timeZone.ItemsSource = OgSetupCatalog.TimeZones;
        _timeZone.SelectedItem = OgSetupCatalog.Find(OgSetupCatalog.TimeZones, current.TimeZoneId)
                                 ?? OgSetupCatalog.TimeZones.FirstOrDefault();

        _fiscalMonth.ItemsSource = OgSetupCatalog.FiscalMonths;
        _fiscalMonth.SelectedItem = OgSetupCatalog.FiscalMonths.FirstOrDefault(choice =>
            string.Equals(
                choice.Code,
                current.FiscalYearStartMonth.ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal)) ?? OgSetupCatalog.FiscalMonths.FirstOrDefault();

        _companyType.ItemsSource = OgSetupCatalog.CompanyTypes;
        _companyType.SelectedItem = OgSetupCatalog.Find(OgSetupCatalog.CompanyTypes, current.CompanyTypeCode)
                                    ?? OgSetupCatalog.CompanyTypes.FirstOrDefault();

        _unitSystem.ItemsSource = OgSetupCatalog.UnitSystems;
        _unitSystem.SelectedItem = OgSetupCatalog.Find(OgSetupCatalog.UnitSystems, current.UnitSystemCode)
                                   ?? OgSetupCatalog.UnitSystems.FirstOrDefault();

        _field.Text = current.DefaultFieldName ?? string.Empty;

        Content = BuildContent();
    }

    /// <summary>The edited values, or null until <c>Save</c> succeeds.</summary>
    public OgInstallationSettings? Result { get; private set; }

    /// <summary>True when the country or the unit system changed, so the shared environment must be re-written.</summary>
    public bool ChangesEnvironment { get; private set; }

    private FrameworkElement BuildContent()
    {
        var panel = new StackPanel { Margin = new Thickness(20) };

        panel.Children.Add(new TextBlock
        {
            Text = "Company settings",
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            Foreground = Application.Current?.TryFindResource("BeepTextPrimaryBrush") as Brush ?? Brushes.Black,
        });
        panel.Children.Add(Caption(
            "These values are the installation's company record. In a company database the change is written to the " +
            "database, so every user reads the new values the next time the application starts."));

        var grid = new Grid { Margin = new Thickness(0, 8, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        for (var row = 0; row < 5; row++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        Add(grid, 0, 0, "Company name", _name);
        Add(grid, 0, 2, "Company code", _code);
        Add(grid, 1, 0, "Country", _country);
        Add(grid, 1, 2, "Base currency (ISO)", _currency);
        Add(grid, 2, 0, "Interface language", _culture);
        Add(grid, 2, 2, "Time zone", _timeZone);
        Add(grid, 3, 0, "Fiscal year starts", _fiscalMonth);
        Add(grid, 3, 2, "Company type", _companyType);
        Add(grid, 4, 0, "Unit system", _unitSystem);
        Add(grid, 4, 2, "Default field or asset", _field);

        panel.Children.Add(grid);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 12, 0, 0) };

        var save = new Button
        {
            Content = "Save",
            IsDefault = true,
            Padding = new Thickness(16, 5, 16, 5),
            MinWidth = 100,
            Margin = new Thickness(0, 0, 8, 0),
        };
        save.Click += OnSave;

        var cancel = new Button
        {
            Content = "Cancel",
            IsCancel = true,
            Padding = new Thickness(16, 5, 16, 5),
            MinWidth = 100,
        };

        buttons.Children.Add(save);
        buttons.Children.Add(cancel);
        panel.Children.Add(buttons);
        panel.Children.Add(_status);

        return panel;
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var name = _name.Text.Trim();
        var code = _code.Text.Trim();
        var currency = _currency.Text.Trim().ToUpperInvariant();

        if (name.Length < 2)
        {
            _status.Text = "A company name of at least two characters is required.";
            _name.Focus();
            return;
        }

        if (code.Length is < 2 or > 20
            || code.Any(character => !char.IsLetterOrDigit(character) && character is not ('-' or '_')))
        {
            _status.Text = "The company code must be 2–20 letters, digits, '-' or '_'.";
            _code.Focus();
            return;
        }

        if (currency.Length != 3 || currency.Any(character => character is < 'A' or > 'Z'))
        {
            _status.Text = "The base currency is a three-letter ISO code, in upper case (for example USD).";
            _currency.Focus();
            return;
        }

        if (_country.SelectedItem is not OgSetupChoice country
            || _culture.SelectedItem is not OgSetupChoice culture
            || _timeZone.SelectedItem is not OgSetupChoice timeZone
            || _fiscalMonth.SelectedItem is not OgSetupChoice fiscalMonth
            || _companyType.SelectedItem is not OgSetupChoice companyType
            || _unitSystem.SelectedItem is not OgSetupChoice unitSystem)
        {
            _status.Text = "Country, language, time zone, fiscal month, company type and unit system are all required.";
            return;
        }

        Result = new OgInstallationSettings(
            name,
            code,
            country.Code,
            culture.Code,
            timeZone.Code,
            currency,
            int.TryParse(fiscalMonth.Code, CultureInfo.InvariantCulture, out var month) ? month : 1,
            companyType.Code,
            unitSystem.Code,
            _field.Text.Trim(),
            string.Empty,
            DateTimeOffset.UtcNow);

        // The environment follows the country and the unit system, so a change to either must be written too.
        ChangesEnvironment =
            !string.Equals(_originalCountry, country.Code, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(_originalUnits, unitSystem.Code, StringComparison.OrdinalIgnoreCase);

        DialogResult = true;
    }

    private static void Add(Grid grid, int row, int column, string label, FrameworkElement field)
    {
        var cell = new StackPanel { Margin = new Thickness(0, 0, 16, 8) };
        cell.Children.Add(Caption(label));
        cell.Children.Add(field);

        Grid.SetRow(cell, row);
        Grid.SetColumn(cell, column);
        grid.Children.Add(cell);
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        FontSize = 11.5,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 0, 0, 4),
        Foreground = Application.Current?.TryFindResource("BeepTextSecondaryBrush") as Brush ?? Brushes.Gray,
    };
}
