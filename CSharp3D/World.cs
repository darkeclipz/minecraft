using OpenTK.Mathematics;

namespace CSharp3D;

public class World
{
    private readonly int _renderDistance;
    
    public Chunk[,] Chunks { get; private set; }

    public World(int renderDistance)
    {
        _renderDistance = renderDistance;
        
        Chunks = new Chunk[2 * _renderDistance + 1, 2 * _renderDistance + 1];

        for (var x = 0; x < Chunks.GetLength(0); x++)
        {
            for (var y = 0; y < Chunks.GetLength(1); y++)
            {
                Chunks[x, y] = new Chunk
                {
                    Position = new Vector3(x * Chunk.Dimensions.X, 0, y * Chunk.Dimensions.Z)
                };

                TerrainGenerator.GenerateChunk(Chunks[x, y], this);
                
                Chunks[x, y].UpdateMesh(this);
            }
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