using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Beep.OilandGas.Accounting.Constants;

namespace Beep.OilandGas.Accounting.Calculations;

/// <summary>
/// One exception the three-way match found (ACC-05), with the three quantities and the tolerance that decided it.
/// </summary>
/// <param name="Kind">The exception kind, from <see cref="ProcurementCodes"/>.</param>
/// <param name="Ordered">What the order says, or null when the line has no order.</param>
/// <param name="Received">What the receipts say, or null when nothing has been received.</param>
/// <param name="Invoiced">What the invoice says.</param>
/// <param name="Tolerance">The recorded limit that was exceeded, or null when the exception is not a tolerance one.</param>
public sealed record MatchException(
    string Kind,
    decimal? Ordered,
    decimal? Received,
    decimal? Invoiced,
    decimal? Tolerance)
{
    public string Describe() => ProcurementCodes.DescribeException(Kind) +
                                " · ordered " + Show(Ordered) +
                                " · received " + Show(Received) +
                                " · invoiced " + Show(Invoiced) +
                                (Tolerance is { } limit ? " · tolerance " + Show(limit) + " %" : string.Empty);

    private static string Show(decimal? value) =>
        value?.ToString("#,0.####", System.Globalization.CultureInfo.InvariantCulture) ?? "n/a";
}

/// <summary>The answer to a three-way match (ACC-05).</summary>
/// <param name="State">MATCHED, EXCEPTION, EXCEPTION_APPROVED or NON_PO, from <see cref="ProcurementCodes"/>.</param>
/// <param name="Exceptions">Every exception found, in a stable order.</param>
public sealed record MatchResult(string State, IReadOnlyList<MatchException> Exceptions)
{
    public bool Matched => string.Equals(State, ProcurementCodes.MatchMatched, StringComparison.OrdinalIgnoreCase);

    public bool NeedsApproval => string.Equals(State, ProcurementCodes.MatchException, StringComparison.OrdinalIgnoreCase);

    public bool IsPayable => ProcurementCodes.IsPayable(State);

    public string ExceptionsText => Exceptions.Count == 0
        ? "no exception"
        : string.Join(Environment.NewLine, Exceptions.Select(exception => exception.Describe()));
}

/// <summary>
/// The three-way match - order against receipt against invoice (ACC-05 AC-1, AC-2).
/// </summary>
/// <remarks>
/// <para>
/// The tolerances are <b>percentages of the recorded basis</b>, which is the shape the owner already states in
/// <c>CompanyMatchingPolicyRules</c> (<c>QuantityTolerancePercent</c> and <c>PriceTolerancePercent</c>, each 0 to 100).
/// The comparison itself is the owner's too: <c>APInvoiceFinancialReview.MatchingPolicy</c> compares decimals by their exact
/// <see cref="decimal.GetBits(int)"/> representation through <see cref="BigInteger"/>, so a percentage is evaluated without
/// the rounding a <c>decimal</c> division would introduce. The same arithmetic is used here, and the reason is the same -
/// a tolerance test that rounds is a tolerance test you cannot reproduce from the two numbers it decided on.
/// </para>
/// <para>
/// Where this differs from the owner's policy checks, it is deliberate and stricter: the owner compares the invoice against
/// the <b>full</b> ordered quantity and warns in its own text that it does not consider "accepted receipt capacity or
/// consumption by other invoices". This match adds those two: an invoice above what was actually <b>received</b> is an
/// exception even when it is inside the ordered quantity, and the quantity exceptions are stated separately so a reviewer
/// can see which ceiling was passed.
/// </para>
/// </remarks>
public static class ThreeWayMatch
{
    /// <summary>
    /// Compares one line. <paramref name="quantityTolerancePercent"/> and <paramref name="priceTolerancePercent"/> are
    /// percentages (5 means 5 %); null means no policy was found and the line is an exception for that reason.
    /// </summary>
    public static MatchResult Compare(
        decimal? orderedQuantity,
        decimal? orderedUnitPrice,
        decimal? receivedQuantity,
        decimal? invoicedQuantity,
        decimal? invoicedUnitPrice,
        decimal? quantityTolerancePercent,
        decimal? priceTolerancePercent)
    {
        // A non-PO invoice has nothing to match: the owner's separate-review rule, stated rather than passed silently.
        if (orderedQuantity is null && orderedUnitPrice is null)
        {
            return new MatchResult(ProcurementCodes.MatchNotApplicable, []);
        }

        var exceptions = new List<MatchException>();

        if (quantityTolerancePercent is null || priceTolerancePercent is null)
        {
            exceptions.Add(new MatchException(
                ProcurementCodes.ExceptionNoPolicy, orderedQuantity, receivedQuantity, invoicedQuantity, null));
        }

        var invoiced = invoicedQuantity ?? 0m;
        var ordered = orderedQuantity ?? 0m;
        var received = receivedQuantity ?? 0m;

        // AC-1's rule, at the invoice: an invoice above what was ordered is an exception regardless of tolerance.
        if (invoiced > ordered)
        {
            exceptions.Add(new MatchException(
                ProcurementCodes.ExceptionQuantityAboveOrder, ordered, received, invoiced, null));
        }

        // The stricter ceiling: an invoice above what actually arrived needs a reviewer even inside the order.
        if (invoiced > received)
        {
            exceptions.Add(new MatchException(
                ProcurementCodes.ExceptionQuantityAboveReceived, ordered, received, invoiced, null));
        }

        // AC-2: a price outside the recorded tolerance is an exception, never a silent pass.
        if (quantityTolerancePercent is not null
            && priceTolerancePercent is not null
            && orderedUnitPrice is { } orderedPrice
            && orderedPrice > 0m
            && invoicedUnitPrice is { } invoicedPrice
            && !WithinPercent(invoicedPrice, orderedPrice, priceTolerancePercent.Value))
        {
            exceptions.Add(new MatchException(
                ProcurementCodes.ExceptionPriceOutsideTolerance, orderedPrice, orderedPrice, invoicedPrice,
                priceTolerancePercent));
        }

        return new MatchResult(
            exceptions.Count == 0 ? ProcurementCodes.MatchMatched : ProcurementCodes.MatchException,
            exceptions);
    }

    /// <summary>Compares a whole invoice: every line in order, the currency once.</summary>
    public static MatchResult CompareInvoice(
        bool currencyAgrees,
        IReadOnlyList<MatchResult> lines,
        decimal? quantityTolerancePercent,
        decimal? priceTolerancePercent)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var exceptions = new List<MatchException>();

        if (!currencyAgrees)
        {
            exceptions.Add(new MatchException(
                ProcurementCodes.ExceptionCurrencyMismatch, null, null, null, null));
        }

        if (quantityTolerancePercent is null || priceTolerancePercent is null)
        {
            // Said once for the invoice rather than once per line.
            if (!exceptions.Any(exception => exception.Kind == ProcurementCodes.ExceptionNoPolicy))
            {
                exceptions.Add(new MatchException(
                    ProcurementCodes.ExceptionNoPolicy, null, null, null, null));
            }
        }

        foreach (var line in lines)
        {
            exceptions.AddRange(line.Exceptions.Where(exception =>
                exception.Kind != ProcurementCodes.ExceptionNoPolicy
                && exception.Kind != ProcurementCodes.ExceptionCurrencyMismatch));
        }

        var nonPo = lines.Count > 0 && lines.All(line =>
            string.Equals(line.State, ProcurementCodes.MatchNotApplicable, StringComparison.OrdinalIgnoreCase));

        if (exceptions.Count == 0 && nonPo)
        {
            return new MatchResult(ProcurementCodes.MatchNotApplicable, []);
        }

        return new MatchResult(
            exceptions.Count == 0 ? ProcurementCodes.MatchMatched : ProcurementCodes.MatchException,
            exceptions);
    }

    /// <summary>
    /// Whether <paramref name="observed"/> is inside <paramref name="percent"/> of <paramref name="basis"/>.
    /// </summary>
    /// <remarks>
    /// The arithmetic is the owner's, copied in shape from <c>APInvoiceFinancialReview.MatchingPolicy</c>: each decimal is
    /// taken apart into its scaled integer and scale, and the test is
    /// <c>|observed - basis| x 100 x 10^scale(percent) &lt;= basis x unscaled(percent)</c>. Nothing is divided, so no
    /// rounding decides the answer and the same two numbers always give the same verdict.
    /// </remarks>
    public static bool WithinPercent(decimal observed, decimal basis, decimal percent)
    {
        if (basis <= 0m || percent < 0m)
        {
            return false;
        }

        static (BigInteger Value, int Scale) Parts(decimal value)
        {
            var bits = decimal.GetBits(value);

            return (((BigInteger)(uint)bits[2] << 64) + ((BigInteger)(uint)bits[1] << 32) + (uint)bits[0],
                (bits[3] >> 16) & 255);
        }

        var valuePart = Parts(observed);
        var referencePart = Parts(basis);
        var limitPart = Parts(percent);
        var scale = Math.Max(valuePart.Scale, referencePart.Scale);

        var scaledValue = valuePart.Value * BigInteger.Pow(10, scale - valuePart.Scale);
        var scaledBasis = referencePart.Value * BigInteger.Pow(10, scale - referencePart.Scale);

        return BigInteger.Abs(scaledValue - scaledBasis) * 100 * BigInteger.Pow(10, limitPart.Scale)
               <= scaledBasis * limitPart.Value;
    }
}
