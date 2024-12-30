using System.Diagnostics.Tracing;
using System.Security;
using OpenTK.Mathematics;

namespace CSharp3D;

public struct Block
{
    public BlockType Type;
    public int LightLevel;
    
    public static Block LitAir = new Block { Type = BlockType.Air, LightLevel = 15 };
    public static Block UnlitAir = new Block { Type = BlockType.Air, LightLevel = 0 };
}

public class Chunk : IDisposable
{
    public Vector3 Position { get; }
    
    public static readonly Vector3i Dimensions = new (16, 384, 16);

    public Block[,,] Blocks { get; set; } = new Block[Dimensions.X, Dimensions.Y, Dimensions.Z];

    public Mesh? Mesh { get; private set; }

    public bool IsLoaded { get; private set; } = false;
    
    public AABB BoundingBox { get; }
    
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

    private World _world;

    public Chunk? Front { get; set; }
    
    public Chunk? Back { get; set; }
    
    public Chunk? Left { get; set; }
    
    public Chunk? Right { get; set; }
    
    public bool HasNeighbours => !(Front is null || Back is null || Left is null || Right is null);

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
        var block = GetBlock(x, y, z);

        if (block.HasValue)
        {
            return block.Value.Type != BlockType.Air;
        }

        return false;
    }
    
    public Block? GetBlock(int x, int y, int z)
    {
        if (y < 0 || y > Dimensions.Y - 1) return null;
        
        if (x < 0)
        {
            if (Left is null) return null;
            return Left.GetBlock(x + Dimensions.X, y, z);
        }

        if (x >= Dimensions.X)
        {
            if (Right is null) return null;
            return Right.GetBlock(x - Dimensions.X, y, z);
        }

        if (z < 0)
        {
            if (Back is null) return null;
            return Back.GetBlock(x, y, z + Dimensions.Z);
        }

        if (z >= Dimensions.Z)
        {
            if (Front is null) return null;
            return Front.GetBlock(x, y, z - Dimensions.Z);
        }
        
        var block = Blocks[x, y, z];

        return block;
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