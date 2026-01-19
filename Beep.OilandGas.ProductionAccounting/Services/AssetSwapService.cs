using System;
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
    /// Asset swap and farm-in/farm-out accounting service.
    /// </summary>
    public class AssetSwapService : IAssetSwapService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<AssetSwapService> _logger;

        public AssetSwapService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<AssetSwapService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        public async Task<ASSET_SWAP_TRANSACTION> RecordSwapAsync(
            ASSET_SWAP_TRANSACTION swap,
            string userId,
            string cn = "PPDM39")
        {
            if (swap == null)
                throw new ArgumentNullException(nameof(swap));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            swap.ASSET_SWAP_ID ??= Guid.NewGuid().ToString();
            swap.SWAP_DATE ??= DateTime.UtcNow;
            swap.GAIN_LOSS = (swap.FAIR_VALUE_RECEIVED ?? 0m) - (swap.FAIR_VALUE_GIVEN ?? 0m);
            swap.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
            swap.PPDM_GUID ??= Guid.NewGuid().ToString();
            swap.ROW_CREATED_BY = userId;
            swap.ROW_CREATED_DATE = DateTime.UtcNow;

            var metadata = await _metadata.GetTableMetadataAsync("ASSET_SWAP_TRANSACTION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ASSET_SWAP_TRANSACTION);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ASSET_SWAP_TRANSACTION");

            await repo.InsertAsync(swap, userId);

            _logger?.LogInformation(
                "Recorded asset swap {SwapId} with gain/loss {GainLoss}",
                swap.ASSET_SWAP_ID, swap.GAIN_LOSS);

            return swap;
        }
    }
}
