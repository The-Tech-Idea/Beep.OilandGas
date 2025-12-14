using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.DataLoaders.WITSML
{
    /// <summary>
    /// Loads data acquisition orders from WITSML DataWorkOrder v1.0 files.
    /// DataWorkOrder is used to specify what data should be acquired during well operations.
    /// </summary>
    public class DataWorkOrderLoader
    {
        private readonly string filePath;
        private XDocument document;
        private bool isConnected = false;

        // WITSML namespaces
        private readonly XNamespace witsml = "http://www.witsml.org/schemas/1series";
        private readonly XNamespace dwo = "http://www.witsml.org/schemas/1series/dataworkorder_v1.0";
        private readonly XNamespace eml = "http://www.energistics.org/schemas/eml";

        public string DataSource => filePath;
        public bool IsConnected => isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataWorkOrderLoader"/> class.
        /// </summary>
        /// <param name="filePath">The path to the WITSML DataWorkOrder XML file.</param>
        public DataWorkOrderLoader(string filePath)
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
                    throw new FileNotFoundException($"DataWorkOrder file not found: {filePath}");
                }

                document = XDocument.Load(filePath);
                isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to DataWorkOrder file: {ex.Message}");
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
                    throw new FileNotFoundException($"DataWorkOrder file not found: {filePath}");
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
                Console.WriteLine($"Error connecting to DataWorkOrder file: {ex.Message}");
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

        /// <summary>
        /// Loads all data work orders from the file.
        /// </summary>
        /// <returns>A list of data work order information.</returns>
        public List<DataWorkOrderInfo> LoadDataWorkOrders()
        {
            if (!isConnected) Connect();
            if (!isConnected) return new List<DataWorkOrderInfo>();

            var orders = new List<DataWorkOrderInfo>();

            try
            {
                // Find all DataWorkOrder elements
                var orderElements = document.Descendants(dwo + "DataWorkOrder")
                    .Concat(document.Descendants().Where(e => e.Name.LocalName == "DataWorkOrder"));

                foreach (var orderElement in orderElements)
                {
                    var order = new DataWorkOrderInfo
                    {
                        WellboreReference = ExtractWellboreReference(orderElement),
                        Field = orderElement.Element(dwo + "Field")?.Value,
                        DataProvider = orderElement.Element(dwo + "DataProvider")?.Value,
                        DataConsumer = orderElement.Element(dwo + "DataConsumer")?.Value,
                        Description = orderElement.Element(dwo + "Description")?.Value,
                        PlannedStartTime = ParseDateTime(orderElement.Element(dwo + "DTimPlannedStart")?.Value),
                        PlannedStopTime = ParseDateTime(orderElement.Element(dwo + "DTimPlannedStop")?.Value)
                    };

                    // Load data source configurations
                    var configSets = orderElement.Elements(dwo + "DataSourceConfigurationSet");
                    foreach (var configSet in configSets)
                    {
                        var configs = LoadDataSourceConfigurations(configSet);
                        order.DataSourceConfigurations.AddRange(configs);
                    }

                    orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading DataWorkOrders: {ex.Message}");
            }

            return orders;
        }

        /// <summary>
        /// Loads data source configurations from a DataSourceConfigurationSet element.
        /// </summary>
        private List<DataSourceConfigurationInfo> LoadDataSourceConfigurations(XElement configSet)
        {
            var configs = new List<DataSourceConfigurationInfo>();

            try
            {
                var configElements = configSet.Elements(dwo + "DataSourceConfiguration");
                
                foreach (var configElement in configElements)
                {
                    var config = new DataSourceConfigurationInfo
                    {
                        VersionNumber = ParseLong(configElement.Element(dwo + "VersionNumber")?.Value),
                        Name = configElement.Element(dwo + "Name")?.Value,
                        Description = configElement.Element(dwo + "Description")?.Value,
                        Status = configElement.Element(dwo + "Status")?.Value,
                        PlannedStartTime = ParseDateTime(configElement.Element(dwo + "DTimPlannedStart")?.Value),
                        PlannedStopTime = ParseDateTime(configElement.Element(dwo + "DTimPlannedStop")?.Value),
                        PlannedStartDepth = ParseDouble(configElement.Element(dwo + "MdPlannedStart")?.Value),
                        PlannedStopDepth = ParseDouble(configElement.Element(dwo + "MdPlannedStop")?.Value)
                    };

                    // Load channel configurations
                    var channelElements = configElement.Elements(dwo + "Channel");
                    foreach (var channelElement in channelElements)
                    {
                        var channel = LoadChannelConfiguration(channelElement);
                        config.Channels.Add(channel);
                    }

                    configs.Add(config);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading DataSourceConfigurations: {ex.Message}");
            }

            return configs;
        }

        /// <summary>
        /// Loads channel configuration from a Channel element.
        /// </summary>
        private ChannelConfigurationInfo LoadChannelConfiguration(XElement channelElement)
        {
            var channel = new ChannelConfigurationInfo
            {
                Mnemonic = channelElement.Element(dwo + "Mnemonic")?.Value,
                UnitOfMeasure = channelElement.Element(dwo + "Uom")?.Value,
                GlobalMnemonic = channelElement.Element(dwo + "GlobalMnemonic")?.Value,
                IndexKind = channelElement.Element(dwo + "IndexKind")?.Value,
                LoggingMethod = channelElement.Element(dwo + "LoggingMethod")?.Value,
                ToolClass = channelElement.Element(dwo + "ToolClass")?.Value,
                ToolName = channelElement.Element(dwo + "ToolName")?.Value,
                Service = channelElement.Element(dwo + "Service")?.Value,
                Criticality = channelElement.Element(dwo + "Criticality")?.Value,
                Description = channelElement.Element(dwo + "Description")?.Value
            };

            // Load channel requirements
            var requirementElements = channelElement.Elements(dwo + "Requirement");
            foreach (var reqElement in requirementElements)
            {
                var requirement = new ChannelRequirementInfo
                {
                    Purpose = reqElement.Element(dwo + "Purpose")?.Value,
                    MinInterval = ParseDouble(reqElement.Element(dwo + "MinInterval")?.Value),
                    MaxInterval = ParseDouble(reqElement.Element(dwo + "MaxInterval")?.Value),
                    MinValue = ParseDouble(reqElement.Element(dwo + "MinValue")?.Value),
                    MaxValue = ParseDouble(reqElement.Element(dwo + "MaxValue")?.Value),
                    MinStep = ParseDouble(reqElement.Element(dwo + "MinStep")?.Value),
                    MaxStep = ParseDouble(reqElement.Element(dwo + "MaxStep")?.Value),
                    Latency = ParseDouble(reqElement.Element(dwo + "Latency")?.Value),
                    MdThreshold = ParseDouble(reqElement.Element(dwo + "MdThreshold")?.Value)
                };

                channel.Requirements.Add(requirement);
            }

            return channel;
        }

        private string ExtractWellboreReference(XElement orderElement)
        {
            var wellboreRef = orderElement.Element(dwo + "Wellbore");
            if (wellboreRef != null)
            {
                // Extract UUID or title from DataObjectReference
                return wellboreRef.Attribute("uuid")?.Value ??
                       wellboreRef.Element(eml + "Title")?.Value ??
                       wellboreRef.Value;
            }
            return null;
        }

        private DateTime? ParseDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParse(value, out var dateTime))
                return dateTime;

            return null;
        }

        private double? ParseDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (double.TryParse(value, out var result))
                return result;

            return null;
        }

        private long? ParseLong(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (long.TryParse(value, out var result))
                return result;

            return null;
        }
    }

    /// <summary>
    /// Represents a DataWorkOrder from WITSML.
    /// </summary>
    public class DataWorkOrderInfo
    {
        public string WellboreReference { get; set; }
        public string Field { get; set; }
        public string DataProvider { get; set; }
        public string DataConsumer { get; set; }
        public string Description { get; set; }
        public DateTime? PlannedStartTime { get; set; }
        public DateTime? PlannedStopTime { get; set; }
        public List<DataSourceConfigurationInfo> DataSourceConfigurations { get; set; } = new List<DataSourceConfigurationInfo>();
    }

    /// <summary>
    /// Represents a DataSourceConfiguration from WITSML DataWorkOrder.
    /// </summary>
    public class DataSourceConfigurationInfo
    {
        public long? VersionNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? PlannedStartTime { get; set; }
        public DateTime? PlannedStopTime { get; set; }
        public double? PlannedStartDepth { get; set; }
        public double? PlannedStopDepth { get; set; }
        public List<ChannelConfigurationInfo> Channels { get; set; } = new List<ChannelConfigurationInfo>();
    }

    /// <summary>
    /// Represents a ChannelConfiguration from WITSML DataWorkOrder.
    /// </summary>
    public class ChannelConfigurationInfo
    {
        public string Mnemonic { get; set; }
        public string UnitOfMeasure { get; set; }
        public string GlobalMnemonic { get; set; }
        public string IndexKind { get; set; }
        public string LoggingMethod { get; set; }
        public string ToolClass { get; set; }
        public string ToolName { get; set; }
        public string Service { get; set; }
        public string Criticality { get; set; }
        public string Description { get; set; }
        public List<ChannelRequirementInfo> Requirements { get; set; } = new List<ChannelRequirementInfo>();
    }

    /// <summary>
    /// Represents a ChannelRequirement from WITSML DataWorkOrder.
    /// </summary>
    public class ChannelRequirementInfo
    {
        public string Purpose { get; set; }
        public double? MinInterval { get; set; }
        public double? MaxInterval { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public double? MinStep { get; set; }
        public double? MaxStep { get; set; }
        public double? Latency { get; set; }
        public double? MdThreshold { get; set; }
    }
}

