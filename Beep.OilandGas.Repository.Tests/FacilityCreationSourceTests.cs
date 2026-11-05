using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Moq;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class FacilityCreationSourceTests
{
    [Theory][InlineData("valid")][InlineData("first")][InlineData("second")][InlineData("revoked")][InlineData("drift")][InlineData("lost")][InlineData("active")][InlineData("overlap")][InlineData("changed")][InlineData("capacity")][InlineData("split")][InlineData("begin")][InlineData("rollback")]
    public async Task EquipmentInstallationUsesReviewedSourceAtomicIntentAndExactRecovery(string mode)
    {
        using var f=await CompanyMaintenanceQualifiedAssetTests.ReviewFixture.Create();foreach(var grant in await f.Native.Db.Set<AppUserAssetAccess>().ToListAsync())grant.AccessLevel="WRITE";await f.Native.Db.SaveChangesAsync();
        var prior=f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single();prior.ACTIVE_IND="N";prior.REMOVE_DATE=DateTime.UtcNow.AddDays(-1);if(mode=="active")prior.ACTIVE_IND="Y";if(mode=="overlap")prior.REMOVE_DATE=DateTime.UtcNow;
        f.Native.Metadata.Setup(m=>m.GetTableMetadataAsync("FACILITY_EQUIPMENT_INSTALL_INTENT")).ReturnsAsync(new PPDMTableMetadata{TableName="FACILITY_EQUIPMENT_INSTALL_INTENT",EntityTypeName=nameof(FACILITY_EQUIPMENT_INSTALL_INTENT),PrimaryKeyColumn="FACILITY_EQUIPMENT_INSTALL_INTENT_ID"});
        f.Native.Metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("FACILITY_EQUIPMENT_INSTALL_INTENT")).ReturnsAsync(["FACILITY_EQUIPMENT_INSTALL_INTENT_ID"]);
        var creator=new ClassCreator(f.Native.Editor.Object);f.Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool _)=>{
            var model=requested.EntityName=="FACILITY_EQUIPMENT_INSTALL_INTENT"?typeof(FACILITY_EQUIPMENT_INSTALL_INTENT):typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+requested.EntityName)!;var shape=creator.ConvertToEntityStructure(model);var keys=f.Native.Metadata.Object.GetPrimaryKeyColumnsAsync(requested.EntityName).GetAwaiter().GetResult();
            foreach(var column in shape.Fields){column.IsKey=keys.Contains(column.FieldName,StringComparer.Ordinal);if(requested.EntityName=="FACILITY_EQUIPMENT" && column.Fieldtype==typeof(string).FullName){column.Size=4096;column.Size1=4096;}if(mode=="capacity" && requested.EntityName=="FACILITY_EQUIPMENT_INSTALL_INTENT" && column.FieldName=="COMMAND_JSON"){column.Size=1;column.Size1=1;}}return shape;
        });
        var owner=new FacilityManagementService(f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),f.Context.Database.ConnectionName,resolveModuleConnection:module=>Task.FromResult(mode=="split" && module=="FACILITY"?"other":f.Context.Database.ConnectionName),maintenanceAssetAccess:f.Native.Policy);
        var proposal=new DevelopmentEquipmentInstallationProposal(f.Context,"plant","gas","pump","Engineer","Install reviewed pump");var source=await owner.ReviewDevelopmentEquipmentInstallationAsync(proposal,f.Guard);var review=new DevelopmentEquipmentInstallationReview(proposal,source,new string('A',64),DateTimeOffset.UtcNow);var command=new DevelopmentEquipmentInstallation(review,Guid.NewGuid(),Guid.NewGuid(),DateTime.UtcNow.AddMinutes(-1),true);
        if(mode=="changed")prior.SOURCE="changed";
        var properties=f.Native.Source.Object.Dataconnection.ConnectionProp;var connection=new Mock<IDataConnection>();connection.SetupGet(value=>value.ConnectionProp).Returns(properties);connection.As<IBeepConnectionEvidence>().SetupGet(value=>value.DbConn).Returns(f.Native.LeasePhysicalConnection.Object);f.Native.Source.SetupGet(value=>value.Dataconnection).Returns(connection.Object);f.Native.EnableCorrectiveActionTransactions();
        if(mode=="first")f.Native.LeaseFailWriteNumber=1;if(mode is "second" or "rollback")f.Native.LeaseFailWriteNumber=2;if(mode=="rollback")f.Native.LeaseRollbackFailed=true;if(mode=="revoked")f.Native.AfterLeaseWrite=_=>f.Revoked=true;if(mode=="drift")f.Native.AfterLeaseWrite=_=>prior.SOURCE="changed during write";if(mode=="begin")f.Native.LeaseBeginNoOp=true;if(mode=="lost"){f.Native.LeaseCommitFailed=true;f.Native.LeaseCommitAfterFailure=true;}
        if(mode=="valid")
        {
            var entry=await owner.RecordEquipmentInstallationAsync(command,f.Guard);var restored=entry.Restore(f.Context.Company,f.Context.ActorUserId);Assert.Equal(command.Revision(),restored.Command.Revision());Assert.Equal(8,restored.Installation.INSTALL_OBS_NO);Assert.Equal(2,f.Native.Writes.Count);
            var retry=await owner.RecordEquipmentInstallationAsync(command,f.Guard);Assert.Equal(entry.RECORDED_REVISION,retry.RECORDED_REVISION);Assert.Equal(2,f.Native.Writes.Count);
            await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordEquipmentInstallationAsync(command with{InstallationGuid=Guid.NewGuid()},f.Guard));var recovery=await owner.RecoverEquipmentInstallationAsync(command,f.Guard);Assert.Equal(entry.RECORDED_REVISION,recovery.RECORDED_REVISION);Assert.Equal(2,f.Native.Writes.Count);
            f.Revoked=true;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>owner.RecoverEquipmentInstallationAsync(command,f.Guard));
        }
        else if(mode=="rollback")
        {
            await Assert.ThrowsAsync<AggregateException>(()=>owner.RecordEquipmentInstallationAsync(command,f.Guard));Assert.Equal(2,f.Native.Writes.Count);Assert.NotNull(f.Native.LeaseActiveTransaction);
        }
        else if(mode=="lost")
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordEquipmentInstallationAsync(command,f.Guard));Assert.Single(f.Native.Rows.OfType<FACILITY_EQUIPMENT_INSTALL_INTENT>());Assert.Equal(2,f.Native.Writes.Count);f.Native.LeaseCommitFailed=false;var recovery=await owner.RecoverEquipmentInstallationAsync(command,f.Guard);Assert.Equal(command.Revision(),recovery.Restore(f.Context.Company,f.Context.ActorUserId).Command.Revision());Assert.Equal(2,f.Native.Writes.Count);
        }
        else
        {
            var error=await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordEquipmentInstallationAsync(command,f.Guard));if(mode is "active" or "overlap")Assert.Contains("reconciled, non-overlapping UTC history",error.Message);Assert.Empty(f.Native.Rows.OfType<FACILITY_EQUIPMENT_INSTALL_INTENT>());Assert.DoesNotContain(f.Native.Rows.OfType<FACILITY_EQUIPMENT>(),row=>row.PPDM_GUID==command.InstallationGuid.ToString("D").ToUpperInvariant());
            Assert.Equal(mode switch{"first"=>1,"second"=>2,"revoked"=>1,"drift"=>2,_=>0},f.Native.Writes.Count);
        }
    }

    [Theory][InlineData("valid")][InlineData("collision")][InlineData("missing")][InlineData("duplicate")][InlineData("inactive")][InlineData("revoked")][InlineData("split")][InlineData("changed")][InlineData("operator-missing")][InlineData("operator-inactive")][InlineData("operator-duplicate")][InlineData("operator-changed")][InlineData("preflight")][InlineData("preflight-link")][InlineData("preflight-intent")][InlineData("preflight-capacity")][InlineData("preflight-facility-capacity")][InlineData("preflight-nonnullable-description")][InlineData("preflight-operator-drift")][InlineData("write-valid")][InlineData("write-first")][InlineData("write-second")][InlineData("write-third")][InlineData("write-revoke")][InlineData("write-drift")][InlineData("write-lost")][InlineData("write-rollback")][InlineData("write-begin")]
    public async Task NativeCreationReviewRequiresUniqueIdentityStableActiveTypeAndAuthority(string mode)
    {
        using var f=await CompanyMaintenanceQualifiedAssetTests.ReviewFixture.Create();var draft=new DevelopmentFacilityCreationDraft(f.Context,"Editor","new-plant","PROCESSING","explicit-operator","New plant",null,"Expansion");
        var type=new R_FACILITY_TYPE{FACILITY_TYPE="PROCESSING",ACTIVE_IND=mode=="inactive"?"N":"Y",LONG_NAME="Recorded type"};
        var operatorRow=new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="explicit-operator",ACTIVE_IND=mode=="operator-inactive"?"N":"Y",BA_LONG_NAME="Recorded operator"};
        if(mode!="operator-missing")f.Native.Rows.Add(operatorRow);if(mode=="operator-duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(operatorRow));
        if(mode!="missing")f.Native.Rows.Add(type);if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(type));
        if(mode=="collision")f.Native.Rows.Add(new FACILITY{FACILITY_ID="new-plant",FACILITY_TYPE="PROCESSING",PRIMARY_FIELD_ID="foreign-field",CURRENT_OPERATOR="foreign-company"});
        f.Native.Metadata.Setup(m=>m.GetTableMetadataAsync("R_FACILITY_TYPE")).ReturnsAsync(new PPDMTableMetadata{TableName="R_FACILITY_TYPE",EntityTypeName=nameof(R_FACILITY_TYPE),PrimaryKeyColumn="FACILITY_TYPE"});
        f.Native.Metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("R_FACILITY_TYPE")).ReturnsAsync(["FACILITY_TYPE"]);
        f.Native.Metadata.Setup(m=>m.GetTableMetadataAsync("BUSINESS_ASSOCIATE")).ReturnsAsync(new PPDMTableMetadata{TableName="BUSINESS_ASSOCIATE",EntityTypeName=nameof(BUSINESS_ASSOCIATE),PrimaryKeyColumn="BUSINESS_ASSOCIATE_ID"});
        f.Native.Metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("BUSINESS_ASSOCIATE")).ReturnsAsync(["BUSINESS_ASSOCIATE_ID"]);
        f.Native.Metadata.Setup(m=>m.GetTableMetadataAsync("FACILITY_CREATE_INTENT")).ReturnsAsync(new PPDMTableMetadata{TableName="FACILITY_CREATE_INTENT",EntityTypeName=nameof(FACILITY_CREATE_INTENT),PrimaryKeyColumn="FACILITY_CREATE_INTENT_ID"});
        f.Native.Metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("FACILITY_CREATE_INTENT")).ReturnsAsync(["FACILITY_CREATE_INTENT_ID"]);
        var creator=new ClassCreator(f.Native.Editor.Object);f.Native.Source.Setup(source=>source.CheckEntityExist(It.IsAny<string>())).Returns(true);
        f.Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool _)=>
        {
            var model=requested.EntityName=="FACILITY_CREATE_INTENT"?typeof(FACILITY_CREATE_INTENT):typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+requested.EntityName)!;var shape=creator.ConvertToEntityStructure(model);
            var keys=f.Native.Metadata.Object.GetPrimaryKeyColumnsAsync(requested.EntityName).GetAwaiter().GetResult();foreach(var column in shape.Fields){column.IsKey=keys.Contains(column.FieldName,StringComparer.Ordinal);if(requested.EntityName is "FACILITY" or "FACILITY_FIELD" && column.Fieldtype==typeof(string).FullName){column.Size=4096;column.Size1=4096;if(requested.EntityName=="FACILITY" && column.FieldName=="DESCRIPTION")column.AllowDBNull=mode!="preflight-nonnullable-description";if(mode=="preflight-facility-capacity" && requested.EntityName=="FACILITY" && column.FieldName=="FACILITY_LONG_NAME"){column.Size=1;column.Size1=1;}}}return shape;
        });
        if(mode=="revoked")f.Revoked=true;var reads=0;
        if(mode=="changed")f.Native.AfterRead=()=>{var last=f.Native.Source.Invocations.Last(call=>call.Method.Name=="GetEntityAsync");if(last.Arguments[0] as string=="R_FACILITY_TYPE" && ++reads==2)type.LONG_NAME="Changed";return Task.CompletedTask;};
        if(mode=="operator-changed")f.Native.AfterRead=()=>{var last=f.Native.Source.Invocations.Last(call=>call.Method.Name=="GetEntityAsync");if(last.Arguments[0] as string=="BUSINESS_ASSOCIATE" && ++reads==2)operatorRow.BA_LONG_NAME="Changed";return Task.CompletedTask;};
        var owner=new FacilityManagementService(f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),f.Context.Database.ConnectionName,
            resolveModuleConnection:module=>Task.FromResult(mode=="split" && module=="FACILITY"?"other":f.Context.Database.ConnectionName),maintenanceAssetAccess:f.Native.Policy);
        if(mode.StartsWith("preflight",StringComparison.Ordinal) || mode.StartsWith("write",StringComparison.Ordinal))
        {
            var source=await owner.ReadFacilityCreationSourceAsync(draft,f.Guard);var review=new DevelopmentFacilityCreationReview(draft,source.TypeSource,source.OperatorRevision,new string('A',64),"",DateTimeOffset.UtcNow);review=review with{Revision=review.ContentRevision()};
            var command=new DevelopmentFacilityCreation(review,Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),true);
            if(mode=="preflight-link")f.Native.Rows.Add(new FACILITY_FIELD{FACILITY_ID=draft.FacilityId,FACILITY_TYPE=draft.FacilityType,FIELD_ID=draft.Context.FieldId!});
            if(mode=="preflight-intent")f.Native.Rows.Add(new FACILITY_CREATE_INTENT{FACILITY_CREATE_INTENT_ID=command.IntentId.ToString("N")});
            if(mode=="preflight-operator-drift")operatorRow.BA_LONG_NAME="Later change";
            if(mode=="preflight-capacity")f.Native.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(value=>value.EntityName=="FACILITY_CREATE_INTENT"),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(typeof(FACILITY_CREATE_INTENT));foreach(var column in shape.Fields){column.IsKey=column.FieldName=="FACILITY_CREATE_INTENT_ID";if(column.FieldName=="COMMAND_JSON"){column.Size=1;column.Size1=1;}}return shape;});
            if(mode.StartsWith("write",StringComparison.Ordinal))
            {
                var properties=f.Native.Source.Object.Dataconnection.ConnectionProp;var connection=new Mock<IDataConnection>();connection.SetupGet(value=>value.ConnectionProp).Returns(properties);
                connection.As<IBeepConnectionEvidence>().SetupGet(value=>value.DbConn).Returns(f.Native.LeasePhysicalConnection.Object);f.Native.Source.SetupGet(value=>value.Dataconnection).Returns(connection.Object);
                f.Native.EnableCorrectiveActionTransactions();if(mode=="write-first")f.Native.LeaseFailWriteNumber=1;if(mode is "write-second" or "write-rollback")f.Native.LeaseFailWriteNumber=2;if(mode=="write-third")f.Native.LeaseFailWriteNumber=3;
                if(mode=="write-rollback")f.Native.LeaseRollbackFailed=true;if(mode=="write-revoke")f.Native.AfterLeaseWrite=_=>f.Revoked=true;if(mode=="write-drift")f.Native.AfterLeaseWrite=_=>operatorRow.BA_LONG_NAME="Changed during command";if(mode=="write-begin")f.Native.LeaseBeginNoOp=true;if(mode=="write-lost"){f.Native.LeaseCommitFailed=true;f.Native.LeaseCommitAfterFailure=true;}
                if(mode=="write-valid")
                {
                    var saved=await owner.RecordFacilityCreationAsync(command,f.Guard);var restored=saved.Restore(draft.Context.Company,draft.Context.ActorUserId);Assert.Equal(command.Revision(),restored.Command.Revision());Assert.Equal(draft.FacilityId,restored.Link.FACILITY_ID);Assert.Equal(3,f.Native.Writes.Count);
                    var repeated=await owner.RecordFacilityCreationAsync(command,f.Guard);Assert.Equal(saved.RECORDED_REVISION,repeated.RECORDED_REVISION);Assert.Equal(3,f.Native.Writes.Count);
                    await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordFacilityCreationAsync(command with{FacilityGuid=Guid.NewGuid()},f.Guard));Assert.Equal(3,f.Native.Writes.Count);
                    var recovered=await owner.RecoverFacilityCreationAsync(command,f.Guard);Assert.Equal(saved.RECORDED_REVISION,recovered.RECORDED_REVISION);Assert.Equal(3,f.Native.Writes.Count);
                    f.Revoked=true;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>owner.RecoverFacilityCreationAsync(command,f.Guard));Assert.Equal(3,f.Native.Writes.Count);
                }
                else if(mode=="write-lost")
                {
                    await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordFacilityCreationAsync(command,f.Guard));Assert.Single(f.Native.Rows.OfType<FACILITY_CREATE_INTENT>());var writes=f.Native.Writes.Count;Assert.Equal(3,writes);f.Native.LeaseCommitFailed=false;
                    var recovered=await owner.RecoverFacilityCreationAsync(command,f.Guard);Assert.Equal(command.Revision(),recovered.Restore(draft.Context.Company,draft.Context.ActorUserId).Command.Revision());Assert.Equal(writes,f.Native.Writes.Count);
                    await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecoverFacilityCreationAsync(command with{IntentId=Guid.NewGuid()},f.Guard));Assert.Equal(writes,f.Native.Writes.Count);
                }
                else if(mode=="write-rollback"){await Assert.ThrowsAsync<AggregateException>(()=>owner.RecordFacilityCreationAsync(command,f.Guard));Assert.Equal(2,f.Native.Writes.Count);Assert.NotNull(f.Native.LeaseActiveTransaction);}
                else{await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordFacilityCreationAsync(command,f.Guard));Assert.Empty(f.Native.Rows.OfType<FACILITY_CREATE_INTENT>());Assert.DoesNotContain(f.Native.Rows.OfType<FACILITY>(),value=>value.FACILITY_ID==draft.FacilityId);Assert.DoesNotContain(f.Native.Rows.OfType<FACILITY_FIELD>(),value=>value.FACILITY_ID==draft.FacilityId);Assert.Equal(mode switch{"write-first"=>1,"write-second"=>2,"write-third"=>3,"write-drift"=>3,"write-revoke"=>1,_=>0},f.Native.Writes.Count);if(mode!="write-begin")Assert.Contains("rollback",f.Native.LeaseTransactions);}
                return;
            }
            if(mode=="preflight")await owner.ReadFacilityCreationPreflightAsync(command,f.Guard);
            else
            {
                var error=await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.ReadFacilityCreationPreflightAsync(command,f.Guard));
                var expected=mode switch{"preflight-link" or "preflight-intent"=>"FIELD link or creation intent","preflight-capacity"=>"creation storage capacity","preflight-facility-capacity" or "preflight-nonnullable-description"=>"FIELD-link storage capacity",_=>"type or operator source changed"};
                Assert.Contains(expected,error.Message);
            }
        }
        else if(mode=="valid"){var result=await owner.ReadFacilityCreationSourceAsync(draft,f.Guard);Assert.Equal("Recorded type",result.TypeSource.LONG_NAME);Assert.True(ProcessDefinitionRevision.IsValid(result.OperatorRevision));}
        else await Assert.ThrowsAnyAsync<Exception>(()=>owner.ReadFacilityCreationSourceAsync(draft,f.Guard));
        Assert.Empty(f.Native.Writes);
    }
}
