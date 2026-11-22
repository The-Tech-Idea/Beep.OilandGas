using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Accounting.Calculations;

/// <summary>
/// Receivables ageing, computed from due dates (ACC-04 AC-2).
/// </summary>
/// <remarks>
/// The accounting project had an <c>ARAgingSummary</c> request shape but no stated bucket rule, so it is written once here:
/// an invoice is bucketed by how many days past its due date it is at the as-of date - <b>current</b> when it is not yet
/// due, then 30, 60 and 90+ days. An invoice with no due date is bucketed by its invoice date, and that choice is stated
/// rather than left to the screen. The buckets are inclusive at their upper bound: 30 days past due is still "30", 31 is
/// "60".
/// </remarks>
public static class ReceivableAgeing
{
    /// <summary>Current: not yet due.</summary>
    public const string Current = "CURRENT";

    /// <summary>One to thirty days past due.</summary>
    public const string Days30 = "30";

    /// <summary>Thirty-one to sixty days past due.</summary>
    public const string Days60 = "60";

    /// <summary>More than sixty days past due.</summary>
    public const string Days90 = "90+";

    /// <summary>The bucket names, in the order the grid shows them.</summary>
    public static readonly string[] Buckets = [Current, Days30, Days60, Days90];

    /// <summary>How many days past due an invoice is at the as-of date; negative when it is not yet due.</summary>
    public static int DaysPastDue(DateTime dueDate, DateTime asOf) =>
        (asOf.Date - dueDate.Date).Days;

    /// <summary>
    /// The bucket an open invoice belongs to, from its due date and the as-of date.
    /// </summary>
    public static string BucketOf(DateTime? dueDate, DateTime invoiceDate, DateTime asOf)
    {
        var days = DaysPastDue(dueDate?.Date ?? invoiceDate.Date, asOf);

        if (days <= 0)
        {
            return Current;
        }

        return days <= 30
            ? Days30
            : days <= 60
                ? Days60
                : Days90;
    }

    /// <summary>The bucket totals of a set of open items.</summary>
    public static ReceivableAgeingTotals Of(
        IEnumerable<(DateTime? DueDate, DateTime InvoiceDate, decimal Balance)> items,
        DateTime asOf)
    {
        ArgumentNullException.ThrowIfNull(items);

        var totals = new Dictionary<string, decimal>(StringComparer.Ordinal)
        {
            [Current] = 0m,
            [Days30] = 0m,
            [Days60] = 0m,
            [Days90] = 0m,
        };

        foreach (var (dueDate, invoiceDate, balance) in items)
        {
            totals[BucketOf(dueDate, invoiceDate, asOf)] += balance;
        }

        return new ReceivableAgeingTotals(
            totals[Current],
            totals[Days30],
            totals[Days60],
            totals[Days90]);
    }
}

/// <summary>The four ageing buckets and their total (ACC-04).</summary>
/// <param name="Current">Not yet due.</param>
/// <param name="Days30">One to thirty days past due.</param>
/// <param name="Days60">Thirty-one to sixty days past due.</param>
/// <param name="Days90">More than sixty days past due.</param>
public readonly record struct ReceivableAgeingTotals(
    decimal Current,
    decimal Days30,
    decimal Days60,
    decimal Days90)
{
    /// <summary>Everything outstanding, which is the four buckets added up.</summary>
    public decimal Total => Current + Days30 + Days60 + Days90;
}
