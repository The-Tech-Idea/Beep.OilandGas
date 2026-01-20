using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// IAS 24 related party disclosure rollups from AR/AP/Invoice/Lease activity.
    /// </summary>
    public class RelatedPartyDisclosureService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<RelatedPartyDisclosureService> _logger;
        private const string ConnectionName = "PPDM39";

        public RelatedPartyDisclosureService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<RelatedPartyDisclosureService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GenerateDisclosureAsync(
            List<string> relatedPartyBaIds,
            DateTime periodStart,
            DateTime periodEnd,
            string? connectionName = null)
        {
            if (relatedPartyBaIds == null || relatedPartyBaIds.Count == 0)
                throw new ArgumentException("Related party BA IDs are required", nameof(relatedPartyBaIds));

            var cn = connectionName ?? ConnectionName;
            var arInvoices = await GetArInvoicesAsync(relatedPartyBaIds, periodStart, periodEnd, cn);
            var apInvoices = await GetApInvoicesAsync(relatedPartyBaIds, periodStart, periodEnd, cn);
            var invoices = await GetInvoicesAsync(relatedPartyBaIds, periodStart, periodEnd, cn);
            var leaseContracts = await GetLeasesAsync(relatedPartyBaIds, periodStart, periodEnd, cn);

            var sb = new StringBuilder();
            sb.AppendLine("RELATED PARTY DISCLOSURE SUMMARY");
            sb.AppendLine($"Period: {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}");
            sb.AppendLine();

            foreach (var partyId in relatedPartyBaIds)
            {
                var arTotal = arInvoices.Where(x => string.Equals(x.CUSTOMER_BA_ID, partyId, StringComparison.OrdinalIgnoreCase))
                    .Sum(x => x.TOTAL_AMOUNT ?? 0m);
                var apTotal = apInvoices.Where(x => string.Equals(x.VENDOR_BA_ID, partyId, StringComparison.OrdinalIgnoreCase))
                    .Sum(x => x.TOTAL_AMOUNT ?? 0m);
                var invoiceTotal = invoices.Where(x => string.Equals(x.CUSTOMER_BA_ID, partyId, StringComparison.OrdinalIgnoreCase))
                    .Sum(x => x.TOTAL_AMOUNT ?? 0m);
                var leaseCount = leaseContracts.Count(x => string.Equals(x.LESSOR_BA_ID, partyId, StringComparison.OrdinalIgnoreCase));

                sb.AppendLine($"Party: {partyId}");
                sb.AppendLine($"  AR Invoices: {arTotal:N2}");
                sb.AppendLine($"  AP Invoices: {apTotal:N2}");
                sb.AppendLine($"  General Invoices: {invoiceTotal:N2}");
                sb.AppendLine($"  Lease Contracts: {leaseCount}");
                sb.AppendLine();
            }

            _logger?.LogInformation("Generated related party disclosure summary for {Count} parties", relatedPartyBaIds.Count);
            return sb.ToString();
        }

        private async Task<List<AR_INVOICE>> GetArInvoicesAsync(
            List<string> partyIds,
            DateTime start,
            DateTime end,
            string cn)
        {
            var repo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVOICE_DATE", Operator = ">=", FilterValue = start.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "INVOICE_DATE", Operator = "<=", FilterValue = end.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<AR_INVOICE>()
                .Where(x => partyIds.Contains(x.CUSTOMER_BA_ID, StringComparer.OrdinalIgnoreCase))
                .ToList() ?? new List<AR_INVOICE>();
        }

        private async Task<List<AP_INVOICE>> GetApInvoicesAsync(
            List<string> partyIds,
            DateTime start,
            DateTime end,
            string cn)
        {
            var repo = await GetRepoAsync<AP_INVOICE>("AP_INVOICE", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVOICE_DATE", Operator = ">=", FilterValue = start.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "INVOICE_DATE", Operator = "<=", FilterValue = end.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<AP_INVOICE>()
                .Where(x => partyIds.Contains(x.VENDOR_BA_ID, StringComparer.OrdinalIgnoreCase))
                .ToList() ?? new List<AP_INVOICE>();
        }

        private async Task<List<INVOICE>> GetInvoicesAsync(
            List<string> partyIds,
            DateTime start,
            DateTime end,
            string cn)
        {
            var repo = await GetRepoAsync<INVOICE>("INVOICE", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVOICE_DATE", Operator = ">=", FilterValue = start.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "INVOICE_DATE", Operator = "<=", FilterValue = end.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<INVOICE>()
                .Where(x => partyIds.Contains(x.CUSTOMER_BA_ID, StringComparer.OrdinalIgnoreCase))
                .ToList() ?? new List<INVOICE>();
        }

        private async Task<List<LEASE_CONTRACT>> GetLeasesAsync(
            List<string> partyIds,
            DateTime start,
            DateTime end,
            string cn)
        {
            var repo = await GetRepoAsync<LEASE_CONTRACT>("LEASE_CONTRACT", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COMMENCEMENT_DATE", Operator = ">=", FilterValue = start.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "COMMENCEMENT_DATE", Operator = "<=", FilterValue = end.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<LEASE_CONTRACT>()
                .Where(x => partyIds.Contains(x.LESSOR_BA_ID, StringComparer.OrdinalIgnoreCase))
                .ToList() ?? new List<LEASE_CONTRACT>();
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
