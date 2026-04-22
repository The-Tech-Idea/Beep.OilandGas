using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.Web.Services;

public interface IProductionServiceClient
{
    Task<ProductionDashboardSummary?> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<List<ProductionWellStatusDto>> GetDashboardWellsAsync(CancellationToken cancellationToken = default);
    Task<ProductionWellPerformanceDto?> GetWellPerformanceAsync(string wellId, CancellationToken cancellationToken = default);
    Task<WellPerformanceAnalysisResponse?> GetWellPerformanceAnalysisAsync(string wellId, CancellationToken cancellationToken = default);
    Task<PerformanceDeviationResult> LogPerformanceDeviationAsync(string wellId, PerformanceDeviationRequest request, CancellationToken cancellationToken = default);
    Task<List<ProductionInterventionCandidateDto>> GetInterventionCandidatesAsync(CancellationToken cancellationToken = default);
    Task<ProductionInterventionDecisionResult> RecordInterventionDecisionAsync(string wellId, ProductionInterventionDecisionRequest request, CancellationToken cancellationToken = default);
    Task<ProductionDecommissioningTriggerResult> TransitionToDecommissioningAsync(string wellId, ProductionDecommissioningTriggerRequest request, CancellationToken cancellationToken = default);
    Task<List<WellTestResponse>> GetWellTestsAsync(string wellId, CancellationToken cancellationToken = default);
    Task<bool> PatchAllocationAsync(string period, string wellId, ProductionAllocationPatchRequest request, CancellationToken cancellationToken = default);
    Task<List<FIELD>> GetFieldsAsync(CancellationToken cancellationToken = default);
    Task<List<POOL>> GetPoolsAsync(CancellationToken cancellationToken = default);
    Task<List<RESERVE_ENTITY>> GetReservesAsync(CancellationToken cancellationToken = default);
    Task<List<PDEN_VOL_SUMMARY>> GetReportingAsync(CancellationToken cancellationToken = default);
    Task<List<ProductionForecastResponse>> GetForecastsAsync(CancellationToken cancellationToken = default);
}

public sealed class ProductionWellPerformanceDto
{
    public string WellId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double OilRate { get; set; }
    public double GasRate { get; set; }
    public double WaterRate { get; set; }
    public double PotentialRate { get; set; }
    public double CumOil { get; set; }
    public DateTime? LastTestDate { get; set; }
    public List<WellTestResponse> WellTests { get; set; } = new();
}

public sealed class ProductionInterventionCandidateDto
{
    public string WellId { get; set; } = string.Empty;
    public string WellName { get; set; } = string.Empty;
    public string InterventionType { get; set; } = string.Empty;
    public string Problem { get; set; } = string.Empty;
    public double DeferredBopd { get; set; }
    public int EstDaysSinceOnset { get; set; }
    public string Priority { get; set; } = "MEDIUM";
    public string Status { get; set; } = "REVIEW";
    public double EstCostUsd { get; set; }
    public string? WorkOrderId { get; set; }
    public string? AfeId { get; set; }
    public string? AfeNumber { get; set; }
    public string? AbandonmentId { get; set; }
    public string? ProcessInstanceId { get; set; }
}

public sealed class ProductionInterventionDecisionRequest
{
    public string? Decision { get; set; }
    public string? Note { get; set; }
    public string? InterventionType { get; set; }
    public string? Problem { get; set; }
    public double? EstimatedCostUsd { get; set; }
    public bool CreateWorkOrder { get; set; } = true;
    public bool LinkAfe { get; set; } = true;
    public string? WorkOrderId { get; set; }
    public string? AfeId { get; set; }
}

public sealed class ProductionInterventionDecisionResult
{
    public bool Success { get; set; }
    public string WellId { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public string? WorkOrderId { get; set; }
    public string? AfeId { get; set; }
    public string? AfeNumber { get; set; }
    public string? Message { get; set; }
}

public sealed class ProductionDecommissioningTriggerRequest
{
    public string? Note { get; set; }
    public string? InterventionType { get; set; }
    public string? Problem { get; set; }
    public double? EstimatedCostUsd { get; set; }
    public string? AbandonmentType { get; set; }
    public string? AbandonmentMethod { get; set; }
    public bool StartWorkflow { get; set; } = true;
}

public sealed class ProductionDecommissioningTriggerResult
{
    public bool Success { get; set; }
    public bool AlreadyExists { get; set; }
    public bool WorkflowStarted { get; set; }
    public string WellId { get; set; } = string.Empty;
    public string? AbandonmentId { get; set; }
    public string? ProcessInstanceId { get; set; }
    public string? Message { get; set; }
}

public sealed class ProductionAllocationPatchRequest
{
    public double AllocatedOilBopd { get; set; }
    public double AllocatedGasMmscfd { get; set; }
    public double AllocatedWaterBopd { get; set; }
}