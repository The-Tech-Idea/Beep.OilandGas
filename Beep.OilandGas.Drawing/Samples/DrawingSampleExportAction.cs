using System;
using Beep.OilandGas.Drawing.Core;

namespace Beep.OilandGas.Drawing.Samples
{
    /// <summary>
    /// Describes how a host should present a supplemental export action.
    /// </summary>
    public enum DrawingSampleExportPresentationKind
    {
        Default,
        MapExchange,
        RasterBundle,
        EngineeringData
    }

    /// <summary>
    /// Describes an explicit scene-specific export action that a host can surface without guessing from scene names or layer types.
    /// </summary>
    public sealed class DrawingSampleExportAction
    {
        private readonly Func<DrawingEngine, byte[]> export;

        public DrawingSampleExportAction(
            string id,
            string label,
            string fileNameSuffix,
            string contentType,
            Func<DrawingEngine, byte[]> export,
            string description = null,
            DrawingSampleExportPresentationKind presentationKind = DrawingSampleExportPresentationKind.Default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));
            if (string.IsNullOrWhiteSpace(fileNameSuffix))
                throw new ArgumentNullException(nameof(fileNameSuffix));
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentNullException(nameof(contentType));

            this.export = export ?? throw new ArgumentNullException(nameof(export));
            Id = id;
            Label = label;
            FileNameSuffix = fileNameSuffix;
            ContentType = contentType;
            Description = description ?? string.Empty;
            PresentationKind = presentationKind;
        }

        public string Id { get; }

        public string Label { get; }

        public string Description { get; }

        public string FileNameSuffix { get; }

        public string ContentType { get; }

        public DrawingSampleExportPresentationKind PresentationKind { get; }

        public byte[] Export(DrawingEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));

            return export(engine);
        }
    }
}