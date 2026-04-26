using System;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Data.PermitTables
{
    public partial class INJECTION_PERMIT_APPLICATION : ModelEntityBase
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

        private string? _injectionType;
        public string? INJECTION_TYPE { get => _injectionType; set => SetProperty(ref _injectionType, value); }

        private string? _injectionZone;
        public string? INJECTION_ZONE { get => _injectionZone; set => SetProperty(ref _injectionZone, value); }

        private decimal? _maxInjectionPressure;
        public decimal? MAX_INJECTION_PRESSURE { get => _maxInjectionPressure; set => SetProperty(ref _maxInjectionPressure, value); }

        private decimal? _maxInjectionRate;
        public decimal? MAX_INJECTION_RATE { get => _maxInjectionRate; set => SetProperty(ref _maxInjectionRate, value); }

        private string? _fluidType;
        public string? FLUID_TYPE { get => _fluidType; set => SetProperty(ref _fluidType, value); }

        private string? _injectionFluid;
        public string? INJECTION_FLUID { get => _injectionFluid; set => SetProperty(ref _injectionFluid, value); }

        private string? _injectionWellUwi;
        public string? INJECTION_WELL_UWI { get => _injectionWellUwi; set => SetProperty(ref _injectionWellUwi, value); }

        private decimal? _maximumInjectionPressure;
        public decimal? MAXIMUM_INJECTION_PRESSURE { get => _maximumInjectionPressure; set => SetProperty(ref _maximumInjectionPressure, value); }

        private decimal? _maximumInjectionRate;
        public decimal? MAXIMUM_INJECTION_RATE { get => _maximumInjectionRate; set => SetProperty(ref _maximumInjectionRate, value); }

        private string? _injectionRateUnit;
        public string? INJECTION_RATE_UNIT { get => _injectionRateUnit; set => SetProperty(ref _injectionRateUnit, value); }

        private string? _monitoringRequirements;
        public string? MONITORING_REQUIREMENTS { get => _monitoringRequirements; set => SetProperty(ref _monitoringRequirements, value); }

        private string? _isCo2StorageInd;
        public string? IS_CO2_STORAGE_IND { get => _isCo2StorageInd; set => SetProperty(ref _isCo2StorageInd, value); }

        private string? _isGasStorageInd;
        public string? IS_GAS_STORAGE_IND { get => _isGasStorageInd; set => SetProperty(ref _isGasStorageInd, value); }

        private string? _isBrineMiningInd;
        public string? IS_BRINE_MINING_IND { get => _isBrineMiningInd; set => SetProperty(ref _isBrineMiningInd, value); }
    }
}
