using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class DecommissioningCostResponse : ModelEntityBase
    {
        private string CostIdValue = string.Empty;

        public string CostId

        {

            get { return this.CostIdValue; }

            set { SetProperty(ref CostIdValue, value); }

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
        private string? AbandonmentIdValue;

        public string? AbandonmentId

        {

            get { return this.AbandonmentIdValue; }

            set { SetProperty(ref AbandonmentIdValue, value); }

        }
        private string? DecommissioningIdValue;

        public string? DecommissioningId

        {

            get { return this.DecommissioningIdValue; }

            set { SetProperty(ref DecommissioningIdValue, value); }

        }
        
        // Cost classification
        private string? CostTypeValue;

        public string? CostType

        {

            get { return this.CostTypeValue; }

            set { SetProperty(ref CostTypeValue, value); }

        }
        private string? CostCategoryValue;

        public string? CostCategory

        {

            get { return this.CostCategoryValue; }

            set { SetProperty(ref CostCategoryValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Cost amount
        private decimal? CostAmountValue;

        public decimal? CostAmount

        {

            get { return this.CostAmountValue; }

            set { SetProperty(ref CostAmountValue, value); }

        }
        private string? CostCurrencyValue;

        public string? CostCurrency

        {

            get { return this.CostCurrencyValue; }

            set { SetProperty(ref CostCurrencyValue, value); }

        }
        private DateTime? CostDateValue;

        public DateTime? CostDate

        {

            get { return this.CostDateValue; }

            set { SetProperty(ref CostDateValue, value); }

        }
        
        // Cost breakdown
        private decimal? LaborCostValue;

        public decimal? LaborCost

        {

            get { return this.LaborCostValue; }

            set { SetProperty(ref LaborCostValue, value); }

        }
        private decimal? MaterialCostValue;

        public decimal? MaterialCost

        {

            get { return this.MaterialCostValue; }

            set { SetProperty(ref MaterialCostValue, value); }

        }
        private decimal? EquipmentCostValue;

        public decimal? EquipmentCost

        {

            get { return this.EquipmentCostValue; }

            set { SetProperty(ref EquipmentCostValue, value); }

        }
        private decimal? TransportationCostValue;

        public decimal? TransportationCost

        {

            get { return this.TransportationCostValue; }

            set { SetProperty(ref TransportationCostValue, value); }

        }
        private decimal? RegulatoryCostValue;

        public decimal? RegulatoryCost

        {

            get { return this.RegulatoryCostValue; }

            set { SetProperty(ref RegulatoryCostValue, value); }

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
