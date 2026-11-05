using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CurrentFieldLifecycleGateTests
{
    [Fact]
    public async Task ActualCompanyPersonnelIdentityHttpRequiresCurrentCompanyAdministratorAndPreservesNativeBindingRecovery()
    {
        using var f=await Ready();f.Db.Users.Add(new OilGasUser{Id="identity-subject",UserName="Actual company account",IsActive=true,ConcurrencyStamp="subject-version"});f.Db.UserRoles.Add(new IdentityUserRole<string>{UserId="identity-subject",RoleId="admin"});await f.Db.SaveChangesAsync();
        await f.Select("company","identity-subject");await f.Select("company","actor");CompanyProductionBoundaryTests.NativeDecisionProvider(f);
        var utc=DateTime.UtcNow;var employee=new BA_EMPLOYEE{EMPLOYER_BA_ID="company",EMPLOYEE_BA_ID="native-person",EMPLOYEE_OBS_NO=1,ACTIVE_IND="Y",STATUS="Reported employment status",EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(30)};var person=new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="native-person",BA_LONG_NAME="Recorded native person",BA_TYPE="Reported person type",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(30)};f.Rows.Add(employee);f.Rows.Add(person);
        var creator=new ClassCreator(f.Editor.Object);
        foreach(var (type,keys) in new[]{(typeof(BA_EMPLOYEE),new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"}),(typeof(BUSINESS_ASSOCIATE),new[]{"BUSINESS_ASSOCIATE_ID"}),(typeof(COMPANY_PERSONNEL_IDENTITY),new[]{"IDENTITY_RECORD_ID"})})
        {
            var table=type.Name;var metadata=new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=string.Join(',',keys)};foreach(var column in table=="BA_EMPLOYEE"?new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID"}:table=="COMPANY_PERSONNEL_IDENTITY"?new[]{"PERSON_BA_ID","OWNER_COMPANY_BA_ID"}:Array.Empty<string>())metadata.ForeignKeys.Add(new(){ForeignKeyColumn=column,ReferencedTable="BUSINESS_ASSOCIATE",ReferencedPrimaryKey="BUSINESS_ASSOCIATE_ID"});f.Metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(metadata);f.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync(keys);f.Source.Setup(value=>value.CheckEntityExist(table)).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(value=>value.EntityName==table),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(type);foreach(var field in shape.Fields){field.IsKey=keys.Contains(field.FieldName);if(field.IsKey)field.AllowDBNull=false;}return shape;});
        }
        var profile=Profile(f);await Save(profile);using var boundary=new Boundary(f,profile);var tenant=new RepositoryTenantContext();tenant.SelectCompany(f.Gate.Company);var modules=new ModuleConnectionResolver(f.Db,f.Editor.Object,tenant);
        var owner=new ContractorManagementService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,async()=>{var current=await f.Gate.RefreshCompanyAsync();var connection=await modules.ResolveAsync("PPDM_CORE");return(current.Scope,current.UserId,new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],connection)));},NullLogger<ContractorManagementService>.Instance);var service=new CompanyPersonnelIdentityService(f.Gate,modules,owner,f.Db);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(CompanyPersonnelIdentityController),services=>{services.AddSingleton(service);services.AddSingleton<Func<string>>(()=>"actor");services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();services.AddAuthentication(options=>{options.DefaultAuthenticateScheme="receiving-fixture";options.DefaultChallengeScheme="receiving-fixture";}).AddScheme<AuthenticationSchemeOptions,ReceivingAuthentication>("receiving-fixture",_=>{});});var url=host.Url+"/api/company/current/personnel-identities";var before=f.Writes.Count;var reads=f.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && value.Arguments[0] as string=="COMPANY_PERSONNEL_IDENTITY");
        using(var denied=await host.Http.GetAsync(url))Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);Assert.Equal(reads,f.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && value.Arguments[0] as string=="COMPANY_PERSONNEL_IDENTITY"));
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue="HSE.ConfigurePersonnelIdentity"});await f.Db.SaveChangesAsync();
        var query=new CompanyPersonnelIdentityQuery("identity-subject","native-person");using var reviewed=await host.Http.PostAsJsonAsync(url+"/review",query);Assert.True(reviewed.StatusCode==HttpStatusCode.OK,reviewed.StatusCode+" "+await reviewed.Content.ReadAsStringAsync());var review=(await reviewed.Content.ReadFromJsonAsync<CompanyPersonnelIdentityReview>())!;review.Require();Assert.Equal(query,review.Query);Assert.Equal("Actual company account",review.Account.UserName);Assert.Equal("Recorded native person",review.Person.Name);
        var command=new CompanyPersonnelIdentityCommand(review,1,Guid.NewGuid(),utc,utc.AddDays(10),"Actual identity verification evidence","Private original administrator identity rationale",true);command.RequireCatalog(review.Catalog);
        using(var saved=await host.Http.PostAsJsonAsync(url,command)){Assert.True(saved.StatusCode==HttpStatusCode.OK,await saved.Content.ReadAsStringAsync());command.RequireRecorded((await saved.Content.ReadFromJsonAsync<COMPANY_PERSONNEL_IDENTITY>())!);}Assert.Equal(before+1,f.Writes.Count);
        var original=JsonSerializer.Serialize(f.Rows.OfType<COMPANY_PERSONNEL_IDENTITY>().Single());using(var recovered=await host.Http.PostAsJsonAsync(url+"/recovery",command)){Assert.True(recovered.StatusCode==HttpStatusCode.OK,await recovered.Content.ReadAsStringAsync());command.RequireRecorded((await recovered.Content.ReadFromJsonAsync<COMPANY_PERSONNEL_IDENTITY>())!);}
        var reference=new CompanyPersonnelIdentityReference(command.RecordId(),command.IntentId);using(var recovered=await host.Http.PostAsJsonAsync(url+"/recovery/reference",reference)){Assert.True(recovered.StatusCode==HttpStatusCode.OK,await recovered.Content.ReadAsStringAsync());(await recovered.Content.ReadFromJsonAsync<CompanyPersonnelIdentityRecovery>())!.Require(review.Context.Company,"actor",reference);}Assert.Equal(before+1,f.Writes.Count);
        using(var catalog=await host.Http.GetAsync(url)){Assert.True(catalog.StatusCode==HttpStatusCode.OK,await catalog.Content.ReadAsStringAsync());Assert.Single((await catalog.Content.ReadFromJsonAsync<CompanyPersonnelIdentityCatalog>())!.Versions);Assert.DoesNotContain(command.Reason,await catalog.Content.ReadAsStringAsync());}
        using(var duplicate=await host.Http.PostAsJsonAsync(url,command with{IntentId=Guid.NewGuid()}))Assert.Equal(HttpStatusCode.Conflict,duplicate.StatusCode);Assert.Equal(original,JsonSerializer.Serialize(f.Rows.OfType<COMPANY_PERSONNEL_IDENTITY>().Single()));Assert.Equal(before+1,f.Writes.Count);
        var personalReads=f.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && value.Arguments[0] as string=="BUSINESS_ASSOCIATE");using(var missingAccount=await host.Http.PostAsJsonAsync(url+"/review",query with{UserId="another-company-account"}))Assert.Equal(HttpStatusCode.Forbidden,missingAccount.StatusCode);Assert.Equal(personalReads,f.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && value.Arguments[0] as string=="BUSINESS_ASSOCIATE"));
        f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(value=>value.ClaimValue=="HSE.ConfigurePersonnelIdentity"));await f.Db.SaveChangesAsync();using(var revoked=await host.Http.PostAsJsonAsync(url+"/recovery/reference",reference))Assert.Equal(HttpStatusCode.Forbidden,revoked.StatusCode);Assert.Equal(before+1,f.Writes.Count);
    }
}
