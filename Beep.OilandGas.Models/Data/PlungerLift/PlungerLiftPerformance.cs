using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    public partial class PlungerLiftPerformance : ModelEntityBase {
        private string _performanceIdValue = "";
        public string PerformanceId
        {
            get => _performanceIdValue;
            set => SetProperty(ref _performanceIdValue, value);
        }

        private string _wellUWIValue = "";
        public string WellUWI
        {
            get => _wellUWIValue;
            set => SetProperty(ref _wellUWIValue, value);
        }

        private DateTime _performanceDateValue = DateTime.UtcNow;
        public DateTime PerformanceDate
        {
            get => _performanceDateValue;
            set => SetProperty(ref _performanceDateValue, value);
        }

        private decimal _productionRateValue;
        public decimal ProductionRate
        {
            get => _productionRateValue;
            set => SetProperty(ref _productionRateValue, value);
        }

        private int _cycleTimeValue;
        public int CycleTime
        {
            get => _cycleTimeValue;
            set => SetProperty(ref _cycleTimeValue, value);
        }

        private decimal _efficiencyValue;
        public decimal Efficiency
        {
            get => _efficiencyValue;
            set => SetProperty(ref _efficiencyValue, value);
        }

        private decimal _averagePressureValue;
        public decimal AveragePressure
        {
            get => _averagePressureValue;
            set => SetProperty(ref _averagePressureValue, value);
        }

        private int _operatingHoursValue;
        public int OperatingHours
        {
            get => _operatingHoursValue;
            set => SetProperty(ref _operatingHoursValue, value);
        }

        private int _downtimeHoursValue;
        public int DowntimeHours
        {
            get => _downtimeHoursValue;
            set => SetProperty(ref _downtimeHoursValue, value);
        }

        private string _statusValue = "";
        public string Status
        {
            get => _statusValue;
            set => SetProperty(ref _statusValue, value);
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
