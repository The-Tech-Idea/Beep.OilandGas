using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class HEAT_MAP_CONFIGURATION : ModelEntityBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        private string _configurationJson;
        public string ConfigurationJson
        {
            get { return _configurationJson; }
            set { SetProperty(ref _configurationJson, value); }
        }

        private string _userId;
        public string UserId
        {
            get { return _userId; }
            set { SetProperty(ref _userId, value); }
        }
    }
}
