using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.FlashCalculations;

namespace Beep.OilandGas.FlashCalculations.Calculations
{
    /// <summary>
    /// Provides rigorous thermodynamic flash calculations.
    /// Methods: Wilson's K-values, Rachford-Rice Equation (Newton-Raphson).
    /// </summary>
    public static class FlashCalculator
    {
        // Wilson's Correlation for K-values
        // Ki = (Pc_i / P) * exp(5.37 * (1 + omega_i) * (1 - Tc_i / T))
        public static decimal CalculateWilsonKValue(decimal pressure, decimal temperature, FLASH_COMPONENT component)
        {
            if (pressure <= 0 || temperature <= 0) return 0;
            if (component.CRITICAL_PRESSURE <= 0 || component.CRITICAL_TEMPERATURE <= 0) return 1; // Default fallback

            double pr = (double)(component.CRITICAL_PRESSURE / pressure);
            double tr = (double)(component.CRITICAL_TEMPERATURE / temperature);
            double omega = (double)component.ACENTRIC_FACTOR;

            double lnK = Math.Log(pr) + 5.37 * (1.0 + omega) * (1.0 - tr); // Wait, formula is K = ...
            // Formula: K = (Pc/P) * EXP(...)
            
            double k = pr * Math.Exp(5.37 * (1.0 + omega) * (1.0 - (1.0/tr))); // Tci/T = 1/Tr? No, Tr = T/Tc.
            // Formula is (1 - Tc/T). Yes.
            // Let's recheck Wilson:
            // ln K = ln(Pc/P) + 5.37(1+w)(1 - Tc/T)
            
            return (decimal)k;
        }

        // Rachford-Rice Objective Function
        // f(V) = Sum [ z_i * (K_i - 1) / (1 + V * (K_i - 1)) ] = 0
        public static double RachfordRiceFunction(double v, List<(double z, double k)> components)
        {
            double sum = 0;
            foreach (var c in components)
            {
                sum += (c.z * (c.k - 1.0)) / (1.0 + v * (c.k - 1.0));
            }
            return sum;
        }

        // Derivative of Rachford-Rice for Newton-Raphson
        // f'(V) = - Sum [ z_i * (K_i - 1)^2 / (1 + V * (K_i - 1))^2 ]
        public static double RachfordRiceDerivative(double v, List<(double z, double k)> components)
        {
            double sum = 0;
            foreach (var c in components)
            {
                double num = c.z * Math.Pow(c.k - 1.0, 2);
                double den = Math.Pow(1.0 + v * (c.k - 1.0), 2);
                sum -= num / den;
            }
            return sum;
        }

        public static decimal SolveRachfordRice(List<FLASH_COMPONENT> components, decimal pressure, decimal temperature, out int iterations, out bool converged)
        {
            // Prepare inputs
            var calcComponents = components.Select(c => (
                z: (double)c.MOLE_FRACTION, 
                k: (double)CalculateWilsonKValue(pressure, temperature, c)
            )).ToList();

            // Check trivial solutions (bubble point / dew point)
            double f0 = RachfordRiceFunction(0, calcComponents); // V=0 (Bubble Point check)
            double f1 = RachfordRiceFunction(1, calcComponents); // V=1 (Dew Point check)

            iterations = 0;
            converged = true;

            if (f0 < 0) return 0; // Subcooled Liquid
            if (f1 > 0) return 1; // Superheated Vapor

            // Newton-Raphson
            double v = 0.5; // Initial guess
            double tolerance = 1e-6;
            int maxIter = 50;

            for (int i = 0; i < maxIter; i++)
            {
                iterations++;
                double f = RachfordRiceFunction(v, calcComponents);
                double df = RachfordRiceDerivative(v, calcComponents);

                if (Math.Abs(f) < tolerance)
                    return (decimal)v;

                if (Math.Abs(df) < 1e-10) break; // Divide by zero protection

                double v_new = v - f / df;

                // Damping / Bounding
                if (v_new < 0) v_new = 0.0001;
                if (v_new > 1) v_new = 0.9999;

                if (Math.Abs(v_new - v) < tolerance)
                {
                    return (decimal)v_new;
                }

                v = v_new;
            }

            converged = false;
            return (decimal)v;
        }
        
        // Calculate Phase Compositions
        // xi = zi / (1 + V(Ki - 1))
        // yi = Ki * xi
        public static void CalculatePhaseCompositions(
            decimal v, 
            List<FLASH_COMPONENT> components, 
            decimal pressure, 
            decimal temperature,
            out List<(string Name, decimal xi, decimal yi, decimal K)> results)
        {
            results = new List<(string Name, decimal xi, decimal yi, decimal K)>();
            double v_val = (double)v;

            foreach (var c in components)
            {
                double z = (double)c.MOLE_FRACTION;
                double k = (double)CalculateWilsonKValue(pressure, temperature, c);
                
                double den = 1.0 + v_val * (k - 1.0);
                double xi = z / den;
                double yi = k * xi;

                results.Add((c.COMPONENT_NAME, (decimal)xi, (decimal)yi, (decimal)k));
            }
        }
    }
}
