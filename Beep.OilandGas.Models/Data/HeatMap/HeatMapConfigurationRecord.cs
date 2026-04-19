using System;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class HeatMapConfigurationRecord : ModelEntityBase
    {
        private string _configurationId = string.Empty;
        public string ConfigurationId
        {
            get { return _configurationId; }
            set { SetProperty(ref _configurationId, value); }
        }

        private string _configurationName = string.Empty;
        public string ConfigurationName
        {
            get { return _configurationName; }
            set { SetProperty(ref _configurationName, value); }
        }

        private DateTime _createdDate;
        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set { SetProperty(ref _createdDate, value); }
        }
    }
}
