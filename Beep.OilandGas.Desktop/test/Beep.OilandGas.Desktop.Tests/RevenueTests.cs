using Beep.OilandGas.Accounting.Calculations;
using Beep.OilandGas.Accounting.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ACC-03: the owning tax calculator, the invoice states, the line and currency rules and the record's derived money - the
/// parts that need no database. The build/issue/credit/review journey, the single journal and the traceability are proven
/// in the gateway test.
/// </summary>
public sealed class RevenueTests
{
    private const string PageFileName = "RevenuePageView.cs";

    [Fact]
    public void The_tax_is_computed_by_the_owning_calculator()
    {
        // AC-5: every money figure is the accounting project's, rounded to the currency's minor unit away from zero.
        Assert.Equal(7000.00m, SalesInvoiceTax.Net(100m, 70m));
        Assert.Equal(700.00m, SalesInvoiceTax.Tax(7000m, 0.10m));

        // A half-cent rounds up, not to even.
        Assert.Equal(0.01m, SalesInvoiceTax.Round(0.005m));
        Assert.Equal(0.03m, SalesInvoiceTax.Round(0.025m));

        var totals = SalesInvoiceTax.Totals([7000m, 3000m], 0.10m);

        Assert.Equal(10_000m, totals.Subtotal);
        Assert.Equal(1_000m, totals.Tax);
        Assert.Equal(11_000m, totals.Total);

        var line = SalesInvoiceTax.Line(150m, 62.5m, 0.125m);

        Assert.Equal(9_375m, line.Net);
        Assert.Equal(1_171.88m, line.Tax);
        Assert.Equal(10_546.88m, line.Total);
        Assert.Equal(line.Net + line.Tax, line.Total);

        // A rate outside 0..1 is refused rather than silently applied.
        Assert.Throws<ArgumentOutOfRangeException>(() => SalesInvoiceTax.Tax(100m, 12.5m));
    }

    [Fact]
    public void The_invoice_states_are_the_owners()
    {
        Assert.Equal(["DRAFT", "ISSUED", "APPROVED", "RETURNED"], RevenueCodes.InvoiceStates);
        Assert.True(RevenueCodes.IsDraft("DRAFT"));
        Assert.False(RevenueCodes.IsDraft("ISSUED"));
        Assert.True(RevenueCodes.IsIssued("ISSUED"));
        Assert.False(RevenueCodes.IsIssued("DRAFT"));

        Assert.Equal("returned to the author", RevenueCodes.Describe("RETURNED"));
        Assert.Equal("issued", RevenueCodes.Describe("ISSUED"));

        // The three accounts an issued invoice touches, from the house defaults where one exists.
        Assert.Equal("1110", RevenueCodes.AccountsReceivableAccount);
        Assert.Equal("4001", RevenueCodes.RevenueAccount);
        Assert.Equal("2300", RevenueCodes.SalesTaxAccount);
        Assert.Equal(RevenueCodes.InvoiceStates, RevenueRules.States);
    }

    [Fact]
    public void A_volume_without_a_price_is_blocked_and_names_the_volume()
    {
        // The state table's first row.
        var unpriced = RevenueRules.CheckLines(
        [
            new OgSalesLineInput("OIL", "Woodford crude", 100m, "bbl", 70m, "VOL-1"),
            new OgSalesLineInput("GAS", "Woodford gas", 50m, "Mscf", null, "VOL-2"),
        ]);

        Assert.Equal(RevenueRules.CodeVolumeWithoutPrice, unpriced!.Value.Code);
        Assert.Contains("VOL-2", unpriced.Value.Message, StringComparison.Ordinal);
        Assert.Contains("has no price", unpriced.Value.Message, StringComparison.Ordinal);

        // A zero quantity is a different refusal.
        var empty = RevenueRules.CheckLines(
            [new OgSalesLineInput("OIL", null, 0m, "bbl", 70m, "VOL-3")]);

        Assert.Equal(RevenueRules.CodeInputInvalid, empty!.Value.Code);
        Assert.Contains("VOL-3", empty.Value.Message, StringComparison.Ordinal);

        Assert.Null(RevenueRules.CheckLines(
            [new OgSalesLineInput("OIL", null, 100m, "bbl", 70m, "VOL-1")]));
    }

    [Fact]
    public void An_invoice_must_be_in_its_contract_currency()
    {
        // The state table's "currency mismatch with contract - refused".
        var mismatch = RevenueRules.CheckCurrency(
            Inputs() with { ContractCurrency = "EUR", Currency = "USD" });

        Assert.Equal(RevenueRules.CodeCurrencyMismatch, mismatch!.Value.Code);
        Assert.Contains("EUR", mismatch.Value.Message, StringComparison.Ordinal);
        Assert.Contains("USD", mismatch.Value.Message, StringComparison.Ordinal);

        Assert.Null(RevenueRules.CheckCurrency(Inputs() with { ContractCurrency = "USD", Currency = "usd" }));
        Assert.Null(RevenueRules.CheckCurrency(Inputs() with { ContractCurrency = null }));
    }

    [Fact]
    public void An_invoice_needs_its_buyer_date_currency_and_a_line()
    {
        Assert.Equal(
            ["buyer", "invoice date", "currency", "at least one line"],
            RevenueRules.Missing(new OgInvoiceInputs(null, null, null, null, null, null, null, [])));

        // A due date before the invoice date is not a due date.
        Assert.Contains(
            "a due date not before the invoice date",
            RevenueRules.Missing(Inputs() with { DueDate = new DateTime(2026, 1, 1) }));

        Assert.Empty(RevenueRules.Missing(Inputs()));
    }

    [Fact]
    public void The_record_reports_its_money_balance_and_state()
    {
        var invoice = Invoice() with { Total = 11_000m, Subtotal = 10_000m, Tax = 1_000m, BalanceDue = 11_000m };

        Assert.Contains("subtotal 10,000.00", invoice.TotalsText, StringComparison.Ordinal);
        Assert.Contains("tax 1,000.00", invoice.TotalsText, StringComparison.Ordinal);
        Assert.Contains("total 11,000.00", invoice.TotalsText, StringComparison.Ordinal);
        Assert.Contains("balance 11,000.00 USD", invoice.BalanceText, StringComparison.Ordinal);

        // AC-1 at the record's surface: the balance is the total less what was paid and credited.
        var credited = invoice with { Credited = 1_500m, BalanceDue = 11_000m - 1_500m };

        Assert.Equal(9_500m, credited.BalanceDue);
        Assert.Contains("credited 1,500.00", credited.BalanceText, StringComparison.Ordinal);

        // The state table's "invoice with payments - deletion refused".
        Assert.True(invoice.CanDelete);
        Assert.False((invoice with { Paid = 10m }).CanDelete);
        Assert.False((invoice with { State = RevenueCodes.InvoiceIssued }).CanDelete);

        Assert.False(invoice.HasJournal);
        Assert.Contains("no journal yet", invoice.JournalText, StringComparison.Ordinal);
        Assert.True((invoice with { JournalId = "J1", State = RevenueCodes.InvoiceIssued }).HasJournal);
    }

    [Fact]
    public void The_page_names_no_state_literal_of_its_own()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => Path.GetFileName(source) == PageFileName));

        Assert.Contains("workspace.Currencies", page, StringComparison.Ordinal);
        Assert.Contains("invoice.StateText", page, StringComparison.Ordinal);
        Assert.Contains("invoice.Lines", page, StringComparison.Ordinal);
        Assert.Contains("invoice.CreditNotes", page, StringComparison.Ordinal);

        Assert.DoesNotContain("\"ISSUED\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"DRAFT\"", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ helpers

    private static OgInvoiceInputs Inputs() =>
        new(
            "BUYER-1",
            "CONTRACT-1",
            "USD",
            new DateTime(2026, 3, 10),
            new DateTime(2026, 4, 10),
            "USD",
            0.10m,
            [
                new OgSalesLineInput("OIL", "Woodford crude", 100m, "bbl", 70m, "VOL-1"),
                new OgSalesLineInput("GAS", "Woodford gas", 50m, "Mscf", 60m, "VOL-2"),
            ]);

    private static OgSalesInvoice Invoice() =>
        new(
            "INV-ID",
            string.Empty,
            "BUYER-1",
            "CONTRACT-1",
            new DateTime(2026, 3, 10),
            new DateTime(2026, 4, 10),
            "USD",
            10_000m,
            1_000m,
            11_000m,
            0m,
            0m,
            11_000m,
            RevenueCodes.InvoiceDraft,
            "sara",
            null,
            null,
            null,
            null,
            null,
            null,
            [],
            []);
}
