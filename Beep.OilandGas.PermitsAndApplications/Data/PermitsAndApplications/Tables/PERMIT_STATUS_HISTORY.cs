using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.PermitsAndApplications.Data.PermitTables
{
    /// <summary>
    /// Permit status history — tracks every status transition for a permit application.
    /// </summary>
    public partial class PERMIT_STATUS_HISTORY : ModelEntityBase
    {
        private string _permitStatusHistoryId = string.Empty;
        public string PERMIT_STATUS_HISTORY_ID
        {
            get => _permitStatusHistoryId;
            set => SetProperty(ref _permitStatusHistoryId, value);
        }

        private string _permitApplicationId = string.Empty;
        public string PERMIT_APPLICATION_ID
        {
            get => _permitApplicationId;
            set => SetProperty(ref _permitApplicationId, value);
        }

        private string _status = string.Empty;
        public string STATUS
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private DateTime _statusDate;
        public DateTime STATUS_DATE
        {
            get => _statusDate;
            set => SetProperty(ref _statusDate, value);
        }

        private string? _statusRemarks;
        public string? STATUS_REMARKS
        {
            get => _statusRemarks;
            set => SetProperty(ref _statusRemarks, value);
        }

        private string _updatedBy = string.Empty;
        public string UPDATED_BY
        {
            get => _updatedBy;
            set => SetProperty(ref _updatedBy, value);
        }
    }
}
