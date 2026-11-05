using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentFacilityEvidenceControllerTests
{
    [Theory][InlineData("valid")][InlineData("empty")][InlineData("foreign")][InlineData("missing-id")][InlineData("missing-type")][InlineData("duplicate")][InlineData("distinct-types")][InlineData("null")][InlineData("changed-permission")][InlineData("wrong-field")]
    public async Task EndpointConfirmsAuthorityAndActualFacilityOwnerBeforeReturning(string mode)
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();var authority=new DevelopmentFacilityReadAuthority(f.Account,f.Company,f.Fields.Object,f.Preference.Object,f.Assets.Object,f.Grants.Object);
        var field=new Mock<IFieldOrchestrator>();field.SetupGet(x=>x.CurrentFieldId).Returns(mode=="wrong-field"?"other":"field");
        var owner=new Mock<IFieldDevelopmentService>();
        owner.Setup(x=>x.GetFacilitiesForFieldAsync("field",It.IsAny<List<AppFilter>?>())).Returns(()=>
        {
            if(mode=="changed-permission")f.Permissions=[];
            if(mode=="null")return Task.FromResult<List<FACILITY>>(null!);
            var row=new FACILITY{FACILITY_ID=mode=="missing-id"?"":"id",FACILITY_TYPE=mode=="missing-type"?"":"PROCESSING",PRIMARY_FIELD_ID=mode=="foreign"?"other":"field"};
            return Task.FromResult(mode=="empty"?new List<FACILITY>():mode=="duplicate"?[row,row]:mode=="distinct-types"?[row,new FACILITY{FACILITY_ID="id",FACILITY_TYPE="STORAGE",PRIMARY_FIELD_ID="field"}]:[row]);
        });
        var controller=new DevelopmentFacilityEvidenceController(authority,field.Object,owner.Object){ControllerContext=new(){HttpContext=f.Http.HttpContext!}};
        if(mode is "valid" or "empty" or "distinct-types")
        {
            var result=Assert.IsType<OkObjectResult>((await controller.GetAsync()).Result);var rows=Assert.IsType<List<FACILITY>>(result.Value);
            Assert.Equal(mode=="empty"?0:mode=="distinct-types"?2:1,rows.Count);Assert.True(f.AssetReads>=2);
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>controller.GetAsync());
        owner.Verify(x=>x.GetFacilitiesForFieldAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>?>()),mode=="wrong-field"?Times.Never():Times.Once());
    }
}
#endif
