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

        public DateTime VerificationDate { get; set; }
        public string VerifiedBy { get; set; }
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

    #region Decommissioning Advanced DTOs

    public class WellPluggingPlan : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private double WellDepthValue;
        public double WellDepth
        {
            get { return this.WellDepthValue; }
            set { SetProperty(ref WellDepthValue, value); }
        }

        private int ZonesIdentifiedValue;
        public int ZonesIdentified
        {
            get { return this.ZonesIdentifiedValue; }
            set { SetProperty(ref ZonesIdentifiedValue, value); }
        }

        private double FreshwaterAquiferDepthValue;
        public double FreshwaterAquiferDepth
        {
            get { return this.FreshwaterAquiferDepthValue; }
            set { SetProperty(ref FreshwaterAquiferDepthValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<string> CriticalZonesValue = new();
        public List<string> CriticalZones
        {
            get { return this.CriticalZonesValue; }
            set { SetProperty(ref CriticalZonesValue, value); }
        }

        private string PluggingStrategyValue = string.Empty;
        public string PluggingStrategy
        {
            get { return this.PluggingStrategyValue; }
            set { SetProperty(ref PluggingStrategyValue, value); }
        }

        private double CementRequirementsValue;
        public double CementRequirements
        {
            get { return this.CementRequirementsValue; }
            set { SetProperty(ref CementRequirementsValue, value); }
        }

        private List<string> PlugSpecificationsValue = new();
        public List<string> PlugSpecifications
        {
            get { return this.PlugSpecificationsValue; }
            set { SetProperty(ref PlugSpecificationsValue, value); }
        }

        private int EstimatedDaysRequiredValue;
        public int EstimatedDaysRequired
        {
            get { return this.EstimatedDaysRequiredValue; }
            set { SetProperty(ref EstimatedDaysRequiredValue, value); }
        }

        private List<string> PotentialIssuesValue = new();
        public List<string> PotentialIssues
        {
            get { return this.PotentialIssuesValue; }
            set { SetProperty(ref PotentialIssuesValue, value); }
        }
    }

    public class DecommissioningCostAnalysis : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private double WellDepthValue;
        public double WellDepth
        {
            get { return this.WellDepthValue; }
            set { SetProperty(ref WellDepthValue, value); }
        }

        private string WellTypeValue = string.Empty;
        public string WellType
        {
            get { return this.WellTypeValue; }
            set { SetProperty(ref WellTypeValue, value); }
        }

        private string LocationValue = string.Empty;
        public string Location
        {
            get { return this.LocationValue; }
            set { SetProperty(ref LocationValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private double WellPluggingCostValue;
        public double WellPluggingCost
        {
            get { return this.WellPluggingCostValue; }
            set { SetProperty(ref WellPluggingCostValue, value); }
        }

        private double WellheadRemovalCostValue;
        public double WellheadRemovalCost
        {
            get { return this.WellheadRemovalCostValue; }
            set { SetProperty(ref WellheadRemovalCostValue, value); }
        }

        private double SiteRestorationCostValue;
        public double SiteRestorationCost
        {
            get { return this.SiteRestorationCostValue; }
            set { SetProperty(ref SiteRestorationCostValue, value); }
        }

        private double EnvironmentalRemediationCostValue;
        public double EnvironmentalRemediationCost
        {
            get { return this.EnvironmentalRemediationCostValue; }
            set { SetProperty(ref EnvironmentalRemediationCostValue, value); }
        }

        private double AbandonmentBondCostValue;
        public double AbandonmentBondCost
        {
            get { return this.AbandonmentBondCostValue; }
            set { SetProperty(ref AbandonmentBondCostValue, value); }
        }

        private double TotalEstimatedCostValue;
        public double TotalEstimatedCost
        {
            get { return this.TotalEstimatedCostValue; }
            set { SetProperty(ref TotalEstimatedCostValue, value); }
        }

        private double PluggingCostPercentageValue;
        public double PluggingCostPercentage
        {
            get { return this.PluggingCostPercentageValue; }
            set { SetProperty(ref PluggingCostPercentageValue, value); }
        }

        private double WellheadRemovalPercentageValue;
        public double WellheadRemovalPercentage
        {
            get { return this.WellheadRemovalPercentageValue; }
            set { SetProperty(ref WellheadRemovalPercentageValue, value); }
        }

        private double SiteRestorationPercentageValue;
        public double SiteRestorationPercentage
        {
            get { return this.SiteRestorationPercentageValue; }
            set { SetProperty(ref SiteRestorationPercentageValue, value); }
        }

        private double ContingencyAmountValue;
        public double ContingencyAmount
        {
            get { return this.ContingencyAmountValue; }
            set { SetProperty(ref ContingencyAmountValue, value); }
        }

        private double TotalWithContingencyValue;
        public double TotalWithContingency
        {
            get { return this.TotalWithContingencyValue; }
            set { SetProperty(ref TotalWithContingencyValue, value); }
        }
    }

    public class EnvironmentalRemediationAnalysis : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private string LocationValue = string.Empty;
        public string Location
        {
            get { return this.LocationValue; }
            set { SetProperty(ref LocationValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<string> PotentialContaminantsValue = new();
        public List<string> PotentialContaminants
        {
            get { return this.PotentialContaminantsValue; }
            set { SetProperty(ref PotentialContaminantsValue, value); }
        }

        private string EnvironmentalRiskLevelValue = string.Empty;
        public string EnvironmentalRiskLevel
        {
            get { return this.EnvironmentalRiskLevelValue; }
            set { SetProperty(ref EnvironmentalRiskLevelValue, value); }
        }

        private List<string> RemediationActivitiesValue = new();
        public List<string> RemediationActivities
        {
            get { return this.RemediationActivitiesValue; }
            set { SetProperty(ref RemediationActivitiesValue, value); }
        }

        private int EstimatedRemediationMonthsValue;
        public int EstimatedRemediationMonths
        {
            get { return this.EstimatedRemediationMonthsValue; }
            set { SetProperty(ref EstimatedRemediationMonthsValue, value); }
        }

        private int MonitoringPeriodYearsValue;
        public int MonitoringPeriodYears
        {
            get { return this.MonitoringPeriodYearsValue; }
            set { SetProperty(ref MonitoringPeriodYearsValue, value); }
        }

        private double LongTermLiabilityCostValue;
        public double LongTermLiabilityCost
        {
            get { return this.LongTermLiabilityCostValue; }
            set { SetProperty(ref LongTermLiabilityCostValue, value); }
        }

        private List<string> RegulatoryRequirementsValue = new();
        public List<string> RegulatoryRequirements
        {
            get { return this.RegulatoryRequirementsValue; }
            set { SetProperty(ref RegulatoryRequirementsValue, value); }
        }
    }

    public class RegulatoryComplianceAnalysis : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private string JurisdictionValue = string.Empty;
        public string Jurisdiction
        {
            get { return this.JurisdictionValue; }
            set { SetProperty(ref JurisdictionValue, value); }
        }

        private string WellClassValue = string.Empty;
        public string WellClass
        {
            get { return this.WellClassValue; }
            set { SetProperty(ref WellClassValue, value); }
        }

        private DateTime AbandonmentDateValue;
        public DateTime AbandonmentDate
        {
            get { return this.AbandonmentDateValue; }
            set { SetProperty(ref AbandonmentDateValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<string> ApplicableRegulationsValue = new();
        public List<string> ApplicableRegulations
        {
            get { return this.ApplicableRegulationsValue; }
            set { SetProperty(ref ApplicableRegulationsValue, value); }
        }

        private List<string> ComplianceRequirementsValue = new();
        public List<string> ComplianceRequirements
        {
            get { return this.ComplianceRequirementsValue; }
            set { SetProperty(ref ComplianceRequirementsValue, value); }
        }

        private DateTime ComplianceDeadlineDateValue;
        public DateTime ComplianceDeadlineDate
        {
            get { return this.ComplianceDeadlineDateValue; }
            set { SetProperty(ref ComplianceDeadlineDateValue, value); }
        }

        private List<string> RequiredDocumentationValue = new();
        public List<string> RequiredDocumentation
        {
            get { return this.RequiredDocumentationValue; }
            set { SetProperty(ref RequiredDocumentationValue, value); }
        }

        private string BondingRequirementsValue = string.Empty;
        public string BondingRequirements
        {
            get { return this.BondingRequirementsValue; }
            set { SetProperty(ref BondingRequirementsValue, value); }
        }

        private List<string> InspectionRequirementsValue = new();
        public List<string> InspectionRequirements
        {
            get { return this.InspectionRequirementsValue; }
            set { SetProperty(ref InspectionRequirementsValue, value); }
        }

        private double ComplianceCostEstimateValue;
        public double ComplianceCostEstimate
        {
            get { return this.ComplianceCostEstimateValue; }
            set { SetProperty(ref ComplianceCostEstimateValue, value); }
        }
    }

    public class SalvageValueAnalysis : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private string WellTypeValue = string.Empty;
        public string WellType
        {
            get { return this.WellTypeValue; }
            set { SetProperty(ref WellTypeValue, value); }
        }

        private double WellDepthValue;
        public double WellDepth
        {
            get { return this.WellDepthValue; }
            set { SetProperty(ref WellDepthValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private double EquipmentSalvageValueValue;
        public double EquipmentSalvageValue
        {
            get { return this.EquipmentSalvageValueValue; }
            set { SetProperty(ref EquipmentSalvageValueValue, value); }
        }

        private double MetalScrapValueValue;
        public double MetalScrapValue
        {
            get { return this.MetalScrapValueValue; }
            set { SetProperty(ref MetalScrapValueValue, value); }
        }

        private double WellheadEquipmentValueValue;
        public double WellheadEquipmentValue
        {
            get { return this.WellheadEquipmentValueValue; }
            set { SetProperty(ref WellheadEquipmentValueValue, value); }
        }

        private double TotalSalvageValueValue;
        public double TotalSalvageValue
        {
            get { return this.TotalSalvageValueValue; }
            set { SetProperty(ref TotalSalvageValueValue, value); }
        }

        private double SalvageValuePercentageOfDecomCostValue;
        public double SalvageValuePercentageOfDecomCost
        {
            get { return this.SalvageValuePercentageOfDecomCostValue; }
            set { SetProperty(ref SalvageValuePercentageOfDecomCostValue, value); }
        }

        private List<string> SalvageableItemsValue = new();
        public List<string> SalvageableItems
        {
            get { return this.SalvageableItemsValue; }
            set { SetProperty(ref SalvageableItemsValue, value); }
        }

        private double TransportationCostValue;
        public double TransportationCost
        {
            get { return this.TransportationCostValue; }
            set { SetProperty(ref TransportationCostValue, value); }
        }

        private double NetSalvageValueValue;
        public double NetSalvageValue
        {
            get { return this.NetSalvageValueValue; }
            set { SetProperty(ref NetSalvageValueValue, value); }
        }
    }

    public class DecommissioningSchedule : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private double WellDepthValue;
        public double WellDepth
        {
            get { return this.WellDepthValue; }
            set { SetProperty(ref WellDepthValue, value); }
        }

        private int PriorityLevelValue;
        public int PriorityLevel
        {
            get { return this.PriorityLevelValue; }
            set { SetProperty(ref PriorityLevelValue, value); }
        }

        private DateTime PlannedStartDateValue;
        public DateTime PlannedStartDate
        {
            get { return this.PlannedStartDateValue; }
            set { SetProperty(ref PlannedStartDateValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<DecommissioningPhase> ProjectPhasesValue = new();
        public List<DecommissioningPhase> ProjectPhases
        {
            get { return this.ProjectPhasesValue; }
            set { SetProperty(ref ProjectPhasesValue, value); }
        }

        private int EstimatedTotalDaysValue;
        public int EstimatedTotalDays
        {
            get { return this.EstimatedTotalDaysValue; }
            set { SetProperty(ref EstimatedTotalDaysValue, value); }
        }

        private DateTime EstimatedCompletionDateValue;
        public DateTime EstimatedCompletionDate
        {
            get { return this.EstimatedCompletionDateValue; }
            set { SetProperty(ref EstimatedCompletionDateValue, value); }
        }

        private List<string> CriticalPathItemsValue = new();
        public List<string> CriticalPathItems
        {
            get { return this.CriticalPathItemsValue; }
            set { SetProperty(ref CriticalPathItemsValue, value); }
        }

        private string ScheduleRiskLevelValue = string.Empty;
        public string ScheduleRiskLevel
        {
            get { return this.ScheduleRiskLevelValue; }
            set { SetProperty(ref ScheduleRiskLevelValue, value); }
        }

        private int ContingencyDaysValue;
        public int ContingencyDays
        {
            get { return this.ContingencyDaysValue; }
            set { SetProperty(ref ContingencyDaysValue, value); }
        }

        private DateTime FinalEstimatedDateValue;
        public DateTime FinalEstimatedDate
        {
            get { return this.FinalEstimatedDateValue; }
            set { SetProperty(ref FinalEstimatedDateValue, value); }
        }

        private int EstimatedCrewSizeValue;
        public int EstimatedCrewSize
        {
            get { return this.EstimatedCrewSizeValue; }
            set { SetProperty(ref EstimatedCrewSizeValue, value); }
        }

        private List<string> EstimatedEquipmentNeedsValue = new();
        public List<string> EstimatedEquipmentNeeds
        {
            get { return this.EstimatedEquipmentNeedsValue; }
            set { SetProperty(ref EstimatedEquipmentNeedsValue, value); }
        }
    }

    public class DecommissioningPhase : ModelEntityBase
    {
        private string PhaseValue = string.Empty;
        public string Phase
        {
            get { return this.PhaseValue; }
            set { SetProperty(ref PhaseValue, value); }
        }

        private int DurationValue;
        public int Duration
        {
            get { return this.DurationValue; }
            set { SetProperty(ref DurationValue, value); }
        }
    }

    public class AbandonmentFeasibility : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private double WellDepthValue;
        public double WellDepth
        {
            get { return this.WellDepthValue; }
            set { SetProperty(ref WellDepthValue, value); }
        }

        private string WellStatusValue = string.Empty;
        public string WellStatus
        {
            get { return this.WellStatusValue; }
            set { SetProperty(ref WellStatusValue, value); }
        }

        private DateTime LastProductionDateValue;
        public DateTime LastProductionDate
        {
            get { return this.LastProductionDateValue; }
            set { SetProperty(ref LastProductionDateValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private string WellConditionStatusValue = string.Empty;
        public string WellConditionStatus
        {
            get { return this.WellConditionStatusValue; }
            set { SetProperty(ref WellConditionStatusValue, value); }
        }

        private bool AbandonmentFeasibleValue;
        public bool AbandonmentFeasible
        {
            get { return this.AbandonmentFeasibleValue; }
            set { SetProperty(ref AbandonmentFeasibleValue, value); }
        }

        private List<string> AbandonmentChallengesValue = new();
        public List<string> AbandonmentChallenges
        {
            get { return this.AbandonmentChallengesValue; }
            set { SetProperty(ref AbandonmentChallengesValue, value); }
        }

        private string RecommendedApproachValue = string.Empty;
        public string RecommendedApproach
        {
            get { return this.RecommendedApproachValue; }
            set { SetProperty(ref RecommendedApproachValue, value); }
        }

        private bool CanAbandonWithin12MonthsValue;
        public bool CanAbandonWithin12Months
        {
            get { return this.CanAbandonWithin12MonthsValue; }
            set { SetProperty(ref CanAbandonWithin12MonthsValue, value); }
        }

        private double AbandonmentBenefitValue;
        public double AbandonmentBenefit
        {
            get { return this.AbandonmentBenefitValue; }
            set { SetProperty(ref AbandonmentBenefitValue, value); }
        }

        private double AbandonmentCostValue;
        public double AbandonmentCost
        {
            get { return this.AbandonmentCostValue; }
            set { SetProperty(ref AbandonmentCostValue, value); }
        }

        private double NetBenefitValue;
        public double NetBenefit
        {
            get { return this.NetBenefitValue; }
            set { SetProperty(ref NetBenefitValue, value); }
        }

        private string AbandonmentRiskLevelValue = string.Empty;
        public string AbandonmentRiskLevel
        {
            get { return this.AbandonmentRiskLevelValue; }
            set { SetProperty(ref AbandonmentRiskLevelValue, value); }
        }
    }

    public class PortfolioDecommissioningAnalysis : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;
        public string FieldId
        {
            get { return this.FieldIdValue; }
            set { SetProperty(ref FieldIdValue, value); }
        }

        private int WellsToDecommissionValue;
        public int WellsToDecommission
        {
            get { return this.WellsToDecommissionValue; }
            set { SetProperty(ref WellsToDecommissionValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<PortfolioWellDecommissioning> WellAnalysesValue = new();
        public List<PortfolioWellDecommissioning> WellAnalyses
        {
            get { return this.WellAnalysesValue; }
            set { SetProperty(ref WellAnalysesValue, value); }
        }

        private double TotalEstimatedCostValue;
        public double TotalEstimatedCost
        {
            get { return this.TotalEstimatedCostValue; }
            set { SetProperty(ref TotalEstimatedCostValue, value); }
        }

        private int TotalEstimatedDaysValue;
        public int TotalEstimatedDays
        {
            get { return this.TotalEstimatedDaysValue; }
            set { SetProperty(ref TotalEstimatedDaysValue, value); }
        }

        private double AverageCostPerWellValue;
        public double AverageCostPerWell
        {
            get { return this.AverageCostPerWellValue; }
            set { SetProperty(ref AverageCostPerWellValue, value); }
        }

        private double AverageDaysPerWellValue;
        public double AverageDaysPerWell
        {
            get { return this.AverageDaysPerWellValue; }
            set { SetProperty(ref AverageDaysPerWellValue, value); }
        }

        private int PhaseCountValue;
        public int PhaseCount
        {
            get { return this.PhaseCountValue; }
            set { SetProperty(ref PhaseCountValue, value); }
        }

        private int WellsPerPhaseValue;
        public int WellsPerPhase
        {
            get { return this.WellsPerPhaseValue; }
            set { SetProperty(ref WellsPerPhaseValue, value); }
        }

        private double ContingencyPercentageValue;
        public double ContingencyPercentage
        {
            get { return this.ContingencyPercentageValue; }
            set { SetProperty(ref ContingencyPercentageValue, value); }
        }

        private double TotalWithContingencyValue;
        public double TotalWithContingency
        {
            get { return this.TotalWithContingencyValue; }
            set { SetProperty(ref TotalWithContingencyValue, value); }
        }
    }

    public class PortfolioWellDecommissioning : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private double WellDepthValue;
        public double WellDepth
        {
            get { return this.WellDepthValue; }
            set { SetProperty(ref WellDepthValue, value); }
        }

        private double EstimatedCostValue;
        public double EstimatedCost
        {
            get { return this.EstimatedCostValue; }
            set { SetProperty(ref EstimatedCostValue, value); }
        }

        private int EstimatedDaysValue;
        public int EstimatedDays
        {
            get { return this.EstimatedDaysValue; }
            set { SetProperty(ref EstimatedDaysValue, value); }
        }
    }

    #endregion
}


