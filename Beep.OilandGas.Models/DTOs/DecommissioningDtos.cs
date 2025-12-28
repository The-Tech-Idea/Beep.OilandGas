using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    #region Well Abandonment DTOs

    /// <summary>
    /// Request for creating or updating well abandonment record (maps to WELL_ABANDONMENT table)
    /// </summary>
    public class WellAbandonmentRequest
    {
        public string? AbandonmentId { get; set; }
        public string? WellId { get; set; }
        public string? FieldId { get; set; } // Auto-set by service
        
        // Abandonment classification
        public string? AbandonmentType { get; set; } // e.g., "PLUGGED", "TEMPORARILY_ABANDONED", "PERMANENTLY_ABANDONED"
        public string? AbandonmentMethod { get; set; } // e.g., "CEMENT_PLUG", "BRIDGE_PLUG", "MECHANICAL_PLUG"
        public string? Status { get; set; } // e.g., "PLANNED", "IN_PROGRESS", "COMPLETED"
        
        // Dates
        public DateTime? AbandonmentStartDate { get; set; }
        public DateTime? AbandonmentEndDate { get; set; }
        public DateTime? PluggingDate { get; set; }
        
        // Plugging details
        public decimal? PlugDepth { get; set; }
        public string? PlugDepthOuom { get; set; } // e.g., "FT", "M"
        public int? NumberOfPlugs { get; set; }
        public string? PlugType { get; set; }
        public decimal? CementVolume { get; set; }
        public string? CementVolumeOuom { get; set; } // e.g., "BBL", "M3"
        
        // Costs
        public decimal? AbandonmentCost { get; set; }
        public string? AbandonmentCostCurrency { get; set; }
        public decimal? PluggingCost { get; set; }
        public string? PluggingCostCurrency { get; set; }
        public decimal? SiteRestorationCost { get; set; }
        public string? SiteRestorationCostCurrency { get; set; }
        
        // Regulatory information
        public string? RegulatoryApprovalNumber { get; set; }
        public DateTime? RegulatoryApprovalDate { get; set; }
        public string? RegulatoryAuthority { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing well abandonment data (includes audit fields from WELL_ABANDONMENT table)
    /// </summary>
    public class WellAbandonmentResponse
    {
        public string AbandonmentId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? FieldId { get; set; }
        
        // Abandonment classification
        public string? AbandonmentType { get; set; }
        public string? AbandonmentMethod { get; set; }
        public string? Status { get; set; }
        
        // Dates
        public DateTime? AbandonmentStartDate { get; set; }
        public DateTime? AbandonmentEndDate { get; set; }
        public DateTime? PluggingDate { get; set; }
        
        // Plugging details
        public decimal? PlugDepth { get; set; }
        public string? PlugDepthOuom { get; set; }
        public int? NumberOfPlugs { get; set; }
        public string? PlugType { get; set; }
        public decimal? CementVolume { get; set; }
        public string? CementVolumeOuom { get; set; }
        
        // Costs
        public decimal? AbandonmentCost { get; set; }
        public string? AbandonmentCostCurrency { get; set; }
        public decimal? PluggingCost { get; set; }
        public string? PluggingCostCurrency { get; set; }
        public decimal? SiteRestorationCost { get; set; }
        public string? SiteRestorationCostCurrency { get; set; }
        
        // Regulatory information
        public string? RegulatoryApprovalNumber { get; set; }
        public DateTime? RegulatoryApprovalDate { get; set; }
        public string? RegulatoryAuthority { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
        public string? PreferredInd { get; set; }
        
        // Audit fields
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
    }

    #endregion

    #region Facility Decommissioning DTOs

    /// <summary>
    /// Request for creating or updating facility decommissioning record (maps to FACILITY_DECOMMISSIONING table)
    /// </summary>
    public class FacilityDecommissioningRequest
    {
        public string? DecommissioningId { get; set; }
        public string? FacilityId { get; set; }
        public string? FieldId { get; set; } // Auto-set by service
        
        // Decommissioning classification
        public string? DecommissioningType { get; set; } // e.g., "REMOVAL", "IN_SITU", "PARTIAL_REMOVAL"
        public string? DecommissioningMethod { get; set; } // e.g., "EXPLOSIVE_DEMOLITION", "MECHANICAL_DISMANTLING"
        public string? Status { get; set; } // e.g., "PLANNED", "IN_PROGRESS", "COMPLETED"
        
        // Dates
        public DateTime? DecommissioningStartDate { get; set; }
        public DateTime? DecommissioningEndDate { get; set; }
        public DateTime? RemovalDate { get; set; }
        public DateTime? SiteClearanceDate { get; set; }
        
        // Costs
        public decimal? DecommissioningCost { get; set; }
        public string? DecommissioningCostCurrency { get; set; }
        public decimal? RemovalCost { get; set; }
        public string? RemovalCostCurrency { get; set; }
        public decimal? SiteRestorationCost { get; set; }
        public string? SiteRestorationCostCurrency { get; set; }
        public decimal? TotalCost { get; set; }
        public string? TotalCostCurrency { get; set; }
        
        // Restoration information
        public string? RestorationStatus { get; set; } // e.g., "NOT_STARTED", "IN_PROGRESS", "COMPLETED"
        public DateTime? RestorationCompletionDate { get; set; }
        public string? RestorationMethod { get; set; }
        
        // Regulatory information
        public string? RegulatoryApprovalNumber { get; set; }
        public DateTime? RegulatoryApprovalDate { get; set; }
        public string? RegulatoryAuthority { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing facility decommissioning data (includes audit fields from FACILITY_DECOMMISSIONING table)
    /// </summary>
    public class FacilityDecommissioningResponse
    {
        public string DecommissioningId { get; set; } = string.Empty;
        public string? FacilityId { get; set; }
        public string? FieldId { get; set; }
        
        // Decommissioning classification
        public string? DecommissioningType { get; set; }
        public string? DecommissioningMethod { get; set; }
        public string? Status { get; set; }
        
        // Dates
        public DateTime? DecommissioningStartDate { get; set; }
        public DateTime? DecommissioningEndDate { get; set; }
        public DateTime? RemovalDate { get; set; }
        public DateTime? SiteClearanceDate { get; set; }
        
        // Costs
        public decimal? DecommissioningCost { get; set; }
        public string? DecommissioningCostCurrency { get; set; }
        public decimal? RemovalCost { get; set; }
        public string? RemovalCostCurrency { get; set; }
        public decimal? SiteRestorationCost { get; set; }
        public string? SiteRestorationCostCurrency { get; set; }
        public decimal? TotalCost { get; set; }
        public string? TotalCostCurrency { get; set; }
        
        // Restoration information
        public string? RestorationStatus { get; set; }
        public DateTime? RestorationCompletionDate { get; set; }
        public string? RestorationMethod { get; set; }
        
        // Regulatory information
        public string? RegulatoryApprovalNumber { get; set; }
        public DateTime? RegulatoryApprovalDate { get; set; }
        public string? RegulatoryAuthority { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
        public string? PreferredInd { get; set; }
        
        // Audit fields
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
    }

    #endregion

    #region Environmental Restoration DTOs

    /// <summary>
    /// Request for creating or updating environmental restoration activity (maps to ENVIRONMENTAL_RESTORATION table)
    /// </summary>
    public class EnvironmentalRestorationRequest
    {
        public string? RestorationId { get; set; }
        public string? FieldId { get; set; } // Auto-set by service
        public string? WellId { get; set; }
        public string? FacilityId { get; set; }
        
        // Restoration classification
        public string? RestorationType { get; set; } // e.g., "SOIL_REMEDIATION", "WATER_TREATMENT", "VEGETATION_RESTORATION"
        public string? RestorationMethod { get; set; } // e.g., "BIO_REMEDIATION", "EXCAVATION", "PHYTOREMEDIATION"
        public string? Status { get; set; } // e.g., "PLANNED", "IN_PROGRESS", "COMPLETED", "VERIFIED"
        
        // Dates
        public DateTime? RestorationStartDate { get; set; }
        public DateTime? RestorationEndDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? VerificationDate { get; set; }
        
        // Area and scope
        public decimal? RestorationArea { get; set; }
        public string? RestorationAreaOuom { get; set; } // e.g., "ACRE", "M2"
        public string? RestorationScope { get; set; } // Description of scope
        
        // Costs
        public decimal? RestorationCost { get; set; }
        public string? RestorationCostCurrency { get; set; }
        
        // Environmental impact
        public string? ImpactDescription { get; set; }
        public string? RemediationDescription { get; set; }
        public string? VerificationResults { get; set; }
        
        // Regulatory information
        public string? RegulatoryApprovalNumber { get; set; }
        public DateTime? RegulatoryApprovalDate { get; set; }
        public string? RegulatoryAuthority { get; set; }
        public string? ComplianceStatus { get; set; } // e.g., "COMPLIANT", "NON_COMPLIANT", "PENDING"
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing environmental restoration data (includes audit fields from ENVIRONMENTAL_RESTORATION table)
    /// </summary>
    public class EnvironmentalRestorationResponse
    {
        public string RestorationId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? WellId { get; set; }
        public string? FacilityId { get; set; }
        
        // Restoration classification
        public string? RestorationType { get; set; }
        public string? RestorationMethod { get; set; }
        public string? Status { get; set; }
        
        // Dates
        public DateTime? RestorationStartDate { get; set; }
        public DateTime? RestorationEndDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? VerificationDate { get; set; }
        
        // Area and scope
        public decimal? RestorationArea { get; set; }
        public string? RestorationAreaOuom { get; set; }
        public string? RestorationScope { get; set; }
        
        // Costs
        public decimal? RestorationCost { get; set; }
        public string? RestorationCostCurrency { get; set; }
        
        // Environmental impact
        public string? ImpactDescription { get; set; }
        public string? RemediationDescription { get; set; }
        public string? VerificationResults { get; set; }
        
        // Regulatory information
        public string? RegulatoryApprovalNumber { get; set; }
        public DateTime? RegulatoryApprovalDate { get; set; }
        public string? RegulatoryAuthority { get; set; }
        public string? ComplianceStatus { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
        public string? PreferredInd { get; set; }
        
        // Audit fields
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
    }

    #endregion

    #region Decommissioning Cost DTOs

    /// <summary>
    /// Request for creating or updating decommissioning cost record (maps to DECOMMISSIONING_COST table)
    /// </summary>
    public class DecommissioningCostRequest
    {
        public string? CostId { get; set; }
        public string? FieldId { get; set; } // Auto-set by service
        public string? WellId { get; set; }
        public string? FacilityId { get; set; }
        public string? AbandonmentId { get; set; }
        public string? DecommissioningId { get; set; }
        
        // Cost classification
        public string? CostType { get; set; } // e.g., "PLUGGING", "SITE_RESTORATION", "EQUIPMENT_REMOVAL", "REGULATORY"
        public string? CostCategory { get; set; } // e.g., "CAPITAL", "OPERATING"
        public string? Description { get; set; }
        
        // Cost amount
        public decimal? CostAmount { get; set; }
        public string? CostCurrency { get; set; }
        public DateTime? CostDate { get; set; }
        
        // Cost breakdown (optional)
        public decimal? LaborCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public decimal? EquipmentCost { get; set; }
        public decimal? TransportationCost { get; set; }
        public decimal? RegulatoryCost { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing decommissioning cost data (includes audit fields from DECOMMISSIONING_COST table)
    /// </summary>
    public class DecommissioningCostResponse
    {
        public string CostId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? WellId { get; set; }
        public string? FacilityId { get; set; }
        public string? AbandonmentId { get; set; }
        public string? DecommissioningId { get; set; }
        
        // Cost classification
        public string? CostType { get; set; }
        public string? CostCategory { get; set; }
        public string? Description { get; set; }
        
        // Cost amount
        public decimal? CostAmount { get; set; }
        public string? CostCurrency { get; set; }
        public DateTime? CostDate { get; set; }
        
        // Cost breakdown
        public decimal? LaborCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public decimal? EquipmentCost { get; set; }
        public decimal? TransportationCost { get; set; }
        public decimal? RegulatoryCost { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
        public string? PreferredInd { get; set; }
        
        // Audit fields
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
    }

    #endregion

    #region Decommissioning Cost Estimate DTOs

    /// <summary>
    /// Response for decommissioning cost estimation results
    /// </summary>
    public class DecommissioningCostEstimateResponse
    {
        public string EstimateId { get; set; } = Guid.NewGuid().ToString();
        public string? FieldId { get; set; }
        public DateTime EstimateDate { get; set; } = DateTime.UtcNow;
        
        // Estimated costs by category
        public decimal? EstimatedWellAbandonmentCost { get; set; }
        public decimal? EstimatedFacilityDecommissioningCost { get; set; }
        public decimal? EstimatedSiteRestorationCost { get; set; }
        public decimal? EstimatedRegulatoryCost { get; set; }
        public decimal? EstimatedTotalCost { get; set; }
        public string? CostCurrency { get; set; }
        
        // Cost breakdown by entity
        public int EstimatedWellsToAbandon { get; set; }
        public int EstimatedFacilitiesToDecommission { get; set; }
        public List<WellAbandonmentCostBreakdown> WellBreakdown { get; set; } = new List<WellAbandonmentCostBreakdown>();
        public List<FacilityDecommissioningCostBreakdown> FacilityBreakdown { get; set; } = new List<FacilityDecommissioningCostBreakdown>();
        
        // Assumptions and methodology
        public string? EstimationMethod { get; set; } // e.g., "ANALOG", "ENGINEERING_ESTIMATE", "HISTORICAL_DATA"
        public Dictionary<string, object>? Assumptions { get; set; }
        public string? Notes { get; set; }
        
        // Confidence levels
        public decimal? P10Estimate { get; set; } // Optimistic (10th percentile)
        public decimal? P50Estimate { get; set; } // Most likely (50th percentile)
        public decimal? P90Estimate { get; set; } // Conservative (90th percentile)
    }

    /// <summary>
    /// Well abandonment cost breakdown
    /// </summary>
    public class WellAbandonmentCostBreakdown
    {
        public string? WellId { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? CostCurrency { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Facility decommissioning cost breakdown
    /// </summary>
    public class FacilityDecommissioningCostBreakdown
    {
        public string? FacilityId { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? CostCurrency { get; set; }
        public string? Notes { get; set; }
    }

    #endregion
}
