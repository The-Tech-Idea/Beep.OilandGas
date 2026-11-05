#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.DrillingAndConstruction.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Auth.Tests;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingCompletionTestInspectionTests
{
    internal sealed class Fixture
    {
        internal DrillingCompletionReviewStartNativeTests.Fixture Review=new();internal PersonaTaskRoutingTests.Fixture Native=new();
        internal DrillingCompletionTestSelection Selection=null!;internal WELL_TEST Row=null!;internal WELL Well=null!;internal DrillingOperationService Service=null!;
        internal int TestReads,WellReads;internal bool NullRows,FailedRows,Revoked,IgnoreKeys;internal Action<string,int>? OnRead;internal Action<EntityStructure>? ChangeSchema;
        internal async Task Prepare(DrillingCompletionAssignedReview? supplied=null)
        {
            if(supplied is null){await Review.Prepare();await Review.Start();supplied=await DrillingCompletionAssignedStateTests.Packet(Review);}
            Selection=new(supplied,0,"LAB-CERTIFICATES","PRESSURE_TEST","run-1","test-1");var scope=supplied.Source.History.Execution.Basis.Scope();var well=supplied.Source.History.Execution.Basis.Source.Basis.Well;var reported=supplied.Original.Source.ReportedCompletion!.Report.Tests[0];
            Native.Rows.Clear();Native.Connection=scope.CoreDatabase.ConnectionName;Native.Source.SetupGet(value=>value.DatasourceName).Returns(Native.Connection);
            var configured=new ConnectionProperties{ConnectionName=Native.Connection,DatabaseType=DataSourceType.SqlServer,Host="fixture",Database="original-core"};
            var config=new Mock<IConfigEditor>();config.SetupGet(value=>value.DataConnections).Returns(new List<ConnectionProperties>{configured});Native.Editor.SetupGet(value=>value.ConfigEditor).Returns(config.Object);Mock.Get(Native.Source.Object.Dataconnection).SetupGet(value=>value.ConnectionProp).Returns(configured);
            Well=new(){UWI=well.Uwi,ASSIGNED_FIELD=well.FieldId,WELL_NAME=well.Name,ACTIVE_IND="Y"};Assert.Equal(well,DrillingProgramWellFacts.FromRecord(Well));Native.Rows.Add(Well);
            var at=DateTime.UtcNow.AddMinutes(-1);Row=new(){UWI=well.Uwi,SOURCE=Selection.Source,TEST_TYPE=Selection.TestType,RUN_NUM=Selection.RunNumber,TEST_NUM=Selection.TestNumber,ACTIVE_IND="Y",REPORT_TEST_NUM=reported.Reference,TEST_DATE=DateTime.SpecifyKind(reported.TestedAtUtc.UtcDateTime,DateTimeKind.Unspecified),TEST_RESULT_CODE="RECORDED_RESULT_CODE",REMARK="Original signed laboratory result; no threshold inferred",ROW_CREATED_BY="lab-recorder",ROW_CREATED_DATE=at,ROW_CHANGED_BY="lab-recorder",ROW_CHANGED_DATE=at};Native.Rows.Add(Row);
            Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>
            {var schema=new ClassCreator(Native.Editor.Object).ConvertToEntityStructure(request.EntityName=="WELL_TEST"?typeof(WELL_TEST):typeof(WELL));if(request.EntityName=="WELL_TEST"){foreach(var field in schema.Fields)field.IsKey=new[]{"UWI","SOURCE","TEST_TYPE","RUN_NUM","TEST_NUM"}.Contains(field.FieldName);ChangeSchema?.Invoke(schema);}return schema;});
            Native.Source.SetupGet(value=>value.ErrorObject).Returns(new ErrorsInfo{Flag=Errors.Ok});
            Native.Source.Setup(source=>source.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>
            {
                var count=table=="WELL_TEST"?++TestReads:++WellReads;OnRead?.Invoke(table,count);Native.Source.Object.ErrorObject.Flag=FailedRows?Errors.Failed:Errors.Ok;
                return NullRows?null!:Native.Rows.Where(row=>row.GetType().Name==table && (IgnoreKeys || filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue))).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
            });
            Service=new(Native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>());
        }
        internal Task Guard(){if(Revoked)throw new UnauthorizedAccessException("Original reviewer FIELD authority revoked");return Task.CompletedTask;}
        internal Func<Task> Confirm=()=>Task.CompletedTask;
        internal Task<DrillingCompletionTestInspection> Inspect()=>Service.InspectCompletionTestAsync(Selection,Guard,Confirm);
    }
    [Fact] public async Task ExactCompositeTestReadsCompleteSourceAndAuditWithoutRecordingAcceptance()
    {
        var f=new Fixture();await f.Prepare();var before=JsonSerializer.Serialize(f.Native.Rows);var result=await f.Inspect();result.Require(f.Selection);
        Assert.True(result.ReferenceMatches);Assert.True(result.CalendarDateMatches);Assert.Equal(DateTimeKind.Unspecified,result.Native.TestDate.Kind);Assert.Equal("RECORDED_RESULT_CODE",result.Native.ResultCode);
        Assert.Equal(f.Selection.Review.Original.Source.ReportedCompletion!.Report.Tests[0].ReportedResult,result.Reported.ReportedResult);Assert.Equal(3,f.TestReads);Assert.Equal(4,f.WellReads);Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows));Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Theory,InlineData("reference"),InlineData("date")]
    public async Task ExplicitSourceMismatchStaysVisibleAndCannotBecomeAnInferredPass(string field)
    {
        var f=new Fixture();await f.Prepare();if(field=="reference")f.Row.REPORT_TEST_NUM="different-certificate";else f.Row.TEST_DATE=f.Row.TEST_DATE!.Value.AddDays(-1);
        var result=await f.Inspect();Assert.Equal(field!="reference",result.ReferenceMatches);Assert.Equal(field!="date",result.CalendarDateMatches);Assert.Equal(f.Selection.Review.Original.Source.ReportedCompletion!.Report.Tests[0].ReportedResult,result.Reported.ReportedResult);Assert.Equal(0,f.Native.Writes);
    }
    [Theory,InlineData("missing"),InlineData("duplicate"),InlineData("inactive"),InlineData("null"),InlineData("failed"),InlineData("borrowed"),InlineData("foreign-well"),InlineData("wrong-key"),InlineData("audit"),InlineData("local-date"),InlineData("missing-date"),InlineData("schema-key"),InlineData("schema-result")]
    public async Task UnqualifiedSourceKeySchemaAuditAndTransactionNeverProduceInspection(string fault)
    {
        var f=new Fixture();await f.Prepare();switch(fault)
        {
            case "missing":f.Native.Rows.Remove(f.Row);break;case "duplicate":f.Native.Rows.Add(CompanyInventorySourceReview.Copy(f.Row));break;case "inactive":f.Row.ACTIVE_IND="N";break;
            case "null":f.NullRows=true;break;case "failed":f.FailedRows=true;break;case "borrowed":f.Native.Borrowed=true;break;
            case "foreign-well":f.Well.ASSIGNED_FIELD="another-field";break;case "wrong-key":f.IgnoreKeys=true;f.Row.RUN_NUM="wrong-run";break;
            case "audit":f.Row.ROW_CHANGED_DATE=DateTime.UtcNow.AddDays(1);break;case "local-date":f.Row.TEST_DATE=DateTime.Now;break;case "missing-date":f.Row.TEST_DATE=null;break;
            case "schema-key":f.ChangeSchema=s=>s.Fields.ForEach(field=>field.IsKey=false);break;case "schema-result":f.ChangeSchema=s=>s.Fields.RemoveAll(field=>field.FieldName=="TEST_RESULT_CODE");break;
        }
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Inspect());Assert.Equal(0,f.Native.Writes);
    }
    [Theory,InlineData("result"),InlineData("unprojected-value"),InlineData("revoke"),InlineData("closing-well"),InlineData("binding"),InlineData("closing-source")]
    public async Task MidReadAndClosingSourceChangesAreRefused(string fault)
    {
        var f=new Fixture();await f.Prepare();f.OnRead=(table,count)=>
        {
            if(table=="WELL_TEST" && count==2){if(fault=="result")f.Row.TEST_RESULT_CODE="changed";if(fault=="unprojected-value")f.Row.CASING_PRESSURE=25m;if(fault=="revoke")f.Revoked=true;if(fault=="binding")f.Native.Editor.Setup(value=>value.GetDataSource(It.IsAny<string>())).Returns(new PersonaTaskRoutingTests.Fixture().Source.Object);}
            if(table=="WELL" && count==4 && fault=="closing-well")f.Row.REMARK="Changed during closing well read";
        };
        if(fault=="closing-source")f.Confirm=()=>{f.Row.TEST_RESULT_CODE="Changed during final program/resource confirmation";return Task.CompletedTask;};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Inspect());Assert.Equal(0,f.Native.Writes);
    }
}
#endif
