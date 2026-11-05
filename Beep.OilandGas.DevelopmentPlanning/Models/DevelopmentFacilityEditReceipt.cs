using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json.Serialization;
using Beep.OilandGas.PPDM39.Models;
namespace Beep.OilandGas.DevelopmentPlanning.Models;

/// <summary>Confirmed original outcome; never an assertion that current facility fields remain unchanged.</summary>
public sealed record DevelopmentFacilityEditReceipt([property:JsonRequired]string IntentRevision,[property:JsonRequired]FACILITY Result,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DevelopmentFacilityEdit original)
    {
        ArgumentNullException.ThrowIfNull(original);original.Require();
        if(Result is null || IntentRevision!=original.Revision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The original facility edit receipt is incomplete.");
        _=FACILITY_EDIT_INTENT.Create(original,Result);
    }
}
