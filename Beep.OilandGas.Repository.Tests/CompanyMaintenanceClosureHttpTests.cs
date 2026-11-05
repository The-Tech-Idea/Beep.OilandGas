using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    private static async Task VerifyMaintenanceClosureHttp(CompanyPPDMRowAccessTests.Fixture f,HttpClient http,string acceptanceUrl,CompanyWorkOrderCompletionQuery query,CompanyWorkOrderReturnRecovery originalAcceptance,Func<string> detail)
    {
        var url=acceptanceUrl.Replace("/return-acceptance","/closure",StringComparison.Ordinal);var writes=f.Writes.Count;
        int Reads()=>f.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && value.Arguments[0] as string=="WORK_ORDER_EXECUTION_SUBMISSION");var reads=Reads();using(var denied=await http.PostAsJsonAsync(url+"/review",query))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(reads,Reads());Assert.Equal(writes,f.Writes.Count);
        using(var absent=await http.PostAsJsonAsync(url+"/status",query)){Assert.True(absent.StatusCode==HttpStatusCode.OK,await absent.Content.ReadAsStringAsync()+detail());Assert.Null((await absent.Content.ReadFromJsonAsync<CompanyMaintenanceClosureStatus>())!.Recorded);}
        var execute=new IdentityRoleClaim<string>{RoleId="physical-return-role",ClaimType="permission",ClaimValue="Workflow.Execute"};f.Db.RoleClaims.Add(execute);await f.Db.SaveChangesAsync();
        using var response=await http.PostAsJsonAsync(url+"/review",query);Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync()+detail());var review=(await response.Content.ReadFromJsonAsync<CompanyMaintenanceClosureReview>())!;review.Require();Assert.Equal(originalAcceptance.Receipt.Revision,review.Acceptance.Revision);Assert.Equal(originalAcceptance.Command.Review.PhysicalSubjectRevision(),review.Current.PhysicalSubjectRevision());Assert.Equal(writes,f.Writes.Count);
        var command=new CompanyMaintenanceClosureCommand(review,Guid.NewGuid(),"Original confirmed final return duty closure",true);var report=JsonSerializer.Serialize(f.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single());var approval=JsonSerializer.Serialize(f.Rows.OfType<PROCESS_APPROVAL>().ToArray());
        using var closed=await http.PostAsJsonAsync(url,command);Assert.True(closed.StatusCode==HttpStatusCode.OK,await closed.Content.ReadAsStringAsync()+detail());Assert.Equal("no-store",closed.Headers.CacheControl?.ToString());var saved=(await closed.Content.ReadFromJsonAsync<CompanyMaintenanceClosureRecovery>())!;saved.Require(review.Stage.Context.Company,review.Stage.Context.ActorUserId,command.Reference());Assert.Equal(writes+saved.Receipt.ClosedTaskIds.Count+3,f.Writes.Count);writes=f.Writes.Count;Assert.Equal("COMPLETED",f.Rows.OfType<PROCESS_INSTANCE>().Single().STATUS);Assert.Equal(report,JsonSerializer.Serialize(f.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single()));Assert.Equal(approval,JsonSerializer.Serialize(f.Rows.OfType<PROCESS_APPROVAL>().ToArray()));Assert.All(f.Rows.OfType<CROSS_PERSONA_TASK>(),row=>Assert.Equal("COMPLETED",row.TASK_STATUS));
        foreach(var (path,body) in new[]{(url,(object)command),(url+"/recovery/reference",(object)command.Reference())}){using var retry=await http.PostAsJsonAsync(path,body);Assert.True(retry.StatusCode==HttpStatusCode.OK,await retry.Content.ReadAsStringAsync()+detail());Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(await retry.Content.ReadFromJsonAsync<CompanyMaintenanceClosureRecovery>()));Assert.Equal(writes,f.Writes.Count);}
        using(var prior=await http.PostAsJsonAsync(acceptanceUrl+"/recovery/reference",originalAcceptance.Command.Reference())){Assert.True(prior.StatusCode==HttpStatusCode.OK,await prior.Content.ReadAsStringAsync()+detail());Assert.Equal(JsonSerializer.Serialize(originalAcceptance),JsonSerializer.Serialize(await prior.Content.ReadFromJsonAsync<CompanyWorkOrderReturnRecovery>()));}
        using(var replacement=await http.PostAsJsonAsync(url,command with{IntentId=Guid.NewGuid()}))Assert.Equal(HttpStatusCode.Conflict,replacement.StatusCode);Assert.Equal(writes,f.Writes.Count);
        f.Db.RoleClaims.Remove(execute);await f.Db.SaveChangesAsync();reads=Reads();using(var denied=await http.PostAsJsonAsync(url+"/recovery/reference",command.Reference()))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(reads,Reads());
        // The previous request correctly invalidated its captured company access. Select a fresh
        // authoritative app-RBAC snapshot before proving that public status needs no Execute grant.
        var refreshed=await new RepositoryCompanyContextResolver(new RepositoryAccessService(f.Db)).ResolveAsync(f.Gate.Company.UserId,f.Gate.Company.Scope);Assert.NotNull(refreshed);Assert.DoesNotContain("Workflow.Execute",refreshed.Permissions);f.CompanySelection.SelectCompany(refreshed);
        using(var statusResponse=await http.PostAsJsonAsync(url+"/status",query)){Assert.True(statusResponse.StatusCode==HttpStatusCode.OK,await statusResponse.Content.ReadAsStringAsync()+detail());var status=(await statusResponse.Content.ReadFromJsonAsync<CompanyMaintenanceClosureStatus>())!;status.Require();Assert.Equal(saved.Receipt.HistoryId,status.Recorded!.HistoryId);Assert.Equal(command.Review.Acceptance.ReportRevision,status.Recorded.ReportRevision);Assert.DoesNotContain(command.Reason,await statusResponse.Content.ReadAsStringAsync());Assert.DoesNotContain(command.IntentId.ToString(),await statusResponse.Content.ReadAsStringAsync());}Assert.Equal(writes,f.Writes.Count);
        // Even an internally consistent replacement report row cannot replace the original closure subject.
        var originalReport=f.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single();var index=f.Rows.IndexOf(originalReport);f.Rows[index]=CompanyWorkOrderSubmissionCommand.Read(originalReport).Prepare(DateTime.UtcNow);
        try{using var changed=await http.PostAsJsonAsync(url+"/status",query);Assert.Equal(HttpStatusCode.Conflict,changed.StatusCode);Assert.Equal(writes,f.Writes.Count);}finally{f.Rows[index]=originalReport;}
        await VerifyMaintenanceAssetEvidenceHttp(f,http,url,query,detail);
    }
}
