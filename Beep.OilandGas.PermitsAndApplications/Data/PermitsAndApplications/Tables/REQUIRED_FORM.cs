using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.PermitsAndApplications.Data.PermitTables
{
    public partial class REQUIRED_FORM : ModelEntityBase
    {
        private string _requiredFormId = string.Empty;
        public string REQUIRED_FORM_ID { get => _requiredFormId; set => SetProperty(ref _requiredFormId, value); }

        private string _permitApplicationId = string.Empty;
        public string PERMIT_APPLICATION_ID { get => _permitApplicationId; set => SetProperty(ref _permitApplicationId, value); }

        private string _formName = string.Empty;
        public string FORM_NAME { get => _formName; set => SetProperty(ref _formName, value); }

        private string? _formCode;
        public string? FORM_CODE { get => _formCode; set => SetProperty(ref _formCode, value); }

        private string? _formType;
        public string? FORM_TYPE { get => _formType; set => SetProperty(ref _formType, value); }

        private string _status = string.Empty;
        public string STATUS { get => _status; set => SetProperty(ref _status, value); }

        private string? _requiredInd;
        public string? REQUIRED_IND { get => _requiredInd; set => SetProperty(ref _requiredInd, value); }

        private DateTime? _submittedDate;
        public DateTime? SUBMITTED_DATE { get => _submittedDate; set => SetProperty(ref _submittedDate, value); }

        private string? _remarks;
        public string? REMARKS { get => _remarks; set => SetProperty(ref _remarks, value); }
    }
}
