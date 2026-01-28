using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public partial class DrawdownAnalysisResult : ModelEntityBase {
        /// <summary>
        /// Analysis method used (e.g., "Constant Rate Drawdown", "Type Curve Matching")
        /// </summary>
        private string _analysisMethodValue;
        public string AnalysisMethod
        {
            get { return _analysisMethodValue; }
            set { SetProperty(ref _analysisMethodValue, value); }
        }

        /// <summary>
        /// Calculated permeability (millidarcies)
        /// </summary>
        private double? _permeabilityValue;
        public double? Permeability
        {
            get { return _permeabilityValue; }
            set { SetProperty(ref _permeabilityValue, value); }
        }

        /// <summary>
        /// Skin factor (dimensionless)
        /// </summary>
        private double? _skinFactorValue;
        public double? SkinFactor
        {
            get { return _skinFactorValue; }
            set { SetProperty(ref _skinFactorValue, value); }
        }

        /// <summary>
        /// Initial reservoir pressure (psia)
        /// </summary>
        private double? _initialReservoirPressureValue;
        public double? InitialReservoirPressure
        {
            get { return _initialReservoirPressureValue; }
            set { SetProperty(ref _initialReservoirPressureValue, value); }
        }

        /// <summary>
        /// Productivity index (STB/day/psi)
        /// </summary>
        private double? _productivityIndexValue;
        public double? ProductivityIndex
        {
            get { return _productivityIndexValue; }
            set { SetProperty(ref _productivityIndexValue, value); }
        }

        /// <summary>
        /// Flow efficiency (fraction, 0-1)
        /// </summary>
        private double? _flowEfficiencyValue;
        public double? FlowEfficiency
        {
            get { return _flowEfficiencyValue; }
            set { SetProperty(ref _flowEfficiencyValue, value); }
        }

        /// <summary>
        /// Damage ratio (dimensionless)
        /// </summary>
        private double? _damageRatioValue;
        public double? DamageRatio
        {
            get { return _damageRatioValue; }
            set { SetProperty(ref _damageRatioValue, value); }
        }

        /// <summary>
        /// Radius of investigation (feet)
        /// </summary>
        private double? _radiusOfInvestigationValue;
        public double? RadiusOfInvestigation
        {
            get { return _radiusOfInvestigationValue; }
            set { SetProperty(ref _radiusOfInvestigationValue, value); }
        }

        /// <summary>
        /// Identified flow regime type
        /// </summary>
        private string _flowRegimeValue;
        public string FlowRegime
        {
            get { return _flowRegimeValue; }
            set { SetProperty(ref _flowRegimeValue, value); }
        }

        /// <summary>
        /// R-squared goodness of fit (0-1)
        /// </summary>
        private double? _rSquaredValue;
        public double? RSquared
        {
            get { return _rSquaredValue; }
            set { SetProperty(ref _rSquaredValue, value); }
        }

        /// <summary>
        /// Analysis quality rating (Excellent/Good/Fair/Poor)
        /// </summary>
        private string _qualityRatingValue;
        public string QualityRating
        {
            get { return _qualityRatingValue; }
            set { SetProperty(ref _qualityRatingValue, value); }
        }

       
        /// <summary>
        /// Default constructor
        /// </summary>
        public DrawdownAnalysisResult()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
