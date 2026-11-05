using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.Editor;
namespace Beep.OilandGas.DrillingAndConstruction.Services;
/// <summary>Canonical source-qualified program and execution operations through Beep.</summary>
public partial class DrillingOperationService(IDMEEditor editor,ICommonColumnHandler commonColumnHandler,IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata)
    : Beep.OilandGas.DrillingAndConstruction.Core.Interfaces.IDrillingOperationService
{
    private readonly IDMEEditor _editor=editor??throw new ArgumentNullException(nameof(editor));
    private readonly ICommonColumnHandler _commonColumnHandler=commonColumnHandler??throw new ArgumentNullException(nameof(commonColumnHandler));
    private readonly IPPDM39DefaultsRepository _defaults=defaults??throw new ArgumentNullException(nameof(defaults));
    private readonly IPPDMMetadataRepository _metadata=metadata??throw new ArgumentNullException(nameof(metadata));
}
