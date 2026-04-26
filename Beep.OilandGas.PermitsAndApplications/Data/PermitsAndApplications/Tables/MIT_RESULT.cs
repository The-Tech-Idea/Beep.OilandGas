using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.PermitsAndApplications.Data.PermitTables
{
    /// <summary>
    /// Mechanical integrity test result — associated with injection permit applications.
    /// </summary>
    public partial class MIT_RESULT : ModelEntityBase
    {
        private string _mitResultId = string.Empty;
        public string MIT_RESULT_ID
        {
            get => _mitResultId;
            set => SetProperty(ref _mitResultId, value);
        }

        private string _injectionPermitApplicationId = string.Empty;
        public string INJECTION_PERMIT_APPLICATION_ID
        {
            get => _injectionPermitApplicationId;
            set => SetProperty(ref _injectionPermitApplicationId, value);
        }

        private DateTime _testDate;
        public DateTime TEST_DATE
        {
            get => _testDate;
            set => SetProperty(ref _testDate, value);
        }

        private string? _testType;
        public string? TEST_TYPE
        {
            get => _testType;
            set => SetProperty(ref _testType, value);
        }

        private decimal? _testPressure;
        public decimal? TEST_PRESSURE
        {
            get => _testPressure;
            set => SetProperty(ref _testPressure, value);
        }

        private string? _result;
        public string? RESULT
        {
            get => _result;
            set => SetProperty(ref _result, value);
        }

        private string? _remarks;
        public string? REMARKS
        {
            get => _remarks;
            set => SetProperty(ref _remarks, value);
        }
    }
}
