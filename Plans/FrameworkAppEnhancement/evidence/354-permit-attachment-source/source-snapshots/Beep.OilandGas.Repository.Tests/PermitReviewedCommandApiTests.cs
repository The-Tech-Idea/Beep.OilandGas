#if RECORDED_APPROVAL_HTTP_FOCUSED
extern alias PermitSdk;
using Beep.OilandGas.Client.App;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.ApiService.Controllers.Permits;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;
public sealed class PermitReviewedCommandApiTests
{
    private const string Id="permit/شركة?first#2";
    private static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.Web);
    private static PermitApplicationDataScope Scope()=>new(new(new("tenant","company"),new("core","core-binding"),"actor","selected-field-v1","A"),new("permits-current","permit-binding"));
    private static PermitApplicationSourceReview Review(PermitApplicationDataScope scope,string id=Id)=>PermitApplicationSourceReview.Capture(scope,
        new PERMIT_APPLICATION{PERMIT_APPLICATION_ID=id,OWNER_TENANT_ID=scope.Field.Company.TenantId,OWNER_COMPANY_ID=scope.Field.Company.BusinessAssociateId,FIELD_ID=scope.Field.FieldId!,APPLICANT_ID="Recorded applicant",COUNTRY=Country.Kuwait},[]);
    private static PermitApplicationCommand Command()=>new(Guid.NewGuid(),Review(Scope()),"SUBMIT","Reviewed recorded source evidence");
    private static PermitsController Controller(Mock<IPermitApplicationLifecycleService> lifecycle,Func<PermitApplicationDataScope> scope,Func<string> field)
    {
        var fields=new Mock<IFieldOrchestrator>();fields.SetupGet(x=>x.CurrentFieldId).Returns(field);
        var access=new Mock<ICompanyPermitApplicationDataAccess>();access.Setup(x=>x.RequireReadScopeAsync()).ReturnsAsync(scope);
        return new(lifecycle.Object,Mock.Of<IPermitApplicationWorkflowService>(),Mock.Of<IPermitComplianceCheckService>(),Mock.Of<IPermitStatusHistoryService>(),fields.Object,access.Object)
        {ControllerContext=new(){HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id","actor")],"test"))}}};
    }
    [Theory][InlineData("valid")][InlineData("application")][InlineData("actor")][InlineData("company")][InlineData("module")][InlineData("late-field")][InlineData("denied")][InlineData("missing")]
    public async Task PermitReviewedSourceApiRefusesUnboundOrLateReview(string mode)
    {
        var context=Scope();var field="A";var returned=context;
        if(mode=="actor")returned=returned with{Field=returned.Field with{ActorUserId="another-account"}};
        if(mode=="company")returned=returned with{Field=returned.Field with{Company=new("tenant","another-company")}};
        if(mode=="module")returned=returned with{Database=new("other-permits","other-binding")};
        var source=Review(returned,mode=="application"?"another-application":Id);var lifecycle=new Mock<IPermitApplicationLifecycleService>();
        lifecycle.Setup(x=>x.ReviewSourceAsync(Id)).ReturnsAsync(()=>{if(mode=="late-field")field="B";return source;});
        if(mode=="missing")lifecycle.Setup(x=>x.ReviewSourceAsync(Id)).ThrowsAsync(new Beep.OilandGas.PermitsAndApplications.Exceptions.PermitNotFoundException("Unavailable",Id));
        var controller=Controller(lifecycle,()=>mode=="denied"?throw new UnauthorizedAccessException("View permission revoked"):context,()=>field);
        if(mode=="valid")Assert.Equal(source,Assert.IsType<OkObjectResult>((await controller.ReviewSourceAsync(Id)).Result).Value);
        else if(mode=="missing")
        {
            var refusal=await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>controller.ReviewSourceAsync(Id));
            Assert.Equal(Beep.OilandGas.Models.Core.Refusals.RefusalKind.NotFound,refusal.Kind);
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>controller.ReviewSourceAsync(Id));
        lifecycle.Verify(x=>x.ReviewSourceAsync(Id),mode=="denied"?Times.Never():Times.Once());
    }
    [Theory][InlineData("record","valid")][InlineData("record","missing")][InlineData("record","different")][InlineData("record","late-field")][InlineData("record","wrong-context")][InlineData("record","wrong-actor")][InlineData("record","denied")]
    [InlineData("recovery","valid")][InlineData("recovery","missing")][InlineData("recovery","different")][InlineData("recovery","late-field")][InlineData("recovery","wrong-context")][InlineData("recovery","wrong-actor")][InlineData("recovery","denied")]
    public async Task PermitReviewedCommandApiPreservesExactOriginalAndReadOnlyRecovery(string operation,string mode)
    {
        var original=Command();var context=Scope();var field="A";
        if(mode=="wrong-context")context=context with{Field=context.Field with{Company=new("tenant","another-company")}};
        if(mode=="wrong-actor")original=original with{Review=Review(original.Review.Context with{Field=original.Review.Context.Field with{ActorUserId="another-account"}})};
        var outcome=PermitApplicationCommandOutcome.Prepare(mode=="different"?original with{IntentId=Guid.NewGuid()}:original,DateTimeOffset.UtcNow);
        var lifecycle=new Mock<IPermitApplicationLifecycleService>();
        lifecycle.Setup(x=>x.RecordCommandAsync(original)).ReturnsAsync(()=>{if(mode=="late-field")field="B";return mode=="missing"?null!:outcome;});
        lifecycle.Setup(x=>x.RecoverCommandAsync(original)).ReturnsAsync(()=>{if(mode=="late-field")field="B";return mode=="missing"?null:outcome;});
        var controller=Controller(lifecycle,()=>mode=="denied"?throw new UnauthorizedAccessException("View permission revoked"):context,()=>field);
        if(mode=="valid" || operation=="recovery" && mode=="missing")
        {
            if(operation=="record")Assert.Equal(outcome,Assert.IsType<OkObjectResult>((await controller.RecordCommandAsync(original)).Result).Value);
            else
            {
                var recovery=Assert.IsType<PermitApplicationCommandRecovery>(Assert.IsType<OkObjectResult>((await controller.RecoverCommandAsync(original)).Result).Value);
                recovery.Require(original);Assert.Equal(mode=="missing"?null:outcome,recovery.Outcome);
            }
        }
        else if(operation=="record")await Assert.ThrowsAnyAsync<Exception>(()=>controller.RecordCommandAsync(original));
        else await Assert.ThrowsAnyAsync<Exception>(()=>controller.RecoverCommandAsync(original));
        if(mode is "wrong-context" or "wrong-actor" or "denied")lifecycle.VerifyNoOtherCalls();
        else
        {
            lifecycle.Verify(x=>x.RecordCommandAsync(original),operation=="record"?Times.Once():Times.Never());
            lifecycle.Verify(x=>x.RecoverCommandAsync(original),operation=="recovery"?Times.Once():Times.Never());
        }
    }
    private sealed class Transport(Func<HttpRequestMessage,HttpResponseMessage> answer):HttpMessageHandler
    {
        public string? Uri;public HttpMethod? Method;public string? Body;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {Uri=request.RequestUri!.PathAndQuery;Method=request.Method;Body=request.Content is null?null:await request.Content.ReadAsStringAsync(token);return answer(request);}
    }
    [Theory][InlineData("source-valid")][InlineData("source-company")][InlineData("source-actor")][InlineData("source-field")][InlineData("source-application")][InlineData("source-missing")]
    [InlineData("record-valid")][InlineData("record-original")][InlineData("record-missing")]
    [InlineData("recovery-valid")][InlineData("recovery-absent")][InlineData("recovery-original")][InlineData("recovery-missing")]
    public async Task PermitReviewedTypedClientBindsSelectionOriginalAndExplicitRecovery(string mode)
    {
        var original=Command();var source=original.Review;var context=source.Context;
        if(mode=="source-company")context=context with{Field=context.Field with{Company=new("tenant","another-company")}};
        if(mode=="source-actor")context=context with{Field=context.Field with{ActorUserId="another-account"}};
        if(mode=="source-field")context=context with{Field=context.Field with{FieldId="B"}};
        source=Review(context,mode=="source-application"?"another-application":Id);
        var responseOriginal=mode.EndsWith("-original",StringComparison.Ordinal)?original with{IntentId=Guid.NewGuid()}:original;
        var outcome=PermitApplicationCommandOutcome.Prepare(responseOriginal,DateTimeOffset.UtcNow);
        object result=mode.StartsWith("source-",StringComparison.Ordinal)?source:mode.StartsWith("record-",StringComparison.Ordinal)?outcome:
            new PermitApplicationCommandRecovery(responseOriginal,mode=="recovery-absent"?null:outcome);
        using var transport=new Transport(_=>mode.EndsWith("-missing",StringComparison.Ordinal)?new(HttpStatusCode.NoContent):new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(result,result.GetType(),Json),Encoding.UTF8,"application/json")});
        using var http=new HttpClient(transport){BaseAddress=new("https://oilgas.test")};using var company=new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.ViewPermits"])));
        var client=new PermitServiceClient(new ApiClient(http,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),company));
        async Task Run()
        {
            if(mode.StartsWith("source-",StringComparison.Ordinal))Assert.Equal(original.Review,await client.ReviewSourceAsync(Id,original.Review.Context.Field));
            else if(mode.StartsWith("record-",StringComparison.Ordinal))Assert.Equal(outcome,await client.RecordCommandAsync(original));
            else
            {
                var recovery=await client.RecoverCommandAsync(original);recovery.Require(original);
                Assert.Equal(mode=="recovery-absent"?null:outcome,recovery.Outcome);
            }
        }
        if(mode.EndsWith("-valid",StringComparison.Ordinal) || mode=="recovery-absent")await Run();else await Assert.ThrowsAnyAsync<Exception>(Run);
        if(mode.StartsWith("source-",StringComparison.Ordinal))
        {
            Assert.Equal(HttpMethod.Get,transport.Method);Assert.Equal("/api/field/current/permits/application-review?applicationId="+System.Uri.EscapeDataString(Id),transport.Uri);
        }
        else
        {
            Assert.Equal(HttpMethod.Post,transport.Method);Assert.Equal("/api/field/current/permits/application-command"+(mode.StartsWith("recovery-",StringComparison.Ordinal)?"/recovery":""),transport.Uri);
            Assert.Equal(original,JsonSerializer.Deserialize<PermitApplicationCommand>(transport.Body!,Json));
        }
    }
    [Theory][InlineData("valid")][InlineData("field")][InlineData("company")][InlineData("tenant")][InlineData("actor")]
    public async Task PermitSourceClientRequiresSelectedCompanyAccountAndFieldBeforeTransport(string mode)
    {
        var original=Command();var expected=original.Review.Context.Field;
        if(mode=="field")expected=expected with{FieldId=null};
        if(mode=="company")expected=expected with{Company=null!};
        if(mode=="tenant")expected=expected with{Company=expected.Company with{TenantId=""}};
        if(mode=="actor")expected=expected with{ActorUserId=""};
        using var transport=new Transport(_=>new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(original.Review,Json),Encoding.UTF8,"application/json")});
        using var http=new HttpClient(transport){BaseAddress=new("https://oilgas.test")};using var company=new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.ViewPermits"])));
        var client=new PermitServiceClient(new ApiClient(http,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),company));
        if(mode=="valid"){Assert.Equal(original.Review,await client.ReviewSourceAsync(Id,expected));Assert.NotNull(transport.Uri);}
        else {await Assert.ThrowsAsync<InvalidOperationException>(()=>client.ReviewSourceAsync(Id,expected));Assert.Null(transport.Uri);}
    }
    [Theory][InlineData("record","intent")][InlineData("record","reason")][InlineData("record","json")][InlineData("recovery","intent")][InlineData("recovery","reason")][InlineData("recovery","json")]
    public async Task PermitInvalidOriginalUsesCanonicalBadRequestBeforeNativeCall(string operation,string mode)
    {
        var original=Command();
        if(mode=="intent")original=original with{IntentId=Guid.Empty};
        if(mode=="reason")original=original with{Reason=" "};
        if(mode=="json")original=original with{Review=original.Review with{ApplicationJson="{"}};
        var lifecycle=new Mock<IPermitApplicationLifecycleService>();var controller=Controller(lifecycle,Scope,()=>"A");
        Beep.OilandGas.Models.Core.Refusals.RefusalException refusal;
        if(operation=="record")refusal=await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>controller.RecordCommandAsync(original));
        else refusal=await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>controller.RecoverCommandAsync(original));
        Assert.Equal(Beep.OilandGas.Models.Core.Refusals.RefusalKind.Invalid,refusal.Kind);lifecycle.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task PermitReviewedRequestContextUsesCanonicalForbiddenRefusal()
    {
        var current=Scope();var original=Command() with{Review=Review(current with{Field=current.Field with{Company=new("tenant","another-company")}})};
        var lifecycle=new Mock<IPermitApplicationLifecycleService>();var controller=Controller(lifecycle,()=>current,()=>"A");
        var refusal=await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>controller.RecordCommandAsync(original));
        Assert.Equal(Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden,refusal.Kind);lifecycle.VerifyNoOtherCalls();
    }
    [Theory][InlineData("source")][InlineData("record")][InlineData("recovery")][InlineData("absent")][InlineData("missing")][InlineData("local")]
    public async Task PermitUnifiedSdkUsesReviewedApiAndRetiresObsoleteRoutes(string mode)
    {
        var original=Command();var outcome=PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);
        object response=mode=="source"?original.Review:mode=="record"?outcome:new PermitApplicationCommandRecovery(original,mode=="absent"?null:outcome);
        using var transport=new Transport(_=>mode=="missing"?new(HttpStatusCode.NoContent):new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(response,response.GetType(),Json),Encoding.UTF8,"application/json")});
        using var http=new HttpClient(transport);using var company=new PermitSdk::Beep.OilandGas.Client.App.CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.ViewPermits"])));
        if(mode=="local")
        {
            using var services=new ServiceCollection().BuildServiceProvider();var editor=new Mock<TheTechIdea.Beep.Editor.IDMEEditor>();
            var local=new PermitSdk::Beep.OilandGas.Client.App.BeepOilandGasApp(services,editor.Object,new PermitSdk::Beep.OilandGas.Client.App.AppOptions());
            await Assert.ThrowsAsync<InvalidOperationException>(()=>local.Permits.RecordCommandAsync(original));
            editor.Verify(x=>x.GetDataSource(It.IsAny<string>()),Times.Never());Assert.Null(transport.Uri);return;
        }
        var app=new PermitSdk::Beep.OilandGas.Client.App.BeepOilandGasApp(http,new PermitSdk::Beep.OilandGas.Client.App.AppOptions{ApiBaseUrl="https://oilgas.test",MaxRetries=0},company);
        var client=app.Permits;
        if(mode=="source")Assert.Equal(original.Review,await client.ReviewSourceAsync(Id,original.Review.Context.Field));
        else if(mode=="record")Assert.Equal(outcome,await client.RecordCommandAsync(original));
        else if(mode=="missing")await Assert.ThrowsAsync<InvalidOperationException>(()=>client.RecoverCommandAsync(original));
        else {var recovery=await client.RecoverCommandAsync(original);recovery.Require(original);Assert.Equal(mode=="absent"?null:outcome,recovery.Outcome);}
        var path=mode=="source"?"/api/field/current/permits/application-review?applicationId="+System.Uri.EscapeDataString(Id):"/api/field/current/permits/application-command"+(mode=="record"?"":"/recovery");
        Assert.Equal(path,transport.Uri);
        if(mode!="source")Assert.Equal(original,JsonSerializer.Deserialize<PermitApplicationCommand>(transport.Body!,Json));
        Assert.Null(typeof(PermitSdk::Beep.OilandGas.Client.App.Services.Permits.IPermitsService).GetMethod("SubmitPermitAsync"));
        Assert.Equal(5,typeof(PermitSdk::Beep.OilandGas.Client.App.Services.Permits.IPermitsService).GetMethods().Length);
    }
    [Theory][InlineData("api-record")][InlineData("api-recovery")][InlineData("web-record")][InlineData("web-recovery")][InlineData("sdk-record")][InlineData("sdk-recovery")]
    public async Task PermitCreationOriginalUsesExistingApiWebAndSdkCommandRecoveryWire(string mode)
    {
        var creation=PermitApplicationCreationReview.Capture(Scope(),CreationProposal(),"field-source","well-source","facility-source");
        await VerifyCommandWireAsync(creation.PrepareOriginal(Guid.NewGuid(),"Reviewed the exact company draft and linked resources."),mode);
    }
    [Theory][InlineData("api-record")][InlineData("api-recovery")][InlineData("web-record")][InlineData("web-recovery")][InlineData("sdk-record")][InlineData("sdk-recovery")]
    public async Task PermitDrillingOriginalUsesExistingApiWebAndSdkCommandRecoveryWire(string mode)
    {
        var parent=Review(Scope()).ReadApplication();parent.WELL_ID="well-a";
        var child=new DRILLING_PERMIT_APPLICATION { PERMIT_APPLICATION_ID=Id,ACTIVE_IND="Y",PROPOSED_DEPTH=100,PROPOSED_DEPTH_UNIT="ft" };
        var review=PermitApplicationSourceReview.Capture(Scope(),parent,[],PermitApplicationSupportingSnapshot.Capture(parent,child,null,null));
        var original=new PermitApplicationCommand(Guid.NewGuid(),review,"AMEND_DRILLING","Reviewed the recorded drilling source and explicit unit")
            { DrillingChanges=PermitDrillingDraftChanges.From(child) with { ProposedDepth=1234.5m,DepthUnit="m",Remarks="Reviewed notes" } };
        await VerifyCommandWireAsync(original,mode);
    }
    [Theory]
    [InlineData("environmental","api-record")]
    [InlineData("environmental","api-recovery")]
    [InlineData("environmental","web-record")]
    [InlineData("environmental","web-recovery")]
    [InlineData("environmental","sdk-record")]
    [InlineData("environmental","sdk-recovery")]
    [InlineData("injection","api-record")]
    [InlineData("injection","api-recovery")]
    [InlineData("injection","web-record")]
    [InlineData("injection","web-recovery")]
    [InlineData("injection","sdk-record")]
    [InlineData("injection","sdk-recovery")]
    public async Task PermitSupportingOriginalUsesExistingApiWebAndSdkCommandRecoveryWire(string kind,string mode)
    {
        var parent=Review(Scope()).ReadApplication();parent.WELL_ID="well-a";parent.APPLICATION_TYPE=kind=="environmental"?PermitApplicationType.Environmental:PermitApplicationType.Injection;
        var review=PermitApplicationSourceReview.Capture(Scope(),parent,[]);
        await VerifyCommandWireAsync(PermitProviderQualificationTests.SupportingOriginal(review,kind),mode);
    }
    [Theory]
    [InlineData("api-record")]
    [InlineData("api-recovery")]
    [InlineData("web-record")]
    [InlineData("web-recovery")]
    [InlineData("sdk-record")]
    [InlineData("sdk-recovery")]
    public async Task PermitAttachmentMetadataUsesExistingCorrectionAndRecoveryWire(string mode)
    {
        var parent=Review(Scope()).ReadApplication();parent.STATUS=PermitApplicationStatus.Submitted;parent.SUBMITTED_BY_USER_ID="issuer";
        var metadata=new APPLICATION_ATTACHMENT { APPLICATION_ATTACHMENT_ID="metadata-1",PERMIT_APPLICATION_ID=Id,FILE_NAME="recorded.pdf",ACTIVE_IND="Y" };
        var review=PermitApplicationSourceReview.Capture(Scope(),parent,[],PermitApplicationSupportingSnapshot.Capture(parent,null,null,null,[metadata]));
        var original=new PermitApplicationCommand(Guid.NewGuid(),review,"RETURN_FOR_CORRECTION","File content must be confirmed before a positive internal decision");
        await VerifyCommandWireAsync(original,mode);
    }
    private async Task VerifyCommandWireAsync(PermitApplicationCommand original,string mode)
    {
        var outcome=PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);
        var recovery=new PermitApplicationCommandRecovery(original,outcome);var record=mode.EndsWith("record",StringComparison.Ordinal);
        if(mode.StartsWith("api-",StringComparison.Ordinal))
        {
            var lifecycle=new Mock<IPermitApplicationLifecycleService>();lifecycle.Setup(x=>x.RecordCommandAsync(original)).ReturnsAsync(outcome);lifecycle.Setup(x=>x.RecoverCommandAsync(original)).ReturnsAsync(outcome);
            var controller=Controller(lifecycle,Scope,()=>"A");
            var returned=record?Assert.IsType<OkObjectResult>((await controller.RecordCommandAsync(original)).Result).Value:Assert.IsType<OkObjectResult>((await controller.RecoverCommandAsync(original)).Result).Value;
            Assert.Equal(record?(object)outcome:recovery,returned);return;
        }
        object response=record?outcome:recovery;
        using var transport=new Transport(_=>new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(response,response.GetType(),Json),Encoding.UTF8,"application/json")});
        using var http=new HttpClient(transport){BaseAddress=new("https://oilgas.test")};
        if(mode.StartsWith("web-",StringComparison.Ordinal))
        {
            using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.ViewPermits"])));
            var client=new PermitServiceClient(new ApiClient(http,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),company));
            if(record)Assert.Equal(outcome,await client.RecordCommandAsync(original));else Assert.Equal(recovery,await client.RecoverCommandAsync(original));
        }
        else
        {
            using var company=new PermitSdk::Beep.OilandGas.Client.App.CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.ViewPermits"])));
            var app=new PermitSdk::Beep.OilandGas.Client.App.BeepOilandGasApp(http,new PermitSdk::Beep.OilandGas.Client.App.AppOptions{ApiBaseUrl="https://oilgas.test",MaxRetries=0},company);
            if(record)Assert.Equal(outcome,await app.Permits.RecordCommandAsync(original));else Assert.Equal(recovery,await app.Permits.RecoverCommandAsync(original));
        }
        Assert.Equal(original,JsonSerializer.Deserialize<PermitApplicationCommand>(transport.Body!,Json));
        Assert.Equal("/api/field/current/permits/application-command"+(record?"":"/recovery"),transport.Uri);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task PermitSdkCreationReviewPostsExactProposalAndRejectsForeignField(bool foreign)
    {
        var context=Scope();var proposal=CreationProposal();var response=PermitApplicationCreationReview.Capture(foreign?context with{Field=context.Field with{FieldId="B"}}:context,proposal,"field-source","well-source","facility-source");
        using var transport=new Transport(_=>new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(response,Json),Encoding.UTF8,"application/json")});
        using var http=new HttpClient(transport);using var company=new PermitSdk::Beep.OilandGas.Client.App.CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.ViewPermits"])));
        var app=new PermitSdk::Beep.OilandGas.Client.App.BeepOilandGasApp(http,new PermitSdk::Beep.OilandGas.Client.App.AppOptions{ApiBaseUrl="https://oilgas.test",MaxRetries=0},company);
        if(foreign)await Assert.ThrowsAsync<InvalidOperationException>(()=>app.Permits.ReviewCreationAsync(proposal,context.Field));else Assert.Equal(response,await app.Permits.ReviewCreationAsync(proposal,context.Field));
        Assert.Equal(proposal,JsonSerializer.Deserialize<PermitApplicationCreationProposal>(transport.Body!,Json));Assert.Equal("/api/field/current/permits/application-creation-review",transport.Uri);
    }
    private static PermitApplicationCreationProposal CreationProposal()=>new(new(default,default,Country.Kuwait,default,"Applicant",null),"well","shared","gas");
    [Theory][InlineData("valid")][InlineData("proposal")][InlineData("field")][InlineData("module")][InlineData("late-field")]
    public async Task PermitCreationReviewApiConfirmsExactProposalSelectionAndModule(string mode)
    {
        var context=Scope();var field="A";var proposal=CreationProposal();var returned=context;
        if(mode=="field")returned=returned with{Field=returned.Field with{FieldId="B"}};
        if(mode=="module")returned=returned with{Database=new("other","other-binding")};
        var result=PermitApplicationCreationReview.Capture(returned,mode=="proposal"?proposal with{FacilityType="oil"}:proposal,"field-source","well-source","facility-source");
        var access=new Mock<ICompanyPermitApplicationDataAccess>();access.Setup(x=>x.RequireReadScopeAsync()).ReturnsAsync(context);
        access.Setup(x=>x.ReviewCreationAsync(proposal)).ReturnsAsync(()=>{if(mode=="late-field")field="B";return result;});
        var fields=new Mock<IFieldOrchestrator>();fields.SetupGet(x=>x.CurrentFieldId).Returns(()=>field);
        var controller=new PermitsController(Mock.Of<IPermitApplicationLifecycleService>(),Mock.Of<IPermitApplicationWorkflowService>(),Mock.Of<IPermitComplianceCheckService>(),Mock.Of<IPermitStatusHistoryService>(),fields.Object,access.Object)
        {ControllerContext=new(){HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id","actor")],"test"))}}};
        if(mode=="valid")Assert.Equal(result,Assert.IsType<OkObjectResult>((await controller.ReviewCreationAsync(proposal)).Result).Value);
        else await Assert.ThrowsAnyAsync<Exception>(()=>controller.ReviewCreationAsync(proposal));
    }
    [Theory][InlineData("valid")][InlineData("proposal")][InlineData("field")][InlineData("missing")][InlineData("invalid-input")]
    public async Task PermitCreationReviewWebClientBindsPostedProposalAndRejectsForeignOrMissingEvidence(string mode)
    {
        var context=Scope();var proposal=CreationProposal();
        var result=PermitApplicationCreationReview.Capture(mode=="field"?context with{Field=context.Field with{FieldId="B"}}:context,
            mode=="proposal"?proposal with{FacilityType="oil"}:proposal,"field-source","well-source","facility-source");
        using var transport=new Transport(_=>mode=="missing"?new(HttpStatusCode.NoContent):new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(result,Json),Encoding.UTF8,"application/json")});
        using var http=new HttpClient(transport){BaseAddress=new("https://oilgas.test")};using var company=new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.ViewPermits","HSE.IssuePermit"])));
        var client=new PermitServiceClient(new ApiClient(http,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),company));
        if(mode=="valid")Assert.Equal(result,await client.ReviewCreationAsync(proposal,context.Field));
        else await Assert.ThrowsAnyAsync<Exception>(()=>client.ReviewCreationAsync(mode=="invalid-input"?proposal with{FacilityType=null}:proposal,context.Field));
        if(mode=="invalid-input"){Assert.Null(transport.Uri);return;}
        Assert.Equal(HttpMethod.Post,transport.Method);Assert.Equal("/api/field/current/permits/application-creation-review",transport.Uri);
        Assert.Equal(proposal,JsonSerializer.Deserialize<PermitApplicationCreationProposal>(transport.Body!,Json));
    }
    [Theory][InlineData("valid")][InlineData("foreign")][InlineData("duplicates")][InlineData("changed-authority")][InlineData("late-field")]
    public async Task PermitInboxApiBindsReturnedRecordsAndFreshCommandCapabilities(string mode)
    {
        var context=Scope();var field="A";var authority=new PermitApplicationInboxAuthority(context,true,true,ProcessDefinitionRevision.FromJson("{}"));var reads=0;
        var access=new Mock<ICompanyPermitApplicationDataAccess>();access.Setup(x=>x.RequireReadScopeAsync()).ReturnsAsync(context);
        access.Setup(x=>x.ReviewInboxAuthorityAsync()).ReturnsAsync(()=>mode=="changed-authority" && reads++>0?authority with{CanReview=false}:authority);
        var root=new PERMIT_APPLICATION{PERMIT_APPLICATION_ID="submitted",FIELD_ID=mode=="foreign"?"B":"A",STATUS=PermitApplicationStatus.Submitted,COUNTRY=Country.Kuwait,SUBMITTED_BY_USER_ID="issuer"};
        var lifecycle=new Mock<IPermitApplicationLifecycleService>();lifecycle.Setup(x=>x.GetApplicationsAsync()).ReturnsAsync(()=>{if(mode=="late-field")field="B";return mode=="duplicates"?new[]{root,root}:new[]{root};});
        var fields=new Mock<IFieldOrchestrator>();fields.SetupGet(x=>x.CurrentFieldId).Returns(()=>field);
        var controller=new PermitsController(lifecycle.Object,Mock.Of<IPermitApplicationWorkflowService>(),Mock.Of<IPermitComplianceCheckService>(),Mock.Of<IPermitStatusHistoryService>(),fields.Object,access.Object)
        {ControllerContext=new(){HttpContext=new DefaultHttpContext{User=new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id","actor")],"test"))}}};
        if(mode=="valid")
        {
            var review=Assert.IsType<PermitApplicationInboxReview>(Assert.IsType<OkObjectResult>((await controller.ReadInboxAsync()).Result).Value);review.RequireFor(context.Field);
            var row=Assert.Single(review.ReadApplications());Assert.Equal("issuer",row.SubmittedByUserId);Assert.Equal("Kuwait",row.Country);Assert.Equal("Independent review",PermitApplicationInboxReview.Queue(row,"actor",true,true));
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>controller.ReadInboxAsync());
        lifecycle.Verify(x=>x.RecordCommandAsync(It.IsAny<PermitApplicationCommand>()),Times.Never());
    }
    [Theory][InlineData(false,"valid")][InlineData(false,"foreign")][InlineData(false,"missing")][InlineData(true,"valid")][InlineData(true,"foreign")][InlineData(true,"missing")]
    public async Task PermitInboxWebAndSdkUseCanonicalBoundReadOnlyEndpoint(bool sdk,string mode)
    {
        var context=Scope();var expected=context.Field;var row=new PermitApplicationSummary{PermitApplicationId="one",FieldId=mode=="foreign"?"B":"A",Status="Submitted",SubmittedByUserId="issuer"};
        var response=new PermitApplicationInboxReview(new(context,true,true,ProcessDefinitionRevision.FromJson("{}")),DateTimeOffset.UtcNow,JsonSerializer.Serialize(new[]{row}));
        using var transport=new Transport(_=>mode=="missing"?new(HttpStatusCode.NoContent):new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(response,Json),Encoding.UTF8,"application/json")});
        using var http=new HttpClient(transport){BaseAddress=new("https://oilgas.test")};
        async Task<PermitApplicationInboxReview> Read()
        {
            if(sdk)
            {
                using var company=new PermitSdk::Beep.OilandGas.Client.App.CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.ViewPermits"])));
                var app=new PermitSdk::Beep.OilandGas.Client.App.BeepOilandGasApp(http,new PermitSdk::Beep.OilandGas.Client.App.AppOptions{ApiBaseUrl="https://oilgas.test",MaxRetries=0},company);return await app.Permits.ReadInboxAsync(expected);
            }
            using(var company=new CompanyClientContext())
            {Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Safety"],["HSE.ViewPermits"])));return await new PermitServiceClient(new ApiClient(http,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),company)).ReadInboxAsync(expected);}
        }
        if(mode=="valid")Assert.Equal(response,await Read());else await Assert.ThrowsAnyAsync<Exception>(Read);
        Assert.Equal(HttpMethod.Get,transport.Method);Assert.Equal("/api/field/current/permits/application-inbox",transport.Uri);
    }
    [Fact]
    public async Task PermitCreationRejectsRetiredDuplicateDrillingClassificationBeforeAccess()
    {
        var proposal=CreationProposal();proposal=proposal with{Header=proposal.Header with{ApplicationType=(PermitApplicationType)8}};
        var lifecycle=new Mock<IPermitApplicationLifecycleService>();var controller=Controller(lifecycle,()=>throw new InvalidOperationException("Invalid classifications must refuse before native access."),()=>"A");
        var refusal=await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>controller.ReviewCreationAsync(proposal));Assert.Equal(Beep.OilandGas.Models.Core.Refusals.RefusalKind.Invalid,refusal.Kind);lifecycle.VerifyNoOtherCalls();
    }
    [Fact]
    public void PermitRoutesExposeReviewedCommandsAndRetireDirectSubmissionDecision()
    {
        foreach(var type in new[]{typeof(PermitsController),typeof(IPermitApplicationLifecycleService),typeof(IPermitServiceClient)})
        {Assert.Null(type.GetMethod("CreateAsync"));Assert.Null(type.GetMethod("UpdateAsync"));Assert.Null(type.GetMethod("SubmitAsync"));Assert.Null(type.GetMethod("ProcessDecisionAsync"));}
        Assert.Equal("application-review",typeof(PermitsController).GetMethod(nameof(PermitsController.ReviewSourceAsync))!.GetCustomAttributes(typeof(HttpGetAttribute),true).Cast<HttpGetAttribute>().Single().Template);
        Assert.Equal("application-command",typeof(PermitsController).GetMethod(nameof(PermitsController.RecordCommandAsync))!.GetCustomAttributes(typeof(HttpPostAttribute),true).Cast<HttpPostAttribute>().Single().Template);
        Assert.Equal("application-command/recovery",typeof(PermitsController).GetMethod(nameof(PermitsController.RecoverCommandAsync))!.GetCustomAttributes(typeof(HttpPostAttribute),true).Cast<HttpPostAttribute>().Single().Template);
    }
}
#endif
