using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Production
{
    /// <summary>
    /// Service for Production & Reserves data management
    /// </summary>
    public class PPDMProductionService : IPPDMProductionService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;

        public PPDMProductionService(
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
        }

        public async Task<List<object>> GetFieldsAsync(List<AppFilter> filters = null)
        {
            // Get FIELD table metadata
            var fieldMetadata = await _metadata.GetTableMetadataAsync("FIELD");
            if (fieldMetadata == null)
                return new List<object>();

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{fieldMetadata.EntityTypeName}");
            if (entityType == null)
                return new List<object>();

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "FIELD");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.ToList();
        }

        public async Task<List<object>> GetPoolsAsync(List<AppFilter> filters = null)
        {
            var poolMetadata = await _metadata.GetTableMetadataAsync("POOL");
            if (poolMetadata == null)
                return new List<object>();

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{poolMetadata.EntityTypeName}");
            if (entityType == null)
                return new List<object>();

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "POOL");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.ToList();
        }

        public async Task<List<object>> GetProductionAsync(List<AppFilter> filters = null)
        {
            var productionMetadata = await _metadata.GetTableMetadataAsync("PRODUCTION");
            if (productionMetadata == null)
                return new List<object>();

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{productionMetadata.EntityTypeName}");
            if (entityType == null)
                return new List<object>();

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "PRODUCTION");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.ToList();
        }

        public async Task<List<object>> GetReservesAsync(List<AppFilter> filters = null)
        {
            var reservesMetadata = await _metadata.GetTableMetadataAsync("RESERVES");
            if (reservesMetadata == null)
                return new List<object>();

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{reservesMetadata.EntityTypeName}");
            if (entityType == null)
                return new List<object>();

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "RESERVES");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.ToList();
        }

        public async Task<List<object>> GetProductionReportingAsync(List<AppFilter> filters = null)
        {
            var reportingMetadata = await _metadata.GetTableMetadataAsync("PRODUCTION_REPORTING");
            if (reportingMetadata == null)
                return new List<object>();

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{reportingMetadata.EntityTypeName}");
            if (entityType == null)
                return new List<object>();

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "PRODUCTION_REPORTING");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.ToList();
        }
    }
}


