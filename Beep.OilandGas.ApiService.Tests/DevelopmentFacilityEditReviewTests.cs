#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TheTechIdea.Beep.Report;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DevelopmentFacilityEditReviewTests
{
    [Theory][InlineData("valid")][InlineData("permission")][InlineData("role")][InlineData("facility-write")][InlineData("field-write")][InlineData("wrong-field")][InlineData("missing")][InlineData("duplicate")][InlineData("changed")][InlineData("revoked")]
    public async Task ReviewRequiresExactCurrentFacilityAndWriteAuthority(string mode)
    {
        using var f=new DevelopmentDraftAuthorityTests.Fixture();f.Permissions=[..f.Permissions,PermissionConstants.DevelopmentPlanning.Edit];
        f.Assets.Setup(a=>a.CheckAssetAccessAsync("planner","id","FACILITY","WRITE","PROCESSING")).ReturnsAsync(new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=mode!="facility-write"});
        if(mode=="permission")f.Permissions=[PermissionConstants.DevelopmentPlanning.View];if(mode=="field-write")f.FieldAccess=false;
        var authority=new DevelopmentFacilityReadAuthority(f.Account,f.Company,f.Fields.Object,f.Preference.Object,f.Assets.Object,f.Grants.Object);
        var field=new Mock<IFieldOrchestrator>();field.SetupGet(x=>x.CurrentFieldId).Returns(mode=="wrong-field"?"other":"field");
        var owner=new Mock<IFieldDevelopmentService>();var reads=0;
        owner.Setup(x=>x.GetFacilitiesForFieldAsync("field",It.IsAny<List<AppFilter>?>())).Returns(()=>
        {
            reads++;if(mode=="revoked")f.Permissions=[];
            var row=new FACILITY{FACILITY_ID="id",FACILITY_TYPE="PROCESSING",PRIMARY_FIELD_ID="field",FACILITY_LONG_NAME=mode=="changed" && reads>1?"Changed":"Recorded"};
            return Task.FromResult(mode=="missing"?new List<FACILITY>():mode=="duplicate"?[row,row]:[row]);
        });
        var controller=new DevelopmentFacilityEvidenceController(authority,field.Object,owner.Object){ControllerContext=new(){HttpContext=f.Http.HttpContext!}};
        if(mode=="valid")
        {
            var result=Assert.IsType<OkObjectResult>((await controller.ReviewEditAsync("id","PROCESSING","ConfiguredPlanner")).Result);
            var review=Assert.IsType<TheTechIdea.Data.OilGas.DevelopmentFacilityEditReview>(result.Value);review.Require();Assert.Equal("Recorded",review.Original.FACILITY_LONG_NAME);Assert.Equal(2,reads);
        }
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>controller.ReviewEditAsync("id","PROCESSING",mode=="role"?"OtherRole":"ConfiguredPlanner"));
            Assert.Equal(mode is "changed"?2:mode is "missing" or "duplicate" or "revoked"?1:0,reads);
        }
    }
}
#endif
