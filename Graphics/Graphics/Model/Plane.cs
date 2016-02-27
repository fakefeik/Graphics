using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SharpDX;

namespace Graphics.Model
{
    public class Plane : Mesh
    {
        public Plane(float width, float height, int widthSegments, int heightSegments, Func<float, float, float> f)
        {
            var vertices = new List<Vertex>();
            var faces = new List<Face>();

            var xOffset = width/-2;
            var yOffset = height/-2;
            var xWidth = width/widthSegments;
            var yHeight = height/heightSegments;
            var w = widthSegments + 1;

            for (var y = 0; y < heightSegments + 1; y++)
                for (var x = 0; x < widthSegments + 1; x++)
                {
                    var vX = xOffset + x * xWidth;
                    var vY = yOffset + y * yHeight;

                    var off = new[] {1.0f, 1.0f, 0.0f};
                    var hL = TryGetResult(f, vX - off[0], vY - off[2]);
                    var hR = TryGetResult(f, vX + off[0], vY + off[2]);
                    var hD = TryGetResult(f, vX - off[2], vY - off[1]);
                    var hU = TryGetResult(f, vX + off[2], vY + off[1]);

                    var N = new float[] {0, 0, 2};
                    N[0] = hL - hR;
                    N[1] = hD - hU;
                    N[2] = -2.0f;

                    N = Vector3.Normalize(new Vector3(N)).ToArray();

                    vertices.Add(new Vertex
                    {
                        Coordinates = new Vector3(vX, vY, TryGetResult(f, vX, vY)),
                        Normal = new Vector3(N[0], N[1], N[2]),
                        TextureCoordinates = new Vector2(x / widthSegments, 1 - y / heightSegments)
                    });

                    var n = y * (widthSegments + 1) + x;
                    if (y < heightSegments && x < widthSegments)
                    {
                        faces.Add(new Face
                        {
                            A = n,
                            B = n + 1,
                            C = n + w
                        });
                        faces.Add(new Face
                        {
                            A = n + 1,
                            B = n + 1 + w,
                            C = n + 1 + w - 1 
                        
                        });
                    }
                }
            
            Vertices = vertices.ToArray();
            Faces = faces.ToArray();
            ComputeFacesNormals();
            Texture = new Texture("", 0, 0);
        }

        private float TryGetResult(Func<float, float, float> f, float x, float y)
        {
            try
            {
                return f(x, y);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
