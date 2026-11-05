using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceCorrectionHistoryTests
{
    private sealed class Fixture
    {
        public readonly InvoiceVarianceDecisionCommandTests.Fixture Existing=new();
        public APInvoiceCorrectionSubmission Input=null!;
        public APInvoiceCorrectionResult Result=null!;
        public async Task Prepare(){await Existing.Prepare(within:false);await Existing.Record();await Next(3m);}
        public async Task Next(decimal total)
        {
            var current=await InvoiceVarianceDecisionReviewTests.Financial(Existing.Existing,"actor");
            var draft=new APInvoiceCorrectionDraft(current.Invoice.Revision,current.Revision,Existing.Command.IntentId,
                APInvoiceCorrectionReview.DecisionRevision(current.Invoice.Invoice.VARIANCE_DECISION_JSON!),total,"Correct billed values for the recorded controller request.",[new("line",total,1m,total,"Explicit corrected line")]);
            var review=APInvoiceCorrectionReview.Create(current,draft,Existing.Command.BindingRevision);var prior=current.Invoice.Invoice.CORRECTION_HISTORY_JSON;
            var count=prior is null?0:APInvoiceCorrectionHistory.Require(Existing.Command.Company,current.Invoice.Invoice,current.Invoice.Lines).Versions.Count;
            var command=new APInvoiceCorrectionCommand(Existing.Command.Company,"actor","invoice",draft,review.Revision,review.BindingRevision,count+1,prior is null?null:ProcessDefinitionRevision.FromJson(prior),Guid.NewGuid().ToString("N"));
            Input=new(command,review);var row=PurchaseReceiptDecisionProof.Copy(current.Invoice.Invoice);var lines=PurchaseReceiptDecisionProof.Copy(current.Invoice.Lines.ToArray());
            var changed=current.Invoice.Invoice.ROW_CHANGED_DATE!.Value.AddSeconds(1);row.TOTAL_AMOUNT=total;row.ROW_CHANGED_BY="actor";row.ROW_CHANGED_DATE=changed;
            foreach(var line in lines){line.QUANTITY=total;line.UNIT_PRICE=1m;line.LINE_TOTAL=total;line.ITEM_DESCRIPTION="Explicit corrected line";line.ROW_CHANGED_BY="actor";line.ROW_CHANGED_DATE=changed;}
            row.CORRECTION_HISTORY_JSON=APInvoiceCorrectionHistory.Create(command,review,row,lines);Result=new(command,row,lines);Result.Require(Input);
        }
        public void SeedRecordedForNextReview()
        {
            // Canonical history fixture only: replace test source rows; this is not a production command or mutation gate.
            var rows=Existing.Existing.Existing.ReadOwner.Native.Native.Rows;
            rows.RemoveAll(row=>row is AP_INVOICE invoice && invoice.AP_INVOICE_ID=="invoice" || row is AP_INVOICE_LINE line && line.AP_INVOICE_ID=="invoice");
            rows.Add(PurchaseReceiptDecisionProof.Copy(Result.Invoice));rows.AddRange(Result.Lines.Select(line=>(object)PurchaseReceiptDecisionProof.Copy(line)));
        }
    }
    [Theory][InlineData("valid")][InlineData("total")][InlineData("line")][InlineData("currency")][InlineData("reference")][InlineData("status")][InlineData("audit")][InlineData("reason")][InlineData("version")][InlineData("revision")][InlineData("unknown-member")][InlineData("foreign-company")][InlineData("oversize")][InlineData("result")][InlineData("null-root-line")][InlineData("empty-root-lines")][InlineData("missing-result-lines")]
    public async Task CompleteCorrectionHistoryPreservesOriginalProofsAndRefusesAlteredRecordedValues(string fault)
    {
        var f=new Fixture();await f.Prepare();var row=PurchaseReceiptDecisionProof.Copy(f.Result.Invoice);var lines=PurchaseReceiptDecisionProof.Copy(f.Result.Lines.ToArray());
        var originalWorkflow=f.Input.Review.Current.Invoice.ForCompanyWorkflow();
        if(fault=="total")row.TOTAL_AMOUNT=99m;if(fault=="line")lines[0].ITEM_DESCRIPTION="Changed after recording";if(fault=="currency")row.CURRENCY_CODE="another";if(fault=="reference")lines[0].PO_LINE_ITEM_ID="another";
        if(fault=="status")row.STATUS="APPROVED";if(fault=="audit")row.ROW_CHANGED_BY="another";if(fault=="reason")row.CORRECTION_HISTORY_JSON=row.CORRECTION_HISTORY_JSON!.Replace(f.Input.Command.Draft.Reason,"A substituted correction explanation.",StringComparison.Ordinal);
        if(fault is "null-root-line" or "empty-root-lines")
        {var history=APInvoiceCorrectionHistory.Require(f.Input.Command.Company,row,lines);row.CORRECTION_HISTORY_JSON=JsonSerializer.Serialize(history with{OriginalLines=fault=="null-root-line"?[null!]:[]});}
        if(fault is "version" or "revision")
        {var history=APInvoiceCorrectionHistory.Require(f.Input.Command.Company,row,lines);var version=history.Versions[0];row.CORRECTION_HISTORY_JSON=JsonSerializer.Serialize(history with{Versions=[fault=="version"?version with{Command=version.Command with{Version=2,PreviousHistoryRevision=new string('C',64)}}:version with{RecordedRevision=new string('C',64)}]});}
        if(fault=="unknown-member")row.CORRECTION_HISTORY_JSON=row.CORRECTION_HISTORY_JSON![..^1]+",\"UnknownMember\":true}";if(fault=="foreign-company")row.OWNER_COMPANY_BA_ID="another";if(fault=="oversize")row.CORRECTION_HISTORY_JSON=new string('x',APInvoiceCorrectionHistory.MaximumLength+1);
        if(fault=="result")Assert.Throws<InvalidOperationException>(()=>(f.Result with{Command=f.Input.Command with{IntentId=Guid.NewGuid().ToString("N")}}).Require(f.Input));
        else if(fault=="missing-result-lines")Assert.Throws<InvalidOperationException>(()=>(f.Result with{Lines=null!}).Require(f.Input));
        else if(fault!="valid")
        {
            Assert.Throws<InvalidOperationException>(()=>APInvoiceCorrectionHistory.Require(f.Input.Command.Company,row,lines));
            Assert.Throws<InvalidOperationException>(()=>APInvoiceSourceReview.Create(f.Input.Command.Company,"actor",row,lines));
        }
        else
        {
            var history=APInvoiceCorrectionHistory.Require(f.Input.Command.Company,row,lines);Assert.Single(history.Versions);Assert.Equal(2.01m,history.OriginalInvoice.TOTAL_AMOUNT);Assert.Equal(3m,row.TOTAL_AMOUNT);
            Assert.Equal(history.OriginalInvoice.VARIANCE_PROPOSAL_JSON,row.VARIANCE_PROPOSAL_JSON);Assert.Equal(history.OriginalInvoice.VARIANCE_DECISION_JSON,row.VARIANCE_DECISION_JSON);
            Assert.Equal(f.Existing.Command.IntentId,APInvoiceVarianceDecisionProof.Require(f.Input.Command.Company,row).Command.IntentId);Assert.Equal("actor",APInvoiceVarianceProof.Require(f.Input.Command.Company,row).Command.ActorUserId);
            var source=APInvoiceSourceReview.Create(f.Input.Command.Company,"actor",row,lines);Assert.NotEqual(f.Input.Review.Current.Invoice.Revision,source.Revision);Assert.Equal(originalWorkflow.Revision,source.ForCompanyWorkflow().Revision);
            history.OriginalInvoice.TOTAL_AMOUNT=999m;lines[0].QUANTITY=999m;f.Result.Require(f.Input);Assert.Equal(2.01m,APInvoiceCorrectionHistory.Require(f.Input.Command.Company,f.Result.Invoice,f.Result.Lines).OriginalInvoice.TOTAL_AMOUNT);
        }
        Assert.Equal(2,f.Existing.Existing.Existing.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("valid")][InlineData("duplicate-intent")][InlineData("predecessor")][InlineData("order")][InlineData("missing-version")][InlineData("limit")]
    public async Task SuccessiveCorrectionsRequireExactPredecessorsAndBoundedLinearHistory(string fault)
    {
        var f=new Fixture();await f.Prepare();var first=PurchaseReceiptDecisionProof.Copy(f.Result);f.SeedRecordedForNextReview();await f.Next(4m);
        var history=APInvoiceCorrectionHistory.Require(f.Input.Command.Company,f.Result.Invoice,f.Result.Lines);Assert.Equal(2,history.Versions.Count);Assert.Equal(2.01m,history.OriginalInvoice.TOTAL_AMOUNT);
        Assert.Equal(ProcessDefinitionRevision.FromJson(first.Invoice.CORRECTION_HISTORY_JSON!),f.Input.Command.PreviousHistoryRevision);Assert.Equal(4m,f.Result.Invoice.TOTAL_AMOUNT);
        if(fault is "duplicate-intent" or "predecessor" or "order" or "missing-version")
        {
            var last=history.Versions[^1];var versions=history.Versions.ToArray();
            if(fault=="duplicate-intent")versions[1]=last with{Command=last.Command with{IntentId=versions[0].Command.IntentId}};
            if(fault=="predecessor")versions[1]=last with{Command=last.Command with{PreviousHistoryRevision=new string('C',64)}};
            if(fault=="order")versions=[versions[1],versions[0]];if(fault=="missing-version")versions=[versions[1]];
            var row=PurchaseReceiptDecisionProof.Copy(f.Result.Invoice);row.CORRECTION_HISTORY_JSON=JsonSerializer.Serialize(history with{Versions=versions});Assert.Throws<InvalidOperationException>(()=>APInvoiceCorrectionHistory.Require(f.Input.Command.Company,row,f.Result.Lines));
        }
        if(fault=="limit")
        {
            for(var version=3;version<=APInvoiceCorrectionHistory.MaximumVersions;version++){f.SeedRecordedForNextReview();await f.Next(version+2m);}
            Assert.Equal(8,APInvoiceCorrectionHistory.Require(f.Input.Command.Company,f.Result.Invoice,f.Result.Lines).Versions.Count);
            var lastInput=f.Input;f.SeedRecordedForNextReview();await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Next(99m));f.Input=lastInput;
        }
        f.Result.Require(f.Input);Assert.Equal(2,f.Existing.Existing.Existing.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("actor")][InlineData("review")][InlineData("version")][InlineData("predecessor")][InlineData("intent")][InlineData("binding")][InlineData("schema")][InlineData("backwards-audit")]
    public async Task CommandsRequireTheDisplayedReviewAndBeepBoundedNullableMetadata(string fault)
    {
        var f=new Fixture();await f.Prepare();var command=f.Input.Command;
        if(fault=="backwards-audit")
        {
            var row=PurchaseReceiptDecisionProof.Copy(f.Result.Invoice);var lines=PurchaseReceiptDecisionProof.Copy(f.Result.Lines.ToArray());var changed=f.Input.Review.Current.Invoice.Invoice.ROW_CHANGED_DATE!.Value.AddSeconds(-1);
            row.ROW_CHANGED_DATE=changed;foreach(var line in lines)line.ROW_CHANGED_DATE=changed;
            Assert.Throws<InvalidOperationException>(()=>APInvoiceCorrectionHistory.Create(command,f.Input.Review,row,lines));return;
        }
        if(fault=="schema")
        {
            var field=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(AP_INVOICE)).Fields.Single(value=>value.FieldName=="CORRECTION_HISTORY_JSON");
            Assert.Equal(typeof(string).FullName,field.Fieldtype);Assert.True(field.AllowDBNull);Assert.Equal(APInvoiceCorrectionHistory.MaximumLength,field.Size);
            Assert.DoesNotContain("CORRECTION_HISTORY_JSON",JsonSerializer.Serialize(f.Input.Review.Current.Invoice.Invoice));return;
        }
        if(fault=="actor")command=command with{ActorUserId="another"};if(fault=="review")command=command with{ExpectedReviewRevision=new string('C',64)};if(fault=="version")command=command with{Version=2,PreviousHistoryRevision=new string('C',64)};
        if(fault=="predecessor")command=command with{PreviousHistoryRevision=new string('C',64)};if(fault=="intent")command=command with{IntentId="invalid"};if(fault=="binding")command=command with{BindingRevision=new string('C',64)};
        Assert.ThrowsAny<Exception>(()=>command.Require(f.Input.Review));
    }
}
#endif
