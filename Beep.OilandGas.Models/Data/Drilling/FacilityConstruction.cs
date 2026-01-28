using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FacilityConstruction : ModelEntityBase
    {
        private string ConstructionIdValue = string.Empty;

        public string ConstructionId

        {

            get { return this.ConstructionIdValue; }

            set { SetProperty(ref ConstructionIdValue, value); }

        }
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string FacilityNameValue = string.Empty;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }
        private string FacilityTypeValue = string.Empty;

        public string FacilityType

        {

            get { return this.FacilityTypeValue; }

            set { SetProperty(ref FacilityTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private DateTime? CommissioningDateValue;

        public DateTime? CommissioningDate

        {

            get { return this.CommissioningDateValue; }

            set { SetProperty(ref CommissioningDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private decimal? ActualCostValue;

        public decimal? ActualCost

        {

            get { return this.ActualCostValue; }

            set { SetProperty(ref ActualCostValue, value); }

        }
        private string? CurrencyValue;

        public string? Currency

        {

            get { return this.CurrencyValue; }

            set { SetProperty(ref CurrencyValue, value); }

        }
        private string? ContractorValue;

        public string? Contractor

        {

            get { return this.ContractorValue; }

            set { SetProperty(ref ContractorValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }
}
