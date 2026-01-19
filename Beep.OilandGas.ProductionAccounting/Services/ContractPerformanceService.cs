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
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Tracks ASC 606 performance obligations for sales contracts.
    /// </summary>
    public class ContractPerformanceService : IContractPerformanceService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ContractPerformanceService> _logger;
        private const string ConnectionName = "PPDM39";

        public ContractPerformanceService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ContractPerformanceService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<CONTRACT_PERFORMANCE_OBLIGATION> CreateObligationAsync(CONTRACT_PERFORMANCE_OBLIGATION obligation, string userId, string cn = "PPDM39")
        {
            if (obligation == null)
                throw new ArgumentNullException(nameof(obligation));
            if (string.IsNullOrWhiteSpace(obligation.SALES_CONTRACT_ID))
                throw new ProductionAccountingException("SALES_CONTRACT_ID is required");

            obligation.CONTRACT_PERFORMANCE_OBLIGATION_ID ??= Guid.NewGuid().ToString();
            obligation.STATUS ??= "OPEN";
            obligation.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            obligation.PPDM_GUID ??= Guid.NewGuid().ToString();
            obligation.ROW_CREATED_BY = userId;
            obligation.ROW_CREATED_DATE = DateTime.UtcNow;

            var repo = await GetRepoAsync<CONTRACT_PERFORMANCE_OBLIGATION>("CONTRACT_PERFORMANCE_OBLIGATION", cn);
            await repo.InsertAsync(obligation, userId);
            return obligation;
        }

        public async Task<CONTRACT_PERFORMANCE_OBLIGATION> MarkSatisfiedAsync(string obligationId, DateTime satisfiedDate, string userId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(obligationId))
                throw new ArgumentNullException(nameof(obligationId));

            var repo = await GetRepoAsync<CONTRACT_PERFORMANCE_OBLIGATION>("CONTRACT_PERFORMANCE_OBLIGATION", cn);
            var obligation = await repo.GetByIdAsync(obligationId) as CONTRACT_PERFORMANCE_OBLIGATION;
            if (obligation == null)
                throw new ProductionAccountingException($"Obligation not found: {obligationId}");

            obligation.SATISFIED_DATE = satisfiedDate;
            obligation.STATUS = "SATISFIED";
            obligation.ROW_CHANGED_BY = userId;
            obligation.ROW_CHANGED_DATE = DateTime.UtcNow;

            await repo.UpdateAsync(obligation, userId);
            return obligation;
        }

        public async Task<List<CONTRACT_PERFORMANCE_OBLIGATION>> GetOutstandingAsync(string salesContractId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(salesContractId))
                throw new ArgumentNullException(nameof(salesContractId));

            var repo = await GetRepoAsync<CONTRACT_PERFORMANCE_OBLIGATION>("CONTRACT_PERFORMANCE_OBLIGATION", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "SALES_CONTRACT_ID", Operator = "=", FilterValue = salesContractId },
                new AppFilter { FieldName = "STATUS", Operator = "!=", FilterValue = "SATISFIED" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<CONTRACT_PERFORMANCE_OBLIGATION>().ToList()
                ?? new List<CONTRACT_PERFORMANCE_OBLIGATION>();
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
