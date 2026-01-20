using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for pump operations (HydraulicPump, PlungerLift, SuckerRodPumping).
    /// </summary>
    public interface IPumpServiceClient
    {
        // Hydraulic Pump Operations
        Task<HydraulicPumpDesign> DesignHydraulicPumpSystemAsync(string wellUWI, string pumpType, decimal wellDepth, decimal desiredFlowRate);
        Task<PumpPerformanceAnalysis> AnalyzeHydraulicPumpPerformanceAsync(string pumpId);
        Task<bool> SaveHydraulicPumpDesignAsync(HydraulicPumpDesign design, string? userId = null);
        Task<List<PumpPerformanceHistory>> GetHydraulicPumpPerformanceHistoryAsync(string pumpId);

        // Plunger Lift Operations
        Task<PlungerLiftDesign> DesignPlungerLiftSystemAsync(string wellUWI, PlungerLiftWellProperties wellProperties);
        Task<PlungerLiftPerformance> AnalyzePlungerLiftPerformanceAsync(string wellUWI);
        Task<bool> SavePlungerLiftDesignAsync(PlungerLiftDesign design, string? userId = null);

        // Sucker Rod Pumping Operations
        Task<SuckerRodPumpDesign> DesignSuckerRodPumpSystemAsync(string wellUWI, SuckerRodPumpWellProperties wellProperties);
        Task<SuckerRodPumpPerformance> AnalyzeSuckerRodPumpPerformanceAsync(string pumpId);
        Task<bool> SaveSuckerRodPumpDesignAsync(SuckerRodPumpDesign design, string? userId = null);
    }
}

