namespace TheTechIdea.Data.OilGas;
public sealed record PurchaseOrderInventoryLinkResolution(PurchaseOrderSourceReview Purchase,string BindingRevision)
{
    public void Require(PurchaseOrderInventoryLinkProposalSubmission original)
    {
        original.Require(original.Command.Company,original.Command.ActorUserId,original.Command.OrderId,original.Command.LineId);
        if(Purchase is null || BindingRevision!=original.Command.BindingRevision)throw new InvalidOperationException("The original preparation resolution binding is unavailable.");
        Purchase.Require(original.Command.Company,original.Command.ActorUserId,original.Command.OrderId);
        var line=Purchase.Lines.Single(row=>row.PO_LINE_ITEM_ID==original.Command.LineId);
        var decision=PurchaseOrderInventoryLinkDecisionProof.Require(original.Command.Company,line);
        var prepared=decision.Review.Purchase.Lines.Single(row=>row.PO_LINE_ITEM_ID==original.Command.LineId);original.RequireResult(prepared);
        var preparedSource=PurchaseOrderSourceReview.Create(original.Command.Company,original.Command.ActorUserId,Purchase.Header,Purchase.Lines.Select(row=>row.PO_LINE_ITEM_ID==original.Command.LineId?prepared:row).ToArray(),Purchase.Receipts);
        if(decision.Command.BindingRevision!=BindingRevision || preparedSource.Revision!=decision.Review.Purchase.Revision)
            throw new InvalidOperationException("The complete independent resolution source changed.");
    }
}
