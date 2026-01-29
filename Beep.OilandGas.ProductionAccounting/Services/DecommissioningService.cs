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
    /// IAS 37 decommissioning and ARO service.
    /// </summary>
    public class DecommissioningService : IDecommissioningService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<DecommissioningService> _logger;

        public DecommissioningService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<DecommissioningService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<ASSET_RETIREMENT_OBLIGATION> UpdateAroEstimateAsync(
            ASSET_RETIREMENT_OBLIGATION obligation,
            DateTime asOfDate,
            string userId,
            string cn = "PPDM39")
        {
            if (obligation == null)
                throw new ArgumentNullException(nameof(obligation));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            obligation.ARO_ID ??= Guid.NewGuid().ToString();
            obligation.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            obligation.PPDM_GUID ??= Guid.NewGuid().ToString();

            if (obligation.ESTIMATED_RETIREMENT_DATE.HasValue
                && obligation.ESTIMATED_COST > 0m
                && obligation.DISCOUNT_RATE > 0m)
            {
                var years = Math.Max(0.0,
                    (obligation.ESTIMATED_RETIREMENT_DATE.Value - asOfDate).TotalDays / 365.0);
                var discountFactor = Math.Pow(1 + (double)obligation.DISCOUNT_RATE, years);
                obligation.PRESENT_VALUE = obligation.ESTIMATED_COST / (decimal)discountFactor;
            }

            obligation.ROW_CHANGED_BY = userId;
            obligation.ROW_CHANGED_DATE = DateTime.UtcNow;

            var metadata = await _metadata.GetTableMetadataAsync("ASSET_RETIREMENT_OBLIGATION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ASSET_RETIREMENT_OBLIGATION);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ASSET_RETIREMENT_OBLIGATION");

            var existing = await repo.GetByIdAsync(obligation.ARO_ID) as ASSET_RETIREMENT_OBLIGATION;
            if (existing == null)
            {
                obligation.ROW_CREATED_BY = userId;
                obligation.ROW_CREATED_DATE = DateTime.UtcNow;
                await repo.InsertAsync(obligation, userId);
            }
            else
            {
                obligation.ROW_CHANGED_BY = userId;
                obligation.ROW_CHANGED_DATE = DateTime.UtcNow;
                await repo.UpdateAsync(obligation, userId);
            }

            _logger?.LogInformation(
                "Updated ARO estimate {AroId} with present value {PresentValue}",
                obligation.ARO_ID, obligation.PRESENT_VALUE);

            return obligation;
        }

        public async Task<ASSET_RETIREMENT_OBLIGATION> AccreteAroAsync(
            string aroId,
            DateTime periodEnd,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(aroId))
                throw new ArgumentNullException(nameof(aroId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var metadata = await _metadata.GetTableMetadataAsync("ASSET_RETIREMENT_OBLIGATION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ASSET_RETIREMENT_OBLIGATION);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ASSET_RETIREMENT_OBLIGATION");

            var existing = await repo.GetByIdAsync(aroId) as ASSET_RETIREMENT_OBLIGATION;
            if (existing == null)
                return null;

            if (existing.PRESENT_VALUE > 0m && existing.DISCOUNT_RATE > 0m)
            {
                var accretion = existing.PRESENT_VALUE * existing.DISCOUNT_RATE / 12m;
                existing.ACCRETION_EXPENSE = accretion;
                existing.PRESENT_VALUE += accretion;
            }

            existing.ROW_CHANGED_BY = userId;
            existing.ROW_CHANGED_DATE = DateTime.UtcNow;

            await repo.UpdateAsync(existing, userId);

            _logger?.LogInformation(
                "Accreted ARO {AroId} for period {PeriodEnd}",
                aroId, periodEnd.ToShortDateString());

            return existing;
        }
    }
}
