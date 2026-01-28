using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class ComparisonMetadata : ModelEntityBase
    {
        /// <summary>
        /// When the comparison was generated
        /// </summary>
        private DateTime ComparisonDateValue = DateTime.Now;

        public DateTime ComparisonDate

        {

            get { return this.ComparisonDateValue; }

            set { SetProperty(ref ComparisonDateValue, value); }

        }

        /// <summary>
        /// User who requested the comparison
        /// </summary>
        private string RequestedByValue;

        public string RequestedBy

        {

            get { return this.RequestedByValue; }

            set { SetProperty(ref RequestedByValue, value); }

        }

        /// <summary>
        /// Number of wells being compared
        /// </summary>
        private int WellCountValue;

        public int WellCount

        {

            get { return this.WellCountValue; }

            set { SetProperty(ref WellCountValue, value); }

        }

        /// <summary>
        /// Number of fields being compared
        /// </summary>
        private int FieldCountValue;

        public int FieldCount

        {

            get { return this.FieldCountValue; }

            set { SetProperty(ref FieldCountValue, value); }

        }

        /// <summary>
        /// Data sources involved in the comparison
        /// </summary>
        private List<string> DataSourcesValue = new List<string>();

        public List<string> DataSources

        {

            get { return this.DataSourcesValue; }

            set { SetProperty(ref DataSourcesValue, value); }

        }

        /// <summary>
        /// Additional comparison options/settings
        /// </summary>
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
    }
}
