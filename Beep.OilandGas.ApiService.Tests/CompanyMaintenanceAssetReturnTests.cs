using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyMaintenanceAssetReturnTests
{
    [Fact]
    public async Task PublicFactsAllowAnotherAuthorizedReadAccountWithoutExposingPrivateBodyOrReplaying()
    {
        var f=new Fixture();await f.Ready();var saved=await f.Record();var writes=f.Native.Writes;f.Well.Level="READ";var viewer=f.Well.Context with{ActorUserId="authorized-reader"};
        var facts=await f.Well.Owner.ReadMaintenanceAssetReturnFactsAsync(viewer,f.Command.Reference().Query,()=>Task.CompletedTask);Assert.NotNull(facts);facts.Require();Assert.Equal(saved.Receipt.ActorUserId,facts.ActorUserId);Assert.Equal(saved.Receipt.ReturnId,facts.ReturnId);Assert.Equal(f.Command.Review.Assets.Closure.Revision,facts.ClosureRevision);Assert.Equal("Shut In",Assert.Single(facts.Wells).PreviousCondition);Assert.Equal(f.Command.StatusId("well"),Assert.Single(facts.Wells).RecordedStatusId);
        var body=JsonSerializer.Serialize(facts);Assert.DoesNotContain(f.Command.IntentId.ToString("D"),body);Assert.DoesNotContain(f.Command.Reason,body);foreach(var column in new[]{"COMMAND_JSON","BEFORE_JSON","AFTER_JSON"})Assert.DoesNotContain(column,body);Assert.Equal(writes,f.Native.Writes);
        var reads=f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="WELL_STATUS");f.Well.Revoked=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Well.Owner.ReadMaintenanceAssetReturnFactsAsync(viewer,f.Command.Reference().Query,()=>Task.CompletedTask));Assert.Equal(reads,f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="WELL_STATUS"));Assert.Equal(writes,f.Native.Writes);
    }
    [Fact]
    public async Task PublicFactsDoNotInventAbsenceForMissingSchemaOrConfirmChangedProofOrNativeEffects()
    {
        var f=new Fixture();await f.Ready();var writes=f.Native.Writes;Assert.Null(await f.Well.Owner.ReadMaintenanceAssetReturnFactsAsync(f.Well.Context,f.Command.Reference().Query,()=>Task.CompletedTask));Assert.Equal(writes,f.Native.Writes);
        f.MissingSchema=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Well.Owner.ReadMaintenanceAssetReturnFactsAsync(f.Well.Context,f.Command.Reference().Query,()=>Task.CompletedTask));f.MissingSchema=false;
        await f.Record();writes=f.Native.Writes;var active=f.Native.Rows.OfType<WELL_STATUS>().Single(row=>row.STATUS_ID==f.Command.StatusId("well"));active.PERCENT_CAPABILITY=99m;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Well.Owner.ReadMaintenanceAssetReturnFactsAsync(f.Well.Context,f.Command.Reference().Query,()=>Task.CompletedTask));active.PERCENT_CAPABILITY=72.5m;
        var intent=f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>().Single();intent.AFTER_JSON="{}";await Assert.ThrowsAnyAsync<Exception>(()=>f.Well.Owner.ReadMaintenanceAssetReturnFactsAsync(f.Well.Context,f.Command.Reference().Query,()=>Task.CompletedTask));
        intent.AFTER_JSON=null!;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Well.Owner.ReadMaintenanceAssetReturnFactsAsync(f.Well.Context,f.Command.Reference().Query,()=>Task.CompletedTask));Assert.Equal(writes,f.Native.Writes);
    }
    [Theory][InlineData("status","insert")][InlineData("status","update")][InlineData("status","delete")][InlineData("status","batch")][InlineData("status","uow")][InlineData("status","upsert")][InlineData("intent","insert")][InlineData("intent","update")][InlineData("intent","delete")][InlineData("intent","batch")][InlineData("intent","uow")][InlineData("intent","upsert")]
    public async Task RawRepositoryCannotBypassNativeStatusOrDurableIntentOwnership(string kind,string operation)
    {
        var f=new CompanyMaintenanceWellEvidenceTests.Fixture();var table=kind=="status"?"WELL_STATUS":"WORK_ORDER_ASSET_RETURN";var type=kind=="status"?typeof(WELL_STATUS):typeof(WORK_ORDER_ASSET_RETURN);
        object row=kind=="status"?CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<WELL_STATUS>().First()):new WORK_ORDER_ASSET_RETURN{RETURN_ID="original-return"};
        var repository=new PPDMGenericRepository(f.Native.Editor.Object,Moq.Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Moq.Mock.Of<IPPDM39DefaultsRepository>(),f.Metadata.Object,type,"lifecycle",table);
        async Task Attempt(){switch(operation){case "insert":await repository.InsertAsync(row,"actor");break;case "update":await repository.UpdateAsync(row,"actor");break;case "delete":await repository.DeleteAsync(row);break;case "batch":await repository.InsertBatchAsync([row],"actor");break;case "uow":_ =repository.UnitOfWork;break;case "upsert":await repository.UpsertBatchAsync([row],"actor");break;}}
        await Assert.ThrowsAnyAsync<Exception>(Attempt);Assert.Equal(0,f.Native.Writes);
        await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>await Assert.ThrowsAnyAsync<Exception>(Attempt));Assert.Equal(0,f.Native.Writes);
    }
    [Fact]
    public async Task LegacyFacetSetterCannotExpireOrAppendAConditionOutsideTheReviewedCommand()
    {
        var f=new CompanyMaintenanceWellEvidenceTests.Fixture();var before=JsonSerializer.Serialize(f.Native.Rows.OfType<WELL_STATUS>().ToArray());
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.SetFacetAsync(new(){UWI="well",StatusType="Condition",Status="Active",EffectiveDate=DateTime.UtcNow,Source="recorded-source"},"return-actor"));
        Assert.Equal(0,f.Native.Writes);Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows.OfType<WELL_STATUS>().ToArray()));
    }
    [Theory][InlineData("before","dictionary")][InlineData("before","table")][InlineData("readback","dictionary")][InlineData("readback","table")][InlineData("recovery","dictionary")][InlineData("recovery","table")]
    public async Task MissingNativeWellProofColumnCannotWriteCommitOrConfirmAnOriginalReturn(string stage,string shape)
    {
        var f=new Fixture();await f.Ready();if(stage=="recovery")await f.Record();
        var before=JsonSerializer.Serialize(f.Native.Rows);var writes=f.Native.Writes;var snapshots=0;
        f.Native.Source.Setup(value=>value.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>
        {
            var omit=table=="WELL_STATUS" && filters.All(filter=>filter.FieldName!="STATUS_TYPE") && (stage!="readback" || ++snapshots>=2);
            var rows=f.Native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).ToArray();
            return rows.Select(row=>omit?CompanyMaintenanceWellEvidenceTests.ProjectNativeRow(row,shape,"END_TIME"):JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
        });
        if(stage=="recovery")
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Well.Owner.RecoverMaintenanceAssetReturnAsync(f.Well.Context,f.Command.Reference(),f.Authority));
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Well.Owner.ReadMaintenanceAssetReturnFactsAsync(f.Well.Context,f.Command.Reference().Query,()=>f.Authority(null)));
        }
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());
        Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows));if(stage!="readback")Assert.Equal(writes,f.Native.Writes);else Assert.Contains("rollback",f.Native.Transactions);
    }
    internal sealed class Fixture
    {
        internal readonly CompanyMaintenanceClosureTests.Fixture Flow=new();
        internal CompanyMaintenanceWellEvidenceTests.Fixture Well=null!;
        internal CompanyMaintenanceClosureRecovery Closure=null!;
        internal CompanyMaintenanceAssetReturnCommand Command=null!;
        internal bool MissingSchema,BadKey,SmallBody,PhysicalChanged,OwnedPhysicalChanged,ClosureChanged;
        internal int FailAt;
        internal PersonaTaskRoutingTests.Fixture Native=>Flow.Flow.Flow.Flow.Flow.Flow.Owner.Native;
        internal async Task Ready(bool actualAudit=false,string zone="current",bool multipleWells=false)
        {
            await Flow.Ready(actualAudit,assetReturn:true,multipleWells:multipleWells);Closure=await Flow.Flow.Flow.Flow.Flow.Flow.Owner.Service.CloseMaintenanceAsync(Flow.Command,Flow.Flow.Authority,Flow.Flow.Read);
            Well=new(zone,Flow.Command.Review.Current.Completion.Context,Native);Well.Metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_ASSET_RETURN")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_ASSET_RETURN",EntityTypeName=typeof(WORK_ORDER_ASSET_RETURN).FullName!,PrimaryKeyColumn="RETURN_ID",Module="PPDM_CORE"});Well.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_ASSET_RETURN")).ReturnsAsync(["RETURN_ID"]);
            Native.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_ASSET_RETURN")).Returns(()=>!MissingSchema);
            Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),It.IsAny<bool>())).Returns((EntityStructure query,bool _)=>
            {
                var type=query.EntityName=="WORK_ORDER_ASSET_RETURN"?typeof(WORK_ORDER_ASSET_RETURN):typeof(WELL).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+query.EntityName)!;var shape=Native.Editor.Object.classCreator.ConvertToEntityStructure(type);var keys=query.EntityName=="WORK_ORDER_ASSET_RETURN"?new[]{"RETURN_ID"}:query.EntityName=="WELL_STATUS"?(BadKey?new[]{"STATUS_ID"}:new[]{"UWI","SOURCE","STATUS_ID"}):Array.Empty<string>();foreach(var field in shape.Fields)field.IsKey=keys.Contains(field.FieldName,StringComparer.Ordinal);if(SmallBody && query.EntityName=="WORK_ORDER_ASSET_RETURN")shape.Fields.Single(field=>field.FieldName=="COMMAND_JSON").Size1=8;return shape;
            });
            foreach(var table in new[]{"WELL_STATUS","WORK_ORDER_ASSET_RETURN"})
            {
                Native.Source.Setup(value=>value.InsertEntity(table,It.IsAny<object>())).Returns((string name,object row)=>{Native.Writes++;if(Native.Writes==FailAt)return new ErrorsInfo{Flag=Errors.Failed};Native.Rows.Add(JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!);return new ErrorsInfo{Flag=Errors.Ok};});
            }
            Native.Source.Setup(value=>value.UpdateEntity("WELL_STATUS",It.IsAny<object>())).Returns((string _,object row)=>{Native.Writes++;if(Native.Writes==FailAt)return new ErrorsInfo{Flag=Errors.Failed};var update=(WELL_STATUS)row;var index=Native.Rows.FindIndex(value=>value is WELL_STATUS status && status.UWI==update.UWI && status.SOURCE==update.SOURCE && status.STATUS_ID==update.STATUS_ID);if(index<0)return new ErrorsInfo{Flag=Errors.Failed};Native.Rows[index]=CompanyInventorySourceReview.Copy(update);return new ErrorsInfo{Flag=Errors.Ok};});
            if(actualAudit)Well.Owner=new WellServices(Native.Editor.Object,new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler(),Moq.Mock.Of<IPPDM39DefaultsRepository>(),Well.Metadata.Object,Well.Context.Database.ConnectionName,Well.Access.Object,zone=="unspecified-kuwait"?"Asia/Kuwait":"UTC");
            if(multipleWells)
            {
                var parent=CompanyInventorySourceReview.Copy(Native.Rows.OfType<WELL>().Single());parent.UWI="well-2";Native.Rows.Add(parent);
                foreach(var row in Native.Rows.OfType<WELL_STATUS>().ToArray()){var copy=CompanyInventorySourceReview.Copy(row);copy.UWI="well-2";Native.Rows.Add(copy);}
                Well.Access.Setup(value=>value.GetReadConjunctionsAsync(typeof(WELL),"WELL")).ReturnsAsync(new List<IReadOnlyList<AppFilter>>{new[]{new AppFilter{FieldName="UWI",Operator="=",FilterValue="well"}},new[]{new AppFilter{FieldName="UWI",Operator="=",FilterValue="well-2"}}});
            }
            var physical=(await Flow.Flow.Read(false))!;var allWells=new List<CompanyMaintenanceWellEvidence>{await Well.Read()};
            if(multipleWells)allWells.Add(await Well.Owner.ReadMaintenanceWellEvidenceAsync(Well.Context,"well-2",Well.Guard));
            var utc=DateTime.UtcNow;var closureFacts=CompanyMaintenanceClosureFacts.From(Closure);var assets=new CompanyMaintenanceAssetEvidence(Well.Context,physical.Completion.Report,closureFacts,physical.Completion.Order,allWells,utc,CompanyMaintenanceAssetEvidence.GetBlockers(Well.Context,physical.Completion.Report,closureFacts,physical.Completion.Order,allWells,utc));
            physical=(await Flow.Flow.Read(false))!;var review=new CompanyMaintenanceAssetReturnReview(assets,physical);review.Require();Command=new(review,Guid.NewGuid(),DateTime.UtcNow,allWells.Select(well=>new CompanyWellReturnActivation(well.WellId,well.Facets.Single(row=>row.Facet=="Condition").Source,72.5m)).ToArray(),"Explicit observed capability and native condition return",true);Command.Require();
        }
        internal Task Authority(string? role)=>Flow.Flow.Authority(role);
        internal async Task<CompanyWorkOrderReturnReview> Physical(bool owned){if(PhysicalChanged || owned && OwnedPhysicalChanged)throw new InvalidOperationException("Changed original physical evidence");return(await Flow.Flow.Read(owned))!;}
        internal Task<CompanyMaintenanceClosureFacts?> ReadClosure(bool owned)=>Task.FromResult<CompanyMaintenanceClosureFacts?>(ClosureChanged?CompanyMaintenanceClosureFacts.From(Closure) with{Revision=new string('F',64)}:CompanyMaintenanceClosureFacts.From(Closure));
        internal Task<CompanyMaintenanceAssetReturnRecovery> Record(CompanyMaintenanceAssetReturnCommand? command=null)=>Well.Owner.RecordMaintenanceAssetReturnAsync(command??Command,Authority,Physical,ReadClosure);
    }
    [Theory][InlineData(false,"current")][InlineData(true,"current")][InlineData(false,"unspecified-kuwait")]
    public async Task ActualOwnedReturnChangesOnlyTheReviewedConditionAndRetainsOriginalAuditAndRecovery(bool actualAudit,string zone)
    {
        var f=new Fixture();await f.Ready(actualAudit,zone);var preserved=JsonSerializer.Serialize(f.Native.Rows.Where(row=>row is not WELL_STATUS));var original=f.Native.Rows.OfType<WELL_STATUS>().Single(row=>row.STATUS_TYPE=="Condition");var oldCreated=original.ROW_CREATED_DATE;var writes=f.Native.Writes;var saved=await f.Record();saved.Require(f.Well.Context.Company,f.Well.Context.ActorUserId,f.Command.Reference());Assert.Equal(writes+3,f.Native.Writes);writes=f.Native.Writes;
        Assert.Equal(preserved,JsonSerializer.Serialize(f.Native.Rows.Where(row=>row is not WELL_STATUS && row is not WORK_ORDER_ASSET_RETURN)));Assert.Single(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());var expired=f.Native.Rows.OfType<WELL_STATUS>().Single(row=>row.STATUS_ID==original.STATUS_ID);Assert.Equal("N",expired.ACTIVE_IND);Assert.Equal(oldCreated,expired.ROW_CREATED_DATE);Assert.Equal(original.ROW_CREATED_BY,expired.ROW_CREATED_BY);var active=f.Native.Rows.OfType<WELL_STATUS>().Single(row=>row.STATUS_ID==f.Command.StatusId("well"));Assert.Equal("Active",active.STATUS);Assert.Equal("Condition",active.STATUS_TYPE);Assert.Equal("UTC",active.TIMEZONE);Assert.Equal(72.5m,active.PERCENT_CAPABILITY);Assert.Equal(f.Command.ActivatedAtUtc,active.EFFECTIVE_DATE);Assert.Equal(f.Well.Context.ActorUserId,active.ROW_CREATED_BY);Assert.Equal(saved.Receipt.RecordedAtUtc,active.ROW_CREATED_DATE);
        if(zone=="unspecified-kuwait"){Assert.Equal(DateTimeKind.Unspecified,expired.END_TIME!.Value.Kind);Assert.Equal(f.Command.ActivatedAtUtc.AddHours(3).Ticks,expired.END_TIME.Value.Ticks);}Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(await f.Record()));Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(await f.Well.Owner.RecoverMaintenanceAssetReturnAsync(f.Well.Context,f.Command.Reference(),f.Authority)));Assert.Equal(writes,f.Native.Writes);
        Assert.Equal(JsonSerializer.Serialize(f.Closure),JsonSerializer.Serialize(await f.Flow.Flow.Flow.Flow.Flow.Flow.Owner.Service.RecoverMaintenanceClosureAsync(f.Flow.Command.Review.Stage.Context,f.Flow.Command.Reference(),f.Flow.Flow.Authority)));Assert.Equal(writes,f.Native.Writes);await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(f.Command with{IntentId=Guid.NewGuid()}));Assert.Equal(writes,f.Native.Writes);
    }
    [Theory][InlineData("complete")][InlineData("omitted")][InlineData("second-write-failure")][InlineData("second-rights")]
    public async Task EveryOriginalAffectedWellMustAdvanceTogetherOrRemainUnchanged(string mode)
    {
        var f=new Fixture();await f.Ready(multipleWells:true);Assert.Equal(2,f.Command.Review.Assets.Wells.Count);
        var before=JsonSerializer.Serialize(f.Native.Rows.OfType<WELL_STATUS>().ToArray());var writes=f.Native.Writes;var command=f.Command;
        if(mode=="omitted")command=command with{Wells=command.Wells.Take(1).ToArray()};
        if(mode=="second-write-failure")f.FailAt=writes+4;
        if(mode=="second-rights")f.Well.Access.Setup(value=>value.GetEffectiveAccessLevelAsync(typeof(WELL),"WELL",It.IsAny<object>(),true)).ReturnsAsync((Type _,string _,object row,bool _)=>((WELL)row).UWI=="well-2"?"READ":"WRITE");
        if(mode=="complete")
        {
            var result=await f.Record(command);Assert.Equal(new[]{"well","well-2"},result.Receipt.WellIds);Assert.Equal(writes+5,f.Native.Writes);writes=f.Native.Writes;
            foreach(var well in command.Wells){var conditions=f.Native.Rows.OfType<WELL_STATUS>().Where(row=>row.UWI==well.WellId && row.STATUS_TYPE=="Condition").ToArray();Assert.Equal(2,conditions.Length);Assert.Single(conditions,row=>row.ACTIVE_IND=="N");Assert.Single(conditions,row=>row.ACTIVE_IND=="Y" && row.STATUS=="Active");}
            Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await f.Well.Owner.RecoverMaintenanceAssetReturnAsync(f.Well.Context,command.Reference(),f.Authority)));Assert.Equal(writes,f.Native.Writes);
        }
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(command));Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows.OfType<WELL_STATUS>().ToArray()));Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());
            if(mode=="second-write-failure")Assert.Contains("rollback",f.Native.Transactions);else Assert.Equal(writes,f.Native.Writes);
        }
    }
    [Theory][InlineData("schema")][InlineData("key")][InlineData("body")][InlineData("physical")][InlineData("owned-physical")][InlineData("closure")][InlineData("revoked")][InlineData("missing-percent")][InlineData("source-stream")][InlineData("future")][InlineData("unconfirmed")]
    public async Task MissingSchemaSourceAuthorityOrExplicitObservationsCannotActivate(string fault)
    {
        var f=new Fixture();await f.Ready();f.MissingSchema=fault=="schema";f.BadKey=fault=="key";f.SmallBody=fault=="body";f.PhysicalChanged=fault=="physical";f.OwnedPhysicalChanged=fault=="owned-physical";f.ClosureChanged=fault=="closure";f.Flow.Flow.Revoked=fault=="revoked";var command=f.Command;if(fault=="missing-percent")command=command with{Wells=[new("well",command.Wells[0].RecordingSource,null)]};if(fault=="source-stream")command=command with{Wells=[new("well","different-source",72.5m)]};if(fault=="future")command=command with{ActivatedAtUtc=DateTime.UtcNow.AddDays(1)};if(fault=="unconfirmed")command=command with{Confirmed=false};var writes=f.Native.Writes;await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(command));Assert.Equal(writes,f.Native.Writes);Assert.DoesNotContain(f.Native.Rows,row=>row is WORK_ORDER_ASSET_RETURN);
    }
    [Theory][InlineData("uncertain")][InlineData("old-condition")][InlineData("new-condition")][InlineData("unrelated-facet")][InlineData("intent")][InlineData("foreign-actor")][InlineData("foreign-reference")]
    public async Task OriginalRecoveryChecksExactNativeEffectsAndCannotReplayOrAcceptForeignProof(string fault)
    {
        var f=new Fixture();await f.Ready();f.Native.FailCommit=fault=="uncertain";var error=await Xunit.Record.ExceptionAsync(()=>f.Record());if(fault=="uncertain")Assert.NotNull(error);else Assert.Null(error);f.Native.FailCommit=false;var writes=f.Native.Writes;var context=f.Well.Context;var reference=f.Command.Reference();
        if(fault=="old-condition")f.Native.Rows.OfType<WELL_STATUS>().Single(row=>row.STATUS_TYPE=="Condition" && row.STATUS_ID!=f.Command.StatusId("well")).ACTIVE_IND="Y";if(fault=="new-condition")f.Native.Rows.OfType<WELL_STATUS>().Single(row=>row.STATUS_ID==f.Command.StatusId("well")).PERCENT_CAPABILITY=99m;if(fault=="unrelated-facet")f.Native.Rows.OfType<WELL_STATUS>().Single(row=>row.STATUS_TYPE=="Role").STATUS="Inject";if(fault=="intent")f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>().Single().AFTER_JSON="{}";if(fault=="foreign-actor")context=context with{ActorUserId="other"};if(fault=="foreign-reference")reference=reference with{IntentId=Guid.NewGuid()};
        if(fault=="uncertain"){var recovered=await f.Well.Owner.RecoverMaintenanceAssetReturnAsync(context,reference,f.Authority);recovered.Receipt.Require(f.Command);}else await Assert.ThrowsAnyAsync<Exception>(()=>f.Well.Owner.RecoverMaintenanceAssetReturnAsync(context,reference,f.Authority));Assert.Equal(writes,f.Native.Writes);
    }
    [Fact]
    public async Task ClosedReturnReaderRetainsOriginalRoleAndClosureBeforeAndInsideOwnedCore()
    {
        var f=new Fixture();await f.Ready();var service=f.Flow.Flow.Flow.Flow.Flow.Flow.Owner.Service;
        var context=f.Flow.Command.Review.Stage.Context;var reference=f.Command.Reference();var writes=f.Native.Writes;
        var source=await service.ReadClosedReturnSourceAsync(context,reference.Query,f.Well.Context.Database.ConnectionName,false,f.Authority);
        Assert.NotNull(source);source.Require();Assert.Equal("FacilityRole",source.Assignment.Stage.AssignedRole);
        Assert.Equal(JsonSerializer.Serialize(CompanyMaintenanceClosureFacts.From(f.Closure)),JsonSerializer.Serialize(source.Closure));
        await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,async()=>
        {
            var owned=await service.ReadClosedReturnSourceAsync(context,reference.Query,f.Well.Context.Database.ConnectionName,true,f.Authority);
            Assert.Equal(JsonSerializer.Serialize(source),JsonSerializer.Serialize(owned));
        });
        Assert.Equal(writes,f.Native.Writes);
        f.Flow.Flow.Revoked=true;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.ReadClosedReturnSourceAsync(context,reference.Query,f.Well.Context.Database.ConnectionName,false,f.Authority));
        Assert.Equal(writes,f.Native.Writes);
    }
    [Fact]
    public async Task SeparateLifecycleIsComparedOutsideAndNeverBorrowedByOwnedCore()
    {
        var f=new Fixture();await f.Ready();var service=f.Flow.Flow.Flow.Flow.Flow.Flow.Owner.Service;
        var context=f.Flow.Command.Review.Stage.Context;var query=f.Command.Reference().Query;
        var core=new PersonaTaskRoutingTests.Fixture();f.Native.Editor.Setup(editor=>editor.GetDataSource("separate-core")).Returns(core.Source.Object);
        var before=await service.ReadClosedReturnSourceAsync(context,query,"separate-core",false,f.Authority);Assert.NotNull(before);
        var reads=f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync");var writes=f.Native.Writes;
        await PPDMDataSourceTransaction.ExecuteAsync(core.Source.Object,async()=>
        {
            Assert.Null(await service.ReadClosedReturnSourceAsync(context,query,"separate-core",true,f.Authority));
            Assert.Equal(reads,f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync"));
        });
        var after=await service.ReadClosedReturnSourceAsync(context,query,"separate-core",false,f.Authority);
        Assert.Equal(JsonSerializer.Serialize(before),JsonSerializer.Serialize(after));Assert.Equal(writes,f.Native.Writes);Assert.Equal(0,core.Writes);
    }
    [Fact]
    public async Task PrivateRecoveryRequiresCurrentWriteRightsForEveryOriginalWell()
    {
        var f=new Fixture();await f.Ready();await f.Record();var writes=f.Native.Writes;f.Well.Level="READ";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Well.Owner.RecoverMaintenanceAssetReturnAsync(f.Well.Context,f.Command.Reference(),f.Authority));
        Assert.Equal(writes,f.Native.Writes);
    }
    [Fact]
    public async Task PartialNativeFailureRollsBackOriginalConditionAndDoesNotLeaveAReplacementIntent()
    {
        var f=new Fixture();await f.Ready();var before=JsonSerializer.Serialize(f.Native.Rows.OfType<WELL_STATUS>().ToArray());f.FailAt=f.Native.Writes+2;await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows.OfType<WELL_STATUS>().ToArray()));Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_ASSET_RETURN>());Assert.Contains("rollback",f.Native.Transactions);
    }
}
#endif
