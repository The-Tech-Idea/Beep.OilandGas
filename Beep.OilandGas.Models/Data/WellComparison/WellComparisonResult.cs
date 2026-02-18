using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.WellComparison
{
    public class WellComparisonResult : ModelEntityBase
    {
        public string WellId { get; set; } = string.Empty;
        public Dictionary<string, object> FieldValues { get; set; } = new Dictionary<string, object>();
        public List<string> Differences { get; set; } = new List<string>();
        public bool IsMatch { get; set; }
    }
}
