using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class HeatMapResult : ModelEntityBase
    {
        private string _heatMapId = string.Empty;
        public string HeatMapId
        {
            get { return _heatMapId; }
            set { SetProperty(ref _heatMapId, value); }
        }

        private string _heatMapName = string.Empty;
        public string HeatMapName
        {
            get { return _heatMapName; }
            set { SetProperty(ref _heatMapName, value); }
        }

        private DateTime _generatedDate;
        public DateTime GeneratedDate
        {
            get { return _generatedDate; }
            set { SetProperty(ref _generatedDate, value); }
        }

        private List<HEAT_MAP_DATA_POINT> _dataPoints = new();
        public List<HEAT_MAP_DATA_POINT> DataPoints
        {
            get { return _dataPoints; }
            set { SetProperty(ref _dataPoints, value); }
        }

        private HeatMapConfigurationRecord _configuration = new();
        public HeatMapConfigurationRecord Configuration
        {
            get { return _configuration; }
            set { SetProperty(ref _configuration, value); }
        }
    }
}
