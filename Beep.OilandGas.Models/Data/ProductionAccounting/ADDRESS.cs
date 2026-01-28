using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class Address : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        private string StreetAddressValue = string.Empty;

        public string StreetAddress

        {

            get { return this.StreetAddressValue; }

            set { SetProperty(ref StreetAddressValue, value); }

        }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        private string CityValue = string.Empty;

        public string City

        {

            get { return this.CityValue; }

            set { SetProperty(ref CityValue, value); }

        }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        private string StateValue = string.Empty;

        public string State

        {

            get { return this.StateValue; }

            set { SetProperty(ref StateValue, value); }

        }

        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        private string ZipCodeValue = string.Empty;

        public string ZipCode

        {

            get { return this.ZipCodeValue; }

            set { SetProperty(ref ZipCodeValue, value); }

        }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        private string CountryValue = "USA";

        public string Country

        {

            get { return this.CountryValue; }

            set { SetProperty(ref CountryValue, value); }

        }
    }
}
