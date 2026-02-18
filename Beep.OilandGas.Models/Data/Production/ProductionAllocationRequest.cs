using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Production
{
    public class ProductionAllocationRequest
    {
        public DateTime AllocationDate { get; set; }
        public string AllocationMethod { get; set; } = "WellTestBase"; // WellTestBase, DowntimeAdjusted, etc.
        public List<string> WellIds { get; set; } = new List<string>();
        public bool DryRun { get; set; }
    }
}
