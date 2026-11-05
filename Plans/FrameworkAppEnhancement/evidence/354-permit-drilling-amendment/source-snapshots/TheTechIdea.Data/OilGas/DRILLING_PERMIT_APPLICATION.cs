using System;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Data.PermitTables
{
    public partial class DRILLING_PERMIT_APPLICATION : ModelEntityBase
    {
        private string _permitApplicationId = string.Empty;
        [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string PERMIT_APPLICATION_ID { get => _permitApplicationId; set => SetProperty(ref _permitApplicationId, value); }

        private string? _remarks;
        [System.ComponentModel.DataAnnotations.MaxLength(4096)]
        public string? REMARKS { get => _remarks; set => SetProperty(ref _remarks, value); }

        private string? _targetFormation;
        public string? TARGET_FORMATION { get => _targetFormation; set => SetProperty(ref _targetFormation, value); }

        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? PROPOSED_DEPTH_UNIT { get; set; }

        private decimal _proposedDepth;
        public decimal PROPOSED_DEPTH { get => _proposedDepth; set => SetProperty(ref _proposedDepth, value); }

        private string? _drillingMethod;
        public string? DRILLING_METHOD { get => _drillingMethod; set => SetProperty(ref _drillingMethod, value); }

        private string? _mudType;
        public string? MUD_TYPE { get => _mudType; set => SetProperty(ref _mudType, value); }

        private string? _casingProgram;
        [System.ComponentModel.DataAnnotations.MaxLength(4096)]
        public string? CASING_PROGRAM { get => _casingProgram; set => SetProperty(ref _casingProgram, value); }

        private string? _legalDescription;
        [System.ComponentModel.DataAnnotations.MaxLength(4096)]
        public string? LEGAL_DESCRIPTION { get => _legalDescription; set => SetProperty(ref _legalDescription, value); }

        private string? _surfaceOwnerNotifiedInd;
        public string? SURFACE_OWNER_NOTIFIED_IND { get => _surfaceOwnerNotifiedInd; set => SetProperty(ref _surfaceOwnerNotifiedInd, value); }

        private string? _environmentalAssessmentRequiredInd;
        public string? ENVIRONMENTAL_ASSESSMENT_REQUIRED_IND { get => _environmentalAssessmentRequiredInd; set => SetProperty(ref _environmentalAssessmentRequiredInd, value); }

        private string? _environmentalAssessmentReference;
        public string? ENVIRONMENTAL_ASSESSMENT_REFERENCE { get => _environmentalAssessmentReference; set => SetProperty(ref _environmentalAssessmentReference, value); }

        private string? _spacingUnit;
        public string? SPACING_UNIT { get => _spacingUnit; set => SetProperty(ref _spacingUnit, value); }

        private string? _permitType;
        public string? PERMIT_TYPE { get => _permitType; set => SetProperty(ref _permitType, value); }

    }
}
