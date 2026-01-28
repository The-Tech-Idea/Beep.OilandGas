using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public enum PeriodClosingState
    {
        Open,
        InProgress,
        Closed,
        Locked,
        Reopened
    }
}
