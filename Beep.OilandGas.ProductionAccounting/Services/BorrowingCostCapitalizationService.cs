using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// IAS 23 borrowing cost capitalization service.
    /// </summary>
    public class BorrowingCostCapitalizationService : IBorrowingCostCapitalizationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<BorrowingCostCapitalizationService> _logger;

        public BorrowingCostCapitalizationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<BorrowingCostCapitalizationService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<ACCOUNTING_COST> CapitalizeBorrowingCostAsync(
            ACCOUNTING_COST cost,
            DateTime periodStart,
            DateTime periodEnd,
            string userId,
            string cn = "PPDM39")
        {
            if (cost == null)
                throw new ArgumentNullException(nameof(cost));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            cost.ACCOUNTING_COST_ID ??= Guid.NewGuid().ToString();
            cost.COST_CATEGORY = "BORROWING_COST";
            cost.IS_CAPITALIZED = _defaults.GetActiveIndicatorYes();
            cost.IS_EXPENSED = "N";
            cost.COST_DATE = periodEnd;
            cost.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            cost.PPDM_GUID ??= Guid.NewGuid().ToString();
            cost.ROW_CREATED_BY = userId;
            cost.ROW_CREATED_DATE = DateTime.UtcNow;

            var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ACCOUNTING_COST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ACCOUNTING_COST");

            await repo.InsertAsync(cost, userId);

            _logger?.LogInformation(
                "Capitalized borrowing cost {CostId} for period {Start} - {End}",
                cost.ACCOUNTING_COST_ID, periodStart.ToShortDateString(), periodEnd.ToShortDateString());

            return cost;
        }
    }
}
