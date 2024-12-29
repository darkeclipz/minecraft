using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace CSharp3D;

public class World
{
    public Chunk CurrentChunk { get; private set; }
    
    public Dictionary<Vector2, Chunk> Chunks { get; private set; } = new ();
    
    private List<ChunkGenerationRequest> _chunkGenerationRequests = [];
    
    private ChunkGenerator _chunkGenerator;
    
    private readonly int _renderDistance;

    public const int UpdateDistanceThreshold = 20;

    public World(int renderDistance)
    {
        _renderDistance = renderDistance;
        _chunkGenerator = new ChunkGenerator(this, Game.CancellationToken);

        foreach (var point in GetPointsAroundChunkWithinRenderDistance(Vector3.Zero, _renderDistance))
        {
            LoadChunk(point.Xz);
        }

        CurrentChunk = Chunks.Values.First();

        DispatchGenerationRequests();
    }

    public void UpdateCurrentChunk(Vector3 position, Chunk newChunk)
    {
        if (CurrentChunk == newChunk) return;

        var pointsAroundPlayer = GetPointsAroundChunkWithinRenderDistance(newChunk.Position, _renderDistance).ToList();

        foreach (var point in pointsAroundPlayer)
        {
            var chunkMidpoint = point + new Vector3(Chunk.Dimensions.X / 2, 0, Chunk.Dimensions.Z / 2);
            var distance = Vector2.Distance(position.Xz, chunkMidpoint.Xz) / Chunk.Dimensions.X;
            
            if (!Chunks.ContainsKey(point.Xz) && distance < _renderDistance + 1)
            {
                LoadChunk(point.Xz);
            }
        }
        
        CurrentChunk = newChunk;

        foreach (var chunk in Chunks.Values.ToList())
        {
            if (!pointsAroundPlayer.Contains(chunk.Position))
            {
                UnloadChunk(chunk);
            }
        }
        
        DispatchGenerationRequests();
        
        Console.WriteLine($"Current chunk updated to chunk {newChunk.Position.X}, {newChunk.Position.Z}");
    }

    public Chunk GetNearestChunk(Vector2 position)
    {
        Chunk? nearestChunk = Chunks.Values.First();
        var minDistance = double.MaxValue;

        foreach (var chunk in Chunks.Values)
        {
            var distance = Vector2.DistanceSquared(position, chunk.Position.Xz);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestChunk = chunk;
            }
        }

        return nearestChunk;
    }

    private void LoadChunk(Vector2 position)
    {
        var chunkPosition = new Vector3(position.X, 0, position.Y);

        Chunks[chunkPosition.Xz] = new Chunk(chunkPosition);
            
        _chunkGenerationRequests.Add(new ChunkGenerationRequest
        {
            Chunk = Chunks[chunkPosition.Xz],
            DistanceFromCamera = Vector3.Distance(Chunks[chunkPosition.Xz].Position, Vector3.Zero)
        });
    }
    
    private void DispatchGenerationRequests()
    {
        foreach (var requestedChunk in _chunkGenerationRequests
                     .OrderBy(gr => gr.DistanceFromCamera)
                     .Select(gr => gr.Chunk))
        {
            _chunkGenerator.DispatchChunk(requestedChunk);
        }

        _chunkGenerationRequests.Clear();
    }

    private void UnloadChunk(Chunk chunk)
    {
        Chunks.Remove(chunk.Position.Xz);
        chunk.Dispose();
    }

    private IEnumerable<Vector3> GetPointsAroundChunkWithinRenderDistance(Chunk chunk, int renderDistance) 
        => GetPointsAroundChunkWithinRenderDistance(chunk.Position, renderDistance);
    
    private IEnumerable<Vector3> GetPointsAroundChunkWithinRenderDistance(Vector3 position, int renderDistance)
    {
        for (int x = -renderDistance; x <= renderDistance; x++)
        for (int z = -renderDistance; z <= renderDistance; z++)
        {
            var chunkX = position.X + x * Chunk.Dimensions.X;
            var chunkZ = position.Z + z * Chunk.Dimensions.Z;

            yield return new Vector3(chunkX, 0, chunkZ);
        }
    }
}