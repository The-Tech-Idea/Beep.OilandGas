using System;
using System.Linq;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Represents a well identifier with multiple formats.
    /// </summary>
    public class WellIdentifier
    {
        /// <summary>
        /// Gets or sets the UWI (Unique Well Identifier).
        /// </summary>
        public string UWI { get; set; }

        /// <summary>
        /// Gets or sets the API number.
        /// </summary>
        public string ApiNumber { get; set; }

        /// <summary>
        /// Gets or sets the well name.
        /// </summary>
        public string WellName { get; set; }

        /// <summary>
        /// Gets or sets the lease name.
        /// </summary>
        public string LeaseName { get; set; }

        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the operator name.
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the state/province.
        /// </summary>
        public string StateProvince { get; set; }

        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// Gets or sets the primary identifier (UWI or API).
        /// </summary>
        public string PrimaryIdentifier => !string.IsNullOrEmpty(UWI) ? UWI : ApiNumber;

        /// <summary>
        /// Gets a display name for the well.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(WellName))
                    return WellName;
                if (!string.IsNullOrEmpty(LeaseName))
                    return LeaseName;
                return PrimaryIdentifier;
            }
        }

        /// <summary>
        /// Creates a well identifier from a string (auto-detects format).
        /// </summary>
        public static WellIdentifier FromString(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return null;

            var wellId = new WellIdentifier();

            // Try to detect format
            if (identifier.Length == 14 && identifier.All(char.IsDigit))
            {
                // Likely API number
                wellId.ApiNumber = identifier;
            }
            else if (identifier.Contains("-") || identifier.Contains("_"))
            {
                // Likely UWI format
                wellId.UWI = identifier;
            }
            else
            {
                // Default to well name
                wellId.WellName = identifier;
            }

            return wellId;
        }

        /// <summary>
        /// Returns the primary identifier as string.
        /// </summary>
        public override string ToString()
        {
            return PrimaryIdentifier ?? DisplayName ?? "";
        }
    }
}

