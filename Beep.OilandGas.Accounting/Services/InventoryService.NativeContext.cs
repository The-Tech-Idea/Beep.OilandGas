using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
namespace Beep.OilandGas.Accounting.Services;
public partial class InventoryService
{
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<InventoryService> _logger;
        private readonly Func<Task<string>>? _resolveConnection;
        private const string ConnectionName = "PPDM39";

        public InventoryService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<InventoryService> logger,
            IAccountMappingService? accountMapping = null,
            Func<Task<string>>? resolveConnection = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
            _resolveConnection = resolveConnection;
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName,bool stockEvidence=false)
        {
            if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION resolver is required for company inventory.");
            var connection = await _resolveConnection();
            if (string.IsNullOrWhiteSpace(connection))
                throw new InvalidOperationException("A PRODUCTION database binding is required.");
            
            RequireCompanySourceSchema(connection,tableName,stockEvidence);
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), connection, tableName);
        }

}
