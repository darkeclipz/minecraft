using System.Diagnostics.Tracing;
using System.Security;
using OpenTK.Mathematics;

namespace CSharp3D;

public struct Block
{
    public BlockType Type { get; set; }
    public int LightLevel { get; set; }
    
    public static Block LitAir = new Block { Type = BlockType.Air, LightLevel = 15 };
    public static Block UnlitAir = new Block { Type = BlockType.Air, LightLevel = 0 };
}

public class Chunk : IDisposable
{
    public Vector3 Position { get; }
    
    public static Vector3i Dimensions => new (16, 384, 16);

    public Block[,,] Blocks { get; set; } = new Block[Dimensions.X, Dimensions.Y, Dimensions.Z];

    public Mesh Mesh { get; private set; } = null!;

    public bool IsLoaded { get; private set; } = false;
    
    public AABB BoundingBox { get; }
    
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.Now;

    private World _world;

    public Chunk(Vector3 position, World world)
    {
        Position = position;
        _world = world;
        
        BoundingBox = new AABB
        {
            Position = Position,
            Size = Dimensions,
        };
    }
    
    private Chunk() { }

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

        return block.Type != BlockType.Air;
    }

    public int HeightAt(int x, int z)
    {
        if (x < 0 || x > Dimensions.X - 1) return 0;
        if (z < 0 || z > Dimensions.Z - 1) return 0;
        
        for (var y = Dimensions.Y - 1; y >= 0; y--)
        {
            if (Blocks[x, y, z].Type != BlockType.Air) return y;
        }

        return 0;
    }

    public Vector3 Midpoint => new Vector3(Position.X + Dimensions.X / 2, Position.Y + Dimensions.Y / 2, Position.Z - Dimensions.Z / 2);

    private bool disposed = false;
    
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        Mesh?.Dispose();
    }

    public float GetOpacity()
    {
        var timeAlive = DateTimeOffset.UtcNow.Subtract(CreatedAt);
        var millisecondsAlive = timeAlive.TotalMilliseconds;
        var fadeInTimeInMilliseconds = 300;
        return (float)Math.Clamp(millisecondsAlive / fadeInTimeInMilliseconds, 0.0, 1.0);
    }

    public BlockRef GetBlockRef(Vector3 position) => GetBlockRef((int)position.X, (int)position.Y, (int)position.Z);
    public BlockRef GetBlockRef(int x, int y, int z)
    {
        if (x < 0 || x > Dimensions.X - 1) throw new ArgumentOutOfRangeException(nameof(x));
        if (y < 0 || y > Dimensions.Y - 1) throw new ArgumentOutOfRangeException(nameof(y));
        if (z < 0 || z > Dimensions.Z - 1) throw new ArgumentOutOfRangeException(nameof(z));
        return BlockRef.From(this, x, y, z);
    }

    public bool IsLocalPointInChunk(Vector3i position) => IsLocalPointInChunk(position.X, position.Y, position.Z);
    public bool IsLocalPointInChunk(int x, int y, int z)
    {
        if (x < 0 || x > Dimensions.X - 1) return false;
        if (y < 0 || y > Dimensions.Y - 1) return false;
        if (z < 0 || z > Dimensions.Z - 1) return false;
        return true;
    }

    public bool IsWorldPointInChunk(Vector3i position)
    {
        return BoundingBox.IntersectsWith(position);
    }
}