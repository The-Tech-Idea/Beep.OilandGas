using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyDevelopmentFacilityEditServiceTests
{
    [Theory][InlineData("write")][InlineData("recover")][InlineData("role-revoked")][InlineData("different-company")][InlineData("split-binding")][InlineData("revoked-during")][InlineData("different-receipt")]
    public async Task OriginalCommandIsAdmittedByCurrentCompanyAndExactAuthority(string mode)
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();f.Permissions=[PermissionConstants.DevelopmentPlanning.View,PermissionConstants.DevelopmentPlanning.Edit];
        f.Tenant.SelectCompany((await new RepositoryCompanyContextResolver(f.Accounts.Object).ResolveAsync("planner",f.Context.Company))!);
        f.Db.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="FACILITY",ConnectionName="core",ConnectionKey="CORE",ConcurrencyStamp="facility-binding"});await f.Db.SaveChangesAsync();
        f.Assets.Setup(a=>a.CheckAssetAccessAsync("planner","id","FACILITY",It.IsAny<string?>(),"PROCESSING")).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        var authority=new DevelopmentFacilityReadAuthority(f.Account,f.Company,f.Fields.Object,f.Preference.Object,f.Assets.Object,f.Grants.Object);var scope=await authority.CaptureEditAsync(f.User,"id","PROCESSING","ConfiguredPlanner");
        var row=new FACILITY{FACILITY_ID="id",FACILITY_TYPE="PROCESSING",PRIMARY_FIELD_ID="field",FACILITY_LONG_NAME="Original"};
        var review=new DevelopmentFacilityEditReview(scope.Context,row,"ConfiguredPlanner",DevelopmentFacilityEditReview.SourceDigest(row),scope.AuthorityRevision,"",DateTimeOffset.UtcNow);
        if(mode=="different-company")review=review with{Context=review.Context with{Company=new("tenant","other")}};
        review=review with{Revision=review.ContentRevision()};var command=new DevelopmentFacilityEdit(review,Guid.NewGuid(),"Updated",null,"Correct description",true);
        var result=CompanyInventorySourceReview.Copy(row);result.FACILITY_LONG_NAME="Updated";result.DESCRIPTION=null!;result.ROW_CHANGED_BY="planner";result.ROW_CHANGED_DATE=DateTime.UtcNow;
        var owner=new Mock<ICompanyFacilityEditOwner>();var called=0;
        owner.Setup(x=>x.RecordFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<Func<Task>>())).Returns(async(DevelopmentFacilityEdit value,Func<Task> check)=>{called++;if(mode=="revoked-during")f.Permissions=[];await check();return FACILITY_EDIT_INTENT.Create(mode=="different-receipt"?value with{IntentId=Guid.NewGuid()}:value,result);});
        owner.Setup(x=>x.RecoverFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<Func<Task>>())).Returns(async(DevelopmentFacilityEdit value,Func<Task> check)=>{called++;await check();return FACILITY_EDIT_INTENT.Create(value,result);});
        if(mode is "recover" or "role-revoked") {f.Permissions=[PermissionConstants.DevelopmentPlanning.View];f.Tenant.SelectCompany((await new RepositoryCompanyContextResolver(f.Accounts.Object).ResolveAsync("planner",f.Context.Company))!);}
        if(mode=="split-binding"){var binding=await f.Db.ModuleDatabases.SingleAsync(x=>x.ModuleId=="FACILITY");binding.ConnectionName="planning";binding.ConnectionKey="PLANNING";await f.Db.SaveChangesAsync();}
        var service=new CompanyDevelopmentFacilityEditService(authority,f.Modules,_=>owner.Object);
        if(mode is "write" or "recover") {var receipt=await service.ExecuteAsync(f.User,command,mode=="recover");receipt.Require(command);Assert.Equal("Updated",receipt.Result.FACILITY_LONG_NAME);Assert.Equal(1,called);}
        else {await Assert.ThrowsAnyAsync<Exception>(()=>service.ExecuteAsync(f.User,command));Assert.Equal(mode is "revoked-during" or "different-receipt"?1:0,called);}
    }
}
#endif
