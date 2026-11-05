using System.Text.Json.Serialization;
using Beep.OilandGas.PPDM39.Models;

namespace TheTechIdea.Data.OilGas;

public sealed record DevelopmentFacilityCreationReceipt([property:JsonRequired]string IntentRevision,[property:JsonRequired]FACILITY Facility,[property:JsonRequired]FACILITY_FIELD Link,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DevelopmentFacilityCreation original)
    {
        ArgumentNullException.ThrowIfNull(original);original.Require();
        if(Facility is null || Link is null || IntentRevision!=original.Revision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || CheckedAtUtc>DateTimeOffset.UtcNow)throw new InvalidOperationException("The original facility creation receipt is unconfirmed.");
        _=FACILITY_CREATE_INTENT.Create(original,Facility,Link);
    }
}

public interface ICompanyFacilityCreationOwner
{
    Task<FACILITY_CREATE_INTENT> RecordFacilityCreationAsync(DevelopmentFacilityCreation input,Func<Task> authority);
    Task<FACILITY_CREATE_INTENT> RecoverFacilityCreationAsync(DevelopmentFacilityCreation input,Func<Task> readAuthority);
}
