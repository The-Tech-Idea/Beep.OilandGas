using SkiaSharp;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;


namespace Beep.WellSchematics
{
    public class SchematicsDraw
    {
        public int paddingBetweenTubes { get; set; }
        public float paddingToSide { get; set; }
        public float wellboreWidth { get; set; }
        private const float tubeDiameter = 40;
        private Dictionary<string, SKSvg> svgResources;
        private float depthScale=1;
        private float diameterScale=1;
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
        private float clickedDepth = -1;

        // Points on the outer and inner curves of the wellbore
        private Dictionary<int, List<(float x, float y)>> outerWellborePoints = new Dictionary<int, List<(float x, float y)>>();
        private Dictionary<int, List<(float x, float y)>> innerWellborePoints = new Dictionary<int, List<(float x, float y)>>();

        // Points on the outer and inner curves of the wellbore
        private Dictionary<int, List<(float x, float y)>> outerCasingPoints = new Dictionary<int, List<(float x, float y)>>();
        private Dictionary<int, List<(float x, float y)>> innerCasingPoints = new Dictionary<int, List<(float x, float y)>>();
        private Dictionary<int, List<(float inclination, float depth)>> inclinations = new Dictionary<int, List<(float inclination, float depth)>>();
        private Dictionary<int, List<float>> boreholeDepths = new Dictionary<int, List<float>>();
        private Dictionary<int, List<float>> boreholeCurvatures = new Dictionary<int, List<float>>();

         public  WellData WellData { get; set; } = new WellData();   
        public Dictionary<string, SkiaSharp.Extended.Svg.SKSvg> equipmentBoundsList { get; set; }
        bool IsSVGLocal = true;


        float minDepth;
        float maxDepth;
        float minDiameter;
        float maxDiameter;
      
        public SchematicsDraw(IDMEEditor dMEEditor, string svgpath = "")
        {
            DMEEditor = dMEEditor;
            Svgpath = svgpath;
            LoadFacilities(Svgpath);
        }
        public IDMEEditor DMEEditor { get; }
        public string Svgpath { get; set; }

        public IErrorsInfo LoadFacilities(string svgpath = "")
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
            return DMEEditor.ErrorObject;
        }
        public void DrawWellbore(SKCanvas canvas, int wellboreIndex)
        {
            // Validation checks
            if (WellData.BoreHoles == null || WellData.BoreHoles.Count <= wellboreIndex)
            {
                throw new ArgumentException("Invalid wellbore index");
            }

            var wellboreData = WellData.BoreHoles[wellboreIndex];

            if (wellboreData == null)
            {
                throw new ArgumentException("Wellbore data cannot be null");
            }

            if (WellData.BoreHoles.Any(d => d.TopDepth < 0 || d.BottomDepth < 0 || d.Diameter < 0))
            {
                throw new ArgumentException("Wellbore data cannot contain negative depths or diameters");
            }

            var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };

            var wellborePath = new SKPath();
       

            // Assuming you reset the stored points for each drawing
            outerWellborePoints.Remove(wellboreIndex);
            innerWellborePoints.Remove(wellboreIndex);
            WellData_Borehole borehole = WellData.BoreHoles[wellboreIndex];
          
            if(borehole.IsVertical)
            {
                DrawVerticalWellbore(canvas, wellboreIndex, borehole, paint, wellborePath);
            }
            else
            {
                DrawCurvedWellbore(canvas, wellboreIndex, borehole, paint);
            }   
          
        }
        private void DrawVerticalWellbore(SKCanvas canvas, int wellboreIndex, WellData_Borehole borehole, SKPaint paint, SKPath wellborePath)
        {
            var thisOuterWellborePoints = new List<(float x, float y)>();
            var thisInnerWellborePoints = new List<(float x, float y)>();
            float yTop = borehole.TopDepth * depthScale;
            float yBottom = borehole.BottomDepth * depthScale;
            borehole.OuterDiameterOffset = borehole.Diameter * diameterScale;
            float innerDiameterOffset = (borehole.Diameter - 2) * diameterScale / 2; // Adjust 'someThickness' as needed
           
            float xOuterLeft = centerX - borehole.OuterDiameterOffset;
            float xOuterRight = centerX + borehole.OuterDiameterOffset;
            float xInnerLeft = centerX - innerDiameterOffset;
            float xInnerRight = centerX + innerDiameterOffset;
            // Constructing and drawing the wellbore path
            wellborePath.MoveTo(xOuterLeft, yTop);
            wellborePath.LineTo(xOuterLeft, yBottom);
            wellborePath.LineTo(xInnerLeft, yBottom);
            wellborePath.LineTo(xInnerRight, yBottom);
            wellborePath.LineTo(xOuterRight, yBottom);
            wellborePath.LineTo(xOuterRight, yTop);
            wellborePath.Close();
            // Store the calculated points
            thisOuterWellborePoints.Add((xOuterLeft, yTop));
            thisOuterWellborePoints.Add((xOuterLeft, yBottom));
            thisInnerWellborePoints.Add((xInnerLeft, yTop));
            thisInnerWellborePoints.Add((xInnerLeft, yBottom));
            // Draw the paths
            canvas.DrawPath(wellborePath, paint);
          


            // Store these points for later use
            outerWellborePoints.Add(wellboreIndex, thisOuterWellborePoints);
            innerWellborePoints.Add(wellboreIndex, thisInnerWellborePoints);
  
        }
        private void DrawCurvedWellbore(SKCanvas canvas, int wellboreIndex, WellData_Borehole wellboreData, SKPaint paint)
        {
            var thisOuterWellborePoints = new List<(float x, float y)>();
            var thisInnerWellborePoints = new List<(float x, float y)>();
            // Determine direction and depth offset for the control point
            bool curveRight = wellboreIndex % 2 == 0;
            float depthOffsetFactor = 0.5f; // Adjust this factor as needed
            float controlDepth = (wellboreData.TopDepth + wellboreData.BottomDepth) * depthOffsetFactor * depthScale;

            // Set the horizontal offset for control points based on borehole index
            float horizontalOffset = 50; // Adjust the magnitude of curve
            if (!curveRight)
            {
                horizontalOffset *= -1; // Curve to the left for odd-indexed boreholes
            }

            // Define control points for the Bezier curve
            List<SKPoint> controlPoints = new List<SKPoint>
    {
        new SKPoint(100, wellboreData.TopDepth * depthScale), // Start point
        new SKPoint(100 + horizontalOffset, controlDepth),    // Control point
        new SKPoint(100, wellboreData.BottomDepth * depthScale) // End point
    };

            // Calculate the points on the Bezier curve
            List<SKPoint> bezierPoints = Helper.CalculateBezierCurve(controlPoints);

            // Create and draw the Bezier curve
            var path = new SKPath();
            path.MoveTo(bezierPoints[0]);
            foreach (var point in bezierPoints)
            {
                path.LineTo(point);
            }
            canvas.DrawPath(path, paint);

            // Calculate outer and inner points along the Bezier curve
            float outerDiameterOffset = wellboreData.Diameter * diameterScale / 2;
            float innerDiameterOffset = (wellboreData.Diameter - 2) * diameterScale / 2; // Adjust as needed

            thisOuterWellborePoints.Clear();
            thisInnerWellborePoints.Clear();
            foreach (var point in bezierPoints)
            {
                // Assuming outer and inner points are offset horizontally from the main curve
                thisOuterWellborePoints.Add((point.X - outerDiameterOffset, point.Y));
                thisInnerWellborePoints.Add((point.X + innerDiameterOffset, point.Y)); // Adjust if inner points need different offset
            }

            // Store these points for later use
            outerWellborePoints.Add(wellboreIndex, thisOuterWellborePoints);
            innerWellborePoints.Add(wellboreIndex, thisInnerWellborePoints);
        }
        private void DrawCurvedWellbore(SKCanvas canvas, float startX, int wellboreIndex, WellData_Borehole wellboreData, SKPaint paint)
        {
            var thisOuterWellborePoints = new List<(float x, float y)>();
            var thisInnerWellborePoints = new List<(float x, float y)>();

            if (wellboreIndex == 0 && wellboreData.IsVertical)
            {
                // Skip drawing for the first vertical wellbore
                return;
            }

            // Calculate the wellbore width to accommodate at least two tubing sizes and padding
            float wellboreWidth = CalculateWellboreWidth(wellboreIndex);

            // Determine direction and depth offset for the control point
            bool curveRight = wellboreIndex % 2 == 0;
            float depthOffsetFactor = 0.5f; // Adjust this factor as needed
            float controlDepth = (wellboreData.TopDepth + wellboreData.BottomDepth) * depthOffsetFactor * depthScale;

            // Set the fixed horizontal offset for control points based on wellboreWidth
            float horizontalOffset = curveRight ? wellboreWidth / 2 : -wellboreWidth / 2;

            // Define control points for the Bezier curve
            List<SKPoint> controlPoints = new List<SKPoint>
    {
        new SKPoint(startX, wellboreData.TopDepth * depthScale), // Start point
        new SKPoint(startX + horizontalOffset, controlDepth),    // Control point
        new SKPoint(startX, wellboreData.BottomDepth * depthScale) // End point
    };

            // Calculate the points on the Bezier curve
            List<SKPoint> bezierPoints = Helper.CalculateBezierCurve(controlPoints);

            // Create and draw the Bezier curve
            var path = new SKPath();
            path.MoveTo(bezierPoints[0]);
            foreach (var point in bezierPoints)
            {
                path.LineTo(point);
            }
            canvas.DrawPath(path, paint);

            // Calculate outer and inner points along the Bezier curve
            float outerDiameterOffset = wellboreWidth / 2; // Adjusted wellbore width
            float innerDiameterOffset = (wellboreWidth - 2) / 2; // Adjust as needed

            thisOuterWellborePoints.Clear();
            thisInnerWellborePoints.Clear();
            foreach (var point in bezierPoints)
            {
                // Assuming outer and inner points are offset horizontally from the main curve
                thisOuterWellborePoints.Add((point.X - outerDiameterOffset, point.Y));
                thisInnerWellborePoints.Add((point.X + innerDiameterOffset, point.Y)); // Adjust if inner points need different offset
            }

            // Store these points for later use
            outerWellborePoints.Add(wellboreIndex, thisOuterWellborePoints);
            innerWellborePoints.Add(wellboreIndex, thisInnerWellborePoints);
        }

        // Be sure to include the CalculateBezierCurve and other necessary methods
     
        
        public void DrawCasing(SKCanvas canvas, int wellboreIndex)
        {
            var boreholeData = WellData.BoreHoles[wellboreIndex];
            var casingDataList = boreholeData.Casing.OrderByDescending(c => c.BottomDepth).ToList(); // Sort casings by TopDepth

            var paint = new SKPaint
            {
                Color = SKColors.DarkGray,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };
            int offset = 1;
            foreach (var casing in casingDataList)
            {
                // Assuming outerDiameterOffset is accessible here or calculate based on borehole data
                float casingOffset = boreholeData.OuterDiameterOffset + (10*offset); // Additional offset to draw outside the wellbore
                offset++;
                float topDepth = casing.TopDepth * depthScale;
                float bottomDepth = casing.BottomDepth * depthScale;

                // Draw left line (outside the wellbore)
                SKPoint topPointLeft = new SKPoint(centerX - casingOffset, topDepth);
                SKPoint bottomPointLeft = new SKPoint(centerX - casingOffset, bottomDepth);
                canvas.DrawLine(topPointLeft, bottomPointLeft, paint);

                // Draw right line (outside the wellbore)
                SKPoint topPointRight = new SKPoint(centerX + casingOffset, topDepth);
                SKPoint bottomPointRight = new SKPoint(centerX + casingOffset, bottomDepth);
                canvas.DrawLine(topPointRight, bottomPointRight, paint);
            }
        }
        public void DrawEquipment(SKCanvas canvas, int wellboreIndex)
        {
            // Check if the wellbore ID exists in the dictionaries
            if (WellData.BoreHoles == null || WellData.BoreHoles.Count <= wellboreIndex)
            {
                throw new ArgumentException("Invalid wellbore index");
            }
            // Check if the provided data is valid
            
            var equipmentDataList = WellData.BoreHoles[wellboreIndex].Equip;
           
           
            List<SKPoint> outerWellboreSKPoints = outerWellborePoints[wellboreIndex].Select(p => new SKPoint(p.Item1, p.Item2)).ToList();
            List<SKPoint> innerWellboreSKPoints = innerWellborePoints[wellboreIndex].Select(p => new SKPoint(p.Item1, p.Item2)).ToList();

            var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = SKColors.Blue,
                StrokeWidth = 1
            };

            foreach (var equipment in equipmentDataList)
            {
                float topT = (equipment.TopDepth - minDepth) / (maxDepth - minDepth);
                float bottomT = (equipment.BottomDepth - minDepth) / (maxDepth - minDepth);

                SKPoint topPosition = Helper.GetPointOnCurve(topT, innerWellboreSKPoints);
                SKPoint bottomPosition = Helper.GetPointOnCurve(bottomT, innerWellboreSKPoints);

                float height = bottomPosition.Y - topPosition.Y;
                


                // Load SVG
                SkiaSharp.Extended.Svg.SKSvg svg = Helper.LoadSvg(equipment.EquipmentSvg, false);
                


                if (svg != null && svg.Picture != null)
                {
                    equipment.Diameter=svg.Picture.CullRect.Width;
                    float width = equipment.Diameter * diameterScale;
                    // Draw SVG
                    SKRect equipmentRect = new SKRect(topPosition.X - width / 2, topPosition.Y, topPosition.X + width / 2, topPosition.Y + height);
                    SKMatrix matrix = SKMatrix.CreateScale(width / svg.CanvasSize.Width, height / svg.CanvasSize.Height);
                    matrix.PostConcat(SKMatrix.CreateTranslation(equipmentRect.Left, equipmentRect.Top));
                    canvas.DrawPicture(svg.Picture, ref matrix, paint);

                    // Add equipment bounds, tooltip text, click handler, and ID to the list.
                    equipmentInteractions.Add((equipmentRect, equipment.ToolTipText, () => { /* Click action here */ }, equipment.ID));
                }
                
            }
        }
        private void DrawTubeEquipment(SKCanvas canvas, int boreholeIndex,int tubeindex)
        {
            if (WellData.BoreHoles == null || WellData.BoreHoles.Count <= boreholeIndex)
            {
                throw new ArgumentException("Invalid wellbore index");
            }
          
            var tubes = WellData.BoreHoles[boreholeIndex].Tubing;
            var equipmentDataList = tubes[tubeindex].Equip;
            var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = SKColors.Blue,
                StrokeWidth = 1
            };

            foreach (var equipment in equipmentDataList)
            {
                // Get the tube data for this equipment
                var tube = tubes[equipment.TubeIndex];

                // Generate the tube path
                var tubePath = GetTubePath(boreholeIndex, equipment.TubeIndex, maxDiameter / tubes.Count);

                // Calculate the positions of the equipment on the tube path
                var topPoint = Helper.GetPointOnCurve(tubePath, equipment.TopDepth * depthScale);
                var bottomPoint = Helper.GetPointOnCurve(tubePath, equipment.BottomDepth * depthScale);

                // Load SVG
                SkiaSharp.Extended.Svg.SKSvg svg = Helper.LoadSvg(equipment.EquipmentSvg, false);

                var equipmentBounds = svg.Picture.CullRect;

                // Calculate scale factors
                float scaleX = Math.Abs(bottomPoint.X - topPoint.X) / equipmentBounds.Width;
                float scaleY = tube.Diameter * diameterScale / equipmentBounds.Height;

                // Create transformation matrix
                SKMatrix matrix = SKMatrix.CreateScale(scaleX, scaleY);
                matrix = matrix.PreConcat(SKMatrix.CreateTranslation(topPoint.X, topPoint.Y));

                // Draw SVG
                canvas.Save();
                canvas.SetMatrix(matrix);
                canvas.DrawPicture(svg.Picture, ref matrix, paint);
                canvas.Restore();

                // Store bounds and tooltip text for interaction
                SKRect equipmentRect = new SKRect(topPoint.X - scaleX * equipmentBounds.Width / 2, topPoint.Y, bottomPoint.X + scaleX * equipmentBounds.Width / 2, bottomPoint.Y);
                equipmentInteractions.Add((equipmentRect, equipment.ToolTipText, () => { /* click action here */ }, equipment.ID));
            }
        }
        private void DrawTubes(SKCanvas canvas, int boreholeIndex, float startX)
        {
            if (WellData.BoreHoles == null || WellData.BoreHoles.Count <= boreholeIndex)
            {
                throw new ArgumentException("Invalid wellbore index");
            }

            var tubes = WellData.BoreHoles[boreholeIndex].Tubing;

            var tubePaint = new SKPaint
            {
                Color = SKColors.Blue,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };

            float totalPadding = (tubes.Count - 1) * paddingBetweenTubes + 2 * paddingToSide;
            float tubeSpace = (wellboreWidth - totalPadding) / tubes.Count;

            for (int tubeIndex = 0; tubeIndex < tubes.Count; tubeIndex++)
            {
                var tube = tubes[tubeIndex];
                var tubePath = GetTubePath(startX,  boreholeIndex, tubeIndex, tubeSpace);
                canvas.DrawPath(tubePath, tubePaint);
                DrawTubeEquipment(canvas, boreholeIndex, tubeIndex);
            }
        }
        private SKPath GetTubePath(float startX,  int boreholeIndex, int tubeIndex, float tubeSpace)
        {
            float leftPadding = paddingToSide;
            float topY = WellData.BoreHoles[boreholeIndex].TopDepth * depthScale;
            float bottomY = WellData.BoreHoles[boreholeIndex].BottomDepth * depthScale;
            float scaledtubeDiameter = tubeDiameter * diameterScale;

            // Calculate the horizontal position of the tube within the wellbore
            float tubeXLeft = startX + leftPadding + tubeSpace * tubeIndex;
            float tubeXRight = tubeXLeft + scaledtubeDiameter; // Adjust for the tube's diameter

            // Create a path for the tube as a rectangle (side view of the cylindrical tube)
            var tubePath = new SKPath();
            tubePath.MoveTo(tubeXLeft, topY);
            tubePath.LineTo(tubeXRight, topY);
            tubePath.LineTo(tubeXRight, bottomY);
            tubePath.LineTo(tubeXLeft, bottomY);
            tubePath.Close();

            return tubePath;
        }
        private SKPath GetTubePath(int boreholeIndex, int tubeIndex, float tubeSpace)
        {

            var tube = WellData.BoreHoles[boreholeIndex].Tubing[tubeIndex];

            SKPath tubePath = new SKPath();

            var startDepth = tube.TopDepth;
            var endDepth = tube.BottomDepth;

            // Let's iterate through the borehole depths at a 1m interval
            for (float depth = startDepth; depth <= endDepth; depth += 1)
            {
                SKPoint pointOnCurve = Helper.GetPointOnCurve(innerWellborePoints[boreholeIndex], depth);
                float inclination = GetInclinationAtDepth(boreholeIndex, depth);
                float shift = (float)(tubeSpace / 2 * (1 + Math.Cos(inclination)));

                float tubeCenterX = pointOnCurve.X;
                float tubeInnerX = tubeCenterX - tube.Diameter * tubeSpace / 2;
                float tubeOuterX = tubeCenterX + tube.Diameter * tubeSpace / 2;

                tubeInnerX -= shift;
                tubeOuterX += shift;


                // For the start of the tube, move to the first point
                if (depth == startDepth)
                {
                    tubePath.MoveTo(tubeInnerX, depth * depthScale);
                }
                else
                {
                    // Draw lines to the inner and outer points of the tube
                    tubePath.LineTo(tubeInnerX, depth * depthScale);
                    tubePath.LineTo(tubeOuterX, depth * depthScale);
                }
            }

            // After the last point, close the path to make it a complete loop
            tubePath.Close();

            return tubePath;
        }
        public void DrawPerforations(SKCanvas canvas, int wellboreIndex)
        {
            if (WellData.BoreHoles == null || WellData.BoreHoles.Count <= wellboreIndex)
            {
                throw new ArgumentException("Invalid wellbore index");
            }
            // Check if the provided data is valid
           

            var perforationDataList = WellData.BoreHoles[wellboreIndex].Perforation;

            if (perforationDataList == null || perforationDataList.Count == 0)
            {
                return;
            }

            if (perforationDataList.Any(d => d.TopDepth < 0 || d.BottomDepth < 0))
            {
                throw new ArgumentException("Perforation data cannot contain negative depths");
            }

            var perforationPaint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1
            };

            var wellborePoints = outerWellborePoints[wellboreIndex];
            var topDepth = perforationDataList.Min(p => p.TopDepth);
            var bottomDepth = perforationDataList.Max(p => p.BottomDepth);

            foreach (var perforation in perforationDataList)
            {
                float topT = (perforation.TopDepth - topDepth) / (bottomDepth - topDepth);
                float bottomT = (perforation.BottomDepth - topDepth) / (bottomDepth - topDepth);

                SKPoint topPosition = Helper.GetPointOnCurve(wellborePoints, topT);
                SKPoint bottomPosition = Helper.GetPointOnCurve(wellborePoints, bottomT);

                var middlePosition = new SKPoint((topPosition.X + bottomPosition.X) / 2, (topPosition.Y + bottomPosition.Y) / 2);
                var height = bottomPosition.Y - topPosition.Y;
                var trianglePoints = new SKPoint[3];
                trianglePoints[0] = new SKPoint(middlePosition.X, middlePosition.Y - height / 2);
                trianglePoints[1] = new SKPoint(middlePosition.X - height / 4, middlePosition.Y + height / 2);
                trianglePoints[2] = new SKPoint(middlePosition.X + height / 4, middlePosition.Y + height / 2);

                canvas.DrawVertices(SKVertexMode.Triangles, trianglePoints, null, null, perforationPaint);
            }
        }
        public void DrawCompleteWellbore(SKCanvas canvas, int wellboreIndex)
        {
           
            // Draw the borehole first
            DrawWellbore(canvas, wellboreIndex);
            // Draw casing in the borehole
            DrawCasing(canvas, wellboreIndex);
            // Draw equipment on the borehole
            DrawEquipment(canvas, wellboreIndex);
            // Draw perforations on the borehole
            DrawPerforations(canvas, wellboreIndex);
            // Draw tubes in the borehole
            DrawTubes(canvas, wellboreIndex, wellboreWidth); // Pass the wellboreWidth parameter
                                                             // Draw equipment on the tube
          //  DrawEquipment(canvas, wellboreIndex); // Pass the wellboreWidth parameter
        }

        public void DrawWell(SKCanvas canvas, WellData well,int width,int height)
        {
            canvasHeight=height;
            canvasWidth = width;
            // Calculate the center X-coordinate based on the canvas size
            centerX = canvasWidth / 2;
            WellData = well;
            spacing = 5; // Spacing between casing segments

          
            CalcMinMaxWellBore();
            SetDepthScaling(canvasHeight, minDepth, maxDepth);
            foreach (var borehole in WellData.BoreHoles)
            {
                DrawCompleteWellbore(canvas, borehole.BoreHoleIndex);
            }
        }
        public void SetDepthScaling(float canvasHeight, float minDepth, float maxDepth)
        {
            float depthRange = maxDepth - minDepth;
            depthScale = canvasHeight / depthRange;
        }
        private float GetInclinationAtDepth(int boreholeIndex, float depth)
        {
            // Assuming boreholeInclinations is a Dictionary<int, List<(float depth, float inclination)>>
            // which maps boreholeIndex to a list of depth-inclination pairs
            if(inclinations==null)
            {
                return 0;
            }
            if (inclinations.Count==0)
            {
                return 0;
            }
            var pinclinations = inclinations[boreholeIndex];

            // If there are no inclinations for this borehole, return 0
            if (pinclinations == null || pinclinations.Count == 0)
            {
                return 0;
            }

            // If depth is less than the first depth, return the first inclination
            if (depth <= pinclinations[0].depth)
            {
                return pinclinations[0].inclination;
            }

            // If depth is more than the last depth, return the last inclination
            // If depth is more than the last depth, return the last inclination
            if (depth >= pinclinations[pinclinations.Count - 1].depth)
            {
                return pinclinations[pinclinations.Count - 1].inclination;
            }


            // Find the two depths which our depth is between
            for (int i = 0; i < pinclinations.Count - 1; i++)
            {
                if (pinclinations[i].depth <= depth && depth <= pinclinations[i + 1].depth)
                {
                    // Interpolate the inclination
                    float incl1 = pinclinations[i].inclination;
                    float incl2 = pinclinations[i + 1].inclination;
                    float depth1 = pinclinations[i].depth;
                    float depth2 = pinclinations[i + 1].depth;

                    return incl1 + ((depth - depth1) / (depth2 - depth1)) * (incl2 - incl1);
                }
            }

            // If somehow we didn't return already, return 0
            return 0;
        }
        private float GetCurvatureAtDepth(int boreholeIndex, float depth)
        {
            // This function assumes you have a list of depths and corresponding curvatures for each borehole
            int depthIndex = boreholeDepths[boreholeIndex].BinarySearch(depth);
            if (depthIndex < 0)
                depthIndex = ~depthIndex;
            return boreholeCurvatures[boreholeIndex][depthIndex];
        }
        public void CalcMinMaxWellBore()
        {
            minDepth = WellData.BoreHoles.Min(d => d.TopDepth);
            maxDepth = WellData.BoreHoles.Max(d=> d.BottomDepth);
            minDiameter = WellData.BoreHoles.Min(d => d.Diameter);
            maxDiameter = WellData.BoreHoles.Max(d => d.Diameter);

        }
      
     
        private float CalculateWellboreWidth(int boreholeIndex)
        {
            // Determine the maximum tubing diameter among all tubing in this wellbore
            var tubes = WellData.BoreHoles[boreholeIndex].Tubing;
            float maxTubingDiameter = tubes.Max(tube => tube.Diameter);

            // Calculate the minimum wellbore width to accommodate at least two tubing sizes and padding
            float minWidth = maxTubingDiameter * 2 + paddingBetweenTubes * 2;

            // You can adjust the minWidth further if needed

            return minWidth;
        }

    }
}
