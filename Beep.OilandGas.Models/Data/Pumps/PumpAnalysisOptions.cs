using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class PumpAnalysisOptions : ModelEntityBase
    {
        public List<double>? FlowRates { get; set; }
        public List<double>? Heads { get; set; }
        public List<double>? Powers { get; set; }
        public double? VaporPressure { get; set; }
    }
}
