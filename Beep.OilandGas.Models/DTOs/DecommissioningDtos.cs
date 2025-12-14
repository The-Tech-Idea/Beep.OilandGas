using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for well plugging operation.
    /// </summary>
    public class WellPluggingDto
    {
        public string PluggingId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string WellName { get; set; } = string.Empty;
        public DateTime? PluggingDate { get; set; }
        public string? PluggingMethod { get; set; }
        public decimal? PlugDepth { get; set; }
        public string? Status { get; set; }
        public string? VerifiedBy { get; set; }
        public DateTime? VerificationDate { get; set; }
        public bool? VerificationPassed { get; set; }
        public decimal? Cost { get; set; }
        public string? Currency { get; set; }
        public string? Contractor { get; set; }
        public string? Remarks { get; set; }
        public List<PluggingReportDto> Reports { get; set; } = new();
    }

    /// <summary>
    /// DTO for plugging report.
    /// </summary>
    public class PluggingReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string PluggingId { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public decimal? Depth { get; set; }
        public string? Activity { get; set; }
        public string? MaterialUsed { get; set; }
        public decimal? Quantity { get; set; }
        public string? QuantityUnit { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for facility decommissioning.
    /// </summary>
    public class FacilityDecommissioningDto
    {
        public string DecommissioningId { get; set; } = string.Empty;
        public string FacilityId { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? Status { get; set; }
        public string? DecommissioningMethod { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public string? Currency { get; set; }
        public string? Contractor { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for site restoration.
    /// </summary>
    public class SiteRestorationDto
    {
        public string RestorationId { get; set; } = string.Empty;
        public string SiteId { get; set; } = string.Empty;
        public string SiteName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? Status { get; set; }
        public string? RestorationType { get; set; }
        public bool? EnvironmentalClearance { get; set; }
        public DateTime? ClearanceDate { get; set; }
        public string? ClearedBy { get; set; }
        public decimal? Cost { get; set; }
        public string? Currency { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for abandonment operation.
    /// </summary>
    public class AbandonmentDto
    {
        public string AbandonmentId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string WellName { get; set; } = string.Empty;
        public DateTime? AbandonmentDate { get; set; }
        public string? AbandonmentReason { get; set; }
        public string? Status { get; set; }
        public bool? Plugged { get; set; }
        public bool? SiteRestored { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for creating a well plugging operation.
    /// </summary>
    public class CreateWellPluggingDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public DateTime? PlannedPluggingDate { get; set; }
        public string? PluggingMethod { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Currency { get; set; }
    }

    /// <summary>
    /// DTO for creating facility decommissioning.
    /// </summary>
    public class CreateFacilityDecommissioningDto
    {
        public string FacilityId { get; set; } = string.Empty;
        public DateTime? PlannedStartDate { get; set; }
        public string? DecommissioningMethod { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Currency { get; set; }
    }
}

