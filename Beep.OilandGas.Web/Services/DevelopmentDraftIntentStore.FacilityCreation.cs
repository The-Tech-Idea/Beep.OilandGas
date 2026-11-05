using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<DevelopmentEquipmentInstallation?> ReadEquipmentInstallationAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<DevelopmentEquipmentInstallation>(Key(company,actor)+".equipment-installation");if(!saved.Success)return null;
        var original=saved.Value??throw new InvalidOperationException("The retained installation is unavailable.");original.Require();var context=original.Review.Proposal.Context;
        if(context.Company!=company || context.ActorUserId!=actor)throw new InvalidOperationException("The retained installation belongs to another company or account.");return original;
    }
    public async Task RetainEquipmentInstallationAsync(DevelopmentEquipmentInstallation original)
    {
        original.Require();await gate.WaitAsync();try
        {
            var context=original.Review.Proposal.Context;var previous=await ReadEquipmentInstallationAsync(context.Company,context.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original installation before preparing another.");
            await storage.SetAsync(Key(context.Company,context.ActorUserId)+".equipment-installation",original);
            if((await ReadEquipmentInstallationAsync(context.Company,context.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The installation original was not confirmed retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearEquipmentInstallationAsync(DevelopmentEquipmentInstallation original)
    {
        original.Require();await gate.WaitAsync();try
        {
            var context=original.Review.Proposal.Context;var previous=await ReadEquipmentInstallationAsync(context.Company,context.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The retained installation changed and was preserved.");await storage.DeleteAsync(Key(context.Company,context.ActorUserId)+".equipment-installation");
        }
        finally{gate.Release();}
    }
    public async Task<DevelopmentFacilityCreation?> ReadFacilityCreationAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<DevelopmentFacilityCreation>(Key(company,actor)+".facility-creation");if(!saved.Success)return null;
        var original=saved.Value??throw new InvalidOperationException("The retained facility creation is unavailable.");original.Require();
        if(original.Review.Draft.Context.Company!=company || original.Review.Draft.Context.ActorUserId!=actor)throw new InvalidOperationException("The retained facility creation belongs to another company or account.");return original;
    }
    public async Task RetainFacilityCreationAsync(DevelopmentFacilityCreation original)
    {
        original.Require();await gate.WaitAsync();try
        {
            var context=original.Review.Draft.Context;var previous=await ReadFacilityCreationAsync(context.Company,context.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original facility creation before preparing another.");
            await storage.SetAsync(Key(context.Company,context.ActorUserId)+".facility-creation",original);
            if((await ReadFacilityCreationAsync(context.Company,context.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original facility creation was not confirmed retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearFacilityCreationAsync(DevelopmentFacilityCreation original)
    {
        original.Require();await gate.WaitAsync();try
        {
            var context=original.Review.Draft.Context;var previous=await ReadFacilityCreationAsync(context.Company,context.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The retained facility creation changed and was preserved.");
            await storage.DeleteAsync(Key(context.Company,context.ActorUserId)+".facility-creation");
        }
        finally{gate.Release();}
    }
}
