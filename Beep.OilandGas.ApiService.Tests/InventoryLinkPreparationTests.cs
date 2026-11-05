using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InventoryLinkPreparationTests
{
    [Theory][InlineData("valid")][InlineData("retry")][InlineData("existing-link")][InlineData("grant")][InlineData("authority-during")][InlineData("target")][InlineData("schema")][InlineData("corrupt")][InlineData("write")][InlineData("unknown")][InlineData("intent")][InlineData("reason")][InlineData("post-commit")]
    public async Task OwnedPreparationRecordsOnlyReviewProofAndPreservesOriginalInventoryReference(string mode)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();var n=f.Existing.Native;
        var originalReference=mode=="existing-link"?"current-item":null;f.Existing.OrderLine.INVENTORY_ITEM_ID=originalReference;
        var metadata=(IPPDMMetadataRepository)typeof(PurchaseOrderService).GetField("_metadata",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(f.Existing.PO)!;
        Mock.Get(metadata).Setup(value=>value.GetPrimaryKeyColumnsAsync("PO_LINE_ITEM")).ReturnsAsync(new List<string>{"PO_LINE_ITEM_ID"});
        var target=new INVENTORY_ITEM{INVENTORY_ITEM_ID="target",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",UNIT_OF_MEASURE="bbl",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",QUANTITY_ON_HAND=null};
        CompanyInventorySourceReview Target()=>CompanyInventorySourceReview.Create(new("tenant","company"),"actor",target,[]);
        var source=(await f.Existing.PO.GetSourceReviewAsync(new("tenant","company"),"order","actor"))!;
        var review=PurchaseOrderInventoryLinkProposalReview.Create(source,Target(),"po-line","binding");
        var command=new PurchaseOrderInventoryLinkProposalCommand(source.Company,"actor","order","po-line","target",review.Revision,"binding",Guid.NewGuid().ToString("N"),"Reviewed target warehouse and matching unit.");
        var authorityCalls=0;
        Task Authority(){++authorityCalls;if(mode=="grant" || mode=="authority-during" && authorityCalls>1)throw new UnauthorizedAccessException();return Task.CompletedTask;}
        n.Source.Setup(value=>value.UpdateEntity("PO_LINE_ITEM",It.IsAny<object>())).Returns((string table,object value)=>{
            ++n.Writes;if(mode=="write")return new ErrorsInfo{Flag=Errors.Failed};
            var line=CompanyInventorySourceReview.Copy((PO_LINE_ITEM)value);if(mode=="corrupt")line.INVENTORY_ITEM_ID="tampered";
            n.Rows.RemoveAll(row=>row is PO_LINE_ITEM existing && existing.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID);n.Rows.Add(line);return new ErrorsInfo{Flag=Errors.Ok};});
        if(mode=="target")target.LOCATION_ID="other";
        if(mode=="schema")
        {
            var schema=new TheTechIdea.Beep.Tools.ClassCreator(n.Editor.Object).ConvertToEntityStructure(typeof(PO_LINE_ITEM));schema.Fields.RemoveAll(field=>field.FieldName=="INVENTORY_LINK_REVIEW_JSON");
            n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(entity=>entity.EntityName=="PO_LINE_ITEM"),true)).Returns(schema);
        }
        if(mode=="unknown")n.FailCommit=true;if(mode=="post-commit")n.AfterCommit=()=>target.LOCATION_ID="other";
        Task<PO_LINE_ITEM> Record()=>f.Existing.PO.PrepareInventoryLinkAsync(command,review,()=>Task.FromResult(Target()),Authority);
        if(mode is "grant" or "authority-during" or "target" or "schema" or "corrupt" or "write")
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>Record());var stored=Assert.Single(n.Rows.OfType<PO_LINE_ITEM>());Assert.Equal(originalReference,stored.INVENTORY_ITEM_ID);Assert.Null(stored.INVENTORY_LINK_REVIEW_JSON);
            Assert.Equal(mode is "corrupt" or "write"?1:0,n.Writes);if(mode!="grant")Assert.Contains("rollback",n.Transactions);
        }
        else
        {
            if(mode is "unknown" or "post-commit"){await Assert.ThrowsAnyAsync<Exception>(()=>Record());n.FailCommit=false;n.AfterCommit=null;target.LOCATION_ID="bin";}
            var stored=await Record();var proof=PurchaseOrderInventoryLinkProposalProof.Require(source.Company,stored);
            Assert.Equal(command,proof.Command);Assert.Equal(originalReference,stored.INVENTORY_ITEM_ID);Assert.Equal(1,n.Writes);Assert.Equal("actor",stored.ROW_CHANGED_BY);Assert.NotNull(stored.ROW_CHANGED_DATE);
            if(mode is "intent" or "reason"){command=mode=="intent"?command with{IntentId=Guid.NewGuid().ToString("N")}:command with{Reason="Changed reason"};await Assert.ThrowsAsync<InvalidOperationException>(()=>Record());}
            else{await Record();Assert.Equal(1,n.Writes);}
            Assert.Null(Assert.Single(n.Rows.OfType<PO_RECEIPT>()).RECEIVER_DECISION_JSON);
        }
    }
    [Theory][InlineData("unit")][InlineData("company")][InlineData("location")][InlineData("active")][InlineData("same-item")]
    public async Task ProposalReviewRequiresAnExplicitOwnedActiveDifferentTargetWithMatchingUnit(string mode)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();var source=(await f.Existing.PO.GetSourceReviewAsync(new("tenant","company"),"order","actor"))!;
        var target=new INVENTORY_ITEM{INVENTORY_ITEM_ID="item",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",UNIT_OF_MEASURE="bbl",WAREHOUSE_ID="warehouse",LOCATION_ID="bin"};
        if(mode=="unit")target.UNIT_OF_MEASURE="m³";if(mode=="company")target.OWNER_COMPANY_BA_ID="other";if(mode=="location")target.LOCATION_ID=null;if(mode=="active")target.ACTIVE_IND="N";
        if(mode=="same-item"){f.Existing.OrderLine.INVENTORY_ITEM_ID="item";source=(await f.Existing.PO.GetSourceReviewAsync(new("tenant","company"),"order","actor"))!;}
        Assert.Throws<InvalidOperationException>(()=>PurchaseOrderInventoryLinkProposalReview.Create(source,CompanyInventorySourceReview.Create(new("tenant","company"),"actor",target,[]),"po-line","binding"));
    }
}
#endif
