using System.Linq;
using Beep.OilandGas.Models.Core.Interfaces;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class EconomicAnalysisInterfaceContractTests
{
    [Fact]
    public void CoreInterface_DoesNotExposeAdvancedAnalysisMembers()
    {
        var names = typeof(IEconomicAnalysisService).GetMethods().Select(m => m.Name).ToHashSet();

        Assert.DoesNotContain("PerformMonteCarloSimulationAsync", names);
        Assert.DoesNotContain("PerformRealOptionsAnalysisAsync", names);
        Assert.DoesNotContain("PerformDecisionTreeAnalysisAsync", names);
        Assert.DoesNotContain("PerformAfterTaxAnalysisAsync", names);
        Assert.DoesNotContain("CalculateEnterpriseValueAsync", names);
        Assert.DoesNotContain("PerformLeaseBuyAnalysisAsync", names);
        Assert.DoesNotContain("AnalyzeOptimalCapitalStructureAsync", names);
        Assert.DoesNotContain("AnalyzeCommodityPriceSensitivityAsync", names);
    }
}
