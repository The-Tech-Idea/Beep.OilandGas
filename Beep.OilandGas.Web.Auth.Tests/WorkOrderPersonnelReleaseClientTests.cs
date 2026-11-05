using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderPersonnelReleaseClientTests
{
    internal static CompanyWorkOrderPersonnelReleaseCommand Original()
    {
        var assignment=WorkOrderPersonnelReviewFixture.Original();return new(new(assignment,assignment.Prepare(DateTime.UtcNow)),Guid.NewGuid(),"Private original cancellation rationale");
    }
    [Fact] public void ReleasePublicFactsPreserveOriginalAndHistoricalRosterJsonAndAddOnlyCurrentCancellationEvidence()
    {
        var command=Original();var row=command.Prepare(DateTime.UtcNow);command.RequireRecorded(row);var original=command.Original.Recorded;var fact=CompanyWorkOrderPersonnelAssignmentFacts.From(original);var context=command.Original.Command.Review.Personnel.Context;var permit=command.Original.Command.Review.Personnel.Query.Permit;var utc=DateTime.UtcNow;
        var legacy=CompanyWorkOrderPersonnelAssignmentRoster.Create(context,permit,[fact],utc);Assert.DoesNotContain("Releases",JsonSerializer.Serialize(legacy));var oldDigest=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context=context,Permit=permit,Assignments=new[]{fact},ComparedAtUtc=utc}));Assert.Equal(oldDigest,legacy.Revision);
        var release=CompanyWorkOrderPersonnelReleaseFacts.From(row,original);var current=CompanyWorkOrderPersonnelAssignmentRoster.Create(context,permit,[fact],utc,[release]);current.Require(context.Company,context.ActorUserId,permit);Assert.NotEqual(legacy.Revision,current.Revision);Assert.Contains(CompanyWorkOrderAssignedPersonnelReview.GetBlockers(fact,command.Original.Command.Review,current.Releases),value=>value.Contains("assignment was cancelled"));
        Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(current));Assert.DoesNotContain(command.IntentId.ToString("D"),JsonSerializer.Serialize(current));Assert.Throws<InvalidOperationException>(()=>command.RequireRecorded(withClone()));
        WORK_ORDER_PERSONNEL_RELEASE withClone(){var copy=CompanyInventorySourceReview.Copy(row);copy.RELEASED_AT_UTC=copy.RELEASED_AT_UTC.AddSeconds(1);return copy;}
    }
    private sealed class Transport(CompanyWorkOrderPersonnelReleaseCommand command,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;var row=command.Prepare(DateTime.UtcNow);if(mode=="forged")row.ASSIGNMENT_ID="other";if(mode=="late")change();
            if(mode=="reference")
            {
                Assert.EndsWith("/personnel-releases/recovery/reference",request.RequestUri!.AbsolutePath);var reference=await request.Content!.ReadFromJsonAsync<CompanyWorkOrderPersonnelReleaseReference>(token);Assert.Equal(command.IntentId,reference!.IntentId);return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyWorkOrderPersonnelReleaseRecovery(command,row))};
            }
            Assert.EndsWith("/personnel-releases"+(mode=="recovery"?"/recovery":""),request.RequestUri!.AbsolutePath);Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderPersonnelReleaseCommand>(token)));return new(HttpStatusCode.OK){Content=JsonContent.Create(row)};
        }
    }
    [Theory][InlineData("record")][InlineData("recovery")][InlineData("reference")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("actor")]
    public async Task FullPersonnelReleaseClientKeepsOriginalCompanyMakerBodyAuditAndIndependentPermission(string mode)
    {
        var command=Original();var context=command.Original.Command.Review.Personnel.Context;using var company=new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":context.ActorUserId,new(context.Company,["ConfiguredRole"],mode=="permission"?["HSE.ApprovePermit"]:["HSE.ApprovePermit","Maintenance.ReleasePersonnel"])));
        var transport=new Transport(command,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://personnel.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>
        {
            if(mode=="reference"){var reference=new CompanyWorkOrderPersonnelReleaseReference(new(command.Original.Command.Review.Personnel.Query.Permit,command.Original.Command.IntentId),command.IntentId);var result=await client.RecoverWorkOrderPersonnelReleaseReferenceAsync(reference);result.Require(context.Company,context.ActorUserId,reference);}
            else{var row=mode=="recovery"?await client.RecoverWorkOrderPersonnelReleaseAsync(command):await client.RecordWorkOrderPersonnelReleaseAsync(command);command.RequireRecorded(row);}
        });
        if(mode is "record" or "recovery" or "reference")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode is "permission" or "actor"?0:1,transport.Calls);
    }
    [Theory][InlineData("lost")][InlineData("retain")][InlineData("company")][InlineData("late-recovery")]
    public async Task ReleaseStateRetainsBeforeTransportRecoversWithoutResubmissionAndRequiresConfirmationBeforeNewDuty(string mode)
    {
        var command=Original();var assignment=command.Original;var context=assignment.Command.Review.Personnel.Context;using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["HSE.ApprovePermit","Maintenance.ReleasePersonnel"])));
        using var state=new WorkOrderExecutionPermitDecisionState(company);state.Attach(assignment.Command.Review.Personnel.Query.Permit,"source");var client=new Mock<ICompanyMaintenanceHandoffClient>();var assignmentReference=new CompanyWorkOrderPersonnelAssignmentReference(assignment.Command.Review.Personnel.Query.Permit,assignment.Command.IntentId);
        client.Setup(c=>c.RecoverWorkOrderPersonnelAssignmentReferenceAsync(assignmentReference,It.IsAny<CancellationToken>())).ReturnsAsync(assignment);await state.RecoverPersonnelReferenceAsync(client.Object,assignmentReference);
        var writes=0;CompanyWorkOrderPersonnelReleaseCommand? original=null;
        client.Setup(c=>c.RecordWorkOrderPersonnelReleaseAsync(It.IsAny<CompanyWorkOrderPersonnelReleaseCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderPersonnelReleaseCommand body,CancellationToken _)=>{writes++;original=CompanyInventorySourceReview.Copy(body);Assert.Equal(body.IntentId,state.VisiblePersonnelReleaseReference!.IntentId);if(mode=="company")company.Clear();throw new IOException("Confirmation unavailable");});
        await Assert.ThrowsAnyAsync<Exception>(()=>state.ReleasePersonnelAsync(client.Object,"Explicit cancellation",reference=>{Assert.NotNull(state.VisiblePersonnelReleaseCommand);Assert.Equal(reference,state.VisiblePersonnelReleaseReference);if(mode=="retain")throw new IOException("Reference retention failed");return Task.CompletedTask;}));
        Assert.False(state.CanReleasePersonnel);Assert.Null(state.PersonnelReleaseReceipt);Assert.ThrowsAny<Exception>(()=>state.BeginNewPersonnelDuty());Assert.Equal(mode=="retain"?0:1,writes);
        if(mode=="company"){Assert.Null(state.VisiblePersonnelReleaseCommand);Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["HSE.ApprovePermit","Maintenance.ReleasePersonnel"])));}
        state.Attach(assignmentReference.Permit,"source");var retained=state.VisiblePersonnelReleaseCommand!;Assert.NotNull(retained);var reference=state.VisiblePersonnelReleaseReference!;
        client.Setup(c=>c.RecoverWorkOrderPersonnelReleaseReferenceAsync(reference,It.IsAny<CancellationToken>())).Returns(()=>{if(original is not null)Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(retained));if(mode=="late-recovery")company.Clear();return Task.FromResult(new CompanyWorkOrderPersonnelReleaseRecovery(retained,retained.Prepare(DateTime.UtcNow)));});
        await state.RecoverPersonnelReleaseReferenceAsync(client.Object,reference);Assert.Equal(mode=="retain"?0:1,writes);
        if(mode=="late-recovery"){Assert.Null(state.PersonnelReleaseReceipt);return;}
        retained.RequireRecorded(state.PersonnelReleaseReceipt!);state.BeginNewPersonnelDuty();Assert.True(state.CanAssignPersonnel);Assert.Null(state.VisiblePersonnelReference);Assert.Null(state.VisiblePersonnelReleaseReference);Assert.Equal(mode=="retain"?0:1,writes);
    }
}
#endif
