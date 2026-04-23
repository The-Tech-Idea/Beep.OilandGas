using System;
using System.Data.Common;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Layers;
using Beep.OilandGas.Drawing.Rendering;
using Beep.OilandGas.Drawing.Scenes;
using Beep.OilandGas.Drawing.Visualizations.Logs;

namespace Beep.OilandGas.Drawing.Builders
{
    /// <summary>
    /// Builder for creating well log visualizations with a fluent API.
    /// </summary>
    public class WellLogBuilder
    {
        private LogData logData;
        private DeviationSurvey deviationSurvey;
        private LogRendererConfiguration configuration = new LogRendererConfiguration();
        private ILogLoader logLoader;
        private LogLoadConfiguration loadConfiguration;
        private string wellIdentifier;
        private string logName;
        private string dataSource;
        private DataLoaderType? loaderType;
        private Func<DbConnection> connectionFactory;
        private float zoom = 1.0f;
        private int width = 1200;
        private int height = 1600;

        /// <summary>
        /// Sets the log data directly.
        /// </summary>
        public WellLogBuilder WithLogData(LogData data)
        {
            logData = data;
            return this;
        }

        /// <summary>
        /// Sets the deviation survey used to place the log along a directional path.
        /// </summary>
        public WellLogBuilder WithDeviationSurvey(DeviationSurvey survey)
        {
            deviationSurvey = survey;
            return this;
        }

        /// <summary>
        /// Sets the renderer configuration.
        /// </summary>
        public WellLogBuilder WithConfiguration(LogRendererConfiguration configuration)
        {
            this.configuration = configuration ?? new LogRendererConfiguration();
            return this;
        }

        /// <summary>
        /// Sets a loader instance and identifiers for loading real log data.
        /// </summary>
        public WellLogBuilder WithLoader(ILogLoader loader, string wellIdentifier = null, string logName = null, LogLoadConfiguration configuration = null)
        {
            logLoader = loader ?? throw new ArgumentNullException(nameof(loader));
            this.wellIdentifier = wellIdentifier;
            this.logName = logName;
            loadConfiguration = configuration;
            dataSource = null;
            loaderType = null;
            connectionFactory = null;
            return this;
        }

        /// <summary>
        /// Sets a file path and infers the appropriate log loader automatically.
        /// </summary>
        public WellLogBuilder WithFile(string filePath, string wellIdentifier = null, string logName = null, LogLoadConfiguration configuration = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            return WithLoader(DataLoaderFactory.CreateLogLoaderFromFile(filePath), wellIdentifier, logName, configuration);
        }

        /// <summary>
        /// Sets a data source description for loading real log data through the data loader factory.
        /// </summary>
        public WellLogBuilder WithDataSource(
            string dataSource,
            DataLoaderType loaderType,
            string wellIdentifier = null,
            string logName = null,
            LogLoadConfiguration configuration = null,
            Func<DbConnection> connectionFactory = null)
        {
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            this.loaderType = loaderType;
            this.wellIdentifier = wellIdentifier;
            this.logName = logName;
            loadConfiguration = configuration;
            this.connectionFactory = connectionFactory;
            logLoader = null;
            return this;
        }

        /// <summary>
        /// Sets the zoom factor.
        /// </summary>
        public WellLogBuilder WithZoom(float zoom)
        {
            this.zoom = zoom;
            return this;
        }

        /// <summary>
        /// Sets the canvas size.
        /// </summary>
        public WellLogBuilder WithSize(int width, int height)
        {
            this.width = width;
            this.height = height;
            return this;
        }

        /// <summary>
        /// Builds the drawing engine with the well log layer.
        /// </summary>
        public DrawingEngine Build()
        {
            var effectiveLogData = ResolveLogData();
            if (effectiveLogData == null)
                throw new InvalidOperationException("Log data must be set or loadable before building.");

            var engine = new DrawingEngine(width, height)
            {
                BackgroundColor = configuration.BackgroundColor
            };
            engine.SetZoom(zoom);

            ILayer layer = new WellLogLayer(effectiveLogData, deviationSurvey, configuration)
            {
                Name = "Well Log",
                IsVisible = true,
                ZOrder = 0
            };

            engine.UseScene(CreateScene(effectiveLogData, layer));
            engine.AddLayer(layer);
            return engine;
        }

        /// <summary>
        /// Creates a new builder instance.
        /// </summary>
        public static WellLogBuilder Create()
        {
            return new WellLogBuilder();
        }

        private LogData ResolveLogData()
        {
            if (logData != null)
                return logData;

            var effectiveLoader = logLoader;
            if (effectiveLoader == null && !string.IsNullOrWhiteSpace(dataSource) && loaderType.HasValue)
            {
                effectiveLoader = DataLoaderFactory.CreateLogLoader(dataSource, loaderType.Value, connectionFactory);
            }

            if (effectiveLoader == null)
                return null;

            var result = effectiveLoader.LoadLogWithResult(wellIdentifier, logName, loadConfiguration);
            if (result == null || !result.Success || result.Data == null)
            {
                var message = result == null
                    ? "Unknown error occurred while loading log data."
                    : string.Join("; ", result.Errors);
                throw new InvalidOperationException($"Failed to load log data: {message}");
            }

            logData = result.Data;
            return logData;
        }

        private DrawingScene CreateScene(LogData effectiveLogData, ILayer layer)
        {
            if (effectiveLogData == null)
                throw new ArgumentNullException(nameof(effectiveLogData));

            var sceneName = !string.IsNullOrWhiteSpace(effectiveLogData.LogName)
                ? effectiveLogData.LogName
                : !string.IsNullOrWhiteSpace(effectiveLogData.WellIdentifier)
                    ? effectiveLogData.WellIdentifier
                    : "Well Log";

            var scene = DrawingScene.CreateDepthScene(sceneName, ResolveDepthUnitCode(effectiveLogData.DepthUnit));
            scene.SourceSystem = effectiveLogData.WellIdentifier ?? sceneName;

            var bounds = layer.GetBounds();
            if (bounds.Width > 0 && bounds.Height > 0)
                scene.WorldBounds = bounds;

            scene.SetMetadata("Renderer", layer.Name ?? layer.GetType().Name);
            scene.SetMetadata("WellIdentifier", effectiveLogData.WellIdentifier ?? string.Empty);
            scene.SetMetadata("LogName", effectiveLogData.LogName ?? string.Empty);
            scene.SetMetadata("LogType", effectiveLogData.LogType ?? string.Empty);
            scene.SetMetadata("CurveCount", (effectiveLogData.Curves?.Count ?? 0).ToString());
            scene.SetMetadata("DepthUnit", effectiveLogData.DepthUnit ?? string.Empty);
            return scene;
        }

        private static string ResolveDepthUnitCode(string depthUnit)
        {
            if (string.IsNullOrWhiteSpace(depthUnit))
                return "ft";

            var normalized = depthUnit.Trim().ToLowerInvariant();
            return normalized switch
            {
                "m" or "meter" or "meters" or "metre" or "metres" => "m",
                _ => "ft"
            };
        }
    }
}