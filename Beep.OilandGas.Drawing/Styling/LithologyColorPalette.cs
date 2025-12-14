using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.Styling
{
    /// <summary>
    /// Provides industry-standard color palettes and patterns for lithology and facies visualization.
    /// Based on USGS Digital Cartographic Standard for Geologic Map Symbolization and industry conventions.
    /// </summary>
    public static class LithologyColorPalette
    {
        /// <summary>
        /// Standard lithology colors based on industry conventions.
        /// </summary>
        public static class LithologyColors
        {
            // Clastic Sedimentary Rocks
            public static SKColor Sandstone => new SKColor(255, 255, 0);      // Yellow
            public static SKColor Shale => new SKColor(128, 128, 128);       // Gray
            public static SKColor Siltstone => new SKColor(192, 192, 192);  // Light Gray
            public static SKColor Claystone => new SKColor(160, 160, 160);   // Medium Gray
            public static SKColor Mudstone => new SKColor(140, 140, 140);    // Dark Gray
            public static SKColor Conglomerate => new SKColor(255, 200, 0);   // Orange-Yellow
            public static SKColor Breccia => new SKColor(255, 150, 0);      // Orange

            // Carbonate Rocks
            public static SKColor Limestone => new SKColor(0, 128, 255);      // Blue
            public static SKColor Dolomite => new SKColor(0, 200, 255);      // Light Blue
            public static SKColor Chalk => new SKColor(200, 220, 255);      // Very Light Blue
            public static SKColor Marl => new SKColor(150, 200, 255);       // Blue-Gray

            // Evaporites
            public static SKColor Anhydrite => new SKColor(255, 192, 203);   // Pink
            public static SKColor Gypsum => new SKColor(255, 182, 193);     // Light Pink
            public static SKColor Halite => new SKColor(255, 255, 255);     // White
            public static SKColor Salt => new SKColor(240, 240, 240);       // Off-White

            // Igneous Rocks
            public static SKColor Basalt => new SKColor(64, 64, 64);        // Dark Gray
            public static SKColor Granite => new SKColor(192, 192, 192);   // Light Gray
            public static SKColor Volcanic => new SKColor(96, 96, 96);      // Medium Dark Gray

            // Metamorphic Rocks
            public static SKColor Schist => new SKColor(128, 96, 64);       // Brown
            public static SKColor Gneiss => new SKColor(160, 128, 96);      // Light Brown
            public static SKColor Quartzite => new SKColor(224, 224, 224);  // Very Light Gray

            // Other
            public static SKColor Coal => new SKColor(0, 0, 0);              // Black
            public static SKColor Chert => new SKColor(200, 200, 200);      // Light Gray
            public static SKColor Unknown => new SKColor(255, 255, 255);     // White
        }

        /// <summary>
        /// Standard facies colors.
        /// </summary>
        public static class FaciesColors
        {
            // Depositional Facies
            public static SKColor Channel => new SKColor(255, 255, 0);      // Yellow
            public static SKColor SheetSand => new SKColor(255, 220, 0);    // Orange-Yellow
            public static SKColor CrevasseSplay => new SKColor(255, 200, 100); // Light Orange
            public static SKColor Floodplain => new SKColor(192, 192, 192); // Light Gray
            public static SKColor Overbank => new SKColor(160, 160, 160);   // Medium Gray

            // Marine Facies
            public static SKColor Beach => new SKColor(255, 255, 200);     // Very Light Yellow
            public static SKColor Barrier => new SKColor(255, 240, 150);    // Light Yellow
            public static SKColor Lagoon => new SKColor(200, 240, 255);     // Light Blue
            public static SKColor Shelf => new SKColor(150, 200, 255);      // Blue
            public static SKColor DeepMarine => new SKColor(100, 150, 200); // Dark Blue

            // Carbonate Facies
            public static SKColor Reef => new SKColor(0, 255, 255);          // Cyan
            public static SKColor Shoal => new SKColor(0, 200, 255);        // Light Cyan
            public static SKColor CarbonateLagoon => new SKColor(150, 255, 255);     // Very Light Cyan
            public static SKColor TidalFlat => new SKColor(200, 255, 200);  // Light Green

            // Other
            public static SKColor Shale => new SKColor(128, 128, 128);      // Gray
            public static SKColor Unknown => new SKColor(255, 255, 255);    // White
        }

        /// <summary>
        /// Fluid zone colors.
        /// </summary>
        public static class FluidColors
        {
            public static SKColor Oil => new SKColor(0, 0, 0);              // Black
            public static SKColor Gas => new SKColor(255, 0, 0);           // Red
            public static SKColor Water => new SKColor(0, 0, 255);          // Blue
            public static SKColor OilWaterContact => new SKColor(255, 165, 0); // Orange
            public static SKColor GasOilContact => new SKColor(255, 255, 0);   // Yellow
            public static SKColor GasWaterContact => new SKColor(0, 255, 0);   // Green
            public static SKColor FreeWaterLevel => new SKColor(0, 0, 255);     // Blue (dashed)
        }

        /// <summary>
        /// Gets a color for a lithology type.
        /// </summary>
        public static SKColor GetLithologyColor(string lithology)
        {
            if (string.IsNullOrWhiteSpace(lithology))
                return LithologyColors.Unknown;

            var lith = lithology.ToLower().Trim();

            return lith switch
            {
                // Clastic
                "sandstone" or "sand" => LithologyColors.Sandstone,
                "shale" => LithologyColors.Shale,
                "siltstone" or "silt" => LithologyColors.Siltstone,
                "claystone" or "clay" => LithologyColors.Claystone,
                "mudstone" or "mud" => LithologyColors.Mudstone,
                "conglomerate" => LithologyColors.Conglomerate,
                "breccia" => LithologyColors.Breccia,

                // Carbonate
                "limestone" or "lime" => LithologyColors.Limestone,
                "dolomite" or "dolo" => LithologyColors.Dolomite,
                "chalk" => LithologyColors.Chalk,
                "marl" => LithologyColors.Marl,

                // Evaporite
                "anhydrite" or "anh" => LithologyColors.Anhydrite,
                "gypsum" or "gyp" => LithologyColors.Gypsum,
                "halite" => LithologyColors.Halite,
                "salt" => LithologyColors.Salt,

                // Igneous
                "basalt" => LithologyColors.Basalt,
                "granite" => LithologyColors.Granite,
                "volcanic" => LithologyColors.Volcanic,

                // Metamorphic
                "schist" => LithologyColors.Schist,
                "gneiss" => LithologyColors.Gneiss,
                "quartzite" => LithologyColors.Quartzite,

                // Other
                "coal" => LithologyColors.Coal,
                "chert" => LithologyColors.Chert,

                _ => LithologyColors.Unknown
            };
        }

        /// <summary>
        /// Gets a color for a facies type.
        /// </summary>
        public static SKColor GetFaciesColor(string facies)
        {
            if (string.IsNullOrWhiteSpace(facies))
                return FaciesColors.Unknown;

            var fac = facies.ToLower().Trim();

            return fac switch
            {
                // Fluvial
                "channel" or "ch" => FaciesColors.Channel,
                "sheetsand" or "sheet sand" or "ss" => FaciesColors.SheetSand,
                "crevassesplay" or "crevasse splay" or "cs" => FaciesColors.CrevasseSplay,
                "floodplain" or "fp" => FaciesColors.Floodplain,
                "overbank" or "ob" => FaciesColors.Overbank,

                // Marine
                "beach" => FaciesColors.Beach,
                "barrier" => FaciesColors.Barrier,
                "lagoon" => FaciesColors.Lagoon,
                "shelf" => FaciesColors.Shelf,
                "deepmarine" or "deep marine" => FaciesColors.DeepMarine,

                // Carbonate
                "reef" => FaciesColors.Reef,
                "shoal" => FaciesColors.Shoal,
                "carbonatelagoon" or "carbonate lagoon" => FaciesColors.CarbonateLagoon,
                "tidalflat" or "tidal flat" => FaciesColors.TidalFlat,

                // Other
                "shale" => FaciesColors.Shale,

                _ => FaciesColors.Unknown
            };
        }

        /// <summary>
        /// Gets a pattern type for a lithology (for rendering).
        /// </summary>
        public static LithologyPattern GetLithologyPattern(string lithology)
        {
            if (string.IsNullOrWhiteSpace(lithology))
                return LithologyPattern.Solid;

            var lith = lithology.ToLower().Trim();

            return lith switch
            {
                "sandstone" or "sand" => LithologyPattern.Dots,
                "shale" => LithologyPattern.HorizontalLines,
                "limestone" or "lime" => LithologyPattern.DiagonalLines,
                "dolomite" or "dolo" => LithologyPattern.DiagonalCrossHatch,
                "anhydrite" or "anh" => LithologyPattern.VerticalLines,
                "salt" or "halite" => LithologyPattern.Solid,
                "coal" => LithologyPattern.Solid,
                _ => LithologyPattern.Solid
            };
        }

        /// <summary>
        /// Gets a dictionary of all lithology colors.
        /// </summary>
        public static Dictionary<string, SKColor> GetAllLithologyColors()
        {
            return new Dictionary<string, SKColor>
            {
                { "Sandstone", LithologyColors.Sandstone },
                { "Shale", LithologyColors.Shale },
                { "Siltstone", LithologyColors.Siltstone },
                { "Claystone", LithologyColors.Claystone },
                { "Mudstone", LithologyColors.Mudstone },
                { "Conglomerate", LithologyColors.Conglomerate },
                { "Breccia", LithologyColors.Breccia },
                { "Limestone", LithologyColors.Limestone },
                { "Dolomite", LithologyColors.Dolomite },
                { "Chalk", LithologyColors.Chalk },
                { "Marl", LithologyColors.Marl },
                { "Anhydrite", LithologyColors.Anhydrite },
                { "Gypsum", LithologyColors.Gypsum },
                { "Halite", LithologyColors.Halite },
                { "Salt", LithologyColors.Salt },
                { "Basalt", LithologyColors.Basalt },
                { "Granite", LithologyColors.Granite },
                { "Volcanic", LithologyColors.Volcanic },
                { "Schist", LithologyColors.Schist },
                { "Gneiss", LithologyColors.Gneiss },
                { "Quartzite", LithologyColors.Quartzite },
                { "Coal", LithologyColors.Coal },
                { "Chert", LithologyColors.Chert }
            };
        }
    }

    /// <summary>
    /// Pattern types for lithology rendering.
    /// </summary>
    public enum LithologyPattern
    {
        /// <summary>
        /// Solid fill (no pattern).
        /// </summary>
        Solid,

        /// <summary>
        /// Horizontal lines.
        /// </summary>
        HorizontalLines,

        /// <summary>
        /// Vertical lines.
        /// </summary>
        VerticalLines,

        /// <summary>
        /// Diagonal lines (forward slash).
        /// </summary>
        DiagonalLines,

        /// <summary>
        /// Diagonal cross-hatch.
        /// </summary>
        DiagonalCrossHatch,

        /// <summary>
        /// Dots.
        /// </summary>
        Dots,

        /// <summary>
        /// Cross-hatch (horizontal and vertical).
        /// </summary>
        CrossHatch,

        /// <summary>
        /// Brick pattern.
        /// </summary>
        Brick,

        /// <summary>
        /// Zigzag pattern.
        /// </summary>
        Zigzag
    }
}

