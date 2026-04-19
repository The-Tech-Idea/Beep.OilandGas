using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FacilityDecommissioningResponse : ModelEntityBase
    {
        private string DecommissioningIdValue = string.Empty;

        public string DecommissioningId

        {

            get { return this.DecommissioningIdValue; }

            set { SetProperty(ref DecommissioningIdValue, value); }

        }
        private string? FacilityIdValue;

        public string? FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        
        // Decommissioning classification
        private string? DecommissioningTypeValue;

        public string? DecommissioningType

        {

            get { return this.DecommissioningTypeValue; }

            set { SetProperty(ref DecommissioningTypeValue, value); }

        }
        private string? DecommissioningMethodValue;

        public string? DecommissioningMethod

        {

            get { return this.DecommissioningMethodValue; }

            set { SetProperty(ref DecommissioningMethodValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        
        // Dates
        private DateTime? DecommissioningStartDateValue;

        public DateTime? DecommissioningStartDate

        {

            get { return this.DecommissioningStartDateValue; }

            set { SetProperty(ref DecommissioningStartDateValue, value); }

        }
        private DateTime? DecommissioningEndDateValue;

        public DateTime? DecommissioningEndDate

        {

            get { return this.DecommissioningEndDateValue; }

            set { SetProperty(ref DecommissioningEndDateValue, value); }

        }
        private DateTime? RemovalDateValue;

        public DateTime? RemovalDate

        {

            get { return this.RemovalDateValue; }

            set { SetProperty(ref RemovalDateValue, value); }

        }
        private DateTime? SiteClearanceDateValue;

        public DateTime? SiteClearanceDate

        {

            get { return this.SiteClearanceDateValue; }

            set { SetProperty(ref SiteClearanceDateValue, value); }

        }
        
        // Costs
        private decimal? DecommissioningCostValue;

        public decimal? DecommissioningCost

        {

            get { return this.DecommissioningCostValue; }

            set { SetProperty(ref DecommissioningCostValue, value); }

        }
        private string? DecommissioningCostCurrencyValue;

        public string? DecommissioningCostCurrency

        {

            get { return this.DecommissioningCostCurrencyValue; }

            set { SetProperty(ref DecommissioningCostCurrencyValue, value); }

        }
        private decimal? RemovalCostValue;

        public decimal? RemovalCost

        {

            get { return this.RemovalCostValue; }

            set { SetProperty(ref RemovalCostValue, value); }

        }
        private string? RemovalCostCurrencyValue;

        public string? RemovalCostCurrency

        {

            get { return this.RemovalCostCurrencyValue; }

            set { SetProperty(ref RemovalCostCurrencyValue, value); }

        }
        private decimal? SiteRestorationCostValue;

        public decimal? SiteRestorationCost

        {

            get { return this.SiteRestorationCostValue; }

            set { SetProperty(ref SiteRestorationCostValue, value); }

        }
        private string? SiteRestorationCostCurrencyValue;

        public string? SiteRestorationCostCurrency

        {

            get { return this.SiteRestorationCostCurrencyValue; }

            set { SetProperty(ref SiteRestorationCostCurrencyValue, value); }

        }
        private decimal? TotalCostValue;

        public decimal? TotalCost

        {

            get { return this.TotalCostValue; }

            set { SetProperty(ref TotalCostValue, value); }

        }
        private string? TotalCostCurrencyValue;

        public string? TotalCostCurrency

        {

            get { return this.TotalCostCurrencyValue; }

            set { SetProperty(ref TotalCostCurrencyValue, value); }

        }
        
        // Restoration information
        private string? RestorationStatusValue;

        public string? RestorationStatus

        {

            get { return this.RestorationStatusValue; }

            set { SetProperty(ref RestorationStatusValue, value); }

        }
        private DateTime? RestorationCompletionDateValue;

        public DateTime? RestorationCompletionDate

        {

            get { return this.RestorationCompletionDateValue; }

            set { SetProperty(ref RestorationCompletionDateValue, value); }

        }
        private string? RestorationMethodValue;

        public string? RestorationMethod

        {

            get { return this.RestorationMethodValue; }

            set { SetProperty(ref RestorationMethodValue, value); }

        }
        
        // Regulatory information
        private string? RegulatoryApprovalNumberValue;

        public string? RegulatoryApprovalNumber

        {

            get { return this.RegulatoryApprovalNumberValue; }

            set { SetProperty(ref RegulatoryApprovalNumberValue, value); }

        }
        private DateTime? RegulatoryApprovalDateValue;

        public DateTime? RegulatoryApprovalDate

        {

            get { return this.RegulatoryApprovalDateValue; }

            set { SetProperty(ref RegulatoryApprovalDateValue, value); }

        }
        private string? RegulatoryAuthorityValue;

        public string? RegulatoryAuthority

        {

            get { return this.RegulatoryAuthorityValue; }

            set { SetProperty(ref RegulatoryAuthorityValue, value); }

        }
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
        private string? PreferredIndValue;

        public string? PreferredInd

        {

            get { return this.PreferredIndValue; }

            set { SetProperty(ref PreferredIndValue, value); }

        }
        
        // Audit fields
        private DateTime? CreateDateValue;

        public DateTime? CreateDate

        {

            get { return this.CreateDateValue; }

            set { SetProperty(ref CreateDateValue, value); }

        }
        private string? CreateUserValue;

        public string? CreateUser

        {

            get { return this.CreateUserValue; }

            set { SetProperty(ref CreateUserValue, value); }

        }
        private DateTime? UpdateDateValue;

        public DateTime? UpdateDate

        {

            get { return this.UpdateDateValue; }

            set { SetProperty(ref UpdateDateValue, value); }

        }
        private string? UpdateUserValue;

        public string? UpdateUser

        {

            get { return this.UpdateUserValue; }

            set { SetProperty(ref UpdateUserValue, value); }

        }
    }
}
