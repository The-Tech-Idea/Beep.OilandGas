using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.GasLift;

namespace Beep.OilandGas.GasLift.Calculations
{
    /// <summary>
    /// Dynamic gas lift valve behavior modeling.
    ///
    /// Gas lift valves are pressure-operated devices that open and close in response
    /// to the balance between tubing and casing pressures acting on the valve bellows
    /// or dome charge.
    ///
    /// Valve types covered:
    ///  1. <b>IPO (Injection Pressure Operated)</b> — casing pressure acts on the large
    ///     bellows area minus the port area; opens when P_casing × A_bellows > P_dome × A_bellows.
    ///  2. <b>PPO (Production Pressure Operated)</b> — tubing (production) pressure
    ///     acts on the bellows; less common, used in high-GOR wells.
    ///  3. <b>Pilot-operated</b> — small pilot valve opens first, which then opens a
    ///     large main valve; provides stable injection at large gas volumes.
    ///
    /// IPO valve force balance (Winkler-Blann 1992):
    ///   Open condition:  P_casing ≥ P_open = (P_dome × A_b − P_tubing × A_port) / (A_b − A_port)
    ///   Close condition: P_casing ≤ P_close = P_dome − P_tubing × (A_port / (A_b − A_port))
    ///
    /// Valve throughput (critical or subcritical gas flow through port):
    ///   Q_crit = 1.53 × A_port × P_upstream × √(k / (Z × T × MW))
    ///   (Thornhill-Craver equation, 1961 API RP 11V2)
    ///
    /// Valve hysteresis and dynamic closure:
    ///   P_close < P_open due to seating geometry (hysteresis band typically 20–50 psi).
    ///
    /// References:
    ///   - Winkler, H.W. and Blann, J.R. (1992) "Gas Lift", SPE Petroleum Engineering Handbook Vol. IV
    ///   - Thornhill-Craver (1961) "Design Equations and Tables for Gas Lift Valves" API RP 11V2
    ///   - Brown, K.E. (1967) "Gas Lift Theory and Practice"
    /// </summary>
    public static class DynamicValveBehavior
    {
        private const double R_UNIVERSAL = 10.73;  // psia·ft³/(lbmol·°R)

        // ─────────────────────────────────────────────────────────────────
        //  1. IPO valve opening / closing pressures
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the opening and closing pressures for an IPO (Injection Pressure
        /// Operated) gas lift valve.
        ///
        ///   P_open  = (P_dome × A_b − P_tubing × A_port) / (A_b − A_port)
        ///   P_close = P_dome × (A_b − A_port) / A_b  [tubing pressure = 0 approx.]
        ///
        /// Practical: P_close ≈ P_dome when tubing pressure contribution is small.
        /// </summary>
        /// <param name="valve">Valve configuration from model.</param>
        /// <param name="tubingPressurePsia">Current tubing pressure at valve depth (psia).</param>
        /// <param name="bellowsAreaIn2">Effective bellows area (in²). Typical 0.77–1.0 in² for 1.5" OD valve.</param>
        /// <param name="portAreaIn2">Port (seat) area (in²). Computed from PORT_SIZE if not supplied.</param>
        /// <returns><see cref="ValvePressureResult"/>.</returns>
        public static ValvePressureResult CalculateIpoValvePressures(
            GAS_LIFT_VALVE valve,
            double tubingPressurePsia,
            double bellowsAreaIn2 = 0.77,
            double portAreaIn2 = -1.0)
        {
            if (valve == null) throw new ArgumentNullException(nameof(valve));

            if (portAreaIn2 <= 0)
            {
                double portDiamIn = (double)valve.PORT_SIZE / 64.0;  // PORT_SIZE in 64ths of inch
                portAreaIn2 = Math.PI * portDiamIn * portDiamIn / 4.0;
            }

            double domePressure = (double)valve.OPENING_PRESSURE; // set to dome charge at 60°F
            double Ab = bellowsAreaIn2;
            double Ap = portAreaIn2;

            // Temperature correction for dome pressure (nitrogen-charged bellows)
            // P_dome_T = P_dome_60F × (T_valve + 460) / (60 + 460)
            double valveTempF = valve.TEMPERATURE > 0 ? (double)valve.TEMPERATURE : 150.0;
            double domePressureAtTemp = domePressure * (valveTempF + 459.67) / (60.0 + 459.67);

            // IPO force balance
            double pOpen  = (domePressureAtTemp * Ab - tubingPressurePsia * Ap) / (Ab - Ap);
            double pClose = domePressureAtTemp * (Ab - Ap) / Ab;

            // Hysteresis: closing pressure is lower than opening pressure
            double hystBand = pOpen - pClose;

            bool isOpen = tubingPressurePsia < pOpen;  // IPO: casing side

            return new ValvePressureResult
            {
                ValveId              = valve.GAS_LIFT_VALVE_ID,
                DepthFt              = (double)valve.DEPTH,
                DomePressurePsia     = domePressureAtTemp,
                OpeningPressurePsia  = pOpen,
                ClosingPressurePsia  = pClose,
                HysteresisBandPsi    = hystBand,
                TubingPressurePsia   = tubingPressurePsia,
                BellowsAreaIn2       = Ab,
                PortAreaIn2          = Ap,
                IsOpen               = isOpen,
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  2. Thornhill-Craver gas throughput
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Estimates gas injection rate through a gas lift valve port using the
        /// Thornhill-Craver (1961) equation.
        ///
        /// Critical (sonic) flow:
        ///   Q = 155.5 × C_d × A_port × P_upstream × √(k / (Z × T × G_g))  (Mscfd)
        ///
        /// Subcritical flow:
        ///   Q = Q_critical × √(1 − ((P_down/P_up)^((k+1)/k) − (P_down/P_up)^(2/k))
        ///                        / ((2k/(k+1))^(k/(k-1)) …))
        ///   Simplified using Perkins (1990) subcritical factor.
        /// </summary>
        /// <param name="portAreaIn2">Port area (in²).</param>
        /// <param name="upstreamPressurePsia">Upstream (casing) pressure (psia).</param>
        /// <param name="downstreamPressurePsia">Downstream (tubing) pressure (psia).</param>
        /// <param name="gasTemperatureF">Gas temperature at valve (°F).</param>
        /// <param name="gasSpecificGravity">Gas specific gravity (air=1).</param>
        /// <param name="zFactor">Gas compressibility factor Z (default 0.9).</param>
        /// <param name="k">Specific heat ratio Cp/Cv (default 1.28 for natural gas).</param>
        /// <param name="cd">Discharge coefficient (default 0.865).</param>
        /// <returns>Gas injection rate in Mscfd.</returns>
        public static ValveThroughputResult CalculateThornhillCraverThroughput(
            double portAreaIn2,
            double upstreamPressurePsia,
            double downstreamPressurePsia,
            double gasTemperatureF,
            double gasSpecificGravity = 0.65,
            double zFactor = 0.90,
            double k = 1.28,
            double cd = 0.865)
        {
            if (upstreamPressurePsia <= 0) throw new ArgumentOutOfRangeException(nameof(upstreamPressurePsia));

            double T_R    = gasTemperatureF + 459.67;
            double r      = downstreamPressurePsia / upstreamPressurePsia;
            double r_crit = Math.Pow(2.0 / (k + 1.0), k / (k - 1.0));  // critical pressure ratio

            bool isCritical = r <= r_crit;

            // Critical flow (Thornhill-Craver simplified)
            double qCritMscfd = 155.5 * cd * portAreaIn2 * upstreamPressurePsia
                                * Math.Sqrt(k / (zFactor * T_R * gasSpecificGravity));

            double qMscfd;
            double subcritFactor = 1.0;

            if (isCritical)
            {
                qMscfd = qCritMscfd;
            }
            else
            {
                // Subcritical correction factor (Perkins 1990 approximation)
                subcritFactor = Math.Sqrt((Math.Pow(r, 2.0 / k) - Math.Pow(r, (k + 1.0) / k))
                                          / (Math.Pow(r_crit, 2.0 / k) - Math.Pow(r_crit, (k + 1.0) / k)));
                subcritFactor = Math.Max(0, Math.Min(1, subcritFactor));
                qMscfd = qCritMscfd * subcritFactor;
            }

            return new ValveThroughputResult
            {
                GasInjectionRateMscfd = qMscfd,
                IsCriticalFlow        = isCritical,
                PressureRatio         = r,
                CriticalPressureRatio = r_crit,
                SubcriticalFactor     = subcritFactor,
                PortAreaIn2           = portAreaIn2,
                UpstreamPressurePsia  = upstreamPressurePsia,
                DownstreamPressurePsia = downstreamPressurePsia,
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  3. Pilot-operated valve behavior
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Models a pilot-operated valve as a two-stage device:
        ///  • Pilot stage: small IPO valve (opens main valve when casing pressure ≥ P_pilot)
        ///  • Main stage: large orifice (open/closed based on pilot state)
        ///
        /// The pilot valve has its own force-balance opening pressure; when triggered,
        /// the main port is fully open. This gives stable high-rate injection.
        /// </summary>
        /// <param name="pilotOpeningPressurePsia">Pressure at which pilot stage triggers (psia).</param>
        /// <param name="mainPortAreaIn2">Main stage port area (in²).</param>
        /// <param name="casingPressurePsia">Current annular casing pressure (psia).</param>
        /// <param name="tubingPressurePsia">Current tubing pressure at valve depth (psia).</param>
        /// <param name="gasTemperatureF">Gas temperature at valve (°F).</param>
        /// <param name="gasSpecificGravity">Gas SG (air=1).</param>
        public static PilotValveResult CalculatePilotValveBehavior(
            double pilotOpeningPressurePsia,
            double mainPortAreaIn2,
            double casingPressurePsia,
            double tubingPressurePsia,
            double gasTemperatureF = 150.0,
            double gasSpecificGravity = 0.65)
        {
            bool pilotOpen = casingPressurePsia >= pilotOpeningPressurePsia;

            double gasRateMscfd = 0;
            if (pilotOpen)
            {
                var throughput = CalculateThornhillCraverThroughput(
                    mainPortAreaIn2, casingPressurePsia, tubingPressurePsia,
                    gasTemperatureF, gasSpecificGravity);
                gasRateMscfd = throughput.GasInjectionRateMscfd;
            }

            return new PilotValveResult
            {
                IsPilotOpen              = pilotOpen,
                IsMainValveOpen          = pilotOpen,
                PilotOpeningPressurePsia = pilotOpeningPressurePsia,
                CasingPressurePsia       = casingPressurePsia,
                GasInjectionRateMscfd   = gasRateMscfd,
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  4. Full valve string simulation (top-down)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Simulates the behavior of all valves in a gas lift string given current
        /// operating conditions, determining which valves are open and total gas injection.
        /// </summary>
        /// <param name="valves">List of valves ordered top to bottom.</param>
        /// <param name="casingPressureAtSurfacePsia">Casing pressure measured at surface (psia).</param>
        /// <param name="casingPressureGradientPsiPerFt">Casing gas column pressure gradient (psi/ft).</param>
        /// <param name="tubingPressureAtSurfacePsia">Wellhead tubing pressure (psia).</param>
        /// <param name="tubingFluidGradientPsiPerFt">Tubing fluid gradient (psi/ft).</param>
        /// <param name="gasTemperatureF">Average valve temperature (°F).</param>
        public static ValveStringSimulationResult SimulateValveString(
            IEnumerable<GAS_LIFT_VALVE> valves,
            double casingPressureAtSurfacePsia,
            double casingPressureGradientPsiPerFt,
            double tubingPressureAtSurfacePsia,
            double tubingFluidGradientPsiPerFt,
            double gasTemperatureF = 150.0)
        {
            var valveResults = new List<ValveStateSnapshot>();
            double totalInjectionMscfd = 0;

            foreach (var v in valves ?? Array.Empty<GAS_LIFT_VALVE>())
            {
                double depthFt = (double)v.DEPTH;
                double casingAtDepth = casingPressureAtSurfacePsia + casingPressureGradientPsiPerFt * depthFt;
                double tubingAtDepth = tubingPressureAtSurfacePsia + tubingFluidGradientPsiPerFt * depthFt;

                var pressureResult = CalculateIpoValvePressures(v, tubingAtDepth);
                bool isOpen = casingAtDepth >= pressureResult.OpeningPressurePsia;

                double injectionMscfd = 0;
                if (isOpen)
                {
                    double portDiamIn = (double)v.PORT_SIZE / 64.0;
                    double portArea   = Math.PI * portDiamIn * portDiamIn / 4.0;
                    var throughput = CalculateThornhillCraverThroughput(
                        portArea, casingAtDepth, tubingAtDepth, gasTemperatureF);
                    injectionMscfd = throughput.GasInjectionRateMscfd;
                    totalInjectionMscfd += injectionMscfd;
                }

                valveResults.Add(new ValveStateSnapshot
                {
                    ValveId               = v.GAS_LIFT_VALVE_ID,
                    DepthFt               = depthFt,
                    CasingPressurePsia    = casingAtDepth,
                    TubingPressurePsia    = tubingAtDepth,
                    OpeningPressurePsia   = pressureResult.OpeningPressurePsia,
                    ClosingPressurePsia   = pressureResult.ClosingPressurePsia,
                    IsOpen                = isOpen,
                    GasInjectionRateMscfd = injectionMscfd,
                });
            }

            return new ValveStringSimulationResult
            {
                ValveStates              = valveResults,
                TotalGasInjectionMscfd   = totalInjectionMscfd,
                NumberOfOpenValves       = valveResults.FindAll(vs => vs.IsOpen).Count,
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>IPO valve opening/closing pressure result.</summary>
        public sealed class ValvePressureResult
        {
            public string ValveId             { get; set; } = string.Empty;
            public double DepthFt             { get; set; }
            public double DomePressurePsia    { get; set; }
            public double OpeningPressurePsia  { get; set; }
            public double ClosingPressurePsia  { get; set; }
            public double HysteresisBandPsi   { get; set; }
            public double TubingPressurePsia  { get; set; }
            public double BellowsAreaIn2      { get; set; }
            public double PortAreaIn2         { get; set; }
            public bool   IsOpen              { get; set; }
        }

        /// <summary>Thornhill-Craver gas throughput result.</summary>
        public sealed class ValveThroughputResult
        {
            public double GasInjectionRateMscfd  { get; set; }
            public bool   IsCriticalFlow          { get; set; }
            public double PressureRatio           { get; set; }
            public double CriticalPressureRatio   { get; set; }
            public double SubcriticalFactor       { get; set; }
            public double PortAreaIn2             { get; set; }
            public double UpstreamPressurePsia    { get; set; }
            public double DownstreamPressurePsia  { get; set; }
        }

        /// <summary>Pilot-operated valve state and flow result.</summary>
        public sealed class PilotValveResult
        {
            public bool   IsPilotOpen              { get; set; }
            public bool   IsMainValveOpen           { get; set; }
            public double PilotOpeningPressurePsia  { get; set; }
            public double CasingPressurePsia        { get; set; }
            public double GasInjectionRateMscfd    { get; set; }
        }

        /// <summary>State snapshot for one valve in the string.</summary>
        public sealed class ValveStateSnapshot
        {
            public string ValveId                { get; set; } = string.Empty;
            public double DepthFt                { get; set; }
            public double CasingPressurePsia     { get; set; }
            public double TubingPressurePsia     { get; set; }
            public double OpeningPressurePsia     { get; set; }
            public double ClosingPressurePsia     { get; set; }
            public bool   IsOpen                  { get; set; }
            public double GasInjectionRateMscfd  { get; set; }
        }

        /// <summary>Full valve string simulation result.</summary>
        public sealed class ValveStringSimulationResult
        {
            public List<ValveStateSnapshot> ValveStates          { get; set; } = new();
            public double TotalGasInjectionMscfd                 { get; set; }
            public int    NumberOfOpenValves                     { get; set; }
        }
    }
}
