#if WORKFLOW_INBOX_FOCUSED
extern alias actualOwners;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
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
public sealed class WorkOrderExecutionPermitApprovalPanelTests
{
    private sealed class Auth(string actor):AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"ConfiguredRole")],"fixture"))));
    }
    private sealed class Nav:NavigationManager
    {
        public Nav()=>Initialize("http://fixture/","http://fixture/ppdm39/process/instances/original" );
        protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();
    }
    private sealed class Components:IComponentActivator
    {
        internal WorkOrderExecutionPermitApprovalReviewPanel Panel=null!;
        internal WorkOrderRecordedApprovalPanel RecordedPanel=null!;
        internal WorkOrderExecutionStartPanel StartPanel=null!;
        internal WorkOrderSubmissionPanel SubmissionPanel=null!;
        internal WorkOrderMaterialUsagePanel MaterialUsagePanel=null!;
        internal WorkOrderLaborUsagePanel LaborUsagePanel=null!;
        public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is WorkOrderExecutionPermitApprovalReviewPanel panel)Panel=panel;if(result is WorkOrderRecordedApprovalPanel recorded)RecordedPanel=recorded;if(result is WorkOrderExecutionStartPanel start)StartPanel=start;if(result is WorkOrderSubmissionPanel submission)SubmissionPanel=submission;if(result is WorkOrderMaterialUsagePanel usage)MaterialUsagePanel=usage;if(result is WorkOrderLaborUsagePanel labor)LaborUsagePanel=labor;return result;}
    }
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);
    }
    private static Task Invoke(object panel,string method)=>(Task)panel.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("late-company")][InlineData("utc")][InlineData("quantity")][InlineData("role")][InlineData("permission")][InlineData("unit")][InlineData("field")]
    public async Task ActualLaborFormRetainsOriginalReferenceAndRequiresExplicitElapsedUnitAndUtcInterval(string mode)
    {
        var originalStart=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Original(true);var startCommand=originalStart.Command;var started=startCommand.Prepare(DateTime.UtcNow);
        var comparison=CompanyWorkOrderSubmissionInspectionReview.Create(new(startCommand,started),startCommand.Review.Prerequisites.Requirements,DateTime.UtcNow);
        var review=new CompanyWorkOrderLaborUsageReview(comparison,startCommand.Review.Stage,startCommand.Review.Prerequisites.Personnel,[],null,DateTime.UtcNow);
        var command=new CompanyWorkOrderLaborUsageCommand(review,Guid.NewGuid(),started.STARTED_AT_UTC,review.ComparedAtUtc,(decimal)(review.ComparedAtUtc.Ticks-started.STARTED_AT_UTC.Ticks)/TimeSpan.TicksPerSecond,WorkOrderLaborTimeUnit.Second,"Actual worked labor","Original duty report");command.Require();var context=comparison.Current.Context;
        var start=new CompanyWorkOrderExecutionStartReference(command.Review.Execution.Start.Permit,command.Review.Execution.Start.IntentId);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,
            mode=="role"?["OtherRole"]:["ConfiguredRole"],mode=="permission"?[]:["Maintenance.ExecuteWork"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"foreign":start.Permit.Task.FieldId);
        var nav=new Nav();var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;var sends=0;CompanyWorkOrderLaborUsageRecovery? recorded=null;
        client.Setup(value=>value.ReviewWorkOrderLaborUsageAsync(start,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult(command.Review);});
        client.Setup(value=>value.RecordWorkOrderLaborUsageAsync(It.IsAny<CompanyWorkOrderLaborUsageCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderLaborUsageCommand original,CancellationToken _)=>
        {
            sends++;Assert.Contains("laborUsage="+original.IntentId.ToString("D"),nav.Uri);var utc=DateTime.UtcNow;
            recorded=new(original,original.Prepare(utc));
            if(mode=="unknown")throw new InvalidOperationException("Lost physical stock acknowledgement");if(mode=="late-company")company.Clear();return Task.FromResult(recorded);
        });
        client.Setup(value=>value.RecoverWorkOrderLaborUsageAsync(It.IsAny<CompanyWorkOrderLaborUsageReference>(),It.IsAny<CancellationToken>()))
            .Returns((CompanyWorkOrderLaborUsageReference reference,CancellationToken _)=>{recorded!.Require(context.Company,context.ActorUserId,reference);return Task.FromResult(recorded);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderLaborUsageState>();
        services.AddSingleton<NavigationManager>(nav);services.AddSingleton(fields.Object);services.AddSingleton(client.Object);
        services.AddSingleton<AuthenticationStateProvider>(new Auth(context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();
        services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();
        await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"Start",start}});
            var root=await renderer.RenderComponentAsync<WorkOrderLaborUsagePanel>(parameters);var panel=((Components)provider.GetRequiredService<IComponentActivator>()).LaborUsagePanel;
            await Invoke(panel,"ReviewAsync");if(mode is "role" or "permission" or "field"){Assert.Equal(0,sends);return;}Assert.Equal(1,reads);
            void Set(string key,object value)=>panel.GetType().GetField(key,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,value);
            Set("_quantity",mode=="quantity"?command.Quantity+1:command.Quantity);Set("_from",command.FromUtc.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",System.Globalization.CultureInfo.InvariantCulture));Set("_to",mode=="utc"?"2026-10-14T05:00:00":command.ToUtc.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",System.Globalization.CultureInfo.InvariantCulture));Set("_unit",mode=="unit"?WorkOrderLaborTimeUnit.Unspecified:WorkOrderLaborTimeUnit.Second);Set("_work","Actual performed labor");
            Set("_reason","Actual physical use");Set("_confirm",true);await Invoke(panel,"RecordAsync");await Task.Yield();
            if(mode is "quantity" or "utc" or "unit"){Assert.Equal(0,sends);return;}Assert.Equal(1,sends);
            if(mode=="late-company")
            {
                Assert.DoesNotContain(recorded!.Recorded.LABOR_ID,root.ToHtmlString());
                Assert.DoesNotContain(recorded.Command.IntentId.ToString("D"),root.ToHtmlString());
                Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["Maintenance.ExecuteWork","Inventory.View"])));
                root=await renderer.RenderComponentAsync<WorkOrderLaborUsagePanel>(parameters);panel=((Components)provider.GetRequiredService<IComponentActivator>()).LaborUsagePanel;
            }
            Assert.Contains(recorded!.Command.IntentId.ToString("D"),root.ToHtmlString());await Invoke(panel,"RecoverAsync");await Task.Yield();
            Assert.Contains(recorded.Recorded.LABOR_ID,root.ToHtmlString());Assert.Contains("Labor rates, currency and financial posting",root.ToHtmlString());
            await Invoke(panel,"RecordAsync");Assert.Equal(1,sends);
        });
    }
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("late-company")][InlineData("utc")][InlineData("quantity")][InlineData("role")][InlineData("inventory")][InlineData("field")]
    public async Task ActualPhysicalUsageFormRetainsOriginalReferenceAndRequiresExplicitQuantityUnitUtc(string mode)
    {
        var command=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Usage();var context=command.Review.Material.Source.Context;
        var start=new CompanyWorkOrderExecutionStartReference(command.Review.Start.Permit,command.Review.Start.IntentId);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,
            mode=="role"?["OtherRole"]:["ConfiguredRole"],mode=="inventory"?["Maintenance.ExecuteWork"]:["Maintenance.ExecuteWork","Inventory.View"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"foreign":start.Permit.Task.FieldId);
        var nav=new Nav();var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;var sends=0;CompanyWorkOrderMaterialUsageRecovery? recorded=null;
        client.Setup(value=>value.ReviewWorkOrderMaterialUsageAsync(start,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult(command.Review);});
        client.Setup(value=>value.RecordWorkOrderMaterialUsageAsync(It.IsAny<CompanyWorkOrderMaterialUsageCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialUsageCommand original,CancellationToken _)=>
        {
            sends++;Assert.Contains("materialUsage="+original.IntentId.ToString("D"),nav.Uri);var utc=DateTime.UtcNow;
            recorded=new(original,original.Prepare(utc),original.PrepareMovement(utc));
            if(mode=="unknown")throw new InvalidOperationException("Lost physical stock acknowledgement");if(mode=="late-company")company.Clear();return Task.FromResult(recorded);
        });
        client.Setup(value=>value.RecoverWorkOrderMaterialUsageAsync(It.IsAny<CompanyWorkOrderMaterialUsageReference>(),It.IsAny<CancellationToken>()))
            .Returns((CompanyWorkOrderMaterialUsageReference reference,CancellationToken _)=>{recorded!.Require(context.Company,context.ActorUserId,reference);return Task.FromResult(recorded);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderMaterialUsageState>();services.AddScoped<WorkOrderLaborUsageState>();
        services.AddSingleton<NavigationManager>(nav);services.AddSingleton(fields.Object);services.AddSingleton(client.Object);
        services.AddSingleton<AuthenticationStateProvider>(new Auth(context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();
        services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();
        await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"Start",start}});
            var root=await renderer.RenderComponentAsync<WorkOrderMaterialUsagePanel>(parameters);var panel=((Components)provider.GetRequiredService<IComponentActivator>()).MaterialUsagePanel;
            await Invoke(panel,"ReviewAsync");if(mode is "role" or "inventory" or "field"){Assert.Equal(0,sends);return;}Assert.Equal(1,reads);
            void Set(string key,object value)=>panel.GetType().GetField(key,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,value);
            Set("_quantity",mode=="quantity"?3m:1m);Set("_used",mode=="utc"?"2026-10-14T05:00:00":DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",System.Globalization.CultureInfo.InvariantCulture));
            Set("_reason","Actual physical use");Set("_confirm",true);await Invoke(panel,"RecordAsync");await Task.Yield();
            if(mode is "quantity" or "utc"){Assert.Equal(0,sends);return;}Assert.Equal(1,sends);
            if(mode=="late-company")
            {
                Assert.DoesNotContain(recorded!.Recorded.USAGE_ID,root.ToHtmlString());
                Assert.DoesNotContain(recorded.Command.IntentId.ToString("D"),root.ToHtmlString());
                Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["Maintenance.ExecuteWork","Inventory.View"])));
                root=await renderer.RenderComponentAsync<WorkOrderMaterialUsagePanel>(parameters);panel=((Components)provider.GetRequiredService<IComponentActivator>()).MaterialUsagePanel;
            }
            Assert.Contains(recorded!.Command.IntentId.ToString("D"),root.ToHtmlString());await Invoke(panel,"RecoverAsync");await Task.Yield();
            Assert.Contains(recorded.Recorded.USAGE_ID,root.ToHtmlString());Assert.Contains("Valuation, financial posting",root.ToHtmlString());
            await Invoke(panel,"RecordAsync");Assert.Equal(1,sends);
        });
    }
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("late-company")][InlineData("utc")][InlineData("defects")][InlineData("role")][InlineData("permission")][InlineData("field")]
    public async Task ActualSubmissionFormRetainsReportUrlAndOnlyRecoversOriginalUnknownOutcome(string mode)
    {
        var f=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission();var proof=f.Command.Review;var context=proof.Inspections.Current.Context;
        var start=new CompanyWorkOrderExecutionStartReference(proof.Inspections.Start.Permit,f.Start.IntentId);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,mode=="role"?["OtherRole"]:["ConfiguredRole"],mode=="permission"?[]:["Maintenance.ExecuteWork"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"foreign":start.Permit.Task.FieldId);var nav=new Nav();var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;var sends=0;CompanyWorkOrderSubmissionCommand? original=null;WORK_ORDER_EXECUTION_SUBMISSION? recorded=null;
        client.Setup(value=>value.ReviewWorkOrderSubmissionAsync(start,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult(proof);});
        client.Setup(value=>value.SubmitWorkOrderAsync(It.IsAny<CompanyWorkOrderSubmissionCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderSubmissionCommand command,CancellationToken _)=>
        {
            sends++;Assert.Contains("workSubmission="+command.IntentId.ToString("D"),nav.Uri);original=CompanyInventorySourceReview.Copy(command);recorded=command.Prepare(DateTime.UtcNow);
            if(mode=="unknown")throw new InvalidOperationException("Lost work-report acknowledgement");if(mode=="late-company")company.Clear();
            return Task.FromResult(new CompanyWorkOrderSubmissionReceipt(context.Company,context.ActorUserId,command.IntentId,recorded.SUBMISSION_ID,recorded.START_ID,recorded.COMPLETED_AT_UTC,recorded.RECORDED_REVISION));
        });
        client.Setup(value=>value.RecoverWorkOrderSubmissionAsync(It.IsAny<CompanyWorkOrderSubmissionReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderSubmissionReference reference,CancellationToken _)=>{Assert.Equal(original!.IntentId,reference.IntentId);return Task.FromResult(new CompanyWorkOrderSubmissionRecovery(original,recorded!));});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderSubmissionState>();services.AddScoped<WorkOrderMaterialUsageState>();services.AddScoped<WorkOrderLaborUsageState>();services.AddSingleton<NavigationManager>(nav);services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"Start",start}});var root=await renderer.RenderComponentAsync<WorkOrderSubmissionPanel>(parameters);var panel=((Components)provider.GetRequiredService<IComponentActivator>()).SubmissionPanel;
            await Invoke(panel,"ReviewAsync");if(mode is "role" or "permission" or "field"){Assert.Equal(0,reads);Assert.Equal(0,sends);return;}Assert.Equal(1,reads);
            void Set(string key,object value)=>panel.GetType().GetField(key,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,value);
            Set("_completed",mode=="utc"?"2026-10-14T05:00:00":f.Command.CompletedAtUtc.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",System.Globalization.CultureInfo.InvariantCulture));Set("_work","Actual work report");Set("_reason","Explicit work rationale");Set("_confirm",true);Set("_noDefects",mode!="defects");
            await Invoke(panel,"SubmitAsync");await Task.Yield();if(mode is "utc" or "defects"){Assert.Equal(0,sends);return;}Assert.Equal(1,sends);
            if(mode=="late-company"){Assert.DoesNotContain(recorded!.SUBMISSION_ID,root.ToHtmlString());Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["Maintenance.ExecuteWork"])));root=await renderer.RenderComponentAsync<WorkOrderSubmissionPanel>(parameters);panel=((Components)provider.GetRequiredService<IComponentActivator>()).SubmissionPanel;}
            Assert.Contains(original!.IntentId.ToString("D"),root.ToHtmlString());await Invoke(panel,"RecoverAsync");await Task.Yield();Assert.Contains(recorded!.SUBMISSION_ID,root.ToHtmlString());Assert.Contains("Independent verification, material use, costs and return acceptance remain separate",root.ToHtmlString());await Invoke(panel,"SubmitAsync");Assert.Equal(1,sends);
        });
    }
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("late-company")][InlineData("permission")][InlineData("inventory")][InlineData("role")][InlineData("field")][InlineData("submission-inspections")][InlineData("submission-inspections-late-company")]
    public async Task ActualStartPanelRetainsOriginalUrlBeforeTransportAndOnlyReadsOriginalOutcome(string mode)
    {
        var fixture=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Original(mode.StartsWith("submission-inspections",StringComparison.Ordinal));var review=fixture.Command.Review;var context=review.Prerequisites.Requirements.Context;var permit=review.Prerequisites.Requirements.Approval.Permit;
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,mode=="role"?["OtherRole"]:["ConfiguredRole"],mode=="permission"?[]:mode=="inventory"?["Maintenance.ExecuteWork"]:["Maintenance.ExecuteWork","Inventory.View"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"foreign":permit.Task.FieldId);
        var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;var starts=0;var recoveries=0;CompanyWorkOrderExecutionStartCommand? original=null;WORK_ORDER_EXECUTION_START? recorded=null;var navigation=new Nav();
        client.Setup(value=>value.ReviewWorkOrderExecutionStartAsync(permit,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult(review);});
        client.Setup(value=>value.StartWorkOrderExecutionAsync(It.IsAny<CompanyWorkOrderExecutionStartCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionStartCommand command,CancellationToken _)=>
        {
            starts++;Assert.Contains("workStart="+command.IntentId.ToString("D"),navigation.Uri);original=CompanyInventorySourceReview.Copy(command);recorded=command.Prepare(DateTime.UtcNow);
            if(mode=="unknown")throw new InvalidOperationException("Lost original start acknowledgement");if(mode=="late-company")company.Clear();
            return Task.FromResult(new CompanyWorkOrderExecutionStartReceipt(context.Company,context.ActorUserId,command.IntentId,recorded.START_ID,recorded.PERMIT_ID,recorded.STARTED_AT_UTC,recorded.RECORDED_REVISION));
        });
        client.Setup(value=>value.RecoverWorkOrderExecutionStartAsync(It.IsAny<CompanyWorkOrderExecutionStartReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionStartReference reference,CancellationToken _)=>{recoveries++;Assert.Equal(original!.IntentId,reference.IntentId);return Task.FromResult(new CompanyWorkOrderExecutionStartRecovery(original,recorded!));});
        var inspectionReads=0;
        client.Setup(value=>value.ReviewWorkOrderSubmissionInspectionsAsync(It.IsAny<CompanyWorkOrderExecutionStartReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionStartReference reference,CancellationToken _)=>
        {
            inspectionReads++;Assert.Equal(original!.IntentId,reference.IntentId);var result=CompanyWorkOrderSubmissionInspectionReview.Create(new(original,recorded!),original.Review.Prerequisites.Requirements,DateTime.UtcNow);
            if(mode=="submission-inspections-late-company")company.Clear();return Task.FromResult(result);
        });
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderExecutionStartState>();services.AddScoped<WorkOrderSubmissionState>();services.AddScoped<WorkOrderMaterialUsageState>();services.AddScoped<WorkOrderLaborUsageState>();services.AddSingleton<NavigationManager>(navigation);services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"Permit",permit}});
            var root=await renderer.RenderComponentAsync<WorkOrderExecutionStartPanel>(parameters);var panel=((Components)provider.GetRequiredService<IComponentActivator>()).StartPanel;
            await Invoke(panel,"ReviewAsync");await Task.Yield();
            if(mode is "permission" or "inventory" or "role" or "field"){Assert.Equal(0,reads);Assert.Equal(0,starts);return;}
            Assert.Equal(1,reads);Assert.Contains("Mapped account",root.ToHtmlString());
            panel.GetType().GetField("_reason",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,"Explicit actual work start");
            panel.GetType().GetField("_confirm",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,true);
            await Invoke(panel,"StartAsync");await Task.Yield();Assert.Equal(1,starts);Assert.NotNull(original);
            if(mode=="late-company")
            {
                Assert.DoesNotContain(recorded!.START_ID,root.ToHtmlString());Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["Maintenance.ExecuteWork","Inventory.View"])));
                root=await renderer.RenderComponentAsync<WorkOrderExecutionStartPanel>(parameters);panel=((Components)provider.GetRequiredService<IComponentActivator>()).StartPanel;
            }
            Assert.Contains(original!.IntentId.ToString("D"),root.ToHtmlString());
            await Invoke(panel,"RecoverAsync");await Task.Yield();Assert.Equal(1,recoveries);Assert.Equal(1,starts);Assert.Contains(recorded!.START_ID,root.ToHtmlString());Assert.Contains("no stock consumption",root.ToHtmlString());
            await Invoke(panel,"StartAsync");Assert.Equal(1,starts);
            if(mode.StartsWith("submission-inspections",StringComparison.Ordinal))
            {
                await Invoke(panel,"SubmissionInspectionsAsync");Assert.Equal(1,inspectionReads);Assert.Equal(1,starts);
                var html=root.ToHtmlString();if(mode=="submission-inspections"){Assert.Contains("Submission inspection comparison",html);Assert.Contains("Current work-submission inspection evidence is not accepted",html);Assert.Contains("WORK_SUBMISSION",html);company.Clear();await Task.Yield();}
                Assert.DoesNotContain("Submission inspection comparison",root.ToHtmlString());
            }
        });
    }
    [Theory][InlineData("current")][InlineData("permission")][InlineData("role")][InlineData("field")][InlineData("late-company")][InlineData("requirements")][InlineData("requirements-changed")][InlineData("requirements-late-company")][InlineData("prerequisites")][InlineData("prerequisites-permission")][InlineData("prerequisites-inventory")][InlineData("prerequisites-late-company")][InlineData("recorded-start")][InlineData("recorded-start-empty")][InlineData("recorded-start-late-company")]
    public async Task ExecutionPersonaReadsOnlyPublicRecordedApprovalAndClearsChangedScope(string mode)
    {
        var review=Beep.OilandGas.ApiService.Tests.WorkOrderCompleteApprovalFixture.Create(mode.StartsWith("requirements",StringComparison.Ordinal)).Review;var proposal=review.Proposal;var query=new CompanyWorkOrderExecutionPermitReference(proposal.Task,proposal.IntentId);
        var approval=new CompanyWorkOrderRecordedApproval(review.Context.Company,query,proposal.PermitId(),Guid.NewGuid(),proposal.Context.ActorUserId,review.Context.ActorUserId,DateTime.UtcNow,proposal.ValidFromUtc,proposal.ValidToUtc,proposal.PermitType,proposal.Jurisdiction,proposal.ScopeDescription,proposal.RequirementEvidenceVersion,review.PrerequisiteEvidenceVersion,review.OriginalSource.RequirementFactsRevision(),new string('F',64));
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"executor",new(review.Context.Company,mode=="role"?["OtherRole"]:["ConfiguredRole"],mode=="permission"?[]:mode=="prerequisites-permission"?["Maintenance.ViewApprovedPermit","Inventory.View"]:mode=="prerequisites-inventory"?["Maintenance.ExecuteWork"]:mode.StartsWith("prerequisites",StringComparison.Ordinal)?["Maintenance.ExecuteWork","Inventory.View"]:["Maintenance.ViewApprovedPermit"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"foreign":proposal.Task.FieldId);var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;
        client.Setup(value=>value.ReadRecordedWorkOrderPermitApprovalAsync(query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="late-company")company.Clear();return Task.FromResult(approval);});
        var rows=review.CurrentSource.Inspections.Select(row=>mode=="requirements-changed"?row with{RequiredText="Changed current requirement"}:row).ToArray();var originalPopulation=CompanyWorkOrderExecutionRequirements.Population(query.Task.FieldId,query.Task.WorkOrderId,review.OriginalSource.Inspections);var compared=DateTime.UtcNow;
        var requirements=new CompanyWorkOrderExecutionRequirements(review.Context with{ActorUserId="executor"},approval,review.CurrentSource.Order,rows,originalPopulation,CompanyWorkOrderExecutionRequirements.OrderRevision(review.OriginalSource.Order),compared,CompanyWorkOrderExecutionRequirements.GetBlockers(approval,originalPopulation,rows,CompanyWorkOrderExecutionRequirements.OrderRevision(review.OriginalSource.Order),review.CurrentSource.Order,compared));
        client.Setup(value=>value.ReadWorkOrderExecutionRequirementsAsync(query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="requirements-late-company")company.Clear();return Task.FromResult(requirements);});
        var prerequisites=WorkOrderExecutionPermitApprovalClientTests.ExecutionProof(review,approval);
        client.Setup(value=>value.ReadWorkOrderExecutionPrerequisitesAsync(query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="prerequisites-late-company")company.Clear();return Task.FromResult(prerequisites);});
        var startIntent=Guid.NewGuid();var start=new CompanyWorkOrderRecordedStart(review.Context.Company,query,startIntent,new CompanyWorkOrderExecutionStartReference(query,startIntent).StartId(review.Context.Company),proposal.PermitId(),"historical-executor","recorded-employee","ConfiguredRole","recorded-step",DateTime.UtcNow,DateTime.UtcNow,new string('A',64),new string('B',64));
        client.Setup(value=>value.ReadRecordedWorkOrderStartAsync(query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="recorded-start-late-company")company.Clear();return Task.FromResult(mode=="recorded-start-empty"?(CompanyWorkOrderRecordedStart?)null:start);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderExecutionStartState>();services.AddScoped<WorkOrderSubmissionState>();services.AddScoped<WorkOrderMaterialUsageState>();services.AddScoped<WorkOrderLaborUsageState>();services.AddSingleton<NavigationManager,Nav>();services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth("executor"));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderRecordedApprovalPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Task",proposal.Task},{"OriginalReference",proposal.IntentId}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).RecordedPanel;await Invoke(panel,mode.StartsWith("recorded-start",StringComparison.Ordinal)?"RecordedStartAsync":mode.StartsWith("prerequisites",StringComparison.Ordinal)?"PrerequisitesAsync":mode.StartsWith("requirements",StringComparison.Ordinal)?"RequirementsAsync":"ReviewAsync");await Task.Yield();
            Assert.Equal(mode is "permission" or "role" or "field" or "prerequisites-permission" or "prerequisites-inventory"?0:1,reads);var html=root.ToHtmlString();Assert.Contains("Recorded approval is historical evidence",html);
            if(mode is "current" or "requirements" or "requirements-changed" or "prerequisites"){if(mode.StartsWith("requirements",StringComparison.Ordinal)){Assert.Contains("Current requirements",html);Assert.Contains("Current native work-order requirements",html);Assert.Contains("Recorded evidence",html);Assert.Contains("No result recorded",html);Assert.Contains("Inspector: Not recorded",html);Assert.Contains("WORK_SUBMISSION",html);Assert.Contains("UNRECORDED",html);if(mode=="requirements-changed")Assert.Contains("Original approved native requirements or applicability changed",html);}Assert.Contains(approval.DecisionId.ToString(),html);Assert.Contains(approval.ScopeDescription,html);company.Clear();await Task.Yield();Assert.DoesNotContain(approval.DecisionId.ToString(),root.ToHtmlString());}
            else Assert.DoesNotContain(approval.DecisionId.ToString(),html);
            if(mode=="recorded-start"){Assert.Contains(start.StartId,html);Assert.Contains(start.ExecutorUserId,html);Assert.Contains(start.EmployeeBaId,html);company.Clear();await Task.Yield();Assert.DoesNotContain(start.StartId,root.ToHtmlString());}
            if(mode=="recorded-start-empty")Assert.Contains("Complete authorized lookup found no recorded start",html);
            if(mode=="recorded-start-late-company")Assert.DoesNotContain(start.StartId,html);
            if(mode=="prerequisites"){Assert.Contains("Execution prerequisites compared UTC",html);Assert.Contains(prerequisites.Personnel.AssignmentId,html);Assert.Contains(prerequisites.Material.ItemId,System.Net.WebUtility.HtmlDecode(html));Assert.Contains("these providers are not atomic together",html);Assert.Contains("This review records no start",html);}
            client.Verify(value=>value.DecideWorkOrderExecutionPermitAsync(It.IsAny<CompanyWorkOrderExecutionPermitDecision>(),It.IsAny<CancellationToken>()),Times.Never);
        });
    }
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("inventory")][InlineData("field")][InlineData("company")][InlineData("missing-proof")][InlineData("expired")][InlineData("role")][InlineData("phase-aware")]
    public async Task ActualCompleteApprovalActionRetainsOriginalBeforeTransportAndRefusesReplacement(string mode)
    {
        var review=Beep.OilandGas.ApiService.Tests.WorkOrderCompleteApprovalFixture.Create(mode=="phase-aware").Review;
        if(mode is "missing-proof" or "expired")
        {
            var observed=mode=="expired"?review.Proposal.ValidToUtc.AddSeconds(1):review.ComparedAtUtc;
            review=review with{ComparedAtUtc=observed,CurrentMaterialAcceptance=mode=="missing-proof"?null:review.CurrentMaterialAcceptance};
            review=review with{Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(review.Proposal,review.OriginalSource,review.CurrentSource,observed,6,review.CurrentMaterialAcceptance,review.CurrentPersonnelAcceptance,review.CurrentDocumentAcceptance)};
        }
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Context.ActorUserId,new(review.Context.Company,mode=="role"?["OtherRole"]:["ConfiguredRole"],mode=="inventory"?["HSE.ApprovePermit"]:["HSE.ApprovePermit","Inventory.View"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(review.Proposal.Task.FieldId);
        var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewWorkOrderExecutionPermitApprovalAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var nav=new Nav();CompanyWorkOrderExecutionPermitDecision? original=null;var writes=0;var recoveries=0;
        CompanyWorkOrderExecutionPermitDecisionReceipt Receipt(CompanyWorkOrderExecutionPermitDecision decision)=>new(review.Context.Company,review.Context.ActorUserId,decision.IntentId,review.Proposal.PermitId(),"Approve",new string('F',64));
        client.Setup(value=>value.DecideWorkOrderExecutionPermitAsync(It.IsAny<CompanyWorkOrderExecutionPermitDecision>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitDecision decision,CancellationToken _)=>
        {
            Assert.Equal("Approve",decision.Outcome);Assert.Contains("permitDecisionId="+decision.IntentId.ToString("D"),nav.Uri);Assert.Contains("permitReviewId="+review.Proposal.IntentId.ToString("D"),nav.Uri);original=decision;writes++;
            return mode=="unknown"?Task.FromException<CompanyWorkOrderExecutionPermitDecisionReceipt>(new HttpRequestException("Uncertain acknowledgement")):Task.FromResult(Receipt(decision));
        });
        client.Setup(value=>value.RecoverWorkOrderExecutionPermitDecisionReferenceAsync(It.IsAny<CompanyWorkOrderExecutionPermitDecisionReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitDecisionReference reference,CancellationToken _)=>
        {recoveries++;Assert.Equal(original!.IntentId,reference.IntentId);Assert.Equal(original.ProposalReference(),reference.Proposal);return Task.FromResult(new CompanyWorkOrderExecutionPermitDecisionRecovery(original,Receipt(original)));});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderExecutionStartState>();services.AddScoped<WorkOrderSubmissionState>();services.AddScoped<WorkOrderMaterialUsageState>();services.AddScoped<WorkOrderLaborUsageState>();services.AddScoped<WorkOrderExecutionPermitDecisionState>();services.AddSingleton<NavigationManager>(nav);services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(review.Context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderExecutionPermitApprovalReviewPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Task",review.Proposal.Task},{"SourceRevision","source"},{"OriginalReference",review.Proposal.IntentId}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            if(mode is "inventory" or "missing-proof" or "expired" or "role"){Assert.DoesNotContain("Retain original decision and approve permit",root.ToHtmlString());await Invoke(panel,"ApproveAsync");Assert.Equal(0,writes);return;}
            Assert.Contains("Retain original decision and approve permit",root.ToHtmlString());Assert.Contains("Independent decision rationale",root.ToHtmlString());
            panel.GetType().GetField("_rationale",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,"Original complete approval rationale");
            if(mode=="company")company.Clear();if(mode=="field"){fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("foreign");fields.Raise(value=>value.CurrentFieldChanged+=null!,"foreign");}
            await Invoke(panel,"ApproveAsync");if(mode is "company" or "field"){Assert.Equal(0,writes);Assert.DoesNotContain("Retain original decision and approve permit",root.ToHtmlString());return;}
            Assert.Equal(1,writes);Assert.Contains("Original complete approval rationale",root.ToHtmlString());
            if(mode=="unknown"){Assert.Contains("outcome is unconfirmed",root.ToHtmlString());await Invoke(panel,"ApproveAsync");Assert.Equal(1,writes);await Invoke(panel,"RecoverAsync");Assert.Equal(1,recoveries);}
            Assert.Contains("Recorded decision: Approve",root.ToHtmlString());Assert.Contains("Physical execution requires its separate current authorization",root.ToHtmlString());Assert.DoesNotContain("Retain original decision and approve permit",root.ToHtmlString());Assert.Equal(1,writes);
        });
    }
    [Theory][InlineData("current")][InlineData("company")][InlineData("field")][InlineData("role")]
    public async Task ActualApprovedReceiptShowsRecordedOutcomeAndClearsPrivateProofOnAuthorityChanges(string mode)
    {
        var complete=Beep.OilandGas.ApiService.Tests.WorkOrderCompleteApprovalFixture.Create();var review=complete.Review;var decision=new CompanyWorkOrderExecutionPermitDecision(review,Guid.NewGuid(),"Approve","Original independent complete approval rationale");
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Context.ActorUserId,new(review.Context.Company,mode=="role"?["OtherRole"]:["ConfiguredRole"],["HSE.ApprovePermit","Inventory.View"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(review.Proposal.Task.FieldId);var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;
        client.Setup(value=>value.RecoverWorkOrderExecutionPermitDecisionReferenceAsync(It.IsAny<CompanyWorkOrderExecutionPermitDecisionReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitDecisionReference reference,CancellationToken _)=>
        {
            reads++;Assert.Equal(decision.IntentId,reference.IntentId);return Task.FromResult(new CompanyWorkOrderExecutionPermitDecisionRecovery(decision,new(review.Context.Company,review.Context.ActorUserId,decision.IntentId,review.Proposal.PermitId(),"Approve",new string('F',64))));
        });
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderExecutionStartState>();services.AddScoped<WorkOrderSubmissionState>();services.AddScoped<WorkOrderMaterialUsageState>();services.AddScoped<WorkOrderLaborUsageState>();services.AddScoped<WorkOrderExecutionPermitDecisionState>();services.AddSingleton<NavigationManager,Nav>();services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(review.Context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderExecutionPermitApprovalReviewPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Task",review.Proposal.Task},{"SourceRevision","source"},{"OriginalReference",review.Proposal.IntentId},{"DecisionReference",decision.IntentId}}));
            if(mode=="role"){Assert.Equal(0,reads);Assert.DoesNotContain(decision.Rationale,root.ToHtmlString());return;}
            Assert.Equal(1,reads);Assert.Contains("Recorded decision: Approve",root.ToHtmlString());Assert.Contains(decision.Rationale,root.ToHtmlString());Assert.DoesNotContain("independent rejection is recorded",root.ToHtmlString());Assert.Contains("Physical execution requires its separate current authorization",root.ToHtmlString());
            if(mode=="company")company.Clear();if(mode=="field"){fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("foreign");fields.Raise(value=>value.CurrentFieldChanged+=null!,"foreign");}
            if(mode is "company" or "field"){await Task.Yield();Assert.DoesNotContain("Recorded decision: Approve",root.ToHtmlString());Assert.DoesNotContain(decision.Rationale,root.ToHtmlString());}
            client.Verify(value=>value.DecideWorkOrderExecutionPermitAsync(It.IsAny<CompanyWorkOrderExecutionPermitDecision>(),It.IsAny<CancellationToken>()),Times.Never);
        });
    }
    [Theory][InlineData("unchanged")][InlineData("changed")][InlineData("missing")][InlineData("phase-aware")]
    public async Task VersionedReviewerDistinguishesImmutableRequirementChangeFromAcceptedResults(string mode)
    {
        var review=mode=="phase-aware"?WorkOrderExecutionPermitStateTests.PhaseAwareReview():WorkOrderExecutionPermitStateTests.RequirementApprovalReview(mode=="changed");if(mode=="missing"){var current=review.CurrentSource with{Inspections=[],SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(review.CurrentSource.Stage,review.CurrentSource.InspectionDatabase,review.CurrentSource.Order,[])};review=review with{CurrentSource=current,PrerequisiteEvidenceVersion=1,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(review.Proposal,review.OriginalSource,current,review.ComparedAtUtc,1)};}using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(review.Context.Company,["ConfiguredRole"],["HSE.ApprovePermit"])));var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("field");var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewWorkOrderExecutionPermitApprovalAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderExecutionStartState>();services.AddScoped<WorkOrderSubmissionState>();services.AddScoped<WorkOrderMaterialUsageState>();services.AddScoped<WorkOrderLaborUsageState>();services.AddScoped<WorkOrderExecutionPermitDecisionState>();services.AddSingleton<NavigationManager,Nav>();services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth("independent"));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<WorkOrderExecutionPermitApprovalReviewPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Task",review.Proposal.Task},{"SourceRevision","source"},{"OriginalReference",review.Proposal.IntentId}}));var html=root.ToHtmlString();Assert.Contains("Original requirement comparison",html);Assert.Contains(mode=="missing"?"Current immutable requirement evidence is unavailable":mode=="changed"?"A new review is required":"Recorded results are reviewed separately",html);if(mode!="missing")Assert.Contains("material acceptance",html);if(mode=="phase-aware"){Assert.Contains("Phase-aware approval checks the explicitly classified pre-start conditions",html);Assert.Contains("Inspect actual work before submission",html);Assert.Contains("Responsible stage",html);Assert.Contains("WORK_SUBMISSION",html);Assert.Contains("UNRECORDED",html);Assert.DoesNotContain("Future operational or close-out conditions remain unaccepted",html);}Assert.DoesNotContain("Retain original decision and approve",html);company.Clear();await Task.Yield();Assert.DoesNotContain("Original requirement comparison",root.ToHtmlString());});
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task ReviewerDisplaysExactRecordedPrerequisiteInspectionAcceptanceWithInventoryBlocker(bool independent)
    {
        var original=WorkOrderExecutionPermitStateTests.ApprovalReview();var utc=original.ComparedAtUtc;var rows=new List<actualOwners::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition>();var seq=0;
        foreach(var pair in new[]{("ISOLATION",original.Proposal.IsolationReference),("RISK",original.Proposal.RiskReference),("PERSONNEL",original.Proposal.PersonnelReference),("MATERIAL",original.Proposal.MaterialReference)})
        {
            var requirement=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareRequirement(new(){FieldId="field",CondSeq=++seq,CondType="PRE_START",RequiredText="Explicit "+pair.Item1+" prerequisite",Jurisdiction=original.Proposal.Jurisdiction,RegulatoryRef=pair.Item2,PermitPrerequisiteKind=pair.Item1},original.Proposal.Task.WorkOrderId,"planner","inspection-"+seq,utc.AddMinutes(-2));
            var saved=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareResult(requirement,new(){FieldId="field",InspectionId=requirement.INSPECTION_ID,ExpectedSourceRevision=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.Revision(requirement),Result="PASS",Notes="Actual prerequisite inspection"},independent?"inspector":original.Proposal.Context.ActorUserId,utc.AddMinutes(-1));rows.Add(Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.Project(saved));
        }
        var source=original.OriginalSource with{Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(original.OriginalSource.Stage,original.OriginalSource.InspectionDatabase,original.OriginalSource.Order,rows)};var current=original.CurrentSource with{Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(original.CurrentSource.Stage,original.CurrentSource.InspectionDatabase,original.CurrentSource.Order,rows)};var proposal=original.Proposal with{InspectionRevision=source.SnapshotRevision};var review=original with{Proposal=proposal,OriginalSource=source,CurrentSource=current,PrerequisiteEvidenceVersion=1,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,source,current,utc,1)};
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(review.Context.Company,["ConfiguredRole"],["HSE.ApprovePermit"])));var fields=new Mock<IFieldSelectionContext>();fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("field");var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(c=>c.ReviewWorkOrderExecutionPermitApprovalAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderExecutionStartState>();services.AddScoped<WorkOrderSubmissionState>();services.AddScoped<WorkOrderMaterialUsageState>();services.AddScoped<WorkOrderLaborUsageState>();services.AddScoped<WorkOrderExecutionPermitDecisionState>();services.AddSingleton<NavigationManager,Nav>();services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth("independent"));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<WorkOrderExecutionPermitApprovalReviewPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Task",proposal.Task},{"SourceRevision","source"},{"OriginalReference",proposal.IntentId}}));var html=root.ToHtmlString();Assert.Contains("Recorded prerequisite inspection acceptance",html);Assert.Contains("inspection-1",html);Assert.Contains("ISOLATION",html);Assert.Contains("inventory quantity",html);Assert.Contains(independent?"Recorded independent inspection acceptance":"The permit issuer cannot supply independent acceptance",html);});
    }
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("fresh")][InlineData("company")][InlineData("field")]
    public async Task ActualReviewerRetainsDecisionInUrlBeforeTransportAndRestoresOnlyOriginalReadOnly(string fault)
    {
        var review=WorkOrderExecutionPermitStateTests.ApprovalReview();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(review.Context.Company,["ConfiguredRole"],["HSE.ApprovePermit"])));var fields=new Mock<IFieldSelectionContext>();fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("field");var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(c=>c.ReviewWorkOrderExecutionPermitApprovalAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var nav=new Nav();CompanyWorkOrderExecutionPermitDecision? original=fault=="fresh"?new(review,Guid.NewGuid(),"Reject","Original independent rationale"):null;var writes=0;var recoveries=0;
        CompanyWorkOrderExecutionPermitDecisionReceipt Receipt(CompanyWorkOrderExecutionPermitDecision decision)=>new(review.Context.Company,"independent",decision.IntentId,review.Proposal.PermitId(),"Reject",new string('F',64));
        client.Setup(c=>c.DecideWorkOrderExecutionPermitAsync(It.IsAny<CompanyWorkOrderExecutionPermitDecision>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitDecision decision,CancellationToken _)=>{Assert.Contains("permitDecisionId="+decision.IntentId.ToString("D"),nav.Uri);Assert.Contains("permitReviewId="+review.Proposal.IntentId.ToString("D"),nav.Uri);original=decision;writes++;return fault=="unknown"?Task.FromException<CompanyWorkOrderExecutionPermitDecisionReceipt>(new HttpRequestException("Unknown commit")):Task.FromResult(Receipt(decision));});
        client.Setup(c=>c.RecoverWorkOrderExecutionPermitDecisionReferenceAsync(It.IsAny<CompanyWorkOrderExecutionPermitDecisionReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitDecisionReference query,CancellationToken _)=>{recoveries++;Assert.Equal(original!.IntentId,query.IntentId);Assert.Equal(original.ProposalReference(),query.Proposal);return Task.FromResult(new CompanyWorkOrderExecutionPermitDecisionRecovery(original,Receipt(original)));});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderExecutionStartState>();services.AddScoped<WorkOrderSubmissionState>();services.AddScoped<WorkOrderMaterialUsageState>();services.AddScoped<WorkOrderLaborUsageState>();services.AddScoped<WorkOrderExecutionPermitDecisionState>();services.AddSingleton<NavigationManager>(nav);services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth("independent"));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderExecutionPermitApprovalReviewPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Task",review.Proposal.Task},{"SourceRevision","source"},{"OriginalReference",review.Proposal.IntentId},{"DecisionReference",original?.IntentId}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            if(fault=="fresh"){Assert.Equal(0,writes);Assert.Equal(1,recoveries);Assert.Contains("Original independent rationale",root.ToHtmlString());Assert.Contains("Recorded decision: Reject",root.ToHtmlString());return;}
            panel.GetType().GetField("_rationale",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,"Original independent rationale");await Invoke(panel,"RejectAsync");Assert.Equal(1,writes);
            if(fault=="unknown"){Assert.Contains("outcome is unconfirmed",root.ToHtmlString());await Invoke(panel,"RejectAsync");Assert.Equal(1,writes);await Invoke(panel,"RecoverAsync");Assert.Equal(1,recoveries);}
            Assert.Contains("Original independent rationale",root.ToHtmlString());Assert.Contains("Recorded decision: Reject",root.ToHtmlString());
            if(fault=="company")company.Clear();if(fault=="field"){fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("foreign");fields.Raise(f=>f.CurrentFieldChanged+=null!,"foreign");}
            if(fault is "company" or "field"){await Task.Yield();Assert.DoesNotContain("Original independent rationale",root.ToHtmlString());Assert.DoesNotContain("Recorded decision: Reject",root.ToHtmlString());}
            Assert.Equal(1,writes);
        });
    }
    [Theory][InlineData("original")][InlineData("actor")][InlineData("permission")][InlineData("foreign-field")][InlineData("late-field")][InlineData("company")][InlineData("blockers")]
    public async Task ActualIndependentReviewRendersOriginalAndCurrentFactsAndClearsLateScope(string fault)
    {
        var review=WorkOrderExecutionPermitStateTests.ApprovalReview();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(review.Context.Company,["ConfiguredRole"],fault=="permission"?[]:["HSE.ApprovePermit"])));var fields=new Mock<IFieldSelectionContext>();fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync(fault=="foreign-field"?"foreign":"field");var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;
        client.Setup(c=>c.ReviewWorkOrderExecutionPermitApprovalAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitReference query,CancellationToken _)=>{reads++;if(fault=="late-field"){fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("foreign");fields.Raise(f=>f.CurrentFieldChanged+=null!,"foreign");}return Task.FromResult(fault=="blockers"?review with{Blockers=[]}:review);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderExecutionStartState>();services.AddScoped<WorkOrderSubmissionState>();services.AddScoped<WorkOrderMaterialUsageState>();services.AddScoped<WorkOrderLaborUsageState>();services.AddScoped<WorkOrderExecutionPermitDecisionState>();services.AddSingleton<NavigationManager,Nav>();services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(fault=="actor"?"foreign":"independent"));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderExecutionPermitApprovalReviewPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Task",review.Proposal.Task},{"SourceRevision","source"},{"OriginalReference",review.Proposal.IntentId}}));var html=root.ToHtmlString();Assert.Equal(fault is "actor" or "permission" or "foreign-field"?0:1,reads);
            if(fault is "original" or "company"){Assert.Contains("Original issue-source facts",html);Assert.Contains("Current reviewer-source facts",html);Assert.Contains("Original scope",html);Assert.Contains("not yet recorded",html);Assert.Contains("Independent reviewer: independent",html);Assert.Contains("No recorded inspection requirements",html);}
            else Assert.DoesNotContain("Original scope",html);
            if(fault=="company"){company.Clear();await Task.Yield();Assert.DoesNotContain("Original scope",root.ToHtmlString());}
            Assert.Contains("The comparison shows recorded acceptance and current evidence",root.ToHtmlString());Assert.Contains("Starting physical work requires separate current execution authorization",root.ToHtmlString());client.Verify(c=>c.IssueWorkOrderExecutionPermitAsync(It.IsAny<CompanyWorkOrderExecutionPermitIntent>(),It.IsAny<CancellationToken>()),Times.Never);
        });
    }
    [Theory][InlineData("current")][InlineData("stock")][InlineData("released")]
    public async Task ActualPermitReviewerShowsRecordedAcceptanceAndLiveMaterialValiditySeparately(string mode)
    {
        var review=WorkOrderAcceptedMaterialReviewTests.Review(mode);using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(review.Context.Company,["ConfiguredRole"],["HSE.ApprovePermit","Inventory.View"])));var workflows=new Mock<ICompanyMaintenanceHandoffClient>();workflows.Setup(c=>c.ReviewWorkOrderExecutionPermitApprovalAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);var fields=new Mock<IFieldSelectionContext>();fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("field");var state=new WorkOrderExecutionPermitDecisionState(company);var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderExecutionStartState>();services.AddScoped<WorkOrderSubmissionState>();services.AddScoped<WorkOrderMaterialUsageState>();services.AddScoped<WorkOrderLaborUsageState>();services.AddSingleton(state);services.AddSingleton(workflows.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth("independent"));services.AddSingleton<NavigationManager,Nav>();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderExecutionPermitApprovalReviewPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Task",review.Proposal.Task},{"SourceRevision","source"},{"OriginalReference",review.Proposal.IntentId}}));var html=root.ToHtmlString();Assert.Contains("Recorded material acceptance and current evidence",html);Assert.Contains("Accepted by: independent",html);Assert.Contains("Original acceptance",html);Assert.Contains("Required: 2 EA",html);Assert.Contains("Warehouse: warehouse",html);Assert.Contains("personnel qualifications",html);Assert.DoesNotContain("Private original logistics rationale",html);
            if(mode=="current")Assert.Contains("matches this current inventory/reservation comparison",html);if(mode=="stock")Assert.Contains("changed since independent material acceptance",html);if(mode=="released")Assert.Contains("has been released",html);Assert.NotNull(state.Review);company.Clear();await Task.Yield();Assert.DoesNotContain("Recorded material acceptance and current evidence",root.ToHtmlString());
        });
    }
}
#endif
