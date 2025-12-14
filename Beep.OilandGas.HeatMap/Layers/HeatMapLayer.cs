using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Layers
{
    /// <summary>
    /// Represents a layer in a multi-layer heat map.
    /// </summary>
    public class HeatMapLayer
    {
        /// <summary>
        /// Gets or sets the name of the layer.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the data points in this layer.
        /// </summary>
        public List<HeatMapDataPoint> DataPoints { get; set; }

        /// <summary>
        /// Gets or sets whether the layer is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets the opacity of the layer (0.0 to 1.0).
        /// </summary>
        public double Opacity { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the z-order of the layer (higher values render on top).
        /// </summary>
        public int ZOrder { get; set; } = 0;

        /// <summary>
        /// Gets or sets the color scheme type for this layer.
        /// </summary>
        public ColorSchemes.ColorSchemeType ColorSchemeType { get; set; } = ColorSchemes.ColorSchemeType.Viridis;

        /// <summary>
        /// Gets or sets custom colors for this layer.
        /// </summary>
        public SKColor[] CustomColors { get; set; }

        /// <summary>
        /// Gets or sets the minimum point radius for this layer.
        /// </summary>
        public float MinPointRadius { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the maximum point radius for this layer.
        /// </summary>
        public float MaxPointRadius { get; set; } = 20f;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeatMapLayer"/> class.
        /// </summary>
        public HeatMapLayer()
        {
            DataPoints = new List<HeatMapDataPoint>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeatMapLayer"/> class.
        /// </summary>
        /// <param name="name">Name of the layer.</param>
        /// <param name="dataPoints">Data points in the layer.</param>
        public HeatMapLayer(string name, List<HeatMapDataPoint> dataPoints)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DataPoints = dataPoints ?? throw new ArgumentNullException(nameof(dataPoints));
        }
    }

    /// <summary>
    /// Manages multiple layers in a heat map.
    /// </summary>
    public class MultiLayerHeatMap
    {
        /// <summary>
        /// Gets the list of layers.
        /// </summary>
        public List<HeatMapLayer> Layers { get; }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLayerHeatMap"/> class.
        /// </summary>
        public MultiLayerHeatMap()
        {
            Layers = new List<HeatMapLayer>();
        }

        /// <summary>
        /// Adds a layer to the heat map.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        public void AddLayer(HeatMapLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            Layers.Add(layer);
            SortLayersByZOrder();
        }

        /// <summary>
        /// Removes a layer from the heat map.
        /// </summary>
        /// <param name="layerName">Name of the layer to remove.</param>
        /// <returns>True if the layer was found and removed.</returns>
        public bool RemoveLayer(string layerName)
        {
            var layer = Layers.FirstOrDefault(l => l.Name == layerName);
            if (layer != null)
            {
                Layers.Remove(layer);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a layer by name.
        /// </summary>
        /// <param name="layerName">Name of the layer.</param>
        /// <returns>The layer, or null if not found.</returns>
        public HeatMapLayer GetLayer(string layerName)
        {
            return Layers.FirstOrDefault(l => l.Name == layerName);
        }

        /// <summary>
        /// Toggles the visibility of a layer.
        /// </summary>
        /// <param name="layerName">Name of the layer.</param>
        /// <returns>True if the layer was found.</returns>
        public bool ToggleLayerVisibility(string layerName)
        {
            var layer = GetLayer(layerName);
            if (layer != null)
            {
                layer.IsVisible = !layer.IsVisible;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the opacity of a layer.
        /// </summary>
        /// <param name="layerName">Name of the layer.</param>
        /// <param name="opacity">Opacity value (0.0 to 1.0).</param>
        /// <returns>True if the layer was found.</returns>
        public bool SetLayerOpacity(string layerName, double opacity)
        {
            var layer = GetLayer(layerName);
            if (layer != null)
            {
                layer.Opacity = Math.Max(0.0, Math.Min(1.0, opacity));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the z-order of a layer.
        /// </summary>
        /// <param name="layerName">Name of the layer.</param>
        /// <param name="zOrder">Z-order value (higher renders on top).</param>
        /// <returns>True if the layer was found.</returns>
        public bool SetLayerZOrder(string layerName, int zOrder)
        {
            var layer = GetLayer(layerName);
            if (layer != null)
            {
                layer.ZOrder = zOrder;
                SortLayersByZOrder();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all visible layers sorted by z-order.
        /// </summary>
        /// <returns>List of visible layers.</returns>
        public List<HeatMapLayer> GetVisibleLayers()
        {
            return Layers.Where(l => l.IsVisible)
                        .OrderBy(l => l.ZOrder)
                        .ToList();
        }

        /// <summary>
        /// Sorts layers by z-order.
        /// </summary>
        private void SortLayersByZOrder()
        {
            Layers.Sort((a, b) => a.ZOrder.CompareTo(b.ZOrder));
        }
    }
}

