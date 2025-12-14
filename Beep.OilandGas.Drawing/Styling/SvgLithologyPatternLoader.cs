using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using SkiaSharp;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace Beep.OilandGas.Drawing.Styling
{
    /// <summary>
    /// Loads and manages SVG lithology patterns from files or embedded resources.
    /// Supports USGS and FGDC standard lithology patterns.
    /// </summary>
    public class SvgLithologyPatternLoader
    {
        private readonly Dictionary<string, SKSvg> svgCache;
        private readonly Dictionary<string, SKBitmap> patternCache;
        private readonly Assembly resourceAssembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgLithologyPatternLoader"/> class.
        /// </summary>
        /// <param name="resourceAssembly">Assembly containing embedded SVG resources (optional, defaults to executing assembly).</param>
        public SvgLithologyPatternLoader(Assembly resourceAssembly = null)
        {
            svgCache = new Dictionary<string, SKSvg>();
            patternCache = new Dictionary<string, SKBitmap>();
            this.resourceAssembly = resourceAssembly ?? Assembly.GetExecutingAssembly();
        }

        /// <summary>
        /// Gets the embedded resource name for a pattern.
        /// </summary>
        /// <param name="patternCode">The pattern code (e.g., "sed601", "igm701").</param>
        /// <returns>The resource name, or null if not found.</returns>
        private string GetEmbeddedResourceName(string patternCode)
        {
            if (string.IsNullOrEmpty(patternCode))
                return null;

            string directory = UsgsFgdcPatternMapping.GetPatternDirectory(patternCode);
            if (string.IsNullOrEmpty(directory))
                return null;

            // Resource name format: Beep.OilandGas.Drawing.LithologySymbols.USGS-FGDC-master.{directory}.{patternCode}.svg
            string resourceName = $"Beep.OilandGas.Drawing.LithologySymbols.USGS-FGDC-master.{directory}.{patternCode}.svg";
            return resourceName;
        }

        /// <summary>
        /// Loads an SVG pattern from an embedded resource.
        /// </summary>
        /// <param name="patternName">Name of the pattern or USGS-FGDC pattern code.</param>
        /// <param name="strokeColor">Optional stroke color to apply to parameterized SVGs.</param>
        /// <param name="fillColor">Optional fill color to apply to parameterized SVGs.</param>
        /// <returns>The loaded SVG, or null if not found.</returns>
        private SKSvg LoadSvgFromEmbeddedResource(string patternName, SKColor? strokeColor = null, SKColor? fillColor = null)
        {
            if (string.IsNullOrEmpty(patternName))
                return null;

            string cacheKey = strokeColor.HasValue 
                ? $"{patternName}_{strokeColor.Value}_{fillColor?.ToString() ?? "none"}" 
                : patternName;

            if (svgCache.ContainsKey(cacheKey))
                return svgCache[cacheKey];

            try
            {
                string resourceName = null;

                // Check if it's a USGS-FGDC pattern code
                if (UsgsFgdcPatternMapping.IsValidPatternCode(patternName))
                {
                    resourceName = GetEmbeddedResourceName(patternName);
                }
                else
                {
                    // Try common resource name patterns
                    string[] possibleNames = new[]
                    {
                        $"Beep.OilandGas.Drawing.LithologySymbols.USGS-FGDC-master.SED.{patternName}.svg",
                        $"Beep.OilandGas.Drawing.LithologySymbols.USGS-FGDC-master.IGM.{patternName}.svg",
                        $"Beep.OilandGas.Drawing.LithologyPatterns.{patternName}.svg"
                    };

                    foreach (var name in possibleNames)
                    {
                        if (resourceAssembly.GetManifestResourceStream(name) != null)
                        {
                            resourceName = name;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(resourceName))
                    return null;

                using (var stream = resourceAssembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        return null;

                    var svg = new SKSvg();
                    
                    // If colors are provided, process the SVG to replace parameters
                    if (strokeColor.HasValue)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            string svgContent = reader.ReadToEnd();
                            string strokeHex = SvgColorProcessor.SkColorToHex(strokeColor.Value);
                            string fillHex = fillColor.HasValue ? SvgColorProcessor.SkColorToHex(fillColor.Value) : "none";
                            string processedSvg = SvgColorProcessor.ProcessSvgColors(svgContent, strokeHex, fillHex);
                            
                            using (var processedStream = new MemoryStream(Encoding.UTF8.GetBytes(processedSvg)))
                            {
                                svg.Load(processedStream);
                            }
                        }
                    }
                    else
                    {
                        svg.Load(stream);
                    }
                    
                    svgCache[cacheKey] = svg;
                    return svg;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading SVG pattern {patternName} from embedded resource: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Loads an SVG pattern from embedded resources.
        /// Supports both USGS-FGDC pattern codes and simple pattern names.
        /// Uses SED/ and IGM/ directories (attributable colors).
        /// </summary>
        /// <param name="patternName">Name of the pattern or USGS-FGDC pattern code (e.g., "sed601", "igm701").</param>
        /// <param name="strokeColor">Optional stroke color to apply to the pattern.</param>
        /// <param name="fillColor">Optional fill color to apply to the pattern.</param>
        /// <returns>The loaded SVG, or null if not found.</returns>
        public SKSvg LoadSvgPattern(string patternName, SKColor? strokeColor = null, SKColor? fillColor = null)
        {
            if (string.IsNullOrEmpty(patternName))
                return null;

            // Try to load from embedded resources
            return LoadSvgFromEmbeddedResource(patternName, strokeColor, fillColor);
        }


        /// <summary>
        /// Creates a tiled pattern bitmap from an SVG pattern.
        /// </summary>
        /// <param name="patternName">Name of the pattern or USGS-FGDC pattern code.</param>
        /// <param name="baseColor">Base color to apply to the pattern.</param>
        /// <param name="patternSize">Size of the pattern tile in pixels.</param>
        /// <param name="strokeColor">Optional stroke color for the pattern lines.</param>
        /// <returns>A tiled pattern bitmap, or null if pattern not found.</returns>
        public SKBitmap CreatePatternBitmap(string patternName, SKColor baseColor, int patternSize = 32, SKColor? strokeColor = null)
        {
            // Use stroke color if provided, otherwise use a darker version of base color
            SKColor actualStrokeColor = strokeColor ?? new SKColor(
                (byte)(baseColor.Red * 0.7),
                (byte)(baseColor.Green * 0.7),
                (byte)(baseColor.Blue * 0.7));

            string cacheKey = $"{patternName}_{baseColor}_{actualStrokeColor}_{patternSize}";
            if (patternCache.ContainsKey(cacheKey))
                return patternCache[cacheKey];

            var svg = LoadSvgPattern(patternName, actualStrokeColor, baseColor);
            if (svg == null || svg.Picture == null)
                return null;

            try
            {
                // Create bitmap for pattern tile
                var bitmap = new SKBitmap(patternSize, patternSize);
                using (var canvas = new SKCanvas(bitmap))
                {
                    // Fill with base color
                    canvas.Clear(baseColor);

                    // Calculate scale to fit SVG in pattern size
                    var svgBounds = svg.Picture.CullRect;
                    float scaleX = patternSize / svgBounds.Width;
                    float scaleY = patternSize / svgBounds.Height;
                    float scale = Math.Min(scaleX, scaleY);

                    // Center the SVG in the pattern tile
                    float offsetX = (patternSize - svgBounds.Width * scale) / 2;
                    float offsetY = (patternSize - svgBounds.Height * scale) / 2;

                    canvas.Save();
                    canvas.Translate(offsetX, offsetY);
                    canvas.Scale(scale);

                    // Draw SVG
                    canvas.DrawPicture(svg.Picture);

                    canvas.Restore();
                }

                patternCache[cacheKey] = bitmap;
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating pattern bitmap for {patternName}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates a paint with an SVG pattern shader.
        /// </summary>
        /// <param name="patternName">Name of the SVG pattern or USGS-FGDC pattern code.</param>
        /// <param name="baseColor">Base color for the pattern.</param>
        /// <param name="patternSize">Pattern tile size.</param>
        /// <param name="strokeColor">Optional stroke color for the pattern lines.</param>
        /// <returns>A paint with the pattern shader, or null if pattern not found.</returns>
        public SKPaint CreateSvgPatternPaint(string patternName, SKColor baseColor, int patternSize = 32, SKColor? strokeColor = null)
        {
            var bitmap = CreatePatternBitmap(patternName, baseColor, patternSize, strokeColor);
            if (bitmap == null)
                return null;

            var shader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            return new SKPaint
            {
                Shader = shader,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        /// <summary>
        /// Maps lithology names to SVG pattern file names.
        /// First tries USGS-FGDC pattern codes, then falls back to simple names.
        /// </summary>
        /// <param name="lithology">Lithology name.</param>
        /// <returns>SVG pattern file name or USGS-FGDC pattern code, or null if not mapped.</returns>
        public static string GetSvgPatternName(string lithology)
        {
            if (string.IsNullOrEmpty(lithology))
                return null;

            // First try USGS-FGDC pattern mapping
            string patternCode = UsgsFgdcPatternMapping.GetPatternCode(lithology);
            if (!string.IsNullOrEmpty(patternCode))
            {
                return patternCode;
            }

            // Fall back to simple name mapping
            string normalized = lithology.ToLowerInvariant().Trim();
            return normalized switch
            {
                "sandstone" or "sand" => "sandstone",
                "shale" => "shale",
                "siltstone" or "silt" => "siltstone",
                "limestone" => "limestone",
                "dolomite" => "dolomite",
                "claystone" or "clay" => "claystone",
                "mudstone" or "mud" => "mudstone",
                "conglomerate" => "conglomerate",
                "breccia" => "breccia",
                "anhydrite" => "anhydrite",
                "gypsum" => "gypsum",
                "halite" or "salt" => "halite",
                "coal" => "coal",
                "basalt" => "basalt",
                "granite" => "granite",
                "schist" => "schist",
                "gneiss" => "gneiss",
                "quartzite" => "quartzite",
                "chalk" => "chalk",
                "marl" => "marl",
                _ => null
            };
        }

        /// <summary>
        /// Gets all available embedded resource names for debugging.
        /// </summary>
        /// <returns>List of all embedded resource names.</returns>
        public List<string> GetAvailableResources()
        {
            var resources = new List<string>();
            try
            {
                string[] resourceNames = resourceAssembly.GetManifestResourceNames();
                foreach (string name in resourceNames)
                {
                    if (name.Contains("LithologySymbols") || name.Contains(".svg"))
                    {
                        resources.Add(name);
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
            return resources;
        }

        /// <summary>
        /// Clears the SVG and pattern caches.
        /// </summary>
        public void ClearCache()
        {
            // SKSvg doesn't implement IDisposable, just clear the cache
            svgCache.Clear();

            foreach (var bitmap in patternCache.Values)
            {
                bitmap?.Dispose();
            }
            patternCache.Clear();
        }
    }
}

