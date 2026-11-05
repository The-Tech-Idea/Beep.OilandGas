using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components.BusinessProcess;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderPersonnelSourcePanelTests
{
    [Theory][InlineData("record")][InlineData("lost")][InlineData("fresh")][InlineData("permission")][InlineData("actor")][InlineData("late-field")][InlineData("original-reference")]
    public async Task ActualIndependentPersonnelAcceptanceFormRetainsBeforeTransportAndRecoversExactPrivateReference(string mode)
    {
        var f=new Beep.OilandGas.ApiService.Tests.WorkOrderPersonnelAcceptanceFixture(requirementProof:mode=="original-reference",originalReference:mode=="original-reference");var context=f.Evidence.Assignment.Roster.Context;var original=f.Command;using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],mode=="permission"?[]:["HSE.ApprovePermit"])));
        using var state=new WorkOrderExecutionPermitDecisionState(company);var fields=new Mock<IFieldSelectionContext>();fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("field");var nav=new Nav();var client=new Mock<ICompanyMaintenanceHandoffClient>();var writes=0;var recoveries=0;
        if(mode=="fresh")nav.NavigateTo(nav.GetUriWithQueryParameters(new Dictionary<string,object?>{{"personnelAcceptanceInspection",original.InspectionId},{"personnelAcceptanceIntent",original.IntentId.ToString("D")}}));
        client.Setup(c=>c.RecordWorkOrderPersonnelAcceptanceAsync(It.IsAny<CompanyWorkOrderPersonnelAcceptanceCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderPersonnelAcceptanceCommand command,CancellationToken _)=>
        {
            writes++;Assert.Equal(f.Command.RequirementReferenceVersion,command.RequirementReferenceVersion);original=CompanyInventorySourceReview.Copy(command);Assert.Contains("personnelAcceptanceInspection="+command.InspectionId,nav.Uri);Assert.Contains("personnelAcceptanceIntent="+command.IntentId.ToString("D"),nav.Uri);Assert.NotNull(state.VisiblePersonnelAcceptanceCommand);
            if(mode=="lost")throw new IOException("Unconfirmed acceptance");if(mode=="late-field"){fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("other");fields.Raise(c=>c.CurrentFieldChanged+=null!,"other");}return Task.FromResult(command.Prepare(f.Pending,DateTime.UtcNow));
        });
        client.Setup(c=>c.RecoverWorkOrderPersonnelAcceptanceReferenceAsync(It.IsAny<CompanyWorkOrderPersonnelAcceptanceReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderPersonnelAcceptanceReference reference,CancellationToken _)=>{recoveries++;Assert.Equal(original.IntentId,reference.IntentId);Assert.Equal(original.InspectionId,reference.InspectionId);return Task.FromResult(new CompanyWorkOrderPersonnelAcceptanceRecovery(original,original.Prepare(f.Pending,DateTime.UtcNow)));});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<NavigationManager>(nav);services.AddSingleton<AuthenticationStateProvider>(new Auth(mode=="actor"?"other":context.ActorUserId));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderPermitPersonnelSourcePanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Permit",original.Review.Assignment.Query.Permit},{"SourceRevision","source"}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            void Set(string name,object value)=>panel.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,value);
            Task Invoke(string name)=>EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!)).InvokeAsync();
            if(mode=="fresh"){await Invoke("RecoverAcceptanceAsync");Assert.Equal(0,writes);Assert.Equal(1,recoveries);Assert.Contains("Independent personnel acceptance recorded",root.ToHtmlString());return;}
            Set("_policyEvidence",f.Evidence);Set("_acceptInspection",f.Pending.INSPECTION_ID);Set("_acceptReason","Explicit original independent personnel acceptance");await Invoke("AcceptPersonnelAsync");Assert.Equal(mode is "actor" or "permission"?0:1,writes);
            if(mode=="lost"){Assert.Null(state.PersonnelAcceptanceReceipt);Assert.False(state.CanAcceptPersonnel);Assert.Contains(original.IntentId.ToString("D"),root.ToHtmlString());await Invoke("RecoverAcceptanceAsync");Assert.Equal(1,writes);Assert.Equal(1,recoveries);}
            if(mode is "record" or "lost" or "original-reference"){original.RequireRecorded(state.PersonnelAcceptanceReceipt!);Assert.Contains("Independent personnel acceptance recorded",root.ToHtmlString());Assert.Contains("Permit approval and authorization to work remain separate",root.ToHtmlString());}
            if(mode=="late-field"){Assert.Null(state.PersonnelAcceptanceReceipt);Assert.DoesNotContain("Independent personnel acceptance recorded",root.ToHtmlString());}
        });
    }
    [Theory][InlineData("record")][InlineData("lost")][InlineData("fresh")][InlineData("permission")][InlineData("actor")][InlineData("late-field")]
    public async Task ActualMakerCancellationFormRetainsOriginalReferencesRecoversAndBeginsFreshDutyOnlyAfterConfirmation(string mode)
    {
        var original=WorkOrderPersonnelReleaseClientTests.Original();var assignment=original.Original;var context=assignment.Command.Review.Personnel.Context;using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],mode=="permission"?["HSE.ApprovePermit"]:["HSE.ApprovePermit","Maintenance.ReleasePersonnel"])));
        using var state=new WorkOrderExecutionPermitDecisionState(company);state.Attach(assignment.Command.Review.Personnel.Query.Permit,"source");var client=new Mock<ICompanyMaintenanceHandoffClient>();var assignmentReference=new CompanyWorkOrderPersonnelAssignmentReference(assignment.Command.Review.Personnel.Query.Permit,assignment.Command.IntentId);
        client.Setup(c=>c.RecoverWorkOrderPersonnelAssignmentReferenceAsync(assignmentReference,It.IsAny<CancellationToken>())).ReturnsAsync(assignment);if(mode!="permission" && mode!="fresh")await state.RecoverPersonnelReferenceAsync(client.Object,assignmentReference);
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("field");var nav=new Nav();var writes=0;var recoveries=0;
        if(mode=="fresh")nav.NavigateTo(nav.GetUriWithQueryParameters(new Dictionary<string,object?>{{"personnelAssignmentId",assignment.Command.IntentId.ToString("D")},{"personnelReleaseId",original.IntentId.ToString("D")}}));
        client.Setup(c=>c.RecordWorkOrderPersonnelReleaseAsync(It.IsAny<CompanyWorkOrderPersonnelReleaseCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderPersonnelReleaseCommand command,CancellationToken _)=>
        {
            writes++;Assert.Contains("personnelAssignmentId="+assignment.Command.IntentId.ToString("D"),nav.Uri);Assert.Contains("personnelReleaseId="+command.IntentId.ToString("D"),nav.Uri);original=CompanyInventorySourceReview.Copy(command);
            if(mode=="lost")throw new IOException("Cancellation confirmation unavailable");
            if(mode=="late-field"){fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("other");fields.Raise(c=>c.CurrentFieldChanged+=null!,"other");}
            return Task.FromResult(command.Prepare(DateTime.UtcNow));
        });
        client.Setup(c=>c.RecoverWorkOrderPersonnelReleaseReferenceAsync(It.IsAny<CompanyWorkOrderPersonnelReleaseReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderPersonnelReleaseReference reference,CancellationToken _)=>{recoveries++;Assert.Equal(original.IntentId,reference.IntentId);return Task.FromResult(new CompanyWorkOrderPersonnelReleaseRecovery(original,original.Prepare(DateTime.UtcNow)));});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(mode=="actor"?"other":context.ActorUserId));services.AddSingleton<NavigationManager>(nav);services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderPermitPersonnelSourcePanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Permit",assignmentReference.Permit},{"SourceRevision","source"}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            Task Invoke(string name)=>EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!)).InvokeAsync();
            if(mode=="fresh"){await Invoke("RecoverReleaseAsync");Assert.Equal(1,recoveries);Assert.Equal(0,writes);Assert.Contains("Personnel cancellation recorded",root.ToHtmlString());return;}
            if(mode=="record"){Assert.Contains("Explicit personnel cancellation rationale",root.ToHtmlString());Assert.Equal("",(string)panel.GetType().GetField("_releaseReason",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel)!);}
            panel.GetType().GetField("_releaseReason",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,"Explicit original cancellation");await Invoke("ReleaseAsync");Assert.Equal(mode is "actor" or "permission"?0:1,writes);
            if(mode=="lost"){Assert.Null(state.PersonnelReleaseReceipt);Assert.False(state.CanReleasePersonnel);Assert.Contains(original.IntentId.ToString("D"),root.ToHtmlString());await Invoke("RecoverReleaseAsync");Assert.Equal(1,writes);Assert.Equal(1,recoveries);}
            if(mode is "record" or "lost"){original.RequireRecorded(state.PersonnelReleaseReceipt!);Assert.Contains("Personnel cancellation recorded",root.ToHtmlString());await Invoke("BeginNewDutyAsync");Assert.True(state.CanAssignPersonnel);Assert.Null(state.VisiblePersonnelReference);Assert.DoesNotContain("personnelReleaseId",nav.Uri);Assert.Equal("",(string)panel.GetType().GetField("_employee",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel)!);Assert.Equal(1,writes);}
            if(mode=="late-field"){Assert.Null(state.PersonnelReleaseReceipt);Assert.DoesNotContain("Personnel cancellation recorded",root.ToHtmlString());}
        });
    }
    [Theory][InlineData("current")][InlineData("empty")][InlineData("changed-license")][InlineData("actor")][InlineData("permission")][InlineData("late-field")][InlineData("late-selection")][InlineData("company")][InlineData("cancelled")][InlineData("policy")][InlineData("policy-zero")][InlineData("employment-rejected")][InlineData("employment-choice")][InlineData("employment-position")]
    public async Task ActualIndependentReviewerSelectsPublicAssignmentAndComparesCurrentEvidenceWithoutMakerPermission(string mode)
    {
        var policyEvidence=mode.StartsWith("employment-",StringComparison.Ordinal)?CompanyEmploymentEligibilityTests.Review(mode=="employment-rejected"?"rejected":mode=="employment-choice"?"company-choice":"wrong-position"):mode is "policy" or "policy-zero"?CompanyPersonnelPolicyClientTests.Evidence(mode=="policy-zero"?"zero":"current"):null;
        var review=policyEvidence?.Assignment??WorkOrderAssignedPersonnelTests.Review(mode is "changed-license" or "cancelled"?mode:"current");var context=review.Roster.Context;
        var roster=mode=="empty"?CompanyWorkOrderPersonnelAssignmentRoster.Create(context,review.Query.Permit,[],DateTime.UtcNow):review.Roster;
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],mode=="permission"?[]:["HSE.ApprovePermit"])));
        using var state=new WorkOrderExecutionPermitDecisionState(company);var fields=new Mock<IFieldSelectionContext>();fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("field");var workflows=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;var comparisons=0;WorkOrderPermitPersonnelSourcePanel? panel=null;
        if(policyEvidence is not null)workflows.Setup(c=>c.ReviewWorkOrderPersonnelPolicyEvidenceAsync(review.Query,It.IsAny<CancellationToken>())).ReturnsAsync(policyEvidence);
        workflows.Setup(c=>c.ReviewWorkOrderPersonnelAssignmentsAsync(review.Query.Permit,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult(new CompanyWorkOrderPersonnelAssignmentRosterReview(review.CurrentPersonnel.PermitReview,roster));});
        workflows.Setup(c=>c.ReviewWorkOrderAssignedPersonnelAsync(review.Query,It.IsAny<CancellationToken>())).Returns(()=>
        {
            comparisons++;if(mode=="late-field"){fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("other");fields.Raise(c=>c.CurrentFieldChanged+=null!,"other");}
            if(mode=="late-selection")panel!.GetType().GetMethod("AssignmentChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,["other-selection"]);return Task.FromResult(review);
        });
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton(workflows.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(mode=="actor"?"other":context.ActorUserId));services.AddSingleton<NavigationManager>(new Nav());services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderPermitPersonnelSourcePanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Permit",review.Query.Permit},{"SourceRevision","source"}}));panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            Task Invoke(string name)=>EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!)).InvokeAsync();
            await Invoke("ReadRosterAsync");Assert.Equal(mode is "actor" or "permission"?0:1,reads);Assert.DoesNotContain("Retain and record personnel assignment",root.ToHtmlString());
            if(mode is "actor" or "permission"){Assert.DoesNotContain("RecordedDuty",root.ToHtmlString());return;}
            if(mode=="empty"){Assert.Contains("No native personnel assignments",root.ToHtmlString());Assert.Equal(0,comparisons);return;}
            Assert.Contains("Recorded personnel assignment",root.ToHtmlString());Assert.Equal("",(string)panel.GetType().GetField("_assignment",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel)!);
            panel.GetType().GetMethod("AssignmentChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,[review.Query.AssignmentId]);await Invoke("CompareAssignedAsync");Assert.Equal(1,comparisons);var html=root.ToHtmlString();
            if(mode is "late-field" or "late-selection"){Assert.DoesNotContain("Recorded assignment and current evidence",html);return;}
            Assert.Contains("Recorded assignment and current evidence",html);Assert.Contains("RecordedDuty",html);Assert.Contains("Current company membership",html);Assert.Contains("Configured duty/competency/jurisdiction",html);Assert.DoesNotContain("Private original personnel rationale",html);
            if(policyEvidence is not null){await Invoke("ReviewPolicyEvidenceAsync");var evidenceHtml=root.ToHtmlString();Assert.Contains("Current company personnel policy and source evidence",evidenceHtml);Assert.Contains("TRAINING_HOURS",evidenceHtml);Assert.Contains("Independent PERSONNEL acceptance",evidenceHtml);Assert.DoesNotContain("Private policy maker rationale",evidenceHtml);if(mode=="policy-zero")Assert.Contains("Value unconfirmed",evidenceHtml);if(mode=="employment-rejected")Assert.Contains("employment status is not explicitly accepted",evidenceHtml);if(mode=="employment-choice")Assert.Contains("Company-specific eligible status",evidenceHtml);if(mode=="employment-position")Assert.Contains("employment position does not match",evidenceHtml);}
            if(mode=="changed-license")Assert.Contains("licence evidence changed",html);if(mode=="cancelled"){Assert.Contains("Personnel assignment cancelled by",html);Assert.DoesNotContain("Private cancellation rationale",html);}
            if(mode=="company"){company.Clear();await Task.Yield();Assert.DoesNotContain("RecordedDuty",root.ToHtmlString());Assert.DoesNotContain("Current company membership",root.ToHtmlString());}
        });
    }
    [Theory][InlineData("record")][InlineData("lost")][InlineData("invalid-utc")][InlineData("permission")][InlineData("actor")][InlineData("late-field")][InlineData("fresh")]
    public async Task ActualPersonnelAssignmentFormRetainsBeforeTransportAndRecoversOriginalPrivateReference(string mode)
    {
        var review=WorkOrderPersonnelSourceClientTests.Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(review.Personnel.Context.Company,["ConfiguredRole"],mode=="permission"?["HSE.ApprovePermit"]:["HSE.ApprovePermit","Maintenance.AssignPersonnel"])));
        using var state=new WorkOrderExecutionPermitDecisionState(company);var fields=new Mock<IFieldSelectionContext>();fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("field");var workflows=new Mock<ICompanyMaintenanceHandoffClient>();var nav=new Nav();var writes=0;var recoveries=0;
        var original=new CompanyWorkOrderPersonnelAssignmentCommand(review,Guid.NewGuid(),"RecordedDuty",review.PermitReview.Proposal.ValidFromUtc,review.PermitReview.Proposal.ValidToUtc,"Explicit original personnel rationale");var recorded=original.Prepare(DateTime.UtcNow);
        workflows.Setup(c=>c.ReviewWorkOrderPersonnelSourceAsync(review.Personnel.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        workflows.Setup(c=>c.RecordWorkOrderPersonnelAssignmentAsync(It.IsAny<CompanyWorkOrderPersonnelAssignmentCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderPersonnelAssignmentCommand command,CancellationToken _)=>
        {
            writes++;Assert.Contains("personnelAssignmentId="+command.IntentId.ToString("D"),nav.Uri);Assert.Contains("permitReviewId="+review.Personnel.Query.Permit.IntentId.ToString("D"),nav.Uri);
            original=CompanyInventorySourceReview.Copy(command);recorded=original.Prepare(DateTime.UtcNow);
            if(mode=="late-field"){fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("other");fields.Raise(c=>c.CurrentFieldChanged+=null!,"other");}
            if(mode=="lost")throw new IOException("Unconfirmed original assignment");return Task.FromResult(recorded);
        });
        workflows.Setup(c=>c.RecoverWorkOrderPersonnelAssignmentReferenceAsync(It.IsAny<CompanyWorkOrderPersonnelAssignmentReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderPersonnelAssignmentReference reference,CancellationToken _)=>{recoveries++;Assert.Equal(original.IntentId,reference.IntentId);return Task.FromResult(new CompanyWorkOrderPersonnelAssignmentRecovery(original,recorded));});
        if(mode=="fresh")nav.NavigateTo(nav.GetUriWithQueryParameters(new Dictionary<string,object?>{{"personnelAssignmentId",original.IntentId.ToString("D")}}));
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton(workflows.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(mode=="actor"?"other":"independent"));services.AddSingleton<NavigationManager>(nav);services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderPermitPersonnelSourcePanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Permit",review.Personnel.Query.Permit},{"SourceRevision","source"}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            void Set(string name,string value)=>panel.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,value);
            Task Invoke(string name)=>EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!)).InvokeAsync();
            if(mode=="fresh"){await Invoke("RecoverAsync");Assert.Equal(1,recoveries);Assert.Equal(0,writes);Assert.Contains("Personnel assignment recorded",root.ToHtmlString());return;}
            Set("_employee",review.Personnel.Query.EmployeeBaId);Set("_license",review.Personnel.Query.LicenseId);await Invoke("ReviewAsync");
            if(mode=="record"){Assert.Contains("Explicit personnel duty",root.ToHtmlString());Assert.Equal("",(string)panel.GetType().GetField("_from",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel)!);}
            Set("_duty","RecordedDuty");Set("_from",mode=="invalid-utc"?"2026-10-13":review.PermitReview.Proposal.ValidFromUtc.ToString("O"));Set("_to",review.PermitReview.Proposal.ValidToUtc.ToString("O"));Set("_reason","Explicit original personnel rationale");await Invoke("AssignAsync");
            Assert.Equal(mode is "permission" or "actor" or "invalid-utc"?0:1,writes);
            if(mode=="record"){original.RequireRecorded(state.PersonnelReceipt!);Assert.Contains("Personnel assignment recorded",root.ToHtmlString());}
            if(mode=="lost"){Assert.Null(state.PersonnelReceipt);Assert.False(state.CanAssignPersonnel);Assert.Contains(original.IntentId.ToString("D"),root.ToHtmlString());await Invoke("RecoverAsync");Assert.Equal(1,writes);Assert.Equal(1,recoveries);original.RequireRecorded(state.PersonnelReceipt!);}
            if(mode=="late-field"){Assert.Null(state.PersonnelReceipt);Assert.DoesNotContain("Personnel assignment recorded",root.ToHtmlString());}
        });
    }
    private sealed class Nav:NavigationManager{public Nav()=>Initialize("https://fixture/","https://fixture/ppdm39/process/instances/original");protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"ConfiguredRole")],"app"))));}
    private sealed class Components:IComponentActivator{internal WorkOrderPermitPersonnelSourcePanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is WorkOrderPermitPersonnelSourcePanel panel)Panel=panel;return result;}}
    private sealed class Js:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("current")][InlineData("unlinked")][InlineData("timezone")][InlineData("actor")][InlineData("permission")][InlineData("late-field")][InlineData("company")]
    public async Task ActualPersonnelPanelShowsOwnedRecordedFactsUnknownStatesAndClearsChangedContext(string mode)
    {
        var review=WorkOrderPersonnelSourceClientTests.Review(mode);using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(review.PermitReview.Context.Company,["ConfiguredRole"],mode=="permission"?[]:["HSE.ApprovePermit"])));var fields=new Mock<IFieldSelectionContext>();fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("field");var workflows=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;workflows.Setup(c=>c.ReviewWorkOrderPersonnelSourceAsync(review.Personnel.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="late-field"){fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("other");fields.Raise(c=>c.CurrentFieldChanged+=null!,"other");}return Task.FromResult(review);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(company);services.AddSingleton(new WorkOrderExecutionPermitDecisionState(company));services.AddSingleton<NavigationManager>(new Nav());services.AddSingleton(workflows.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(mode=="actor"?"other":"independent"));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderPermitPersonnelSourcePanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Permit",review.Personnel.Query.Permit},{"SourceRevision","source"}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;panel.GetType().GetField("_employee",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,review.Personnel.Query.EmployeeBaId);panel.GetType().GetField("_license",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,review.Personnel.Query.LicenseId);
            await EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod("ReviewAsync",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!)).InvokeAsync();var html=root.ToHtmlString();Assert.Equal(mode is "actor" or "permission"?0:1,reads);if(mode is "current" or "company" or "timezone"){Assert.Contains("Recorded technician",html);Assert.Contains("Recorded licence type",html);Assert.Contains("competency requirements",html);}else Assert.DoesNotContain("Recorded technician",html);if(mode=="timezone")Assert.Contains("source time-zone unconfirmed",html);if(mode=="unlinked")Assert.Contains("No exact active company employment membership",html);Assert.Contains("do not establish task assignment or competency",html);if(mode=="company"){company.Clear();await Task.Yield();Assert.DoesNotContain("Recorded technician",root.ToHtmlString());}
        });
    }
}
#endif
