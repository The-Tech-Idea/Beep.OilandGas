using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    private static async Task VerifyMaintenanceAssetEvidenceHttp(CompanyPPDMRowAccessTests.Fixture f,HttpClient http,string closureUrl,CompanyWorkOrderCompletionQuery query,Func<string> detail)
    {
        var url=closureUrl.Replace("/closure","/asset-return/evidence",StringComparison.Ordinal);var wells=f.Rows.OfType<WORK_ORDER_COMPONENT>().Where(row=>row.WORK_ORDER_ID==query.Task.WorkOrderId && row.ACTIVE_IND=="Y" && !string.IsNullOrWhiteSpace(row.UWI)).Select(row=>row.UWI).Distinct(StringComparer.Ordinal).ToArray();Assert.NotEmpty(wells);var utc=DateTime.UtcNow;var writes=f.Writes.Count;
        // Explicit already-recorded native well/facet/reference seam. No original report/component is rewritten.
        foreach(var id in wells)
        {
            var well=f.Rows.OfType<WELL>().Single(row=>row.UWI==id);well.EFFECTIVE_DATE=utc.AddYears(-1);well.EXPIRY_DATE=utc.AddYears(1);
            foreach(var (facet,value) in new[]{("Life Cycle","Operating"),("Role","Produce"),("Condition","Shut In")})f.Rows.Add(new WELL_STATUS{UWI=id,SOURCE="recorded-source",STATUS_ID=facet+"-prior",STATUS_TYPE=facet,STATUS=value,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-1),TIMEZONE="UTC"});
        }
        foreach(var (facet,value) in new[]{("Life Cycle","Operating"),("Role","Produce"),("Condition","Shut In"),("Condition","Active")})f.Rows.Add(new R_WELL_STATUS{STATUS_TYPE=facet,STATUS=value,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});if(!f.Rows.OfType<R_TIMEZONE>().Any(row=>row.TIMEZONE=="UTC"))f.Rows.Add(new R_TIMEZONE{TIMEZONE="UTC",ACTIVE_IND="Y"});
        int Reads()=>f.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && value.Arguments[0] as string=="WELL_STATUS");var reads=Reads();using(var denied=await http.PostAsJsonAsync(url,query))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(reads,Reads());Assert.Equal(writes,f.Writes.Count);
        var resolver=new RepositoryCompanyContextResolver(new RepositoryAccessService(f.Db));var administrator=await resolver.ResolveAsync("receiver",f.Gate.Company.Scope);Assert.NotNull(administrator);var tenant=new RepositoryTenantContext();tenant.SelectCompany(administrator);var gate=new CompanyModuleConnection(new(f.Db,f.Editor.Object,tenant),tenant,resolver);var store=new RepositoryAssetAccessStore(f.Db,gate,Moq.Mock.Of<IAssetGrantTargetVerifier>());var fingerprint=(await f.Gate.ResolveAsync()).Fingerprint;var actor=f.Gate.Company.UserId;
        if(f.Rows.OfType<WORK_ORDER_COMPONENT>().Any(row=>row.WORK_ORDER_ID==query.Task.WorkOrderId && !string.IsNullOrWhiteSpace(row.EQUIPMENT_ID))){Assert.True(await store.GrantAsync(actor,fingerprint,"FACILITY","plant","READ",false,facilityType:"gas"));Assert.True(await store.GrantAsync(actor,fingerprint,"EQUIPMENT","pump","READ",false));}
        foreach(var id in wells)Assert.True(await store.GrantAsync(actor,fingerprint,"WELL",id,"READ",false));
        using(var response=await http.PostAsJsonAsync(url,query))
        {
            Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync()+detail());Assert.Equal("no-store",response.Headers.CacheControl?.ToString());var review=(await response.Content.ReadFromJsonAsync<CompanyMaintenanceAssetEvidence>())!;review.Require();Assert.Equal(query,review.Report.Query);Assert.Equal(wells.Length,review.Wells.Count);Assert.All(review.Wells,well=>{Assert.Equal("READ",well.AccessLevel);Assert.Contains(well.Facets,row=>row.Facet=="Condition" && row.Value=="Shut In");Assert.Contains(well.Facets,row=>row.Facet=="Role" && row.Value=="Produce");Assert.Contains(well.Blockers,value=>value.Contains("WRITE",StringComparison.Ordinal));});Assert.Contains(CompanyMaintenanceAssetEvidence.PendingCommand,review.Blockers);Assert.All(review.Order.Components,row=>Assert.True(ProcessDefinitionRevision.IsValid(row.NativeRevision)));Assert.Equal(writes,f.Writes.Count);
        }
        foreach(var id in wells)Assert.True(await store.RevokeAsync(actor,fingerprint,"WELL",id));reads=Reads();using(var revoked=await http.PostAsJsonAsync(url,query))Assert.Equal(HttpStatusCode.Forbidden,revoked.StatusCode);Assert.Equal(reads,Reads());Assert.Equal(writes,f.Writes.Count);
        await VerifyMaintenanceAssetReturnHttp(f,http,closureUrl,query,wells,store,fingerprint,actor,detail);
    }
}
