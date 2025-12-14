using System;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.FieldManagement.DataMapping
{
    /// <summary>
    /// Provides value retrieval functions for mapping PPDM39 entities to domain models.
    /// These can be overloaded to provide custom data retrieval logic.
    /// </summary>
    public static class ValueRetrievers
    {
        /// <summary>
        /// Retrieves tubing diameter from WELL_TUBULAR or related entities.
        /// </summary>
        public static Func<WELL, WELL_TUBULAR?, double> GetTubingDiameter { get; set; } = (well, tubular) =>
        {
            if (tubular != null && tubular.INSIDE_DIAMETER > 0)
            {
                var diameter = (double)tubular.INSIDE_DIAMETER;
                if (tubular.INSIDE_DIAMETER_OUOM?.ToUpper() == "M" || tubular.INSIDE_DIAMETER_OUOM?.ToUpper() == "METER")
                {
                    diameter *= 39.3701; // meters to inches
                }
                else if (tubular.INSIDE_DIAMETER_OUOM?.ToUpper() == "CM")
                {
                    diameter *= 0.393701; // cm to inches
                }
                return diameter;
            }
            throw new InvalidOperationException("Tubing diameter not available. Provide GetTubingDiameter function or WELL_TUBULAR entity.");
        };

        /// <summary>
        /// Retrieves tubing length from WELL_TUBULAR or WELL.
        /// </summary>
        public static Func<WELL, WELL_TUBULAR?, double> GetTubingLength { get; set; } = (well, tubular) =>
        {
            if (tubular != null && tubular.LEFT_IN_HOLE_LENGTH > 0)
            {
                return (double)tubular.LEFT_IN_HOLE_LENGTH;
            }
            if (well.BASE_DEPTH > 0)
            {
                return (double)well.BASE_DEPTH;
            }
            throw new InvalidOperationException("Tubing length not available. Provide GetTubingLength function or WELL_TUBULAR entity.");
        };

        /// <summary>
        /// Retrieves wellhead pressure from WELL_PRESSURE.
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, double> GetWellheadPressure { get; set; } = (well, wellPressure) =>
        {
            if (wellPressure != null && wellPressure.FLOW_TUBING_PRESSURE > 0)
            {
                return (double)wellPressure.FLOW_TUBING_PRESSURE;
            }
            throw new InvalidOperationException("Wellhead pressure not available. Provide GetWellheadPressure function or WELL_PRESSURE entity.");
        };

        /// <summary>
        /// Retrieves wellhead temperature.
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, double> GetWellheadTemperature { get; set; } = (well, wellPressure) =>
        {
            throw new InvalidOperationException("Wellhead temperature not available. Provide GetWellheadTemperature function.");
        };

        /// <summary>
        /// Retrieves bottomhole temperature.
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, double> GetBottomholeTemperature { get; set; } = (well, wellPressure) =>
        {
            if (well.BASE_DEPTH > 0)
            {
                var surfaceTemp = GetWellheadTemperature(well, wellPressure);
                var depthInFeet = (double)well.BASE_DEPTH;
                var geothermalGradient = 1.5; // °F per 100 ft
                return surfaceTemp + (depthInFeet / 100.0) * geothermalGradient;
            }
            throw new InvalidOperationException("Bottomhole temperature not available. Provide GetBottomholeTemperature function.");
        };

        /// <summary>
        /// Retrieves reservoir pressure from WELL_PRESSURE.
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, double> GetReservoirPressure { get; set; } = (well, wellPressure) =>
        {
            if (wellPressure != null && wellPressure.INIT_RESERVOIR_PRESSURE > 0)
            {
                return (double)wellPressure.INIT_RESERVOIR_PRESSURE;
            }
            throw new InvalidOperationException("Reservoir pressure not available. Provide GetReservoirPressure function or WELL_PRESSURE entity.");
        };

        /// <summary>
        /// Retrieves water cut (fraction).
        /// </summary>
        public static Func<WELL, double> GetWaterCut { get; set; } = (well) =>
        {
            throw new InvalidOperationException("Water cut not available. Provide GetWaterCut function.");
        };

        /// <summary>
        /// Retrieves gas-oil ratio in SCF/STB.
        /// </summary>
        public static Func<WELL, double> GetGasOilRatio { get; set; } = (well) =>
        {
            throw new InvalidOperationException("Gas-oil ratio not available. Provide GetGasOilRatio function.");
        };

        /// <summary>
        /// Retrieves oil gravity in API.
        /// </summary>
        public static Func<WELL, double> GetOilGravity { get; set; } = (well) =>
        {
            throw new InvalidOperationException("Oil gravity not available. Provide GetOilGravity function.");
        };

        /// <summary>
        /// Retrieves gas specific gravity.
        /// </summary>
        public static Func<WELL, double> GetGasSpecificGravity { get; set; } = (well) =>
        {
            throw new InvalidOperationException("Gas specific gravity not available. Provide GetGasSpecificGravity function.");
        };

        /// <summary>
        /// Retrieves formation volume factor in RB/STB.
        /// </summary>
        public static Func<WELL, double> GetFormationVolumeFactor { get; set; } = (well) =>
        {
            throw new InvalidOperationException("Formation volume factor not available. Provide GetFormationVolumeFactor function.");
        };

        /// <summary>
        /// Retrieves oil viscosity in cp.
        /// </summary>
        public static Func<WELL, double> GetOilViscosity { get; set; } = (well) =>
        {
            throw new InvalidOperationException("Oil viscosity not available. Provide GetOilViscosity function.");
        };

        /// <summary>
        /// Retrieves bubble point pressure in psi.
        /// </summary>
        public static Func<WELL, double> GetBubblePointPressure { get; set; } = (well) =>
        {
            throw new InvalidOperationException("Bubble point pressure not available. Provide GetBubblePointPressure function.");
        };

        /// <summary>
        /// Retrieves productivity index in BPD/psi.
        /// </summary>
        public static Func<WELL, double> GetProductivityIndex { get; set; } = (well) =>
        {
            throw new InvalidOperationException("Productivity index not available. Provide GetProductivityIndex function.");
        };

        /// <summary>
        /// Retrieves wellbore radius in feet.
        /// </summary>
        public static Func<WELL, WELL_TUBULAR?, double> GetWellboreRadius { get; set; } = (well, tubular) =>
        {
            if (tubular != null && tubular.INSIDE_DIAMETER > 0)
            {
                var radius = (double)tubular.INSIDE_DIAMETER / 2.0;
                if (tubular.INSIDE_DIAMETER_OUOM?.ToUpper() == "M" || tubular.INSIDE_DIAMETER_OUOM?.ToUpper() == "METER")
                {
                    radius *= 3.28084; // meters to feet
                }
                else if (tubular.INSIDE_DIAMETER_OUOM?.ToUpper() == "IN" || tubular.INSIDE_DIAMETER_OUOM?.ToUpper() == "INCH")
                {
                    radius /= 12.0; // inches to feet
                }
                return radius;
            }
            throw new InvalidOperationException("Wellbore radius not available. Provide GetWellboreRadius function or WELL_TUBULAR entity.");
        };

        /// <summary>
        /// Retrieves wellbore radius in feet (returns decimal).
        /// </summary>
        public static Func<WELL, WELL_TUBULAR?, decimal> GetWellboreRadiusDecimal { get; set; } = (well, tubular) => (decimal)GetWellboreRadius(well, tubular);

        /// <summary>
        /// Retrieves formation volume factor in RB/STB (returns decimal).
        /// </summary>
        public static Func<WELL, decimal> GetFormationVolumeFactorDecimal { get; set; } = (well) => (decimal)GetFormationVolumeFactor(well);

        /// <summary>
        /// Retrieves reservoir temperature in Rankine.
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, decimal> GetReservoirTemperature { get; set; } = (well, wellPressure) =>
        {
            if (well.BASE_DEPTH > 0)
            {
                var surfaceTemp = 60.0m; // Fahrenheit surface temperature
                var depthInFeet = well.BASE_DEPTH;
                var geothermalGradient = 1.5m; // °F per 100 ft
                var tempF = surfaceTemp + (depthInFeet / 100.0m) * geothermalGradient;
                return tempF + 459.67m; // Convert to Rankine
            }
            throw new InvalidOperationException("Reservoir temperature not available. Provide GetReservoirTemperature function.");
        };

        /// <summary>
        /// Retrieves well depth in feet.
        /// </summary>
        public static Func<WELL, WELL_TUBULAR?, decimal> GetWellDepth { get; set; } = (well, tubular) =>
        {
            if (well.BASE_DEPTH > 0)
            {
                return well.BASE_DEPTH;
            }
            throw new InvalidOperationException("Well depth not available. Provide GetWellDepth function or ensure WELL.BASE_DEPTH is set.");
        };

        /// <summary>
        /// Retrieves casing diameter in inches.
        /// </summary>
        public static Func<WELL, WELL_TUBULAR?, decimal> GetCasingDiameter { get; set; } = (well, tubular) =>
        {
            if (tubular != null && tubular.OUTSIDE_DIAMETER > 0)
            {
                var diameter = tubular.OUTSIDE_DIAMETER;
                if (tubular.OUTSIDE_DIAMETER_OUOM?.ToUpper() == "M" || tubular.OUTSIDE_DIAMETER_OUOM?.ToUpper() == "METER")
                {
                    diameter *= 39.3701m; // meters to inches
                }
                else if (tubular.OUTSIDE_DIAMETER_OUOM?.ToUpper() == "CM")
                {
                    diameter *= 0.393701m; // cm to inches
                }
                return diameter;
            }
            throw new InvalidOperationException("Casing diameter not available. Provide GetCasingDiameter function or WELL_TUBULAR entity.");
        };

        /// <summary>
        /// Retrieves casing pressure in psia.
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, decimal> GetCasingPressure { get; set; } = (well, wellPressure) =>
        {
            if (wellPressure != null && wellPressure.FLOW_CASING_PRESSURE > 0)
            {
                return wellPressure.FLOW_CASING_PRESSURE;
            }
            throw new InvalidOperationException("Casing pressure not available. Provide GetCasingPressure function or WELL_PRESSURE entity.");
        };

        /// <summary>
        /// Retrieves pump setting depth in feet (defaults to 80% of well depth).
        /// </summary>
        public static Func<WELL, WELL_TUBULAR?, decimal> GetPumpSettingDepth { get; set; } = (well, tubular) =>
        {
            if (well.BASE_DEPTH > 0)
            {
                return well.BASE_DEPTH * 0.8m; // Default: 80% of well depth
            }
            throw new InvalidOperationException("Pump setting depth not available. Provide GetPumpSettingDepth function.");
        };

        /// <summary>
        /// Converts temperature from Fahrenheit to Rankine.
        /// </summary>
        public static Func<double, decimal> ConvertFahrenheitToRankine { get; set; } = (tempF) => (decimal)(tempF + 459.67);

        /// <summary>
        /// Retrieves temperature in Rankine from wellhead temperature.
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, decimal> GetWellheadTemperatureInRankine { get; set; } = (well, wellPressure) =>
        {
            var tempF = GetWellheadTemperature(well, wellPressure);
            return ConvertFahrenheitToRankine(tempF);
        };

        /// <summary>
        /// Retrieves bottomhole temperature in Rankine.
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, decimal> GetBottomholeTemperatureInRankine { get; set; } = (well, wellPressure) =>
        {
            var tempF = GetBottomholeTemperature(well, wellPressure);
            return ConvertFahrenheitToRankine(tempF);
        };

        /// <summary>
        /// Retrieves reservoir temperature in Fahrenheit (calculated from depth).
        /// </summary>
        public static Func<WELL, double> GetReservoirTemperatureFahrenheit { get; set; } = (well) =>
        {
            if (well.BASE_DEPTH > 0)
            {
                var surfaceTemp = 60.0;
                var depthInFeet = (double)well.BASE_DEPTH;
                var geothermalGradient = 1.5; // °F per 100 ft
                return surfaceTemp + (depthInFeet / 100.0) * geothermalGradient;
            }
            throw new InvalidOperationException("Reservoir temperature not available. Provide GetReservoirTemperatureFahrenheit function.");
        };

        /// <summary>
        /// Estimates downstream pressure as a percentage of upstream pressure.
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, decimal, decimal> GetDownstreamPressure { get; set; } = (well, wellPressure, percentage) =>
        {
            var upstream = GetWellheadPressure(well, wellPressure);
            return (decimal)upstream * percentage;
        };

        /// <summary>
        /// Estimates discharge pressure as a multiple of suction pressure.
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, decimal, decimal> GetDischargePressure { get; set; } = (well, wellPressure, multiplier) =>
        {
            var suction = GetWellheadPressure(well, wellPressure);
            return (decimal)suction * multiplier;
        };

        /// <summary>
        /// Calculates gas molecular weight from specific gravity.
        /// </summary>
        public static Func<WELL, decimal> GetGasMolecularWeight { get; set; } = (well) =>
        {
            var sg = GetGasSpecificGravity(well);
            return (decimal)(sg * 28.97); // Air molecular weight * specific gravity
        };

        /// <summary>
        /// Converts API gravity to specific gravity.
        /// </summary>
        public static Func<WELL, decimal> GetLiquidSpecificGravityFromAPIGravity { get; set; } = (well) =>
        {
            var apiGravity = GetOilGravity(well);
            return (decimal)(141.5 / (apiGravity + 131.5)); // Convert API to specific gravity
        };

        /// <summary>
        /// Calculates discharge temperature in Rankine (suction temperature + delta in Fahrenheit).
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, double, decimal> GetDischargeTemperature { get; set; } = (well, wellPressure, deltaFahrenheit) =>
        {
            var tempF = GetWellheadTemperature(well, wellPressure);
            return ConvertFahrenheitToRankine(tempF + deltaFahrenheit);
        };

        /// <summary>
        /// Gets default compressor efficiency (0.75).
        /// </summary>
        public static Func<WELL, decimal> GetDefaultCompressorEfficiency { get; set; } = (well) => 0.75m;

        /// <summary>
        /// Gets default mechanical efficiency (0.95).
        /// </summary>
        public static Func<WELL, decimal> GetDefaultMechanicalEfficiency { get; set; } = (well) => 0.95m;

        /// <summary>
        /// Gets default discharge coefficient for chokes (0.85).
        /// </summary>
        public static Func<WELL, decimal> GetDefaultDischargeCoefficient { get; set; } = (well) => 0.85m;

        /// <summary>
        /// Gets default pipeline roughness for steel (0.00015 feet).
        /// </summary>
        public static Func<WELL, decimal> GetDefaultPipelineRoughness { get; set; } = (well) => 0.00015m;

        /// <summary>
        /// Gets default elevation change (0 feet).
        /// </summary>
        public static Func<WELL, decimal> GetDefaultElevationChange { get; set; } = (well) => 0m;

        // Decimal-returning wrappers for common properties (to avoid type conversion in mappers)

        /// <summary>
        /// Retrieves tubing diameter in inches (returns decimal).
        /// </summary>
        public static Func<WELL, WELL_TUBULAR?, decimal> GetTubingDiameterDecimal { get; set; } = (well, tubular) => (decimal)GetTubingDiameter(well, tubular);

        /// <summary>
        /// Retrieves wellhead pressure in psi (returns decimal).
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, decimal> GetWellheadPressureDecimal { get; set; } = (well, wellPressure) => (decimal)GetWellheadPressure(well, wellPressure);

        /// <summary>
        /// Retrieves reservoir pressure in psi (returns decimal).
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, decimal> GetReservoirPressureDecimal { get; set; } = (well, wellPressure) => (decimal)GetReservoirPressure(well, wellPressure);

        /// <summary>
        /// Retrieves oil gravity in API (returns decimal).
        /// </summary>
        public static Func<WELL, decimal> GetOilGravityDecimal { get; set; } = (well) => (decimal)GetOilGravity(well);

        /// <summary>
        /// Retrieves water cut (fraction, returns decimal).
        /// </summary>
        public static Func<WELL, decimal> GetWaterCutDecimal { get; set; } = (well) => (decimal)GetWaterCut(well);

        /// <summary>
        /// Retrieves gas-oil ratio in SCF/STB (returns decimal).
        /// </summary>
        public static Func<WELL, decimal> GetGasOilRatioDecimal { get; set; } = (well) => (decimal)GetGasOilRatio(well);

        /// <summary>
        /// Retrieves gas specific gravity (returns decimal).
        /// </summary>
        public static Func<WELL, decimal> GetGasSpecificGravityDecimal { get; set; } = (well) => (decimal)GetGasSpecificGravity(well);

        /// <summary>
        /// Retrieves oil viscosity in cp (returns decimal).
        /// </summary>
        public static Func<WELL, decimal> GetOilViscosityDecimal { get; set; } = (well) => (decimal)GetOilViscosity(well);

        // Wrapper methods for common parameterized calls

        /// <summary>
        /// Gets downstream pressure at 80% of upstream (for chokes).
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, decimal> GetDownstreamPressure80Percent { get; set; } = (well, wellPressure) => GetDownstreamPressure(well, wellPressure, 0.8m);

        /// <summary>
        /// Gets downstream pressure at 90% of upstream (for pipelines).
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, decimal> GetDownstreamPressure90Percent { get; set; } = (well, wellPressure) => GetDownstreamPressure(well, wellPressure, 0.9m);

        /// <summary>
        /// Gets discharge pressure at 2x suction (for compressors).
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, decimal> GetDischargePressure2x { get; set; } = (well, wellPressure) => GetDischargePressure(well, wellPressure, 2.0m);

        /// <summary>
        /// Gets discharge temperature at suction + 50°F (for compressors).
        /// </summary>
        public static Func<WELL, WELL_PRESSURE?, decimal> GetDischargeTemperaturePlus50F { get; set; } = (well, wellPressure) => GetDischargeTemperature(well, wellPressure, 50.0);
    }
}

