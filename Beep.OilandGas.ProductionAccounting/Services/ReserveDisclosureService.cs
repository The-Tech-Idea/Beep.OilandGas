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

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// IFRS-style reserve and resource disclosure service.
    /// </summary>
    public class ReserveDisclosureService : IReserveDisclosureService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ReserveDisclosureService> _logger;

        public ReserveDisclosureService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ReserveDisclosureService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<RESERVE_DISCLOSURE_PACKAGE> BuildDisclosureAsync(
            string propertyId,
            DateTime? asOfDate = null,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));

            var effectiveDate = asOfDate ?? DateTime.UtcNow;

            var reserves = await GetLatestReservesAsync(propertyId, effectiveDate, cn);
            var cashflows = await GetCashflowsAsync(propertyId, effectiveDate, cn);

            var totalOil = (reserves?.PROVED_DEVELOPED_OIL_RESERVES ?? 0m)
                + (reserves?.PROVED_UNDEVELOPED_OIL_RESERVES ?? 0m);
            var totalGas = (reserves?.PROVED_DEVELOPED_GAS_RESERVES ?? 0m)
                + (reserves?.PROVED_UNDEVELOPED_GAS_RESERVES ?? 0m);

            var pv10 = CalculatePv(cashflows, 0.10m, effectiveDate);

            var disclosure = new RESERVE_DISCLOSURE_PACKAGE
            {
                RESERVE_DISCLOSURE_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = propertyId,
                AS_OF_DATE = effectiveDate,
                TOTAL_PROVED_OIL = totalOil,
                TOTAL_PROVED_GAS = totalGas,
                PV10 = pv10,
                DISCOUNT_RATE = 0.10m,
                DISCLOSURE_NOTES = "IFRS-style reserve disclosure package generated from proved reserves and cashflows.",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = "system",
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var metadata = await _metadata.GetTableMetadataAsync("RESERVE_DISCLOSURE_PACKAGE");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(RESERVE_DISCLOSURE_PACKAGE);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "RESERVE_DISCLOSURE_PACKAGE");

            await repo.InsertAsync(disclosure, disclosure.ROW_CREATED_BY);

            _logger?.LogInformation(
                "Generated reserve disclosure package {DisclosureId} for property {PropertyId}",
                disclosure.RESERVE_DISCLOSURE_ID, propertyId);

            return disclosure;
        }

        private async Task<PROVED_RESERVES> GetLatestReservesAsync(
            string propertyId,
            DateTime asOfDate,
            string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("PROVED_RESERVES");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(PROVED_RESERVES);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "PROVED_RESERVES");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "RESERVE_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var reserveList = results?.Cast<PROVED_RESERVES>().OrderByDescending(r => r.RESERVE_DATE).ToList()
                ?? new List<PROVED_RESERVES>();

            return reserveList.FirstOrDefault();
        }

        private async Task<List<RESERVE_CASHFLOW>> GetCashflowsAsync(
            string propertyId,
            DateTime asOfDate,
            string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("RESERVE_CASHFLOW");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(RESERVE_CASHFLOW);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "RESERVE_CASHFLOW");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "PERIOD_END_DATE", Operator = ">=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<RESERVE_CASHFLOW>().OrderBy(r => r.PERIOD_END_DATE).ToList()
                ?? new List<RESERVE_CASHFLOW>();
        }

        private static decimal CalculatePv(
            List<RESERVE_CASHFLOW> cashflows,
            decimal discountRate,
            DateTime asOfDate)
        {
            if (cashflows == null || cashflows.Count == 0)
                return 0m;

            decimal pv = 0m;
            foreach (var cf in cashflows)
            {
                if (cf.PERIOD_END_DATE == null || cf.NET_CASH_FLOW == null)
                    continue;

                var days = (cf.PERIOD_END_DATE.Value - asOfDate).TotalDays;
                if (days < 0)
                    continue;

                var years = days / 365.0;
                var discountFactor = (decimal)Math.Pow(1 + (double)discountRate, years);
                pv += cf.NET_CASH_FLOW.Value / discountFactor;
            }

            return pv;
        }
    }
}
