using SkiaSharp;
using System.Numerics;
using System.Reflection;

namespace Beep.WellSchematics
{
    public static class Helper
    {

        public static Dictionary<string, SkiaSharp.Extended.Svg.SKSvg> SvgResources { get; set; } = new Dictionary<string, SkiaSharp.Extended.Svg.SKSvg>();
        public static List<string> GetEmbeddedSvgResources()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Get all resource names
            string[] resourceNames = assembly.GetManifestResourceNames();

            // Filter for SVG files
            List<string> svgResources = new List<string>();
            foreach (var name in resourceNames)
            {
                if (name.EndsWith(".svg"))
                {
                    svgResources.Add(name);
                }
            }

            return svgResources;
        }
        public static double Bernstein(int n, int i, double t)
        {
            return Combination(n, i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
        }
        public static int Combination(int n, int k)
        {
            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }
        public static int Factorial(int n)
        {
            int result = 1;
            for (int i = 1; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }
        public static void Bezier2D(double[] b, int cpts, double[] p)
        {
            int npts = (b.Length) / 2;
            int icount, jcount;
            double step, t;

            // Calculate points on curve

            icount = 0;
            t = 0;
            step = (double)1.0 / (cpts - 1);

            for (int i1 = 0; i1 != cpts; i1++)
            {
                if ((1.0 - t) < 5e-6)
                    t = 1.0;

                jcount = 0;
                p[icount] = 0.0;
                p[icount + 1] = 0.0;
                for (int i = 0; i != npts; i++)
                {
                    double basis = Bernstein(npts - 1, i, t);
                    p[icount] += basis * b[jcount];
                    p[icount + 1] += basis * b[jcount + 1];
                    jcount = jcount + 2;
                }

                icount += 2;
                t += step;
            }
        }
        public static List<SKPoint> CalculateBezierCurve(List<SKPoint> controlPoints)
        {
            List<SKPoint> bezierPoints = new List<SKPoint>();

            for (double t = 0.0; t <= 1.0; t += 0.01)
            {
                SKPoint point = new SKPoint();
                for (int i = 0; i < controlPoints.Count; i++)
                {
                    double bernstein = Bernstein(controlPoints.Count - 1, i, t);
                    point.X += (float)(controlPoints[i].X * bernstein);
                    point.Y += (float)(controlPoints[i].Y * bernstein);
                }
                bezierPoints.Add(point);
            }

            return bezierPoints;
        }
        public static SKPoint Calculate2DProjection(DeviationSurveyPoint point)
        {
            // Converting polar coordinates (depth, deviation, azimuth) to Cartesian coordinates (x, y)

            // Calculate the horizontal displacement ('x' and 'y' in 2D space) based on azimuth and deviation angle
            double azimuthRadians = DegreesToRadians(point.AZIMUTH);
            double inclinationRadians = DegreesToRadians(point.DEVIATION_ANGLE);

            // Assuming 'MD' is the measured depth along the path of the wellbore
            // The horizontal displacement (x, y) can be calculated using trigonometric functions
            double x = point.MD * Math.Sin(inclinationRadians) * Math.Cos(azimuthRadians);
            double y = point.MD * Math.Sin(inclinationRadians) * Math.Sin(azimuthRadians);

            return new SKPoint((float)x, (float)y);
        }
        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
        public static SkiaSharp.Extended.Svg.SKSvg LoadSvg(string componentName, bool isFromFile)
{
    SkiaSharp.Extended.Svg.SKSvg svg = new SkiaSharp.Extended.Svg.SKSvg();

    try
    {
        string svgFileName = null;

        // Use DefaultMapGenerator to get the correct SVG file name
        var defaultMaps = DefaultMapGenerator.GenerateDefaultMaps();
        var map = defaultMaps.FirstOrDefault(dm => dm.ComponentSpecification.ComponentNames.Contains(componentName.ToLower()));
        if (map != null)
        {
            svgFileName = map.SymbolFile;
        }

        if (isFromFile)
        {
            if (!string.IsNullOrEmpty(svgFileName))
            {
                using (var stream = File.OpenRead(svgFileName))
                {
                    svg.Load(stream);
                }
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(svgFileName))
            {
                string name = svgFileName.ToLower();
                if (SvgResources.ContainsKey(name))
                {
                    svg = SvgResources[name];
                }
                else
                {
                    svg = null;
                }
            }
            // If svgFileName is null, handle the case (e.g., set svg to a default value or throw an exception)
        }
    }
    catch (Exception ex)
    {
        // Handle or log the exception as necessary
    }

    return svg;
}
        public static float SetDepthScaling(float canvasHeight, float minDepth, float maxDepth)
        {
            float depthRange = maxDepth - minDepth;
            return  canvasHeight / depthRange;
        }
        public static SKPoint GetPointOnCurve(float t, List<SKPoint> pathPoints)
        {
            var pathLengths = CalculatePathLengths(pathPoints);
            float totalLength = pathLengths.Last();
            float desiredLength = t * totalLength;

            for (int i = 1; i < pathLengths.Count; i++)
            {
                if (pathLengths[i] >= desiredLength)
                {
                    float segmentLength = pathLengths[i] - pathLengths[i - 1];
                    float remainder = desiredLength - pathLengths[i - 1];
                    float segmentT = remainder / segmentLength;

                    // Manually calculate linear interpolation
                    float x = pathPoints[i - 1].X + segmentT * (pathPoints[i].X - pathPoints[i - 1].X);
                    float y = pathPoints[i - 1].Y + segmentT * (pathPoints[i].Y - pathPoints[i - 1].Y);

                    return new SKPoint(x, y);
                }
            }

            return pathPoints.Last();
        }
        public static SKPoint GetPointOnCurve(List<(float x, float y)> wellborePoints, float depth)
        {
            float totalDepth = wellborePoints.Last().Item2 - wellborePoints.First().Item2;

            // Lerp parameter t in range [0, 1]
            float t = (depth - wellborePoints.First().Item2) / totalDepth;

            // Index of the point to the left (floor)
            int idxLeft = (int)Math.Floor(t * (wellborePoints.Count - 1));
            // Index of the point to the right (ceil)
            int idxRight = (int)Math.Ceiling(t * (wellborePoints.Count - 1));

            // If indices are out of range, clamp them
            idxLeft = Math.Max(0, Math.Min(idxLeft, wellborePoints.Count - 1));
            idxRight = Math.Max(0, Math.Min(idxRight, wellborePoints.Count - 1));

            // Interpolation parameter between the left and right points
            float s = (t - (float)idxLeft / (wellborePoints.Count - 1)) * (wellborePoints.Count - 1);

            // Interpolate between the two points
            var pointLeft = new SKPoint(wellborePoints[idxLeft].Item1, wellborePoints[idxLeft].Item2);
            var pointRight = new SKPoint(wellborePoints[idxRight].Item1, wellborePoints[idxRight].Item2);

            float interpolatedX = pointLeft.X + s * (pointRight.X - pointLeft.X);
            float interpolatedY = pointLeft.Y + s * (pointRight.Y - pointLeft.Y);
            var pointOnCurve = new SKPoint(interpolatedX, interpolatedY);

            return pointOnCurve;
        }
        public static SKPoint GetPointOnCurve(List<SKPoint> points, float depth)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                // If the depth lies between the depths of two points
                if (points[i].Y <= depth && points[i + 1].Y > depth)
                {
                    float s = (depth - points[i].Y) / (points[i + 1].Y - points[i].Y);
                    SKPoint pointOnCurve = new SKPoint(points[i].X + s * (points[i + 1].X - points[i].X), points[i].Y + s * (points[i + 1].Y - points[i].Y));

                    return pointOnCurve;
                }
            }

            // If depth is not found within the points (should not happen if depths are sorted), return the first point
            return points[0];
        }
        public static SKPoint GetPointOnCurve(SKPath path, float depth)
        {
            var points = GetPointsFromPath(path);
            return GetPointOnCurve(points, depth);
        }
        public static List<SKPoint> GetPointsFromPath(SKPath path)
        {
            var pathMeasure = new SKPathMeasure(path, false);
            float pathLength = pathMeasure.Length;
            int numPoints = (int)pathLength; // This is an approximation, adjust as needed

            List<SKPoint> points = new List<SKPoint>(numPoints);
            for (int i = 0; i < numPoints; ++i)
            {
                float distance = i * pathLength / numPoints;
                SKPoint point;
                SKPoint tangent;
                pathMeasure.GetPositionAndTangent(distance, out point, out tangent);
                points.Add(point);
            }

            return points;
        }
        public static Dictionary<string, SkiaSharp.Extended.Svg.SKSvg> LoadSvgResources(IEnumerable<string> resourceNames)
        {
            SvgResources = new Dictionary<string, SkiaSharp.Extended.Svg.SKSvg>();

            foreach (string resourceName in resourceNames)
            {
                var assembly = Assembly.GetExecutingAssembly();

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var svg = new SkiaSharp.Extended.Svg.SKSvg();
                        svg.Load(stream);
                        int lastDotIndex = resourceName.LastIndexOf('.');
                        string fileName = "";
                        if (lastDotIndex >= 0)
                        {
                            int fileNameStartIndex = resourceName.LastIndexOf('.', lastDotIndex - 1) + 1;
                            if (fileNameStartIndex >= 0)
                            {
                                fileName = resourceName.Substring(fileNameStartIndex); // This will give you "pic.svg"
                            }
                        }
                        SvgResources[fileName] = svg;
                    }
                }
            }

            return SvgResources;
        }
        public static List<float> CalculatePathLengths(List<SKPoint> pathPoints)
        {
            var lengths = new List<float>();
            float totalLength = 0;
            lengths.Add(totalLength);

            for (int i = 1; i < pathPoints.Count; i++)
            {
                totalLength += Vector2.Distance(new Vector2(pathPoints[i - 1].X, pathPoints[i - 1].Y), new Vector2(pathPoints[i].X, pathPoints[i].Y));
                lengths.Add(totalLength);
            }

            return lengths;
        }
    
        private static SKPoint Normalize(SKPoint point)
        {
            float length = (float)Math.Sqrt(point.X * point.X + point.Y * point.Y);
            return new SKPoint(point.X / length, point.Y / length);
        }

    }
}
