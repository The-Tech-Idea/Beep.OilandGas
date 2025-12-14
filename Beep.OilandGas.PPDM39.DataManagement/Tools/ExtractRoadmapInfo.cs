using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Extracts subject area and module information from PPDM 3.9 Roadmaps HTML
    /// </summary>
    public class ExtractRoadmapInfo
    {
        /// <summary>
        /// Extracts text content from HTML (removes tags and extracts visible text)
        /// </summary>
        public static string ExtractTextFromHtml(string htmlPath)
        {
            var html = File.ReadAllText(htmlPath, Encoding.UTF8);
            
            // Remove script and style tags
            html = Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<style[^>]*>[\s\S]*?</style>", "", RegexOptions.IgnoreCase);
            
            // Extract text from span elements (the HTML uses spans for text)
            var spanPattern = @"<span[^>]*>([^<]+)</span>";
            var matches = Regex.Matches(html, spanPattern, RegexOptions.IgnoreCase);
            
            var textBuilder = new StringBuilder();
            foreach (Match match in matches)
            {
                var text = match.Groups[1].Value;
                // Clean up HTML entities
                text = text.Replace("&nbsp;", " ").Replace("&amp;", "&");
                text = text.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    textBuilder.AppendLine(text);
                }
            }
            
            return textBuilder.ToString();
        }

        /// <summary>
        /// Parses roadmap text to extract subject areas and modules
        /// </summary>
        public static Dictionary<string, List<string>> ExtractSubjectAreasAndModules(string text)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            
            // Canonical subject area names (from PPDMTreeBuilder)
            var canonicalSubjectAreas = new[]
            {
                "Support Modules",
                "Data Management & Units of Measure",
                "Stratigraphy, Lithology & Sample Analysis",
                "Production & Reserves",
                "Wells",
                "Product Management & Classifications",
                "Seismic",
                "Support Facilities",
                "Operations Support",
                "Paleontology & Biostratigraphy",
                "Land & Legal Management"
            };

            // Map variations to canonical names
            var subjectAreaVariations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Support Modules", "Support Modules" },
                { "Data Management & Units of Measure", "Data Management & Units of Measure" },
                { "Data Management & Units", "Data Management & Units of Measure" },
                { "Data Management", "Data Management & Units of Measure" },
                { "Units of Measure", "Data Management & Units of Measure" },
                { "Stratigraphy, Lithology & Sample Analysis", "Stratigraphy, Lithology & Sample Analysis" },
                { "Stratigraphy, Lithology and Sample Analysis", "Stratigraphy, Lithology & Sample Analysis" },
                { "Stratigraphy Lithology & Sample Analysis", "Stratigraphy, Lithology & Sample Analysis" },
                { "Production & Reserves", "Production & Reserves" },
                { "Production and Reserves", "Production & Reserves" },
                { "Wells", "Wells" },
                { "Product Management & Classifications", "Product Management & Classifications" },
                { "Product Management and Classifications", "Product Management & Classifications" },
                { "Product Management", "Product Management & Classifications" },
                { "Classifications", "Product Management & Classifications" },
                { "Seismic", "Seismic" },
                { "Support Facilities", "Support Facilities" },
                { "Operations Support", "Operations Support" },
                { "Paleontology & Biostratigraphy", "Paleontology & Biostratigraphy" },
                { "Paleontology and Biostratigraphy", "Paleontology & Biostratigraphy" },
                { "Paleontology", "Paleontology & Biostratigraphy" },
                { "Biostratigraphy", "Paleontology & Biostratigraphy" },
                { "Land & Legal Management", "Land & Legal Management" },
                { "Land and Legal Management", "Land & Legal Management" },
                { "Land Management", "Land & Legal Management" },
                { "Legal Management", "Land & Legal Management" }
            };

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            string currentSubjectArea = null;
            var currentModules = new List<string>();

            foreach (var line in lines)
            {
                // Check if line contains a subject area (try canonical names first, then variations)
                string matchedSubjectArea = null;
                
                // First try exact canonical matches
                foreach (var canonical in canonicalSubjectAreas)
                {
                    if (line.Contains(canonical, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedSubjectArea = canonical;
                        break;
                    }
                }
                
                // If no canonical match, try variations
                if (matchedSubjectArea == null)
                {
                    foreach (var variation in subjectAreaVariations.Keys)
                    {
                        if (line.Contains(variation, StringComparison.OrdinalIgnoreCase))
                        {
                            matchedSubjectArea = subjectAreaVariations[variation];
                            break;
                        }
                    }
                }

                if (matchedSubjectArea != null)
                {
                    // Save previous subject area if exists
                    if (currentSubjectArea != null && currentModules.Count > 0)
                    {
                        if (!result.ContainsKey(currentSubjectArea))
                        {
                            result[currentSubjectArea] = new List<string>();
                        }
                        // Merge modules (avoid duplicates)
                        foreach (var module in currentModules)
                        {
                            if (!result[currentSubjectArea].Contains(module, StringComparer.OrdinalIgnoreCase))
                            {
                                result[currentSubjectArea].Add(module);
                            }
                        }
                    }
                    
                    currentSubjectArea = matchedSubjectArea;
                    currentModules.Clear();
                }

                // Check for module patterns (numbers followed by module name and colon)
                // Pattern: "9 Additives Catalogue:" or "11 Data Management:"
                var moduleMatch = Regex.Match(line, @"^\d+\s+([^:]+):\s*(.+)$");
                if (moduleMatch.Success)
                {
                    var moduleName = moduleMatch.Groups[1].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(moduleName) && currentSubjectArea != null)
                    {
                        if (!currentModules.Contains(moduleName, StringComparer.OrdinalIgnoreCase))
                        {
                            currentModules.Add(moduleName);
                        }
                    }
                }
            }

            // Save last subject area
            if (currentSubjectArea != null && currentModules.Count > 0)
            {
                if (!result.ContainsKey(currentSubjectArea))
                {
                    result[currentSubjectArea] = new List<string>();
                }
                foreach (var module in currentModules)
                {
                    if (!result[currentSubjectArea].Contains(module, StringComparer.OrdinalIgnoreCase))
                    {
                        result[currentSubjectArea].Add(module);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Main extraction method
        /// </summary>
        public static void ExtractAndSave(string htmlPath, string outputJsonPath)
        {
            Console.WriteLine("Extracting text from HTML...");
            var text = ExtractTextFromHtml(htmlPath);
            
            Console.WriteLine("Parsing subject areas and modules...");
            var roadmap = ExtractSubjectAreasAndModules(text);
            
            // Create output directory if needed
            var outputDir = Path.GetDirectoryName(outputJsonPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Save as JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(roadmap, options);
            File.WriteAllText(outputJsonPath, json, Encoding.UTF8);

            Console.WriteLine($"Extracted {roadmap.Count} subject areas");
            foreach (var kvp in roadmap)
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value.Count} modules");
            }
            Console.WriteLine($"Output written to: {outputJsonPath}");
        }
    }
}

