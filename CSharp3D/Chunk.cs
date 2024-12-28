using System.Diagnostics.Tracing;
using OpenTK.Mathematics;

namespace CSharp3D;

public static class TerrainGenerator
{
    public static void GenerateChunk(Chunk chunk)
    {
        for (int y = 0; y < Chunk.Dimensions.Y; y++)
            for (int x = 0; x < Chunk.Dimensions.X; x++)
                for (int z = 0; z < Chunk.Dimensions.Z; z++)
                {
                    /*
                     * --- 0
                     *  | 
                     *  |
                     *  | ...
                     *  |
                     *  |
                     * --- Chunk.Dimensions.Y
                     */
                    
                    if (y >= Chunk.Dimensions.Y - 60 && y <= Chunk.Dimensions.Y - 57)
                    {
                        chunk.Blocks[x, y, z] = BlockType.Dirt;
                    }

                    if (y > Chunk.Dimensions.Y - 57)
                    {
                        chunk.Blocks[x, y, z] = BlockType.Stone;
                    }
                }
    }
}

public class World
{
    private readonly int _renderDistance;
    
    public Chunk[,] Chunks { get; private set; }

    public World(int renderDistance)
    {
        _renderDistance = renderDistance;
        
        Chunks = new Chunk[2 * _renderDistance + 1, 2 * _renderDistance + 1];

        foreach (var chunk in Chunks)
        {
            TerrainGenerator.GenerateChunk(chunk);
        }
    }
    
    public BlockRef this[Vector3i position] => this[position.X, position.Y, position.Z];

    public BlockRef? this[int x, int y, int z]
    {
        get
        {
            return default(BlockRef);
        }
    }
}

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

    public void UpdateMesh()
    {
        Mesh = Mesh.From(this);
    }
}

public class BlockRef
{
    public Chunk Chunk { get; }
    
    public Vector3i Position { get; }
    
    public BlockType Type { get; private set; }

    public void SetBlockType(BlockType type)
    {
        Type = type;
        Chunk.Blocks[Position.X, Position.Y, Position.Z] = type;
    }

    private BlockRef(Chunk chunk, int x, int y, int z, BlockType type)
    {
        Chunk = chunk;
        Position = new Vector3i(x, y, z);
        Type = type;
    }

    public static BlockRef From(Chunk chunk, int x, int y, int z)
    {
        return new BlockRef(chunk, x, y, z, chunk.Blocks[x, y, z]);
    }
}

public enum BlockType : byte
{
    Air = 0,
    Stone = 1,
    Dirt = 2,
}