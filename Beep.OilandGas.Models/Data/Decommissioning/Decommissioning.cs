using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    #region Well Abandonment DTOs

    /// <summary>
    /// Request for creating or updating well abandonment class (maps to WELL_ABANDONMENT table)
    /// </summary>
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

    /// <summary>
    /// Response containing well abandonment data (includes audit fields from WELL_ABANDONMENT table)
    /// </summary>
    public class WellAbandonmentResponse : ModelEntityBase
    {
        private string AbandonmentIdValue = string.Empty;

        public string AbandonmentId

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

        }
        
        // Abandonment classification
        private string? AbandonmentTypeValue;

        public string? AbandonmentType

        {

            get { return this.AbandonmentTypeValue; }

            set { SetProperty(ref AbandonmentTypeValue, value); }

        }
        private string? AbandonmentMethodValue;

        public string? AbandonmentMethod

        {

            get { return this.AbandonmentMethodValue; }

            set { SetProperty(ref AbandonmentMethodValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        
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

        }
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

        }
        
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

    #endregion

    #region Facility Decommissioning DTOs

    /// <summary>
    /// Request for creating or updating facility decommissioning class (maps to FACILITY_DECOMMISSIONING table)
    /// </summary>
    public class FacilityDecommissioningRequest : ModelEntityBase
    {
        private string? DecommissioningIdValue;

        public string? DecommissioningId

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

        } // Auto-set by service
        
        // Decommissioning classification
        private string? DecommissioningTypeValue;

        public string? DecommissioningType

        {

            get { return this.DecommissioningTypeValue; }

            set { SetProperty(ref DecommissioningTypeValue, value); }

        } // e.g., "REMOVAL", "IN_SITU", "PARTIAL_REMOVAL"
        private string? DecommissioningMethodValue;

        public string? DecommissioningMethod

        {

            get { return this.DecommissioningMethodValue; }

            set { SetProperty(ref DecommissioningMethodValue, value); }

        } // e.g., "EXPLOSIVE_DEMOLITION", "MECHANICAL_DISMANTLING"
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // e.g., "PLANNED", "IN_PROGRESS", "COMPLETED"
        
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

        } // e.g., "NOT_STARTED", "IN_PROGRESS", "COMPLETED"
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
    }

    /// <summary>
    /// Response containing facility decommissioning data (includes audit fields from FACILITY_DECOMMISSIONING table)
    /// </summary>
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

    #endregion

    #region Environmental Restoration DTOs

    /// <summary>
    /// Request for creating or updating environmental restoration activity (maps to ENVIRONMENTAL_RESTORATION table)
    /// </summary>
    public class EnvironmentalRestorationRequest : ModelEntityBase
    {
        private string? RestorationIdValue;

        public string? RestorationId

        {

            get { return this.RestorationIdValue; }

            set { SetProperty(ref RestorationIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        } // Auto-set by service
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

        } // e.g., "SOIL_REMEDIATION", "WATER_TREATMENT", "VEGETATION_RESTORATION"
        private string? RestorationMethodValue;

        public string? RestorationMethod

        {

            get { return this.RestorationMethodValue; }

            set { SetProperty(ref RestorationMethodValue, value); }

        } // e.g., "BIO_REMEDIATION", "EXCAVATION", "PHYTOREMEDIATION"
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // e.g., "PLANNED", "IN_PROGRESS", "COMPLETED", "VERIFIED"
        
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

        } // e.g., "ACRE", "M2"
        private string? RestorationScopeValue;

        public string? RestorationScope

        {

            get { return this.RestorationScopeValue; }

            set { SetProperty(ref RestorationScopeValue, value); }

        } // Description of scope
        
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

        } // e.g., "COMPLIANT", "NON_COMPLIANT", "PENDING"
        
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

    /// <summary>
    /// Response containing environmental restoration data (includes audit fields from ENVIRONMENTAL_RESTORATION table)
    /// </summary>
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

    #endregion

    #region Decommissioning Cost DTOs

    /// <summary>
    /// Request for creating or updating decommissioning cost class (maps to DECOMMISSIONING_COST table)
    /// </summary>
    public class DecommissioningCostRequest : ModelEntityBase
    {
        private string? CostIdValue;

        public string? CostId

        {

            get { return this.CostIdValue; }

            set { SetProperty(ref CostIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        } // Auto-set by service
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

        } // e.g., "PLUGGING", "SITE_RESTORATION", "EQUIPMENT_REMOVAL", "REGULATORY"
        private string? CostCategoryValue;

        public string? CostCategory

        {

            get { return this.CostCategoryValue; }

            set { SetProperty(ref CostCategoryValue, value); }

        } // e.g., "CAPITAL", "OPERATING"
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
        
        // Cost breakdown (optional)
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
    }

    /// <summary>
    /// Response containing decommissioning cost data (includes audit fields from DECOMMISSIONING_COST table)
    /// </summary>
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

    #endregion

    #region Decommissioning Cost Estimate DTOs

    /// <summary>
    /// Response for decommissioning cost estimation results
    /// </summary>
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

    /// <summary>
    /// Well abandonment cost breakdown
    /// </summary>
    public class WellAbandonmentCostBreakdown : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? CostCurrencyValue;

        public string? CostCurrency

        {

            get { return this.CostCurrencyValue; }

            set { SetProperty(ref CostCurrencyValue, value); }

        }
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    /// <summary>
    /// Facility decommissioning cost breakdown
    /// </summary>
    public class FacilityDecommissioningCostBreakdown : ModelEntityBase
    {
        private string? FacilityIdValue;

        public string? FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? CostCurrencyValue;

        public string? CostCurrency

        {

            get { return this.CostCurrencyValue; }

            set { SetProperty(ref CostCurrencyValue, value); }

        }
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    #endregion
}


