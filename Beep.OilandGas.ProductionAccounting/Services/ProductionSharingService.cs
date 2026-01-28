using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Production sharing agreement (PSA) service.
    /// </summary>
    public class ProductionSharingService : IProductionSharingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ProductionSharingService> _logger;

        public ProductionSharingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ProductionSharingService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<PRODUCTION_SHARING_AGREEMENT> GetActiveAgreementAsync(
            string propertyId,
            DateTime asOfDate,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PRODUCTION_SHARING_AGREEMENT");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(PRODUCTION_SHARING_AGREEMENT);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "PRODUCTION_SHARING_AGREEMENT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var agreements = results?.Cast<PRODUCTION_SHARING_AGREEMENT>().ToList()
                    ?? new List<PRODUCTION_SHARING_AGREEMENT>();

                return agreements
                    .Where(a => (a.EFFECTIVE_DATE == null || a.EFFECTIVE_DATE <= asOfDate)
                        && (a.EXPIRY_DATE == null || a.EXPIRY_DATE >= asOfDate))
                    .OrderByDescending(a => a.EFFECTIVE_DATE)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "PSA agreement lookup failed for property {PropertyId}", propertyId);
                return null;
            }
        }

        public async Task<PRODUCTION_SHARING_ENTITLEMENT> CalculateEntitlementAsync(
            ALLOCATION_DETAIL ALLOCATION_DETAIL,
            DateTime productionDate,
            string userId,
            string cn = "PPDM39")
        {
            if (ALLOCATION_DETAIL == null)
                throw new ArgumentNullException(nameof(ALLOCATION_DETAIL));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            try
            {
                var RUN_TICKET = await GetRunTicketAsync(ALLOCATION_DETAIL, cn);
                if (RUN_TICKET == null || string.IsNullOrWhiteSpace(RUN_TICKET.LEASE_ID))
                    return null;

                var agreement = await GetActiveAgreementAsync(RUN_TICKET.LEASE_ID, productionDate, cn);
                if (agreement == null)
                    return null;

                var totalVolume = ALLOCATION_DETAIL.ALLOCATED_VOLUME ?? 0m;
                if (totalVolume <= 0m)
                    return null;

                var costRecoveryLimit = NormalizePercent(agreement.COST_RECOVERY_LIMIT_PCT, 0.6m);
                var governmentSplit = NormalizePercent(agreement.GOVERNMENT_PROFIT_SPLIT_PCT, 0.5m);
                var contractorSplit = NormalizePercent(agreement.CONTRACTOR_PROFIT_SPLIT_PCT, 1m - governmentSplit);

                if (governmentSplit + contractorSplit == 0m)
                {
                    governmentSplit = 0.5m;
                    contractorSplit = 0.5m;
                }

                var costOilVolume = totalVolume * costRecoveryLimit;
                var profitVolume = Math.Max(0m, totalVolume - costOilVolume);
                var contractorVolume = costOilVolume + (profitVolume * contractorSplit);
                var governmentVolume = profitVolume * governmentSplit;

                var entitlement = new PRODUCTION_SHARING_ENTITLEMENT
                {
                    PSA_ENTITLEMENT_ID = Guid.NewGuid().ToString(),
                    PSA_ID = agreement.PSA_ID,
                    PROPERTY_ID = RUN_TICKET.LEASE_ID,
                    ALLOCATION_DETAIL_ID = ALLOCATION_DETAIL.ALLOCATION_DETAIL_ID,
                    PRODUCTION_DATE = productionDate,
                    TOTAL_VOLUME = totalVolume,
                    COST_OIL_VOLUME = costOilVolume,
                    PROFIT_OIL_VOLUME = profitVolume,
                    CONTRACTOR_VOLUME = contractorVolume,
                    GOVERNMENT_VOLUME = governmentVolume,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var metadata = await _metadata.GetTableMetadataAsync("PRODUCTION_SHARING_ENTITLEMENT");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(PRODUCTION_SHARING_ENTITLEMENT);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "PRODUCTION_SHARING_ENTITLEMENT");

                await repo.InsertAsync(entitlement, userId);

                _logger?.LogInformation(
                    "PSA entitlement calculated for allocation {AllocationDetailId}: ContractorVolume={Contractor}, GovVolume={Government}",
                    ALLOCATION_DETAIL.ALLOCATION_DETAIL_ID, contractorVolume, governmentVolume);

                return entitlement;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "PSA entitlement calculation failed for allocation {AllocationDetailId}", ALLOCATION_DETAIL.ALLOCATION_DETAIL_ID);
                return null;
            }
        }

        private async Task<RUN_TICKET> GetRunTicketAsync(ALLOCATION_DETAIL ALLOCATION_DETAIL, string cn)
        {
            if (string.IsNullOrWhiteSpace(ALLOCATION_DETAIL.ALLOCATION_RESULT_ID))
                return null;

            var allocationMetadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
            var allocationEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{allocationMetadata.EntityTypeName}")
                ?? typeof(ALLOCATION_RESULT);

            var allocationRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                allocationEntityType, cn, "ALLOCATION_RESULT");

            var ALLOCATION_RESULT = await allocationRepo.GetByIdAsync(ALLOCATION_DETAIL.ALLOCATION_RESULT_ID) as ALLOCATION_RESULT;
            if (ALLOCATION_RESULT == null || string.IsNullOrWhiteSpace(ALLOCATION_RESULT.ALLOCATION_REQUEST_ID))
                return null;

            var runTicketMetadata = await _metadata.GetTableMetadataAsync("RUN_TICKET");
            var runTicketEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{runTicketMetadata.EntityTypeName}")
                ?? typeof(RUN_TICKET);

            var runTicketRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                runTicketEntityType, cn, "RUN_TICKET");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_REQUEST_ID", Operator = "=", FilterValue = ALLOCATION_RESULT.ALLOCATION_REQUEST_ID },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await runTicketRepo.GetAsync(filters);
            return results?.Cast<RUN_TICKET>().FirstOrDefault();
        }

        private static decimal NormalizePercent(decimal? rawPercent, decimal defaultValue)
        {
            if (!rawPercent.HasValue)
                return defaultValue;

            var value = rawPercent.Value;
            if (value > 1m)
                value /= 100m;
            if (value < 0m)
                value = 0m;
            if (value > 1m)
                value = 1m;
            return value;
        }
    }
}
