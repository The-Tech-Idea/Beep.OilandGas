using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
    public class GenerateHeatMapRequest : ModelEntityBase
    {
        private List<HEAT_MAP_DATA_POINT> DataPointsValue = new();

        [Required(ErrorMessage = "DataPoints are required")]
        [MinLength(1, ErrorMessage = "At least one data point is required")]
        public List<HEAT_MAP_DATA_POINT> DataPoints

        {

            get { return this.DataPointsValue; }

            set { SetProperty(ref DataPointsValue, value); }

        }

        private HEAT_MAP_CONFIGURATION ConfigurationValue = null!;


        [Required(ErrorMessage = "Configuration is required")]
        public HEAT_MAP_CONFIGURATION Configuration


        {


            get { return this.ConfigurationValue; }


            set { SetProperty(ref ConfigurationValue, value); }


        }
    }
}
