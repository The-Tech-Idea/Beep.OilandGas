using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.LifeCycle.Services
{
    /// <summary>
    /// Service for comparing wells side-by-side
    /// Supports comparing wells from same or different data sources
    /// </summary>
    public class WellComparisonService : IWellComparisonService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _defaultConnectionName;

        public WellComparisonService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string defaultConnectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _defaultConnectionName = defaultConnectionName;
        }

        /// <summary>
        /// Compares multiple wells by their identifiers (UWIs)
        /// </summary>
        public async Task<WellComparisonDTO> CompareWellsAsync(
            List<string> wellIdentifiers,
            List<string> fieldNames = null,
            string connectionName = null)
        {
            if (wellIdentifiers == null || wellIdentifiers.Count == 0)
                throw new ArgumentException("At least one well identifier is required", nameof(wellIdentifiers));

            connectionName = connectionName ?? _defaultConnectionName;

            // Create well repository
            var wellRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(WELL), connectionName, "WELL");

            // Fetch all wells
            var wells = new List<WELL>();
            foreach (var identifier in wellIdentifiers)
            {
                var well = await GetWellByIdentifierAsync(wellRepo, identifier);
                if (well != null)
                {
                    wells.Add(well);
                }
            }

            if (wells.Count == 0)
                throw new InvalidOperationException("No wells found for comparison");

            // Build comparison
            return await BuildComparisonAsync(wells, fieldNames, connectionName);
        }

        /// <summary>
        /// Compares wells from different data sources
        /// </summary>
        public async Task<WellComparisonDTO> CompareWellsFromMultipleSourcesAsync(
            List<WellSourceMapping> wellComparisons,
            List<string> fieldNames = null)
        {
            if (wellComparisons == null || wellComparisons.Count == 0)
                throw new ArgumentException("At least one well comparison is required", nameof(wellComparisons));

            var wells = new List<(WELL well, string dataSource)>();

            // Fetch wells from different sources
            foreach (var mapping in wellComparisons)
            {
                var wellRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL), mapping.DataSource, "WELL");

                var well = await GetWellByIdentifierAsync(wellRepo, mapping.WellIdentifier);
                if (well != null)
                {
                    wells.Add((well, mapping.DataSource));
                }
            }

            if (wells.Count == 0)
                throw new InvalidOperationException("No wells found for comparison");

            // Build comparison with data source information
            return await BuildComparisonFromMultipleSourcesAsync(wells, fieldNames);
        }

        /// <summary>
        /// Gets available comparison fields for wells
        /// Organized by categories important for oil and gas auditing and compliance
        /// </summary>
        public async Task<List<ComparisonField>> GetAvailableComparisonFieldsAsync()
        {
            var fields = new List<ComparisonField>();
            var wellType = typeof(WELL);
            var properties = wellType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            int order = 0;

            // ===== BASIC IDENTIFICATION =====
            // Critical for well identification and cross-referencing
            AddField(fields, wellType, "UWI", "UWI (Unique Well Identifier)", "Basic Identification", order++);
            AddField(fields, wellType, "WELL_NAME", "Well Name", "Basic Identification", order++);
            AddField(fields, wellType, "WELL_NUMBER", "Well Number", "Basic Identification", order++);
            AddField(fields, wellType, "WELL_ID", "Well ID", "Basic Identification", order++);

            // ===== WELL CLASSIFICATION =====
            // Important for regulatory reporting and well type categorization
            AddField(fields, wellType, "WELL_TYPE", "Well Type", "Well Classification", order++);
            AddField(fields, wellType, "WELL_CLASS", "Well Class", "Well Classification", order++);
            AddField(fields, wellType, "CURRENT_CLASS", "Current Class", "Well Classification", order++);
            AddField(fields, wellType, "LAHEE_CLASS", "Lahee Class", "Well Classification", order++);

            // ===== LOCATION & COORDINATES =====
            // Critical for regulatory compliance, mapping, and spatial analysis
            AddField(fields, wellType, "LATITUDE", "Latitude", "Location & Coordinates", order++);
            AddField(fields, wellType, "LONGITUDE", "Longitude", "Location & Coordinates", order++);
            AddField(fields, wellType, "SURFACE_LATITUDE", "Surface Latitude", "Location & Coordinates", order++);
            AddField(fields, wellType, "SURFACE_LONGITUDE", "Surface Longitude", "Location & Coordinates", order++);
            AddField(fields, wellType, "BOTTOM_HOLE_LATITUDE", "Bottom Hole Latitude", "Location & Coordinates", order++);
            AddField(fields, wellType, "BOTTOM_HOLE_LONGITUDE", "Bottom Hole Longitude", "Location & Coordinates", order++);
            AddField(fields, wellType, "GROUND_ELEV", "Ground Elevation", "Location & Coordinates", order++);
            AddField(fields, wellType, "KB", "Kelly Bushing", "Location & Coordinates", order++);
            AddField(fields, wellType, "CASING_FLANGE_ELEV", "Casing Flange Elevation", "Location & Coordinates", order++);

            // ===== OPERATIONAL DATES =====
            // Critical for regulatory reporting, compliance, and lifecycle tracking
            AddField(fields, wellType, "SPUD_DATE", "Spud Date", "Operational Dates", order++);
            AddField(fields, wellType, "COMPLETION_DATE", "Completion Date", "Operational Dates", order++);
            AddField(fields, wellType, "PLUG_DATE", "Plug Date", "Operational Dates", order++);
            AddField(fields, wellType, "ABANDONMENT_DATE", "Abandonment Date", "Operational Dates", order++);
            AddField(fields, wellType, "FIRST_PROD_DATE", "First Production Date", "Operational Dates", order++);
            AddField(fields, wellType, "LAST_PROD_DATE", "Last Production Date", "Operational Dates", order++);

            // ===== WELL STATUS =====
            // Critical for operational status tracking and regulatory compliance
            AddField(fields, wellType, "CURRENT_STATUS", "Current Status", "Well Status", order++);
            AddField(fields, wellType, "CURRENT_STATUS_DATE", "Status Date", "Well Status", order++);
            AddField(fields, wellType, "ACTIVE_IND", "Active Indicator", "Well Status", order++);

            // ===== DEPTHS & MEASUREMENTS =====
            // Critical for well integrity, regulatory reporting, and engineering analysis
            AddField(fields, wellType, "TOTAL_DEPTH", "Total Depth", "Depths & Measurements", order++);
            AddField(fields, wellType, "TOTAL_DEPTH_OUOM", "Total Depth UOM", "Depths & Measurements", order++);
            AddField(fields, wellType, "BASE_DEPTH", "Base Depth", "Depths & Measurements", order++);
            AddField(fields, wellType, "BASE_DEPTH_OUOM", "Base Depth UOM", "Depths & Measurements", order++);
            AddField(fields, wellType, "CONFIDENTIAL_DEPTH", "Confidential Depth", "Depths & Measurements", order++);
            AddField(fields, wellType, "CONFIDENTIAL_DEPTH_OUOM", "Confidential Depth UOM", "Depths & Measurements", order++);

            // ===== OWNERSHIP & LEGAL =====
            // Critical for ownership verification, legal compliance, and financial audits
            AddField(fields, wellType, "AREA_ID", "Area ID", "Ownership & Legal", order++);
            AddField(fields, wellType, "AREA_TYPE", "Area Type", "Ownership & Legal", order++);
            AddField(fields, wellType, "ASSIGNED_FIELD", "Assigned Field", "Ownership & Legal", order++);
            AddField(fields, wellType, "BUSINESS_ASSOCIATE_ID", "Business Associate ID", "Ownership & Legal", order++);
            AddField(fields, wellType, "OPERATOR", "Operator", "Ownership & Legal", order++);
            AddField(fields, wellType, "DRILLER", "Driller", "Ownership & Legal", order++);

            // ===== REGULATORY & COMPLIANCE =====
            // Critical for regulatory audits, permit compliance, and government reporting
            AddField(fields, wellType, "PERMIT_NUMBER", "Permit Number", "Regulatory & Compliance", order++);
            AddField(fields, wellType, "PERMIT_DATE", "Permit Date", "Regulatory & Compliance", order++);
            AddField(fields, wellType, "CONFIDENTIAL_TYPE", "Confidential Type", "Regulatory & Compliance", order++);
            AddField(fields, wellType, "CONFIDENTIAL_DATE", "Confidential Date", "Regulatory & Compliance", order++);
            AddField(fields, wellType, "ROW_QUALITY", "Data Quality", "Regulatory & Compliance", order++);

            // ===== OPERATIONAL DETAILS =====
            // Important for operational audits and well performance analysis
            AddField(fields, wellType, "RIG_NAME", "Rig Name", "Operational Details", order++);
            AddField(fields, wellType, "RIG_ID", "Rig ID", "Operational Details", order++);
            AddField(fields, wellType, "DRILLING_METHOD", "Drilling Method", "Operational Details", order++);
            AddField(fields, wellType, "DRILLING_CONTRACTOR", "Drilling Contractor", "Operational Details", order++);

            // ===== STRATIGRAPHIC INFORMATION =====
            // Important for geological analysis and reservoir characterization
            AddField(fields, wellType, "BASE_STRAT_UNIT_ID", "Base Stratigraphic Unit ID", "Stratigraphic Information", order++);
            AddField(fields, wellType, "BASE_STRAT_NAME_SET_ID", "Base Stratigraphic Name Set ID", "Stratigraphic Information", order++);
            AddField(fields, wellType, "BASE_NODE_ID", "Base Node ID", "Stratigraphic Information", order++);

            // ===== DATA QUALITY & SOURCE =====
            // Critical for data quality audits and source verification
            AddField(fields, wellType, "SOURCE", "Source", "Data Quality & Source", order++);
            AddField(fields, wellType, "SOURCE_DOCUMENT_ID", "Source Document ID", "Data Quality & Source", order++);
            AddField(fields, wellType, "REMARK", "Remark", "Data Quality & Source", order++);

            // ===== AUDIT TRAIL =====
            // Critical for change tracking and audit compliance
            AddField(fields, wellType, "ROW_CREATED_BY", "Created By", "Audit Trail", order++);
            AddField(fields, wellType, "ROW_CREATED_DATE", "Created Date", "Audit Trail", order++);
            AddField(fields, wellType, "ROW_CHANGED_BY", "Changed By", "Audit Trail", order++);
            AddField(fields, wellType, "ROW_CHANGED_DATE", "Changed Date", "Audit Trail", order++);
            AddField(fields, wellType, "ROW_EFFECTIVE_DATE", "Effective Date", "Audit Trail", order++);
            AddField(fields, wellType, "ROW_EXPIRY_DATE", "Expiry Date", "Audit Trail", order++);
            AddField(fields, wellType, "PPDM_GUID", "PPDM GUID", "Audit Trail", order++);

            return fields;
        }

        /// <summary>
        /// Gets a well by identifier (UWI or Well ID)
        /// </summary>
        private async Task<WELL> GetWellByIdentifierAsync(PPDMGenericRepository wellRepo, string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return null;

            // Try to get by UWI first
            var filters = new List<TheTechIdea.Beep.Report.AppFilter>
            {
                new TheTechIdea.Beep.Report.AppFilter { FieldName = "UWI", FilterValue = identifier, Operator = "=" }
            };

            var results = await wellRepo.GetAsync(filters);
            var well = results?.FirstOrDefault() as WELL;

            if (well != null)
                return well;

            // If not found by UWI, try by Well ID if identifier is numeric
            // Get primary key from metadata
            var metadata = await _metadata.GetTableMetadataAsync("WELL");
            if (metadata != null)
            {
                var pkColumn = metadata.PrimaryKeyColumn.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrWhiteSpace(pkColumn) && int.TryParse(identifier, out int wellId))
                {
                    well = await wellRepo.GetByIdAsync(wellId) as WELL;
                }
            }

            return well;
        }

        /// <summary>
        /// Builds comparison DTO from list of wells
        /// </summary>
        private async Task<WellComparisonDTO> BuildComparisonAsync(
            List<WELL> wells,
            List<string> fieldNames,
            string connectionName)
        {
            var comparison = new WellComparisonDTO();

            // Get available fields
            var availableFields = await GetAvailableComparisonFieldsAsync();

            // Filter to requested fields if specified
            if (fieldNames != null && fieldNames.Count > 0)
            {
                availableFields = availableFields
                    .Where(f => fieldNames.Contains(f.FieldName, StringComparer.OrdinalIgnoreCase))
                    .ToList();
            }

            // Build comparison items for each well
            foreach (var well in wells)
            {
                // Get primary key value using reflection
                var wellId = GetPrimaryKeyValue(well);

                var item = new WellComparisonItem
                {
                    WellIdentifier = well.UWI ?? wellId?.ToString(),
                    WellName = well.WELL_NAME,
                    DataSource = connectionName,
                    FieldValues = new Dictionary<string, object>(),
                    Metadata = new Dictionary<string, object>
                    {
                        { "WellId", wellId },
                        { "UWI", well.UWI }
                    }
                };

                // Extract field values
                foreach (var field in availableFields)
                {
                    var property = typeof(WELL).GetProperty(field.FieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property != null)
                    {
                        var value = property.GetValue(well);
                        item.FieldValues[field.FieldName] = value;
                    }
                }

                comparison.Wells.Add(item);
            }

            // Set comparison fields and detect differences
            comparison.ComparisonFields = availableFields;
            DetectDifferences(comparison);

            // Set metadata
            comparison.Metadata = new ComparisonMetadata
            {
                ComparisonDate = DateTime.Now,
                WellCount = wells.Count,
                FieldCount = availableFields.Count,
                DataSources = new List<string> { connectionName }
            };

            return comparison;
        }

        /// <summary>
        /// Builds comparison DTO from wells from multiple sources
        /// </summary>
        private async Task<WellComparisonDTO> BuildComparisonFromMultipleSourcesAsync(
            List<(WELL well, string dataSource)> wells,
            List<string> fieldNames)
        {
            var comparison = new WellComparisonDTO();

            // Get available fields
            var availableFields = await GetAvailableComparisonFieldsAsync();

            // Filter to requested fields if specified
            if (fieldNames != null && fieldNames.Count > 0)
            {
                availableFields = availableFields
                    .Where(f => fieldNames.Contains(f.FieldName, StringComparer.OrdinalIgnoreCase))
                    .ToList();
            }

            // Build comparison items for each well
            foreach (var (well, dataSource) in wells)
            {
                // Get primary key value using reflection
                var wellId = GetPrimaryKeyValue(well);

                var item = new WellComparisonItem
                {
                    WellIdentifier = well.UWI ?? wellId?.ToString(),
                    WellName = well.WELL_NAME,
                    DataSource = dataSource,
                    FieldValues = new Dictionary<string, object>(),
                    Metadata = new Dictionary<string, object>
                    {
                        { "WellId", wellId },
                        { "UWI", well.UWI }
                    }
                };

                // Extract field values
                foreach (var field in availableFields)
                {
                    var property = typeof(WELL).GetProperty(field.FieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property != null)
                    {
                        var value = property.GetValue(well);
                        item.FieldValues[field.FieldName] = value;
                    }
                }

                comparison.Wells.Add(item);
            }

            // Set comparison fields and detect differences
            comparison.ComparisonFields = availableFields;
            DetectDifferences(comparison);

            // Set metadata
            comparison.Metadata = new ComparisonMetadata
            {
                ComparisonDate = DateTime.Now,
                WellCount = wells.Count,
                FieldCount = availableFields.Count,
                DataSources = wells.Select(w => w.dataSource).Distinct().ToList()
            };

            return comparison;
        }

        /// <summary>
        /// Detects differences between wells for each field
        /// </summary>
        private void DetectDifferences(WellComparisonDTO comparison)
        {
            if (comparison.Wells.Count < 2)
                return;

            foreach (var field in comparison.ComparisonFields)
            {
                var values = comparison.Wells
                    .Select(w => w.FieldValues.ContainsKey(field.FieldName) ? w.FieldValues[field.FieldName] : null)
                    .ToList();

                // Check if all values are the same
                var firstValue = values.FirstOrDefault();
                field.HasDifferences = !values.All(v => 
                    (v == null && firstValue == null) || 
                    (v != null && v.Equals(firstValue)));
            }
        }

        /// <summary>
        /// Helper to add a comparison field
        /// </summary>
        private void AddField(
            List<ComparisonField> fields,
            Type entityType,
            string propertyName,
            string displayLabel,
            string category,
            int order)
        {
            var property = entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                fields.Add(new ComparisonField
                {
                    FieldName = propertyName,
                    DisplayLabel = displayLabel,
                    DataType = property.PropertyType,
                    Category = category,
                    DisplayOrder = order
                });
            }
        }

        /// <summary>
        /// Gets primary key value from well entity using metadata (synchronous version)
        /// </summary>
        private object GetPrimaryKeyValue(WELL well)
        {
            var metadata = _metadata.GetTableMetadataAsync("WELL").GetAwaiter().GetResult();
            if (metadata == null)
                return null;

            var pkColumns = metadata.PrimaryKeyColumn.Split(',').Select(c => c.Trim()).ToList();
            if (pkColumns.Count == 0)
                return null;

            // Get first primary key column value
            var firstPkColumn = pkColumns[0];
            var property = typeof(WELL).GetProperty(firstPkColumn, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                return property.GetValue(well);
            }

            return null;
        }
    }
}

