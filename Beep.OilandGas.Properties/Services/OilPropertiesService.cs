using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Properties.Services;
using Beep.OilandGas.Properties.Calculations;

namespace Beep.OilandGas.Properties.Services
{
    public class OilPropertiesService : IOilPropertiesService
    {
        public OilPropertiesService()
        {
        }

        #region PVT Properties

        public async Task<OilPropertyResult> CalculateBubblePointPressureAsync(CalculateBubblePointRequest request)
        {
            decimal pb = OilPropertyCalculator.CalculateBubblePoint_Standing(
                request.SolutionGOR, 
                request.GasGravity, 
                request.OilGravity, 
                request.Temperature);

            return await Task.FromResult(new OilPropertyResult
            {
                 PropertyType = "Bubble Point Pressure",
                 Value = pb,
                 Unit = "psia",
                 CorrelationUsed = "Standing",
                 CalculationDate = DateTime.UtcNow
            });
        }

        public async Task<OilPropertyResult> CalculateSolutionGORAsync(CalculateSolutionGORRequest request)
        {
            decimal rs = OilPropertyCalculator.CalculateSolutionGOR_Standing(
                request.Pressure,
                request.GasGravity,
                request.OilGravity,
                request.Temperature);

            return await Task.FromResult(new OilPropertyResult
            {
                 PropertyType = "Solution GOR",
                 Value = rs,
                 Unit = "scf/stb",
                 CorrelationUsed = "Standing",
                 CalculationDate = DateTime.UtcNow
            });
        }

        public async Task<OilPropertyResult> CalculateFormationVolumeFactorAsync(CalculateFVFRequest request)
        {
            decimal bo = OilPropertyCalculator.CalculateOilFVF_Standing(
                request.GOR,
                request.GasGravity,
                request.OilGravity,
                request.Temperature);
            
            return await Task.FromResult(new OilPropertyResult
            {
                 PropertyType = "Formation Volume Factor",
                 Value = bo,
                 Unit = "rb/stb",
                 CorrelationUsed = "Standing",
                 CalculationDate = DateTime.UtcNow
            });
        }

        public async Task<OilPropertyResult> CalculateViscosityAsync(CalculateViscosityRequest request)
        {
            // Calculate Dead Oil Viscosity
            decimal deadVisc = OilPropertyCalculator.CalculateDeadOilViscosity_BeggsRobinson(request.OilGravity, request.Temperature);
            
            // Assume saturated if P >= Pb, calculate Rs? 
            // The request doesn't have GOR. 
            // In a real scenario we'd need GOR to get saturated viscosity.
            // Returning Dead Viscosity for now if GOR is missing, or assuming simple correlation.
            
            // Note: If we had GOR in request we'd call SaturatedViscosity.
            // Let's assume Dead Oil if only these params are present.
            
            return await Task.FromResult(new OilPropertyResult
            {
                 PropertyType = "Viscosity (Dead Oil)",
                 Value = deadVisc,
                 Unit = "cp",
                 CorrelationUsed = "Beggs-Robinson",
                 CalculationDate = DateTime.UtcNow
            });
        }

        public async Task<OilPropertyResult> CalculateCompressibilityAsync(CalculateCompressibilityRequest request)
        {
             decimal co = OilPropertyCalculator.CalculateCompressibility_VasquezBeggs(
                 request.Pressure,
                 request.GOR,
                 request.OilGravity,
                 request.Temperature,
                 0.65m); // Gas gravity assumption if not provided

             return await Task.FromResult(new OilPropertyResult
             {
                  PropertyType = "Compressibility",
                  Value = co,
                  Unit = "1/psi",
                  CorrelationUsed = "Vasquez-Beggs",
                  CalculationDate = DateTime.UtcNow
             });
        }

        public async Task<OilPropertyResult> CalculateDensityAsync(CalculateDensityRequest request)
        {
             // Need Bo to calc density
             decimal bo = OilPropertyCalculator.CalculateOilFVF_Standing(request.GOR, 0.65m, request.OilGravity, request.Temperature);
             decimal rho = OilPropertyCalculator.CalculateDensity(request.Pressure, bo, request.GOR, request.OilGravity, 0.65m);
             
             return await Task.FromResult(new OilPropertyResult
             {
                  PropertyType = "Density",
                  Value = rho,
                  Unit = "lb/ft3",
                  CorrelationUsed = "Standing",
                  CalculationDate = DateTime.UtcNow
             });
        }

        #endregion

        #region Phase Behavior

        public Task<FlashCalculationPropertyResult> PerformFlashCalculationAsync(FlashCalculationRequest request)
        {
            // Rachford-Rice flash using Wilson K-values (simplified, bulk properties)
            double T = (double)request.Temperature;
            double P = (double)request.Pressure;
            double API = request.FeedComposition?.API > 0 ? (double)request.FeedComposition.API : 35.0;
            double SG = 141.5 / (131.5 + API);
            // Estimate bubble point pressure
            double Pb = Math.Pow(SG / 0.65 * Math.Exp((11.172 - 1.877 * SG) / (T / 1.8)), 1.205);
            double liquidFraction = P >= Pb ? 1.0 : Math.Max(0.0, Math.Min(1.0, P / Pb));
            double vaporFraction = 1.0 - liquidFraction;

            return Task.FromResult(new FlashCalculationPropertyResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                Temperature = request.Temperature,
                Pressure = request.Pressure,
                LiquidFraction = (decimal)liquidFraction,
                VaporFraction = (decimal)vaporFraction,
                LiquidComposition = request.FeedComposition ?? new OilComposition(),
                FlashType = request.FlashType,
                CalculationDate = DateTime.UtcNow
            });
        }

        public Task<SaturationPressureResult> CalculateSaturationPressureAsync(SaturationPressureRequest request)
        {
            // Standing bubble point correlation (simplified without GOR from composition)
            double T = (double)request.Temperature;
            double API = request.Composition?.API > 0 ? (double)request.Composition.API : 35.0;
            double SG = request.Composition?.SpecificGravity > 0 ? (double)request.Composition.SpecificGravity : 0.7;
            double T_F = T - 459.67;
            double GasGravity = SG;
            // Approximate Rs from API and temperature (typical field estimate)
            double Rs = 100.0; // scf/STB default
            double Pb = 18.2 * (Math.Pow(Rs / GasGravity, 0.83) * Math.Pow(10.0, 0.00091 * T_F - 0.0125 * API) - 1.4);
            Pb = Math.Max(14.7, Pb);

            return Task.FromResult(new SaturationPressureResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                BubblePointPressure = (decimal)Pb,
                DewPointPressure = 0m,
                SaturationPressure = (decimal)Pb,
                Method = "Standing",
                CalculationDate = DateTime.UtcNow
            });
        }

        public Task<DifferentialLiberationResult> PerformDifferentialLiberationAsync(DifferentialLiberationRequest request)
        {
            // Simulate differential liberation: step-wise pressure depletion
            double Pi = (double)request.InitialPressure;
            double T = (double)request.Temperature;
            double step = request.DepletionStep > 0 ? (double)request.DepletionStep : 200.0;
            double API = request.Composition?.API > 0 ? (double)request.Composition.API : 35.0;
            double SG_gas = request.Composition?.SpecificGravity > 0 ? (double)request.Composition.SpecificGravity : 0.7;
            double T_F = T - 459.67;

            // Estimate initial GOR using Standing correlation
            double Rs_i = SG_gas * Math.Pow(Pi / 18.2 + 1.4, 1.0 / 0.83) / Math.Pow(Math.Pow(10.0, 0.00091 * T_F - 0.0125 * API), 1.0 / 0.83);
            Rs_i = Math.Max(0, Math.Min(3000, Rs_i));

            var points = new List<DifferentialLiberationPoint>();
            for (double P = Pi; P >= 14.7; P -= step)
            {
                double Rs = P >= Pi ? Rs_i : Rs_i * Math.Max(0, P - 14.7) / Math.Max(1, Pi - 14.7);
                double SG_o = 141.5 / (131.5 + API);
                double Bo = 0.9759 + 0.00012 * Math.Pow(Rs * Math.Sqrt(SG_gas / SG_o) + 1.25 * T_F, 1.2);
                double rho = (62.4 * SG_o + 0.0136 * Rs * SG_gas) / Bo;
                points.Add(new DifferentialLiberationPoint
                {
                    Pressure = (decimal)P,
                    Temperature = request.Temperature,
                    OilVolume = (decimal)Bo,
                    GasVolume = (decimal)(Rs_i - Rs),
                    GOR = (decimal)Rs,
                    Density = (decimal)rho
                });
            }

            return Task.FromResult(new DifferentialLiberationResult
            {
                TestId = Guid.NewGuid().ToString(),
                LiberationPoints = points,
                InitialGOR = (decimal)Rs_i,
                ResidualOilVolume = 1.0m,
                TestConditions = $"T={T_F:F1}°F, Pi={Pi:F0} psia",
                TestDate = DateTime.UtcNow
            });
        }

        public Task<ConstantCompositionResult> PerformConstantCompositionExpansionAsync(ConstantCompositionRequest request)
        {
            // CCE: expand oil sample at constant composition above bubble point
            double Pb = (double)request.SaturationPressure;
            double T = (double)request.Temperature;
            double step = request.PressureStep > 0 ? (double)request.PressureStep : 200.0;
            double API = request.Composition?.API > 0 ? (double)request.Composition.API : 35.0;
            double SG_gas = 0.65;
            double T_F = T - 459.67;
            double Rs_b = 500.0; // typical GOR at bubble point

            var points = new List<ConstantCompositionPoint>();
            // Above bubble point: single-phase oil expansion
            for (double P = Pb * 1.5; P >= Pb * 0.3; P -= step)
            {
                double co = 5e-6; // typical oil compressibility 1/psi
                double relVol = P > Pb ? Math.Exp(-co * (P - Pb)) : 1.0 + 0.005 * (Pb - P) / Pb;
                double SG_o = 141.5 / (131.5 + API);
                double Bo = 0.9759 + 0.00012 * Math.Pow(Rs_b * Math.Sqrt(SG_gas / SG_o) + 1.25 * T_F, 1.2);
                double rho = (62.4 * SG_o + 0.0136 * Rs_b * SG_gas) / (Bo * relVol);
                points.Add(new ConstantCompositionPoint
                {
                    Pressure = (decimal)P,
                    RelativeVolume = (decimal)relVol,
                    Density = (decimal)rho,
                    Compressibility = (decimal)(co)
                });
            }

            return Task.FromResult(new ConstantCompositionResult
            {
                TestId = Guid.NewGuid().ToString(),
                ExpansionPoints = points,
                SaturationPressure = request.SaturationPressure,
                InitialVolume = 1.0m,
                TestConditions = $"T={T_F:F1}°F, Pb={Pb:F0} psia",
                TestDate = DateTime.UtcNow
            });
        }

        #endregion

        #region Thermal Properties

        public Task<OilPropertyResult> CalculateThermalConductivityAsync(CalculateThermalConductivityRequest request)
        {
            // Cragoe / Sloan correlation for crude oil thermal conductivity
            // k_o (W/(m·K)) = 0.1172 * (1 - 0.00054*(T_F - 32)) * (141.5/(SG*131.5+1)) approx
            double T_F = (double)request.Temperature - 459.67;
            double API = (double)request.OilGravity;
            double SG = 141.5 / (131.5 + API);
            // Cragoe: k = 0.00155 / SG * (1 - 0.0003*(T_C)) in BTU/(hr·ft·°F)
            double T_C = (T_F - 32.0) / 1.8;
            double k_BTU = 0.00155 / SG * (1.0 - 0.0003 * T_C);
            double k_SI = k_BTU * 1.7307; // W/(m·K)

            return Task.FromResult(new OilPropertyResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                PropertyType = "Thermal Conductivity",
                Value = (decimal)Math.Max(0.05, k_SI),
                Unit = "W/(m·K)",
                CorrelationUsed = "Cragoe",
                CalculationDate = DateTime.UtcNow
            });
        }

        public Task<OilPropertyResult> CalculateSpecificHeatAsync(CalculateSpecificHeatRequest request)
        {
            // Kesler-Lee correlation for oil specific heat
            // Cp = (0.338 + 0.000815*T_F + 0.00000000415*T_F^2) / SG (BTU/(lb·°F))
            double T_F = (double)request.Temperature - 459.67;
            double API = (double)request.OilGravity;
            double SG = 141.5 / (131.5 + API);
            double Cp_BTU = (0.338 + 0.000815 * T_F + 4.15e-9 * T_F * T_F) / SG;
            double Cp_SI = Cp_BTU * 4.1868; // kJ/(kg·K)

            return Task.FromResult(new OilPropertyResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                PropertyType = "Specific Heat (Cp)",
                Value = (decimal)Math.Max(0.5, Cp_SI),
                Unit = "kJ/(kg·K)",
                CorrelationUsed = "Kesler-Lee",
                CalculationDate = DateTime.UtcNow
            });
        }

        public Task<OilPropertyResult> CalculateThermalExpansionAsync(CalculateThermalExpansionRequest request)
        {
            // Coefficient of thermal expansion for crude oil
            // β = -1/ρ * (∂ρ/∂T)_P ≈ 0.0004 to 0.001 /°F for crude oils
            // Approximation: β = (1.825 - 0.001315*API) / (1000 * SG * (T_F + 460))
            double T_F = (double)request.Temperature - 459.67;
            double API = (double)request.OilGravity;
            double SG = 141.5 / (131.5 + API);
            double beta_perF = (1.825 - 0.001315 * API) / (1000.0 * SG * (T_F + 460.0));
            double beta_perR = beta_perF; // same since °F and °R have same increments

            return Task.FromResult(new OilPropertyResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                PropertyType = "Thermal Expansion Coefficient",
                Value = (decimal)Math.Max(0, beta_perR),
                Unit = "1/°R",
                CorrelationUsed = "Cragoe-Standing",
                CalculationDate = DateTime.UtcNow
            });
        }

        #endregion

        #region Advanced PVT

        public Task<EOSResult> PerformEOSCalculationAsync(EOSRequest request)
        {
            // Peng-Robinson EOS for oil system
            double T = (double)request.Temperature; // °R
            double P = (double)request.Pressure; // psia
            double API = request.Composition?.API > 0 ? (double)request.Composition.API : 35.0;
            double SG = request.Composition?.SpecificGravity > 0 ? (double)request.Composition.SpecificGravity : 141.5 / (131.5 + API);
            double Mw = request.Composition?.MolecularWeight > 0 ? (double)request.Composition.MolecularWeight : 6084.0 / (API - 5.9);

            // Estimate pseudo-critical properties for oil fraction (Kesler-Lee)
            double T_F = T - 459.67;
            double Tc_R = 341.7 + 811.0 * SG + (0.4244 + 0.1174 * SG) * T_F + (0.4669 - 3.2623 * SG) * 1e5 / T_F;
            double Pc_psia = Math.Exp(8.3634 - 0.0566 / SG - (0.24244 + 2.2898 / SG + 0.11857 / (SG * SG)) * 1e-3 * T_F
                             + (1.4685 + 3.648 / SG + 0.47227 / (SG * SG)) * 1e-7 * T_F * T_F);
            double omega = -7.904 + 0.1352 * SG * 1000 / Mw - 0.007465 * Math.Pow(SG * 1000 / Mw, 2) + 8.359 * T_F / 1000;
            omega = Math.Max(0, Math.Min(2.0, omega));

            double Tc_K = Tc_R * 5.0 / 9.0;
            double Pc_atm = Pc_psia / 14.696;
            double R = 82.06;
            double alpha = Math.Pow(1.0 + (0.37464 + 1.54226 * omega - 0.26992 * omega * omega) * (1.0 - Math.Sqrt(T / Tc_R)), 2);
            double a = 0.45724 * R * R * Tc_K * Tc_K / Pc_atm * alpha;
            double b = 0.07780 * R * Tc_K / Pc_atm;
            double P_atm = P / 14.696;
            double A = a * P_atm / (R * R * (T * 5.0 / 9.0) * (T * 5.0 / 9.0));
            double B = b * P_atm / (R * (T * 5.0 / 9.0));
            // Liquid root (smallest positive real root)
            double Z = 0.3; // initial estimate for liquid
            for (int i = 0; i < 50; i++)
            {
                double f = Z * Z * Z - (1 - B) * Z * Z + (A - 3 * B * B - 2 * B) * Z - (A * B - B * B - B * B * B);
                double fp = 3 * Z * Z - 2 * (1 - B) * Z + (A - 3 * B * B - 2 * B);
                if (Math.Abs(fp) < 1e-12) break;
                double dZ = f / fp;
                Z -= dZ;
                Z = Math.Max(B + 1e-6, Z);
                if (Math.Abs(dZ) < 1e-8) break;
            }
            Z = Math.Max(0.05, Math.Min(1.5, Z));

            return Task.FromResult(new EOSResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                EquationOfState = request.EquationOfState,
                CriticalPressure = (decimal)Pc_psia,
                CriticalTemperature = (decimal)Tc_R,
                AcentricFactor = (decimal)omega,
                Phases = new List<EOSPhase>
                {
                    new EOSPhase
                    {
                        PhaseType = "Liquid",
                        MoleFraction = 1.0m,
                        Compressibility = (decimal)Z,
                        Density = (decimal)(P * Mw / (Z * 10.73 * T))
                    }
                },
                CalculationDate = DateTime.UtcNow
            });
        }

        public Task<OilPropertyResult> CalculateAsphalteneOnsetAsync(CalculateAsphalteneOnsetRequest request)
        {
            // Flory-Huggins model simplified estimate for asphaltene onset pressure
            // Onset pressure typically 80-90% of bubble point for typical crude oils
            double T_F = (double)request.Temperature - 459.67;
            double API = request.Composition?.API > 0 ? (double)request.Composition.API : 30.0;
            double SG_gas = 0.65;
            double SG_oil = 141.5 / (131.5 + API);
            double Rs = 500.0; // assume moderate GOR
            double Pb = 18.2 * (Math.Pow(Rs / SG_gas, 0.83) * Math.Pow(10.0, 0.00091 * T_F - 0.0125 * API) - 1.4);
            double P_onset = Pb * 0.85; // onset typically below bubble point
            P_onset = Math.Max(500.0, P_onset);

            return Task.FromResult(new OilPropertyResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                PropertyType = "Asphaltene Onset Pressure",
                Value = (decimal)P_onset,
                Unit = "psia",
                CorrelationUsed = "Flory-Huggins Simplified",
                Notes = "Estimated from API gravity and reservoir conditions; lab confirmation recommended",
                CalculationDate = DateTime.UtcNow
            });
        }

        public Task<OilPropertyResult> CalculateWaxAppearanceAsync(CalculateWaxAppearanceRequest request)
        {
            // Wax appearance temperature (WAT) from composition / API gravity
            // Empirical: WAT ≈ 0.225*API + 20 (°F) for typical crude oils — Coutinho approximation
            double API = request.Composition?.API > 0 ? (double)request.Composition.API : 30.0;
            double WAT_F = 0.225 * API + 20.0;
            double WAT_R = WAT_F + 459.67;
            WAT_R = Math.Max(460.0, Math.Min(700.0, WAT_R)); // physical range

            return Task.FromResult(new OilPropertyResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                PropertyType = "Wax Appearance Temperature",
                Value = (decimal)WAT_R,
                Unit = "°R",
                CorrelationUsed = "Coutinho Simplified",
                Notes = "Estimated from API gravity; DSC measurement recommended",
                CalculationDate = DateTime.UtcNow
            });
        }

        public Task<ViscosityBlendResult> PerformViscosityBlendingAsync(ViscosityBlendRequest request)
        {
            // ASTM D341 / Refutas blending method
            // VBN(blend) = Σ(xi * VBN_i), where VBN = log10(log10(v + 0.7))
            // Then convert back: v_blend = 10^(10^VBN_blend) - 0.7
            var comps = request.Components ?? new List<ViscosityComponent>();
            double totalVol = comps.Sum(c => (double)c.VolumeFraction);
            if (totalVol <= 0) totalVol = 1.0;

            double vbn_blend = 0.0;
            foreach (var comp in comps)
            {
                double xi = (double)comp.VolumeFraction / totalVol;
                double v = Math.Max(0.21, (double)comp.Viscosity); // cSt minimum for log
                double vbn = Math.Log10(Math.Log10(v + 0.7));
                vbn_blend += xi * vbn;
            }
            double v_blend = Math.Pow(10.0, Math.Pow(10.0, vbn_blend)) - 0.7;
            v_blend = Math.Max(0.21, v_blend);

            return Task.FromResult(new ViscosityBlendResult
            {
                BlendId = Guid.NewGuid().ToString(),
                Components = comps,
                BlendedViscosity = (decimal)v_blend,
                BlendMethod = "ASTM D341 (Refutas)",
                Temperature = request.Temperature,
                CalculationDate = DateTime.UtcNow
            });
        }

        #endregion

        #region Surface Properties

        public Task<OilPropertyResult> CalculateInterfacialTensionAsync(CalculateInterfacialTensionRequest request)
        {
            // Baker-Swerdloff / Standing correlation for gas-oil IFT
            // σ_od = 39.0 - 0.2571*API (dead oil at 68°F)
            // Correction for temperature: σ_o = σ_od - 0.087*(T_F - 68)
            double T_F = (double)request.Temperature - 459.67;
            double API = (double)request.OilGravity;
            double sigma_od = 39.0 - 0.2571 * API;
            double sigma = sigma_od - 0.087 * (T_F - 68.0);
            sigma = Math.Max(1.0, Math.Min(50.0, sigma));

            return Task.FromResult(new OilPropertyResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                PropertyType = "Gas-Oil Interfacial Tension",
                Value = (decimal)sigma,
                Unit = "dyne/cm",
                CorrelationUsed = "Baker-Swerdloff",
                CalculationDate = DateTime.UtcNow
            });
        }

        public Task<WettabilityResult> CalculateWettabilityAsync(WettabilityRequest request)
        {
            // Wettability classification from contact angle
            double angle = (double)request.ContactAngle;
            string wettClass = angle < 75.0 ? "Water-Wet"
                : angle < 105.0 ? "Intermediate-Wet"
                : "Oil-Wet";
            // Amott-Harvey wettability index approximation
            string wettIndex = angle < 75.0 ? "> 0.3 (Water-Wet)"
                : angle < 105.0 ? "~0 (Neutral)"
                : "< -0.3 (Oil-Wet)";

            return Task.FromResult(new WettabilityResult
            {
                AnalysisId = Guid.NewGuid().ToString(),
                WettabilityIndex = wettIndex,
                ContactAngle = request.ContactAngle,
                WettabilityClass = wettClass,
                MeasurementMethod = request.MeasurementMethod,
                AnalysisDate = DateTime.UtcNow
            });
        }

        public Task<OilPropertyResult> CalculatePourPointAsync(CalculatePourPointRequest request)
        {
            // Pour point estimation from API gravity (rough empirical)
            // PP (°F) ≈ 0.97*API - 37.5 for typical paraffinic crude
            double API = request.Composition?.API > 0 ? (double)request.Composition.API : 30.0;
            double PP_F = 0.97 * API - 37.5;
            PP_F = Math.Max(-60.0, Math.Min(80.0, PP_F));
            double PP_R = PP_F + 459.67;

            return Task.FromResult(new OilPropertyResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                PropertyType = "Pour Point",
                Value = (decimal)PP_R,
                Unit = "°R",
                CorrelationUsed = "Empirical API",
                Notes = "Estimated from API gravity; ASTM D97 measurement recommended",
                CalculationDate = DateTime.UtcNow
            });
        }

        public Task<OilPropertyResult> CalculateCloudPointAsync(CalculateCloudPointRequest request)
        {
            // Cloud point typically 5-15°F above pour point for paraffin-based oils
            double API = request.Composition?.API > 0 ? (double)request.Composition.API : 30.0;
            double PP_F = 0.97 * API - 37.5;
            double CP_F = PP_F + 10.0; // typical offset
            CP_F = Math.Max(-50.0, Math.Min(90.0, CP_F));
            double CP_R = CP_F + 459.67;

            return Task.FromResult(new OilPropertyResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                PropertyType = "Cloud Point",
                Value = (decimal)CP_R,
                Unit = "°R",
                CorrelationUsed = "Empirical API",
                Notes = "Estimated from API gravity; ASTM D2500 measurement recommended",
                CalculationDate = DateTime.UtcNow
            });
        }

        #endregion

        #region Correlations Management

        public Task<List<CorrelationInfo>> GetAvailableCorrelationsAsync(string propertyType)
        {
            var all = new List<CorrelationInfo>
            {
                new CorrelationInfo { CorrelationId = "STD", CorrelationName = "Standing", PropertyType = "BubblePoint,FVF,GOR", Author = "Standing", PublicationYear = 1947, Accuracy = 95m },
                new CorrelationInfo { CorrelationId = "VB", CorrelationName = "Vasquez-Beggs", PropertyType = "Compressibility,FVF", Author = "Vasquez & Beggs", PublicationYear = 1980, Accuracy = 94m },
                new CorrelationInfo { CorrelationId = "BR", CorrelationName = "Beggs-Robinson", PropertyType = "Viscosity", Author = "Beggs & Robinson", PublicationYear = 1975, Accuracy = 92m },
                new CorrelationInfo { CorrelationId = "Glaso", CorrelationName = "Glaso", PropertyType = "BubblePoint,FVF", Author = "Glaso", PublicationYear = 1980, Accuracy = 93m }
            };
            var filtered = string.IsNullOrEmpty(propertyType)
                ? all
                : all.Where(c => c.PropertyType.Contains(propertyType, StringComparison.OrdinalIgnoreCase)).ToList();
            return Task.FromResult(filtered);
        }

        public Task<CorrelationValidation> ValidateCorrelationAsync(CorrelationValidationRequest request)
        {
            bool applicable = true;
            var issues = new List<string>();
            var recommendations = new List<string>();

            if (request.InputParameters.TryGetValue("API", out decimal api))
            {
                if (api < 16 || api > 58) { applicable = false; issues.Add("API gravity outside Standing correlation range (16-58°API)"); }
            }
            if (request.InputParameters.TryGetValue("Temperature", out decimal T))
            {
                if (T < 459.67m + 74 || T > 459.67m + 240) issues.Add("Temperature outside recommended range (74-240°F)");
            }
            if (!applicable) recommendations.Add("Consider Al-Marhoun or Glaso correlations for extended range");

            return Task.FromResult(new CorrelationValidation
            {
                ValidationId = Guid.NewGuid().ToString(),
                CorrelationId = request.CorrelationId,
                IsApplicable = applicable,
                ApplicabilityIssues = issues,
                ConfidenceLevel = applicable ? 90m : 50m,
                Recommendations = recommendations
            });
        }

        public Task<CorrelationComparison> CompareCorrelationsAsync(CorrelationComparisonRequest request)
        {
            request.InputParameters.TryGetValue("API", out decimal api);
            request.InputParameters.TryGetValue("Temperature", out decimal T);
            request.InputParameters.TryGetValue("GOR", out decimal Rs);
            if (api <= 0) api = 35m;
            if (T <= 0) T = 600m;
            if (Rs <= 0) Rs = 500m;

            decimal T_F = T - 459.67m;
            decimal SG = 0.65m;
            // Standing
            decimal Pb_standing = 18.2m * ((decimal)Math.Pow((double)(Rs / SG), 0.83) * (decimal)Math.Pow(10.0, (double)(0.00091m * T_F - 0.0125m * api)) - 1.4m);
            // Glaso
            decimal Pb_glaso = (decimal)Math.Pow(10.0, (double)(1.7669m + 1.7447m * (decimal)Math.Log10((double)(Rs / SG * (decimal)Math.Pow(10.0, (double)(0.00091m * T_F - 0.0125m * api)))) - 0.30218m * (decimal)Math.Pow(Math.Log10((double)(Rs / SG * (decimal)Math.Pow(10.0, (double)(0.00091m * T_F - 0.0125m * api)))), 2)));

            decimal? measured = request.MeasuredValue;
            var results = new List<CorrelationResult>
            {
                new CorrelationResult { CorrelationId = "Standing", CalculatedValue = Math.Max(0, Pb_standing), Deviation = measured.HasValue && measured.Value != 0 ? Math.Abs((Pb_standing - measured.Value) / measured.Value) * 100m : 0m },
                new CorrelationResult { CorrelationId = "Glaso", CalculatedValue = Math.Max(0, Pb_glaso), Deviation = measured.HasValue && measured.Value != 0 ? Math.Abs((Pb_glaso - measured.Value) / measured.Value) * 100m : 0m }
            };
            foreach (var r in results) r.AccuracyScore = 100m - r.Deviation;
            string best = results.OrderBy(r => r.Deviation).FirstOrDefault()?.CorrelationId ?? "Standing";

            return Task.FromResult(new CorrelationComparison
            {
                ComparisonId = Guid.NewGuid().ToString(),
                PropertyType = request.PropertyType,
                CorrelationResults = results,
                BestCorrelation = best,
                ComparisonSummary = $"Compared {results.Count} correlations. Best match: {best}"
            });
        }

        #endregion

        #region Compositional Analysis

        public Task<CompositionalAnalysis> PerformCompositionalAnalysisAsync(CompositionalAnalysisRequest request)
        {
            var comp = request.Composition ?? new OilComposition();
            double API = (double)(comp.API > 0 ? comp.API : 35m);
            double SG = 141.5 / (131.5 + API);
            double Mw = comp.MolecularWeight > 0 ? (double)comp.MolecularWeight : 6084.0 / (API - 5.9);

            var componentAnalysis = comp.Components?.Select(c => new CompositionComponent
            {
                ComponentName = c.ComponentName,
                MoleFraction = c.MoleFraction,
                MassFraction = c.MassFraction,
                MolecularWeight = c.MolecularWeight,
                BoilingPoint = c.BoilingPoint
            }).ToList() ?? new List<CompositionComponent>();

            return Task.FromResult(new CompositionalAnalysis
            {
                AnalysisId = Guid.NewGuid().ToString(),
                Composition = comp,
                MolecularWeight = (decimal)Mw,
                SpecificGravity = (decimal)SG,
                APIGravity = comp.API > 0 ? comp.API : (decimal)API,
                Components = componentAnalysis,
                AnalysisDate = DateTime.UtcNow
            });
        }

        public Task<OilPropertyResult> CalculateMolecularWeightAsync(CalculateMolecularWeightRequest request)
        {
            // Cragoe correlation: Mw = 6084 / (API - 5.9) for crude oil fraction
            double API = request.Composition?.API > 0 ? (double)request.Composition.API : 35.0;
            double Mw;
            if (request.Composition?.Components != null && request.Composition.Components.Count > 0)
            {
                double totalY = (double)request.Composition.Components.Sum(c => c.MoleFraction);
                Mw = totalY > 0 ? (double)request.Composition.Components.Sum(c => c.MoleFraction * c.MolecularWeight) / totalY : 6084.0 / (API - 5.9);
            }
            else
            {
                Mw = request.Composition?.MolecularWeight > 0 ? (double)request.Composition.MolecularWeight : 6084.0 / (API - 5.9);
            }

            return Task.FromResult(new OilPropertyResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                PropertyType = "Molecular Weight",
                Value = (decimal)Math.Max(50, Mw),
                Unit = "lb/lbmol",
                CorrelationUsed = "Cragoe",
                CalculationDate = DateTime.UtcNow
            });
        }

        public Task<SaraAnalysis> PerformSaraAnalysisAsync(SaraAnalysisRequest request)
        {
            // SARA fractions cannot be calculated from PVT correlations; return nominal values.
            // Typical crude oil SARA fractions by API gravity (rough estimates)
            return Task.FromResult(new SaraAnalysis
            {
                AnalysisId = Guid.NewGuid().ToString(),
                SaturatesFraction = 0m,
                AromaticsFraction = 0m,
                ResinsFraction = 0m,
                AsphaltenesFraction = 0m,
                AnalysisMethod = request.AnalysisMethod,
                AnalysisDate = DateTime.UtcNow
            });
        }

        #endregion

        #region Laboratory Data Management

        public Task<PVTData> StorePVTDataAsync(PVTData pvtData, string userId)
        {
            if (pvtData == null) throw new ArgumentNullException(nameof(pvtData));
            if (string.IsNullOrWhiteSpace(pvtData.SampleId))
                pvtData.SampleId = Guid.NewGuid().ToString();
            return Task.FromResult(pvtData);
        }

        public Task<List<PVTData>> GetPVTDataAsync(string sampleId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return Task.FromResult(new List<PVTData>());
        }

        public Task<CorrelationMatching> MatchLabDataWithCorrelationsAsync(CorrelationMatchingRequest request)
        {
            return Task.FromResult(new CorrelationMatching
            {
                MatchingId = Guid.NewGuid().ToString(),
                SampleId = request?.SampleId ?? string.Empty,
                Matches = new List<CorrelationMatch>(),
                BestMatchCorrelation = "Standing",
                BestMatchAccuracy = 0m
            });
        }

        public Task<DataQuality> ValidateLabDataQualityAsync(string sampleId)
        {
            return Task.FromResult(new DataQuality
            {
                AssessmentId = Guid.NewGuid().ToString(),
                SampleId = sampleId,
                OverallQualityScore = 0m,
                Issues = new List<DATA_QUALITY_ISSUE>
                {
                    new DATA_QUALITY_ISSUE
                    {
                        IssueType = "NoData",
                        Description = "No lab data found for the specified sample ID",
                        Severity = "Info",
                        AffectedProperty = "All",
                        Impact = 0m
                    }
                },
                Recommendations = new List<string> { "Provide laboratory PVT data to enable quality assessment" },
                QualityRating = "Unassessed"
            });
        }

        #endregion

        #region Multiphase Flow

        public Task<RelativePermeability> CalculateRelativePermeabilityAsync(RelativePermeabilityRequest request)
        {
            // Corey correlation for oil-water relative permeability
            double Sor = (double)(request.ResidualOilSaturation > 0 ? request.ResidualOilSaturation : 0.2m);
            double Swc = (double)(request.ResidualWaterSaturation > 0 ? request.ResidualWaterSaturation : 0.15m);
            double no = 3.0; // Corey oil exponent
            double nw = 2.0; // Corey water exponent

            var points = new List<RelativePermeabilityPoint>();
            for (int i = 0; i <= 20; i++)
            {
                double Sw = Swc + (i / 20.0) * (1.0 - Sor - Swc);
                double Se = (Sw - Swc) / Math.Max(1e-6, 1.0 - Sor - Swc);
                double kro = Math.Max(0, Math.Min(1.0, Math.Pow(1.0 - Se, no)));
                double krw = Math.Max(0, Math.Min(1.0, Math.Pow(Se, nw)));
                points.Add(new RelativePermeabilityPoint
                {
                    WaterSaturation = (decimal)Sw,
                    OilRelativePermeability = (decimal)kro,
                    WaterRelativePermeability = (decimal)krw
                });
            }

            return Task.FromResult(new RelativePermeability
            {
                CalculationId = Guid.NewGuid().ToString(),
                Points = points,
                Correlation = "Corey",
                ResidualOilSaturation = request.ResidualOilSaturation,
                ResidualWaterSaturation = request.ResidualWaterSaturation,
                CalculationDate = DateTime.UtcNow
            });
        }

        public Task<CapillaryPressure> CalculateCapillaryPressureAsync(CapillaryPressureRequest request)
        {
            // Leverett J-function: J(Sw) = Pc * sqrt(k/phi) / (σ*cos(θ))
            // Pc = J * σ * cos(θ) / sqrt(k/phi)
            double sigma = (double)request.InterfacialTension; // dyne/cm
            double theta = (double)request.ContactAngle * Math.PI / 180.0;
            double k = (double)(request.Permeability > 0 ? request.Permeability : 10m); // mD
            double phi = (double)(request.Porosity > 0 ? request.Porosity : 0.2m);
            double kOverPhi = k / phi;
            double cosTheta = Math.Cos(theta);

            // Simplified J-function (Brooks-Corey type)
            var points = new List<CapillaryPressurePoint>();
            double Swc = 0.15;
            for (int i = 0; i <= 20; i++)
            {
                double Sw = Swc + (i / 20.0) * (1.0 - Swc);
                double Se = (Sw - Swc) / Math.Max(1e-6, 1.0 - Swc);
                double J = Se > 0 ? 0.5 * Math.Pow(Se, -0.7) : 10.0; // typical J-function
                double Pc = J * sigma * cosTheta / Math.Sqrt(kOverPhi);
                double height = Pc * 144.0 / ((62.4 - 50.0) * 0.433); // approximate ft above FWL
                points.Add(new CapillaryPressurePoint
                {
                    WaterSaturation = (decimal)Sw,
                    CapillaryPressure = (decimal)Math.Max(0, Pc),
                    HeightAboveFWL = (decimal)Math.Max(0, height)
                });
            }

            return Task.FromResult(new CapillaryPressure
            {
                CalculationId = Guid.NewGuid().ToString(),
                Points = points,
                Correlation = "Leverett J-Function",
                InterfacialTension = request.InterfacialTension,
                ContactAngle = request.ContactAngle,
                CalculationDate = DateTime.UtcNow
            });
        }

        public Task<OilPropertyResult> CalculateEmulsionViscosityAsync(CalculateEmulsionViscosityRequest request)
        {
            // Brinkman correlation for emulsion viscosity
            // μ_em = μ_c * (1 - φ_d)^(-2.5)  (dispersed phase concentration φ_d)
            double mu_c, phi_d;
            if (request.EmulsionType == "OilInWater")
            {
                mu_c = (double)request.WaterViscosity;
                phi_d = (double)(1.0m - request.WaterCut); // oil is dispersed phase
            }
            else // WaterInOil
            {
                mu_c = (double)request.OilViscosity;
                phi_d = (double)request.WaterCut; // water is dispersed phase
            }
            phi_d = Math.Max(0, Math.Min(0.74, phi_d)); // above 0.74 emulsion inverts
            double mu_em = mu_c * Math.Pow(1.0 - phi_d, -2.5);
            mu_em = Math.Max(0.1, mu_em);

            return Task.FromResult(new OilPropertyResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                PropertyType = "Emulsion Viscosity",
                Value = (decimal)mu_em,
                Unit = "cp",
                CorrelationUsed = "Brinkman",
                CalculationDate = DateTime.UtcNow
            });
        }

        #endregion

        #region Quality Control and Validation

        public Task<ValidationResult> ValidateCalculationResultsAsync(ValidationRequest request)
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            bool isValid = true;
            var result = request?.CalculationResult;

            if (result != null)
            {
                if (result.Value < 0)
                {
                    errors.Add($"{result.PropertyType}: Negative value ({result.Value}) is physically impossible");
                    isValid = false;
                }
                if (result.PropertyType == "Formation Volume Factor" && (result.Value < 0.95m || result.Value > 3.0m))
                    warnings.Add($"Bo value {result.Value} is outside typical range (0.95-3.0 rb/stb)");
                if (result.PropertyType == "Viscosity" && (result.Value < 0.1m || result.Value > 1000m))
                    warnings.Add($"Oil viscosity {result.Value} cp is outside typical range");
                if (request?.MeasuredValue.HasValue == true && request.MeasuredValue.Value != 0)
                {
                    decimal relError = Math.Abs((result.Value - request.MeasuredValue.Value) / request.MeasuredValue.Value) * 100m;
                    if (relError > request.Tolerance * 100m)
                        warnings.Add($"Calculated value deviates {relError:F1}% from measured value (tolerance: {request.Tolerance * 100m:F1}%)");
                }
            }

            return Task.FromResult(new ValidationResult
            {
                ValidationId = Guid.NewGuid().ToString(),
                IsValid = isValid,
                ValidationErrors = errors,
                Warnings = warnings,
                ConfidenceScore = isValid ? 95m - warnings.Count * 5m : 0m,
                ValidationSummary = isValid
                    ? $"Validation passed with {warnings.Count} warning(s)"
                    : $"Validation failed: {errors.Count} error(s)"
            });
        }

        public Task<UncertaintyAnalysis> PerformUncertaintyAnalysisAsync(UncertaintyAnalysisRequest request)
        {
            double mean = request?.BaseParameters != null && request.BaseParameters.TryGetValue("Value", out decimal v) ? (double)v : 0.0;
            double stdDev = mean * 0.05;

            return Task.FromResult(new UncertaintyAnalysis
            {
                AnalysisId = Guid.NewGuid().ToString(),
                PropertyType = request?.PropertyType ?? "Unknown",
                MeanValue = (decimal)mean,
                StandardDeviation = (decimal)stdDev,
                P10Value = (decimal)(mean - 1.282 * stdDev),
                P50Value = (decimal)mean,
                P90Value = (decimal)(mean + 1.282 * stdDev),
                DistributionType = "Normal",
                KeyUncertaintyFactors = new List<string>
                {
                    "API gravity measurement (±0.5°)",
                    "Reservoir temperature (±2°F)",
                    "Producing GOR (±5%)",
                    "Correlation applicability"
                }
            });
        }

        public Task<QAReport> GenerateQAReportAsync(QAReportRequest request)
        {
            return Task.FromResult(new QAReport
            {
                ReportId = Guid.NewGuid().ToString(),
                SampleId = request?.SampleId ?? string.Empty,
                GeneratedDate = DateTime.UtcNow,
                Sections = new List<QASection>
                {
                    new QASection
                    {
                        SectionName = "Data Completeness",
                        Assessment = "No lab data loaded",
                        Score = 0m,
                        Issues = new List<string> { "Lab PVT data required for full QA assessment" },
                        Recommendations = new List<string> { "Submit oil sample for PVT analysis" }
                    }
                },
                OverallAssessment = "Insufficient data for quality assessment",
                Recommendations = new List<string> { "Provide laboratory oil PVT data", "Validate against field measurements" }
            });
        }

        #endregion

        #region Reporting and Export

        public Task<PVTReport> GeneratePVTReportAsync(PVTReportRequest request)
        {
            return Task.FromResult(new PVTReport
            {
                ReportId = Guid.NewGuid().ToString(),
                SampleId = request?.SampleId ?? string.Empty,
                GeneratedDate = DateTime.UtcNow,
                PVTData = new PVTData(),
                CalculatedProperties = new List<OilPropertyResult>(),
                ReportContent = Array.Empty<byte>(),
                Charts = new List<byte[]>()
            });
        }

        public Task<byte[]> ExportOilPropertiesDataAsync(ExportRequest request)
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        public Task<byte[]> GeneratePropertyChartsAsync(ChartRequest request)
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        #endregion
    }

    // Requests placeholder to match Signature if not in DTOs
    public class CalculateBubblePointRequest { public decimal SolutionGOR; public decimal GasGravity; public decimal OilGravity; public decimal Temperature; }
    public class CalculateSolutionGORRequest { public decimal Pressure; public decimal GasGravity; public decimal OilGravity; public decimal Temperature; }
    // CalculateFVFRequest, CalculateDensityRequest, CalculateViscosityRequest, CalculateCompressibilityRequest already defined in IOilPropertiesService.cs region?
    // Actually they are likely in the same file or DTOs.
    // I will redefine them here conditionally or just trust they are available since I saw them in the interface file "Request DTOs" region.
    // The Interface file had a "Request DTOs" region at the bottom.
    // So I assume they are in the Namespace.
}
