using System.Collections.Concurrent;
using System.Net;

namespace CSharp3D;

public class ChunkGenerator
{
    private readonly Camera _camera;
    private readonly World _world;
    private readonly CancellationToken _cancellationToken;
    private readonly ConcurrentQueue<Chunk> _chunkGenerationRequests = [];

    public ChunkGenerator(Camera camera, World world, CancellationToken cancellationToken)
    {
        _camera = camera;
        _world = world;
        _cancellationToken = cancellationToken;
        
        Task.Run(ProcessQueue, cancellationToken);
    }

    private void ProcessQueue()
    {
        const int ItemsPerPass = 8;
        
        while (_cancellationToken.IsCancellationRequested == false)
        {
            for (var i = 0; i < ItemsPerPass; i++)
                if (_chunkGenerationRequests.TryDequeue(out var chunk))
                {
                    Task.Run(() =>
                    {
                        TerrainGenerator.GenerateChunk(chunk, _world);
                        chunk.UpdateMesh(_world);

                    }, _cancellationToken);
                }
                
            Thread.Sleep(25);
        }
    }

    public void DispatchChunk(Chunk chunk)
    {
        _chunkGenerationRequests.Enqueue(chunk);
    }

    public void Clear()
    {
        _chunkGenerationRequests.Clear();
    }
}

public class ChunkGenerationRequest
{
    public required Chunk Chunk { get; init; }
    public float DistanceFromCamera { get; init; }
}