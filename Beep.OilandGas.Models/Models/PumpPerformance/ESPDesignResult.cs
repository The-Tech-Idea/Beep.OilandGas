using System.Collections.Generic;

namespace Beep.OilandGas.Models.PumpPerformance
{
    /// <summary>
    /// Represents ESP design results
    /// DTO for calculations - Entity class: ESP_DESIGN_RESULT
    /// </summary>
    public class ESPDesignResult
    {
        /// <summary>
        /// Selected pump stage count
        /// </summary>
        public int PumpStages { get; set; }

        /// <summary>
        /// Required pump horsepower
        /// </summary>
        public decimal RequiredHorsepower { get; set; }

        /// <summary>
        /// Motor horsepower
        /// </summary>
        public decimal MotorHorsepower { get; set; }

        /// <summary>
        /// Motor voltage
        /// </summary>
        public decimal MotorVoltage { get; set; }

        /// <summary>
        /// Motor current in amps
        /// </summary>
        public decimal MotorCurrent { get; set; }

        /// <summary>
        /// Cable size in AWG
        /// </summary>
        public int CableSize { get; set; }

        /// <summary>
        /// Cable length in feet
        /// </summary>
        public decimal CableLength { get; set; }

        /// <summary>
        /// Total system efficiency
        /// </summary>
        public decimal SystemEfficiency { get; set; }

        /// <summary>
        /// Pump efficiency
        /// </summary>
        public decimal PumpEfficiency { get; set; }

        /// <summary>
        /// Motor efficiency
        /// </summary>
        public decimal MotorEfficiency { get; set; }

        /// <summary>
        /// Power consumption in kW
        /// </summary>
        public decimal PowerConsumption { get; set; }

        /// <summary>
        /// Operating flow rate in bbl/day
        /// </summary>
        public decimal OperatingFlowRate { get; set; }

        /// <summary>
        /// Operating head in feet
        /// </summary>
        public decimal OperatingHead { get; set; }

        /// <summary>
        /// Pump performance points
        /// </summary>
        public List<ESPPumpPoint> PerformancePoints { get; set; } = new();
    }
}
