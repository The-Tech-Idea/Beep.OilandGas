using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Typed client for enhanced recovery domain (EOR screening, injection ops, pilot economics).
/// </summary>
public interface IEnhancedRecoveryClient
{
    Task<List<EnhancedRecoveryOperationDto>> GetOperationsAsync(string? fieldId = null, CancellationToken cancellationToken = default);
    Task<EnhancedRecoveryOperationDto?> GetOperationAsync(string operationId, CancellationToken cancellationToken = default);
    Task<List<InjectionOperationDto>> GetInjectionOperationsAsync(string? wellUwi = null, CancellationToken cancellationToken = default);
    Task<List<WaterFloodingDto>> GetWaterFloodingAsync(string? fieldId = null, CancellationToken cancellationToken = default);
    Task<List<GasInjectionDto>> GetGasInjectionAsync(string? fieldId = null, CancellationToken cancellationToken = default);
    Task<EorEconomicAnalysisDto> AnalyzeEconomicsAsync(EorEconomicRequest request, CancellationToken cancellationToken = default);
}

public class EnhancedRecoveryOperationDto
{
    public string OperationId { get; set; } = string.Empty;
    public string FieldId { get; set; } = string.Empty;
    public string RecoveryMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal EstimatedIncrementalOil { get; set; }
    public decimal ActualIncrementalOil { get; set; }
}

public class InjectionOperationDto
{
    public string OperationId { get; set; } = string.Empty;
    public string WellUwi { get; set; } = string.Empty;
    public string InjectionType { get; set; } = string.Empty;
    public decimal InjectionRate { get; set; }
    public decimal InjectionPressure { get; set; }
    public DateTime StartDate { get; set; }
}

public class WaterFloodingDto
{
    public string FieldId { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public decimal InjectionRate { get; set; }
    public decimal WaterCut { get; set; }
    public DateTime StartDate { get; set; }
}

public class GasInjectionDto
{
    public string FieldId { get; set; } = string.Empty;
    public string GasType { get; set; } = string.Empty;
    public decimal InjectionRate { get; set; }
    public decimal InjectionPressure { get; set; }
    public DateTime StartDate { get; set; }
}

public class EorEconomicAnalysisDto
{
    public decimal NPV { get; set; }
    public decimal IRR { get; set; }
    public decimal PaybackPeriod { get; set; }
    public decimal ProfitabilityIndex { get; set; }
    public bool IsViable { get; set; }
}

public class EorEconomicRequest
{
    public string FieldId { get; set; } = string.Empty;
    public double EstimatedIncrementalOil { get; set; }
    public double OilPrice { get; set; }
    public double CapitalCost { get; set; }
    public double OperatingCostPerBarrel { get; set; }
    public int ProjectLifeYears { get; set; }
    public double DiscountRate { get; set; } = 0.10;
}
