using Beep.OilandGas.Models.Data.AccessControl;
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
public sealed class CompanyPersonnelIdentityClientTests
{
    internal static CompanyPersonnelIdentityReview Review()
    {
        var company=new CompanyScope("tenant","company");var context=new CompanyPersonnelPolicyContext(company,new("core","binding"),"administrator",new string('A',64));var utc=DateTime.UtcNow;var catalog=CompanyPersonnelIdentityCatalog.Create(context,[],utc);var person=new CompanyNativePersonFacts(company,"person","Reported person name","Reported person type",utc.AddDays(-1),utc.AddDays(30),new("company","person",1,"Reported position","Reported status",utc.AddDays(-1),utc.AddDays(30),new string('C',64)),new string('D',64));return CompanyPersonnelIdentityReview.Create(context,new("reviewer","person"),new(company,"reviewer","Actual account",true,new string('B',64)),person,catalog,DateTime.UtcNow);
    }
    internal static CompanyWorkOrderDocumentContributorReview Contributors(string mode="current",CompanyWorkOrderPermitDocumentReview? documentReview=null)
    {
        var document=documentReview??WorkOrderPermitDocumentSourceClientTests.ContentReview();var context=document.PermitReview.Context;var utc=DateTime.UtcNow;var admin=new CompanyPersonnelPolicyContext(context.Company,context.Database,"identity-administrator",new string('A',64));var catalog=CompanyPersonnelIdentityCatalog.Create(admin,[],utc);var person=new CompanyNativePersonFacts(context.Company,"reviewer-person","Same reported name","Recorded person type",utc.AddDays(-1),utc.AddDays(2),new(context.Company.BusinessAssociateId,"reviewer-person",1,null,null,utc.AddDays(-1),utc.AddDays(2),new string('B',64)),new string('C',64));var query=new CompanyPersonnelIdentityQuery(context.ActorUserId,person.PersonBaId);var review=CompanyPersonnelIdentityReview.Create(admin,query,new(context.Company,query.UserId,"Actual account",true,new string('D',64)),person,catalog,utc);var command=new CompanyPersonnelIdentityCommand(review,1,Guid.NewGuid(),document.PermitReview.Proposal.ValidFromUtc.AddMinutes(-1),utc.AddDays(1),"Actual identity evidence","Private mapper rationale",true);var version=CompanyPersonnelIdentityVersion.From(command.Prepare(utc));var identityContext=admin with{ActorUserId=context.ActorUserId};var identities=CompanyPersonnelIdentityCatalog.Create(identityContext,mode=="missing-map"?[]:[version],utc);
        var contributor=new CompanyPermitDocumentContributorFacts("creator",mode=="self"?person.PersonBaId:"external-person",mode=="unknown-role"?null:"Reported author","Recorded person type","Same reported name",utc.AddDays(-1),utc.AddDays(2),mode=="group"?"Unmapped group type":"Recorded person type",utc.AddDays(-1),utc.AddDays(2),mode=="missing-person"?null:new string('E',64),new string('F',64));var people=CompanyWorkOrderDocumentContributors.Create(document.Source,mode=="empty"?[]:[contributor],DateTime.UtcNow);utc=DateTime.UtcNow;var result=new CompanyWorkOrderDocumentContributorReview(document,people,identities,mode=="missing-map"?null:person,utc,CompanyWorkOrderDocumentContributorReview.GetBlockers(document,people,identities,mode=="missing-map"?null:person,utc));result.Require();return result;
    }
    private sealed class ContributorTransport(CompanyWorkOrderDocumentContributorReview review,Action? change=null):HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Assert.EndsWith("/documents/contributors",request.RequestUri!.AbsolutePath);Assert.Equal(review.Document.Source.Query,await request.Content!.ReadFromJsonAsync<CompanyWorkOrderPermitDocumentQuery>(token));change?.Invoke();return new(HttpStatusCode.OK){Content=JsonContent.Create(review)};}
    }
    [Theory][InlineData("current")][InlineData("self")][InlineData("unknown-role")][InlineData("group")][InlineData("missing-person")][InlineData("missing-map")][InlineData("empty")][InlineData("late")]
    public async Task FullContributorClientComparesExactNativeIdentitiesWithoutInferringIndependenceFromEqualNames(string mode)
    {
        var review=Contributors(mode);if(mode=="current")Assert.Single(review.Blockers);else if(mode!="late")Assert.True(review.Blockers.Count>1);using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Document.PermitReview.Context.ActorUserId,new(review.Document.PermitReview.Context.Company,["ConfiguredRole"],["HSE.ApprovePermit"])));using var http=new HttpClient(new ContributorTransport(review,mode=="late"?()=>company.Clear():null)){BaseAddress=new("https://contributors.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var error=await Record.ExceptionAsync(async()=>{var result=await client.ReviewWorkOrderDocumentContributorsAsync(review.Document.Source.Query);result.Require();Assert.DoesNotContain("Private mapper rationale",JsonSerializer.Serialize(result));});if(mode=="late")Assert.NotNull(error);else Assert.Null(error);
    }
    private sealed class Transport(CompanyPersonnelIdentityReview review,string mode,Action change):HttpMessageHandler
    {
        public int Calls;public CompanyPersonnelIdentityCommand? Original;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;var path=request.RequestUri!.AbsolutePath;
            if(mode=="catalog"){Assert.Equal(HttpMethod.Get,request.Method);Assert.EndsWith("/personnel-identities",path);return new(HttpStatusCode.OK){Content=JsonContent.Create(review.Catalog)};}
            if(mode is "review" or "review-late" or "forged" or "permission" or "role" or "actor"){Assert.EndsWith("/personnel-identities/review",path);Assert.Equal(review.Query,await request.Content!.ReadFromJsonAsync<CompanyPersonnelIdentityQuery>(token));if(mode=="review-late")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="forged"?review with{Revision=new string('F',64)}:review)};}
            Assert.Contains("/personnel-identities",path);var original=Original!;if(mode=="reference"){var reference=await request.Content!.ReadFromJsonAsync<CompanyPersonnelIdentityReference>(token);Assert.Equal(original.IntentId,reference!.IntentId);return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyPersonnelIdentityRecovery(original,original.Prepare(DateTime.UtcNow)))};}
            var command=await request.Content!.ReadFromJsonAsync<CompanyPersonnelIdentityCommand>(token);Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(command));var row=command!.Prepare(DateTime.UtcNow);if(mode=="tampered")row.ROW_CREATED_BY="another";if(mode=="record-late")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(row)};
        }
    }
    [Theory][InlineData("catalog")][InlineData("review")][InlineData("review-late")][InlineData("forged")][InlineData("permission")][InlineData("role")][InlineData("actor")][InlineData("record")][InlineData("recovery")][InlineData("reference")][InlineData("tampered")][InlineData("record-late")]
    public async Task FullIdentityClientRequiresAdministratorExactCurrentCompanyAndOriginalNativeProof(string mode)
    {
        var review=Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":"administrator",new(review.Context.Company,mode=="role"?["Other"]:["Administrator"],mode=="permission"?[]:["HSE.ConfigurePersonnelIdentity"])));var transport=new Transport(review,mode,()=>company.Clear());transport.Original=new(review,1,Guid.NewGuid(),review.ComparedAtUtc,review.ComparedAtUtc.AddDays(10),"Actual identity evidence","Private original administrator rationale",true);using var http=new HttpClient(transport){BaseAddress=new("https://identities.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{if(mode=="catalog")(await client.ReviewCompanyPersonnelIdentitiesAsync()).Require();else if(mode is "review" or "review-late" or "forged" or "permission" or "role" or "actor")(await client.ReviewCompanyPersonnelIdentityAsync(review.Query)).Require();else if(mode=="reference"){var reference=new CompanyPersonnelIdentityReference(transport.Original.RecordId(),transport.Original.IntentId);(await client.RecoverCompanyPersonnelIdentityReferenceAsync(reference)).Require(review.Context.Company,review.Context.ActorUserId,reference);}else transport.Original.RequireRecorded(mode=="recovery"?await client.RecoverCompanyPersonnelIdentityAsync(transport.Original):await client.RecordCompanyPersonnelIdentityAsync(transport.Original));});
        if(mode is "catalog" or "review" or "record" or "recovery" or "reference")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode is "permission" or "role"?0:1,transport.Calls);
    }
    [Theory][InlineData("record")][InlineData("lost")][InlineData("retain")][InlineData("late")][InlineData("unverified")]
    public async Task IdentityStateRetainsOriginalBeforeTransportAndUncertaintyCannotBecomeReplacement(string mode)
    {
        var review=Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"administrator",new(review.Context.Company,["Administrator"],["HSE.ConfigurePersonnelIdentity"])));using var state=new CompanyPersonnelIdentityState(company);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewCompanyPersonnelIdentityAsync(review.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);var retained=false;var writes=0;CompanyPersonnelIdentityCommand? original=null;COMPANY_PERSONNEL_IDENTITY? saved=null;
        client.Setup(value=>value.RecordCompanyPersonnelIdentityAsync(It.IsAny<CompanyPersonnelIdentityCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyPersonnelIdentityCommand command,CancellationToken _)=>{Assert.True(retained);writes++;original=command;saved=command.Prepare(DateTime.UtcNow);if(mode=="late")company.Clear();if(mode=="lost")throw new HttpRequestException("Lost response");return Task.FromResult(saved);});await state.ReviewAsync(client.Object,review.Query);
        var error=await Record.ExceptionAsync(()=>state.RecordAsync(client.Object,review.ComparedAtUtc,review.ComparedAtUtc.AddDays(10),"Actual evidence","Original administrator rationale",mode!="unverified",reference=>{retained=true;if(mode=="retain")throw new IOException("Retaining reference failed");return Task.CompletedTask;}));if(mode=="record")Assert.Null(error);else if(mode!="late")Assert.NotNull(error);Assert.Equal(mode is "unverified" or "retain"?0:1,writes);
        if(mode=="unverified"){Assert.Null(state.VisibleReference);return;}await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordAsync(client.Object,review.ComparedAtUtc,review.ComparedAtUtc.AddDays(10),"replacement","replacement",true,_=>Task.CompletedTask));if(mode=="lost"){Assert.Throws<InvalidOperationException>(()=>state.BeginNextReview());var reference=state.VisibleReference!;client.Setup(value=>value.RecoverCompanyPersonnelIdentityReferenceAsync(reference,It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyPersonnelIdentityRecovery(original!,saved!));await state.RecoverAsync(client.Object,reference,_=>Task.CompletedTask);original!.RequireRecorded(state.Receipt!);Assert.Equal(1,writes);}if(mode=="late")Assert.Null(state.Receipt);if(mode is "record" or "lost"){state.BeginNextReview();Assert.True(state.CanReview);Assert.Null(state.VisibleReference);}
    }
}
#endif
