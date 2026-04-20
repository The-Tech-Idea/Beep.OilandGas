using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionOperations;

namespace Beep.OilandGas.ProductionOperations.Services
{
    public partial class ProductionOperationsService
    {
        // Explicit implementations of Models.Core.Interfaces.IProductionOperationsService
        // Delegates to the public methods on this class which use live PPDM data.
        Task<List<ProductionData>> Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService.GetProductionDataAsync(
            string? wellUWI, string? fieldId, DateTime startDate, DateTime endDate)
        {
            return GetProductionDataAsync(wellUWI, fieldId, startDate, endDate);
        }

        Task Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService.RecordProductionDataAsync(
            ProductionData productionData, string userId)
        {
            return RecordProductionDataAsync(productionData, userId);
        }

        Task<List<ProductionOptimizationRecommendation>> Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService.OptimizeProductionAsync(
            string wellUWI, Dictionary<string, object> optimizationGoals)
        {
            return OptimizeProductionAsync(wellUWI, optimizationGoals);
        }
    }
}
