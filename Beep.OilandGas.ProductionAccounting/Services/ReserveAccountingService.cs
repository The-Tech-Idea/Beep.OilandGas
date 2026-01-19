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
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Reserve accounting service for maintaining proved reserves and depletion inputs.
    /// </summary>
    public class ReserveAccountingService : IReserveAccountingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ReserveAccountingService> _logger;
        private const string ConnectionName = "PPDM39";

        public ReserveAccountingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ReserveAccountingService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<PROVED_RESERVES> RecordReservesAsync(PROVED_RESERVES reserves, string userId, string cn = "PPDM39")
        {
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(reserves.PROPERTY_ID))
                throw new ProductionAccountingException("PROPERTY_ID is required for reserves");

            reserves.RESERVES_ID ??= Guid.NewGuid().ToString();
            reserves.RESERVE_DATE ??= DateTime.UtcNow.Date;
            reserves.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            reserves.PPDM_GUID ??= Guid.NewGuid().ToString();
            reserves.ROW_CREATED_BY = userId;
            reserves.ROW_CREATED_DATE = DateTime.UtcNow;

            var metadata = await _metadata.GetTableMetadataAsync("PROVED_RESERVES");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(PROVED_RESERVES);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "PROVED_RESERVES");

            await repo.InsertAsync(reserves, userId);
            return reserves;
        }

        public async Task<PROVED_RESERVES> GetLatestReservesAsync(string propertyId, DateTime? asOfDate = null, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));

            var metadata = await _metadata.GetTableMetadataAsync("PROVED_RESERVES");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(PROVED_RESERVES);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "PROVED_RESERVES");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var reserves = results?.Cast<PROVED_RESERVES>().ToList() ?? new List<PROVED_RESERVES>();

            if (asOfDate.HasValue)
            {
                reserves = reserves
                    .Where(r => !r.RESERVE_DATE.HasValue || r.RESERVE_DATE.Value.Date <= asOfDate.Value.Date)
                    .ToList();
            }

            return reserves
                .OrderByDescending(r => r.RESERVE_DATE ?? DateTime.MinValue)
                .FirstOrDefault();
        }

        public async Task<decimal> CalculateDepletionRateAsync(
            string propertyId,
            decimal netCapitalizedCosts,
            DateTime? asOfDate = null,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));
            if (netCapitalizedCosts <= 0)
                return 0m;

            var reserves = await GetLatestReservesAsync(propertyId, asOfDate, cn);
            if (reserves == null)
            {
                _logger?.LogWarning("No reserves found for property {PropertyId}", propertyId);
                return 0m;
            }

            decimal totalReserves =
                (reserves.PROVED_DEVELOPED_OIL_RESERVES ?? 0m) +
                (reserves.PROVED_UNDEVELOPED_OIL_RESERVES ?? 0m) +
                (reserves.PROVED_DEVELOPED_GAS_RESERVES ?? 0m) +
                (reserves.PROVED_UNDEVELOPED_GAS_RESERVES ?? 0m);

            if (totalReserves <= 0m)
                return 0m;

            return netCapitalizedCosts / totalReserves;
        }

        public Task<bool> ValidateReservesAsync(PROVED_RESERVES reserves, string cn = "PPDM39")
        {
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));
            if (string.IsNullOrWhiteSpace(reserves.PROPERTY_ID))
                throw new ProductionAccountingException("PROPERTY_ID is required");
            if (!reserves.RESERVE_DATE.HasValue)
                throw new ProductionAccountingException("RESERVE_DATE is required");

            bool valid = (reserves.PROVED_DEVELOPED_OIL_RESERVES ?? 0m) >= 0m &&
                         (reserves.PROVED_UNDEVELOPED_OIL_RESERVES ?? 0m) >= 0m &&
                         (reserves.PROVED_DEVELOPED_GAS_RESERVES ?? 0m) >= 0m &&
                         (reserves.PROVED_UNDEVELOPED_GAS_RESERVES ?? 0m) >= 0m;

            return Task.FromResult(valid);
        }

        public async Task<RESERVE_CASHFLOW> RecordCashflowAsync(
            RESERVE_CASHFLOW cashflow,
            string userId,
            string cn = "PPDM39")
        {
            if (cashflow == null)
                throw new ArgumentNullException(nameof(cashflow));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(cashflow.PROPERTY_ID))
                throw new ProductionAccountingException("PROPERTY_ID is required for cashflow");
            if (!cashflow.PERIOD_END_DATE.HasValue)
                throw new ProductionAccountingException("PERIOD_END_DATE is required for cashflow");

            cashflow.RESERVE_CASHFLOW_ID ??= Guid.NewGuid().ToString();
            cashflow.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            cashflow.PPDM_GUID ??= Guid.NewGuid().ToString();
            cashflow.ROW_CREATED_BY = userId;
            cashflow.ROW_CREATED_DATE = DateTime.UtcNow;

            var metadata = await _metadata.GetTableMetadataAsync("RESERVE_CASHFLOW");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(RESERVE_CASHFLOW);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "RESERVE_CASHFLOW");

            await repo.InsertAsync(cashflow, userId);
            return cashflow;
        }

        public async Task<List<RESERVE_CASHFLOW>> GetCashflowsAsync(
            string propertyId,
            DateTime? asOfDate = null,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));

            var metadata = await _metadata.GetTableMetadataAsync("RESERVE_CASHFLOW");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(RESERVE_CASHFLOW);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "RESERVE_CASHFLOW");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var cashflows = results?.Cast<RESERVE_CASHFLOW>().ToList() ?? new List<RESERVE_CASHFLOW>();

            if (asOfDate.HasValue)
            {
                var date = asOfDate.Value.Date;
                cashflows = cashflows
                    .Where(c => c.PERIOD_END_DATE.HasValue && c.PERIOD_END_DATE.Value.Date >= date)
                    .ToList();
            }

            return cashflows;
        }

        public async Task<decimal> CalculatePresentValueAsync(
            string propertyId,
            DateTime? asOfDate = null,
            string cn = "PPDM39")
        {
            var cashflows = await GetCashflowsAsync(propertyId, asOfDate, cn);
            if (cashflows.Count == 0)
                return 0m;

            var baseDate = asOfDate ?? DateTime.UtcNow.Date;
            decimal totalPv = 0m;

            foreach (var cashflow in cashflows)
            {
                if (!cashflow.PERIOD_END_DATE.HasValue)
                    continue;

                var netCashFlow = cashflow.NET_CASH_FLOW;
                if (!netCashFlow.HasValue)
                {
                    netCashFlow = CalculateNetCashFlow(cashflow);
                }

                var years = (decimal)(cashflow.PERIOD_END_DATE.Value.Date - baseDate.Date).TotalDays / 365m;
                if (years < 0m)
                    years = 0m;

                var rate = cashflow.DISCOUNT_RATE ?? 0.10m;
                if (rate > 1m)
                    rate /= 100m;

                var discountFactor = (decimal)Math.Pow((double)(1m + rate), (double)years);
                if (discountFactor <= 0m)
                    discountFactor = 1m;

                totalPv += netCashFlow.Value / discountFactor;
            }

            return totalPv;
        }

        private static decimal CalculateNetCashFlow(RESERVE_CASHFLOW cashflow)
        {
            var oilRevenue = (cashflow.OIL_VOLUME ?? 0m) * (cashflow.OIL_PRICE ?? 0m);
            var gasRevenue = (cashflow.GAS_VOLUME ?? 0m) * (cashflow.GAS_PRICE ?? 0m);
            var grossRevenue = oilRevenue + gasRevenue;

            var operating = cashflow.OPERATING_COST ?? 0m;
            var development = cashflow.DEVELOPMENT_COST ?? 0m;
            var abandonment = cashflow.ABANDONMENT_COST ?? 0m;

            var taxRate = cashflow.TAX_RATE ?? 0m;
            if (taxRate > 1m)
                taxRate /= 100m;
            var taxes = grossRevenue * taxRate;

            return grossRevenue - operating - development - abandonment - taxes;
        }
    }
}
