using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class LeaseLocation : ModelEntityBase
    {
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
        /// Gets or sets the county.
        /// </summary>
        private string? CountyValue;

        public string? County

        {

            get { return this.CountyValue; }

            set { SetProperty(ref CountyValue, value); }

        }

        /// <summary>
        /// Gets or sets the township.
        /// </summary>
        private string? TownshipValue;

        public string? Township

        {

            get { return this.TownshipValue; }

            set { SetProperty(ref TownshipValue, value); }

        }

        /// <summary>
        /// Gets or sets the range.
        /// </summary>
        private string? RangeValue;

        public string? Range

        {

            get { return this.RangeValue; }

            set { SetProperty(ref RangeValue, value); }

        }

        /// <summary>
        /// Gets or sets the section.
        /// </summary>
        private string? SectionValue;

        public string? Section

        {

            get { return this.SectionValue; }

            set { SetProperty(ref SectionValue, value); }

        }

        /// <summary>
        /// Gets or sets the number of acres.
        /// </summary>
        private decimal? AcresValue;

        public decimal? Acres

        {

            get { return this.AcresValue; }

            set { SetProperty(ref AcresValue, value); }

        }

        /// <summary>
        /// Gets or sets the API number (if assigned).
        /// </summary>
        private string? ApiNumberValue;

        public string? ApiNumber

        {

            get { return this.ApiNumberValue; }

            set { SetProperty(ref ApiNumberValue, value); }

        }
    }
}
