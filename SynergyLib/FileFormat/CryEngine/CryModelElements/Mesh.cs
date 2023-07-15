namespace SynergyLib.FileFormat.CryEngine.CryModelElements;

public class Mesh {
    public string MaterialName;
    public Vertex[] Vertices;
    public ushort[] Indices;

    public Mesh(string materialName, Vertex[] vertices, ushort[] indices) {
        MaterialName = materialName;
        Vertices = vertices;
        Indices = indices;
    }

    public override string ToString() =>
        $"{nameof(Mesh)}: {MaterialName} (vert={Vertices.Length} ind={Indices.Length})";
}
