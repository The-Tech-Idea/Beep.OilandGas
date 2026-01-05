using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Drilling;

namespace Beep.OilandGas.Client.App.Services.Drilling
{
    /// <summary>
    /// Service interface for Drilling operations
    /// Includes Operations and Enhanced Recovery
    /// </summary>
    public interface IDrillingService
    {
        #region Operations

        Task<DRILLING_OPERATION> CreateDrillingProgramAsync(DRILLING_OPERATION request, CancellationToken cancellationToken = default);
        Task<DRILLING_OPERATION> GetDrillingStatusAsync(string wellId, CancellationToken cancellationToken = default);
        Task<WELL_CONSTRUCTION> GetBHADesignAsync(string wellId, CancellationToken cancellationToken = default);
        Task<DRILLING_REPORT> GetMudProgramAsync(string wellId, CancellationToken cancellationToken = default);
        Task<DRILLING_OPERATION> UpdateDrillingProgressAsync(string wellId, DRILLING_REPORT progress, CancellationToken cancellationToken = default);
        Task<List<DRILLING_REPORT>> GetDrillingHistoryAsync(string wellId, CancellationToken cancellationToken = default);
        Task<List<CASING_STRING>> GetCasingDesignAsync(string wellId, CancellationToken cancellationToken = default);
        Task<List<COMPLETION_STRING>> GetCementingProgramAsync(string wellId, CancellationToken cancellationToken = default);

        #endregion

        #region Enhanced Recovery

        Task<EORAnalysisResult> AnalyzeEORAsync(EORAnalysisRequest request, CancellationToken cancellationToken = default);
        Task<InjectionPlan> GetInjectionPlanAsync(string fieldId, CancellationToken cancellationToken = default);
        Task<RecoveryFactor> GetRecoveryFactorAsync(string reservoirId, CancellationToken cancellationToken = default);
        Task<WaterfloodDesign> CreateWaterfloodDesignAsync(WaterfloodDesign request, CancellationToken cancellationToken = default);
        Task<GasInjectionDesign> CreateGasInjectionDesignAsync(GasInjectionDesign request, CancellationToken cancellationToken = default);
        Task<EORPerformance> GetEORPerformanceAsync(string projectId, CancellationToken cancellationToken = default);
        Task<InjectionPattern> OptimizeInjectionPatternAsync(InjectionPattern request, CancellationToken cancellationToken = default);

        #endregion
    }

    #region Enhanced Recovery Entities

    /// <summary>
    /// EOR Analysis Request entity
    /// </summary>
    public class EORAnalysisRequest
    {
        public string ReservoirId { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string EORType { get; set; } = string.Empty;
        public decimal? CurrentRecoveryFactor { get; set; }
        public decimal? TargetRecoveryFactor { get; set; }
        public decimal? ReservoirPressure { get; set; }
        public decimal? ReservoirTemperature { get; set; }
    }

    /// <summary>
    /// EOR Analysis Result entity
    /// </summary>
    public class EORAnalysisResult
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string ReservoirId { get; set; } = string.Empty;
        public string RecommendedEORType { get; set; } = string.Empty;
        public decimal? ExpectedRecoveryFactor { get; set; }
        public decimal? IncrementalOil { get; set; }
        public decimal? NPV { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// Injection Plan entity
    /// </summary>
    public class InjectionPlan
    {
        public string PlanId { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string InjectionType { get; set; } = string.Empty;
        public decimal? InjectionRate { get; set; }
        public decimal? InjectionPressure { get; set; }
        public System.DateTime? StartDate { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// Recovery Factor entity
    /// </summary>
    public class RecoveryFactor
    {
        public string ReservoirId { get; set; } = string.Empty;
        public decimal? PrimaryRecovery { get; set; }
        public decimal? SecondaryRecovery { get; set; }
        public decimal? TertiaryRecovery { get; set; }
        public decimal? TotalRecovery { get; set; }
        public decimal? OOIP { get; set; }
        public decimal? CumulativeProduction { get; set; }
    }

    /// <summary>
    /// Waterflood Design entity
    /// </summary>
    public class WaterfloodDesign
    {
        public string DesignId { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public int? NumberOfInjectors { get; set; }
        public int? NumberOfProducers { get; set; }
        public decimal? InjectionRate { get; set; }
        public decimal? ExpectedRecovery { get; set; }
    }

    /// <summary>
    /// Gas Injection Design entity
    /// </summary>
    public class GasInjectionDesign
    {
        public string DesignId { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string GasType { get; set; } = string.Empty;
        public decimal? InjectionRate { get; set; }
        public decimal? InjectionPressure { get; set; }
        public decimal? ExpectedRecovery { get; set; }
        public bool? IsMiscible { get; set; }
    }

    /// <summary>
    /// EOR Performance entity
    /// </summary>
    public class EORPerformance
    {
        public string ProjectId { get; set; } = string.Empty;
        public decimal? CumulativeInjection { get; set; }
        public decimal? IncrementalOil { get; set; }
        public decimal? CurrentRecoveryFactor { get; set; }
        public decimal? InjectionEfficiency { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// Injection Pattern entity
    /// </summary>
    public class InjectionPattern
    {
        public string PatternId { get; set; } = string.Empty;
        public string PatternType { get; set; } = string.Empty;
        public decimal? WellSpacing { get; set; }
        public decimal? InjectorProducerRatio { get; set; }
        public decimal? OptimalRate { get; set; }
        public decimal? SweepEfficiency { get; set; }
    }

    #endregion
}
