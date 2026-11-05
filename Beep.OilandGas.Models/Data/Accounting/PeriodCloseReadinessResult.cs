using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Whether the current field's period ending <see cref="PeriodEnd"/> may be closed: the checks the close itself makes
    /// before it runs (the period not closed already, allocations with their details, royalties accrued or paid, revenue
    /// allocated, production matching revenue, journal entries balanced), and what stands in the way when it may not.
    /// </summary>
    public class PeriodCloseReadinessResult
    {
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// The period's close status as its closing journal records it, a <c>PERIOD_CLOSE_STATUS</c> code
        /// (<c>OPEN</c>, <c>CLOSING</c>, <c>CLOSED</c>, <c>REOPENED</c>; ProductionAccounting's <c>PeriodCloseStatusCodes</c>).
        /// </summary>
        public string PeriodStatus { get; set; } = string.Empty;

        /// <summary>True when every check passes, so the close will run.</summary>
        public bool Ready { get; set; }

        /// <summary>The server's account of what is not reconciled; empty when <see cref="Ready"/>.</summary>
        public List<string> OutstandingItems { get; set; } = new();
    }
}
