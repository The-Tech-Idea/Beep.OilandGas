#if WORK_ORDER_FINANCIAL_CONTRACTS_FOCUSED
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class WorkOrderFinancialContractOwnershipTests
{
    [Theory][InlineData("AFE","AFE_ID","Beep.OilandGas.Models")]
    [InlineData("AFE_LINE_ITEM","AFE_LINE_ITEM_ID","Beep.OilandGas.ProductionAccounting")]
    [InlineData("ACCOUNTING_COST","ACCOUNTING_COST_ID","Beep.OilandGas.Models")]
    [InlineData("APPROVAL_WORKFLOW","APPROVAL_WORKFLOW_ID","Beep.OilandGas.Models")]
    public void FinancialEntityHasOneNativeOwnerWithoutForwarders(string name,string key,string originalAssembly)
    {
        var original=Assembly.Load(originalAssembly);
        var type=original.GetType("Beep.OilandGas.Models.Data.ProductionAccounting."+name)!;
        Assert.NotNull(type);Assert.Equal(originalAssembly,type.Assembly.GetName().Name);
        Assert.Equal(key,Assert.Single(type.GetProperties().Where(property=>property.IsDefined(typeof(KeyAttribute),true))).Name);
        Assert.DoesNotContain(type,original.GetForwardedTypes());
        Assert.Equal(type,Type.GetType(type.FullName+", "+originalAssembly,true));
    }
    [Fact] public void ExistingAfeAndLineAmountsRemainExplicitNullableAndRoundTripWithoutInventedBudgetOrActual()
    {
        var afe=JsonSerializer.Deserialize<AFE>("{\"AFE_ID\":\"existing-afe\",\"AFE_NUMBER\":\"recorded-number\",\"FIELD_ID\":\"field\",\"STATUS\":\"PENDING\"}")!;
        Assert.Null(afe.ESTIMATED_COST);Assert.Null(afe.ACTUAL_COST);Assert.Null(afe.APPROVAL_DATE);
        var body=JsonSerializer.Serialize(afe);var recovered=JsonSerializer.Deserialize<AFE>(body)!;Assert.Equal(body,JsonSerializer.Serialize(recovered));Assert.Equal("existing-afe",recovered.AFE_ID);
        var line=JsonSerializer.Deserialize<AFE_LINE_ITEM>("{\"AFE_LINE_ITEM_ID\":\"line\",\"AFE_ID\":\"existing-afe\",\"BUDGET_AMOUNT\":17.125}")!;
        Assert.Equal(17.125m,line.BUDGET_AMOUNT);Assert.Null(line.ACTUAL_AMOUNT);Assert.Null(line.VARIANCE);
        Assert.Equal(JsonSerializer.Serialize(line),JsonSerializer.Serialize(JsonSerializer.Deserialize<AFE_LINE_ITEM>(JsonSerializer.Serialize(line))));
    }
    [Fact] public void ExistingActualCostPreservesCategoryDateAndAmountWithoutInventingAWorkOrderLink()
    {
        var utc=new DateTime(2026,10,14,8,0,0,DateTimeKind.Utc);var row=new ACCOUNTING_COST{ACCOUNTING_COST_ID="cost",AFE_ID="recorded-afe",FIELD_ID="field",AMOUNT=12.875m,COST_TYPE="LABOR",COST_CATEGORY="recorded-category",COST_DATE=utc,DESCRIPTION="Actual source evidence"};
        var copy=JsonSerializer.Deserialize<ACCOUNTING_COST>(JsonSerializer.Serialize(row))!;
        Assert.Equal(row.AMOUNT,copy.AMOUNT);Assert.Equal(utc,copy.COST_DATE);Assert.Equal(DateTimeKind.Utc,copy.COST_DATE!.Value.Kind);Assert.Equal(row.AFE_ID,copy.AFE_ID);
        Assert.Null(copy.FINANCE_ID);Assert.DoesNotContain("WORK_ORDER_ID",JsonSerializer.Serialize(copy));
    }
}
#endif
