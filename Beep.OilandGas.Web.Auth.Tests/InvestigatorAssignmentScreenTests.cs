#if WORKFLOW_INBOX_FOCUSED
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Web.Pages.PPDM39.HSE;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class InvestigatorAssignmentScreenTests
{
    [Fact]
    public async Task IncidentReportOriginalSurvivesReopenAndCannotBeReplacedByAnotherReport()
    {
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var original = new IncidentReportIntent(Guid.NewGuid(), context, new("SET", "SPILL", "TIER_2"), new string('A', 64),
            new DateTime(2026, 1, 2, 3, 0, 0, DateTimeKind.Utc), "Site", "Incident", "الكويت");
        var tab = new DevelopmentDraftIntentStoreTests.TabStorage(); var protection = new EphemeralDataProtectionProvider();
        IIncidentReportIntentStore Store() => new IncidentReportIntentStore(new ProtectedSessionStorage(tab, protection));
        await Store().RetainAsync(original);
        Assert.Equal(original, await Store().ReadAsync(context));
        Assert.Equal(original, await Store().ReadAsync(context with { Revision = "new-review" }));
        Assert.Null(await Store().ReadAsync(context with { Company = new("tenant", "other") }));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainAsync(original with { Description = "Changed" }));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().ClearAsync(original with { Description = "Changed" }));
        Assert.Equal(original, await Store().ReadAsync(context));
        await Store().ClearAsync(original);
        Assert.Null(await Store().ReadAsync(context));
    }
    [Fact]
    public async Task ChangedFieldDuringReviewCannotClearRetainedOriginal()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["SafetyOfficer"],[PermissionConstants.HSE.View,PermissionConstants.HSE.ManageIncidents])));
        var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var current=context;
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>new FieldResponse{SelectionAvailable=true,FieldId="field",SelectionContext=current});
        var witness=new RecordedIncidentParticipant(17,4,"person","WITNESS");var assigned=new RecordedIncidentParticipant(17,5,"person",HseReferenceCodes.Investigator.Role);var review=new InvestigatorAssignmentReview(context,"case",new string('A',64),[witness],null);var original=new InvestigatorAssignmentIntent(review,witness);
        var store=new Mock<IInvestigatorAssignmentIntentStore>();store.Setup(x=>x.ReadAsync(context,"case")).ReturnsAsync(original);
        var api=new Mock<IHSEServiceClient>();api.Setup(x=>x.ReviewInvestigatorAssignmentAsync("case","field",It.IsAny<CancellationToken>())).ReturnsAsync(()=>{current=context with{Revision="changed"};return new InvestigatorAssignmentReview(context,"case",new string('B',64),[witness,assigned],assigned);});
        using var state=new InvestigatorAssignmentState(company,fields.Object,api.Object,(_,_,fallback)=>fallback,store.Object);await state.LoadAsync("case");
        Assert.False(state.Loaded);Assert.NotNull(state.Error);store.Verify(x=>x.ClearAsync(It.IsAny<InvestigatorAssignmentIntent>()),Times.Never);
    }
    [Fact]
    public async Task ChangedFieldDuringRetentionCannotSubmitInvestigator()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["SafetyOfficer"],[PermissionConstants.HSE.View,PermissionConstants.HSE.ManageIncidents])));
        var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var current=context;
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>new FieldResponse{SelectionAvailable=true,FieldId="field",SelectionContext=current});
        var witness=new RecordedIncidentParticipant(17,4,"person","WITNESS");var review=new InvestigatorAssignmentReview(context,"case",new string('A',64),[witness],null);
        var store=new Mock<IInvestigatorAssignmentIntentStore>();store.Setup(x=>x.RetainAsync(It.IsAny<InvestigatorAssignmentIntent>())).Callback(()=>current=context with{Revision="changed"}).Returns(Task.CompletedTask);
        var api=new Mock<IHSEServiceClient>();api.Setup(x=>x.ReviewInvestigatorAssignmentAsync("case","field",It.IsAny<CancellationToken>())).ReturnsAsync(review);
        using var state=new InvestigatorAssignmentState(company,fields.Object,api.Object,(_,_,fallback)=>fallback,store.Object);await state.LoadAsync("case");Assert.True(state.Select(Assert.Single(state.Participants)));await state.AssignAsync();
        Assert.NotNull(state.Error);api.Verify(x=>x.AssignInvestigatorAsync(It.IsAny<InvestigatorAssignmentReview>(),It.IsAny<RecordedIncidentParticipant>(),It.IsAny<CancellationToken>()),Times.Never);store.Verify(x=>x.ClearAsync(It.IsAny<InvestigatorAssignmentIntent>()),Times.Never);
    }
    [Fact]
    public async Task ProtectedOriginalIsScopedAndCannotBeReplacedOrClearedByAnotherSubmission()
    {
        var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var witness=new RecordedIncidentParticipant(17,4,"person","WITNESS");var review=new InvestigatorAssignmentReview(context,"case",new string('A',64),[witness],null);var original=new InvestigatorAssignmentIntent(review,witness);
        var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();IInvestigatorAssignmentIntentStore Store()=>new InvestigatorAssignmentIntentStore(new ProtectedSessionStorage(tab,protection));await Store().RetainAsync(original);
        Assert.Equal(original.Revision(),(await Store().ReadAsync(context,"case"))!.Revision());Assert.Null(await Store().ReadAsync(context with{Company=new("tenant","other")},"case"));Assert.Null(await Store().ReadAsync(context with{ActorUserId="other"},"case"));Assert.Null(await Store().ReadAsync(context,"another"));
        var changed=original with{Review=review with{Revision=new string('B',64)}};await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainAsync(changed));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearAsync(changed));Assert.NotNull(await Store().ReadAsync(context,"case"));await Store().ClearAsync(original);Assert.Empty(tab.Values);
    }
    [Fact]
    public async Task OriginalSurvivesPageDisposalUntilRecordedInvestigatorMatches()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["SafetyOfficer"],[PermissionConstants.HSE.View,PermissionConstants.HSE.ManageIncidents])));
        var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{SelectionAvailable=true,FieldId="field",SelectionContext=context});
        var witness=new RecordedIncidentParticipant(17,4,"person","WITNESS");var assigned=new RecordedIncidentParticipant(17,5,"person",HseReferenceCodes.Investigator.Role);var committed=false;var api=new Mock<IHSEServiceClient>();api.Setup(x=>x.ReviewInvestigatorAssignmentAsync("case","field",It.IsAny<CancellationToken>())).ReturnsAsync(()=>new InvestigatorAssignmentReview(context,"case",new string(committed?'B':'A',64),committed?[witness,assigned]:[witness],committed?assigned:null));api.Setup(x=>x.AssignInvestigatorAsync(It.IsAny<InvestigatorAssignmentReview>(),It.IsAny<RecordedIncidentParticipant>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("lost"));
        var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();IInvestigatorAssignmentIntentStore Store()=>new InvestigatorAssignmentIntentStore(new ProtectedSessionStorage(tab,protection));
        using(var first=new InvestigatorAssignmentState(company,fields.Object,api.Object,(_,_,fallback)=>fallback,Store())){await first.LoadAsync("case");Assert.True(first.Select(Assert.Single(first.Participants)));await first.AssignAsync();Assert.True(first.OutcomeReviewRequired);}
        Assert.Single(tab.Values);Assert.DoesNotContain("person",Assert.Single(tab.Values).Value);
        using var next=new InvestigatorAssignmentState(company,fields.Object,api.Object,(_,_,fallback)=>fallback,Store());await next.LoadAsync("case");Assert.True(next.OutcomeReviewRequired);Assert.False(next.CanAssign);Assert.Contains("remains unconfirmed",next.Message);
        committed=true;await next.LoadAsync("case");Assert.False(next.OutcomeReviewRequired);Assert.Contains("currently recorded investigator matches",next.Message);Assert.Empty(tab.Values);api.Verify(x=>x.AssignInvestigatorAsync(It.IsAny<InvestigatorAssignmentReview>(),It.IsAny<RecordedIncidentParticipant>(),It.IsAny<CancellationToken>()),Times.Once);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public void SafetyStaffCanReviewAndAssignOnlyRecordedParticipant(bool manage)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["SafetyOfficer"],manage?[PermissionConstants.HSE.View,PermissionConstants.HSE.ManageIncidents]:[PermissionConstants.HSE.View])));
        var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{SelectionAvailable=true,FieldId="field",SelectionContext=context});
        var participant=new RecordedIncidentParticipant(17,4,"شريك","WITNESS");var assigned=new RecordedIncidentParticipant(17,5,"شريك",HseReferenceCodes.Investigator.Role);var api=new Mock<IHSEServiceClient>();var saved=false;
        api.Setup(x=>x.ReviewInvestigatorAssignmentAsync("case/1","field",It.IsAny<CancellationToken>())).ReturnsAsync(()=>new InvestigatorAssignmentReview(context,"case/1",new string(saved?'B':'A',64),saved?[participant,assigned]:[participant],saved?assigned:null));
        api.Setup(x=>x.AssignInvestigatorAsync(It.IsAny<InvestigatorAssignmentReview>(),It.IsAny<RecordedIncidentParticipant>(),It.IsAny<CancellationToken>())).Callback(()=>saved=true).Returns(Task.CompletedTask);
        using var state=new InvestigatorAssignmentState(company,fields.Object,api.Object,(_,_,fallback)=>fallback,new Beep.OilandGas.Repository.Tests.InvestigatorAssignmentStateTests.MemoryStore());using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(state);
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/hse/incidents/investigator?incidentId="+Uri.EscapeDataString("case/1"));var page=ui.RenderComponent<InvestigatorAssignment>();page.WaitForAssertion(()=>Assert.Contains("شريك",page.Markup));page.FindAll("button").Single(x=>x.TextContent.Contains("Select participant")).Click();var assign=page.FindAll("button").Single(x=>x.TextContent.Contains("Assign selected investigator"));
        if(manage){Assert.False(assign.HasAttribute("disabled"));assign.Click();page.WaitForAssertion(()=>Assert.Contains("Review the recorded outcome",page.Markup));page.FindAll("button").Single(x=>x.TextContent.Contains("Review recorded investigator")).Click();page.WaitForAssertion(()=>Assert.Contains("currently recorded investigator matches",page.Markup));api.Verify(x=>x.AssignInvestigatorAsync(It.IsAny<InvestigatorAssignmentReview>(),It.IsAny<RecordedIncidentParticipant>(),It.IsAny<CancellationToken>()),Times.Once);}
        else{Assert.True(assign.HasAttribute("disabled"));Assert.Contains("permissions do not permit",page.Markup);api.Verify(x=>x.AssignInvestigatorAsync(It.IsAny<InvestigatorAssignmentReview>(),It.IsAny<RecordedIncidentParticipant>(),It.IsAny<CancellationToken>()),Times.Never);}
        company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("شريك",page.Markup));
    }
}
#endif
