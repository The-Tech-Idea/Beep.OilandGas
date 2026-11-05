using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class ProcessTransitionReviewTests
{
    private static Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel Order()=>new(){InstanceId="order",FieldId="field",Source="WORK_ORDER",InstanceName="WO-42",Instructions="Recorded isolation",Components=[new("order","component","field","well","equipment")]};
    private static ProcessDefinition MaintenanceDefinition(){var definition=Definition();definition.EntityType="WORK_ORDER";definition.ProcessType="WORK_ORDER";return definition;}
    [Theory][InlineData("type")][InlineData("entity")]
    public void EquipmentOrUnconfirmedDefinitionCannotAuthorizeMaintenanceActions(string fault)
    {
        var definition=MaintenanceDefinition();if(fault=="type")definition.ProcessType="EQUIPMENT";else definition.EntityType="EQUIPMENT";
        Assert.Throws<InvalidOperationException>(()=>ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),Maintenance(),definition,[],workOrder:Order(),inspections:[]));
    }
    private static ProcessInstance Maintenance(){var source=Source();source.EntityType="WORK_ORDER";source.EntityId="order";return source;}
    [Fact] public void MaintenanceReviewUsesActualHeaderAndEmptyInspectionEvidence()
    {
        var review=ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),Maintenance(),MaintenanceDefinition(),[],workOrder:Order(),inspections:[]);
        Assert.Single(review.Choices);Assert.Equal("WO-42",review.WorkOrder!.RecordedNumber);Assert.Empty(review.WorkOrder.Inspections);
        Assert.Equal("well",Assert.Single(review.WorkOrder.Components).WellId);
    }
    [Fact] public void MissingMaintenanceHeaderCannotOfferActions()=>Assert.Empty(ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),Maintenance(),MaintenanceDefinition(),[]).Choices);
    [Theory][InlineData("header")][InlineData("field")][InlineData("source")][InlineData("components")][InlineData("inspections")]
    public void UnconfirmedMaintenanceEvidenceCannotEnterActionReview(string fault)
    {
        var order=Order();if(fault=="header")order.InstanceId="foreign";if(fault=="field")order.FieldId="foreign";if(fault=="source")order.Source="PROJECT";if(fault=="components")order.Components=[];
        Assert.Throws<InvalidOperationException>(()=>ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),Maintenance(),MaintenanceDefinition(),[],workOrder:order,inspections:fault=="inspections"?null:[]));
    }
    [Theory][InlineData("instructions")][InlineData("due")][InlineData("component")][InlineData("requirement")][InlineData("result")]
    public void MaintenanceSourceChangesInvalidateReviewedActions(string fault)
    {
        var order=Order();var row=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareRequirement(new(){FieldId="field",CondSeq=1,CondType=Beep.OilandGas.Models.Data.WorkOrder.ConditionType.CloseOut,RequiredText="Recorded closeout",Jurisdiction="Authority"},"order","planner","inspection",DateTime.UtcNow);
        var inspections=new List<Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition>{Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.Project(row)};
        var original=ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),Maintenance(),MaintenanceDefinition(),[],workOrder:order,inspections:inspections);
        switch(fault)
        {
            case "instructions":order.Instructions="Changed isolation";break;
            case "due":order.DueDate=DateTime.UtcNow;break;
            case "component":order.Components[0]=order.Components[0] with{EquipmentId="other"};break;
            case "requirement":row.REQUIRED_TEXT="Changed closeout";inspections[0]=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.Project(row);break;
            default:row=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareResult(row,new(){FieldId="field",InspectionId="inspection",ExpectedSourceRevision=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.Revision(row),Result="FAIL",Notes="Recorded failure"},"technician",DateTime.UtcNow);inspections[0]=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.Project(row);break;
        }
        Assert.NotEqual(original.Revision,ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),Maintenance(),MaintenanceDefinition(),[],workOrder:order,inspections:inspections).Revision);
    }
    private static ClaimsPrincipal User(string role="Reviewer")=>new(new ClaimsIdentity([new(ClaimTypes.Role,role)],"test"));
    private static ProcessInstance Source()=>new(){InstanceId="p",ProcessId="definition",FieldId="field",EntityId="journal",EntityType="JOURNAL_ENTRY",CurrentState="draft",CurrentStepId="review",StartedBy="maker",StepInstances=[new(){StepId="review",RequiredRole="Reviewer"}]};
    private static JOURNAL_ENTRY Journal()=>new(){JOURNAL_ENTRY_ID="journal",ROW_CREATED_BY="journal-maker",STATUS="DRAFT"};
    private static ProcessDefinition Definition()=>new(){ProcessId="definition",Steps=[new(){StepId="review",RequiredRoles=["Reviewer"]}],Transitions=new(){["t"]=new(){TransitionId="t",FromStateId="draft",ToStateId="approved",Trigger="approve",RequiresApproval=true,RequiredRoles=["Reviewer"]}}};
    [Fact] public void EligibleIndependentActorSeesExactChoice(){var review=ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),Source(),Definition(),[],Journal(),[]);Assert.Equal("approved",Assert.Single(review.Choices).ToStateId);Assert.Equal(64,review.Revision.Length);}
    [Theory][InlineData("company")][InlineData("actor")][InlineData("state")][InlineData("definition")][InlineData("history")][InlineData("roles")]
    public void ChangedReviewedEvidenceChangesRevision(string change)
    {
        var source=Source();var def=Definition();var user=User();var company=new CompanyScope("tenant","A");var actor="reviewer";var history=new List<ProcessHistoryEntry>();
        var original=ProcessTransitionReviews.Create(company,actor,user,source,def,history,Journal(),[]);
        switch(change){case "company":company=new("tenant","B");break;case "actor":actor="other";break;case "state":source.CurrentState="changed";break;case "definition":def.Transitions["t"].Trigger="changed";break;case "history":history.Add(new(){HistoryId="h",InstanceId="p",Action="CHANGE"});break;default:user=User("Reader");break;}
        Assert.NotEqual(original.Revision,ProcessTransitionReviews.Create(company,actor,user,source,def,history,Journal(),[]).Revision);
    }
    [Fact] public void CreatorCannotSeeOwnApproval(){Assert.Empty(ProcessTransitionReviews.Create(new("tenant","A"),"maker",User("Administrator"),Source(),Definition(),[],Journal(),[]).Choices);}
    [Theory][InlineData("Reviewer")][InlineData("Administrator")]
    public void JournalPreparerCannotApproveProcessCreatedBySomeoneElse(string role)
    {
        Assert.Empty(ProcessTransitionReviews.Create(new("tenant","A"),"journal-maker",User(role),Source(),Definition(),[],Journal(),[]).Choices);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public void MissingJournalOrUnknownPreparerCannotOfferApproval(bool unknownPreparer)
    {
        var journal=unknownPreparer?Journal():null;if(journal is not null)journal.ROW_CREATED_BY="";
        Assert.Empty(ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User("Administrator"),Source(),Definition(),[],journal,journal is null ? null : []).Choices);
    }
    [Fact] public void WrongJournalIdentityCannotEnterActionReview()
    {
        var journal=Journal();journal.JOURNAL_ENTRY_ID="other";
        Assert.Throws<InvalidOperationException>(()=>ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),Source(),Definition(),[],journal,journal is null ? null : []));
    }
    [Fact] public void ChangedJournalPreparerOrStatusChangesOriginalRevision()
    {
        var journal=Journal();var source=Source();var definition=Definition();var user=User();
        var original=ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",user,source,definition,[],journal,journal is null ? null : []);
        journal.ROW_CREATED_BY="other-maker";
        var changed=ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",user,source,definition,[],journal,journal is null ? null : []);Assert.NotEqual(original.Revision,changed.Revision);
        journal.STATUS="POSTED";Assert.NotEqual(changed.Revision,ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",user,source,definition,[],journal,journal is null ? null : []).Revision);
    }
    [Fact] public void JournalProjectionPreservesRecordedHeaderWithoutInventingMissingAmounts()
    {
        var journal=Journal();journal.ENTRY_NUMBER="JE-42";journal.SOURCE="local-book";journal.REMARK="BOOK=local-book";
        journal.TOTAL_DEBIT=null;journal.TOTAL_CREDIT=0m;journal.REFERENCE_NUMBER="invoice/42";
        var before=DateTimeOffset.UtcNow;
        var review=ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),Source(),Definition(),[],journal,journal is null ? null : []);
        var row=Assert.IsType<JournalSourceReview>(review.Journal);
        Assert.Equal("journal",row.JournalEntryId);Assert.Equal("JE-42",row.EntryNumber);Assert.Equal("local-book",row.BookId);
        Assert.Equal("journal-maker",row.PreparerUserId);Assert.Equal("DRAFT",row.Status);Assert.Equal("invoice/42",row.ReferenceNumber);
        Assert.Null(row.TotalDebit);Assert.Equal(0m,row.TotalCredit);Assert.InRange(row.RetrievedAtUtc,before,DateTimeOffset.UtcNow);Assert.Equal(TimeSpan.Zero,row.RetrievedAtUtc.Offset);
    }
    [Theory][InlineData(null,null)][InlineData("", "BOOK=")][InlineData("IFRS","BOOK=GAAP")]
    public void UnconfirmedBookMetadataRemainsUnavailable(string? source,string? remark)
    {
        var journal=Journal();journal.SOURCE=source!;journal.REMARK=remark!;
        Assert.Null(ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),Source(),Definition(),[],journal,journal is null ? null : []).Journal!.BookId);
    }
    [Fact] public void NonJournalWorkflowHasNoJournalProjection()
    {
        var source=Source();source.EntityType="INVOICE";
        Assert.Null(ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),source,Definition(),[]).Journal);
    }
    private static JOURNAL_ENTRY_LINE Line(string id="line")=>new(){JOURNAL_ENTRY_LINE_ID=id,JOURNAL_ENTRY_ID="journal",GL_ACCOUNT_ID="account",LINE_NUMBER=1,DEBIT_AMOUNT=10m,CREDIT_AMOUNT=null,CURRENCY_CODE="KWD",EXCHANGE_RATE=null,COST_CENTER_ID="cost"};
    [Theory][InlineData("missing")][InlineData("identity")][InlineData("foreign")][InlineData("account")][InlineData("duplicate")]
    public void UnconfirmedLinesCannotEnterJournalReview(string fault)
    {
        var line=Line();IReadOnlyList<JOURNAL_ENTRY_LINE>? lines=[line];
        switch(fault){case "missing":lines=null;break;case "identity":line.JOURNAL_ENTRY_LINE_ID="";break;case "foreign":line.JOURNAL_ENTRY_ID="other";break;case "account":line.GL_ACCOUNT_ID="";break;default:lines=[line,line];break;}
        Assert.Throws<InvalidOperationException>(()=>ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),Source(),Definition(),[],Journal(),lines));
    }
    [Fact] public void RecordedLineProjectionPreservesCurrencyAndMissingCreditOrExchangeRate()
    {
        var review=ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",User(),Source(),Definition(),[],Journal(),[Line()]);
        var row=Assert.Single(review.Journal!.Lines!);Assert.Equal("line",row.LineId);Assert.Equal("journal",row.JournalEntryId);
        Assert.Equal("account",row.AccountId);Assert.Equal("KWD",row.CurrencyCode);Assert.Equal("cost",row.CostCenterId);
        Assert.Equal(10m,row.Debit);Assert.Null(row.Credit);Assert.Null(row.ExchangeRate);
    }
    [Fact] public void ChangedRecordedLineInvalidatesOriginalActionRevision()
    {
        var line=Line();var source=Source();var definition=Definition();var journal=Journal();var user=User();
        var original=ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",user,source,definition,[],journal,[line]);
        line.DEBIT_AMOUNT=12m;var amountChanged=ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",user,source,definition,[],journal,[line]);
        Assert.NotEqual(original.Revision,amountChanged.Revision);line.CURRENCY_CODE="USD";
        Assert.NotEqual(amountChanged.Revision,ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",user,source,definition,[],journal,[line]).Revision);
    }
    [Fact] public void ProviderLineOrderDoesNotChangeRevisionAndDisplayOrderIsStable()
    {
        var first=Line("A");var second=Line("B");first.LINE_NUMBER=2;second.LINE_NUMBER=1;
        var source=Source();var definition=Definition();var journal=Journal();var user=User();
        var initial=ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",user,source,definition,[],journal,[first,second]);
        var reordered=ProcessTransitionReviews.Create(new("tenant","A"),"reviewer",user,source,definition,[],journal,[second,first]);
        Assert.Equal(initial.Revision,reordered.Revision);Assert.Equal(new[]{"B","A"},reordered.Journal!.Lines!.Select(row=>row.LineId));
    }
}
