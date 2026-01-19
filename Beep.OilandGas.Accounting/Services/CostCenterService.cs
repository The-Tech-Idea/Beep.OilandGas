using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Cost center CRUD workflows.
    /// </summary>
    public class CostCenterService : ICostCenterService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<CostCenterService> _logger;
        private const string ConnectionName = "PPDM39";

        public CostCenterService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<CostCenterService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<COST_CENTER> CreateCostCenterAsync(COST_CENTER costCenter, string userId, string cn = "PPDM39")
        {
            if (costCenter == null)
                throw new ArgumentNullException(nameof(costCenter));
            if (string.IsNullOrWhiteSpace(costCenter.COST_CENTER_NAME))
                throw new InvalidOperationException("COST_CENTER_NAME is required");

            costCenter.COST_CENTER_ID ??= Guid.NewGuid().ToString();
            costCenter.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            costCenter.PPDM_GUID ??= Guid.NewGuid().ToString();
            costCenter.ROW_CREATED_BY = userId;
            costCenter.ROW_CREATED_DATE = DateTime.UtcNow;

            var repo = await GetRepoAsync<COST_CENTER>("COST_CENTER", cn);
            await repo.InsertAsync(costCenter, userId);
            return costCenter;
        }

        public async Task<COST_CENTER?> GetCostCenterAsync(string costCenterId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(costCenterId))
                throw new ArgumentNullException(nameof(costCenterId));

            var repo = await GetRepoAsync<COST_CENTER>("COST_CENTER", cn);
            var result = await repo.GetByIdAsync(costCenterId);
            return result as COST_CENTER;
        }

        public async Task<List<COST_CENTER>> GetCostCentersAsync(string? fieldId, string cn = "PPDM39")
        {
            var repo = await GetRepoAsync<COST_CENTER>("COST_CENTER", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrWhiteSpace(fieldId))
                filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });

            var results = await repo.GetAsync(filters);
            return results?.Cast<COST_CENTER>().ToList() ?? new List<COST_CENTER>();
        }

        public async Task<COST_CENTER> UpdateCostCenterAsync(COST_CENTER costCenter, string userId, string cn = "PPDM39")
        {
            if (costCenter == null)
                throw new ArgumentNullException(nameof(costCenter));
            if (string.IsNullOrWhiteSpace(costCenter.COST_CENTER_ID))
                throw new InvalidOperationException("COST_CENTER_ID is required");

            var repo = await GetRepoAsync<COST_CENTER>("COST_CENTER", cn);
            costCenter.ROW_CHANGED_BY = userId;
            costCenter.ROW_CHANGED_DATE = DateTime.UtcNow;
            await repo.UpdateAsync(costCenter, userId);
            return costCenter;
        }

        public async Task<bool> DeactivateCostCenterAsync(string costCenterId, string userId, string cn = "PPDM39")
        {
            var costCenter = await GetCostCenterAsync(costCenterId, cn);
            if (costCenter == null)
                return false;

            costCenter.ACTIVE_IND = _defaults.GetActiveIndicatorNo();
            costCenter.ROW_CHANGED_BY = userId;
            costCenter.ROW_CHANGED_DATE = DateTime.UtcNow;

            var repo = await GetRepoAsync<COST_CENTER>("COST_CENTER", cn);
            await repo.UpdateAsync(costCenter, userId);
            return true;
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, tableName);
        }
    }
}
