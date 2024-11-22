using SkiaSharp;
namespace Beep.HeatMap
{
    public class HeatMapGenerator
    {
        public List<HeatMapDataPoint> DataPoints { get; set; }
      //  public bool IsRedtoGreen { get; set; } = false;
        public SKColor StartColor { get; set; }= SKColors.Gray;
        public SKColor EndColor { get; set; } = SKColors.Black;
        private SKColor[] colorScale;
        private SKPaint textPaint;
        public double Zoom { get; set; } = 1;
        public double PanX { get; set; } = 0;
        public double PanY { get; set; } = 0;
        double centerX;
        double centerY;
        double width;
        double height;
        double minX;
        double maxX;
        double minY;
        double maxY;
        float currentCenterX;
        float currentCenterY;
        float canvasCenterX;
        float canvasCenterY;
        float dataMidXInPixels;
        float dataMidYInPixels;
        double dataWidthInPixels;
        double dataHeightInPixels;

        private SKPoint panOffset = new SKPoint(0, 0); // Start with no panning
       
        public HeatMapGenerator(List<HeatMapDataPoint> datapoints,double width, double height,  SKColor color)
        {


            // Convert UTM points to canvas coordinates using the method
            //  double[,] canvasCoordinates = ConvertListTo2DArrayUTMtoCanvas(datapoints);

            // Find the minimum and maximum values of the converted coordinates

           
            this.width = width;
            this.height = height;
           
            LoadPoint(datapoints);
            InitializeColorScale();
            InitializeTextPaint();
            init();
            // Ensure that dataWidthInPixels and dataHeightInPixels are not zero
            //if (datawidthinpixels > 0 && dataheightinpixels > 0)
            //{
            //    // calculate the zoom levels for width and height in pixel units
            //    double zoomlevelx = width / datawidthinpixels;
            //    double zoomlevely = height / dataheightinpixels;

            //    // use the smaller zoom level to ensure all data fits
            //    zoom = math.min(zoomlevelx, zoomlevely);
            //}
            //else
            //{
                // Handle the case when data dimensions are zero
                Zoom = 1.0;
           // }
        }
        public HeatMapGenerator(double width, double height, SKColor color)
        {


            // Convert UTM points to canvas coordinates using the method
            //  double[,] canvasCoordinates = ConvertListTo2DArrayUTMtoCanvas(datapoints);

            // Find the minimum and maximum values of the converted coordinates


            this.width = width;
            this.height = height;


            InitializeColorScale();
            InitializeTextPaint();
            //init();
            // Ensure that dataWidthInPixels and dataHeightInPixels are not zero
            if (dataWidthInPixels > 0 && dataHeightInPixels > 0)
            {
                // calculate the zoom levels for width and height in pixel units
                double zoomlevelx = width / dataWidthInPixels;
                double zoomlevely = height / dataHeightInPixels;

                // use the smaller zoom level to ensure all data fits
                Zoom = Math.Min(zoomlevelx, zoomlevely);
            }
            else
            {
               // Handle the case when data dimensions are zero
               Zoom = 1;
            }
            
        }
        public void LoadPoint(List<HeatMapDataPoint> datapoints)
        {
            DataPoints = new List<HeatMapDataPoint>();
            DataPoints = datapoints;
            ConvertUTMPointsToCanvasCoordinates(ref datapoints);
        }
        private void init()
        {
          

            // Calculate the midpoint of your data in canvas pixel coordinates
             dataMidXInPixels = (float)((maxX + minX) / 2.0);
             dataMidYInPixels = (float)((maxY + minY)  / 2.0);

            // These are the centers of the canvas in pixels
             canvasCenterX = (float)(width / 2.0);
             canvasCenterY = (float)(height / 2.0);

            // Calculate the zoom level to ensure all data fits within the canvas
             dataWidthInPixels = (maxX - minX) ;
             dataHeightInPixels = (maxY - minY);

          

            // Calculate the pan offset to align the data midpoint with the canvas center
            panOffset.X = (float)(canvasCenterX - dataMidXInPixels * Zoom);
            panOffset.Y = (float)(canvasCenterY - dataMidYInPixels * Zoom);

            // Update the currentCenterX and currentCenterY based on panOffset and Zoom
            currentCenterX = (float)(-panOffset.X + (width / 2.0) );
            currentCenterY = (float)(-panOffset.Y + (height / 2.0) );
        }
        public void Resize(double width, double height)
        {
            this.width = width;
            this.height = height;
            if (DataPoints != null)
            {
                minX = DataPoints.Min(point => point.X);
                maxX = DataPoints.Max(point => point.X);
                minY = DataPoints.Min(point => point.Y);
                maxY = DataPoints.Max(point => point.Y);
                init();
            }
         
        }
        private void InitializeColorScale()
        {
            // Define your color scale here, from low to high intensity
            colorScale = new SKColor[] { SKColors.Blue, SKColors.Green, SKColors.Yellow, SKColors.Red };
        }

        private void InitializeTextPaint()
        {
            textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextAlign = SKTextAlign.Center,
                TextSize = 8
            };
        }

        public void AddDataPoint(HeatMapDataPoint point)
        {
            DataPoints.Add(point);
        }
      
        public void Render(SKCanvas canvas)
        {
           
            canvas.Clear(SKColors.White);
             canvas.Save(); // Save the current state of the canvas
            // Apply the zoom and pan transformations
            canvas.Scale((float)Zoom); // Apply zoom
            
            canvas.Translate(panOffset.X, panOffset.Y); // Apply panning

            if (DataPoints == null)
            {
                return;
            }

            foreach (var point in DataPoints)
            {
                DrawDataPoint(canvas, point );
            }
            //  canvas.Restore(); // Restore the state of the canvas
            //canvas.Save();
            //// Apply transformations and draw
         //   canvas.Restore();

        }

        private void DrawDataPoint(SKCanvas canvas, HeatMapDataPoint point)
        {
            // Convert point coordinates to canvas coordinates
            var margin = 0.01; // 5% margin
            double x = point.X;
            double y = point.Y ;
            SKColor color;
            //if (IsRedtoGreen) { color = FromRedtoGreen(point); }
            //else { color = FromRedtoBlue(point);}
            color = GetColorFromValue(point.Value);
            //// Determine the intensity of the color based on the value
            //// Assuming the value is normalized between 0 (light) and 1 (dark)
            //byte alpha = (byte)(255 * point.Value); // Scales the alpha with the value
            //var color = new SKColor(Color.Red, Color.Green, Color.Blue, alpha);

            //// Determine color based on the value
            //var colorIndex = (int)(point.Value * (colorScale.Length - 1));
            //var color = colorScale[colorIndex];
            // Determine circle size based on the value
            // Here, the circle radius will scale between a min and max size based on the value
            float minRadius = 5; // Minimum radius for value = 0
            float maxRadius = 20; // Maximum radius for value = 1
            double radius = (minRadius + (point.Value * (maxRadius - minRadius)));

            // Draw the point
            var paint = new SKPaint { Color = color };
            canvas.DrawCircle(new SKPoint((float)x, (float)y), (float)radius, paint);

            // Draw the label
            // Adjust the y-offset for the label based on the circle's radius
            canvas.DrawText(point.Label, (float)x, (float)(y - radius - textPaint.TextSize), textPaint);
        }
        private static SKColor FromRedtoGreen(HeatMapDataPoint point)
        {
            byte redComponent = (byte)(255 * (1 - point.Value)); // Red decreases as value increases
            byte greenComponent = (byte)(255 * point.Value); // Green increases as value increases
           return new SKColor(redComponent, greenComponent, 0); // No blue component

        }
        private static SKColor FromRedtoBlue(HeatMapDataPoint point)
        {
            byte redComponent = (byte)(255 * (1 - point.Value)); // Red decreases as value increases
            byte blueComponent = (byte)(255 * point.Value); // Blue increases as value increases
            return new SKColor(redComponent, 0, blueComponent); // No green component

        }
      
        private static SKColor GetColorFromValue(double value)
        {
            // Linear interpolation between colors
            // Define your color scale here
            var colors = new SKColor[]  {SKColors.Gray,SKColors.Black };
            var index = value * (colors.Length - 1);
            var lowerIndex = (int)Math.Floor(index);
            var upperIndex = (int)Math.Ceiling(index);

            var mix = index - lowerIndex;
            return InterpolateColor(colors[lowerIndex], colors[Math.Min(upperIndex, colors.Length - 1)], mix);
        }
        public static SKColor lerp(SKColor color1, SKColor color2, double t)
        {
            byte r = (byte)Math.Round(color1.Red + (color2.Red - color1.Red) * t);
            byte g = (byte)Math.Round(color1.Green + (color2.Green - color1.Green) * t);
            byte b = (byte)Math.Round(color1.Blue + (color2.Blue - color1.Blue) * t);
            byte a = (byte)Math.Round(color1.Alpha + (color2.Alpha - color1.Alpha) * t);

            return new SKColor(r, g, b, a);
        }
        private static SKColor InterpolateColor(SKColor color1, SKColor color2, double amount)
        {
            var r = (byte)(color1.Red + (color2.Red - color1.Red) * amount);
            var g = (byte)(color1.Green + (color2.Green - color1.Green) * amount);
            var b = (byte)(color1.Blue + (color2.Blue - color1.Blue) * amount);
            var a = (byte)(color1.Alpha + (color2.Alpha - color1.Alpha) * amount);

            return new SKColor(r, g, b, a);
        }
        // Event handler for zooming (could be a button click, mouse wheel scroll, etc.)
        public void OnZoomIn()
        {
            Zoom *= .01f; // Increase zoom level by 10%
                          // Store the current center
             currentCenterX = (float)(-panOffset.X + (width / 2f) );
             currentCenterY = (float)(-panOffset.Y + (height / 2f) );
            // Adjust panOffset to keep the current center
            panOffset.X = (float)-(currentCenterX * Zoom - width / 2f);
            panOffset.Y = (float)-(currentCenterY * Zoom - height / 2f);
            // skControl.Invalidate(); // Invalidate the SKControl to trigger a repaint
        }

        public void OnZoomOut()
        {
            Zoom /= 1.1f; // Decrease zoom level by 10%
            currentCenterX = (float)(-panOffset.X + (width / 2f) );
            currentCenterY = (float)(-panOffset.Y + (height / 2f) );
            // Adjust panOffset to keep the current center
            panOffset.X = (float)-(currentCenterX * Zoom - width / 2f);
            panOffset.Y = (float)-(currentCenterY * Zoom - height / 2f);
            //  skControl.Invalidate(); // Invalidate the SKControl to trigger a repaint
        }

        // Event handler for panning (could be mouse move event with some button pressed)
        public void OnPan(SKPoint delta)
        {
            panOffset.X += delta.X;
            panOffset.Y += delta.Y;
           // skControl.Invalidate(); // Invalidate the SKControl to trigger a repaint
        }
        public void ZoomTo(double zoomLevel)
        {
            

            // Update the zoom level
            Zoom = zoomLevel;

            // skControl.Invalidate(); // Invalidate the SKControl to trigger a repaint
        }
        public void ZoomTo(double zoomLevel, SKPoint center)
        {
            // Calculate the factor by which the zoom level is changing
            double zoomFactor = zoomLevel / Zoom;

            // Update the zoom level
            Zoom = zoomLevel;

            // Adjust the pan offsets to keep the zoom center stationary
            panOffset.X = (float)((center.X + panOffset.X) * zoomFactor - center.X);
            panOffset.Y = (float)((center.Y + panOffset.Y) * zoomFactor - center.Y);


            // Update current center points
            currentCenterX = center.X;
            currentCenterY = center.Y;
            // skControl.Invalidate(); // Invalidate the SKControl to trigger a repaint
        }
        public void PanTo(HeatMapDataPoint referencePoint)
        {

            /// Assuming referencePoint.X and referencePoint.Y are normalized [0, 1]
            float refPointCanvasX = (float)(referencePoint.X * width);
            float refPointCanvasY = (float)(referencePoint.Y * height);

            // Canvas center
            float canvasCenterX = (float)(width / 2.0f);
            float canvasCenterY = (float)(height / 2.0f);

            // Calculate panOffset
            // You need to shift the canvas in the opposite direction of the reference point's position
            panOffset.X = canvasCenterX - refPointCanvasX;
            panOffset.Y = canvasCenterY - refPointCanvasY;
            currentCenterX = (float)(-panOffset.X + (width / 2f) / Zoom);
            currentCenterY = (float)(-panOffset.Y + (height / 2f) / Zoom);

        }
        public void ConvertUTMPointsToCanvasCoordinates(ref List<HeatMapDataPoint> utmPoints)
        {
            if (utmPoints == null || utmPoints.Count == 0)
                return;

            minX = utmPoints.Min(point => point.X);
            maxX = utmPoints.Max(point => point.X);
            minY = utmPoints.Min(point => point.Y);
            maxY = utmPoints.Max(point => point.Y);

            // Check for invalid ranges to prevent division by zero
            if (minX == maxX || minY == maxY)
                return;
                //throw new InvalidOperationException("Invalid range of UTM points. Cannot have all points with the same X or Y values.");

            int width = (int)this.width; // Assuming 'this.width' represents the canvas width
            int height = (int)this.height; // Assuming 'this.height' represents the canvas height
                                           // Value normalization
            double minValue = utmPoints.Min(point => point.Value);
            double maxValue = utmPoints.Max(point => point.Value);

            foreach (var point in utmPoints)
            {
                point.X = (point.X - minX) / (maxX - minX) * width;
                point.Y = (1 - (point.Y - minY) / (maxY - minY)) * height; // Invert Y-axis

                // Value normalization
                if (maxValue != minValue) // Check to prevent division by zero
                {
                    point.Value = (point.Value - minValue) / (maxValue - minValue);
                }
                else
                {
                    point.Value = 0; // Or some default normalized value
                }
            }
        }

        public double[,] ConvertListTo2DArrayUTMtoCanvas(List<HeatMapDataPoint> utmPoints)
        {
            int width = (int)this.width; // Assuming 'this.width' represents the canvas width
            int height = (int)this.height; // Assuming 'this.height' represents the canvas height

            minX = utmPoints.Min(point => point.X);
            maxX = utmPoints.Max(point => point.X);
            minY = utmPoints.Min(point => point.Y);
            maxY = utmPoints.Max(point => point.Y);

            double[,] canvasCoordinates = new double[utmPoints.Count, 2];

            for (int i = 0; i < utmPoints.Count; i++)
            {
                double x = (utmPoints[i].X - minX) / (maxX - minX) * width;
                double y = (1 - (utmPoints[i].Y - minY) / (maxY - minY)) * height; // Invert Y-axis
                canvasCoordinates[i, 0] = x;
                canvasCoordinates[i, 1] = y;
            }

            return canvasCoordinates;
        }
        private double ConvertUTMToCanvasX(double utmX, double minX, double maxX, double canvasWidth)
        {
            // Scale UTM X to fit within the canvas width
            double canvasX = (utmX - minX) / (maxX - minX) * canvasWidth;

            return canvasX;
        }

        private double ConvertUTMToCanvasY(double utmY, double minY, double maxY, double canvasHeight)
        {
            // Scale UTM Y to fit within the canvas height
            double canvasY = (utmY - minY) / (maxY - minY) * canvasHeight;

            // Invert Y-axis if needed (e.g., if canvas origin is top-left)
            // canvasY = canvasHeight - canvasY;

            return canvasY;
        }
      

    }
}
