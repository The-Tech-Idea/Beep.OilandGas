using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class CompareWellsRequest : ModelEntityBase
    {
        /// <summary>
        /// List of well identifiers to compare
        /// </summary>
        private List<string> WellIdentifiersValue = new List<string>();

        [Required(ErrorMessage = "WellIdentifiers are required")]
        [MinLength(2, ErrorMessage = "At least two wells are required for comparison")]
        public List<string> WellIdentifiers

        {

            get { return this.WellIdentifiersValue; }

            set { SetProperty(ref WellIdentifiersValue, value); }

        }

        /// <summary>
        /// Optional list of field names to include in comparison
        /// </summary>
        private List<string>? FieldNamesValue;

        public List<string>? FieldNames

        {

            get { return this.FieldNamesValue; }

            set { SetProperty(ref FieldNamesValue, value); }

        }
    }
}
