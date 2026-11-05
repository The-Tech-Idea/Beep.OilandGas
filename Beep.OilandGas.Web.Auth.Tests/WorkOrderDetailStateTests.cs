using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderDetailStateTests
{
    private static CompanyClientContext Company(){var c=new CompanyClientContext();Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant","company"),[],[])));return c;}
    private static WorkOrderDetailModel Row(string state="DRAFT")=>new(){InstanceId="order",FieldId="field",WoSubType="WO-CORRECTIVE",State=state};
    private static Task Load(WorkOrderDetailState s)=>s.LoadAsync("order",()=>Task.FromResult<string?>("field"),_=>Task.FromResult<WorkOrderDetailModel?>(Row()));
    [Fact] public async Task ConfirmRequiresResponseAndRecordedReload(){using var c=Company();using var s=new WorkOrderDetailState(c);await Load(s);var a=s.Begin("PLANNED","original notes",["PLANNED"]);Assert.True(s.Blocked);Assert.Equal("DRAFT",a.Request.ExpectedState);Assert.Equal("field",a.Request.ExpectedFieldId);s.Confirm(a,Row("PLANNED"),Row("PLANNED"));Assert.False(s.Blocked);}
    [Theory][InlineData("id")][InlineData("field")][InlineData("state")][InlineData("subtype")][InlineData("null")]
    public async Task WrongResponseKeepsOriginalIntentBlocked(string fault){using var c=Company();using var s=new WorkOrderDetailState(c);await Load(s);var a=s.Begin("PLANNED",null,["PLANNED"]);var r=Row("PLANNED");if(fault=="id")r.InstanceId="other";if(fault=="field")r.FieldId="other";if(fault=="state")r.State="DRAFT";if(fault=="subtype")r.WoSubType="other";Assert.Throws<InvalidOperationException>(()=>s.Confirm(a,fault=="null"?null:r,Row("PLANNED")));Assert.True(s.Blocked);Assert.Throws<InvalidOperationException>(()=>s.Begin("PLANNED",null,["PLANNED"]));}
    [Fact] public async Task WrongReadbackCannotConfirmAcknowledgedCommand(){using var c=Company();using var s=new WorkOrderDetailState(c);await Load(s);var a=s.Begin("PLANNED",null,["PLANNED"]);Assert.Throws<InvalidOperationException>(()=>s.Confirm(a,Row("PLANNED"),Row()));Assert.True(s.Blocked);}
    [Theory][InlineData("id")][InlineData("field")][InlineData("state")][InlineData("null")]
    public async Task DetailMustMatchOriginalIdentityAndField(string fault){using var c=Company();using var s=new WorkOrderDetailState(c);var r=Row();if(fault=="id")r.InstanceId="other";if(fault=="field")r.FieldId="other";if(fault=="state")r.State="";await Assert.ThrowsAsync<InvalidOperationException>(()=>s.LoadAsync("order",()=>Task.FromResult<string?>("field"),_=>Task.FromResult<WorkOrderDetailModel?>(fault=="null"?null:r)));Assert.Null(s.Current);}
    [Fact] public async Task LateCompanyReadCannotPublish(){using var c=Company();using var s=new WorkOrderDetailState(c);var wait=new TaskCompletionSource<WorkOrderDetailModel?>();var load=s.LoadAsync("order",()=>Task.FromResult<string?>("field"),_=>wait.Task);c.Clear();wait.SetResult(Row());await load;Assert.Null(s.Current);}
    [Fact] public async Task OriginalCompanyOutcomeStaysBlockedAfterAwayAndBack(){using var c=Company();using var s=new WorkOrderDetailState(c);await Load(s);var a=s.Begin("PLANNED",null,["PLANNED"]);c.Clear();Assert.False(s.IsCurrent(a,"order"));Assert.Null(s.Current);Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant","company"),[],[])));await Load(s);Assert.True(s.Blocked);}
    [Fact] public async Task DifferentCompanyDoesNotInheritOriginalUncertainty(){using var c=Company();using var s=new WorkOrderDetailState(c);await Load(s);s.Begin("PLANNED",null,["PLANNED"]);Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant","other"),[],[])));await Load(s);Assert.False(s.Blocked);}
    [Theory][InlineData("")][InlineData("DRAFT")][InlineData("COMPLETED")]
    public async Task UnreviewedDestinationCannotSubmit(string destination){using var c=Company();using var s=new WorkOrderDetailState(c);await Load(s);Assert.Throws<InvalidOperationException>(()=>s.Begin(destination,null,["PLANNED"]));Assert.False(s.Blocked);}
    [Fact] public async Task NoCompanyDoesNotRead(){using var c=new CompanyClientContext();using var s=new WorkOrderDetailState(c);await s.LoadAsync("order",()=>throw new Exception(),_=>throw new Exception());Assert.Null(s.Current);}
    [Fact] public async Task DisposalRefusesLateReadAndSubmission(){using var c=Company();var s=new WorkOrderDetailState(c);var wait=new TaskCompletionSource<WorkOrderDetailModel?>();var load=s.LoadAsync("order",()=>Task.FromResult<string?>("field"),_=>wait.Task);s.Dispose();wait.SetResult(Row());await load;Assert.Null(s.Current);Assert.Throws<InvalidOperationException>(()=>s.Begin("PLANNED",null,["PLANNED"]));}
    [Theory][InlineData("ExpectedFieldId")][InlineData("ExpectedState")]
    public void MissingReviewedScopeCannotDeserialize(string missing)
    {
        var payload=new Dictionary<string,object?>{{"ExpectedFieldId","field"},{"ExpectedState","DRAFT"},{"ToState","PLANNED"}};
        payload.Remove(missing);
        Assert.Throws<System.Text.Json.JsonException>(()=>System.Text.Json.JsonSerializer.Deserialize<TransitionWorkOrderRequest>(System.Text.Json.JsonSerializer.Serialize(payload)));
    }
    [Fact] public async Task NewerRouteReadWinsAgainstLateOriginalRead()
    {
        using var c=Company();using var s=new WorkOrderDetailState(c);var wait=new TaskCompletionSource<WorkOrderDetailModel?>();
        var original=s.LoadAsync("order",()=>Task.FromResult<string?>("field"),_=>wait.Task);
        var newer=Row();newer.InstanceId="newer";
        await s.LoadAsync("newer",()=>Task.FromResult<string?>("field"),_=>Task.FromResult<WorkOrderDetailModel?>(newer));
        wait.SetResult(Row());await original;Assert.Equal("newer",s.Current!.InstanceId);
    }
}
