using System;
using System.Collections.Generic;
using SkiaSharp;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Rendering
{
    public class SaveChartConfigurationRequest : ModelEntityBase
    {
        private string ChartTypeValue;

        public string ChartType

        {

            get { return this.ChartTypeValue; }

            set { SetProperty(ref ChartTypeValue, value); }

        }
        private string ConfigurationNameValue;

        public string ConfigurationName

        {

            get { return this.ConfigurationNameValue; }

            set { SetProperty(ref ConfigurationNameValue, value); }

        }
        private string ConfigurationDataValue;

        public string ConfigurationData

        {

            get { return this.ConfigurationDataValue; }

            set { SetProperty(ref ConfigurationDataValue, value); }

        }
        private bool IsDefaultValue = false;

        public bool IsDefault

        {

            get { return this.IsDefaultValue; }

            set { SetProperty(ref IsDefaultValue, value); }

        }
    }
}
