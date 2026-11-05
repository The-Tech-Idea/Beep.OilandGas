using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;
public enum PurchaseOrderInventoryLinkDecision { Reject=1, Approve=2 }
public sealed record PurchaseOrderInventoryLinkDecisionReview(PurchaseOrderSourceReview Purchase,CompanyInventorySourceReview Target,string LineId,string BindingRevision,string Revision)
{
    public void Require(CompanyScope company,string actor,string order,string line,string item)
    {
        if(Purchase is null || Target is null || LineId!=line || string.IsNullOrWhiteSpace(BindingRevision) || BindingRevision.Length>512 || BindingRevision.Any(char.IsControl))throw new InvalidOperationException("Original independent inventory-link decision evidence is required.");
        Purchase.Require(company,actor,order);Target.Require(company,actor,item);
        var prepared=Purchase.Lines.SingleOrDefault(row=>row.PO_LINE_ITEM_ID==line) ?? throw new InvalidOperationException("The line is outside the owned order.");
        if(prepared.INVENTORY_LINK_DECISION_JSON is not null)throw new InvalidOperationException("The preparation is already independently resolved.");
        var proof=PurchaseOrderInventoryLinkProposalProof.Require(company,prepared);
        var original=PurchaseOrderSourceReview.Create(company,proof.Command.ActorUserId,Purchase.Header,Purchase.Lines.Select(row=>row.PO_LINE_ITEM_ID==line?proof.Review.Purchase.Lines.Single(source=>source.PO_LINE_ITEM_ID==line):row).ToArray(),Purchase.Receipts);
        if(actor==proof.Command.ActorUserId || actor=="SYSTEM" || proof.Command.TargetItemId!=item || proof.Command.BindingRevision!=BindingRevision ||
            original.Revision!=proof.Review.Purchase.Revision || Target.Revision!=proof.Review.Target.Revision ||
            Purchase.Header.ACTIVE_IND!="Y" || prepared.ACTIVE_IND!="Y" || Target.Item.ACTIVE_IND!="Y" || Revision!=Digest(Purchase,Target,line,BindingRevision))
            throw new InvalidOperationException("An independent reviewer and unchanged original purchase, target and binding are required.");
    }
    public static PurchaseOrderInventoryLinkDecisionReview Create(PurchaseOrderSourceReview purchase,CompanyInventorySourceReview target,string line,string binding)
    {
        purchase=CompanyInventorySourceReview.Copy(purchase);target=CompanyInventorySourceReview.Copy(target);
        var result=new PurchaseOrderInventoryLinkDecisionReview(purchase,target,line,binding,Digest(purchase,target,line,binding));
        result.Require(purchase.Company,purchase.ActorUserId,purchase.Header.PURCHASE_ORDER_ID,line,target.Item.INVENTORY_ITEM_ID);return result;
    }
    private static string Digest(PurchaseOrderSourceReview purchase,CompanyInventorySourceReview target,string line,string binding)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{purchase.Company,purchase.ActorUserId,Purchase=purchase.Revision,Target=target.Revision,Line=line,Binding=binding}));
}
public sealed record PurchaseOrderInventoryLinkDecisionCommand(CompanyScope Company,string ActorUserId,string OrderId,string LineId,string TargetItemId,PurchaseOrderInventoryLinkDecision Decision,string ReviewRevision,string BindingRevision,string IntentId,string Reason)
{
    public void Require(PurchaseOrderInventoryLinkDecisionReview review)
    {
        static bool Text(string? value,int bound)=>!string.IsNullOrWhiteSpace(value) && value.Length<=bound && !value.Any(char.IsControl);
        if(review is null || !Text(ActorUserId,128) || !Text(OrderId,128) || !Text(LineId,128) || !Text(TargetItemId,128) || !Enum.IsDefined(Decision) ||
            !Text(Reason,512) || !Guid.TryParseExact(IntentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=IntentId || ReviewRevision!=review.Revision || BindingRevision!=review.BindingRevision)
            throw new InvalidOperationException("Original reviewer, explicit decision, reason, source and canonical intent are required.");
        review.Require(Company,ActorUserId,OrderId,LineId,TargetItemId);
    }
}
public sealed record PurchaseOrderInventoryLinkDecisionProof(PurchaseOrderInventoryLinkDecisionCommand Command,PurchaseOrderInventoryLinkDecisionReview Review,string RowRevision)
{
    public const int MaximumLength=1048576;
    public static string Digest(PO_LINE_ITEM row){var copy=CompanyInventorySourceReview.Copy(row);copy.INVENTORY_LINK_DECISION_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static string Create(PurchaseOrderInventoryLinkDecisionCommand command,PurchaseOrderInventoryLinkDecisionReview review,PO_LINE_ITEM row)
    {
        command.Require(review);var text=JsonSerializer.Serialize(new PurchaseOrderInventoryLinkDecisionProof(command,review,Digest(row)));
        if(text.Length>MaximumLength)throw new InvalidOperationException("The retained decision exceeds its migrated column bound.");return text;
    }
    public static PurchaseOrderInventoryLinkDecisionProof Require(CompanyScope company,PO_LINE_ITEM row)
    {
        if(string.IsNullOrWhiteSpace(row.INVENTORY_LINK_DECISION_JSON) || row.INVENTORY_LINK_DECISION_JSON.Length>MaximumLength)throw new InvalidOperationException("Recorded independent inventory-link decision is unavailable.");
        PurchaseOrderInventoryLinkDecisionProof? proof;try{proof=JsonSerializer.Deserialize<PurchaseOrderInventoryLinkDecisionProof>(row.INVENTORY_LINK_DECISION_JSON);}catch(JsonException){throw new InvalidOperationException("Retained inventory-link decision is malformed.");}
        if(proof?.Command is null || proof.Review is null || proof.Command.Company!=company)throw new InvalidOperationException("The decision does not belong to this company.");
        proof.Command.Require(proof.Review);var expected=CompanyInventorySourceReview.Copy(proof.Review.Purchase.Lines.Single(line=>line.PO_LINE_ITEM_ID==proof.Command.LineId));
        var preparedAt=expected.ROW_CHANGED_DATE;expected.ROW_CHANGED_BY=proof.Command.ActorUserId;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;
        if(proof.Command.Decision==PurchaseOrderInventoryLinkDecision.Approve)expected.INVENTORY_ITEM_ID=proof.Command.TargetItemId;
        if(row.ROW_CHANGED_DATE is not {} utc || utc.Kind!=DateTimeKind.Utc || utc==default || preparedAt is not {} preparation || utc<preparation || row.ROW_CHANGED_BY!=proof.Command.ActorUserId ||
            proof.RowRevision!=Digest(row) || Digest(expected)!=Digest(row) || Create(proof.Command,proof.Review,row)!=row.INVENTORY_LINK_DECISION_JSON)
            throw new InvalidOperationException("The decision changed unapproved line/proposal data or lacks exact independent reviewer audit.");
        return proof;
    }
}
public sealed record PurchaseOrderInventoryLinkDecisionSubmission(PurchaseOrderInventoryLinkDecisionCommand Command,PurchaseOrderInventoryLinkDecisionReview Review)
{
    public void Require(CompanyScope company,string actor,string order,string line)
    {
        if(Command is null || Review is null || Command.Company!=company || Command.ActorUserId!=actor || Command.OrderId!=order || Command.LineId!=line)
            throw new InvalidOperationException("Retain the original decision company, reviewer, order and line.");
        Command.Require(Review);
    }
    public void RequireResult(PO_LINE_ITEM row)
    {
        var proof=PurchaseOrderInventoryLinkDecisionProof.Require(Command.Company,row);
        if(proof.Command!=Command || JsonSerializer.Serialize(proof.Review)!=JsonSerializer.Serialize(Review))
            throw new InvalidOperationException("The recorded decision does not confirm the original submitted intent and full review.");
    }
}
