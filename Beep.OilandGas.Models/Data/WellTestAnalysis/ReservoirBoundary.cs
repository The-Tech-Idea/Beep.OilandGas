using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
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

       
        /// <summary>
        /// Default constructor
        /// </summary>
        public ReservoirBoundary()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}


