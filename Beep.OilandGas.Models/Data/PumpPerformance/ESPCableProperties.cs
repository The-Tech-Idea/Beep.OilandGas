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
        private int CableSizeValue;

        public int CableSize

        {

            get { return this.CableSizeValue; }

            set { SetProperty(ref CableSizeValue, value); }

        }

        /// <summary>
        /// Cable length in feet
        /// </summary>
        private decimal CableLengthValue;

        public decimal CableLength

        {

            get { return this.CableLengthValue; }

            set { SetProperty(ref CableLengthValue, value); }

        }

        /// <summary>
        /// Cable resistance in ohms per 1000 feet
        /// </summary>
        private decimal ResistancePer1000FeetValue;

        public decimal ResistancePer1000Feet

        {

            get { return this.ResistancePer1000FeetValue; }

            set { SetProperty(ref ResistancePer1000FeetValue, value); }

        }

        /// <summary>
        /// Voltage drop in volts
        /// </summary>
        private decimal VoltageDropValue;

        public decimal VoltageDrop

        {

            get { return this.VoltageDropValue; }

            set { SetProperty(ref VoltageDropValue, value); }

        }
    }
}




