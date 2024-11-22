using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beep.OilandGas.Models
{
    // Well Equipment
    public class EquipmentEntry
    {
        public EquipmentEntry()
        {
            ID = Guid.NewGuid();
        }
        public EquipmentEntry(int sq,string name, float depth, float width, float height)
        {
            ID = Guid.NewGuid();
            Seq = sq;
            Name = name;
            Depth = depth;
            Width = width;
            Height = height;
        }
        public EquipmentEntry(int sq, string name, float depth, float width, float height, SKPoint drawingPosition)
        {
            ID = Guid.NewGuid();
            Seq = sq;
            Name = name;
            Depth = depth;
            Width = width;
            Height = height;
            DrawingPosition = drawingPosition;
        }
        public EquipmentEntry(int sq, Guid id, string name, float depth, float width, float height, SKPoint drawingPosition)
        {
            ID = id;
            Seq = sq;
            Name = name;
            Depth = depth;
            Width = width;
            Height = height;
            DrawingPosition = drawingPosition;
        }

        public Guid ID { get; set; }
        public int Seq { get; set; }
        public string Name { get; set; }
        public float Depth { get; set; }
        public float Width { get;set; }
        public float Height { get; set; }
        public bool IsVertical { get; set; }
        public SKPoint DrawingPosition { get; set; } // Position where the equipment is drawn
    }

    public class WellData_Equip
    {
        public int ID { get; set; }
        public string UWI { get; set; }
        public string UBHI { get; set; }
        public int BoreHoleIndex { get; set; }
        public WellData_Equip()
        { GuidID = Guid.NewGuid().ToString(); }
        public string GuidID { get; set; }
        public float TopDepth { get; set; }
        public float BottomDepth { get; set; }
        public float Diameter { get; set; }
        public int TubeIndex { get; set; }
        public string EquipmentType { get; set; }
        public string EquipmentName { get; set; }
        public string EquipmentSvg { get; set; }
        public string ToolTipText { get; set; }
        public string EquipmentDescription { get; set; }
        public string EquipmentStatus { get; set; }
    }
}
