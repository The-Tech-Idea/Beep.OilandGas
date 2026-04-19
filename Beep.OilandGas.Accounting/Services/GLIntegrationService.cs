using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Accounting;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Accounting.Services
{
    public class GLIntegrationService
    {
        private readonly IJournalEntryService _journalEntryService;
        private readonly GLAccountMappingService _accountMapping;
        private readonly ILogger<GLIntegrationService> _logger;

        public GLIntegrationService(
            IJournalEntryService journalEntryService,
            GLAccountMappingService accountMapping,
            ILogger<GLIntegrationService> logger)
        {
            _journalEntryService = journalEntryService;
            _accountMapping = accountMapping;
            _logger = logger;
        }

        public Task<string> PostTraditionalAccountingToGL(
            string entityId,
            string entityType,
            List<JournalEntryLineData> lines,
            DateTime? transactionDate,
            string userId)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public Task<string> PostRoyaltyToGL(
            string paymentId,
            decimal royaltyAmount,
            DateTime? transactionDate,
            string userId)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public Task<string> PostRevenueToGL(
            string transactionId,
            decimal amount,
            bool isCash,
            DateTime transactionDate,
            string userId)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public Task<string> PostProductionToGL(
            string ticketNumber,
            decimal amount,
            bool isCash,
            DateTime? transactionDate,
            string userId)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public Task<string> PostFinancialAccountingToGL(
            string entityId,
            string accountingType,
            decimal amount,
            bool isCash,
            DateTime transactionDate,
            string userId)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public Task<string> PostCostToGL(
            string propertyId,
            decimal amount,
            bool isCapitalized,
            bool isCash,
            DateTime transactionDate,
            string userId)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }
}
