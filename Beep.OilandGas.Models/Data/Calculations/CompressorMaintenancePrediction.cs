using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CompressorMaintenancePrediction : ModelEntityBase
    {
        private string PredictionIdValue = string.Empty;

        public string PredictionId

        {

            get { return this.PredictionIdValue; }

            set { SetProperty(ref PredictionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private DateTime NextMaintenanceDateValue;

        public DateTime NextMaintenanceDate

        {

            get { return this.NextMaintenanceDateValue; }

            set { SetProperty(ref NextMaintenanceDateValue, value); }

        }
        private int HoursUntilMaintenanceValue;

        public int HoursUntilMaintenance

        {

            get { return this.HoursUntilMaintenanceValue; }

            set { SetProperty(ref HoursUntilMaintenanceValue, value); }

        }
        private string MaintenanceTypeValue = string.Empty;

        public string MaintenanceType

        {

            get { return this.MaintenanceTypeValue; }

            set { SetProperty(ref MaintenanceTypeValue, value); }

        } // Minor, Major, Overhaul
        private List<string> MaintenanceItemsValue = new();

        public List<string> MaintenanceItems

        {

            get { return this.MaintenanceItemsValue; }

            set { SetProperty(ref MaintenanceItemsValue, value); }

        }
        private decimal MaintenancePriorityValue;

        public decimal MaintenancePriority

        {

            get { return this.MaintenancePriorityValue; }

            set { SetProperty(ref MaintenancePriorityValue, value); }

        } // 0-100 scale
        private string RiskLevelValue = string.Empty;

        public string RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        } // Low, Medium, High
    }
}
