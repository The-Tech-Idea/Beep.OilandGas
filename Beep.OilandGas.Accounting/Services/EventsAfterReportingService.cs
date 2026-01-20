using System;
using System.Collections.Generic;
using System.Linq;
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
    /// IAS 10 events after the reporting period.
    /// </summary>
    public class EventsAfterReportingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly ILogger<EventsAfterReportingService> _logger;
        private const string ConnectionName = "PPDM39";

        public EventsAfterReportingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<EventsAfterReportingService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ACCOUNTING_COST> RecordSubsequentEventAsync(
            DateTime eventDate,
            DateTime periodEndDate,
            string description,
            bool isAdjusting,
            decimal amount,
            string userId,
            string? debitAccountId = null,
            string? creditAccountId = null,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (amount < 0m)
                throw new InvalidOperationException("Amount cannot be negative");
            if (isAdjusting && amount <= 0m)
                throw new InvalidOperationException("Adjusting events require a positive amount");
            if (isAdjusting && (string.IsNullOrWhiteSpace(debitAccountId) || string.IsNullOrWhiteSpace(creditAccountId)))
                throw new InvalidOperationException("Adjusting events require debit and credit accounts");

            var cn = connectionName ?? ConnectionName;
            var cost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                FINANCE_ID = $"IAS10_{periodEndDate:yyyyMMdd}",
                COST_TYPE = "SUBSEQUENT_EVENT",
                COST_CATEGORY = isAdjusting ? "ADJUSTING" : "NON_ADJUSTING",
                AMOUNT = amount,
                COST_DATE = eventDate,
                DESCRIPTION = description,
                REMARK = $"PeriodEnd={periodEndDate:yyyy-MM-dd}",
                SOURCE = "IAS10",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            await repo.InsertAsync(cost, userId);

            if (isAdjusting)
            {
                await _basisPosting.PostBalancedEntryByAccountAsync(
                    debitAccountId!,
                    creditAccountId!,
                    amount,
                    $"IAS 10 adjusting event: {description}",
                    userId,
                    cn);
            }

            _logger?.LogInformation("Recorded IAS 10 event {EventDate} adjusting={Adjusting}",
                eventDate, isAdjusting);

            return cost;
        }

        public async Task<List<ACCOUNTING_COST>> GetSubsequentEventsAsync(
            DateTime periodEndDate,
            DateTime? throughDate = null,
            string? connectionName = null)
        {
            var cn = connectionName ?? ConnectionName;
            var endDate = throughDate ?? DateTime.UtcNow;

            var repo = await GetRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_TYPE", Operator = "=", FilterValue = "SUBSEQUENT_EVENT" },
                new AppFilter { FieldName = "COST_DATE", Operator = ">", FilterValue = periodEndDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "COST_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<ACCOUNTING_COST>().ToList() ?? new List<ACCOUNTING_COST>();
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



