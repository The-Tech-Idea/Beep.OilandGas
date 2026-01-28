using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class WellComparisonItem : ModelEntityBase
    {
        /// <summary>
        /// Well identifier (UWI, Well ID, etc.)
        /// </summary>
        private string WellIdentifierValue;

        public string WellIdentifier

        {

            get { return this.WellIdentifierValue; }

            set { SetProperty(ref WellIdentifierValue, value); }

        }

        /// <summary>
        /// Well name
        /// </summary>
        private string WellNameValue;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }

        /// <summary>
        /// Data source name (if comparing across different sources)
        /// </summary>
        private string DataSourceValue;

        public string DataSource

        {

            get { return this.DataSourceValue; }

            set { SetProperty(ref DataSourceValue, value); }

        }

        /// <summary>
        /// Field values for this well (keyed by field name)
        /// </summary>
        public Dictionary<string, object> FieldValues { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Additional metadata for this well
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
