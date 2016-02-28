using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Graphics.Model;
using SharpDX;
using Matrix = SharpDX.Matrix;
using PixelFormat = System.Windows.Media.PixelFormat;
using Point = System.Drawing.Point;

namespace Graphics.ViewModel
{
    class RendererViewModel : BaseViewModel
    {
        public ICommand ChangeResolutionCommand { get; private set; }

        private PixelFormat pf = PixelFormats.Rgb24;
        private int rawStride;
        private byte[] backBuffer;
        private float[] depthBuffer;
        private object[] lockBuffer;
        private WriteableBitmap bmp;
        private int width;
        private int height;
        public bool Wireframe { get; set; }
        public string ImageName { get; set; }
        public string TaskName { get; set; }
        private BitmapSource _imageSource;

        public BitmapSource ImageSource
        {
            get { return _imageSource; }
            private set
            {
                _imageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }

        public RendererViewModel(string s)
        {
            TaskName = s;
            ImageName = $"../Images/{s}.png";
            ChangeResolutionCommand = new RelayCommand(o =>
            {
                if ((string) o == "-1")
                {
                    if (width >= 256 && height >= 256)
                    {
                        InitializeBitmapResolution(width/2, height/2);
                    }
                }
                else
                {
                    if (width < 1024 && height < 1024)
                    {
                        InitializeBitmapResolution(width*2, height*2);
                    }
                }
            });

            InitializeBitmapResolution(512, 512);
        }

        private void InitializeBitmapResolution(int width, int height)
        {
            this.width = width;
            this.height = height;
            rawStride = (width*pf.BitsPerPixel + 7)/8;
            backBuffer = new byte[rawStride*height];
            depthBuffer = new float[width*height];
            lockBuffer = new object[width*height];
            for (var i = 0; i < lockBuffer.Length; i++)
            {
                lockBuffer[i] = new object();
            }
        }

        private void SetPixel(int x, int y, Color4 color)
        {

            var xIndex = x*3;
            var yIndex = y*rawStride;
            backBuffer[xIndex + yIndex] = (byte) (color.Red*255);
            backBuffer[xIndex + yIndex + 1] = (byte) (color.Green*255);
            backBuffer[xIndex + yIndex + 2] = (byte) (color.Blue*255);

        }

        public void PutPixel(int x, int y, float z, Color4 color)
        {
            var index = x + y*width;

            lock (lockBuffer[index])
            {
                if (depthBuffer[index] < z)
                    return; // Discard

                depthBuffer[index] = z;
                SetPixel(x, y, color);
            }
        }

        public void Clear(byte r, byte g, byte b, byte a)
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    SetPixel(i, j, new Color4(r/255.0f, g/255.0f, b/255.0f, a/255.0f));
            for (var index = 0; index < depthBuffer.Length; index++)
                depthBuffer[index] = float.MaxValue;
        }

        public void Present()
        {
            ImageSource = BitmapSource.Create(width, height,
                96, 96, pf, null, backBuffer, rawStride);
        }

        public void DrawPoint(Vector3 point, Color4 color)
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < width && point.Y < height)
                PutPixel((int) point.X, (int) point.Y, point.Z, color);
        }

        private float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        private float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min)*Clamp(gradient);
        }

        public Vertex Project(Vertex vertex, Matrix transMat, Matrix world)
        {
            var point2D = Vector3.TransformCoordinate(vertex.Coordinates, transMat);
            var point3DWorld = Vector3.TransformCoordinate(vertex.Coordinates, world);
            var normal3DWorld = Vector3.TransformCoordinate(vertex.Normal, world);

            var x = point2D.X*width + width/2.0f;
            var y = -point2D.Y*height + height/2.0f;

            return new Vertex
            {
                Coordinates = new Vector3(x, y, point2D.Z),
                Normal = normal3DWorld,
                WorldCoordinates = point3DWorld,
                TextureCoordinates = vertex.TextureCoordinates
            };
        }

        private float ComputeNDotL(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            var lightDirection = lightPosition - vertex;

            normal.Normalize();
            lightDirection.Normalize();

            return Math.Max(0, Vector3.Dot(normal, lightDirection));
        }

        private void ProcessScanLine(ScanLineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd, Color4 color,
            Texture texture)
        {
            var pa = va.Coordinates;
            var pb = vb.Coordinates;
            var pc = vc.Coordinates;
            var pd = vd.Coordinates;

            var gradient1 = pa.Y != pb.Y ? (data.currentY - pa.Y)/(pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (data.currentY - pc.Y)/(pd.Y - pc.Y) : 1;

            var sx = (int) Interpolate(pa.X, pb.X, gradient1);
            var ex = (int) Interpolate(pc.X, pd.X, gradient2);

            var z1 = Interpolate(pa.Z, pb.Z, gradient1);
            var z2 = Interpolate(pc.Z, pd.Z, gradient2);

            var snl = Interpolate(data.ndotla, data.ndotlb, gradient1);
            var enl = Interpolate(data.ndotlc, data.ndotld, gradient2);

            var su = Interpolate(data.ua, data.ub, gradient1);
            var eu = Interpolate(data.uc, data.ud, gradient2);
            var sv = Interpolate(data.va, data.vb, gradient1);
            var ev = Interpolate(data.vc, data.vd, gradient2);

            for (var x = sx; x < ex; x++)
            {
                var gradient = (x - sx)/(float) (ex - sx);

                var z = Interpolate(z1, z2, gradient);
                var ndotl = Interpolate(snl, enl, gradient);
                var u = Interpolate(su, eu, gradient);
                var v = Interpolate(sv, ev, gradient);

                var textureColor = texture?.Map(u, v) ?? new Color4(1, 1, 1, 1);
                DrawPoint(new Vector3(x, data.currentY, z), color*ndotl*textureColor);
            }
        }

        public void DrawBline(Vector3 point0, Vector3 point1, Color4 color)
        {
            int x0 = (int) point0.X;
            int y0 = (int) point0.Y;
            int x1 = (int) point1.X;
            int y1 = (int) point1.Y;

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = (x0 < x1) ? 1 : -1;
            var sy = (y0 < y1) ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                DrawPoint(new Vector3(x0, y0, point0.Z), color);

                if ((x0 == x1) && (y0 == y1)) break;
                var e2 = 2*err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public void DrawTriangle(Vertex v1, Vertex v2, Vertex v3, Color4 color, Texture texture)
        {
            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            if (v2.Coordinates.Y > v3.Coordinates.Y)
            {
                var temp = v2;
                v2 = v3;
                v3 = temp;
            }

            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            var p1 = v1.Coordinates;
            var p2 = v2.Coordinates;
            var p3 = v3.Coordinates;

            var lightPos = new Vector3(0, 10, 10);

            var nl1 = ComputeNDotL(v1.WorldCoordinates, v1.Normal, lightPos);
            var nl2 = ComputeNDotL(v2.WorldCoordinates, v2.Normal, lightPos);
            var nl3 = ComputeNDotL(v3.WorldCoordinates, v3.Normal, lightPos);

            var data = new ScanLineData();

            float dP1P2, dP1P3;
            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X)/(p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X)/(p3.Y - p1.Y);
            else
                dP1P3 = 0;
            if (dP1P2 > dP1P3)
            {
                for (var y = (int) p1.Y; y <= (int) p3.Y; y++)
                {
                    data.currentY = y;

                    if (y < p2.Y)
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl3;
                        data.ndotlc = nl1;
                        data.ndotld = nl2;

                        data.ua = v1.TextureCoordinates.X;
                        data.ub = v3.TextureCoordinates.X;
                        data.uc = v1.TextureCoordinates.X;
                        data.ud = v2.TextureCoordinates.X;

                        data.va = v1.TextureCoordinates.Y;
                        data.vb = v3.TextureCoordinates.Y;
                        data.vc = v1.TextureCoordinates.Y;
                        data.vd = v2.TextureCoordinates.Y;

                        ProcessScanLine(data, v1, v3, v1, v2, color, texture);
                    }
                    else
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl3;
                        data.ndotlc = nl2;
                        data.ndotld = nl3;

                        data.ua = v1.TextureCoordinates.X;
                        data.ub = v3.TextureCoordinates.X;
                        data.uc = v2.TextureCoordinates.X;
                        data.ud = v3.TextureCoordinates.X;

                        data.va = v1.TextureCoordinates.Y;
                        data.vb = v3.TextureCoordinates.Y;
                        data.vc = v2.TextureCoordinates.Y;
                        data.vd = v3.TextureCoordinates.Y;

                        ProcessScanLine(data, v1, v3, v2, v3, color, texture);
                    }
                }
            }
            else
            {
                for (var y = (int) p1.Y; y <= (int) p3.Y; y++)
                {
                    data.currentY = y;

                    if (y < p2.Y)
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl2;
                        data.ndotlc = nl1;
                        data.ndotld = nl3;

                        data.ua = v1.TextureCoordinates.X;
                        data.ub = v2.TextureCoordinates.X;
                        data.uc = v1.TextureCoordinates.X;
                        data.ud = v3.TextureCoordinates.X;

                        data.va = v1.TextureCoordinates.Y;
                        data.vb = v2.TextureCoordinates.Y;
                        data.vc = v1.TextureCoordinates.Y;
                        data.vd = v3.TextureCoordinates.Y;

                        ProcessScanLine(data, v1, v2, v1, v3, color, texture);
                    }
                    else
                    {
                        data.ndotla = nl2;
                        data.ndotlb = nl3;
                        data.ndotlc = nl1;
                        data.ndotld = nl3;

                        data.ua = v2.TextureCoordinates.X;
                        data.ub = v3.TextureCoordinates.X;
                        data.uc = v1.TextureCoordinates.X;
                        data.ud = v3.TextureCoordinates.X;

                        data.va = v2.TextureCoordinates.Y;
                        data.vb = v3.TextureCoordinates.Y;
                        data.vc = v1.TextureCoordinates.Y;
                        data.vd = v3.TextureCoordinates.Y;

                        ProcessScanLine(data, v2, v3, v1, v3, color, texture);
                    }
                }
            }
        }

        public void Render(Camera camera, params Mesh[] meshes)
        {
            var viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, camera.Up);
            var projectionMatrix = Matrix.PerspectiveFovLH(0.78f, (float) width/height, 0.01f, 1.0f);

            foreach (var mesh in meshes)
            {
                var worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z)*
                                  Matrix.Translation(mesh.Position);

                var worldView = worldMatrix*viewMatrix;
                var transformMatrix = worldView*projectionMatrix;

                Parallel.For(0, mesh.Faces.Length, faceIndex =>
                {
                    var face = mesh.Faces[faceIndex];

                    // Face-back culling
                    //var transformedNormal = Vector3.TransformNormal(face.Normal, worldView);

                    //if (transformedNormal.Z >= 0)
                    //{
                    //    return;
                    //}

                    // Render this face
                    var vertexA = mesh.Vertices[face.A];
                    var vertexB = mesh.Vertices[face.B];
                    var vertexC = mesh.Vertices[face.C];

                    var pixelA = Project(vertexA, transformMatrix, worldMatrix);
                    var pixelB = Project(vertexB, transformMatrix, worldMatrix);
                    var pixelC = Project(vertexC, transformMatrix, worldMatrix);

                    //var color = 0.25f + (faceIndex % mesh.Faces.Length) * 0.75f / mesh.Faces.Length;
                    var color = 1.0f;


                    if (Wireframe)
                    {
                        DrawBline(pixelA.Coordinates, pixelB.Coordinates, new Color4(color, color, color, 1));
                        DrawBline(pixelB.Coordinates, pixelC.Coordinates, new Color4(color, color, color, 1));
                        DrawBline(pixelC.Coordinates, pixelA.Coordinates, new Color4(color, color, color, 1));
                    }
                    else
                    {
                        DrawTriangle(pixelA, pixelB, pixelC, new Color4(color, color, color, 1), mesh.Texture);
                    }
                });
            }
        }
    }
}
