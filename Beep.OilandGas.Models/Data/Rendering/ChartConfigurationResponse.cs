using System;
using System.Collections.Generic;
using SkiaSharp;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Rendering
{
    public class ChartConfigurationResponse : ModelEntityBase
    {
        private string ConfigurationIdValue;

        public string ConfigurationId

        {

            get { return this.ConfigurationIdValue; }

            set { SetProperty(ref ConfigurationIdValue, value); }

        }
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
        private bool IsDefaultValue;

        public bool IsDefault

        {

            get { return this.IsDefaultValue; }

            set { SetProperty(ref IsDefaultValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private string CreatedByValue;

        public string CreatedBy

        {

            get { return this.CreatedByValue; }

            set { SetProperty(ref CreatedByValue, value); }

        }
    }
}
