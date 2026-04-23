using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Beep.OilandGas.Drawing.CoordinateSystems;
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
                }

                if (configuration.LoadGrids)
                {
                    result.Data.Grids = LoadGridsFromResqml(reservoirElement, configuration);
                }

                if (configuration.LoadSurfaces)
                {
                    result.Data.Surfaces = LoadSurfacesFromResqml(reservoirElement, configuration);
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
                stats.RecordsLoaded = (result.Data.Layers?.Count ?? 0)
                    + (result.Data.Grids?.Count ?? 0)
                    + (result.Data.Surfaces?.Count ?? 0);

                // Extract coordinate system
                var crs = reservoirElement.Element(resqml + "LocalCrs");
                if (crs != null)
                {
                    var crsIdentifier = crs.Attribute("uuid")?.Value ?? "Local";
                    result.Data.CoordinateReferenceSystem = new CoordinateReferenceSystem(
                        crsIdentifier,
                        $"RESQML Local CRS {crsIdentifier}",
                        CoordinateAuthority.Energistics,
                        CoordinateReferenceSystemKind.Custom,
                        new[]
                        {
                            new CoordinateAxisDefinition(CoordinateAxisKind.Easting, "Local X", MeasurementUnit.Unknown),
                            new CoordinateAxisDefinition(CoordinateAxisKind.Northing, "Local Y", MeasurementUnit.Unknown),
                            new CoordinateAxisDefinition(CoordinateAxisKind.Depth, "Depth", MeasurementUnit.Unknown, isInverted: true)
                        });
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
                var gridElements = GetGridRepresentationElements(reservoirElement);

                foreach (var grid in gridElements)
                {
                    var layer = new LayerData
                    {
                        LayerId = grid.Attribute("uuid")?.Value ?? Guid.NewGuid().ToString(),
                        LayerName = grid.Element(resqml + "Citation")?.Element(eml + "Title")?.Value ?? "Unnamed Layer"
                    };

                    ExtractLayerDepths(grid, layer, configuration);

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

        private List<ReservoirGridData> LoadGridsFromResqml(XElement reservoirElement, ReservoirLoadConfiguration configuration)
        {
            var grids = new List<ReservoirGridData>();

            try
            {
                foreach (var gridElement in GetGridRepresentationElements(reservoirElement))
                {
                    var points = ExtractGeometryPoints(gridElement);
                    var grid = new ReservoirGridData
                    {
                        GridId = gridElement.Attribute("uuid")?.Value ?? Guid.NewGuid().ToString(),
                        GridName = gridElement.Element(resqml + "Citation")?.Element(eml + "Title")?.Value ?? "Unnamed Grid",
                        GridKind = ResolveGridKind(gridElement.Name.LocalName),
                        BoundingBox = CreateBoundingBox(points)
                    };

                    ExtractGridDimensions(gridElement, grid);

                    if (configuration.LoadGeometry && points.Count > 0)
                    {
                        grid.Nodes = CreateGridNodes(points, grid.ColumnCount, grid.RowCount, grid.LayerCount);
                    }

                    grid.Metadata["RepresentationType"] = gridElement.Name.LocalName;

                    if (!PassesDepthFilter(grid.BoundingBox, configuration))
                        continue;

                    grids.Add(grid);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading grid models from RESQML: {ex.Message}");
            }

            return grids;
        }

        private List<ReservoirSurfaceData> LoadSurfacesFromResqml(XElement reservoirElement, ReservoirLoadConfiguration configuration)
        {
            var surfaces = new List<ReservoirSurfaceData>();

            try
            {
                foreach (var surfaceElement in GetSurfaceRepresentationElements(reservoirElement))
                {
                    var points = ExtractGeometryPoints(surfaceElement);
                    if (points.Count == 0)
                        continue;

                    string title = surfaceElement.Element(resqml + "Citation")?.Element(eml + "Title")?.Value;
                    var surface = new ReservoirSurfaceData
                    {
                        SurfaceId = surfaceElement.Attribute("uuid")?.Value ?? Guid.NewGuid().ToString(),
                        SurfaceName = title ?? surfaceElement.Name.LocalName,
                        SurfaceKind = ResolveSurfaceKind(surfaceElement.Name.LocalName, title),
                        SourceRepresentationType = surfaceElement.Name.LocalName,
                        SourceGridId = surfaceElement.Name.LocalName.Contains("Grid2dRepresentation") ? surfaceElement.Attribute("uuid")?.Value : null,
                        Points = configuration.LoadGeometry ? points : new List<Point3D>(),
                        BoundingBox = CreateBoundingBox(points)
                    };

                    surface.Metadata["RepresentationType"] = surfaceElement.Name.LocalName;

                    if (!PassesDepthFilter(surface.BoundingBox, configuration))
                        continue;

                    surfaces.Add(surface);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading surfaces from RESQML: {ex.Message}");
            }

            return surfaces;
        }

        private void ExtractLayerDepths(XElement geometryOwner, LayerData layer, ReservoirLoadConfiguration configuration)
        {
            var points = ExtractGeometryPoints(geometryOwner);

            if (points.Any())
            {
                if (configuration.LoadGeometry)
                {
                    layer.Geometry = points;
                }

                layer.TopDepth = points.Min(point => point.Z);
                layer.BottomDepth = points.Max(point => point.Z);
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
            try
            {
                return CreateBoundingBox(ExtractGeometryPoints(reservoirElement)) ?? new BoundingBox();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting bounding box: {ex.Message}");
            }

            return new BoundingBox();
        }

        private List<XElement> GetGridRepresentationElements(XElement reservoirElement)
        {
            return reservoirElement.Descendants()
                .Where(e => e.Name.Namespace == resqml &&
                            (e.Name.LocalName.Contains("Grid2dRepresentation") ||
                             e.Name.LocalName.Contains("Grid3dRepresentation") ||
                             e.Name.LocalName.Contains("IjkGridRepresentation")))
                .ToList();
        }

        private List<XElement> GetSurfaceRepresentationElements(XElement reservoirElement)
        {
            return reservoirElement.Descendants()
                .Where(e => e.Name.Namespace == resqml &&
                            (e.Name.LocalName.Contains("Grid2dRepresentation") ||
                             e.Name.LocalName.Contains("TriangulatedSetRepresentation") ||
                             e.Name.LocalName.Contains("PointSetRepresentation") ||
                             e.Name.LocalName.Contains("Surface") ||
                             e.Name.LocalName.Contains("HorizonInterpretation") ||
                             e.Name.LocalName.Contains("FaultInterpretation")))
                .GroupBy(e => e.Attribute("uuid")?.Value
                    ?? e.Element(resqml + "Citation")?.Element(eml + "Title")?.Value
                    ?? e.GetHashCode().ToString())
                .Select(group => group.First())
                .ToList();
        }

        private List<Point3D> ExtractGeometryPoints(XElement element)
        {
            var points = new List<Point3D>();
            if (element == null)
                return points;

            foreach (var pos in element.Descendants(gml + "pos"))
            {
                points.AddRange(ParsePointTriples(pos.Value));
            }

            foreach (var posList in element.Descendants(gml + "posList"))
            {
                points.AddRange(ParsePointTriples(posList.Value));
            }

            return points;
        }

        private List<Point3D> ParsePointTriples(string coordinateText)
        {
            var points = new List<Point3D>();
            if (string.IsNullOrWhiteSpace(coordinateText))
                return points;

            var parts = coordinateText
                .Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            for (int index = 0; index + 2 < parts.Length; index += 3)
            {
                if (double.TryParse(parts[index], out var x) &&
                    double.TryParse(parts[index + 1], out var y) &&
                    double.TryParse(parts[index + 2], out var z))
                {
                    points.Add(new Point3D { X = x, Y = y, Z = z });
                }
            }

            return points;
        }

        private BoundingBox CreateBoundingBox(List<Point3D> points)
        {
            if (points == null || points.Count == 0)
                return null;

            return new BoundingBox
            {
                MinX = points.Min(point => point.X),
                MaxX = points.Max(point => point.X),
                MinY = points.Min(point => point.Y),
                MaxY = points.Max(point => point.Y),
                MinZ = points.Min(point => point.Z),
                MaxZ = points.Max(point => point.Z)
            };
        }

        private void ExtractGridDimensions(XElement gridElement, ReservoirGridData grid)
        {
            grid.ColumnCount = TryParseIntegerDescendant(gridElement, "Ni", "ColumnCount", "Columns", "ICount");
            grid.RowCount = TryParseIntegerDescendant(gridElement, "Nj", "RowCount", "Rows", "JCount");
            grid.LayerCount = TryParseIntegerDescendant(gridElement, "Nk", "LayerCount", "Layers", "KCount");
        }

        private int? TryParseIntegerDescendant(XElement element, params string[] localNames)
        {
            foreach (var name in localNames)
            {
                var value = element.Descendants()
                    .FirstOrDefault(descendant => descendant.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ?.Value;

                if (int.TryParse(value, out var parsed) && parsed > 0)
                    return parsed;
            }

            return null;
        }

        private List<ReservoirGridNode> CreateGridNodes(List<Point3D> points, int? columnCount, int? rowCount, int? layerCount)
        {
            var nodes = new List<ReservoirGridNode>(points.Count);
            int? effectiveColumns = columnCount > 0 ? columnCount : null;
            int? effectiveRows = rowCount > 0 ? rowCount : null;
            int nodesPerLayer = effectiveColumns.HasValue && effectiveRows.HasValue
                ? effectiveColumns.Value * effectiveRows.Value
                : 0;

            for (int index = 0; index < points.Count; index++)
            {
                int? i = null;
                int? j = null;
                int? k = null;

                if (effectiveColumns.HasValue)
                {
                    i = index % effectiveColumns.Value;

                    if (effectiveRows.HasValue)
                    {
                        j = (index / effectiveColumns.Value) % effectiveRows.Value;

                        if (nodesPerLayer > 0)
                        {
                            k = index / nodesPerLayer;
                        }
                    }
                }

                nodes.Add(new ReservoirGridNode
                {
                    Index = index,
                    I = i,
                    J = j,
                    K = k,
                    Position = points[index]
                });
            }

            return nodes;
        }

        private ReservoirGridKind ResolveGridKind(string localName)
        {
            if (localName.Contains("IjkGridRepresentation", StringComparison.OrdinalIgnoreCase))
                return ReservoirGridKind.CornerPoint3D;
            if (localName.Contains("Grid3dRepresentation", StringComparison.OrdinalIgnoreCase))
                return ReservoirGridKind.Structured3D;
            if (localName.Contains("Grid2dRepresentation", StringComparison.OrdinalIgnoreCase))
                return ReservoirGridKind.Structured2D;

            return ReservoirGridKind.Unknown;
        }

        private ReservoirSurfaceKind ResolveSurfaceKind(string localName, string title)
        {
            string candidate = (title ?? string.Empty) + " " + localName;

            if (candidate.Contains("fault", StringComparison.OrdinalIgnoreCase))
                return ReservoirSurfaceKind.Fault;
            if (candidate.Contains("isochore", StringComparison.OrdinalIgnoreCase) || candidate.Contains("thickness", StringComparison.OrdinalIgnoreCase))
                return ReservoirSurfaceKind.Isochore;
            if (candidate.Contains("porosity", StringComparison.OrdinalIgnoreCase) ||
                candidate.Contains("permeability", StringComparison.OrdinalIgnoreCase) ||
                candidate.Contains("saturation", StringComparison.OrdinalIgnoreCase))
                return ReservoirSurfaceKind.Property;
            if (candidate.Contains("horizon", StringComparison.OrdinalIgnoreCase) ||
                candidate.Contains("top", StringComparison.OrdinalIgnoreCase) ||
                candidate.Contains("base", StringComparison.OrdinalIgnoreCase))
                return ReservoirSurfaceKind.Horizon;
            if (localName.Contains("Grid2dRepresentation", StringComparison.OrdinalIgnoreCase))
                return ReservoirSurfaceKind.GridDerived;
            if (candidate.Contains("structure", StringComparison.OrdinalIgnoreCase) || candidate.Contains("surface", StringComparison.OrdinalIgnoreCase))
                return ReservoirSurfaceKind.Structure;

            return ReservoirSurfaceKind.Unknown;
        }

        private bool PassesDepthFilter(BoundingBox boundingBox, ReservoirLoadConfiguration configuration)
        {
            if (boundingBox == null)
                return true;

            if (configuration.MinDepth > 0 && boundingBox.MaxZ < configuration.MinDepth)
                return false;
            if (configuration.MaxDepth > 0 && boundingBox.MinZ > configuration.MaxDepth)
                return false;

            return true;
        }

        #endregion
    }
}

