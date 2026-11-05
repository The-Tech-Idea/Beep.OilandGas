using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LeaseAcquisition.Models;

// Application data association and request authority, not legal title or financial operatorship.
public sealed record LeaseDataScope(CompanyScope Company, AssetDatabaseScope Database, string ActorUserId,
    string LandRightSubtype, string CompanyComponentType, string StatusType, string DateZoneId);
public interface ICompanyLeaseDataAccess
{
    Task<LeaseDataScope> ResolveAsync();
    Task RequireCurrentAsync(LeaseDataScope scope);
    Task RequireFieldAsync(LeaseDataScope scope, string fieldId, string level, string? actor = null);
    Task<object[]> ReadAsync(LeaseDataScope scope, Type type, List<AppFilter> filters);
    Task ValidateCodesAsync(LeaseDataScope scope, string? agreementType, string status);
    Task ExecuteCommandAsync(LeaseDataScope scope, string fieldId, Func<Task> command, Func<Task> confirm);
    IPPDMRowAccessPolicy ForMutation(LeaseDataScope scope, string fieldId, string leaseId, object expectedEntity, bool creating);
    bool IsCurrent(LeaseDataScope scope, object row);
}
