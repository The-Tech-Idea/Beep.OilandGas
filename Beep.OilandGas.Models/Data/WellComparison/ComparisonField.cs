using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class ComparisonField : ModelEntityBase
    {
        /// <summary>
        /// Field name (e.g., "UWI", "Well Name", "Status", "Spud Date")
        /// </summary>
        private string FieldNameValue;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }

        /// <summary>
        /// Display label for the field
        /// </summary>
        private string DisplayLabelValue;

        public string DisplayLabel

        {

            get { return this.DisplayLabelValue; }

            set { SetProperty(ref DisplayLabelValue, value); }

        }

        /// <summary>
        /// Field data type
        /// </summary>
        private Type DataTypeValue;

        public Type DataType

        {

            get { return this.DataTypeValue; }

            set { SetProperty(ref DataTypeValue, value); }

        }

        /// <summary>
        /// Field category/group (e.g., "Basic Info", "Status", "Location", "Dates")
        /// </summary>
        private string CategoryValue;

        public string Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }

        /// <summary>
        /// Whether this field shows differences between wells
        /// </summary>
        private bool HasDifferencesValue;

        public bool HasDifferences

        {

            get { return this.HasDifferencesValue; }

            set { SetProperty(ref HasDifferencesValue, value); }

        }

        /// <summary>
        /// Order/sequence for display
        /// </summary>
        private int DisplayOrderValue;

        public int DisplayOrder

        {

            get { return this.DisplayOrderValue; }

            set { SetProperty(ref DisplayOrderValue, value); }

        }
    }
}
