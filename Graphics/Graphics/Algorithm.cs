using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Graphics
{
    public class Algorithm
    {
        public static void Line(float x1, float y1, float x2, float y2, Color color, Action<int, int, Color> setPixel)
        {
            // Bresenham's line algorithm
            var steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);
            float temp;
            if (steep)
            {
                temp = x1;
                x1 = y1;
                y1 = temp;

                temp = x2;
                x2 = y2;
                y2 = temp;
            }

            if (x1 > x2)
            {
                temp = x1;
                x1 = x2;
                x2 = temp;

                temp = y1;
                y1 = y2;
                y2 = temp;
            }

            var dx = x2 - x1;
            var dy = Math.Abs(y2 - y1);

            var error = dx / 2.0f;
            var ystep = y1 < y2 ? 1 : -1;
            var y = (int)y1;

            var maxX = (int)x2;

            for (int x = (int)x1; x < maxX; x++)
            {
                if (steep)
                    setPixel(y, x, color);
                else
                    setPixel(x, y, color);

                error -= dy;

                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }

        public static bool IsInBoundingBox(List<Point> poly, int x, int y)
        {
            if (poly == null || poly.Count == 0)
                return false;

            var leftmost = poly.Min(p => p.X);
            var rightmost = poly.Max(p => p.X);
            var upmost = poly.Min(p => p.Y);
            var downmost = poly.Max(p => p.Y);
            return x >= leftmost && x <= rightmost && y >= upmost && y <= downmost;
        }

        public static bool IsInPolygon(List<Point> poly, float x, float y)
        {
            var locatedInPolygon = false;
            for (int i = 0; i < poly.Count; i++)
            {
                var j = (i + 1)%poly.Count;

                float vertex1X = poly[i].X;
                float vertex1Y = poly[i].Y;
                float vertex2X = poly[j].X;
                float vertex2Y = poly[j].Y;

                var testX = x;
                var testY = y;

                var belowLowY = vertex1Y > testY;
                var belowHighY = vertex2Y > testY;

                var withinYsEdges = belowLowY != belowHighY;

                if (withinYsEdges)
                {
                    // this is the slope of the line that connects vertices i and i+1 of the polygon
                    var slopeOfLine = (vertex2X - vertex1X) / (vertex2Y - vertex1Y);

                    // this looks up the x-coord of a point lying on the above line, given its y-coord
                    var pointOnLine = slopeOfLine * (testY - vertex1Y) + vertex1X;

                    //checks to see if x-coord of testPoint is smaller than the point on the line with the same y-coord
                    var isLeftToLine = testX < pointOnLine;

                    if (isLeftToLine)
                        locatedInPolygon = !locatedInPolygon;
                }
            }

            return locatedInPolygon;
        }
    }
}
