using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Beep.OilandGas.Drawing.Core;

namespace Beep.OilandGas.Drawing.Samples
{
    /// <summary>
    /// Describes a reusable sample scene that can construct a fully configured drawing engine.
    /// </summary>
    public sealed class DrawingSampleScene
    {
        private readonly Func<DrawingEngine> createEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingSampleScene"/> class.
        /// </summary>
        public DrawingSampleScene(
            string name,
            string description,
            int width,
            int height,
            Func<DrawingEngine> createEngine,
            IEnumerable<DrawingSampleExportAction> supplementalExports = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            this.createEngine = createEngine ?? throw new ArgumentNullException(nameof(createEngine));
            Name = name;
            Description = description;
            Width = width;
            Height = height;
            SupplementalExports = new ReadOnlyCollection<DrawingSampleExportAction>(
                new List<DrawingSampleExportAction>(supplementalExports ?? Array.Empty<DrawingSampleExportAction>()));
        }

        /// <summary>
        /// Gets the stable scene name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the scene description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the recommended render width.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the recommended render height.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets any explicit scene-specific export actions a host can surface.
        /// </summary>
        public IReadOnlyList<DrawingSampleExportAction> SupplementalExports { get; }

        /// <summary>
        /// Builds a new drawing engine instance for the sample scene.
        /// </summary>
        public DrawingEngine CreateEngine()
        {
            return createEngine();
        }
    }
}