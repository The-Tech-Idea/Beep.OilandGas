using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core.Tree;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Tree
{
    /// <summary>
    /// Builds tree structures from PPDM 3.9 metadata
    /// UI-framework agnostic - can be used in WinForms, WPF, Blazor, etc.
    /// </summary>
    public class PPDMTreeBuilder : IPPDMTreeBuilder
    {
        private readonly IPPDMMetadataRepository _metadataRepository;
        private Dictionary<string, SubjectAreaInfo> _subjectAreasCache;
        private Dictionary<string, Dictionary<string, List<string>>> _tableToModuleMappingCache;
        private readonly object _lockObject = new object();

        public PPDMTreeBuilder(IPPDMMetadataRepository metadataRepository)
        {
            _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
        }

        /// <summary>
        /// Builds a complete tree structure from metadata
        /// Organized by Subject Areas as per PPDM 3.9 Roadmap
        /// All nodes are linked to actual tables in metadata
        /// </summary>
        public async Task<PPDMTreeNode> BuildTreeAsync(bool includeRelationships = false, bool includeColumns = false)
        {
            var root = new PPDMTreeNode
            {
                Id = "PPDM39_ROOT",
                Text = "PPDM 3.9 Data Model",
                Description = "Petroleum Public Data Model Version 3.9",
                NodeType = PPDMTreeNodeType.Root,
                IconKey = "Root",
                IsExpanded = true,
                IsVisible = true
            };

            // Get all table metadata
            var allMetadata = PPDM39Metadata.GetMetadata();
            var allTables = allMetadata.Values.ToList();
            
            System.Diagnostics.Debug.WriteLine($"Loaded {allTables.Count} tables from metadata");
            
            // Get subject areas in order
            var subjectAreas = GetSubjectAreas();
            System.Diagnostics.Debug.WriteLine($"Loaded {subjectAreas.Count} subject areas");
            var subjectAreaSet = new HashSet<string>(subjectAreas.Select(sa => sa.Name), StringComparer.OrdinalIgnoreCase);

            // Group tables by SubjectArea and Module from metadata
            var tablesBySubjectArea = allTables
                .Where(t => !string.IsNullOrWhiteSpace(t.SubjectArea) && 
                           subjectAreaSet.Contains(t.SubjectArea))
                .GroupBy(t => t.SubjectArea, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
            
            System.Diagnostics.Debug.WriteLine($"Grouped tables into {tablesBySubjectArea.Count} subject areas");
            
            // Log tables without subject areas
            var tablesWithoutSubjectArea = allTables
                .Where(t => string.IsNullOrWhiteSpace(t.SubjectArea) || !subjectAreaSet.Contains(t.SubjectArea))
                .ToList();
            if (tablesWithoutSubjectArea.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: {tablesWithoutSubjectArea.Count} tables have no valid subject area");
            }

            // Build subject area nodes in roadmap order
            foreach (var subjectArea in subjectAreas)
            {
                if (!tablesBySubjectArea.TryGetValue(subjectArea.Name, out var subjectTables))
                    continue;

                var subjectAreaNode = new PPDMTreeNode
                {
                    Id = $"SUBJECT_{subjectArea.Name}",
                    Text = subjectArea.Name,
                    Description = GetSubjectAreaDescription(subjectArea.Name),
                    NodeType = PPDMTreeNodeType.SubjectArea,
                    IconKey = "SubjectArea",
                    IsExpanded = false,
                    IsVisible = true,
                    Data = new Dictionary<string, object>
                    {
                        { "SubjectAreaName", subjectArea.Name }
                    }
                };

                // Group tables by Module from metadata
                var moduleGroups = subjectTables
                    .GroupBy(t => string.IsNullOrWhiteSpace(t.Module) ? "General" : t.Module, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(g => GetModuleOrderIndex(g.Key, subjectArea.Modules))
                    .ThenBy(g => g.Key);

                foreach (var moduleGroup in moduleGroups)
                {
                    var moduleName = moduleGroup.Key;
                    var moduleTables = moduleGroup.OrderBy(t => t.TableName).ToList();

                    var moduleNode = new PPDMTreeNode
                    {
                        Id = $"MODULE_{moduleName}",
                        Text = moduleName,
                        Description = GetModuleDescription(moduleName),
                        NodeType = PPDMTreeNodeType.Module,
                        IconKey = "Module",
                        IsExpanded = false,
                        IsVisible = true,
                        Data = new Dictionary<string, object>
                        {
                            { "ModuleName", moduleName },
                            { "TableCount", moduleTables.Count },
                            { "SubjectArea", subjectArea.Name },
                            { "TableNames", moduleTables.Select(t => t.TableName).ToList() },
                            { "TableMetadata", moduleTables }
                        }
                    };

                    foreach (var tableMeta in moduleTables)
                    {
                        var tableNode = CreateTableNode(tableMeta, includeRelationships, includeColumns);
                        tableNode.Data["TableMetadata"] = tableMeta;
                        if (tableMeta.ForeignKeys != null && tableMeta.ForeignKeys.Count > 0)
                        {
                            tableNode.Data["ForeignKeys"] = tableMeta.ForeignKeys;
                        }
                        moduleNode.AddChild(tableNode);
                    }

                    subjectAreaNode.AddChild(moduleNode);
                }

                subjectAreaNode.Data["ModuleCount"] = subjectAreaNode.Children.Count;
                subjectAreaNode.Data["TableCount"] = subjectAreaNode.Children.Sum(m => m.Children.Count);

                if (subjectAreaNode.Children.Count > 0)
                {
                    root.AddChild(subjectAreaNode);
                }
            }

            // Ensure all nodes are visible by default
            SetVisibility(root, true);

            return root;
        }

        /// <summary>
        /// Builds a tree for a specific module
        /// </summary>
        public async Task<PPDMTreeNode> BuildModuleTreeAsync(string moduleName, bool includeRelationships = false, bool includeColumns = false)
        {
            var moduleNode = new PPDMTreeNode
            {
                Id = $"MODULE_{moduleName}",
                Text = moduleName,
                Description = GetModuleDescription(moduleName),
                NodeType = PPDMTreeNodeType.Module,
                IconKey = "Module",
                IsExpanded = true
            };

            var tables = await _metadataRepository.GetTablesByModuleAsync(moduleName);
            var tableList = tables.ToList();

            moduleNode.Data = new Dictionary<string, object>
            {
                { "ModuleName", moduleName },
                { "TableCount", tableList.Count },
                { "TableNames", tableList.Select(t => t.TableName).ToList() },
                { "TableMetadata", tableList } // Link to actual table metadata
            };
            
            // Group tables by subcategory
            var tablesBySubcategory = GroupTablesBySubcategory(moduleName, tableList);
            
            if (tablesBySubcategory.Count > 1)
            {
                // Multiple subcategories - create subcategory nodes
                foreach (var subcategoryKvp in tablesBySubcategory.OrderBy(x => x.Key))
                {
                    var subcategoryName = subcategoryKvp.Key;
                    var subcategoryTables = subcategoryKvp.Value;
                    
                    var subcategoryNode = new PPDMTreeNode
                    {
                        Id = $"SUBCAT_{moduleName}_{subcategoryName}",
                        Text = subcategoryName,
                        Description = GetSubcategoryDescription(moduleName, subcategoryName),
                        NodeType = PPDMTreeNodeType.Subcategory,
                        IconKey = "Subcategory",
                        IsExpanded = false,
                        Data = new Dictionary<string, object>
                        {
                            { "SubcategoryName", subcategoryName },
                            { "ModuleName", moduleName },
                            { "TableCount", subcategoryTables.Count },
                            { "TableNames", subcategoryTables.Select(t => t.TableName).ToList() },
                            { "TableMetadata", subcategoryTables } // Link to actual table metadata
                        }
                    };
                    
                    // Add tables to subcategory
                    foreach (var tableMeta in subcategoryTables.OrderBy(t => t.TableName))
                    {
                        var tableNode = CreateTableNode(tableMeta, includeRelationships, includeColumns);
                        tableNode.Data["TableMetadata"] = tableMeta;
                        subcategoryNode.AddChild(tableNode);
                    }
                    
                    moduleNode.AddChild(subcategoryNode);
                }
            }
            else
            {
                // Single subcategory or no subcategories - add tables directly to module
                var tablesToAdd = tablesBySubcategory.Values.FirstOrDefault() ?? tableList;
                foreach (var tableMeta in tablesToAdd.OrderBy(t => t.TableName))
                {
                    var tableNode = CreateTableNode(tableMeta, includeRelationships, includeColumns);
                    tableNode.Data["TableMetadata"] = tableMeta;
                    moduleNode.AddChild(tableNode);
                }
            }

            return moduleNode;
        }

        /// <summary>
        /// Builds a tree showing relationships for a specific table
        /// </summary>
        public async Task<PPDMTreeNode> BuildTableRelationshipsTreeAsync(string tableName)
        {
            var tableMeta = await _metadataRepository.GetTableMetadataAsync(tableName);
            if (tableMeta == null)
                throw new ArgumentException($"Table {tableName} not found", nameof(tableName));

            var tableNode = CreateTableNode(tableMeta, includeRelationships: true, includeColumns: false);
            tableNode.IsExpanded = true;

            // Add referenced tables (tables this table references)
            if (tableMeta.ForeignKeys != null && tableMeta.ForeignKeys.Count > 0)
            {
                var referencesNode = new PPDMTreeNode
                {
                    Id = $"{tableName}_REFERENCES",
                    Text = "References (Foreign Keys)",
                    Description = "Tables this table references",
                    NodeType = PPDMTreeNodeType.Relationship,
                    IconKey = "References",
                    IsExpanded = true
                };

                foreach (var fk in tableMeta.ForeignKeys.OrderBy(f => f.ReferencedTable))
                {
                    var refTableMeta = await _metadataRepository.GetTableMetadataAsync(fk.ReferencedTable);
                    if (refTableMeta != null)
                    {
                        var refNode = new PPDMTreeNode
                        {
                            Id = $"{tableName}_FK_{fk.ForeignKeyColumn}",
                            Text = $"{fk.ReferencedTable} ({fk.ForeignKeyColumn})",
                            Description = $"Foreign Key: {fk.ForeignKeyColumn} → {fk.ReferencedTable}.{fk.ReferencedPrimaryKey}",
                            NodeType = PPDMTreeNodeType.Relationship,
                            IconKey = "ForeignKey",
                            Data = new Dictionary<string, object>
                            {
                                { "ForeignKeyColumn", fk.ForeignKeyColumn },
                                { "ReferencedTable", fk.ReferencedTable },
                                { "ReferencedPrimaryKey", fk.ReferencedPrimaryKey },
                                { "RelationshipType", fk.RelationshipType },
                                { "SourceTable", tableName },
                                { "SourceTableMetadata", tableMeta },
                                { "ReferencedTableMetadata", refTableMeta } // Link to actual referenced table metadata
                            }
                        };
                        referencesNode.AddChild(refNode);
                    }
                }

                tableNode.AddChild(referencesNode);
            }

            // Add referencing tables (tables that reference this table)
            var referencingTables = await _metadataRepository.GetReferencingTablesAsync(tableName);
            var referencingList = referencingTables.ToList();

            if (referencingList.Count > 0)
            {
                var referencedByNode = new PPDMTreeNode
                {
                    Id = $"{tableName}_REFERENCED_BY",
                    Text = "Referenced By",
                    Description = "Tables that reference this table",
                    NodeType = PPDMTreeNodeType.Relationship,
                    IconKey = "ReferencedBy",
                    IsExpanded = false
                };

                foreach (var refTableMeta in referencingList.OrderBy(t => t.TableName))
                {
                    var refNode = new PPDMTreeNode
                    {
                        Id = $"{refTableMeta.TableName}_REF_{tableName}",
                        Text = refTableMeta.TableName,
                        Description = $"References {tableName}",
                        NodeType = PPDMTreeNodeType.Table,
                        IconKey = "Table",
                        Data = new Dictionary<string, object>
                        {
                            { "TableName", refTableMeta.TableName },
                            { "Module", refTableMeta.Module },
                            { "TableMetadata", refTableMeta }, // Link to actual table metadata
                            { "ReferencedTable", tableName },
                            { "ReferencedTableMetadata", tableMeta }
                        }
                    };
                    referencedByNode.AddChild(refNode);
                }

                tableNode.AddChild(referencedByNode);
            }

            return tableNode;
        }

        /// <summary>
        /// Gets all modules as a flat list
        /// </summary>
        public async Task<List<PPDMTreeNode>> GetModulesAsync()
        {
            var modules = await _metadataRepository.GetModulesAsync();
            return modules.Select(m => new PPDMTreeNode
            {
                Id = $"MODULE_{m}",
                Text = m,
                Description = GetModuleDescription(m),
                NodeType = PPDMTreeNodeType.Module,
                IconKey = "Module",
                Data = new Dictionary<string, object> { { "ModuleName", m } }
            }).ToList();
        }

        /// <summary>
        /// Gets all tables in a module as a flat list
        /// </summary>
        public async Task<List<PPDMTreeNode>> GetModuleTablesAsync(string moduleName)
        {
            var tables = await _metadataRepository.GetTablesByModuleAsync(moduleName);
            return tables.Select(t => CreateTableNode(t, includeRelationships: false, includeColumns: false)).ToList();
        }

        /// <summary>
        /// Filters the tree based on search text
        /// </summary>
        public Task<PPDMTreeNode> FilterTreeAsync(PPDMTreeNode root, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Reset visibility
                SetVisibility(root, true);
                return Task.FromResult(root);
            }

            var searchLower = searchText.ToLowerInvariant();
            FilterNode(root, searchLower);
            return Task.FromResult(root);
        }

        private Dictionary<string, string> BuildModuleToSubjectAreaLookup(IEnumerable<SubjectArea> subjectAreas)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var area in subjectAreas)
            {
                foreach (var module in area.Modules)
                {
                    if (!map.ContainsKey(module))
                    {
                        map[module] = area.Name;
                    }
                }
            }
            return map;
        }

        private string DetermineSubjectArea(PPDMTableMetadata tableMeta, string moduleName, Dictionary<string, string> moduleToSubjectArea, HashSet<string> validSubjectAreas)
        {
            if (!string.IsNullOrWhiteSpace(tableMeta.SubjectArea) && validSubjectAreas.Contains(tableMeta.SubjectArea))
            {
                return tableMeta.SubjectArea;
            }

            if (moduleToSubjectArea.TryGetValue(moduleName, out var subjectArea))
            {
                return subjectArea;
            }

            // Default to Support Modules to keep every table visible
            return "Support Modules";
        }

        private int GetModuleOrderIndex(string moduleName, IList<string> orderedModules)
        {
            if (orderedModules == null || orderedModules.Count == 0)
            {
                return int.MaxValue;
            }

            for (int i = 0; i < orderedModules.Count; i++)
            {
                if (moduleName.Equals(orderedModules[i], StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return int.MaxValue;
        }

        /// <summary>
        /// Creates a table node linked to its metadata
        /// </summary>
        private PPDMTreeNode CreateTableNode(PPDMTableMetadata tableMeta, bool includeRelationships, bool includeColumns)
        {
            var tableNode = new PPDMTreeNode
            {
                Id = $"TABLE_{tableMeta.TableName}",
                Text = tableMeta.TableName,
                Description = $"Entity: {tableMeta.EntityTypeName}, Primary Key: {tableMeta.PrimaryKeyColumn}",
                NodeType = PPDMTreeNodeType.Table,
                IconKey = "Table",
                IsExpanded = false,
                Data = new Dictionary<string, object>
                {
                    { "TableName", tableMeta.TableName },
                    { "EntityTypeName", tableMeta.EntityTypeName },
                    { "PrimaryKeyColumn", tableMeta.PrimaryKeyColumn },
                    { "Module", tableMeta.Module },
                    { "ForeignKeyCount", tableMeta.ForeignKeys?.Count ?? 0 },
                    { "TableMetadata", tableMeta }, // Link to actual table metadata
                    { "CommonColumns", tableMeta.CommonColumns ?? new List<string>() },
                    { "ForeignKeys", tableMeta.ForeignKeys ?? new List<PPDMForeignKey>() }
                }
            };

            // Add primary key node
            if (!string.IsNullOrWhiteSpace(tableMeta.PrimaryKeyColumn))
            {
                var pkNode = new PPDMTreeNode
                {
                    Id = $"{tableMeta.TableName}_PK",
                    Text = $"Primary Key: {tableMeta.PrimaryKeyColumn}",
                    Description = $"Primary key column(s): {tableMeta.PrimaryKeyColumn}",
                    NodeType = PPDMTreeNodeType.PrimaryKey,
                    IconKey = "PrimaryKey",
                    Data = new Dictionary<string, object> { { "PrimaryKeyColumn", tableMeta.PrimaryKeyColumn } }
                };
                tableNode.AddChild(pkNode);
            }

            // Add common columns group
            if (tableMeta.CommonColumns != null && tableMeta.CommonColumns.Count > 0)
            {
                var commonColsNode = new PPDMTreeNode
                {
                    Id = $"{tableMeta.TableName}_COMMON_COLS",
                    Text = $"Common Columns ({tableMeta.CommonColumns.Count})",
                    Description = "Standard PPDM columns",
                    NodeType = PPDMTreeNodeType.CommonColumns,
                    IconKey = "CommonColumns",
                    IsExpanded = false
                };

                if (includeColumns)
                {
                    foreach (var col in tableMeta.CommonColumns.OrderBy(c => c))
                    {
                        var colNode = new PPDMTreeNode
                        {
                            Id = $"{tableMeta.TableName}_COL_{col}",
                            Text = col,
                            Description = $"Common column: {col}",
                            NodeType = PPDMTreeNodeType.Column,
                            IconKey = "Column",
                            Data = new Dictionary<string, object> { { "ColumnName", col }, { "IsCommonColumn", true } }
                        };
                        commonColsNode.AddChild(colNode);
                    }
                }

                tableNode.AddChild(commonColsNode);
            }

            // Add foreign key relationships (linked to actual referenced table metadata)
            if (includeRelationships && tableMeta.ForeignKeys != null && tableMeta.ForeignKeys.Count > 0)
            {
                var fkNode = new PPDMTreeNode
                {
                    Id = $"{tableMeta.TableName}_FKS",
                    Text = $"Foreign Keys ({tableMeta.ForeignKeys.Count})",
                    Description = "Foreign key relationships",
                    NodeType = PPDMTreeNodeType.Relationship,
                    IconKey = "ForeignKeys",
                    IsExpanded = false,
                    Data = new Dictionary<string, object>
                    {
                        { "SourceTable", tableMeta.TableName },
                        { "SourceTableMetadata", tableMeta }
                    }
                };

                foreach (var fk in tableMeta.ForeignKeys.OrderBy(f => f.ReferencedTable))
                {
                    // Try to get referenced table metadata
                    var referencedTableMeta = _metadataRepository.GetTableMetadataAsync(fk.ReferencedTable).GetAwaiter().GetResult();
                    
                    var fkItemNode = new PPDMTreeNode
                    {
                        Id = $"{tableMeta.TableName}_FK_{fk.ForeignKeyColumn}",
                        Text = $"{fk.ForeignKeyColumn} → {fk.ReferencedTable}",
                        Description = $"{fk.RelationshipType}: {fk.ForeignKeyColumn} references {fk.ReferencedTable}.{fk.ReferencedPrimaryKey}",
                        NodeType = PPDMTreeNodeType.Relationship,
                        IconKey = "ForeignKey",
                        Data = new Dictionary<string, object>
                        {
                            { "ForeignKeyColumn", fk.ForeignKeyColumn },
                            { "ReferencedTable", fk.ReferencedTable },
                            { "ReferencedPrimaryKey", fk.ReferencedPrimaryKey },
                            { "RelationshipType", fk.RelationshipType },
                            { "SourceTable", tableMeta.TableName },
                            { "SourceTableMetadata", tableMeta },
                            { "ReferencedTableMetadata", referencedTableMeta } // Link to actual referenced table metadata
                        }
                    };
                    fkNode.AddChild(fkItemNode);
                }

                tableNode.AddChild(fkNode);
            }

            return tableNode;
        }

        /// <summary>
        /// Maps actual module names from metadata to subject areas
        /// Based on PPDM 3.9 Roadmap structure
        /// This mapping is used as a fallback when metadata doesn't have SubjectArea set
        /// </summary>
        private Dictionary<string, List<string>> GetModuleToSubjectAreaMapping()
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                // Support Modules
                { "Support Modules", new List<string> 
                    { 
                        "Areas",
                        "Spatial Locations",
                        "Spatial Parcels",
                        "Entitlements",
                        "Finances",
                        "Rate Schedules",
                        "Source Document Bibliography",
                        "Additives",
                        "Equipment",
                        "Coordinate Systems",
                        "Business Associates",
                        "Products and Substances",
                        "Projects",
                        "Work Orders",
                        "General"
                    } 
                },
                
                // Data Management & Units of Measure
                { "Data Management & Units of Measure", new List<string> 
                    { 
                        "Data Management",
                        "Unit of Measure",
                        "Volume Conversions",
                        "Reference Table Management"
                    } 
                },
                
                // Stratigraphy, Lithology & Sample Analysis
                { "Stratigraphy, Lithology & Sample Analysis", new List<string> 
                    { 
                        "Stratigraphy",
                        "Lithology",
                        "Ecozones and Environments",
                        "Paleontology",
                        "Interpretation",
                        "Sample Management",
                        "Sample Analysis"
                    } 
                },
                
                // Production & Reserves
                { "Production & Reserves", new List<string> 
                    { 
                        "Fields",
                        "Pools",
                        "Production Reporting",
                        "Production Strings",
                        "Production Lease Units",
                        "Spacing Units",
                        "Production Facilities",
                        "Reporting Hierarchies",
                        "Reserves Reporting"
                    } 
                },
                
                // Wells
                { "Wells", new List<string> 
                    { 
                        "Wells",
                        "Well Logs",
                        "Legal Locations"
                    } 
                },
                
                // Product Management & Classifications
                { "Product Management & Classifications", new List<string> 
                    { 
                        "Classification Systems",
                        "Product and Information Management"
                    } 
                },
                
                // Seismic
                { "Seismic", new List<string> 
                    { 
                        "Seismic"
                    } 
                },
                
                // Support Facilities
                { "Support Facilities", new List<string> 
                    { 
                        "Support Facilities"
                    } 
                },
                
                // Operations Support
                { "Operations Support", new List<string> 
                    { 
                        "Applications",
                        "Consultations",
                        "Consents",
                        "Negotiations",
                        "Contests",
                        "Disputes",
                        "Notifications",
                        "Health Safety & Environment"
                    } 
                },
                
                // Land & Legal Management
                { "Land & Legal Management", new List<string> 
                    { 
                        "Land",
                        "Land Rights",
                        "Contracts",
                        "Obligations",
                        "Instruments",
                        "Interest Sets",
                        "Restrictions"
                    } 
                },
                
                // Other - catch-all for modules not explicitly mapped
                { "Other", new List<string> 
                    { 
                        "Other", "General", "Uncategorized"
                    } 
                }
            };
        }

        /// <summary>
        /// Finds the subject area for a given module name
        /// Uses flexible matching to handle variations in module names and table names
        /// </summary>
        private string FindSubjectAreaForModule(string moduleName, Dictionary<string, List<string>> moduleToSubjectAreaMap, List<PPDMTableMetadata> moduleTables = null)
        {
            // Normalize module name: replace underscores with spaces for comparison
            // Metadata uses underscores (WELL_LOG) but mapping uses spaces (WELL LOG)
            var normalizedModuleName = moduleName.Replace("_", " ");
            
            // First try exact match on module name (with normalization)
            foreach (var kvp in moduleToSubjectAreaMap)
            {
                // Check if any mapped module matches (with normalization)
                foreach (var mappedModule in kvp.Value)
                {
                    var normalizedMappedModule = mappedModule.Replace("_", " ");
                    if (normalizedModuleName.Equals(normalizedMappedModule, StringComparison.OrdinalIgnoreCase))
                    {
                        return kvp.Key;
                    }
                }
            }
            
            // Try matching by table names in the module (more reliable)
            // PPDM uses table name prefixes (e.g., WELL, STRAT, SEIS, etc.)
            // IMPORTANT: Metadata uses underscores (WELL_LOG) but mapping uses spaces (WELL LOG)
            // So we need to normalize both for comparison
            if (moduleTables != null && moduleTables.Count > 0)
            {
                // Count matches per subject area
                var subjectAreaMatches = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                
                foreach (var table in moduleTables)
                {
                    // Normalize table name: replace underscores with spaces for comparison
                    var tableName = table.TableName.ToUpperInvariant().Replace("_", " ");
                    
                    foreach (var kvp in moduleToSubjectAreaMap)
                    {
                        foreach (var mappedItem in kvp.Value)
                        {
                            // Normalize mapped item: replace underscores with spaces (in case some have underscores)
                            var mappedItemUpper = mappedItem.ToUpperInvariant().Replace("_", " ");
                            
                            // Check if table name starts with the mapped item (prefix match)
                            // e.g., "WELL LOG" matches "WELL LOG CURVE", "WELL LOG AXIS", etc.
                            if (tableName.StartsWith(mappedItemUpper, StringComparison.OrdinalIgnoreCase))
                            {
                                if (!subjectAreaMatches.ContainsKey(kvp.Key))
                                    subjectAreaMatches[kvp.Key] = 0;
                                subjectAreaMatches[kvp.Key]++;
                            }
                            // Also check if table name contains the mapped item
                            // e.g., "SEIS" matches "SEIS SET", "SEIS ACQTN", etc.
                            else if (tableName.Contains(mappedItemUpper, StringComparison.OrdinalIgnoreCase))
                            {
                                if (!subjectAreaMatches.ContainsKey(kvp.Key))
                                    subjectAreaMatches[kvp.Key] = 0;
                                subjectAreaMatches[kvp.Key]++;
                            }
                            // Also check the original table name (with underscores) against normalized mapped item
                            // This handles cases where mapped item is just a prefix like "WELL" or "SEIS"
                            var tableNameOriginal = table.TableName.ToUpperInvariant();
                            if (tableNameOriginal.StartsWith(mappedItemUpper.Replace(" ", "_"), StringComparison.OrdinalIgnoreCase) ||
                                tableNameOriginal.Contains(mappedItemUpper.Replace(" ", "_"), StringComparison.OrdinalIgnoreCase))
                            {
                                if (!subjectAreaMatches.ContainsKey(kvp.Key))
                                    subjectAreaMatches[kvp.Key] = 0;
                                subjectAreaMatches[kvp.Key]++;
                            }
                        }
                    }
                }
                
                // Return the subject area with the most matches
                if (subjectAreaMatches.Count > 0)
                {
                    return subjectAreaMatches.OrderByDescending(x => x.Value).First().Key;
                }
            }
            
            // Try partial matching on module name (e.g., "Well" matches "Wells", "Well Logs", etc.)
            // Normalize both by replacing underscores with spaces
            var moduleNameLower = moduleName.Replace("_", " ").ToLowerInvariant();
            foreach (var kvp in moduleToSubjectAreaMap)
            {
                foreach (var mappedModule in kvp.Value)
                {
                    var mappedModuleLower = mappedModule.Replace("_", " ").ToLowerInvariant();
                    // Check if module name contains mapped module or vice versa
                    // But avoid very short matches that might be too generic
                    if (mappedModuleLower.Length >= 3 && moduleNameLower.Length >= 3)
                    {
                        if (moduleNameLower.Contains(mappedModuleLower) || mappedModuleLower.Contains(moduleNameLower))
                        {
                            return kvp.Key;
                        }
                    }
                }
            }
            
            // Try keyword-based matching for common patterns
            if (moduleNameLower.Contains("well"))
            {
                return "Wells";
            }
            // Check for directional survey specifically (part of Wells but might be separate module)
            if (moduleNameLower.Contains("directional") || moduleNameLower.Contains("dir srv") || moduleNameLower.Contains("survey"))
            {
                return "Wells";
            }
            if (moduleNameLower.Contains("strat") || moduleNameLower.Contains("lith") || moduleNameLower.Contains("paleo") || moduleNameLower.Contains("biostrat") || moduleNameLower.Contains("sample") || moduleNameLower.Contains("analysis") || moduleNameLower.Contains("fossil"))
            {
                return "Stratigraphy, Lithology & Sample Analysis";
            }
            if (moduleNameLower.Contains("production") || moduleNameLower.Contains("reserve") || moduleNameLower.Contains("field") || moduleNameLower.Contains("pool"))
            {
                return "Production & Reserves";
            }
            if (moduleNameLower.Contains("seismic"))
            {
                return "Seismic";
            }
            if (moduleNameLower.Contains("land") || moduleNameLower.Contains("legal") || moduleNameLower.Contains("contract") || moduleNameLower.Contains("obligation") || moduleNameLower.Contains("interest"))
            {
                return "Land & Legal Management";
            }
            if (moduleNameLower.Contains("facility") || moduleNameLower.Contains("rig") || moduleNameLower.Contains("road") || moduleNameLower.Contains("vessel"))
            {
                return "Support Facilities";
            }
            // Data Management & Units of Measure - check for PPDM tables, units, measurements, conversions, reference tables
            if (moduleNameLower.Contains("data management") || moduleNameLower.Contains("unit") || moduleNameLower.Contains("measure") || 
                moduleNameLower.Contains("conversion") || moduleNameLower.Contains("ppdm") || moduleNameLower.Contains("reference") ||
                moduleNameLower.Contains("volume") || moduleNameLower.Contains("regime") || moduleNameLower.StartsWith("ra_"))
            {
                return "Data Management & Units of Measure";
            }
            if (moduleNameLower.Contains("classification") || moduleNameLower.Contains("taxonomy") || moduleNameLower.Contains("product management") || moduleNameLower.Contains("information management") || moduleNameLower.Contains("records management") || moduleNameLower.Contains("rm "))
            {
                return "Product Management & Classifications";
            }
            // Software Application - could be Support Modules or Operations Support
            if (moduleNameLower.Contains("software application") || moduleNameLower.Contains("software app"))
            {
                return "Support Modules";
            }
            if (moduleNameLower.Contains("application") || moduleNameLower.Contains("applic") || moduleNameLower.Contains("consent") || moduleNameLower.Contains("consultation") || moduleNameLower.Contains("notification") || moduleNameLower.Contains("hse") || moduleNameLower.Contains("health") || moduleNameLower.Contains("safety") || moduleNameLower.Contains("incident"))
            {
                return "Operations Support";
            }
            // Products and Substances
            if (moduleNameLower.Contains("substance") || moduleNameLower.Contains("product") && !moduleNameLower.Contains("product management"))
            {
                return "Support Modules";
            }
            
            // Default to "Other" if not found
            return "Other";
        }

        /// <summary>
        /// Gets subject area description
        /// </summary>
        private string GetSubjectAreaDescription(string subjectAreaName)
        {
            var descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Support Modules", "Core support capabilities such as areas, entitlements, finance, additives, and reference content" },
                { "Data Management & Units of Measure", "PPDM data management assets, units of measure, conversions, and reference tables" },
                { "Stratigraphy, Lithology & Sample Analysis", "Ecozones, lithology, paleontology, interpretations, and sample management/analysis" },
                { "Production & Reserves", "Fields, pools, production reporting, strings, spacing units, facilities, and reserves reporting" },
                { "Wells", "Well lifecycle data including activity, logs, status, materials, equipment, and tests" },
                { "Product Management & Classifications", "Classification systems plus product and information management" },
                { "Seismic", "Seismic acquisition, processing, interpretation, and survey sets" },
                { "Support Facilities", "Support facilities such as rigs, roads, vessels, and related infrastructure" },
                { "Operations Support", "Applications, consents, consultations, contests/disputes, notifications, and HSE" },
                { "Land & Legal Management", "Contracts, obligations, instruments, interest sets, restrictions, and land rights" },
                { "Paleontology & Biostratigraphy", "Detailed stratigraphy, biostratigraphy, and fossil interpretation context" },
                { "Other", "Modules not categorized in main subject areas" }
            };

            return descriptions.TryGetValue(subjectAreaName, out var desc) ? desc : $"Subject Area: {subjectAreaName}";
        }

        /// <summary>
        /// Gets all unique module names from metadata for debugging
        /// </summary>
        public List<string> GetAllModulesFromMetadata()
        {
            var allMetadata = PPDM39Metadata.GetMetadata();
            var modules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var kvp in allMetadata)
            {
                var module = kvp.Value.Module;
                if (string.IsNullOrWhiteSpace(module))
                    module = "General";
                modules.Add(module);
            }
            
            return modules.OrderBy(m => m).ToList();
        }

        /// <summary>
        /// Loads subject areas and modules from embedded JSON resource
        /// </summary>
        private Dictionary<string, SubjectAreaInfo> LoadSubjectAreasFromJson()
        {
            if (_subjectAreasCache != null)
                return _subjectAreasCache;

            lock (_lockObject)
            {
                if (_subjectAreasCache != null)
                    return _subjectAreasCache;

                try
                {
                    // Get the assembly that contains this class (DataManagement assembly), not the executing assembly
                    var assembly = typeof(PPDMTreeBuilder).Assembly;
                    
                    // Get all embedded resources to find the correct name
                    var allResources = assembly.GetManifestResourceNames();
                    System.Diagnostics.Debug.WriteLine($"Available embedded resources in {assembly.FullName}: {string.Join(", ", allResources)}");
                    
                    // Try to find the resource by name (ends with the filename)
                    var resourceName = allResources.FirstOrDefault(r => 
                        r.EndsWith("PPDM39SubjectAreasAndModules.json", StringComparison.OrdinalIgnoreCase));
                    
                    if (resourceName == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Embedded resource PPDM39SubjectAreasAndModules.json not found. Falling back to default subject areas.");
                        return GetDefaultSubjectAreas();
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Loading subject areas from embedded resource: {resourceName}");
                    
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not open stream for resource: {resourceName}");
                        return GetDefaultSubjectAreas();
                    }
                    
                    using var reader = new StreamReader(stream);
                    var jsonContent = reader.ReadToEnd();
                    var jsonDoc = JsonDocument.Parse(jsonContent);
                    var result = ParseSubjectAreasJson(jsonDoc);
                    _subjectAreasCache = result;
                    System.Diagnostics.Debug.WriteLine($"Successfully loaded {result.Count} subject areas from embedded resource");
                    return result;
                }
                catch (Exception ex)
                {
                    // Log error and fallback to default
                    System.Diagnostics.Debug.WriteLine($"Error loading subject areas from embedded resource: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    return GetDefaultSubjectAreas();
                }
            }
        }

        /// <summary>
        /// Parses JSON document into SubjectAreaInfo dictionary
        /// </summary>
        private Dictionary<string, SubjectAreaInfo> ParseSubjectAreasJson(JsonDocument jsonDoc)
        {
            var result = new Dictionary<string, SubjectAreaInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var subjectAreaProp in jsonDoc.RootElement.EnumerateObject())
            {
                var subjectAreaName = subjectAreaProp.Name;
                var modules = new List<ModuleInfo>();

                if (subjectAreaProp.Value.TryGetProperty("Modules", out var modulesElement))
                {
                    foreach (var moduleElement in modulesElement.EnumerateArray())
                    {
                        var moduleName = moduleElement.GetProperty("Name").GetString();
                        var moduleDescription = moduleElement.TryGetProperty("Description", out var descProp)
                            ? descProp.GetString()
                            : string.Empty;

                        modules.Add(new ModuleInfo
                        {
                            Name = moduleName ?? string.Empty,
                            Description = moduleDescription ?? string.Empty
                        });
                    }
                }

                result[subjectAreaName] = new SubjectAreaInfo
                {
                    Name = subjectAreaName,
                    Modules = modules
                };
            }

            return result;
        }

        /// <summary>
        /// Gets default subject areas as fallback
        /// </summary>
        private Dictionary<string, SubjectAreaInfo> GetDefaultSubjectAreas()
        {
            return new Dictionary<string, SubjectAreaInfo>(StringComparer.OrdinalIgnoreCase)
            {
                ["Support Modules"] = new SubjectAreaInfo
                {
                    Name = "Support Modules",
                    Modules = new List<ModuleInfo>
                    {
                        new ModuleInfo { Name = "Additives Catalogue", Description = "Describes drilling additives used during field operations" },
                        new ModuleInfo { Name = "Areas", Description = "Areas of any type, such as geographic, jurisdictional or project" },
                        new ModuleInfo { Name = "Spatial Locations", Description = "Geographic and other location information for business objects" },
                        new ModuleInfo { Name = "Spatial Parcels", Description = "Geographic and other location information for business objects" },
                        new ModuleInfo { Name = "Entitlements", Description = "Describes the rights you have to data and information" },
                        new ModuleInfo { Name = "Finances", Description = "Summarize cost information from your accounting systems" },
                        new ModuleInfo { Name = "Rate Schedules", Description = "Outline fee schedules for functions and services" },
                        new ModuleInfo { Name = "Source Documents and Bibliographies", Description = "Bibliographic reference for source documents" },
                        new ModuleInfo { Name = "Equipment", Description = "Describes equipment used in the field, lab or office" },
                        new ModuleInfo { Name = "Coordinate Systems", Description = "Geographic and cartographic spatial reference systems" },
                        new ModuleInfo { Name = "Business Associates", Description = "Companies, people or organizations you do business with" },
                        new ModuleInfo { Name = "Products and Substances", Description = "Composition of hydrocarbons and other substances" },
                        new ModuleInfo { Name = "Projects", Description = "Capture and track task lists for operational activities" },
                        new ModuleInfo { Name = "Work Orders and Requests", Description = "Tracks work orders that support business operations" }
                    }
                }
            };
        }

        /// <summary>
        /// Gets subject areas based on PPDM 3.9 Roadmap from JSON file
        /// </summary>
        private List<SubjectArea> GetSubjectAreas()
        {
            var subjectAreasJson = LoadSubjectAreasFromJson();
            var result = new List<SubjectArea>();

            foreach (var kvp in subjectAreasJson)
            {
                var subjectArea = new SubjectArea
                {
                    Name = kvp.Value.Name,
                    Description = GetSubjectAreaDescription(kvp.Value.Name),
                    Modules = kvp.Value.Modules.Select(m => m.Name).ToList()
                };
                result.Add(subjectArea);
            }

            return result;
        }

        /// <summary>
        /// Gets module description based on PPDM 3.9 roadmap from JSON file
        /// </summary>
        private string GetModuleDescription(string moduleName)
        {
            var subjectAreasJson = LoadSubjectAreasFromJson();

            // Search through all subject areas and modules to find the description
            foreach (var subjectArea in subjectAreasJson.Values)
            {
                var module = subjectArea.Modules.FirstOrDefault(m => 
                    m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
                
                if (module != null && !string.IsNullOrWhiteSpace(module.Description))
                {
                    return module.Description;
                }
            }

            // Fallback descriptions for common variations
            var fallbackDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Additives", "Describes drilling additives used during field operations" },
                { "Source Document Bibliography", "Bibliographic reference for source documents" },
                { "Work Orders", "Tracks work orders that support business operations" },
                { "Requests", "Tracks work orders that support business operations" },
                { "Ecozones", "Paleontological ecozones or environment definitions" },
                { "Environments", "Paleontological ecozones or environment definitions" },
                { "Production", "Volumes reporting for all products, time periods, activities or methods" },
                { "Reserves", "Summarizes reserves information, including forecasts" },
                { "Seismic Acquisition", "Seismic acquisition data" },
                { "Seismic Processing", "Seismic processing data" },
                { "Facilities", "Descriptive information about facilities such as pipelines, storage tanks or batteries" },
                { "Rigs", "Rig information" },
                { "Roads", "Road information" },
                { "Vessels", "Vessel information" },
                { "Consultations", "Discussions between various parties" },
                { "Negotiations", "Discussions between various parties" },
                { "Contests", "Summary information about legal disputes and their outcomes" },
                { "Disputes", "Summary information about legal disputes and their outcomes" },
                { "Health Safety & Environment", "Incident reporting summaries" },
                { "Land", "Land rights, contracts, legal agreements, and obligations" },
                { "Interest Sets", "Partnerships and division of interests" },
                { "General", "General reference and support tables" }
            };

            if (fallbackDescriptions.TryGetValue(moduleName, out var fallbackDesc))
            {
                return fallbackDesc;
            }

            return $"Module: {moduleName}";
        }

        /// <summary>
        /// Groups tables by subcategory based on table name patterns
        /// </summary>
        private Dictionary<string, List<PPDMTableMetadata>> GroupTablesBySubcategory(string moduleName, List<PPDMTableMetadata> tables)
        {
            var result = new Dictionary<string, List<PPDMTableMetadata>>(StringComparer.OrdinalIgnoreCase);
            
            // Get subcategory mappings for this module
            var subcategoryMappings = GetSubcategoryMappings(moduleName);
            
            foreach (var table in tables)
            {
                string subcategory = "General";
                
                // Try to match table name to subcategory patterns
                foreach (var mapping in subcategoryMappings)
                {
                    if (mapping.Value.Any(pattern => table.TableName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase)))
                    {
                        subcategory = mapping.Key;
                        break;
                    }
                }
                
                if (!result.ContainsKey(subcategory))
                {
                    result[subcategory] = new List<PPDMTableMetadata>();
                }
                
                result[subcategory].Add(table);
            }
            
            return result;
        }

        /// <summary>
        /// Gets subcategory mappings for a module based on table name prefixes
        /// </summary>
        private Dictionary<string, List<string>> GetSubcategoryMappings(string moduleName)
        {
            var mappings = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase)
            {
                // Wells module subcategories
                ["Wells"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Well General Information"] = new List<string> { "WELL ALIAS", "WELL AREA", "WELL FACILITY", "WELL MISC DATA", "WELL SUPPORT FACILITY", "WELL REMARK", "WELL VERISON", "WELL VERSION", "WELL XREF", "WELL COMPONENT" },
                    ["Well Tests"] = new List<string> { "WELL TEST" },
                    ["Well Pressures"] = new List<string> { "WELL PRESSURE" },
                    ["Well Drill Bits"] = new List<string> { "WELL DRILL BIT" },
                    ["Well Operations"] = new List<string> { "WELL DRILL SHAKER", "WELL DRILL ASSEMBLY", "WELL DRILL EQUIPMENT", "WELL DRILL PIPE", "WELL EQUIPMENT", "WELL TUBULAR", "WELL CEMENT", "WELL AIR DRILL", "WELL HORIZ DRILL", "WELL SHOW", "WELL DRILL ADD", "WELL DRILL MUD" },
                    ["Well Completions"] = new List<string> { "WELL COMPLETION", "WELL PERFORMATION", "WELL PERF", "WELL TREATMENT", "WELL PLUGBACK" },
                    ["Well Drilling Reports"] = new List<string> { "WELL DRILL REPORT", "WELL DRILL PERIOD", "WELL DRILL STATISTIC", "WELL DRILL WEATHER", "WELL DRILL INT", "WELL DRILL REMARK" },
                    ["Well Sieve Analysis"] = new List<string> { "WELL SIEVE" },
                    ["Well Operations Safety"] = new List<string> { "WELL DRILL CHECK" },
                    ["Well Agreements"] = new List<string> { "WELL BA SERVICE", "LAND RIGHT WELL" },
                    ["Well Activity Summaries"] = new List<string> { "WELL ACTIVITY" },
                    ["Well Status"] = new List<string> { "WELL STATUS", "PROD STRING", "PROD STR STAT", "PR STR FORM STAT" },
                    ["Well Locations"] = new List<string> { "WELL NODE", "Z_WELL NODE", "Z_WELL GEOMETRY" },
                    ["Well Licenses"] = new List<string> { "WELL LICENSE" },
                    ["Well Cores"] = new List<string> { "WELL CORE" },
                    ["Well Core Analysis"] = new List<string> { "WELL CORE ANAL" },
                    ["Well Directional Survey"] = new List<string> { "WELL DIR SRVY" },
                    ["Well Interpretations"] = new List<string> { "WELL POROUS", "WELL PAYZONE", "STRAT WELL SECTION", "SEIS VELOCITY", "WELL ZONE" },
                    ["Legal Locations"] = new List<string> { "LEGAL LOC", "LEGAL GEODETIC", "LEGAL CARTER", "LEGAL CONGRESS", "LEGAL DLS", "LEGAL FPS", "LEGAL NORTH SEA", "LEGAL NTS", "LEGAL OFFSHORE", "LEGAL OHIO", "LEGAL TEXAS" }
                },
                
                // Well Logs module subcategories
                ["Well Logs"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Well Logs"] = new List<string> { "WELL LOG", "WELL LOG AXIS", "WELL LOG REMARK", "WELL LOG PARM" },
                    ["Well Log Classification"] = new List<string> { "WELL LOG CLASS", "WELL LOG CLS", "WELL LOG CRV CLS" },
                    ["Well Log Curves"] = new List<string> { "WELL LOG CURVE" },
                    ["Mud Logs"] = new List<string> { "WELL MUD" },
                    ["Well Log Acquisition"] = new List<string> { "WELL LOG JOB", "WELL LOG PASS", "WELL LOG TRIP" },
                    ["Raster Logs"] = new List<string> { "WELL LOG CURVE SPLICE", "WELL LOG DGTZ", "WELL LOG CURVE SCALE" },
                    ["Well Log Dictionaries"] = new List<string> { "WELL LOG DICT" }
                },
                
                // Stratigraphy module subcategories
                ["Stratigraphy"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Name Sets"] = new List<string> { "STRAT NAME SET" },
                    ["Relationships"] = new List<string> { "STRAT ALIAS", "STRAT EQUIVALENCE", "STRAT HIERARCHY", "STRAT TOPO" },
                    ["Descriptions"] = new List<string> { "STRAT UNIT DESCRIPTION", "STRAT UNIT AGE", "STRAT UNIT COMPONENT" },
                    ["Stratigraphic Columns"] = new List<string> { "STRAT COLUMN" },
                    ["Field Stations"] = new List<string> { "STRAT FIELD STATION", "STRAT FIELD NODE", "STRAT NODE VERSION", "Z_STRAT FIELD" },
                    ["Interpretations and Ages"] = new List<string> { "STRAT COLUMN ACQTN", "STRAT COL UNIT AGE", "STRAT FIELD SECTION", "STRAT FIELD ACQTN", "STRAT FLD INTERP", "STRAT WELL SECTION", "STRAT WELL ACQTN", "STRAT WELL INTERP", "STRAT ACQTN METHOD", "STRAT INTERP CORR" }
                },
                
                // Analysis module subcategories
                ["Analysis"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Analysis Steps"] = new List<string> { "ANL ANALYSIS STEP", "ANL STEP", "ANL BA", "ANL EQUIP", "ANL PARM", "ANL ANALYSIS BATCH" },
                    ["Analysis Methods"] = new List<string> { "ANL METHOD" },
                    ["Analysis General Results"] = new List<string> { "ANL ACCURACY", "ANL DETAIL", "ANL PROBLEM", "ANL REMARK", "ANL TABLE RESULT" },
                    ["Analysis Validation"] = new List<string> { "ANL VALID" },
                    ["Analysis Calculations"] = new List<string> { "ANL CALC" },
                    ["Analysis Summary"] = new List<string> { "ANL ANALYSIS REPORT", "ANL REPORT", "ANL COMPONENT" },
                    ["Elemental Analysis"] = new List<string> { "ANL ELEMENTAL" },
                    ["Isotope Analysis"] = new List<string> { "ANL ISOTOPE" },
                    ["Gas Analysis"] = new List<string> { "ANL GAS" },
                    ["Oil Analysis"] = new List<string> { "ANL OIL" },
                    ["Maceral / Paleo Analysis"] = new List<string> { "ANL MACERAL", "ANL PALEO", "ANL COAL", "ANL FLUOR", "ANL QGF" },
                    ["Pyrolysis"] = new List<string> { "ANL PYROLYSIS" },
                    ["Spectroscopy"] = new List<string> { "ANL LIQUID CHRO", "ANL GAS CHRO" },
                    ["Water Analysis"] = new List<string> { "ANL WATER" }
                },
                
                // Seismic module subcategories
                ["Seismic"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Subtypes"] = new List<string> { "SEIS 3D", "SEIS ACQTN SURVEY", "SEIS INTERP SET", "SEIS LINE", "SEIS PROC SET", "SEIS SEGMENT", "SEIS SET PLAN", "SEIS WELL" },
                    ["Description"] = new List<string> { "SEIS ACTIVITY", "SEIS ALIAS", "SEIS BA SERVICE", "SEIS SET AREA", "SEIS SET JURISDICTION", "SEIS SET STATUS", "SEIS SET COMPONENT", "SEIS SET AUTHORIZE", "SEIS SET PLAN" },
                    ["Acquisition"] = new List<string> { "SEIS ACQTN", "SEIS RECVR", "SEIS PATCH", "SEIS RECORD", "SEIS CHANNEL", "SEIS VESSEL", "SEIS STREAMER" },
                    ["Locations"] = new List<string> { "SEIS POINT", "SEIS SP SURVEY", "SEIS POINT FLOW", "SEIS BIN", "Z_SEIS SET GEOMETRY" },
                    ["Interpretation"] = new List<string> { "SEIS INTERP", "SEIS PICK", "SEIS VELOCITY" },
                    ["Processing"] = new List<string> { "SEIS PROC" },
                    ["Transactions"] = new List<string> { "SEIS SET AUTHORIZE", "SEIS INSPECTION", "SEIS TRANSACTION" },
                    ["Licenses"] = new List<string> { "SEIS LICENSE" },
                    ["Seismic Set Relationships"] = new List<string> { "SEIS GROUP COMP" }
                },
                
                // Support Facilities module subcategories
                ["Support Facilities"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Descriptions"] = new List<string> { "SF ALIAS", "SF AREA", "SF BA CREW", "SF BA SERVICE", "SF DESCRIPTION", "SF EQUIPMENT", "SF MAINTAIN", "SF STATUS" },
                    ["Restrictions"] = new List<string> { "SF RESTRICTION", "SF REST REMARK" },
                    ["Relationships"] = new List<string> { "SF COMPONENT", "SF XREF", "Z_SF GEOMETRY" },
                    ["Subtypes"] = new List<string> { "SF AIRCRAFT", "SF AIRSTRIP", "SF BRIDGE", "SF DISPOSAL", "SF DOCK", "SF ELECTRIC", "SF HABITAT", "SF LANDING", "SF MONUMENT", "SF OTHER", "SF PAD", "SF PLATFORM", "SF PORT", "SF RAILWAY", "SF RIG", "SF ROAD", "SF TOWER", "SF VEHICLE", "SF VESSEL", "SF WASTE" }
                },
                
                // Applications and Approvals module subcategories
                ["Applications"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Applications"] = new List<string> { "APPLIC", "APPLICATION" },
                    ["Consultations"] = new List<string> { "CONSULT" },
                    ["Consents"] = new List<string> { "CONSENT" },
                    ["Notifications"] = new List<string> { "NOTIFICATION", "NOTIF" },
                    ["Contests"] = new List<string> { "CONTEST" }
                },
                
                // Health Safety & Environment module subcategories
                ["HSE"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Incidents"] = new List<string> { "HSE INCIDENT" },
                    ["Incident Sets"] = new List<string> { "HSE INCIDENT SET", "HSE INCIDENT TYPE", "HSE INCIDENT SEVERITY" }
                },
                
                // Land Rights module subcategories
                ["Land Rights"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Land Right Subtypes"] = new List<string> { "LAND AGREE", "LAND AGREEMENT", "LAND TITLE", "LAND UNIT", "LAND TRACT" },
                    ["Land Status"] = new List<string> { "LAND STATUS", "LAND TERMINATION" },
                    ["Land Acquisition"] = new List<string> { "LAND SALE", "LAND RIGHT COMPONENT" },
                    ["General Land Information"] = new List<string> { "LAND ALIAS", "LAND BA SERVICE", "LAND OCCUPANT", "LAND REMARK", "LAND SIZE", "LAND XREF" },
                    ["Land Right Authorizations"] = new List<string> { "LAND RIGHT APPLIC", "LAND RIGHT BA LIC" },
                    ["Land Relationships"] = new List<string> { "LAND AREA", "LAND RIGHT FACILITY", "LAND RIGHT FIELD", "LAND RIGHT INSTRUMENT", "LAND RIGHT POOL", "LAND RIGHT REST", "LAND RIGHT WELL" }
                },
                
                // Contracts module subcategories
                ["Contracts"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Contract Details"] = new List<string> { "CONT ALIAS", "CONT AREA", "CONT BA", "CONT BA SERVICE", "CONT EXTENSION", "CONT REMARK", "CONT STATUS", "CONT TYPE", "CONT XREF", "CONTRACT COMPONENT" },
                    ["Management Procedures"] = new List<string> { "CONT ACCOUNT PROC", "CONT ALLOW EXPENSE", "CONT OPER PROC", "CONT MKTG", "CONT VOTING PROC" },
                    ["Contract Provisions"] = new List<string> { "CONT PROVISION", "CONT KEY WORD", "CONT EXEMPTION", "CONT PROVISION TEXT", "CONT PROVISION XREF", "CONT JURISDICTION" }
                },
                
                // Obligations module subcategories
                ["Obligations"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Obligations"] = new List<string> { "OBLIG", "OBLIGATION" }
                },
                
                // Partnerships & Interest Sets module subcategories
                ["Interest Sets"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Interest Sets"] = new List<string> { "INT SET", "INTEREST SET" }
                },
                
                // Restrictions module subcategories
                ["Restrictions"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Restrictions"] = new List<string> { "REST", "RESTRICTION" }
                },
                
                // Instruments module subcategories
                ["Instruments"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Instruments"] = new List<string> { "INSTRUMENT" }
                },
                
                // Information Management module subcategories
                ["Information Management"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Information Items"] = new List<string> { "RM CREATOR", "RM CUSTODY", "RM INFO", "Z_RM INFO ITEM GEOMETRY" },
                    ["File Content"] = new List<string> { "RM FILE CONTENT", "RM DATA CONTENT" },
                    ["Physical Management"] = new List<string> { "RM PHYS ITEM", "RM COPY RECORD" },
                    ["Encoding"] = new List<string> { "RM ENCODING", "RM DECRYPT KEY" },
                    ["Subtypes"] = new List<string> { "RM COMPOSITE", "RM DOCUMENT", "RM EQUIPMENT", "RM FOSSIL", "RM LITH SAMPLE", "RM MAP", "RM SEIS TRACE", "RM TRACE HEADER", "RM AUX CHANNEL", "RM WELL LOG", "RM SPATIAL DATA SET" },
                    ["Thesaurii"] = new List<string> { "RM THESAURUS", "RM THESAURUS WORD", "RM KEYWORD", "RM THESAURUS GLOSSARY" },
                    ["Physical Items"] = new List<string> { "RM PHYSICAL ITEM" },
                    ["Images and Calibrations"] = new List<string> { "RM IMAGE" },
                    ["Storage Location"] = new List<string> { "RM PHYS ITEM STORE", "RM DATA STORE" },
                    ["Circulation"] = new List<string> { "RM CIRCULATION", "RM CIRC PROCESS" }
                },
                
                // Classification Systems module subcategories
                ["Classification Systems"] = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Classification Systems"] = new List<string> { "CLASS SYSTEM", "CLASS LEVEL" }
                }
            };
            
            return mappings.TryGetValue(moduleName, out var moduleMappings) 
                ? moduleMappings 
                : new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets subcategory description
        /// </summary>
        private string GetSubcategoryDescription(string moduleName, string subcategoryName)
        {
            // Default description - can be enhanced with specific descriptions if needed
            return $"{subcategoryName} ({moduleName})";
        }

        /// <summary>
        /// Represents a subject area from PPDM 3.9 Roadmap
        /// </summary>
        private class SubjectArea
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public List<string> Modules { get; set; } = new List<string>();
        }

        /// <summary>
        /// Represents subject area information from JSON
        /// </summary>
        private class SubjectAreaInfo
        {
            public string Name { get; set; }
            public List<ModuleInfo> Modules { get; set; } = new List<ModuleInfo>();
        }

        /// <summary>
        /// Represents module information from JSON
        /// </summary>
        private class ModuleInfo
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        /// <summary>
        /// Recursively filters nodes based on search text
        /// </summary>
        private bool FilterNode(PPDMTreeNode node, string searchText)
        {
            bool matches = node.Text.ToLowerInvariant().Contains(searchText) ||
                          (node.Description?.ToLowerInvariant().Contains(searchText) ?? false);

            bool childMatches = false;
            foreach (var child in node.Children)
            {
                if (FilterNode(child, searchText))
                {
                    childMatches = true;
                }
            }

            node.IsVisible = matches || childMatches;
            if (childMatches)
            {
                node.IsExpanded = true;
            }

            return node.IsVisible;
        }

        /// <summary>
        /// Sets visibility for all nodes
        /// </summary>
        private void SetVisibility(PPDMTreeNode node, bool visible)
        {
            node.IsVisible = visible;
            foreach (var child in node.Children)
            {
                SetVisibility(child, visible);
            }
        }
    }
}
