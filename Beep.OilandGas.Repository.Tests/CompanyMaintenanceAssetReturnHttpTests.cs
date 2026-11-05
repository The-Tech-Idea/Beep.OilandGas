using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    private static async Task VerifyMaintenanceAssetReturnHttp(CompanyPPDMRowAccessTests.Fixture f,HttpClient http,string closureUrl,CompanyWorkOrderCompletionQuery query,string[] wells,RepositoryAssetAccessStore store,string fingerprint,string actor,Func<string> detail)
    {
        var timings=new System.Collections.Concurrent.ConcurrentDictionary<string,(long Count,long Ticks)>();
        var active=new System.Collections.Concurrent.ConcurrentDictionary<string,(string Name,DateTime At)>();
        using var listener=new System.Diagnostics.ActivityListener
        {
            ShouldListenTo=source=>source.Name=="Beep.OilandGas.WorkOrderPermitDecision",
            Sample=(ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> _)=>System.Diagnostics.ActivitySamplingResult.AllData,
            ActivityStarted=activity=>active[activity.Id!]=(activity.OperationName,DateTime.UtcNow),
            ActivityStopped=activity=>{active.TryRemove(activity.Id!,out _);timings.AddOrUpdate(activity.OperationName,(1,activity.Duration.Ticks),(_,value)=>(value.Count+1,value.Ticks+activity.Duration.Ticks));}
        };
        System.Diagnostics.ActivitySource.AddActivityListener(listener);
        var timingLog=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"oilgas-asset-return-http-"+Guid.NewGuid().ToString("N")+".log");
        async Task<HttpResponseMessage> Post(string path,object body)
        {
            timings.Clear();var watch=System.Diagnostics.Stopwatch.StartNew();var route=new Uri(path).AbsolutePath;
            string ProofTiming()=>string.Join(",",timings.OrderByDescending(item=>item.Value.Ticks).Take(20).Select(item=>$"{item.Key}:{item.Value.Count} calls/{TimeSpan.FromTicks(item.Value.Ticks).TotalSeconds:F1}s inclusive").Concat(active.Values.Select(value=>$"{value.Name}:{(DateTime.UtcNow-value.At).TotalSeconds:F1}s active")));
            try
            {
                var response=await http.PostAsJsonAsync(path,body);
                // Only fixed endpoint names, status and timing: no private command, actor, source identity or response body.
                System.IO.File.AppendAllText(timingLog,$"{DateTime.UtcNow:O} {route} {(int)response.StatusCode} {watch.Elapsed.TotalSeconds:F1}s; {ProofTiming()}{Environment.NewLine}");
                return response;
            }
            catch(Exception error) when(error is HttpRequestException or TaskCanceledException)
            {
                var reads=string.Join(",",f.Source.Invocations.Where(call=>call.Method.Name=="GetEntityAsync").GroupBy(call=>call.Arguments[0] as string).OrderByDescending(group=>group.Count()).Take(12).Select(group=>$"{group.Key}:{group.Count()}"));
                var diagnostic=$"Protected {route} failed after {watch.Elapsed.TotalSeconds:F1}s; native read counts {reads}; {ProofTiming()}. Timing log: {timingLog}";
                System.IO.File.AppendAllText(timingLog,diagnostic+Environment.NewLine);throw new InvalidOperationException(diagnostic,error);
            }
        }
        var url=closureUrl.Replace("/closure","/asset-return",StringComparison.Ordinal);var writes=f.Writes.Count;
        var qualified=f.Rows.OfType<WORK_ORDER_COMPONENT>().Any(row=>row.WORK_ORDER_ID==query.Task.WorkOrderId && !string.IsNullOrWhiteSpace(row.EQUIPMENT_ID));
        int Reads(string table)=>f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string==table);
        var reads=Reads("WELL_STATUS");using(var denied=await Post(url+"/review",query))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(reads,Reads("WELL_STATUS"));Assert.Equal(writes,f.Writes.Count);
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="physical-return-role",ClaimType="permission",ClaimValue="Workflow.Execute"});
        var capability=new IdentityRoleClaim<string>{RoleId="physical-return-role",ClaimType="permission",ClaimValue="WellManagement.UpdateWellStatus"};f.Db.RoleClaims.Add(capability);
        if(qualified)foreach(var permission in new[]{"Facilities.Edit","Facilities.ManageEquipment"})f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="physical-return-role",ClaimType="permission",ClaimValue=permission});await f.Db.SaveChangesAsync();
        // The earlier denial invalidated its captured access; reselect the current authoritative RBAC snapshot.
        var resolver=new RepositoryCompanyContextResolver(new RepositoryAccessService(f.Db));var access=await resolver.ResolveAsync(actor,f.Gate.Company.Scope);Assert.NotNull(access);f.CompanySelection.SelectCompany(access);
        reads=Reads("WELL_STATUS");using(var absentWell=await Post(url+"/review",query))Assert.Equal(HttpStatusCode.Forbidden,absentWell.StatusCode);Assert.Equal(reads,Reads("WELL_STATUS"));Assert.Equal(writes,f.Writes.Count);
        foreach(var well in wells)Assert.True(await store.GrantAsync(actor,fingerprint,"WELL",well,"WRITE",false));
        if(qualified){Assert.True(await store.GrantAsync(actor,fingerprint,"FACILITY","plant","WRITE",false,facilityType:"gas"));Assert.True(await store.GrantAsync(actor,fingerprint,"EQUIPMENT","pump","WRITE",false));}
        // Actual declared native physical schema seam. No startup migration or status command is simulated here.
        foreach(var (type,keys) in new[]{(typeof(WELL_STATUS),new[]{"UWI","SOURCE","STATUS_ID"}),(typeof(WORK_ORDER_ASSET_RETURN),new[]{"RETURN_ID"})})
        {
            var table=type.Name;f.Metadata.Setup(metadata=>metadata.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync(keys);
            if(type==typeof(WORK_ORDER_ASSET_RETURN))f.Metadata.Setup(metadata=>metadata.GetTableMetadataAsync(table)).ReturnsAsync(new PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn="RETURN_ID",Module="PPDM_CORE"});
            f.Source.Setup(source=>source.CheckEntityExist(table)).Returns(true);
            f.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName==table),true)).Returns(()=>{var shape=f.Editor.Object.classCreator.ConvertToEntityStructure(type);foreach(var column in shape.Fields)column.IsKey=keys.Contains(column.FieldName,StringComparer.Ordinal);return shape;});
        }
        using(var absent=await Post(url+"/status",query))
        {Assert.True(absent.StatusCode==HttpStatusCode.OK,await absent.Content.ReadAsStringAsync()+detail());Assert.Equal("no-store",absent.Headers.CacheControl?.ToString());var status=(await absent.Content.ReadFromJsonAsync<CompanyMaintenanceAssetReturnStatus>())!;status.Require();Assert.Null(status.Recorded);Assert.Equal(writes,f.Writes.Count);}
        if(qualified)
        {
            using var evidence=await Post(url+"/evidence",query);Assert.True(evidence.StatusCode==HttpStatusCode.OK,await evidence.Content.ReadAsStringAsync()+detail());
            var sources=(await evidence.Content.ReadFromJsonAsync<CompanyMaintenanceAssetEvidence>())!;sources.Require();
            var blockers=sources.Blockers.Where(value=>value!=CompanyMaintenanceAssetEvidence.PendingCommand && value!=CompanyMaintenanceAssetEvidence.PendingQualifiedCommand).ToArray();
            Assert.True(blockers.Length==0,string.Join(Environment.NewLine,blockers));Assert.Equal(writes,f.Writes.Count);
        }
        using var response=await Post(url+"/review",query);Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync()+detail());Assert.Equal("no-store",response.Headers.CacheControl?.ToString());
        var review=(await response.Content.ReadFromJsonAsync<CompanyMaintenanceAssetReturnReview>())!;review.Require();Assert.Equal(wells.Length,review.Assets.Wells.Count);
        var command=new CompanyMaintenanceAssetReturnCommand(review,Guid.NewGuid(),DateTime.UtcNow,review.Assets.Wells.Select(well=>new CompanyWellReturnActivation(well.WellId,well.Facets.Single(row=>row.Facet=="Condition" && row.ActiveIndicator=="Y").Source,73m)).ToArray(),"Responsible facility return with explicit observed native capability",true);
        if(qualified)
        {
            Assert.NotNull(review.Assets.Report.QualifiedWorkAssets);Assert.NotNull(review.Assets.QualifiedAssets);Assert.Empty(review.Assets.QualifiedAssets.ReturnPolicy!.Blockers);
            command=command with{QualifiedAssets=[new("FACILITY","plant","gas",0m),new("EQUIPMENT","pump",null,72.5m)]};command.Require();
        }
        using(var unconfirmed=await Post(url,command with{Confirmed=false}))Assert.Equal(HttpStatusCode.BadRequest,unconfirmed.StatusCode);Assert.Equal(writes,f.Writes.Count);
        var report=JsonSerializer.Serialize(f.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().ToArray());
        using var recorded=await Post(url,command);Assert.True(recorded.StatusCode==HttpStatusCode.OK,await recorded.Content.ReadAsStringAsync()+detail());Assert.Equal("no-store",recorded.Headers.CacheControl?.ToString());
        var saved=(await recorded.Content.ReadFromJsonAsync<CompanyMaintenanceAssetReturnRecovery>())!;saved.Require(f.Gate.Company.Scope,actor,command.Reference());Assert.Equal(writes+2*(wells.Length+(qualified?2:0))+1,f.Writes.Count);writes=f.Writes.Count;
        Assert.Single(f.Rows.OfType<WORK_ORDER_ASSET_RETURN>());Assert.Equal(report,JsonSerializer.Serialize(f.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().ToArray()));Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(saved.Command));
        foreach(var well in wells)Assert.Single(f.Rows.OfType<WELL_STATUS>(),row=>row.UWI==well && row.STATUS_TYPE=="Condition" && row.ACTIVE_IND=="Y" && row.STATUS=="Active" && row.PERCENT_CAPABILITY==73m);
        if(qualified)
        {
            Assert.Single(f.Rows.OfType<FACILITY_STATUS>(),row=>row.FACILITY_ID=="plant" && row.FACILITY_TYPE=="gas" && row.ACTIVE_IND=="Y" && row.STATUS=="RECORDED_TARGET" && row.PERCENT_CAPABILITY==0m);
            Assert.Single(f.Rows.OfType<EQUIPMENT_STATUS>(),row=>row.EQUIPMENT_ID=="pump" && row.ACTIVE_IND=="Y" && row.EQUIP_STATUS=="RECORDED_TARGET" && row.PERCENT_CAPABILITY==72.5m);
            Assert.Equal(2,CompanyMaintenanceAssetReturnFacts.From(saved).QualifiedAssets!.Count);
        }
        foreach(var (path,body) in new[]{(url,(object)command),(url+"/recovery/reference",(object)command.Reference())})
        {using var recovery=await Post(path,body);Assert.True(recovery.StatusCode==HttpStatusCode.OK,await recovery.Content.ReadAsStringAsync()+detail());Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(await recovery.Content.ReadFromJsonAsync<CompanyMaintenanceAssetReturnRecovery>()));Assert.Equal(writes,f.Writes.Count);}
        using(var replacement=await Post(url,command with{IntentId=Guid.NewGuid()}))Assert.Equal(HttpStatusCode.Conflict,replacement.StatusCode);Assert.Equal(writes,f.Writes.Count);
        f.Db.RoleClaims.Remove(capability);await f.Db.SaveChangesAsync();reads=Reads("WORK_ORDER_ASSET_RETURN");using(var revoked=await Post(url+"/recovery/reference",command.Reference()))Assert.Equal(HttpStatusCode.Forbidden,revoked.StatusCode);Assert.Equal(reads,Reads("WORK_ORDER_ASSET_RETURN"));Assert.Equal(writes,f.Writes.Count);
        var execution=f.Db.RoleClaims.Single(row=>row.RoleId=="physical-return-role" && row.ClaimValue=="Workflow.Execute");f.Db.RoleClaims.Remove(execution);await f.Db.SaveChangesAsync();
        foreach(var resource in new[]{("FIELD",query.Task.FieldId),("WORK_ORDER",query.Task.WorkOrderId)})Assert.True(await store.GrantAsync(actor,fingerprint,resource.Item1,resource.Item2,"READ",false));
        foreach(var well in wells)Assert.True(await store.GrantAsync(actor,fingerprint,"WELL",well,"READ",false));
        if(qualified)
        {
            Assert.True(await store.GrantAsync(actor,fingerprint,"FACILITY","plant","READ",false,facilityType:"gas"));Assert.True(await store.GrantAsync(actor,fingerprint,"EQUIPMENT","pump","READ",false));
            f.Db.RoleClaims.RemoveRange(f.Db.RoleClaims.Where(row=>row.RoleId=="physical-return-role" && (row.ClaimValue=="Facilities.Edit" || row.ClaimValue=="Facilities.ManageEquipment")));await f.Db.SaveChangesAsync();
        }
        access=await resolver.ResolveAsync(actor,f.Gate.Company.Scope);Assert.NotNull(access);Assert.DoesNotContain("Workflow.Execute",access.Permissions);Assert.DoesNotContain("WellManagement.UpdateWellStatus",access.Permissions);f.CompanySelection.SelectCompany(access);
        using(var publicResponse=await Post(url+"/status",query))
        {
            Assert.True(publicResponse.StatusCode==HttpStatusCode.OK,await publicResponse.Content.ReadAsStringAsync()+detail());Assert.Equal("no-store",publicResponse.Headers.CacheControl?.ToString());var body=await publicResponse.Content.ReadAsStringAsync();var status=(await publicResponse.Content.ReadFromJsonAsync<CompanyMaintenanceAssetReturnStatus>())!;status.Require();Assert.NotNull(status.Recorded);Assert.Equal(saved.Receipt.ReturnId,status.Recorded.ReturnId);Assert.Equal(saved.Command.Review.Assets.Closure.Revision,status.Recorded.ClosureRevision);Assert.Equal(wells.Length,status.Recorded.Wells.Count);Assert.All(status.CurrentWells,well=>{Assert.Equal("READ",well.AccessLevel);Assert.Contains(well.Facets,row=>row.Facet=="Condition" && row.Value=="Active");});Assert.DoesNotContain(command.IntentId.ToString("D"),body);Assert.DoesNotContain(command.Reason,body);Assert.DoesNotContain("COMMAND_JSON",body);Assert.DoesNotContain("BEFORE_JSON",body);Assert.DoesNotContain("AFTER_JSON",body);Assert.Equal(writes,f.Writes.Count);
        }
        reads=Reads("WORK_ORDER_ASSET_RETURN");using(var stillDenied=await Post(url+"/recovery/reference",command.Reference()))Assert.Equal(HttpStatusCode.Forbidden,stillDenied.StatusCode);Assert.Equal(reads,Reads("WORK_ORDER_ASSET_RETURN"));
        access=await resolver.ResolveAsync(actor,f.Gate.Company.Scope);Assert.NotNull(access);f.CompanySelection.SelectCompany(access);Assert.True(await store.RevokeAsync(actor,fingerprint,"WELL",wells[0]));reads=Reads("WELL_STATUS");
        using(var revokedRead=await Post(url+"/status",query))Assert.Equal(HttpStatusCode.Forbidden,revokedRead.StatusCode);Assert.Equal(reads,Reads("WELL_STATUS"));Assert.Equal(writes,f.Writes.Count);
    }
}
