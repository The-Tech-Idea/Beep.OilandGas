using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Accounting.Calculations;

/// <summary>
/// Sales-invoice tax and totals, in <c>decimal</c>, computed by the owning project rather than the screen (ACC-03 AC-5).
/// </summary>
/// <remarks>
/// The accounting project had no invoice tax computation at all - the tax services it does carry are income-tax provision
/// and deferral records, not sales tax on a line. This states the arithmetic once: net is quantity times price, tax is that
/// net times the rate rounded to the currency's minor unit (away from zero, the usual commercial rounding), and the line
/// total is net plus tax. The screen never computes a tax amount; it asks for this.
/// </remarks>
public static class SalesInvoiceTax
{
    /// <summary>The minor-unit precision the money is rounded to: two places.</summary>
    public const int CurrencyDecimals = 2;

    /// <summary>
    /// Rounds a money amount to the currency's minor unit, away from zero.
    /// </summary>
    /// <remarks>
    /// Away from zero rather than to-even: a half-cent on an invoice is conventionally rounded up on the document, and the
    /// choice is stated here so the line, the total and the ledger all agree.
    /// </remarks>
    public static decimal Round(decimal amount) =>
        Math.Round(amount, CurrencyDecimals, MidpointRounding.AwayFromZero);

    /// <summary>The net amount of a line: quantity times unit price, rounded once.</summary>
    public static decimal Net(decimal quantity, decimal unitPrice) => Round(quantity * unitPrice);

    /// <summary>The tax on a net amount at a rate expressed as a fraction (12.5 % is 0.125), rounded once.</summary>
    public static decimal Tax(decimal net, decimal rate)
    {
        if (rate < 0m || rate > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rate), rate, "A sales-tax rate is a fraction between 0 and 1.");
        }

        return Round(net * rate);
    }

    /// <summary>The three totals of a set of line nets at one rate.</summary>
    public static SalesInvoiceTotals Totals(IEnumerable<decimal> lineNets, decimal rate)
    {
        ArgumentNullException.ThrowIfNull(lineNets);

        var nets = lineNets.ToList();
        var subtotal = Round(nets.Sum());
        var tax = Tax(subtotal, rate);

        return new SalesInvoiceTotals(subtotal, tax, subtotal + tax);
    }

    /// <summary>One line's money: its net, its tax and its total.</summary>
    public static SalesInvoiceLineMoney Line(decimal quantity, decimal unitPrice, decimal rate)
    {
        var net = Net(quantity, unitPrice);
        var tax = Tax(net, rate);

        return new SalesInvoiceLineMoney(net, tax, net + tax);
    }
}

/// <summary>The subtotal, the tax and the total of an invoice (ACC-03).</summary>
/// <param name="Subtotal">The lines' net total.</param>
/// <param name="Tax">The tax on that subtotal at the invoice's rate.</param>
/// <param name="Total">Subtotal plus tax - what the buyer owes before any credit.</param>
public readonly record struct SalesInvoiceTotals(decimal Subtotal, decimal Tax, decimal Total);

/// <summary>One line's net, tax and total (ACC-03).</summary>
/// <param name="Net">Quantity times unit price.</param>
/// <param name="Tax">The tax on the net.</param>
/// <param name="Total">Net plus tax.</param>
public readonly record struct SalesInvoiceLineMoney(decimal Net, decimal Tax, decimal Total);
