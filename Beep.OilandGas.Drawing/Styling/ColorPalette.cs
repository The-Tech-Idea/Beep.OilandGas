using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.Styling
{
    /// <summary>
    /// Provides industry-standard color palettes for oil and gas visualizations.
    /// </summary>
    public static class ColorPalette
    {
        /// <summary>
        /// Standard well schematic colors.
        /// </summary>
        public static class WellSchematic
        {
            public static SKColor Wellbore => SKColors.Black;
            public static SKColor Casing => SKColors.Gray;
            public static SKColor Tubing => SKColors.Blue;
            public static SKColor Equipment => SKColors.DarkGreen;
            public static SKColor Perforation => SKColors.Red;
            public static SKColor Cement => SKColors.LightGray;
            public static SKColor Formation => SKColors.Brown;
        }

        /// <summary>
        /// Standard log display colors.
        /// </summary>
        public static class LogDisplay
        {
            public static SKColor GammaRay => SKColors.Black;
            public static SKColor Resistivity => SKColors.Red;
            public static SKColor Porosity => SKColors.Blue;
            public static SKColor Density => SKColors.Green;
            public static SKColor Neutron => SKColors.Orange;
            public static SKColor Sonic => SKColors.Purple;
            public static SKColor Caliper => SKColors.DarkGray;
        }

        /// <summary>
        /// Standard reservoir visualization colors.
        /// </summary>
        public static class Reservoir
        {
            public static SKColor Oil => SKColors.Black;
            public static SKColor Gas => SKColors.Red;
            public static SKColor Water => SKColors.Blue;
            public static SKColor OilWaterContact => SKColors.Orange;
            public static SKColor GasOilContact => SKColors.Yellow;
            public static SKColor Formation => SKColors.Brown;
        }

        /// <summary>
        /// Production chart colors.
        /// </summary>
        public static class Production
        {
            public static SKColor OilProduction => SKColors.Black;
            public static SKColor GasProduction => SKColors.Red;
            public static SKColor WaterProduction => SKColors.Blue;
            public static SKColor WaterCut => SKColors.Green;
            public static SKColor GasOilRatio => SKColors.Orange;
        }

        /// <summary>
        /// Gets a color by name from standard palettes.
        /// </summary>
        /// <param name="category">The color category (WellSchematic, LogDisplay, Reservoir, Production).</param>
        /// <param name="colorName">The color name.</param>
        /// <returns>The color, or White if not found.</returns>
        public static SKColor GetColor(string category, string colorName)
        {
            return category?.ToLower() switch
            {
                "wellschematic" => colorName?.ToLower() switch
                {
                    "wellbore" => WellSchematic.Wellbore,
                    "casing" => WellSchematic.Casing,
                    "tubing" => WellSchematic.Tubing,
                    "equipment" => WellSchematic.Equipment,
                    "perforation" => WellSchematic.Perforation,
                    "cement" => WellSchematic.Cement,
                    "formation" => WellSchematic.Formation,
                    _ => SKColors.White
                },
                "logdisplay" => colorName?.ToLower() switch
                {
                    "gammaray" => LogDisplay.GammaRay,
                    "resistivity" => LogDisplay.Resistivity,
                    "porosity" => LogDisplay.Porosity,
                    "density" => LogDisplay.Density,
                    "neutron" => LogDisplay.Neutron,
                    "sonic" => LogDisplay.Sonic,
                    "caliper" => LogDisplay.Caliper,
                    _ => SKColors.White
                },
                "reservoir" => colorName?.ToLower() switch
                {
                    "oil" => Reservoir.Oil,
                    "gas" => Reservoir.Gas,
                    "water" => Reservoir.Water,
                    "oilwatercontact" => Reservoir.OilWaterContact,
                    "gasoilcontact" => Reservoir.GasOilContact,
                    "formation" => Reservoir.Formation,
                    _ => SKColors.White
                },
                "production" => colorName?.ToLower() switch
                {
                    "oilproduction" => Production.OilProduction,
                    "gasproduction" => Production.GasProduction,
                    "waterproduction" => Production.WaterProduction,
                    "watercut" => Production.WaterCut,
                    "gasoilratio" => Production.GasOilRatio,
                    _ => SKColors.White
                },
                _ => SKColors.White
            };
        }

        /// <summary>
        /// Gets a gradient color palette for continuous data.
        /// </summary>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Maximum value.</param>
        /// <param name="value">Current value.</param>
        /// <param name="startColor">Start color (for min value).</param>
        /// <param name="endColor">End color (for max value).</param>
        /// <returns>Interpolated color.</returns>
        public static SKColor GetGradientColor(
            double minValue,
            double maxValue,
            double value,
            SKColor startColor,
            SKColor endColor)
        {
            if (maxValue <= minValue)
                return startColor;

            double t = Math.Max(0, Math.Min(1, (value - minValue) / (maxValue - minValue)));

            byte r = (byte)(startColor.Red + t * (endColor.Red - startColor.Red));
            byte g = (byte)(startColor.Green + t * (endColor.Green - startColor.Green));
            byte b = (byte)(startColor.Blue + t * (endColor.Blue - startColor.Blue));
            byte a = (byte)(startColor.Alpha + t * (endColor.Alpha - startColor.Alpha));

            return new SKColor(r, g, b, a);
        }
    }
}

