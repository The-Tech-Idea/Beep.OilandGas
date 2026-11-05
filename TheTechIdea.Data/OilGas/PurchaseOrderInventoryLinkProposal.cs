using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

public sealed record PurchaseOrderInventoryLinkProposalReview(PurchaseOrderSourceReview Purchase,CompanyInventorySourceReview Target,string LineId,string BindingRevision,string Revision)
{
    public void Require(CompanyScope company,string actor,string order,string line,string item)
    {
        if(Purchase is null || Target is null || Target.Item is null || LineId!=line || string.IsNullOrWhiteSpace(BindingRevision) || BindingRevision.Length>512 || BindingRevision.Any(char.IsControl))throw new InvalidOperationException("Original inventory link preparation evidence is required.");
        Purchase.Require(company,actor,order);Target.Require(company,actor,item);
        var source=Purchase.Lines.SingleOrDefault(row=>row.PO_LINE_ITEM_ID==line) ?? throw new InvalidOperationException("The line is outside this owned order.");
        if(Purchase.Header.ACTIVE_IND!="Y" || source.ACTIVE_IND!="Y" || source.INVENTORY_LINK_REVIEW_JSON is not null || source.INVENTORY_ITEM_ID==item || Target.Item.ACTIVE_IND!="Y" ||
            string.IsNullOrWhiteSpace(Target.Item.WAREHOUSE_ID) || string.IsNullOrWhiteSpace(Target.Item.LOCATION_ID) || string.IsNullOrWhiteSpace(source.QUANTITY_UOM) || source.QUANTITY_UOM!=Target.Item.UNIT_OF_MEASURE ||
            Revision!=Digest(Purchase,Target,line,BindingRevision))throw new InvalidOperationException("The active line, target item, unit, location or unprepared source revision does not agree.");
    }
    public static PurchaseOrderInventoryLinkProposalReview Create(PurchaseOrderSourceReview purchase,CompanyInventorySourceReview target,string line,string binding)
    {
        ArgumentNullException.ThrowIfNull(purchase);ArgumentNullException.ThrowIfNull(target);
        purchase=CompanyInventorySourceReview.Copy(purchase);target=CompanyInventorySourceReview.Copy(target);
        var result=new PurchaseOrderInventoryLinkProposalReview(purchase,target,line,binding,Digest(purchase,target,line,binding));
        result.Require(purchase.Company,purchase.ActorUserId,purchase.Header.PURCHASE_ORDER_ID,line,target.Item.INVENTORY_ITEM_ID);return result;
    }
    private static string Digest(PurchaseOrderSourceReview purchase,CompanyInventorySourceReview target,string line,string binding)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{purchase.Company,purchase.ActorUserId,Purchase=purchase.Revision,Target=target.Revision,Line=line,Binding=binding}));
}
public sealed record PurchaseOrderInventoryLinkProposalCommand(CompanyScope Company,string ActorUserId,string OrderId,string LineId,string TargetItemId,string ReviewRevision,string BindingRevision,string IntentId,string Reason)
{
    public void Require(PurchaseOrderInventoryLinkProposalReview review)
    {
        static bool Text(string? value,int length)=>!string.IsNullOrWhiteSpace(value) && value.Length<=length && !value.Any(char.IsControl);
        if(review is null || !Text(ActorUserId,128) || ActorUserId=="SYSTEM" || !Text(OrderId,128) || !Text(LineId,128) || !Text(TargetItemId,128) ||
            string.IsNullOrWhiteSpace(Reason) || Reason.Length>512 || Reason.Any(char.IsControl) ||
            !Guid.TryParseExact(IntentId,"N",out var id) || id==Guid.Empty || id.ToString("N")!=IntentId || ReviewRevision!=review.Revision || BindingRevision!=review.BindingRevision)
            throw new InvalidOperationException("The original preparer, reviewed source, reason and canonical intent are required.");
        review.Require(Company,ActorUserId,OrderId,LineId,TargetItemId);
    }
}
public sealed record PurchaseOrderInventoryLinkProposalSubmission(PurchaseOrderInventoryLinkProposalCommand Command,PurchaseOrderInventoryLinkProposalReview Review)
{
    public void Require(CompanyScope company,string actor,string order,string line)
    {
        if(Command is null || Review is null || Command.Company!=company || Command.ActorUserId!=actor || Command.OrderId!=order || Command.LineId!=line)
            throw new InvalidOperationException("The preparation must retain the original company, account, order and line.");
        Command.Require(Review);
    }
    public void RequireResult(PO_LINE_ITEM row)
    {
        if(row.INVENTORY_LINK_DECISION_JSON is not null)throw new InvalidOperationException("Preparation has an independent decision. Review its recorded resolution.");
        var proof=PurchaseOrderInventoryLinkProposalProof.Require(Command.Company,row);
        if(proof.Command!=Command || JsonSerializer.Serialize(proof.Review)!=JsonSerializer.Serialize(Review))
            throw new InvalidOperationException("The recorded preparation does not confirm the original submitted intent and review.");
    }
}
/// <summary>One retained preparation, not independent approval or an applied inventory link.</summary>
public sealed record PurchaseOrderInventoryLinkProposalProof(PurchaseOrderInventoryLinkProposalCommand Command,PurchaseOrderInventoryLinkProposalReview Review,string RowRevision)
{
    public const int MaximumLength=262144;
    public static string Digest(PO_LINE_ITEM row){var copy=CompanyInventorySourceReview.Copy(row);copy.INVENTORY_LINK_REVIEW_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static string Create(PurchaseOrderInventoryLinkProposalCommand command,PurchaseOrderInventoryLinkProposalReview review,PO_LINE_ITEM row)
    {
        command.Require(review);var text=JsonSerializer.Serialize(new PurchaseOrderInventoryLinkProposalProof(command,review,Digest(row)));
        if(text.Length>MaximumLength)throw new InvalidOperationException("The recorded preparation exceeds its migrated bound.");return text;
    }
    public static PurchaseOrderInventoryLinkProposalProof Require(CompanyScope company,PO_LINE_ITEM row)
    {
        if(row.INVENTORY_LINK_DECISION_JSON is not null)
        {
            var decision=PurchaseOrderInventoryLinkDecisionProof.Require(company,row);
            return Require(company,decision.Review.Purchase.Lines.Single(line=>line.PO_LINE_ITEM_ID==decision.Command.LineId));
        }
        if(string.IsNullOrEmpty(row.INVENTORY_LINK_REVIEW_JSON) || row.INVENTORY_LINK_REVIEW_JSON.Length>MaximumLength)throw new InvalidOperationException("Recorded inventory link preparation is unavailable.");
        PurchaseOrderInventoryLinkProposalProof? proof;
        try{proof=JsonSerializer.Deserialize<PurchaseOrderInventoryLinkProposalProof>(row.INVENTORY_LINK_REVIEW_JSON);}catch(JsonException){throw new InvalidOperationException("Recorded inventory link preparation is malformed.");}
        if(proof?.Command is null || proof.Review is null || proof.Command.Company!=company)throw new InvalidOperationException("The preparation does not belong to this company.");
        proof.Command.Require(proof.Review);
        var expected=CompanyInventorySourceReview.Copy(proof.Review.Purchase.Lines.Single(line=>line.PO_LINE_ITEM_ID==proof.Command.LineId));
        expected.ROW_CHANGED_BY=proof.Command.ActorUserId;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;
        if(row.ROW_CHANGED_DATE is not {} utc || utc.Kind!=DateTimeKind.Utc || utc==default || row.ROW_CREATED_DATE is {} created && utc<created || row.ROW_CHANGED_BY!=proof.Command.ActorUserId ||
            proof.RowRevision!=Digest(row) || Digest(expected)!=Digest(row) || Create(proof.Command,proof.Review,row)!=row.INVENTORY_LINK_REVIEW_JSON)
            throw new InvalidOperationException("The recorded preparation changed original line data or lacks exact preparer audit.");
        return proof;
    }
}
