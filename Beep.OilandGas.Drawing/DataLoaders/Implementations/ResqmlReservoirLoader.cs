using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Beep.OilandGas.Drawing.DataLoaders;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.DataLoaders.Implementations
{
    /// <summary>
    /// Implements <see cref="IReservoirLoader"/> for loading reservoir data from RESQML v2.2 files.
    /// RESQML (Reservoir Model) is an Energistics standard for exchanging reservoir geological models.
    /// </summary>
    public class ResqmlReservoirLoader : IReservoirLoader
    {
        private readonly string filePath;
        private XDocument document;
        private bool isConnected = false;

        // RESQML namespaces
        private readonly XNamespace resqml = "http://www.energistics.org/schemas/resqmlv2";
        private readonly XNamespace eml = "http://www.energistics.org/schemas/eml";
        private readonly XNamespace gml = "http://www.opengis.net/gml/3.2";

        public string DataSource => filePath;
        public bool IsConnected => isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResqmlReservoirLoader"/> class.
        /// </summary>
        /// <param name="filePath">The path to the RESQML XML file.</param>
        public ResqmlReservoirLoader(string filePath)
        {
            this.filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public bool Connect()
        {
            if (isConnected) return true;

            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"RESQML file not found: {filePath}");
                }

                document = XDocument.Load(filePath);
                isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to RESQML file: {ex.Message}");
                isConnected = false;
                return false;
            }
        }

        public async Task<bool> ConnectAsync()
        {
            if (isConnected) return true;

            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"RESQML file not found: {filePath}");
                }

                using (var stream = File.OpenRead(filePath))
                {
                    document = await XDocument.LoadAsync(stream, LoadOptions.None, default);
                }
                isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to RESQML file: {ex.Message}");
                isConnected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            document = null;
            isConnected = false;
        }

        public void Dispose()
        {
            Disconnect();
        }

        public bool ValidateConnection()
        {
            if (!isConnected || document == null) return false;

            try
            {
                // Check if document has RESQML root element
                var root = document.Root;
                return root != null && root.Name.LocalName.Contains("EpcExternalPartReference") ||
                       root?.Elements().Any(e => e.Name.Namespace == resqml) == true;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetAvailableIdentifiers()
        {
            if (!isConnected) Connect();
            if (!isConnected) return new List<string>();

            var identifiers = new List<string>();

            try
            {
                // Find all reservoir representation objects
                var reservoirObjects = document.Descendants()
                    .Where(e => e.Name.Namespace == resqml && 
                                (e.Name.LocalName.Contains("RepresentationSetRepresentation") ||
                                 e.Name.LocalName.Contains("Grid2dRepresentation") ||
                                 e.Name.LocalName.Contains("Grid3dRepresentation")));

                foreach (var obj in reservoirObjects)
                {
                    var uuid = obj.Attribute("uuid")?.Value;
                    var title = obj.Element(resqml + "Citation")?.Element(eml + "Title")?.Value;
                    
                    if (!string.IsNullOrEmpty(uuid))
                    {
                        identifiers.Add(uuid);
                    }
                    else if (!string.IsNullOrEmpty(title))
                    {
                        identifiers.Add(title);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting available identifiers: {ex.Message}");
            }

            return identifiers;
        }

        public Task<List<string>> GetAvailableIdentifiersAsync()
        {
            return Task.Run(() => GetAvailableIdentifiers());
        }

        public ReservoirData LoadReservoir(string reservoirId, ReservoirLoadConfiguration configuration = null)
        {
            var result = LoadReservoirWithResult(reservoirId, configuration);
            return result.Data;
        }

        public async Task<ReservoirData> LoadReservoirAsync(string reservoirId, ReservoirLoadConfiguration configuration = null)
        {
            var result = await LoadReservoirWithResultAsync(reservoirId, configuration);
            return result.Data;
        }

        public DataLoadResult<ReservoirData> LoadReservoirWithResult(string reservoirId, ReservoirLoadConfiguration configuration = null)
        {
            var result = new DataLoadResult<ReservoirData> { Data = new ReservoirData() };
            var stats = new DataLoadStatistics();
            stats.StartTime = DateTime.UtcNow;

            configuration = configuration ?? new ReservoirLoadConfiguration();

            try
            {
                if (!isConnected) Connect();
                if (!isConnected)
                {
                    result.Success = false;
                    result.Errors.Add("Failed to connect to RESQML file.");
                    return result;
                }

                // Find the reservoir representation by UUID or title
                var reservoirElement = FindReservoirElement(reservoirId);
                if (reservoirElement == null)
                {
                    result.Success = false;
                    result.Errors.Add($"Reservoir not found: {reservoirId}");
                    return result;
                }

                // Extract reservoir metadata
                var citation = reservoirElement.Element(resqml + "Citation");
                result.Data.ReservoirName = citation?.Element(eml + "Title")?.Value ?? reservoirId;
                result.Data.ReservoirId = reservoirElement.Attribute("uuid")?.Value ?? reservoirId;

                // Extract formation name from citation or metadata
                result.Data.FormationName = citation?.Element(eml + "Originator")?.Value ??
                                           reservoirElement.Element(resqml + "Metadata")?.Value;

                // Load layers if requested
                if (configuration.LoadGeometry || configuration.LoadProperties)
                {
                    var layers = LoadLayersFromResqml(reservoirElement, configuration);
                    result.Data.Layers = layers;
                    stats.RecordsLoaded = layers.Count;
                }

                // Load fluid contacts if requested
                if (configuration.LoadFluidContacts)
                {
                    result.Data.FluidContacts = LoadFluidContactsFromResqml(reservoirElement);
                }

                // Load properties if requested
                if (configuration.LoadProperties)
                {
                    result.Data.Properties = LoadPropertiesFromResqml(reservoirElement);
                }

                // Extract bounding box from geometry
                result.Data.BoundingBox = ExtractBoundingBox(reservoirElement);

                // Extract coordinate system
                var crs = reservoirElement.Element(resqml + "LocalCrs");
                if (crs != null)
                {
                    result.Data.CoordinateSystem = crs.Attribute("uuid")?.Value ?? "Local";
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Error loading RESQML reservoir: {ex.Message}");
                Console.WriteLine($"Exception in LoadReservoirWithResult: {ex}");
            }
            finally
            {
                stats.EndTime = DateTime.UtcNow;
                result.Statistics = stats;
            }

            return result;
        }

        public async Task<DataLoadResult<ReservoirData>> LoadReservoirWithResultAsync(string reservoirId, ReservoirLoadConfiguration configuration = null)
        {
            return await Task.Run(() => LoadReservoirWithResult(reservoirId, configuration));
        }

        public List<LayerData> LoadLayers(string reservoirId, ReservoirLoadConfiguration configuration = null)
        {
            if (!isConnected) Connect();
            if (!isConnected) return new List<LayerData>();

            var reservoirElement = FindReservoirElement(reservoirId);
            if (reservoirElement == null) return new List<LayerData>();

            return LoadLayersFromResqml(reservoirElement, configuration ?? new ReservoirLoadConfiguration());
        }

        public async Task<List<LayerData>> LoadLayersAsync(string reservoirId, ReservoirLoadConfiguration configuration = null)
        {
            return await Task.Run(() => LoadLayers(reservoirId, configuration));
        }

        public FluidContacts LoadFluidContacts(string reservoirId)
        {
            if (!isConnected) Connect();
            if (!isConnected) return new FluidContacts();

            var reservoirElement = FindReservoirElement(reservoirId);
            if (reservoirElement == null) return new FluidContacts();

            return LoadFluidContactsFromResqml(reservoirElement);
        }

        public async Task<FluidContacts> LoadFluidContactsAsync(string reservoirId)
        {
            return await Task.Run(() => LoadFluidContacts(reservoirId));
        }

        public List<string> GetAvailableReservoirs()
        {
            return GetAvailableIdentifiers();
        }

        public async Task<List<string>> GetAvailableReservoirsAsync()
        {
            return await GetAvailableIdentifiersAsync();
        }

        /// <summary>
        /// Implements IDataLoader.Load - loads the first available reservoir.
        /// </summary>
        public ReservoirData Load(object criteria = null)
        {
            var reservoirs = GetAvailableReservoirs();
            if (reservoirs.Count == 0)
                return new ReservoirData();

            return LoadReservoir(reservoirs[0]);
        }

        /// <summary>
        /// Implements IDataLoader.LoadAsync.
        /// </summary>
        public async Task<ReservoirData> LoadAsync(object criteria = null)
        {
            var reservoirs = await GetAvailableReservoirsAsync();
            if (reservoirs.Count == 0)
                return new ReservoirData();

            return await LoadReservoirAsync(reservoirs[0]);
        }

        #region Private Helper Methods

        private XElement FindReservoirElement(string reservoirId)
        {
            // Try to find by UUID first
            var byUuid = document.Descendants()
                .FirstOrDefault(e => e.Attribute("uuid")?.Value == reservoirId);

            if (byUuid != null) return byUuid;

            // Try to find by title
            var byTitle = document.Descendants()
                .Where(e => e.Name.Namespace == resqml)
                .FirstOrDefault(e => e.Element(resqml + "Citation")?.Element(eml + "Title")?.Value == reservoirId);

            return byTitle;
        }

        private List<LayerData> LoadLayersFromResqml(XElement reservoirElement, ReservoirLoadConfiguration configuration)
        {
            var layers = new List<LayerData>();

            try
            {
                // Find grid representations (layers in RESQML)
                var gridElements = reservoirElement.Descendants()
                    .Where(e => e.Name.Namespace == resqml &&
                                (e.Name.LocalName.Contains("Grid2dRepresentation") ||
                                 e.Name.LocalName.Contains("Grid3dRepresentation") ||
                                 e.Name.LocalName.Contains("IjkGridRepresentation")));

                foreach (var grid in gridElements)
                {
                    var layer = new LayerData
                    {
                        LayerId = grid.Attribute("uuid")?.Value ?? Guid.NewGuid().ToString(),
                        LayerName = grid.Element(resqml + "Citation")?.Element(eml + "Title")?.Value ?? "Unnamed Layer"
                    };

                    // Extract depth information from geometry
                    var geometry = grid.Element(resqml + "Geometry");
                    if (geometry != null)
                    {
                        ExtractLayerDepths(geometry, layer, configuration);
                    }

                    // Extract properties if requested
                    if (configuration.LoadProperties)
                    {
                        ExtractLayerProperties(grid, layer);
                    }

                    // Apply depth filter
                    if (configuration.MinDepth > 0 && layer.TopDepth < configuration.MinDepth) continue;
                    if (configuration.MaxDepth > 0 && layer.BottomDepth > configuration.MaxDepth) continue;

                    layers.Add(layer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading layers from RESQML: {ex.Message}");
            }

            return layers;
        }

        private void ExtractLayerDepths(XElement geometry, LayerData layer, ReservoirLoadConfiguration configuration)
        {
            // Extract Z values from geometry points
            var points = geometry.Descendants(gml + "pos")
                .Select(p => p.Value.Split(' '))
                .Where(parts => parts.Length >= 3)
                .Select(parts => double.TryParse(parts[2], out var z) ? z : (double?)null)
                .Where(z => z.HasValue)
                .Select(z => z.Value)
                .ToList();

            if (points.Any())
            {
                layer.TopDepth = points.Min();
                layer.BottomDepth = points.Max();
            }
        }

        private void ExtractLayerProperties(XElement gridElement, LayerData layer)
        {
            // Find property values associated with this grid
            var propertySet = gridElement.Element(resqml + "PropertySet");
            if (propertySet == null) return;

            // Look for porosity, permeability, saturation properties
            var properties = propertySet.Elements(resqml + "Property")
                .ToList();

            foreach (var prop in properties)
            {
                var propName = prop.Element(resqml + "Citation")?.Element(eml + "Title")?.Value?.ToLower() ?? "";
                var values = prop.Descendants(resqml + "Values")
                    .SelectMany(v => v.Elements().Select(e => e.Value))
                    .Where(v => double.TryParse(v, out _))
                    .Select(v => double.Parse(v))
                    .ToList();

                if (!values.Any()) continue;

                var avgValue = values.Average();

                switch (propName)
                {
                    case var p when p.Contains("porosity"):
                        layer.Porosity = avgValue;
                        break;
                    case var p when p.Contains("permeability"):
                        layer.Permeability = avgValue;
                        break;
                    case var p when p.Contains("water") && p.Contains("saturation"):
                        layer.WaterSaturation = avgValue;
                        break;
                    case var p when p.Contains("oil") && p.Contains("saturation"):
                        layer.OilSaturation = avgValue;
                        break;
                    case var p when p.Contains("gas") && p.Contains("saturation"):
                        layer.GasSaturation = avgValue;
                        break;
                }
            }

            // Determine if pay zone based on properties
            layer.IsPayZone = (layer.Porosity > 0.1 && layer.Permeability > 1.0) ||
                             (layer.OilSaturation > 0.2 || layer.GasSaturation > 0.2);
        }

        private FluidContacts LoadFluidContactsFromResqml(XElement reservoirElement)
        {
            var contacts = new FluidContacts();

            try
            {
                // Find fluid contact representations
                var contactElements = reservoirElement.Descendants()
                    .Where(e => e.Name.Namespace == resqml &&
                                e.Name.LocalName.Contains("HorizonInterpretation"));

                foreach (var contact in contactElements)
                {
                    var title = contact.Element(resqml + "Citation")?.Element(eml + "Title")?.Value?.ToLower() ?? "";
                    var depth = ExtractContactDepth(contact);

                    if (!depth.HasValue) continue;

                    if (title.Contains("fwl") || title.Contains("free water level"))
                    {
                        contacts.FreeWaterLevel = depth;
                    }
                    else if (title.Contains("owc") || title.Contains("oil water contact"))
                    {
                        contacts.OilWaterContact = depth;
                    }
                    else if (title.Contains("goc") || title.Contains("gas oil contact"))
                    {
                        contacts.GasOilContact = depth;
                    }
                    else if (title.Contains("gwc") || title.Contains("gas water contact"))
                    {
                        contacts.GasWaterContact = depth;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading fluid contacts from RESQML: {ex.Message}");
            }

            return contacts;
        }

        private double? ExtractContactDepth(XElement contactElement)
        {
            // Extract depth from geometry or metadata
            var geometry = contactElement.Element(resqml + "Geometry");
            if (geometry != null)
            {
                var zValues = geometry.Descendants(gml + "pos")
                    .Select(p => p.Value.Split(' '))
                    .Where(parts => parts.Length >= 3)
                    .Select(parts => double.TryParse(parts[2], out var z) ? z : (double?)null)
                    .Where(z => z.HasValue)
                    .Select(z => z.Value)
                    .ToList();

                if (zValues.Any())
                {
                    return zValues.Average();
                }
            }

            return null;
        }

        private ReservoirProperties LoadPropertiesFromResqml(XElement reservoirElement)
        {
            var properties = new ReservoirProperties();

            try
            {
                // Extract properties from property sets
                var propertySets = reservoirElement.Descendants(resqml + "PropertySet");
                
                foreach (var propSet in propertySets)
                {
                    var props = propSet.Elements(resqml + "Property").ToList();
                    
                    foreach (var prop in props)
                    {
                        var propName = prop.Element(resqml + "Citation")?.Element(eml + "Title")?.Value?.ToLower() ?? "";
                        var values = prop.Descendants(resqml + "Values")
                            .SelectMany(v => v.Elements().Select(e => e.Value))
                            .Where(v => double.TryParse(v, out _))
                            .Select(v => double.Parse(v))
                            .ToList();

                        if (!values.Any()) continue;

                        var avgValue = values.Average();

                        if (propName.Contains("porosity"))
                        {
                            properties.AveragePorosity = avgValue;
                        }
                        else if (propName.Contains("permeability"))
                        {
                            properties.AveragePermeability = avgValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading properties from RESQML: {ex.Message}");
            }

            return properties;
        }

        private BoundingBox ExtractBoundingBox(XElement reservoirElement)
        {
            var bbox = new BoundingBox();

            try
            {
                var points = reservoirElement.Descendants(gml + "pos")
                    .Select(p => p.Value.Split(' '))
                    .Where(parts => parts.Length >= 3)
                    .Select(parts => new
                    {
                        X = double.TryParse(parts[0], out var x) ? x : 0,
                        Y = double.TryParse(parts[1], out var y) ? y : 0,
                        Z = double.TryParse(parts[2], out var z) ? z : 0
                    })
                    .ToList();

                if (points.Any())
                {
                    bbox.MinX = points.Min(p => p.X);
                    bbox.MaxX = points.Max(p => p.X);
                    bbox.MinY = points.Min(p => p.Y);
                    bbox.MaxY = points.Max(p => p.Y);
                    bbox.MinZ = points.Min(p => p.Z);
                    bbox.MaxZ = points.Max(p => p.Z);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting bounding box: {ex.Message}");
            }

            return bbox;
        }

        #endregion
    }
}

