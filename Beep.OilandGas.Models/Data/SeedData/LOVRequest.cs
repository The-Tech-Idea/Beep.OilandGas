using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class LOVRequest : ModelEntityBase
    {
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string? ValueTypeValue;

        public string? ValueType

        {

            get { return this.ValueTypeValue; }

            set { SetProperty(ref ValueTypeValue, value); }

        }
        private string? CategoryValue;

        public string? Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
        private string? ModuleValue;

        public string? Module

        {

            get { return this.ModuleValue; }

            set { SetProperty(ref ModuleValue, value); }

        }

    }
}
