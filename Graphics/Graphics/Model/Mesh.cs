using System.Threading.Tasks;
using SharpDX;

namespace Graphics.Model
{
    public class Mesh
    {
        public string Name { get; set; }
        public Vertex[] Vertices { get; protected set; }
        public Face[] Faces { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Texture Texture { get; set; }

        public Mesh()
        {
        }

        public Mesh(string name, int verticesCount, int facesCount)
        {
            Vertices = new Vertex[verticesCount];
            Faces = new Face[facesCount];
            Name = name;
        }

        public void ComputeFacesNormals()
        {
            Parallel.For(0, Faces.Length, faceIndex =>
            {
                var face = Faces[faceIndex];
                var vertexA = Vertices[face.A];
                var vertexB = Vertices[face.B];
                var vertexC = Vertices[face.C];

                Faces[faceIndex].Normal = (vertexA.Normal + vertexB.Normal + vertexC.Normal) / 3.0f;
                Faces[faceIndex].Normal.Normalize();
            });

        }
    }
}
