using TheTechIdea.Beep.Report;

namespace TheTechIdea.Data.OilGas;

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
