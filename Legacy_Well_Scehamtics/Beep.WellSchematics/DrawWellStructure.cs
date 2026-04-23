using SkiaSharp;


namespace Beep.WellSchematics
{
    public class WellStructureDrawer
    {
        private float diameterScale = 1;
        private float zoomFactor = 1.0f;
        private const float ZoomIncrement = 0.2f;
        float canvasWidth;
        float canvasHeight;
        float centerX;
        float spacing = 5;
        private int scaleWidth = 60; // Width of the scale area
                                     // Assume we have this field in the class:
                                     // This list will hold the bounds of each equipment along with associated data.
        private List<(SKRect bounds, string tooltip, Action onClick, int equipmentId)> equipmentInteractions = new List<(SKRect bounds, string tooltip, Action onClick, int equipmentId)>();

        private List<(float Depth, string Comment)> comments = new List<(float, string)>();
        private const int StartYOffset = 50; // Starting offset from the top of the canvas
        private const int BoreholeSpacing = 150; // Vertical spacing between boreholes
        private const int BoreholeDiameter = 100;
        private const int CasingOffset = 10; // Offset for casing from borehole center
        private const int TubingWidth = 20; // Width of the tubing
        private const float DepthToLengthScale = 1.0f; // Scaling factor from depth to length, adjust as needed
        private  int CanvasCenterX ; // Assuming a canvas width, adjust as needed
        private float depthScale;
        private const int BoreholeSpacingVertical = 150; // Vertical spacing between boreholes
        private const int BoreholeOffsetHorizontal = 200; // Horizontal offset for alternate boreholes
        public string Svgpath { get; set; }
        public Dictionary<string, SkiaSharp.Extended.Svg.SKSvg> equipmentBoundsList { get; set; }
        public bool IsSVGLocal { get; private set; }
        public void DrawWellStructure(SKCanvas canvas, WellData wellData,int width)
        {
            int yPos = StartYOffset;
            CanvasCenterX =( width / 2) - (BoreholeDiameter/2);
            bool drawOnLeft = true; // Flag to alternate between left and right

            foreach (var borehole in wellData.BoreHoles.OrderBy(b => b.TopDepth)) // Assuming a Depth property for ordering
            {
                int xPos = drawOnLeft ? CanvasCenterX - BoreholeOffsetHorizontal : CanvasCenterX + BoreholeOffsetHorizontal;

                DrawBorehole(canvas, borehole, xPos, yPos);

                // Alternate side for next borehole
                drawOnLeft = !drawOnLeft;

                // Update yPos for the next borehole if it's horizontal
                if (!borehole.IsVertical)
                {
                    yPos += BoreholeSpacingVertical;
                }
            }
        }
        private void DrawBorehole(SKCanvas canvas, WellData_Borehole borehole, int xPos, int yPos)
        {
            // Draw the borehole cylinder
            SKPaint boreholePaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Black, StrokeWidth = 5 };
            float BoreholeLength=borehole.BottomDepth-borehole.TopDepth;
            if (borehole.IsVertical)
            {
                // Draw a straight vertical cylinder for a vertical borehole
                canvas.DrawLine(new SKPoint(xPos, yPos), new SKPoint(xPos, yPos + BoreholeLength), boreholePaint);
            }
            else
            {
                // Draw a curved cylinder for an inclined/horizontal borehole
                SKPath boreholePath = new SKPath();
                boreholePath.MoveTo(xPos, yPos);
                boreholePath.CubicTo(xPos + BoreholeDiameter / 2, yPos + BoreholeLength / 3, xPos + BoreholeDiameter / 2, yPos + 2 * BoreholeLength / 3, xPos, yPos + BoreholeLength);
                canvas.DrawPath(boreholePath, boreholePaint);
            }

            // Draw tubing inside the borehole
          
                DrawTubing(canvas, borehole, xPos, yPos);
           

            // Draw casing on each side of the borehole
            DrawCasing(canvas, xPos, yPos);

            // Draw perforations and equipment
            DrawPerforations(canvas, borehole, xPos, yPos);
            DrawEquipmentOnBorehole(canvas, borehole, xPos, yPos);

            boreholePaint.Dispose();
        }
        public void DrawVerticalWellbore(SKCanvas canvas, WellData_Borehole borehole, int centerX, float depthToLengthScale)
        {
            if (!borehole.IsVertical)
            {
                throw new InvalidOperationException("The borehole is not vertical.");
            }

            SKPaint boreholePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 2
            };

            float topY = borehole.TopDepth * depthToLengthScale;
            float bottomY = borehole.BottomDepth * depthToLengthScale;
            float leftX = centerX - borehole.Diameter / 2;
            float rightX = centerX + borehole.Diameter / 2;

            SKRect boreholeRect = new SKRect(leftX, topY, rightX, bottomY);
            canvas.DrawRect(boreholeRect, boreholePaint);
        }

        private void DrawCasing(SKCanvas canvas, int xPos, int yPos)
        {
            // Drawing logic for casing on each side of the borehole
            // ...
        }
        private void DrawTubing(SKCanvas canvas, WellData_Borehole borehole, int xPos, int yPos)
        {
            int numberOfTubings = borehole.Tubing.Count;
            int tubingSpacing = 10; // Spacing between tubings

            if (numberOfTubings == 1)
            {
                // Draw a single tubing in the center of the borehole
                float tubingLength = borehole.Tubing[0].BottomDepth * DepthToLengthScale; // Scale depth to length
                SKRect tubingRect = new SKRect(xPos - TubingWidth / 2, yPos, xPos + TubingWidth / 2, yPos + tubingLength);
                DrawTubingRect(canvas, tubingRect);
            }
            else if (numberOfTubings == 2)
            {
                // Determine lengths based on depth
                float firstTubingLength = borehole.Tubing[0].BottomDepth * DepthToLengthScale;
                float secondTubingLength = borehole.Tubing[1].BottomDepth * DepthToLengthScale;

                // Calculate positions for two tubings
                float firstTubingXPos = xPos - TubingWidth - tubingSpacing / 2;
                float secondTubingXPos = xPos + tubingSpacing / 2;

                SKRect firstTubingRect = new SKRect(firstTubingXPos, yPos, firstTubingXPos + TubingWidth, yPos + firstTubingLength);
                SKRect secondTubingRect = new SKRect(secondTubingXPos, yPos, secondTubingXPos + TubingWidth, yPos + secondTubingLength);

                DrawTubingRect(canvas, firstTubingRect);
                DrawTubingRect(canvas, secondTubingRect);
            }
        }
        private void DrawTubingRect(SKCanvas canvas, SKRect tubingRect)
        {
            using (var paint = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.Blue })
            {
                canvas.DrawRect(tubingRect, paint);
            }
        }
        private void DrawEquipmentOnTubing(SKCanvas canvas, WellData_Tubing tubing, int xPos, int yPos)
        {
            // Drawing logic for equipment on tubing
            // ...
        }
        private void DrawEquipmentOnBorehole(SKCanvas canvas, WellData_Borehole borehole, int xPos, int yPos)
        {
            // Drawing logic for equipment on borehole
            // ...
        }
        private void DrawPerforations(SKCanvas canvas, WellData_Borehole borehole, int xPos, int yPos)
        {
            // Drawing logic for perforations on the casing closest to the borehole
            // ...
        }
        private void DrawSvgEquipment(SKCanvas canvas, int xPos, int yPos)
        {
            // Implement SVG rendering logic
            // ...
        }
        public void SetDepthScaling(float canvasHeight, float minDepth, float maxDepth)
        {
            float depthRange = maxDepth - minDepth;
            depthScale = canvasHeight / depthRange;
        }
        public void LoadFacilities(string svgpath = "")
        {
            Svgpath = svgpath;
            if (string.IsNullOrEmpty(svgpath))
            {
                IsSVGLocal = true;
            }
            if (IsSVGLocal)
            {
                equipmentBoundsList = Helper.LoadSvgResources(Helper.GetEmbeddedSvgResources());
            }
            
        }

    }
}
