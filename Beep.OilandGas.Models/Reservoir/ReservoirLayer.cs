using SkiaSharp;
using SkiaSharp.Extended.Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beep.OilandGas.Models
{
    // Class representing a reservoir layer
    public class ReservoirLayer
    {
        public float TopDepth { get; set; }
        public float BottomDepth { get; set; }
        public SKBitmap PatternOrTexture { get; set; }
        public SkiaSharp.Extended.Svg.SKSvg PatternOrTextureSvg { get; set; }
        public string LegendText { get; set; }

        // Constructor and other properties/methods...
    }
}

