using System;

namespace Beep.OilandGas.Models.Data
{
    public class Address : ModelEntityBase
    {
        private string Address1Value = string.Empty;
        public string Address1
        {
            get { return this.Address1Value; }
            set { SetProperty(ref Address1Value, value); }
        }

        private string Address2Value = string.Empty;
        public string Address2
        {
            get { return this.Address2Value; }
            set { SetProperty(ref Address2Value, value); }
        }

        private string CityValue = string.Empty;
        public string City
        {
            get { return this.CityValue; }
            set { SetProperty(ref CityValue, value); }
        }

        private string StateValue = string.Empty;
        public string State
        {
            get { return this.StateValue; }
            set { SetProperty(ref StateValue, value); }
        }

        private string ZipCodeValue = string.Empty;
        public string ZipCode
        {
            get { return this.ZipCodeValue; }
            set { SetProperty(ref ZipCodeValue, value); }
        }

        private string CountryValue = string.Empty;
        public string Country
        {
            get { return this.CountryValue; }
            set { SetProperty(ref CountryValue, value); }
        }
    }
}
