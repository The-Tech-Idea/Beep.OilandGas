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
    /// IFRS 16 lease accounting service.
    /// </summary>
    public class LeasingService : ILeasingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<LeasingService> _logger;

        public LeasingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<LeasingService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<LEASE_ACCOUNTING_ENTRY> RecognizeLeaseAsync(
            LEASE_CONTRACT leaseContract,
            IEnumerable<LEASE_PAYMENT> payments,
            DateTime commencementDate,
            string userId,
            string cn = "PPDM39")
        {
            if (leaseContract == null)
                throw new ArgumentNullException(nameof(leaseContract));
            if (payments == null)
                throw new ArgumentNullException(nameof(payments));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var discountRate = leaseContract.DISCOUNT_RATE ?? 0.0m;
            var pv = CalculatePresentValue(payments, discountRate, commencementDate);

            var entry = new LEASE_ACCOUNTING_ENTRY
            {
                LEASE_ACCOUNTING_ENTRY_ID = Guid.NewGuid().ToString(),
                LEASE_ID = leaseContract.LEASE_ID,
                MEASUREMENT_DATE = commencementDate,
                ROU_ASSET = pv,
                LEASE_LIABILITY = pv,
                INTEREST_EXPENSE = 0m,
                AMORTIZATION_EXPENSE = 0m,
                CURRENCY_CODE = leaseContract.CURRENCY_CODE ?? "USD",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var metadata = await _metadata.GetTableMetadataAsync("LEASE_ACCOUNTING_ENTRY");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(LEASE_ACCOUNTING_ENTRY);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "LEASE_ACCOUNTING_ENTRY");

            await repo.InsertAsync(entry, userId);

            _logger?.LogInformation(
                "Recognized lease {LeaseId} with initial ROU asset {RouAsset}",
                leaseContract.LEASE_ID, pv);

            return entry;
        }

        public async Task<LEASE_ACCOUNTING_ENTRY> RemeasureLeaseAsync(
            string leaseId,
            DateTime measurementDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var lease = await GetLeaseAsync(leaseId, cn);
            if (lease == null)
                return null;

            var payments = await GetPaymentsAsync(leaseId, cn);
            var discountRate = lease.DISCOUNT_RATE ?? 0.0m;
            var pv = CalculatePresentValue(payments, discountRate, measurementDate);

            var entry = new LEASE_ACCOUNTING_ENTRY
            {
                LEASE_ACCOUNTING_ENTRY_ID = Guid.NewGuid().ToString(),
                LEASE_ID = leaseId,
                MEASUREMENT_DATE = measurementDate,
                ROU_ASSET = pv,
                LEASE_LIABILITY = pv,
                INTEREST_EXPENSE = 0m,
                AMORTIZATION_EXPENSE = 0m,
                CURRENCY_CODE = lease.CURRENCY_CODE ?? "USD",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var metadata = await _metadata.GetTableMetadataAsync("LEASE_ACCOUNTING_ENTRY");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(LEASE_ACCOUNTING_ENTRY);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "LEASE_ACCOUNTING_ENTRY");

            await repo.InsertAsync(entry, userId);

            _logger?.LogInformation(
                "Remeasured lease {LeaseId} at {MeasurementDate} with liability {Liability}",
                leaseId, measurementDate.ToShortDateString(), pv);

            return entry;
        }

        private async Task<LEASE_CONTRACT> GetLeaseAsync(string leaseId, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("LEASE_CONTRACT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(LEASE_CONTRACT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "LEASE_CONTRACT");

            var result = await repo.GetByIdAsync(leaseId);
            return result as LEASE_CONTRACT;
        }

        private async Task<List<LEASE_PAYMENT>> GetPaymentsAsync(string leaseId, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("LEASE_PAYMENT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(LEASE_PAYMENT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "LEASE_PAYMENT");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<LEASE_PAYMENT>().OrderBy(p => p.PAYMENT_DATE).ToList()
                ?? new List<LEASE_PAYMENT>();
        }

        private static decimal CalculatePresentValue(
            IEnumerable<LEASE_PAYMENT> payments,
            decimal discountRate,
            DateTime baseDate)
        {
            decimal pv = 0m;
            foreach (var payment in payments)
            {
                if (payment.PAYMENT_DATE == null || payment.PAYMENT_AMOUNT == null)
                    continue;

                var days = (payment.PAYMENT_DATE.Value - baseDate).TotalDays;
                if (days < 0)
                    continue;

                var years = days / 365.0;
                var discountFactor = (decimal)Math.Pow(1 + (double)discountRate, years);
                pv += payment.PAYMENT_AMOUNT.Value / discountFactor;
            }

            return pv;
        }
    }
}
