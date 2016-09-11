namespace Soft3DEngine
{
    public class Mesh
    {
        public string Name { get; set; }
        public Vector3[] Vertices { get; private set; }
        public Face[] Faces { get; private set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Mesh(string name, int vertexCount, int faceCount)
        {
            Name = name;
            Vertices = new Vector3[vertexCount];
            Faces = new Face[faceCount];
        }
    }
}
