using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class MaintenanceAssignedStageClientTests
{
    private sealed class Response(CompanyMaintenanceAssignedStageReview review,Action? change) : HttpMessageHandler
    {
        internal int Reads;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Reads++;Assert.Equal(HttpMethod.Post,request.Method);Assert.Equal("/api/field/current/maintenance-handoffs/assigned-stage/review",request.RequestUri!.AbsolutePath);
            Assert.Equal(review.Task,(await request.Content!.ReadFromJsonAsync<CompanyMaintenanceReceivingQuery>(token))!);change?.Invoke();
            return new(HttpStatusCode.OK){Content=JsonContent.Create(review)};
        }
    }
    [Theory][InlineData("allowed")][InlineData("company")][InlineData("actor")][InlineData("field")][InlineData("role")][InlineData("revision")][InlineData("late-company")][InlineData("company-return")]
    public async Task TypedNextRoleReadRequiresOriginalCompanyActorTaskRoleAndCurrentResponseScope(string fault)
    {
        using var company=new CompanyClientContext();var scope=new CompanyScope("tenant","company");Assert.True(company.TrySelect(company.Clear(),"supervisor",new(scope,["SupervisorRole"],[])));
        var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(scope,Guid.NewGuid()),"field","order/العالم%2F");
        var review=new CompanyMaintenanceAssignedStageReview(new(scope,new("lifecycle","fingerprint"),"supervisor","revision","field"),task,"next-step","execute","Configured next duty","SupervisorRole","receiver","history","approval",new string('A',64),new string('B',64));
        if(fault=="company")review=review with{Context=review.Context with{Company=new("tenant","foreign")}};
        if(fault=="actor")review=review with{Context=review.Context with{ActorUserId="foreign"}};
        if(fault=="field")review=review with{Context=review.Context with{FieldId="foreign"}};
        if(fault=="role")review=review with{AssignedRole="ForeignRole"};if(fault=="revision")review=review with{ProcessRevision="invalid"};
        Action? change=fault is "late-company" or "company-return"?()=>{company.Clear();if(fault=="company-return")Assert.True(company.TrySelect(company.Version,"supervisor",new(scope,["SupervisorRole"],[])));}:null;
        var transport=new Response(review,change);using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        if(fault=="allowed"){var result=await client.ReviewMaintenanceAssignedStageAsync(task);Assert.Equal(review,result);Assert.NotSame(review,result);}
        else {var error=await Record.ExceptionAsync(()=>client.ReviewMaintenanceAssignedStageAsync(task));Assert.True(error is InvalidOperationException or OperationCanceledException,error?.ToString()??"Unexpected stage publication");}
        Assert.Equal(1,transport.Reads);
    }
}
#endif
