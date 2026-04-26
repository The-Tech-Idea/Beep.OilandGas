using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class DecommissioningCostEstimateResponse : ModelEntityBase
    {
        private string EstimateIdValue = Guid.NewGuid().ToString();

        public string EstimateId

        {

            get { return this.EstimateIdValue; }

            set { SetProperty(ref EstimateIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime EstimateDateValue = DateTime.UtcNow;

        public DateTime EstimateDate

        {

            get { return this.EstimateDateValue; }

            set { SetProperty(ref EstimateDateValue, value); }

        }
        
        // Estimated costs by category
        private decimal? EstimatedWellAbandonmentCostValue;

        public decimal? EstimatedWellAbandonmentCost

        {

            get { return this.EstimatedWellAbandonmentCostValue; }

            set { SetProperty(ref EstimatedWellAbandonmentCostValue, value); }

        }
        private decimal? EstimatedFacilityDecommissioningCostValue;

        public decimal? EstimatedFacilityDecommissioningCost

        {

            get { return this.EstimatedFacilityDecommissioningCostValue; }

            set { SetProperty(ref EstimatedFacilityDecommissioningCostValue, value); }

        }
        private decimal? EstimatedSiteRestorationCostValue;

        public decimal? EstimatedSiteRestorationCost

        {

            get { return this.EstimatedSiteRestorationCostValue; }

            set { SetProperty(ref EstimatedSiteRestorationCostValue, value); }

        }
        private decimal? EstimatedRegulatoryCostValue;

        public decimal? EstimatedRegulatoryCost

        {

            get { return this.EstimatedRegulatoryCostValue; }

            set { SetProperty(ref EstimatedRegulatoryCostValue, value); }

        }
        private decimal? EstimatedTotalCostValue;

        public decimal? EstimatedTotalCost

        {

            get { return this.EstimatedTotalCostValue; }

            set { SetProperty(ref EstimatedTotalCostValue, value); }

        }
        private string? CostCurrencyValue;

        public string? CostCurrency

        {

            get { return this.CostCurrencyValue; }

            set { SetProperty(ref CostCurrencyValue, value); }

        }
        
        // Cost breakdown by entity
        private int EstimatedWellsToAbandonValue;

        public int EstimatedWellsToAbandon

        {

            get { return this.EstimatedWellsToAbandonValue; }

            set { SetProperty(ref EstimatedWellsToAbandonValue, value); }

        }
        private int EstimatedFacilitiesToDecommissionValue;

        public int EstimatedFacilitiesToDecommission

        {

            get { return this.EstimatedFacilitiesToDecommissionValue; }

            set { SetProperty(ref EstimatedFacilitiesToDecommissionValue, value); }

        }
        private List<WellAbandonmentCostBreakdown> WellBreakdownValue = new List<WellAbandonmentCostBreakdown>();

        public List<WellAbandonmentCostBreakdown> WellBreakdown

        {

            get { return this.WellBreakdownValue; }

            set { SetProperty(ref WellBreakdownValue, value); }

        }
        private List<FacilityDecommissioningCostBreakdown> FacilityBreakdownValue = new List<FacilityDecommissioningCostBreakdown>();

        public List<FacilityDecommissioningCostBreakdown> FacilityBreakdown

        {

            get { return this.FacilityBreakdownValue; }

            set { SetProperty(ref FacilityBreakdownValue, value); }

        }
        
        // Assumptions and methodology
        private string? EstimationMethodValue;

        public string? EstimationMethod

        {

            get { return this.EstimationMethodValue; }

            set { SetProperty(ref EstimationMethodValue, value); }

        } // e.g., "ANALOG", "ENGINEERING_ESTIMATE", "HISTORICAL_DATA"
        public Dictionary<string, object>? Assumptions { get; set; }
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
        
        // Confidence levels
        private decimal? P10EstimateValue;

        public decimal? P10Estimate

        {

            get { return this.P10EstimateValue; }

            set { SetProperty(ref P10EstimateValue, value); }

        } // Optimistic (10th percentile)
        private decimal? P50EstimateValue;

        public decimal? P50Estimate

        {

            get { return this.P50EstimateValue; }

            set { SetProperty(ref P50EstimateValue, value); }

        } // Most likely (50th percentile)
        private decimal? P90EstimateValue;

        public decimal? P90Estimate

        {

            get { return this.P90EstimateValue; }

            set { SetProperty(ref P90EstimateValue, value); }

        } // Conservative (90th percentile)
    }
}
