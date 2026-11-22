using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Beep.OilandGas.ProductionAccounting.Calculations;

/// <summary>
/// The royalty arithmetic (ACC-07 AC-1), in <c>decimal</c>.
/// </summary>
/// <remarks>
/// <para>
/// The relation is the owner's own, stated in <c>RoyaltyService</c>'s header and implemented in its calculation:
/// <b>net revenue = gross revenue - deductions</b> and <b>royalty = net revenue x rate</b>, where the rate is stored in
/// <b>percentage points</b> (12.5 is 12.5 %) - which is why the owner divides by 100 before multiplying.
/// </para>
/// <para>
/// The feature's AC-1 reads "royalty nets exactly to value x rate - deductions". Taken literally that would take the whole
/// deduction off the royalty amount rather than off the revenue it is a royalty <i>on</i>, charging the mineral owner the
/// entire cost instead of their rate share - so the arithmetic here is the documented one, and it is exactly reproducible
/// from the three inputs: <c>rate/100 x (value - deductions)</c>. That is algebraically
/// <c>value x rate/100 - deductions x rate/100</c>, which is what the acceptance test asserts, in both forms.
/// </para>
/// <para>
/// Everything is rounded once, to the currency's minor unit, away from zero - the same commercial rounding
/// <see cref="Beep.OilandGas.Accounting.Calculations.SalesInvoiceTax"/> uses - so a stored amount can be re-derived from its
/// own row to the cent.
/// </para>
/// </remarks>
public static class RoyaltyMath
{
    /// <summary>The minor-unit precision money is rounded to.</summary>
    public const int CurrencyDecimals = 2;

    /// <summary>Rounds money to the currency's minor unit, away from zero.</summary>
    public static decimal Round(decimal amount) =>
        Math.Round(amount, CurrencyDecimals, MidpointRounding.AwayFromZero);

    /// <summary>
    /// The royalty on a gross value, given its deductions and a rate in percentage points.
    /// </summary>
    /// <param name="grossValue">The value of the owner's share of production, before deductions.</param>
    /// <param name="deductions">Transportation, ad valorem, severance and processing, added together.</param>
    /// <param name="ratePercent">The royalty rate in percentage points: 12.5 is 12.5 %.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The value is negative, the rate is outside 0-100, or the deductions exceed the value - a royalty cannot be computed on
    /// a negative net revenue, which is the owner's own rule rather than an omission here.
    /// </exception>
    public static RoyaltyAnswer Calculate(decimal grossValue, decimal deductions, decimal ratePercent)
    {
        if (grossValue < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(grossValue), grossValue, "A gross value cannot be negative.");
        }

        if (ratePercent < 0m || ratePercent > 100m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ratePercent), ratePercent, "A royalty rate is in percentage points between 0 and 100.");
        }

        if (deductions < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(deductions), deductions, "Deductions cannot be negative; they reduce the value.");
        }

        if (deductions > grossValue)
        {
            throw new ArgumentException(
                "The deductions exceed the gross value, so there is no net revenue to take a royalty on. " +
                "Review the recorded deductions.",
                nameof(deductions));
        }

        var netRevenue = Round(grossValue - deductions);
        var rate = ratePercent / 100m;
        var amount = Round(netRevenue * rate);

        return new RoyaltyAnswer(netRevenue, rate, amount);
    }

    /// <summary>The gross value of a volume at a price.</summary>
    public static decimal GrossValue(decimal volume, decimal pricePerUnit) =>
        Round(volume * pricePerUnit);
}

/// <summary>The three numbers a royalty calculation produces (ACC-07).</summary>
/// <param name="NetRevenue">Gross value less deductions.</param>
/// <param name="Rate">The rate as a fraction, so 12.5 % is 0.125.</param>
/// <param name="Amount">Net revenue times the rate - the royalty payable.</param>
public readonly record struct RoyaltyAnswer(decimal NetRevenue, decimal Rate, decimal Amount);

/// <summary>
/// The ownership-fraction gate (ACC-07 AC-2).
/// </summary>
/// <remarks>
/// "Fractions do not sum to 1" is the feature's own rule, with one stated exception: a lease may declare a residual - the
/// share that is not owned by the parties being paid, typically a government or unleased interest - and then the fractions
/// plus that residual must sum to 1. The owner stores these figures in <b>percentage points</b> (<c>JointInterestBillingService</c>
/// writes <c>NET_REVENUE_INTEREST = interestPercent</c>, and its recorded-inputs test seeds a rate of 12.5), so the check is
/// written in percentage points: the fractions plus the declared residual must equal <b>100</b> exactly. Both are accepted
/// in either form only when they are unambiguous, which they are not - so the unit is stated, and a caller that passes a
/// fraction where a percentage belongs is refused by the sum rather than silently accepted.
/// </remarks>
public static class OwnershipFractions
{
    /// <summary>The total every lease's ownership must reach, in percentage points.</summary>
    public const decimal FullOwnership = 100m;

    /// <summary>
    /// Validates a lease's ownership: at least one fraction, none negative, and the fractions plus the declared residual
    /// equal to 100 exactly.
    /// </summary>
    /// <param name="leaseId">The lease the fractions belong to, named in every refusal.</param>
    /// <param name="fractions">Each owner and their net revenue interest in percentage points.</param>
    /// <param name="declaredResidualPercent">The stated unowned share in percentage points; zero when there is none.</param>
    public static (string Code, string Message)? Validate(
        string leaseId,
        IReadOnlyList<(string OwnerId, decimal FractionPercent)> fractions,
        decimal declaredResidualPercent)
    {
        ArgumentNullException.ThrowIfNull(fractions);

        var lease = string.IsNullOrWhiteSpace(leaseId) ? "(unnamed lease)" : leaseId.Trim();

        if (fractions.Count == 0)
        {
            return (Codes.MissingFraction,
                $"Lease {lease} has no recorded ownership fraction, so a royalty cannot be attributed to an owner. " +
                "Record the ownership interests before calculating.");
        }

        if (fractions.Any(fraction => fraction.FractionPercent < 0m))
        {
            return (Codes.NegativeFraction,
                $"Lease {lease} has a negative ownership fraction; an interest cannot be less than zero.");
        }

        if (declaredResidualPercent < 0m)
        {
            return (Codes.NegativeFraction,
                $"Lease {lease} declares a negative residual; an unowned share cannot be less than zero.");
        }

        var total = fractions.Sum(fraction => fraction.FractionPercent) + declaredResidualPercent;

        if (total == FullOwnership)
        {
            return null;
        }

        var owners = string.Join(", ", fractions.Select(fraction =>
            fraction.OwnerId + " " + Number(fraction.FractionPercent) + " %"));

        return (Codes.FractionsDoNotSum,
            $"Lease {lease} is over- or under-owned: {owners}" +
            (declaredResidualPercent == 0m
                ? string.Empty
                : ", residual " + Number(declaredResidualPercent) + " %") +
            $" totals {Number(total)} %, not 100 %. Correct the fractions or declare the residual share.");
    }

    /// <summary>The refusal codes the ownership gate returns, so a caller cannot misread a sum for a missing set.</summary>
    public static class Codes
    {
        /// <summary>No fraction is recorded for the lease.</summary>
        public const string MissingFraction = "royalty-ownership-missing";

        /// <summary>A fraction is negative.</summary>
        public const string NegativeFraction = "royalty-ownership-negative";

        /// <summary>The fractions and the residual do not reach 100 %.</summary>
        public const string FractionsDoNotSum = "royalty-ownership-not-summing";
    }

    private static string Number(decimal value) =>
        value.ToString("#,0.####", CultureInfo.InvariantCulture);
}

/// <summary>
/// The joint-interest billing arithmetic (ACC-07 AC-4): the net a partner owes is the charges less the credits.
/// </summary>
/// <remarks>
/// Kept as one method so the statement's header totals and the lines under them cannot disagree: the header is computed
/// from the lines that were written, not stored beside them.
/// </remarks>
public static class JibMath
{
    /// <summary>The net amount due, rounded once to the currency's minor unit.</summary>
    public static decimal Net(IEnumerable<decimal> charges, IEnumerable<decimal> credits)
    {
        ArgumentNullException.ThrowIfNull(charges);
        ArgumentNullException.ThrowIfNull(credits);

        return RoyaltyMath.Round(charges.Sum() - credits.Sum());
    }

    /// <summary>The total of a set of amounts, rounded once.</summary>
    public static decimal Total(IEnumerable<decimal> amounts)
    {
        ArgumentNullException.ThrowIfNull(amounts);

        return RoyaltyMath.Round(amounts.Sum());
    }
}

/// <summary>
/// The cost allocation arithmetic (ACC-07 AC-5), shared by the preview and the commit so they cannot differ.
/// </summary>
/// <remarks>
/// The pool is split by weight, in decimal, to the currency's minor unit, and the cents the rounding leaves over are given
/// to the largest remainders (the largest-remainder, or Hare, method) so the allocated amounts add back to the pool
/// <b>exactly</b>. A cost allocation that does not sum to the cost it allocates is a defect, not a rounding note, which is why
/// the remainder is distributed rather than left as a variance. Ties break on the target id, so the same input always gives the
/// same output and a preview can be compared with its commit.
/// </remarks>
public static class CostAllocationMath
{
    /// <summary>One target of an allocation: who receives a share, and its weight.</summary>
    /// <param name="TargetId">The property, well or cost centre.</param>
    /// <param name="TargetType">What kind of target it is, as the operator states it.</param>
    /// <param name="Weight">The basis weight; it only has to be proportional, not a percentage.</param>
    public readonly record struct AllocationTarget(string TargetId, string TargetType, decimal Weight);

    /// <summary>One allocated share.</summary>
    /// <param name="TargetId">The property, well or cost centre.</param>
    /// <param name="TargetType">What kind of target it is.</param>
    /// <param name="Percentage">The share of the pool, in percentage points.</param>
    /// <param name="Amount">The allocated money, to the cent.</param>
    /// <param name="Basis">The weight the share was computed from.</param>
    public readonly record struct AllocationShare(
        string TargetId,
        string TargetType,
        decimal Percentage,
        decimal Amount,
        decimal Basis);

    /// <summary>
    /// Allocates a pool across the targets by weight, exactly.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// No target was given, a weight is negative, or every weight is zero - there is nothing to allocate by.
    /// </exception>
    public static IReadOnlyList<AllocationShare> Allocate(
        decimal poolAmount,
        IReadOnlyList<AllocationTarget> targets)
    {
        ArgumentNullException.ThrowIfNull(targets);

        if (targets.Count == 0)
        {
            throw new ArgumentException("An allocation needs at least one target.", nameof(targets));
        }

        if (poolAmount < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(poolAmount), poolAmount, "A cost pool cannot be negative.");
        }

        if (targets.Any(target => target.Weight < 0m))
        {
            throw new ArgumentException("An allocation basis cannot be negative.", nameof(targets));
        }

        if (targets.Select(target => target.TargetId).Distinct(StringComparer.Ordinal).Count() != targets.Count)
        {
            throw new ArgumentException(
                "Each allocation target may appear once; two shares for one property cannot be told apart.",
                nameof(targets));
        }

        var totalWeight = targets.Sum(target => target.Weight);

        if (totalWeight <= 0m)
        {
            throw new ArgumentException(
                "Every allocation basis is zero, so there is nothing to allocate by.", nameof(targets));
        }

        // The exact share of each target, floored to the minor unit, and what the flooring leaves over.
        var poolCents = ToCents(poolAmount);

        var computed = targets
            .Select(target => (Target: target, Exact: poolCents * target.Weight / totalWeight))
            .Select(row => (row.Target, Cents: (long)decimal.Floor(row.Exact), Remainder: row.Exact - decimal.Floor(row.Exact)))
            .ToList();

        var allocated = computed.Sum(row => row.Cents);
        var remainder = poolCents - allocated;

        // Largest remainder first; ties broken on the target id so the result is deterministic.
        var order = computed
            .OrderByDescending(row => row.Remainder)
            .ThenBy(row => row.Target.TargetId, StringComparer.Ordinal)
            .Select((row, index) => (row.Target, row.Cents, index))
            .ToList();

        var cents = new Dictionary<string, long>(StringComparer.Ordinal);

        foreach (var (target, baseCents, index) in order)
        {
            cents[target.TargetId] = baseCents + (index < remainder ? 1 : 0);
        }

        return targets
            .Select(target => new AllocationShare(
                target.TargetId,
                target.TargetType,
                RoyaltyMath.Round(target.Weight / totalWeight * 100m),
                FromCents(cents[target.TargetId]),
                target.Weight))
            .ToList();
    }

    private static long ToCents(decimal amount) =>
        (long)decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);

    private static decimal FromCents(long cents) => cents / 100m;
}
