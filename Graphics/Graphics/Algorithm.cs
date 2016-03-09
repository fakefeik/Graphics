using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SharpDX;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

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

        public static bool IsInBoundingBox(List<PointF> poly, float x, float y)
        {
            if (poly == null || poly.Count == 0)
                return false;

            var leftmost = poly.Min(p => p.X);
            var rightmost = poly.Max(p => p.X);
            var upmost = poly.Min(p => p.Y);
            var downmost = poly.Max(p => p.Y);
            return x >= leftmost && x <= rightmost && y >= upmost && y <= downmost;
        }

        public static bool IsInPolygon(List<PointF> poly, float x, float y)
        {
            var locatedInPolygon = false;
            for (int i = 0; i < poly.Count; i++)
            {
                var j = (i + 1)%poly.Count;

                var vertex1X = poly[i].X;
                var vertex1Y = poly[i].Y;
                var vertex2X = poly[j].X;
                var vertex2Y = poly[j].Y;

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

        public static void GetHorizons(Vector3 cameraPosition, Vector2 windowSize, int windowStart, int screenStep, int xMax,
            int xMin, int zMax, int zMin, int zStep, Func<double, double, double> function)
        {
            var cameraDirection = cameraPosition/Vector3.Normalize(cameraPosition);
            var rightVector = Vector3.Cross(new Vector3(cameraDirection.X, 0, cameraDirection.Z), new Vector3(0, 1, 0));
            rightVector /= Vector3.Normalize(rightVector);
            var cameraHPlaneNormal = Vector3.Cross(rightVector, cameraDirection);
            cameraHPlaneNormal /= Vector3.Normalize(cameraHPlaneNormal);
            IEnumerable<float> horizonDistances = Range(zMin, zMax, zStep);
            if (cameraPosition[2] > (zMin + zMax)/2.0)
                horizonDistances = horizonDistances.Reverse();
            var screenXPositions = Range(-windowSize[0]/2, windowSize[0]/2, screenStep);
            var lowerBuffer = 0;
        }

        private static float[] Range(float start, float end, float step)
        {
            var values = new List<float>();
            for (var i = start; i < end; i += step)
                values.Add(i);
            return values.ToArray();
        }

    //lower_buffer = np.array(screen_x_positions)
    //higher_buffer = np.array(screen_x_positions)
    //lower_buffer[:] = np.NAN
    //higher_buffer[:] = np.NAN
    //for horizon_distance in horizon_distances:
    //    horizon = np.array(screen_x_positions)
    //    horizon[:] = np.NAN
    //    for i in range(screen_x_positions.shape[0]):
    //        screen_x = screen_x_positions[i]
    //        ray_start = np.array([camera_position[0], 0, camera_position[2]]) + screen_x* right_vector
    //        x_intersection = ray_start[0] - (ray_start[2] - horizon_distance)*camera_direction[0]/camera_direction[2]
    //        if x_intersection > x_max or x_intersection<x_min:
    //            continue
    //        function_value = function(x_intersection, horizon_distance)
    //        camera_h_plane_intersection_y = \
    //                -((x_intersection - camera_position[0])*camera_h_plane_normal[0] + \
    //                (horizon_distance - camera_position[2])*camera_h_plane_normal[2])/\
    //                camera_h_plane_normal[1] + camera_position[1]
    //        screen_y = np.array([x_intersection, function_value - camera_h_plane_intersection_y, horizon_distance]).dot(
    //                camera_h_plane_normal)
    //        if np.isnan(higher_buffer[i]) or screen_y > higher_buffer[i]:
    //            higher_buffer[i] = screen_y
    //            horizon[i] = screen_y
    //        if np.isnan(lower_buffer[i]) or screen_y<lower_buffer[i]:
    //            lower_buffer[i] = screen_y
    //            horizon[i] = screen_y
    //    #with printoptions(precision=3, suppress=True):
    //    #    print(horizon)
    //    #with printoptions(precision=3, suppress=True):
    //    #    print(screen_x_positions)
    //    horizon = window_start[1] + window_size[1] / 2 - horizon
    //    screen_x_positions_shifted = screen_x_positions + window_start[0] + window_size[0] / 2
    //    #print()
    //    segments = []
    //    for i in range(1, screen_x_positions.shape[0]):
    //        if not np.isnan(horizon[i]) and not np.isnan(horizon[i - 1]):
    //            segments.append(
    //                    (screen_x_positions_shifted[i - 1], horizon[i - 1], screen_x_positions_shifted[i], horizon[i]))
    //    yield segments

    }
}
