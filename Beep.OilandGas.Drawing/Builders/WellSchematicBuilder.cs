using System;
using System.Collections.Generic;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Exceptions;
using Beep.OilandGas.Drawing.Layers;
using Beep.OilandGas.Drawing.Scenes;
using Beep.OilandGas.Drawing.Styling;
using Beep.OilandGas.Drawing.Validation;
using Beep.OilandGas.Drawing.Visualizations.WellSchematic;
using Beep.OilandGas.Drawing.Visualizations.WellSchematic.Configuration;
using Beep.OilandGas.Models;

namespace Beep.OilandGas.Drawing.Builders
{
    /// <summary>
    /// Builder for creating well schematic visualizations with a fluent API.
    /// </summary>
    public class WellSchematicBuilder
    {
        private WellData wellData;
        private readonly Dictionary<string, DeviationSurvey> deviationSurveys = new(StringComparer.OrdinalIgnoreCase);
        private Theme theme = Theme.Standard;
        private WellSchematicConfiguration configuration = WellSchematicConfiguration.Default;
        private float zoom = 1.0f;
        private bool showAnnotations = true;
        private bool showGrid = false;
        private bool showDepthScale = true;
        private bool useEnhancedRenderer = true;
        private int width = 800;
        private int height = 600;

        /// <summary>
        /// Sets the well data.
        /// </summary>
        /// <param name="data">The well data.</param>
        /// <returns>The builder instance.</returns>
        public WellSchematicBuilder WithWellData(WellData data)
        {
            wellData = data;
            return this;
        }

        /// <summary>
        /// Sets the well schematic wrapper data, including any deviation surveys.
        /// </summary>
        /// <param name="data">The well schematic wrapper data.</param>
        /// <returns>The builder instance.</returns>
        public WellSchematicBuilder WithWellSchematicData(WellSchematicData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            wellData = data.WellData;
            deviationSurveys.Clear();

            if (data.DeviationSurveys != null)
            {
                WithDeviationSurveys(data.DeviationSurveys);
            }

            return this;
        }

        /// <summary>
        /// Adds a deviation survey for a borehole.
        /// </summary>
        /// <param name="survey">The deviation survey.</param>
        /// <param name="key">Optional explicit key. Defaults to the borehole identifier.</param>
        /// <returns>The builder instance.</returns>
        public WellSchematicBuilder WithDeviationSurvey(DeviationSurvey survey, string key = null)
        {
            if (survey == null)
                throw new ArgumentNullException(nameof(survey));

            var report = DataNormalizationValidator.ValidateDeviationSurvey(survey);
            if (report.HasErrors)
                throw new DrawingValidationException($"Deviation survey validation failed: {report.BuildSummary()}", report);

            deviationSurveys[ResolveDeviationSurveyKey(survey, key)] = survey;
            return this;
        }

        /// <summary>
        /// Adds multiple deviation surveys.
        /// </summary>
        /// <param name="surveys">The surveys keyed by borehole identifier.</param>
        /// <returns>The builder instance.</returns>
        public WellSchematicBuilder WithDeviationSurveys(IDictionary<string, DeviationSurvey> surveys)
        {
            if (surveys == null)
                throw new ArgumentNullException(nameof(surveys));

            foreach (var entry in surveys)
            {
                if (entry.Value == null)
                    continue;

                WithDeviationSurvey(entry.Value, entry.Key);
            }

            return this;
        }

        /// <summary>
        /// Sets the theme.
        /// </summary>
        /// <param name="theme">The theme to use.</param>
        /// <returns>The builder instance.</returns>
        public WellSchematicBuilder WithTheme(Theme theme)
        {
            this.theme = theme ?? Theme.Standard;
            configuration.Theme = this.theme;
            return this;
        }

        /// <summary>
        /// Sets the configuration.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        /// <returns>The builder instance.</returns>
        public WellSchematicBuilder WithConfiguration(WellSchematicConfiguration config)
        {
            this.configuration = config ?? WellSchematicConfiguration.Default;
            return this;
        }

        /// <summary>
        /// Sets whether to use the enhanced renderer.
        /// </summary>
        /// <param name="useEnhanced">True to use enhanced renderer.</param>
        /// <returns>The builder instance.</returns>
        public WellSchematicBuilder UseEnhancedRenderer(bool useEnhanced = true)
        {
            useEnhancedRenderer = useEnhanced;
            return this;
        }

        /// <summary>
        /// Sets whether to show depth scale.
        /// </summary>
        /// <param name="show">True to show depth scale.</param>
        /// <returns>The builder instance.</returns>
        public WellSchematicBuilder WithDepthScale(bool show = true)
        {
            showDepthScale = show;
            return this;
        }

        /// <summary>
        /// Sets the zoom factor.
        /// </summary>
        /// <param name="zoom">The zoom factor.</param>
        /// <returns>The builder instance.</returns>
        public WellSchematicBuilder WithZoom(float zoom)
        {
            this.zoom = zoom;
            return this;
        }

        /// <summary>
        /// Sets whether to show annotations.
        /// </summary>
        /// <param name="show">True to show annotations.</param>
        /// <returns>The builder instance.</returns>
        public WellSchematicBuilder WithAnnotations(bool show)
        {
            showAnnotations = show;
            return this;
        }

        /// <summary>
        /// Sets whether to show grid.
        /// </summary>
        /// <param name="show">True to show grid.</param>
        /// <returns>The builder instance.</returns>
        public WellSchematicBuilder WithGrid(bool show)
        {
            showGrid = show;
            return this;
        }

        /// <summary>
        /// Sets the canvas size.
        /// </summary>
        /// <param name="width">Canvas width.</param>
        /// <param name="height">Canvas height.</param>
        /// <returns>The builder instance.</returns>
        public WellSchematicBuilder WithSize(int width, int height)
        {
            this.width = width;
            this.height = height;
            return this;
        }

        /// <summary>
        /// Builds the drawing engine with the well schematic layer.
        /// </summary>
        /// <returns>The configured drawing engine.</returns>
        public DrawingEngine Build()
        {
            if (wellData == null)
                throw new System.InvalidOperationException("Well data must be set before building.");

            var engine = new DrawingEngine(width, height);
            engine.BackgroundColor = theme.BackgroundColor;

            ILayer layer;
            if (useEnhancedRenderer)
            {
                var enhancedLayer = new EnhancedWellSchematicRenderer(
                    wellData,
                    configuration,
                    deviationSurveys.Count == 0 ? null : deviationSurveys)
                {
                    Name = "Enhanced Well Schematic",
                    IsVisible = true,
                    ZOrder = 0,
                    ShowAnnotations = showAnnotations,
                    ShowGrid = showGrid,
                    ShowDepthScale = showDepthScale
                };
                layer = enhancedLayer;
            }
            else
            {
                layer = new WellSchematicLayer(wellData, theme)
                {
                    Name = "Well Schematic",
                    IsVisible = true,
                    ZOrder = 0
                };
            }

            engine.UseScene(CreateScene(layer));
            engine.AddLayer(layer);
            engine.ZoomToFit();
            engine.SetZoom(engine.Viewport.Zoom * zoom);

            return engine;
        }

        /// <summary>
        /// Creates a new builder instance.
        /// </summary>
        /// <returns>A new builder instance.</returns>
        public static WellSchematicBuilder Create()
        {
            return new WellSchematicBuilder();
        }

        private static string ResolveDeviationSurveyKey(DeviationSurvey survey, string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
                return key;

            if (!string.IsNullOrWhiteSpace(survey.BoreholeIdentifier))
                return survey.BoreholeIdentifier;

            if (!string.IsNullOrWhiteSpace(survey.WellIdentifier))
                return survey.WellIdentifier;

            return Guid.NewGuid().ToString("N");
        }

        private DrawingScene CreateScene(ILayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            var sceneName = !string.IsNullOrWhiteSpace(wellData.UBHI)
                ? wellData.UBHI
                : !string.IsNullOrWhiteSpace(wellData.UWI)
                    ? wellData.UWI
                    : "Well Schematic";

            var scene = DrawingScene.CreateDepthScene(sceneName, "ft");
            scene.SourceSystem = wellData.UWI ?? sceneName;

            var bounds = layer.GetBounds();
            if (bounds.Width > 0 && bounds.Height > 0)
                scene.WorldBounds = bounds;

            scene.SetMetadata("Renderer", layer.Name ?? layer.GetType().Name);
            scene.SetMetadata("WellUwi", wellData.UWI ?? string.Empty);
            scene.SetMetadata("WellUbhi", wellData.UBHI ?? string.Empty);
            scene.SetMetadata("BoreholeCount", (wellData.BoreHoles?.Count ?? 0).ToString());
            return scene;
        }
    }
}

