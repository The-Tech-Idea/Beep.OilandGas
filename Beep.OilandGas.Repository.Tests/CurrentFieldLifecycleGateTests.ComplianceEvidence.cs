using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Compliance;
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Controllers.Compliance;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Identity;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.Report;
using Moq;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CurrentFieldLifecycleGateTests
{
    [Theory][InlineData(true,200)][InlineData(false,403)]
    public async Task RecordedObligationHttpUsesRealApplicationPermissionAndFieldGrant(bool permitted,int expected)
    {
        using var f=await Ready();
        if(permitted){f.Db.RoleClaims.Add(new IdentityRoleClaim<string>{RoleId="admin",ClaimType="permission",ClaimValue=PermissionConstants.Regulatory.ViewCompliance});await f.Db.SaveChangesAsync();await f.Select("company");}
        var owner=Profile(f);await Save(owner);
        f.Rows.AddRange([new OBLIGATION{OBLIGATION_ID="case",OBLIGATION_SEQ_NO=2,ACTIVE_IND="Y",OBLIGATION_TYPE="recorded"},new OBLIGATION_COMPONENT{OBLIGATION_ID="case",OBLIGATION_SEQ_NO=2,COMPONENT_OBS_NO=1,ACTIVE_IND="Y",FIELD_ID="A"}]);
        var assets=new RepositoryAssetAccessStore(f.Db,f.Gate,Mock.Of<IAssetGrantTargetVerifier>());
        var access=new UserAssetAccessService(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,new RepositoryApplicationAuthorizationReader(f.Gate),Mock.Of<IApplicationRolePermissionStore>(),assets,()=>f.Gate.ResolveAsync(),_=>f.Policy,Mock.Of<ICompanyFieldInterestReview>());
        using var boundary=new Boundary(f,owner);
        await using var host=await HttpHost.Start(f,owner,boundary,true,typeof(RecordedObligationsController),services=>services.AddSingleton(new CompanyFieldObligationReader(f.Gate,owner,access,f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object)),accessOverride:access);
        using var response=await host.Http.GetAsync(host.Url+"/api/field/current/compliance/recorded-obligations");Assert.Equal(expected,(int)response.StatusCode);
        if(permitted){var result=await response.Content.ReadFromJsonAsync<FieldObligationSourceReview>();Assert.Equal("case",Assert.Single(result!.Obligations).ObligationId);}
        else f.Source.Verify(x=>x.GetEntityAsync("OBLIGATION_COMPONENT",It.IsAny<List<AppFilter>>()),Times.Never);
        Assert.Empty(f.Writes);
    }
    [Theory][InlineData("allowed",200)][InlineData("anonymous",401)][InlineData("denied",403)]
    public async Task RecordedObligationHttpUsesCurrentFieldAndCompliancePermission(string mode, int expected)
    {
        using var f=await Ready();var owner=Profile(f);await Save(owner);
        f.Rows.AddRange([new OBLIGATION{OBLIGATION_ID="case",OBLIGATION_SEQ_NO=2,ACTIVE_IND="Y",OBLIGATION_TYPE="recorded"},new OBLIGATION_COMPONENT{OBLIGATION_ID="case",OBLIGATION_SEQ_NO=2,COMPONENT_OBS_NO=1,ACTIVE_IND="Y",FIELD_ID="A"},new OBLIGATION_COMPONENT{OBLIGATION_ID="foreign",OBLIGATION_SEQ_NO=1,COMPONENT_OBS_NO=1,ACTIVE_IND="Y",FIELD_ID="B"}]);
        using var boundary=new Boundary(f,owner,mode!="denied");
        await using var host=await HttpHost.Start(f,owner,boundary,mode!="anonymous",typeof(RecordedObligationsController),services=>services.AddSingleton(new CompanyFieldObligationReader(f.Gate,owner,boundary.Access.Object,f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object)));
        using var response=await host.Http.GetAsync(host.Url+"/api/field/current/compliance/recorded-obligations");
        Assert.Equal(expected,(int)response.StatusCode);
        if(mode=="allowed")
        {
            var result=await response.Content.ReadFromJsonAsync<FieldObligationSourceReview>();Assert.NotNull(result);Assert.Equal("A",result.Context.FieldId);Assert.Equal("case",Assert.Single(result.Obligations).ObligationId);
            using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(result.Context.Company,["SafetyOfficer"],[PermissionConstants.Regulatory.ViewCompliance])));
            using var transport=new HttpClient{BaseAddress=new Uri(host.Url)};var client=new ComplianceServiceClient(new ApiClient(transport,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),company),NullLogger<ComplianceServiceClient>.Instance,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>());
            var viaClient=await client.GetRecordedFieldObligationsAsync();Assert.Equal(result.Context,viaClient.Context);Assert.Equal("case",Assert.Single(viaClient.Obligations).ObligationId);
            boundary.Access.Verify(x=>x.CheckAssetAccessAsync("actor","A","FIELD",PermissionConstants.Regulatory.ViewCompliance,null),Times.AtLeast(3));
        }
        else f.Source.Verify(x=>x.GetEntityAsync("OBLIGATION_COMPONENT",It.IsAny<List<AppFilter>>()),Times.Never);
        Assert.Empty(f.Writes);
    }
}
