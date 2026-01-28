using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    public partial class PlungerLiftDesign : ModelEntityBase {
        private string _designIdValue = "";
        public string DesignId
        {
            get => _designIdValue;
            set => SetProperty(ref _designIdValue, value);
        }

        private string _wellUWIValue = "";
        public string WellUWI
        {
            get => _wellUWIValue;
            set => SetProperty(ref _wellUWIValue, value);
        }

        private DateTime _designDateValue = DateTime.UtcNow;
        public DateTime DesignDate
        {
            get => _designDateValue;
            set => SetProperty(ref _designDateValue, value);
        }

        private int _plungerTypeValue;
        public int PlungerType
        {
            get => _plungerTypeValue;
            set => SetProperty(ref _plungerTypeValue, value);
        }

        private decimal _operatingPressureValue;
        public decimal OperatingPressure
        {
            get => _operatingPressureValue;
            set => SetProperty(ref _operatingPressureValue, value);
        }

        private decimal _minimumPressureValue;
        public decimal MinimumPressure
        {
            get => _minimumPressureValue;
            set => SetProperty(ref _minimumPressureValue, value);
        }

        private decimal _maximumPressureValue;
        public decimal MaximumPressure
        {
            get => _maximumPressureValue;
            set => SetProperty(ref _maximumPressureValue, value);
        }

        private int _cycleTimeValue;
        public int CycleTime
        {
            get => _cycleTimeValue;
            set => SetProperty(ref _cycleTimeValue, value);
        }

        private decimal _tubingSizeValue;
        public decimal TubingSize
        {
            get => _tubingSizeValue;
            set => SetProperty(ref _tubingSizeValue, value);
        }

        private decimal _casingSizeValue;
        public decimal CasingSize
        {
            get => _casingSizeValue;
            set => SetProperty(ref _casingSizeValue, value);
        }

        private string _statusValue = "";
        public string Status
        {
            get => _statusValue;
            set => SetProperty(ref _statusValue, value);
        }

        private string _designNotesValue = "";
        public string DesignNotes
        {
            get => _designNotesValue;
            set => SetProperty(ref _designNotesValue, value);
        }

        // PPDM Audit Fields
        private string _rowCreatedByValue = "";

        private DateTime? _rowCreatedDateValue = DateTime.UtcNow;

        private string _rowChangedByValue = "";

        private DateTime? _rowChangedDateValue = DateTime.UtcNow;

        private DateTime? _rowEffectiveDateValue;

        private DateTime? _rowExpiryDateValue;

        private string _activeIndValue = "Y";

        private string _ppdmGuidValue = "";

        private string _rowQualityValue = "";

    }
}
