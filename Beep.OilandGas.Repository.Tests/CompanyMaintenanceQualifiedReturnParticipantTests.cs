using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TheTechIdea.Data.OilGas;
using Xunit;
using ReviewFixture=Beep.OilandGas.Repository.Tests.CompanyMaintenanceQualifiedAssetTests.ReviewFixture;
namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CompanyMaintenanceQualifiedLicensingTests
{
    internal static async Task ConfigureQualifiedReturnApi(ReviewFixture f,CompanyMaintenanceAssetReturnCommand command)
    {
        f.Native.Db.Roles.Add(new IdentityRole{Id="qualified-return-role",Name="FacilityRole",NormalizedName="FACILITYROLE"});
        f.Native.Db.UserRoles.Add(new IdentityUserRole<string>{UserId=f.Context.ActorUserId,RoleId="qualified-return-role"});
        foreach(var permission in CompanyMaintenanceAssetReturnCapabilities.ForOrder(command.Review.Assets.Order))
            f.Native.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="qualified-return-role",ClaimType="permission",ClaimValue=permission});
        await f.Native.Db.SaveChangesAsync();await f.Native.Select(f.Context.Company.BusinessAssociateId,f.Context.ActorUserId);
        f.Native.EligibilityConfiguration.Companies.Single().TimeZoneId=command.Review.Assets.QualifiedAssets!.DateTimeZoneId;
    }
    internal static CompanyMaintenanceWellEvidenceReader QualifiedReturnApi(ReviewFixture f)
    {
        var tenant=new Beep.OilandGas.Repository.RepositoryTenantContext();tenant.SelectCompany(f.Native.Gate.Company);
        var modules=new ModuleConnectionResolver(f.Native.Db,f.Native.Editor.Object,tenant);
        var policies=new CompanyMaintenanceReturnPolicyService(f.Native.Gate,modules,f.Native.Db,f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>());
        var options=Options.Create(f.Native.EligibilityConfiguration);
        var qualified=new CompanyMaintenanceQualifiedAssetEvidenceReader(f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object,f.Native.Policy,options,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),policies);
        // Use the same interface injection as production; the optional argument must not disconnect the participant.
        using var services=new ServiceCollection().AddSingleton<ICompanyMaintenanceQualifiedAssetEvidenceOwner>(qualified).BuildServiceProvider();
        return ActivatorUtilities.CreateInstance<CompanyMaintenanceWellEvidenceReader>(services,f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object,f.Native.Policy,options);
    }
    internal static async Task QualifiedReturnApiAuthority(ReviewFixture f,CompanyMaintenanceAssetReturnCommand command,string? role,bool write=true)
    {
        await f.Guard();var access=await f.Native.Gate.RefreshCompanyAsync();f.Native.Gate.RequireActor(f.Context.ActorUserId);
        if(access.Scope!=f.Context.Company || write && (CompanyMaintenanceAssetReturnCapabilities.ForOrder(command.Review.Assets.Order).Any(permission=>!access.Permissions.Contains(permission,StringComparer.Ordinal)) || role is not null && !access.Roles.Contains(role,StringComparer.Ordinal)) || !access.Permissions.Contains("Maintenance.ReturnToService",StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Current original app return authority is required.");
    }
    [Theory][InlineData("equipment-only",false)][InlineData("current",true)]
    public async Task ActualApiAdaptersJoinCompleteNativeReturnAndSeparatePrivateRecoveryFromReadOnlyFacts(string mode,bool includeWell)
    {
        var ready=await QualifiedCommand(mode,includeWell);using var f=ready.Fixture;var command=ready.Command;
        await ConfigureQualifiedReturnApi(f,command);var api=QualifiedReturnApi(f);var writes=f.Native.Writes.Count;
        Task Guard(string? role)=>QualifiedReturnApiAuthority(f,command,role);
        var saved=await api.RecordMaintenanceAssetReturnAsync(command,Guard,_=>Task.FromResult(command.Review.Physical),_=>Task.FromResult<CompanyMaintenanceClosureFacts?>(command.Review.Assets.Closure));
        saved.Require(f.Context.Company,f.Context.ActorUserId,command.Reference());Assert.True(f.Native.Writes.Count>writes);Assert.Single(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());writes=f.Native.Writes.Count;
        var recovered=await api.RecoverMaintenanceAssetReturnAsync(f.Context,command.Reference(),Guard);Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(recovered));Assert.Equal(writes,f.Native.Writes.Count);
        // Keep the read capability through a different app role; the assigned return role is no longer configured.
        await f.Native.Grant("FACILITY","plant","READ",facilityType:"gas");await f.Native.Grant("EQUIPMENT","pump","READ");if(includeWell)await f.Native.Grant("WELL","well","READ");
        f.Native.Db.RoleClaims.RemoveRange(await f.Native.Db.RoleClaims.Where(row=>row.RoleId=="qualified-return-role").ToArrayAsync());
        f.Native.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ReturnToService"});
        await f.Native.Db.SaveChangesAsync();await f.Native.Select(f.Context.Company.BusinessAssociateId,f.Context.ActorUserId);
        var readApi=QualifiedReturnApi(f);var facts=await readApi.ReadMaintenanceAssetReturnFactsAsync(f.Context,command.Reference().Query,()=>QualifiedReturnApiAuthority(f,command,null,false));
        Assert.NotNull(facts);facts.Require();Assert.Equal(saved.Receipt.ReturnId,facts.ReturnId);Assert.NotEmpty(facts.QualifiedAssets!);Assert.Equal(writes,f.Native.Writes.Count);
        await Assert.ThrowsAnyAsync<Exception>(()=>readApi.RecoverMaintenanceAssetReturnAsync(f.Context,command.Reference(),role=>QualifiedReturnApiAuthority(f,command,role)));
        Assert.Equal(writes,f.Native.Writes.Count);
    }
    [Theory][InlineData("role-before")][InlineData("zone-before")][InlineData("role-after-write")][InlineData("zone-after-write")]
    public async Task ActualApiQualifiedParticipantRefusesChangedConfiguredRoleOrZoneAndRollsBackAllTargets(string mode)
    {
        var ready=await QualifiedCommand(includeWell:true);using var f=ready.Fixture;var command=ready.Command;await ConfigureQualifiedReturnApi(f,command);
        async Task Change()
        {
            if(mode.StartsWith("zone",StringComparison.Ordinal))f.Native.EligibilityConfiguration.Companies.Single().TimeZoneId="UTC";
            else
            {
                var permission=await f.Native.Db.RoleClaims.SingleAsync(row=>row.RoleId=="qualified-return-role" && row.ClaimValue=="Maintenance.ReturnToService");
                f.Native.Db.RoleClaims.Remove(permission);f.Native.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ReturnToService"});await f.Native.Db.SaveChangesAsync();
            }
        }
        if(mode.EndsWith("before",StringComparison.Ordinal)){await Change();await f.Native.Select(f.Context.Company.BusinessAssociateId,f.Context.ActorUserId);}
        var api=QualifiedReturnApi(f);var before=JsonSerializer.Serialize(f.Native.Rows);var writes=f.Native.Writes.Count;var changed=false;
        if(mode.EndsWith("after-write",StringComparison.Ordinal))f.Native.AfterLeaseWrite=_=>{if(!changed){changed=true;Change().GetAwaiter().GetResult();}};
        await Assert.ThrowsAnyAsync<Exception>(()=>api.RecordMaintenanceAssetReturnAsync(command,role=>QualifiedReturnApiAuthority(f,command,role),_=>Task.FromResult(command.Review.Physical),_=>Task.FromResult<CompanyMaintenanceClosureFacts?>(command.Review.Assets.Closure)));
        Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows));Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());
        if(mode.EndsWith("after-write",StringComparison.Ordinal)){Assert.True(changed);Assert.True(f.Native.Writes.Count>writes);}
        else Assert.Equal(writes,f.Native.Writes.Count);
    }
    // Canonical report/physical/closure DTO composition is a controlled source seam here.
    // Native statuses, policy, transaction, original intent and recovery are the real owners.
    private static CompanyWorkOrderReturnReview QualifiedPhysical(ReviewFixture f,CompanyMaintenanceQualifiedAssets sources,Action<WORK_ORDER_EXECUTION_SUBMISSION>? captureReport=null)
    {
        var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(f.Context.Company,Guid.NewGuid()),f.Order.FieldId,f.Order.InstanceId);
        var submitted=WorkOrderExecutionStartFixture.Submission(database:f.Context.Database,fieldId:f.Order.FieldId,originalOrder:f.Order,taskOverride:task,completionVerification:true,returnAcceptance:true,qualifiedWorkAssets:sources);
        var preparedReport=submitted.Command.Prepare(DateTime.UtcNow);captureReport?.Invoke(CompanyInventorySourceReview.Copy(preparedReport));var report=CompanyWorkOrderSubmissionFacts.From(preparedReport);
        var before=submitted.Command.Review.Inspections.Current;var utc=DateTime.UtcNow;var handoff=utc;
        var requirements=before.Requirements.Select(row=>row.ApplicableStage is "COMPLETION_VERIFICATION" or "RETURN_TO_SERVICE"?
            row with{Status="PASS",ResultText="Actual independent observation",Inspector=row.ApplicableStage=="RETURN_TO_SERVICE"?f.Context.ActorUserId:"completion-reviewer",InspectDate=DateTime.UtcNow,SourceRevision=new string('C',64)}:row).ToArray();
        var person=new CompanyNativePersonFacts(f.Context.Company,"return-person","Recorded independent return person","PERSON",utc.AddDays(-1),utc.AddDays(1),
            new(f.Context.Company.BusinessAssociateId,"return-person",1,"Return duty","ACTIVE",utc.AddDays(-1),utc.AddDays(1),new string('D',64)),new string('E',64));
        var link=new CompanyPersonnelIdentityVersion(f.Context.Company,f.Context.Database,CompanyPersonnelIdentityCommand.RecordIdFor(f.Context.Company,f.Context.ActorUserId,1),f.Context.ActorUserId,person.PersonBaId,"PERSON",1,utc.AddDays(-1),utc.AddDays(1),"Independent original mapping","identity-administrator",utc.AddMinutes(-1),new string('F',64));
        var verifierLink=new CompanyPersonnelIdentityVersion(f.Context.Company,f.Context.Database,CompanyPersonnelIdentityCommand.RecordIdFor(f.Context.Company,"completion-reviewer",1),"completion-reviewer","verification-person","PERSON",1,utc.AddDays(-1),utc.AddDays(1),"Independent original verifier mapping","identity-administrator",utc.AddMinutes(-1),new string('A',64));
        var catalog=CompanyPersonnelIdentityCatalog.Create(new(f.Context.Company,f.Context.Database,f.Context.ActorUserId,new string('B',64)),[link,verifierLink],DateTime.UtcNow);
        var compared=DateTime.UtcNow;
        var completion=new CompanyWorkOrderCompletionReview(f.Context,report,f.Order,requirements,catalog,person,compared,
            CompanyWorkOrderCompletionReview.GetBlockers(f.Context,report,f.Order,requirements,catalog,person,compared));
        var verification=new CompanyWorkOrderCompletionDecisionFacts(f.Context.Company,f.Context.Database,report.Query,"completion-reviewer","verification-person","SupervisorRole","VERIFIED",utc.AddTicks(-1),"verification-approval","verification-history",report.Revision,completion.SourceRevision(),new string('C',64)){WorkSubjectRevision=completion.WorkSubjectRevision()};
        var stage=new CompanyMaintenanceAssignedStageReview(f.Context,task,"return-step-instance","return","Independent return approval","FacilityRole","receiver","receiving-history","receiving-approval",new string('A',64),new string('B',64));
        var assignment=new CompanyMaintenanceReturnAssignment(stage,verification,"handoff-history",handoff,new string('C',64));
        var result=new CompanyWorkOrderReturnReview(assignment,completion,[person],CompanyWorkOrderReturnReview.GetBlockers(assignment,completion,[person]));
        Assert.Empty(result.Blockers);result.RequireAcceptance();return result;
    }
    internal static async Task<(ReviewFixture Fixture,WellServices Owner,CompanyMaintenanceAssetReturnCommand Command)> QualifiedCommand(string mode="current",bool includeWell=false,Action<WORK_ORDER_EXECUTION_SUBMISSION>? captureReport=null)
    {
        var ready=await ReadyQualifiedReturn(mode,completeCommand:true,includeWell:includeWell);var f=ready.Fixture;
        var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase))["WORK_ORDER_ASSET_RETURN"];
        f.Native.Metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_ASSET_RETURN")).ReturnsAsync(metadata);
        f.Native.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_ASSET_RETURN")).ReturnsAsync(["RETURN_ID"]);
        var owner=new WellServices(f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object,f.Context.Database.ConnectionName,f.Native.Policy,"Asia/Kuwait",f.Owner);
        var wells=new List<CompanyMaintenanceWellEvidence>();
        if(includeWell)
        {
            await f.Native.Grant("WELL","well","WRITE");var utc=DateTime.UtcNow;
            f.Native.Rows.Add(new WELL{UWI="well",ASSIGNED_FIELD=f.Context.FieldId,OPERATOR=f.Context.Company.BusinessAssociateId,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-30),EXPIRY_DATE=utc.AddYears(1)});
            foreach(var (facet,value) in new[]{("Life Cycle","Operating"),("Role","Produce"),("Condition","Shut In")})
            {
                f.Native.Rows.Add(new WELL_STATUS{UWI="well",SOURCE="recorded-source",STATUS_ID=facet+"-original",STATUS_TYPE=facet,STATUS=value,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-1),TIMEZONE="UTC"});
                f.Native.Rows.Add(new R_WELL_STATUS{STATUS_TYPE=facet,STATUS=value,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});
            }
            f.Native.Rows.Add(new R_WELL_STATUS{STATUS_TYPE="Condition",STATUS="Active",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});
            wells.Add(await owner.ReadMaintenanceWellEvidenceAsync(f.Context,"well",f.Guard));
        }
        var physical=QualifiedPhysical(f,ready.Sources,captureReport);var closure=new CompanyMaintenanceClosureFacts(f.Context.Company,f.Context.Database,physical.Completion.Report.Query,f.Context.ActorUserId,"FacilityRole","closure-history",["closed-task"],DateTime.UtcNow,new string('D',64),physical.PhysicalSubjectRevision(),physical.Completion.Report.Revision,new string('E',64));
        var compared=DateTime.UtcNow;physical=physical with{Completion=physical.Completion with{ComparedAtUtc=compared}};physical.RequireAcceptance();
        var assets=new CompanyMaintenanceAssetEvidence(f.Context,physical.Completion.Report,closure,f.Order,wells,compared,
            CompanyMaintenanceAssetEvidence.GetBlockers(f.Context,physical.Completion.Report,closure,f.Order,wells,compared,ready.Sources)){QualifiedAssets=ready.Sources};
        var observations=mode=="facility-only"?new[]{QualifiedObservations()[0]}:mode=="equipment-only"?new[]{QualifiedObservations()[1]}:QualifiedObservations();
        var command=new CompanyMaintenanceAssetReturnCommand(new(assets,physical),Guid.NewGuid(),DateTime.UtcNow,wells.Select(row=>new CompanyWellReturnActivation(row.WellId,"recorded-source",61m)).ToArray(),"Original observed mixed return",true){QualifiedAssets=observations};
        command.Require();return(f,owner,command);
    }
    private static Task<CompanyMaintenanceAssetReturnRecovery> RecordQualified(WellServices owner,CompanyMaintenanceAssetReturnCommand command,Func<string?,Task> authority)=>
        owner.RecordMaintenanceAssetReturnAsync(command,authority,_=>Task.FromResult(command.Review.Physical),_=>Task.FromResult<CompanyMaintenanceClosureFacts?>(command.Review.Assets.Closure));

    [Theory][InlineData("current",false)][InlineData("facility-only",false)][InlineData("equipment-only",false)][InlineData("current",true)]
    public async Task CompleteQualifiedReturnUsesOneOriginalIntentCommonAuditAndReadOnlyRecovery(string mode,bool includeWell)
    {
        var ready=await QualifiedCommand(mode,includeWell);using var f=ready.Fixture;var command=ready.Command;var writes=f.Native.Writes.Count;
        var saved=await RecordQualified(ready.Owner,command,_=>f.Guard());saved.Require(f.Context.Company,f.Context.ActorUserId,command.Reference());
        var changes=CompanyMaintenanceQualifiedAssetReturn.Resolve(command.Review.Assets.QualifiedAssets!,f.Order,command.QualifiedAssets!,"FacilityRole",command.ActivatedAtUtc);
        Assert.Equal(writes+2*(changes.Count+command.Wells.Count)+1,f.Native.Writes.Count);var row=Assert.Single(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());
        Assert.Equal(saved.Receipt.RecordedAtUtc,row.RECORDED_AT_UTC);Assert.Contains("QualifiedProof",row.BEFORE_JSON);Assert.Contains("QualifiedProof",row.AFTER_JSON);
        writes=f.Native.Writes.Count;var repeated=await RecordQualified(ready.Owner,command,_=>f.Guard());Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(repeated));
        var recovered=await ready.Owner.RecoverMaintenanceAssetReturnAsync(f.Context,command.Reference(),_=>f.Guard());Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(recovered));
        foreach(var change in changes)
        {
            var native=f.Native.Rows.Single(value=>value.GetType().Name==(change.AssetType=="FACILITY"?"FACILITY_STATUS":"EQUIPMENT_STATUS") && value.GetType().GetProperty(change.AssetType=="FACILITY"?"STATUS_ID":"EQUIP_STATUS_ID")!.GetValue(value) as string==change.StatusId(command.ReturnId()));
            Assert.Equal(saved.Receipt.RecordedAtUtc,native.GetType().GetProperty("ROW_CREATED_DATE")!.GetValue(native));
        }
        await f.Native.Grant("FACILITY","plant","READ",facilityType:"gas");if(mode!="facility-only")await f.Native.Grant("EQUIPMENT","pump","READ");if(includeWell)await f.Native.Grant("WELL","well","READ");
        var facts=await ready.Owner.ReadMaintenanceAssetReturnFactsAsync(f.Context,command.Reference().Query,f.Guard);Assert.NotNull(facts);facts.Require();Assert.Equal(changes.Count,facts.QualifiedAssets!.Count);
        var body=JsonSerializer.Serialize(facts);Assert.DoesNotContain(command.Reason,body);Assert.DoesNotContain(command.IntentId.ToString("D"),body);Assert.DoesNotContain("QualifiedProof",body);
        await Assert.ThrowsAnyAsync<Exception>(()=>ready.Owner.RecoverMaintenanceAssetReturnAsync(f.Context,command.Reference(),_=>f.Guard()));Assert.Equal(writes,f.Native.Writes.Count);
    }

    [Theory][InlineData("intent")][InlineData("qualified-insert")][InlineData("revoked")][InlineData("readback")][InlineData("physical")]
    public async Task CompleteMixedReturnRollsBackWellQualifiedTargetsAndOriginalIntentTogether(string mode)
    {
        var ready=await QualifiedCommand(includeWell:true);using var f=ready.Fixture;var before=JsonSerializer.Serialize(f.Native.Rows);var writes=f.Native.Writes.Count;
        if(mode=="intent")f.Native.LeaseFailWriteNumber=writes+7;if(mode=="qualified-insert")f.Native.LeaseFailWriteNumber=writes+6;
        f.Native.AfterLeaseWrite=row=>{if(mode=="revoked" && row is EQUIPMENT_STATUS)f.Revoked=true;if(mode=="readback" && row is EQUIPMENT_STATUS status && status.ACTIVE_IND=="Y")status.PERCENT_CAPABILITY=99m;};
        await Assert.ThrowsAnyAsync<Exception>(()=>ready.Owner.RecordMaintenanceAssetReturnAsync(ready.Command,_=>f.Guard(),
            owned=>{if(mode=="physical" && owned && f.Native.Writes.Count>writes)throw new InvalidOperationException("Physical proof changed after native effects.");return Task.FromResult(ready.Command.Review.Physical);},
            _=>Task.FromResult<CompanyMaintenanceClosureFacts?>(ready.Command.Review.Assets.Closure)));
        Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows));Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());
    }

    [Theory][InlineData("facility-effect")][InlineData("equipment-effect")][InlineData("well-effect")]
    [InlineData("licensing")][InlineData("zone")][InlineData("policy")][InlineData("owner")]
    [InlineData("installation")][InlineData("grant")][InlineData("intent-body")]
    public async Task FinalOwnedReceiptRejectsLateProofChangesAndRollsBackTheCompleteUncommittedReturn(string fault)
    {
        var ready=await QualifiedCommand(includeWell:true);using var f=ready.Fixture;
        bool External(object row)=>fault switch
        {
            "licensing"=>row is FACILITY_LICENSE,
            "zone"=>row is R_TIMEZONE{TIMEZONE:"UTC"},
            "policy"=>row is WORK_ORDER_MAINTENANCE_RETURN_POLICY,
            "owner"=>row is EQUIPMENT{EQUIPMENT_ID:"pump"},
            "installation"=>row is FACILITY_EQUIPMENT,
            _=>false
        };
        var before=JsonSerializer.Serialize(f.Native.Rows.Where(row=>!External(row)));var writes=f.Native.Writes.Count;var changed=false;string? externalAfter=null;
        f.Native.AfterLeaseWrite=row=>
        {
            if(row is not WORK_ORDER_ASSET_RETURN intent || changed)return;
            if(fault=="facility-effect")f.Native.Rows.OfType<FACILITY_STATUS>().Single(value=>value.ACTIVE_IND=="Y").PERCENT_CAPABILITY=99m;
            if(fault=="equipment-effect")f.Native.Rows.OfType<EQUIPMENT_STATUS>().Single(value=>value.ACTIVE_IND=="Y").PERCENT_CAPABILITY=99m;
            if(fault=="well-effect")f.Native.Rows.OfType<WELL_STATUS>().Single(value=>value.UWI=="well" && value.STATUS_ID==ready.Command.StatusId("well")).PERCENT_CAPABILITY=99m;
            if(fault=="licensing")f.Native.Rows.OfType<FACILITY_LICENSE>().Single().FEES_PAID_IND="N";
            if(fault=="zone")f.Native.Rows.OfType<R_TIMEZONE>().Single(value=>value.TIMEZONE=="UTC").ROW_CHANGED_BY="late-reference-change";
            if(fault=="policy")f.Native.Rows.OfType<WORK_ORDER_MAINTENANCE_RETURN_POLICY>().Single().POLICY_JSON="{}";
            if(fault=="owner")f.Native.Rows.OfType<EQUIPMENT>().Single(value=>value.EQUIPMENT_ID=="pump").CURRENT_OWNER_BA_ID="other-company";
            if(fault=="installation")f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single().INSTALL_OBS_NO=8;
            if(fault=="grant")
            {
                var grant=f.Native.Db.Set<AppUserAssetAccess>().Single(value=>value.AssetType=="EQUIPMENT");
                grant.IsActive=false;f.Native.Db.SaveChanges();
            }
            if(fault=="intent-body")intent.COMMAND_JSON="{}";
            externalAfter=JsonSerializer.Serialize(f.Native.Rows.Where(External));
            changed=true;
        };
        await Assert.ThrowsAnyAsync<Exception>(()=>RecordQualified(ready.Owner,ready.Command,_=>f.Guard()));
        Assert.True(changed);Assert.True(f.Native.Writes.Count>writes);
        Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows.Where(row=>!External(row))));Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());
        // Independently injected source changes are not effects owned by the refused return transaction.
        Assert.Equal(externalAfter,JsonSerializer.Serialize(f.Native.Rows.Where(External)));
        if(fault=="grant")Assert.False((await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(value=>value.AssetType=="EQUIPMENT")).IsActive);
    }

    [Theory][InlineData("licensing",true)][InlineData("zone",true)][InlineData("owner",true)]
    [InlineData("licensing",false)][InlineData("zone",false)][InlineData("owner",false)]
    public async Task RetainedQualifiedProofClosesCurrentSourcesAfterReadingTheExactPostimage(string fault,bool privateRead)
    {
        var ready=await QualifiedCommand();using var f=ready.Fixture;
        var saved=await RecordQualified(ready.Owner,ready.Command,_=>f.Guard());var writes=f.Native.Writes.Count;var changed=false;
        f.Native.AfterRead=()=>
        {
            var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");
            if(changed || call.Arguments[0] as string!="FACILITY_STATUS")return Task.CompletedTask;
            if(fault=="licensing")f.Native.Rows.OfType<FACILITY_LICENSE>().Single().FEES_PAID_IND="N";
            if(fault=="zone")f.Native.Rows.OfType<R_TIMEZONE>().Single(value=>value.TIMEZONE=="UTC").ROW_CHANGED_BY="late-reference-change";
            if(fault=="owner")f.Native.Rows.OfType<EQUIPMENT>().Single(value=>value.EQUIPMENT_ID=="pump").CURRENT_OWNER_BA_ID="other-company";
            changed=true;
            return Task.CompletedTask;
        };
        if(privateRead)await Assert.ThrowsAnyAsync<Exception>(()=>ready.Owner.RecoverMaintenanceAssetReturnAsync(f.Context,ready.Command.Reference(),_=>f.Guard()));
        else await Assert.ThrowsAnyAsync<Exception>(()=>ready.Owner.ReadMaintenanceAssetReturnFactsAsync(f.Context,ready.Command.Reference().Query,f.Guard));
        Assert.True(changed);Assert.Equal(writes,f.Native.Writes.Count);
        Assert.Equal(saved.Receipt.ReturnId,Assert.Single(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>()).RETURN_ID);
    }

    [Theory][InlineData(false)][InlineData(true)]
    public async Task CompleteMixedReturnConfirmsTheOriginalOutcomeAfterCommitAcknowledgementFailure(bool committed)
    {
        var ready=await QualifiedCommand(includeWell:true);using var f=ready.Fixture;var before=JsonSerializer.Serialize(f.Native.Rows);
        f.Native.LeaseCommitFailed=true;f.Native.LeaseCommitAfterFailure=committed;
        await Assert.ThrowsAnyAsync<Exception>(()=>RecordQualified(ready.Owner,ready.Command,_=>f.Guard()));
        f.Native.LeaseCommitFailed=false;var writes=f.Native.Writes.Count;
        if(committed)
        {
            Assert.Single(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());
            var recovered=await ready.Owner.RecoverMaintenanceAssetReturnAsync(f.Context,ready.Command.Reference(),_=>f.Guard());
            var repeated=await RecordQualified(ready.Owner,ready.Command,_=>f.Guard());Assert.Equal(JsonSerializer.Serialize(recovered),JsonSerializer.Serialize(repeated));
        }
        else
        {
            Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows));
            await Assert.ThrowsAnyAsync<Exception>(()=>ready.Owner.RecoverMaintenanceAssetReturnAsync(f.Context,ready.Command.Reference(),_=>f.Guard()));
        }
        Assert.Equal(writes,f.Native.Writes.Count);
    }

    [Fact]
    public async Task CompleteQualifiedReturnRequiresItsParticipantAndRejectsForgedFrozenProofWithoutNativeWrites()
    {
        var ready=await QualifiedCommand();using var f=ready.Fixture;var writes=f.Native.Writes.Count;
        var legacy=new WellServices(f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object,f.Context.Database.ConnectionName,f.Native.Policy,"Asia/Kuwait");
        await Assert.ThrowsAnyAsync<Exception>(()=>RecordQualified(legacy,ready.Command,_=>f.Guard()));
        await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            var proof=await f.Owner.PrepareMaintenanceQualifiedReturnAsync(ready.Command,f.Guard);
            foreach(var fault in new[]{"target","preimage","zone","audit"})
            {
                var body=System.Text.Json.Nodes.JsonNode.Parse(proof.Body)!;
                if(fault=="target")body["AfterEquipment"]![1]!["PERCENT_CAPABILITY"]=99m;
                if(fault=="preimage")body["BeforeFacilities"]![0]!["PERCENT_CAPABILITY"]=0m;
                if(fault=="zone")body["TimeZoneReferences"]![0]!["REMARK"]="Changed native reference";
                if(fault=="audit")body["RecordedAtUtc"]=proof.RecordedAtUtc.AddTicks(-1);
                Assert.ThrowsAny<Exception>(()=>f.Owner.RequireMaintenanceQualifiedReturnProof(ready.Command,proof with{Body=body.ToJsonString()}));
            }
            Assert.ThrowsAny<Exception>(()=>f.Owner.RequireMaintenanceQualifiedReturnProof(ready.Command with{IntentId=Guid.NewGuid(),Reason="Changed original reason"},proof));
        });
        Assert.Equal(writes,f.Native.Writes.Count);Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());
    }

    [Fact]
    public async Task CompleteQualifiedReturnRecoveryRetainsOriginalPolicyAfterALaterValidAppend()
    {
        var ready=await QualifiedCommand();using var f=ready.Fixture;var saved=await RecordQualified(ready.Owner,ready.Command,_=>f.Guard());
        var context=ready.Command.Review.Assets.QualifiedAssets!.ReturnPolicy!.Catalog.Context;
        var catalog=await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard);
        var previous=catalog.Versions.Single().Policy;var next=ApplicablePolicy() with{EffectiveFromUtc=previous.EffectiveToUtc,EffectiveToUtc=previous.EffectiveToUtc.AddDays(30)};
        await f.Owner.RecordMaintenanceReturnPolicyAsync(new(context,catalog.Revision,next,2,Guid.NewGuid(),"Later reviewed policy version"),f.Guard);
        var writes=f.Native.Writes.Count;var recovered=await ready.Owner.RecoverMaintenanceAssetReturnAsync(f.Context,ready.Command.Reference(),_=>f.Guard());
        Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(recovered));Assert.Equal(writes,f.Native.Writes.Count);
    }
}
