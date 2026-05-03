using System;

namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Canonical relative URLs for the dedicated nodal analysis API (<see cref="Prefix"/>; controller name <c>NodalAnalysis</c>).
    /// Legacy calculation-aggregator nodal lives under <c>/api/Calculations/nodal</c>; prefer these routes for new clients.
    /// ASP.NET Core matches routes case-insensitively; prefix is lowercase for consistent client composition.
    /// </summary>
    public static class NodalAnalysisHttpRoutes
    {
        public const string Prefix = "/api/nodalanalysis";

        public static string Analyze => $"{Prefix}/analyze";

        public static string Optimize => $"{Prefix}/optimize";

        public static string Result => $"{Prefix}/result";

        public static string History(string wellUwi) =>
            $"{Prefix}/history/{Uri.EscapeDataString(wellUwi ?? string.Empty)}";

        public static string PerformanceMatching => $"{Prefix}/performance-matching";

        public static string Sensitivity => $"{Prefix}/sensitivity";

        public static string ArtificialLift => $"{Prefix}/artificial-lift";

        public static string Diagnostics => $"{Prefix}/diagnostics";

        public static string ProductionForecast => $"{Prefix}/production-forecast";

        public static string PressureMaintenance => $"{Prefix}/pressure-maintenance";
    }
}
