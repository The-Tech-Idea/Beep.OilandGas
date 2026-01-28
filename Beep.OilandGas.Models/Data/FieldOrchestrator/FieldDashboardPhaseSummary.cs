using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FieldDashboardPhaseSummary : ModelEntityBase
    {
        private string PhaseNameValue = string.Empty;

        public string PhaseName

        {

            get { return this.PhaseNameValue; }

            set { SetProperty(ref PhaseNameValue, value); }

        }
        private int EntityCountValue;

        public int EntityCount

        {

            get { return this.EntityCountValue; }

            set { SetProperty(ref EntityCountValue, value); }

        }
        public Dictionary<string, int> EntityCountsByType { get; set; } = new Dictionary<string, int>();
        private DateTime? LastActivityDateValue;

        public DateTime? LastActivityDate

        {

            get { return this.LastActivityDateValue; }

            set { SetProperty(ref LastActivityDateValue, value); }

        }
        private List<FieldPerformanceMetric> PhaseMetricsValue = new List<FieldPerformanceMetric>();

        public List<FieldPerformanceMetric> PhaseMetrics

        {

            get { return this.PhaseMetricsValue; }

            set { SetProperty(ref PhaseMetricsValue, value); }

        }
    }
}
