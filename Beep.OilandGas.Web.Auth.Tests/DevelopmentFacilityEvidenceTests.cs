using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MudBlazor.Services;
using MudBlazor;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DevelopmentFacilityEvidenceTests
{
    private const string Field="field/واحد%2F";
    private static CompanyClientContext Company(){var value=new CompanyClientContext();Assert.True(value.TrySelect(value.Clear(),"engineer",new(new("tenant","company"),["Engineer"],[])));return value;}
    private static FieldResponse Selection()=>new(){FieldId=Field,FieldName="Recorded field",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"engineer","revision",Field)};
    private static FACILITY Row()=>new(){FACILITY_ID="facility-1",FACILITY_TYPE="PROCESSING",PRIMARY_FIELD_ID=Field,FACILITY_LONG_NAME="Recorded facility",ACTIVE_IND="Y",REMARK="maintenance note"};
    [Theory][InlineData("success")][InlineData("lost")][InlineData("retain")][InlineData("readback")][InlineData("clear")]
    public async Task InstallationRetainsExactOriginalBeforeSaveAndRecoversWithoutSecondWrite(string mode)
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Edit"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);
        api.Setup(x=>x.ReviewEquipmentInstallationAsync(It.IsAny<DevelopmentEquipmentInstallationProposal>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentEquipmentInstallationProposal p,CancellationToken _)=>new(p,new("company",0,0,new string('A',64)),new string('B',64),DateTimeOffset.UtcNow));
        var journal=new DevelopmentDraftWorkspaceTests.Journal{FailRetain=mode=="retain",FailRetainReadback=mode=="readback",FailClear=mode=="clear"};var writes=0;DevelopmentEquipmentInstallationReceipt? recorded=null;
        api.Setup(x=>x.SaveEquipmentInstallationAsync(It.IsAny<DevelopmentEquipmentInstallation>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentEquipmentInstallation original,CancellationToken _)=>{Assert.Equal(original.Revision(),journal.EquipmentInstallationOriginal!.Revision());writes++;recorded=new(original.Revision(),original.Expected(1,DateTime.UtcNow),DateTimeOffset.UtcNow);if(mode=="lost")throw new HttpRequestException("Response lost");return recorded;});
        api.Setup(x=>x.RecoverEquipmentInstallationAsync(It.IsAny<DevelopmentEquipmentInstallation>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentEquipmentInstallation original,CancellationToken _)=>{Assert.NotNull(recorded);Assert.Equal(original.Revision(),recorded!.IntentRevision);return recorded;});
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback,journal);await state.LoadAsync();await state.ReviewInstallationAsync("facility-1","PROCESSING","pump","Engineer","Repair");state.PrepareInstallation(DateTime.UtcNow.AddMinutes(-1).ToString("O"),true);await state.SubmitInstallationAsync();
        if(mode=="success"){Assert.Equal(1,writes);Assert.Null(journal.EquipmentInstallationOriginal);Assert.NotNull(state.InstallationConfirmation);return;}
        Assert.Equal(mode is "retain" or "readback"?0:1,writes);Assert.NotNull(state.PendingInstallation);Assert.NotNull(state.InstallationError);
        if(mode=="retain"){Assert.Null(journal.EquipmentInstallationOriginal);Assert.True(state.CanRetryInstallationRetention);return;}
        Assert.NotNull(journal.EquipmentInstallationOriginal);journal.FailClear=false;journal.FailRetainReadback=false;await state.LoadAsync();Assert.NotNull(state.PendingInstallation);
        if(mode=="readback"){Assert.True(state.CanRetryInstallation);await state.RetryInstallationAsync();Assert.Equal(1,writes);}else {Assert.True(state.CanRecoverInstallation);await state.RecoverInstallationAsync();Assert.Equal(1,writes);}
        Assert.Null(journal.EquipmentInstallationOriginal);Assert.NotNull(state.InstallationConfirmation);
    }

    [Theory][InlineData("valid")][InlineData("valid-other-type")][InlineData("tenant")][InlineData("company")][InlineData("field")][InlineData("id")][InlineData("type")][InlineData("partial")]
    public void FacilitySourceLinkSelectsOnlyExactCurrentCompanyFieldAndCompositeIdentity(string mode)
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();
        var other=Row();other.FACILITY_TYPE="STORAGE";other.FACILITY_LONG_NAME="Other type";api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row(),other]);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(new DevelopmentDraftWorkspaceTests.Journal());
        var url="/ppdm39/development/facilities?tenantId="+Uri.EscapeDataString(mode=="tenant"?"other-tenant":"tenant")+"&companyId="+Uri.EscapeDataString(mode=="company"?"other-company":"company")+"&fieldId="+Uri.EscapeDataString(mode=="field"?"other-field":Field)+"&facilityId="+Uri.EscapeDataString(mode=="id"?"other-facility":"facility-1")+(mode=="partial"?"":"&facilityType="+Uri.EscapeDataString(mode=="valid-other-type"?"STORAGE":mode=="type"?"OTHER":"PROCESSING"));
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo(url);
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.Facilities>();
        if(mode is "valid" or "valid-other-type")
        {
            page.WaitForAssertion(()=>Assert.Contains("Recorded facility fields",page.Markup));Assert.Contains(mode=="valid-other-type"?"Long name: Other type":"Long name: Recorded facility",page.Markup);Assert.DoesNotContain("does not match an accessible record",page.Markup);
            if(mode=="valid")
            {
                ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo(url.Replace("facilityType=PROCESSING","facilityType=STORAGE",StringComparison.Ordinal));page.WaitForAssertion(()=>Assert.Contains("Long name: Other type",page.Markup));
                ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo(url.Replace("fieldId="+Uri.EscapeDataString(Field),"fieldId=other-field",StringComparison.Ordinal));page.WaitForAssertion(()=>Assert.Contains("does not match an accessible record",page.Markup));Assert.DoesNotContain("Recorded facility fields",page.Markup);
            }
            fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");page.WaitForAssertion(()=>Assert.DoesNotContain("Recorded facility fields",page.Markup));
        }
        else {page.WaitForAssertion(()=>Assert.Contains("does not match an accessible record",page.Markup));Assert.DoesNotContain("Recorded facility fields",page.Markup);}
    }

    [Theory][InlineData("valid")][InlineData("no-zone")][InlineData("future")][InlineData("unconfirmed")]
    public async Task InstallationOriginalRequiresExplicitUtcAndConfirmation(string mode)
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Edit"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);
        api.Setup(x=>x.ReviewEquipmentInstallationAsync(It.IsAny<DevelopmentEquipmentInstallationProposal>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentEquipmentInstallationProposal proposal,CancellationToken _)=>new(proposal,new("company",0,0,new string('A',64)),new string('B',64),DateTimeOffset.UtcNow));
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback);await state.LoadAsync();await state.ReviewInstallationAsync("facility-1","PROCESSING","pump","Engineer","Repair");
        var date=mode switch{"no-zone"=>"2026-01-01T00:00:00","future"=>DateTime.UtcNow.AddDays(1).ToString("O"),_=>DateTime.UtcNow.AddMinutes(-1).ToString("O")};state.PrepareInstallation(date,mode!="unconfirmed");
        if(mode=="valid"){Assert.NotNull(state.PreparedInstallation);Assert.True(state.PreparedInstallation!.Confirmed);state.ClearPreparedInstallation();Assert.Null(state.PreparedInstallation);}else {Assert.Null(state.PreparedInstallation);Assert.NotNull(state.InstallationError);}
    }

    [Theory][InlineData("review")][InlineData("field-changed")][InlineData("changed-input")]
    public async Task InstallationReviewIsUnsavedAndNeverSurvivesChangedScopeOrInput(string mode)
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Edit"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);
        var pending=new TaskCompletionSource<DevelopmentEquipmentInstallationReview>(TaskCreationOptions.RunContinuationsAsynchronously);DevelopmentEquipmentInstallationProposal? sent=null;
        api.Setup(x=>x.ReviewEquipmentInstallationAsync(It.IsAny<DevelopmentEquipmentInstallationProposal>(),It.IsAny<CancellationToken>())).Callback<DevelopmentEquipmentInstallationProposal,CancellationToken>((value,_)=>sent=value).Returns(pending.Task);
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback);await state.LoadAsync();var loading=state.ReviewInstallationAsync("facility-1","PROCESSING","pump","Engineer","Required repair");
        if(mode=="field-changed")fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");if(mode=="changed-input")state.ClearInstallation();
        pending.SetResult(new(sent!,new("company",2,1,new string('A',64)),new string('B',64),DateTimeOffset.UtcNow));await loading;
        if(mode=="review"){Assert.Equal("pump",state.InstallationReview!.Proposal.EquipmentId);Assert.Equal(2,state.InstallationReview.Source.RecordedRows);}else Assert.Null(state.InstallationReview);
        api.Verify(x=>x.ReviewEquipmentInstallationAsync(It.IsAny<DevelopmentEquipmentInstallationProposal>(),It.IsAny<CancellationToken>()),Times.Once);
    }

    [Theory][InlineData("loaded")][InlineData("field-changed")][InlineData("closed")][InlineData("duplicate")]
    public async Task FacilityEquipmentScreenKeepsOnlyCurrentQualifiedSelection(string mode)
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);
        var pending=new TaskCompletionSource<List<DevelopmentFacilityEquipmentItem>>(TaskCreationOptions.RunContinuationsAsynchronously);
        api.Setup(x=>x.GetFacilityEquipmentAsync("facility-1","PROCESSING",Field,It.IsAny<CancellationToken>())).Returns(pending.Task);
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback);await state.LoadAsync();var loading=state.LoadEquipmentAsync("facility-1","PROCESSING");
        if(mode=="field-changed")fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");if(mode=="closed")state.ClearEquipment();
        pending.SetResult(mode=="duplicate"?[new("pump",1,"source"),new("pump",1,"source")]:[new("pump",1,"source")]);await loading;
        if(mode=="loaded"){Assert.True(state.EquipmentLoaded);Assert.Equal("pump",Assert.Single(state.Equipment).EquipmentId);}else {Assert.Empty(state.Equipment);Assert.False(state.EquipmentLoaded);if(mode=="duplicate")Assert.NotNull(state.EquipmentError);}
    }

    [Fact]
    public async Task FacilityProposalRequiresCurrentCreatorAndClearsWithCompanyOrField()
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Create"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback);await state.LoadAsync();
        Assert.False(state.CanPrepareCreation("Unknown"));state.PrepareCreation("Engineer","facility-1","PROCESSING","operator","Name",null,"Reason");Assert.Null(state.CreationDraft);Assert.NotNull(state.CreationError);
        state.PrepareCreation("Engineer","new-facility","PROCESSING","operator","Name",null,"");Assert.Null(state.CreationDraft);
        state.PrepareCreation("Engineer","new-facility","PROCESSING","operator","Name",null,"Required expansion");Assert.Equal(Field,state.CreationDraft!.Context.FieldId);Assert.Equal("new-facility",state.CreationDraft.FacilityId);
        fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");Assert.Null(state.CreationDraft);Assert.False(state.CanPrepareCreation("Engineer"));
        await state.LoadAsync();state.PrepareCreation("Engineer","other","STORAGE","operator","Name",null,"Reason");Assert.NotNull(state.CreationDraft);company.Clear();Assert.Null(state.CreationDraft);
        api.Verify(x=>x.SaveFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]
    public async Task LateCreationSourceReviewCannotAppearAfterCompanyChanges()
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Create"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);
        var completion=new TaskCompletionSource<DevelopmentFacilityCreationReview>(TaskCreationOptions.RunContinuationsAsynchronously);api.Setup(x=>x.ReviewFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreationDraft>(),It.IsAny<CancellationToken>())).Returns(completion.Task);
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback);await state.LoadAsync();state.PrepareCreation("Engineer","new","PROCESSING","operator","Name",null,"Reason");var draft=state.CreationDraft!;
        var pending=state.ReviewCreationAsync();company.Clear();var review=new DevelopmentFacilityCreationReview(draft,new R_FACILITY_TYPE{FACILITY_TYPE="PROCESSING",ACTIVE_IND="Y"},new string('A',64),new string('B',64),"",DateTimeOffset.UtcNow);completion.SetResult(review with{Revision=review.ContentRevision()});await pending;
        Assert.Null(state.CreationReview);Assert.Null(state.CreationDraft);Assert.False(state.ReviewingCreation);
    }
    [Fact]
    public async Task FacilityPageShowsAnExplicitUnsavedCreationProposal()
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Create"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);
        api.Setup(x=>x.ReviewFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreationDraft>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentFacilityCreationDraft draft,CancellationToken _)=>{var review=new DevelopmentFacilityCreationReview(draft,new R_FACILITY_TYPE{FACILITY_TYPE=draft.FacilityType,ACTIVE_IND="Y",LONG_NAME="Recorded type"},new string('A',64),new string('B',64),"",DateTimeOffset.UtcNow);return review with{Revision=review.ContentRevision()};});
        var journal=new DevelopmentDraftWorkspaceTests.Journal();api.Setup(x=>x.SaveFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreation>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentFacilityCreation original,CancellationToken _)=>{Assert.Equal(original.Revision(),journal.FacilityCreationOriginal!.Revision());var rows=original.Expected(DateTime.UtcNow);return new(original.Revision(),rows.Facility,rows.Link,DateTimeOffset.UtcNow);});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.Facilities>();page.WaitForAssertion(()=>Assert.Contains("Prepare a new facility",page.Markup));
        await page.InvokeAsync(()=>page.FindComponent<MudSelect<string>>().Instance.ValueChanged.InvokeAsync("Engineer"));var inputs=page.FindComponents<MudTextField<string>>();
        await page.InvokeAsync(()=>inputs.Single(input=>input.Instance.Label=="Facility ID").Instance.ValueChanged.InvokeAsync("new-facility"));await page.InvokeAsync(()=>inputs.Single(input=>input.Instance.Label=="Facility type code").Instance.ValueChanged.InvokeAsync("PROCESSING"));
        await page.InvokeAsync(()=>inputs.Single(input=>input.Instance.Label=="Operator business associate ID").Instance.ValueChanged.InvokeAsync("operator"));
        await page.InvokeAsync(()=>inputs.Single(input=>input.Instance.Label=="Facility long name").Instance.ValueChanged.InvokeAsync("New plant"));await page.InvokeAsync(()=>inputs.Single(input=>input.Instance.Label=="Reason for creation").Instance.ValueChanged.InvokeAsync("Required expansion"));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Review unsaved proposal")).Click();page.WaitForAssertion(()=>Assert.Contains("Unsaved proposal",page.Markup));Assert.Contains("new-facility",page.Markup);Assert.Contains("proposal is not saved",page.Markup);
        page.FindAll("button").Single(button=>button.TextContent.Contains("Check recorded source")).Click();page.WaitForAssertion(()=>Assert.Contains("Recorded type",page.Markup));Assert.Contains("reserve the identity or save a facility",page.Markup);Assert.Contains("does not establish the operator",page.Markup);
        await page.InvokeAsync(()=>page.FindComponent<MudCheckBox<bool>>().Instance.ValueChanged.InvokeAsync(true));page.FindAll("button").Single(button=>button.TextContent.Contains("Prepare original creation")).Click();page.WaitForAssertion(()=>Assert.Contains("Save facility",page.Markup));
        page.FindAll("button").Single(button=>button.TextContent.Trim()=="Save facility").Click();page.WaitForAssertion(()=>Assert.Contains("original facility creation was confirmed",page.Markup));Assert.Null(journal.FacilityCreationOriginal);api.Verify(x=>x.SaveFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreation>(),It.IsAny<CancellationToken>()),Times.Once());
        company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("new-facility",page.Markup));
    }
    [Theory][InlineData("success")][InlineData("lost")][InlineData("retry")][InlineData("retention")][InlineData("retention-readback")][InlineData("clear")]
    public async Task FacilityCreationRetainsOriginalBeforeSendingAndCanRecover(string mode)
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Create"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);
        api.Setup(x=>x.ReviewFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreationDraft>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentFacilityCreationDraft draft,CancellationToken _)=>{var review=new DevelopmentFacilityCreationReview(draft,new R_FACILITY_TYPE{FACILITY_TYPE=draft.FacilityType,ACTIVE_IND="Y"},new string('A',64),new string('B',64),"",DateTimeOffset.UtcNow);return review with{Revision=review.ContentRevision()};});
        var journal=new DevelopmentDraftWorkspaceTests.Journal{FailRetain=mode=="retention",FailRetainReadback=mode=="retention-readback",FailClear=mode=="clear"};DevelopmentFacilityCreationReceipt? saved=null;var sends=0;
        api.Setup(x=>x.SaveFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreation>(),It.IsAny<CancellationToken>())).Returns((DevelopmentFacilityCreation command,CancellationToken _)=>
        {
            Assert.Equal(command.Revision(),journal.FacilityCreationOriginal!.Revision());sends++;var result=command.Expected(DateTime.UtcNow);saved??=new(command.Revision(),result.Facility,result.Link,DateTimeOffset.UtcNow);if(mode=="lost" || mode=="retry" && sends==1)throw new HttpRequestException("Lost response");return Task.FromResult(saved);
        });
        api.Setup(x=>x.RecoverFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreation>(),It.IsAny<CancellationToken>())).Returns((DevelopmentFacilityCreation command,CancellationToken _)=>{Assert.Equal(command.Revision(),saved!.IntentRevision);return Task.FromResult(saved);});
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback,journal);await state.LoadAsync();state.PrepareCreation("Engineer","new","PROCESSING","operator","Name",null,"Reason");await state.ReviewCreationAsync();state.PrepareCreationCommand(true);await state.SubmitCreationAsync();
        if(mode is "retention" or "retention-readback"){Assert.Equal(0,sends);Assert.True(state.CanRetryCreationRetention);journal.FailRetain=false;journal.FailRetainReadback=false;await state.RetryCreationRetentionAsync();}
        if(mode=="retry"){await state.LoadAsync();Assert.True(state.CanRetryCreation);await state.RetryCreationAsync();}
        if(mode is "lost" or "clear")
        {
            Assert.NotNull(state.PendingCreation);journal.FailClear=false;var current=company.Selection!;company.TrySelect(company.Clear(),current.UserId,new(current.Scope,current.Roles.ToArray(),["DevelopmentPlanning.View"]));await state.LoadAsync();Assert.True(state.CanRecoverCreation);await state.RecoverCreationAsync();
        }
        Assert.Equal(mode=="retry"?2:1,sends);Assert.Null(state.PendingCreation);Assert.Null(journal.FacilityCreationOriginal);Assert.NotNull(state.CreationReceipt);Assert.NotNull(state.CreationConfirmation);
    }
    [Fact]
    public async Task LateCreationResponseCannotAppearUnderAnotherCompanyAndCanBeRecovered()
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Create"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);
        api.Setup(x=>x.ReviewFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreationDraft>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentFacilityCreationDraft draft,CancellationToken _)=>{var review=new DevelopmentFacilityCreationReview(draft,new R_FACILITY_TYPE{FACILITY_TYPE=draft.FacilityType,ACTIVE_IND="Y"},new string('A',64),new string('B',64),"",DateTimeOffset.UtcNow);return review with{Revision=review.ContentRevision()};});
        var journal=new DevelopmentDraftWorkspaceTests.Journal();var completion=new TaskCompletionSource<DevelopmentFacilityCreationReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);DevelopmentFacilityCreationReceipt? saved=null;
        api.Setup(x=>x.SaveFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreation>(),It.IsAny<CancellationToken>())).Returns((DevelopmentFacilityCreation command,CancellationToken _)=>{Assert.Equal(command.Revision(),journal.FacilityCreationOriginal!.Revision());var rows=command.Expected(DateTime.UtcNow);saved=new(command.Revision(),rows.Facility,rows.Link,DateTimeOffset.UtcNow);return completion.Task;});
        api.Setup(x=>x.RecoverFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreation>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentFacilityCreation command,CancellationToken _)=>{Assert.Equal(command.Revision(),saved!.IntentRevision);return saved;});
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback,journal);await state.LoadAsync();state.PrepareCreation("Engineer","new","PROCESSING","operator","Name",null,"Reason");await state.ReviewCreationAsync();state.PrepareCreationCommand(true);
        var pending=state.SubmitCreationAsync();Assert.NotNull(saved);var previous=company.Selection!;company.Clear();completion.SetResult(saved!);await pending;Assert.Null(state.CreationReceipt);Assert.NotNull(journal.FacilityCreationOriginal);
        company.TrySelect(company.Clear(),previous.UserId,new(previous.Scope,previous.Roles.ToArray(),["DevelopmentPlanning.View"]));await state.LoadAsync();Assert.True(state.CanRecoverCreation);await state.RecoverCreationAsync();Assert.NotNull(state.CreationReceipt);Assert.Null(journal.FacilityCreationOriginal);
        api.Verify(x=>x.SaveFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreation>(),It.IsAny<CancellationToken>()),Times.Once());
    }
    [Theory]
    [InlineData("current")][InlineData("empty")][InlineData("failed")][InlineData("missing")][InlineData("foreign")][InlineData("identity")][InlineData("missing-type")][InlineData("duplicate")][InlineData("distinct-types")][InlineData("no-field")][InlineData("revision")][InlineData("late-company")][InlineData("late-field")]
    public async Task FacilityRowsRequireCurrentReviewedCompanyAndField(string mode)
    {
        using var company=Company();var fields=new Mock<IFieldSelectionContext>();var count=0;
        fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).Returns(()=>
        {
            var value=Selection();if(mode=="no-field"){value.SelectionAvailable=false;value.FieldId="";value.SelectionContext=value.SelectionContext! with{FieldId=null};}
            if(++count>1 && mode=="revision")value.SelectionContext=value.SelectionContext! with{Revision="changed"};
            return Task.FromResult(value);
        });
        var api=new Mock<IDevelopmentServiceClient>();var source=Row();
        api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).Returns(()=>
        {
            if(mode=="failed")throw new HttpRequestException("unavailable");
            if(mode=="foreign")source.PRIMARY_FIELD_ID="other";if(mode=="identity")source.FACILITY_ID="";if(mode=="missing-type")source.FACILITY_TYPE="";
            if(mode=="late-company")company.Clear();if(mode=="late-field")fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");
            return Task.FromResult(mode=="missing"?null!:mode=="empty"?new List<FACILITY>():mode=="duplicate"?[source,source]:mode=="distinct-types"?[source,new FACILITY{FACILITY_ID=source.FACILITY_ID,FACILITY_TYPE="STORAGE",PRIMARY_FIELD_ID=Field}]:[source]);
        });
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback);await state.LoadAsync();
        if(mode is "current" or "empty" or "distinct-types")
        {
            Assert.True(state.Loaded);Assert.Null(state.Error);Assert.Equal(Field,state.Review!.FieldId);
            if(mode=="current"){Assert.Equal("maintenance note",Assert.Single(state.Rows).REMARK);source.REMARK="changed";Assert.Equal("maintenance note",Assert.Single(state.Rows).REMARK);}
            else if(mode=="distinct-types")Assert.Equal(2,state.Rows.Count);else Assert.Empty(state.Rows);
        }
        else
        {
            Assert.False(state.Loaded);Assert.Empty(state.Rows);Assert.Null(state.Review);
            if(mode=="no-field"){Assert.True(state.NeedsField);api.Verify(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),It.IsAny<string?>()),Times.Never);}
            else if(mode is not ("late-company" or "late-field"))Assert.NotNull(state.Error);
        }
        Assert.False(state.Loading);
    }
    [Fact]
    public async Task LateReadCannotOverwriteNewerSelection()
    {
        using var company=Company();var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var api=new Mock<IDevelopmentServiceClient>();var pending=new TaskCompletionSource<List<FACILITY>>(TaskCreationOptions.RunContinuationsAsynchronously);var calls=0;
        api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).Returns(()=>++calls==1?pending.Task:Task.FromResult(new List<FACILITY>()));
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback);var old=state.LoadAsync();await state.LoadAsync();pending.SetResult([Row()]);await old;
        Assert.True(state.Loaded);Assert.Empty(state.Rows);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public void ActualPageDoesNotTreatFailureOrRemarkAsFacilityStatus(bool fails)
    {
        using var company=Company();var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).Returns(()=>fails?throw new HttpRequestException("unavailable"):Task.FromResult(new List<FACILITY>{Row()}));
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(new DevelopmentDraftWorkspaceTests.Journal());
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.Facilities>();
        if(fails){page.WaitForAssertion(()=>Assert.Contains("facility evidence could not be confirmed",page.Markup));Assert.DoesNotContain("No facility rows",page.Markup);}
        else{page.WaitForAssertion(()=>Assert.Contains("maintenance note",page.Markup));Assert.Contains("Recorded facility",page.Markup);Assert.Contains("Active indicator",page.Markup);Assert.DoesNotContain("Operational",page.Markup);Assert.DoesNotContain("Add Facility",page.Markup);}
    }
    [Fact]
    public void FacilityDetailsUseBothKeysAndClearWhenCompanyChanges()
    {
        using var company=Company();var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var api=new Mock<IDevelopmentServiceClient>();var first=Row();first.DESCRIPTION="First source description";
        var second=Row();second.FACILITY_TYPE="STORAGE";second.DESCRIPTION="Storage source description";
        api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([first,second]);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(new DevelopmentDraftWorkspaceTests.Journal());
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.Facilities>();
        page.WaitForAssertion(()=>Assert.Equal(2,page.FindAll("button[aria-label^='Inspect facility-1']").Count));
        page.Find("button[aria-label='Inspect facility-1, type STORAGE']").Click();
        Assert.Contains("Storage source description",page.Markup);Assert.DoesNotContain("First source description",page.Markup);
        company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("Storage source description",page.Markup));
        Assert.DoesNotContain("Recorded facility fields",page.Markup);
    }
    [Theory][InlineData("valid")][InlineData("readonly")][InlineData("changed-source")][InlineData("changed-role")][InlineData("company-change")]
    public async Task EditReviewStaysBoundToTheSelectedFacilityAndCompany(string mode)
    {
        using var company=Company();var selected=company.Selection!;
        company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),mode=="readonly"?["DevelopmentPlanning.View"]:["DevelopmentPlanning.View","DevelopmentPlanning.Edit"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var api=new Mock<IDevelopmentServiceClient>();var row=Row();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([row]);
        var pending=new TaskCompletionSource<DevelopmentFacilityEditReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        api.Setup(x=>x.ReviewFacilityEditAsync("facility-1","PROCESSING","Engineer",Field,It.IsAny<CancellationToken>())).Returns(pending.Task);
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback);await state.LoadAsync();
        if(mode=="readonly"){Assert.False(state.CanReviewEdit("facility-1","PROCESSING","Engineer"));await state.ReviewEditAsync("facility-1","PROCESSING","Engineer");api.Verify(x=>x.ReviewFacilityEditAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never);return;}
        var operation=state.ReviewEditAsync("facility-1","PROCESSING","Engineer");if(mode=="company-change")company.Clear();
        var reviewed=Row();if(mode=="changed-source")reviewed.REMARK="changed";
        var review=new DevelopmentFacilityEditReview(Selection().SelectionContext!,reviewed,mode=="changed-role"?"OtherRole":"Engineer",DevelopmentFacilityEditReview.SourceDigest(reviewed),new string('A',64),"",DateTimeOffset.UtcNow);
        review=review with{Revision=review.ContentRevision()};pending.SetResult(review);await operation;
        if(mode=="valid"){Assert.NotNull(state.EditReview);Assert.Null(state.EditError);state.ClearEdit();Assert.Null(state.EditReview);}
        else{Assert.Null(state.EditReview);if(mode!="company-change")Assert.NotNull(state.EditError);}
    }
    [Fact]
    public async Task FacilityScreenShowsUnsavedInstallationReview()
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Edit"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);
        api.Setup(x=>x.ReviewEquipmentInstallationAsync(It.IsAny<DevelopmentEquipmentInstallationProposal>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentEquipmentInstallationProposal proposal,CancellationToken _)=>new(proposal,new("company",2,1,new string('A',64)),new string('B',64),DateTimeOffset.UtcNow));
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(new DevelopmentDraftWorkspaceTests.Journal());
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.Facilities>();page.WaitForAssertion(()=>Assert.NotEmpty(page.FindAll("button[aria-label^='Inspect facility-1']")));page.Find("button[aria-label='Inspect facility-1, type PROCESSING']").Click();
        await page.InvokeAsync(()=>page.FindComponents<MudSelect<string>>().Single(c=>c.Instance.Label=="Your role for the installation").Instance.ValueChanged.InvokeAsync("Engineer"));
        await page.InvokeAsync(()=>page.FindComponents<MudTextField<string>>().Single(c=>c.Instance.Label=="Equipment ID").Instance.ValueChanged.InvokeAsync("pump"));
        await page.InvokeAsync(()=>page.FindComponents<MudTextField<string>>().Single(c=>c.Instance.Label=="Reason for installation").Instance.ValueChanged.InvokeAsync("Repair"));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Check installation evidence")).Click();page.WaitForAssertion(()=>Assert.Contains("Recorded equipment pump",page.Markup));Assert.Contains("Nothing has been saved",page.Markup);Assert.Contains("Recorded active installation rows must be reconciled",page.Markup);
        await page.InvokeAsync(()=>page.FindComponents<MudTextField<string>>().Single(c=>c.Instance.Label=="Recorded installation time (UTC)").Instance.ValueChanged.InvokeAsync(DateTime.UtcNow.AddMinutes(-1).ToString("O")));
        await page.InvokeAsync(()=>page.FindComponents<MudCheckBox<bool>>().Single(c=>c.Instance.Label=="I confirm the reviewed equipment and recorded installation time.").Instance.ValueChanged.InvokeAsync(true));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Prepare installation original")).Click();page.WaitForAssertion(()=>Assert.Contains("Original prepared for pump",page.Markup));Assert.Contains("Save equipment installation",page.Markup);Assert.True(page.FindAll("button").Single(button=>button.TextContent.Contains("Save equipment installation")).HasAttribute("disabled"));

        await page.InvokeAsync(()=>page.FindComponents<MudTextField<string>>().Single(c=>c.Instance.Label=="Equipment ID").Instance.ValueChanged.InvokeAsync("other"));page.WaitForAssertion(()=>Assert.DoesNotContain("Recorded equipment pump",page.Markup));
    }

    [Theory][InlineData(false)][InlineData(true)]
    public async Task FacilityScreenRetainsAndSendsConfirmedInstallation(bool lostResponse)
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Edit","DevelopmentPlanning.Create"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);
        api.Setup(x=>x.ReviewEquipmentInstallationAsync(It.IsAny<DevelopmentEquipmentInstallationProposal>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentEquipmentInstallationProposal p,CancellationToken _)=>new(p,new("company",0,0,new string('A',64)),new string('B',64),DateTimeOffset.UtcNow));var journal=new DevelopmentDraftWorkspaceTests.Journal();
        DevelopmentEquipmentInstallationReceipt? receipt=null;
        api.Setup(x=>x.SaveEquipmentInstallationAsync(It.IsAny<DevelopmentEquipmentInstallation>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentEquipmentInstallation original,CancellationToken _)=>{Assert.Equal(original.Revision(),journal.EquipmentInstallationOriginal!.Revision());receipt=new(original.Revision(),original.Expected(1,DateTime.UtcNow),DateTimeOffset.UtcNow);if(lostResponse)throw new HttpRequestException("Lost response");return receipt;});
        api.Setup(x=>x.RecoverEquipmentInstallationAsync(It.IsAny<DevelopmentEquipmentInstallation>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentEquipmentInstallation original,CancellationToken _)=>{Assert.Equal(original.Revision(),receipt!.IntentRevision);return receipt;});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(journal);
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.Facilities>();page.WaitForAssertion(()=>Assert.NotEmpty(page.FindAll("button[aria-label^='Inspect facility-1']")));page.Find("button[aria-label='Inspect facility-1, type PROCESSING']").Click();
        await page.InvokeAsync(()=>page.FindComponents<MudSelect<string>>().Single(c=>c.Instance.Label=="Your role for the installation").Instance.ValueChanged.InvokeAsync("Engineer"));
        await page.InvokeAsync(()=>page.FindComponents<MudTextField<string>>().Single(c=>c.Instance.Label=="Equipment ID").Instance.ValueChanged.InvokeAsync("pump"));await page.InvokeAsync(()=>page.FindComponents<MudTextField<string>>().Single(c=>c.Instance.Label=="Reason for installation").Instance.ValueChanged.InvokeAsync("Repair"));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Check installation evidence")).Click();page.WaitForAssertion(()=>Assert.Contains("Recorded equipment pump",page.Markup));
        await page.InvokeAsync(()=>page.FindComponents<MudTextField<string>>().Single(c=>c.Instance.Label=="Recorded installation time (UTC)").Instance.ValueChanged.InvokeAsync(DateTime.UtcNow.AddMinutes(-1).ToString("O")));await page.InvokeAsync(()=>page.FindComponents<MudCheckBox<bool>>().Single(c=>c.Instance.Label=="I confirm the reviewed equipment and recorded installation time.").Instance.ValueChanged.InvokeAsync(true));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Prepare installation original")).Click();page.WaitForAssertion(()=>Assert.Contains("Save equipment installation",page.Markup));page.FindAll("button").Single(button=>button.TextContent.Contains("Save equipment installation")).Click();
        if(lostResponse){page.WaitForAssertion(()=>Assert.Contains("Recover original installation",page.Markup));Assert.DoesNotContain("Prepare a new facility",page.Markup);Assert.DoesNotContain("Check facility edit authority",page.Markup);Assert.DoesNotContain("Check installation evidence",page.Markup);page.FindAll("button").Single(button=>button.TextContent.Contains("Recover original installation")).Click();}
        page.WaitForAssertion(()=>Assert.Contains("original equipment installation was confirmed",page.Markup));Assert.Null(journal.EquipmentInstallationOriginal);api.Verify(x=>x.SaveEquipmentInstallationAsync(It.IsAny<DevelopmentEquipmentInstallation>(),It.IsAny<CancellationToken>()),Times.Once);
    }

    [Fact]
    public void FacilityScreenShowsRecordedEquipmentAndClearsItOnClose()
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);api.Setup(x=>x.GetFacilityEquipmentAsync("facility-1","PROCESSING",Field,It.IsAny<CancellationToken>())).ReturnsAsync([new("pump-1",7,"recorded-source")]);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(new DevelopmentDraftWorkspaceTests.Journal());
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.Facilities>();page.WaitForAssertion(()=>Assert.NotEmpty(page.FindAll("button[aria-label^='Inspect facility-1']")));
        page.Find("button[aria-label='Inspect facility-1, type PROCESSING']").Click();page.FindAll("button").Single(button=>button.TextContent.Contains("Load equipment evidence")).Click();
        page.WaitForAssertion(()=>Assert.Contains("pump-1",page.Markup));Assert.Contains("recorded-source",page.Markup);Assert.Contains("do not establish current installation",page.Markup);
        page.FindAll("button").Single(button=>button.TextContent.Contains("Close facility details")).Click();Assert.DoesNotContain("pump-1",page.Markup);
    }

    [Fact]
    public void EditorSeesRoleSelectionAndUnfinishedSaveStatusOnSelectedFacility()
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Edit"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([Row()]);
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(new DevelopmentDraftWorkspaceTests.Journal());
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.Facilities>();page.WaitForAssertion(()=>Assert.NotEmpty(page.FindAll("button[aria-label^='Inspect facility-1']")));
        page.Find("button[aria-label='Inspect facility-1, type PROCESSING']").Click();
        Assert.Contains("Your current editor role",page.Markup);Assert.Contains("Check facility edit authority",page.Markup);
        company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("Your current editor role",page.Markup));
    }
    [Theory][InlineData("success")][InlineData("lost")][InlineData("retry")][InlineData("retention")][InlineData("retention-readback")][InlineData("clear")]
    public async Task EditRetainsOriginalBeforeTransportAndRecoversWithoutRepeatingWrite(string mode)
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Edit"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var api=new Mock<IDevelopmentServiceClient>();var source=Row();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([source]);
        var review=new DevelopmentFacilityEditReview(Selection().SelectionContext!,CompanyInventorySourceReview.Copy(source),"Engineer",DevelopmentFacilityEditReview.SourceDigest(source),new string('A',64),"",DateTimeOffset.UtcNow);review=review with{Revision=review.ContentRevision()};
        api.Setup(x=>x.ReviewFacilityEditAsync("facility-1","PROCESSING","Engineer",Field,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var journal=new DevelopmentDraftWorkspaceTests.Journal{FailRetain=mode=="retention",FailRetainReadback=mode=="retention-readback",FailClear=mode=="clear"};DevelopmentFacilityEditReceipt? saved=null;var sends=0;
        api.Setup(x=>x.SaveFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<CancellationToken>())).Returns((DevelopmentFacilityEdit command,CancellationToken _)=>
        {
            Assert.Equal(command.Revision(),journal.FacilityEditOriginal!.Revision());sends++;var result=CompanyInventorySourceReview.Copy(command.Review.Original);result.FACILITY_LONG_NAME=command.LongName;result.DESCRIPTION=command.Description!;result.ROW_CHANGED_BY=command.Review.Context.ActorUserId;result.ROW_CHANGED_DATE=DateTime.UtcNow;
            saved??=new(command.Revision(),result,DateTimeOffset.UtcNow);if(mode=="lost" || mode=="retry" && sends==1)throw new HttpRequestException("Lost response");return Task.FromResult(saved);
        });
        api.Setup(x=>x.RecoverFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<CancellationToken>())).Returns((DevelopmentFacilityEdit command,CancellationToken _)=>{Assert.Equal(command.Revision(),saved!.IntentRevision);return Task.FromResult(saved);});
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback,journal);
        await state.LoadAsync();await state.ReviewEditAsync("facility-1","PROCESSING","Engineer");state.PrepareEdit("Updated",null,"Correct the recorded name",true);Assert.NotNull(state.PreparedEdit);
        await state.SubmitEditAsync();
        if(mode is "retention" or "retention-readback"){Assert.Equal(0,sends);Assert.True(state.CanRetryRetention);Assert.False(state.RetentionConfirmed);journal.FailRetain=false;journal.FailRetainReadback=false;await state.RetryRetentionAsync();}
        if(mode=="retry"){Assert.NotNull(state.PendingEdit);await state.LoadAsync();Assert.True(state.CanRetryEdit);await state.RetryEditAsync();Assert.Equal(2,sends);}
        if(mode is "lost" or "clear")
        {
            Assert.NotNull(state.PendingEdit);Assert.Equal(1,sends);journal.FailClear=false;
            var current=company.Selection!;company.TrySelect(company.Clear(),current.UserId,new(current.Scope,current.Roles.ToArray(),["DevelopmentPlanning.View"]));
            await state.LoadAsync();Assert.NotNull(state.PendingEdit);Assert.True(state.CanRecoverEdit);await state.RecoverEditAsync();
        }
        Assert.Equal(mode=="retry"?2:1,sends);Assert.Null(state.PendingEdit);Assert.Null(journal.FacilityEditOriginal);Assert.NotNull(state.EditReceipt);Assert.NotNull(state.EditConfirmation);
    }
    [Fact]
    public async Task LateEditResponseDoesNotLeakAcrossCompanySelectionAndOriginalCanBeRecovered()
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Edit"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var api=new Mock<IDevelopmentServiceClient>();var source=Row();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([source]);
        var review=new DevelopmentFacilityEditReview(Selection().SelectionContext!,CompanyInventorySourceReview.Copy(source),"Engineer",DevelopmentFacilityEditReview.SourceDigest(source),new string('A',64),"",DateTimeOffset.UtcNow);review=review with{Revision=review.ContentRevision()};
        api.Setup(x=>x.ReviewFacilityEditAsync("facility-1","PROCESSING","Engineer",Field,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var journal=new DevelopmentDraftWorkspaceTests.Journal();var completion=new TaskCompletionSource<DevelopmentFacilityEditReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);DevelopmentFacilityEditReceipt? saved=null;
        api.Setup(x=>x.SaveFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<CancellationToken>())).Returns((DevelopmentFacilityEdit command,CancellationToken _)=>
        {
            Assert.Equal(command.Revision(),journal.FacilityEditOriginal!.Revision());var row=CompanyInventorySourceReview.Copy(command.Review.Original);row.FACILITY_LONG_NAME=command.LongName;row.DESCRIPTION=command.Description!;row.ROW_CHANGED_BY=command.Review.Context.ActorUserId;row.ROW_CHANGED_DATE=DateTime.UtcNow;saved=new(command.Revision(),row,DateTimeOffset.UtcNow);return completion.Task;
        });
        api.Setup(x=>x.RecoverFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentFacilityEdit command,CancellationToken _)=>{Assert.Equal(command.Revision(),saved!.IntentRevision);return saved;});
        using var state=new DevelopmentFacilityEvidenceState(company,fields.Object,api.Object,(_,_,fallback)=>fallback,journal);
        await state.LoadAsync();await state.ReviewEditAsync("facility-1","PROCESSING","Engineer");state.PrepareEdit("Updated",null,"Correct the recorded name",true);
        var submit=state.SubmitEditAsync();Assert.NotNull(saved);var previous=company.Selection!;company.Clear();completion.SetResult(saved!);await submit;
        Assert.Null(state.EditReceipt);Assert.NotNull(journal.FacilityEditOriginal);
        company.TrySelect(company.Clear(),previous.UserId,new(previous.Scope,previous.Roles.ToArray(),["DevelopmentPlanning.View"]));await state.LoadAsync();Assert.True(state.CanRecoverEdit);await state.RecoverEditAsync();
        Assert.NotNull(state.EditReceipt);Assert.Null(journal.FacilityEditOriginal);api.Verify(x=>x.SaveFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<CancellationToken>()),Times.Once());
    }
    [Fact]
    public async Task ActualFacilityPageRetriesTheRetainedOriginal()
    {
        using var company=Company();var selected=company.Selection!;company.TrySelect(company.Clear(),selected.UserId,new(selected.Scope,selected.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Edit"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var source=Row();var review=new DevelopmentFacilityEditReview(Selection().SelectionContext!,CompanyInventorySourceReview.Copy(source),"Engineer",DevelopmentFacilityEditReview.SourceDigest(source),new string('A',64),"",DateTimeOffset.UtcNow);review=review with{Revision=review.ContentRevision()};
        var original=new DevelopmentFacilityEdit(review,Guid.NewGuid(),"Updated",null,"Correct the recorded name",true);var journal=new DevelopmentDraftWorkspaceTests.Journal();await journal.RetainFacilityEditAsync(original);
        var api=new Mock<IDevelopmentServiceClient>();api.Setup(x=>x.SaveFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentFacilityEdit command,CancellationToken _)=>
        {
            Assert.Equal(original.Revision(),command.Revision());var row=CompanyInventorySourceReview.Copy(command.Review.Original);row.FACILITY_LONG_NAME=command.LongName;row.DESCRIPTION=command.Description!;row.ROW_CHANGED_BY=command.Review.Context.ActorUserId;row.ROW_CHANGED_DATE=DateTime.UtcNow;return new(command.Revision(),row,DateTimeOffset.UtcNow);
        });
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.Facilities>();page.WaitForAssertion(()=>Assert.Contains("Retry original edit",page.Markup));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Retry original edit")).Click();page.WaitForAssertion(()=>Assert.Contains("original facility edit was confirmed",page.Markup));
        Assert.Null(journal.FacilityEditOriginal);api.Verify(x=>x.SaveFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<CancellationToken>()),Times.Once());
    }
    [Fact]
    public async Task ActualFacilityPageReviewsAndSavesTheExplicitRetainedEdit()
    {
        using var company=Company();var selection=company.Selection!;company.TrySelect(company.Clear(),selection.UserId,new(selection.Scope,selection.Roles.ToArray(),["DevelopmentPlanning.View","DevelopmentPlanning.Edit"]));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Selection());
        var api=new Mock<IDevelopmentServiceClient>();var source=Row();api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),Field)).ReturnsAsync([source]);
        var review=new DevelopmentFacilityEditReview(Selection().SelectionContext!,CompanyInventorySourceReview.Copy(source),"Engineer",DevelopmentFacilityEditReview.SourceDigest(source),new string('A',64),"",DateTimeOffset.UtcNow);review=review with{Revision=review.ContentRevision()};
        api.Setup(x=>x.ReviewFacilityEditAsync("facility-1","PROCESSING","Engineer",Field,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var journal=new DevelopmentDraftWorkspaceTests.Journal();api.Setup(x=>x.SaveFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentFacilityEdit command,CancellationToken _)=>
        {
            Assert.Equal(command.Revision(),journal.FacilityEditOriginal!.Revision());var row=CompanyInventorySourceReview.Copy(command.Review.Original);row.FACILITY_LONG_NAME=command.LongName;row.DESCRIPTION=command.Description!;row.ROW_CHANGED_BY=command.Review.Context.ActorUserId;row.ROW_CHANGED_DATE=DateTime.UtcNow;return new(command.Revision(),row,DateTimeOffset.UtcNow);
        });
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.Facilities>();page.WaitForAssertion(()=>Assert.NotEmpty(page.FindAll("button[aria-label^='Inspect facility-1']")));
        page.Find("button[aria-label='Inspect facility-1, type PROCESSING']").Click();await page.InvokeAsync(()=>page.FindComponents<MudSelect<string>>().Single(component=>component.Instance.Label=="Your current editor role").Instance.ValueChanged.InvokeAsync("Engineer"));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Check facility edit authority")).Click();page.WaitForAssertion(()=>Assert.Contains("Review proposed edit",page.Markup));
        var inputs=page.FindComponents<MudTextField<string>>();await page.InvokeAsync(()=>inputs.Single(input=>input.Instance.Label=="Facility long name").Instance.ValueChanged.InvokeAsync("Updated name"));
        await page.InvokeAsync(()=>inputs.Single(input=>input.Instance.Label=="Reason for change").Instance.ValueChanged.InvokeAsync("Correct the recorded name"));
        await page.InvokeAsync(()=>page.FindComponent<MudCheckBox<bool>>().Instance.ValueChanged.InvokeAsync(true));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Review proposed edit")).Click();page.WaitForAssertion(()=>Assert.Contains("Confirm the original facility edit",page.Markup));
        page.FindAll("button").Single(button=>button.TextContent.Contains("Save facility edit")).Click();page.WaitForAssertion(()=>Assert.Contains("original facility edit was confirmed",page.Markup));
        Assert.Null(journal.FacilityEditOriginal);api.Verify(x=>x.SaveFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<CancellationToken>()),Times.Once());
    }
    private sealed class Transport:HttpMessageHandler
    {
        public string? Address;public bool Missing;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Address=request.RequestUri!.AbsoluteUri;return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=Missing?new StringContent("null",System.Text.Encoding.UTF8,"application/json"):request.RequestUri.AbsolutePath.EndsWith("/equipment",StringComparison.Ordinal)?JsonContent.Create(new[]{new DevelopmentFacilityEquipmentItem("pump",7,"source")}):JsonContent.Create(new[]{Row()})});}
    }
    [Fact]
    public async Task TypedClientAssertsReviewedFieldAndRejectsMissingPopulation()
    {
        using var company=Company();var transport=new Transport();using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};
        var client=new DevelopmentServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<DevelopmentServiceClient>.Instance);
        Assert.Single(await client.GetFacilitiesAsync(fieldId:Field));Assert.EndsWith("/facilities?fieldId="+Uri.EscapeDataString(Field),transport.Address);
        Assert.Equal("pump",Assert.Single(await client.GetFacilityEquipmentAsync("plant/one","gas+type",Field)).EquipmentId);
        Assert.EndsWith("/equipment?facilityId="+Uri.EscapeDataString("plant/one")+"&facilityType="+Uri.EscapeDataString("gas+type")+"&fieldId="+Uri.EscapeDataString(Field),transport.Address);
        transport.Missing=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetFacilitiesAsync(fieldId:Field));
    }
}
