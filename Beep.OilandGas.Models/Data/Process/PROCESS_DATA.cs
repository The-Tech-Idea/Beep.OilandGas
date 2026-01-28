using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Process
{
    public class PROCESS_DATA : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;
        public string InstanceId
        {
            get => InstanceIdValue;
            set => SetProperty(ref InstanceIdValue, value);
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
