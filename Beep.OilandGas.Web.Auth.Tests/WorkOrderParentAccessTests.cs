using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderParentAccessTests
{
    [Fact] public void MultipleFieldsArePreservedWithoutPickingOneParent(){var fields=WorkOrderParentAccess.Fields("order",[new("order","a","field-a","well","equipment"),new("order","b","field-b","", ""),new("order","c","field-a","", "")]);Assert.Equal(new[]{"field-a","field-b"},fields);}
    [Theory][InlineData("order")][InlineData("component")][InlineData("field")][InlineData("duplicate")][InlineData("empty")][InlineData("null")]
    public void UnconfirmedComponentsCannotEstablishScope(string fault){var row=new WorkOrderComponentReference(fault=="order"?"other":"order",fault=="component"?"":"component",fault=="field"?"":"field","","");IReadOnlyList<WorkOrderComponentReference>? rows=fault switch{"duplicate"=>[row,row],"empty"=>[],"null"=>null,_=>[row]};Assert.Throws<InvalidOperationException>(()=>WorkOrderParentAccess.Fields("order",rows!));}
    [Theory][InlineData("WRITE","WRITE","READ","READ")][InlineData("DELETE","WRITE","WRITE","WRITE")][InlineData("READ","DELETE","WRITE","READ")][InlineData("WRITE","WRITE",null,null)][InlineData("ADMIN","WRITE","WRITE",null)][InlineData("WRITE","READ","READ","READ")]
    public void EveryCurrentParentAndDirectGrantCapsEffectiveAccess(string grant,string first,string? second,string? expected)=>Assert.Equal(expected,WorkOrderParentAccess.Intersect(grant,[first,second]));
    [Fact] public void MissingParentPopulationDoesNotGrantAccess()=>Assert.Null(WorkOrderParentAccess.Intersect("DELETE",[]));
    [Fact] public void LaterEntitlementRevocationCannotBeDiscarded(){Assert.Equal("WRITE",WorkOrderParentAccess.Intersect("WRITE",["WRITE","WRITE"]));Assert.Null(WorkOrderParentAccess.Intersect("WRITE",["WRITE","WRITE","WRITE",null]));}
}
