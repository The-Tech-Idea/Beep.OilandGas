using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.DataLoaders
{
    /// <summary>
    /// Interface for loading reservoir data from various sources (RESQML, PPDM38, database, etc.).
    /// </summary>
    public interface IReservoirLoader : IDataLoader<ReservoirData>
    {
        /// <summary>
        /// Loads reservoir data for a specific reservoir identifier.
        /// </summary>
        /// <param name="reservoirId">The reservoir identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The reservoir data.</returns>
        ReservoirData LoadReservoir(string reservoirId, ReservoirLoadConfiguration configuration = null);

        /// <summary>
        /// Loads reservoir data asynchronously.
        /// </summary>
        /// <param name="reservoirId">The reservoir identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The reservoir data.</returns>
        Task<ReservoirData> LoadReservoirAsync(string reservoirId, ReservoirLoadConfiguration configuration = null);

        /// <summary>
        /// Loads reservoir data with result object.
        /// </summary>
        /// <param name="reservoirId">The reservoir identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The load result.</returns>
        DataLoadResult<ReservoirData> LoadReservoirWithResult(string reservoirId, ReservoirLoadConfiguration configuration = null);

        /// <summary>
        /// Loads reservoir data with result object asynchronously.
        /// </summary>
        /// <param name="reservoirId">The reservoir identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The load result.</returns>
        Task<DataLoadResult<ReservoirData>> LoadReservoirWithResultAsync(string reservoirId, ReservoirLoadConfiguration configuration = null);

        /// <summary>
        /// Loads layers for a specific reservoir.
        /// </summary>
        /// <param name="reservoirId">The reservoir identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>List of layer data.</returns>
        List<LayerData> LoadLayers(string reservoirId, ReservoirLoadConfiguration configuration = null);

        /// <summary>
        /// Loads layers asynchronously.
        /// </summary>
        /// <param name="reservoirId">The reservoir identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>List of layer data.</returns>
        Task<List<LayerData>> LoadLayersAsync(string reservoirId, ReservoirLoadConfiguration configuration = null);

        /// <summary>
        /// Loads fluid contacts for a reservoir.
        /// </summary>
        /// <param name="reservoirId">The reservoir identifier.</param>
        /// <returns>The fluid contacts.</returns>
        FluidContacts LoadFluidContacts(string reservoirId);

        /// <summary>
        /// Loads fluid contacts asynchronously.
        /// </summary>
        /// <param name="reservoirId">The reservoir identifier.</param>
        /// <returns>The fluid contacts.</returns>
        Task<FluidContacts> LoadFluidContactsAsync(string reservoirId);

        /// <summary>
        /// Gets available reservoir identifiers.
        /// </summary>
        /// <returns>List of available reservoir identifiers.</returns>
        List<string> GetAvailableReservoirs();

        /// <summary>
        /// Gets available reservoir identifiers asynchronously.
        /// </summary>
        /// <returns>List of available reservoir identifiers.</returns>
        Task<List<string>> GetAvailableReservoirsAsync();
    }

    /// <summary>
    /// Configuration for loading reservoir data.
    /// </summary>
    public class ReservoirLoadConfiguration
    {
        /// <summary>
        /// Gets or sets whether to load layer geometry.
        /// </summary>
        public bool LoadGeometry { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to load layer properties (porosity, permeability, etc.).
        /// </summary>
        public bool LoadProperties { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to load fluid contacts.
        /// </summary>
        public bool LoadFluidContacts { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum depth filter (TVDSS).
        /// </summary>
        public double MinDepth { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum depth filter (TVDSS).
        /// </summary>
        public double MaxDepth { get; set; } = 0;

        /// <summary>
        /// Gets or sets whether to filter only pay zones.
        /// </summary>
        public bool PayZonesOnly { get; set; } = false;

        /// <summary>
        /// Gets or sets specific layer IDs to load (null = load all).
        /// </summary>
        public List<string> LayerIds { get; set; } = null;

        /// <summary>
        /// Gets or sets whether to validate data after loading.
        /// </summary>
        public bool ValidateAfterLoad { get; set; } = false;
    }
}

