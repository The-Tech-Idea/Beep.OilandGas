using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.PlungerLift
{
    /// <summary>
    /// Plunger lift design DTO - inherits from Entity for PPDM compliance
    /// </summary>
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

    /// <summary>
    /// Plunger lift performance DTO - inherits from Entity for PPDM compliance
    /// </summary>
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

    /// <summary>
    /// Plunger lift optimization result DTO - inherits from Entity for PPDM compliance
    /// </summary>
    public partial class PlungerLiftOptimizationResult : ModelEntityBase {
        private string _optimizationIdValue = "";
        public string OptimizationId
        {
            get => _optimizationIdValue;
            set => SetProperty(ref _optimizationIdValue, value);
        }

        private string _wellUWIValue = "";
        public string WellUWI
        {
            get => _wellUWIValue;
            set => SetProperty(ref _wellUWIValue, value);
        }

        private DateTime _optimizationDateValue = DateTime.UtcNow;
        public DateTime OptimizationDate
        {
            get => _optimizationDateValue;
            set => SetProperty(ref _optimizationDateValue, value);
        }

        private decimal _expectedProductionIncreaseValue;
        public decimal ExpectedProductionIncrease
        {
            get => _expectedProductionIncreaseValue;
            set => SetProperty(ref _expectedProductionIncreaseValue, value);
        }

        private decimal _estimatedCostValue;
        public decimal EstimatedCost
        {
            get => _estimatedCostValue;
            set => SetProperty(ref _estimatedCostValue, value);
        }

        private string _recommendationsValue = "";
        public string Recommendations
        {
            get => _recommendationsValue;
            set => SetProperty(ref _recommendationsValue, value);
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


