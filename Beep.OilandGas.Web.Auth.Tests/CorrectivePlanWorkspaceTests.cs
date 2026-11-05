using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.App.Services.Field;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Web.Pages.PPDM39.HSE;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using MudBlazor;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class CorrectivePlanWorkspaceTests
{
    [Fact]
    public async Task RetainedPlanCannotAppearAfterSilentFieldRevisionChange()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["SafetyOfficer"],[PermissionConstants.HSE.ManageIncidents])));var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var current=context;var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>new FieldResponse{SelectionAvailable=true,FieldId="field",SelectionContext=current});
        var participant=new RecordedIncidentParticipant(1,2,"person","WITNESS");var review=new CAPlanCreationReview(context,"case",new string('A',64),[participant],["PLAN"]);var original=new CorrectivePlanOriginal(review,new(context,new CreateCAPlanRequest(Guid.NewGuid(),"Plan","P-1","PLAN",1,2,"person",review.Revision)));var store=new Mock<ICorrectivePlanIntentStore>();store.Setup(x=>x.ReadAsync(context,"case")).ReturnsAsync(()=>{current=context with{Revision="changed"};return original;});
        using var state=new CorrectivePlanWorkspaceState(company,fields.Object,Mock.Of<IFieldService>(),store.Object,(_,_,fallback)=>fallback);state.Start();await state.LoadAsync("case");Assert.Null(state.Pending);Assert.Null(state.Review);Assert.NotNull(state.Error);store.Verify(x=>x.ClearAsync(It.IsAny<CorrectivePlanOriginal>()),Times.Never);
    }
    [Fact]
    public async Task FailedRetentionNeverSendsAPlanAndRetryRetainsFirst()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["SafetyOfficer"],[PermissionConstants.HSE.ManageIncidents])));var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{SelectionAvailable=true,FieldId="field",SelectionContext=context});
        var api=new Mock<IFieldService>();api.Setup(x=>x.ReviewCorrectiveActionPlanCreationAsync("case","field",It.IsAny<CancellationToken>())).ReturnsAsync(new CAPlanCreationReview(context,"case",new string('A',64),[new(1,2,"person","WITNESS")],["PLAN"]));var store=new Mock<ICorrectivePlanIntentStore>();store.Setup(x=>x.ReadAsync(context,"case")).ReturnsAsync((CorrectivePlanOriginal?)null);store.Setup(x=>x.RetainAsync(It.IsAny<CorrectivePlanOriginal>())).ThrowsAsync(new InvalidOperationException("retention failed"));
        using var state=new CorrectivePlanWorkspaceState(company,fields.Object,api.Object,store.Object,(_,_,fallback)=>fallback);state.Start();await state.LoadAsync("case");state.Select(Assert.Single(state.Review!.Participants));await state.SubmitAsync("Plan","P-1","PLAN",true);Assert.NotNull(state.Pending);Assert.NotNull(state.Error);await state.RetryAsync();store.Verify(x=>x.RetainAsync(It.IsAny<CorrectivePlanOriginal>()),Times.Exactly(2));api.Verify(x=>x.CreateCorrectiveActionPlanAsync(It.IsAny<string>(),It.IsAny<CreateCAPlanSubmission>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Fact]
    public async Task LateReviewCannotAppearAfterCompanyChanges()
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["SafetyOfficer"],[PermissionConstants.HSE.ManageIncidents])));var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{SelectionAvailable=true,FieldId="field",SelectionContext=context});var result=new TaskCompletionSource<CAPlanCreationReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        var api=new Mock<IFieldService>();api.Setup(x=>x.ReviewCorrectiveActionPlanCreationAsync("case","field",It.IsAny<CancellationToken>())).Returns(result.Task);var store=new Mock<ICorrectivePlanIntentStore>();store.Setup(x=>x.ReadAsync(context,"case")).ReturnsAsync((CorrectivePlanOriginal?)null);using var state=new CorrectivePlanWorkspaceState(company,fields.Object,api.Object,store.Object,(_,_,fallback)=>fallback);state.Start();var loading=state.LoadAsync("case");company.Clear();result.SetResult(new CAPlanCreationReview(context,"case",new string('A',64),[new(1,2,"person","WITNESS")],["PLAN"]));await loading;Assert.Null(state.Review);Assert.Null(state.Pending);Assert.False(state.CanPrepare);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task OriginalPlanIsRetainedBeforeSendingAndOnlyExactRetryClearsIt(bool lost)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["SafetyOfficer"],[PermissionConstants.HSE.View,PermissionConstants.HSE.ManageIncidents])));
        var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{SelectionAvailable=true,FieldId="field",SelectionContext=context});
        var person=new RecordedIncidentParticipant(17,4,"شريك","WITNESS");var api=new Mock<IFieldService>();api.Setup(x=>x.ReviewCorrectiveActionPlanCreationAsync("case/1","field",It.IsAny<CancellationToken>())).ReturnsAsync(new CAPlanCreationReview(context,"case/1",new string('A',64),[person],["PLAN"]));
        var tab=new DevelopmentDraftIntentStoreTests.TabStorage();var protection=new EphemeralDataProtectionProvider();ICorrectivePlanIntentStore Store()=>new CorrectivePlanIntentStore(new ProtectedSessionStorage(tab,protection));var calls=0;CreateCAPlanSubmission? first=null;
        api.Setup(x=>x.CreateCorrectiveActionPlanAsync("case/1",It.IsAny<CreateCAPlanSubmission>(),It.IsAny<CancellationToken>())).Returns((string _,CreateCAPlanSubmission submission,CancellationToken _) => {calls++;first??=submission;Assert.Equal(first,submission);Assert.Single(tab.Values);if(lost && calls==1)throw new HttpRequestException("lost acknowledgement");return Task.FromResult(new CAPlanResolution(context,"case/1",submission.Intent.PlanId.ToString("N")));});
        api.Setup(x=>x.GetCorrectiveActionPlanAsync("case/1",context,It.IsAny<CancellationToken>())).ReturnsAsync(()=>new CAPlanResolution(context,"case/1",first!.Intent.PlanId.ToString("N")));
        using(var state=new CorrectivePlanWorkspaceState(company,fields.Object,api.Object,Store(),(_,_,fallback)=>fallback))
        {
            state.Start();await state.LoadAsync("case/1");state.Select(Assert.Single(state.Review!.Participants));Assert.True(state.CanPrepare);await state.SubmitAsync("Plan","P-1","PLAN",true);
            if(!lost){Assert.Null(state.Pending);Assert.Empty(tab.Values);Assert.Contains("confirmed",state.Message);return;}
            Assert.NotNull(state.Pending);Assert.Single(tab.Values);Assert.DoesNotContain("شريك",Assert.Single(tab.Values).Value);
        }
        using var recovered=new CorrectivePlanWorkspaceState(company,fields.Object,api.Object,Store(),(_,_,fallback)=>fallback);recovered.Start();await recovered.LoadAsync("case/1");Assert.NotNull(recovered.Pending);await recovered.ReviewLinkAsync();Assert.Contains("does not confirm",recovered.Message);Assert.Single(tab.Values);await recovered.RetryAsync();Assert.Null(recovered.Pending);Assert.Empty(tab.Values);Assert.Equal(2,calls);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ScreenUsesReviewedParticipantAndRequiresPermission(bool manage)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["SafetyOfficer"],manage?[PermissionConstants.HSE.View,PermissionConstants.HSE.ManageIncidents]:[PermissionConstants.HSE.View])));
        var context=new FieldSelectionReview(new("tenant","company"),new("core","binding"),"actor","revision","field");var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{SelectionAvailable=true,FieldId="field",SelectionContext=context});var api=new Mock<IFieldService>();api.Setup(x=>x.ReviewCorrectiveActionPlanCreationAsync("case/1","field",It.IsAny<CancellationToken>())).ReturnsAsync(new CAPlanCreationReview(context,"case/1",new string('A',64),[new(17,4,"person","WITNESS")],["PLAN"]));
        api.Setup(x=>x.CreateCorrectiveActionPlanAsync("case/1",It.IsAny<CreateCAPlanSubmission>(),It.IsAny<CancellationToken>())).ReturnsAsync((string _,CreateCAPlanSubmission submission,CancellationToken _)=>new CAPlanResolution(context,"case/1",submission.Intent.PlanId.ToString("N")));
        var tab=new DevelopmentDraftIntentStoreTests.TabStorage();using var state=new CorrectivePlanWorkspaceState(company,fields.Object,api.Object,new CorrectivePlanIntentStore(new ProtectedSessionStorage(tab,new EphemeralDataProtectionProvider())),(_,_,fallback)=>fallback);using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(state);ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/hse/incidents/corrective-plan?incidentId="+Uri.EscapeDataString("case/1"));var page=ui.RenderComponent<CorrectivePlanWorkspace>();page.WaitForAssertion(()=>Assert.Contains("person",page.Markup));page.FindAll("button").Single(b=>b.TextContent.Contains("Select participant")).Click();
        await page.InvokeAsync(()=>page.FindComponents<MudTextField<string>>().Single(x=>x.Instance.Label=="Plan name").Instance.ValueChanged.InvokeAsync("Plan"));await page.InvokeAsync(()=>page.FindComponents<MudTextField<string>>().Single(x=>x.Instance.Label=="Plan number").Instance.ValueChanged.InvokeAsync("P-1"));await page.InvokeAsync(()=>page.FindComponents<MudSelect<string>>().Single(x=>x.Instance.Label=="Recorded response type").Instance.ValueChanged.InvokeAsync("PLAN"));await page.InvokeAsync(()=>page.FindComponents<MudCheckBox<bool>>().Single().Instance.ValueChanged.InvokeAsync(true));var button=page.FindAll("button").Single(b=>b.TextContent.Contains("Create corrective action plan"));
        if(manage){Assert.False(button.HasAttribute("disabled"));button.Click();page.WaitForAssertion(()=>Assert.Contains("original corrective action plan was confirmed",page.Markup));api.Verify(x=>x.CreateCorrectiveActionPlanAsync("case/1",It.IsAny<CreateCAPlanSubmission>(),It.IsAny<CancellationToken>()),Times.Once);}else{Assert.True(button.HasAttribute("disabled"));api.Verify(x=>x.CreateCorrectiveActionPlanAsync(It.IsAny<string>(),It.IsAny<CreateCAPlanSubmission>(),It.IsAny<CancellationToken>()),Times.Never);}
        company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("person",page.Markup));
    }
}
#endif
