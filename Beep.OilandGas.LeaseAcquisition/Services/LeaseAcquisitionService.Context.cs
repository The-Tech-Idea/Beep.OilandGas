using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.LeaseAcquisition.Services;

// The canonical interface and actual shared constructor can be exercised independently of advanced analyses.
public partial class LeaseAcquisitionService : ILeaseAcquisitionService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly ICompanyLeaseDataAccess _leaseAccess;
    private readonly ILogger<LeaseAcquisitionService>? _logger;
    public LeaseAcquisitionService(IDMEEditor editor, ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, ICompanyLeaseDataAccess leaseAccess,
        ILogger<LeaseAcquisitionService>? logger = null)
    {
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
        _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _leaseAccess = leaseAccess ?? throw new ArgumentNullException(nameof(leaseAccess)); _logger = logger;
    }
    private async Task<string> ResolveConnectionAsync() => (await _leaseAccess.ResolveAsync()).Database.ConnectionName;
}
