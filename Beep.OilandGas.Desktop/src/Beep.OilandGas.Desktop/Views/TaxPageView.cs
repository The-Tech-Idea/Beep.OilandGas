using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ACC-08: the tax workbench - production tax, current and deferred provision, computed from the configured rates and posted
/// once each.
/// </summary>
/// <remarks>
/// A computation names its jurisdiction, rate and effective date (AC-1); a missing rate blocks it (AC-2); a provision is
/// refused while its fiscal year is incomplete (AC-3); posting writes exactly one journal per computation (AC-4); the rate
/// table is read-only here and rates are never entered on this screen (AC-5); and the author cannot post their own
/// computation (AC-6).
/// </remarks>
public sealed class TaxPageView : OgPageBase
{
    public const string Route = "tax";

    private readonly OgApplication _application;
    private readonly IOgTax _tax;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _fiscalYear;
    private readonly TextBlock _reference;
    private readonly TextBlock _rates;
    private readonly TextBlock _summary;
    private readonly TextBlock _rateText;
    private readonly TextBlock _posting;
    private readonly TextBlock _journal;
    private readonly TextBlock _deferredText;
    private readonly TextBox _periodFrom;
    private readonly TextBox _periodTo;
    private readonly TextBox _grossRevenue;
    private readonly TextBox _taxableIncome;
    private readonly TextBox _deferredAsset;
    private readonly TextBox _deferredLiability;
    private readonly TextBox _property;
    private readonly ComboBox _jurisdiction;
    private readonly ComboBox _kind;
    private readonly ComboBox _currency;
    private readonly DataGrid _jurisdictionGrid;
    private readonly DataGrid _computationGrid;
    private readonly DataGrid _deferredGrid;
    private readonly Button _computeButton;
    private readonly Button _postButton;

    private OgTaxWorkspace? _workspace;
    private OgTaxComputation? _computation;

    public TaxPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _tax = application.Tax;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 4, 0, 4);

        _fiscalYear = Body(string.Empty);
        _fiscalYear.TextWrapping = TextWrapping.Wrap;
        _fiscalYear.Margin = new Thickness(0, 4, 0, 4);

        _reference = Body(string.Empty);
        _reference.TextWrapping = TextWrapping.Wrap;
        _reference.Margin = new Thickness(0, 0, 0, 4);

        _rates = Body(string.Empty);
        _rates.TextWrapping = TextWrapping.Wrap;
        _rates.Margin = new Thickness(0, 4, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _rateText = Body(string.Empty);
        _rateText.TextWrapping = TextWrapping.Wrap;
        _rateText.Margin = new Thickness(0, 0, 0, 4);

        _posting = Body(string.Empty);
        _posting.TextWrapping = TextWrapping.Wrap;
        _posting.Margin = new Thickness(0, 0, 0, 4);

        _journal = Body(string.Empty);
        _journal.TextWrapping = TextWrapping.Wrap;
        _journal.Margin = new Thickness(0, 0, 0, 4);

        _deferredText = Body(string.Empty);
        _deferredText.TextWrapping = TextWrapping.Wrap;
        _deferredText.Margin = new Thickness(0, 4, 0, 4);

        _periodFrom = Field("Period from yyyy-MM-dd", 160);
        _periodTo = Field("Period to yyyy-MM-dd", 150);
        _grossRevenue = Field("Gross revenue", 120);
        _taxableIncome = Field("Taxable income", 120);
        _deferredAsset = Field("Closing deferred tax asset", 170);
        _deferredLiability = Field("Closing deferred tax liability", 180);
        _property = Field("Property id", 110);

        _jurisdiction = Picker("Jurisdiction", 160);
        _kind = Picker("Tax kind", 200);
        _currency = Picker("Currency", 100);

        _jurisdictionGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Jurisdiction", "Jurisdiction", 140),
                Text("Severance %", "SeveranceRate", 110),
                Text("Ad valorem %", "AdValoremRate", 110),
                Text("Income %", "IncomeTaxRate", 100),
                Text("Effective from", "EffectiveFrom", 120),
                Text("Effective to", "EffectiveTo", 120),
            },
        };

        Named(_jurisdictionGrid, "Configured rates by jurisdiction");
        _jurisdictionGrid.SelectionChanged += (_, _) => SelectJurisdiction();

        _computationGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 180,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Kind", "Kind", 160),
                Text("Jurisdiction", "Jurisdiction", 120),
                Text("Period", "PeriodText", 190),
                Text("Basis", "Basis", 110),
                Text("Rate %", "RatePercent", 90),
                Text("Tax", "Amount", 110),
                Text("Status", "Status", 90),
            },
        };

        Named(_computationGrid, "Tax computations");
        _computationGrid.SelectionChanged += (_, _) => SelectComputation();

        _deferredGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 80,
            MaxHeight = 130,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Period end", "PeriodEnd", 120),
                Text("Asset", "Asset", 110),
                Text("Liability", "Liability", 110),
                Text("Net", "Net", 110),
            },
        };

        Named(_deferredGrid, "Deferred tax balances");

        _computeButton = Button("Compute", (_, _) => _ = ComputeAsync());
        _postButton = Button("Post", (_, _) => _ = PostAsync());
        _postButton.IsEnabled = false;

        var periodBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        periodBar.Children.Add(Caption("Jurisdiction"));
        periodBar.Children.Add(_jurisdiction);
        periodBar.Children.Add(Caption("Kind"));
        periodBar.Children.Add(_kind);
        periodBar.Children.Add(_currency);

        var periodBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        periodBar2.Children.Add(Caption("From"));
        periodBar2.Children.Add(_periodFrom);
        periodBar2.Children.Add(Caption("To"));
        periodBar2.Children.Add(_periodTo);
        periodBar2.Children.Add(Caption("Property"));
        periodBar2.Children.Add(_property);

        var basisBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        basisBar.Children.Add(Caption("Gross revenue"));
        basisBar.Children.Add(_grossRevenue);
        basisBar.Children.Add(Caption("Taxable income"));
        basisBar.Children.Add(_taxableIncome);

        var deferredBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        deferredBar.Children.Add(Caption("Deferred asset"));
        deferredBar.Children.Add(_deferredAsset);
        deferredBar.Children.Add(Caption("Deferred liability"));
        deferredBar.Children.Add(_deferredLiability);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_computeButton);
        actionBar.Children.Add(_postButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_fiscalYear);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Computation"));
        panel.Children.Add(periodBar);
        panel.Children.Add(periodBar2);
        panel.Children.Add(Caption("Basis"));
        panel.Children.Add(basisBar);
        panel.Children.Add(Caption("Deferred balances (closing)"));
        panel.Children.Add(deferredBar);
        panel.Children.Add(_missing);
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Configured rates - read only; rates are configuration, not a screen input"));
        panel.Children.Add(_rates);
        panel.Children.Add(_jurisdictionGrid);
        panel.Children.Add(Caption("Computations"));
        panel.Children.Add(_computationGrid);
        panel.Children.Add(Caption("Selected computation"));
        panel.Children.Add(_summary);
        panel.Children.Add(_rateText);
        panel.Children.Add(_posting);
        panel.Children.Add(_journal);
        panel.Children.Add(Caption("Deferred balances carried"));
        panel.Children.Add(_deferredText);
        panel.Children.Add(_deferredGrid);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _periodFrom, _periodTo, _grossRevenue, _taxableIncome, _deferredAsset, _deferredLiability,
                     _property,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _kind.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgTaxWorkspace? Workspace => _workspace;

    public OgTaxComputation? Computation => _computation;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string FiscalYearText => _fiscalYear.Text;

    public string ReferenceText => _reference.Text;

    public string RatesText => _rates.Text;

    public string SummaryText => _summary.Text;

    public string RateLineText => _rateText.Text;

    public string PostingText => _posting.Text;

    public string JournalText => _journal.Text;

    public string DeferredText => _deferredText.Text;

    public bool CanCompute => TaxRules.Missing(Inputs()).Count == 0;

    public bool CanPost => _computation is { IsPosted: false };

    public bool FiscalYearComplete => _workspace?.FiscalYearComplete ?? false;

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    private OgTaxContext Context => OgTaxContext.FromSession(_application.Session);

    public bool Load()
    {
        var loaded = _tax.LoadAsync(Context).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _computationGrid.ItemsSource = null;
            _header.Text = "Tax";
            _notice.Text = loaded.Message ?? "The tax computations could not be read.";
            ShowState(OgViewState.Failed, "The tax computations could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public Task<bool> ComputeAsync() =>
        RunAsync(() => _tax.ComputeAsync(Inputs(), Context, _application.Session.UserId),
            "accounting.tax.post", "Tax computed");

    public Task<bool> PostAsync()
    {
        if (_computation is not { } computation)
        {
            _notice.Text = "Select the computation to post.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _tax.PostAsync(computation.Id, Context, _application.Session.UserId),
            "accounting.tax.post", "Posted - one journal written");
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgTaxWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _notice.Text = outcome.Message ?? "The tax command was refused.";
            ShowState(OgViewState.Failed, "The tax command was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(outcome.Value);

        _application.AuditLog.Add(
            auditAction,
            "accounting",
            outcome.Value.SelectedComputation?.Jurisdiction ?? "(tax)",
            "ok",
            _application.Session.UserId);

        _notice.Text = done + ". " + outcome.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    private void Apply(OgTaxWorkspace workspace)
    {
        _workspace = workspace;

        _kind.ItemsSource = workspace.Kinds.ToList();
        _kind.SelectedIndex = _kind.SelectedIndex < 0 ? 0 : _kind.SelectedIndex;

        _currency.ItemsSource = workspace.Currencies.ToList();
        _currency.SelectedIndex = _currency.SelectedIndex < 0 ? 0 : _currency.SelectedIndex;

        _jurisdiction.ItemsSource = workspace.Jurisdictions.Select(row => row.Jurisdiction).ToList();
        _jurisdiction.SelectedIndex = _jurisdiction.SelectedIndex < 0 && workspace.Jurisdictions.Count > 0
            ? 0
            : _jurisdiction.SelectedIndex;

        _jurisdictionGrid.ItemsSource = workspace.Jurisdictions.ToList();
        _computationGrid.ItemsSource = workspace.Computations.ToList();
        _deferredGrid.ItemsSource = workspace.Deferred.ToList();

        _header.Text = "Accounting · tax";
        _fiscalYear.Text = (workspace.FiscalYearComplete ? "Fiscal year: " : "Fiscal year BLOCKS provisions: ") +
                           workspace.FiscalYearText;
        _reference.Text = workspace.ReferenceText;
        _rates.Text = workspace.RateTableText;
        _notice.Text = workspace.HistoryFailed
            ? "The tax computations are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText + " · " + workspace.UnpostedText;
        _deferredText.Text = workspace.DeferredText;

        _computation = workspace.SelectedComputation;
        ShowComputation();
        RefreshEnabled();
    }

    private void ShowComputation()
    {
        if (_computation is not { } computation)
        {
            _summary.Text = "no computation is selected";
            _rateText.Text = string.Empty;
            _posting.Text = string.Empty;
            _journal.Text = string.Empty;
            return;
        }

        _summary.Text = computation.Describe() + " · " + computation.BasisDescription;
        _rateText.Text = "Rate: " + computation.RateText +
                         (computation.NamesItsBasis ? string.Empty : " · INCOMPLETE - the rate or its date is missing");
        _posting.Text = "Posting: " + (_workspace?.PostingPreviewText ?? string.Empty);
        _journal.Text = computation.JournalText;
    }

    private void SelectJurisdiction()
    {
        if (_jurisdictionGrid.SelectedItem is not OgTaxJurisdiction selected)
        {
            return;
        }

        _jurisdiction.SelectedItem = selected.Jurisdiction;
        RefreshEnabled();
    }

    private void SelectComputation()
    {
        if (_computationGrid.SelectedItem is not OgTaxComputation selected)
        {
            return;
        }

        _computation = selected;
        ShowComputation();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var missing = TaxRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The computation needs " + string.Join(", ", missing) + ".";

        _computeButton.IsEnabled = missing.Count == 0;
        _postButton.IsEnabled = CanPost;
    }

    private OgTaxInputs Inputs() =>
        new(
            _jurisdiction.SelectedItem as string,
            _kind.SelectedItem as string,
            Date(_periodFrom.Text),
            Date(_periodTo.Text),
            _currency.SelectedItem as string,
            Parse(_grossRevenue.Text),
            Parse(_taxableIncome.Text),
            Parse(_deferredAsset.Text),
            Parse(_deferredLiability.Text),
            Empty(_property.Text));

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal? Parse(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    private static DateTime? Date(string? text) =>
        DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : null;

    private static TextBox Field(string name, double width)
    {
        var box = new TextBox
        {
            MinWidth = width,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = name,
        };

        Named(box, name);
        return box;
    }

    private static ComboBox Picker(string name, double width)
    {
        var picker = new ComboBox
        {
            MinWidth = width,
            Margin = new Thickness(0, 0, 8, 0),
        };

        Named(picker, name);
        return picker;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
        TextWrapping = TextWrapping.Wrap,
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

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };
}
