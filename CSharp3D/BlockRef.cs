using OpenTK.Mathematics;

namespace CSharp3D;

public class BlockRef
{
    public Chunk Chunk { get; }
    
    public Vector3i Position { get; }
    
    public BlockType Type { get; private set; }
    
    public bool IsAir => Type == BlockType.Air;

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

    public IEnumerable<BlockRef> GetNeighbours()
    {
        List<BlockRef> neighbours = [];
        
        for (int x = -1; x <= 1; x++)
        for (int y = -1; y <= 1; y++)
        for (int z = -1; z <= 1; z++)
        {
            var position = new Vector3i(Position.X + x, Position.Y + y, Position.Z + z);

            if (Chunk.IsLocalPointInChunk(position) && position != Position)
            {
                neighbours.Add(From(Chunk, position.X, position.Y, position.Z));
            }
        }

        return neighbours;
    }
}