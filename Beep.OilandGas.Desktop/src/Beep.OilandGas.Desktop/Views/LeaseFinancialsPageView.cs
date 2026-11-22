using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// LSE-03: the financial and lifecycle side of a lease.
/// </summary>
/// <remarks>
/// The grid carries the current terms, the derived expiry state and the overdue count; the detail carries the
/// append-only term history, the rental/obligation schedule, the payment history and the lifecycle events. The
/// NRI arithmetic is refused by the service when it cannot be represented (AC-1), the royalty rate is a fraction
/// at eight places (AC-2), a renewal appends a term and retains the old one (AC-4), and overdue obligations are
/// computed from their due dates (AC-5). The expiring set is exposed for the task inbox (AC-3); the inbox itself
/// arrives with WFL-05, and the page says so.
/// </remarks>
public sealed class LeaseFinancialsPageView : OgPageBase
{
    public const string Route = "leasefinancials";

    private readonly OgApplication _application;
    private readonly IOgLeaseFinancials _financials;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _summary;
    private readonly TextBlock _detail;
    private readonly DataGrid _grid;

    private readonly TextBox _bonus;
    private readonly TextBox _bonusCurrency;
    private readonly TextBox _royalty;
    private readonly TextBox _nri;
    private readonly TextBox _overriding;
    private readonly TextBox _burdens;
    private readonly TextBox _effective;
    private readonly TextBox _expiry;

    private readonly ComboBox _obligationType;
    private readonly TextBox _obligationDescription;
    private readonly TextBox _obligationDue;
    private readonly TextBox _obligationAmount;
    private readonly TextBox _obligationCurrency;

    private readonly TextBox _paymentAmount;
    private readonly TextBox _paymentCurrency;
    private readonly TextBox _paymentKind;
    private readonly TextBox _paymentReference;

    private readonly TextBox _renewalExpiry;
    private readonly TextBox _rationale;
    private readonly TextBox _effectiveDate;

    public LeaseFinancialsPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _financials = application.LeaseFinancials;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 8, 0, 8);

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 190,
            MaxHeight = 280,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Name", "Name", 170),
                Text("Region", "Region", 80),
                Text("Expiry", "ExpiryText", 100),
                Text("State", "ExpiryState", 90),
                Text("Royalty", "RoyaltyText", 80),
                Text("NRI", "NetRevenueText", 80),
                Text("Overdue", "OverdueText", 150),
                Text("Lifecycle", "Lifecycle", 100),
            },
        };

        Named(_grid, "Lease financials");
        _grid.SelectionChanged += (_, _) => _ = ShowSelectedAsync();

        _bonus = Field("Bonus", 90);
        _bonusCurrency = Field("Bonus currency", 60);
        _royalty = Field("Royalty rate", 90);
        _nri = Field("Net revenue interest", 90);
        _overriding = Field("Overriding royalty", 90);
        _burdens = Field("Burdens (name=fraction; ...)", 220);
        _effective = Field("Effective yyyy-MM-dd", 100);
        _expiry = Field("Expiry yyyy-MM-dd", 100);

        _obligationType = new ComboBox
        {
            MinWidth = 120,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = OgLeaseObligation.Types,
            SelectedIndex = 0,
        };

        Named(_obligationType, "Obligation type");

        _obligationDescription = Field("Obligation description", 180);
        _obligationDue = Field("Due yyyy-MM-dd", 100);
        _obligationAmount = Field("Minimum spend", 90);
        _obligationCurrency = Field("Obligation currency", 60);

        _paymentAmount = Field("Payment amount", 90);
        _paymentCurrency = Field("Payment currency", 60);
        _paymentKind = Field("Payment kind", 80);
        _paymentReference = Field("Payment reference", 120);

        _renewalExpiry = Field("Renew to yyyy-MM-dd", 110);
        _rationale = Field("Rationale", 220);
        _effectiveDate = Field("Effective yyyy-MM-dd", 100);

        var actions = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actions.Children.Add(Button("Refresh", (_, _) => Load()));
        actions.Children.Add(Button("Retry renewal", (_, _) => _ = RetryAsync()));

        var terms = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        terms.Children.Add(Caption("Terms"));
        terms.Children.Add(_bonus);
        terms.Children.Add(_bonusCurrency);
        terms.Children.Add(Caption("Royalty"));
        terms.Children.Add(_royalty);
        terms.Children.Add(Caption("NRI"));
        terms.Children.Add(_nri);
        terms.Children.Add(Caption("ORR"));
        terms.Children.Add(_overriding);
        terms.Children.Add(_burdens);
        terms.Children.Add(_effective);
        terms.Children.Add(_expiry);
        terms.Children.Add(Button("Save terms", (_, _) => _ = SaveTermsAsync()));

        var obligations = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        obligations.Children.Add(Caption("Obligation"));
        obligations.Children.Add(_obligationType);
        obligations.Children.Add(_obligationDescription);
        obligations.Children.Add(_obligationDue);
        obligations.Children.Add(_obligationAmount);
        obligations.Children.Add(_obligationCurrency);
        obligations.Children.Add(Button("Record", (_, _) => _ = RecordObligationAsync()));

        var payments = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        payments.Children.Add(Caption("Payment"));
        payments.Children.Add(_paymentAmount);
        payments.Children.Add(_paymentCurrency);
        payments.Children.Add(_paymentKind);
        payments.Children.Add(_paymentReference);
        payments.Children.Add(Button("Record payment", (_, _) => _ = RecordPaymentAsync()));

        var lifecycle = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        lifecycle.Children.Add(Caption("Lifecycle"));
        lifecycle.Children.Add(_renewalExpiry);
        lifecycle.Children.Add(_rationale);
        lifecycle.Children.Add(_effectiveDate);
        lifecycle.Children.Add(Button("Renew", (_, _) => _ = RenewAsync()));
        lifecycle.Children.Add(Button("Relinquish", (_, _) => _ = RelinquishAsync()));
        lifecycle.Children.Add(Button("Expire", (_, _) => _ = ExpireAsync()));

        var panel = new StackPanel();
        panel.Children.Add(actions);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_summary);
        panel.Children.Add(_grid);
        panel.Children.Add(_detail);
        panel.Children.Add(terms);
        panel.Children.Add(obligations);
        panel.Children.Add(payments);
        panel.Children.Add(lifecycle);

        ShowContent(panel);

        Load();
    }

    public OgLeaseFinancialSnapshot? Snapshot { get; private set; }

    public IReadOnlyList<OgLeaseFinancialSummary> Leases => Snapshot?.Leases ?? [];

    public OgLeaseFinancialSummary? Selected { get; private set; }

    public OgLeaseFinancialDetail? Detail { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string SummaryText => _summary.Text;

    public string DetailText => _detail.Text;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _financials
            .LoadAsync(session.FieldId, session.FieldName, DateTime.Today)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Snapshot = null;
            Selected = null;
            Detail = null;
            _grid.ItemsSource = null;
            _header.Text = "Lease financials";
            _summary.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "Lease financials could not be read.";
            ShowState(OgViewState.Failed, "Lease financials could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Snapshot = loaded.Value;
        Selected = null;
        Detail = null;

        _grid.ItemsSource = Leases.ToList();

        _header.Text =
            $"{Snapshot.Count.ToString(CultureInfo.InvariantCulture)} lease(s) · " +
            $"{(string.IsNullOrWhiteSpace(session.FieldName) ? "(no field selected)" : session.FieldName)} · " +
            $"as of {Snapshot.AsOf.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

        _summary.Text = Snapshot.IsEmpty
            ? "No lease is recorded for this field."
            : Snapshot.Summary;

        _notice.Text = Snapshot.IsEmpty
            ? "No lease is recorded for this field."
            : Snapshot.OverdueObligations == 0
                ? "No obligation is overdue. The task inbox (WFL-05) is not built; the expiring set is ready for it."
                : $"{Snapshot.OverdueObligations.ToString(CultureInfo.InvariantCulture)} obligation(s) are " +
                  "overdue; the grid names the worst delay.";

        _detail.Text = "Select a lease for its terms, obligations, payments and lifecycle.";
        ShowState(Snapshot.IsEmpty ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    /// <summary>AC-4: a renewal appends a new term and retains the old one.</summary>
    public async Task<bool> RenewAsync()
    {
        if (Selected is not { } lease)
        {
            _notice.Text = "Select a lease first.";
            return false;
        }

        if (!TryDate(_renewalExpiry.Text, out var expiry))
        {
            _notice.Text = "The renewal needs a new expiry date (yyyy-MM-dd).";
            return false;
        }

        var renewed = await _financials
            .RenewAsync(lease.Id, expiry, _rationale.Text, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(renewed, "lease.lifecycle.renew", lease.Id,
            $"expiry={expiry:yyyy-MM-dd} rationale={_rationale.Text.Trim()}");
    }

    public async Task<bool> RelinquishAsync()
    {
        if (Selected is not { } lease)
        {
            _notice.Text = "Select a lease first.";
            return false;
        }

        var effective = TryDate(_effectiveDate.Text, out var parsed) ? parsed : DateTime.Today;

        var changed = await _financials
            .RelinquishAsync(lease.Id, effective, _rationale.Text, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(changed, "lease.lifecycle.relinquish", lease.Id,
            $"effective={effective:yyyy-MM-dd} rationale={_rationale.Text.Trim()}");
    }

    public async Task<bool> ExpireAsync()
    {
        if (Selected is not { } lease)
        {
            _notice.Text = "Select a lease first.";
            return false;
        }

        var effective = TryDate(_effectiveDate.Text, out var parsed) ? parsed : DateTime.Today;

        var changed = await _financials
            .ExpireAsync(lease.Id, effective, _rationale.Text, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(changed, "lease.lifecycle.expire", lease.Id,
            $"effective={effective:yyyy-MM-dd} rationale={_rationale.Text.Trim()}");
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _financials.RetryAsync(DateTime.Today).ConfigureAwait(true);
        return AfterWrite(retried, "lease.lifecycle.renew", Selected?.Id ?? string.Empty, "retry");
    }

    /// <summary>AC-1/AC-2: the terms are checked by the service before anything is written.</summary>
    public async Task<bool> SaveTermsAsync()
    {
        if (Selected is not { } lease)
        {
            _notice.Text = "Select a lease first.";
            return false;
        }

        if (!TryDecimal(_royalty.Text, out var royalty) || !TryDecimal(_nri.Text, out var nri)
            || !TryDecimal(_overriding.Text, out var overriding))
        {
            _notice.Text = "Royalty, NRI and ORR must be decimals (a fraction, e.g. 0.125).";
            return false;
        }

        var burdens = ParseBurdens(_burdens.Text);

        if (burdens is null)
        {
            _notice.Text = "Burdens must be written as name=fraction, separated by ';'.";
            return false;
        }

        var bonus = TryDecimal(_bonus.Text, out var parsedBonus) ? parsedBonus : (decimal?)null;
        var effective = TryDate(_effective.Text, out var effectiveDate) ? effectiveDate : (DateTime?)null;
        var expiry = TryDate(_expiry.Text, out var expiryDate) ? expiryDate : (DateTime?)null;

        var saved = await _financials
            .SaveTermsAsync(
                lease.Id,
                new OgLeaseTerms(
                    bonus,
                    Empty(_bonusCurrency.Text),
                    royalty,
                    nri,
                    overriding,
                    burdens,
                    effective,
                    expiry,
                    _rationale.Text),
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(saved, "lease.financial.terms", lease.Id,
            $"royalty={royalty} nri={nri} orr={overriding}");
    }

    public async Task<bool> RecordObligationAsync()
    {
        if (Selected is not { } lease)
        {
            _notice.Text = "Select a lease first.";
            return false;
        }

        if (!TryDate(_obligationDue.Text, out var due))
        {
            _notice.Text = "The obligation needs a due date (yyyy-MM-dd).";
            return false;
        }

        var amount = TryDecimal(_obligationAmount.Text, out var parsed) ? parsed : 0m;

        var recorded = await _financials
            .RecordObligationAsync(
                lease.Id,
                _obligationType.SelectedItem as string ?? OgLeaseObligation.Other,
                _obligationDescription.Text,
                due,
                amount,
                _obligationCurrency.Text,
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(recorded, "lease.financial.obligation", lease.Id,
            $"type={_obligationType.SelectedItem} due={due:yyyy-MM-dd} amount={amount}");
    }

    public async Task<bool> RecordPaymentAsync()
    {
        if (Selected is not { } lease)
        {
            _notice.Text = "Select a lease first.";
            return false;
        }

        if (!TryDecimal(_paymentAmount.Text, out var amount))
        {
            _notice.Text = "The payment amount must be a decimal.";
            return false;
        }

        var paid = await _financials
            .RecordPaymentAsync(
                lease.Id,
                amount,
                _paymentCurrency.Text,
                _paymentKind.Text,
                _paymentReference.Text,
                null,
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(paid, "lease.financial.payment", lease.Id,
            $"amount={amount} {_paymentCurrency.Text.Trim()}");
    }

    private bool AfterWrite(
        OgResult<OgLeaseFinancialDetail> result,
        string verb,
        string leaseId,
        string detail)
    {
        _notice.Text = result.Success
            ? result.Message ?? "Saved."
            : result.Message ?? "The change was not applied.";

        if (result.Success)
        {
            _application.AuditLog.Add(verb, "lease", leaseId, "ok", detail);
            Load();
            ShowDetail(result.Value);
        }

        return result.Success;
    }

    private async Task ShowSelectedAsync()
    {
        var index = _grid.SelectedIndex;

        if (index < 0 || index >= Leases.Count)
        {
            return;
        }

        Selected = Leases[index];

        var loaded = await _financials
            .LoadDetailAsync(Selected.Id, DateTime.Today)
            .ConfigureAwait(true);

        if (!loaded.Success || loaded.Value is null)
        {
            _detail.Text = loaded.Message ?? "The lease financials could not be read.";
            return;
        }

        FillTerms(loaded.Value.Terms);
        ShowDetail(loaded.Value);
    }

    private void FillTerms(OgLeaseTerms terms)
    {
        _bonus.Text = terms.Bonus?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _bonusCurrency.Text = terms.BonusCurrency ?? string.Empty;
        _royalty.Text = terms.RoyaltyRate.ToString("0.########", CultureInfo.InvariantCulture);
        _nri.Text = terms.NetRevenueInterest.ToString("0.########", CultureInfo.InvariantCulture);
        _overriding.Text = terms.OverridingRoyalty.ToString("0.########", CultureInfo.InvariantCulture);
        _burdens.Text = string.Join("; ", terms.Burdens.Select(burden =>
            $"{burden.Name}={burden.Fraction.ToString("0.########", CultureInfo.InvariantCulture)}"));
        _effective.Text = terms.Effective?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        _expiry.Text = terms.Expiry?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        _renewalExpiry.Text = terms.Expiry?.AddYears(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        _effectiveDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private void ShowDetail(OgLeaseFinancialDetail? detail)
    {
        Detail = detail;

        if (detail is null)
        {
            return;
        }

        var lines = new List<string>
        {
            detail.Summary.Describe(),
            $"Terms: {detail.Terms.Describe()}",
            $"Lifecycle: {detail.LifecycleState}",
            $"Obligations ({detail.Obligations.Count.ToString(CultureInfo.InvariantCulture)}, " +
            $"{detail.OverdueCount.ToString(CultureInfo.InvariantCulture)} overdue):",
        };

        lines.AddRange(detail.Obligations.Select(obligation => "  " + obligation.Describe()));

        lines.Add($"Payments ({detail.Payments.Count.ToString(CultureInfo.InvariantCulture)}):");
        lines.AddRange(detail.Payments.Select(payment => "  " + payment.Describe()));

        lines.Add($"Term history ({detail.TermsHistory.Count.ToString(CultureInfo.InvariantCulture)}):");
        lines.AddRange(detail.TermsHistory.Select(term =>
            $"  {term.Effective?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "?"} to " +
            $"{term.Expiry?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "?"} · {term.Describe()}"));

        lines.Add($"Lifecycle events ({detail.Lifecycle.Count.ToString(CultureInfo.InvariantCulture)}):");
        lines.AddRange(detail.Lifecycle.Select(entry => "  " + entry.Describe()));

        _detail.Text = string.Join(Environment.NewLine, lines);
    }

    private static IReadOnlyList<OgLeaseBurden>? ParseBurdens(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var burdens = new List<OgLeaseBurden>();

        foreach (var part in text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = part.IndexOf('=');

            if (separator <= 0
                || !decimal.TryParse(part[(separator + 1)..].Trim(), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var fraction))
            {
                return null;
            }

            burdens.Add(new OgLeaseBurden(part[..separator].Trim(), fraction));
        }

        return burdens;
    }

    private static bool TryDate(string? text, out DateTime value) =>
        DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out value);

    private static bool TryDecimal(string? text, out decimal value) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out value);

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
