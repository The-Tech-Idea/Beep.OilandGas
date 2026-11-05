using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Core.Refusals;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class RecordedWorkOrderCreationTests
{
    private static CreateWorkOrderRequest Request()=>new(){WorkOrderNumber="WO-001",InstanceName="Repair pump",FieldId="field",WoSubType=WorkOrderSubType.Corrective,Jurisdiction="Recorded authority",Description="Actual instructions"};
    [Fact] public void CanonicalRowsPreserveExplicitIntentWithoutInventingStatusOrCountry()
    {
        var now=DateTime.UtcNow;var (header,component)=RecordedWorkOrderCreation.Prepare(Request(),"actor","id",now);
        Assert.Equal("id",header.WORK_ORDER_ID);Assert.Equal("WO-001",header.WORK_ORDER_NUMBER);Assert.Equal("WO-CORRECTIVE",header.WORK_ORDER_TYPE);Assert.Contains("Repair pump",header.INSTRUCTIONS);Assert.Contains("Recorded authority",header.INSTRUCTIONS);Assert.Contains("Actual instructions",header.INSTRUCTIONS);
        Assert.Equal(now,header.REQUEST_DATE);Assert.Null(header.DUE_DATE);Assert.Null(header.COMPLETE_DATE);Assert.Equal("actor",header.ROW_CREATED_BY);Assert.Equal("Y",header.ACTIVE_IND);
        Assert.Equal("id",component.WORK_ORDER_ID);Assert.Equal("field",component.FIELD_ID);Assert.NotEmpty(component.COMPONENT_ID);Assert.Null(component.JURISDICTION);Assert.Null(component.LAND_SALE_NUMBER);Assert.Null(component.EQUIPMENT_ID);
        var summary=RecordedWorkOrderRows.Project("field",[component],[header]).Single();Assert.Equal("",summary.State);RecordedWorkOrderCreation.RequireReadback(Request(),"id",summary);
    }
    [Theory][InlineData("number")][InlineData("title")][InlineData("field")][InlineData("type")][InlineData("jurisdiction")][InlineData("actor")][InlineData("id")][InlineData("time")][InlineData("zone")][InlineData("scheduled")][InlineData("duration")]
    public void IncompleteOrUnsupportedIntentCannotPrepareRows(string fault)
    {
        var r=Request();var actor="actor";var id="id";var time=DateTime.UtcNow;if(fault=="number")r.WorkOrderNumber="";if(fault=="title")r.InstanceName="";if(fault=="field")r.FieldId="";if(fault=="type")r.WoSubType="";if(fault=="jurisdiction")r.Jurisdiction="";if(fault=="actor")actor="";if(fault=="id")id="";if(fault=="time")time=default;if(fault=="zone")time=DateTime.SpecifyKind(time,DateTimeKind.Unspecified);if(fault=="scheduled")r.ProposedStart=time;if(fault=="duration")r.Duration=TimeSpan.FromHours(1);Assert.Throws<RefusalException>(()=>RecordedWorkOrderCreation.Prepare(r,actor,id,time));
    }
    [Theory][InlineData("number")][InlineData("field")][InlineData("identity")][InlineData("type")][InlineData("instructions")][InlineData("state")][InlineData("source")][InlineData("null")]
    public void WrongSavedResultCannotConfirmCreation(string fault)
    {
        var r=Request();var prepared=RecordedWorkOrderCreation.Prepare(r,"actor","id",DateTime.UtcNow);var row=RecordedWorkOrderRows.Project("field",[prepared.Component],[prepared.Header])[0];if(fault=="number")row.InstanceName="other";if(fault=="field")row.FieldId="other";if(fault=="identity")row.InstanceId="other";if(fault=="type")row.WoSubType="other";if(fault=="instructions")row.Instructions="other";if(fault=="state")row.State="DRAFT";if(fault=="source")row.Source="PROJECT";Assert.Throws<RefusalException>(()=>RecordedWorkOrderCreation.RequireReadback(r,"id",fault=="null"?null:row));
    }

    [Fact] public void WellAndEquipmentAreSavedInTheirSeparateCanonicalColumns()
    {
        var request=Request();request.WellId="well";request.EquipmentId="pump";
        var prepared=RecordedWorkOrderCreation.Prepare(request,"actor","order",DateTime.UtcNow);
        Assert.Equal("well",prepared.Component.UWI);Assert.Equal("pump",prepared.Component.EQUIPMENT_ID);
        Assert.Null(prepared.Component.WELL_ACTIVITY_SET_ID);Assert.Null(prepared.Component.WELL_ACTIVITY_SOURCE);Assert.Null(prepared.Component.WELL_ACTIVITY_TYPE_ID);
        var row=RecordedWorkOrderRows.Project("field",[prepared.Component],[prepared.Header])[0];RecordedWorkOrderCreation.RequireReadback(request,"order",row);
    }
    [Theory][InlineData("well")][InlineData("equipment")][InlineData("missing")][InlineData("duplicate")]
    public void SavedReferenceMismatchCannotConfirmCreation(string fault)
    {
        var request=Request();request.WellId="well";request.EquipmentId="pump";var prepared=RecordedWorkOrderCreation.Prepare(request,"actor","order",DateTime.UtcNow);
        var row=RecordedWorkOrderRows.Project("field",[prepared.Component],[prepared.Header])[0];
        if(fault=="well")row.Components[0]=row.Components[0] with{WellId="other"};if(fault=="equipment")row.Components[0]=row.Components[0] with{EquipmentId="other"};if(fault=="missing")row.Components.Clear();if(fault=="duplicate")row.Components.Add(row.Components[0]);
        Assert.Throws<InvalidOperationException>(()=>RecordedWorkOrderCreation.RequireReadback(request,"order",row));
    }

    [Fact] public void ExactActiveWellReferenceIsAdmitted()=>RecordedWorkOrderCreation.RequireWellReference("well",[new Beep.OilandGas.PPDM39.Models.WELL{UWI="well",ACTIVE_IND="Y"}]);
    [Theory][InlineData("missing")][InlineData("duplicate")][InlineData("inactive")][InlineData("different")][InlineData("wrongtype")][InlineData("null")]
    public void UnconfirmedNativeWellCannotBeAttached(string fault)
    {
        var well=new Beep.OilandGas.PPDM39.Models.WELL{UWI=fault=="different"?"other":"well",ACTIVE_IND=fault=="inactive"?"N":"Y"};
        IReadOnlyList<object>? rows=fault switch{"missing"=>[],"duplicate"=>[well,well],"wrongtype"=>[new object()],"null"=>null,_=>[well]};
        Assert.Throws<RefusalException>(()=>RecordedWorkOrderCreation.RequireWellReference("well",rows));
    }
}
