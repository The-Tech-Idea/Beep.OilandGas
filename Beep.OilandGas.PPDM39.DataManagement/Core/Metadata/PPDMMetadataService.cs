using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Metadata
{
    /// <summary>
    /// Unified service for PPDM metadata management
    /// Provides caching, JSON loading, and comprehensive query methods
    /// </summary>
    public class PPDMMetadataService : IPPDMMetadataRepository
    {
        private readonly ILogger<PPDMMetadataService> _logger;
        private readonly string? _jsonMetadataPath;
        private readonly Lazy<Dictionary<string, PPDMTableMetadata>> _metadataCache;
        private readonly object _loadLock = new object();

        /// <summary>
        /// Initializes a new instance of PPDMMetadataService
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="jsonMetadataPath">Optional path to PPDM39Metadata.json file. If not provided, will try to locate automatically or use generated class.</param>
        public PPDMMetadataService(ILogger<PPDMMetadataService> logger, string? jsonMetadataPath = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonMetadataPath = jsonMetadataPath;
            _metadataCache = new Lazy<Dictionary<string, PPDMTableMetadata>>(LoadMetadata, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Gets the cached metadata dictionary
        /// </summary>
        private Dictionary<string, PPDMTableMetadata> Metadata => _metadataCache.Value;

        #region IPPDMMetadataRepository Implementation

        public Task<PPDMTableMetadata> GetTableMetadataAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                _logger.LogWarning("GetTableMetadataAsync called with null or empty table name");
                return Task.FromResult<PPDMTableMetadata>(null);
            }

            Metadata.TryGetValue(tableName.ToUpper(), out var tableMeta);
            if (tableMeta == null)
            {
                _logger.LogDebug("Table metadata not found: {TableName}", tableName);
            }
            return Task.FromResult(tableMeta);
        }

        public Task<IEnumerable<PPDMTableMetadata>> GetTablesByModuleAsync(string module)
        {
            if (string.IsNullOrWhiteSpace(module))
            {
                return Task.FromResult(Enumerable.Empty<PPDMTableMetadata>());
            }

            var tables = Metadata.Values
                .Where(m => m.Module != null && m.Module.Equals(module, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            _logger.LogDebug("Found {Count} tables in module: {Module}", tables.Count, module);
            return Task.FromResult<IEnumerable<PPDMTableMetadata>>(tables);
        }

        public Task<IEnumerable<PPDMForeignKey>> GetForeignKeysAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return Task.FromResult(Enumerable.Empty<PPDMForeignKey>());
            }

            var metadata = Metadata.GetValueOrDefault(tableName.ToUpper());
            return Task.FromResult(metadata?.ForeignKeys ?? Enumerable.Empty<PPDMForeignKey>());
        }

        public Task<IEnumerable<PPDMTableMetadata>> GetReferencingTablesAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return Task.FromResult(Enumerable.Empty<PPDMTableMetadata>());
            }

            var referencing = Metadata.Values
                .Where(m => m.ForeignKeys != null && m.ForeignKeys.Any(fk => 
                    fk.ReferencedTable != null && fk.ReferencedTable.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            
            _logger.LogDebug("Found {Count} tables referencing {TableName}", referencing.Count, tableName);
            return Task.FromResult<IEnumerable<PPDMTableMetadata>>(referencing);
        }

        public Task<string> GetPrimaryKeyColumnAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return Task.FromResult<string>(null);
            }

            var metadata = Metadata.GetValueOrDefault(tableName.ToUpper());
            return Task.FromResult(metadata?.PrimaryKeyColumn);
        }

        public Task<string> GetEntityTypeNameAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return Task.FromResult<string>(null);
            }

            var metadata = Metadata.GetValueOrDefault(tableName.ToUpper());
            return Task.FromResult(metadata?.EntityTypeName);
        }

        public Task<bool> HasCommonColumnsAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return Task.FromResult(false);
            }

            var metadata = Metadata.GetValueOrDefault(tableName.ToUpper());
            return Task.FromResult(metadata?.CommonColumns?.Any() ?? false);
        }

        public Task<IEnumerable<string>> GetModulesAsync()
        {
            var modules = Metadata.Values
                .Where(m => !string.IsNullOrWhiteSpace(m.Module))
                .Select(m => m.Module)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            
            _logger.LogDebug("Found {Count} distinct modules", modules.Count);
            return Task.FromResult<IEnumerable<string>>(modules);
        }

        #endregion

        #region Extended Interface Methods

        public Task<IEnumerable<string>> GetPrimaryKeyColumnsAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }

            var metadata = Metadata.GetValueOrDefault(tableName.ToUpper());
            if (metadata == null || string.IsNullOrWhiteSpace(metadata.PrimaryKeyColumn))
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }

            // Parse comma-separated primary key columns
            var columns = metadata.PrimaryKeyColumn
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToList();

            return Task.FromResult<IEnumerable<string>>(columns);
        }

        public Task<IEnumerable<PPDMTableMetadata>> GetTablesBySubjectAreaAsync(string subjectArea)
        {
            if (string.IsNullOrWhiteSpace(subjectArea))
            {
                return Task.FromResult(Enumerable.Empty<PPDMTableMetadata>());
            }

            var tables = Metadata.Values
                .Where(m => !string.IsNullOrWhiteSpace(m.SubjectArea) && 
                           m.SubjectArea.Contains(subjectArea, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            _logger.LogDebug("Found {Count} tables in subject area: {SubjectArea}", tables.Count, subjectArea);
            return Task.FromResult<IEnumerable<PPDMTableMetadata>>(tables);
        }

        public Task<IEnumerable<PPDMTableMetadata>> GetTablesByPatternAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                return Task.FromResult(Enumerable.Empty<PPDMTableMetadata>());
            }

            // Convert wildcard pattern to regex
            // * matches any sequence of characters
            // ? matches any single character
            var regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            var tables = Metadata.Values
                .Where(m => regex.IsMatch(m.TableName))
                .ToList();
            
            _logger.LogDebug("Found {Count} tables matching pattern: {Pattern}", tables.Count, pattern);
            return Task.FromResult<IEnumerable<PPDMTableMetadata>>(tables);
        }

        public Task<bool> IsCompositeKeyAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return Task.FromResult(false);
            }

            var metadata = Metadata.GetValueOrDefault(tableName.ToUpper());
            if (metadata == null || string.IsNullOrWhiteSpace(metadata.PrimaryKeyColumn))
            {
                return Task.FromResult(false);
            }

            // Check if primary key contains comma (indicating composite key)
            var isComposite = metadata.PrimaryKeyColumn.Contains(',', StringComparison.Ordinal);
            return Task.FromResult(isComposite);
        }

        public Task RefreshMetadataAsync()
        {
            _logger.LogInformation("Refreshing metadata cache");
            
            lock (_loadLock)
            {
                // Force reload by accessing the value property which will trigger reload
                // Note: Lazy<T> doesn't support reset, so we need to create a new Lazy instance
                // For now, we'll just log - in a production scenario, you might want to use a different caching strategy
                _logger.LogWarning("Metadata cache refresh requested, but Lazy<T> cache cannot be reset. Consider restarting the service or using a different caching mechanism.");
            }

            return Task.CompletedTask;
        }

        public Task<Dictionary<string, PPDMTableMetadata>> GetAllMetadataAsync()
        {
            // Return a copy to prevent external modification
            var copy = new Dictionary<string, PPDMTableMetadata>(Metadata, StringComparer.OrdinalIgnoreCase);
            return Task.FromResult(copy);
        }

        #endregion

        #region Private Loading Methods

        /// <summary>
        /// Loads metadata from the configured source
        /// </summary>
        private Dictionary<string, PPDMTableMetadata> LoadMetadata()
        {
            lock (_loadLock)
            {
                _logger.LogInformation("Loading PPDM metadata");

                // Try loading from JSON file first
                if (!string.IsNullOrWhiteSpace(_jsonMetadataPath))
                {
                    var metadata = LoadFromJson(_jsonMetadataPath);
                    if (metadata != null && metadata.Count > 0)
                    {
                        _logger.LogInformation("Loaded {Count} tables from JSON file: {Path}", metadata.Count, _jsonMetadataPath);
                        return metadata;
                    }
                }

                // Try loading from embedded resource or file system
                var jsonMetadata = TryLoadJsonMetadata();
                if (jsonMetadata != null && jsonMetadata.Count > 0)
                {
                    _logger.LogInformation("Loaded {Count} tables from JSON (auto-detected)", jsonMetadata.Count);
                    return jsonMetadata;
                }

                // Fallback to generated class
                _logger.LogInformation("JSON metadata not found, falling back to generated class");
                var generatedMetadata = LoadFromGeneratedClass();
                _logger.LogInformation("Loaded {Count} tables from generated class", generatedMetadata.Count);
                return generatedMetadata;
            }
        }

        /// <summary>
        /// Loads metadata from JSON file
        /// </summary>
        private Dictionary<string, PPDMTableMetadata> LoadFromJson(string jsonPath)
        {
            try
            {
                if (!File.Exists(jsonPath))
                {
                    _logger.LogWarning("JSON metadata file not found: {Path}", jsonPath);
                    return null;
                }

                var jsonContent = File.ReadAllText(jsonPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                var metadata = JsonSerializer.Deserialize<Dictionary<string, PPDMTableMetadata>>(jsonContent, options);
                
                if (metadata != null)
                {
                    // Normalize keys to uppercase
                    var normalized = new Dictionary<string, PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase);
                    foreach (var kvp in metadata)
                    {
                        normalized[kvp.Key.ToUpper()] = kvp.Value;
                    }
                    return normalized;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading metadata from JSON file: {Path}", jsonPath);
                return null;
            }
        }

        /// <summary>
        /// Tries to load JSON metadata from embedded resource or file system
        /// </summary>
        private Dictionary<string, PPDMTableMetadata> TryLoadJsonMetadata()
        {
            try
            {
                // Try embedded resource first
                var assembly = typeof(PPDM39Metadata).Assembly;
                var allResources = assembly.GetManifestResourceNames();
                var resourceName = allResources.FirstOrDefault(r => 
                    r.EndsWith("PPDM39Metadata.json", StringComparison.OrdinalIgnoreCase));
                
                if (resourceName != null)
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        var json = reader.ReadToEnd();
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            ReadCommentHandling = JsonCommentHandling.Skip,
                            AllowTrailingCommas = true
                        };
                        var metadata = JsonSerializer.Deserialize<Dictionary<string, PPDMTableMetadata>>(json, options);
                        
                        if (metadata != null)
                        {
                            var normalized = new Dictionary<string, PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase);
                            foreach (var kvp in metadata)
                            {
                                normalized[kvp.Key.ToUpper()] = kvp.Value;
                            }
                            return normalized;
                        }
                    }
                }
                
                // Fallback to file system
                var candidates = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "PPDM39Metadata.json"),
                    Path.Combine(AppContext.BaseDirectory, "Core", "Metadata", "PPDM39Metadata.json"),
                    Path.Combine(AppContext.BaseDirectory, "Beep.OilandGas.PPDM39.DataManagement", "Core", "Metadata", "PPDM39Metadata.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Beep.OilandGas.PPDM39.DataManagement", "Core", "Metadata", "PPDM39Metadata.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Core", "Metadata", "PPDM39Metadata.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "PPDM39Metadata.json")
                };

                var jsonPath = candidates.FirstOrDefault(File.Exists);
                if (!string.IsNullOrWhiteSpace(jsonPath))
                {
                    return LoadFromJson(jsonPath);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to load JSON metadata");
                return null;
            }
        }

        /// <summary>
        /// Loads metadata from generated C# class
        /// </summary>
        private Dictionary<string, PPDMTableMetadata> LoadFromGeneratedClass()
        {
            try
            {
                var metadata = PPDM39Metadata.GetMetadata();
                if (metadata != null && metadata.Count > 0)
                {
                    // Normalize keys to uppercase
                    var normalized = new Dictionary<string, PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase);
                    foreach (var kvp in metadata)
                    {
                        normalized[kvp.Key.ToUpper()] = kvp.Value;
                    }
                    return normalized;
                }

                _logger.LogWarning("Generated class returned empty metadata");
                return new Dictionary<string, PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading metadata from generated class");
                return new Dictionary<string, PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase);
            }
        }

        #endregion
    }
}
