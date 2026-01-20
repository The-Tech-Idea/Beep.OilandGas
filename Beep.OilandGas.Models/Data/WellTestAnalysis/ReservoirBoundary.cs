using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    /// <summary>
    /// Represents a detected boundary in the reservoir from well test analysis.
    /// Includes boundary type, location, and confidence metrics.
    /// DTO for calculations - Entity class: RESERVOIR_BOUNDARY
    /// </summary>
    public partial class ReservoirBoundary : ModelEntityBase {
        /// <summary>
        /// Boundary type (e.g., "No Flow Boundary", "Constant Pressure Boundary", "Fault")
        /// </summary>
        private string _boundaryTypeValue;
        public string BoundaryType
        {
            get { return _boundaryTypeValue; }
            set { SetProperty(ref _boundaryTypeValue, value); }
        }

        /// <summary>
        /// Distance from well to boundary (feet)
        /// </summary>
        private double? _distanceValue;
        public double? Distance
        {
            get { return _distanceValue; }
            set { SetProperty(ref _distanceValue, value); }
        }

        /// <summary>
        /// Boundary angle/orientation (degrees, if applicable)
        /// </summary>
        private double? _angleValue;
        public double? Angle
        {
            get { return _angleValue; }
            set { SetProperty(ref _angleValue, value); }
        }

        /// <summary>
        /// Time when boundary effect first appears (hours)
        /// </summary>
        private double? _boundaryEffectTimeValue;
        public double? BoundaryEffectTime
        {
            get { return _boundaryEffectTimeValue; }
            set { SetProperty(ref _boundaryEffectTimeValue, value); }
        }

        /// <summary>
        /// Pressure derivative signature indicating boundary (slope change indicator)
        /// </summary>
        private double? _signatureStrengthValue;
        public double? SignatureStrength
        {
            get { return _signatureStrengthValue; }
            set { SetProperty(ref _signatureStrengthValue, value); }
        }

        /// <summary>
        /// Confidence level in boundary detection (0-1, where 1 is highest confidence)
        /// </summary>
        private double? _confidenceLevelValue;
        public double? ConfidenceLevel
        {
            get { return _confidenceLevelValue; }
            set { SetProperty(ref _confidenceLevelValue, value); }
        }

        /// <summary>
        /// Detection method used (e.g., "Derivative Peak Detection", "Type Curve Matching")
        /// </summary>
        private string _detectionMethodValue;
        public string DetectionMethod
        {
            get { return _detectionMethodValue; }
            set { SetProperty(ref _detectionMethodValue, value); }
        }

        /// <summary>
        /// Comments or interpretation notes
        /// </summary>
        private string _notesValue;
        public string Notes
        {
            get { return _notesValue; }
            set { SetProperty(ref _notesValue, value); }
        }

        // PPDM Entity Properties

        private string _activeIndValue = "Y";
        public string ACTIVE_IND
        {
            get { return _activeIndValue; }
            set { SetProperty(ref _activeIndValue, value); }
        }

        private string _rowCreatedByValue;
        public string ROW_CREATED_BY
        {
            get { return _rowCreatedByValue; }
            set { SetProperty(ref _rowCreatedByValue, value); }
        }

        private DateTime? _rowCreatedDateValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return _rowCreatedDateValue; }
            set { SetProperty(ref _rowCreatedDateValue, value); }
        }

        private string _rowChangedByValue;
        public string ROW_CHANGED_BY
        {
            get { return _rowChangedByValue; }
            set { SetProperty(ref _rowChangedByValue, value); }
        }

        private DateTime? _rowChangedDateValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return _rowChangedDateValue; }
            set { SetProperty(ref _rowChangedDateValue, value); }
        }

        private DateTime? _rowEffectiveDateValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return _rowEffectiveDateValue; }
            set { SetProperty(ref _rowEffectiveDateValue, value); }
        }

        private DateTime? _rowExpiryDateValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return _rowExpiryDateValue; }
            set { SetProperty(ref _rowExpiryDateValue, value); }
        }

        private string _rowQualityValue;
        public string ROW_QUALITY
        {
            get { return _rowQualityValue; }
            set { SetProperty(ref _rowQualityValue, value); }
        }

        private string _ppdmGuidValue;
        public string PPDM_GUID
        {
            get { return _ppdmGuidValue; }
            set { SetProperty(ref _ppdmGuidValue, value); }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ReservoirBoundary()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
