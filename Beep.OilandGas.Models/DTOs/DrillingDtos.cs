using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for drilling operation.
    /// </summary>
    public class DrillingOperationDto
    {
        public string OperationId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string WellName { get; set; } = string.Empty;
        public DateTime? SpudDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? Status { get; set; }
        public decimal? CurrentDepth { get; set; }
        public decimal? TargetDepth { get; set; }
        public string? DrillingContractor { get; set; }
        public string? RigName { get; set; }
        public decimal? DailyCost { get; set; }
        public decimal? TotalCost { get; set; }
        public string? Currency { get; set; }
        public List<DrillingReportDto> Reports { get; set; } = new();
    }

    /// <summary>
    /// DTO for drilling report.
    /// </summary>
    public class DrillingReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string OperationId { get; set; } = string.Empty;
        public DateTime? ReportDate { get; set; }
        public decimal? Depth { get; set; }
        public string? Activity { get; set; }
        public decimal? Hours { get; set; }
        public string? Remarks { get; set; }
        public string? ReportedBy { get; set; }
    }

    /// <summary>
    /// DTO for well construction.
    /// </summary>
    public class WellConstructionDto
    {
        public string ConstructionId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public List<CasingStringDto> CasingStrings { get; set; } = new();
        public List<CompletionStringDto> CompletionStrings { get; set; } = new();
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for casing string.
    /// </summary>
    public class CasingStringDto
    {
        public string CasingId { get; set; } = string.Empty;
        public string ConstructionId { get; set; } = string.Empty;
        public string CasingType { get; set; } = string.Empty;
        public decimal? TopDepth { get; set; }
        public decimal? BottomDepth { get; set; }
        public decimal? Diameter { get; set; }
        public string? DiameterUnit { get; set; }
        public string? Grade { get; set; }
        public decimal? Weight { get; set; }
        public string? WeightUnit { get; set; }
    }

    /// <summary>
    /// DTO for completion string.
    /// </summary>
    public class CompletionStringDto
    {
        public string CompletionId { get; set; } = string.Empty;
        public string ConstructionId { get; set; } = string.Empty;
        public string CompletionType { get; set; } = string.Empty;
        public decimal? TopDepth { get; set; }
        public decimal? BottomDepth { get; set; }
        public decimal? Diameter { get; set; }
        public string? DiameterUnit { get; set; }
        public List<PerforationDto> Perforations { get; set; } = new();
    }

    /// <summary>
    /// DTO for perforation.
    /// </summary>
    public class PerforationDto
    {
        public string PerforationId { get; set; } = string.Empty;
        public string CompletionId { get; set; } = string.Empty;
        public decimal? TopDepth { get; set; }
        public decimal? BottomDepth { get; set; }
        public int? ShotsPerFoot { get; set; }
        public string? PerforationType { get; set; }
        public DateTime? PerforationDate { get; set; }
    }

    /// <summary>
    /// DTO for facility construction.
    /// </summary>
    public class FacilityConstructionDto
    {
        public string ConstructionId { get; set; } = string.Empty;
        public string FacilityId { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public string FacilityType { get; set; } = string.Empty;
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? CommissioningDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public string? Currency { get; set; }
        public string? Contractor { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for creating a drilling operation.
    /// </summary>
    public class CreateDrillingOperationDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public DateTime? PlannedSpudDate { get; set; }
        public decimal? TargetDepth { get; set; }
        public string? DrillingContractor { get; set; }
        public string? RigName { get; set; }
        public decimal? EstimatedDailyCost { get; set; }
    }

    /// <summary>
    /// DTO for updating a drilling operation.
    /// </summary>
    public class UpdateDrillingOperationDto
    {
        public string? Status { get; set; }
        public decimal? CurrentDepth { get; set; }
        public decimal? DailyCost { get; set; }
        public DateTime? CompletionDate { get; set; }
    }

    /// <summary>
    /// DTO for creating a drilling report.
    /// </summary>
    public class CreateDrillingReportDto
    {
        public DateTime ReportDate { get; set; }
        public decimal? Depth { get; set; }
        public string? Activity { get; set; }
        public decimal? Hours { get; set; }
        public string? Remarks { get; set; }
    }
}

