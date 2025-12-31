using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for pump operations (HydraulicPump, PlungerLift, SuckerRodPumping).
    /// </summary>
    public interface IPumpServiceClient
    {
        // Hydraulic Pump Operations
        Task<HydraulicPumpDesignDto> DesignHydraulicPumpSystemAsync(string wellUWI, string pumpType, decimal wellDepth, decimal desiredFlowRate);
        Task<PumpPerformanceAnalysisDto> AnalyzeHydraulicPumpPerformanceAsync(string pumpId);
        Task<bool> SaveHydraulicPumpDesignAsync(HydraulicPumpDesignDto design, string? userId = null);
        Task<List<PumpPerformanceHistoryDto>> GetHydraulicPumpPerformanceHistoryAsync(string pumpId);

        // Plunger Lift Operations
        Task<PlungerLiftDesignDto> DesignPlungerLiftSystemAsync(string wellUWI, PlungerLiftWellPropertiesDto wellProperties);
        Task<PlungerLiftPerformanceDto> AnalyzePlungerLiftPerformanceAsync(string wellUWI);
        Task<bool> SavePlungerLiftDesignAsync(PlungerLiftDesignDto design, string? userId = null);

        // Sucker Rod Pumping Operations
        Task<SuckerRodPumpDesignDto> DesignSuckerRodPumpSystemAsync(string wellUWI, SuckerRodPumpWellPropertiesDto wellProperties);
        Task<SuckerRodPumpPerformanceDto> AnalyzeSuckerRodPumpPerformanceAsync(string pumpId);
        Task<bool> SaveSuckerRodPumpDesignAsync(SuckerRodPumpDesignDto design, string? userId = null);
    }
}

