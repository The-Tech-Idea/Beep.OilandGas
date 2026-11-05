using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.ProductionOperations.Services;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Xunit;
using ReviewFixture=Beep.OilandGas.Repository.Tests.CompanyMaintenanceQualifiedAssetTests.ReviewFixture;
namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyMaintenanceReturnPolicyTests
{
    internal static CompanyMaintenanceReturnPolicyDraft Draft(DateTime utc)=>new("CORRECTIVE","gas","Explicit jurisdiction / Ã™â€¦Ã™â€ Ã˜Â·Ã™â€šÃ˜Â© Ã˜ÂªÃ˜Â´Ã˜ÂºÃ™Å Ã™â€ž","recorded-area","RecordedRegion",["FacilityRole"],[new("FACILITY","NativeCondition",["INACTIVE"],"RECORDED_TARGET","recorded-source","Reviewed transition source"),new("EQUIPMENT","NativeCondition",["IDLE"],"RECORDED_TARGET","recorded-source","Reviewed equipment source")],[new("issuer","RecordedType","REQUIRED","REQUIRED_PAID",[new("RecordedDecision",["RECORDED_APPROVAL"],"Reviewed decision source")],[new("Inspection","RecordedCheck","FULFILLED_ONLY","RecordedDue",null,null,new("EXACT",0,"value-unit","Reviewed zero term"),new("NOT_APPLICABLE",null,null,"Reviewed due term applicability"),"NO_RECORDED_RESTRICTION",null,null,null,null,"Reviewed condition source")],[new("RecordedViolation",["RecordedResolution"],"Reviewed resolution source")],"Reviewed licence applicability")],utc.AddDays(-1),utc.AddDays(30),"Reviewed company policy source / Ã™â€¦Ã˜Â±Ã˜Â¬Ã˜Â¹");
    private static CompanyMaintenanceReturnPolicyContext Context(ReviewFixture f)=>new(f.Context.Company,f.Context.Database,"actor",new string('A',64));
    private static void Schema(ReviewFixture f,Func<EntityStructure,EntityStructure>? alter=null)
    {
        var connection=new Mock<IDataConnection>();connection.SetupGet(value=>value.ConnectionProp).Returns(f.Native.Source.Object.Dataconnection.ConnectionProp);connection.As<IBeepConnectionEvidence>().SetupGet(value=>value.DbConn).Returns(f.Native.LeasePhysicalConnection.Object);f.Native.Source.SetupGet(value=>value.Dataconnection).Returns(connection.Object);
        var entry=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase))["WORK_ORDER_MAINTENANCE_RETURN_POLICY"];
        f.Native.Metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_MAINTENANCE_RETURN_POLICY")).ReturnsAsync(entry);f.Native.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_MAINTENANCE_RETURN_POLICY")).ReturnsAsync(["POLICY_RECORD_ID"]);
        f.Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure _,bool __)=>{var shape=f.Native.Editor.Object.classCreator.ConvertToEntityStructure(typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY));return alter?.Invoke(shape)??shape;});
    }
    [Theory][InlineData("record")][InlineData("unknown")][InlineData("write")][InlineData("readback")][InlineData("authority")][InlineData("missing-schema")][InlineData("key")][InlineData("capacity")][InlineData("isolation")][InlineData("binding")][InlineData("changed-schema")]
    public async Task NativeVersionAppendIsOwnedAtomicImmutableAndRecoverableFromItsOriginalIntent(string mode)
    {
        using var f=await ReviewFixture.Create();f.Native.EnableCorrectiveActionTransactions(cloneReads:false);var context=Context(f);var utc=DateTime.UtcNow;var reads=0;
        Schema(f,shape=>{if(mode=="key")shape.Fields.Single(row=>row.FieldName=="POLICY_RECORD_ID").IsKey=false;if(mode=="capacity")shape.Fields.Single(row=>row.FieldName=="POLICY_JSON").Size1=10;if(mode=="changed-schema" && reads>0)shape.Fields.RemoveAll(row=>row.FieldName=="COMMAND_JSON");return shape;});
        if(mode=="missing-schema")f.SchemaMissing=true;if(mode=="authority")f.Revoked=true;if(mode=="isolation")f.Native.TransactionIsolation=IsolationLevel.ReadCommitted;if(mode=="write")f.Native.LeaseFailWriteNumber=1;if(mode=="unknown"){f.Native.LeaseCommitFailed=true;f.Native.LeaseCommitAfterFailure=true;}if(mode=="binding")context=context with{Database=context.Database with{ConnectionName="other-source"}};
        f.Native.AfterRead=()=>{reads++;if(mode=="readback" && f.Native.Rows.OfType<WORK_ORDER_MAINTENANCE_RETURN_POLICY>().SingleOrDefault() is {} row)row.POLICY_JSON="corrupt original policy";return Task.CompletedTask;};
        var catalog=CompanyMaintenanceReturnPolicyCatalog.Create(context,[],utc);var command=new CompanyMaintenanceReturnPolicyCommand(context,catalog.Revision,Draft(utc),1,Guid.NewGuid(),"Explicit original maintenance policy rationale");command.RequireCatalog(catalog);
        var failure=await Record.ExceptionAsync(()=>f.Owner.RecordMaintenanceReturnPolicyAsync(command,f.Guard));
        if(mode=="record")Assert.Null(failure);else Assert.NotNull(failure);
        if(mode is "record" or "unknown")
        {
            var stored=Assert.Single(f.Native.Rows.OfType<WORK_ORDER_MAINTENANCE_RETURN_POLICY>());command.RequireRecorded(stored);var original=JsonSerializer.Serialize(stored);var writes=f.Native.Writes.Count;f.Native.AfterRead=null;
            var recovered=await f.Owner.RecordMaintenanceReturnPolicyAsync(command,f.Guard,true);Assert.Equal(original,JsonSerializer.Serialize(recovered));var reference=new CompanyMaintenanceReturnPolicyReference(command.RecordId(),command.IntentId);var envelope=await f.Owner.RecoverMaintenanceReturnPolicyReferenceAsync(context,reference,f.Guard);envelope.Require(context.Company,context.ActorUserId,reference);Assert.Equal(original,JsonSerializer.Serialize(envelope.Recorded));Assert.Equal(writes,f.Native.Writes.Count);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.RecordMaintenanceReturnPolicyAsync(command with{IntentId=Guid.NewGuid()},f.Guard));Assert.Equal(writes,f.Native.Writes.Count);
            var history=await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard);Assert.Single(history.Versions);Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(history));Assert.DoesNotContain(command.IntentId.ToString("D"),JsonSerializer.Serialize(history));
            if(mode=="record")
            {
                var next=new CompanyMaintenanceReturnPolicyCommand(context,history.Revision,command.Policy with{EffectiveFromUtc=command.Policy.EffectiveToUtc,EffectiveToUtc=command.Policy.EffectiveToUtc.AddDays(30)},2,Guid.NewGuid(),"Explicit next version");await f.Owner.RecordMaintenanceReturnPolicyAsync(next,f.Guard);Assert.Equal(original,JsonSerializer.Serialize(f.Native.Rows.OfType<WORK_ORDER_MAINTENANCE_RETURN_POLICY>().Single(row=>row.VERSION_NO==1)));Assert.Equal(2,(await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard)).Versions.Count);
                var current=await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.RecordMaintenanceReturnPolicyAsync(next with{ExpectedCatalogRevision=current.Revision,Version=3,IntentId=Guid.NewGuid()},f.Guard));Assert.Equal(writes+1,f.Native.Writes.Count);
            }
        }
        else
        {
            Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_MAINTENANCE_RETURN_POLICY>());if(mode is "write" or "readback" or "isolation")Assert.Contains("rollback",f.Native.LeaseTransactions);else Assert.Empty(f.Native.LeaseTransactions);
        }
        Assert.DoesNotContain(f.Native.Source.Invocations,call=>call.Method.Name is "CreateEntityAs" or "ExecuteSql" or "RunScript");
    }
    [Theory][InlineData("company")][InlineData("duplicate")][InlineData("body")][InlineData("maker-before-body")]
    public async Task PolicyHistoryCannotAdmitForeignAmbiguousOrModifiedPrivateRecords(string mode)
    {
        using var f=await ReviewFixture.Create();Schema(f);var context=Context(f);var utc=DateTime.UtcNow;var catalog=CompanyMaintenanceReturnPolicyCatalog.Create(context,[],utc);var command=new CompanyMaintenanceReturnPolicyCommand(context,catalog.Revision,Draft(utc),1,Guid.NewGuid(),"Original rationale");var row=command.Prepare(utc);f.Native.Rows.Add(row);
        if(mode=="company"){row.OWNER_COMPANY_BA_ID="other-company";f.Native.IgnoreFilters=true;}if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(row));if(mode is "body" or "maker-before-body")row.COMMAND_JSON="invalid private proof";
        if(mode=="maker-before-body")await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Owner.RecoverMaintenanceReturnPolicyReferenceAsync(context with{ActorUserId="another-maker"},new(command.RecordId(),command.IntentId),f.Guard));else await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard));Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("roles")][InlineData("target")][InlineData("duplicate-transition")][InlineData("duplicate-licence")][InlineData("term-null")][InlineData("term-nonapplicable")][InlineData("restriction-version")][InlineData("local-date")][InlineData("empty-status")]
    public void SharedPolicyRequiresExplicitBoundedChoicesWithoutGuessedStatusOrNumericDefaults(string mode)
    {
        var policy=Draft(DateTime.UtcNow);var license=policy.Licenses[0];var condition=license.Conditions[0];
        policy=mode switch{"roles"=>policy with{AuthorizedReturnRoles=[]},"target"=>policy with{Transitions=[policy.Transitions[0] with{TargetValue="INACTIVE"}]},"duplicate-transition"=>policy with{Transitions=[policy.Transitions[0],policy.Transitions[0]]},"duplicate-licence"=>policy with{Licenses=[license,license]},"term-null"=>policy with{Licenses=[license with{Conditions=[condition with{Value=condition.Value with{Value=null}}]}]},"term-nonapplicable"=>policy with{Licenses=[license with{Conditions=[condition with{DueTerm=condition.DueTerm with{Value=0}}]}]},"restriction-version"=>policy with{Licenses=[license with{Conditions=[condition with{RestrictionDecision="EXACT_VERSION",RestrictionId="restriction",RestrictionSourceDocumentId="source-document",RestrictionInterpretation="Explicit reviewed restriction interpretation"}]}]},"local-date"=>policy with{EffectiveFromUtc=DateTime.Now},_=>policy with{Licenses=[license with{Statuses=[]}]}};
        Assert.ThrowsAny<Exception>(policy.Require);
    }
    [Fact]
    public async Task GenericNativeMutationAndAppRowPolicyCannotWritePolicyIntentOrExposeAWorkflowOverride()
    {
        using var f=await ReviewFixture.Create();Schema(f);var context=Context(f);var utc=DateTime.UtcNow;var command=new CompanyMaintenanceReturnPolicyCommand(context,CompanyMaintenanceReturnPolicyCatalog.Create(context,[],utc).Revision,Draft(utc),1,Guid.NewGuid(),"Explicit original rationale");var row=command.Prepare(utc);
        var generic=new PPDMGenericRepository(f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object,typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY),context.Database.ConnectionName,"WORK_ORDER_MAINTENANCE_RETURN_POLICY");await Assert.ThrowsAnyAsync<Exception>(()=>generic.InsertAsync(row,"actor"));await Assert.ThrowsAnyAsync<Exception>(()=>f.Native.Policy.ValidateMutationAsync(typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY),"WORK_ORDER_MAINTENANCE_RETURN_POLICY",row,null,PPDMRowMutation.Insert,"actor"));Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public void PolicyUsesTheCanonicalExplicitBeepCoreManifestAndRejectsMetadataCollisionBeforeRegistration()
    {
        var type=typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY);Assert.Contains(type,Beep.OilandGas.Models.Data.WorkOrder.WorkOrderSchema.EntityTypes);var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase));Assert.Equal(type.FullName,metadata[type.Name].EntityTypeName);Assert.Equal("PPDM_CORE",metadata[type.Name].Module);var collision=new Dictionary<string,Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"work_order_maintenance_return_policy",new(){PrimaryKeyColumn="foreign"}}};Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionMetadata.AddTo(collision));Assert.Single(collision);
    }
    [Fact]
    public async Task ExistingAppPermissionCatalogExposesPolicyConfigurationWithoutGrantingItAutomatically()
    {
        using var f=await ReviewFixture.Create();var services=f.Native.Request.RequestServices;var roles=new RepositoryRoleAssignmentService(f.Native.Db,services.GetRequiredService<UserManager<OilGasUser>>(),services.GetRequiredService<RoleManager<IdentityRole>>(),NullLogger<RepositoryRoleAssignmentService>.Instance);var catalog=await roles.GetPermissionCatalogAsync();Assert.Single(catalog,row=>row.PermissionKey==PermissionConstants.Maintenance.ConfigureReturnPolicy);Assert.DoesNotContain(f.Native.Db.RoleClaims,row=>row.ClaimValue==PermissionConstants.Maintenance.ConfigureReturnPolicy);Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData(DateTimeKind.Unspecified)][InlineData(DateTimeKind.Local)]
    public async Task DeclaredPolicyUtcStoragePreservesItsClockWithoutUsingTheMachineZone(DateTimeKind kind)
    {
        using var f=await ReviewFixture.Create();Schema(f);var context=Context(f);var utc=DateTime.UtcNow;var command=new CompanyMaintenanceReturnPolicyCommand(context,CompanyMaintenanceReturnPolicyCatalog.Create(context,[],utc).Revision,Draft(utc),1,Guid.NewGuid(),"Original UTC policy rationale");var row=command.Prepare(utc);row.EFFECTIVE_FROM_UTC=DateTime.SpecifyKind(row.EFFECTIVE_FROM_UTC,kind);row.EFFECTIVE_TO_UTC=DateTime.SpecifyKind(row.EFFECTIVE_TO_UTC,kind);row.ROW_CREATED_DATE=DateTime.SpecifyKind(row.ROW_CREATED_DATE!.Value,kind);row.ROW_CHANGED_DATE=DateTime.SpecifyKind(row.ROW_CHANGED_DATE!.Value,kind);f.Native.Rows.Add(row);var before=JsonSerializer.Serialize(row);
        if(kind==DateTimeKind.Local)await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard));
        else{var history=await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard);Assert.Equal(command.Policy.EffectiveFromUtc,Assert.Single(history.Versions).Policy.EffectiveFromUtc);var reference=new CompanyMaintenanceReturnPolicyReference(command.RecordId(),command.IntentId);var recovered=await f.Owner.RecoverMaintenanceReturnPolicyReferenceAsync(context,reference,f.Guard);command.RequireRecorded(recovered.Recorded);Assert.Equal(DateTimeKind.Utc,recovered.Recorded.ROW_CREATED_DATE!.Value.Kind);Assert.Equal(utc,recovered.Recorded.ROW_CREATED_DATE);}
        Assert.Equal(before,JsonSerializer.Serialize(row));Assert.Empty(f.Native.Writes);Assert.Empty(f.Native.LeaseTransactions);
    }
    [Fact]
    public void OptionalPolicyScopeAndPreservationChoicesKeepEarlierRecordedCommandBodiesUnchanged()
    {
        var utc=DateTime.UtcNow;var context=new CompanyMaintenanceReturnPolicyContext(new("tenant","company"),new("core","fingerprint"),"maker",new string('A',64));var policy=Draft(utc);var empty=CompanyMaintenanceReturnPolicyCatalog.Create(context,[],utc);var command=new CompanyMaintenanceReturnPolicyCommand(context,empty.Revision,policy,1,Guid.NewGuid(),"Original policy rationale");var row=command.Prepare(utc);Assert.DoesNotContain("AreaApplicability",row.COMMAND_JSON);Assert.DoesNotContain("Disposition",row.COMMAND_JSON);Assert.DoesNotContain("AcceptedFulfilledByBaIds",row.COMMAND_JSON);JsonSerializer.Deserialize<CompanyMaintenanceReturnPolicyCommand>(row.COMMAND_JSON)!.RequireRecorded(row);
        var preserved=policy.Transitions[0] with{Disposition="PRESERVE",TargetValue="",NativeSource=""};preserved.Require();Assert.Throws<ArgumentException>(()=>(preserved with{TargetValue="replacement"}).Require());var nonApplicable=policy with{AreaApplicability="NOT_APPLICABLE",Transitions=[],Licenses=[]};nonApplicable.Require();Assert.Throws<ArgumentException>(()=>(nonApplicable with{Transitions=policy.Transitions}).Require());
    }

    [Fact]
    public async Task PolicyCatalogCanJoinTheOriginalOwnedCommandWithoutStartingAnotherTransaction()
    {
        using var f=await ReviewFixture.Create();f.Native.EnableCorrectiveActionTransactions();Schema(f);var context=Context(f);
        var empty=await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard);
        var command=new CompanyMaintenanceReturnPolicyCommand(context,empty.Revision,Draft(DateTime.UtcNow),1,Guid.NewGuid(),"Reviewed original policy");
        await f.Owner.RecordMaintenanceReturnPolicyAsync(command,f.Guard);
        var original=await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard);var writes=f.Native.Writes.Count;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard,true));
        await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard));
            var current=await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard,true);
            Assert.Equal(original.Revision,current.Revision);Assert.Single(current.Versions);
            PPDMDataSourceTransaction.RequireOwned(f.Native.Source.Object,IsolationLevel.Serializable);
        });
        Assert.Equal(writes,f.Native.Writes.Count);Assert.Single(f.Native.Rows.OfType<WORK_ORDER_MAINTENANCE_RETURN_POLICY>());
        f.Native.TransactionIsolation=IsolationLevel.ReadCommitted;
        await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ReadMaintenanceReturnPoliciesAsync(context,f.Guard,true)));
        Assert.Equal(writes,f.Native.Writes.Count);
    }

    [Theory][InlineData(false)][InlineData(true)]
    public async Task PolicyCatalogRefusesSourceReplacementAtTheFinalAuthorityCheck(bool owned)
    {
        using var f=await ReviewFixture.Create();f.Native.EnableCorrectiveActionTransactions();Schema(f);var context=Context(f);
        var calls=0;async Task Count(){await f.Guard();calls++;}
        if(owned)await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,Count,true));
        else await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,Count);
        var closingCall=calls;calls=0;var replacement=new Mock<TheTechIdea.Beep.IDataSource>();
        async Task Changed(){await f.Guard();if(++calls==closingCall)f.Native.Editor.Setup(value=>value.GetDataSource(f.Context.Database.ConnectionName)).Returns(replacement.Object);}
        if(owned)await Assert.ThrowsAsync<InvalidOperationException>(()=>PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>await f.Owner.ReadMaintenanceReturnPoliciesAsync(context,Changed,true)));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ReadMaintenanceReturnPoliciesAsync(context,Changed));
        Assert.Equal(closingCall,calls);Assert.Empty(f.Native.Writes);
    }
}
