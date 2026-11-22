using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.EconomicAnalysis.Calculations;

/// <summary>
/// The money arithmetic of an economic evaluation, in <c>decimal</c> (ECO-01 AC-3).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EconomicCalculator"/> computes in <c>double</c>, and the stage requires that <b>all money is decimal, never
/// double</b>. Rather than change formulas other callers rely on, the same relations are stated here in decimal, next to
/// them in the same owning project, and the screen uses these. The formulas are the ones the module already documents:
/// NPV = Σ amount_t / (1 + r)^t; IRR is the rate where that sum is zero; payback is the interpolated period where the
/// cumulative turns non-negative.
/// </para>
/// <para>
/// Where it differs from <see cref="EconomicCalculator"/>, it is deliberately stricter: the discount factor is computed by
/// repeated decimal multiplication (so it is exact decimal arithmetic rather than a <c>Math.Pow</c> double), and
/// <see cref="TryIrr"/> returns <b>false</b> when the cash flow never changes sign instead of returning a rate that does
/// not exist (AC-2). A payback that never happens is <c>null</c> rather than <c>double.MaxValue</c>.
/// </para>
/// </remarks>
public static class EconomicMoneyCalculator
{
    /// <summary>One period's money.</summary>
    /// <param name="Period">The period index, 0 for the first.</param>
    /// <param name="Amount">The net cash for that period; negative is an outflow.</param>
    public readonly record struct MoneyPeriod(int Period, decimal Amount);

    /// <summary>The guaranteed decimal range of an IRR search, matching the engine's own clamp.</summary>
    public const decimal MinimumRate = -0.99m;

    /// <summary>The upper bound of the IRR search - the engine clamps at 1,000 %.</summary>
    public const decimal MaximumRate = 10m;

    /// <summary>How fine the IRR bisection is; 1e-9 is far below any rate a report prints.</summary>
    public const decimal RateTolerance = 0.000000001m;

    /// <summary>
    /// Discounts one amount by a whole number of periods, in decimal.
    /// </summary>
    /// <remarks>
    /// <c>(1 + r)^t</c> is built by repeated multiplication, so the factor is a decimal value computed with decimal
    /// arithmetic - no <c>Math.Pow</c> and no binary floating point anywhere in the money path.
    /// </remarks>
    public static decimal DiscountFactor(decimal rate, int period)
    {
        if (period < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(period), period, "A period cannot be negative.");
        }

        if (rate <= -1m)
        {
            throw new ArgumentOutOfRangeException(nameof(rate), rate, "A discount rate must be above -100 %.");
        }

        var factor = 1m;
        var growth = 1m + rate;

        for (var step = 0; step < period; step++)
        {
            factor *= growth;
        }

        return 1m / factor;
    }

    /// <summary>
    /// The net present value of the periods, in decimal.
    /// </summary>
    /// <exception cref="ArgumentException">No periods were given.</exception>
    public static decimal Npv(IReadOnlyList<MoneyPeriod> periods, decimal rate)
    {
        ArgumentNullException.ThrowIfNull(periods);

        if (periods.Count == 0)
        {
            throw new ArgumentException("An evaluation needs at least one period.", nameof(periods));
        }

        var npv = 0m;

        foreach (var period in periods)
        {
            npv += period.Amount * DiscountFactor(rate, period.Period);
        }

        return npv;
    }

    /// <summary>
    /// The internal rate of return, or <b>false</b> when it does not exist (AC-2).
    /// </summary>
    /// <remarks>
    /// An IRR exists only when the cash flow changes sign, because the NPV curve has to cross zero. This checks that
    /// first and then bisects the NPV between the engine's own bounds, so the answer is a rate at which the NPV really is
    /// zero rather than whatever a Newton step happened to converge on.
    /// </remarks>
    public static bool TryIrr(IReadOnlyList<MoneyPeriod> periods, out decimal irr)
    {
        ArgumentNullException.ThrowIfNull(periods);
        irr = 0m;

        if (periods.Count == 0)
        {
            return false;
        }

        var hasOutflow = periods.Any(period => period.Amount < 0m);
        var hasInflow = periods.Any(period => period.Amount > 0m);

        // Every flow the same sign: the sum can never be zero at a positive rate, so there is no rate to report.
        if (!hasOutflow || !hasInflow)
        {
            return false;
        }

        var low = MinimumRate;
        var high = MaximumRate;
        var lowValue = Npv(periods, low);
        var highValue = Npv(periods, high);

        if (lowValue * highValue > 0m)
        {
            // The curve does not cross zero between the bounds - a second sign change or an all-positive profile.
            return false;
        }

        if (lowValue == 0m)
        {
            irr = low;
            return true;
        }

        if (highValue == 0m)
        {
            irr = high;
            return true;
        }

        for (var step = 0; step < 200; step++)
        {
            var middle = (low + high) / 2m;
            var value = Npv(periods, middle);

            if (Math.Abs(value) < 0.0000001m || high - low < RateTolerance)
            {
                irr = middle;
                return true;
            }

            if (value * lowValue > 0m)
            {
                low = middle;
                lowValue = value;
            }
            else
            {
                high = middle;
            }
        }

        irr = (low + high) / 2m;
        return true;
    }

    /// <summary>
    /// The payback period in periods, interpolated within the period it is recovered in; <b>null</b> when it never is.
    /// </summary>
    public static decimal? Payback(IReadOnlyList<MoneyPeriod> periods)
    {
        ArgumentNullException.ThrowIfNull(periods);

        var cumulative = 0m;
        var everNegative = false;

        foreach (var period in periods.OrderBy(period => period.Period))
        {
            var previous = cumulative;
            cumulative += period.Amount;

            if (cumulative < 0m)
            {
                everNegative = true;
            }

            if (previous < 0m && cumulative >= 0m && period.Amount != 0m)
            {
                // How far into the period the cumulative crosses zero.
                return period.Period - 1m + (-previous / period.Amount);
            }
        }

        // A project that is never out of pocket has nothing to pay back - it is recovered at the start, not never.
        return everNegative ? null : 0m;
    }

    /// <summary>The most negative cumulative balance - the money that has to be found before the project pays for itself.</summary>
    public static decimal PeakFunding(IReadOnlyList<MoneyPeriod> periods)
    {
        ArgumentNullException.ThrowIfNull(periods);

        var cumulative = 0m;
        var lowest = 0m;

        foreach (var period in periods.OrderBy(period => period.Period))
        {
            cumulative += period.Amount;
            lowest = Math.Min(lowest, cumulative);
        }

        return lowest;
    }

    /// <summary>The undiscounted sum of the periods.</summary>
    public static decimal Total(IReadOnlyList<MoneyPeriod> periods)
    {
        ArgumentNullException.ThrowIfNull(periods);

        return periods.Sum(period => period.Amount);
    }
}
