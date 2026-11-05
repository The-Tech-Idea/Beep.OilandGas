#if WORKFLOW_INBOX_FOCUSED
extern alias actualData;
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using Detail=actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel;
using Inspection=actualData::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class MaintenanceInspectionClientTests
{
    private sealed class Response(CompanyMaintenanceInspectionReview review,Action? change):HttpMessageHandler
    {
        internal int Reads;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Reads++;Assert.Equal(HttpMethod.Post,request.Method);Assert.Equal("/api/field/current/maintenance-handoffs/assigned-stage/inspections",request.RequestUri!.AbsolutePath);
            Assert.Equal(review.Stage.Task,(await request.Content!.ReadFromJsonAsync<CompanyMaintenanceReceivingQuery>(token))!);change?.Invoke();return new(HttpStatusCode.OK){Content=JsonContent.Create(review)};
        }
    }
    [Theory][InlineData("recorded")][InlineData("empty")][InlineData("foreign-order")][InlineData("foreign-field")][InlineData("foreign-row")][InlineData("duplicate")][InlineData("missing-rationale")][InlineData("header-state")][InlineData("role")][InlineData("snapshot")][InlineData("database")][InlineData("time")][InlineData("late-company")][InlineData("company-return")]
    public async Task AssignedInspectionReadRetainsExactRecordedFactsAndRefusesUnconfirmedOrStaleEvidence(string fault)
    {
        using var company=new CompanyClientContext();var scope=new CompanyScope("tenant","company");Assert.True(company.TrySelect(company.Clear(),"supervisor",new(scope,["SupervisorRole"],[])));
        var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(scope,Guid.NewGuid()),"field","order/العالم%2F");
        var stage=new CompanyMaintenanceAssignedStageReview(new(scope,new("lifecycle","fingerprint"),"supervisor","revision","field"),task,"next-step","execute","Configured next duty","SupervisorRole","receiver","history","approval",new string('A',64),new string('B',64));
        var order=new Detail{InstanceId=task.WorkOrderId,FieldId=task.FieldId,Source="WORK_ORDER",SOURCE="Recorded PPDM audit source",State="",EvidenceUnavailableReason="No recorded order workflow state",Components=[new(task.WorkOrderId,"component",task.FieldId,null,null)]};
        var row=new Inspection(1,"PRE_START","Recorded isolation test","FAIL","Reported defect",DateTime.UtcNow,"Reported reference"){InspectionId="inspection",FieldId=task.FieldId,WorkOrderId=task.WorkOrderId,Jurisdiction="Explicit reported authority",RequirementActor="planner",Inspector="technician",SourceRevision=new string('C',64)};
        Inspection[] rows=fault=="empty"?[]:[row];var database=new AssetDatabaseScope("core","core-fingerprint");
        var review=new CompanyMaintenanceInspectionReview(stage,database,order,rows,CompanyMaintenanceInspectionReview.Revision(stage,database,order,rows),DateTimeOffset.UtcNow);
        if(fault=="foreign-order")order.InstanceId="foreign";if(fault=="foreign-field")order.FieldId="foreign";
        if(fault=="foreign-row")review=review with{Inspections=[row with{WorkOrderId="foreign"}]};if(fault=="duplicate")review=review with{Inspections=[row,row]};if(fault=="missing-rationale")review=review with{Inspections=[row with{ResultText=null}]};
        if(fault=="header-state")order.State="COMPLETED";if(fault=="role")review=review with{Stage=stage with{AssignedRole="ForeignRole"}};if(fault=="snapshot")review=review with{SnapshotRevision=new string('D',64)};if(fault=="database")review=review with{InspectionDatabase=database with{Fingerprint=""}};if(fault=="time")review=review with{ReviewedAtUtc=default};
        Action? change=fault is "late-company" or "company-return"?()=>{company.Clear();if(fault=="company-return")Assert.True(company.TrySelect(company.Version,"supervisor",new(scope,["SupervisorRole"],[])));}:null;
        var transport=new Response(review,change);using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        if(fault is "recorded" or "empty")
        {
            var result=await client.ReviewMaintenanceAssignedInspectionsAsync(task);result.Require(scope,"supervisor",task);Assert.Equal(review.SnapshotRevision,result.SnapshotRevision);Assert.NotSame(order,result.Order);
            Assert.Equal("WORK_ORDER",result.Order.Source);Assert.Equal("Recorded PPDM audit source",result.Order.SOURCE);
            if(fault=="empty")Assert.Empty(result.Inspections);else{Assert.Equal("FAIL",Assert.Single(result.Inspections).Status);Assert.Equal("Reported defect",result.Inspections[0].ResultText);}
        }
        else {var error=await Record.ExceptionAsync(()=>client.ReviewMaintenanceAssignedInspectionsAsync(task));Assert.True(error is InvalidOperationException or OperationCanceledException,error?.ToString()??"Unexpected inspection publication");}
        Assert.Equal(1,transport.Reads);
    }
}
#endif
