using System.Security.Claims;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class PurchaseOrderOwnershipTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    private static PURCHASE_ORDER Order()=>new(){PURCHASE_ORDER_ID="order",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier"};
    [Fact] public void CanonicalPurchaseTypesHaveOneDataOwnerAndNullableUnassignedOwnershipMetadata()
    {
        Assert.Equal(typeof(CompanyScope).Assembly,typeof(PURCHASE_ORDER).Assembly);Assert.Equal(typeof(CompanyScope).Assembly,typeof(PO_LINE_ITEM).Assembly);Assert.Equal(typeof(CompanyScope).Assembly,typeof(PO_RECEIPT).Assembly);
        var creator=new ClassCreator(Mock.Of<IDMEEditor>());var schema=creator.ConvertToEntityStructure(typeof(PURCHASE_ORDER));
        foreach(var column in new[]{"OWNER_TENANT_ID","OWNER_COMPANY_BA_ID"})
        {
            var field=Assert.Single(schema.Fields,row=>row.FieldName==column);Assert.Equal(typeof(string).FullName,field.Fieldtype);Assert.True(field.AllowDBNull);
        }
        Assert.Equal(64,Assert.Single(schema.Fields,row=>row.FieldName=="OWNER_TENANT_ID").Size1);
        Assert.Equal(128,Assert.Single(schema.Fields,row=>row.FieldName=="OWNER_COMPANY_BA_ID").Size1);
    }
    [Fact] public void OwningCompanyDoesNotHaveToBeTheVendor(){PurchaseOrderOwnership.Require(Company,"order",Order());}
    [Theory][InlineData("missing")][InlineData("tenant")][InlineData("company")][InlineData("identity")][InlineData("vendor-as-owner")]
    public void UnassignedOrOtherCompanyHeaderCannotBeAdopted(string fault)
    {
        var order=Order();if(fault=="missing"){order.OWNER_TENANT_ID=null;order.OWNER_COMPANY_BA_ID=null;}if(fault=="tenant")order.OWNER_TENANT_ID="other";
        if(fault=="company")order.OWNER_COMPANY_BA_ID="other";if(fault=="identity")order.PURCHASE_ORDER_ID="other";
        if(fault=="vendor-as-owner"){order.VENDOR_BA_ID="company";order.OWNER_COMPANY_BA_ID="other";}
        Assert.Throws<InvalidOperationException>(()=>PurchaseOrderOwnership.Require(Company,"order",order));
    }
    [Theory][InlineData("missing")][InlineData("foreign")][InlineData("line")][InlineData("duplicate")]
    public void UnconfirmedReceiptMembershipCannotBeReported(string fault)
    {
        var receipt=new PO_RECEIPT{PO_RECEIPT_ID="receipt",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="line"};
        if(fault=="foreign")receipt.PURCHASE_ORDER_ID="other";if(fault=="line")receipt.PO_LINE_ITEM_ID="";
        Assert.Throws<InvalidOperationException>(()=>PurchaseOrderOwnership.RequireReceipts("order",fault=="missing"?null:fault=="duplicate"?[receipt,receipt]:[receipt]));
    }
    [Fact] public void ReadNeedsFreshCompanyRoleAndIndependentPermission()
    {
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,"ConfiguredBuyer")],"app"));
        Assert.True(PurchaseOrderReadChecks.CanRead(user,Company,new(Company,["ConfiguredBuyer"],["Procurement.View"])));
        Assert.False(PurchaseOrderReadChecks.CanRead(user,Company,new(Company,["ConfiguredBuyer"],[])));
        Assert.False(PurchaseOrderReadChecks.CanRead(user,Company,new(Company,["AnotherRole"],["Procurement.View"])));
        Assert.False(PurchaseOrderReadChecks.CanRead(user,Company,new(new("tenant","other"),["ConfiguredBuyer"],["Procurement.View"])));
    }
    [Fact] public async Task CanonicalServiceCannotFallBackToSharedDefaultConnection()
    {
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);
        var service=new PurchaseOrderService(editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),NullLogger<PurchaseOrderService>.Instance);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>service.GetPOByIdAsync(Company,"order"));editor.VerifyNoOtherCalls();
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task MissingFreshOwnershipColumnsRefuseBeforeRepositoryRead(bool allMissing)
    {
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(PURCHASE_ORDER));
        schema.Fields.RemoveAll(field=>allMissing || field.FieldName=="OWNER_COMPANY_BA_ID");
        var source=new Mock<IDataSource>(MockBehavior.Strict);source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(schema);
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);editor.Setup(value=>value.GetDataSource("bound-production")).Returns(source.Object);
        var service=new PurchaseOrderService(editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),NullLogger<PurchaseOrderService>.Instance,()=>Task.FromResult("bound-production"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>service.GetPOByIdAsync(Company,"order"));
        source.Verify(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="PURCHASE_ORDER" && request.Fields.Count==0),true),Times.Once);source.VerifyNoOtherCalls();
    }
    [Fact] public void CanonicalPurchaseEntitiesDeclareSingleNativeIdentityKeys()
    {
        var creator=new ClassCreator(Mock.Of<IDMEEditor>());
        foreach(var type in new[]{typeof(PURCHASE_ORDER),typeof(PO_LINE_ITEM),typeof(PO_RECEIPT)})
        {
            var key=Assert.Single(creator.ConvertToEntityStructure(type).Fields,field=>field.IsKey);
            Assert.Equal(type.Name+"_ID",key.FieldName);Assert.False(key.AllowDBNull);
        }
    }
    [Theory][InlineData("missing")][InlineData("nullable")][InlineData("compound")]
    public async Task UnconfirmedHeaderKeyCannotEstablishChildOwnership(string fault)
    {
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(PURCHASE_ORDER));
        var key=Assert.Single(schema.Fields,field=>field.IsKey);
        if(fault=="missing")key.IsKey=false;if(fault=="nullable")key.AllowDBNull=true;
        if(fault=="compound")schema.Fields.Single(field=>field.FieldName=="OWNER_COMPANY_BA_ID").IsKey=true;
        var source=new Mock<IDataSource>(MockBehavior.Strict);source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(schema);
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);editor.Setup(value=>value.GetDataSource("bound-production")).Returns(source.Object);
        var service=new PurchaseOrderService(editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),NullLogger<PurchaseOrderService>.Instance,()=>Task.FromResult("bound-production"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>service.GetSourceReviewAsync(Company,"order","buyer"));
        source.Verify(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true),Times.Once);source.VerifyNoOtherCalls();
    }
    [Fact] public void FinancialViewPermissionDoesNotSubstituteForProcurementAuthority()
    {
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,"ConfiguredBuyer")],"app"));
        Assert.False(PurchaseOrderReadChecks.CanRead(user,Company,new(Company,["ConfiguredBuyer"],["Accounting.View"])));
    }
    [Theory][InlineData("PURCHASE_ORDER","CURRENCY_CODE",32)][InlineData("PO_LINE_ITEM","QUANTITY_UOM",64)][InlineData("PO_RECEIPT","QUANTITY_UOM",64)]
    public void MeasurementFieldsUseNullableBoundedBeepMetadata(string table,string column,int length)
    {
        var type=typeof(PURCHASE_ORDER).Assembly.GetType("Beep.OilandGas.Models.Data.ProductionAccounting."+table)!;
        var field=Assert.Single(new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(type).Fields,row=>row.FieldName==column);
        Assert.Equal("System.String",field.Fieldtype);Assert.True(field.AllowDBNull);Assert.Equal(length,field.Size1);
    }
    [Theory][InlineData("PURCHASE_ORDER","CURRENCY_CODE",false)][InlineData("PURCHASE_ORDER","CURRENCY_CODE",true)]
    [InlineData("PO_LINE_ITEM","QUANTITY_UOM",false)][InlineData("PO_LINE_ITEM","QUANTITY_UOM",true)]
    [InlineData("PO_RECEIPT","QUANTITY_UOM",false)][InlineData("PO_RECEIPT","QUANTITY_UOM",true)]
    public async Task MissingOrAmbiguousMeasurementMetadataRefusesBeforeRepositoryAccess(string table,string column,bool duplicate)
    {
        var type=typeof(PURCHASE_ORDER).Assembly.GetType("Beep.OilandGas.Models.Data.ProductionAccounting."+table)!;
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(type);
        var field=Assert.Single(schema.Fields,row=>row.FieldName==column);
        if(duplicate)schema.Fields.Add(field);else schema.Fields.Remove(field);
        var source=new Mock<IDataSource>(MockBehavior.Strict);source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(schema);
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);editor.Setup(value=>value.GetDataSource("bound-production")).Returns(source.Object);
        var service=new PurchaseOrderService(editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),NullLogger<PurchaseOrderService>.Instance,()=>Task.FromResult("bound-production"));
        var factory=typeof(PurchaseOrderService).GetMethod("GetRepoAsync",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.MakeGenericMethod(type);
        await Assert.ThrowsAsync<InvalidOperationException>(async()=>await (Task)factory.Invoke(service,[table])!);
        source.Verify(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName==table && request.Fields.Count==0),true),Times.Once);source.VerifyNoOtherCalls();
    }
    [Theory][InlineData("valid")][InlineData("foreign")][InlineData("inactive")][InlineData("duplicate")][InlineData("null")]
    public async Task CompanyQueueUsesNativeOriginalOwnerPredicatesAndRefusesWrongRows(string fault)
    {
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(PURCHASE_ORDER));
        var order=Order();order.ACTIVE_IND="Y";if(fault=="foreign")order.OWNER_COMPANY_BA_ID="other";if(fault=="inactive")order.ACTIVE_IND="N";
        var source=new Mock<IDataSource>();source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(schema);
        source.Setup(value=>value.GetEntityAsync("PURCHASE_ORDER",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
            Assert.Equal(3,filters.Count);
            Assert.Contains(filters,f=>f.FieldName=="OWNER_TENANT_ID" && f.FilterValue=="tenant" && f.Operator=="=");
            Assert.Contains(filters,f=>f.FieldName=="OWNER_COMPANY_BA_ID" && f.FilterValue=="company" && f.Operator=="=");
            Assert.Contains(filters,f=>f.FieldName=="ACTIVE_IND" && f.FilterValue=="Y" && f.Operator=="=");
            return fault=="null"?null!:fault=="duplicate"?new[]{order,order}:new[]{order};});
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);editor.Setup(value=>value.GetDataSource("bound-production")).Returns(source.Object);
        var service=new PurchaseOrderService(editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),NullLogger<PurchaseOrderService>.Instance,()=>Task.FromResult("bound-production"));
        if(fault=="valid")Assert.Equal("order",Assert.Single(await service.GetCompanyPOsAsync(Company)).PURCHASE_ORDER_ID);
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>service.GetCompanyPOsAsync(Company));
    }
    [Fact] public async Task EmptyCompanyQueueSupplierAssertionDoesNotOpenAProvider()
    {
        var editor=new Mock<IDMEEditor>(MockBehavior.Strict);var service=new PurchaseOrderService(editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),NullLogger<PurchaseOrderService>.Instance);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>service.GetCompanyPOsAsync(Company," "));editor.VerifyNoOtherCalls();
    }
}

