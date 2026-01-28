using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public enum SyncStatusType
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }
}
