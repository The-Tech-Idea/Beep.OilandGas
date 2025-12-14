using System.Reflection;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service to provide metadata about PPDM39 entities including API endpoints and display names
    /// </summary>
    public class EntityMetadataService
    {
        private readonly Dictionary<string, EntityMetadata> _entityMetadata = new();

        public EntityMetadataService()
        {
            InitializeMetadata();
        }

        private void InitializeMetadata()
        {
            // Stratigraphy entities
            RegisterEntity("STRAT_COLUMN", "Stratigraphic Columns", "stratigraphy/columns", typeof(STRAT_COLUMN), "STRAT_COLUMN_ID", "STRAT_COLUMN_NAME");
            RegisterEntity("STRAT_UNIT", "Stratigraphic Units", "stratigraphy/units", typeof(STRAT_UNIT), "STRAT_UNIT_ID", "SHORT_NAME", "LONG_NAME");
            RegisterEntity("STRAT_HIERARCHY", "Stratigraphic Hierarchy", "stratigraphy/hierarchy", typeof(STRAT_HIERARCHY), "HIERARCHY_ID");
            RegisterEntity("STRAT_WELL_SECTION", "Well Sections", "stratigraphy/well-sections", typeof(STRAT_WELL_SECTION), "STRAT_WELL_SECTION_ID");

            // Wells entities
            RegisterEntity("WELL", "Wells", "well", typeof(WELL), "UWI", "WELL_NAME");
            RegisterEntity("WELL_STATUS", "Well Status", "well/status", typeof(WELL_STATUS), "UWI", "STATUS");

            // Production entities
            RegisterEntity("FIELD", "Fields", "production/fields", typeof(object), "FIELD_ID", "FIELD_NAME");
            RegisterEntity("POOL", "Pools", "production/pools", typeof(object), "POOL_ID", "POOL_NAME");

            // Seismic entities
            RegisterEntity("SURVEY", "Surveys", "seismic/surveys", typeof(object), "SURVEY_ID", "SURVEY_NAME");
            RegisterEntity("ACQUISITION", "Acquisition", "seismic/acquisition", typeof(object), "ACQUISITION_ID");
            RegisterEntity("PROCESSING", "Processing", "seismic/processing", typeof(object), "PROCESSING_ID");

            // Areas
            RegisterEntity("AREA", "Areas", "area", typeof(AREA), "AREA_ID", "AREA_NAME");

            // Business Associates
            RegisterEntity("BUSINESS_ASSOCIATE", "Business Associates", "business-associate", typeof(BUSINESS_ASSOCIATE), "BA_ID", "BA_NAME");

            // Equipment
            RegisterEntity("EQUIPMENT", "Equipment", "equipment", typeof(EQUIPMENT), "EQUIPMENT_ID", "EQUIPMENT_NAME");
            RegisterEntity("CAT_EQUIPMENT", "Catalog Equipment", "equipment/catalog", typeof(CAT_EQUIPMENT), "CAT_EQUIPMENT_ID", "CAT_EQUIPMENT_NAME");
        }

        private void RegisterEntity(string entityKey, string displayName, string apiEndpoint, Type entityType, string idField, params string[] nameFields)
        {
            _entityMetadata[entityKey] = new EntityMetadata
            {
                EntityKey = entityKey,
                DisplayName = displayName,
                ApiEndpoint = apiEndpoint,
                EntityType = entityType,
                IdField = idField,
                NameFields = nameFields.ToList()
            };
        }

        public EntityMetadata? GetMetadata(string entityKey)
        {
            return _entityMetadata.TryGetValue(entityKey, out var metadata) ? metadata : null;
        }

        public List<EntityMetadata> GetAllMetadata()
        {
            return _entityMetadata.Values.OrderBy(e => e.DisplayName).ToList();
        }

        public List<EntityMetadata> GetMetadataByCategory(string category)
        {
            return _entityMetadata.Values
                .Where(e => e.Category == category)
                .OrderBy(e => e.DisplayName)
                .ToList();
        }

        public string? GetDisplayName(string entityKey)
        {
            return GetMetadata(entityKey)?.DisplayName;
        }

        public string? GetApiEndpoint(string entityKey)
        {
            return GetMetadata(entityKey)?.ApiEndpoint;
        }
    }

    public class EntityMetadata
    {
        public string EntityKey { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ApiEndpoint { get; set; } = string.Empty;
        public Type EntityType { get; set; } = typeof(object);
        public string IdField { get; set; } = string.Empty;
        public List<string> NameFields { get; set; } = new();
        public string Category { get; set; } = string.Empty;
    }

    public class TreeNode
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? EntityKey { get; set; }
        public List<TreeNode> Children { get; set; } = new();
        public bool Expanded { get; set; } = false;
    }
}
