using System;
using System.Collections.Generic;
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
    /// IAS 8 error correction handling via reversals and corrected postings.
    /// </summary>
    public class ErrorCorrectionService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly JournalEntryService _journalEntryService;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly ILogger<ErrorCorrectionService> _logger;
        private const string ConnectionName = "PPDM39";

        public ErrorCorrectionService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            JournalEntryService journalEntryService,
            AccountingBasisPostingService basisPosting,
            ILogger<ErrorCorrectionService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(JOURNAL_ENTRY Reversal, JOURNAL_ENTRY Correction)> ReverseAndCorrectAsync(
            string originalJournalEntryId,
            string correctionReason,
            List<JOURNAL_ENTRY_LINE> correctedLines,
            DateTime effectiveDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(originalJournalEntryId))
                throw new ArgumentNullException(nameof(originalJournalEntryId));
            if (string.IsNullOrWhiteSpace(correctionReason))
                throw new ArgumentNullException(nameof(correctionReason));
            if (correctedLines == null || correctedLines.Count == 0)
                throw new ArgumentException("Corrected lines are required", nameof(correctedLines));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var originalEntry = await _journalEntryService.GetEntryByIdAsync(originalJournalEntryId);
            if (originalEntry == null)
                throw new InvalidOperationException($"Journal entry not found: {originalJournalEntryId}");
            if (!string.Equals(originalEntry.STATUS, "POSTED", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only posted journal entries can be corrected");

            var reversal = await _journalEntryService.ReverseEntryAsync(
                originalJournalEntryId,
                correctionReason,
                userId);
            await _basisPosting.PostExistingEntryAsync(reversal.JOURNAL_ENTRY_ID, userId);
            await UpdateEntryRemarkAsync(reversal, originalEntry, correctionReason, userId, connectionName);

            var correctionResult = await _basisPosting.PostEntryAsync(
                effectiveDate,
                $"Correction of {originalEntry.ENTRY_NUMBER}: {correctionReason}",
                correctedLines,
                userId,
                referenceNumber: $"CORR-{originalEntry.ENTRY_NUMBER}",
                sourceModule: "CORRECTION");
            var correction = correctionResult.IfrsEntry ?? throw new InvalidOperationException("IFRS entry was not created.");
            await UpdateEntryRemarkAsync(correction, originalEntry, correctionReason, userId, connectionName);

            await UpdateEntryRemarkAsync(originalEntry, originalEntry, correctionReason, userId, connectionName,
                $"Corrected by {correction.ENTRY_NUMBER}");

            _logger?.LogInformation(
                "Corrected journal entry {EntryNumber} with reversal {ReversalNumber} and correction {CorrectionNumber}",
                originalEntry.ENTRY_NUMBER, reversal.ENTRY_NUMBER, correction.ENTRY_NUMBER);

            return (reversal, correction);
        }

        private async Task UpdateEntryRemarkAsync(
            JOURNAL_ENTRY target,
            JOURNAL_ENTRY original,
            string reason,
            string userId,
            string? connectionName,
            string? prefix = null)
        {
            var cn = connectionName ?? ConnectionName;
            var repo = await GetRepoAsync<JOURNAL_ENTRY>("JOURNAL_ENTRY", cn);
            var notePrefix = string.IsNullOrWhiteSpace(prefix) ? "IAS8" : prefix;
            target.REMARK = $"{notePrefix}: {reason} (OriginalEntryId={original.JOURNAL_ENTRY_ID})";
            target.ROW_CHANGED_BY = userId;
            target.ROW_CHANGED_DATE = DateTime.UtcNow;
            await repo.UpdateAsync(target, userId);
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


