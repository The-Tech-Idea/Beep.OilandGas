using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CurrentFieldLifecycleGateTests
{
    [Theory]
    [InlineData("company","current")][InlineData("other-company","current")]
    [InlineData("company","empty")][InlineData("company","standard-role")][InlineData("company","wrong-party")]
    [InlineData("company","app-role")][InlineData("company","revoked-field")][InlineData("company","binding")]
    [InlineData("company","role-during-read")][InlineData("company","provider")][InlineData("company","failed")]
    [InlineData("company","predicate")]
    public async Task ProtectedExecutivePortfolioUsesCurrentAppAuthorityAndNativeCompanyFields(string organization,string mode)
    {
        using var f=await CompanyPPDMRowAccessTests.Fixture.Create();if(organization!="company")await f.Select(organization);
        f.Rows.AddRange([new FIELD{FIELD_ID="shared-id",FIELD_NAME="Recorded "+organization+" field",ACTIVE_IND="Y"},new FIELD{FIELD_ID="private-other",FIELD_NAME="Private other field",ACTIVE_IND="Y"},new FIELD{FIELD_ID="inactive",FIELD_NAME="Inactive field",ACTIVE_IND="N"}]);
        if(mode!="empty")await f.Grant("FIELD","shared-id","READ");
        var original=f.Gate.Company;var changed=false;
        if(mode is "revoked-field" or "binding" or "role-during-read" or "provider")f.AfterRead=async()=>
        {
            if(changed || f.Source.Invocations.Last(call=>call.Method.Name=="GetEntityAsync").Arguments[0] as string!="FIELD")return;changed=true;
            if(mode=="revoked-field"){var grant=await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FIELD" && row.AssetId=="shared-id");grant.IsActive=false;await f.Db.SaveChangesAsync();}
            if(mode=="binding"){var binding=await f.Db.ModuleDatabases.SingleAsync(row=>row.ModuleId=="PPDM_CORE");binding.ConcurrencyStamp="changed-executive-binding";await f.Db.SaveChangesAsync();}
            if(mode=="role-during-read"){f.Db.Set<AppUserRoleExtension>().RemoveRange(await f.Db.Set<AppUserRoleExtension>().Where(row=>row.UserId=="actor").ToArrayAsync());await f.Db.SaveChangesAsync();}
            if(mode=="provider")f.Editor.Setup(editor=>editor.GetDataSource("selected-tenant")).Returns(Moq.Mock.Of<TheTechIdea.Beep.IDataSource>());
        };
        if(mode=="app-role"){f.Db.Set<AppUserRoleExtension>().RemoveRange(await f.Db.Set<AppUserRoleExtension>().Where(row=>row.UserId=="actor").ToArrayAsync());await f.Db.SaveChangesAsync();}
        if(mode=="failed")f.Source.Setup(source=>source.GetEntityAsync("FIELD",It.IsAny<List<AppFilter>>())).ThrowsAsync(new InvalidOperationException("Controlled native source unavailable"));
        if(mode=="predicate")f.IgnoreFilters=true;
        var profile=Profile(f);using var boundary=new Boundary(f,profile);
        await using var host=await HttpHost.Start(f,profile,boundary,true,typeof(ExecutiveController),services=>
        {
            services.AddSingleton(f.Editor.Object);services.AddSingleton(f.Columns.Object);services.AddSingleton(f.Defaults.Object);services.AddSingleton(f.Metadata.Object);services.AddSingleton<IPPDMRowAccessPolicy>(f.Policy);services.AddSingleton<ExecutiveAggregationService>();
            if(mode!="standard-role")services.AddSingleton<IClaimsTransformation,HandoffRoleBridge>();
            if(mode=="wrong-party")
            {
                services.AddSingleton<Func<string>>(()=>"other-account");services.AddAuthentication(options=>{options.DefaultAuthenticateScheme="receiving-fixture";options.DefaultChallengeScheme="receiving-fixture";}).AddScheme<AuthenticationSchemeOptions,ReceivingAuthentication>("receiving-fixture",_=>{});
            }
        });
        host.Http.BaseAddress=new Uri(host.Url);
        using var response=await host.Http.GetAsync(host.Url+"/api/executive/portfolio");
        if(mode is "current" or "empty")
        {
            Assert.True(response.StatusCode==HttpStatusCode.OK,await response.Content.ReadAsStringAsync());Assert.Equal("no-store",response.Headers.CacheControl?.ToString());
            var result=(await response.Content.ReadFromJsonAsync<ExecutivePortfolioReview>())!;ExecutivePortfolioChecks.Require(result,new("tenant",organization),"actor");
            if(mode=="empty")Assert.Empty(result.Assets);
            else{var field=Assert.Single(result.Assets);Assert.Equal("shared-id",field.FieldId);Assert.Equal("Recorded "+organization+" field",field.FieldName);Assert.Null(field.ActiveWells);Assert.Null(field.LifecyclePhase);Assert.NotSame(original,f.Gate.Company);}
            using var clientScope=new Beep.OilandGas.Client.App.CompanyClientContext();Assert.True(clientScope.TrySelect(clientScope.Clear(),"actor",new(result.Company,f.Gate.Company.Roles.ToArray(),f.Gate.Company.Permissions.ToArray())));
            var client=new Beep.OilandGas.Web.Services.ExecutiveServiceClient(new Beep.OilandGas.Web.Services.ApiClient(host.Http,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),clientScope));
            var clientResult=await client.GetPortfolioAsync();ExecutivePortfolioChecks.Require(clientResult,result.Company,result.Actor);Assert.Equal(result.Assets,clientResult.Assets);
            using var assets=await host.Http.GetAsync(host.Url+"/api/executive/assets");Assert.True(assets.StatusCode==HttpStatusCode.OK,await assets.Content.ReadAsStringAsync());Assert.Equal("no-store",assets.Headers.CacheControl?.ToString());Assert.Equal(result.Assets.Count,(await assets.Content.ReadFromJsonAsync<ExecutiveAsset[]>())!.Length);
            using var kpi=await host.Http.GetAsync(host.Url+"/api/executive/kpi");Assert.Equal(HttpStatusCode.NotImplemented,kpi.StatusCode);
        }
        else
        {
            Assert.False(response.IsSuccessStatusCode,await response.Content.ReadAsStringAsync());var body=await response.Content.ReadAsStringAsync();Assert.DoesNotContain("Recorded "+organization+" field",body);Assert.DoesNotContain("Private other field",body);Assert.DoesNotContain("\"assets\":[]",body);
            if(mode is "standard-role" or "wrong-party" or "app-role")Assert.DoesNotContain(f.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="FIELD");
            if(mode is "revoked-field" or "binding" or "role-during-read" or "provider")Assert.True(changed);
        }
        Assert.Empty(f.Writes);Assert.Empty(f.LeaseTransactions);
    }
}
