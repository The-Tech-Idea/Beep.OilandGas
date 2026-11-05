using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.DrillingAndConstruction.Services;
public partial class DrillingOperationService
{
    public async Task<DrillingCompletionTestInspection> InspectCompletionTestAsync(DrillingCompletionTestSelection input,Func<Task> authority,Func<Task> confirmSource)
    {
        input.Require();var selection=CompanyInventorySourceReview.Copy(input);var scope=selection.Review.Source.History.Execution.Basis.Scope();var core=scope.CoreDatabase;
        var read=await ReadNativeCompletionTestAsync(scope,selection.Review.Source.History.Execution.Basis.Source.Basis.Well,selection.Source,selection.TestType,selection.RunNumber,selection.TestNumber,authority,confirmSource);
        var result=new DrillingCompletionTestInspection(selection,read.Facts,read.Well,"",read.CheckedAt);result=result with{Revision=result.ContentRevision()};result.Require(selection);return result;
    }
    public async Task<DrillingAcceptedCompletionTests> ReadAcceptedCompletionTestsAsync(DrillingCompletionReviewProgress input,Func<Task> authority,Func<Task> confirmSource)
    {
        var accepted=CompanyInventorySourceReview.Copy(input);var scope=accepted.Source.History.Execution.Basis.Scope();accepted.RequireFacts(scope.Company,scope.ActorUserId,accepted.InstanceId);
        if(!accepted.OriginalCompletionAccepted || !accepted.AcceptedClaimIsCurrent)throw new InvalidOperationException("A current independently accepted completion claim is required before reporting closure.");
        var expected=accepted.Stages.Last().AcceptedDecision!.Acceptance!.Tests.OrderBy(test=>test.ReportedTestIndex).Select(test=>test.Native).ToArray();var well=accepted.Source.History.Execution.Basis.Source.Basis.Well;
        async Task<DrillingNativeWellTest[]> Read()
        {
            var facts=new List<DrillingNativeWellTest>();
            foreach(var test in expected)
            {
                var current=await ReadNativeCompletionTestAsync(scope,well,test.Source,test.TestType,test.RunNumber,test.TestNumber,authority,()=>Task.CompletedTask);
                if(current.Facts!=test)throw new InvalidOperationException("An accepted physical test changed before current reporting readiness could be confirmed.");facts.Add(current.Facts);
            }
            return facts.ToArray();
        }
        var first=await Read();await confirmSource();await authority();var closing=await Read();
        if(!first.SequenceEqual(closing))throw new InvalidOperationException("The accepted native test population changed around closing source proof.");
        var result=new DrillingAcceptedCompletionTests(accepted,closing,well,"",DateTimeOffset.UtcNow);result=result with{Revision=result.ContentRevision()};result.Require(scope.Company,scope.ActorUserId,accepted.InstanceId);await authority();return result;
    }
    private sealed record NativeCompletionTestRead(DrillingNativeWellTest Facts,DrillingProgramWellFacts Well,DateTimeOffset CheckedAt);
    private async Task<NativeCompletionTestRead> ReadNativeCompletionTestAsync(DevelopmentAssignedReviewScope scope,DrillingProgramWellFacts expectedWell,string nativeSource,string testType,string run,string number,Func<Task> authority,Func<Task> confirmSource)
    {
        ArgumentNullException.ThrowIfNull(authority);ArgumentNullException.ThrowIfNull(confirmSource);APInvoiceOwnership.RequireScope(scope.Company);
        if(expectedWell.FieldId!=scope.FieldId || !LeaseSummaryValidator.Identifier(scope.ActorUserId) || !new[]{nativeSource,testType,run,number}.All(value=>!string.IsNullOrWhiteSpace(value) && value==value.Trim() && value.Length<=128 && !value.Any(char.IsControl)))throw new InvalidOperationException("The owner-admitted original well/FIELD, current reader and exact native test key are required.");
        var core=scope.CoreDatabase;
        var source=_editor.GetDataSource(core.ConnectionName)??throw new InvalidOperationException("The original native test source is unavailable.");
        async Task Guard()
        {
            PPDMDataSourceTransaction.RequireIdle(source);await authority();
            if(!ReferenceEquals(source,_editor.GetDataSource(core.ConnectionName)))throw new InvalidOperationException("The original native test source changed.");
            MigrationConnectionTarget.Validate(_editor,source,core.ConnectionName);PPDMDataSourceTransaction.RequireIdle(source);
        }
        void Schema()
        {
            var schema=source.GetEntityStructure(new EntityStructure{EntityName="WELL_TEST",DatasourceEntityName="WELL_TEST"},true)??throw new InvalidOperationException("Qualify the existing WELL_TEST schema through Beep Migration before inspection.");
            var keys=new[]{"UWI","SOURCE","TEST_TYPE","RUN_NUM","TEST_NUM"};
            if(schema.Fields is null || !schema.Fields.Where(field=>field.IsKey).Select(field=>field.FieldName).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal)))throw new InvalidOperationException("The native test requires its exact five-part PPDM key.");
            foreach(var name in keys.Concat(new[]{"REPORT_TEST_NUM","TEST_RESULT_CODE","REMARK","ACTIVE_IND","ROW_CREATED_BY","ROW_CHANGED_BY","TEST_DATE","ROW_CREATED_DATE","ROW_CHANGED_DATE"}))
            {
                var fields=schema.Fields.Where(field=>field.FieldName==name).ToArray();var type=name is "TEST_DATE" or "ROW_CREATED_DATE" or "ROW_CHANGED_DATE"?typeof(DateTime):typeof(string);
                if(fields.Length!=1 || fields[0].Fieldtype!=type.FullName)throw new InvalidOperationException("The native test schema column "+name+" is unconfirmed.");
            }
        }
        var repository=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WELL_TEST),core.ConnectionName,"WELL_TEST");
        async Task<DrillingNativeWellTest> Read()
        {
            await Guard();Schema();AppFilter Eq(string name,string value)=>new(){FieldName=name,FilterValue=value,Operator="="};var uwi=expectedWell.Uwi;
            var rows=(await repository.GetAsync([Eq("UWI",uwi),Eq("SOURCE",nativeSource),Eq("TEST_TYPE",testType),Eq("RUN_NUM",run),Eq("TEST_NUM",number)]))?.ToArray()??throw new InvalidOperationException("The exact native test could not be read.");
            if(rows.Length!=1 || rows[0] is not WELL_TEST row || row.ACTIVE_IND!="Y" || row.TEST_DATE is not {} date || row.ROW_CREATED_DATE is not {} created || row.ROW_CHANGED_DATE is not {} changed)throw new InvalidOperationException("The exact active native test is missing, ambiguous or lacks accountable date/audit evidence.");
            var json=JsonSerializer.Serialize(row);if(json.Length>1024*1024)throw new InvalidOperationException("The complete native test exceeds its supported evidence bound.");
            var result=new DrillingNativeWellTest(row.UWI,row.SOURCE,row.TEST_TYPE,row.RUN_NUM,row.TEST_NUM,row.REPORT_TEST_NUM,date,row.TEST_RESULT_CODE,row.REMARK,row.ROW_CREATED_BY,created,row.ROW_CHANGED_BY,changed,ProcessDefinitionRevision.FromJson(json));
            result.Require(expectedWell,nativeSource,testType,run,number,DateTimeOffset.UtcNow);await Guard();return result;
        }
        await Guard();var well=await ReadProgramWellAsync(core,scope.FieldId,expectedWell.Uwi,false,Guard);
        if(well!=expectedWell)throw new InvalidOperationException("The original completion well/FIELD changed.");
        var before=await Read();var after=await Read();if(before!=after)throw new InvalidOperationException("The complete native test changed during inspection.");
        var closing=await ReadProgramWellAsync(core,scope.FieldId,well.Uwi,false,Guard);if(closing!=well)throw new InvalidOperationException("The native test well/FIELD changed during inspection.");
        await confirmSource();await Guard();
        var final=await Read();if(final!=after)throw new InvalidOperationException("The native test changed during closing source and well/FIELD checks.");
        await Guard();return new(final,closing,DateTimeOffset.UtcNow);
    }
}
