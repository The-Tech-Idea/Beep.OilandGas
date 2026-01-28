using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public enum OperationStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Cancelled,
        Skipped
    }
}
