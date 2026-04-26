using System;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Data.PermitTables
{
    /// <summary>
    /// Permit application table — extension schema not in the original PPDM39 distribution.
    /// Tracks the full lifecycle of regulatory permit filings: draft → submitted → under review → approved/rejected.
    /// </summary>
    public partial class PERMIT_APPLICATION : ModelEntityBase
    {
        private string _permitApplicationId = string.Empty;
        public string PERMIT_APPLICATION_ID
        {
            get => _permitApplicationId;
            set => SetProperty(ref _permitApplicationId, value);
        }

        private string _applicantId = string.Empty;
        public string APPLICANT_ID
        {
            get => _applicantId;
            set => SetProperty(ref _applicantId, value);
        }

        private PermitApplicationType _applicationType;
        public PermitApplicationType APPLICATION_TYPE
        {
            get => _applicationType;
            set => SetProperty(ref _applicationType, value);
        }

        private PermitApplicationStatus _status;
        public PermitApplicationStatus STATUS
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private Country _country;
        public Country COUNTRY
        {
            get => _country;
            set => SetProperty(ref _country, value);
        }

        private StateProvince _stateProvince;
        public StateProvince STATE_PROVINCE
        {
            get => _stateProvince;
            set => SetProperty(ref _stateProvince, value);
        }

        private RegulatoryAuthority _regulatoryAuthority;
        public RegulatoryAuthority REGULATORY_AUTHORITY
        {
            get => _regulatoryAuthority;
            set => SetProperty(ref _regulatoryAuthority, value);
        }

        private DateTime? _createdDate;
        public DateTime? CREATED_DATE
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        private DateTime? _submittedDate;
        public DateTime? SUBMITTED_DATE
        {
            get => _submittedDate;
            set => SetProperty(ref _submittedDate, value);
        }

        private DateTime? _receivedDate;
        public DateTime? RECEIVED_DATE
        {
            get => _receivedDate;
            set => SetProperty(ref _receivedDate, value);
        }

        private DateTime? _decisionDate;
        public DateTime? DECISION_DATE
        {
            get => _decisionDate;
            set => SetProperty(ref _decisionDate, value);
        }

        private DateTime? _effectiveDate;
        public DateTime? EFFECTIVE_DATE
        {
            get => _effectiveDate;
            set => SetProperty(ref _effectiveDate, value);
        }

        private DateTime? _expiryDate;
        public DateTime? EXPIRY_DATE
        {
            get => _expiryDate;
            set => SetProperty(ref _expiryDate, value);
        }

        private string? _decision;
        public string? DECISION
        {
            get => _decision;
            set => SetProperty(ref _decision, value);
        }

        private string? _referenceNumber;
        public string? REFERENCE_NUMBER
        {
            get => _referenceNumber;
            set => SetProperty(ref _referenceNumber, value);
        }

        private string? _feesDescription;
        public string? FEES_DESCRIPTION
        {
            get => _feesDescription;
            set => SetProperty(ref _feesDescription, value);
        }

        private bool _feesPaid;
        public bool FEES_PAID
        {
            get => _feesPaid;
            set => SetProperty(ref _feesPaid, value);
        }

        private string? _remarks;
        public string? REMARKS
        {
            get => _remarks;
            set => SetProperty(ref _remarks, value);
        }

        private bool _submissionComplete;
        public bool SUBMISSION_COMPLETE
        {
            get => _submissionComplete;
            set => SetProperty(ref _submissionComplete, value);
        }

        private string? _submissionDescription;
        public string? SUBMISSION_DESCRIPTION
        {
            get => _submissionDescription;
            set => SetProperty(ref _submissionDescription, value);
        }

        private string? _wellId;
        public string? WELL_ID
        {
            get => _wellId;
            set => SetProperty(ref _wellId, value);
        }

        private string? _facilityId;
        public string? FACILITY_ID
        {
            get => _facilityId;
            set => SetProperty(ref _facilityId, value);
        }

        private string _fieldId = string.Empty;
        public string FIELD_ID
        {
            get => _fieldId;
            set => SetProperty(ref _fieldId, value);
        }

        private string? _applicationId;
        public string? APPLICATION_ID
        {
            get => _applicationId;
            set => SetProperty(ref _applicationId, value);
        }

        private string? _operatorId;
        public string? OPERATOR_ID
        {
            get => _operatorId;
            set => SetProperty(ref _operatorId, value);
        }

        private string? _relatedWellUwi;
        public string? RELATED_WELL_UWI
        {
            get => _relatedWellUwi;
            set => SetProperty(ref _relatedWellUwi, value);
        }

        private string? _relatedFacilityId;
        public string? RELATED_FACILITY_ID
        {
            get => _relatedFacilityId;
            set => SetProperty(ref _relatedFacilityId, value);
        }

        private string? _feesPaidInd;
        public string? FEES_PAID_IND
        {
            get => _feesPaidInd;
            set => SetProperty(ref _feesPaidInd, value);
        }

        private string? _submissionCompleteInd;
        public string? SUBMISSION_COMPLETE_IND
        {
            get => _submissionCompleteInd;
            set => SetProperty(ref _submissionCompleteInd, value);
        }

        private string? _wasteType;
        public string? WASTE_TYPE
        {
            get => _wasteType;
            set => SetProperty(ref _wasteType, value);
        }
    }
}
