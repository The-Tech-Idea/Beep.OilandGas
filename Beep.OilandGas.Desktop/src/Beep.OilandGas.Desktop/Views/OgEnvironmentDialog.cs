using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using TheTechIdea.Beep.OilandGas.Desktop.Setup;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// SETUP-10 AC-7: the SystemAdmin edits the shared environment — the country preset, the unit system and the
/// company's own overrides — as one document.
/// </summary>
/// <remarks>
/// The window only composes and checks the document; the settings page writes it through
/// <c>OgSharedSetupService</c> with the revision it read, so the save and its audit row stay in one place. An explicit
/// override wins over the preset: a person who typed <c>"units": {"system": "metric-kpa"}</c> meant it.
/// </remarks>
public sealed class OgEnvironmentDialog : Window
{
    private readonly ComboBox _country = new() { MinWidth = 240, Margin = new Thickness(0, 2, 0, 8) };
    private readonly ComboBox _unitSystem = new() { MinWidth = 240, Margin = new Thickness(0, 2, 0, 8) };
    private readonly TextBox _overrides = new()
    {
        MinWidth = 420,
        MinHeight = 140,
        AcceptsReturn = true,
        AcceptsTab = true,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        FontFamily = new FontFamily("Consolas"),
        FontSize = 12,
        Margin = new Thickness(0, 2, 0, 8),
    };

    private readonly TextBlock _status;

    public OgEnvironmentDialog(OgApplication application, string? documentJson)
    {
        ArgumentNullException.ThrowIfNull(application);

        Title = "Shared environment";
        Width = 620;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.CanResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Application.Current?.TryFindResource("BeepBackgroundBrush") as Brush ?? Brushes.White;
        FlowDirection = application.Loc.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        _status = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize = 11.5,
            Margin = new Thickness(0, 6, 0, 0),
            Foreground = Application.Current?.TryFindResource("BeepDangerBrush") as Brush ?? Brushes.Firebrick,
        };

        _country.ItemsSource = OgSetupCatalog.Countries;
        _unitSystem.ItemsSource = OgSetupCatalog.UnitSystems;

        var (storedCountry, storedUnits, storedOverrides) = OgEnvironmentDocument.Read(documentJson);

        _country.SelectedItem = OgSetupCatalog.Find(OgSetupCatalog.Countries, storedCountry)
                                ?? OgSetupCatalog.Find(OgSetupCatalog.Countries, "US")
                                ?? OgSetupCatalog.Countries.FirstOrDefault();

        _unitSystem.SelectedItem = OgSetupCatalog.Find(OgSetupCatalog.UnitSystems, storedUnits)
                                   ?? OgSetupCatalog.Find(OgSetupCatalog.UnitSystems, "oilfield")
                                   ?? OgSetupCatalog.UnitSystems.FirstOrDefault();

        _overrides.Text = storedOverrides ?? string.Empty;

        Content = BuildContent();
    }

    /// <summary>The composed document, or null until it validates.</summary>
    public string? DocumentJson { get; private set; }

    private FrameworkElement BuildContent()
    {
        var panel = new StackPanel { Margin = new Thickness(20) };

        panel.Children.Add(new TextBlock
        {
            Text = "Shared environment",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 6),
            Foreground = Application.Current?.TryFindResource("BeepTextPrimaryBrush") as Brush ?? Brushes.Black,
        });

        panel.Children.Add(Caption(
            "Every user of this database reads this document at sign-in: the country preset, the unit system and any " +
            "overrides your company needs. The presets ship with the application, so the document records the choice " +
            "and the application supplies the values."));

        panel.Children.Add(Caption("Country preset"));
        panel.Children.Add(_country);
        panel.Children.Add(Caption("Unit system"));
        panel.Children.Add(_unitSystem);
        panel.Children.Add(Caption("Overrides (JSON object, optional)"));
        panel.Children.Add(_overrides);
        panel.Children.Add(Caption(
            "An override wins over the preset. For example {\"printing\": {\"paper\": \"A3\"}} changes the paper, and " +
            "{\"units\": {\"system\": \"metric-kpa\"}} replaces the unit system chosen above. Leave it " +
            "empty to use the preset as it ships."));
        panel.Children.Add(_status);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };

        var save = new Button
        {
            Content = "Save the environment",
            IsDefault = true,
            Padding = new Thickness(16, 5, 16, 5),
            MinWidth = 160,
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

        return panel;
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        _status.Text = string.Empty;

        var composed = OgEnvironmentDocument.Compose(
            (_country.SelectedItem as OgSetupChoice)?.Code,
            (_unitSystem.SelectedItem as OgSetupChoice)?.Code,
            _overrides.Text);

        if (!composed.Success || composed.Value is null)
        {
            _status.Text = composed.Message ?? "The environment is not usable.";
            return;
        }

        DocumentJson = composed.Value;
        DialogResult = true;
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
