using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Controllers.WorkOrder;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact] public async Task ActualWorkOrderPrerequisiteInspectionHttpRequiresAppRolePermissionAndDirectOrderRights()
    {
        using var f=await Ready();const string order="order-الكويت";await f.Grant("FIELD","A","WRITE");await f.Grant("WORK_ORDER",order,"WRITE");f.EnableCorrectiveActionTransactions();
        f.Rows.Add(new WORK_ORDER{WORK_ORDER_ID=order,WORK_ORDER_TYPE=WorkOrderSubType.Safety,INSTRUCTIONS="Explicit recorded safety work",ACTIVE_IND="Y"});f.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID=order,COMPONENT_ID="component",FIELD_ID="A",ACTIVE_IND="Y"});
        f.Metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName="WORK_ORDER_INSPECTION",PrimaryKeyColumn="INSPECTION_ID",EntityTypeName=typeof(WORK_ORDER_INSPECTION).FullName!});f.Metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(["INSPECTION_ID"]);f.Source.Setup(s=>s.CheckEntityExist("WORK_ORDER_INSPECTION")).Returns(true);var creator=new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object);f.Source.Setup(s=>s.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(value=>value.EntityName=="WORK_ORDER_INSPECTION"),true)).Returns((TheTechIdea.Beep.DataBase.EntityStructure _,bool __)=>creator.ConvertToEntityStructure(typeof(WORK_ORDER_INSPECTION)));
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);var orderRight=true;boundary.Access.Setup(a=>a.CheckAssetAccessAsync("actor",order,"WORK_ORDER",It.IsAny<string?>(),null)).ReturnsAsync(()=>new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=orderRight});var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);var owners=MaintenanceReadOwners(f,modules,profile);
        var scheduling=new Mock<ISchedulingService>(MockBehavior.Strict);var contractors=new Mock<IContractorManagementService>(MockBehavior.Strict);var processes=new Mock<IProcessService>(MockBehavior.Strict);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(WorkOrderController),services=>{services.AddSingleton(owners.Order);services.AddSingleton(owners.Inspections);services.AddSingleton(scheduling.Object);services.AddSingleton(contractors.Object);services.AddSingleton(processes.Object);services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();});
        foreach(var retired in new[]{
            (HttpMethod.Get,"/api/field/current/workorder/order/afe"),
            (HttpMethod.Post,"/api/field/current/workorder/order/afe"),
            (HttpMethod.Get,"/api/field/current/workorder/order/costs"),
            (HttpMethod.Post,"/api/field/current/workorder/order/costs/afe"),
            (HttpMethod.Post,"/api/field/current/workorder/order/costs/line"),
            (HttpMethod.Get,"/api/lifecycle/workorders/order/afe?connectionName=foreign"),
            (HttpMethod.Post,"/api/lifecycle/workorders/order/afe?connectionName=foreign"),
            (HttpMethod.Post,"/api/lifecycle/workorders/order/costs?connectionName=foreign")})
        {
            using var request=new HttpRequestMessage(retired.Item1,host.Url+retired.Item2);
            using var response=await host.Http.SendAsync(request);
            Assert.Equal(HttpStatusCode.NotFound,response.StatusCode);
        }
        Assert.Empty(f.Writes);
        var path=host.Url+"/api/field/current/workorder/"+Uri.EscapeDataString(order)+"/checklist";var required=new AddWorkOrderInspectionRequirementRequest{FieldId="A",CondSeq=1,CondType=ConditionType.PreStart,RequiredText="Original isolation inspection",Jurisdiction="Explicit reported authority",RegulatoryRef="isolation-document",PermitPrerequisiteKind="ISOLATION"};
        using(var denied=await host.Http.PostAsJsonAsync(path,required))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Empty(f.Writes);
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="HSE.ApprovePermit"});await f.Db.SaveChangesAsync();
        using var added=await host.Http.PostAsJsonAsync(path,required);Assert.Equal(HttpStatusCode.OK,added.StatusCode);var inspection=(await added.Content.ReadFromJsonAsync<InspectionCondition>())!;Assert.Equal("ISOLATION",inspection.PermitPrerequisiteKind);Assert.Equal("UNRECORDED",inspection.Status);Assert.Single(f.Writes);
        var result=new RecordInspectionResultRequest{FieldId="A",InspectionId=inspection.InspectionId,ExpectedSourceRevision=inspection.SourceRevision,Result="PASS",Notes="Explicit authenticated acceptance evidence"};
        using var saved=await host.Http.PostAsJsonAsync(path+"/1",result);Assert.Equal(HttpStatusCode.OK,saved.StatusCode);var accepted=(await saved.Content.ReadFromJsonAsync<InspectionCondition>())!;Assert.Equal("PASS",accepted.Status);Assert.Equal("actor",accepted.Inspector);Assert.Equal("ISOLATION",accepted.PermitPrerequisiteKind);Assert.Equal(2,f.Writes.Count);
        using var reread=await host.Http.GetAsync(path);Assert.Equal(HttpStatusCode.OK,reread.StatusCode);var rows=(await reread.Content.ReadFromJsonAsync<List<InspectionCondition>>())!;Assert.Equal(accepted,Assert.Single(rows));Assert.Equal(2,f.Writes.Count);
        orderRight=false;using(var foreign=await host.Http.GetAsync(path))Assert.Equal(HttpStatusCode.Forbidden,foreign.StatusCode);using(var missingOrderRight=await host.Http.PostAsJsonAsync(path,required))Assert.Equal(HttpStatusCode.Forbidden,missingOrderRight.StatusCode);Assert.Equal(2,f.Writes.Count);orderRight=true;
        f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(c=>c.ClaimValue=="HSE.ApprovePermit"));await f.Db.SaveChangesAsync();required.CondSeq=2;using(var revoked=await host.Http.PostAsJsonAsync(path,required))Assert.Equal(HttpStatusCode.Forbidden,revoked.StatusCode);Assert.Equal(2,f.Writes.Count);Assert.Empty(scheduling.Invocations);Assert.Empty(contractors.Invocations);Assert.Empty(processes.Invocations);
    }
}
