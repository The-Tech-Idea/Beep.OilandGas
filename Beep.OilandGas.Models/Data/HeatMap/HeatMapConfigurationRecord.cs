using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
    public class HeatMapConfigurationRecord : ModelEntityBase
    {
        private string ConfigurationIdValue = string.Empty;

        public string ConfigurationId

        {

            get { return this.ConfigurationIdValue; }

            set { SetProperty(ref ConfigurationIdValue, value); }

        }
        private string ConfigurationNameValue = string.Empty;

        public string ConfigurationName

        {

            get { return this.ConfigurationNameValue; }

            set { SetProperty(ref ConfigurationNameValue, value); }

        }
        private string ColorSchemeTypeValue = "Viridis";

        public string ColorSchemeType

        {

            get { return this.ColorSchemeTypeValue; }

            set { SetProperty(ref ColorSchemeTypeValue, value); }

        }
        private int ColorStepsValue = 256;

        public int ColorSteps

        {

            get { return this.ColorStepsValue; }

            set { SetProperty(ref ColorStepsValue, value); }

        }
        private bool ShowLegendValue = true;

        public bool ShowLegend

        {

            get { return this.ShowLegendValue; }

            set { SetProperty(ref ShowLegendValue, value); }

        }
        private bool UseInterpolationValue = false;

        public bool UseInterpolation

        {

            get { return this.UseInterpolationValue; }

            set { SetProperty(ref UseInterpolationValue, value); }

        }
        private string InterpolationMethodValue = "InverseDistanceWeighting";

        public string InterpolationMethod

        {

            get { return this.InterpolationMethodValue; }

            set { SetProperty(ref InterpolationMethodValue, value); }

        }
        private double InterpolationCellSizeValue = 10.0;

        public double InterpolationCellSize

        {

            get { return this.InterpolationCellSizeValue; }

            set { SetProperty(ref InterpolationCellSizeValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
    }
}
