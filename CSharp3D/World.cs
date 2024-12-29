using System.Net.Http.Headers;
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
        
        List<ChunkGenerationRequest> chunkGenerationRequests = [];
        
        for (var x = 0; x < Chunks.GetLength(0); x++)
        {
            for (var y = 0; y < Chunks.GetLength(1); y++)
            {
                Chunks[x, y] = new Chunk
                {
                    Position = new Vector3((x - renderDistance) * Chunk.Dimensions.X - renderDistance, 0, (y - renderDistance) * Chunk.Dimensions.Z)
                };

                chunkGenerationRequests.Add(new ChunkGenerationRequest
                {
                    Chunk = Chunks[x, y],
                    DistanceFromCamera = Vector3.Distance(Chunks[x, y].Position, Vector3.Zero)
                });
            }
        }

        foreach (var requestedChunk in chunkGenerationRequests
                     .OrderBy(gr => gr.DistanceFromCamera)
                     .Select(gr => gr.Chunk))
        {
            Task.Run(() =>
            {
                Thread.Sleep(20);
                TerrainGenerator.GenerateChunk(requestedChunk, this);
                requestedChunk.UpdateMesh(this);
            });
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

    class ChunkGenerationRequest
    {
        public Chunk Chunk { get; init; }
        public float DistanceFromCamera { get; init; }
    }
}