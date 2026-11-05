#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderPersonnelSourceClientTests
{
    [Theory][InlineData("lost")][InlineData("retain")][InlineData("company")][InlineData("source")][InlineData("late-recovery")]
    public async Task PersonnelAssignmentStateRetainsOneOriginalIntentBeforeTransportAndNeverResubmitsOnRecovery(string mode)
    {
        var review=Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(review.Personnel.Context.Company,["ConfiguredRole"],["HSE.ApprovePermit","Maintenance.AssignPersonnel"])));
        using var state=new WorkOrderExecutionPermitDecisionState(company);state.Attach(review.Personnel.Query.Permit,"source");var client=new Mock<ICompanyMaintenanceHandoffClient>();var writes=0;CompanyWorkOrderPersonnelAssignmentCommand? original=null;
        client.Setup(c=>c.RecordWorkOrderPersonnelAssignmentAsync(It.IsAny<CompanyWorkOrderPersonnelAssignmentCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderPersonnelAssignmentCommand command,CancellationToken _)=>
        {
            writes++;original=CompanyInventorySourceReview.Copy(command);Assert.Equal(command.IntentId,state.VisiblePersonnelReference!.IntentId);Assert.NotNull(state.VisiblePersonnelCommand);
            if(mode=="company")company.Clear();if(mode=="source")state.Attach(review.Personnel.Query.Permit,"changed");throw new IOException("Confirmation unavailable");
        });
        await Assert.ThrowsAnyAsync<Exception>(()=>state.AssignPersonnelAsync(client.Object,review,"RecordedDuty",review.PermitReview.Proposal.ValidFromUtc,review.PermitReview.Proposal.ValidToUtc,"Explicit original assignment",reference=>
        {
            Assert.Equal(reference,state.VisiblePersonnelReference);Assert.NotNull(state.VisiblePersonnelCommand);if(mode=="retain")throw new IOException("Reference retention failed");return Task.CompletedTask;
        }));
        Assert.False(state.CanAssignPersonnel);Assert.Null(state.PersonnelReceipt);Assert.Equal(mode=="retain"?0:1,writes);
        var retained=state.VisiblePersonnelCommand;if(mode=="company"){Assert.Null(retained);Assert.True(company.TrySelect(company.Clear(),"independent",new(review.Personnel.Context.Company,["ConfiguredRole"],["HSE.ApprovePermit","Maintenance.AssignPersonnel"])));}
        state.Attach(review.Personnel.Query.Permit,"source");retained=state.VisiblePersonnelCommand!;Assert.NotNull(retained);
        var reference=new CompanyWorkOrderPersonnelAssignmentReference(review.Personnel.Query.Permit,retained.IntentId);var recoveryCalls=0;
        client.Setup(c=>c.RecoverWorkOrderPersonnelAssignmentReferenceAsync(reference,It.IsAny<CancellationToken>())).Returns(()=>
        {
            recoveryCalls++;if(original is not null)Assert.Equal(System.Text.Json.JsonSerializer.Serialize(original),System.Text.Json.JsonSerializer.Serialize(retained));
            var row=retained.Prepare(DateTime.UtcNow);if(mode=="late-recovery")company.Clear();return Task.FromResult(new CompanyWorkOrderPersonnelAssignmentRecovery(retained,row));
        });
        await state.RecoverPersonnelReferenceAsync(client.Object,reference);Assert.Equal(1,recoveryCalls);Assert.Equal(mode=="retain"?0:1,writes);
        if(mode=="late-recovery")Assert.Null(state.PersonnelReceipt);else retained.RequireRecorded(state.PersonnelReceipt!);
    }
    private sealed class ReferenceTransport(CompanyWorkOrderPersonnelAssignmentCommand command,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.EndsWith("/personnel-assignments/recovery/reference",request.RequestUri!.AbsolutePath);var reference=await request.Content!.ReadFromJsonAsync<CompanyWorkOrderPersonnelAssignmentReference>(token);Assert.Equal(command.IntentId,reference!.IntentId);
            var row=command.Prepare(DateTime.UtcNow);if(mode=="forged")row.ROW_CREATED_BY="other";if(mode=="late")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyWorkOrderPersonnelAssignmentRecovery(command,row))};
        }
    }
    [Theory][InlineData("original")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("actor")]
    public async Task PrivatePersonnelReferenceClientRequiresOriginalActorCompanyBodyAndAudit(string mode)
    {
        var review=Review();var command=new CompanyWorkOrderPersonnelAssignmentCommand(review,Guid.NewGuid(),"RecordedDuty",review.PermitReview.Proposal.ValidFromUtc,review.PermitReview.Proposal.ValidToUtc,"Original assignment");using var company=new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":"independent",new(review.Personnel.Context.Company,["ConfiguredRole"],mode=="permission"?["HSE.ApprovePermit"]:["HSE.ApprovePermit","Maintenance.AssignPersonnel"])));
        var transport=new ReferenceTransport(command,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://personnel.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var reference=new CompanyWorkOrderPersonnelAssignmentReference(review.Personnel.Query.Permit,command.IntentId);
        var error=await Record.ExceptionAsync(async()=>{var result=await client.RecoverWorkOrderPersonnelAssignmentReferenceAsync(reference);result.Require(review.Personnel.Context.Company,"independent",reference);});if(mode=="original")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,transport.Calls);
    }
    private sealed class AssignmentTransport(CompanyWorkOrderPersonnelAssignmentCommand command,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.EndsWith("/execution-permit/personnel-assignments"+(mode=="recovery"?"/recovery":""),request.RequestUri!.AbsolutePath);
            var body=await request.Content!.ReadFromJsonAsync<CompanyWorkOrderPersonnelAssignmentCommand>(token);Assert.Equal(System.Text.Json.JsonSerializer.Serialize(command),System.Text.Json.JsonSerializer.Serialize(body));
            var row=command.Prepare(DateTime.UtcNow);if(mode=="forged")row.EMPLOYEE_BA_ID="other";if(mode=="late")change();
            return new(HttpStatusCode.OK){Content=JsonContent.Create(row)};
        }
    }
    [Theory][InlineData("record")][InlineData("recovery")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("actor")][InlineData("role")]
    public async Task FullPersonnelAssignmentClientRetainsExactBodyAndFencesCurrentCompanyPermissionAndAudit(string mode)
    {
        var review=Review();var command=new CompanyWorkOrderPersonnelAssignmentCommand(review,Guid.NewGuid(),"RecordedDuty",review.PermitReview.Proposal.ValidFromUtc,review.PermitReview.Proposal.ValidToUtc,"Explicit original assignment");
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":"independent",new(review.Personnel.Context.Company,mode=="role"?["OtherRole"]:["ConfiguredRole"],mode=="permission"?["HSE.ApprovePermit"]:["HSE.ApprovePermit","Maintenance.AssignPersonnel"])));
        var transport=new AssignmentTransport(command,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://personnel.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{var row=mode=="recovery"?await client.RecoverWorkOrderPersonnelAssignmentAsync(command):await client.RecordWorkOrderPersonnelAssignmentAsync(command);command.RequireRecorded(row);});
        if(mode is "record" or "recovery")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode is "permission" or "actor" or "role"?0:1,transport.Calls);
    }
    internal static CompanyWorkOrderPersonnelSourceReview Review(string mode="current")
    {
        var f=new WorkOrderMaterialAcceptanceFixture();var permit=f.Command.Review.PermitReview;var query=new CompanyWorkOrderPersonnelSourceQuery(f.Command.Review.Query.Permit,"employee",permit.Proposal.PersonnelReference);var utc=DateTime.UtcNow;var from=utc.AddDays(-2);var to=utc.AddDays(2);if(mode=="timezone")from=DateTime.SpecifyKind(from,DateTimeKind.Unspecified);if(mode=="expired")to=utc.AddMinutes(-1);
        var employee=mode=="unlinked"?null:new CompanyEmployeeSourceFacts("company","employee",1,"Recorded technician",mode=="unknown"?null:"ACTIVE",from,to,new string('A',64));var licence=mode is "unlinked" or "missing-license"?null:new CompanyPersonnelLicenseFacts("employee",query.LicenseId,mode=="unknown"?null:"Recorded licence type","number","recorded-authority",utc.AddDays(-1),from,to,new string('B',64));var source=CompanyPersonnelSourceFacts.Create(permit.Context,query,employee,licence,utc);return new(permit,source,CompanyWorkOrderPersonnelSourceReview.GetBlockers(permit.Proposal,source));
    }
    [Theory][InlineData("current")][InlineData("unlinked")][InlineData("missing-license")][InlineData("timezone")][InlineData("expired")][InlineData("unknown")]
    public void MissingPersonnelFactsRemainExplicitAndNeverBecomeQualifiedDefaults(string mode)
    {
        var review=Review(mode);review.Require(review.PermitReview.Context.Company,"independent",review.Personnel.Query);Assert.Contains(review.Blockers,row=>row.Contains("competency requirements"));if(mode=="unlinked"){Assert.Null(review.Personnel.Employment);Assert.Null(review.Personnel.License);}if(mode=="timezone")Assert.Contains(review.Blockers,row=>row.Contains("time-zone"));if(mode=="expired")Assert.Contains(review.Blockers,row=>row.Contains("validity window"));if(mode=="unknown")Assert.Contains(review.Blockers,row=>row.Contains("unconfirmed"));
    }
    private sealed class Transport(CompanyWorkOrderPersonnelSourceReview review,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Assert.EndsWith("/execution-permit/personnel-source",request.RequestUri!.AbsolutePath);Assert.Equal(review.Personnel.Query,await request.Content!.ReadFromJsonAsync<CompanyWorkOrderPersonnelSourceQuery>(token));if(mode=="late")change();var result=mode=="forged"?review with{Blockers=[]}:review;return new(HttpStatusCode.OK){Content=JsonContent.Create(result)};}
    }
    [Theory][InlineData("original")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("actor")]
    public async Task FullPersonnelSourceClientRequiresExactOriginalAccountQueryEvidenceAndRole(string mode)
    {
        var review=Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":"independent",new(review.PermitReview.Context.Company,["ConfiguredRole"],mode=="permission"?[]:["HSE.ApprovePermit"])));var transport=new Transport(review,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://personnel.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var error=await Record.ExceptionAsync(async()=>{var result=await client.ReviewWorkOrderPersonnelSourceAsync(review.Personnel.Query);result.Require(review.PermitReview.Context.Company,"independent",review.Personnel.Query);});if(mode=="original")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,transport.Calls);
    }
}
#endif
