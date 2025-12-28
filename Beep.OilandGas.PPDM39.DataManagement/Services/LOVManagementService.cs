using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Service for managing List of Values (LOV) data
    /// Provides CRUD operations and querying capabilities for LIST_OF_VALUE table
    /// </summary>
    public class LOVManagementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private PPDMGenericRepository _repository;

        public LOVManagementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;
            _repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), _connectionName, "LIST_OF_VALUE");
        }

        /// <summary>
        /// Gets LOVs by value type
        /// </summary>
        public async Task<List<ListOfValueDto>> GetLOVByTypeAsync(string valueType, string? category = null, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "VALUE_TYPE", Operator = "=", FilterValue = valueType },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrEmpty(category))
            {
                filters.Add(new AppFilter { FieldName = "CATEGORY", Operator = "=", FilterValue = category });
            }

            var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), connectionName, "LIST_OF_VALUE");

            var entities = await repository.GetAsync(filters);
            return entities.Cast<LIST_OF_VALUE>()
                .OrderBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                .ThenBy(lov => lov.VALUE_NAME)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// Gets all LOVs in a category
        /// </summary>
        public async Task<List<ListOfValueDto>> GetLOVByCategoryAsync(string category, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CATEGORY", Operator = "=", FilterValue = category },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), connectionName, "LIST_OF_VALUE");

            var entities = await repository.GetAsync(filters);
            return entities.Cast<LIST_OF_VALUE>()
                .OrderBy(lov => lov.VALUE_TYPE)
                .ThenBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                .ThenBy(lov => lov.VALUE_NAME)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// Gets all LOVs for a module
        /// </summary>
        public async Task<List<ListOfValueDto>> GetLOVByModuleAsync(string module, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "MODULE", Operator = "=", FilterValue = module },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), connectionName, "LIST_OF_VALUE");

            var entities = await repository.GetAsync(filters);
            return entities.Cast<LIST_OF_VALUE>()
                .OrderBy(lov => lov.VALUE_TYPE)
                .ThenBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                .ThenBy(lov => lov.VALUE_NAME)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// Gets LOVs by source (PPDM, IHS, Custom, API, ISO, etc.)
        /// </summary>
        public async Task<List<ListOfValueDto>> GetLOVBySourceAsync(string source, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "SOURCE", Operator = "=", FilterValue = source },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), connectionName, "LIST_OF_VALUE");

            var entities = await repository.GetAsync(filters);
            return entities.Cast<LIST_OF_VALUE>()
                .OrderBy(lov => lov.VALUE_TYPE)
                .ThenBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                .ThenBy(lov => lov.VALUE_NAME)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// Gets hierarchical LOVs (parent-child relationships)
        /// </summary>
        public async Task<List<ListOfValueDto>> GetHierarchicalLOVAsync(string valueType, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "VALUE_TYPE", Operator = "=", FilterValue = valueType },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), connectionName, "LIST_OF_VALUE");

            var entities = await repository.GetAsync(filters);
            var allLOVs = entities.Cast<LIST_OF_VALUE>().ToList();

            // Build hierarchy: parents first, then children
            var parents = allLOVs.Where(lov => string.IsNullOrEmpty(lov.PARENT_VALUE_ID))
                .OrderBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                .ThenBy(lov => lov.VALUE_NAME)
                .Select(MapToDto)
                .ToList();

            foreach (var parent in parents)
            {
                parent.Children = allLOVs
                    .Where(lov => lov.PARENT_VALUE_ID == parent.ListOfValueId)
                    .OrderBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                    .ThenBy(lov => lov.VALUE_NAME)
                    .Select(MapToDto)
                    .ToList();
            }

            return parents;
        }

        /// <summary>
        /// Searches LOVs by search term
        /// </summary>
        public async Task<List<ListOfValueDto>> SearchLOVsAsync(string searchTerm, LOVRequest? filters = null, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var appFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            // Add search term filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                appFilters.Add(new AppFilter { FieldName = "VALUE_NAME", Operator = "LIKE", FilterValue = $"%{searchTerm}%" });
            }

            // Add additional filters
            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.ValueType))
                {
                    appFilters.Add(new AppFilter { FieldName = "VALUE_TYPE", Operator = "=", FilterValue = filters.ValueType });
                }
                if (!string.IsNullOrEmpty(filters.Category))
                {
                    appFilters.Add(new AppFilter { FieldName = "CATEGORY", Operator = "=", FilterValue = filters.Category });
                }
                if (!string.IsNullOrEmpty(filters.Module))
                {
                    appFilters.Add(new AppFilter { FieldName = "MODULE", Operator = "=", FilterValue = filters.Module });
                }
                if (!string.IsNullOrEmpty(filters.Source))
                {
                    appFilters.Add(new AppFilter { FieldName = "SOURCE", Operator = "=", FilterValue = filters.Source });
                }
            }

            var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), connectionName, "LIST_OF_VALUE");

            var entities = await repository.GetAsync(appFilters);
            return entities.Cast<LIST_OF_VALUE>()
                .OrderBy(lov => lov.VALUE_TYPE)
                .ThenBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                .ThenBy(lov => lov.VALUE_NAME)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// Gets all distinct VALUE_TYPEs
        /// </summary>
        public async Task<List<string>> GetValueTypesAsync(string connectionName = null)
        {
            connectionName ??= _connectionName;
            var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), connectionName, "LIST_OF_VALUE");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var entities = await repository.GetAsync(filters);
            return entities.Cast<LIST_OF_VALUE>()
                .Select(lov => lov.VALUE_TYPE)
                .Distinct()
                .OrderBy(vt => vt)
                .ToList();
        }

        /// <summary>
        /// Adds a new LOV
        /// </summary>
        public async Task<LIST_OF_VALUE> AddLOVAsync(LIST_OF_VALUE lov, string userId, string connectionName = null)
        {
            connectionName ??= _connectionName;
            if (string.IsNullOrEmpty(lov.LIST_OF_VALUE_ID))
            {
                lov.LIST_OF_VALUE_ID = Guid.NewGuid().ToString();
            }

            var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), connectionName, "LIST_OF_VALUE");

            if (lov is IPPDMEntity entity)
                _commonColumnHandler.PrepareForInsert(entity, userId);
            var result = await repository.InsertAsync(lov, userId);
            return result as LIST_OF_VALUE ?? throw new InvalidOperationException("Failed to insert LOV");
        }

        /// <summary>
        /// Updates an existing LOV
        /// </summary>
        public async Task<LIST_OF_VALUE> UpdateLOVAsync(LIST_OF_VALUE lov, string userId, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), connectionName, "LIST_OF_VALUE");

            if (lov is IPPDMEntity entity)
                _commonColumnHandler.PrepareForUpdate(entity, userId);
            var result = await repository.UpdateAsync(lov, userId);
            return result as LIST_OF_VALUE ?? throw new InvalidOperationException("Failed to update LOV");
        }

        /// <summary>
        /// Deletes an LOV (soft delete by setting ACTIVE_IND = 'N')
        /// </summary>
        public async Task<bool> DeleteLOVAsync(string lovId, string userId, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), connectionName, "LIST_OF_VALUE");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LIST_OF_VALUE_ID", Operator = "=", FilterValue = lovId }
            };

            var entities = await repository.GetAsync(filters);
            var lov = entities.Cast<LIST_OF_VALUE>().FirstOrDefault();
            if (lov == null)
            {
                return false;
            }

            // Soft delete
            lov.ACTIVE_IND = "N";
            if (lov is IPPDMEntity entity)
                _commonColumnHandler.PrepareForUpdate(entity, userId);
            await repository.UpdateAsync(lov, userId);
            return true;
        }

        /// <summary>
        /// Maps LIST_OF_VALUE entity to DTO
        /// </summary>
        private ListOfValueDto MapToDto(LIST_OF_VALUE lov)
        {
            return new ListOfValueDto
            {
                ListOfValueId = lov.LIST_OF_VALUE_ID,
                ValueType = lov.VALUE_TYPE,
                ValueCode = lov.VALUE_CODE,
                ValueName = lov.VALUE_NAME,
                Description = lov.DESCRIPTION,
                Category = lov.CATEGORY,
                Module = lov.MODULE,
                SortOrder = lov.SORT_ORDER,
                ParentValueId = lov.PARENT_VALUE_ID,
                IsDefault = lov.IS_DEFAULT,
                ActiveInd = lov.ACTIVE_IND,
                Source = lov.SOURCE
            };
        }
    }
}

