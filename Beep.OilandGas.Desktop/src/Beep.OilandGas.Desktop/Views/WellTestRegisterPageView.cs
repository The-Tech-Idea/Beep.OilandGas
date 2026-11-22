using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.WellTest;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// WTA-02: the well test register - every test with when, how, by whom, with what result, and which analyses were made
/// from it.
/// </summary>
/// <remarks>
/// A test belongs to exactly one well and may carry several analyses (AC-1). The type and result codes are read from
/// <c>R_WELL_TEST_TYPE</c> and <c>R_TEST_RESULT</c> (AC-2), a duplicate well+date+type is refused (AC-3), a test with no
/// result code is rendered as <b>pending</b> and never as a failure (AC-4), every rate and pressure carries its unit (AC-5),
/// and a test with analyses cannot be deleted (AC-6).
/// </remarks>
public sealed class WellTestRegisterPageView : OgPageBase
{
    public const string Route = "welltestregister";

    private readonly OgApplication _application;
    private readonly IOgWellTestRegister _register;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _summary;
    private readonly TextBlock _detail;
    private readonly TextBox _well;
    private readonly TextBox _date;
    private readonly ComboBox _type;
    private readonly TextBox _runNumber;
    private readonly TextBox _testNumber;
    private readonly TextBox _duration;
    private readonly ComboBox _durationUnit;
    private readonly ComboBox _resultCode;
    private readonly TextBox _productionMethod;
    private readonly TextBox _remarks;
    private readonly TextBox _oilRate;
    private readonly TextBox _oilRateUnit;
    private readonly TextBox _gasRate;
    private readonly TextBox _gasRateUnit;
    private readonly TextBox _pressure;
    private readonly TextBox _pressureUnit;
    private readonly DataGrid _tests;
    private readonly Button _saveButton;
    private readonly Button _deleteButton;

    private OgWellTestRegisterWorkspace? _workspace;
    private OgWellTestSummary? _selected;
    private string? _editingId;

    public WellTestRegisterPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _register = application.WellTestRegister;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 4, 0, 4);

        _reference = Body(string.Empty);
        _reference.TextWrapping = TextWrapping.Wrap;
        _reference.Margin = new Thickness(0, 0, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _well = Field("Well id", 120);
        _date = Field("Test date yyyy-MM-dd", 140);
        _date.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _runNumber = Field("Run number", 100);
        _testNumber = Field("Test number", 100);
        _testNumber.Text = "1";
        _duration = Field("Duration", 100);
        _productionMethod = Field("Production method", 140);
        _remarks = Field("Remarks", 240);
        _oilRate = Field("Oil rate", 100);
        _oilRateUnit = Field("Oil rate unit", 110);
        _oilRateUnit.Text = "bbl/d";
        _gasRate = Field("Gas rate", 100);
        _gasRateUnit = Field("Gas rate unit", 110);
        _gasRateUnit.Text = "Mscf/d";
        _pressure = Field("Flowing pressure", 120);
        _pressureUnit = Field("Pressure unit", 110);
        _pressureUnit.Text = "psia";

        _type = Picker("Test type", 160);
        _durationUnit = Picker("Duration unit", 110);
        _resultCode = Picker("Result code", 150);

        _tests = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 130,
            MaxHeight = 220,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Well", "WellId", 110),
                Text("Date", "TestDate", 100),
                Text("Type", "TypeText", 180),
                Text("Duration", "DurationText", 110),
                Text("Result", "StatusText", 180),
                Text("Rates", "RatesText", 240),
                Text("Analyses", "AnalysisText", 190),
                Text("By", "Actor", 90),
            },
        };

        Named(_tests, "Registered well tests");
        _tests.SelectionChanged += (_, _) => SelectTest();

        _saveButton = Button("Save test", (_, _) => _ = SaveAsync());
        _deleteButton = Button("Delete test", (_, _) => _ = DeleteAsync());
        _deleteButton.IsEnabled = false;

        var identityBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        identityBar.Children.Add(Caption("Well"));
        identityBar.Children.Add(_well);
        identityBar.Children.Add(Caption("Date"));
        identityBar.Children.Add(_date);
        identityBar.Children.Add(Caption("Type"));
        identityBar.Children.Add(_type);

        var keyBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        keyBar.Children.Add(Caption("Run"));
        keyBar.Children.Add(_runNumber);
        keyBar.Children.Add(Caption("Test"));
        keyBar.Children.Add(_testNumber);
        keyBar.Children.Add(Caption("Duration"));
        keyBar.Children.Add(_duration);
        keyBar.Children.Add(_durationUnit);

        var resultBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        resultBar.Children.Add(Caption("Result"));
        resultBar.Children.Add(_resultCode);
        resultBar.Children.Add(Caption("Method"));
        resultBar.Children.Add(_productionMethod);
        resultBar.Children.Add(Caption("Remarks"));
        resultBar.Children.Add(_remarks);

        var rateBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        rateBar.Children.Add(Caption("Oil"));
        rateBar.Children.Add(_oilRate);
        rateBar.Children.Add(_oilRateUnit);
        rateBar.Children.Add(Caption("Gas"));
        rateBar.Children.Add(_gasRate);
        rateBar.Children.Add(_gasRateUnit);
        rateBar.Children.Add(Caption("Flowing"));
        rateBar.Children.Add(_pressure);
        rateBar.Children.Add(_pressureUnit);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_saveButton);
        actionBar.Children.Add(_deleteButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Identify the test"));
        panel.Children.Add(identityBar);
        panel.Children.Add(Caption("Run and duration"));
        panel.Children.Add(keyBar);
        panel.Children.Add(Caption("Result"));
        panel.Children.Add(resultBar);
        panel.Children.Add(Caption("Conditions"));
        panel.Children.Add(rateBar);
        panel.Children.Add(_missing);
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Selected test"));
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("Registered tests"));
        panel.Children.Add(_tests);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _well, _date, _runNumber, _testNumber, _duration, _productionMethod, _remarks,
                     _oilRate, _oilRateUnit, _gasRate, _gasRateUnit, _pressure, _pressureUnit,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _type.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgWellTestRegisterWorkspace? Workspace => _workspace;

    public OgWellTestSummary? Test => _selected;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string SummaryText => _summary.Text;

    public string DetailText => _detail.Text;

    public string? EditingId => _editingId;

    public bool CanSave => WellTestRegisterRules.Missing(Entry()).Count == 0;

    public bool CanDelete => _selected is { HasAnalysis: false };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _register.LoadAsync(_application.Session.FieldId).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _tests.ItemsSource = null;
            _header.Text = "Well test register";
            _notice.Text = loaded.Message ?? "The register could not be read.";
            ShowState(OgViewState.Failed, "The register could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        var entry = Entry();

        if (WellTestRegisterRules.Missing(entry).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var saved = await _register.SaveAsync(entry, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _notice.Text = saved.Message ?? "The test was not registered.";
            ShowState(OgViewState.Failed, "The test was not registered.", saved.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(saved.Value);

        _application.AuditLog.Add(
            _editingId is null ? "welltest.register.create" : "welltest.register.update",
            "welltest",
            entry.WellId ?? "(no well)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Registered. " + saved.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    public async Task<bool> DeleteAsync()
    {
        if (_selected is not { } test)
        {
            _notice.Text = "Select the test to delete.";
            return false;
        }

        var deleted = await _register.DeleteAsync(test.RegisterId, _application.Session.UserId)
            .ConfigureAwait(true);

        if (!deleted.Success || deleted.Value is null)
        {
            LastFailed = true;
            _notice.Text = deleted.Message ?? "The test was not deleted.";
            ShowState(OgViewState.Failed, "The test was not deleted.", deleted.Reference);
            RefreshEnabled();
            return false;
        }

        _application.AuditLog.Add(
            "welltest.register.delete",
            "welltest",
            test.WellId ?? "(no well)",
            "ok",
            _application.Session.UserId);

        Apply(deleted.Value);
        _editingId = null;
        _notice.Text = "Deleted. " + deleted.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    /// <summary>Puts the selected test into the editor, so an edit changes that entry rather than adding a second one.</summary>
    public void EditSelected()
    {
        if (_selected is not { } test)
        {
            return;
        }

        _editingId = test.RegisterId;
        _well.Text = test.WellId ?? string.Empty;
        _date.Text = test.TestDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _resultCode.SelectedItem = test.ResultCode;
        _duration.Text = test.Duration?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
        _durationUnit.SelectedItem = test.DurationUnit;
        _productionMethod.Text = test.ProductionMethod ?? string.Empty;
        _remarks.Text = test.Remarks ?? string.Empty;
        _oilRate.Text = test.OilRate?.ToString("0.###", CultureInfo.InvariantCulture) ?? string.Empty;
        _oilRateUnit.Text = test.OilRateUnit ?? string.Empty;
        _gasRate.Text = test.GasRate?.ToString("0.###", CultureInfo.InvariantCulture) ?? string.Empty;
        _gasRateUnit.Text = test.GasRateUnit ?? string.Empty;
        _pressure.Text = test.FlowingPressure?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
        _pressureUnit.Text = test.FlowingPressureUnit ?? string.Empty;

        _notice.Text = "Editing " + test.Describe();
        RefreshEnabled();
    }

    private void Apply(OgWellTestRegisterWorkspace workspace)
    {
        _workspace = workspace;

        _type.ItemsSource = workspace.TestTypes.ToList();
        _type.SelectedIndex = _type.SelectedIndex < 0 && workspace.TestTypes.Count > 0 ? 0 : _type.SelectedIndex;

        _durationUnit.ItemsSource = workspace.DurationUnits.ToList();
        _durationUnit.SelectedIndex = _durationUnit.SelectedIndex < 0 ? 0 : _durationUnit.SelectedIndex;

        _resultCode.ItemsSource = workspace.ResultCodes.ToList();
        _resultCode.SelectedIndex = -1;

        _tests.ItemsSource = workspace.Tests.ToList();

        _header.Text = "Well test register · every test and the analyses made from it";
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The register is unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;

        _selected = workspace.Selected;
        ShowSelected();
        RefreshEnabled();
    }

    private void ShowSelected()
    {
        if (_selected is not { } test)
        {
            _summary.Text = "no test is selected";
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text = test.Describe();
        _detail.Text =
            "units: " + test.RatesText + " · " + test.AnalysisText + " · test key " + test.TestKey +
            (test.Remarks is { Length: > 0 } remarks ? " · " + remarks : string.Empty);
    }

    private void SelectTest()
    {
        if (_tests.SelectedItem is not OgWellTestSummary selected)
        {
            return;
        }

        _selected = selected;
        _editingId = null;
        ShowSelected();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var missing = WellTestRegisterRules.Missing(Entry());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The register entry needs " + string.Join(", ", missing) + ".";

        _saveButton.IsEnabled = missing.Count == 0;
        _deleteButton.IsEnabled = CanDelete;

        if (_selected is { HasAnalysis: true })
        {
            _missing.Text = "This test has analyses linked to it, so it cannot be deleted.";
        }
    }

    private OgWellTestEntry Entry() =>
        new(
            _editingId,
            Empty(_well.Text),
            Date(_date.Text),
            _type.SelectedItem as string,
            (int?)Parse(_runNumber.Text),
            (int?)Parse(_testNumber.Text),
            Parse(_duration.Text),
            _durationUnit.SelectedItem as string,
            _resultCode.SelectedItem as string,
            Empty(_productionMethod.Text),
            Empty(_remarks.Text),
            Parse(_oilRate.Text),
            Empty(_oilRateUnit.Text),
            Parse(_gasRate.Text),
            Empty(_gasRateUnit.Text),
            Parse(_pressure.Text),
            Empty(_pressureUnit.Text));

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
