using SharpDX;

namespace Soft3DEngine
{
    public class UnityMesh
    {
        public Vector3[] Vertices { get; private set; }
        public Face[] Faces { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public UnityMesh(int vertexCount, int faceCount)
        {
            Vertices = new Vector3[vertexCount];
            Faces = new Face[faceCount];
        }
    }
}
