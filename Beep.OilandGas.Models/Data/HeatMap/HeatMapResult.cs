using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
    public class HeatMapResult : ModelEntityBase
    {
        private string HeatMapIdValue = string.Empty;

        public string HeatMapId

        {

            get { return this.HeatMapIdValue; }

            set { SetProperty(ref HeatMapIdValue, value); }

        }
        private string HeatMapNameValue = string.Empty;

        public string HeatMapName

        {

            get { return this.HeatMapNameValue; }

            set { SetProperty(ref HeatMapNameValue, value); }

        }
        private DateTime GeneratedDateValue;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
        private List<HEAT_MAP_DATA_POINT> DataPointsValue = new();

        public List<HEAT_MAP_DATA_POINT> DataPoints

        {

            get { return this.DataPointsValue; }

            set { SetProperty(ref DataPointsValue, value); }

        }
        private HeatMapConfigurationRecord ConfigurationValue = new();

        public HeatMapConfigurationRecord Configuration

        {

            get { return this.ConfigurationValue; }

            set { SetProperty(ref ConfigurationValue, value); }

        }
        private byte[]? RenderedImageValue;

        public byte[]? RenderedImage

        {

            get { return this.RenderedImageValue; }

            set { SetProperty(ref RenderedImageValue, value); }

        }
    }
}
