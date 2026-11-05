using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.WorkOrder;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderSourceChecksTests
{
    private static WorkOrderDetailModel Row()=>new(){InstanceId="order",FieldId="field",WoSubType="WO-CORRECTIVE",State="DRAFT"};
    [Fact] public async Task OriginalSourceRequiresAuthorityBeforeAndAfterRead(){var checks=0;var reads=0;var row=await WorkOrderSourceChecks.RequireAsync("field","order",()=>{++checks;return Task.FromResult(true);},()=>{++reads;return Task.FromResult<WorkOrderDetailModel?>(Row());});Assert.Equal("order",row.InstanceId);Assert.Equal(2,checks);Assert.Equal(1,reads);}
    [Fact] public async Task DeniedAuthorityCannotReadSource(){var reads=0;var refusal=await Assert.ThrowsAsync<RefusalException>(()=>WorkOrderSourceChecks.RequireAsync("field","order",()=>Task.FromResult(false),()=>{++reads;return Task.FromResult<WorkOrderDetailModel?>(Row());}));Assert.Equal(RefusalKind.Forbidden,refusal.Kind);Assert.Equal(0,reads);}
    [Theory][InlineData("field")][InlineData("identity")][InlineData("state")][InlineData("subtype")][InlineData("null")]
    public async Task WrongOrMissingSourceCannotAdmitChildOperations(string fault){var row=Row();if(fault=="field")row.FieldId="foreign";if(fault=="identity")row.InstanceId="foreign";if(fault=="state")row.State="";if(fault=="subtype")row.WoSubType="";var refusal=await Assert.ThrowsAsync<RefusalException>(()=>WorkOrderSourceChecks.RequireAsync("field","order",()=>Task.FromResult(true),()=>Task.FromResult<WorkOrderDetailModel?>(fault=="null"?null:row)));Assert.Equal(fault=="null"?RefusalKind.NotFound:RefusalKind.Conflict,refusal.Kind);}
    [Theory][InlineData("", "order")][InlineData("field", "")]
    public async Task MissingOriginalContextCannotCheckAuthority(string field,string id){var checks=0;await Assert.ThrowsAsync<RefusalException>(()=>WorkOrderSourceChecks.RequireAsync(field,id,()=>{++checks;return Task.FromResult(true);},()=>Task.FromResult<WorkOrderDetailModel?>(Row())));Assert.Equal(0,checks);}
    [Fact] public async Task RevokedAuthorityCannotDeliverSource(){var checks=0;var refusal=await Assert.ThrowsAsync<RefusalException>(()=>WorkOrderSourceChecks.RequireAsync("field","order",()=>Task.FromResult(++checks==1),()=>Task.FromResult<WorkOrderDetailModel?>(Row())));Assert.Equal(RefusalKind.Forbidden,refusal.Kind);}
    [Fact] public async Task ReadFailureDoesNotBecomeAbsentOrder()=>await Assert.ThrowsAsync<HttpRequestException>(()=>WorkOrderSourceChecks.RequireAsync("field","order",()=>Task.FromResult(true),()=>throw new HttpRequestException()));
    [Fact] public async Task AuthorityFailureDoesNotBecomePermission()=>await Assert.ThrowsAsync<HttpRequestException>(()=>WorkOrderSourceChecks.RequireAsync("field","order",()=>throw new HttpRequestException(),()=>Task.FromResult<WorkOrderDetailModel?>(Row())));
}
