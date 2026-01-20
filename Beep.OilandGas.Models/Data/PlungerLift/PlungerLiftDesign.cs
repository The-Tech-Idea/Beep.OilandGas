using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

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
        public string ROW_CREATED_BY
        {
            get => _rowCreatedByValue;
            set => SetProperty(ref _rowCreatedByValue, value);
        }

        private DateTime? _rowCreatedDateValue = DateTime.UtcNow;
        public DateTime? ROW_CREATED_DATE
        {
            get => _rowCreatedDateValue;
            set => SetProperty(ref _rowCreatedDateValue, value);
        }

        private string _rowChangedByValue = "";
        public string ROW_CHANGED_BY
        {
            get => _rowChangedByValue;
            set => SetProperty(ref _rowChangedByValue, value);
        }

        private DateTime? _rowChangedDateValue = DateTime.UtcNow;
        public DateTime? ROW_CHANGED_DATE
        {
            get => _rowChangedDateValue;
            set => SetProperty(ref _rowChangedDateValue, value);
        }

        private DateTime? _rowEffectiveDateValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get => _rowEffectiveDateValue;
            set => SetProperty(ref _rowEffectiveDateValue, value);
        }

        private DateTime? _rowExpiryDateValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get => _rowExpiryDateValue;
            set => SetProperty(ref _rowExpiryDateValue, value);
        }

        private string _activeIndValue = "Y";
        public string ACTIVE_IND
        {
            get => _activeIndValue;
            set => SetProperty(ref _activeIndValue, value);
        }

        private string _ppdmGuidValue = "";
        public string PPDM_GUID
        {
            get => _ppdmGuidValue;
            set => SetProperty(ref _ppdmGuidValue, value);
        }

        private string _rowQualityValue = "";
        public string ROW_QUALITY
        {
            get => _rowQualityValue;
            set => SetProperty(ref _rowQualityValue, value);
        }
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
        public string ROW_CREATED_BY
        {
            get => _rowCreatedByValue;
            set => SetProperty(ref _rowCreatedByValue, value);
        }

        private DateTime? _rowCreatedDateValue = DateTime.UtcNow;
        public DateTime? ROW_CREATED_DATE
        {
            get => _rowCreatedDateValue;
            set => SetProperty(ref _rowCreatedDateValue, value);
        }

        private string _rowChangedByValue = "";
        public string ROW_CHANGED_BY
        {
            get => _rowChangedByValue;
            set => SetProperty(ref _rowChangedByValue, value);
        }

        private DateTime? _rowChangedDateValue = DateTime.UtcNow;
        public DateTime? ROW_CHANGED_DATE
        {
            get => _rowChangedDateValue;
            set => SetProperty(ref _rowChangedDateValue, value);
        }

        private DateTime? _rowEffectiveDateValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get => _rowEffectiveDateValue;
            set => SetProperty(ref _rowEffectiveDateValue, value);
        }

        private DateTime? _rowExpiryDateValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get => _rowExpiryDateValue;
            set => SetProperty(ref _rowExpiryDateValue, value);
        }

        private string _activeIndValue = "Y";
        public string ACTIVE_IND
        {
            get => _activeIndValue;
            set => SetProperty(ref _activeIndValue, value);
        }

        private string _ppdmGuidValue = "";
        public string PPDM_GUID
        {
            get => _ppdmGuidValue;
            set => SetProperty(ref _ppdmGuidValue, value);
        }

        private string _rowQualityValue = "";
        public string ROW_QUALITY
        {
            get => _rowQualityValue;
            set => SetProperty(ref _rowQualityValue, value);
        }
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
        public string ROW_CREATED_BY
        {
            get => _rowCreatedByValue;
            set => SetProperty(ref _rowCreatedByValue, value);
        }

        private DateTime? _rowCreatedDateValue = DateTime.UtcNow;
        public DateTime? ROW_CREATED_DATE
        {
            get => _rowCreatedDateValue;
            set => SetProperty(ref _rowCreatedDateValue, value);
        }

        private string _rowChangedByValue = "";
        public string ROW_CHANGED_BY
        {
            get => _rowChangedByValue;
            set => SetProperty(ref _rowChangedByValue, value);
        }

        private DateTime? _rowChangedDateValue = DateTime.UtcNow;
        public DateTime? ROW_CHANGED_DATE
        {
            get => _rowChangedDateValue;
            set => SetProperty(ref _rowChangedDateValue, value);
        }

        private DateTime? _rowEffectiveDateValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get => _rowEffectiveDateValue;
            set => SetProperty(ref _rowEffectiveDateValue, value);
        }

        private DateTime? _rowExpiryDateValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get => _rowExpiryDateValue;
            set => SetProperty(ref _rowExpiryDateValue, value);
        }

        private string _activeIndValue = "Y";
        public string ACTIVE_IND
        {
            get => _activeIndValue;
            set => SetProperty(ref _activeIndValue, value);
        }

        private string _ppdmGuidValue = "";
        public string PPDM_GUID
        {
            get => _ppdmGuidValue;
            set => SetProperty(ref _ppdmGuidValue, value);
        }

        private string _rowQualityValue = "";
        public string ROW_QUALITY
        {
            get => _rowQualityValue;
            set => SetProperty(ref _rowQualityValue, value);
        }
    }
}
