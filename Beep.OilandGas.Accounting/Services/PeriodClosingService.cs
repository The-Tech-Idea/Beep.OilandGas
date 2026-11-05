using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;


using Beep.OilandGas.Accounting.Models;




using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Period Closing Service
    /// Owns native company/book period records and controlled period operations.
    /// Retired companyless checks and journals do not supply durable period authority.
    /// </summary>
    public partial class PeriodClosingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly TrialBalanceService _trialBalanceService;
        private readonly JournalEntryService _journalEntryService;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<PeriodClosingService> _logger;
        private readonly Func<Task<string>>? _resolveConnection;
        private readonly APInvoiceService _apService;
        private readonly ARService _arService;


        public PeriodClosingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            TrialBalanceService trialBalanceService,
            JournalEntryService journalEntryService,
            AccountingBasisPostingService basisPosting,
            APInvoiceService apService,
            ARService arService,
            ILogger<PeriodClosingService> logger,
            IAccountMappingService? accountMapping = null,
            Func<Task<string>>? resolveConnection = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _trialBalanceService = trialBalanceService ?? throw new ArgumentNullException(nameof(trialBalanceService));
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _apService = apService ?? throw new ArgumentNullException(nameof(apService));
            _arService = arService ?? throw new ArgumentNullException(nameof(arService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
            _resolveConnection = resolveConnection;
        }

        /// <summary>The old companyless checklist cannot establish durable company/book period authority.</summary>
        public Task<PeriodCloseChecklist> ValidatePeriodCloseAsync(DateTime periodEndDate, string? bookId = null)
            => Task.FromException<PeriodCloseChecklist>(new InvalidOperationException("Company/book/currency period readiness requires recorded source reconciliation. The companyless checklist is unavailable."));

        /// <summary>Closing journals alone do not close a recorded company period.</summary>
        public Task<Beep.OilandGas.Models.Data.Accounting.PeriodCloseResult> ClosePeriodAsync(
            DateTime periodEndDate, string periodName, string userId = "SYSTEM", string? bookId = null, bool createReversals = false)
            => Task.FromException<Beep.OilandGas.Models.Data.Accounting.PeriodCloseResult>(new InvalidOperationException("A reviewed company period transition and independently confirmed closing source are required. The companyless close is unavailable."));

        /// <summary>Reversing closing journals alone does not reopen a recorded company period.</summary>
        public Task<bool> ReopenPeriodAsync(DateTime periodEndDate, string userId = "SYSTEM", string? bookId = null)
            => Task.FromException<bool>(new InvalidOperationException("An independently authorized company period transition is required. The companyless reopen is unavailable."));
    }
}
