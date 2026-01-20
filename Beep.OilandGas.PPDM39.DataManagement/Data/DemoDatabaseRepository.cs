using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.Data
{
    /// <summary>
    /// Repository for demo database metadata storage
    /// Uses JSON file storage in the demo directory
    /// </summary>
    public class DemoDatabaseRepository
    {
        private readonly string _metadataFilePath;
        private readonly ILogger<DemoDatabaseRepository> _logger;
        private readonly object _lock = new object();

        public DemoDatabaseRepository(string storagePath, ILogger<DemoDatabaseRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Ensure storage directory exists
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            _metadataFilePath = Path.Combine(storagePath, "demo_metadata.json");
        }

        /// <summary>
        /// Add demo database metadata
        /// </summary>
        public async Task AddAsync(DemoDatabaseMetadata metadata)
        {
            lock (_lock)
            {
                var allMetadata = LoadMetadata();
                allMetadata.Add(metadata);
                SaveMetadata(allMetadata);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Get all demo database metadata
        /// </summary>
        public List<DemoDatabaseMetadata> GetAll()
        {
            lock (_lock)
            {
                return LoadMetadata();
            }
        }

        /// <summary>
        /// Get demo databases for a specific user
        /// </summary>
        public List<DemoDatabaseMetadata> GetByUserId(string userId)
        {
            lock (_lock)
            {
                return LoadMetadata().Where(m => m.UserId == userId).ToList();
            }
        }

        /// <summary>
        /// Get demo database by connection name
        /// </summary>
        public DemoDatabaseMetadata? GetByConnectionName(string connectionName)
        {
            lock (_lock)
            {
                return LoadMetadata().FirstOrDefault(m => m.ConnectionName == connectionName);
            }
        }

        /// <summary>
        /// Get expired demo databases
        /// </summary>
        public List<DemoDatabaseMetadata> GetExpired()
        {
            lock (_lock)
            {
                return LoadMetadata().Where(m => m.IsExpired).ToList();
            }
        }

        /// <summary>
        /// Delete demo database metadata
        /// </summary>
        public async Task<bool> DeleteAsync(string connectionName)
        {
            lock (_lock)
            {
                var allMetadata = LoadMetadata();
                var metadata = allMetadata.FirstOrDefault(m => m.ConnectionName == connectionName);
                if (metadata == null)
                {
                    return false;
                }

                allMetadata.Remove(metadata);
                SaveMetadata(allMetadata);
            }
            await Task.CompletedTask;
            return true;
        }

        /// <summary>
        /// Delete multiple demo database metadata entries
        /// </summary>
        public async Task<int> DeleteMultipleAsync(List<string> connectionNames)
        {
            lock (_lock)
            {
                var allMetadata = LoadMetadata();
                var toRemove = allMetadata.Where(m => connectionNames.Contains(m.ConnectionName)).ToList();
                foreach (var item in toRemove)
                {
                    allMetadata.Remove(item);
                }
                SaveMetadata(allMetadata);
                return toRemove.Count;
            }
        }

        private List<DemoDatabaseMetadata> LoadMetadata()
        {
            if (!File.Exists(_metadataFilePath))
            {
                return new List<DemoDatabaseMetadata>();
            }

            try
            {
                var json = File.ReadAllText(_metadataFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<DemoDatabaseMetadata>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var metadata = JsonSerializer.Deserialize<List<DemoDatabaseMetadata>>(json, options);
                return metadata ?? new List<DemoDatabaseMetadata>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading demo database metadata from {FilePath}", _metadataFilePath);
                return new List<DemoDatabaseMetadata>();
            }
        }

        private void SaveMetadata(List<DemoDatabaseMetadata> metadata)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(metadata, options);
                File.WriteAllText(_metadataFilePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving demo database metadata to {FilePath}", _metadataFilePath);
                throw;
            }
        }
    }
}
