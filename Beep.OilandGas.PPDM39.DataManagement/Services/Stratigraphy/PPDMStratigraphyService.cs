using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Stratigraphy
{
    /// <summary>
    /// Service for Stratigraphy data management
    /// </summary>
    public class PPDMStratigraphyService : IPPDMStratigraphyService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly PPDMGenericRepository _stratColumnRepository;
        private readonly PPDMGenericRepository _stratUnitRepository;
        private readonly PPDMGenericRepository _stratHierarchyRepository;
        private readonly PPDMGenericRepository _stratWellSectionRepository;

        public PPDMStratigraphyService(
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

            // Create repositories for each table
            _stratColumnRepository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(STRAT_COLUMN), _connectionName, "STRAT_COLUMN");

            _stratUnitRepository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(STRAT_UNIT), _connectionName, "STRAT_UNIT");

            _stratHierarchyRepository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(STRAT_HIERARCHY), _connectionName, "STRAT_HIERARCHY");

            _stratWellSectionRepository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(STRAT_WELL_SECTION), _connectionName, "STRAT_WELL_SECTION");
        }

        public async Task<List<STRAT_COLUMN>> GetStratigraphicColumnsAsync(List<AppFilter> filters = null)
        {
            var results = await _stratColumnRepository.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<STRAT_COLUMN>().ToList();
        }

        public async Task<STRAT_COLUMN> GetStratigraphicColumnByIdAsync(string columnId)
        {
            var result = await _stratColumnRepository.GetByIdAsync(columnId);
            return result as STRAT_COLUMN;
        }

        public async Task<List<STRAT_UNIT>> GetStratigraphicUnitsAsync(List<AppFilter> filters = null)
        {
            var results = await _stratUnitRepository.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<STRAT_UNIT>().ToList();
        }

        public async Task<STRAT_UNIT> GetStratigraphicUnitByIdAsync(string unitId)
        {
            var result = await _stratUnitRepository.GetByIdAsync(unitId);
            return result as STRAT_UNIT;
        }

        public async Task<List<STRAT_HIERARCHY>> GetStratigraphicHierarchyAsync(List<AppFilter> filters = null)
        {
            var results = await _stratHierarchyRepository.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<STRAT_HIERARCHY>().ToList();
        }

        public async Task<List<STRAT_WELL_SECTION>> GetWellSectionsAsync(List<AppFilter> filters = null)
        {
            var results = await _stratWellSectionRepository.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<STRAT_WELL_SECTION>().ToList();
        }

        public async Task<List<STRAT_WELL_SECTION>> GetWellSectionsByWellAsync(string uwi)
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = uwi, Operator = "=" }
            };
            return await GetWellSectionsAsync(filters);
        }

        public async Task<STRAT_COLUMN> CreateStratigraphicColumnAsync(STRAT_COLUMN column, string userId)
        {
            var result = await _stratColumnRepository.InsertAsync(column, userId);
            return result as STRAT_COLUMN;
        }

        public async Task<STRAT_COLUMN> UpdateStratigraphicColumnAsync(STRAT_COLUMN column, string userId)
        {
            var result = await _stratColumnRepository.UpdateAsync(column, userId);
            return result as STRAT_COLUMN;
        }

        public async Task<STRAT_UNIT> CreateStratigraphicUnitAsync(STRAT_UNIT unit, string userId)
        {
            var result = await _stratUnitRepository.InsertAsync(unit, userId);
            return result as STRAT_UNIT;
        }

        public async Task<STRAT_UNIT> UpdateStratigraphicUnitAsync(STRAT_UNIT unit, string userId)
        {
            var result = await _stratUnitRepository.UpdateAsync(unit, userId);
            return result as STRAT_UNIT;
        }
    }
}



