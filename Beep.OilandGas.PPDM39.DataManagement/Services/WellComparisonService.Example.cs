using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Example usage of WellComparisonService
    /// This file demonstrates how to use the well comparison feature
    /// </summary>
    public class WellComparisonServiceExample
    {
        /// <summary>
        /// Example: Compare multiple wells from the same data source
        /// </summary>
        public static async Task<WellComparisonDTO> CompareWellsExample(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata)
        {
            // Create comparison service
            var comparisonService = new WellComparisonService(
                editor, commonColumnHandler, defaults, metadata, "PPDM39");

            // Compare wells by their UWIs
            var wellIdentifiers = new List<string>
            {
                "UWI-001",
                "UWI-002",
                "UWI-003"
            };

            // Compare all fields
            var comparison = await comparisonService.CompareWellsAsync(wellIdentifiers);

            // Or compare specific fields only
            var specificFields = new List<string>
            {
                "UWI",
                "WELL_NAME",
                "WELL_TYPE",
                "SPUD_DATE",
                "TOTAL_DEPTH",
                "CURRENT_STATUS"
            };
            var specificComparison = await comparisonService.CompareWellsAsync(wellIdentifiers, specificFields);

            return comparison;
        }

        /// <summary>
        /// Example: Compare wells from different data sources
        /// Useful for comparing production vs development databases, or different companies
        /// </summary>
        public static async Task<WellComparisonDTO> CompareWellsFromMultipleSourcesExample(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata)
        {
            // Create comparison service
            var comparisonService = new WellComparisonService(
                editor, commonColumnHandler, defaults, metadata, "PPDM39");

            // Compare same well from different sources
            var wellComparisons = new List<WellSourceMapping>
            {
                new WellSourceMapping
                {
                    WellIdentifier = "UWI-001",
                    DataSource = "ProductionDB",
                    WellName = "Well-001-Prod"
                },
                new WellSourceMapping
                {
                    WellIdentifier = "UWI-001",
                    DataSource = "DevelopmentDB",
                    WellName = "Well-001-Dev"
                },
                new WellSourceMapping
                {
                    WellIdentifier = "UWI-002",
                    DataSource = "CompanyA_DB",
                    WellName = "Well-002-CompanyA"
                },
                new WellSourceMapping
                {
                    WellIdentifier = "UWI-002",
                    DataSource = "CompanyB_DB",
                    WellName = "Well-002-CompanyB"
                }
            };

            var comparison = await comparisonService.CompareWellsFromMultipleSourcesAsync(wellComparisons);

            return comparison;
        }

        /// <summary>
        /// Example: Display comparison results in a table/grid format
        /// The WellComparisonDTO is designed to be easily bound to UI controls
        /// </summary>
        public static void DisplayComparisonExample(WellComparisonDTO comparison)
        {
            Console.WriteLine($"Comparing {comparison.Metadata.WellCount} wells");
            Console.WriteLine($"Fields: {comparison.Metadata.FieldCount}");
            Console.WriteLine($"Data Sources: {string.Join(", ", comparison.Metadata.DataSources)}");
            Console.WriteLine();

            // Display as columns (one well per column)
            Console.WriteLine("Field Name".PadRight(30) + " | " + 
                string.Join(" | ", comparison.Wells.Select(w => w.WellName.PadRight(20))));
            Console.WriteLine(new string('-', 100));

            foreach (var field in comparison.ComparisonFields.OrderBy(f => f.DisplayOrder))
            {
                var fieldLabel = field.DisplayLabel.PadRight(30);
                var values = comparison.Wells.Select(w =>
                {
                    var value = w.FieldValues.ContainsKey(field.FieldName) 
                        ? w.FieldValues[field.FieldName]?.ToString() ?? "N/A" 
                        : "N/A";
                    return value.PadRight(20);
                });

                var differenceIndicator = field.HasDifferences ? " *" : "";
                Console.WriteLine($"{fieldLabel} | {string.Join(" | ", values)}{differenceIndicator}");
            }

            Console.WriteLine();
            Console.WriteLine("* = Field has differences between wells");
        }

        /// <summary>
        /// Example: Get available comparison fields
        /// Useful for building UI dropdowns or field selection
        /// </summary>
        public static async Task<List<ComparisonField>> GetAvailableFieldsExample(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata)
        {
            var comparisonService = new WellComparisonService(
                editor, commonColumnHandler, defaults, metadata, "PPDM39");

            var availableFields = await comparisonService.GetAvailableComparisonFieldsAsync();

            // Group by category
            var fieldsByCategory = availableFields
                .GroupBy(f => f.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var category in fieldsByCategory)
            {
                Console.WriteLine($"Category: {category.Key}");
                foreach (var field in category.Value)
                {
                    Console.WriteLine($"  - {field.DisplayLabel} ({field.FieldName}) - {field.DataType.Name}");
                }
            }

            return availableFields;
        }
    }
}

