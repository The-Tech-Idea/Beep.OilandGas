using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed partial class DrillingExecutionStartApiTests
{
    private static async Task ExerciseCompletionTestInspection(DrillingProgramApiTests.Fixture f,DrillingServiceClient receiver,HttpClient http,DrillingCompletionAssignedReview assigned)
    {
        var selection=new DrillingCompletionTestSelection(assigned,0,"LAB-NATIVE","PRESSURE_TEST","run-1","test-1");var reference=assigned.Original.Source.ReportedCompletion!.Report.Tests[0];var now=DateTime.UtcNow.AddMinutes(-1);
        var row=new WELL_TEST{UWI=f.Well.UWI,SOURCE=selection.Source,TEST_TYPE=selection.TestType,RUN_NUM=selection.RunNumber,TEST_NUM=selection.TestNumber,ACTIVE_IND="Y",REPORT_TEST_NUM=reference.Reference,TEST_DATE=DateTime.SpecifyKind(reference.TestedAtUtc.UtcDateTime,DateTimeKind.Unspecified),TEST_RESULT_CODE="NATIVE_UNACCEPTED_RESULT",REMARK="Exact native signed test evidence; no threshold inferred",ROW_CREATED_BY="test-laboratory",ROW_CREATED_DATE=now,ROW_CHANGED_BY="test-laboratory",ROW_CHANGED_DATE=now};f.Core.Rows.Add(row);
        var editor=f.Funding.Routing.Catalog.Draft.Drafts.Native.Editor.Object;
        f.Core.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(value=>value.EntityName=="WELL_TEST"),true)).Returns((EntityStructure _,bool refresh)=>{var schema=new ClassCreator(editor).ConvertToEntityStructure(typeof(WELL_TEST));foreach(var field in schema.Fields)field.IsKey=new[]{"UWI","SOURCE","TEST_TYPE","RUN_NUM","TEST_NUM"}.Contains(field.FieldName);return schema;});
        var before=JsonSerializer.Serialize(f.Core.Rows);var writes=f.Funding.Routing.Catalog.Workflow.Native.Writes;
        var found=await MeasureCompletion("native-test-inspection",()=>receiver.InspectCompletionTestAsync(selection));found.Require(selection);Assert.True(found.ReferenceMatches);Assert.True(found.CalendarDateMatches);Assert.Equal(reference.ReportedResult,found.Reported.ReportedResult);Assert.Equal("NATIVE_UNACCEPTED_RESULT",found.Native.ResultCode);Assert.Equal(before,JsonSerializer.Serialize(f.Core.Rows));Assert.Equal(writes,f.Funding.Routing.Catalog.Workflow.Native.Writes);
        var changed=assigned with{TaskRevision=new('F',64)};changed=changed with{Revision=changed.ContentRevision()};Assert.Equal(HttpStatusCode.Conflict,(await http.PostAsJsonAsync("/api/drilling/programs/execution/completion/test/inspect",selection with{Review=changed})).StatusCode);
        var db=f.Funding.Routing.Afe.Db;var grant=await db.Set<AppUserAssetAccess>().SingleAsync(value=>value.Id=="completion-inspection-field");grant.IsActive=false;await db.SaveChangesAsync();
        Assert.Equal(HttpStatusCode.Forbidden,(await http.PostAsJsonAsync("/api/drilling/programs/execution/completion/test/inspect",selection)).StatusCode);grant.IsActive=true;await db.SaveChangesAsync();
        var revoke=true;var changeGraph=false;var reads=0;var pending=f.Funding.Routing.Catalog.Workflow.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single(value=>value.CROSS_TASK_ID==assigned.TaskId);var context=pending.TASK_CONTEXT_JSON;
        f.Core.Source.Setup(source=>source.GetEntityAsync("WELL_TEST",It.IsAny<List<AppFilter>>())).ReturnsAsync((string _,List<AppFilter> filters)=>
        {reads++;if(revoke){revoke=false;grant.IsActive=false;db.SaveChanges();}if(changeGraph && reads==3)pending.TASK_CONTEXT_JSON="Changed during closing native test read";return f.Core.Rows.OfType<WELL_TEST>().Where(value=>filters.All(filter=>value.GetType().GetProperty(filter.FieldName)?.GetValue(value)?.ToString()==filter.FilterValue)).Select(CompanyInventorySourceReview.Copy).ToArray();});
        Assert.Equal(HttpStatusCode.Forbidden,(await http.PostAsJsonAsync("/api/drilling/programs/execution/completion/test/inspect",selection)).StatusCode);grant.IsActive=true;await db.SaveChangesAsync();Assert.Equal(writes,f.Funding.Routing.Catalog.Workflow.Native.Writes);Assert.Equal(before,JsonSerializer.Serialize(f.Core.Rows));
        var graphBefore=JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows);
        reads=0;changeGraph=true;Assert.Equal(HttpStatusCode.Conflict,(await http.PostAsJsonAsync("/api/drilling/programs/execution/completion/test/inspect",selection)).StatusCode);Assert.Equal(3,reads);
        var persisted=f.Funding.Routing.Catalog.Workflow.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single(value=>value.CROSS_TASK_ID==assigned.TaskId);Assert.NotEqual(context,persisted.TASK_CONTEXT_JSON);persisted.TASK_CONTEXT_JSON=context;
        Assert.Equal(graphBefore,JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows));
        await f.Funding.Routing.Catalog.Workflow.Service.ReadRecordedCompletionReviewAsync(assigned.Source.History.Execution.Basis.Scope().Company,assigned.InstanceId,()=>Task.CompletedTask);
        Assert.Equal(writes,f.Funding.Routing.Catalog.Workflow.Native.Writes);Assert.Equal(before,JsonSerializer.Serialize(f.Core.Rows));
    }
}
#endif
