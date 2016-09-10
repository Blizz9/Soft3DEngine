namespace Soft3DEngine
{
    public class Mesh
    {
        public string Name { get; set; }
        public UnityVector3[] Vertices { get; private set; }
        public Face[] Faces { get; private set; }
        public UnityVector3 Position { get; set; }
        public UnityVector3 Rotation { get; set; }

        public Mesh(string name, int vertexCount, int faceCount)
        {
            Name = name;
            Vertices = new UnityVector3[vertexCount];
            Faces = new Face[faceCount];
        }
    }
}
