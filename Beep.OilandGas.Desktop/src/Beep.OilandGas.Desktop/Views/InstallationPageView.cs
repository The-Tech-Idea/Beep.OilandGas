using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Release;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// REL-01: the installation's health - every runtime asset and where it comes from - and the two decisions an installer leaves to a person:
/// whether to keep the data root, and whether this run is an upgrade.
/// </summary>
/// <remarks>
/// The page is Settings → Diagnostics → Verify installation, and it answers for the thing that needs each asset rather than for a list of
/// file names (AC-3): the country presets are counted from the engine, the geomagnetic model from the embedded resource it reads, the native
/// Skia library from the file that actually ships. It prints the install folder and the writable root side by side, because AC-2 is a
/// statement about those two folders (nothing is written beside the executable). Removing the data root needs the exact words, and the
/// warning names the failure store and the logs first (AC-4); an upgrade plan names what it must keep and writes nothing (AC-5).
/// </remarks>
public sealed class InstallationPageView : OgPageBase
{
    public const string Route = "installation";

    private readonly OgApplication _application;
    private readonly IOgInstallation _installation;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _location;
    private readonly TextBlock _missing;
    private readonly TextBlock _warnings;
    private readonly TextBlock _inventory;
    private readonly TextBlock _purgeWarning;
    private readonly TextBlock _upgrade;
    private readonly TextBlock _preserved;
    private readonly TextBlock _report;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _confirmation;
    private readonly DataGrid _assetGrid;
    private readonly Button _verifyButton;
    private readonly Button _reportButton;
    private readonly Button _purgeButton;
    private readonly Button _planButton;
    private readonly Button _recordButton;

    private OgInstallReport? _checked;
    private OgDataRootInventory? _inventory_;
    private OgUpgradePlan? _plan;

    public InstallationPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _installation = application.Installation;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.FontWeight = FontWeights.SemiBold;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _location = Body(string.Empty);
        _location.TextWrapping = TextWrapping.Wrap;
        _location.FontFamily = new System.Windows.Media.FontFamily("Consolas");
        _location.Margin = new Thickness(0, 0, 0, 4);

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 0, 0, 4);

        _warnings = Body(string.Empty);
        _warnings.TextWrapping = TextWrapping.Wrap;
        _warnings.Margin = new Thickness(0, 0, 0, 4);

        _inventory = Body(string.Empty);
        _inventory.TextWrapping = TextWrapping.Wrap;
        _inventory.FontFamily = new System.Windows.Media.FontFamily("Consolas");
        _inventory.Margin = new Thickness(0, 0, 0, 4);

        _purgeWarning = Body(string.Empty);
        _purgeWarning.TextWrapping = TextWrapping.Wrap;
        _purgeWarning.FontWeight = FontWeights.SemiBold;
        _purgeWarning.Margin = new Thickness(0, 0, 0, 4);

        _upgrade = Body(string.Empty);
        _upgrade.TextWrapping = TextWrapping.Wrap;
        _upgrade.Margin = new Thickness(0, 4, 0, 4);

        _preserved = Body(string.Empty);
        _preserved.TextWrapping = TextWrapping.Wrap;
        _preserved.Margin = new Thickness(0, 0, 0, 4);

        _report = Body(string.Empty);
        _report.TextWrapping = TextWrapping.Wrap;
        _report.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        // AC-4: the words that must be typed are shown, and are the path itself - so a person cannot remove a folder they did not mean.
        _confirmation = new TextBox
        {
            MinWidth = 420,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center,
            ToolTip = "Type the exact sentence shown above to remove the data root",
        };

        Named(_confirmation, "Confirmation");

        _assetGrid = Grid("Installation assets", ("Asset", "Name", 240), ("Source", "KindText", 80),
            ("Required", "Requirement", 80), ("Status", "StatusText", 110), ("Found", "Count", 60),
            ("Consequence if missing", "Consequence", 380));

        _verifyButton = Button("Verify installation", (_, _) => Verify());
        _reportButton = Button("Write the report", (_, _) => _ = ReportAsync());
        _purgeButton = Button("Remove the data root", (_, _) => _ = PurgeAsync());
        _planButton = Button("Plan the upgrade", (_, _) => Plan());
        _recordButton = Button("Record this version", (_, _) => _ = RecordAsync());

        var verifyBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        verifyBar.Children.Add(_verifyButton);
        verifyBar.Children.Add(_reportButton);
        verifyBar.Children.Add(Caption("Verify installation is also the Settings → Diagnostics command."));

        var upgradeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        upgradeBar.Children.Add(_planButton);
        upgradeBar.Children.Add(_recordButton);

        var purgeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        purgeBar.Children.Add(Caption("Type to confirm"));
        purgeBar.Children.Add(_confirmation);
        purgeBar.Children.Add(_purgeButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(verifyBar);
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("Where it is installed"));
        panel.Children.Add(_location);
        panel.Children.Add(Caption("Runtime assets"));
        panel.Children.Add(_assetGrid);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Warnings"));
        panel.Children.Add(_warnings);
        panel.Children.Add(Caption("Upgrade"));
        panel.Children.Add(upgradeBar);
        panel.Children.Add(_upgrade);
        panel.Children.Add(_preserved);
        panel.Children.Add(Caption("Uninstall - the data root"));
        panel.Children.Add(_inventory);
        panel.Children.Add(_purgeWarning);
        panel.Children.Add(purgeBar);
        panel.Children.Add(_report);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Verify the installation to list what is present and what would stop working.");
    }

    public OgInstallReport? Report => _checked;

    public OgDataRootInventory? Inventory => _inventory_;

    public OgUpgradePlan? Upgrade => _plan;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string LocationText => _location.Text;

    public string MissingText => _missing.Text;

    public string WarningsText => _warnings.Text;

    public string InventoryText => _inventory.Text;

    public string PurgeWarningText => _purgeWarning.Text;

    public string UpgradeText => _upgrade.Text;

    public string PreservedText => _preserved.Text;

    public string ReportText => _report.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    /// <summary>AC-3: the assets the page lists, in the order the checklist states them.</summary>
    public IReadOnlyList<OgInstallAsset> Assets => _checked?.Assets ?? [];

    public bool Healthy => _checked?.Healthy ?? false;

    public bool Load()
    {
        _header.Text = "Administration · installation";

        var verified = Verify();

        AutoFill();

        var inventory = _installation.InventoryAsync().GetAwaiter().GetResult();

        if (!inventory.Success || inventory.Value is null)
        {
            LastFailed = true;
            _failure.Text = inventory.Message ?? "The data root could not be measured.";
            return false;
        }

        _inventory_ = inventory.Value;
        _inventory.Text = inventory.Value.Describe() + Environment.NewLine + inventory.Value.DetailText;
        _purgeWarning.Text = OgInstallRules.PurgeWarning(inventory.Value) + Environment.NewLine +
                             "Type: " + OgInstallRules.ConfirmationToken(inventory.Value.Root);

        Plan();

        return verified;
    }

    /// <summary>AC-2 and AC-3: the check, with every asset and what stops working without it.</summary>
    public bool Verify()
    {
        var checkedNow = _installation.CheckAsync().GetAwaiter().GetResult();

        if (!checkedNow.Success || checkedNow.Value is null)
        {
            LastFailed = true;
            _checked = null;
            _assetGrid.ItemsSource = null;
            _summary.Text = string.Empty;
            _location.Text = string.Empty;
            _missing.Text = string.Empty;
            _failure.Text = checkedNow.Message ?? "The installation could not be checked.";
            ShowState(OgViewState.Failed, "The installation is not checked.", checkedNow.Reference);
            return false;
        }

        LastFailed = false;
        _checked = checkedNow.Value;

        _summary.Text = checkedNow.Value.SummaryText;
        _location.Text = checkedNow.Value.LocationText;
        _missing.Text = checkedNow.Value.MissingText;
        _warnings.Text = checkedNow.Value.WarningsText;
        _refreshed.Text = checkedNow.Value.CheckedText;

        _assetGrid.ItemsSource = checkedNow.Value.Assets.ToList();

        _failure.Text = string.Empty;
        ShowState(checkedNow.Value.Healthy ? OgViewState.Ready : OgViewState.Failed,
            checkedNow.Value.SummaryText, checkedNow.Value.Healthy ? null : checkedNow.Value.MissingText);

        return checkedNow.Value.Healthy;
    }

    public async Task<bool> ReportAsync()
    {
        var written = await _installation.WriteReportAsync(_application.Session.UserId).ConfigureAwait(true);

        if (!written.Success || written.Value is null)
        {
            LastFailed = true;
            _report.Text = written.Message ?? "The report was not written.";
            _failure.Text = written.Message ?? "The report was not written.";
            return false;
        }

        LastFailed = false;
        _report.Text = "report written to " + written.Value;
        _failure.Text = string.Empty;

        _application.AuditLog.Add("release.installation.report", "release", written.Value, "ok",
            _application.Session.UserId);

        _notice.Text = "The installation report is beside the failure store, not beside the executable.";

        return true;
    }

    public bool Plan()
    {
        var planned = _installation.PlanUpgradeAsync().GetAwaiter().GetResult();

        if (!planned.Success || planned.Value is null)
        {
            LastFailed = true;
            _upgrade.Text = planned.Message ?? "The upgrade could not be planned.";
            return false;
        }

        LastFailed = false;
        _plan = planned.Value;

        _upgrade.Text = planned.Value.Describe() + " · " + planned.Value.DataRoot +
                        (planned.Value.Kind == OgUpgradePlan.Fresh
                            ? " · no version is recorded, so this is a first run"
                            : string.Empty);
        _preserved.Text = planned.Value.PreservedText;

        return true;
    }

    public async Task<bool> RecordAsync()
    {
        var recorded = await _installation.RecordAsync(_application.Session.UserId).ConfigureAwait(true);

        if (!recorded.Success || recorded.Value is null)
        {
            LastFailed = true;
            _failure.Text = recorded.Message ?? "The version was not recorded.";
            return false;
        }

        LastFailed = false;

        _application.AuditLog.Add("release.installation.record", "release", recorded.Value.Version, "ok",
            _application.Session.UserId);

        _notice.Text = "Recorded " + recorded.Value.Describe() + ".";
        _failure.Text = string.Empty;

        Plan();

        return true;
    }

    public async Task<bool> PurgeAsync()
    {
        var purged = await _installation.PurgeDataRootAsync(_confirmation.Text ?? string.Empty).ConfigureAwait(true);

        if (!purged.Success || purged.Value is null)
        {
            // The refusal is the feature working: it names the failure store and the logs and asks for the exact words.
            LastFailed = true;
            _failure.Text = purged.Message ?? "The data root was not removed.";
            return false;
        }

        LastFailed = false;

        _application.AuditLog.Add("release.installation.purge", "release", purged.Value.Root, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = purged.Value.Describe() + ". The install folder is untouched; close the application to finish.";
        _inventory.Text = "The data root is gone.";

        return true;
    }

    private void AutoFill()
    {
        if (_inventory_ is not null && string.IsNullOrWhiteSpace(_confirmation.Text))
        {
            _confirmation.Text = string.Empty;
        }
    }

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 140,
            MaxHeight = 320,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
        };

        foreach (var (header, path, width) in columns)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(path),
                Width = new DataGridLength(width),
            });
        }

        Named(grid, name);
        return grid;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 6, 0, 0),
        FontSize = 11.5,
        FontWeight = FontWeights.SemiBold,
    };

    private static Button Button(string text, RoutedEventHandler onClick)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(10, 3, 10, 3),
            Margin = new Thickness(0, 0, 8, 0),
        };

        button.Click += onClick;
        return button;
    }
}
