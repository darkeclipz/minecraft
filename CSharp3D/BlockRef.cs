using OpenTK.Mathematics;

namespace CSharp3D;

public class BlockRef
{
    public Chunk Chunk { get; }
    
    public Vector3i Position { get; }
    
    public Block Block { get; private set; }
    
    public bool IsAir => Block.Type == BlockType.Air;

    private BlockRef(Chunk chunk, int x, int y, int z, Block block)
    {
        Position = new Vector3i(x, y, z);
        Block = block;
        Chunk = chunk;
    }

    public void SetBlockType(BlockType type)
    {
        Chunk.Blocks[Position.X, Position.Y, Position.Z].Type = type;
    }

    public void SetLightLevel(int level)
    {
        Chunk.Blocks[Position.X, Position.Y, Position.Z].LightLevel = level;
    }

    public static BlockRef From(Chunk chunk, int x, int y, int z)
    {
        return new BlockRef(chunk, x, y, z, chunk.Blocks[x, y, z]);
    }

    public IEnumerable<BlockRef> GetNeighbours9X9()
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
    
    public IEnumerable<BlockRef> GetNeighboursOnSameY()
    {
        List<BlockRef> neighbours = [];
        
        for (int x = -1; x <= 1; x++)
        for (int z = -1; z <= 1; z++)
        {
            var position = new Vector3i(Position.X + x, Position.Y, Position.Z + z);

            if (Chunk.IsLocalPointInChunk(position) && position != Position)
            {
                neighbours.Add(From(Chunk, position.X, position.Y, position.Z));
            }
        }

        return neighbours;
    }
}