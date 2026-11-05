using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Data.OilGas;
using Xunit;
using Moq;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Beep.Editor;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderDatabaseBindingTests
{
    [Fact]
    public async Task DirectNativeTransitionCannotAdoptAnUnrelatedProjectStateOrWriteWithoutOperationalEvidence()
    {
        var editor=new Mock<IDMEEditor>();var metadata=new Mock<IPPDMMetadataRepository>();var inspections=new Mock<IInspectionService>();
        var service=new WorkOrderService(editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,inspections.Object,()=>Task.FromResult(Context()),NullLogger<WorkOrderService>.Instance,(_,_,_,_)=>throw new InvalidOperationException("A generic transition cannot create another order"));
        await Assert.ThrowsAsync<RefusalException>(()=>service.TransitionStateAsync("field","order","IN_PROGRESS","actor"));
        metadata.Verify(value=>value.GetTableMetadataAsync("PROJECT"),Times.Never);Assert.Empty(editor.Invocations);Assert.Empty(inspections.Invocations);
        await Assert.ThrowsAsync<RefusalException>(()=>service.TransitionStateAsync("field","order","COMPLETED","foreign"));Assert.Empty(editor.Invocations);
    }
    private static (CompanyScope Company,string Actor,AssetDatabaseScope Database) Context()=> (new("tenant","company"),"actor",new("company-db","physical-target"));
    [Fact] public async Task EveryAccessRevalidatesWithExistingOwner(){var calls=0;var binding=new WorkOrderDatabaseBinding(()=>{++calls;return Task.FromResult(Context());});Assert.Equal("company-db",await binding.RequireAsync());Assert.Equal("company-db",await binding.RequireAsync("actor"));Assert.Equal(2,calls);}
    [Theory][InlineData("company")][InlineData("tenant")][InlineData("actor")][InlineData("connection")][InlineData("physical")]
    public async Task OriginalContextCannotAdoptAnotherTarget(string changed){var current=Context();var binding=new WorkOrderDatabaseBinding(()=>Task.FromResult(current));await binding.RequireAsync();current=changed switch{"company"=>(new("tenant","other"),current.Actor,current.Database),"tenant"=>(new("other","company"),current.Actor,current.Database),"actor"=>(current.Company,"other",current.Database),"connection"=>(current.Company,current.Actor,new("other-db",current.Database.Fingerprint)),_=>(current.Company,current.Actor,new(current.Database.ConnectionName,"changed-physical"))};await Assert.ThrowsAsync<RefusalException>(()=>binding.RequireAsync());}
    [Theory][InlineData("company")][InlineData("tenant")][InlineData("actor")][InlineData("connection")][InlineData("physical")][InlineData("nullcompany")][InlineData("nulldatabase")]
    public async Task MissingBindingCannotUseDefaultConnection(string missing){var current=Context();current=missing switch{"company"=>(new("tenant",""),current.Actor,current.Database),"tenant"=>(new("","company"),current.Actor,current.Database),"actor"=>(current.Company,"",current.Database),"connection"=>(current.Company,current.Actor,new("",current.Database.Fingerprint)),"physical"=>(current.Company,current.Actor,new(current.Database.ConnectionName,"")),"nullcompany"=>(null!,current.Actor,current.Database),_=>(current.Company,current.Actor,null!)};var binding=new WorkOrderDatabaseBinding(()=>Task.FromResult(current));await Assert.ThrowsAsync<RefusalException>(()=>binding.RequireAsync());}
    [Fact] public async Task WrongMutationActorCannotWrite(){var binding=new WorkOrderDatabaseBinding(()=>Task.FromResult(Context()));await Assert.ThrowsAsync<RefusalException>(()=>binding.RequireAsync("SYSTEM"));}
    [Fact] public async Task ResolverFailurePropagates(){var binding=new WorkOrderDatabaseBinding(()=>throw new HttpRequestException());await Assert.ThrowsAsync<HttpRequestException>(()=>binding.RequireAsync());}
    [Fact] public async Task ChangedPhysicalTargetAfterReadCannotReturnRows()
    {
        var current=Context();var binding=new WorkOrderDatabaseBinding(()=>Task.FromResult(current));
        await Assert.ThrowsAsync<RefusalException>(()=>binding.ReadAsync(()=>{
            current=(current.Company,current.Actor,new(current.Database.ConnectionName,"other-target"));
            return Task.FromResult(new[]{"foreign row"});
        }));
    }
    [Fact] public async Task UnavailableInitialBindingCannotStartProviderRead()
    {
        var binding=new WorkOrderDatabaseBinding(()=>Task.FromResult((new CompanyScope("tenant","company"),"actor",new AssetDatabaseScope("",""))));
        var reads=0;await Assert.ThrowsAsync<RefusalException>(()=>binding.ReadAsync(()=>{++reads;return Task.FromResult(1);}));Assert.Equal(0,reads);
    }
}
