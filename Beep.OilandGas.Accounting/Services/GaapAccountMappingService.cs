using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// GAAP account mapping overrides for parallel ledgers.
    /// </summary>
    public class GaapAccountMappingService : IAccountMappingService
    {
        private readonly AccountMappingService _baseMapping;
        private readonly Dictionary<string, string> _overrides;

        public GaapAccountMappingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<AccountMappingService> logger,
            IDictionary<string, string>? overrides = null)
        {
            _baseMapping = new AccountMappingService(editor, commonColumnHandler, defaults, metadata, logger);
            _overrides = BuildOverrides();

            if (overrides == null)
                return;

            foreach (var kvp in overrides)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key) || string.IsNullOrWhiteSpace(kvp.Value))
                    continue;

                _overrides[kvp.Key] = kvp.Value;
            }
        }

        public Task InitializeAsync(string userId) => Task.CompletedTask;

        public string GetAccountId(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_overrides.TryGetValue(key, out var accountId))
                return accountId;

            return _baseMapping.GetAccountId(key);
        }

        private static Dictionary<string, string> BuildOverrides()
        {
            return new Dictionary<string, string>
            {
                { AccountMappingKeys.ContractAsset, DefaultGlAccounts.GaapContractAsset },
                { AccountMappingKeys.ContractLiability, DefaultGlAccounts.GaapContractLiability },
                { AccountMappingKeys.RightOfUseAsset, DefaultGlAccounts.GaapRightOfUseAsset },
                { AccountMappingKeys.LeaseLiability, DefaultGlAccounts.GaapLeaseLiability },
                { AccountMappingKeys.LossAllowance, DefaultGlAccounts.CeclAllowance },
                { AccountMappingKeys.CeclAllowance, DefaultGlAccounts.CeclAllowance },
                { AccountMappingKeys.CeclExpense, DefaultGlAccounts.CeclExpense }
            };
        }
    }
}
