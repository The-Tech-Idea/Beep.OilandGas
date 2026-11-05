using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Processes;
using Moq;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyProcessReadTests
{
    private static ProcessInstance Source()=>new(){InstanceId="process",FieldId="field",EntityId="well",EntityType="WELL"};
    private sealed class Fixture
    {
        public Mock<IProcessService> Process = new(MockBehavior.Strict);
        public Mock<IAccessControlService> Resources = new(MockBehavior.Strict);
        public CompanyProcessReadService Service;
        public Fixture()
        {
            Process.Setup(x=>x.GetProcessInstanceAsync("process")).ReturnsAsync(Source());
            Process.Setup(x=>x.GetProcessHistoryAsync("process")).ReturnsAsync([new(){HistoryId="history",InstanceId="process"}]);
            Resources.Setup(x=>x.CheckAssetAccessAsync("actor","field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse { HasAccess=true });
            Resources.Setup(x=>x.CheckAssetAccessAsync("actor","well","WELL","READ",null)).ReturnsAsync(new AccessCheckResponse { HasAccess=true });
            Service=new(Resources.Object);
        }
    }
    [Fact] public async Task ValidSourceAndHistoryRequireFreshReadAuthority(){var f=new Fixture();Assert.Equal("well",(await f.Service.ReadAsync(f.Process.Object,"actor","field","process"))!.EntityId);Assert.Single((await f.Service.HistoryAsync(f.Process.Object,"actor","field","process"))!);f.Resources.Verify(x=>x.CheckAssetAccessAsync("actor","well","WELL","READ",null),Times.Exactly(4));}
    [Theory][InlineData(false)][InlineData(true)] public async Task FieldOrEntityDenialPreventsHistory(bool entity){var f=new Fixture();f.Resources.Setup(x=>x.CheckAssetAccessAsync("actor",entity?"well":"field",entity?"WELL":"FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse { HasAccess=false });await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Service.HistoryAsync(f.Process.Object,"actor","field","process"));f.Process.Verify(x=>x.GetProcessHistoryAsync("process"),Times.Never());}
    [Theory][InlineData(false)][InlineData(true)] public async Task ClosingRevocationDoesNotReturnData(bool history){var f=new Fixture();f.Resources.SetupSequence(x=>x.CheckAssetAccessAsync("actor","well","WELL","READ",null)).ReturnsAsync(new AccessCheckResponse { HasAccess=true }).ReturnsAsync(new AccessCheckResponse { HasAccess=false });if(history)await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Service.HistoryAsync(f.Process.Object,"actor","field","process"));else await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Service.ReadAsync(f.Process.Object,"actor","field","process"));}
    [Theory][InlineData(false)][InlineData(true)] public async Task ChangedSourceLinkageCannotReturnData(bool history){var f=new Fixture();var changed=Source();changed.EntityId="foreign";f.Process.SetupSequence(x=>x.GetProcessInstanceAsync("process")).ReturnsAsync(Source()).ReturnsAsync(changed);if(history)await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.HistoryAsync(f.Process.Object,"actor","field","process"));else await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.ReadAsync(f.Process.Object,"actor","field","process"));}
    [Fact] public async Task ForeignHistoryCannotReturn(){var f=new Fixture();f.Process.Setup(x=>x.GetProcessHistoryAsync("process")).ReturnsAsync([new(){HistoryId="history",InstanceId="foreign"}]);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.HistoryAsync(f.Process.Object,"actor","field","process"));}
    [Fact] public async Task MissingSourceDoesNotLoadHistory(){var f=new Fixture();f.Process.Setup(x=>x.GetProcessInstanceAsync("process")).ReturnsAsync((ProcessInstance?)null);Assert.Null(await f.Service.HistoryAsync(f.Process.Object,"actor","field","process"));f.Process.Verify(x=>x.GetProcessHistoryAsync("process"),Times.Never());}
}
