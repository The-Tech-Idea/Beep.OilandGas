using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// IFRS 10 consolidation eliminations and adjustments.
    /// </summary>
    public class ConsolidationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<ConsolidationService> _logger;
        private const string ConnectionName = "PPDM39";

        public ConsolidationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<ConsolidationService> logger,
            IAccountMappingService? accountMapping = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
        }

        public async Task<JOURNAL_ENTRY> RecordIntercompanyEliminationAsync(
            DateTime eliminationDate,
            decimal amount,
            string description,
            string userId,
            string? receivableAccountId = null,
            string? payableAccountId = null,
            string? connectionName = null)
        {
            if (amount <= 0m)
                throw new InvalidOperationException("Elimination amount must be positive");
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var cn = connectionName ?? ConnectionName;
            var receivable = string.IsNullOrWhiteSpace(receivableAccountId)
                ? GetAccountId(AccountMappingKeys.IntercompanyReceivable, DefaultGlAccounts.IntercompanyReceivable)
                : receivableAccountId;
            var payable = string.IsNullOrWhiteSpace(payableAccountId)
                ? GetAccountId(AccountMappingKeys.IntercompanyPayable, DefaultGlAccounts.IntercompanyPayable)
                : payableAccountId;

            var lines = new List<JOURNAL_ENTRY_LINE>
            {
                new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = payable,
                    DEBIT_AMOUNT = amount,
                    CREDIT_AMOUNT = 0m,
                    DESCRIPTION = "Eliminate intercompany payable"
                },
                new JOURNAL_ENTRY_LINE
                {
                    GL_ACCOUNT_ID = receivable,
                    DEBIT_AMOUNT = 0m,
                    CREDIT_AMOUNT = amount,
                    DESCRIPTION = "Eliminate intercompany receivable"
                }
            };

            var result = await _basisPosting.PostEntryAsync(
                eliminationDate,
                $"Consolidation elimination: {description}",
                lines,
                userId,
                referenceNumber: null,
                sourceModule: "CONSOLIDATION");
            var entry = result.IfrsEntry ?? throw new InvalidOperationException("IFRS entry was not created.");

            _logger?.LogInformation("Recorded IFRS 10 elimination {Description} amount {Amount}",
                description, amount);

            return entry;
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }
    }
}


