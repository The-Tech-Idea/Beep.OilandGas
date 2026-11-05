using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderCreateStateTests
{
    private static CompanyClientContext Company(){var c=new CompanyClientContext();Select(c,"A");return c;}
    private static void Select(CompanyClientContext c,string id)=>Assert.True(c.TrySelect(c.Clear(),"actor",new(new("tenant",id),[],[])));
    private static CreateWorkOrderRequest Draft()=>new(){FieldId="field",WorkOrderNumber="WO-001",InstanceName="Repair pump",WoSubType=WorkOrderSubType.Corrective,Jurisdiction="Kuwait authority"};
    private static WorkOrderSummary Recorded()=>new(){InstanceId="order/1",FieldId="field",Components=[new("order/1","component","field","","")],InstanceName="WO-001",WoSubType=WorkOrderSubType.Corrective,State=string.Empty,Source="WORK_ORDER",EvidenceUnavailableReason="No recorded workflow state",Instructions="Title: Repair pump\nReported jurisdiction: Kuwait authority\n"};
    [Fact] public void FrozenRequestCannotAdoptChangedFormValues()
    {
        using var c=Company();var s=new WorkOrderCreateState(c);var draft=Draft();var attempt=s.Begin(draft,"field");draft.WorkOrderNumber="other";draft.FieldId="other";draft.InstanceName="Other";draft.Jurisdiction="Other authority";
        Assert.Equal("WO-001",attempt.Request.WorkOrderNumber);Assert.Equal("field",attempt.Request.FieldId);Assert.Equal("Repair pump",attempt.Request.InstanceName);Assert.Equal("Kuwait authority",attempt.Request.Jurisdiction);Assert.True(s.IsBlocked("field"));
    }
    [Fact] public void UnknownOutcomeBlocksRepeatAcrossCompanyAwayAndBack()
    {
        using var c=Company();var s=new WorkOrderCreateState(c);var attempt=s.Begin(Draft(),"field");Assert.Throws<InvalidOperationException>(()=>s.Begin(Draft(),"field"));Select(c,"B");Assert.False(s.IsCurrent(attempt,"field"));Assert.False(s.IsBlocked("field"));Select(c,"A");Assert.True(s.IsBlocked("field"));Assert.Throws<InvalidOperationException>(()=>s.Begin(Draft(),"field"));
    }
    [Fact] public void FieldChangeInvalidatesFollowUpButRetainsUnknownOriginalOutcome()
    {
        using var c=Company();var s=new WorkOrderCreateState(c);var attempt=s.Begin(Draft(),"field");Assert.False(s.IsCurrent(attempt,"other"));Assert.True(s.IsBlocked("field"));Assert.False(s.IsBlocked("other"));
    }
    [Theory][InlineData("null")][InlineData("identity")][InlineData("field")][InlineData("subtype")][InlineData("title")][InlineData("state")][InlineData("instructions")]
    public void MismatchedResponseCannotConfirmOutcome(string fault)
    {
        using var c=Company();var s=new WorkOrderCreateState(c);var attempt=s.Begin(Draft(),"field");var row=Recorded();switch(fault){case "identity":row.InstanceId="";break;case "field":row.FieldId="other";break;case "subtype":row.WoSubType="other";break;case "title":row.InstanceName="other";break;case "instructions":row.Instructions="Other instructions";break;case "state":row.State=WorkOrderState.Completed;break;default:row=null!;break;}
        Assert.Throws<InvalidOperationException>(()=>s.Confirm(attempt,row));Assert.True(s.IsBlocked("field"));
    }
    [Fact] public void ExactRecordedResponseEnablesAnotherNewOrder()
    {
        using var c=Company();var s=new WorkOrderCreateState(c);var attempt=s.Begin(Draft(),"field");s.Confirm(attempt,Recorded());Assert.False(s.IsBlocked("field"));Assert.True(s.IsCurrent(attempt,"field"));s.Begin(Draft(),"field");
    }
    [Theory][InlineData("field")][InlineData("title")][InlineData("jurisdiction")][InlineData("company")][InlineData("number")]
    public void MissingIntentNeverStartsAttempt(string fault)
    {
        using var c=Company();var s=new WorkOrderCreateState(c);var draft=Draft();switch(fault){case "number":draft.WorkOrderNumber="";break;case "field":draft.FieldId="other";break;case "title":draft.InstanceName=" ";break;case "jurisdiction":draft.Jurisdiction="";break;default:c.Clear();break;}
        Assert.Throws<InvalidOperationException>(()=>s.Begin(draft,"field"));Assert.False(s.IsBlocked("field"));
    }
    [Fact] public void NewDraftDoesNotInventCountry()=>Assert.Equal(string.Empty,new CreateWorkOrderRequest().Jurisdiction);

    [Fact] public void SubmittedWellAndEquipmentStayFrozenUntilExactSavedReferencesReturn()
    {
        using var company=Company();var state=new WorkOrderCreateState(company);var request=Draft();request.WellId="well";request.EquipmentId="pump";var attempt=state.Begin(request,"field");request.WellId="other";request.EquipmentId="other";
        Assert.Equal("well",attempt.Request.WellId);Assert.Equal("pump",attempt.Request.EquipmentId);
        var row=Recorded();Assert.Throws<InvalidOperationException>(()=>state.Confirm(attempt,row));Assert.True(state.IsBlocked("field"));
        row.Components[0]=row.Components[0] with{WellId="well",EquipmentId="pump"};state.Confirm(attempt,row);Assert.False(state.IsBlocked("field"));
    }
}
