using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Accounting.Services
{
    public class GLAccountMappingService
    {
        private readonly IJournalEntryService _journalEntryService;
        private readonly ILogger<GLAccountMappingService> _logger;

        public GLAccountMappingService(
            IJournalEntryService journalEntryService,
            ILogger<GLAccountMappingService> logger)
        {
            _journalEntryService = journalEntryService;
            _logger = logger;
        }
    }
}
