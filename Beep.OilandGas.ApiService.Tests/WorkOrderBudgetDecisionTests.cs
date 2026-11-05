#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Moq;
using Xunit;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkOrderBudgetDecisionTests
{
    private sealed class Fixture
    {
        internal readonly PersonaTaskRoutingTests.Fixture Native=new();
        internal readonly AuthorizationWorkflowService Owner;
        internal readonly AFE Draft;
        internal CompanyWorkOrderBudgetDecisionReview Current;
        internal Fixture()
        {
            var requirements=WorkOrderExecutionStartFixture.Original(true).Command.Review.Prerequisites.Requirements;
            var context=requirements.Context with{ActorUserId="budget-maker",Revision="budget-authority"};var book=new CompanyLedgerBookFacts(Guid.NewGuid().ToString("N"),"book","Recorded book","KWD",3,"TO_EVEN","calendar","UTC","book-maker",DateTime.UtcNow,new string('A',64));
            var basis=new CompanyWorkOrderBudgetReview(context,new("production","native-production-binding"),new(requirements.Order.InstanceId,requirements.Order.FieldId,book.BookId),requirements.Order,book,[],DateTime.UtcNow);
            var budget=new CompanyWorkOrderBudgetCommand(basis,Guid.NewGuid(),"ExplicitAFE","Recorded native budget",12.875m,DateTime.UtcNow,"Budget basis");Draft=budget.Prepare(DateTime.UtcNow);
            Current=new(basis with{Context=context with{ActorUserId="independent-reviewer",Revision="independent-authority"},Budgets=[CompanyWorkOrderBudgetCommand.Facts(Draft)],ComparedAtUtc=DateTime.UtcNow},new(basis.Query,Draft.AFE_ID));Current.Require();
            Native.Rows.Clear();Native.Rows.Add(Draft);var creator=new ClassCreator(Native.Editor.Object);Native.Editor.SetupGet(value=>value.classCreator).Returns(creator);
            Native.Source.Setup(value=>value.CheckEntityExist("APPROVAL_WORKFLOW")).Returns(true);Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure shape,bool _)=>creator.ConvertToEntityStructure(shape.EntityName=="APPROVAL_WORKFLOW"?typeof(APPROVAL_WORKFLOW):typeof(AFE)));
            Native.Source.Setup(value=>value.InsertEntity("APPROVAL_WORKFLOW",It.IsAny<object>())).Returns((string _,object row)=>{Native.Writes++;Native.Rows.Add(CompanyInventorySourceReview.Copy((APPROVAL_WORKFLOW)row));return new ErrorsInfo{Flag=Errors.Ok};});
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("APPROVAL_WORKFLOW")).ReturnsAsync(["APPROVAL_WORKFLOW_ID"]);metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("AFE")).ReturnsAsync(["AFE_ID"]);
            metadata.Setup(value=>value.GetTableMetadataAsync("AFE")).ReturnsAsync(new PPDMTableMetadata{TableName="AFE",EntityTypeName=typeof(AFE).FullName!,PrimaryKeyColumn="AFE_ID",Module="PRODUCTION"});
            Owner=new(Native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
        }
        internal Task Guard()=>Task.CompletedTask;
        internal CompanyWorkOrderBudgetDecisionCommand Command(WorkOrderBudgetDecision decision=WorkOrderBudgetDecision.Approve)=>new(CompanyInventorySourceReview.Copy(Current),Guid.NewGuid(),decision,"Independent scope/book/amount review");
        internal Task<CompanyWorkOrderBudgetDecisionRecovery> Record(CompanyWorkOrderBudgetDecisionCommand command)=>Owner.RecordWorkOrderBudgetDecisionAsync(command,Guard,()=>Task.FromResult(Current with{Current=Current.Current with{ComparedAtUtc=DateTime.UtcNow}}));
    }
    [Theory][InlineData(WorkOrderBudgetDecision.Approve)][InlineData(WorkOrderBudgetDecision.Reject)]
    public async Task IndependentDecisionRecordsExistingApprovalWorkflowOnceAndLeavesOriginalAfeUntouched(WorkOrderBudgetDecision decision)
    {
        var fixture=new Fixture();var draft=JsonSerializer.Serialize(fixture.Draft);var command=fixture.Command(decision);var result=await fixture.Record(command);
        result.Require(command.Review.Current.Context.Company,command.Review.Current.Context.ActorUserId,command.Reference());Assert.Equal(1,fixture.Native.Writes);Assert.Equal(new[]{"begin","commit"},fixture.Native.Transactions);
        Assert.Equal(decision==WorkOrderBudgetDecision.Approve?"APPROVED":"REJECTED",result.Recorded.STATUS);Assert.Equal(command.Review.Original().BudgetAmount,result.Recorded.AMOUNT);
        Assert.Equal(draft,JsonSerializer.Serialize(fixture.Draft));Assert.Equal("DRAFT",fixture.Draft.STATUS);Assert.Null(fixture.Draft.APPROVAL_DATE);Assert.Null(fixture.Draft.ACTUAL_COST);
        Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await fixture.Record(command)));Assert.Equal(1,fixture.Native.Writes);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record(command with{IntentId=Guid.NewGuid(),Decision=decision==WorkOrderBudgetDecision.Approve?WorkOrderBudgetDecision.Reject:WorkOrderBudgetDecision.Approve}));Assert.Equal(1,fixture.Native.Writes);
        fixture.Native.Rows.Remove(fixture.Draft);var recovered=await fixture.Owner.RecoverWorkOrderBudgetDecisionAsync(command.Review.Current.Context,command.Review.Current.BudgetDatabase,command.Reference(),fixture.Guard);
        Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(recovered));Assert.Equal(1,fixture.Native.Writes);
    }
    [Theory][InlineData("self")][InlineData("scope")][InlineData("book")][InlineData("amount")][InlineData("schema")][InlineData("borrowed")]
    public async Task IndependentDecisionRefusesUnconfirmedOriginalScopeBookAuditAndAuthorityBeforeInsert(string fault)
    {
        var fixture=new Fixture();var command=fixture.Command();
        if(fault=="self")command=command with{Review=command.Review with{Current=command.Review.Current with{Context=command.Review.Current.Context with{ActorUserId="budget-maker"}}}};
        if(fault=="scope")fixture.Current.Current.Order.Instructions="Changed work scope";
        if(fault=="book")fixture.Current=fixture.Current with{Current=fixture.Current.Current with{Book=fixture.Current.Current.Book with{NativeRevision=new string('B',64)}}};
        if(fault=="amount")fixture.Draft.ESTIMATED_COST=13;
        if(fault=="schema")fixture.Native.Source.Setup(value=>value.CheckEntityExist("APPROVAL_WORKFLOW")).Returns(false);
        if(fault=="borrowed")fixture.Native.Borrowed=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Record(command));Assert.Equal(0,fixture.Native.Writes);Assert.Empty(fixture.Native.Rows.OfType<APPROVAL_WORKFLOW>());
    }
    [Fact] public async Task LostAcknowledgementKeepsOneDecisionAndReadOnlyRecoveryDoesNotReplay()
    {
        var fixture=new Fixture();var command=fixture.Command();fixture.Native.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record(command));Assert.Single(fixture.Native.Rows.OfType<APPROVAL_WORKFLOW>());Assert.Equal(1,fixture.Native.Writes);
        var recovery=await fixture.Owner.RecoverWorkOrderBudgetDecisionAsync(command.Review.Current.Context,command.Review.Current.BudgetDatabase,command.Reference(),fixture.Guard);
        command.RequireRecorded(recovery.Recorded);Assert.Equal(1,fixture.Native.Writes);
    }
    [Fact] public async Task SeparateCoreChangeAfterCommitKeepsOriginalDecisionRecoverableWithoutClaimingAtomicity()
    {
        var fixture=new Fixture();var command=fixture.Command();fixture.Native.AfterCommit=()=>fixture.Current.Current.Order.Instructions="Changed after separate CORE comparison";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record(command));Assert.Single(fixture.Native.Rows.OfType<APPROVAL_WORKFLOW>());Assert.Equal(1,fixture.Native.Writes);
        var recovery=await fixture.Owner.RecoverWorkOrderBudgetDecisionAsync(command.Review.Current.Context,command.Review.Current.BudgetDatabase,command.Reference(),fixture.Guard);
        command.RequireRecorded(recovery.Recorded);Assert.Equal(1,fixture.Native.Writes);
    }
    [Fact] public async Task PrivateRecoveryRejectsOtherReviewerBeforeMalformedPrivateBody()
    {
        var fixture=new Fixture();var command=fixture.Command();await fixture.Record(command);var row=fixture.Native.Rows.OfType<APPROVAL_WORKFLOW>().Single();row.WORK_ORDER_DECISION_JSON="malformed";
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Owner.RecoverWorkOrderBudgetDecisionAsync(command.Review.Current.Context with{ActorUserId="other-reviewer"},command.Review.Current.BudgetDatabase,command.Reference(),fixture.Guard));Assert.Equal(1,fixture.Native.Writes);
    }
    [Fact] public async Task UnownedLegacyAfeApprovalCannotBecomeAnEmptyDecisionPopulation()
    {
        var fixture=new Fixture();var command=fixture.Command();fixture.Native.Rows.Add(new APPROVAL_WORKFLOW{APPROVAL_WORKFLOW_ID="legacy",ENTITY_NAME="AFE",ENTITY_ID=fixture.Draft.AFE_ID,STATUS="APPROVED"});
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Record(command));Assert.Equal(0,fixture.Native.Writes);Assert.Empty(fixture.Native.Transactions);
    }
    [Theory][InlineData(WorkOrderBudgetDecision.Approve)][InlineData(WorkOrderBudgetDecision.Reject)]
    public async Task OtherAuthorizedActorReadsOutcomeFactsWithoutReviewerRationaleAndCurrentScopeDriftBlocksAdmission(WorkOrderBudgetDecision decision)
    {
        var fixture=new Fixture();var command=fixture.Command(decision);await fixture.Record(command);
        var context=command.Review.Current.Context with{ActorUserId="budget-maker"};var current=command.Review.Current with{Context=context,ComparedAtUtc=DateTime.UtcNow};
        var fact=await fixture.Owner.ReadWorkOrderBudgetDecisionFactsAsync(context,current.BudgetDatabase,command.Review.Query,command.Review.Original(),fixture.Guard);Assert.NotNull(fact);
        var status=new CompanyWorkOrderBudgetDecisionStatus(current,command.Review.Query,fact,CompanyWorkOrderBudgetDecisionStatus.GetBlockers(current,command.Review.Query,fact),DateTime.UtcNow);status.Require();
        Assert.Equal(decision==WorkOrderBudgetDecision.Approve,status.IsApprovedForCurrentScope);Assert.DoesNotContain(command.Rationale,JsonSerializer.Serialize(status));Assert.DoesNotContain("WORK_ORDER_DECISION_JSON",JsonSerializer.Serialize(status));Assert.DoesNotContain(command.IntentId.ToString(),JsonSerializer.Serialize(status));
        var changed=CompanyInventorySourceReview.Copy(current.Order);changed.Instructions="Changed after recorded decision";
        var changedCurrent=current with{Order=changed,ComparedAtUtc=DateTime.UtcNow};var changedStatus=new CompanyWorkOrderBudgetDecisionStatus(changedCurrent,status.Query,fact,CompanyWorkOrderBudgetDecisionStatus.GetBlockers(changedCurrent,status.Query,fact),DateTime.UtcNow);
        changedStatus.Require();Assert.False(changedStatus.IsApprovedForCurrentScope);Assert.Contains("Original budget work scope changed.",changedStatus.Blockers);Assert.Equal(fact,changedStatus.Recorded);Assert.Equal(1,fixture.Native.Writes);
    }
    [Fact] public async Task OnlyCompleteKnownEmptyDecisionPopulationReturnsUnrecordedAndMissingSchemaIsUnavailable()
    {
        var fixture=new Fixture();var review=fixture.Current;
        var fact=await fixture.Owner.ReadWorkOrderBudgetDecisionFactsAsync(review.Current.Context,review.Current.BudgetDatabase,review.Query,review.Original(),fixture.Guard);Assert.Null(fact);
        var status=new CompanyWorkOrderBudgetDecisionStatus(review.Current,review.Query,fact,CompanyWorkOrderBudgetDecisionStatus.GetBlockers(review.Current,review.Query,fact),DateTime.UtcNow);Assert.False(status.IsApprovedForCurrentScope);Assert.Single(status.Blockers);
        fixture.Native.Source.Setup(value=>value.CheckEntityExist("APPROVAL_WORKFLOW")).Returns(false);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Owner.ReadWorkOrderBudgetDecisionFactsAsync(review.Current.Context,review.Current.BudgetDatabase,review.Query,review.Original(),fixture.Guard));Assert.Equal(0,fixture.Native.Writes);
    }
}
#endif
