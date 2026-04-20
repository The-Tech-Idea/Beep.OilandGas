using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 schema metadata: categories, modules, table lists.
    /// Used by the Data Management pages and the database creation wizard.
    /// </summary>
    [ApiController]
    [Route("api/ppdm39/metadata")]
    public class PPDM39MetadataController : ControllerBase
    {
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PPDM39MetadataController> _logger;

        // Predefined top-level categories shown on the Data Management hub
        private static readonly List<DataCategory> _categories = new()
        {
            new DataCategory
            {
                Id          = "WELL",
                Name        = "Wells",
                Description = "Well life cycle: wellbores, logs, tests, status and production strings",
                Icon        = "oil_barrel",
                Prefixes    = new List<string> { "WELL" },
                ModuleHint  = "Wells",
                Color       = "primary"
            },
            new DataCategory
            {
                Id          = "BA",
                Name        = "Business Associates",
                Description = "Companies, people and organisations you do business with",
                Icon        = "business",
                Prefixes    = new List<string> { "BUSINESS_ASSOCIATE", "BA_" },
                ModuleHint  = "Business Associates",
                Color       = "secondary"
            },
            new DataCategory
            {
                Id          = "REFERENCE",
                Name        = "Reference Data",
                Description = "Standard reference / look-up values (R_ and RA_ tables)",
                Icon        = "list_alt",
                Prefixes    = new List<string> { "R_", "RA_" },
                ModuleHint  = "Reference Table Management",
                Color       = "tertiary"
            },
            new DataCategory
            {
                Id          = "PRODUCTION",
                Name        = "Production & Reserves",
                Description = "Field volumes, pools, reserves forecasts and production reporting",
                Icon        = "speed",
                Prefixes    = new List<string> { "PDEN", "POOL", "FIELD" },
                ModuleHint  = "Production & Reserves",
                Color       = "success"
            },
            new DataCategory
            {
                Id          = "EXPLORATION",
                Name        = "Exploration",
                Description = "Prospects, seismic surveys, plays and basins",
                Icon        = "explore",
                Prefixes    = new List<string> { "PROSPECT", "SEIS_", "PLAY" },
                ModuleHint  = "Seismic",
                Color       = "info"
            },
            new DataCategory
            {
                Id          = "FACILITY",
                Name        = "Facilities",
                Description = "Pipelines, tanks, batteries and surface facilities",
                Icon        = "factory",
                Prefixes    = new List<string> { "FACILITY" },
                ModuleHint  = "Production-Related Facilities",
                Color       = "warning"
            },
            new DataCategory
            {
                Id          = "AREA",
                Name        = "Areas & Locations",
                Description = "Geographic areas, jurisdictions, legal locations and parcels",
                Icon        = "map",
                Prefixes    = new List<string> { "AREA", "LAND_", "STRAT_" },
                ModuleHint  = "Areas",
                Color       = "default"
            },
            new DataCategory
            {
                Id          = "SUPPORT",
                Name        = "Support & Equipment",
                Description = "Equipment, rigs, contracts, work orders and projects",
                Icon        = "build",
                Prefixes    = new List<string> { "EQUIP", "CONTRACT", "PROJECT", "WORK_ORDER" },
                ModuleHint  = "Work Orders and Requests",
                Color       = "default"
            }
        };

        public PPDM39MetadataController(IPPDMMetadataRepository metadata, ILogger<PPDM39MetadataController> logger)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger   = logger   ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Returns all predefined data categories with table lists resolved from metadata.
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<List<DataCategoryWithTables>>> GetCategories()
        {
            try
            {
                var all = await _metadata.GetAllMetadataAsync();
                var tableNames = all.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

                var result = _categories.Select(cat =>
                {
                    var tables = tableNames
                        .Where(t => cat.Prefixes.Any(p =>
                            t.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                        .OrderBy(t => t)
                        .ToList();

                    return new DataCategoryWithTables
                    {
                        Id          = cat.Id,
                        Name        = cat.Name,
                        Description = cat.Description,
                        Icon        = cat.Icon,
                        Color       = cat.Color,
                        Tables      = tables,
                        TableCount  = tables.Count
                    };
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data categories");
                return StatusCode(500, new List<DataCategoryWithTables>());
            }
        }

        /// <summary>
        /// Returns all table names for a specific category ID (e.g. WELL, BA, REFERENCE).
        /// </summary>
        [HttpGet("categories/{categoryId}/tables")]
        public async Task<ActionResult<List<TableInfo>>> GetCategoryTables(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId)) return BadRequest(new { error = "Category ID is required." });
            try
            {
                var cat = _categories.FirstOrDefault(c =>
                    c.Id.Equals(categoryId, StringComparison.OrdinalIgnoreCase));

                if (cat == null)
                    return NotFound(new List<TableInfo>());

                var all = await _metadata.GetAllMetadataAsync();

                var tables = all
                    .Where(kvp => cat.Prefixes.Any(p =>
                        kvp.Key.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                    .Select(kvp => new TableInfo
                    {
                        TableName      = kvp.Key,
                        Description    = kvp.Value.Module ?? string.Empty,
                        PrimaryKey     = kvp.Value.PrimaryKeyColumn ?? string.Empty,
                        IsReferenceTable = kvp.Key.StartsWith("R_", StringComparison.OrdinalIgnoreCase)
                                        || kvp.Key.StartsWith("RA_", StringComparison.OrdinalIgnoreCase)
                    })
                    .OrderBy(t => t.TableName)
                    .ToList();

                return Ok(tables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tables for category {CategoryId}", categoryId);
                return StatusCode(500, new List<TableInfo>());
            }
        }

        /// <summary>
        /// Returns tables whose names start with the given prefix (case-insensitive).
        /// </summary>
        [HttpGet("tables/by-prefix/{prefix}")]
        public async Task<ActionResult<List<TableInfo>>> GetTablesByPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return BadRequest(new { error = "Prefix is required." });
            try
            {
                var all = await _metadata.GetAllMetadataAsync();

                var tables = all
                    .Where(kvp => kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .Select(kvp => new TableInfo
                    {
                        TableName      = kvp.Key,
                        Description    = kvp.Value.Module ?? string.Empty,
                        PrimaryKey     = kvp.Value.PrimaryKeyColumn ?? string.Empty,
                        IsReferenceTable = kvp.Key.StartsWith("R_", StringComparison.OrdinalIgnoreCase)
                                        || kvp.Key.StartsWith("RA_", StringComparison.OrdinalIgnoreCase)
                    })
                    .OrderBy(t => t.TableName)
                    .ToList();

                return Ok(tables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tables by prefix {Prefix}", prefix);
                return StatusCode(500, new List<TableInfo>());
            }
        }

        /// <summary>
        /// Returns tables for the given module name.
        /// </summary>
        [HttpGet("tables/by-module/{module}")]
        public async Task<ActionResult<List<TableInfo>>> GetTablesByModule(string module)
        {
            if (string.IsNullOrWhiteSpace(module)) return BadRequest(new { error = "Module is required." });
            try
            {
                var tables = (await _metadata.GetTablesByModuleAsync(module))
                    .Select(m => new TableInfo
                    {
                        TableName      = m.TableName,
                        Description    = m.Module ?? string.Empty,
                        PrimaryKey     = m.PrimaryKeyColumn ?? string.Empty,
                        IsReferenceTable = m.TableName.StartsWith("R_", StringComparison.OrdinalIgnoreCase)
                                        || m.TableName.StartsWith("RA_", StringComparison.OrdinalIgnoreCase)
                    })
                    .OrderBy(t => t.TableName)
                    .ToList();

                return Ok(tables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tables by module {Module}", module);
                return StatusCode(500, new List<TableInfo>());
            }
        }

        /// <summary>
        /// Returns all distinct module names (for tree navigation).
        /// </summary>
        [HttpGet("modules")]
        public async Task<ActionResult<List<string>>> GetModules()
        {
            try
            {
                var modules = (await _metadata.GetModulesAsync()).OrderBy(m => m).ToList();
                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching modules");
                return StatusCode(500, new List<string>());
            }
        }

        /// <summary>
        /// Returns detail metadata for a single table: primary key, module, foreign key list.
        /// </summary>
        [HttpGet("tables/{tableName}")]
        public async Task<ActionResult<TableDetailInfo>> GetTableDetail(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return BadRequest(new { error = "Table name is required." });
            try
            {
                var meta = await _metadata.GetTableMetadataAsync(tableName);
                if (meta == null)
                    return NotFound(new { error = $"Table '{tableName}' not found in metadata" });

                var fks = (await _metadata.GetForeignKeysAsync(tableName))
                    .Select(fk => new ForeignKeyInfo
                    {
                        ForeignKeyColumn   = fk.ForeignKeyColumn ?? string.Empty,
                        ReferencedTable    = fk.ReferencedTable  ?? string.Empty,
                        ReferencedPrimaryKey = fk.ReferencedPrimaryKey ?? string.Empty
                    })
                    .ToList();

                return Ok(new TableDetailInfo
                {
                    TableName    = meta.TableName,
                    PrimaryKey   = meta.PrimaryKeyColumn ?? string.Empty,
                    Module       = meta.Module           ?? string.Empty,
                    SubjectArea  = meta.SubjectArea      ?? string.Empty,
                    ForeignKeys  = fks,
                    IsReferenceTable = tableName.StartsWith("R_", StringComparison.OrdinalIgnoreCase)
                                   || tableName.StartsWith("RA_", StringComparison.OrdinalIgnoreCase)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching table detail for {TableName}", tableName);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Returns all child tables that have a FK pointing to the given table,
        /// along with the joining column names.
        /// </summary>
        [HttpGet("tables/{tableName}/children")]
        public async Task<ActionResult<List<ChildTableRelation>>> GetChildTables(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return BadRequest(new { error = "Table name is required." });
            try
            {
                var childMetas = await _metadata.GetReferencingTablesAsync(tableName);

                var result = new List<ChildTableRelation>();
                foreach (var childMeta in childMetas)
                {
                    // Find which FK in the child table references the parent
                    var matchingFks = (childMeta.ForeignKeys ?? Enumerable.Empty<PPDMForeignKey>())
                        .Where(fk => fk.ReferencedTable != null &&
                                     fk.ReferencedTable.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var fk in matchingFks)
                    {
                        result.Add(new ChildTableRelation
                        {
                            ChildTableName     = childMeta.TableName,
                            ForeignKeyColumn   = fk.ForeignKeyColumn  ?? string.Empty,
                            ReferencedColumn   = fk.ReferencedPrimaryKey ?? string.Empty,
                            ChildPrimaryKey    = childMeta.PrimaryKeyColumn ?? string.Empty
                        });
                    }
                }

                // Deduplicate (same child table / same FK column pair)
                var distinct = result
                    .GroupBy(r => $"{r.ChildTableName}|{r.ForeignKeyColumn}")
                    .Select(g => g.First())
                    .OrderBy(r => r.ChildTableName)
                    .ToList();

                return Ok(distinct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching child tables for {TableName}", tableName);
                return StatusCode(500, new List<ChildTableRelation>());
            }
        }
    }

    // ─── Response DTOs (local — no shared model needed for metadata) ────────────

    public sealed class DataCategory
    {
        public string Id          { get; set; } = string.Empty;
        public string Name        { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon        { get; set; } = string.Empty;
        public string Color       { get; set; } = "default";
        public List<string> Prefixes  { get; set; } = new();
        public string ModuleHint  { get; set; } = string.Empty;
    }

    public sealed class DataCategoryWithTables
    {
        public string Id          { get; set; } = string.Empty;
        public string Name        { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon        { get; set; } = string.Empty;
        public string Color       { get; set; } = "default";
        public List<string> Tables { get; set; } = new();
        public int TableCount     { get; set; }
    }

    public sealed class TableInfo
    {
        public string TableName      { get; set; } = string.Empty;
        public string Description    { get; set; } = string.Empty;
        public string PrimaryKey     { get; set; } = string.Empty;
        public bool   IsReferenceTable { get; set; }
    }

    /// <summary>Single-table detail returned by GET /api/ppdm39/metadata/tables/{tableName}</summary>
    public sealed class TableDetailInfo
    {
        public string TableName        { get; set; } = string.Empty;
        public string PrimaryKey       { get; set; } = string.Empty;
        public string Module           { get; set; } = string.Empty;
        public string SubjectArea      { get; set; } = string.Empty;
        public bool   IsReferenceTable { get; set; }
        public List<ForeignKeyInfo> ForeignKeys { get; set; } = new();
    }

    public sealed class ForeignKeyInfo
    {
        public string ForeignKeyColumn    { get; set; } = string.Empty;
        public string ReferencedTable     { get; set; } = string.Empty;
        public string ReferencedPrimaryKey { get; set; } = string.Empty;
    }

    /// <summary>Child-table FK relationship returned by GET /api/ppdm39/metadata/tables/{tableName}/children</summary>
    public sealed class ChildTableRelation
    {
        public string ChildTableName   { get; set; } = string.Empty;
        /// <summary>Column in the child table that stores the parent's key value.</summary>
        public string ForeignKeyColumn { get; set; } = string.Empty;
        /// <summary>Column in the parent table that is referenced (usually the PK).</summary>
        public string ReferencedColumn { get; set; } = string.Empty;
        public string ChildPrimaryKey  { get; set; } = string.Empty;
    }
}
