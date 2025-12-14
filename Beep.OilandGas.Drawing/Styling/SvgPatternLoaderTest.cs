using System;
using System.IO;

namespace Beep.OilandGas.Drawing.Styling
{
    /// <summary>
    /// Test/example class to verify SVG pattern loading from SED/IGM directories.
    /// </summary>
    public static class SvgPatternLoaderTest
    {
        /// <summary>
        /// Tests that patterns can be loaded from SED and IGM directories.
        /// </summary>
        public static void TestPatternLoading()
        {
            // Initialize loader - it should automatically find USGS-FGDC-master
            var loader = new SvgLithologyPatternLoader();

            // Test loading a sedimentary pattern (should use SED/)
            Console.WriteLine("Testing sedimentary pattern loading...");
            var sedPattern = loader.LoadSvgPattern("sed601");
            if (sedPattern != null && sedPattern.Picture != null)
            {
                Console.WriteLine("✓ Successfully loaded sed601 from SED/ directory");
                Console.WriteLine($"  Pattern bounds: {sedPattern.Picture.CullRect}");
            }
            else
            {
                Console.WriteLine("✗ Failed to load sed601 from SED/ directory");
            }

            // Test loading with custom colors
            var sedColoredPattern = loader.LoadSvgPattern("sed601", strokeColor: SkiaSharp.SKColors.Black, fillColor: SkiaSharp.SKColors.Yellow);
            if (sedColoredPattern != null && sedColoredPattern.Picture != null)
            {
                Console.WriteLine("✓ Successfully loaded sed601 with custom colors from SED/ directory");
            }
            else
            {
                Console.WriteLine("✗ Failed to load sed601 with custom colors from SED/ directory");
            }

            // Test loading an igneous pattern (should use IGM/)
            Console.WriteLine("\nTesting igneous pattern loading...");
            var igmPattern = loader.LoadSvgPattern("igm701");
            if (igmPattern != null && igmPattern.Picture != null)
            {
                Console.WriteLine("✓ Successfully loaded igm701 from IGM/ directory");
                Console.WriteLine($"  Pattern bounds: {igmPattern.Picture.CullRect}");
            }
            else
            {
                Console.WriteLine("✗ Failed to load igm701 from IGM/ directory");
            }

            // Test lithology name mapping
            Console.WriteLine("\nTesting lithology name mapping...");
            string patternCode = UsgsFgdcPatternMapping.GetPatternCode("sandstone");
            Console.WriteLine($"  'sandstone' maps to: {patternCode} (expected: sed601)");
            
            string patternCode2 = UsgsFgdcPatternMapping.GetPatternCode("granite");
            Console.WriteLine($"  'granite' maps to: {patternCode2} (expected: igm701)");

            // Test path construction
            Console.WriteLine("\nTesting path construction...");
            string baseDir = Path.Combine("Beep.OilandGas.Drawing", "LithologySymbols", "USGS-FGDC-master");
            string sedPath = UsgsFgdcPatternMapping.GetPatternFilePath(baseDir, "sed601");
            Console.WriteLine($"  SED path: {sedPath}");
            Console.WriteLine($"  File exists: {File.Exists(sedPath)}");

            string igmPath = UsgsFgdcPatternMapping.GetPatternFilePath(baseDir, "igm701");
            Console.WriteLine($"  IGM path: {igmPath}");
            Console.WriteLine($"  File exists: {File.Exists(igmPath)}");
        }
    }
}

