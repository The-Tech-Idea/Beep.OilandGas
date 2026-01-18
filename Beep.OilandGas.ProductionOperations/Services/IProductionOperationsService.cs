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
        Task RecordWellProductionAsync(WellProductionDataDto productionData, string userId);

        /// <summary>
        /// Retrieves well production data for specified period
        /// </summary>
        Task<List<WellProductionDataDto>> GetWellProductionAsync(string wellUWI, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calculates well uptime percentage
        /// </summary>
        Task<WellUptimeDto> CalculateWellUptimeAsync(string wellUWI, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets current well status and operational parameters
        /// </summary>
        Task<WellStatusDto> GetWellStatusAsync(string wellUWI);

        /// <summary>
        /// Updates well operational parameters
        /// </summary>
        Task UpdateWellParametersAsync(string wellUWI, WellParametersDto parameters, string userId);

        #endregion

        #region Equipment Reliability & Maintenance

        /// <summary>
        /// Records equipment maintenance activity
        /// </summary>
        Task RecordEquipmentMaintenanceAsync(EquipmentMaintenanceDto maintenance, string userId);

        /// <summary>
        /// Gets equipment maintenance history
        /// </summary>
        Task<List<EquipmentMaintenanceDto>> GetEquipmentMaintenanceHistoryAsync(string equipmentId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Schedules preventive maintenance
        /// </summary>
        Task ScheduleMaintenanceAsync(MaintenanceScheduleDto schedule, string userId);

        /// <summary>
        /// Gets upcoming maintenance schedules
        /// </summary>
        Task<List<MaintenanceScheduleDto>> GetUpcomingMaintenanceAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calculates equipment reliability metrics
        /// </summary>
        Task<EquipmentReliabilityDto> CalculateEquipmentReliabilityAsync(string equipmentId, DateTime startDate, DateTime endDate);

        #endregion

        #region Facility Operations

        /// <summary>
        /// Records facility production data
        /// </summary>
        Task RecordFacilityProductionAsync(FacilityProductionDto productionData, string userId);

        /// <summary>
        /// Gets facility production data
        /// </summary>
        Task<List<FacilityProductionDto>> GetFacilityProductionAsync(string facilityId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Updates facility operational status
        /// </summary>
        Task UpdateFacilityStatusAsync(string facilityId, FacilityStatusDto status, string userId);

        /// <summary>
        /// Gets facility operational status
        /// </summary>
        Task<FacilityStatusDto> GetFacilityStatusAsync(string facilityId);

        #endregion

        #region Safety & Incident Management

        /// <summary>
        /// Records safety incident
        /// </summary>
        Task RecordSafetyIncidentAsync(SafetyIncidentDto incident, string userId);

        /// <summary>
        /// Gets safety incidents for specified criteria
        /// </summary>
        Task<List<SafetyIncidentDto>> GetSafetyIncidentsAsync(DateTime startDate, DateTime endDate, string? wellUWI = null, string? facilityId = null);

        /// <summary>
        /// Updates safety incident status
        /// </summary>
        Task UpdateSafetyIncidentAsync(string incidentId, SafetyIncidentDto incident, string userId);

        /// <summary>
        /// Calculates safety KPIs
        /// </summary>
        Task<SafetyKPIsDto> CalculateSafetyKPIsAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region Environmental Compliance

        /// <summary>
        /// Records environmental monitoring data
        /// </summary>
        Task RecordEnvironmentalDataAsync(EnvironmentalDataDto data, string userId);

        /// <summary>
        /// Gets environmental monitoring data
        /// </summary>
        Task<List<EnvironmentalDataDto>> GetEnvironmentalDataAsync(DateTime startDate, DateTime endDate, string? locationId = null);

        /// <summary>
        /// Performs environmental compliance check
        /// </summary>
        Task<ComplianceCheckDto> PerformEnvironmentalComplianceCheckAsync(string locationId, DateTime checkDate);

        /// <summary>
        /// Gets environmental compliance status
        /// </summary>
        Task<List<ComplianceStatusDto>> GetEnvironmentalComplianceStatusAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region Cost Analysis & Reporting

        /// <summary>
        /// Records operational costs
        /// </summary>
        Task RecordOperationalCostsAsync(OperationalCostsDto costs, string userId);

        /// <summary>
        /// Gets operational cost data
        /// </summary>
        Task<List<OperationalCostsDto>> GetOperationalCostsAsync(DateTime startDate, DateTime endDate, string? wellUWI = null, string? facilityId = null);

        /// <summary>
        /// Calculates cost per barrel/boe
        /// </summary>
        Task<CostAnalysisDto> CalculateCostAnalysisAsync(string wellUWI, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Generates operations report
        /// </summary>
        Task<OperationsReportDto> GenerateOperationsReportAsync(DateTime startDate, DateTime endDate, string? wellUWI = null, string? facilityId = null);

        #endregion

        #region Production Optimization

        /// <summary>
        /// Identifies production optimization opportunities
        /// </summary>
        Task<List<OptimizationOpportunityDto>> IdentifyOptimizationOpportunitiesAsync(string wellUWI);

        /// <summary>
        /// Implements production optimization recommendation
        /// </summary>
        Task ImplementOptimizationAsync(string opportunityId, string userId);

        /// <summary>
        /// Monitors optimization effectiveness
        /// </summary>
        Task<OptimizationEffectivenessDto> MonitorOptimizationEffectivenessAsync(string opportunityId);

        #endregion

        #region Data Management

        /// <summary>
        /// Gets production operations summary
        /// </summary>
        Task<ProductionOperationsSummaryDto> GetProductionOperationsSummaryAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Exports operations data to specified format
        /// </summary>
        Task<byte[]> ExportOperationsDataAsync(string dataType, DateTime startDate, DateTime endDate, string format = "CSV");

        /// <summary>
        /// Validates operations data integrity
        /// </summary>
        Task<DataValidationResultDto> ValidateOperationsDataAsync(string dataType, DateTime startDate, DateTime endDate);

        #endregion
    }

    #region Production Operations DTOs

    /// <summary>
    /// Well production data DTO
    /// </summary>
    public class WellProductionDataDto
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
    public class WellUptimeDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public int ProducingDays { get; set; }
        public int DownDays { get; set; }
        public decimal UptimePercentage { get; set; }
        public List<DowntimeEventDto> DowntimeEvents { get; set; } = new();
    }

    /// <summary>
    /// Downtime event DTO
    /// </summary>
    public class DowntimeEventDto
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
    public class WellStatusDto
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
        public List<WellIssueDto> CurrentIssues { get; set; } = new();
    }

    /// <summary>
    /// Well issue DTO
    /// </summary>
    public class WellIssueDto
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
    public class WellParametersDto
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
    public class EquipmentMaintenanceDto
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
    public class MaintenanceScheduleDto
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
    public class EquipmentReliabilityDto
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
    public class FacilityProductionDto
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
    public class FacilityStatusDto
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
    public class SafetyIncidentDto
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
    public class SafetyKPIsDto
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
    public class EnvironmentalDataDto
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
    public class ComplianceCheckDto
    {
        public string LocationId { get; set; } = string.Empty;
        public DateTime CheckDate { get; set; }
        public bool IsCompliant { get; set; }
        public List<ComplianceViolationDto> Violations { get; set; } = new();
        public string OverallStatus { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Compliance violation DTO
    /// </summary>
    public class ComplianceViolationDto
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
    public class ComplianceStatusDto
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
    public class OperationalCostsDto
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
    public class CostAnalysisDto
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
    }

    /// <summary>
    /// Operations report DTO
    /// </summary>
    public class OperationsReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ProductionOperationsSummaryDto Summary { get; set; } = new();
        public List<WellPerformanceDto> WellPerformance { get; set; } = new();
        public List<FacilityPerformanceDto> FacilityPerformance { get; set; } = new();
        public SafetyKPIsDto SafetyMetrics { get; set; } = new();
        public List<ComplianceStatusDto> EnvironmentalCompliance { get; set; } = new();
        public CostAnalysisDto CostAnalysis { get; set; } = new();
    }

    /// <summary>
    /// Production operations summary DTO
    /// </summary>
    public class ProductionOperationsSummaryDto
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
    public class WellPerformanceDto
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
    public class FacilityPerformanceDto
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
    public class OptimizationOpportunityDto
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
    public class OptimizationEffectivenessDto
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
    public class DataValidationResultDto
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