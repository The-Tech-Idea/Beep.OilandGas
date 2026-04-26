using System;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Data.PermitTables
{
    public partial class ENVIRONMENTAL_PERMIT_APPLICATION : ModelEntityBase
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

        private string? _environmentalPermitType;
        public string? ENVIRONMENTAL_PERMIT_TYPE { get => _environmentalPermitType; set => SetProperty(ref _environmentalPermitType, value); }

        private string? _wasteType;
        public string? WASTE_TYPE { get => _wasteType; set => SetProperty(ref _wasteType, value); }

        private decimal? _wasteVolume;
        public decimal? WASTE_VOLUME { get => _wasteVolume; set => SetProperty(ref _wasteVolume, value); }

        private string? _wasteVolumeUnit;
        public string? WASTE_VOLUME_UNIT { get => _wasteVolumeUnit; set => SetProperty(ref _wasteVolumeUnit, value); }

        private string? _disposalMethod;
        public string? DISPOSAL_METHOD { get => _disposalMethod; set => SetProperty(ref _disposalMethod, value); }

        private string? _facilityLocation;
        public string? FACILITY_LOCATION { get => _facilityLocation; set => SetProperty(ref _facilityLocation, value); }

        private string? _monitoringPlan;
        public string? MONITORING_PLAN { get => _monitoringPlan; set => SetProperty(ref _monitoringPlan, value); }

        private string? _environmentalImpact;
        public string? ENVIRONMENTAL_IMPACT { get => _environmentalImpact; set => SetProperty(ref _environmentalImpact, value); }

        private string? _mitigationPlan;
        public string? MITIGATION_PLAN { get => _mitigationPlan; set => SetProperty(ref _mitigationPlan, value); }

        private string? _wasteDisposalMethod;
        public string? WASTE_DISPOSAL_METHOD { get => _wasteDisposalMethod; set => SetProperty(ref _wasteDisposalMethod, value); }

        private string? _waterSource;
        public string? WATER_SOURCE { get => _waterSource; set => SetProperty(ref _waterSource, value); }

        private string? _normInvolvedInd;
        public string? NORM_INVOLVED_IND { get => _normInvolvedInd; set => SetProperty(ref _normInvolvedInd, value); }

        private string? _environmentalPermitApplicationId;
        public string? ENVIRONMENTAL_PERMIT_APPLICATION_ID { get => _environmentalPermitApplicationId; set => SetProperty(ref _environmentalPermitApplicationId, value); }
    }
}
