using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Accounting.Models;

public enum PurchaseOrderInventoryLinkCycleAction { ReopenResolved=1, SupersedePending=2 }
public sealed record PurchaseOrderInventoryLinkCycleReview(PurchaseOrderSourceReview Purchase,PurchaseOrderInventoryLinkHistoryReview History,string LineId,PurchaseOrderInventoryLinkCycleAction Action,string BindingRevision,string Revision)
{
    public void Require(CompanyScope company,string actor,string order,string line)
    {
        if(Purchase is null || History is null || LineId!=line || !Enum.IsDefined(Action) || string.IsNullOrWhiteSpace(BindingRevision) || BindingRevision.Length>512 || BindingRevision.Any(char.IsControl))
            throw new InvalidOperationException("Explicit original cycle action and binding are required.");
        Purchase.Require(company,actor,order);
        var row=Purchase.Lines.SingleOrDefault(value=>value.PO_LINE_ITEM_ID==line) ?? throw new InvalidOperationException("The cycle line is outside the owned order.");
        History.Require(company,actor,order,line,row.INVENTORY_LINK_HISTORY_REVISION);
        _=PurchaseOrderInventoryLinkProposalProof.Require(company,row);
        if(Purchase.Header.ACTIVE_IND!="Y" || row.ACTIVE_IND!="Y" || actor=="SYSTEM" ||
            (Action==PurchaseOrderInventoryLinkCycleAction.ReopenResolved)!=(row.INVENTORY_LINK_DECISION_JSON is not null) ||
            History.Entries.Count>=PurchaseOrderInventoryLinkHistoryReview.MaximumEntries || Revision!=Digest(Purchase,History,line,Action,BindingRevision))
            throw new InvalidOperationException("A complete active resolved or pending cycle and retained history are required.");
    }
    public static PurchaseOrderInventoryLinkCycleReview Create(PurchaseOrderSourceReview purchase,PurchaseOrderInventoryLinkHistoryReview history,string line,PurchaseOrderInventoryLinkCycleAction action,string binding)
    {
        purchase=CompanyInventorySourceReview.Copy(purchase);history=CompanyInventorySourceReview.Copy(history);
        var result=new PurchaseOrderInventoryLinkCycleReview(purchase,history,line,action,binding,Digest(purchase,history,line,action,binding));
        result.Require(purchase.Company,purchase.ActorUserId,purchase.Header.PURCHASE_ORDER_ID,line);return result;
    }
    private static string Digest(PurchaseOrderSourceReview purchase,PurchaseOrderInventoryLinkHistoryReview history,string line,PurchaseOrderInventoryLinkCycleAction action,string binding)
        =>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{purchase.Company,purchase.ActorUserId,Purchase=purchase.Revision,History=history.Revision,Line=line,Action=action,Binding=binding}));
}
public sealed record PurchaseOrderInventoryLinkCycleCommand(CompanyScope Company,string ActorUserId,string OrderId,string LineId,PurchaseOrderInventoryLinkCycleAction Action,string ReviewRevision,string BindingRevision,string IntentId,string Reason)
{
    public void Require(PurchaseOrderInventoryLinkCycleReview review)
    {
        static bool Text(string? value,int bound)=>!string.IsNullOrWhiteSpace(value) && value.Length<=bound && !value.Any(char.IsControl);
        if(review is null || !Text(ActorUserId,128) || !Text(OrderId,128) || !Text(LineId,128) || !Text(Reason,512) ||
            !Guid.TryParseExact(IntentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=IntentId || Action!=review.Action || ReviewRevision!=review.Revision || BindingRevision!=review.BindingRevision)
            throw new InvalidOperationException("Retain the explicit original cycle action, reason, actor and canonical intent.");
        review.Require(Company,ActorUserId,OrderId,LineId);
    }
    public void RequireEntry(PurchaseOrderInventoryLinkCycleReview review,PO_INVENTORY_LINK_HISTORY entry)
    {
        Require(review);_=PurchaseOrderInventoryLinkHistoryReview.RequireEntry(Company,OrderId,LineId,entry);
        var original=review.Purchase.Lines.Single(value=>value.PO_LINE_ITEM_ID==LineId);var predecessor=review.History.Entries.LastOrDefault();
        if(entry.PO_INVENTORY_LINK_HISTORY_ID!=IntentId || entry.CYCLE_INTENT_ID!=IntentId || entry.SOURCE_REVISION!=review.Purchase.Revision || entry.BINDING_REVISION!=BindingRevision || entry.REASON!=Reason ||
            entry.ROW_CREATED_BY!=ActorUserId || entry.SEQUENCE_NUMBER!=review.History.Entries.Count+1 || entry.PREVIOUS_ENTRY_ID!=predecessor?.PO_INVENTORY_LINK_HISTORY_ID || entry.PREVIOUS_REVISION!=original.INVENTORY_LINK_HISTORY_REVISION ||
            entry.ARCHIVED_LINE_JSON!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The archived cycle does not confirm the original complete submitted intent.");
    }
    public PO_LINE_ITEM ReopenedLine(PurchaseOrderInventoryLinkCycleReview review,PO_INVENTORY_LINK_HISTORY entry)
    {
        RequireEntry(review,entry);var row=CompanyInventorySourceReview.Copy(review.Purchase.Lines.Single(value=>value.PO_LINE_ITEM_ID==LineId));
        row.INVENTORY_LINK_REVIEW_JSON=null;row.INVENTORY_LINK_DECISION_JSON=null;row.INVENTORY_LINK_HISTORY_REVISION=entry.RECORDED_REVISION;
        row.ROW_CHANGED_BY=ActorUserId;row.ROW_CHANGED_DATE=entry.ROW_CREATED_DATE;return row;
    }
}
public sealed record PurchaseOrderInventoryLinkCycleSubmission(PurchaseOrderInventoryLinkCycleCommand Command,PurchaseOrderInventoryLinkCycleReview Review)
{
    public void Require(CompanyScope company,string actor,string order,string line)
    {
        if(Command is null || Review is null || Command.Company!=company || Command.ActorUserId!=actor || Command.OrderId!=order || Command.LineId!=line)
            throw new InvalidOperationException("Retain the original cycle company, account, order and line.");
        Command.Require(Review);
    }
    public void RequireResult(PurchaseOrderInventoryLinkCycleResult result)
    {
        Require(Command.Company,Command.ActorUserId,Command.OrderId,Command.LineId);
        if(result?.Purchase is null || result.History is null)throw new InvalidOperationException("Complete committed cycle evidence is unavailable.");
        result.Require(Command.Company,Command.ActorUserId,Command.OrderId,Command.LineId);
        var row=result.Purchase.Lines.Single(value=>value.PO_LINE_ITEM_ID==Command.LineId);
        var entry=result.History.Entries.LastOrDefault() ?? throw new InvalidOperationException("The original archive is unavailable.");
        if(JsonSerializer.Serialize(result.History.Entries.Take(result.History.Entries.Count-1))!=JsonSerializer.Serialize(Review.History.Entries) || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(Command.ReopenedLine(Review,entry)))
            throw new InvalidOperationException("The original history or reopened line changed.");
        var reconstructed=PurchaseOrderSourceReview.Create(Command.Company,Command.ActorUserId,result.Purchase.Header,result.Purchase.Lines.Select(value=>value.PO_LINE_ITEM_ID==Command.LineId?Review.Purchase.Lines.Single(original=>original.PO_LINE_ITEM_ID==Command.LineId):value).ToArray(),result.Purchase.Receipts);
        if(reconstructed.Revision!=Review.Purchase.Revision)throw new InvalidOperationException("The complete original order changed across the cycle transition.");
    }
}
public sealed record PurchaseOrderInventoryLinkCycleResult(PurchaseOrderSourceReview Purchase,PurchaseOrderInventoryLinkHistoryReview History)
{
    public void Require(CompanyScope company,string actor,string order,string line)
    {
        if(Purchase is null || History is null)throw new InvalidOperationException("Complete owned order and history evidence is required.");
        Purchase.Require(company,actor,order);
        var row=Purchase.Lines.SingleOrDefault(value=>value.PO_LINE_ITEM_ID==line) ?? throw new InvalidOperationException("History line is outside the owned order.");
        History.Require(company,actor,order,line,row.INVENTORY_LINK_HISTORY_REVISION);
    }
}
