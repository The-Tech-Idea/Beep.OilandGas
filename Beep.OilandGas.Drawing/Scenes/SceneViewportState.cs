namespace Beep.OilandGas.Drawing.Scenes
{
    /// <summary>
    /// Represents viewport state that can be persisted with a scene.
    /// </summary>
    public sealed class SceneViewportState
    {
        /// <summary>
        /// Gets or sets the zoom factor.
        /// </summary>
        public float Zoom { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the horizontal pan.
        /// </summary>
        public float PanX { get; set; }

        /// <summary>
        /// Gets or sets the vertical pan.
        /// </summary>
        public float PanY { get; set; }

        /// <summary>
        /// Creates a copy of the current state.
        /// </summary>
        public SceneViewportState Clone()
        {
            return new SceneViewportState
            {
                Zoom = Zoom,
                PanX = PanX,
                PanY = PanY
            };
        }
    }
}