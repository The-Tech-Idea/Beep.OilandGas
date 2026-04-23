using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Rendering;
using Beep.OilandGas.Drawing.Visualizations.Logs;

namespace Beep.OilandGas.Drawing.Builders
{
    /// <summary>
    /// Builder for creating multi-well correlation layouts from loaded or direct log sources.
    /// </summary>
    public sealed class WellCorrelationBuilder
    {
        private readonly List<PanelSource> panelSources = new List<PanelSource>();
        private LogRendererConfiguration defaultLogConfiguration = new LogRendererConfiguration();
        private WellCorrelationConfiguration correlationConfiguration = new WellCorrelationConfiguration();
        private float zoom = 1.0f;
        private int width = 2200;
        private int height = 1800;

        /// <summary>
        /// Adds a fully resolved correlation panel.
        /// </summary>
        public WellCorrelationBuilder AddPanel(WellCorrelationPanel panel)
        {
            if (panel == null)
                throw new ArgumentNullException(nameof(panel));
            if (panel.LogData == null)
                throw new ArgumentException("Correlation panel must include log data.", nameof(panel));

            panelSources.Add(PanelSource.FromResolvedPanel(panel));
            return this;
        }

        /// <summary>
        /// Adds a panel from direct log data.
        /// </summary>
        public WellCorrelationBuilder AddLogData(
            LogData logData,
            string panelName = null,
            DeviationSurvey deviationSurvey = null,
            IEnumerable<WellCorrelationMarker> markers = null,
            LogRendererConfiguration configuration = null)
        {
            if (logData == null)
                throw new ArgumentNullException(nameof(logData));

            return AddPanel(new WellCorrelationPanel
            {
                PanelName = panelName,
                LogData = logData,
                DeviationSurvey = deviationSurvey,
                LogConfiguration = configuration,
                Markers = markers?.ToList() ?? new List<WellCorrelationMarker>()
            });
        }

        /// <summary>
        /// Adds a panel from a file path using the appropriate log loader.
        /// </summary>
        public WellCorrelationBuilder AddFile(
            string filePath,
            string panelName = null,
            string wellIdentifier = null,
            string logName = null,
            LogLoadConfiguration loadConfiguration = null,
            DeviationSurvey deviationSurvey = null,
            IEnumerable<WellCorrelationMarker> markers = null,
            LogRendererConfiguration configuration = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            return AddLoader(
                DataLoaderFactory.CreateLogLoaderFromFile(filePath),
                panelName,
                wellIdentifier,
                logName,
                loadConfiguration,
                deviationSurvey,
                markers,
                configuration);
        }

        /// <summary>
        /// Adds a panel from a provided loader instance.
        /// </summary>
        public WellCorrelationBuilder AddLoader(
            ILogLoader loader,
            string panelName = null,
            string wellIdentifier = null,
            string logName = null,
            LogLoadConfiguration loadConfiguration = null,
            DeviationSurvey deviationSurvey = null,
            IEnumerable<WellCorrelationMarker> markers = null,
            LogRendererConfiguration configuration = null)
        {
            if (loader == null)
                throw new ArgumentNullException(nameof(loader));

            panelSources.Add(new PanelSource
            {
                PanelName = panelName,
                Loader = loader,
                WellIdentifier = wellIdentifier,
                LogName = logName,
                LoadConfiguration = loadConfiguration,
                DeviationSurvey = deviationSurvey,
                Markers = markers?.ToList() ?? new List<WellCorrelationMarker>(),
                LogConfiguration = configuration
            });

            return this;
        }

        /// <summary>
        /// Adds a panel from a data source resolved through the loader factory.
        /// </summary>
        public WellCorrelationBuilder AddDataSource(
            string dataSource,
            DataLoaderType loaderType,
            string panelName = null,
            string wellIdentifier = null,
            string logName = null,
            LogLoadConfiguration loadConfiguration = null,
            Func<DbConnection> connectionFactory = null,
            DeviationSurvey deviationSurvey = null,
            IEnumerable<WellCorrelationMarker> markers = null,
            LogRendererConfiguration configuration = null)
        {
            if (string.IsNullOrWhiteSpace(dataSource))
                throw new ArgumentNullException(nameof(dataSource));

            panelSources.Add(new PanelSource
            {
                PanelName = panelName,
                DataSource = dataSource,
                LoaderType = loaderType,
                WellIdentifier = wellIdentifier,
                LogName = logName,
                LoadConfiguration = loadConfiguration,
                ConnectionFactory = connectionFactory,
                DeviationSurvey = deviationSurvey,
                Markers = markers?.ToList() ?? new List<WellCorrelationMarker>(),
                LogConfiguration = configuration
            });

            return this;
        }

        /// <summary>
        /// Sets the default log configuration applied to panels that do not provide one.
        /// </summary>
        public WellCorrelationBuilder WithDefaultLogConfiguration(LogRendererConfiguration configuration)
        {
            defaultLogConfiguration = configuration ?? new LogRendererConfiguration();
            return this;
        }

        /// <summary>
        /// Sets the correlation layout configuration.
        /// </summary>
        public WellCorrelationBuilder WithCorrelationConfiguration(WellCorrelationConfiguration configuration)
        {
            correlationConfiguration = configuration ?? new WellCorrelationConfiguration();
            return this;
        }

        /// <summary>
        /// Sets the zoom factor.
        /// </summary>
        public WellCorrelationBuilder WithZoom(float zoom)
        {
            this.zoom = zoom;
            return this;
        }

        /// <summary>
        /// Sets the canvas size.
        /// </summary>
        public WellCorrelationBuilder WithSize(int width, int height)
        {
            this.width = width;
            this.height = height;
            return this;
        }

        /// <summary>
        /// Builds the drawing engine with the multi-well correlation layer.
        /// </summary>
        public DrawingEngine Build()
        {
            var panels = ResolvePanels();
            if (panels.Count == 0)
                throw new InvalidOperationException("At least one well correlation panel must be provided before building.");

            var engine = new DrawingEngine(width, height)
            {
                BackgroundColor = defaultLogConfiguration.BackgroundColor
            };
            engine.SetZoom(zoom);

            engine.AddLayer(new MultiWellCorrelationLayer(panels, defaultLogConfiguration, correlationConfiguration)
            {
                Name = "Well Correlation",
                IsVisible = true,
                ZOrder = 0
            });

            return engine;
        }

        /// <summary>
        /// Creates a new builder instance.
        /// </summary>
        public static WellCorrelationBuilder Create()
        {
            return new WellCorrelationBuilder();
        }

        private List<WellCorrelationPanel> ResolvePanels()
        {
            return panelSources.Select(ResolvePanel).ToList();
        }

        private WellCorrelationPanel ResolvePanel(PanelSource source)
        {
            if (source.ResolvedPanel != null)
                return source.ResolvedPanel;

            var loader = source.Loader;
            if (loader == null)
            {
                if (string.IsNullOrWhiteSpace(source.DataSource) || !source.LoaderType.HasValue)
                    throw new InvalidOperationException("Correlation panel source is missing both resolved log data and loader information.");

                loader = DataLoaderFactory.CreateLogLoader(source.DataSource, source.LoaderType.Value, source.ConnectionFactory);
            }

            var result = loader.LoadLogWithResult(source.WellIdentifier, source.LogName, source.LoadConfiguration);
            if (result == null || !result.Success || result.Data == null)
            {
                var message = result == null
                    ? "Unknown error occurred while loading log data."
                    : string.Join("; ", result.Errors);
                throw new InvalidOperationException($"Failed to load correlation panel data: {message}");
            }

            return new WellCorrelationPanel
            {
                PanelName = source.PanelName,
                LogData = result.Data,
                DeviationSurvey = source.DeviationSurvey,
                LogConfiguration = source.LogConfiguration,
                Markers = source.Markers ?? new List<WellCorrelationMarker>()
            };
        }

        private sealed class PanelSource
        {
            public string PanelName { get; set; }
            public WellCorrelationPanel ResolvedPanel { get; set; }
            public ILogLoader Loader { get; set; }
            public string DataSource { get; set; }
            public DataLoaderType? LoaderType { get; set; }
            public Func<DbConnection> ConnectionFactory { get; set; }
            public string WellIdentifier { get; set; }
            public string LogName { get; set; }
            public LogLoadConfiguration LoadConfiguration { get; set; }
            public DeviationSurvey DeviationSurvey { get; set; }
            public List<WellCorrelationMarker> Markers { get; set; }
            public LogRendererConfiguration LogConfiguration { get; set; }

            public static PanelSource FromResolvedPanel(WellCorrelationPanel panel)
            {
                return new PanelSource
                {
                    PanelName = panel.PanelName,
                    ResolvedPanel = panel
                };
            }
        }
    }
}