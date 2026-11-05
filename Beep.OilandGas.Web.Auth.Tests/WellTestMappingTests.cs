using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WellTestMappingTests
{
    private static WELL_TEST Row()=>new(){UWI="well",SOURCE="source",TEST_TYPE="flow",RUN_NUM="run-A",TEST_NUM="test-A",OIL_FLOW_AMOUNT=12,GAS_FLOW_AMOUNT=0,WATER_FLOW_AMOUNT=4,OIL_FLOW_AMOUNT_UOM="m3/d",GAS_FLOW_AMOUNT_UOM="m3/d",WATER_FLOW_AMOUNT_UOM="m3/d",OIL_FLOW_AMOUNT_OUOM="original-oil",ROW_CREATED_BY="operator",ROW_CHANGED_BY="reviewer"};
    private static WellTestResponse Map(WELL_TEST row)=>new PPDMMappingService().ConvertPPDMModelToDTO<WellTestResponse,WELL_TEST>(row);
    [Fact] public void ExactRecordedUnitsAndAmountsArePreserved(){var mapped=Map(Row());Assert.Equal(12,mapped.OilFlowAmount);Assert.Equal(0,mapped.GasFlowAmount);Assert.Equal("m3/d",mapped.OilFlowAmountUom);Assert.Equal("m3/d",mapped.GasFlowAmountUom);Assert.Equal("m3/d",mapped.WaterFlowAmountUom);Assert.Equal("original-oil",mapped.OilFlowAmountOuom);Assert.Equal("operator",mapped.CreateUser);Assert.Equal("reviewer",mapped.UpdateUser);}
    [Fact] public void CompositeSourceIdentityAndAlphanumericRunArePreserved(){var mapped=Map(Row());Assert.Equal("source",mapped.Source);Assert.Equal("run-A",mapped.RunNumber);Assert.Equal("test-A",mapped.TestNumber);Assert.Equal(new[]{"well","source","flow","run-A","test-A"},System.Text.Json.JsonSerializer.Deserialize<string[]>(mapped.TestId));}
    [Theory][InlineData("well")][InlineData("source")][InlineData("type")][InlineData("run")][InlineData("test")]
    public void EveryCompositeKeyPartChangesIdentity(string part){var original=Map(Row()).TestId;var row=Row();switch(part){case "well":row.UWI="other";break;case "source":row.SOURCE="other";break;case "type":row.TEST_TYPE="other";break;case "run":row.RUN_NUM="other";break;default:row.TEST_NUM="other";break;}Assert.NotEqual(original,Map(row).TestId);}
    [Theory][InlineData("well")][InlineData("source")][InlineData("type")][InlineData("run")][InlineData("test")]
    public void MissingRecordedCompositePartCannotInventIdentity(string part){var row=Row();switch(part){case "well":row.UWI="";break;case "source":row.SOURCE="";break;case "type":row.TEST_TYPE="";break;case "run":row.RUN_NUM="";break;default:row.TEST_NUM="";break;}Assert.Throws<InvalidOperationException>(()=>Map(row));}
    [Fact] public void MissingUnitsAreNotAssumed(){var row=Row();row.OIL_FLOW_AMOUNT_UOM=null;row.OIL_FLOW_AMOUNT_OUOM=null;var mapped=Map(row);Assert.Null(mapped.OilFlowAmountUom);Assert.Null(mapped.OilFlowAmountOuom);Assert.Equal(12,mapped.OilFlowAmount);}
}
