using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.ProductionOperations.Models;
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    private static async Task VerifyMaterialSourceHttp(CompanyPPDMRowAccessTests.Fixture f,HttpHost host,CompanyMaintenanceReceivingQuery task,int writes)
    {
        var path=host.Url+"/api/field/current/maintenance-handoffs/materials/source-review";var query=new CompanyWorkOrderMaterialSourceQuery(task,"part-الكويت",2m,"EA");
        int Reads()=>f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="INVENTORY_ITEM");var before=Reads();
        using(var denied=await host.Http.PostAsJsonAsync(path,query))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(before,Reads());Assert.Equal(writes,f.Writes.Count);
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Inventory.View"});await f.Db.SaveChangesAsync();
        using(var valid=await host.Http.PostAsJsonAsync(path,query))
        {
            Assert.True(valid.StatusCode==HttpStatusCode.OK,await valid.Content.ReadAsStringAsync());var review=(await valid.Content.ReadFromJsonAsync<CompanyWorkOrderMaterialSourceReview>())!;review.Require(new("tenant","company"),"receiver",query);Assert.Equal(5m,review.Inventory.Item.QUANTITY_ON_HAND);Assert.Equal("bin",review.Inventory.Item.LOCATION_ID);Assert.Equal("N",Assert.Single(review.Inventory.Transactions).ACTIVE_IND);Assert.Contains(review.Blockers,b=>b.Contains("reservation"));Assert.Equal(writes,f.Writes.Count);
        }
        var allocationPath=host.Url+"/api/field/current/maintenance-handoffs/materials/allocation-review";
        using(var deniedAllocation=await host.Http.PostAsJsonAsync(allocationPath,query))Assert.Equal(HttpStatusCode.Forbidden,deniedAllocation.StatusCode);
        var catalogPath=host.Url+"/api/field/current/maintenance-handoffs/materials/catalog";
        using(var deniedCatalog=await host.Http.PostAsJsonAsync(catalogPath,task))Assert.Equal(HttpStatusCode.Forbidden,deniedCatalog.StatusCode);
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Inventory.ReserveWorkOrderMaterial"});await f.Db.SaveChangesAsync();
        using(var catalogResponse=await host.Http.PostAsJsonAsync(catalogPath,task))
        {
            Assert.True(catalogResponse.StatusCode==HttpStatusCode.OK,await catalogResponse.Content.ReadAsStringAsync());var catalog=(await catalogResponse.Content.ReadFromJsonAsync<CompanyWorkOrderMaterialCatalog>())!;catalog.Require(new("tenant","company"),"receiver",task);var choice=Assert.Single(catalog.Items);Assert.Equal(query.ItemId,choice.INVENTORY_ITEM_ID);Assert.Equal(5m,choice.QUANTITY_ON_HAND);Assert.Equal("EA",choice.UNIT_OF_MEASURE);Assert.Equal("bin",choice.LOCATION_ID);Assert.Equal(writes,f.Writes.Count);
        }
        var directOrderGrant=f.Db.Set<AppUserAssetAccess>().Single(row=>row.UserId=="receiver" && row.AssetType=="WORK_ORDER" && row.AssetId==task.WorkOrderId);directOrderGrant.IsActive=false;await f.Db.SaveChangesAsync();var beforeDirectDenied=Reads();using(var deniedOrderCatalog=await host.Http.PostAsJsonAsync(catalogPath,task))Assert.Equal(HttpStatusCode.Forbidden,deniedOrderCatalog.StatusCode);Assert.Equal(beforeDirectDenied,Reads());directOrderGrant.IsActive=true;await f.Db.SaveChangesAsync();
        using(var allocationResponse=await host.Http.PostAsJsonAsync(allocationPath,query))
        {
            Assert.True(allocationResponse.StatusCode==HttpStatusCode.OK,await allocationResponse.Content.ReadAsStringAsync());var allocation=(await allocationResponse.Content.ReadFromJsonAsync<CompanyWorkOrderMaterialAllocationReview>())!;allocation.Require();Assert.Equal(5m,allocation.AvailableQuantity);Assert.Empty(allocation.Allocations);
            var command=new CompanyWorkOrderMaterialAllocationCommand(allocation,Guid.NewGuid(),allocation.ComparedAtUtc,allocation.ComparedAtUtc.AddHours(2),"Reviewed original work requirement");var pathCommand=host.Url+"/api/field/current/maintenance-handoffs/materials/allocations";
            using(var missing=await host.Http.PostAsJsonAsync(pathCommand+"/recovery",command))Assert.Equal(HttpStatusCode.Conflict,missing.StatusCode);
            using var recorded=await host.Http.PostAsJsonAsync(pathCommand,command);Assert.True(recorded.StatusCode==HttpStatusCode.OK,await recorded.Content.ReadAsStringAsync());var row=(await recorded.Content.ReadFromJsonAsync<WORK_ORDER_MATERIAL_ALLOCATION>())!;command.RequireRecorded(row);Assert.Equal(writes+1,f.Writes.Count);
            using var recovered=await host.Http.PostAsJsonAsync(pathCommand+"/recovery",command);Assert.Equal(HttpStatusCode.OK,recovered.StatusCode);command.RequireRecorded((await recovered.Content.ReadFromJsonAsync<WORK_ORDER_MATERIAL_ALLOCATION>())!);Assert.Equal(writes+1,f.Writes.Count);
            var reference=new CompanyWorkOrderMaterialAllocationReferenceQuery(task,command.IntentId);using var restored=await host.Http.PostAsJsonAsync(pathCommand+"/recovery/reference",reference);Assert.True(restored.StatusCode==HttpStatusCode.OK,await restored.Content.ReadAsStringAsync());var envelope=(await restored.Content.ReadFromJsonAsync<CompanyWorkOrderMaterialAllocationRecovery>())!;envelope.Require(new("tenant","company"),"receiver",reference);Assert.Equal(row.RECORDED_REVISION,envelope.Recorded.RECORDED_REVISION);Assert.Equal(writes+1,f.Writes.Count);
            var saved=f.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>().Single();var actor=saved.ROW_CREATED_BY;var body=saved.COMMAND_JSON;saved.ROW_CREATED_BY="foreign";saved.COMMAND_JSON="malformed private body";using(var deniedPrivate=await host.Http.PostAsJsonAsync(pathCommand+"/recovery/reference",reference))Assert.Equal(HttpStatusCode.Forbidden,deniedPrivate.StatusCode);saved.ROW_CREATED_BY=actor;saved.COMMAND_JSON=body;
            var release=new CompanyWorkOrderMaterialReleaseCommand(envelope,Guid.NewGuid(),"Original work requirement cancelled");
            using(var deniedRelease=await host.Http.PostAsJsonAsync(pathCommand+"/release",release))Assert.Equal(HttpStatusCode.Forbidden,deniedRelease.StatusCode);Assert.Equal(writes+1,f.Writes.Count);
            f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="Inventory.ReleaseWorkOrderMaterial"});await f.Db.SaveChangesAsync();
            using(var missingRelease=await host.Http.PostAsJsonAsync(pathCommand+"/release/recovery",release))Assert.Equal(HttpStatusCode.Conflict,missingRelease.StatusCode);
            using var released=await host.Http.PostAsJsonAsync(pathCommand+"/release",release);Assert.True(released.StatusCode==HttpStatusCode.OK,await released.Content.ReadAsStringAsync());release.RequireRecorded((await released.Content.ReadFromJsonAsync<WORK_ORDER_MATERIAL_RELEASE>())!);Assert.Equal(writes+2,f.Writes.Count);
            using var releaseRecovery=await host.Http.PostAsJsonAsync(pathCommand+"/release/recovery",release);Assert.Equal(HttpStatusCode.OK,releaseRecovery.StatusCode);release.RequireRecorded((await releaseRecovery.Content.ReadFromJsonAsync<WORK_ORDER_MATERIAL_RELEASE>())!);Assert.Equal(writes+2,f.Writes.Count);
            var releaseReference=new CompanyWorkOrderMaterialReleaseReferenceQuery(reference,release.IntentId);using var restoredRelease=await host.Http.PostAsJsonAsync(pathCommand+"/release/recovery/reference",releaseReference);Assert.True(restoredRelease.StatusCode==HttpStatusCode.OK,await restoredRelease.Content.ReadAsStringAsync());var releaseEnvelope=(await restoredRelease.Content.ReadFromJsonAsync<CompanyWorkOrderMaterialReleaseRecovery>())!;releaseEnvelope.Require(new("tenant","company"),"receiver",releaseReference);Assert.Equal(writes+2,f.Writes.Count);
            var storedRelease=f.Rows.OfType<WORK_ORDER_MATERIAL_RELEASE>().Single();var releaseActor=storedRelease.ROW_CREATED_BY;var releaseBody=storedRelease.COMMAND_JSON;storedRelease.ROW_CREATED_BY="foreign";storedRelease.COMMAND_JSON="malformed private release";using(var foreignRelease=await host.Http.PostAsJsonAsync(pathCommand+"/release/recovery/reference",releaseReference))Assert.Equal(HttpStatusCode.Forbidden,foreignRelease.StatusCode);storedRelease.ROW_CREATED_BY=releaseActor;storedRelease.COMMAND_JSON=releaseBody;
            using var freshAllocation=await host.Http.PostAsJsonAsync(allocationPath,query);Assert.True(freshAllocation.StatusCode==HttpStatusCode.OK,await freshAllocation.Content.ReadAsStringAsync());var capacity=(await freshAllocation.Content.ReadFromJsonAsync<CompanyWorkOrderMaterialAllocationReview>())!;capacity.Require();Assert.Equal(0m,capacity.ReservedQuantity);Assert.Equal(5m,capacity.AvailableQuantity);Assert.Single(capacity.Releases!);writes++;
            Assert.Equal(5m,f.Rows.OfType<INVENTORY_ITEM>().Single().QUANTITY_ON_HAND);writes++;
        }
        var item=f.Rows.OfType<INVENTORY_ITEM>().Single();item.OWNER_COMPANY_BA_ID="other";
        using(var foreign=await host.Http.PostAsJsonAsync(path,query))Assert.Equal(HttpStatusCode.Conflict,foreign.StatusCode);item.OWNER_COMPANY_BA_ID="company";
        var sourceReads=0;f.Source.Setup(s=>s.GetEntityAsync("INVENTORY_ITEM",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string _,List<TheTechIdea.Beep.Report.AppFilter> filters)=>
        {
            if(++sourceReads==2)item.QUANTITY_ON_HAND=4m;return new[]{CompanyInventorySourceReview.Copy(item)};
        });
        using(var changed=await host.Http.PostAsJsonAsync(path,query))Assert.Equal(HttpStatusCode.Conflict,changed.StatusCode);Assert.Equal(2,sourceReads);Assert.Equal(writes,f.Writes.Count);
        sourceReads=0;item.QUANTITY_ON_HAND=5m;
        using(var changedCatalog=await host.Http.PostAsJsonAsync(catalogPath,task))Assert.Equal(HttpStatusCode.Conflict,changedCatalog.StatusCode);Assert.Equal(2,sourceReads);Assert.Equal(writes,f.Writes.Count);
        f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(c=>c.ClaimValue=="Inventory.View"));await f.Db.SaveChangesAsync();before=Reads();
        using(var revokedCatalog=await host.Http.PostAsJsonAsync(catalogPath,task))Assert.Equal(HttpStatusCode.Forbidden,revokedCatalog.StatusCode);Assert.Equal(before,Reads());Assert.Equal(writes,f.Writes.Count);
        using(var revoked=await host.Http.PostAsJsonAsync(path,query))Assert.Equal(HttpStatusCode.Forbidden,revoked.StatusCode);Assert.Equal(before,Reads());Assert.Equal(writes,f.Writes.Count);
    }
}