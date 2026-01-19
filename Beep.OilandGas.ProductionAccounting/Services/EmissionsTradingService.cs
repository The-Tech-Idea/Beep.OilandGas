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
    /// Emissions trading and carbon accounting service.
    /// </summary>
    public class EmissionsTradingService : IEmissionsTradingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<EmissionsTradingService> _logger;

        public EmissionsTradingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<EmissionsTradingService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<EMISSIONS_OBLIGATION> UpdateObligationAsync(
            EMISSIONS_OBLIGATION obligation,
            decimal emissionsVolume,
            decimal allowancePrice,
            DateTime periodEnd,
            string userId,
            string cn = "PPDM39")
        {
            if (obligation == null)
                throw new ArgumentNullException(nameof(obligation));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            obligation.EMISSIONS_OBLIGATION_ID ??= Guid.NewGuid().ToString();
            obligation.PERIOD_END = periodEnd;
            obligation.EMISSIONS_VOLUME = emissionsVolume;
            obligation.ALLOWANCE_PRICE = allowancePrice;
            obligation.LIABILITY_AMOUNT = emissionsVolume * allowancePrice;
            obligation.STATUS = "OPEN";
            obligation.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            obligation.PPDM_GUID ??= Guid.NewGuid().ToString();
            obligation.ROW_CREATED_BY = userId;
            obligation.ROW_CREATED_DATE = DateTime.UtcNow;

            var metadata = await _metadata.GetTableMetadataAsync("EMISSIONS_OBLIGATION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(EMISSIONS_OBLIGATION);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "EMISSIONS_OBLIGATION");

            var existing = await repo.GetByIdAsync(obligation.EMISSIONS_OBLIGATION_ID) as EMISSIONS_OBLIGATION;
            if (existing == null)
                await repo.InsertAsync(obligation, userId);
            else
                await repo.UpdateAsync(obligation, userId);

            _logger?.LogInformation(
                "Updated emissions obligation {ObligationId} for period {PeriodEnd}",
                obligation.EMISSIONS_OBLIGATION_ID, periodEnd.ToShortDateString());

            return obligation;
        }

        public async Task<EMISSIONS_SETTLEMENT> SettleAsync(
            string obligationId,
            decimal allowancesSurrendered,
            DateTime settlementDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(obligationId))
                throw new ArgumentNullException(nameof(obligationId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var settlement = new EMISSIONS_SETTLEMENT
            {
                EMISSIONS_SETTLEMENT_ID = Guid.NewGuid().ToString(),
                EMISSIONS_OBLIGATION_ID = obligationId,
                SETTLEMENT_DATE = settlementDate,
                ALLOWANCES_SURRENDERED = allowancesSurrendered,
                SETTLEMENT_VALUE = 0m,
                STATUS = "SETTLED",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var metadata = await _metadata.GetTableMetadataAsync("EMISSIONS_SETTLEMENT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(EMISSIONS_SETTLEMENT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "EMISSIONS_SETTLEMENT");

            await repo.InsertAsync(settlement, userId);

            _logger?.LogInformation(
                "Settled emissions obligation {ObligationId} with {Allowances} allowances",
                obligationId, allowancesSurrendered);

            return settlement;
        }
    }
}
