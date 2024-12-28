using System.Diagnostics.Tracing;
using OpenTK.Mathematics;

namespace CSharp3D;

public class Chunk
{
    public Vector3 Position { get; set; }
    
    public static Vector3i Dimensions = new (16, 384, 16);

    public BlockType[,,] Blocks { get; set; }

    public Mesh Mesh;

    public Chunk()
    {
        Blocks = new BlockType[Dimensions.X, Dimensions.Y, Dimensions.Z];
    }

    public void UpdateMesh(World world)
    {
        Mesh = Mesh.From(this, world);
    }
}