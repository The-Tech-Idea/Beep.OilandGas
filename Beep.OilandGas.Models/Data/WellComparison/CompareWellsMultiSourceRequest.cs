using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class CompareWellsMultiSourceRequest : ModelEntityBase
    {
        /// <summary>
        /// List of well comparisons with source mappings
        /// </summary>
        private List<WellSourceMapping> WellComparisonsValue = new List<WellSourceMapping>();

        [Required(ErrorMessage = "WellComparisons are required")]
        [MinLength(1, ErrorMessage = "At least one well comparison is required")]
        public List<WellSourceMapping> WellComparisons

        {

            get { return this.WellComparisonsValue; }

            set { SetProperty(ref WellComparisonsValue, value); }

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
