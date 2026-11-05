using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyInventorySourceTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    private static INVENTORY_ITEM Item()=>new(){INVENTORY_ITEM_ID="item",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",UNIT_OF_MEASURE="ea",CURRENCY_CODE="KWD",QUANTITY_ON_HAND=null};
    [Theory][InlineData("INVENTORY_ITEM")][InlineData("INVENTORY_TRANSACTION")]
    public void InventoryTypesHaveOneCanonicalDataOwnerAndUniqueNativeKey(string table)
    {
        var type=typeof(CompanyScope).Assembly.GetType("Beep.OilandGas.Models.Data.ProductionAccounting."+table)!;
        Assert.NotNull(type);Assert.Equal(typeof(CompanyScope).Assembly,type.Assembly);
        var key=Assert.Single(new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(type).Fields,field=>field.IsKey);
        Assert.Equal(table+"_ID",key.FieldName);Assert.False(key.AllowDBNull);
    }
    [Theory][InlineData("OWNER_TENANT_ID",64)][InlineData("OWNER_COMPANY_BA_ID",128)][InlineData("WAREHOUSE_ID",128)][InlineData("LOCATION_ID",128)][InlineData("CURRENCY_CODE",32)]
    public void OwnershipAndWarehouseEvidenceUsesNullableBoundedBeepMetadata(string name,int bound)
    {
        var field=Assert.Single(new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(INVENTORY_ITEM)).Fields,row=>row.FieldName==name);
        Assert.Equal("System.String",field.Fieldtype);Assert.True(field.AllowDBNull);Assert.Equal(bound,field.Size1);
    }
    [Theory][InlineData("valid")][InlineData("foreign")][InlineData("duplicate")][InlineData("inactive")][InlineData("null")]
    public async Task CompanyQueueUsesOwnerPredicatesAndRejectsUnconfirmedRows(string mode)
    {
        var item=Item();if(mode=="foreign")item.OWNER_COMPANY_BA_ID="other";if(mode=="inactive")item.ACTIVE_IND="N";
        var source=new Mock<IDataSource>();source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(INVENTORY_ITEM)));
        source.Setup(value=>value.GetEntityAsync("INVENTORY_ITEM",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
            Assert.Equal(3,filters.Count);Assert.Contains(filters,f=>f.FieldName=="OWNER_TENANT_ID" && f.FilterValue=="tenant" && f.Operator=="=");
            Assert.Contains(filters,f=>f.FieldName=="OWNER_COMPANY_BA_ID" && f.FilterValue=="company" && f.Operator=="=");Assert.Contains(filters,f=>f.FieldName=="ACTIVE_IND" && f.FilterValue=="Y");
            return mode=="null"?null!:mode=="duplicate"?new[]{item,item}:new[]{item};});
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);editor.Setup(value=>value.GetDataSource("bound")).Returns(source.Object);
        // Only read methods are exercised; the existing posting dependency must not be invoked.
        var service=CreateService(editor,()=>Task.FromResult("bound"));
        if(mode=="valid"){var returned=Assert.Single(await service.GetCompanyInventoryItemsAsync(Company));item.WAREHOUSE_ID="changed";Assert.Equal("warehouse",returned.WAREHOUSE_ID);Assert.Null(returned.QUANTITY_ON_HAND);}
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>service.GetCompanyInventoryItemsAsync(Company));
    }
    internal static InventoryService CreateService(Mock<IDMEEditor> editor,Func<Task<string>>? resolver=null,ICommonColumnHandler? columns=null,IPPDMMetadataRepository? metadata=null)
    {
        var journal=new JournalEntryService(editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),new GLAccountService(editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),NullLogger<GLAccountService>.Instance),NullLogger<JournalEntryService>.Instance);
        return new InventoryService(editor.Object,columns ?? Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata ?? Mock.Of<IPPDMMetadataRepository>(),new AccountingBasisPostingService(journal),NullLogger<InventoryService>.Instance,resolveConnection:resolver);
    }
    [Fact] public async Task CompanyInventoryCannotUseDefaultConnection()
    {
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);await Assert.ThrowsAsync<InvalidOperationException>(()=>CreateService(editor).GetCompanyInventoryItemsAsync(Company));editor.VerifyNoOtherCalls();
    }
    [Theory][InlineData("foreign")][InlineData("duplicate")][InlineData("changed-item")][InlineData("changed-history")][InlineData("actor")][InlineData("null")]
    public void CompoundReviewRejectsWrongMembershipAndChangedEvidence(string mode)
    {
        var item=Item();var txn=new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID="movement",INVENTORY_ITEM_ID="item",ACTIVE_IND="N",QUANTITY=3m};
        if(mode=="foreign")txn.INVENTORY_ITEM_ID="other";
        if(mode is "foreign" or "duplicate" or "null")Assert.Throws<InvalidOperationException>(()=>CompanyInventorySourceReview.Create(Company,"warehouse-user",item,mode=="null"?null!:mode=="duplicate"?[txn,txn]:[txn]));
        else
        {
            var review=CompanyInventorySourceReview.Create(Company,"warehouse-user",item,[txn]);
            if(mode=="changed-item")review.Item.LOCATION_ID="other";if(mode=="changed-history")review.Transactions[0].QUANTITY=5m;
            Assert.Throws<InvalidOperationException>(()=>review.Require(Company,mode=="actor"?"other":"warehouse-user","item"));
        }
    }
    [Theory][InlineData("valid")][InlineData("foreign-history")][InlineData("duplicate-history")][InlineData("null-history")][InlineData("foreign-item")][InlineData("duplicate-item")][InlineData("missing-item")]
    public async Task ActualSourceReadRequiresOwnedUniqueItemAndCompleteHistory(string mode)
    {
        var item=Item();if(mode=="foreign-item")item.OWNER_TENANT_ID="other";
        var transaction=new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID="movement",INVENTORY_ITEM_ID=mode=="foreign-history"?"other":"item",ACTIVE_IND="N",QUANTITY=null};
        var source=new Mock<IDataSource>();var creator=new ClassCreator(Mock.Of<IDMEEditor>());
        source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>creator.ConvertToEntityStructure(request.EntityName=="INVENTORY_ITEM"?typeof(INVENTORY_ITEM):typeof(INVENTORY_TRANSACTION)));
        source.Setup(value=>value.GetEntityAsync("INVENTORY_ITEM",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
            Assert.Equal(3,filters.Count);Assert.Contains(filters,f=>f.FieldName=="OWNER_TENANT_ID" && f.FilterValue=="tenant");Assert.Contains(filters,f=>f.FieldName=="OWNER_COMPANY_BA_ID" && f.FilterValue=="company");Assert.Contains(filters,f=>f.FieldName=="INVENTORY_ITEM_ID" && f.FilterValue=="item");
            return mode=="duplicate-item"?new[]{item,item}:mode=="missing-item"?Array.Empty<INVENTORY_ITEM>():new[]{item};});
        source.Setup(value=>value.GetEntityAsync("INVENTORY_TRANSACTION",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
            var filter=Assert.Single(filters);Assert.Equal("INVENTORY_ITEM_ID",filter.FieldName);Assert.Equal("item",filter.FilterValue);Assert.Equal("=",filter.Operator);
            return mode=="null-history"?null!:mode=="duplicate-history"?new[]{transaction,transaction}:new[]{transaction};});
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);editor.Setup(value=>value.GetDataSource("bound")).Returns(source.Object);
        var service=CreateService(editor,()=>Task.FromResult("bound"));
        if(mode=="valid")
        {
            var review=await service.GetCompanyInventorySourceAsync(Company,"item","warehouse-user");Assert.NotNull(review);review.Require(Company,"warehouse-user","item");
            Assert.Null(review.Item.QUANTITY_ON_HAND);Assert.Null(Assert.Single(review.Transactions).QUANTITY);Assert.Equal("N",review.Transactions[0].ACTIVE_IND);
            item.LOCATION_ID="other";transaction.ACTIVE_IND="Y";Assert.Equal("bin",review.Item.LOCATION_ID);Assert.Equal("N",review.Transactions[0].ACTIVE_IND);
        }
        else if(mode=="missing-item")Assert.Null(await service.GetCompanyInventorySourceAsync(Company,"item","warehouse-user"));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>service.GetCompanyInventorySourceAsync(Company,"item","warehouse-user"));
        if(mode is "foreign-item" or "duplicate-item" or "missing-item")source.Verify(value=>value.GetEntityAsync("INVENTORY_TRANSACTION",It.IsAny<List<AppFilter>>()),Times.Never);
    }
    [Theory][InlineData("OWNER_TENANT_ID")][InlineData("WAREHOUSE_ID")][InlineData("LOCATION_ID")][InlineData("CURRENCY_CODE")][InlineData("key")][InlineData("duplicate")]
    public async Task UnconfirmedSchemaRefusesBeforeDataRead(string missing)
    {
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(INVENTORY_ITEM));
        if(missing=="key")Assert.Single(schema.Fields,field=>field.IsKey).AllowDBNull=true;
        else if(missing=="duplicate")schema.Fields.Add(schema.Fields.Single(field=>field.FieldName=="WAREHOUSE_ID"));
        else schema.Fields.RemoveAll(field=>field.FieldName==missing);
        var source=new Mock<IDataSource>(MockBehavior.Strict);source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(schema);
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);editor.Setup(value=>value.GetDataSource("bound")).Returns(source.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>CreateService(editor,()=>Task.FromResult("bound")).GetCompanyInventoryItemsAsync(Company));source.Verify(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true),Times.Once);source.VerifyNoOtherCalls();
    }
}
