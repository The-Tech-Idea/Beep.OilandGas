using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class HEAT_MAP_CONFIGURATION : ModelEntityBase
    {
        private string _heatMapId = string.Empty;
        public string HEAT_MAP_ID
        {
            get { return _heatMapId; }
            set { SetProperty(ref _heatMapId, value); }
        }

        private string _configurationName = string.Empty;
        public string CONFIGURATION_NAME
        {
            get { return _configurationName; }
            set { SetProperty(ref _configurationName, value); }
        }

        private string _activeInd = "Y";
        public new string ACTIVE_IND
        {
            get { return _activeInd; }
            set { SetProperty(ref _activeInd, value); }
        }

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

        private bool _useInterpolation;
        public bool USE_INTERPOLATION
        {
            get { return _useInterpolation; }
            set { SetProperty(ref _useInterpolation, value); }
        }

        private string _interpolationMethod = "BILINEAR";
        public string INTERPOLATION_METHOD
        {
            get { return _interpolationMethod; }
            set { SetProperty(ref _interpolationMethod, value); }
        }

        private double _interpolationCellSize = 10.0;
        public double INTERPOLATION_CELL_SIZE
        {
            get { return _interpolationCellSize; }
            set { SetProperty(ref _interpolationCellSize, value); }
        }

        private bool _showLegend = true;
        public bool SHOW_LEGEND
        {
            get { return _showLegend; }
            set { SetProperty(ref _showLegend, value); }
        }
    }
}
