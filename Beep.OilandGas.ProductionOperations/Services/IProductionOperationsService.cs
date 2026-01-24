using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionOperations.Services
{
    /// <summary>
    /// Comprehensive production operations service interface
    /// Manages well production monitoring, equipment reliability, safety incidents, and environmental compliance
    /// </summary>
    public interface IProductionOperationsService
    {
        #region Well Production Monitoring

        /// <summary>
        /// Records daily well production data
        /// </summary>
        Task RecordWellProductionAsync(WellProductionData productionData, string userId);

        /// <summary>
        /// Retrieves well production data for specified period
        /// </summary>
        Task<List<WellProductionData>> GetWellProductionAsync(string wellUWI, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calculates well uptime percentage
        /// </summary>
        Task<WellUptime> CalculateWellUptimeAsync(string wellUWI, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets current well status and operational parameters
        /// </summary>
        Task<WellStatus> GetWellStatusAsync(string wellUWI);

        /// <summary>
        /// Updates well operational parameters
        /// </summary>
        Task UpdateWellParametersAsync(string wellUWI, WellParameters parameters, string userId);

        #endregion

        #region Equipment Reliability & Maintenance

        /// <summary>
        /// Records equipment maintenance activity
        /// </summary>
        Task RecordEquipmentMaintenanceAsync(EquipmentMaintenance maintenance, string userId);

        /// <summary>
        /// Gets equipment maintenance history
        /// </summary>
        Task<List<EquipmentMaintenance>> GetEquipmentMaintenanceHistoryAsync(string equipmentId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Schedules preventive maintenance
        /// </summary>
        Task ScheduleMaintenanceAsync(MaintenanceSchedule schedule, string userId);

        /// <summary>
        /// Gets upcoming maintenance schedules
        /// </summary>
        Task<List<MaintenanceSchedule>> GetUpcomingMaintenanceAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calculates equipment reliability metrics
        /// </summary>
        Task<EquipmentReliability> CalculateEquipmentReliabilityAsync(string equipmentId, DateTime startDate, DateTime endDate);

        #endregion

        #region Facility Operations

        /// <summary>
        /// Records facility production data
        /// </summary>
        Task RecordFacilityProductionAsync(FacilityProduction productionData, string userId);

        /// <summary>
        /// Gets facility production data
        /// </summary>
        Task<List<FacilityProduction>> GetFacilityProductionAsync(string facilityId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Updates facility operational status
        /// </summary>
        Task UpdateFacilityStatusAsync(string facilityId, FacilityStatus status, string userId);

        /// <summary>
        /// Gets facility operational status
        /// </summary>
        Task<FacilityStatus> GetFacilityStatusAsync(string facilityId);

        #endregion

        #region Safety & Incident Management

        /// <summary>
        /// Records safety incident
        /// </summary>
        Task RecordSafetyIncidentAsync(SafetyIncident incident, string userId);

        /// <summary>
        /// Gets safety incidents for specified criteria
        /// </summary>
        Task<List<SafetyIncident>> GetSafetyIncidentsAsync(DateTime startDate, DateTime endDate, string? wellUWI = null, string? facilityId = null);

        /// <summary>
        /// Updates safety incident status
        /// </summary>
        Task UpdateSafetyIncidentAsync(string incidentId, SafetyIncident incident, string userId);

        /// <summary>
        /// Calculates safety KPIs
        /// </summary>
        Task<SafetyKPIs> CalculateSafetyKPIsAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region Environmental Compliance

        /// <summary>
        /// Records environmental monitoring data
        /// </summary>
        Task RecordEnvironmentalDataAsync(EnvironmentalData data, string userId);

        /// <summary>
        /// Gets environmental monitoring data
        /// </summary>
        Task<List<EnvironmentalData>> GetEnvironmentalDataAsync(DateTime startDate, DateTime endDate, string? locationId = null);

        /// <summary>
        /// Performs environmental compliance check
        /// </summary>
        Task<ComplianceCheck> PerformEnvironmentalComplianceCheckAsync(string locationId, DateTime checkDate);

        /// <summary>
        /// Gets environmental compliance status
        /// </summary>
        Task<List<ComplianceStatus>> GetEnvironmentalComplianceStatusAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region Cost Analysis & Reporting

        /// <summary>
        /// Records operational costs
        /// </summary>
        Task RecordOperationalCostsAsync(OperationalCosts costs, string userId);

        /// <summary>
        /// Gets operational cost data
        /// </summary>
        Task<List<OperationalCosts>> GetOperationalCostsAsync(DateTime startDate, DateTime endDate, string? wellUWI = null, string? facilityId = null);

        /// <summary>
        /// Calculates cost per barrel/boe
        /// </summary>
        Task<CostAnalysis> CalculateCostAnalysisAsync(string wellUWI, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Generates operations report
        /// </summary>
        Task<OperationsReport> GenerateOperationsReportAsync(DateTime startDate, DateTime endDate, string? wellUWI = null, string? facilityId = null);

        #endregion

        #region Production Optimization

        /// <summary>
        /// Identifies production optimization opportunities
        /// </summary>
        Task<List<OptimizationOpportunity>> IdentifyOptimizationOpportunitiesAsync(string wellUWI);

        /// <summary>
        /// Implements production optimization recommendation
        /// </summary>
        Task ImplementOptimizationAsync(string opportunityId, string userId);

        /// <summary>
        /// Monitors optimization effectiveness
        /// </summary>
        Task<OptimizationEffectiveness> MonitorOptimizationEffectivenessAsync(string opportunityId);

        #endregion

        #region Data Management

        /// <summary>
        /// Gets production operations summary
        /// </summary>
        Task<ProductionOperationsSummary> GetProductionOperationsSummaryAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Exports operations data to specified format
        /// </summary>
        Task<byte[]> ExportOperationsDataAsync(string dataType, DateTime startDate, DateTime endDate, string format = "CSV");

        /// <summary>
        /// Validates operations data integrity
        /// </summary>
        Task<DataValidationResult> ValidateOperationsDataAsync(string dataType, DateTime startDate, DateTime endDate);

        #endregion
    }

    #region Production Operations DTOs

    /// <summary>
    /// Well production data DTO
    /// </summary>
    public class WellProductionData
    {
        public string WellUWI { get; set; } = string.Empty;
        public DateTime ProductionDate { get; set; }
        public decimal? OilVolume { get; set; } // STB
        public decimal? GasVolume { get; set; } // MSCF
        public decimal? WaterVolume { get; set; } // STB
        public decimal? OilRate { get; set; } // STB/day
        public decimal? GasRate { get; set; } // MSCF/day
        public decimal? WaterRate { get; set; } // STB/day
        public decimal? WellheadPressure { get; set; } // psi
        public decimal? BottomHolePressure { get; set; } // psi
        public decimal? FlowingTubingPressure { get; set; } // psi
        public decimal? CasingPressure { get; set; } // psi
        public string? OperationalStatus { get; set; }
        public string? Comments { get; set; }
    }

    /// <summary>
    /// Well uptime calculation result
    /// </summary>
    public class WellUptime
    {
        public string WellUWI { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public int ProducingDays { get; set; }
        public int DownDays { get; set; }
        public decimal UptimePercentage { get; set; }
        public List<DowntimeEvent> DowntimeEvents { get; set; } = new();
    }

    /// <summary>
    /// Downtime event DTO
    /// </summary>
    public class DowntimeEvent
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int DurationHours { get; set; }
    }

    /// <summary>
    /// Well status DTO
    /// </summary>
    public class WellStatus
    {
        public string WellUWI { get; set; } = string.Empty;
        public DateTime StatusDate { get; set; }
        public string OperationalStatus { get; set; } = string.Empty;
        public string WellType { get; set; } = string.Empty;
        public decimal? CurrentOilRate { get; set; }
        public decimal? CurrentGasRate { get; set; }
        public decimal? CurrentWaterRate { get; set; }
        public decimal? WellheadPressure { get; set; }
        public decimal? CasingPressure { get; set; }
        public string? LastMaintenanceDate { get; set; }
        public string? NextMaintenanceDate { get; set; }
        public List<WellIssue> CurrentIssues { get; set; } = new();
    }

    /// <summary>
    /// Well issue DTO
    /// </summary>
    public class WellIssue
    {
        public string IssueId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ReportedDate { get; set; }
    }

    /// <summary>
    /// Well parameters DTO
    /// </summary>
    public class WellParameters
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal? TargetRate { get; set; }
        public decimal? MaxAllowablePressure { get; set; }
        public decimal? MinAllowablePressure { get; set; }
        public string? ArtificialLiftMethod { get; set; }
        public decimal? PumpFrequency { get; set; }
        public string? ChokeSize { get; set; }
    }

    /// <summary>
    /// Equipment maintenance DTO
    /// </summary>
    public class EquipmentMaintenance
    {
        public string MaintenanceId { get; set; } = string.Empty;
        public string EquipmentId { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public DateTime MaintenanceDate { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public int DowntimeHours { get; set; }
        public List<string> PartsReplaced { get; set; } = new();
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Maintenance schedule DTO
    /// </summary>
    public class MaintenanceSchedule
    {
        public string ScheduleId { get; set; } = string.Empty;
        public string EquipmentId { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public int EstimatedDurationHours { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Equipment reliability DTO
    /// </summary>
    public class EquipmentReliability
    {
        public string EquipmentId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MeanTimeBetweenFailures { get; set; }
        public decimal MeanTimeToRepair { get; set; }
        public decimal AvailabilityPercentage { get; set; }
        public int TotalFailures { get; set; }
        public int TotalMaintenanceEvents { get; set; }
        public decimal TotalDowntimeHours { get; set; }
    }

    /// <summary>
    /// Facility production DTO
    /// </summary>
    public class FacilityProduction
    {
        public string FacilityId { get; set; } = string.Empty;
        public DateTime ProductionDate { get; set; }
        public decimal OilVolume { get; set; }
        public decimal GasVolume { get; set; }
        public decimal WaterVolume { get; set; }
        public decimal OilRate { get; set; }
        public decimal GasRate { get; set; }
        public decimal WaterRate { get; set; }
        public int ConnectedWells { get; set; }
        public int ProducingWells { get; set; }
        public string OperationalStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Facility status DTO
    /// </summary>
    public class FacilityStatus
    {
        public string FacilityId { get; set; } = string.Empty;
        public DateTime StatusDate { get; set; }
        public string OperationalStatus { get; set; } = string.Empty;
        public decimal CapacityUtilization { get; set; }
        public decimal ProcessingEfficiency { get; set; }
        public List<string> ActiveIssues { get; set; } = new();
        public DateTime? LastInspectionDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }
    }

    /// <summary>
    /// Safety incident DTO
    /// </summary>
    public class SafetyIncident
    {
        public string IncidentId { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string IncidentType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Injuries { get; set; } = new();
        public decimal PropertyDamage { get; set; }
        public string RootCause { get; set; } = string.Empty;
        public List<string> CorrectiveActions { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public string ReportedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Safety KPIs DTO
    /// </summary>
    public class SafetyKPIs
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalRecordableIncidents { get; set; }
        public decimal TRIR { get; set; } // Total Recordable Incident Rate
        public int LostTimeIncidents { get; set; }
        public decimal LTIR { get; set; } // Lost Time Incident Rate
        public int DaysAwayFromWork { get; set; }
        public decimal DART { get; set; } // Days Away, Restricted, or Transferred Rate
        public int NearMisses { get; set; }
        public string SafetyRating { get; set; } = string.Empty;
    }

    /// <summary>
    /// Environmental data DTO
    /// </summary>
    public class EnvironmentalData
    {
        public string LocationId { get; set; } = string.Empty;
        public DateTime MeasurementDate { get; set; }
        public string Parameter { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal? Limit { get; set; }
        public bool IsCompliant { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Compliance check result DTO
    /// </summary>
    public class ComplianceCheck
    {
        public string LocationId { get; set; } = string.Empty;
        public DateTime CheckDate { get; set; }
        public bool IsCompliant { get; set; }
        public List<ComplianceViolation> Violations { get; set; } = new();
        public string OverallStatus { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Compliance violation DTO
    /// </summary>
    public class ComplianceViolation
    {
        public string Parameter { get; set; } = string.Empty;
        public decimal MeasuredValue { get; set; }
        public decimal Limit { get; set; }
        public decimal Excess { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    /// <summary>
    /// Compliance status DTO
    /// </summary>
    public class ComplianceStatus
    {
        public string LocationId { get; set; } = string.Empty;
        public string Parameter { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int CompliantDays { get; set; }
        public int NonCompliantDays { get; set; }
        public decimal CompliancePercentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Operational costs DTO
    /// </summary>
    public class OperationalCosts
    {
        public string CostId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string FacilityId { get; set; } = string.Empty;
        public DateTime CostDate { get; set; }
        public string CostCategory { get; set; } = string.Empty;
        public string CostType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cost analysis DTO
    /// </summary>
    public class CostAnalysis
    {
        public string WellUWI { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalOperatingCosts { get; set; }
        public decimal TotalProductionVolume { get; set; }
        public decimal CostPerBOE { get; set; }
        public decimal CostPerBarrel { get; set; }
        public Dictionary<string, decimal> CostByCategory { get; set; } = new();
        public decimal CostTrend { get; set; } // Percentage change
        public int TotalCost { get; internal set; }
    }

    /// <summary>
    /// Operations report DTO
    /// </summary>
    public class OperationsReport
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ProductionOperationsSummary Summary { get; set; } = new();
        public List<WellPerformance> WellPerformance { get; set; } = new();
        public List<FacilityPerformance> FacilityPerformance { get; set; } = new();
        public SafetyKPIs SafetyMetrics { get; set; } = new();
        public List<ComplianceStatus> EnvironmentalCompliance { get; set; } = new();
        public CostAnalysis CostAnalysis { get; set; } = new();
    }

    /// <summary>
    /// Production operations summary DTO
    /// </summary>
    public class ProductionOperationsSummary
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalWells { get; set; }
        public int ActiveWells { get; set; }
        public int Facilities { get; set; }
        public decimal TotalOilProduction { get; set; }
        public decimal TotalGasProduction { get; set; }
        public decimal TotalWaterProduction { get; set; }
        public decimal AverageUptime { get; set; }
        public int SafetyIncidents { get; set; }
        public decimal TotalOperatingCosts { get; set; }
    }

    /// <summary>
    /// Well performance DTO
    /// </summary>
    public class WellPerformance
    {
        public string WellUWI { get; set; } = string.Empty;
        public decimal AverageOilRate { get; set; }
        public decimal AverageGasRate { get; set; }
        public decimal AverageWaterRate { get; set; }
        public decimal UptimePercentage { get; set; }
        public int DowntimeHours { get; set; }
        public decimal OperatingCosts { get; set; }
        public decimal CostPerBOE { get; set; }
        public string PerformanceRating { get; set; } = string.Empty;
    }

    /// <summary>
    /// Facility performance DTO
    /// </summary>
    public class FacilityPerformance
    {
        public string FacilityId { get; set; } = string.Empty;
        public decimal CapacityUtilization { get; set; }
        public decimal ProcessingEfficiency { get; set; }
        public decimal OperatingCosts { get; set; }
        public int ConnectedWells { get; set; }
        public int ProducingWells { get; set; }
        public string PerformanceRating { get; set; } = string.Empty;
    }

    /// <summary>
    /// Optimization opportunity DTO
    /// </summary>
    public class OptimizationOpportunity
    {
        public string OpportunityId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string OpportunityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedGain { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal PaybackPeriod { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Optimization effectiveness DTO
    /// </summary>
    public class OptimizationEffectiveness
    {
        public string OpportunityId { get; set; } = string.Empty;
        public DateTime ImplementationDate { get; set; }
        public decimal BaselinePerformance { get; set; }
        public decimal CurrentPerformance { get; set; }
        public decimal PerformanceImprovement { get; set; }
        public decimal ActualGain { get; set; }
        public decimal ActualCost { get; set; }
        public decimal ROI { get; set; }
        public string EffectivenessRating { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data validation result DTO
    /// </summary>
    public class DataValidationResult
    {
        public string DataType { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public int TotalRecords { get; set; }
        public int ValidRecords { get; set; }
        public int InvalidRecords { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public string ValidationSummary { get; set; } = string.Empty;
    }

    #endregion
}