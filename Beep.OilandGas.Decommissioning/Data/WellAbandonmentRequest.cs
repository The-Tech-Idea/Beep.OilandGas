using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WellAbandonmentRequest : ModelEntityBase
    {
        private string? AbandonmentIdValue;

        public string? AbandonmentId

        {

            get { return this.AbandonmentIdValue; }

            set { SetProperty(ref AbandonmentIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        } // Auto-set by service
        
        // Abandonment classification
        private string? AbandonmentTypeValue;

        public string? AbandonmentType

        {

            get { return this.AbandonmentTypeValue; }

            set { SetProperty(ref AbandonmentTypeValue, value); }

        } // e.g., "PLUGGED", "TEMPORARILY_ABANDONED", "PERMANENTLY_ABANDONED"
        private string? AbandonmentMethodValue;

        public string? AbandonmentMethod

        {

            get { return this.AbandonmentMethodValue; }

            set { SetProperty(ref AbandonmentMethodValue, value); }

        } // e.g., "CEMENT_PLUG", "BRIDGE_PLUG", "MECHANICAL_PLUG"
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // e.g., "PLANNED", "IN_PROGRESS", "COMPLETED"
        
        // Dates
        private DateTime? AbandonmentStartDateValue;

        public DateTime? AbandonmentStartDate

        {

            get { return this.AbandonmentStartDateValue; }

            set { SetProperty(ref AbandonmentStartDateValue, value); }

        }
        private DateTime? AbandonmentEndDateValue;

        public DateTime? AbandonmentEndDate

        {

            get { return this.AbandonmentEndDateValue; }

            set { SetProperty(ref AbandonmentEndDateValue, value); }

        }
        private DateTime? PluggingDateValue;

        public DateTime? PluggingDate

        {

            get { return this.PluggingDateValue; }

            set { SetProperty(ref PluggingDateValue, value); }

        }
        
        // Plugging details
        private decimal? PlugDepthValue;

        public decimal? PlugDepth

        {

            get { return this.PlugDepthValue; }

            set { SetProperty(ref PlugDepthValue, value); }

        }
        private string? PlugDepthOuomValue;

        public string? PlugDepthOuom

        {

            get { return this.PlugDepthOuomValue; }

            set { SetProperty(ref PlugDepthOuomValue, value); }

        } // e.g., "FT", "M"
        private int? NumberOfPlugsValue;

        public int? NumberOfPlugs

        {

            get { return this.NumberOfPlugsValue; }

            set { SetProperty(ref NumberOfPlugsValue, value); }

        }
        private string? PlugTypeValue;

        public string? PlugType

        {

            get { return this.PlugTypeValue; }

            set { SetProperty(ref PlugTypeValue, value); }

        }
        private decimal? CementVolumeValue;

        public decimal? CementVolume

        {

            get { return this.CementVolumeValue; }

            set { SetProperty(ref CementVolumeValue, value); }

        }
        private string? CementVolumeOuomValue;

        public string? CementVolumeOuom

        {

            get { return this.CementVolumeOuomValue; }

            set { SetProperty(ref CementVolumeOuomValue, value); }

        } // e.g., "BBL", "M3"
        
        // Costs
        private decimal? AbandonmentCostValue;

        public decimal? AbandonmentCost

        {

            get { return this.AbandonmentCostValue; }

            set { SetProperty(ref AbandonmentCostValue, value); }

        }
        private string? AbandonmentCostCurrencyValue;

        public string? AbandonmentCostCurrency

        {

            get { return this.AbandonmentCostCurrencyValue; }

            set { SetProperty(ref AbandonmentCostCurrencyValue, value); }

        }
        private decimal? PluggingCostValue;

        public decimal? PluggingCost

        {

            get { return this.PluggingCostValue; }

            set { SetProperty(ref PluggingCostValue, value); }

        }
        private string? PluggingCostCurrencyValue;

        public string? PluggingCostCurrency

        {

            get { return this.PluggingCostCurrencyValue; }

            set { SetProperty(ref PluggingCostCurrencyValue, value); }

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
    }
}
