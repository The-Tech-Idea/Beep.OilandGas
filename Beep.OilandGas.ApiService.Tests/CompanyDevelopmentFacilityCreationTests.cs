#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyDevelopmentFacilityCreationTests
{
    [Theory][InlineData("write")][InlineData("recover")][InlineData("no-inherit")][InlineData("revoked")][InlineData("wrong-company")]
    public async Task CreationRequiresOriginalAuthorityAndInheritedWriteWhileRecoveryUsesCurrentRead(string mode)
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();f.Permissions=["DevelopmentPlanning.View","DevelopmentPlanning.Create"];f.Tenant.SelectCompany((await new RepositoryCompanyContextResolver(f.Accounts.Object).ResolveAsync("planner",f.Context.Company))!);
        f.Db.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="FACILITY",ConnectionName="core",ConnectionKey="CORE",ConcurrencyStamp="facility"});await f.Db.SaveChangesAsync();
        var inherit=mode!="no-inherit";f.Grants.Setup(value=>value.ReadAsync("planner",f.Context.Database.Fingerprint,"company")).ReturnsAsync(()=>new List<AppUserAssetAccess>{new(){Id="grant",UserId="planner",DatabaseScope=f.Context.Database.Fingerprint,OrganizationId="company",AssetType="FIELD",AssetId="field",AccessLevel="WRITE",Inherit=inherit,IsActive=true,ConcurrencyStamp="original"}});
        var authority=new DevelopmentFacilityReadAuthority(f.Account,f.Company,f.Fields.Object,f.Preference.Object,f.Assets.Object,f.Grants.Object);var scope=await authority.CaptureCreateAsync(f.User,"ConfiguredPlanner");
        var draft=new DevelopmentFacilityCreationDraft(f.Context,"ConfiguredPlanner","new","PROCESSING","operator","Name",null,"Reason");if(mode=="wrong-company")draft=draft with{Context=draft.Context with{Company=new("tenant","other")}};
        var review=new DevelopmentFacilityCreationReview(draft,new R_FACILITY_TYPE{FACILITY_TYPE="PROCESSING",ACTIVE_IND="Y"},new string('A',64),scope.AuthorityRevision,"",DateTimeOffset.UtcNow);review=review with{Revision=review.ContentRevision()};var command=new DevelopmentFacilityCreation(review,Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),true);
        var result=command.Expected(DateTime.UtcNow);var saved=FACILITY_CREATE_INTENT.Create(command,result.Facility,result.Link);var owner=new Mock<ICompanyFacilityCreationOwner>();var calls=0;
        owner.Setup(value=>value.RecordFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreation>(),It.IsAny<Func<Task>>())).Returns(async(DevelopmentFacilityCreation _,Func<Task> guard)=>{calls++;if(mode=="revoked")inherit=false;await guard();return saved;});
        owner.Setup(value=>value.RecoverFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreation>(),It.IsAny<Func<Task>>())).Returns(async(DevelopmentFacilityCreation _,Func<Task> guard)=>{calls++;await guard();return saved;});
        if(mode=="recover"){f.Permissions=["DevelopmentPlanning.View"];f.Tenant.SelectCompany((await new RepositoryCompanyContextResolver(f.Accounts.Object).ResolveAsync("planner",f.Context.Company))!);}
        var service=new CompanyDevelopmentFacilityCreationService(authority,f.Modules,_=>Mock.Of<ICompanyFacilityCreationReader>(),_=>owner.Object,f.Grants.Object);
        if(mode is "write" or "recover"){var receipt=await service.ExecuteAsync(f.User,command,mode=="recover");receipt.Require(command);Assert.Equal(1,calls);}
        else{await Assert.ThrowsAnyAsync<Exception>(()=>service.ExecuteAsync(f.User,command));Assert.Equal(mode=="revoked"?1:0,calls);}
    }
    [Theory][InlineData("valid")][InlineData("permission")][InlineData("different-field")][InlineData("split")][InlineData("revoked")]
    public async Task SourceReviewRequiresCurrentCreateRoleFieldAndBinding(string mode)
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();f.Permissions=["DevelopmentPlanning.View","DevelopmentPlanning.Create"];
        f.Tenant.SelectCompany((await new RepositoryCompanyContextResolver(f.Accounts.Object).ResolveAsync("planner",f.Context.Company))!);
        f.Db.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="FACILITY",ConnectionName=mode=="split"?"planning":"core",ConnectionKey=mode=="split"?"PLANNING":"CORE",ConcurrencyStamp="facility"});await f.Db.SaveChangesAsync();
        var authority=new DevelopmentFacilityReadAuthority(f.Account,f.Company,f.Fields.Object,f.Preference.Object,f.Assets.Object,f.Grants.Object);
        var context=mode=="different-field"?f.Context with{FieldId="other"}:f.Context;var draft=new DevelopmentFacilityCreationDraft(context,"ConfiguredPlanner","new","PROCESSING","operator","Name",null,"Reason");
        var reader=new Mock<ICompanyFacilityCreationReader>();var called=0;
        reader.Setup(value=>value.ReadFacilityCreationSourceAsync(It.IsAny<DevelopmentFacilityCreationDraft>(),It.IsAny<Func<Task>>())).Returns(async(DevelopmentFacilityCreationDraft original,Func<Task> guard)=>{called++;if(mode=="revoked")f.Permissions=[];await guard();return new DevelopmentFacilityCreationSource(new R_FACILITY_TYPE{FACILITY_TYPE=original.FacilityType,ACTIVE_IND="Y"},new string('A',64));});
        if(mode=="permission"){f.Permissions=["DevelopmentPlanning.View"];f.Tenant.SelectCompany((await new RepositoryCompanyContextResolver(f.Accounts.Object).ResolveAsync("planner",f.Context.Company))!);}
        var service=new CompanyDevelopmentFacilityCreationService(authority,f.Modules,_=>reader.Object,_=>Mock.Of<ICompanyFacilityCreationOwner>(),f.Grants.Object);
        if(mode=="valid"){var result=await service.ReviewAsync(f.User,draft);result.Require();Assert.Equal("new",result.Draft.FacilityId);Assert.Equal(1,called);}
        else{await Assert.ThrowsAnyAsync<Exception>(()=>service.ReviewAsync(f.User,draft));Assert.Equal(mode=="revoked"?1:0,called);}
    }
}
#endif
