using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.ApiService.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class RecordedWorkOrderRowsTests
{
    private static WORK_ORDER Header()=>new(){WORK_ORDER_ID="order",WORK_ORDER_NUMBER="number",WORK_ORDER_TYPE="WO-CORRECTIVE",ACTIVE_IND="Y",REQUEST_DATE=new DateTime(2026,10,6),DUE_DATE=new DateTime(2026,10,8),INSTRUCTIONS="Recorded work"};
    private static WORK_ORDER_COMPONENT Link()=>new(){WORK_ORDER_ID="order",COMPONENT_ID="link",FIELD_ID="field",ACTIVE_IND="Y"};
    [Fact] public void ActualCanonicalHeaderPreservesEvidenceWithoutInventingWorkflow(){var r=Assert.Single(RecordedWorkOrderRows.Project("field",[Link()],[Header()]));Assert.Equal("order",r.InstanceId);Assert.Equal("number",r.InstanceName);Assert.Equal("field",r.FieldId);Assert.Equal("WORK_ORDER",r.Source);Assert.Equal("",r.State);Assert.Equal(Header().REQUEST_DATE,r.RequestDate);Assert.Equal(Header().DUE_DATE,r.DueDate);Assert.Equal("Recorded work",r.Instructions);Assert.NotNull(r.EvidenceUnavailableReason);Assert.Null(r.PlannedStart);Assert.Null(r.ActualStart);}
    [Theory][InlineData("field")][InlineData("linkid")][InlineData("orderid")][InlineData("duplicate")][InlineData("inactive")][InlineData("wrongtype")]
    public void UnconfirmedFieldRelationshipCannotPublishHeader(string fault){var link=Link();if(fault=="field")link.FIELD_ID="foreign";if(fault=="linkid")link.COMPONENT_ID="";if(fault=="orderid")link.WORK_ORDER_ID="";if(fault=="inactive")link.ACTIVE_IND="N";IReadOnlyList<object> links=fault switch{"duplicate"=>[link,link],"wrongtype"=>[new object()],_=>[link]};Assert.Throws<RefusalException>(()=>RecordedWorkOrderRows.Project("field",links,[Header()]));}
    [Theory][InlineData("foreign")][InlineData("missingid")][InlineData("inactive")][InlineData("duplicate")][InlineData("wrongtype")]
    public void UnconfirmedHeaderCannotPublish(string fault){var row=Header();if(fault=="foreign")row.WORK_ORDER_ID="foreign";if(fault=="missingid")row.WORK_ORDER_ID="";if(fault=="inactive")row.ACTIVE_IND="N";IReadOnlyList<object> rows=fault switch{"duplicate"=>[row,row],"wrongtype"=>[new object()],_=>[row]};Assert.Throws<RefusalException>(()=>RecordedWorkOrderRows.Project("field",[Link()],rows));}
    [Fact] public void EmptyActualRelationshipReturnsEmptyRows()=>Assert.Empty(RecordedWorkOrderRows.Project("field",[],[]));
    [Fact] public async Task RecordedHeaderCanPublishListAndDetailButCannotTransition()
    {
        using var c=new CompanyClientContext();Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant","company"),[],[])));
        var summary=RecordedWorkOrderRows.Project("field",[Link()],[Header()])[0];using var list=new WorkOrderListState(c);
        await list.LoadAsync(null,()=>Task.FromResult<string?>("field"),_=>Task.FromResult(new List<WorkOrderSummary>{summary}));Assert.True(list.Confirmed);Assert.Equal("",list.Rows[0].State);
        var row=new WorkOrderDetailModel{InstanceId=summary.InstanceId,FieldId=summary.FieldId,Source=summary.Source,State=summary.State,WoSubType=summary.WoSubType,EvidenceUnavailableReason=summary.EvidenceUnavailableReason};
        using var detail=new WorkOrderDetailState(c);await detail.LoadAsync("order",()=>Task.FromResult<string?>("field"),_=>Task.FromResult<WorkOrderDetailModel?>(row));Assert.NotNull(detail.Current);Assert.Throws<InvalidOperationException>(()=>detail.Begin("PLANNED",null,["PLANNED"]));
        Assert.Same(row,await WorkOrderSourceChecks.RequireAsync("field","order",()=>Task.FromResult(true),()=>Task.FromResult<WorkOrderDetailModel?>(row),requireState:false));
        await Assert.ThrowsAsync<RefusalException>(()=>WorkOrderSourceChecks.RequireAsync("field","order",()=>Task.FromResult(true),()=>Task.FromResult<WorkOrderDetailModel?>(row)));
    }
    [Fact] public void MissingHeaderCannotBecomeEmptySuccess()=>Assert.Throws<RefusalException>(()=>RecordedWorkOrderRows.Project("field",[Link()],[]));
    [Fact] public void MultipleRecordedComponentsStillContributeOneHeader(){var second=Link();second.COMPONENT_ID="second";Assert.Single(RecordedWorkOrderRows.Project("field",[Link(),second],[Header()]));}

    [Fact] public void MultipleRecordedComponentsPreserveDistinctWellAndEquipmentIdentities()
    {
        var a=Link();a.UWI="well-a";a.EQUIPMENT_ID="pump-a";var b=Link();b.COMPONENT_ID="link-b";b.UWI="well-b";b.EQUIPMENT_ID="pump-b";
        var row=Assert.Single(RecordedWorkOrderRows.Project("field",[a,b],[Header()]));
        Assert.Equal(2,row.Components.Count);Assert.Equal("well-a",row.Components[0].WellId);Assert.Equal("pump-a",row.Components[0].EquipmentId);Assert.Equal("well-b",row.Components[1].WellId);Assert.Equal("pump-b",row.Components[1].EquipmentId);
        Assert.Equal("",row.EquipmentId);Beep.OilandGas.Models.Data.WorkOrder.RecordedWorkOrderComponentChecks.RequireWellLink(row,"field","order","well-b");
    }
    [Fact] public void AnEquipmentIdentityCannotEstablishAWellLink()
    {
        var link=Link();link.EQUIPMENT_ID="well";var row=RecordedWorkOrderRows.Project("field",[link],[Header()])[0];
        Assert.Throws<InvalidOperationException>(()=>Beep.OilandGas.Models.Data.WorkOrder.RecordedWorkOrderComponentChecks.RequireWellLink(row,"field","order","well"));
    }
    [Theory][InlineData("field")][InlineData("order")][InlineData("well")][InlineData("source")][InlineData("type")][InlineData("null")][InlineData("duplicate")][InlineData("componentfield")][InlineData("componentorder")][InlineData("componentid")][InlineData("empty")]
    public void UnconfirmedComponentCannotEstablishAnInterventionHandoff(string fault)
    {
        var link=Link();link.UWI="well";var row=RecordedWorkOrderRows.Project("field",[link],[Header()])[0];
        switch(fault){case "field":row.FieldId="other";break;case "order":row.InstanceId="other";break;case "source":row.Source="PROJECT";break;case "type":row.WoSubType="WO-PREVENTIVE";break;case "null":row.Components=null!;break;case "duplicate":row.Components.Add(row.Components[0]);break;case "componentfield":row.Components[0]=row.Components[0] with{FieldId="other"};break;case "componentorder":row.Components[0]=row.Components[0] with{WorkOrderId="other"};break;case "componentid":row.Components[0]=row.Components[0] with{ComponentId=""};break;case "empty":row.Components.Clear();break;default:row.Components[0]=row.Components[0] with{WellId="other"};break;}
        Assert.Throws<InvalidOperationException>(()=>Beep.OilandGas.Models.Data.WorkOrder.RecordedWorkOrderComponentChecks.RequireWellLink(row,"field","order","well"));
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task WrongComponentScopeCannotPublishInListOrDetail(bool listRead)
    {
        using var c=new CompanyClientContext();Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant","company"),[],[])));
        var row=new WorkOrderDetailModel{InstanceId="order",FieldId="field",State="",Source="WORK_ORDER",EvidenceUnavailableReason="No workflow",Components=[new("order","component","other","well","")]};
        if(listRead){using var state=new WorkOrderListState(c);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync(null,()=>Task.FromResult<string?>("field"),_=>Task.FromResult(new List<WorkOrderSummary>{row})));Assert.False(state.Confirmed);Assert.Empty(state.Rows);}
        else{using var state=new WorkOrderDetailState(c);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync("order",()=>Task.FromResult<string?>("field"),_=>Task.FromResult<WorkOrderDetailModel?>(row)));Assert.Null(state.Current);}
    }
}
