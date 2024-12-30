using System.Collections.Concurrent;
using System.Net;

namespace CSharp3D;

public class ChunkGenerator
{
    private readonly Camera _camera;
    private readonly World _world;
    private readonly CancellationToken _cancellationToken;
    private readonly ConcurrentQueue<Chunk> _chunkGenerationRequests = [];
    private readonly MeshGenerator _meshGenerator;

    public ChunkGenerator(Camera camera, World world, MeshGenerator meshGenerator, CancellationToken cancellationToken)
    {
        _camera = camera;
        _world = world;
        _meshGenerator = meshGenerator;
        _cancellationToken = cancellationToken;
        
        Task.Run(async () => ProcessQueue(), cancellationToken);
    }

    private async Task ProcessQueue()
    {
        const int ItemsPerPass = 8;
        
        while (_cancellationToken.IsCancellationRequested == false)
        {
            for (var i = 0; i < ItemsPerPass; i++)
                if (_chunkGenerationRequests.TryDequeue(out var chunk))
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            TerrainGenerator.GenerateChunk(chunk, _world);
                            _meshGenerator.DispatchChunk(chunk);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ERROR: {ex}");                            
                        }

                    }, _cancellationToken);
                }
            
            await Task.Delay(25);
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
