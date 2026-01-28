using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public partial class TypeCurveMatch : ModelEntityBase {
        /// <summary>
        /// Type curve name/identifier (e.g., "Infinite Acting Homogeneous", "Bounded Reservoir")
        /// </summary>
        private string _typeCurveNameValue;
        public string TypeCurveName
        {
            get { return _typeCurveNameValue; }
            set { SetProperty(ref _typeCurveNameValue, value); }
        }

        /// <summary>
        /// Reservoir model type
        /// </summary>
        private string _reservoirModelValue;
        public string ReservoirModel
        {
            get { return _reservoirModelValue; }
            set { SetProperty(ref _reservoirModelValue, value); }
        }

        /// <summary>
        /// Dimensionless time parameter (match point)
        /// </summary>
        private double? _dimensionlessTimeValue;
        public double? DimensionlessTime
        {
            get { return _dimensionlessTimeValue; }
            set { SetProperty(ref _dimensionlessTimeValue, value); }
        }

        /// <summary>
        /// Dimensionless pressure parameter (match point)
        /// </summary>
        private double? _dimensionlessPressureValue;
        public double? DimensionlessPressure
        {
            get { return _dimensionlessPressureValue; }
            set { SetProperty(ref _dimensionlessPressureValue, value); }
        }

        /// <summary>
        /// Calculated permeability from match (millidarcies)
        /// </summary>
        private double? _permeabilityValue;
        public double? Permeability
        {
            get { return _permeabilityValue; }
            set { SetProperty(ref _permeabilityValue, value); }
        }

        /// <summary>
        /// Calculated skin factor from match (dimensionless)
        /// </summary>
        private double? _skinFactorValue;
        public double? SkinFactor
        {
            get { return _skinFactorValue; }
            set { SetProperty(ref _skinFactorValue, value); }
        }

        /// <summary>
        /// Storage ratio parameter (if applicable)
        /// </summary>
        private double? _storageRatioValue;
        public double? StorageRatio
        {
            get { return _storageRatioValue; }
            set { SetProperty(ref _storageRatioValue, value); }
        }

        /// <summary>
        /// Interporosity flow parameter - lambda (if applicable, dual porosity systems)
        /// </summary>
        private double? _lambdaParameterValue;
        public double? LambdaParameter
        {
            get { return _lambdaParameterValue; }
            set { SetProperty(ref _lambdaParameterValue, value); }
        }

        /// <summary>
        /// Omega parameter - fracture capacity ratio (if applicable, dual porosity systems)
        /// </summary>
        private double? _omegaParameterValue;
        public double? OmegaParameter
        {
            get { return _omegaParameterValue; }
            set { SetProperty(ref _omegaParameterValue, value); }
        }

        /// <summary>
        /// Match quality metric (RMS error or similar, 0-1 scale where 1 is perfect)
        /// </summary>
        private double? _matchQualityValue;
        public double? MatchQuality
        {
            get { return _matchQualityValue; }
            set { SetProperty(ref _matchQualityValue, value); }
        }

        /// <summary>
        /// Confidence level for the match (Low/Medium/High)
        /// </summary>
        private string _confidenceLevelValue;
        public string ConfidenceLevel
        {
            get { return _confidenceLevelValue; }
            set { SetProperty(ref _confidenceLevelValue, value); }
        }

        /// <summary>
        /// Notes or observations about the match
        /// </summary>
        private string _notesValue;
        public string Notes
        {
            get { return _notesValue; }
            set { SetProperty(ref _notesValue, value); }
        }

        
        /// <summary>
        /// Default constructor
        /// </summary>
        public TypeCurveMatch()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
