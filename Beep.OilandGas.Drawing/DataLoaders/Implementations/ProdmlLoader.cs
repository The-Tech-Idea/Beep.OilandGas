using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.DataLoaders.Implementations
{
    /// <summary>
    /// Loader for PRODML (Production Markup Language) production data.
    /// PRODML is an Energistics standard for production data exchange.
    /// </summary>
    public class ProdmlLoader : IDataLoader<ProductionData>
    {
        private readonly string filePath;
        private XDocument document;
        private bool isConnected = false;

        // PRODML namespaces
        private readonly XNamespace prodml = "http://www.energistics.org/schemas/prodmlv2";
        private readonly XNamespace eml = "http://www.energistics.org/schemas/eml";

        public string DataSource => filePath;
        public bool IsConnected => isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProdmlLoader"/> class.
        /// </summary>
        /// <param name="filePath">The path to the PRODML XML file.</param>
        public ProdmlLoader(string filePath)
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
                    throw new FileNotFoundException($"PRODML file not found: {filePath}");
                }

                document = XDocument.Load(filePath);
                isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to PRODML file: {ex.Message}");
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
                    throw new FileNotFoundException($"PRODML file not found: {filePath}");
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
                Console.WriteLine($"Error connecting to PRODML file: {ex.Message}");
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
                var root = document.Root;
                return root != null && (
                    root.Name.Namespace == prodml ||
                    root.Elements().Any(e => e.Name.Namespace == prodml)
                );
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
                // Find ProductionOperation, WellTest, Flow objects
                var operations = document.Descendants(prodml + "ProductionOperation");
                var wellTests = document.Descendants(prodml + "WellTest");
                var flows = document.Descendants(prodml + "Flow");

                foreach (var op in operations)
                {
                    var uid = op.Element(prodml + "uid")?.Value;
                    var name = op.Element(prodml + "name")?.Value;
                    if (!string.IsNullOrEmpty(uid)) identifiers.Add(uid);
                    else if (!string.IsNullOrEmpty(name)) identifiers.Add(name);
                }

                foreach (var test in wellTests)
                {
                    var uid = test.Element(prodml + "uid")?.Value;
                    if (!string.IsNullOrEmpty(uid)) identifiers.Add(uid);
                }

                foreach (var flow in flows)
                {
                    var uid = flow.Element(prodml + "uid")?.Value;
                    if (!string.IsNullOrEmpty(uid)) identifiers.Add(uid);
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

        public ProductionData Load(object criteria = null)
        {
            var identifiers = GetAvailableIdentifiers();
            if (identifiers.Count == 0)
                return new ProductionData();

            return LoadProductionData(identifiers[0]);
        }

        public Task<ProductionData> LoadAsync(object criteria = null)
        {
            return Task.Run(() => Load(criteria));
        }

        /// <summary>
        /// Loads production data for a specific identifier.
        /// </summary>
        public ProductionData LoadProductionData(string identifier)
        {
            if (!isConnected) Connect();
            if (!isConnected) return new ProductionData();

            var productionData = new ProductionData();

            try
            {
                // Find ProductionOperation
                var operation = document.Descendants(prodml + "ProductionOperation")
                    .FirstOrDefault(op => 
                        op.Element(prodml + "uid")?.Value == identifier ||
                        op.Element(prodml + "name")?.Value == identifier);

                if (operation != null)
                {
                    ExtractProductionOperation(operation, productionData);
                }

                // Find WellTest
                var wellTest = document.Descendants(prodml + "WellTest")
                    .FirstOrDefault(test => test.Element(prodml + "uid")?.Value == identifier);

                if (wellTest != null)
                {
                    ExtractWellTest(wellTest, productionData);
                }

                // Find Flow data
                var flows = document.Descendants(prodml + "Flow")
                    .Where(flow => 
                        flow.Element(prodml + "uid")?.Value == identifier ||
                        flow.Ancestors(prodml + "ProductionOperation")
                            .Any(op => op.Element(prodml + "uid")?.Value == identifier));

                foreach (var flow in flows)
                {
                    ExtractFlowData(flow, productionData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading PRODML production data: {ex.Message}");
            }

            return productionData;
        }

        #region Private Helper Methods

        private void ExtractProductionOperation(XElement operation, ProductionData productionData)
        {
            productionData.OperationId = operation.Element(prodml + "uid")?.Value;
            productionData.OperationName = operation.Element(prodml + "name")?.Value;
            productionData.WellIdentifier = operation.Element(prodml + "wellUid")?.Value;
            productionData.WellboreIdentifier = operation.Element(prodml + "wellboreUid")?.Value;

            // Extract time range
            var startTime = operation.Element(prodml + "startTime")?.Value;
            var endTime = operation.Element(prodml + "endTime")?.Value;

            if (DateTime.TryParse(startTime, out var start))
                productionData.StartTime = start;
            if (DateTime.TryParse(endTime, out var end))
                productionData.EndTime = end;
        }

        private void ExtractWellTest(XElement wellTest, ProductionData productionData)
        {
            var testData = new WELL_TEST_DATA
            {
                TestId = wellTest.Element(prodml + "uid")?.Value,
                TestName = wellTest.Element(prodml + "name")?.Value
            };

            // Extract test time
            var testTime = wellTest.Element(prodml + "dTim")?.Value;
            if (DateTime.TryParse(testTime, out var time))
                testData.TestTime = time;

            // Extract flow rates
            var oilRate = wellTest.Element(prodml + "oilRate")?.Value;
            var gasRate = wellTest.Element(prodml + "gasRate")?.Value;
            var waterRate = wellTest.Element(prodml + "waterRate")?.Value;

            if (double.TryParse(oilRate, out var oil))
                testData.OilRate = oil;
            if (double.TryParse(gasRate, out var gas))
                testData.GasRate = gas;
            if (double.TryParse(waterRate, out var water))
                testData.WaterRate = water;

            // Extract pressures
            var flowingPressure = wellTest.Element(prodml + "flowingPressure")?.Value;
            var shutInPressure = wellTest.Element(prodml + "shutInPressure")?.Value;

            if (double.TryParse(flowingPressure, out var fp))
                testData.FlowingPressure = fp;
            if (double.TryParse(shutInPressure, out var sip))
                testData.ShutInPressure = sip;

            productionData.WellTests.Add(testData);
        }

        private void ExtractFlowData(XElement flow, ProductionData productionData)
        {
            var flowData = new FlowData
            {
                FlowId = flow.Element(prodml + "uid")?.Value,
                FlowName = flow.Element(prodml + "name")?.Value
            };

            // Extract flow rates
            var oilRate = flow.Element(prodml + "oilRate")?.Value;
            var gasRate = flow.Element(prodml + "gasRate")?.Value;
            var waterRate = flow.Element(prodml + "waterRate")?.Value;

            if (double.TryParse(oilRate, out var oil))
                flowData.OilRate = oil;
            if (double.TryParse(gasRate, out var gas))
                flowData.GasRate = gas;
            if (double.TryParse(waterRate, out var water))
                flowData.WaterRate = water;

            // Extract time
            var time = flow.Element(prodml + "dTim")?.Value;
            if (DateTime.TryParse(time, out var dt))
                flowData.Time = dt;

            productionData.FlowData.Add(flowData);
        }

        #endregion
    }

    #region PRODML Data Models

    /// <summary>
    /// Represents production data from PRODML.
    /// </summary>
    public class ProductionData
    {
        public string OperationId { get; set; }
        public string OperationName { get; set; }
        public string WellIdentifier { get; set; }
        public string WellboreIdentifier { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<WELL_TEST_DATA> WellTests { get; set; } = new List<WELL_TEST_DATA>();
        public List<FlowData> FlowData { get; set; } = new List<FlowData>();
    }

    /// <summary>
    /// Represents well test data from PRODML.
    /// </summary>
    public class WELL_TEST_DATA
    {
        public string TestId { get; set; }
        public string TestName { get; set; }
        public DateTime? TestTime { get; set; }
        public double? OilRate { get; set; }
        public double? GasRate { get; set; }
        public double? WaterRate { get; set; }
        public double? FlowingPressure { get; set; }
        public double? ShutInPressure { get; set; }
    }

    /// <summary>
    /// Represents flow data from PRODML.
    /// </summary>
    public class FlowData
    {
        public string FlowId { get; set; }
        public string FlowName { get; set; }
        public DateTime? Time { get; set; }
        public double? OilRate { get; set; }
        public double? GasRate { get; set; }
        public double? WaterRate { get; set; }
    }

    #endregion
}

