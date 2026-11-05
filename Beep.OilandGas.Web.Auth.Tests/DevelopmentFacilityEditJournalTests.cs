using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DevelopmentFacilityEditJournalTests
{
    [Fact]
    public async Task EquipmentInstallationOriginalIsProtectedAndCannotBeReplaced()
    {
        var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();var original=FacilityCreationIntentTests.InstallationOriginal();
        DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));await Store().RetainEquipmentInstallationAsync(original);
        Assert.Single(tab.Values);Assert.DoesNotContain(original.Review.Proposal.Reason,Assert.Single(tab.Values.Values));var context=original.Review.Proposal.Context;
        Assert.Equal(original.Revision(),(await Store().ReadEquipmentInstallationAsync(context.Company,context.ActorUserId))!.Revision());Assert.Null(await Store().ReadEquipmentInstallationAsync(new("tenant","other"),context.ActorUserId));
        var different=original with{IntentId=Guid.NewGuid()};await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainEquipmentInstallationAsync(different));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearEquipmentInstallationAsync(different));
        await Store().ClearEquipmentInstallationAsync(original);Assert.Empty(tab.Values);
    }
    [Fact]
    public async Task CreationOriginalIsProtectedAndCannotBeReplacedOrClearedByAnotherCommand()
    {
        var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();var original=FacilityCreationIntentTests.Original();
        DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));await Store().RetainFacilityCreationAsync(original);
        Assert.Single(tab.Values);Assert.DoesNotContain(original.Review.Draft.LongName,Assert.Single(tab.Values.Values));var context=original.Review.Draft.Context;
        Assert.Equal(original.Revision(),(await Store().ReadFacilityCreationAsync(context.Company,context.ActorUserId))!.Revision());Assert.Null(await Store().ReadFacilityCreationAsync(new("tenant","other"),context.ActorUserId));
        var different=original with{IntentId=Guid.NewGuid()};await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainFacilityCreationAsync(different));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearFacilityCreationAsync(different));
        await Store().ClearFacilityCreationAsync(original);Assert.Empty(tab.Values);
    }
    [Fact]
    public async Task ProtectedTabRetainsExactOriginalAndSeparatesAccountsAndCompanies()
    {
        var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();var original=DevelopmentFacilityEditContractTests.Original();
        DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainFacilityEditAsync(original);Assert.Single(tab.Values);Assert.DoesNotContain(original.LongName,Assert.Single(tab.Values.Values));
        var context=original.Review.Context;Assert.Equal(original.Revision(),(await Store().ReadFacilityEditAsync(context.Company,context.ActorUserId))!.Revision());
        Assert.Null(await Store().ReadFacilityEditAsync(new CompanyScope("tenant","other"),context.ActorUserId));Assert.Null(await Store().ReadFacilityEditAsync(context.Company,"other"));
        var different=original with{IntentId=Guid.NewGuid()};await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainFacilityEditAsync(different));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearFacilityEditAsync(different));Assert.Equal(original.Revision(),(await Store().ReadFacilityEditAsync(context.Company,context.ActorUserId))!.Revision());
        await Store().ClearFacilityEditAsync(original);Assert.Empty(tab.Values);
    }
    [Fact]
    public async Task SharedStoreSerializesCompetingOriginals()
    {
        var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var store=new DevelopmentDraftIntentStore(new ProtectedSessionStorage(tab,new EphemeralDataProtectionProvider()));
        var first=DevelopmentFacilityEditContractTests.Original();var second=first with{IntentId=Guid.NewGuid()};
        var results=await Task.WhenAll(new[]{first,second}.Select(async item=>{try{await store.RetainFacilityEditAsync(item);return true;}catch(InvalidOperationException){return false;}}));
        Assert.Single(results.Where(value=>value));var retained=await store.ReadFacilityEditAsync(first.Review.Context.Company,first.Review.Context.ActorUserId);Assert.NotNull(retained);
        Assert.Contains(retained!.Revision(),new[]{first.Revision(),second.Revision()});Assert.Single(tab.Values);
    }
}
