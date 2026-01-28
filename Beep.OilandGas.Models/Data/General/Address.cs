using System;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// Represents a physical address.
    /// </summary>
    public class Address : ModelEntityBase
    {
        private string Address1Value = string.Empty;
        
        /// <summary>
        /// Gets or sets the first line of the street address.
        /// </summary>
        public string Address1
        {
            get { return this.Address1Value; }
            set { SetProperty(ref Address1Value, value); }
        }

        /// <summary>
        /// Gets or sets the street address (alias for Address1).
        /// </summary>
        public string StreetAddress
        {
            get { return this.Address1Value; }
            set { SetProperty(ref Address1Value, value); }
        }

        private string Address2Value = string.Empty;
        
        /// <summary>
        /// Gets or sets the second line of the street address.
        /// </summary>
        public string Address2
        {
            get { return this.Address2Value; }
            set { SetProperty(ref Address2Value, value); }
        }

        private string CityValue = string.Empty;
        
        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City
        {
            get { return this.CityValue; }
            set { SetProperty(ref CityValue, value); }
        }

        private string StateValue = string.Empty;
        
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public string State
        {
            get { return this.StateValue; }
            set { SetProperty(ref StateValue, value); }
        }

        private string ZipCodeValue = string.Empty;
        
        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        public string ZipCode
        {
            get { return this.ZipCodeValue; }
            set { SetProperty(ref ZipCodeValue, value); }
        }

        private string CountryValue = "USA";
        
        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        public string Country
        {
            get { return this.CountryValue; }
            set { SetProperty(ref CountryValue, value); }
        }
    }
}

