using System.Linq;
using Beep.OilandGas.ProductionOperations.Services;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionOperationsInterfaceContractTests
{
    [Fact]
    public void CanonicalInterface_DoesNotExposeStagedMembers()
    {
        var canonicalMethods = typeof(IProductionOperationsService)
            .GetMethods()
            .Select(m => m.Name)
            .ToHashSet();

        Assert.DoesNotContain(nameof(IProductionOperationsAdvancedService.RecordOperationalCostsAsync), canonicalMethods);
        Assert.DoesNotContain(nameof(IProductionOperationsAdvancedService.GetOperationalCostsAsync), canonicalMethods);
        Assert.DoesNotContain(nameof(IProductionOperationsAdvancedService.CalculateCostAnalysisAsync), canonicalMethods);
        Assert.DoesNotContain(nameof(IProductionOperationsAdvancedService.GenerateOperationsReportAsync), canonicalMethods);
        Assert.DoesNotContain(nameof(IProductionOperationsAdvancedService.IdentifyOptimizationOpportunitiesAsync), canonicalMethods);
        Assert.DoesNotContain(nameof(IProductionOperationsAdvancedService.ImplementOptimizationAsync), canonicalMethods);
        Assert.DoesNotContain(nameof(IProductionOperationsAdvancedService.MonitorOptimizationEffectivenessAsync), canonicalMethods);
        Assert.DoesNotContain(nameof(IProductionOperationsAdvancedService.GetProductionOperationsSummaryAsync), canonicalMethods);
        Assert.DoesNotContain(nameof(IProductionOperationsAdvancedService.ExportOperationsDataAsync), canonicalMethods);
        Assert.DoesNotContain(nameof(IProductionOperationsAdvancedService.ValidateOperationsDataAsync), canonicalMethods);
    }
}
