using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderListStateTests
{
    [Theory][InlineData("ORDER/1")][InlineData("WO-42")][InlineData(" WO-42 ")][InlineData("CORRECTIVE")][InlineData("component")][InlineData("well-UWI")][InlineData("equipment-42")]
    public async Task QueueSearchFindsOnlyConfirmedRecordedHeaderOrComponent(string query)
    {
        using var company=Company();using var state=new WorkOrderListState(company);var row=Order();row.InstanceName="WO-42";row.Source="WORK_ORDER";row.State="";row.EvidenceUnavailableReason="No recorded workflow state";row.Components=[new(row.InstanceId,"component","field","well-UWI","equipment-42")];
        await state.LoadAsync(null,()=>Task.FromResult<string?>("field"),_=>Task.FromResult(new List<WorkOrderSummary>{row}));
        Assert.True(state.Matches(row,query));Assert.False(state.Matches(row,"foreign"));Assert.False(state.Matches(Order(),query));
        company.Clear();Assert.False(state.Matches(row,query));Assert.False(state.Matches(row,null));
    }
    private static CompanyClientContext Company(){var c=new CompanyClientContext();Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant","A"),[],[])));return c;}
    private static WorkOrderSummary Order()=>new(){InstanceId="order/1",FieldId="field",State=WorkOrderState.Cancelled,WoSubType=WorkOrderSubType.Corrective};
    [Fact] public async Task RecordedCancelledOrdersAndFieldPublishWithoutDroppingRows()
    {
        using var c=Company();using var s=new WorkOrderListState(c);await s.LoadAsync(null,()=>Task.FromResult<string?>("field"),_=>Task.FromResult(new List<WorkOrderSummary>{Order()}));
        Assert.True(s.Confirmed);Assert.Equal("field",s.FieldId);Assert.Equal(WorkOrderState.Cancelled,Assert.Single(s.Rows).State);
    }
    [Theory][InlineData("field")][InlineData("identity")][InlineData("state")][InlineData("subtype")][InlineData("duplicate")][InlineData("null")]
    public async Task UnconfirmedRowsCannotPublish(string fault)
    {
        using var c=Company();using var s=new WorkOrderListState(c);var row=Order();List<WorkOrderSummary> rows=[row];
        switch(fault){case "field":row.FieldId="other";break;case "identity":row.InstanceId="";break;case "state":row.State="UNKNOWN";break;case "subtype":row.WoSubType=WorkOrderSubType.Preventive;break;case "duplicate":rows.Add(row);break;default:rows=null!;break;}
        await Assert.ThrowsAsync<InvalidOperationException>(()=>s.LoadAsync(WorkOrderSubType.Corrective,()=>Task.FromResult<string?>("field"),_=>Task.FromResult(rows)));Assert.False(s.Confirmed);Assert.Empty(s.Rows);Assert.Null(s.FieldId);
    }
    [Fact] public async Task EmptyRecordedResponseIsConfirmedButFailedReloadIsUnavailable()
    {
        using var c=Company();using var s=new WorkOrderListState(c);await s.LoadAsync(null,()=>Task.FromResult<string?>("field"),_=>Task.FromResult(new List<WorkOrderSummary>()));Assert.True(s.Confirmed);Assert.Empty(s.Rows);
        await Assert.ThrowsAsync<HttpRequestException>(()=>s.LoadAsync(null,()=>Task.FromResult<string?>("field"),_=>throw new HttpRequestException()));Assert.False(s.Confirmed);Assert.Empty(s.Rows);
    }
    [Fact] public async Task CompanySwitchClearsDisplayedOrdersAndDiscardsLateResponse()
    {
        using var c=Company();using var s=new WorkOrderListState(c);await s.LoadAsync(null,()=>Task.FromResult<string?>("field"),_=>Task.FromResult(new List<WorkOrderSummary>{Order()}));
        var pending=new TaskCompletionSource<List<WorkOrderSummary>>(TaskCreationOptions.RunContinuationsAsynchronously);var load=s.LoadAsync(null,()=>Task.FromResult<string?>("field"),_=>pending.Task);
        c.Clear();Assert.Empty(s.Rows);pending.SetResult([Order()]);await load;Assert.False(s.Confirmed);Assert.Empty(s.Rows);
    }
    [Fact] public async Task FieldChangeDuringLookupCannotStartRead()
    {
        using var c=Company();using var s=new WorkOrderListState(c);var field=new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);var reads=0;
        var load=s.LoadAsync(null,()=>field.Task,_=>{reads++;return Task.FromResult(new List<WorkOrderSummary>{Order()});});s.Clear();field.SetResult("field");await load;Assert.Equal(0,reads);
    }
    [Fact] public async Task NoSelectedCompanyCannotRequestWorkOrders()
    {
        using var c=new CompanyClientContext();using var s=new WorkOrderListState(c);await s.LoadAsync(null,()=>throw new Exception("no field read"),_=>throw new Exception("no work read"));Assert.False(s.Confirmed);
    }
    [Theory][InlineData("InstanceId")][InlineData("FieldId")][InlineData("State")]
    public void MissingJsonScopeOrStateCannotBecomeADraftRecord(string field)
    {
        var values=new Dictionary<string,object>{{"InstanceId","order"},{"FieldId","field"},{"State","COMPLETED"}};values.Remove(field);
        Assert.Throws<JsonException>(()=>JsonSerializer.Deserialize<WorkOrderSummary>(JsonSerializer.Serialize(values)));
    }
}
