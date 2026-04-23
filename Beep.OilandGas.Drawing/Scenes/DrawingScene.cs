using SkiaSharp;
using System;
using System.Collections.Generic;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Interaction;
using Beep.OilandGas.Drawing.Validation;

namespace Beep.OilandGas.Drawing.Scenes
{
    /// <summary>
    /// Represents a typed scene contract that can carry CRS, bounds, metadata, and viewport state.
    /// </summary>
    public class DrawingScene
    {
        private readonly Dictionary<string, string> metadata = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the scene name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the scene rendering kind.
        /// </summary>
        public DrawingSceneKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the typed coordinate reference system.
        /// </summary>
        public CoordinateReferenceSystem CoordinateReferenceSystem { get; set; }

        /// <summary>
        /// Gets the persisted viewport state.
        /// </summary>
        public SceneViewportState ViewportState { get; } = new();

        /// <summary>
        /// Gets persisted interaction state for selections and measurements.
        /// </summary>
        public SceneInteractionState InteractionState { get; } = new();

        /// <summary>
        /// Gets or sets optional world bounds.
        /// </summary>
        public SKRect? WorldBounds { get; set; }

        /// <summary>
        /// Gets or sets the source system or adapter label.
        /// </summary>
        public string SourceSystem { get; set; }

        /// <summary>
        /// Gets scene metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata => metadata;

        public DrawingScene(string name, DrawingSceneKind kind, CoordinateReferenceSystem coordinateReferenceSystem)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Scene name is required.", nameof(name));

            Name = name.Trim();
            Kind = kind;
            CoordinateReferenceSystem = coordinateReferenceSystem ?? throw new ArgumentNullException(nameof(coordinateReferenceSystem));

            Validate();
        }

        /// <summary>
        /// Creates a depth scene with a local depth CRS.
        /// </summary>
        public static DrawingScene CreateDepthScene(string name, string depthUnitCode = "ft")
        {
            return new DrawingScene(name, DrawingSceneKind.Depth, CoordinateReferenceSystem.CreateDepth(depthUnitCode));
        }

        /// <summary>
        /// Creates a section scene with local section coordinates.
        /// </summary>
        public static DrawingScene CreateSectionScene(string name, string distanceUnitCode = "ft", string depthUnitCode = "ft")
        {
            return new DrawingScene(name, DrawingSceneKind.Section, CoordinateReferenceSystem.CreateSection(distanceUnitCode, depthUnitCode));
        }

        /// <summary>
        /// Creates a map scene, defaulting to CRS84 if no CRS is supplied.
        /// </summary>
        public static DrawingScene CreateMapScene(string name, CoordinateReferenceSystem coordinateReferenceSystem = null)
        {
            return new DrawingScene(name, DrawingSceneKind.Map, coordinateReferenceSystem ?? CoordinateReferenceSystem.CreateGeographicCrs84());
        }

        /// <summary>
        /// Adds or replaces a metadata value.
        /// </summary>
        public void SetMetadata(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Metadata key is required.", nameof(key));

            metadata[key.Trim()] = value ?? string.Empty;
        }

        /// <summary>
        /// Validates that the scene kind and CRS are compatible.
        /// </summary>
        public void Validate()
        {
            DrawingSceneValidator.EnsureValid(this);
        }
    }
}