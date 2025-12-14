using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core.Metadata;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Tool to update PPDM39Metadata.json with correct SubjectArea and Module mappings
    /// based on PPDM 3.9 Roadmaps
    /// </summary>
    public class UpdateMetadataMappings
    {
        /// <summary>
        /// Maps table names to their correct SubjectArea and Module based on PPDM 3.9 Roadmaps
        /// </summary>
        private static Dictionary<string, (string SubjectArea, string Module)> GetTableMappings()
        {
            var mappings = new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);

            // ============================================
            // SUPPORT MODULES
            // ============================================
            
            // Areas
            AddMapping(mappings, "AREA", "Support Modules", "Areas");
            AddMappingPattern(mappings, "AREA_", "Support Modules", "Areas");
            
            // Spatial Locations
            AddMapping(mappings, "SPATIAL_DESCRIPTION", "Support Modules", "Spatial Locations");
            AddMappingPattern(mappings, "SP_COMPONENT", "Support Modules", "Spatial Locations");
            AddMappingPattern(mappings, "SP_DESC", "Support Modules", "Spatial Locations");
            AddMappingPattern(mappings, "SP_LINE", "Support Modules", "Spatial Locations");
            AddMappingPattern(mappings, "SP_ZONE", "Support Modules", "Spatial Locations");
            AddMappingPattern(mappings, "SP_POINT", "Support Modules", "Spatial Locations");
            AddMappingPattern(mappings, "SP_POLYGON", "Support Modules", "Spatial Locations");
            AddMappingPattern(mappings, "SP_BOUNDARY", "Support Modules", "Spatial Locations");
            AddMapping(mappings, "Z_SP_GEOMETRY", "Support Modules", "Spatial Locations");
            
            // Spatial Parcels
            AddMappingPattern(mappings, "SP_PARCEL", "Support Modules", "Spatial Parcels");
            
            // Entitlements
            AddMapping(mappings, "ENTITLEMENT", "Support Modules", "Entitlements");
            AddMappingPattern(mappings, "ENT_", "Support Modules", "Entitlements");
            
            // Finances
            AddMapping(mappings, "FINANCE", "Support Modules", "Finances");
            AddMappingPattern(mappings, "FIN_", "Support Modules", "Finances");
            
            // Rate Schedules
            AddMapping(mappings, "RATE_SCHEDULE", "Support Modules", "Rate Schedules");
            AddMappingPattern(mappings, "RATE_", "Support Modules", "Rate Schedules");
            
            // Source Document Bibliography
            AddMapping(mappings, "SOURCE_DOCUMENT", "Support Modules", "Source Document Bibliography");
            AddMappingPattern(mappings, "SOURCE_DOC_", "Support Modules", "Source Document Bibliography");
            
            // Additives
            AddMappingPattern(mappings, "CAT_ADDITIVE", "Support Modules", "Additives");
            
            // Equipment
            AddMapping(mappings, "EQUIPMENT", "Support Modules", "Equipment");
            AddMappingPattern(mappings, "EQUIPMENT_", "Support Modules", "Equipment");
            AddMappingPattern(mappings, "CAT_EQUIP", "Support Modules", "Equipment");
            AddMappingPattern(mappings, "EQUIPMENT_MAINTAIN", "Support Modules", "Equipment");
            AddMappingPattern(mappings, "EQUIPMENT_MAINT", "Support Modules", "Equipment");
            AddMappingPattern(mappings, "EQUIPMENT_SPEC", "Support Modules", "Equipment");
            
            // Coordinate Systems
            AddMappingPattern(mappings, "CS_", "Support Modules", "Coordinate Systems");
            
            // Business Associates
            AddMapping(mappings, "BUSINESS_ASSOCIATE", "Support Modules", "Business Associates");
            AddMappingPattern(mappings, "BA_", "Support Modules", "Business Associates");
            
            // Products and Substances
            AddMapping(mappings, "SUBSTANCE", "Support Modules", "Products and Substances");
            AddMappingPattern(mappings, "SUBSTANCE_", "Support Modules", "Products and Substances");
            AddMapping(mappings, "Z_PRODUCT", "Support Modules", "Products and Substances");
            AddMappingPattern(mappings, "Z_PRODUCT_", "Support Modules", "Products and Substances");
            
            // Projects
            AddMapping(mappings, "PROJECT", "Support Modules", "Projects");
            AddMappingPattern(mappings, "PROJECT_", "Support Modules", "Projects");
            
            // Work Orders
            AddMapping(mappings, "WORK_ORDER", "Support Modules", "Work Orders");
            AddMappingPattern(mappings, "WORK_ORDER_", "Support Modules", "Work Orders");

            // ============================================
            // DATA MANAGEMENT & UNITS OF MEASURE
            // ============================================
            
            // Data Management
            AddMappingPattern(mappings, "PPDM_SYSTEM", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_TABLE", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_PROCEDURE", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_METRIC", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_PROPERTY", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_AUDIT", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_QUALITY", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_MAP", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_RULE", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_GROUP", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_CONSTRAINT", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_COLUMN", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_SCHEMA", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_CODE", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_SW_", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_CHECK", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_DOMAIN", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_QUANTITY", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_EXCEPTION", "Data Management & Units of Measure", "Data Management");
            AddMappingPattern(mappings, "PPDM_OBJECT", "Data Management & Units of Measure", "Data Management");
            
            // Unit of Measure
            AddMapping(mappings, "PPDM_MEASUREMENT_SYSTEM", "Data Management & Units of Measure", "Unit of Measure");
            AddMapping(mappings, "PPDM_UNIT_OF_MEASURE", "Data Management & Units of Measure", "Unit of Measure");
            AddMappingPattern(mappings, "PPDM_UOM_", "Data Management & Units of Measure", "Unit of Measure");
            AddMappingPattern(mappings, "PPDM_UNIT_CONVERSION", "Data Management & Units of Measure", "Unit of Measure");
            AddMapping(mappings, "PPDM_DATA_STORE", "Data Management & Units of Measure", "Unit of Measure");
            
            // Volume Conversions
            AddMappingPattern(mappings, "PPDM_VOL_MEAS_", "Data Management & Units of Measure", "Volume Conversions");
            
            // Reference Table Management
            AddMappingPattern(mappings, "R_", "Data Management & Units of Measure", "Reference Table Management");
            AddMappingPattern(mappings, "RA_", "Data Management & Units of Measure", "Reference Table Management");

            // ============================================
            // STRATIGRAPHY, LITHOLOGY & SAMPLE ANALYSIS
            // ============================================
            
            // Lithology
            AddMapping(mappings, "LITH", "Stratigraphy, Lithology & Sample Analysis", "Lithology");
            AddMappingPattern(mappings, "LITH_", "Stratigraphy, Lithology & Sample Analysis", "Lithology");
            
            // Ecozones
            AddMapping(mappings, "ECOZONE", "Stratigraphy, Lithology & Sample Analysis", "Ecozones and Environments");
            AddMappingPattern(mappings, "ECOZONE_", "Stratigraphy, Lithology & Sample Analysis", "Ecozones and Environments");
            AddMapping(mappings, "PALEO_CLIMATE", "Stratigraphy, Lithology & Sample Analysis", "Ecozones and Environments");
            
            // Paleontology
            AddMapping(mappings, "FOSSIL", "Stratigraphy, Lithology & Sample Analysis", "Paleontology");
            AddMappingPattern(mappings, "FOSSIL_", "Stratigraphy, Lithology & Sample Analysis", "Paleontology");
            
            // Interpretation
            AddMappingPattern(mappings, "PALEO_", "Stratigraphy, Lithology & Sample Analysis", "Interpretation");
            
            // Sample Management
            AddMapping(mappings, "SAMPLE", "Stratigraphy, Lithology & Sample Analysis", "Sample Management");
            AddMappingPattern(mappings, "SAMPLE_", "Stratigraphy, Lithology & Sample Analysis", "Sample Management");
            
            // Sample Analysis
            AddMapping(mappings, "ANL", "Stratigraphy, Lithology & Sample Analysis", "Sample Analysis");
            AddMappingPattern(mappings, "ANL_", "Stratigraphy, Lithology & Sample Analysis", "Sample Analysis");
            
            // Stratigraphy
            AddMapping(mappings, "STRAT", "Stratigraphy, Lithology & Sample Analysis", "Stratigraphy");
            AddMappingPattern(mappings, "STRAT_", "Stratigraphy, Lithology & Sample Analysis", "Stratigraphy");

            // ============================================
            // PRODUCTION & RESERVES
            // ============================================
            
            // Fields
            AddMapping(mappings, "FIELD", "Production & Reserves", "Fields");
            AddMappingPattern(mappings, "FIELD_", "Production & Reserves", "Fields");
            
            // Pools
            AddMapping(mappings, "POOL", "Production & Reserves", "Pools");
            AddMappingPattern(mappings, "POOL_", "Production & Reserves", "Pools");
            
            // Production Reporting (PDEN)
            AddMapping(mappings, "PDEN", "Production & Reserves", "Production Reporting");
            AddMappingPattern(mappings, "PDEN_", "Production & Reserves", "Production Reporting");
            
            // Production Strings
            AddMapping(mappings, "PROD_STRING", "Production & Reserves", "Production Strings");
            AddMappingPattern(mappings, "PROD_STRING_", "Production & Reserves", "Production Strings");
            AddMappingPattern(mappings, "PR_STR_", "Production & Reserves", "Production Strings");
            
            // Production Lease Units
            AddMapping(mappings, "PROD_LEASE_UNIT", "Production & Reserves", "Production Lease Units");
            AddMappingPattern(mappings, "PROD_LEASE_UNIT_", "Production & Reserves", "Production Lease Units");
            AddMappingPattern(mappings, "PR_LSE_UNIT_", "Production & Reserves", "Production Lease Units");
            
            // Spacing Units
            AddMapping(mappings, "SPACING_UNIT", "Production & Reserves", "Spacing Units");
            AddMappingPattern(mappings, "SPACING_UNIT_", "Production & Reserves", "Spacing Units");
            
            // Production Facilities
            AddMapping(mappings, "FACILITY", "Production & Reserves", "Production Facilities");
            AddMappingPattern(mappings, "FACILITY_", "Production & Reserves", "Production Facilities");
            
            // Reporting Hierarchies
            AddMapping(mappings, "REPORT_HIER", "Production & Reserves", "Reporting Hierarchies");
            AddMappingPattern(mappings, "REPORT_HIER_", "Production & Reserves", "Reporting Hierarchies");
            
            // Reserves Reporting
            AddMapping(mappings, "RESERVE_CLASS", "Production & Reserves", "Reserves Reporting");
            AddMapping(mappings, "RESERVE_ENTITY", "Production & Reserves", "Reserves Reporting");
            AddMappingPattern(mappings, "RESERVE_", "Production & Reserves", "Reserves Reporting");
            AddMappingPattern(mappings, "RESENT_", "Production & Reserves", "Reserves Reporting");

            // ============================================
            // WELLS
            // ============================================
            
            // Well Core
            AddMapping(mappings, "WELL", "Wells", "Wells");
            AddMappingPattern(mappings, "WELL_", "Wells", "Wells");
            AddMapping(mappings, "WELLBORE", "Wells", "Wells");
            
            // Well Logs
            AddMappingPattern(mappings, "WELL_LOG", "Wells", "Well Logs");
            AddMappingPattern(mappings, "WELL_MUD", "Wells", "Well Logs");
            
            // Legal Locations
            AddMappingPattern(mappings, "LEGAL_", "Wells", "Legal Locations");

            // ============================================
            // PRODUCT MANAGEMENT & CLASSIFICATIONS
            // ============================================
            
            // Classification Systems
            AddMapping(mappings, "CLASS_SYSTEM", "Product Management & Classifications", "Classification Systems");
            AddMappingPattern(mappings, "CLASS_", "Product Management & Classifications", "Classification Systems");
            
            // Product and Information Management
            AddMapping(mappings, "RM_INFORMATION_ITEM", "Product Management & Classifications", "Product and Information Management");
            AddMappingPattern(mappings, "RM_", "Product Management & Classifications", "Product and Information Management");

            // ============================================
            // SEISMIC
            // ============================================
            
            AddMapping(mappings, "SEIS_SET", "Seismic", "Seismic");
            AddMappingPattern(mappings, "SEIS_", "Seismic", "Seismic");

            // ============================================
            // SUPPORT FACILITIES
            // ============================================
            
            AddMapping(mappings, "SF_SUPPORT_FACILITY", "Support Facilities", "Support Facilities");
            AddMappingPattern(mappings, "SF_", "Support Facilities", "Support Facilities");

            // ============================================
            // OPERATIONS SUPPORT
            // ============================================
            
            // Applications
            AddMapping(mappings, "APPLICATION", "Operations Support", "Applications");
            AddMappingPattern(mappings, "APPLIC_", "Operations Support", "Applications");
            
            // Consultations
            AddMapping(mappings, "CONSULT", "Operations Support", "Consultations");
            AddMappingPattern(mappings, "CONSULT_", "Operations Support", "Consultations");
            
            // Consents
            AddMapping(mappings, "CONSENT", "Operations Support", "Consents");
            AddMappingPattern(mappings, "CONSENT_", "Operations Support", "Consents");
            
            // Notifications
            AddMapping(mappings, "NOTIFICATION", "Operations Support", "Notifications");
            AddMappingPattern(mappings, "NOTIF_", "Operations Support", "Notifications");
            
            // Contests
            AddMapping(mappings, "CONTEST", "Operations Support", "Contests");
            AddMappingPattern(mappings, "CONTEST_", "Operations Support", "Contests");
            
            // Disputes
            AddMapping(mappings, "DISPUTE", "Operations Support", "Disputes");
            AddMappingPattern(mappings, "DISPUTE_", "Operations Support", "Disputes");
            
            // Negotiations
            AddMapping(mappings, "NEGOTIATION", "Operations Support", "Negotiations");
            AddMappingPattern(mappings, "NEGOTIATION_", "Operations Support", "Negotiations");
            
            // Health Safety & Environment
            AddMappingPattern(mappings, "HSE_", "Operations Support", "Health Safety & Environment");

            // ============================================
            // LAND & LEGAL MANAGEMENT
            // ============================================
            
            // Land
            AddMapping(mappings, "LAND", "Land & Legal Management", "Land");
            AddMappingPattern(mappings, "LAND_", "Land & Legal Management", "Land");
            
            // Land Rights
            AddMapping(mappings, "LAND_RIGHT", "Land & Legal Management", "Land Rights");
            
            // Contracts
            AddMapping(mappings, "CONTRACT", "Land & Legal Management", "Contracts");
            AddMappingPattern(mappings, "CONT_", "Land & Legal Management", "Contracts");
            
            // Obligations
            AddMapping(mappings, "OBLIGATION", "Land & Legal Management", "Obligations");
            AddMappingPattern(mappings, "OBLIG_", "Land & Legal Management", "Obligations");
            
            // Instruments
            AddMapping(mappings, "INSTRUMENT", "Land & Legal Management", "Instruments");
            AddMappingPattern(mappings, "INSTRUMENT_", "Land & Legal Management", "Instruments");
            
            // Interest Sets
            AddMapping(mappings, "INTEREST_SET", "Land & Legal Management", "Interest Sets");
            AddMappingPattern(mappings, "INT_SET_", "Land & Legal Management", "Interest Sets");
            
            // Restrictions
            AddMapping(mappings, "RESTRICTION", "Land & Legal Management", "Restrictions");
            AddMappingPattern(mappings, "REST_", "Land & Legal Management", "Restrictions");

            return mappings;
        }

        private static void AddMapping(Dictionary<string, (string, string)> mappings, string tableName, string subjectArea, string module)
        {
            mappings[tableName] = (subjectArea, module);
        }

        private static void AddMappingPattern(Dictionary<string, (string, string)> mappings, string pattern, string subjectArea, string module)
        {
            // Store pattern for later matching
            if (!mappings.ContainsKey($"PATTERN:{pattern}"))
            {
                mappings[$"PATTERN:{pattern}"] = (subjectArea, module);
            }
        }

        private static (string SubjectArea, string Module) GetMappingForTable(string tableName, Dictionary<string, (string, string)> mappings)
        {
            // First try exact match
            if (mappings.TryGetValue(tableName, out var exactMatch))
            {
                return exactMatch;
            }

            // Then try pattern matches
            var upperTableName = tableName.ToUpperInvariant();
            foreach (var kvp in mappings)
            {
                if (kvp.Key.StartsWith("PATTERN:"))
                {
                    var pattern = kvp.Key.Substring(8); // Remove "PATTERN:" prefix
                    if (upperTableName.StartsWith(pattern) || upperTableName.Contains(pattern))
                    {
                        return kvp.Value;
                    }
                }
            }

            // Default fallback
            return ("Support Modules", "General");
        }

        /// <summary>
        /// Updates the metadata JSON file with correct SubjectArea and Module mappings
        /// </summary>
        public static void UpdateMetadataFile(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException($"JSON metadata file not found: {jsonFilePath}");

            Console.WriteLine($"Loading metadata from: {jsonFilePath}");
            var json = File.ReadAllText(jsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            var metadata = JsonSerializer.Deserialize<Dictionary<string, PPDMTableMetadata>>(json, options);
            if (metadata == null)
                throw new InvalidOperationException("Failed to deserialize metadata JSON");

            Console.WriteLine($"Loaded {metadata.Count} tables");

            var mappings = GetTableMappings();
            Console.WriteLine($"Created {mappings.Count} mappings");

            int updated = 0;
            foreach (var kvp in metadata)
            {
                var tableName = kvp.Key;
                var tableMeta = kvp.Value;

                var (subjectArea, module) = GetMappingForTable(tableName, mappings);
                
                if (tableMeta.SubjectArea != subjectArea || tableMeta.Module != module)
                {
                    var oldSubjectArea = tableMeta.SubjectArea;
                    var oldModule = tableMeta.Module;
                    
                    tableMeta.SubjectArea = subjectArea;
                    tableMeta.Module = module;
                    if (string.IsNullOrWhiteSpace(tableMeta.OriginalModule))
                    {
                        tableMeta.OriginalModule = oldModule;
                    }

                    updated++;
                    if (updated % 100 == 0)
                    {
                        Console.WriteLine($"Updated {updated} tables...");
                    }
                }
            }

            Console.WriteLine($"Updated {updated} tables with new SubjectArea/Module mappings");

            // Save updated metadata
            Console.WriteLine("Saving updated metadata...");
            var updatedJson = JsonSerializer.Serialize(metadata, options);
            File.WriteAllText(jsonFilePath, updatedJson);
            Console.WriteLine("Metadata file updated successfully!");
        }
    }
}

