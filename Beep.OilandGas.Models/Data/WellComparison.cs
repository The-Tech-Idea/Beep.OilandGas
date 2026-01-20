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
        public List<WellComparisonItem> Wells { get; set; } = new List<WellComparisonItem>();

        /// <summary>
        /// Comparison fields/attributes being compared
        /// </summary>
        public List<ComparisonField> ComparisonFields { get; set; } = new List<ComparisonField>();

        /// <summary>
        /// Metadata about the comparison
        /// </summary>
        public ComparisonMetadata Metadata { get; set; } = new ComparisonMetadata();
    }

    /// <summary>
    /// Represents a single well in the comparison
    /// </summary>
    public class WellComparisonItem : ModelEntityBase
    {
        /// <summary>
        /// Well identifier (UWI, Well ID, etc.)
        /// </summary>
        public string WellIdentifier { get; set; }

        /// <summary>
        /// Well name
        /// </summary>
        public string WellName { get; set; }

        /// <summary>
        /// Data source name (if comparing across different sources)
        /// </summary>
        public string DataSource { get; set; }

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
        public string FieldName { get; set; }

        /// <summary>
        /// Display label for the field
        /// </summary>
        public string DisplayLabel { get; set; }

        /// <summary>
        /// Field data type
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// Field category/group (e.g., "Basic Info", "Status", "Location", "Dates")
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Whether this field shows differences between wells
        /// </summary>
        public bool HasDifferences { get; set; }

        /// <summary>
        /// Order/sequence for display
        /// </summary>
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Metadata about the comparison operation
    /// </summary>
    public class ComparisonMetadata : ModelEntityBase
    {
        /// <summary>
        /// When the comparison was generated
        /// </summary>
        public DateTime ComparisonDate { get; set; } = DateTime.Now;

        /// <summary>
        /// User who requested the comparison
        /// </summary>
        public string RequestedBy { get; set; }

        /// <summary>
        /// Number of wells being compared
        /// </summary>
        public int WellCount { get; set; }

        /// <summary>
        /// Number of fields being compared
        /// </summary>
        public int FieldCount { get; set; }

        /// <summary>
        /// Data sources involved in the comparison
        /// </summary>
        public List<string> DataSources { get; set; } = new List<string>();

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
        [Required(ErrorMessage = "WellIdentifiers are required")]
        [MinLength(2, ErrorMessage = "At least two wells are required for comparison")]
        public List<string> WellIdentifiers { get; set; } = new List<string>();

        /// <summary>
        /// Optional list of field names to include in comparison
        /// </summary>
        public List<string>? FieldNames { get; set; }
    }

    /// <summary>
    /// Request for comparing wells from multiple sources
    /// </summary>
    public class CompareWellsMultiSourceRequest : ModelEntityBase
    {
        /// <summary>
        /// List of well comparisons with source mappings
        /// </summary>
        [Required(ErrorMessage = "WellComparisons are required")]
        [MinLength(1, ErrorMessage = "At least one well comparison is required")]
        public List<WellSourceMapping> WellComparisons { get; set; } = new List<WellSourceMapping>();

        /// <summary>
        /// Optional list of field names to include in comparison
        /// </summary>
        public List<string>? FieldNames { get; set; }
    }
}





