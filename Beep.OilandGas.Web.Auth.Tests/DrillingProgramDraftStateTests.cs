#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Components.Workflow;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DrillingProgramDraftStateTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DrillingProgramDraftNativeTests.Fixture Native=new();internal Mock<IDrillingServiceClient> Client=new();internal DrillingProgramDraftState State=null!;
        internal DevelopmentDraftWorkspaceTests.Journal Journal=>Native.Investment.Handoff.Source.Base.Journal;
        internal Beep.OilandGas.Client.App.CompanyClientContext Company=>Native.Investment.Handoff.Source.Base.Company;
        internal Mock<IPersonaContextService> Personas=>Native.Investment.Handoff.Source.Base.Baseline.Reader.Personas;
        internal Mock<IAfeServiceClient> Afes=>Native.Investment.Handoff.Source.Base.Client;
        internal async Task Prepare()
        {
            await Native.Prepare();Select(true);Client.Setup(c=>c.ReviewProgramBasisAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Native.Current);Client.Setup(c=>c.GetProgramDraftsAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>{var value=new DrillingProgramDraftList(Native.Current,[],"",DateTimeOffset.UtcNow);return value with{Revision=value.ContentRevision()};});State=NewState();
        }
        internal void Select(bool write)=>Company.TrySelect(Company.Clear(),Native.Current.Scope().ActorUserId,new(Native.Current.Scope().Company,["ConfiguredDrillingEngineer"],write?["Drilling.View","Drilling.Create","Accounting.View","DevelopmentPlanning.View","Workflow.View"]:["Drilling.View","Accounting.View","DevelopmentPlanning.View","Workflow.View"]));
        internal DrillingProgramDraftState NewState()=>new(Company,Personas.Object,Client.Object,Afes.Object,Journal,(_,_,text)=>text);
        internal async Task Inspect(){await State.LoadAsync(Native.Current.Well.Uwi);await State.InspectAsync(Native.Current.Funding.Source.AfeId,null);Assert.NotNull(State.Basis);}
        internal void Freeze()=>State.Prepare(Native.Command.Name,Native.Command.Objective,Native.Command.WellType,Native.Command.Profile,Native.Command.MeasuredDepth,Native.Command.VerticalDepth,Native.Command.DepthUnit,Native.Command.DurationDays,Native.Command.PlannedSpudDate.ToDateTime(TimeOnly.MinValue),Native.Command.PlannedCompletionDate.ToDateTime(TimeOnly.MinValue));
        internal static DrillingProgramDraftReceipt Receipt(CreateDrillingProgramDraft original,bool exists=true)=>new(original.Revision(),original.ProgramId(),exists?original.ToRecord(DateTime.UtcNow):null,DateTimeOffset.UtcNow);
        public void Dispose(){State?.Dispose();Native.Dispose();}
    }
    [Fact]public async Task ExplicitInputsAndApprovedSourceAreFrozenAndRetainedBeforeAnyWrite()
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.State.Prepare("Name","Objective",null,null,null,null,null,null,null,null);Assert.Null(f.State.Prepared);f.Freeze();var original=f.State.Prepared!;Assert.Equal(f.Native.Current.Funding.Source.Afe().RequestedAmount,original.Basis.Funding.Source.Afe().RequestedAmount);
        f.Client.Setup(c=>c.RecordProgramDraftAsync(It.IsAny<CreateDrillingProgramDraft>(),It.IsAny<CancellationToken>())).Returns((CreateDrillingProgramDraft body,CancellationToken _) =>{Assert.Equal(body.Revision(),f.Journal.ProgramOriginal!.Revision());return Task.FromResult(Fixture.Receipt(body));});await f.State.SaveAsync();Assert.Null(f.Journal.ProgramOriginal);Assert.Null(f.State.Pending);Assert.Equal("DRAFT",f.State.Receipt!.Recorded!.PROGRAM_STATUS);Assert.Contains("approval, execution and actual cost remain separate",f.State.Confirmation);
    }
    [Theory][InlineData("retain")][InlineData("transport")][InlineData("receipt")][InlineData("clear")]
    public async Task UncertainOutcomeRetainsExactOriginalAndBlocksReplacement(string fault)
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze();var original=f.State.Prepared!;f.Journal.FailRetain=fault=="retain";f.Journal.FailClear=fault=="clear";
        f.Client.Setup(c=>c.RecordProgramDraftAsync(It.IsAny<CreateDrillingProgramDraft>(),It.IsAny<CancellationToken>())).Returns((CreateDrillingProgramDraft body,CancellationToken _) =>fault=="transport"?Task.FromException<DrillingProgramDraftReceipt>(new HttpRequestException()):Task.FromResult(fault=="receipt"?Fixture.Receipt(body) with{IntentRevision=new('F',64)}:Fixture.Receipt(body)));
        await f.State.SaveAsync();Assert.Equal(original.Revision(),f.State.Pending!.Revision());Assert.Null(f.State.Receipt);Assert.NotNull(f.State.Error);Assert.False(f.State.CanPrepare);await f.State.SaveAsync();f.Client.Verify(c=>c.RecordProgramDraftAsync(It.IsAny<CreateDrillingProgramDraft>(),It.IsAny<CancellationToken>()),fault=="retain"?Times.Never():Times.Once());
        f.Journal.FailRetain=false;f.Journal.FailClear=false;f.Client.Setup(c=>c.RecoverProgramDraftAsync(It.IsAny<CreateDrillingProgramDraft>(),It.IsAny<CancellationToken>())).ReturnsAsync(Fixture.Receipt(original,fault!="retain"));await f.State.RecoverAsync();Assert.Null(f.State.Pending);Assert.Null(f.Journal.ProgramOriginal);
    }
    [Fact]public async Task NewReadOnlyOwnerRecoversBeforeAnotherBasisOrFundingRead()
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze();var original=f.State.Prepared!;f.Journal.ProgramOriginal=original;f.Select(false);using var state=f.NewState();await state.LoadAsync(original.Basis.Well.Uwi);Assert.True(state.CanRecover);Assert.False(state.CanCreate);
        f.Client.Setup(c=>c.RecoverProgramDraftAsync(It.IsAny<CreateDrillingProgramDraft>(),It.IsAny<CancellationToken>())).ReturnsAsync(Fixture.Receipt(original));await state.RecoverAsync();Assert.NotNull(state.Receipt!.Recorded);f.Afes.Verify(c=>c.QueueAsync(It.IsAny<CancellationToken>()),Times.Once);f.Client.Verify(c=>c.ReviewProgramBasisAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Once);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")][InlineData("dispose")]
    public async Task LateWriteCannotAcknowledgeOrClearPrivateOriginalAfterContextChanges(string change)
    {
        using var f=new Fixture();await f.Prepare();await f.Inspect();f.Freeze();var original=f.State.Prepared!;var waiting=new TaskCompletionSource<DrillingProgramDraftReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);f.Client.Setup(c=>c.RecordProgramDraftAsync(It.IsAny<CreateDrillingProgramDraft>(),It.IsAny<CancellationToken>())).Returns(waiting.Task);var save=f.State.SaveAsync();
        if(change=="company")f.Company.Clear();else if(change=="persona")f.Personas.Raise(p=>p.Changed+=null);else if(change=="route")await f.State.LoadAsync("another-well");else f.State.Dispose();waiting.SetResult(Fixture.Receipt(original));await save;Assert.Null(f.State.Receipt);Assert.NotNull(f.Journal.ProgramOriginal);
    }
    [Fact]public async Task ActualOperationsPageReloadsRecordedProgramForReadOnlyCompanyReader()
    {
        using var f=new Fixture();await f.Prepare();f.Select(false);var row=f.Native.Command.ToRecord(DateTime.UtcNow);f.Client.Setup(c=>c.GetProgramDraftsAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>{var value=new DrillingProgramDraftList(f.Native.Current,[row],"",DateTimeOffset.UtcNow);return value with{Revision=value.ContentRevision()};});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton(f.Afes.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.RenderComponent<MudPopoverProvider>();
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/ppdm39/operations/drillingoperations?wellUwi="+Uri.EscapeDataString(f.Native.Current.Well.Uwi));var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Operations.DrillingOperations>();page.WaitForAssertion(()=>Assert.Contains("Recorded company AFE source",page.Markup));var funding=page.FindComponents<MudSelect<string>>().Single(c=>c.Instance.Label=="Recorded company AFE source");await funding.InvokeAsync(()=>funding.Instance.ValueChanged.InvokeAsync(f.Native.Current.Funding.Source.AfeId));page.FindAll("button").Single(b=>b.TextContent=="Inspect approved program basis").Click();
        page.WaitForAssertion(()=>Assert.Contains(row.PROGRAM_ID!,page.Markup));Assert.Contains("DRAFT",page.Markup);Assert.DoesNotContain("Review program draft",page.Markup);f.Client.Verify(c=>c.ReviewProgramBasisAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never());f.Client.Verify(c=>c.RecordProgramDraftAsync(It.IsAny<CreateDrillingProgramDraft>(),It.IsAny<CancellationToken>()),Times.Never());
    }
    [Fact]public async Task ActualNewWellProgramRouteReviewsExplicitInputsAndRecordsOnlyTheConfirmedDraft()
    {
        using var f=new Fixture();await f.Prepare();f.Client.Setup(c=>c.RecordProgramDraftAsync(It.IsAny<CreateDrillingProgramDraft>(),It.IsAny<CancellationToken>())).Returns((CreateDrillingProgramDraft body,CancellationToken _) =>{Assert.Equal(body.Revision(),f.Journal.ProgramOriginal!.Revision());return Task.FromResult(Fixture.Receipt(body));});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton(f.Afes.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(Mock.Of<IFieldSelectionContext>());ui.Services.AddSingleton(Mock.Of<IDevelopmentServiceClient>());ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.RenderComponent<MudPopoverProvider>();
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/development/well-design/new?wellUwi="+Uri.EscapeDataString(f.Native.Current.Well.Uwi));var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Development.WellDesign>();
        page.WaitForAssertion(()=>Assert.Contains("Recorded company AFE source",page.Markup));var funding=page.FindComponents<MudSelect<string>>().Single(c=>c.Instance.Label=="Recorded company AFE source");await funding.InvokeAsync(()=>funding.Instance.ValueChanged.InvokeAsync(f.Native.Current.Funding.Source.AfeId));page.FindAll("button").Single(b=>b.TextContent=="Inspect approved program basis").Click();page.WaitForAssertion(()=>Assert.Contains("Approved program basis",page.Markup));Assert.Contains("25.125 KWD",page.Markup);
        foreach(var field in page.FindComponents<MudTextField<string>>()){if(field.Instance.Label=="Program name")await field.InvokeAsync(()=>field.Instance.ValueChanged.InvokeAsync(f.Native.Command.Name));if(field.Instance.Label=="Program objectives and source rationale")await field.InvokeAsync(()=>field.Instance.ValueChanged.InvokeAsync(f.Native.Command.Objective));}
        foreach(var select in page.FindComponents<MudSelect<string>>()){var value=select.Instance.Label switch{"Well type"=>"DEVELOPMENT","Planned profile"=>"VERTICAL","Recorded depth unit"=>"m",_=>null};if(value is not null)await select.InvokeAsync(()=>select.Instance.ValueChanged.InvokeAsync(value));}
        foreach(var number in page.FindComponents<MudNumericField<decimal?>>())await number.InvokeAsync(()=>number.Instance.ValueChanged.InvokeAsync(number.Instance.Label=="Planned measured depth"?2500.25m:2500m));await page.FindComponent<MudNumericField<int?>>().InvokeAsync(()=>page.FindComponent<MudNumericField<int?>>().Instance.ValueChanged.InvokeAsync(25));
        foreach(var date in page.FindComponents<MudDatePicker>())await date.InvokeAsync(()=>date.Instance.DateChanged.InvokeAsync((date.Instance.Label=="Planned spud calendar date"?f.Native.Command.PlannedSpudDate:f.Native.Command.PlannedCompletionDate).ToDateTime(TimeOnly.MinValue)));
        page.FindAll("button").Single(b=>b.TextContent=="Review program draft").Click();page.WaitForAssertion(()=>Assert.Contains("Confirm original drilling program draft",page.Markup));f.Client.Verify(c=>c.RecordProgramDraftAsync(It.IsAny<CreateDrillingProgramDraft>(),It.IsAny<CancellationToken>()),Times.Never());page.FindAll("button").Single(b=>b.TextContent=="Confirm and retain program draft").Click();page.WaitForAssertion(()=>Assert.Contains("Recorded drilling program draft",page.Markup));Assert.Contains("Version 1",page.Markup);Assert.Contains("DRAFT",page.Markup);Assert.Null(f.Journal.ProgramOriginal);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task CompanyDepthSuggestionIsAvailableOnlyBeforeEnteringMeasurements(bool enterDepthFirst)
    {
        using var f=new Fixture();await f.Prepare();
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton(f.Afes.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));ui.RenderComponent<MudPopoverProvider>();
        var scope=f.Company.Selection!.Scope;var settings=new CompanyEnvironmentPreview(scope,"KW","Kuwait","ar-KW","oilfield","ft","psi","in",[],[],[]){TimeZoneId="Asia/Kuwait",DisplayCulture="en-GB",DateTimeFormat="yyyy-MM-dd HH:mm",Calendar="Gregorian"};
        var review=new CompanyEnvironmentEffectiveReview(scope,new string('A',64),new string('B',64),null,new string('C',64),settings){ReviewStatus="Current"};
        var display=new CompanyEnvironmentDisplay(f.Company.Version,review);
        var page=ui.RenderComponent<DrillingProgramDraftPanel>(p=>p.Add(x=>x.WellUwi,f.Native.Current.Well.Uwi).AddCascadingValue(display));
        page.WaitForAssertion(()=>Assert.Contains("Recorded company AFE source",page.Markup));var funding=page.FindComponents<MudSelect<string>>().Single(c=>c.Instance.Label=="Recorded company AFE source");await funding.InvokeAsync(()=>funding.Instance.ValueChanged.InvokeAsync(f.Native.Current.Funding.Source.AfeId));page.FindAll("button").Single(b=>b.TextContent=="Inspect approved program basis").Click();
        page.WaitForAssertion(()=>Assert.Contains("Use company depth unit (ft)",page.Markup));
        var local=display.LocalTime(f.Native.Current.Funding.Source.Recorded!.AtUtc.UtcDateTime);Assert.NotNull(local);Assert.Contains(local,page.Markup);
        if(enterDepthFirst){var depth=page.FindComponents<MudNumericField<decimal?>>().Single(c=>c.Instance.Label=="Planned measured depth");await depth.InvokeAsync(()=>depth.Instance.ValueChanged.InvokeAsync(100m));page.WaitForAssertion(()=>Assert.DoesNotContain("Use company depth unit (ft)",page.Markup));}
        else
        {
            page.FindAll("button").Single(b=>b.TextContent.Contains("Use company depth unit (ft)")).Click();var unit=page.FindComponents<MudSelect<string>>().Single(c=>c.Instance.Label=="Recorded depth unit");Assert.Equal("ft",unit.Instance.Value);
            var depth=page.FindComponents<MudNumericField<decimal?>>().Single(c=>c.Instance.Label=="Planned measured depth");await depth.InvokeAsync(()=>depth.Instance.ValueChanged.InvokeAsync(100m));
            page.WaitForAssertion(()=>Assert.True(page.FindComponents<MudSelect<string>>().Single(c=>c.Instance.Label=="Recorded depth unit").Instance.Disabled));
            page.FindAll("button").Single(b=>b.TextContent.Contains("Clear depths to change unit")).Click();
            page.WaitForAssertion(()=>Assert.False(page.FindComponents<MudSelect<string>>().Single(c=>c.Instance.Label=="Recorded depth unit").Instance.Disabled));
            Assert.Null(typeof(DrillingProgramDraftPanel).GetField("md",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.GetValue(page.Instance));
        }
        f.Client.Verify(c=>c.RecordProgramDraftAsync(It.IsAny<CreateDrillingProgramDraft>(),It.IsAny<CancellationToken>()),Times.Never());
        await page.InvokeAsync(f.Company.Clear);
        var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        page.WaitForAssertion(()=>{Assert.Null(typeof(DrillingProgramDraftPanel).GetField("unit",flags)!.GetValue(page.Instance));Assert.Null(typeof(DrillingProgramDraftPanel).GetField("md",flags)!.GetValue(page.Instance));});
    }
}
#endif
