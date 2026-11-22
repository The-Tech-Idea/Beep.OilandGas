using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Beep.OilandGas.ProductionAccounting.Calculations;
using Beep.OilandGas.ProductionAccounting.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ACC-07: royalties, joint-interest billing and cost allocation - what each owner is owed, what each partner owes, and how a
/// shared cost splits, each traceable to the volume behind it.
/// </summary>
/// <remarks>
/// The royalty nets to net revenue times the rate in decimal (AC-1); a missing or non-summing ownership fraction is refused
/// naming the lease (AC-2); a disputed royalty cannot be paid and the refusal carries the dispute reference (AC-3); the JIB
/// net is its charges less its credits (AC-4); the allocation preview is the arithmetic the commit writes (AC-5); and every
/// royalty and bill carries the volume it came from (AC-6, SG-10-7).
/// </remarks>
public sealed class RoyaltiesPageView : OgPageBase
{
    public const string Route = "royalties";

    private readonly OgApplication _application;
    private readonly IOgRoyalties _royalties;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _summary;
    private readonly TextBlock _trace;
    private readonly TextBlock _payable;
    private readonly TextBlock _deductions;
    private readonly TextBlock _jibSummary;
    private readonly TextBlock _jibTrace;
    private readonly TextBlock _journal;
    private readonly TextBlock _allocationTotals;
    private readonly TextBlock _detail;
    private readonly TextBox _lease;
    private readonly TextBox _owner;
    private readonly TextBox _allocationDetail;
    private readonly TextBox _allocationResult;
    private readonly TextBox _volume;
    private readonly TextBox _price;
    private readonly TextBox _rate;
    private readonly TextBox _residual;
    private readonly TextBox _transport;
    private readonly TextBox _adValorem;
    private readonly TextBox _severance;
    private readonly TextBox _processing;
    private readonly TextBox _other;
    private readonly TextBox _disputeReason;
    private readonly TextBox _partner;
    private readonly TextBox _periodStart;
    private readonly TextBox _periodEnd;
    private readonly TextBox _sourceVolume;
    private readonly TextBox _chargeLines;
    private readonly TextBox _creditLines;
    private readonly TextBox _jibReason;
    private readonly TextBox _pool;
    private readonly TextBox _targets;
    private readonly TextBox _allocationReference;
    private readonly ComboBox _currency;
    private readonly ComboBox _volumeUnit;
    private readonly ComboBox _jibCurrency;
    private readonly ComboBox _allocationCurrency;
    private readonly ComboBox _allocationMethod;
    private readonly DataGrid _royaltyGrid;
    private readonly DataGrid _jibGrid;
    private readonly DataGrid _jibLineGrid;
    private readonly DataGrid _allocationGrid;
    private readonly Button _calculateButton;
    private readonly Button _payButton;
    private readonly Button _disputeButton;
    private readonly Button _resolveButton;
    private readonly Button _raiseJibButton;
    private readonly Button _approveJibButton;
    private readonly Button _returnJibButton;
    private readonly Button _postJibButton;
    private readonly Button _previewButton;
    private readonly Button _commitButton;

    private OgRoyaltyWorkspace? _workspace;
    private OgRoyalty? _royalty;
    private OgJib? _jib;
    private OgAllocationPreview? _preview;

    public RoyaltiesPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _royalties = application.Royalties;

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

        _trace = Body(string.Empty);
        _trace.TextWrapping = TextWrapping.Wrap;
        _trace.Margin = new Thickness(0, 0, 0, 4);

        _payable = Body(string.Empty);
        _payable.TextWrapping = TextWrapping.Wrap;
        _payable.Margin = new Thickness(0, 0, 0, 4);

        _deductions = Body(string.Empty);
        _deductions.TextWrapping = TextWrapping.Wrap;
        _deductions.Margin = new Thickness(0, 0, 0, 4);

        _jibSummary = Body(string.Empty);
        _jibSummary.TextWrapping = TextWrapping.Wrap;
        _jibSummary.Margin = new Thickness(0, 4, 0, 4);

        _jibTrace = Body(string.Empty);
        _jibTrace.TextWrapping = TextWrapping.Wrap;
        _jibTrace.Margin = new Thickness(0, 0, 0, 4);

        _journal = Body(string.Empty);
        _journal.TextWrapping = TextWrapping.Wrap;
        _journal.Margin = new Thickness(0, 0, 0, 4);

        _allocationTotals = Body(string.Empty);
        _allocationTotals.TextWrapping = TextWrapping.Wrap;
        _allocationTotals.Margin = new Thickness(0, 4, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _lease = Field("Lease id", 120);
        _owner = Field("Owner id", 110);
        _allocationDetail = Field("Allocation detail id", 150);
        _allocationResult = Field("Allocation result id", 150);
        _volume = Field("Gross volume", 110);
        _price = Field("Price per unit", 110);
        _rate = Field("Rate %", 80);
        _residual = Field("Declared residual %", 140);
        _transport = Field("Transportation", 110);
        _adValorem = Field("Ad valorem", 110);
        _severance = Field("Severance", 110);
        _processing = Field("Processing", 110);
        _other = Field("Other", 90);
        _disputeReason = Field("Dispute reason", 240);
        _partner = Field("Partner id", 120);
        _periodStart = Field("Period start yyyy-MM-dd", 160);
        _periodEnd = Field("Period end yyyy-MM-dd", 160);
        _sourceVolume = Field("Source volume reference", 190);
        _jibReason = Field("Return reason", 220);
        _pool = Field("Cost pool", 110);
        _allocationReference = Field("Pool reference", 150);

        _currency = Picker("Currency", 100);
        _volumeUnit = Picker("Unit", 90);
        _jibCurrency = Picker("Currency", 100);
        _allocationCurrency = Picker("Currency", 100);
        _allocationMethod = Picker("Method", 190);

        _chargeLines = LinesBox("Charges: description, category, amount");
        _creditLines = LinesBox("Credits: description, category, amount");
        _targets = LinesBox("Allocation targets: type, id, weight");

        _royaltyGrid = new DataGrid
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
                Text("Lease", "LeaseId", 110),
                Text("Owner", "OwnerId", 100),
                Text("Date", "Date", 100),
                Text("Volume", "Volume", 90),
                Text("Gross", "GrossValue", 100),
                Text("Net", "NetRevenue", 100),
                Text("Rate %", "RatePercent", 80),
                Text("Royalty", "Amount", 100),
                Text("Status", "Status", 90),
                Text("Payable", "PayableText", 200),
            },
        };

        Named(_royaltyGrid, "Royalty calculations");
        _royaltyGrid.SelectionChanged += (_, _) => SelectRoyalty();

        _jibGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 170,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Partner", "PartnerId", 110),
                Text("Period start", "PeriodStart", 110),
                Text("Period end", "PeriodEnd", 110),
                Text("Charges", "Charges", 100),
                Text("Credits", "Credits", 100),
                Text("Net", "Net", 100),
                Text("State", "State", 90),
                Text("Source", "SourceVolumeReference", 140),
            },
        };

        Named(_jibGrid, "Joint-interest statements");
        _jibGrid.SelectionChanged += (_, _) => SelectJib();

        _jibLineGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 80,
            MaxHeight = 140,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Kind", "Kind", 80),
                Text("Description", "Description", 220),
                Text("Category", "Category", 120),
                Text("Amount", "Amount", 110),
            },
        };

        Named(_jibLineGrid, "Charges and credits on the selected statement");

        _allocationGrid = new DataGrid
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
                Text("Type", "TargetType", 100),
                Text("Target", "TargetId", 150),
                Text("Percentage", "Percentage", 110),
                Text("Amount", "Amount", 110),
                Text("Basis", "Basis", 100),
            },
        };

        Named(_allocationGrid, "Allocation shares");

        _calculateButton = Button("Calculate royalty", (_, _) => _ = CalculateAsync());
        _payButton = Button("Pay", (_, _) => _ = PayAsync());
        _payButton.IsEnabled = false;
        _disputeButton = Button("Raise dispute", (_, _) => _ = DisputeAsync());
        _disputeButton.IsEnabled = false;
        _resolveButton = Button("Resolve dispute", (_, _) => _ = ResolveAsync());
        _resolveButton.IsEnabled = false;
        _raiseJibButton = Button("Raise statement", (_, _) => _ = RaiseJibAsync());
        _approveJibButton = Button("Approve", (_, _) => _ = ApproveJibAsync());
        _approveJibButton.IsEnabled = false;
        _returnJibButton = Button("Return", (_, _) => _ = ReturnJibAsync());
        _returnJibButton.IsEnabled = false;
        _postJibButton = Button("Post", (_, _) => _ = PostJibAsync());
        _postJibButton.IsEnabled = false;
        _previewButton = Button("Preview", (_, _) => PreviewAllocation());
        _commitButton = Button("Commit allocation", (_, _) => _ = CommitAllocationAsync());
        _commitButton.IsEnabled = false;

        var leaseBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        leaseBar.Children.Add(Caption("Lease"));
        leaseBar.Children.Add(_lease);
        leaseBar.Children.Add(Caption("Owner"));
        leaseBar.Children.Add(_owner);
        leaseBar.Children.Add(Caption("Residual %"));
        leaseBar.Children.Add(_residual);
        leaseBar.Children.Add(_currency);

        var volumeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        volumeBar.Children.Add(Caption("Volume"));
        volumeBar.Children.Add(_volume);
        volumeBar.Children.Add(_volumeUnit);
        volumeBar.Children.Add(Caption("Price"));
        volumeBar.Children.Add(_price);
        volumeBar.Children.Add(Caption("Rate %"));
        volumeBar.Children.Add(_rate);
        volumeBar.Children.Add(Caption("Detail"));
        volumeBar.Children.Add(_allocationDetail);
        volumeBar.Children.Add(Caption("Result"));
        volumeBar.Children.Add(_allocationResult);
        volumeBar.Children.Add(_calculateButton);

        var deductionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        deductionBar.Children.Add(Caption("Transport"));
        deductionBar.Children.Add(_transport);
        deductionBar.Children.Add(Caption("Ad valorem"));
        deductionBar.Children.Add(_adValorem);
        deductionBar.Children.Add(Caption("Severance"));
        deductionBar.Children.Add(_severance);
        deductionBar.Children.Add(Caption("Processing"));
        deductionBar.Children.Add(_processing);
        deductionBar.Children.Add(Caption("Other"));
        deductionBar.Children.Add(_other);

        var royaltyActionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        royaltyActionBar.Children.Add(_payButton);
        royaltyActionBar.Children.Add(Caption("Dispute"));
        royaltyActionBar.Children.Add(_disputeReason);
        royaltyActionBar.Children.Add(_disputeButton);
        royaltyActionBar.Children.Add(_resolveButton);

        var jibBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        jibBar.Children.Add(Caption("Partner"));
        jibBar.Children.Add(_partner);
        jibBar.Children.Add(Caption("From"));
        jibBar.Children.Add(_periodStart);
        jibBar.Children.Add(Caption("To"));
        jibBar.Children.Add(_periodEnd);
        jibBar.Children.Add(_jibCurrency);

        var jibBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        jibBar2.Children.Add(Caption("Source volume"));
        jibBar2.Children.Add(_sourceVolume);
        jibBar2.Children.Add(_raiseJibButton);

        var jibActionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        jibActionBar.Children.Add(_approveJibButton);
        jibActionBar.Children.Add(Caption("Reason"));
        jibActionBar.Children.Add(_jibReason);
        jibActionBar.Children.Add(_returnJibButton);
        jibActionBar.Children.Add(_postJibButton);

        var poolBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        poolBar.Children.Add(Caption("Pool"));
        poolBar.Children.Add(_pool);
        poolBar.Children.Add(_allocationCurrency);
        poolBar.Children.Add(_allocationMethod);
        poolBar.Children.Add(Caption("Reference"));
        poolBar.Children.Add(_allocationReference);
        poolBar.Children.Add(_previewButton);
        poolBar.Children.Add(_commitButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Royalty calculation"));
        panel.Children.Add(leaseBar);
        panel.Children.Add(volumeBar);
        panel.Children.Add(Caption("Deductions"));
        panel.Children.Add(deductionBar);
        panel.Children.Add(_missing);
        panel.Children.Add(royaltyActionBar);
        panel.Children.Add(Caption("Calculations"));
        panel.Children.Add(_royaltyGrid);
        panel.Children.Add(Caption("Selected royalty"));
        panel.Children.Add(_summary);
        panel.Children.Add(_trace);
        panel.Children.Add(_deductions);
        panel.Children.Add(_payable);
        panel.Children.Add(Caption("Joint-interest billing"));
        panel.Children.Add(jibBar);
        panel.Children.Add(jibBar2);
        panel.Children.Add(Caption("Charges"));
        panel.Children.Add(_chargeLines);
        panel.Children.Add(Caption("Credits"));
        panel.Children.Add(_creditLines);
        panel.Children.Add(jibActionBar);
        panel.Children.Add(Caption("Statements"));
        panel.Children.Add(_jibGrid);
        panel.Children.Add(Caption("Selected statement"));
        panel.Children.Add(_jibSummary);
        panel.Children.Add(_jibTrace);
        panel.Children.Add(_journal);
        panel.Children.Add(_jibLineGrid);
        panel.Children.Add(Caption("Cost allocation"));
        panel.Children.Add(poolBar);
        panel.Children.Add(_targets);
        panel.Children.Add(_allocationTotals);
        panel.Children.Add(_allocationGrid);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _lease, _owner, _allocationDetail, _allocationResult, _volume, _price, _rate, _residual,
                     _transport, _adValorem, _severance, _processing, _other, _disputeReason, _partner, _periodStart,
                     _periodEnd, _sourceVolume, _chargeLines, _creditLines, _jibReason, _pool, _targets,
                     _allocationReference,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        Load();
    }

    public OgRoyaltyWorkspace? Workspace => _workspace;

    public OgRoyalty? Royalty => _royalty;

    public OgJib? Jib => _jib;

    public OgAllocationPreview? Preview => _preview;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string SummaryText => _summary.Text;

    public string TraceText => _trace.Text;

    public string PayableText => _payable.Text;

    public string DeductionsText => _deductions.Text;

    public string JibSummaryText => _jibSummary.Text;

    public string JibTraceText => _jibTrace.Text;

    public string JournalText => _journal.Text;

    public string AllocationTotalsText => _allocationTotals.Text;

    public string DetailText => _detail.Text;

    public bool CanCalculate => RoyaltyRules.Missing(Inputs()).Count == 0;

    public bool CanPay => _royalty is { CanPay: true };

    public bool CanDispute => _royalty is { IsPaid: false, IsDisputed: false };

    public bool CanResolve => _royalty is { IsDisputed: true };

    public bool CanRaiseJib => JibLines("CHARGE").Count + JibLines("CREDIT").Count > 0 &&
                               !string.IsNullOrWhiteSpace(_partner.Text) &&
                               !string.IsNullOrWhiteSpace(_sourceVolume.Text);

    public bool CanApproveJib => _jib is { IsDraft: true };

    public bool CanPostJib => _jib is { IsApproved: true };

    public bool CanCommit => _preview is { AddsBack: true } &&
                             !string.IsNullOrWhiteSpace(_allocationReference.Text);

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    private OgRoyaltyContext Context => OgRoyaltyContext.FromSession(_application.Session);

    public bool Load()
    {
        var loaded = _royalties.LoadAsync(Context).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _royaltyGrid.ItemsSource = null;
            _header.Text = "Royalties and JIB";
            _notice.Text = loaded.Message ?? "The royalties could not be read.";
            ShowState(OgViewState.Failed, "The royalties could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public Task<bool> CalculateAsync() =>
        RunAsync(() => _royalties.CalculateAsync(
                Inputs(), Context, _application.Session.UserId,
                CancellationToken.None),
            "accounting.royalty.write",
            "Royalty calculated and accrued");

    public Task<bool> PayAsync()
    {
        if (_royalty is not { } royalty)
        {
            _notice.Text = "Select the royalty to pay.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _royalties.PayAsync(royalty.Id, Context, _application.Session.UserId),
            "accounting.royalty.pay", "Royalty paid");
    }

    public Task<bool> DisputeAsync()
    {
        if (_royalty is not { } royalty)
        {
            _notice.Text = "Select the royalty to dispute.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _royalties.DisputeAsync(
                royalty.Id, _disputeReason.Text ?? string.Empty, _application.Session.UserId),
            "accounting.royalty.dispute",
            "Dispute raised - the royalty cannot be paid");
    }

    public Task<bool> ResolveAsync()
    {
        if (_royalty is not { } royalty)
        {
            _notice.Text = "Select the royalty whose dispute is resolved.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _royalties.ResolveDisputeAsync(
                royalty.Id, _disputeReason.Text ?? string.Empty, _application.Session.UserId),
            "accounting.royalty.dispute",
            "Dispute resolved - the royalty may be paid");
    }

    public Task<bool> RaiseJibAsync()
    {
        var start = Date(_periodStart.Text);
        var end = Date(_periodEnd.Text);

        if (start is null || end is null)
        {
            _notice.Text = "A statement needs its period start and end.";
            return Task.FromResult(false);
        }

        var lines = JibLines("CHARGE").Concat(JibLines("CREDIT")).ToList();

        return RunAsync(() => _royalties.RaiseJibAsync(
                _partner.Text ?? string.Empty,
                start.Value,
                end.Value,
                Empty(_sourceVolume.Text),
                _jibCurrency.SelectedItem as string,
                lines,
                Context,
                _application.Session.UserId),
            "accounting.jib.post",
            "Statement raised");
    }

    public Task<bool> ApproveJibAsync() => ReviewJibAsync(true);

    public Task<bool> ReturnJibAsync() => ReviewJibAsync(false);

    private Task<bool> ReviewJibAsync(bool approve)
    {
        if (_jib is not { } jib)
        {
            _notice.Text = "Select the statement to review.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _royalties.ReviewJibAsync(
                jib.Id, approve, _jibReason.Text ?? string.Empty, _application.Session.UserId),
            "accounting.jib.post",
            approve ? "Statement approved" : "Statement returned");
    }

    public Task<bool> PostJibAsync()
    {
        if (_jib is not { } jib)
        {
            _notice.Text = "Select the statement to post.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _royalties.PostJibAsync(jib.Id, Context, _application.Session.UserId),
            "accounting.jib.post",
            "Statement posted - one journal written");
    }

    public void PreviewAllocation()
    {
        var pool = Parse(_pool.Text);

        if (pool is null)
        {
            _notice.Text = "An allocation needs its pool amount.";
            return;
        }

        var preview = _royalties.PreviewAllocation(
            pool.Value,
            _allocationCurrency.SelectedItem as string ?? string.Empty,
            Targets(),
            _allocationMethod.SelectedItem as string ?? string.Empty);

        if (!preview.Success || preview.Value is null)
        {
            LastFailed = true;
            _preview = null;
            _allocationTotals.Text = preview.Message ?? "The allocation cannot be computed.";
            _allocationGrid.ItemsSource = null;
            ShowState(OgViewState.Failed, "The allocation cannot be computed.", preview.Reference);
            RefreshEnabled();
            return;
        }

        _preview = preview.Value;
        _allocationTotals.Text = preview.Value.TotalsText;
        _allocationGrid.ItemsSource = preview.Value.Shares.ToList();
        _detail.Text = preview.Value.RowsText;
        RefreshEnabled();
    }

    public Task<bool> CommitAllocationAsync()
    {
        if (_preview is not { } preview)
        {
            _notice.Text = "Preview the allocation before committing it.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _royalties.CommitAllocationAsync(
                preview.Pool,
                preview.Currency,
                Targets(),
                _allocationMethod.SelectedItem as string ?? string.Empty,
                _allocationReference.Text ?? string.Empty,
                _application.Session.UserId),
            "accounting.stock.adjust",
            "Allocation committed");
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgRoyaltyWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _notice.Text = outcome.Message ?? "The command was refused.";
            ShowState(OgViewState.Failed, "The command was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(outcome.Value);

        _application.AuditLog.Add(
            auditAction,
            "accounting",
            outcome.Value.SelectedRoyalty?.LeaseId ?? outcome.Value.SelectedJib?.PartnerId ?? "(royalties)",
            "ok",
            _application.Session.UserId);

        _notice.Text = done + ". " + outcome.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    private void Apply(OgRoyaltyWorkspace workspace)
    {
        _workspace = workspace;

        _currency.ItemsSource = workspace.Currencies.ToList();
        _currency.SelectedIndex = _currency.SelectedIndex < 0 ? 0 : _currency.SelectedIndex;

        _jibCurrency.ItemsSource = workspace.Currencies.ToList();
        _jibCurrency.SelectedIndex = _jibCurrency.SelectedIndex < 0 ? 0 : _jibCurrency.SelectedIndex;

        _allocationCurrency.ItemsSource = workspace.Currencies.ToList();
        _allocationCurrency.SelectedIndex = _allocationCurrency.SelectedIndex < 0 ? 0 : _allocationCurrency.SelectedIndex;

        _volumeUnit.ItemsSource = RoyaltyRules.VolumeUnits.ToList();
        _volumeUnit.SelectedIndex = _volumeUnit.SelectedIndex < 0 ? 0 : _volumeUnit.SelectedIndex;

        _allocationMethod.ItemsSource = workspace.AllocationMethods.ToList();
        _allocationMethod.SelectedIndex = _allocationMethod.SelectedIndex < 0 ? 0 : _allocationMethod.SelectedIndex;

        _royaltyGrid.ItemsSource = workspace.Royalties.ToList();
        _jibGrid.ItemsSource = workspace.Jibs.ToList();

        _header.Text = "Accounting · royalties and JIB";
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The royalties are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText + " · " + workspace.PayableText;

        _royalty = workspace.SelectedRoyalty;
        _jib = workspace.SelectedJib;

        ShowRoyalty();
        ShowJib();

        if (workspace.CommittedAllocation.Count > 0)
        {
            _allocationGrid.ItemsSource = workspace.CommittedAllocation.ToList();
            _allocationTotals.Text = "committed: " + workspace.CommittedAllocationText;
        }

        RefreshEnabled();
    }

    private void ShowRoyalty()
    {
        if (_royalty is not { } royalty)
        {
            _summary.Text = "no royalty is selected";
            _trace.Text = string.Empty;
            _deductions.Text = string.Empty;
            _payable.Text = string.Empty;
            return;
        }

        _summary.Text = royalty.Describe() + " · gross " + Money(royalty.GrossValue) + " · net " +
                        Money(royalty.NetRevenue) + " · rate " +
                        royalty.RatePercent.ToString("#,0.####", CultureInfo.InvariantCulture) + " %";
        _trace.Text = "Trace: " + royalty.TraceText +
                      (string.IsNullOrWhiteSpace(royalty.JournalId) ? string.Empty : " · journal " + royalty.JournalId);
        _deductions.Text = "Deductions: " + royalty.Deductions.Describe();
        _payable.Text = "Payment: " + royalty.PayableText;
    }

    private void ShowJib()
    {
        if (_jib is not { } jib)
        {
            _jibSummary.Text = "no statement is selected";
            _jibTrace.Text = string.Empty;
            _journal.Text = string.Empty;
            _jibLineGrid.ItemsSource = null;
            return;
        }

        _jibSummary.Text = jib.Describe() + " · " + jib.TotalsText;
        _jibTrace.Text = "Trace: " + jib.TraceText +
                         (string.IsNullOrWhiteSpace(jib.ApprovedBy)
                             ? string.Empty
                             : " · approved by " + jib.ApprovedBy);
        _journal.Text = jib.JournalText;
        _jibLineGrid.ItemsSource = jib.Lines.ToList();
    }

    private void SelectRoyalty()
    {
        if (_royaltyGrid.SelectedItem is not OgRoyalty selected)
        {
            return;
        }

        _royalty = selected;
        ShowRoyalty();
        RefreshEnabled();
    }

    private void SelectJib()
    {
        if (_jibGrid.SelectedItem is not OgJib selected)
        {
            return;
        }

        _jib = selected;
        ShowJib();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var missing = RoyaltyRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The royalty needs " + string.Join(", ", missing) + ".";

        _calculateButton.IsEnabled = missing.Count == 0;
        _payButton.IsEnabled = CanPay;
        _disputeButton.IsEnabled = CanDispute;
        _resolveButton.IsEnabled = CanResolve;
        _raiseJibButton.IsEnabled = CanRaiseJib;
        _approveJibButton.IsEnabled = CanApproveJib;
        _returnJibButton.IsEnabled = CanApproveJib;
        _postJibButton.IsEnabled = CanPostJib;
        _commitButton.IsEnabled = CanCommit;
    }

    private OgRoyaltyInputs Inputs() =>
        new(
            Empty(_lease.Text),
            Empty(_owner.Text),
            Empty(_allocationDetail.Text),
            Empty(_allocationResult.Text),
            Parse(_volume.Text),
            _volumeUnit.SelectedItem as string,
            Parse(_price.Text),
            Parse(_rate.Text),
            Parse(_residual.Text),
            _currency.SelectedItem as string,
            new OgRoyaltyDeductions(
                Parse(_transport.Text) ?? 0m,
                Parse(_adValorem.Text) ?? 0m,
                Parse(_severance.Text) ?? 0m,
                Parse(_processing.Text) ?? 0m,
                Parse(_other.Text) ?? 0m));

    /// <summary>Reads a charge or credit editor: <c>description, category, amount</c> per line.</summary>
    private IReadOnlyList<OgJibLine> JibLines(string kind)
    {
        var text = kind == "CHARGE" ? _chargeLines.Text : _creditLines.Text;
        var lines = new List<OgJibLine>();

        foreach (var row in Rows(text))
        {
            var parts = row.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 3)
            {
                continue;
            }

            var amount = Parse(parts[2]);

            if (amount is null)
            {
                continue;
            }

            lines.Add(new OgJibLine(
                kind,
                string.IsNullOrWhiteSpace(parts[0]) ? kind.ToLowerInvariant() : parts[0],
                string.IsNullOrWhiteSpace(parts[1]) ? null : parts[1],
                amount.Value));
        }

        return lines;
    }

    /// <summary>Reads the target editor: <c>type, id, weight</c> per line.</summary>
    private IReadOnlyList<CostAllocationMath.AllocationTarget> Targets()
    {
        var targets = new List<CostAllocationMath.AllocationTarget>();

        foreach (var row in Rows(_targets.Text))
        {
            var parts = row.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 3)
            {
                continue;
            }

            var weight = Parse(parts[2]);

            if (weight is null || string.IsNullOrWhiteSpace(parts[1]))
            {
                continue;
            }

            targets.Add(new CostAllocationMath.AllocationTarget(
                parts[1],
                string.IsNullOrWhiteSpace(parts[0]) ? "PROPERTY" : parts[0].ToUpperInvariant(),
                weight.Value));
        }

        return targets;
    }

    private static IEnumerable<string> Rows(string? text) =>
        (text ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(row => !row.StartsWith('#'));

    private static TextBox LinesBox(string name)
    {
        var box = new TextBox
        {
            AcceptsReturn = true,
            IsTabStop = true,
            MinHeight = 56,
            MaxHeight = 110,
            Margin = new Thickness(0, 0, 0, 6),
            TextWrapping = TextWrapping.NoWrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = name,
        };

        Named(box, name);
        box.Text = "# " + name;

        return box;
    }

    private static string Money(decimal value) => value.ToString("#,0.00", CultureInfo.InvariantCulture);

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
