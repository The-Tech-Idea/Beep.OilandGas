using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for plunger lift operations.
    /// Provides plunger lift design, optimization, and performance monitoring.
    /// </summary>
    public interface IPlungerLiftService
    {
        /// <summary>
        /// Designs a plunger lift system for a well.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <param name="wellProperties">Well properties</param>
        /// <returns>Plunger lift design result</returns>
        Task<PlungerLiftDesignDto> DesignPlungerLiftSystemAsync(string wellUWI, PlungerLiftWellPropertiesDto wellProperties);

        /// <summary>
        /// Analyzes plunger lift performance.
        /// </summary>
        /// <param name="wellUWI">Well UWI</param>
        /// <returns>Performance analysis result</returns>
        Task<PlungerLiftPerformanceDto> AnalyzePerformanceAsync(string wellUWI);

        /// <summary>
        /// Saves plunger lift design to database.
        /// </summary>
        /// <param name="design">Plunger lift design</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SavePlungerLiftDesignAsync(PlungerLiftDesignDto design, string userId);
    }

    /// <summary>
    /// DTO for plunger lift design.
    /// </summary>
    public class PlungerLiftDesignDto
    {
        public string DesignId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public System.DateTime DesignDate { get; set; }
        public decimal PlungerType { get; set; }
        public decimal OperatingPressure { get; set; }
        public decimal CycleTime { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for plunger lift well properties.
    /// </summary>
    public class PlungerLiftWellPropertiesDto
    {
        public decimal WellDepth { get; set; }
        public decimal TubingDiameter { get; set; }
        public decimal WellheadPressure { get; set; }
        public decimal BottomHolePressure { get; set; }
        public decimal GasOilRatio { get; set; }
    }

    /// <summary>
    /// DTO for plunger lift performance.
    /// </summary>
    public class PlungerLiftPerformanceDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public System.DateTime PerformanceDate { get; set; }
        public decimal ProductionRate { get; set; }
        public decimal CycleTime { get; set; }
        public decimal Efficiency { get; set; }
        public string? Status { get; set; }
    }
}




