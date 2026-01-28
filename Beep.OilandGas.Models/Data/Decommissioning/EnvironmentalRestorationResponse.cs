using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class EnvironmentalRestorationResponse : ModelEntityBase
    {
        private string RestorationIdValue = string.Empty;

        public string RestorationId

        {

            get { return this.RestorationIdValue; }

            set { SetProperty(ref RestorationIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? FacilityIdValue;

        public string? FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        
        // Restoration classification
        private string? RestorationTypeValue;

        public string? RestorationType

        {

            get { return this.RestorationTypeValue; }

            set { SetProperty(ref RestorationTypeValue, value); }

        }
        private string? RestorationMethodValue;

        public string? RestorationMethod

        {

            get { return this.RestorationMethodValue; }

            set { SetProperty(ref RestorationMethodValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        
        // Dates
        private DateTime? RestorationStartDateValue;

        public DateTime? RestorationStartDate

        {

            get { return this.RestorationStartDateValue; }

            set { SetProperty(ref RestorationStartDateValue, value); }

        }
        private DateTime? RestorationEndDateValue;

        public DateTime? RestorationEndDate

        {

            get { return this.RestorationEndDateValue; }

            set { SetProperty(ref RestorationEndDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private DateTime? VerificationDateValue;

        public DateTime? VerificationDate

        {

            get { return this.VerificationDateValue; }

            set { SetProperty(ref VerificationDateValue, value); }

        }
        
        // Area and scope
        private decimal? RestorationAreaValue;

        public decimal? RestorationArea

        {

            get { return this.RestorationAreaValue; }

            set { SetProperty(ref RestorationAreaValue, value); }

        }
        private string? RestorationAreaOuomValue;

        public string? RestorationAreaOuom

        {

            get { return this.RestorationAreaOuomValue; }

            set { SetProperty(ref RestorationAreaOuomValue, value); }

        }
        private string? RestorationScopeValue;

        public string? RestorationScope

        {

            get { return this.RestorationScopeValue; }

            set { SetProperty(ref RestorationScopeValue, value); }

        }
        
        // Costs
        private decimal? RestorationCostValue;

        public decimal? RestorationCost

        {

            get { return this.RestorationCostValue; }

            set { SetProperty(ref RestorationCostValue, value); }

        }
        private string? RestorationCostCurrencyValue;

        public string? RestorationCostCurrency

        {

            get { return this.RestorationCostCurrencyValue; }

            set { SetProperty(ref RestorationCostCurrencyValue, value); }

        }
        
        // Environmental impact
        private string? ImpactDescriptionValue;

        public string? ImpactDescription

        {

            get { return this.ImpactDescriptionValue; }

            set { SetProperty(ref ImpactDescriptionValue, value); }

        }
        private string? RemediationDescriptionValue;

        public string? RemediationDescription

        {

            get { return this.RemediationDescriptionValue; }

            set { SetProperty(ref RemediationDescriptionValue, value); }

        }
        private string? VerificationResultsValue;

        public string? VerificationResults

        {

            get { return this.VerificationResultsValue; }

            set { SetProperty(ref VerificationResultsValue, value); }

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
        private string? ComplianceStatusValue;

        public string? ComplianceStatus

        {

            get { return this.ComplianceStatusValue; }

            set { SetProperty(ref ComplianceStatusValue, value); }

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
