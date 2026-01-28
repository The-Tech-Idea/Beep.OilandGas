using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
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
