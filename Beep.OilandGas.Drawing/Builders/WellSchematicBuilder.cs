using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.Layers;
using Beep.OilandGas.Drawing.Styling;
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
            engine.SetZoom(zoom);

            ILayer layer;
            if (useEnhancedRenderer)
            {
                var enhancedLayer = new EnhancedWellSchematicRenderer(wellData, configuration)
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

            engine.AddLayer(layer);

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
    }
}

