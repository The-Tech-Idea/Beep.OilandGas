using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    private static async Task VerifyRecordedExecutionHandoffHttp(CompanyPPDMRowAccessTests.Fixture f,HttpHost host,CompanyMaintenanceReceivingQuery task,ModuleConnectionResolver modules,IWorkOrderService orders,bool completionDecision=false,bool returnHandoff=false,bool returnAcceptance=false)
    {
        var core=await modules.ResolveAsync("PPDM_CORE");var database=new AssetDatabaseScope(core,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],core));
        var actualOrder=await orders.GetByIdAsync(task.FieldId,task.WorkOrderId);Assert.NotNull(actualOrder);
        CompanyMaintenanceQualifiedAssets? originalAssets=null;
        if(actualOrder.Components.Any(component=>!string.IsNullOrWhiteSpace(component.EquipmentId) || !string.IsNullOrWhiteSpace(component.FacilityId)))
        {
            await f.Grant("FACILITY","plant","READ",facilityType:"gas");await f.Grant("EQUIPMENT","pump","READ");
            var context=new FieldSelectionReview(f.Gate.Company.Scope,database,"receiver","original-qualified-work-start",task.FieldId);
            originalAssets=await QualifiedHttpReader(f,modules).ReadMaintenanceQualifiedWorkAssetsAsync(context,actualOrder,()=>Task.CompletedTask,false);
        }
        // Already-recorded canonical physical work is the source seam; lifecycle writes and API gates are actual.
        var original=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission(database,task.FieldId,"receiver","Administrator",actualOrder,captureInspectionRows:rows=>{if(completionDecision){f.Rows.RemoveAll(value=>value is WORK_ORDER_INSPECTION inspection && inspection.FIELD_ID==task.FieldId && inspection.WORK_ORDER_ID==task.WorkOrderId);f.Rows.AddRange(rows);}},taskOverride:task,completionVerification:returnHandoff,returnAcceptance:returnAcceptance,qualifiedWorkAssets:originalAssets);
        if(returnHandoff)
        {
            var before=original.Command.Review.Inspections.Current;var rows=f.Rows.OfType<WORK_ORDER_INSPECTION>().Where(value=>value.FIELD_ID==task.FieldId && value.WORK_ORDER_ID==task.WorkOrderId).ToArray();foreach(var check in rows.Where(value=>value.APPLICABLE_STAGE=="WORK_SUBMISSION")){var operationalResult=WorkOrderInspectionRules.PrepareResult(check,new(){FieldId=task.FieldId,InspectionId=check.INSPECTION_ID,ExpectedSourceRevision=WorkOrderInspectionRules.Revision(check),Result="PASS",Notes="Actual controlled operational observation"},"receiver",DateTime.UtcNow);f.Rows[f.Rows.IndexOf(check)]=operationalResult;}
            var current=before with{Requirements=f.Rows.OfType<WORK_ORDER_INSPECTION>().Where(value=>value.FIELD_ID==task.FieldId && value.WORK_ORDER_ID==task.WorkOrderId).Select(WorkOrderInspectionRules.ProjectWithRequirementProof).ToArray()};var evidence=CompanyWorkOrderSubmissionInspectionReview.Create(new(original.Start,original.Started),current,DateTime.UtcNow);original.Command=original.Command with{Review=original.Command.Review with{Inspections=evidence}};original.Command.Require();
        }
        var row=original.Command.Prepare(DateTime.UtcNow);f.Rows.Add(row);
        var creator=new ClassCreator(f.Editor.Object);f.Editor.SetupGet(value=>value.classCreator).Returns(creator);var shape=creator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_SUBMISSION));foreach(var field in shape.Fields){field.IsKey=field.FieldName=="SUBMISSION_ID";if(field.IsKey)field.AllowDBNull=false;}
        f.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_EXECUTION_SUBMISSION")).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(value=>value.EntityName=="WORK_ORDER_EXECUTION_SUBMISSION"),true)).Returns(shape);
        f.Metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_SUBMISSION",EntityTypeName=typeof(WORK_ORDER_EXECUTION_SUBMISSION).FullName!,PrimaryKeyColumn="SUBMISSION_ID",Module="PPDM_CORE"});f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(["SUBMISSION_ID"]);
        var query=new CompanyWorkOrderCompletionQuery(task,row.SUBMISSION_ID);var url=host.Url+"/api/field/current/maintenance-handoffs/execution-permit/report-handoff";var writes=f.Writes.Count;
        int Reads()=>f.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && value.Arguments[0] as string=="WORK_ORDER_EXECUTION_SUBMISSION");
        var reads=Reads();using(var denied=await host.Http.PostAsJsonAsync(url+"/review",query))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(reads,Reads());Assert.Equal(writes,f.Writes.Count);
        var execution=new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.ExecuteWork"};f.Db.RoleClaims.Add(execution);f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Workflow.Execute"});await f.Db.SaveChangesAsync();
        using var reviewed=await host.Http.PostAsJsonAsync(url+"/review",query);Assert.True(reviewed.StatusCode==HttpStatusCode.OK,await reviewed.Content.ReadAsStringAsync());Assert.Equal("no-store",reviewed.Headers.CacheControl?.ToString());var review=(await reviewed.Content.ReadFromJsonAsync<CompanyMaintenanceExecutionHandoffReview>())!;review.Require();Assert.Equal("VerifierRole",review.NextRole);Assert.Equal(writes,f.Writes.Count);
        var command=new CompanyMaintenanceExecutionHandoffCommand(review,Guid.NewGuid(),"Original executor report ready for independent verification");var beforeReport=JsonSerializer.Serialize(row);
        using(var missing=await host.Http.PostAsJsonAsync(url+"/recovery/reference",command.Reference()))Assert.Equal(HttpStatusCode.Conflict,missing.StatusCode);Assert.Equal(writes,f.Writes.Count);
        using var recorded=await host.Http.PostAsJsonAsync(url,command);Assert.True(recorded.StatusCode==HttpStatusCode.OK,await recorded.Content.ReadAsStringAsync());var result=(await recorded.Content.ReadFromJsonAsync<CompanyMaintenanceExecutionHandoffRecovery>())!;result.Require(review.Stage.Context.Company,"receiver",command.Reference());Assert.Single(result.Receipt.ClosedTaskIds);Assert.Equal(2,result.Receipt.ReviewerTaskIds.Count);Assert.True(f.Writes.Count>writes);writes=f.Writes.Count;
        Assert.Equal(beforeReport,JsonSerializer.Serialize(f.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single()));Assert.Equal("verify",f.Rows.OfType<PROCESS_INSTANCE>().Single().CURRENT_STEP_ID);
        foreach(var (path,body) in new[]{(url,(object)command),(url+"/recovery/reference",(object)command.Reference())}){using var response=await host.Http.PostAsJsonAsync(path,body);Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());var saved=(await response.Content.ReadFromJsonAsync<CompanyMaintenanceExecutionHandoffRecovery>())!;Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(saved));Assert.Equal(writes,f.Writes.Count);}
        using(var replacement=await host.Http.PostAsJsonAsync(url,command with{IntentId=Guid.NewGuid()}))Assert.Equal(HttpStatusCode.Conflict,replacement.StatusCode);Assert.Equal(writes,f.Writes.Count);
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Maintenance.VerifyCompletion"});await f.Db.SaveChangesAsync();
        using var discovery=await host.Http.PostAsJsonAsync(host.Url+"/api/field/current/maintenance-handoffs/execution-permit/completion-report",task);Assert.True(discovery.StatusCode==HttpStatusCode.OK,await discovery.Content.ReadAsStringAsync());Assert.Equal("no-store",discovery.Headers.CacheControl?.ToString());Assert.Equal(query,await discovery.Content.ReadFromJsonAsync<CompanyWorkOrderCompletionQuery>());Assert.Equal(writes,f.Writes.Count);Assert.DoesNotContain(command.Reason,await discovery.Content.ReadAsStringAsync());
        f.Db.RoleClaims.Remove(execution);await f.Db.SaveChangesAsync();reads=Reads();using(var revoked=await host.Http.PostAsJsonAsync(url+"/recovery/reference",command.Reference()))Assert.Equal(HttpStatusCode.Forbidden,revoked.StatusCode);Assert.Equal(reads,Reads());Assert.Equal(writes,f.Writes.Count);
        if(completionDecision)await VerifyIndependentCompletionDecisionHttp(f,task,query,returnHandoff,returnAcceptance);
    }
}
