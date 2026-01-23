using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for comparing multiple wells side-by-side
    /// Used for data management comparison views
    /// </summary>
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

    /// <summary>
    /// Represents a single well in the comparison
    /// </summary>
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

    /// <summary>
    /// Represents a comparison field/attribute
    /// </summary>
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

    /// <summary>
    /// Metadata about the comparison operation
    /// </summary>
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

    /// <summary>
    /// Request for comparing wells
    /// </summary>
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

    /// <summary>
    /// Request for comparing wells from multiple sources
    /// </summary>
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







