using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class WellComparison : ModelEntityBase
    {
        /// <summary>
        /// List of wells being compared
        /// </summary>
        private List<WellComparisonItem> WellsValue = new List<WellComparisonItem>();

        public List<WellComparisonItem> Wells

        {

            get { return this.WellsValue; }

            set { SetProperty(ref WellsValue, value); }

        }

        /// <summary>
        /// Comparison fields/attributes being compared
        /// </summary>
        private List<ComparisonField> ComparisonFieldsValue = new List<ComparisonField>();

        public List<ComparisonField> ComparisonFields

        {

            get { return this.ComparisonFieldsValue; }

            set { SetProperty(ref ComparisonFieldsValue, value); }

        }

        /// <summary>
        /// Metadata about the comparison
        /// </summary>
        private ComparisonMetadata MetadataValue = new ComparisonMetadata();

        public ComparisonMetadata Metadata

        {

            get { return this.MetadataValue; }

            set { SetProperty(ref MetadataValue, value); }

        }
    }
}
