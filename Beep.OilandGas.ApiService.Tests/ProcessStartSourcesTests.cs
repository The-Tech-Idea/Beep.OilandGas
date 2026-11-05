using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class ProcessStartSourcesTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    private static ProcessInstanceRequest Request(string type="JOURNAL_ENTRY")=>new(){ProcessId="definition",EntityId=type=="WORK_ORDER"?"order":"journal",EntityType=type,ExpectedFieldId="field",ExpectedDefinitionRevision=new string('A',64)};
    private static JOURNAL_ENTRY Journal()=>new(){JOURNAL_ENTRY_ID="journal",ENTRY_NUMBER="JE-1",STATUS="DRAFT",ROW_CREATED_BY="preparer",TOTAL_DEBIT=12m};
    private static JOURNAL_ENTRY_LINE Line(string id="line")=>new(){JOURNAL_ENTRY_LINE_ID=id,JOURNAL_ENTRY_ID="journal",GL_ACCOUNT_ID="account",CURRENCY_CODE="KWD",DEBIT_AMOUNT=12m};
    private static WorkOrderDetailModel Order()=>new(){InstanceId="order",FieldId="field",Source="WORK_ORDER",InstanceName="WO-1",Instructions="Isolation",Components=[new("order","component","field","well","equipment")]};
    [Fact] public void JournalIdentityMetadataAndWholeRecordedLinesParticipateInReview()
    {
        var request=Request();var journal=Journal();var line=Line();var review=ProcessStartSources.Journal(Company,"actor",request,journal,[line]);
        review.Require(Company,"actor",request);Assert.Equal("JE-1",review.RecordedReference);Assert.Equal("preparer",review.RecordedBy);
        line.CURRENCY_CODE="EUR";Assert.NotEqual(review.Revision,ProcessStartSources.Journal(Company,"actor",request,journal,[line]).Revision);
        line.CURRENCY_CODE="KWD";journal.STATUS="POSTED";Assert.NotEqual(review.Revision,ProcessStartSources.Journal(Company,"actor",request,journal,[line]).Revision);
    }
    [Fact] public void ProviderLineOrderDoesNotChangeContentReview()
    {
        var request=Request();var journal=Journal();var first=Line("A");var second=Line("B");
        Assert.Equal(ProcessStartSources.Journal(Company,"actor",request,journal,[first,second]).Revision,
            ProcessStartSources.Journal(Company,"actor",request,journal,[second,first]).Revision);
    }
    [Theory][InlineData("missing")][InlineData("identity")][InlineData("lines")][InlineData("foreign")][InlineData("duplicate")][InlineData("account")]
    public void MissingOrMalformedJournalCannotBeReviewed(string fault)
    {
        var journal=Journal();var line=Line();IReadOnlyList<JOURNAL_ENTRY_LINE>? lines=[line];
        if(fault=="identity")journal.JOURNAL_ENTRY_ID="another";if(fault=="lines")lines=null;
        if(fault=="foreign")line.JOURNAL_ENTRY_ID="another";if(fault=="duplicate")lines=[line,line];if(fault=="account")line.GL_ACCOUNT_ID="";
        Assert.Throws<InvalidOperationException>(()=>ProcessStartSources.Journal(Company,"actor",Request(),fault=="missing"?null:journal,lines));
    }
    [Fact] public void MaintenanceReviewUsesActualComponentsWithoutInferringLifecycleStatus()
    {
        var request=Request("WORK_ORDER");var order=Order();var review=ProcessStartSources.WorkOrder(Company,"actor",request,order);
        Assert.Equal("WO-1",review.RecordedReference);Assert.Null(review.RecordedStatus);Assert.Null(review.RecordedBy);
        order.Components[0]=order.Components[0] with {EquipmentId="other"};Assert.NotEqual(review.Revision,ProcessStartSources.WorkOrder(Company,"actor",request,order).Revision);
    }
    [Theory][InlineData("missing")][InlineData("identity")][InlineData("field")][InlineData("source")][InlineData("components")][InlineData("duplicate")]
    public void UnconfirmedMaintenanceFieldOrComponentsCannotBeReviewed(string fault)
    {
        var order=Order();if(fault=="identity")order.InstanceId="other";if(fault=="field")order.FieldId="other";if(fault=="source")order.Source="PROJECT";
        if(fault=="components")order.Components.Clear();if(fault=="duplicate")order.Components.Add(order.Components[0]);
        Assert.Throws<InvalidOperationException>(()=>ProcessStartSources.WorkOrder(Company,"actor",Request("WORK_ORDER"),fault=="missing"?null:order));
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("field")][InlineData("source")][InlineData("definition")][InlineData("revision")][InlineData("time")]
    public void ReturnedReviewMustMatchOriginalRequestScope(string fault)
    {
        var request=Request();var review=ProcessStartSources.Journal(Company,"actor",request,Journal(),[]);
        review=fault switch {"company"=>review with{Company=new("tenant","other")},"actor"=>review with{ActorUserId="other"},"field"=>review with{FieldId="other"},"source"=>review with{EntityId="other"},"definition"=>review with{DefinitionRevision=new string('B',64)},"revision"=>review with{Revision="bad"},_=>review with{RetrievedAtUtc=default}};
        Assert.Throws<InvalidOperationException>(()=>review.Require(Company,"actor",request));
    }
}
