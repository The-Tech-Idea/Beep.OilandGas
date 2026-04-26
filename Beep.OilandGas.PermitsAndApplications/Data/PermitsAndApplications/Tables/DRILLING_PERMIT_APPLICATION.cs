using System;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Data.PermitTables
{
    public partial class DRILLING_PERMIT_APPLICATION : ModelEntityBase
    {
        private string _permitApplicationId = string.Empty;
        public string PERMIT_APPLICATION_ID { get => _permitApplicationId; set => SetProperty(ref _permitApplicationId, value); }

        private PermitApplicationType _applicationType;
        public PermitApplicationType APPLICATION_TYPE { get => _applicationType; set => SetProperty(ref _applicationType, value); }

        private PermitApplicationStatus _status;
        public PermitApplicationStatus STATUS { get => _status; set => SetProperty(ref _status, value); }

        private Country _country;
        public Country COUNTRY { get => _country; set => SetProperty(ref _country, value); }

        private StateProvince _stateProvince;
        public StateProvince STATE_PROVINCE { get => _stateProvince; set => SetProperty(ref _stateProvince, value); }

        private RegulatoryAuthority _regulatoryAuthority;
        public RegulatoryAuthority REGULATORY_AUTHORITY { get => _regulatoryAuthority; set => SetProperty(ref _regulatoryAuthority, value); }

        private DateTime? _submittedDate;
        public DateTime? SUBMITTED_DATE { get => _submittedDate; set => SetProperty(ref _submittedDate, value); }

        private DateTime? _receivedDate;
        public DateTime? RECEIVED_DATE { get => _receivedDate; set => SetProperty(ref _receivedDate, value); }

        private DateTime? _decisionDate;
        public DateTime? DECISION_DATE { get => _decisionDate; set => SetProperty(ref _decisionDate, value); }

        private DateTime? _effectiveDate;
        public DateTime? EFFECTIVE_DATE { get => _effectiveDate; set => SetProperty(ref _effectiveDate, value); }

        private DateTime? _expiryDate;
        public DateTime? EXPIRY_DATE { get => _expiryDate; set => SetProperty(ref _expiryDate, value); }

        private string? _decision;
        public string? DECISION { get => _decision; set => SetProperty(ref _decision, value); }

        private string? _referenceNumber;
        public string? REFERENCE_NUMBER { get => _referenceNumber; set => SetProperty(ref _referenceNumber, value); }

        private string? _feesDescription;
        public string? FEES_DESCRIPTION { get => _feesDescription; set => SetProperty(ref _feesDescription, value); }

        private bool _feesPaid;
        public bool FEES_PAID { get => _feesPaid; set => SetProperty(ref _feesPaid, value); }

        private string? _remarks;
        public string? REMARKS { get => _remarks; set => SetProperty(ref _remarks, value); }

        private bool _submissionComplete;
        public bool SUBMISSION_COMPLETE { get => _submissionComplete; set => SetProperty(ref _submissionComplete, value); }

        private string? _submissionDescription;
        public string? SUBMISSION_DESCRIPTION { get => _submissionDescription; set => SetProperty(ref _submissionDescription, value); }

        private string? _targetFormation;
        public string? TARGET_FORMATION { get => _targetFormation; set => SetProperty(ref _targetFormation, value); }

        private decimal _proposedDepth;
        public decimal PROPOSED_DEPTH { get => _proposedDepth; set => SetProperty(ref _proposedDepth, value); }

        private string? _wellId;
        public string? WELL_ID { get => _wellId; set => SetProperty(ref _wellId, value); }

        private string? _wellUwi;
        public string? WELL_UWI { get => _wellUwi; set => SetProperty(ref _wellUwi, value); }

        private string? _drillingMethod;
        public string? DRILLING_METHOD { get => _drillingMethod; set => SetProperty(ref _drillingMethod, value); }

        private string? _mudType;
        public string? MUD_TYPE { get => _mudType; set => SetProperty(ref _mudType, value); }

        private string? _casingProgram;
        public string? CASING_PROGRAM { get => _casingProgram; set => SetProperty(ref _casingProgram, value); }

        private string? _legalDescription;
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

        private string? _drillingPermitApplicationId;
        public string? DRILLING_PERMIT_APPLICATION_ID { get => _drillingPermitApplicationId; set => SetProperty(ref _drillingPermitApplicationId, value); }
    }
}
