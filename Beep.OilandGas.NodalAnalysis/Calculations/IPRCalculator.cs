using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.NodalAnalysis.Calculations
{
    /// <summary>
    /// Provides IPR (Inflow Performance Relationship) calculation methods.
    /// </summary>
    public static class IPRCalculator
    {
        /// <summary>
        /// Generates IPR curve using Vogel method for solution gas drive reservoirs.
        /// </summary>
        public static List<IPRPoint> GenerateVogelIPR(ReservoirProperties reservoir, double maxFlowRate = 5000, int points = 50)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (reservoir.ReservoirPressure <= 0)
                throw new ArgumentException("Reservoir pressure must be positive.", nameof(reservoir));

            var ipr = new List<IPRPoint>();

            // Vogel equation: q/q_max = 1 - 0.2 * (Pwf/Pr) - 0.8 * (Pwf/Pr)²
            // Rearranged: Pwf = Pr * [1 - 0.2 * (q/q_max) - 0.8 * (q/q_max)²] / [1 - 0.2 - 0.8]
            // Actually: q = q_max * [1 - 0.2 * (Pwf/Pr) - 0.8 * (Pwf/Pr)²]

            // Calculate maximum flow rate (at Pwf = 0)
            double qMax = reservoir.ProductivityIndex * reservoir.ReservoirPressure / 1.8;

            for (int i = 0; i <= points; i++)
            {
                double qRatio = (double)i / points;
                double flowRate = qRatio * maxFlowRate;

                if (flowRate > qMax)
                    flowRate = qMax;

                // Solve Vogel equation for Pwf
                double pwfRatio = flowRate / qMax;
                double pwf = reservoir.ReservoirPressure * 
                    (1.0 - 0.2 * pwfRatio - 0.8 * pwfRatio * pwfRatio);

                if (pwf < 0)
                    pwf = 0;

                ipr.Add(new IPRPoint(flowRate, pwf));
            }

            return ipr;
        }

        /// <summary>
        /// Generates IPR curve using Fetkovich method for multi-point IPR.
        /// </summary>
        public static List<IPRPoint> GenerateFetkovichIPR(ReservoirProperties reservoir, 
            List<(double flowRate, double pressure)> testPoints, double maxFlowRate = 5000, int points = 50)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (testPoints == null || testPoints.Count < 2)
                throw new ArgumentException("At least 2 test points are required for Fetkovich method.", nameof(testPoints));

            var ipr = new List<IPRPoint>();

            // Fetkovich: q = C * (Pr² - Pwf²)^n
            // Fit to test points to find C and n
            var sortedPoints = testPoints.OrderByDescending(p => p.pressure).ToList();

            // Use two points to estimate C and n
            var p1 = sortedPoints[0];
            var p2 = sortedPoints[sortedPoints.Count - 1];

            double pr2 = reservoir.ReservoirPressure * reservoir.ReservoirPressure;
            double pwf1_2 = p1.pressure * p1.pressure;
            double pwf2_2 = p2.pressure * p2.pressure;

            double term1 = Math.Log(p1.flowRate / p2.flowRate);
            double term2 = Math.Log((pr2 - pwf1_2) / (pr2 - pwf2_2));

            if (Math.Abs(term2) < 1e-10)
                throw new ArgumentException("Invalid test points for Fetkovich method.", nameof(testPoints));

            double n = term1 / term2;
            double c = p1.flowRate / Math.Pow(pr2 - pwf1_2, n);

            // Generate IPR curve
            for (int i = 0; i <= points; i++)
            {
                double flowRate = (double)i / points * maxFlowRate;
                double pwf2 = pr2 - Math.Pow(flowRate / c, 1.0 / n);
                double pwf = Math.Sqrt(Math.Max(0, pwf2));

                if (pwf > reservoir.ReservoirPressure)
                    pwf = reservoir.ReservoirPressure;

                ipr.Add(new IPRPoint(flowRate, pwf));
            }

            return ipr;
        }

        /// <summary>
        /// Generates IPR curve using Wiggins method for three-phase flow.
        /// </summary>
        public static List<IPRPoint> GenerateWigginsIPR(ReservoirProperties reservoir, double maxFlowRate = 5000, int points = 50)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            var ipr = new List<IPRPoint>();

            // Wiggins method accounts for water cut
            double waterCut = reservoir.WaterCut;
            double oilCut = 1.0 - waterCut;

            // Modified Vogel for three-phase
            double qMax = reservoir.ProductivityIndex * reservoir.ReservoirPressure / 1.8;

            for (int i = 0; i <= points; i++)
            {
                double qRatio = (double)i / points;
                double flowRate = qRatio * maxFlowRate;

                if (flowRate > qMax)
                    flowRate = qMax;

                // Wiggins: accounts for water cut in the IPR
                double pwfRatio = flowRate / qMax;
                double pwf = reservoir.ReservoirPressure *
                    (1.0 - 0.2 * pwfRatio - 0.8 * pwfRatio * pwfRatio);

                // Adjust for water cut
                pwf = pwf * (oilCut + waterCut * 0.8); // Water has different flow characteristics

                if (pwf < 0)
                    pwf = 0;

                ipr.Add(new IPRPoint(flowRate, pwf));
            }

            return ipr;
        }

        /// <summary>
        /// Generates gas well IPR using backpressure equation.
        /// </summary>
        public static List<IPRPoint> GenerateGasWellIPR(double reservoirPressure, double aof, 
            double backpressureExponent = 0.5, double maxFlowRate = 10000, int points = 50)
        {
            var ipr = new List<IPRPoint>();

            // Backpressure equation: q = C * (Pr² - Pwf²)^n
            // AOF is flow rate at Pwf = 0
            double c = aof / Math.Pow(reservoirPressure * reservoirPressure, backpressureExponent);

            for (int i = 0; i <= points; i++)
            {
                double flowRate = (double)i / points * maxFlowRate;

                if (flowRate > aof)
                    flowRate = aof;

                double pr2 = reservoirPressure * reservoirPressure;
                double pwf2 = pr2 - Math.Pow(flowRate / c, 1.0 / backpressureExponent);
                double pwf = Math.Sqrt(Math.Max(0, pwf2));

                ipr.Add(new IPRPoint(flowRate, pwf));
            }

            return ipr;
        }

        /// <summary>
        /// Generates IPR curve with specified parameters (wrapper for DataFlowService).
        /// </summary>
        public static List<IPRPoint> GenerateIPRCurve(
            decimal reservoirPressure,
            decimal bubblePointPressure,
            decimal productivityIndex,
            decimal waterCut,
            decimal gasOilRatio,
            decimal oilGravity,
            decimal formationVolumeFactor,
            decimal oilViscosity,
            int points = 20)
        {
            // Create reservoir properties from parameters
            var reservoir = new ReservoirProperties
            {
                ReservoirPressure = (double)reservoirPressure,
                BubblePointPressure = (double)bubblePointPressure,
                ProductivityIndex = (double)productivityIndex,
                WaterCut = (double)waterCut,
                GasOilRatio = (double)gasOilRatio,
                OilGravity = (double)oilGravity,
                FormationVolumeFactor = (double)formationVolumeFactor,
                OilViscosity = (double)oilViscosity
            };

            // Use Vogel method as default
            return GenerateVogelIPR(reservoir, maxFlowRate: 5000, points: points);
        }

        /// <summary>
        /// Generates IPR curve using the Standing method (Vogel with standing correction for
        /// damaged or stimulated wells). Standing introduced a flow efficiency term (FE) to
        /// shift the Vogel curve for wells with skin damage or stimulation.
        /// Reference: Standing, M.B. (1971) - "Concerning the Calculation of Inflow Performance
        /// of Wells Producing Solution Gas Drive Reservoirs."
        /// </summary>
        /// <param name="reservoir">Reservoir properties.</param>
        /// <param name="flowEfficiency">Flow efficiency (FE). 1.0 = undamaged; &lt;1 = damaged; &gt;1 = stimulated.</param>
        /// <param name="maxFlowRate">Maximum flow rate for curve generation (STB/day).</param>
        /// <param name="points">Number of curve points.</param>
        public static List<IPRPoint> GenerateStandingIPR(ReservoirProperties reservoir,
            double flowEfficiency = 1.0, double maxFlowRate = 5000, int points = 50)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));
            if (reservoir.ReservoirPressure <= 0)
                throw new ArgumentException("Reservoir pressure must be positive.", nameof(reservoir));
            if (flowEfficiency <= 0)
                throw new ArgumentException("Flow efficiency must be positive.", nameof(flowEfficiency));

            var ipr = new List<IPRPoint>();

            // Maximum flow rate using Standing's modified Vogel
            // q_max = J * Pr / (1.8 * FE) where J = PI
            double qMax = reservoir.ProductivityIndex * reservoir.ReservoirPressure / (1.8 * flowEfficiency);

            for (int i = 0; i <= points; i++)
            {
                double pwf = (double)i / points * reservoir.ReservoirPressure;

                // Standing's flow efficiency correction to Vogel:
                // q/q_max = 1 - 0.2*(FE * Pwf/Pr) - 0.8*(FE * Pwf/Pr)²
                double x = flowEfficiency * (pwf / reservoir.ReservoirPressure);
                double qRatio = 1.0 - 0.2 * x - 0.8 * x * x;
                if (qRatio < 0) qRatio = 0;

                double flowRate = qMax * qRatio;
                ipr.Add(new IPRPoint(flowRate, pwf));
            }

            // Sort by flowing pressure descending (high Pwf → zero production) for a conventional IPR
            ipr.Sort((a, b) => b.FlowingBottomholePressure.CompareTo(a.FlowingBottomholePressure));
            return ipr;
        }

        /// <summary>
        /// Generates IPR curve using Jones-Blount-Glaze (JBG) method for turbulence-corrected
        /// inflow. The JBG method accounts for both Darcy (linear) and non-Darcy (turbulent)
        /// pressure losses: ΔP = a·q² + b·q, where a is the turbulence coefficient and b is
        /// the Darcy coefficient.
        /// Reference: Jones, L.G., Blount, E.M., Glaze, O.H. (1976) - "Use of Short Term
        /// Multiple Rate Flow Tests To Predict Performance of Wells Having Turbulence."
        /// </summary>
        /// <param name="reservoir">Reservoir properties.</param>
        /// <param name="turbulenceCoefficient">Non-Darcy turbulence coefficient 'a' (psi²/cp / (Mscf/d)²).</param>
        /// <param name="darcyCoefficient">Darcy flow coefficient 'b' (psi²/cp / (Mscf/d)).</param>
        /// <param name="maxFlowRate">Maximum flow rate for curve generation (STB/day or Mscf/day).</param>
        /// <param name="points">Number of curve points.</param>
        public static List<IPRPoint> GenerateJonesBlountGlazeIPR(ReservoirProperties reservoir,
            double turbulenceCoefficient, double darcyCoefficient,
            double maxFlowRate = 5000, int points = 50)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));
            if (reservoir.ReservoirPressure <= 0)
                throw new ArgumentException("Reservoir pressure must be positive.", nameof(reservoir));
            if (darcyCoefficient <= 0)
                throw new ArgumentException("Darcy coefficient must be positive.", nameof(darcyCoefficient));

            var ipr = new List<IPRPoint>();

            // JBG model: ΔP = a·q² + b·q  →  Pwf² = Pr² - a·q² - b·q (gas form)
            // For oil wells (linearised): Pr - Pwf = a·q² + b·q
            double pr = reservoir.ReservoirPressure;

            for (int i = 0; i <= points; i++)
            {
                double flowRate = (double)i / points * maxFlowRate;

                // Pressure drawdown: ΔP = a·q² + b·q
                double deltaP = turbulenceCoefficient * flowRate * flowRate + darcyCoefficient * flowRate;
                double pwf = pr - deltaP;
                if (pwf < 0) pwf = 0;

                ipr.Add(new IPRPoint(flowRate, pwf));

                if (pwf <= 0) break; // No further production possible
            }

            return ipr;
        }

        /// <summary>
        /// Determines maximum flow rate (AOF for gas / q_max for oil) from two JBG test points.
        /// Fits 'a' and 'b' coefficients from two stabilised flow tests, then extrapolates to Pwf = 0.
        /// </summary>
        /// <param name="reservoirPressure">Static reservoir pressure (psia).</param>
        /// <param name="testPoint1">First stabilised test: (flowRate, bottomholePressure).</param>
        /// <param name="testPoint2">Second stabilised test: (flowRate, bottomholePressure).</param>
        /// <param name="turbulenceCoefficient">Fitted turbulence coefficient 'a' (output).</param>
        /// <param name="darcyCoefficient">Fitted Darcy coefficient 'b' (output).</param>
        /// <returns>Absolute Open Flow potential at Pwf = 0.</returns>
        public static double FitJonesBlountGlazeCoefficients(
            double reservoirPressure,
            (double flowRate, double bhp) testPoint1,
            (double flowRate, double bhp) testPoint2,
            out double turbulenceCoefficient,
            out double darcyCoefficient)
        {
            // ΔP/q = a·q + b  (linear form)
            double dp1 = reservoirPressure - testPoint1.bhp;
            double dp2 = reservoirPressure - testPoint2.bhp;

            double x1 = testPoint1.flowRate;
            double x2 = testPoint2.flowRate;
            double y1 = dp1 / x1;  // ΔP/q at test 1
            double y2 = dp2 / x2;  // ΔP/q at test 2

            // Fit y = a·q + b from two points
            turbulenceCoefficient = (y2 - y1) / (x2 - x1);  // slope = a
            darcyCoefficient = y1 - turbulenceCoefficient * x1;  // intercept = b

            if (darcyCoefficient <= 0)
                throw new InvalidOperationException("Darcy coefficient must be positive; check test data quality.");

            // AOF: solve a·q² + b·q - Pr = 0 (quadratic, take positive root)
            double discriminant = darcyCoefficient * darcyCoefficient + 4.0 * turbulenceCoefficient * reservoirPressure;
            if (discriminant < 0)
                throw new InvalidOperationException("Unable to compute AOF: discriminant is negative.");

            double aof = (-darcyCoefficient + Math.Sqrt(discriminant)) / (2.0 * turbulenceCoefficient);
            return aof;
        }

        /// <summary>
        /// Generates IPR curve for gas condensate wells using the LIT (Laminar-Inertia-Turbulence)
        /// pseudo-pressure method. Below dew point, condensate dropout reduces effective gas
        /// permeability; this method accounts for near-wellbore condensate banking via an
        /// additional skin-equivalent term derived from the condensate saturation build-up.
        /// Reference: Rawlins-Schellhardt extended for condensate systems.
        /// </summary>
        /// <param name="reservoirPressure">Initial reservoir pressure (psia).</param>
        /// <param name="dewPointPressure">Dew-point pressure of the gas condensate (psia).</param>
        /// <param name="aof">Absolute open flow potential at Pwf = 0 (Mscf/day).</param>
        /// <param name="backpressureExponent">Rawlins-Schellhardt exponent n (0.5–1.0).</param>
        /// <param name="condensateSkinFactor">Equivalent skin due to condensate banking (dimensionless).</param>
        /// <param name="maxFlowRate">Maximum flow rate for curve generation (Mscf/day).</param>
        /// <param name="points">Number of curve points.</param>
        public static List<IPRPoint> GenerateGasCondensateIPR(
            double reservoirPressure,
            double dewPointPressure,
            double aof,
            double backpressureExponent = 0.75,
            double condensateSkinFactor = 0.0,
            double maxFlowRate = 10000,
            int points = 50)
        {
            if (reservoirPressure <= 0)
                throw new ArgumentException("Reservoir pressure must be positive.", nameof(reservoirPressure));
            if (dewPointPressure <= 0 || dewPointPressure > reservoirPressure)
                throw new ArgumentException("Dew point pressure must be positive and ≤ reservoir pressure.", nameof(dewPointPressure));
            if (aof <= 0)
                throw new ArgumentException("AOF must be positive.", nameof(aof));
            if (backpressureExponent <= 0 || backpressureExponent > 1.0)
                throw new ArgumentException("Backpressure exponent must be between 0 and 1.", nameof(backpressureExponent));

            var ipr = new List<IPRPoint>();
            double pr2 = reservoirPressure * reservoirPressure;

            // Below dew point: condensate banking reduces deliverability.
            // Account for condensate skin via an effective flow reduction factor.
            double condensateDamageMultiplier = (condensateSkinFactor > 0)
                ? 1.0 / (1.0 + condensateSkinFactor / Math.Log(reservoirPressure / dewPointPressure + 1.0))
                : 1.0;

            for (int i = 0; i <= points; i++)
            {
                double flowRate = (double)i / points * maxFlowRate;
                if (flowRate > aof) flowRate = aof;

                // Rawlins-Schellhardt: q = AOF * ((Pr² - Pwf²) / Pr²)^n
                // Rearranged: (q / AOF)^(1/n) = (Pr² - Pwf²) / Pr²
                // Pwf² = Pr² * (1 - (q / AOF)^(1/n))
                double qRatio = flowRate / (aof * condensateDamageMultiplier);
                if (qRatio > 1.0) qRatio = 1.0;

                double pressureRatioTerm = 1.0 - Math.Pow(qRatio, 1.0 / backpressureExponent);
                if (pressureRatioTerm < 0) pressureRatioTerm = 0;

                double pwf2 = pr2 * pressureRatioTerm;
                double pwf = Math.Sqrt(Math.Max(0, pwf2));

                ipr.Add(new IPRPoint(flowRate, pwf));
            }

            ipr.Sort((a, b) => b.FlowingBottomholePressure.CompareTo(a.FlowingBottomholePressure));
            return ipr;
        }
    }
}

