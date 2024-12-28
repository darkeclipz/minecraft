using OpenTK.Mathematics;

namespace CSharp3D;

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