using SharpDX;

namespace Soft3DEngine
{
    public class UnityMesh
    {
        public string Name { get; set; }
        public Vector3[] Vertices { get; private set; }
        public Face[] Faces { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public UnityMesh(string name, int vertexCount, int faceCount)
        {
            Name = name;
            Vertices = new Vector3[vertexCount];
            Faces = new Face[faceCount];
        }
    }
}
