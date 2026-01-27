using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Process
{
    /// <summary>
    /// Represents strongly typed step data for a process step instance.
    /// Replaces Dictionary<string, object> StepData.
    /// </summary>
    public class PROCESS_STEP_DATA : ModelEntityBase
    {
        private string StepInstanceIdValue = string.Empty;
        public string StepInstanceId
        {
            get => StepInstanceIdValue;
            set => SetProperty(ref StepInstanceIdValue, value);
        }

        private string DataJsonValue = string.Empty;
        public string DataJson
        {
            get => DataJsonValue;
            set => SetProperty(ref DataJsonValue, value);
        }

        private DateTime? LastUpdatedValue;
        public DateTime? LastUpdated
        {
            get => LastUpdatedValue;
            set => SetProperty(ref LastUpdatedValue, value);
        }
    }
}
