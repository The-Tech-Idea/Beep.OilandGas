using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.PermitsAndApplications.Data.PermitTables
{
    /// <summary>
    /// Jurisdiction requirements — regulatory requirements per country/state/authority combination.
    /// </summary>
    public partial class JURISDICTION_REQUIREMENTS : ModelEntityBase
    {
        private string _jurisdictionRequirementsId = string.Empty;
        public string JURISDICTION_REQUIREMENTS_ID
        {
            get => _jurisdictionRequirementsId;
            set => SetProperty(ref _jurisdictionRequirementsId, value);
        }

        private string _country = string.Empty;
        public string COUNTRY
        {
            get => _country;
            set => SetProperty(ref _country, value);
        }

        private string _stateProvince = string.Empty;
        public string STATE_PROVINCE
        {
            get => _stateProvince;
            set => SetProperty(ref _stateProvince, value);
        }

        private string _regulatoryAuthority = string.Empty;
        public string REGULATORY_AUTHORITY
        {
            get => _regulatoryAuthority;
            set => SetProperty(ref _regulatoryAuthority, value);
        }

        private string? _requirementsDescription;
        public string? REQUIREMENTS_DESCRIPTION
        {
            get => _requirementsDescription;
            set => SetProperty(ref _requirementsDescription, value);
        }

        private string? _requiredForms;
        public string? REQUIRED_FORMS
        {
            get => _requiredForms;
            set => SetProperty(ref _requiredForms, value);
        }

        private string? _filingDeadline;
        public string? FILING_DEADLINE
        {
            get => _filingDeadline;
            set => SetProperty(ref _filingDeadline, value);
        }
    }
}
