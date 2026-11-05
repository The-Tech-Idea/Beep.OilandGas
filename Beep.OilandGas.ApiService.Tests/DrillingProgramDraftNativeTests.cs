#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.DrillingAndConstruction.Services;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingProgramDraftNativeTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DevelopmentInvestmentStageNativeTests.Fixture Investment=new();
        internal DevelopmentPlanNativeDraftTests.Fixture Store=new();
        internal CreateDrillingProgramDraft Command=null!;internal DrillingProgramBasis Current=null!;internal DrillingOperationService Service=null!;
        internal bool Revoked,FailInsert,Corrupt,NullRows;internal Action? OnInsert;internal Action<EntityStructure>? ChangeSchema;
        internal async Task Prepare()
        {
            await Investment.Prepare();await Investment.Reserve();Investment.Financial();await Investment.Complete();var status=await Beep.OilandGas.Web.Auth.Tests.DevelopmentInvestmentProgressScreenTests.Status(Investment);
            var well=new WELL{UWI="recorded-well/one",ASSIGNED_FIELD=status.Source.Current.Source.Review.Scope.FieldId,WELL_NAME="Recorded target well",ACTIVE_IND="Y"};
            Current=new(status,DrillingProgramWellFacts.FromRecord(well),new("planning","original-drilling-binding"),"",DateTimeOffset.UtcNow);Current=Current with{Revision=Current.ContentRevision()};
            var start=status.Source.Afe().EffectiveDate.AddDays(2);Command=new(Current,Guid.NewGuid(),"Original drilling program","Explicit program objectives / target evidence","DEVELOPMENT","VERTICAL",2500.25m,2500m,"m",25,start,start.AddDays(30));Command.Require();
            var native=Store.Native;native.Rows.Clear();
            native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool refresh)=>{var result=new ClassCreator(native.Editor.Object).ConvertToEntityStructure(typeof(DRILLING_PROGRAM));ChangeSchema?.Invoke(result);return result;});
            native.Source.Setup(source=>source.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>NullRows?null!:native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray());
            native.Source.Setup(source=>source.InsertEntity("DRILLING_PROGRAM",It.IsAny<object>())).Returns((string table,object input)=>{native.Writes++;if(FailInsert)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((DRILLING_PROGRAM)input);if(Corrupt)row.PROGRAM_NAME="Unexpected native edit";native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            Service=new(native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>());
        }
        internal Task Guard(){if(Revoked)throw new UnauthorizedAccessException("Current program authority changed.");return Task.CompletedTask;}
        internal Task<DrillingProgramBasis> Basis(){Assert.DoesNotContain("begin",Store.Native.Transactions.TakeLast(1));return Task.FromResult(Current);}
        internal Task<DrillingProgramDraftReceipt> Save(bool recovery=false)=>Service.RecordProgramDraftAsync(Command,Guard,Basis,recovery);
        public void Dispose()=>Investment.Dispose();
    }
    [Fact]public async Task ExactApprovedSourceCreatesOneDraftAndRetryRecoveryNeverTouchesWellOrFinance()
    {
        using var f=new Fixture();await f.Prepare();var wellRows=JsonSerializer.Serialize(f.Investment.Handoff.Native.Native.Rows);var financial=f.Investment.Current.Recorded;
        f.OnInsert=()=>{
            var observed=f.Store.Native.Rows.OfType<DRILLING_PROGRAM>().Single();var restored=JsonSerializer.Deserialize<CreateDrillingProgramDraft>(observed.PROGRAM_DRAFT_JSON)!;var expected=restored.ToRecord(DateTime.SpecifyKind(observed.ROW_CREATED_DATE!.Value,DateTimeKind.Utc));expected.ROW_CHANGED_DATE=observed.ROW_CHANGED_DATE;expected.PROGRAM_DRAFT_REVISION=CreateDrillingProgramDraft.Digest(expected);
            var actualJson=JsonSerializer.SerializeToElement(observed);var expectedJson=JsonSerializer.SerializeToElement(expected);var differing=actualJson.EnumerateObject().Where(field=>field.Value.GetRawText()!=expectedJson.GetProperty(field.Name).GetRawText()).Select(field=>field.Name+" actual="+field.Value.GetRawText()[..Math.Min(90,field.Value.GetRawText().Length)]+" expected="+expectedJson.GetProperty(field.Name).GetRawText()[..Math.Min(90,expectedJson.GetProperty(field.Name).GetRawText().Length)]).ToArray();Assert.True(differing.Length==0,string.Join("; ",differing));};
        var result=await f.Save();result.Require(f.Command);var row=Assert.IsType<DRILLING_PROGRAM>(result.Recorded);Assert.Equal("DRAFT",row.PROGRAM_STATUS);Assert.Equal(1,row.PROGRAM_VERSION);Assert.Equal(f.Command.Basis.Funding.Source.Afe().RequestedAmount,row.AFE_COST);Assert.Equal("KWD",row.AFE_CURRENCY);Assert.Equal(f.Command.MeasuredDepth,row.PLANNED_MEASURED_DEPTH);Assert.Equal("m",row.DEPTH_OUOM);Assert.Equal("",row.RIG_NAME);Assert.Equal("",row.DRILLING_CONTRACTOR);
        var again=await f.Save();Assert.Equal(result.Recorded.PROGRAM_ID,again.Recorded!.PROGRAM_ID);Assert.NotNull((await f.Save(true)).Recorded);Assert.Equal(1,f.Store.Native.Writes);Assert.Equal(financial,f.Investment.Current.Recorded);Assert.Equal(wellRows,JsonSerializer.Serialize(f.Investment.Handoff.Native.Native.Rows));
        result.Recorded.PROGRAM_NAME="consumer edit";Assert.Equal(f.Command.Name,(await f.Save(true)).Recorded!.PROGRAM_NAME);
    }
    [Fact]public async Task ConfirmedAbsenceIsReadOnlyAndUnavailableRowsCannotBecomeAbsence()
    {using var f=new Fixture();await f.Prepare();Assert.Null((await f.Save(true)).Recorded);Assert.Equal(0,f.Store.Native.Writes);f.NullRows=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Save(true));Assert.Equal(0,f.Store.Native.Writes);}
    [Theory][InlineData("name")][InlineData("depth")][InlineData("dates")]
    public async Task OriginalIntentCannotBeReusedWithChangedProgramInputs(string fault)
    {
        using var f=new Fixture();await f.Prepare();await f.Save();f.Command=fault switch{"name"=>f.Command with{Name="Different program"},"depth"=>f.Command with{MeasuredDepth=2600m},_=>f.Command with{PlannedCompletionDate=f.Command.PlannedCompletionDate.AddDays(1)}};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Save());Assert.Equal(1,f.Store.Native.Writes);
    }
    [Theory][InlineData("key")][InlineData("owner")][InlineData("proof-width")][InlineData("missing-column")][InlineData("read-committed")][InlineData("borrowed")]
    public async Task UnqualifiedSchemaOrTransactionCannotInsertAProgram(string fault)
    {
        using var f=new Fixture();await f.Prepare();f.ChangeSchema=s=>{if(fault=="key")s.Fields.ForEach(field=>field.IsKey=false);if(fault=="owner")s.Fields.Single(field=>field.FieldName=="OWNER_TENANT_ID").AllowDBNull=true;if(fault=="proof-width"){var field=s.Fields.Single(field=>field.FieldName=="PROGRAM_DRAFT_JSON");field.Size=1;field.Size1=1;}if(fault=="missing-column")s.Fields.RemoveAll(field=>field.FieldName=="INVESTMENT_OUTCOME_REVISION");};
        if(fault=="read-committed")f.Store.Native.Isolation=IsolationLevel.ReadCommitted;if(fault=="borrowed")f.Store.Native.Borrowed=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Save());Assert.Equal(0,f.Store.Native.Writes);Assert.Empty(f.Store.Native.Rows);
    }
    [Theory][InlineData("failed")][InlineData("corrupt")][InlineData("revoked")]
    public async Task NativeFailureOrMidWriteRevocationRollsBackTheWholeDraft(string fault)
    {
        using var f=new Fixture();await f.Prepare();f.FailInsert=fault=="failed";f.Corrupt=fault=="corrupt";if(fault=="revoked")f.OnInsert=()=>f.Revoked=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Save());Assert.Empty(f.Store.Native.Rows);Assert.Contains("rollback",f.Store.Native.Transactions);
    }
    [Fact]public async Task LostCommitAcknowledgementRecoversTheSameOriginalWithoutAnotherInsert()
    {
        using var f=new Fixture();await f.Prepare();f.Store.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Save());Assert.Single(f.Store.Native.Rows);f.Store.Native.FailCommit=false;var recovered=await f.Save(true);Assert.NotNull(recovered.Recorded);Assert.Equal(1,f.Store.Native.Writes);
    }
    [Theory][InlineData("well")][InlineData("funding")][InlineData("binding")]
    public async Task ChangedReviewedSourceCannotWriteOrConfirmAnOriginalProgram(string fault)
    {
        using var f=new Fixture();await f.Prepare();f.Current=fault switch{"well"=>f.Current with{Well=f.Current.Well with{Revision=new('E',64)}},"binding"=>f.Current with{ExecutionDatabase=new("changed","changed-binding")},_=>f.Current with{Funding=f.Current.Funding with{Source=f.Current.Funding.Source with{Recorded=null}}}};f.Current=f.Current with{Revision=f.Current.ContentRevision()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Save());Assert.Equal(0,f.Store.Native.Writes);
    }
    [Fact]public async Task ActualProgramReloadReturnsOriginalRowsAndDoesNotConvertFailureIntoEmptyWork()
    {
        using var f=new Fixture();await f.Prepare();Assert.Empty((await f.Service.ReadProgramDraftsAsync(f.Current,f.Guard)).Drafts);await f.Save();var first=await f.Service.ReadProgramDraftsAsync(f.Current,f.Guard);Assert.Equal(f.Command.ProgramId(),Assert.Single(first.Drafts).PROGRAM_ID);first.Drafts[0].PROGRAM_NAME="changed consumer";Assert.Equal(f.Command.Name,Assert.Single((await f.Service.ReadProgramDraftsAsync(f.Current,f.Guard)).Drafts).PROGRAM_NAME);
        f.Store.Native.Rows.OfType<DRILLING_PROGRAM>().Single().PROGRAM_NAME="changed native";await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.ReadProgramDraftsAsync(f.Current,f.Guard));f.NullRows=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.ReadProgramDraftsAsync(f.Current,f.Guard));Assert.Equal(1,f.Store.Native.Writes);
    }
    [Fact]public async Task HistoricalOriginalRecoveryDoesNotRequireTheOldMutableWellNameOrHash()
    {
        using var f=new Fixture();await f.Prepare();await f.Save();f.Current=f.Current with{Well=f.Current.Well with{Name="Later well metadata",Revision=new('F',64)}};f.Current=f.Current with{Revision=f.Current.ContentRevision()};var result=await f.Save(true);Assert.NotNull(result.Recorded);Assert.Equal(f.Command.Basis.Well.Revision,CreateDrillingProgramDraft.FromRecord(result.Recorded!).Basis.Well.Revision);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Save());Assert.Equal(1,f.Store.Native.Writes);
    }
    [Fact]public async Task GenericRepositoryCannotBypassTheReviewedDrillingProgramOwner()
    {
        using var f=new Fixture();await f.Prepare();var repo=new PPDMGenericRepository(f.Store.Native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),typeof(DRILLING_PROGRAM),"planning","DRILLING_PROGRAM");var row=f.Command.ToRecord(DateTime.UtcNow);
        await Assert.ThrowsAnyAsync<Exception>(()=>repo.InsertAsync(row,"unqualified"));await Assert.ThrowsAnyAsync<Exception>(()=>repo.UpdateAsync(row,"unqualified"));await Assert.ThrowsAnyAsync<Exception>(()=>repo.InsertBatchAsync([row],"unqualified"));Assert.Equal(0,f.Store.Native.Writes);Assert.Empty(f.Store.Native.Rows);
    }
    [Theory][InlineData("unit")][InlineData("tvd")][InlineData("date")][InlineData("profile")]
    public async Task InvalidExplicitProgramInputsCannotReachNativeWrites(string fault)
    {
        using var f=new Fixture();await f.Prepare();f.Command=fault switch{"unit"=>f.Command with{DepthUnit=""},"tvd"=>f.Command with{VerticalDepth=f.Command.MeasuredDepth+1},"date"=>f.Command with{PlannedSpudDate=f.Command.Basis.Funding.Source.Afe().EffectiveDate.AddDays(-1)},_=>f.Command with{Profile=""}};await Assert.ThrowsAnyAsync<Exception>(()=>f.Save());Assert.Equal(0,f.Store.Native.Writes);
    }
    [Fact]public async Task CanonicalModelAndBeepMigrationShapeHaveOneBoundedOwner()
    {
        using var f=new Fixture();await f.Prepare();Assert.Equal(typeof(CreateDrillingProgramDraft).Assembly,typeof(DRILLING_PROGRAM).Assembly);var schema=new ClassCreator(f.Store.Native.Editor.Object).ConvertToEntityStructure(typeof(DRILLING_PROGRAM));var key=Assert.Single(schema.Fields,field=>field.IsKey);Assert.Equal("PROGRAM_ID",key.FieldName);Assert.False(key.AllowDBNull);Assert.Equal(128,key.Size1);
        foreach(var name in new[]{"OWNER_TENANT_ID","OWNER_COMPANY_BA_ID","PROGRAM_DRAFT_JSON","PROGRAM_DRAFT_REVISION","AFE_ID","INVESTMENT_OUTCOME_REVISION"})Assert.False(schema.Fields.Single(field=>field.FieldName==name).AllowDBNull);Assert.Equal(CreateDrillingProgramDraft.MaximumBodyLength,schema.Fields.Single(field=>field.FieldName=="PROGRAM_DRAFT_JSON").Size1);
    }
}
#endif
