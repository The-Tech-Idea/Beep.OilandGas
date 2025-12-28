using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beep.OilandGas.Models
{
    public class Annotation
    {
        public float Depth { get; set; }
        public string Text { get; set; }
        public string Symbole { get; set; }
        public SKPoint Position { get; set; }
    }

}

