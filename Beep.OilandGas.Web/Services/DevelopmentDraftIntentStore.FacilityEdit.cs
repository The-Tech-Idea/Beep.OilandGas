using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<DevelopmentFacilityEdit?> ReadFacilityEditAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<DevelopmentFacilityEdit>(Key(company,actor)+".facility-edit");if(!saved.Success)return null;
        var original=saved.Value??throw new InvalidOperationException("The retained facility edit is unavailable.");original.Require();
        if(original.Review.Context.Company!=company || original.Review.Context.ActorUserId!=actor)throw new InvalidOperationException("The retained facility edit belongs to another company or account.");
        return original;
    }
    public async Task RetainFacilityEditAsync(DevelopmentFacilityEdit original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var context=original.Review.Context;var previous=await ReadFacilityEditAsync(context.Company,context.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original facility edit before preparing another.");
            await storage.SetAsync(Key(context.Company,context.ActorUserId)+".facility-edit",original);
            if((await ReadFacilityEditAsync(context.Company,context.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original facility edit was not confirmed retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearFacilityEditAsync(DevelopmentFacilityEdit original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var context=original.Review.Context;var previous=await ReadFacilityEditAsync(context.Company,context.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The retained facility edit changed and was preserved.");
            await storage.DeleteAsync(Key(context.Company,context.ActorUserId)+".facility-edit");
        }
        finally{gate.Release();}
    }
}
