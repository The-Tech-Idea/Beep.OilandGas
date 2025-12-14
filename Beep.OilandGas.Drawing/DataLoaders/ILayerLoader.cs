using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.DataLoaders
{
    /// <summary>
    /// Interface for loading layer/lithology data from various sources (PPDM38, WITSML, database, etc.).
    /// </summary>
    public interface ILayerLoader : IDataLoader<List<LayerData>>
    {
        /// <summary>
        /// Loads layers for a specific well.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>List of layer data.</returns>
        List<LayerData> LoadLayers(string wellIdentifier, LayerLoadConfiguration configuration = null);

        /// <summary>
        /// Loads layers asynchronously.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>List of layer data.</returns>
        Task<List<LayerData>> LoadLayersAsync(string wellIdentifier, LayerLoadConfiguration configuration = null);

        /// <summary>
        /// Loads layers with result object.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The load result.</returns>
        DataLoadResult<List<LayerData>> LoadLayersWithResult(string wellIdentifier, LayerLoadConfiguration configuration = null);

        /// <summary>
        /// Loads layers with result object asynchronously.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The load result.</returns>
        Task<DataLoadResult<List<LayerData>>> LoadLayersWithResultAsync(string wellIdentifier, LayerLoadConfiguration configuration = null);

        /// <summary>
        /// Gets available lithology types.
        /// </summary>
        /// <returns>List of available lithology types.</returns>
        List<string> GetAvailableLithologies();

        /// <summary>
        /// Gets available facies types.
        /// </summary>
        /// <returns>List of available facies types.</returns>
        List<string> GetAvailableFacies();
    }

    /// <summary>
    /// Configuration for loading layer data.
    /// </summary>
    public class LayerLoadConfiguration
    {
        /// <summary>
        /// Gets or sets the minimum depth filter.
        /// </summary>
        public double MinDepth { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum depth filter.
        /// </summary>
        public double MaxDepth { get; set; } = 0;

        /// <summary>
        /// Gets or sets specific lithology types to load (null = load all).
        /// </summary>
        public List<string> LithologyTypes { get; set; } = null;

        /// <summary>
        /// Gets or sets specific facies types to load (null = load all).
        /// </summary>
        public List<string> FaciesTypes { get; set; } = null;

        /// <summary>
        /// Gets or sets whether to load only pay zones.
        /// </summary>
        public bool PayZonesOnly { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to include color codes.
        /// </summary>
        public bool IncludeColorCodes { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include pattern types.
        /// </summary>
        public bool IncludePatternTypes { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to validate data after loading.
        /// </summary>
        public bool ValidateAfterLoad { get; set; } = false;
    }
}

