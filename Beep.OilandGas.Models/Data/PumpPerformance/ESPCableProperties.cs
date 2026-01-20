namespace Beep.OilandGas.Models.Data.PumpPerformance
{
    /// <summary>
    /// Represents ESP cable properties
    /// DTO for calculations - Entity class: ESP_CABLE_PROPERTIES
    /// </summary>
    public class ESPCableProperties : ModelEntityBase
    {
        /// <summary>
        /// Cable size in AWG
        /// </summary>
        public int CableSize { get; set; }

        /// <summary>
        /// Cable length in feet
        /// </summary>
        public decimal CableLength { get; set; }

        /// <summary>
        /// Cable resistance in ohms per 1000 feet
        /// </summary>
        public decimal ResistancePer1000Feet { get; set; }

        /// <summary>
        /// Voltage drop in volts
        /// </summary>
        public decimal VoltageDrop { get; set; }
    }
}



