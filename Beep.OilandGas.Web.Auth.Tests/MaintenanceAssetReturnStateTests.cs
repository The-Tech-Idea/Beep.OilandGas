#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class MaintenanceAssetReturnStateTests
{
    internal static CompanyMaintenanceAssetReturnReview QualifiedReview(string targets="E")
    {
        // Controlled complete native-source/physical/closure projections for actual client, state and form checks.
        var start=WorkOrderExecutionStartStateTests.QualifiedStart();var source=start.Review.Prerequisites.Requirements;
        var order=CompanyInventorySourceReview.Copy(source.Order);order.Components[0]=order.Components[0] with{WellId=targets.Contains('W')?"original-well":"",EquipmentId=targets.Contains('E')?"pump":"",FacilityId=targets.Contains('F')?"plant":null,FacilityType=targets.Contains('F')?"gas":null};
        var context=source.Context with{ActorUserId="return-approver",Revision="current-qualified-return"};var utc=DateTime.UtcNow;var from=utc.AddDays(-10);var to=utc.AddYears(1);
        var qualified=new CompanyMaintenanceQualifiedAssets(context,CompanyWorkOrderExecutionRequirements.OrderRevision(order),"UTC",
            [new("plant","gas",order.FieldId,context.Company.BusinessAssociateId,targets.Contains('F')?"WRITE":"READ",from,to,new string('A',64))],
            targets.Contains('E')?[new("pump",context.Company.BusinessAssociateId,"WRITE",from,to,from,null,new string('B',64),new("pump","plant","gas",7,from,from,null,null,"installation-source",new string('C',64),1,new string('D',64)))]:[],utc);
        var statuses=new List<CompanyMaintenanceQualifiedStatus>{new("FACILITY","plant","gas","facility-before","NativeCondition","INACTIVE","facility-source","Y",from,from,null,null,"UTC",true,new string('A',64))};
        if(targets.Contains('E'))statuses.Add(new("EQUIPMENT","pump",null,"equipment-before","EquipmentFacet","IDLE","equipment-source","Y",from,from,null,null,"UTC",true,new string('B',64)));
        var statusReferences=statuses.SelectMany(row=>new[]{new CompanyMaintenanceQualifiedStatusReference(row.AssetType,row.StatusType,row.Value,"Y",from,to,true,true,new string('C',64),new string('D',64)),new(row.AssetType,row.StatusType,"RECORDED_TARGET","Y",from,to,true,true,new string('E',64),new string('F',64))}).ToArray();
        var compared=DateTime.UtcNow;var operating=new CompanyMaintenanceQualifiedOperating(context,qualified.IdentityRevision(),statuses,statusReferences,new string('A',64),compared,CompanyMaintenanceQualifiedOperating.GetBlockers(qualified,statuses,statusReferences,compared));qualified=qualified with{Operating=operating};
        CompanyMaintenanceFacilityLicense[] licenses=[new("plant","gas","licence","LIC-UI-RETURN","RecordedType","issuer",context.Company.BusinessAssociateId,order.FieldId,"Y",from,from,to,"N","Y","licence-source",true,new string('B',64))];
        CompanyMaintenanceLicenseStatus[] licenceStatuses=[new("plant","gas","licence","licence-status","RecordedDecision","APPROVED","Y",from,to,"licence-status-source",true,new string('C',64))];
        CompanyMaintenanceLicenseReference Ref(string kind,string key,string? qualifier=null)=>new(kind,key,qualifier,"Y",from,to,true,new string('D',64));
        CompanyMaintenanceLicenseReference[] references=[Ref("BUSINESS_ASSOCIATE","issuer"),Ref("BUSINESS_ASSOCIATE",context.Company.BusinessAssociateId),Ref("FACILITY_LIC_TYPE","issuer","RecordedType"),Ref("R_LIC_STATUS_TYPE","RecordedDecision"),Ref("R_LICENSE_STATUS","RecordedDecision","APPROVED"),Ref("R_AREA_TYPE","RecordedRegion")];
        CompanyMaintenanceLicenseArea[] areas=[new("plant","gas","licence","recorded-area","RecordedRegion","Recorded area","Y",from,to,"area-source",true,new string('E',64),new("recorded-area","RecordedRegion","Recorded region","Y",from,to,"area-catalog-source","area-document",true,new string('F',64)))];
        compared=DateTime.UtcNow;var licensing=new CompanyMaintenanceQualifiedLicensing(context,qualified.IdentityRevision(),licenses,licenceStatuses,[],[],references,compared,CompanyMaintenanceQualifiedLicensing.GetBlockers(qualified,licenses,licenceStatuses,[],[],references,compared,areas)){Areas=areas};qualified=qualified with{Licensing=licensing};
        var transitions=new List<CompanyMaintenanceReturnTransition>{targets.Contains('F')?new("FACILITY","NativeCondition",["INACTIVE"],"RECORDED_TARGET","recorded-return-source","Reviewed transition"):new("FACILITY","NativeCondition",["INACTIVE"],"","","Reviewed parent preservation"){Disposition="PRESERVE"}};
        if(targets.Contains('E'))transitions.Add(new("EQUIPMENT","EquipmentFacet",["IDLE"],"RECORDED_TARGET","recorded-return-source","Reviewed equipment transition"));
        var draft=new CompanyMaintenanceReturnPolicyDraft("CORRECTIVE","gas","Recorded jurisdiction","recorded-area","RecordedRegion",["FacilityRole"],transitions,[new("issuer","RecordedType","REQUIRED","REQUIRED_PAID",[new("RecordedDecision",["APPROVED"],"Recorded status decision")],[],[],"Recorded applicability")],from,to,"Reviewed policy");
        var policyContext=CompanyMaintenanceQualifiedReturnPolicy.ContextFor(qualified,["FacilityRole"]);var empty=CompanyMaintenanceReturnPolicyCatalog.Create(policyContext,[],DateTime.UtcNow);
        var policyCommand=new CompanyMaintenanceReturnPolicyCommand(policyContext,empty.Revision,draft,1,Guid.NewGuid(),"Controlled UI policy rationale");var catalog=CompanyMaintenanceReturnPolicyCatalog.Create(policyContext,[CompanyMaintenanceReturnPolicyVersion.From(policyCommand.Prepare(DateTime.UtcNow))],DateTime.UtcNow);
        var nativeSources=CompanyMaintenanceQualifiedReturnPolicy.RequiredNativeSources(qualified,order,catalog,DateTime.UtcNow).Select(key=>new CompanyMaintenanceReturnNativeSource(key,"Y",from,to,true,new string('A',64))).ToArray();
        var policy=CompanyMaintenanceQualifiedReturnPolicy.Create(qualified,order,catalog,["FacilityRole"],nativeSources,DateTime.UtcNow);Assert.Empty(policy.Blockers);qualified=qualified with{ReturnPolicy=policy};qualified.Require(order);
        var submitted=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission(database:context.Database,fieldId:order.FieldId,originalOrder:order,taskOverride:source.Approval.Permit.Task,completionVerification:true,returnAcceptance:true,qualifiedWorkAssets:qualified);
        var report=CompanyWorkOrderSubmissionFacts.From(submitted.Command.Prepare(DateTime.UtcNow));var original=submitted.Command.Review.Inspections.Current;var handoff=DateTime.UtcNow;
        var requirements=original.Requirements.Select(row=>row.ApplicableStage is "COMPLETION_VERIFICATION" or "RETURN_TO_SERVICE"?row with{Status="PASS",ResultText="Independent observation",Inspector=row.ApplicableStage=="RETURN_TO_SERVICE"?context.ActorUserId:"completion-reviewer",InspectDate=DateTime.UtcNow,SourceRevision=new string('B',64)}:row).ToArray();
        var person=new CompanyNativePersonFacts(context.Company,"return-person","Recorded independent return person","PERSON",from,to,new(context.Company.BusinessAssociateId,"return-person",1,"Return duty","ACTIVE",from,to,new string('C',64)),new string('D',64));
        CompanyPersonnelIdentityVersion[] links=[new(context.Company,context.Database,CompanyPersonnelIdentityCommand.RecordIdFor(context.Company,context.ActorUserId,1),context.ActorUserId,person.PersonBaId,"PERSON",1,from,to,"Independent return mapping","identity-administrator",from,new string('E',64)),new(context.Company,context.Database,CompanyPersonnelIdentityCommand.RecordIdFor(context.Company,"completion-reviewer",1),"completion-reviewer","verification-person","PERSON",1,from,to,"Independent verification mapping","identity-administrator",from,new string('F',64))];
        var identities=CompanyPersonnelIdentityCatalog.Create(new(context.Company,context.Database,context.ActorUserId,new string('A',64)),links,DateTime.UtcNow);compared=DateTime.UtcNow;
        var completion=new CompanyWorkOrderCompletionReview(context,report,order,requirements,identities,person,compared,CompanyWorkOrderCompletionReview.GetBlockers(context,report,order,requirements,identities,person,compared));
        var verification=new CompanyWorkOrderCompletionDecisionFacts(context.Company,context.Database,report.Query,"completion-reviewer","verification-person","SupervisorRole","VERIFIED",handoff.AddTicks(-1),"verification-approval","verification-history",report.Revision,completion.SourceRevision(),new string('B',64)){WorkSubjectRevision=completion.WorkSubjectRevision()};
        var stage=new CompanyMaintenanceAssignedStageReview(context,report.Query.Task,"return-step-instance","return","Independent return approval","FacilityRole","receiver","receiving-history","receiving-approval",new string('C',64),new string('D',64));
        var assignment=new CompanyMaintenanceReturnAssignment(stage,verification,"handoff-history",handoff,new string('E',64));var physical=new CompanyWorkOrderReturnReview(assignment,completion,[person],CompanyWorkOrderReturnReview.GetBlockers(assignment,completion,[person]));physical.RequireAcceptance();
        var closure=new CompanyMaintenanceClosureFacts(context.Company,context.Database,report.Query,context.ActorUserId,"FacilityRole","closure-history",["closed-task"],DateTime.UtcNow,new string('F',64),physical.PhysicalSubjectRevision(),report.Revision,new string('A',64));
        var wells=targets.Contains('W')?Review().Assets.Wells.Select(row=>row with{Context=context}).ToArray():[];
        compared=DateTime.UtcNow;physical=physical with{Completion=physical.Completion with{ComparedAtUtc=compared}};
        var assets=new CompanyMaintenanceAssetEvidence(context,report,closure,order,wells,compared,CompanyMaintenanceAssetEvidence.GetBlockers(context,report,closure,order,wells,compared,qualified)){QualifiedAssets=qualified};
        var review=new CompanyMaintenanceAssetReturnReview(assets,physical);review.Require();return review;
    }
    internal static CompanyMaintenanceAssetReturnReview Review()
    {
        var closure=MaintenanceClosureStateTests.Review("original-well",assetReturn:true);var assets=MaintenanceAssetEvidenceTests.Review(closure,true);
        var physical=closure.Current with{Completion=closure.Current.Completion with{ComparedAtUtc=DateTime.UtcNow}};var result=new CompanyMaintenanceAssetReturnReview(assets,physical);result.Require();return result;
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ImportedOriginalReferenceBlocksReplacementAndSurvivesSourceChanges(bool qualified)
    {
        var review=qualified?QualifiedReview():Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Assets.Context.ActorUserId,new(review.Assets.Context.Company,["FacilityRole"],CompanyMaintenanceAssetReturnCapabilities.ForOrder(review.Assets.Order))));
        using var state=new MaintenanceAssetReturnState(company);state.Attach(review.Assets.Report.Query);var original=Guid.NewGuid();var client=new Mock<ICompanyMaintenanceHandoffClient>();
        Assert.True(state.TryRetainReference(original));Assert.True(state.TryRetainReference(original));Assert.False(state.TryRetainReference(Guid.NewGuid()));Assert.True(state.NeedsRecovery);Assert.False(state.CanReview);Assert.Equal(original,state.VisibleReference);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAsync(client.Object));
        state.Attach(review.Assets.Report.Query with{SubmissionId="another-submission"});Assert.Null(state.VisibleReference);Assert.False(state.TryRetainReference(original));Assert.False(state.CanReview);
        state.Attach(review.Assets.Report.Query);Assert.True(state.TryRetainReference(original));Assert.Equal(original,state.VisibleReference);
        Assert.True(company.TrySelect(company.Clear(),"another-account",new(review.Assets.Context.Company,["FacilityRole"],CompanyMaintenanceAssetReturnCapabilities.ForOrder(review.Assets.Order))));state.Attach(review.Assets.Report.Query);
        Assert.Null(state.VisibleReference);Assert.False(state.TryRetainReference(original));Assert.False(state.CanReview);
        client.Verify(value=>value.ReviewMaintenanceAssetReturnAsync(It.IsAny<CompanyWorkOrderCompletionQuery>(),It.IsAny<CancellationToken>()),Times.Never);
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Fact]
    public async Task ImportedReferenceFencesAnAlreadyRunningReviewBeforeItsLateReply()
    {
        var review=QualifiedReview();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Assets.Context.ActorUserId,new(review.Assets.Context.Company,["FacilityRole"],CompanyMaintenanceAssetReturnCapabilities.ForOrder(review.Assets.Order))));
        using var state=new MaintenanceAssetReturnState(company);state.Attach(review.Assets.Report.Query);var reply=new TaskCompletionSource<CompanyMaintenanceAssetReturnReview>(TaskCreationOptions.RunContinuationsAsynchronously);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewMaintenanceAssetReturnAsync(review.Assets.Report.Query,It.IsAny<CancellationToken>())).Returns(reply.Task);
        var running=state.ReviewAsync(client.Object);var original=Guid.NewGuid();Assert.True(state.Busy);Assert.True(state.TryRetainReference(original));reply.SetResult(review);await running;
        Assert.Null(state.Review);Assert.True(state.NeedsRecovery);Assert.Equal(original,state.VisibleReference);Assert.False(state.CanRecord);Assert.False(state.CanReview);
    }
    [Theory][InlineData("failed")][InlineData("missing")][InlineData("forbidden")]
    public async Task FailedReferenceRecoveryNeverConfirmsAbsenceOrUnlocksAReplacement(string mode)
    {
        var review=QualifiedReview();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Assets.Context.ActorUserId,new(review.Assets.Context.Company,["FacilityRole"],CompanyMaintenanceAssetReturnCapabilities.ForOrder(review.Assets.Order))));
        using var state=new MaintenanceAssetReturnState(company);state.Attach(review.Assets.Report.Query);var original=Guid.NewGuid();Assert.True(state.TryRetainReference(original));var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.RecoverMaintenanceAssetReturnAsync(new(review.Assets.Report.Query,original),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException(mode,null,mode=="missing"?HttpStatusCode.NotFound:mode=="forbidden"?HttpStatusCode.Forbidden:HttpStatusCode.ServiceUnavailable));
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.RecoverAsync(client.Object,original));Assert.True(state.NeedsRecovery);Assert.False(state.CanReview);Assert.Equal(original,state.VisibleReference);Assert.False(state.TryRetainReference(Guid.NewGuid()));
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("F")][InlineData("E")][InlineData("WEF")]
    public async Task QualifiedRecordRetainsEveryOriginalObservationBeforeTransportAndRecoversWithoutReplay(string targets)
    {
        var review=QualifiedReview(targets);using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Assets.Context.ActorUserId,new(review.Assets.Context.Company,["FacilityRole"],CompanyMaintenanceAssetReturnCapabilities.ForOrder(review.Assets.Order))));
        using var state=new MaintenanceAssetReturnState(company);state.Attach(review.Assets.Report.Query);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewMaintenanceAssetReturnAsync(review.Assets.Report.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);await state.ReviewAsync(client.Object);Assert.True(state.CanRecord);
        var observations=new List<CompanyMaintenanceQualifiedReturnObservation>();if(targets.Contains('F'))observations.Add(new("FACILITY","plant","gas",0m));if(targets.Contains('E'))observations.Add(new("EQUIPMENT","pump",null,72.5m));
        Guid retained=Guid.Empty;CompanyMaintenanceAssetReturnCommand? sent=null;
        client.Setup(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceAssetReturnCommand command,CancellationToken _)=>{Assert.Equal(retained,command.IntentId);sent=command;throw new IOException("Unconfirmed acknowledgement");});
        await Assert.ThrowsAsync<IOException>(()=>state.RecordAsync(client.Object,DateTime.UtcNow,Observations(review),"Actual complete qualified observation",true,id=>{retained=id;return Task.CompletedTask;},observations));
        Assert.NotNull(sent);Assert.Equal(observations,sent.QualifiedAssets);Assert.Null(state.Receipt);Assert.False(state.CanReview);Assert.Equal(retained,state.VisibleReference);
        client.Setup(value=>value.RecoverMaintenanceAssetReturnAsync(sent.Reference(),It.IsAny<CancellationToken>())).ReturnsAsync(Saved(sent));await state.RecoverAsync(client.Object,retained);Assert.NotNull(state.Receipt);Assert.Equal(JsonSerializer.Serialize(sent),JsonSerializer.Serialize(state.Receipt.Command));
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Once);
    }
    [Theory][InlineData("missing")][InlineData("parent")][InlineData("qualifier")][InlineData("percent")]
    public async Task QualifiedRecordRejectsIncompleteOrExpandedObservationsBeforeRetainingAnIntent(string mode)
    {
        var review=QualifiedReview();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Assets.Context.ActorUserId,new(review.Assets.Context.Company,["FacilityRole"],CompanyMaintenanceAssetReturnCapabilities.ForOrder(review.Assets.Order))));
        using var state=new MaintenanceAssetReturnState(company);state.Attach(review.Assets.Report.Query);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewMaintenanceAssetReturnAsync(review.Assets.Report.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);await state.ReviewAsync(client.Object);
        var observations=new List<CompanyMaintenanceQualifiedReturnObservation>{new("EQUIPMENT","pump",null,mode=="percent"?null:72.5m)};if(mode=="missing")observations.Clear();if(mode=="parent")observations.Add(new("FACILITY","plant","gas",0m));if(mode=="qualifier")observations[0]=observations[0] with{FacilityType="gas"};var retained=false;
        await Assert.ThrowsAnyAsync<Exception>(()=>state.RecordAsync(client.Object,DateTime.UtcNow,[],"Actual observation",true,_=>{retained=true;return Task.CompletedTask;},observations));Assert.False(retained);Assert.Null(state.VisibleReference);
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Fact]
    public void EarlierWellOnlyReturnCommandKeepsItsExactSerializedBodyWithoutQualifiedObservations()
    {
        var review=Review();var command=new CompanyMaintenanceAssetReturnCommand(review,Guid.NewGuid(),DateTime.UtcNow,Observations(review),"Original well-only return",true);command.Require();
        var original=JsonSerializer.Serialize(new{command.Review,command.IntentId,command.ActivatedAtUtc,command.Wells,command.Reason,command.Confirmed});
        Assert.Equal(original,JsonSerializer.Serialize(command));Assert.DoesNotContain("QualifiedAssets",original);
        var restored=JsonSerializer.Deserialize<CompanyMaintenanceAssetReturnCommand>(original)!;restored.Require();Assert.Null(restored.QualifiedAssets);Assert.Equal(original,JsonSerializer.Serialize(restored));
        Assert.Throws<InvalidOperationException>(()=>(command with{QualifiedAssets=[new("EQUIPMENT","injected-target",null,70m)]}).Require());
    }
    private static readonly string[] Permissions=["Maintenance.ReturnToService","Workflow.Execute","WellManagement.UpdateWellStatus"];
    [Fact]
    public async Task UnknownReturnScopeUsesCommonCapabilitiesAndRejectsUnauthorizedReturnedTargetsBeforeDisplay()
    {
        var review=Review();using var company=Company(review,"WellManagement.UpdateWellStatus");using var state=new MaintenanceAssetReturnState(company);state.Attach(review.Assets.Report.Query);
        Assert.True(state.CanReview);Assert.Equal(CompanyMaintenanceAssetReturnCapabilities.Common,state.RequiredCapabilities);
        var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewMaintenanceAssetReturnAsync(review.Assets.Report.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>state.ReviewAsync(client.Object));Assert.Null(state.Review);Assert.False(state.CanRecord);Assert.Null(state.VisibleReference);
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private static CompanyClientContext Company(CompanyMaintenanceAssetReturnReview review,string mode="current")
    {var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Assets.Context.ActorUserId,new(review.Assets.Context.Company,mode=="role"?["OtherRole"]:[review.Physical.Assignment.Stage.AssignedRole],Permissions.Where(permission=>permission!=mode).ToArray())));return company;}
    internal static CompanyWellReturnActivation[] Observations(CompanyMaintenanceAssetReturnReview review)=>review.Assets.Wells.Select(well=>new CompanyWellReturnActivation(well.WellId,well.Facets.Single(row=>row.Facet=="Condition").Source,65.5m)).ToArray();
    internal static CompanyMaintenanceAssetReturnRecovery Saved(CompanyMaintenanceAssetReturnCommand command)=>new(command,new(command.Review.Assets.Context.Company,command.Review.Assets.Context.Database,command.Review.Assets.Context.ActorUserId,command.IntentId,command.ReturnId(),command.Reference().Query,command.Wells.Select(well=>well.WellId).Order(StringComparer.Ordinal).ToArray(),command.ActivatedAtUtc,DateTime.UtcNow,new string('D',64)));
    [Theory][InlineData("uncertain")][InlineData("late-company")][InlineData("late-source")][InlineData("retention-failed")]
    public async Task OriginalBodyIsRetainedBeforeTransportAndUncertaintyCannotReplayOrReplace(string mode)
    {
        var review=Review();using var company=Company(review);using var state=new MaintenanceAssetReturnState(company);state.Attach(review.Assets.Report.Query);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewMaintenanceAssetReturnAsync(review.Assets.Report.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);await state.ReviewAsync(client.Object);
        Guid retained=Guid.Empty;CompanyMaintenanceAssetReturnCommand? sent=null;
        client.Setup(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceAssetReturnCommand original,CancellationToken _)=>{sent=original;Assert.Equal(retained,original.IntentId);if(mode=="late-company")company.Clear();if(mode=="late-source")state.Clear();throw new IOException("Acknowledgement unavailable");});
        await Assert.ThrowsAsync<IOException>(()=>state.RecordAsync(client.Object,DateTime.UtcNow,Observations(review),"Observed capability and actual return",true,id=>{retained=id;if(mode=="retention-failed")throw new IOException("Reference could not be retained");return Task.CompletedTask;}));
        Assert.Null(state.Receipt);Assert.False(state.CanReview);Assert.NotEqual(Guid.Empty,retained);
        if(mode is "late-company" or "late-source"){Assert.Null(state.VisibleReference);Assert.True(company.TrySelect(company.Clear(),review.Assets.Context.ActorUserId,new(review.Assets.Context.Company,[review.Physical.Assignment.Stage.AssignedRole],Permissions)));state.Attach(review.Assets.Report.Query);}
        Assert.Equal(retained,state.VisibleReference);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAsync(client.Object));await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverAsync(client.Object,Guid.NewGuid()));
        if(mode=="retention-failed"){Assert.Null(sent);client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);return;}
        client.Setup(value=>value.RecoverMaintenanceAssetReturnAsync(sent!.Reference(),It.IsAny<CancellationToken>())).ReturnsAsync(Saved(sent!));await state.RecoverAsync(client.Object,retained);Assert.NotNull(state.Receipt);Assert.Equal(JsonSerializer.Serialize(sent),JsonSerializer.Serialize(state.Receipt!.Command));
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Once);
    }
    [Theory][InlineData("current")][InlineData("role")][InlineData("Maintenance.ReturnToService")][InlineData("Workflow.Execute")][InlineData("WellManagement.UpdateWellStatus")][InlineData("late-company")][InlineData("late-source")]
    public async Task FreshCircuitRecoveryRequiresCurrentRoleAndEveryCapabilityAndNeverWrites(string mode)
    {
        var review=Review();using var company=Company(review,mode);using var state=new MaintenanceAssetReturnState(company);state.Attach(review.Assets.Report.Query);var command=new CompanyMaintenanceAssetReturnCommand(review,Guid.NewGuid(),DateTime.UtcNow,Observations(review),"Original retained observation",true);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.RecoverMaintenanceAssetReturnAsync(command.Reference(),It.IsAny<CancellationToken>())).Returns(()=>{if(mode=="late-company")company.Clear();if(mode=="late-source")state.Clear();return Task.FromResult(Saved(command));});
        var error=await Record.ExceptionAsync(()=>state.RecoverAsync(client.Object,command.IntentId));if(mode=="current"){Assert.Null(error);Assert.NotNull(state.Receipt);}else{Assert.Null(state.Receipt);if(!mode.StartsWith("late-",StringComparison.Ordinal))Assert.IsType<UnauthorizedAccessException>(error);}
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class Reply(Func<HttpRequestMessage,Task<HttpResponseMessage>> read):HttpMessageHandler
    {internal int Calls;protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;return read(request);}}
    [Theory][InlineData("review")][InlineData("record")][InlineData("recovery")][InlineData("late-company")][InlineData("permission")][InlineData("changed-body")]
    public async Task ActualTypedClientUsesProtectedRoutesAndRejectsForeignOrChangedAcknowledgements(string mode)
    {
        var review=Review();using var company=Company(review,mode=="permission"?"WellManagement.UpdateWellStatus":"current");var command=new CompanyMaintenanceAssetReturnCommand(review,Guid.NewGuid(),DateTime.UtcNow,Observations(review),"Explicit retained original observation",true);var reply=new Reply(async request=>
        {
            var path=request.RequestUri!.AbsolutePath;Assert.StartsWith("/api/field/current/maintenance-handoffs/execution-permit/asset-return",path);if(mode=="late-company")company.Clear();
            if(mode=="review")return new(HttpStatusCode.OK){Content=JsonContent.Create(review)};
            if(mode=="recovery"){Assert.EndsWith("/recovery/reference",path);Assert.Equal(command.Reference(),await request.Content!.ReadFromJsonAsync<CompanyMaintenanceAssetReturnReference>());}
            else Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(await request.Content!.ReadFromJsonAsync<CompanyMaintenanceAssetReturnCommand>()));
            return new(HttpStatusCode.OK){Content=JsonContent.Create(Saved(mode=="changed-body"?command with{Reason="Changed observation"}:command))};
        });using var http=new HttpClient(reply){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{if(mode=="review")await client.ReviewMaintenanceAssetReturnAsync(review.Assets.Report.Query);else if(mode=="recovery")await client.RecoverMaintenanceAssetReturnAsync(command.Reference());else await client.RecordMaintenanceAssetReturnAsync(command);});
        if(mode is "review" or "record" or "recovery")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,reply.Calls);
    }
    private sealed class Auth(string actor):AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"FacilityRole")],"fixture"))));}
    private sealed class Nav:NavigationManager
    {internal Nav()=>Initialize("http://fixture/","http://fixture/order");protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class Components:IComponentActivator
    {internal MaintenanceAssetReturnPanel Panel=null!;internal MaintenanceClosurePanel Closure=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is MaintenanceAssetReturnPanel panel)Panel=panel;if(result is MaintenanceClosurePanel closure)Closure=closure;return result;}}
    private sealed class NoBrowser:IJSRuntime
    {public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    private static ServiceCollection Services(CompanyClientContext company,ICompanyMaintenanceHandoffClient client,IFieldSelectionContext fields,Nav nav)
    {var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<MaintenanceClosureState>();services.AddScoped<MaintenanceAssetReturnState>();services.AddScoped<MaintenanceAssetEvidenceState>();services.AddSingleton(client);services.AddSingleton(fields);services.AddSingleton<AuthenticationStateProvider>(new Auth(company.Selection!.UserId));services.AddSingleton<NavigationManager>(nav);services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));return services;}
    [Theory][InlineData(false,"pending")][InlineData(true,"pending")][InlineData(true,"confirmed-review")][InlineData(true,"failed")][InlineData(true,"removed-link")][InlineData(true,"duplicate")][InlineData(true,"malformed")][InlineData(true,"process")][InlineData(true,"submission")][InlineData(true,"orphan")]
    public async Task ActualImportedReferenceScreenRequiresOriginalRecoveryBeforeAnyReview(bool qualified,string mode)
    {
        var review=qualified?QualifiedReview():Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Assets.Context.ActorUserId,new(review.Assets.Context.Company,["FacilityRole"],CompanyMaintenanceAssetReturnCapabilities.ForOrder(review.Assets.Order))));
        var id=Guid.NewGuid();var command=new CompanyMaintenanceAssetReturnCommand(review,id,DateTime.UtcNow,Observations(review),"Original retained return",true){QualifiedAssets=qualified?[new("EQUIPMENT","pump",null,0m)]:null};command.Require();
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(review.Assets.Context.FieldId);var nav=new Nav();
        var url=nav.GetUriWithQueryParameters(new Dictionary<string,object?>{{"maintenanceAssetReturn",mode=="malformed"?"invalid":id.ToString("D")},{"assetReturnProcess",mode=="process"?"another-process":review.Assets.Report.Query.Task.InstanceId},{"assetReturnSubmission",mode=="submission"?null:mode=="orphan"?null:review.Assets.Report.Query.SubmissionId}});
        if(mode=="duplicate")url+="&maintenanceAssetReturn="+Guid.NewGuid().ToString("D");if(mode=="orphan")url="http://fixture/order?assetReturnProcess="+review.Assets.Report.Query.Task.InstanceId;nav.NavigateTo(url);
        var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewMaintenanceAssetReturnAsync(review.Assets.Report.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(value=>value.RecoverMaintenanceAssetReturnAsync(command.Reference(),It.IsAny<CancellationToken>())).Returns(()=>mode=="failed"?Task.FromException<CompanyMaintenanceAssetReturnRecovery>(new HttpRequestException("Original return unavailable",null,HttpStatusCode.NotFound)):Task.FromResult(Saved(command)));
        await using var provider=Services(company,client.Object,fields.Object,nav).BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"Query",review.Assets.Report.Query}});var root=await renderer.RenderComponentAsync<MaintenanceAssetReturnPanel>(parameters);var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;var state=provider.GetRequiredService<MaintenanceAssetReturnState>();
            Task Invoke(string method)=>(Task)typeof(MaintenanceAssetReturnPanel).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;
            await Invoke("ReviewAsync");await Task.Yield();Assert.Null(state.Review);
            if(mode is "duplicate" or "malformed" or "process" or "submission" or "orphan"){Assert.Contains("original return link does not match this report",root.ToHtmlString());return;}
            Assert.True(state.NeedsRecovery);Assert.False(state.CanReview);Assert.Equal(id,state.VisibleReference);Assert.Contains("Recover the original return reference",root.ToHtmlString());
            if(mode=="removed-link"){nav.NavigateTo("http://fixture/order");await panel.SetParametersAsync(parameters);await Task.Yield();Assert.True(state.NeedsRecovery);await Invoke("ReviewAsync");Assert.Null(state.Review);}
            typeof(MaintenanceAssetReturnPanel).GetField("_reference",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,Guid.NewGuid().ToString("D"));await Invoke("RecoverAsync");Assert.True(state.NeedsRecovery);
            typeof(MaintenanceAssetReturnPanel).GetField("_reference",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,id.ToString("D"));await Invoke("RecoverAsync");await Task.Yield();
            if(mode=="failed"){Assert.True(state.NeedsRecovery);Assert.False(state.CanReview);Assert.Null(state.Receipt);Assert.Equal(id,state.VisibleReference);}
            else
            {
                Assert.False(state.NeedsRecovery);Assert.NotNull(state.Receipt);Assert.Contains("Recorded native asset return",root.ToHtmlString());
                if(mode=="confirmed-review"){await panel.SetParametersAsync(parameters);await Invoke("ReviewAsync");await panel.SetParametersAsync(parameters);await Task.Yield();Assert.NotNull(state.Review);Assert.False(state.NeedsRecovery);}
            }
        });
        client.Verify(value=>value.ReviewMaintenanceAssetReturnAsync(It.IsAny<CompanyWorkOrderCompletionQuery>(),It.IsAny<CancellationToken>()),mode=="confirmed-review"?Times.Once():Times.Never());
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("F","confirmed")][InlineData("E","confirmed")][InlineData("WEF","confirmed")][InlineData("E","zero")][InlineData("E","missing-percent")][InlineData("E","no-preview")][InlineData("E","changed-input")][InlineData("E","changed-source")][InlineData("E","conflicting-link")][InlineData("E","uncertain")][InlineData("E","permission")][InlineData("E","company-change")]
    public async Task ActualQualifiedFormChecksCompleteTargetsBeforeConfirmationAndRetainsOriginalRecovery(string targets,string mode)
    {
        var review=QualifiedReview(targets);using var company=new CompanyClientContext();var capabilities=CompanyMaintenanceAssetReturnCapabilities.ForOrder(review.Assets.Order).Where(permission=>mode!="permission" || permission!="Facilities.ManageEquipment").ToArray();
        Assert.True(company.TrySelect(company.Clear(),review.Assets.Context.ActorUserId,new(review.Assets.Context.Company,["FacilityRole"],capabilities)));
        var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewMaintenanceAssetReturnAsync(review.Assets.Report.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(review.Assets.Context.FieldId);var nav=new Nav();CompanyMaintenanceAssetReturnCommand? original=null;
        client.Setup(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceAssetReturnCommand command,CancellationToken _)=>{original=command;var retained=Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri(nav.Uri).Query);Assert.Equal(command.IntentId.ToString("D"),Assert.Single(retained["maintenanceAssetReturn"]));if(mode=="uncertain")throw new IOException("Acknowledgement unavailable");return Task.FromResult(Saved(command));});
        await using var provider=Services(company,client.Object,fields.Object,nav).BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<MaintenanceAssetReturnPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Query",review.Assets.Report.Query}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            Task Invoke(string method)=>(Task)typeof(MaintenanceAssetReturnPanel).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;
            void Set(string field,object value)=>typeof(MaintenanceAssetReturnPanel).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,value);
            void Render()=>typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null);
            await Invoke("ReviewAsync");await Task.Yield();var state=provider.GetRequiredService<MaintenanceAssetReturnState>();
            if(mode=="permission"){Assert.Null(state.Review);Assert.DoesNotContain("Observed capability percent for EQUIPMENT",root.ToHtmlString());return;}
            var html=System.Net.WebUtility.HtmlDecode(root.ToHtmlString());Assert.Contains("Check complete observed return",html);Assert.DoesNotContain("cannot be recorded from this screen",html);
            if(targets.Contains('F'))Assert.Contains("Observed capability percent for FACILITY plant gas",html);else{Assert.Contains("Installation parent (read only)",html);Assert.DoesNotContain("Observed capability percent for FACILITY",html);}
            if(targets.Contains('E'))Assert.Contains("Observed capability percent for EQUIPMENT pump",html);
            Set("_activation",DateTime.UtcNow.ToString("O"));Set("_reason","Actual complete observed return");
            var qualified=(Dictionary<(string Type,string Id,string? Qualifier),decimal?>)typeof(MaintenanceAssetReturnPanel).GetField("_qualifiedCapabilities",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel)!;
            if(mode!="missing-percent"){if(targets.Contains('F'))qualified[("FACILITY","plant","gas")]=0m;if(targets.Contains('E'))qualified[("EQUIPMENT","pump",null)]=mode=="zero"?0m:72.5m;}
            var wells=(Dictionary<string,decimal?>)typeof(MaintenanceAssetReturnPanel).GetField("_capabilities",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel)!;foreach(var well in review.Assets.Wells)wells[well.WellId]=65.5m;
            Render();await Task.Yield();if(mode!="no-preview")await Invoke("PreviewAsync");await Task.Yield();
            if(mode=="changed-input"){typeof(MaintenanceAssetReturnPanel).GetMethod("InputsChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null);Render();await Task.Yield();Assert.DoesNotContain("Reviewed qualified return transitions",root.ToHtmlString());}
            if(mode=="changed-source")
            {
                var current=review with{Physical=review.Physical with{Assignment=review.Physical.Assignment with{Stage=review.Physical.Assignment.Stage with{ProcessRevision=new string('F',64)}}}};current.Require();
                client.Setup(value=>value.ReviewMaintenanceAssetReturnAsync(review.Assets.Report.Query,It.IsAny<CancellationToken>())).ReturnsAsync(current);await state.ReviewAsync(client.Object);await Task.Yield();Assert.DoesNotContain("Reviewed qualified return transitions",root.ToHtmlString());
            }
            if(mode=="company-change"){company.Clear();await Task.Yield();Assert.DoesNotContain("Reviewed qualified return transitions",root.ToHtmlString());return;}
            if(mode=="conflicting-link")
            {
                nav.NavigateTo(nav.GetUriWithQueryParameters(new Dictionary<string,object?>{{"maintenanceAssetReturn",Guid.NewGuid().ToString("D")},{"assetReturnProcess","another-process"},{"assetReturnSubmission",review.Assets.Report.Query.SubmissionId}}));
                await panel.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Query",review.Assets.Report.Query}}));await Task.Yield();Assert.Contains("original return link does not match this report",root.ToHtmlString());
            }
            if(mode is "confirmed" or "zero" or "uncertain"){Assert.Contains("Reviewed qualified return transitions",root.ToHtmlString());Assert.Contains("RECORDED_TARGET",root.ToHtmlString());}
            Set("_confirm",true);Render();await Task.Yield();await Invoke("RecordAsync");await Task.Yield();
            if(mode is "confirmed" or "zero"){Assert.NotNull(state.Receipt);Assert.NotNull(original);Assert.Equal(targets.Contains('F')&&targets.Contains('E')?2:1,original.QualifiedAssets!.Count);Assert.Contains(original.QualifiedAssets,row=>row.ObservedCapabilityPercent==0m || row.ObservedCapabilityPercent==72.5m);Assert.Contains("Recorded native asset return",root.ToHtmlString());}
            else{Assert.Null(state.Receipt);if(mode=="uncertain"){Assert.NotNull(original);Assert.False(state.CanReview);Assert.NotNull(state.VisibleReference);}else Assert.Null(original);}
        });
        if(mode=="uncertain")
        {
            Assert.NotNull(original);client.Setup(value=>value.RecoverMaintenanceAssetReturnAsync(original.Reference(),It.IsAny<CancellationToken>())).ReturnsAsync(Saved(original));
            await using var fresh=Services(company,client.Object,fields.Object,nav).BuildServiceProvider();await using var freshRenderer=new HtmlRenderer(fresh,fresh.GetRequiredService<ILoggerFactory>());
            await freshRenderer.Dispatcher.InvokeAsync(async()=>
            {
                var root=await freshRenderer.RenderComponentAsync<MaintenanceAssetReturnPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Query",review.Assets.Report.Query}}));var panel=((Components)fresh.GetRequiredService<IComponentActivator>()).Panel;
                await (Task)typeof(MaintenanceAssetReturnPanel).GetMethod("RecoverAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;await Task.Yield();Assert.NotNull(fresh.GetRequiredService<MaintenanceAssetReturnState>().Receipt);Assert.Contains("Recorded native asset return",root.ToHtmlString());Assert.Contains("EQUIPMENT pump",root.ToHtmlString());
            });
        }
        if(mode is "confirmed" or "zero" or "uncertain")client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Once);
        else client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("confirmed")][InlineData("uncertain")][InlineData("field")][InlineData("missing-percent")][InlineData("non-utc")][InlineData("ambiguous-date")][InlineData("unconfirmed")][InlineData("late-field")]
    public async Task ActualRoleFormRequiresExplicitObservationsRetainsUrlBeforeTransportAndRecoversWithoutReplay(string mode)
    {
        var review=Review();using var company=Company(review);var nav=new Nav();var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"other":review.Assets.Context.FieldId);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        var reads=0;CompanyMaintenanceAssetReturnCommand? original=null;client.Setup(value=>value.ReviewMaintenanceAssetReturnAsync(review.Assets.Report.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult(review);});
        client.Setup(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceAssetReturnCommand command,CancellationToken _)=>
        {
            original=command;var query=Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri(nav.Uri).Query);Assert.Equal(command.IntentId.ToString("D"),query["maintenanceAssetReturn"].Single());Assert.Equal(command.Reference().Query.SubmissionId,query["assetReturnSubmission"].Single());Assert.Equal(command.Reference().Query.Task.InstanceId,query["assetReturnProcess"].Single());
            if(mode=="uncertain")throw new IOException("Acknowledgement unavailable");if(mode=="late-field")fields.Raise(value=>value.CurrentFieldChanged+=null!,"other");return Task.FromResult(Saved(command));
        });
        static Task Invoke(MaintenanceAssetReturnPanel panel,string method)=>(Task)panel.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;
        await using var provider=Services(company,client.Object,fields.Object,nav).BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<MaintenanceAssetReturnPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Query",review.Assets.Report.Query}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            await Invoke(panel,"ReviewAsync");await Task.Yield();if(mode=="field"){Assert.Equal(0,reads);Assert.Null(provider.GetRequiredService<MaintenanceAssetReturnState>().Review);return;}
            Assert.Contains("original-well",root.ToHtmlString());Assert.Contains("recorded-source",root.ToHtmlString());Assert.Contains("Observed capability percent",root.ToHtmlString());
            void Set(string name,object value)=>panel.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,value);
            Set("_activation",mode=="non-utc"?DateTime.UtcNow.ToString("s"):mode=="ambiguous-date"?"10/15/2026T12:00:00Z":DateTime.UtcNow.ToString("O"));Set("_reason","Actual independently observed return");Set("_confirm",mode!="unconfirmed");
            var observations=(Dictionary<string,decimal?>)panel.GetType().GetField("_capabilities",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel)!;if(mode!="missing-percent")observations["original-well"]=65.5m;
            typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null);await Task.Yield();await Invoke(panel,"RecordAsync");await Task.Yield();
            var state=provider.GetRequiredService<MaintenanceAssetReturnState>();if(mode=="confirmed"){Assert.NotNull(state.Receipt);Assert.Contains("Recorded native asset return",root.ToHtmlString());}
            else Assert.Null(state.Receipt);if(mode is "missing-percent" or "non-utc" or "ambiguous-date" or "unconfirmed")Assert.Null(original);
            if(mode is "confirmed" or "uncertain")Assert.NotNull(original);if(mode=="uncertain"){Assert.False(state.CanReview);Assert.DoesNotContain("Recorded native asset return",root.ToHtmlString());}
        });
        if(mode is "confirmed" or "uncertain")
        {
            client.Setup(value=>value.RecoverMaintenanceAssetReturnAsync(original!.Reference(),It.IsAny<CancellationToken>())).ReturnsAsync(Saved(original!));
            await using var fresh=Services(company,client.Object,fields.Object,nav).BuildServiceProvider();await using var freshRenderer=new HtmlRenderer(fresh,fresh.GetRequiredService<ILoggerFactory>());
            await freshRenderer.Dispatcher.InvokeAsync(async()=>{var root=await freshRenderer.RenderComponentAsync<MaintenanceAssetReturnPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Query",review.Assets.Report.Query}}));var panel=((Components)fresh.GetRequiredService<IComponentActivator>()).Panel;await Invoke(panel,"RecoverAsync");await Task.Yield();Assert.NotNull(fresh.GetRequiredService<MaintenanceAssetReturnState>().Receipt);Assert.Contains("Recorded native asset return",root.ToHtmlString());Assert.Single(Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri(nav.Uri).Query)["maintenanceAssetReturn"]);});
        }
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),mode is "confirmed" or "uncertain" or "late-field"?Times.Once():Times.Never());
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task ActualClosureParentExposesTargetDiscoveryAfterClosureWithoutAssumingWellPermission(bool hasWellCapability)
    {
        var review=Review();using var company=Company(review,hasWellCapability?"current":"WellManagement.UpdateWellStatus");var client=new Mock<ICompanyMaintenanceHandoffClient>();var query=review.Assets.Report.Query;var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(query.Task.FieldId);var nav=new Nav();
        client.Setup(value=>value.ReadMaintenanceClosureStatusAsync(query,It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyMaintenanceClosureStatus(review.Assets.Context,query,review.Assets.Closure));client.Setup(value=>value.ReviewMaintenanceAssetReturnAsync(query,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        await using var provider=Services(company,client.Object,fields.Object,nav).BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<MaintenanceClosurePanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Query",query},{"FieldId",query.Task.FieldId},{"WorkOrderId",query.Task.WorkOrderId}}));Assert.DoesNotContain("Return the affected assets to service",root.ToHtmlString());
            var components=(Components)provider.GetRequiredService<IComponentActivator>();await (Task)typeof(MaintenanceClosurePanel).GetMethod("ReadStatusAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Closure,null)!;await Task.Yield();Assert.Contains("Return the affected assets to service",root.ToHtmlString());
            await (Task)typeof(MaintenanceAssetReturnPanel).GetMethod("ReviewAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Panel,null)!;await Task.Yield();
            if(hasWellCapability){Assert.Contains("Observed capability percent",root.ToHtmlString());Assert.Contains("original-well",root.ToHtmlString());Assert.Contains("Original affected return scope",root.ToHtmlString());}
            else{Assert.Null(provider.GetRequiredService<MaintenanceAssetReturnState>().Review);Assert.DoesNotContain("Observed capability percent",root.ToHtmlString());}
        });
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
