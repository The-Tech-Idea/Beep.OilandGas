using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Report;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentFacilityEvidenceHttpTests
{
    [Fact]
    public async Task ProtectedCreationReviewUsesCurrentRoleFieldAndExactOriginal()
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();f.Permissions=["DevelopmentPlanning.View","DevelopmentPlanning.Create"];f.Tenant.SelectCompany((await new RepositoryCompanyContextResolver(f.Accounts.Object).ResolveAsync("planner",f.Context.Company))!);
        f.Db.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="FACILITY",ConnectionName="core",ConnectionKey="CORE",ConcurrencyStamp="facility"});await f.Db.SaveChangesAsync();
        f.Grants.Setup(x=>x.ReadAsync("planner",f.Context.Database.Fingerprint,"company")).ReturnsAsync(()=>new List<AppUserAssetAccess>{new(){Id="field",UserId="planner",DatabaseScope=f.Context.Database.Fingerprint,OrganizationId="company",AssetType="FIELD",AssetId="field",AccessLevel="WRITE",Inherit=true,IsActive=true,ConcurrencyStamp="original"}});
        var field=new Mock<IFieldOrchestrator>();field.Setup(x=>x.SetActiveFieldAsync("field")).ReturnsAsync(true);field.SetupGet(x=>x.CurrentFieldId).Returns("field");var source=new Mock<ICompanyFacilityCreationReader>();
        source.Setup(x=>x.ReadFacilityCreationSourceAsync(It.IsAny<DevelopmentFacilityCreationDraft>(),It.IsAny<Func<Task>>())).Returns(async(DevelopmentFacilityCreationDraft draft,Func<Task> guard)=>{await guard();return new DevelopmentFacilityCreationSource(new R_FACILITY_TYPE{FACILITY_TYPE=draft.FacilityType,ACTIVE_IND="Y",LONG_NAME="Recorded type"},new string('A',64));});
        var owner=new Mock<ICompanyFacilityCreationOwner>();FACILITY_CREATE_INTENT? recorded=null;var writes=0;
        owner.Setup(x=>x.RecordFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreation>(),It.IsAny<Func<Task>>())).Returns(async(DevelopmentFacilityCreation original,Func<Task> guard)=>{await guard();if(recorded is not null){if(recorded.Restore(original.Review.Draft.Context.Company,original.Review.Draft.Context.ActorUserId).Command.Revision()!=original.Revision())throw new InvalidOperationException();return recorded;}writes++;var rows=original.Expected(DateTime.UtcNow);return recorded=FACILITY_CREATE_INTENT.Create(original,rows.Facility,rows.Link);});
        owner.Setup(x=>x.RecoverFacilityCreationAsync(It.IsAny<DevelopmentFacilityCreation>(),It.IsAny<Func<Task>>())).Returns(async(DevelopmentFacilityCreation original,Func<Task> guard)=>{await guard();return recorded!;});
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>
        {
            services.AddScoped(p=>new RepositoryRequestAccount(p.GetRequiredService<IHttpContextAccessor>(),u=>u.FindFirst("party_id")?.Value,f.Accounts.Object,null!));services.AddScoped(_=>f.Company);services.AddScoped(_=>f.Modules);
            services.AddScoped<ICompanyFieldSelectionService>(_=>f.Fields.Object);services.AddScoped<ICompanyFieldSelectionAuthority>(_=>f.Preference.Object);services.AddScoped<IAccessControlService>(_=>f.Assets.Object);services.AddScoped<IUserAssetAccessStore>(_=>f.Grants.Object);services.AddScoped<IFieldOrchestrator>(_=>field.Object);
            services.AddScoped<IFieldDevelopmentService>(_=>Mock.Of<IFieldDevelopmentService>());services.AddScoped<DevelopmentFacilityReadAuthority>();services.AddScoped<Func<FieldSelectionReview,ICompanyFacilityCreationReader>>(_=>_=>source.Object);services.AddScoped<Func<FieldSelectionReview,ICompanyFacilityCreationOwner>>(_=>_=>owner.Object);services.AddScoped<CompanyDevelopmentFacilityCreationService>();
        });
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","planner");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredPlanner");using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"planner",new(f.Context.Company,["ConfiguredPlanner"],f.Permissions));
        var client=new DevelopmentServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<DevelopmentServiceClient>.Instance);var draft=new DevelopmentFacilityCreationDraft(f.Context,"ConfiguredPlanner","new","PROCESSING","operator","Name",null,"Reason");
        var result=await client.ReviewFacilityCreationAsync(draft);result.Require();Assert.Equal("Recorded type",result.TypeSource.LONG_NAME);var original=new DevelopmentFacilityCreation(result,Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),true);
        (await client.SaveFacilityCreationAsync(original)).Require(original);(await client.SaveFacilityCreationAsync(original)).Require(original);Assert.Equal(1,writes);
        f.Permissions=["DevelopmentPlanning.View"];f.Tenant.SelectCompany((await new RepositoryCompanyContextResolver(f.Accounts.Object).ResolveAsync("planner",f.Context.Company))!);
        company.TrySelect(company.Clear(),"planner",new(f.Context.Company,["ConfiguredPlanner"],f.Permissions));(await client.RecoverFacilityCreationAsync(original)).Require(original);Assert.Equal(1,writes);
        using var denied=await host.Client.PostAsJsonAsync("/api/field/current/development/facilities/creation-review?fieldId=field",draft);Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);source.Verify(x=>x.ReadFacilityCreationSourceAsync(It.IsAny<DevelopmentFacilityCreationDraft>(),It.IsAny<Func<Task>>()),Times.Once());
    }
    [Fact]
    public async Task ProtectedSaveAndReadOnlyRecoveryUseTheSameOriginal()
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();f.Permissions=["DevelopmentPlanning.View","DevelopmentPlanning.Edit"];
        f.Tenant.SelectCompany((await new RepositoryCompanyContextResolver(f.Accounts.Object).ResolveAsync("planner",f.Context.Company))!);
        f.Db.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="FACILITY",ConnectionName="core",ConnectionKey="CORE",ConcurrencyStamp="facility"});await f.Db.SaveChangesAsync();
        f.Assets.Setup(a=>a.CheckAssetAccessAsync("planner","facility","FACILITY",It.IsAny<string?>(),"PROCESSING")).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        var field=new Mock<IFieldOrchestrator>();field.Setup(x=>x.SetActiveFieldAsync("field")).ReturnsAsync(true);field.SetupGet(x=>x.CurrentFieldId).Returns("field");
        var source=new FACILITY{FACILITY_ID="facility",FACILITY_TYPE="PROCESSING",PRIMARY_FIELD_ID="field",FACILITY_LONG_NAME="Recorded"};
        var reads=new Mock<IFieldDevelopmentService>();reads.Setup(x=>x.GetFacilitiesForFieldAsync("field",It.IsAny<List<AppFilter>?>())).ReturnsAsync([source]);
        var owner=new Mock<ICompanyFacilityEditOwner>();FACILITY_EDIT_INTENT? saved=null;var writes=0;
        owner.Setup(x=>x.RecordFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<Func<Task>>())).Returns(async(DevelopmentFacilityEdit command,Func<Task> check)=>
        {
            await check();if(saved is not null){if(saved.Restore(command.Review.Context.Company,command.Review.Context.ActorUserId).Command.Revision()!=command.Revision())throw new InvalidOperationException("The original differs.");return saved;}writes++;var result=CompanyInventorySourceReview.Copy(command.Review.Original);result.FACILITY_LONG_NAME=command.LongName;result.DESCRIPTION=command.Description!;result.ROW_CHANGED_BY="planner";result.ROW_CHANGED_DATE=DateTime.UtcNow;
            return saved=FACILITY_EDIT_INTENT.Create(command,result);
        });
        owner.Setup(x=>x.RecoverFacilityEditAsync(It.IsAny<DevelopmentFacilityEdit>(),It.IsAny<Func<Task>>())).Returns(async(DevelopmentFacilityEdit command,Func<Task> check)=>{await check();return saved!;});
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>
        {
            services.AddScoped(p=>new RepositoryRequestAccount(p.GetRequiredService<IHttpContextAccessor>(),u=>u.FindFirst("party_id")?.Value,f.Accounts.Object,null!));
            services.AddScoped(_=>f.Company);services.AddScoped(_=>f.Modules);services.AddScoped<ICompanyFieldSelectionService>(_=>f.Fields.Object);services.AddScoped<ICompanyFieldSelectionAuthority>(_=>f.Preference.Object);
            services.AddScoped<IAccessControlService>(_=>f.Assets.Object);services.AddScoped<IUserAssetAccessStore>(_=>f.Grants.Object);services.AddScoped<IFieldOrchestrator>(_=>field.Object);
            services.AddScoped<IFieldDevelopmentService>(_=>reads.Object);services.AddScoped<DevelopmentFacilityReadAuthority>();services.AddScoped<Func<FieldSelectionReview,ICompanyFacilityEditOwner>>(_=>_=>owner.Object);services.AddScoped<CompanyDevelopmentFacilityEditService>();
        });
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","planner");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredPlanner");
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"planner",new(f.Context.Company,["ConfiguredPlanner"],f.Permissions));
        var client=new DevelopmentServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<DevelopmentServiceClient>.Instance);
        var review=await client.ReviewFacilityEditAsync("facility","PROCESSING","ConfiguredPlanner","field");var original=new DevelopmentFacilityEdit(review,Guid.NewGuid(),"Updated",null,"Correct the facility name",true);
        Assert.Equal(HttpStatusCode.BadRequest,(await host.Client.PostAsJsonAsync("/api/field/current/development/facilities/edit",original with{Confirmed=false})).StatusCode);Assert.Equal(0,writes);
        var receipt=await client.SaveFacilityEditAsync(original);receipt.Require(original);Assert.Equal(1,writes);
        var repeated=await client.SaveFacilityEditAsync(original);repeated.Require(original);Assert.Equal(1,writes);
        await Assert.ThrowsAnyAsync<Exception>(()=>client.SaveFacilityEditAsync(original with{Reason="Different reason"}));Assert.Equal(1,writes);
        f.Permissions=["DevelopmentPlanning.View"];f.Tenant.SelectCompany((await new RepositoryCompanyContextResolver(f.Accounts.Object).ResolveAsync("planner",f.Context.Company))!);
        company.TrySelect(company.Clear(),"planner",new(f.Context.Company,["ConfiguredPlanner"],f.Permissions));
        var recovered=await client.RecoverFacilityEditAsync(original);recovered.Require(original);Assert.Equal(1,writes);
        Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsJsonAsync("/api/field/current/development/facilities/edit",original)).StatusCode);Assert.Equal(1,writes);
        await Assert.ThrowsAnyAsync<Exception>(()=>client.RecoverFacilityEditAsync(original with{IntentId=Guid.NewGuid()}));Assert.Equal(1,writes);
    }
    [Fact]
    public async Task ProtectedEditReviewUsesTheExactCompositeIdentityAndSelectedRole()
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();f.Permissions=[..f.Permissions,"DevelopmentPlanning.Edit"];
        f.Assets.Setup(a=>a.CheckAssetAccessAsync("planner","facility","FACILITY","WRITE","PROCESSING")).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=true});
        var field=new Mock<IFieldOrchestrator>();field.Setup(x=>x.SetActiveFieldAsync("field")).ReturnsAsync(true);field.SetupGet(x=>x.CurrentFieldId).Returns("field");
        var owner=new Mock<IFieldDevelopmentService>();owner.Setup(x=>x.GetFacilitiesForFieldAsync("field",It.IsAny<List<AppFilter>?>())).ReturnsAsync([new FACILITY{FACILITY_ID="facility",FACILITY_TYPE="PROCESSING",PRIMARY_FIELD_ID="field",FACILITY_LONG_NAME="Recorded"}]);
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>
        {
            services.AddScoped(p=>new RepositoryRequestAccount(p.GetRequiredService<IHttpContextAccessor>(),u=>u.FindFirst("party_id")?.Value,f.Accounts.Object,null!));
            services.AddScoped(_=>f.Company);services.AddScoped<ICompanyFieldSelectionService>(_=>f.Fields.Object);services.AddScoped<ICompanyFieldSelectionAuthority>(_=>f.Preference.Object);
            services.AddScoped<IAccessControlService>(_=>f.Assets.Object);services.AddScoped<IUserAssetAccessStore>(_=>f.Grants.Object);services.AddScoped<IFieldOrchestrator>(_=>field.Object);
            services.AddScoped<IFieldDevelopmentService>(_=>owner.Object);services.AddScoped<DevelopmentFacilityReadAuthority>();
        });
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","planner");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredPlanner");
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"planner",new(new("tenant","company"),["ConfiguredPlanner"],f.Permissions));
        var client=new DevelopmentServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<DevelopmentServiceClient>.Instance);
        var review=await client.ReviewFacilityEditAsync("facility","PROCESSING","ConfiguredPlanner","field");review.Require();
        Assert.Equal("facility",review.Original.FACILITY_ID);Assert.Equal("PROCESSING",review.Original.FACILITY_TYPE);Assert.Equal("ConfiguredPlanner",review.EditorRole);
        owner.Verify(x=>x.GetFacilitiesForFieldAsync("field",It.IsAny<List<AppFilter>?>()),Times.Exactly(2));
        f.Assets.Verify(x=>x.CheckAssetAccessAsync("planner","facility","FACILITY","WRITE","PROCESSING"),Times.AtLeastOnce());
    }
    [Theory][InlineData("allowed")][InlineData("anonymous")][InlineData("role")][InlineData("permission")][InlineData("field")][InlineData("wrong-field")][InlineData("revoked")][InlineData("foreign-row")]
    public async Task ProtectedFacilityReadRequiresCurrentCompanyRolesAndOriginalField(string mode)
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();var field=new Mock<IFieldOrchestrator>();
        field.Setup(x=>x.SetActiveFieldAsync("field")).ReturnsAsync(true);field.SetupGet(x=>x.CurrentFieldId).Returns("field");
        var owner=new Mock<IFieldDevelopmentService>();var reads=0;
        owner.Setup(x=>x.GetFacilitiesForFieldAsync("field",It.IsAny<List<AppFilter>?>())).Returns(()=>
        {
            reads++;if(mode=="revoked")f.Permissions=[];
            return Task.FromResult(new List<FACILITY>{new(){FACILITY_ID="facility",FACILITY_TYPE="PROCESSING",PRIMARY_FIELD_ID=mode=="foreign-row"?"other":"field",REMARK="recorded evidence"}});
        });
        if(mode=="permission")f.Permissions=[];if(mode=="field")f.FieldAccess=false;
        await using var host=await AccountingSourceHttpTests.Start(authenticate:mode!="anonymous",role:mode!="role",configure:(services,_)=>
        {
            services.AddScoped(p=>new RepositoryRequestAccount(p.GetRequiredService<IHttpContextAccessor>(),u=>u.FindFirst("party_id")?.Value,f.Accounts.Object,null!));
            services.AddScoped(_=>f.Company);services.AddScoped<ICompanyFieldSelectionService>(_=>f.Fields.Object);
            services.AddScoped<ICompanyFieldSelectionAuthority>(_=>f.Preference.Object);services.AddScoped<IAccessControlService>(_=>f.Assets.Object);
            services.AddScoped<IUserAssetAccessStore>(_=>f.Grants.Object);services.AddScoped<IFieldOrchestrator>(_=>field.Object);
            services.AddScoped<IFieldDevelopmentService>(_=>owner.Object);services.AddScoped<DevelopmentFacilityReadAuthority>();
        });
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","planner");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredPlanner");
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"planner",new(new("tenant","company"),["ConfiguredPlanner"],f.Permissions));
        var client=new DevelopmentServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<DevelopmentServiceClient>.Instance);
        if(mode=="allowed")
        {
            var rows=await client.GetFacilitiesAsync(fieldId:"field");Assert.Equal("recorded evidence",Assert.Single(rows).REMARK);Assert.Equal(1,reads);
            var response=await host.Client.GetAsync("/api/field/current/development/facilities?fieldId=field");Assert.Equal(HttpStatusCode.OK,response.StatusCode);Assert.Contains("no-store",response.Headers.CacheControl!.ToString());
        }
        else
        {
            var response=await host.Client.GetAsync("/api/field/current/development/facilities?fieldId="+(mode=="wrong-field"?"other":"field"));
            if(mode=="anonymous")Assert.Equal(HttpStatusCode.Unauthorized,response.StatusCode);
            else if(mode=="wrong-field")Assert.True(response.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.Forbidden);
            else if(mode=="foreign-row")Assert.NotEqual(HttpStatusCode.OK,response.StatusCode);
            else Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);
            Assert.Equal(mode is "revoked" or "foreign-row"?1:0,reads);
            Assert.DoesNotContain("recorded evidence",await response.Content.ReadAsStringAsync());
        }
    }
}
#endif
