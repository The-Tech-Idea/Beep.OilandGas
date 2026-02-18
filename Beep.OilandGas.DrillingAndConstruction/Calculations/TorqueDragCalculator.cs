using System;
using System.Collections.Generic;

namespace Beep.OilandGas.DrillingAndConstruction.Calculations
{
    /// <summary>
    /// Provides Torque and Drag calculations for drill strings.
    /// </summary>
    public static class TorqueDragCalculator
    {
        // Inputs: 
        // Survey data (MD, Inc, Azi) -> Calculate Dogleg Severity
        // String data (Weight, OD, ID)
        // Friction Factors (Cased, Open Hole)
        // Operations: RIH, POOH, Rotate
        
        /// <summary>
        /// Calculates Tension and Torque at surface.
        /// Simplified 'Soft String' model (Johancsik et al).
        /// </summary>
        public static TORQUE_DRAG_RESULT CalculateSurfaceLoads(
            List<SURVEY_POINT> trajectory,
            DRILL_STRING_COMPONENT stringComp,
            double frictionFactor,
            double mudDensity,
            string operation) // "RIH", "POOH", "ROTATE"
        {
            if (trajectory == null || trajectory.Count < 2) return new TORQUE_DRAG_RESULT();

            double tension = 0; // Start at bottom (WOB usually subtracted, here assuming 0 WOB for free hanging)
            double torque = 0;
            
            // Buoyancy Factor
            double bf = 1.0 - (mudDensity / 65.5); // Steel density approx
            double buoyedWeightPerFt = stringComp.WEIGHT_PER_FT * bf;
            
            // Integrate from bottom up
            for (int i = trajectory.Count - 2; i >= 0; i--)
            {
                var p1 = trajectory[i];
                var p2 = trajectory[i+1];
                
                double dMD = p2.MD - p1.MD;
                double avgInc = (p1.INC + p2.INC) / 2.0 * Math.PI / 180.0;
                double avgAzi = (p1.AZI + p2.AZI) / 2.0 * Math.PI / 180.0;
                
                // Dogleg Calculation (Simple approx for radius of curvature)
                // DL = acos[ cos(I1-I2) - sin(I1)sin(I2)(1-cos(A2-A1)) ]
                double i1_rad = p1.INC * Math.PI / 180.0;
                double i2_rad = p2.INC * Math.PI / 180.0;
                double a1_rad = p1.AZI * Math.PI / 180.0;
                double a2_rad = p2.AZI * Math.PI / 180.0;
                
                double doglegAngle = Math.Acos(
                    Math.Cos(i2_rad - i1_rad) - 
                    Math.Sin(i1_rad) * Math.Sin(i2_rad) * (1.0 - Math.Cos(a2_rad - a1_rad))
                );
                
                if (double.IsNaN(doglegAngle)) doglegAngle = 0;
                if (Math.Abs(dMD) < 0.001) continue;

                // Normal Force
                // Fn = sqrt( (F_tension * d_alpha)^2 + (F_tension * sin(avgInc) * d_azi)^2 ) ... standard soft string
                // Using Johancsik approx: Fn = sqrt( (F * DL)^2 + (W * sin(I))^2 ) ?? 
                // Standard formulation:
                // Fn = [ (T * d_theta)^2 + (T * sin(theta) * d_phi)^2 ]^0.5 
                // where d_theta is change in inc, d_phi change in azi. 
                // Let's use simpler explicit integration for small segments.
                // Weight component along hole: W * cos(inc)
                // Weight component normal: W * sin(inc)
                
                double segmentWeight = buoyedWeightPerFt * dMD;
                double w_axial = segmentWeight * Math.Cos(avgInc);
                double w_normal = segmentWeight * Math.Sin(avgInc); // Contribution to normal force from weight
                
                // Total Normal Force considering tension
                // Fn_net = sqrt( (tension * doglegAngle)^2 + (w_normal)^2 ) ? Very approximate.
                // Use Standard Soft String Eq:
                // N = sqrt( (T * dInc * sin(Inc) )^2 + (T * dInc )^2 ) ... NO.
                
                // N = sqrt( (T * dInc)^2 + (T * sin(Inc) * dAzi)^2 ) -- this is Tension contribution due to curvature
                // Add Weight component: N_total = N_tension + W * sin(Inc) (approx vector sum)
                
                double curvatureForce = tension * doglegAngle; 
                // Taking worst case vector sum
                double normalForce = Math.Sqrt(Math.Pow(curvatureForce, 2) + Math.Pow(w_normal, 2));

                double frictionForce = frictionFactor * normalForce;
                
                // Update Tension based on operation
                if (operation == "RIH")
                {
                    // Slack off: Tension decreases due to friction, increases due to weight
                    // T_top = T_bot + W_axial - Friction
                    tension = tension + w_axial - frictionForce;
                }
                else if (operation == "POOH")
                {
                    // Pick up: Tension increases due to friction and weight
                    // T_top = T_bot + W_axial + Friction
                    tension = tension + w_axial + frictionForce;
                }
                else // ROTATE
                {
                    // Axial friction is zero (vector mostly tangential)
                    // T_top = T_bot + W_axial
                    tension = tension + w_axial;
                    
                    // Torque = Torque + Friction * Radius
                    double radius = stringComp.OD_INCH / 2.0 / 12.0; // ft
                    torque += frictionForce * radius;
                }
            }
            
            return new TORQUE_DRAG_RESULT
            {
                SURFACE_TENSION = tension,
                SURFACE_TORQUE = torque,
                PICK_UP_WEIGHT = operation == "POOH" ? tension : 0,
                SLACK_OFF_WEIGHT = operation == "RIH" ? tension : 0,
                ROTATING_WEIGHT = operation == "ROTATE" ? tension : 0
            };
        }
    }

    public class SURVEY_POINT
    {
        public double MD { get; set; }
        public double INC { get; set; }
        public double AZI { get; set; }
    }

    public class DRILL_STRING_COMPONENT
    {
        public double OD_INCH { get; set; }
        public double ID_INCH { get; set; }
        public double WEIGHT_PER_FT { get; set; }
    }

    public class TORQUE_DRAG_RESULT
    {
        public double SURFACE_TENSION { get; set; }
        public double SURFACE_TORQUE { get; set; }
        public double PICK_UP_WEIGHT { get; set; }
        public double SLACK_OFF_WEIGHT { get; set; }
        public double ROTATING_WEIGHT { get; set; }
    }
}
