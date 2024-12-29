using System.Diagnostics.Tracing;
using OpenTK.Mathematics;

namespace CSharp3D;

public class Chunk
{
    public Vector3 Position { get; init; }
    
    public static Vector3i Dimensions => new (16, 384, 16);

    public BlockType[,,] Blocks { get; set; } = new BlockType[Dimensions.X, Dimensions.Y, Dimensions.Z];

    public Mesh? Mesh { get; private set; } = new();

    public bool IsLoaded { get; private set; } = false;

    public void UpdateMesh(World world)
    {
        Mesh = Mesh.From(this, world);
        IsLoaded = true;
    }
    
    public bool IsSolid(int x, int y, int z)
    {
        if (x < 0 || x > Dimensions.X - 1) return false;
        if (y < 0 || y > Dimensions.Y - 1) return false;
        if (z < 0 || z > Dimensions.Z - 1) return false;
        
        var block = Blocks[x, y, z];

        return block != BlockType.Air;
    }
}