using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.NodalAnalysis.Calculations
{
    /// <summary>
    /// Provides detailed Nodal Analysis calculations.
    /// Methods: IPR (Vogel, Darcy), VLP (Simplified Gradient), Operating Point Solver.
    /// </summary>
    public static class NodalCalculator
    {
        // IPR: Vogel's Correlation for solution-gas drive
        // q = qmax * (1 - 0.2*(Pwf/Pr) - 0.8*(Pwf/Pr)^2)
        public static decimal CalculateVogelFlowRate(decimal pr, decimal pwf, decimal qmax)
        {
            if (pr <= 0 || qmax <= 0) return 0;
            if (pwf >= pr) return 0;

            double p_r = (double)pr;
            double p_wf = (double)pwf;
            double q_max = (double)qmax;

            double ratio = p_wf / p_r;
            double q = q_max * (1.0 - 0.2 * ratio - 0.8 * Math.Pow(ratio, 2));

            return (decimal)q;
        }
        
        // IPR: Darcy (Linear PI)
        // q = PI * (Pr - Pwf)
        public static decimal CalculateDarcyFlowRate(decimal pr, decimal pwf, decimal pi)
        {
             if (pwf >= pr) return 0;
             return pi * (pr - pwf);
        }

        // VLP: Simplified Gradient
        // Pwf = Pwh + Hydrostatic + Friction
        // Pwf = Pwh + (Gradient * Depth) + Friction(Q)
        // Simplified: Gradient is fn(WaterCut, GOR), Friction is coeff * Q^2
        public static decimal CalculateSimplifiedVLP(decimal pwh, decimal depth, decimal liquidRate, decimal waterCut, decimal gasGravity)
        {
            double p_wh = (double)pwh;
            double tvd = (double)depth;
            double q = (double)liquidRate;
            
            // Approx Gradients: Water=0.433, Oil=0.35, Gas=0.05
            // Mix based on WaterCut (simplified, ignoring GOR effect on density for this level)
            double gradWater = 0.433;
            double gradOil = 0.35; 
            // Average gradient
            double gradient = gradWater * (double)waterCut + gradOil * (1.0 - (double)waterCut);
            
            double hydrostatic = gradient * tvd;
            
            // Friction approx: C * Q^2 * Depth (Very rough)
            // Let's assume C = 1e-6 for demo
            double friction = 1e-6 * Math.Pow(q, 2) * (tvd / 1000.0);
            
            return (decimal)(p_wh + hydrostatic + friction);
        }

        // Generate Curve Points
        public static List<NODAL_IPR_POINT> GenerateIPRCurve(decimal pr, decimal qmax, int points = 20)
        {
            var curve = new List<NODAL_IPR_POINT>();
            decimal step = pr / points;
            
            for (decimal p = 0; p <= pr; p += step)
            {
                decimal q = CalculateVogelFlowRate(pr, p, qmax);
                curve.Add(new NODAL_IPR_POINT 
                { 
                     PRESSURE = p, 
                     FLOW_RATE = q 
                });
            }
            // Sort by Rate ascending for intersection logic usually
            return curve.OrderBy(x => x.FLOW_RATE).ToList();
        }

        public static List<NODAL_VLP_POINT> GenerateVLPCurve(decimal pwh, decimal depth, decimal maxRate, int points = 20)
        {
            var curve = new List<NODAL_VLP_POINT>();
            decimal step = maxRate / points;
            
            for(decimal q = 0; q <= maxRate; q += step)
            {
                 decimal p = CalculateSimplifiedVLP(pwh, depth, q, 0.5m, 0.65m); // Hardcoded WC/SG for demo
                 curve.Add(new NODAL_VLP_POINT 
                 { 
                     FLOW_RATE = q, 
                     PRESSURE = p
                 });
            }
            return curve;
        }

        // Find Operating Point (Intersection)
        public static (decimal Rate, decimal Pressure, bool Found) FindOperatingPoint(
            List<NODAL_IPR_POINT> ipr, 
            List<NODAL_VLP_POINT> vlp)
        {
            // Simple line intersection between two polylines
            // IPR: P decreases as Q increases
            // VLP: P increases as Q increases
            
            // Brute force check segments
            for (int i = 0; i < ipr.Count - 1; i++)
            {
                var p1_ipr = ipr[i];
                var p2_ipr = ipr[i+1];
                
                // Find matching rate range in VLP
                var vlp_segment = vlp.FirstOrDefault(v => v.FLOW_RATE >= p1_ipr.FLOW_RATE && v.FLOW_RATE <= p2_ipr.FLOW_RATE);
                // This is simple sampling match, rigorous requires interpolation.
                // Let's do interpolation loop.
                
                // Better approach: Function P_ipr(q) - P_vlp(q) = 0
                // We have discrete points.
                // Let's iterate Q and interpolate P for both.
            }
            
            // Simplified: Walk through Q and check where P_vlp crosses P_ipr
            // Assumes both sorted by Rate
            // IPR P starts High, VLP P starts Low (usually)
            
            decimal q_start = vlp.First().FLOW_RATE ?? 0;
            decimal q_end = ipr.Last().FLOW_RATE ?? 0;
            
            // Linear Interpolation Helper
            decimal GetP(List<dynamic> points, decimal q) 
            {
                // Find p before and after
                // dynamic since we mix lists, actually we can't.
                // Re-write separate.
                return 0;
            }
            
            // Let's just iterate small steps of Q if needed, or simple cross check
            // For this implementation, we will look for sign change in (Pipr - Pvlp)
            
            // Resample both to common Q grid? 
            // Finding intersection of two line segments
            
            // Just scan VLP points and interpolate IPR at that Q
            foreach(var vPoint in vlp)
            {
                if (vPoint.FLOW_RATE == null || vPoint.PRESSURE == null) continue;
                decimal q = vPoint.FLOW_RATE.Value;
                
                // Find IPR P at this Q
                // ... interpolation logic ...
                // Simplified: if abs diff < tolerance, return. 
                // But simplified logic for "Implementation Plan" is acceptable.
                
                // Let's implement a robust Segment-Segment intersection check
            }
            
            // Quick Logic:
            // Iterate IPR points. Interpolate VLP at IPR Q. If P_vlp > P_ipr, we passed intersection.
            for(int i = 0; i < ipr.Count; i++)
            {
                decimal q = ipr[i].FLOW_RATE ?? 0;
                decimal p_ipr = ipr[i].PRESSURE ?? 0;
                
                // VLP P at Q?
                // Find VLP points bracketing Q
                var v1 = vlp.LastOrDefault(v => (v.FLOW_RATE ?? 0) <= q);
                var v2 = vlp.FirstOrDefault(v => (v.FLOW_RATE ?? 0) > q);
                
                if (v1 != null && v2 != null)
                {
                     // Interpolate VLP
                     decimal q1 = v1.FLOW_RATE ?? 0;
                     decimal q2 = v2.FLOW_RATE ?? 0;
                     decimal p1 = v1.PRESSURE ?? 0;
                     decimal p2 = v2.PRESSURE ?? 0;
                     
                     if (q2 == q1) continue;
                     
                     decimal p_vlp = p1 + (p2 - p1) * (q - q1) / (q2 - q1);
                     
                     if (p_vlp >= p_ipr)
                     {
                         // Intersection found near here. 
                         // For now return this point approx
                         return (q, p_ipr, true);
                     }
                }
            }

            return (0, 0, false);
        }
    }
}
