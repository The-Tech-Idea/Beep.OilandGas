using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class CompanyMaintenanceReturnPolicyClientTests
{
    internal static CompanyMaintenanceReturnPolicyDraft Draft(){var utc=DateTime.UtcNow;return new("CORRECTIVE","gas","Explicit jurisdiction / منطقة تشغيل","recorded-area","RecordedRegion",["FacilityRole"],[new("FACILITY","NativeCondition",["INACTIVE"],"RECORDED_TARGET","recorded-source","Reviewed transition source"),new("EQUIPMENT","NativeCondition",["IDLE"],"RECORDED_TARGET","recorded-source","Reviewed equipment source")],[new("issuer","RecordedType","REQUIRED","REQUIRED_PAID",[new("RecordedDecision",["RECORDED_APPROVAL"],"Reviewed decision source")],[new("Inspection","RecordedCheck","FULFILLED_ONLY","RecordedDue",null,null,new("EXACT",0,"value-unit","Reviewed zero term"),new("NOT_APPLICABLE",null,null,"Reviewed due term applicability"),"NO_RECORDED_RESTRICTION",null,null,null,null,"Reviewed condition source")],[new("RecordedViolation",["RecordedResolution"],"Reviewed resolution source")],"Reviewed licence applicability")],utc.AddDays(-1),utc.AddDays(30),"Reviewed company policy source / مرجع");}
    internal static CompanyMaintenanceReturnPolicyCatalog Catalog()
    {
        var context=new CompanyMaintenanceReturnPolicyContext(new("tenant","company"),new("lifecycle","core-fingerprint"),"administrator",new string('A',64));return CompanyMaintenanceReturnPolicyCatalog.Create(context,[],DateTime.UtcNow);
    }
    private sealed class Transport(CompanyMaintenanceReturnPolicyCommand command,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;if(mode=="catalog"){Assert.EndsWith("/maintenance-return-policies/review",request.RequestUri!.AbsolutePath);return new(HttpStatusCode.OK){Content=JsonContent.Create(Catalog())};}
            var row=command.Prepare(DateTime.UtcNow);if(mode=="forged")row.VERSION_NO=99;if(mode=="late")change();
            if(mode=="reference"){Assert.EndsWith("/recovery/reference",request.RequestUri!.AbsolutePath);var query=await request.Content!.ReadFromJsonAsync<CompanyMaintenanceReturnPolicyReference>(token);Assert.Equal(command.IntentId,query!.IntentId);return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyMaintenanceReturnPolicyRecovery(command,row))};}
            Assert.EndsWith(mode=="recovery"?"/recovery":"/record",request.RequestUri!.AbsolutePath);Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(await request.Content!.ReadFromJsonAsync<CompanyMaintenanceReturnPolicyCommand>(token)));return new(HttpStatusCode.OK){Content=JsonContent.Create(row)};
        }
    }
    [Theory][InlineData("catalog")][InlineData("record")][InlineData("recovery")][InlineData("reference")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("role")][InlineData("actor")]
    public async Task FullPolicyClientRetainsExactCompanyAdminSourceCommandAndRecordedAudit(string mode)
    {
        var catalog=Catalog();var command=new CompanyMaintenanceReturnPolicyCommand(catalog.Context,catalog.Revision,Draft(),1,Guid.NewGuid(),"Explicit policy rationale");using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":"administrator",new(catalog.Context.Company,mode=="role"?["Other"]:["Administrator"],mode=="permission"?[]:["Maintenance.ConfigureReturnPolicy"])));
        var transport=new Transport(command,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://policy.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{if(mode=="catalog"){var result=await client.ReviewCompanyMaintenanceReturnPoliciesAsync();result.Require();}else if(mode=="reference"){var reference=new CompanyMaintenanceReturnPolicyReference(command.RecordId(),command.IntentId);(await client.RecoverCompanyMaintenanceReturnPolicyReferenceAsync(reference)).Require(catalog.Context.Company,"administrator",reference);}else{var row=mode=="recovery"?await client.RecoverCompanyMaintenanceReturnPolicyAsync(command):await client.RecordCompanyMaintenanceReturnPolicyAsync(command);command.RequireRecorded(row);}});
        if(mode is "catalog" or "record" or "recovery" or "reference")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode is "permission" or "role" or "actor"?0:1,transport.Calls);
    }
    [Theory][InlineData("lost")][InlineData("retain")][InlineData("company")][InlineData("late-recovery")]
    public async Task PolicyStateFreezesOriginalBeforeTransportAndRequiresExactRecoveryBeforeNextVersion(string mode)
    {
        var catalog=Catalog();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"administrator",new(catalog.Context.Company,["Administrator"],["Maintenance.ConfigureReturnPolicy"])));using var state=new CompanyMaintenanceReturnPolicyState(company);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(c=>c.ReviewCompanyMaintenanceReturnPoliciesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(catalog);await state.ReviewAsync(client.Object);
        var writes=0;CompanyMaintenanceReturnPolicyCommand? sent=null;client.Setup(c=>c.RecordCompanyMaintenanceReturnPolicyAsync(It.IsAny<CompanyMaintenanceReturnPolicyCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceReturnPolicyCommand command,CancellationToken _)=>{writes++;sent=CompanyInventorySourceReview.Copy(command);Assert.Equal(command.IntentId,state.VisibleReference!.IntentId);if(mode=="company")company.Clear();throw new IOException("Unconfirmed policy append");});
        await Assert.ThrowsAnyAsync<Exception>(()=>state.AppendAsync(client.Object,Draft(),1,"Explicit policy rationale",reference=>{Assert.NotNull(state.VisibleCommand);Assert.Equal(reference,state.VisibleReference);if(mode=="retain")throw new IOException("Retention failed");return Task.CompletedTask;}));Assert.False(state.CanAppend);Assert.ThrowsAny<Exception>(()=>state.BeginNextReview());Assert.Equal(mode=="retain"?0:1,writes);
        if(mode=="company"){Assert.Null(state.VisibleCommand);Assert.True(company.TrySelect(company.Clear(),"administrator",new(catalog.Context.Company,["Administrator"],["Maintenance.ConfigureReturnPolicy"])));}
        var retained=state.VisibleCommand!;var reference=state.VisibleReference!;client.Setup(c=>c.RecoverCompanyMaintenanceReturnPolicyReferenceAsync(reference,It.IsAny<CancellationToken>())).Returns(()=>{if(sent is not null)Assert.Equal(JsonSerializer.Serialize(sent),JsonSerializer.Serialize(retained));if(mode=="late-recovery")company.Clear();return Task.FromResult(new CompanyMaintenanceReturnPolicyRecovery(retained,retained.Prepare(DateTime.UtcNow)));});
        await state.RecoverReferenceAsync(client.Object,reference);Assert.Equal(mode=="retain"?0:1,writes);if(mode=="late-recovery")Assert.Null(state.Receipt);else{retained.RequireRecorded(state.Receipt!);state.BeginNextReview();Assert.True(state.CanReview);Assert.Null(state.VisibleReference);}
    }
}
#endif
