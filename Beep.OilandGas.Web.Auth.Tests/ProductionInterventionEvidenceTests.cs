using Beep.OilandGas.Web.Components.BusinessProcess;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ProductionInterventionEvidenceTests
{
    [Fact]public async Task AcknowledgementReviewAppearsOnlyForRetainedEvidenceAndDisappearsAfterFreshConfirmation()
    {
        var source=Source();using var company=new Beep.OilandGas.Client.App.CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(source.Company!,[],[])));var state=new Beep.OilandGas.Web.Services.ProductionInterventionDecisionState(company);var attempt=state.Begin("well",new(){ExpectedSourceRevision=source.SourceRevision,FieldId="field",Decision="REJECTED",Note="Recorded original rejection"});Assert.DoesNotContain("Review acknowledged decision against recorded activity",await Render(source,state.Original("field","well"),state.HasAcknowledgement("field","well")));
        state.RetainAcknowledgement(attempt,new(){Success=true,FieldId="field",WellId="well",Decision="REJECTED",ReviewedSourceRevision=attempt.Request.ExpectedSourceRevision});Assert.Contains("Review acknowledged decision against recorded activity",await Render(source,state.Original("field","well"),state.HasAcknowledgement("field","well")));source.SourceRevision=new string('B',64);source.Status="REJECTED";source.LatestActivityType="WORKOVER_REJECTED";source.LatestRecordedBy="actor";source.LatestActivityRemark=attempt.Request.Note;await state.ReviewAcknowledgedOutcomeAsync("field","well",()=>Task.FromResult(new List<ProductionInterventionCandidateDto>{source}));var confirmed=await Render(source,state.Original("field","well"),state.HasAcknowledgement("field","well"));Assert.DoesNotContain("original decision outcome is unconfirmed",confirmed);Assert.DoesNotContain("Review acknowledged decision against recorded activity",confirmed);Assert.Contains("WORKOVER_REJECTED",confirmed);
    }
    [Fact]public async Task RecreatedEvidenceShowsSameRetainedOriginalDecisionAndEncodesItsReason()
    {
        var source=Source();using var company=new Beep.OilandGas.Client.App.CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(source.Company!,[],[])));var state=new Beep.OilandGas.Web.Services.ProductionInterventionDecisionState(company);state.Begin("well",new(){ExpectedSourceRevision=source.SourceRevision,FieldId="field",Decision="REJECTED",Note="Original <script>reason</script>"});var first=await Render(source,state.Original("field","well"));var recreated=await Render(source,state.Original("field","well"));Assert.Contains("original decision outcome is unconfirmed",first);Assert.Contains("Original REJECTED",recreated);Assert.Contains("Original &lt;script&gt;reason",recreated);Assert.DoesNotContain("<script>",recreated);Assert.True(state.IsBlocked("field","well"));company.Clear();Assert.DoesNotContain("Original REJECTED",await Render(source,state.Original("field","well")));
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("field")][InlineData("well")]
    public async Task ChangedSourceCannotExposeAnotherRetainedOriginalReason(string change)
    {
        var source=Source();using var company=new Beep.OilandGas.Client.App.CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(source.Company!,[],[])));var state=new Beep.OilandGas.Web.Services.ProductionInterventionDecisionState(company);state.Begin("well",new(){ExpectedSourceRevision=source.SourceRevision,FieldId="field",Decision="REJECTED",Note="Private original reason"});var retained=state.Original("field","well");if(change=="company")source.Company=new("tenant","other");if(change=="actor")source.Actor="other";if(change=="field")source.FieldId="other";if(change=="well")source.WellId="other";var html=await Render(source,retained);Assert.DoesNotContain("Private original reason",html);Assert.DoesNotContain("original decision outcome is unconfirmed",html);
    }
    [Fact] public async Task LatestRecordedDecisionShowsActualActorRationaleAndLinks()
    {
        var source=Source();source.LatestActivityType="WORKOVER_APPROVED";source.LatestRecordedBy="independent-reviewer";source.LatestActivityRemark="<script>rationale</script> WORK_ORDER_ID:order";
        var html=await Render(source);Assert.Contains("WORKOVER_APPROVED",html);Assert.Contains("independent-reviewer",html);Assert.Contains("WORK_ORDER_ID:order",html);
        Assert.Contains("&lt;script&gt;rationale",html);Assert.DoesNotContain("<script>",html);
    }
    private static ProductionInterventionCandidateDto Source()=>new(){SourceRevision=new string('A',64),Company=new("tenant","company"),Actor="actor",FieldId="field",WellId="well",SourceObservationNumber=3,LatestObservationNumber=4,RetrievedAtUtc=DateTimeOffset.UtcNow,InterventionType="Recorded type",Problem="Recorded note"};
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("No browser is available in this static renderer.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    private static async Task<string> Render(ProductionInterventionCandidateDto? source,Beep.OilandGas.Web.Services.ProductionInterventionDecisionState.Attempt? original=null,bool acknowledged=false)
    {
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<ProductionInterventionEvidence>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Source",source},{"OriginalDecision",original},{"HasAcknowledgement",acknowledged}}));return root.ToHtmlString();});
    }
    [Fact] public async Task AbsentMetricsAreUnavailableRatherThanZeroOrAssumedRecommendation()
    {
        var html=await Render(Source());Assert.Contains("Recorded activity for well well",html);Assert.Contains("Company company",html);Assert.Contains("FIELD field",html);Assert.Contains("Source observation: 3",html);Assert.Contains("Latest observation: 4",html);Assert.Contains("Deferred production: Unavailable",html);Assert.Contains("Days deferred: Unavailable",html);Assert.Contains("Estimated cost: Unavailable",html);Assert.Contains("Priority: Unavailable",html);Assert.DoesNotContain("BOPD",html);Assert.DoesNotContain(" USD",html);Assert.DoesNotContain("Proceed with",html);Assert.DoesNotContain("$",html);
    }
    [Fact] public async Task ActualZeroMetricsRemainRecordedWithTheirDeclaredUnits()
    {
        var source=Source();source.DeferredBopd=0;source.EstCostUsd=0;source.EstDaysSinceOnset=0;var html=await Render(source);Assert.Contains(" BOPD",html);Assert.Contains(" USD",html);Assert.Contains("Days deferred: 0",html);Assert.DoesNotContain("Estimated cost: Unavailable",html);Assert.DoesNotContain("$K",html);
    }
    [Fact] public async Task SourceDateDoesNotAcquireAnAssumedTimezoneOrOnsetInterpretation()
    {
        var source=Source();source.SourceStartedAt=new(2026,10,6,10,30,0,DateTimeKind.Unspecified);var html=await Render(source);Assert.Contains("2026-10-06 10:30:00",html);Assert.Contains("source timezone unavailable",html);Assert.Contains("Retrieved (UTC)",html);Assert.DoesNotContain("Rate drop started",html);
    }
    [Fact] public async Task NotesAreHtmlEncoded(){var source=Source();source.Problem="<script>alert(1)</script>";var html=await Render(source);Assert.Contains("&lt;script&gt;",html);Assert.DoesNotContain("<script>",html);}
    [Fact] public async Task MissingSourceHasExplicitEmptyState(){var html=await Render(null);Assert.Contains("Recorded intervention evidence is unavailable",html);Assert.DoesNotContain("Estimated cost:",html);}
    [Fact] public void NullableMetricsAndActualEmptyRowsAreValid(){var source=Source();ProductionInterventionCandidateChecks.Require([source],source.Company!,"actor","field");ProductionInterventionCandidateChecks.Require([],source.Company!,"actor","field");Assert.Null(source.DeferredBopd);Assert.Null(source.EstDaysSinceOnset);Assert.Null(source.EstCostUsd);Assert.Null(source.Priority);}
    [Theory][InlineData("company")][InlineData("actor")][InlineData("field")][InlineData("well")][InlineData("source")][InlineData("latest")][InlineData("retrieved")][InlineData("offset")][InlineData("duplicate")][InlineData("null")][InlineData("rate")][InlineData("cost")][InlineData("days")][InlineData("revision")]
    public void MismatchedOrInvalidEvidenceIsRefused(string fault)
    {
        var source=Source();List<ProductionInterventionCandidateDto> rows=[source];switch(fault){case "revision":source.SourceRevision="";break;case "company":source.Company=new("tenant","other");break;case "actor":source.Actor="other";break;case "field":source.FieldId="other";break;case "well":source.WellId="";break;case "source":source.SourceObservationNumber=null;break;case "latest":source.LatestObservationNumber=null;break;case "retrieved":source.RetrievedAtUtc=default;break;case "offset":source.RetrievedAtUtc=DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(3));break;case "duplicate":rows.Add(source);break;case "null":rows=null!;break;case "rate":source.DeferredBopd=double.NaN;break;case "cost":source.EstCostUsd=-1;break;default:source.EstDaysSinceOnset=-1;break;}
        Assert.Throws<InvalidOperationException>(()=>ProductionInterventionCandidateChecks.Require(rows,new("tenant","company"),"actor","field"));
    }
    [Theory][InlineData(null)][InlineData("preparer")]
    public async Task RecordedMakerIsShownWithoutInventingIdentity(string? preparer){var source=Source();source.SourcePreparer=preparer;var html=await Render(source);Assert.Contains("Recorded preparer: "+(preparer ?? "Unavailable"),html);}
    [Fact] public void ApprovalEligibilityRequiresKnownDifferentActor(){Assert.False(ProductionInterventionDecisionChecks.IsIndependentApprover(null,"actor"));Assert.False(ProductionInterventionDecisionChecks.IsIndependentApprover("actor","actor"));Assert.False(ProductionInterventionDecisionChecks.IsIndependentApprover("preparer",null));Assert.True(ProductionInterventionDecisionChecks.IsIndependentApprover("preparer","actor"));}

}
