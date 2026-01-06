using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for sucker rod pumping operations.
    /// Provides pump design, performance analysis, and optimization.
    /// </summary>
    public interface ISuckerRodPumpingService
    {
        /// <summary>
        /// Designs a sucker rod pumping system.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="wellProperties">Well properties</param>
        /// <returns>Sucker rod pump design result</returns>
        Task<SuckerRodPumpDesignDto> DesignPumpSystemAsync(string wellUWI, SuckerRodPumpWellPropertiesDto wellProperties);

        /// <summary>
        /// Analyzes sucker rod pump performance.
        /// </summary>
        /// <param name="pumpId">Pump identifier</param>
        /// <returns>Performance analysis result</returns>
        Task<SuckerRodPumpPerformanceDto> AnalyzePerformanceAsync(string pumpId);

        /// <summary>
        /// Saves sucker rod pump design to database.
        /// </summary>
        /// <param name="design">Pump design</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SavePumpDesignAsync(SuckerRodPumpDesignDto design, string userId);
    }

    /// <summary>
    /// DTO for sucker rod pump design.
    /// </summary>
    public class SuckerRodPumpDesignDto
    {
        public string DesignId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public System.DateTime DesignDate { get; set; }
        public decimal PumpDepth { get; set; }
        public decimal PumpSize { get; set; }
        public decimal StrokeLength { get; set; }
        public decimal StrokesPerMinute { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for sucker rod pump well properties.
    /// </summary>
    public class SuckerRodPumpWellPropertiesDto
    {
        public decimal WellDepth { get; set; }
        public decimal TubingDiameter { get; set; }
        public decimal WellheadPressure { get; set; }
        public decimal BottomHolePressure { get; set; }
        public decimal DesiredFlowRate { get; set; }
    }

    /// <summary>
    /// DTO for sucker rod pump performance.
    /// </summary>
    public class SuckerRodPumpPerformanceDto
    {
        public string PumpId { get; set; } = string.Empty;
        public System.DateTime PerformanceDate { get; set; }
        public decimal FlowRate { get; set; }
        public decimal Efficiency { get; set; }
        public decimal PowerConsumption { get; set; }
        public string? Status { get; set; }
    }
}




