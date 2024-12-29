using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace CSharp3D;

public class World
{
    public Chunk CurrentChunk { get; private set; }
    
    public Dictionary<Vector2, Chunk> Chunks { get; private set; } = new ();
    
    private List<ChunkGenerationRequest> _chunkGenerationRequests = [];
    
    private ChunkGenerator _chunkGenerator;
    
    public readonly int RenderDistance;

    public const int UpdateDistanceThreshold = 20;

    public World(Camera camera)
    {
        RenderDistance = camera.RenderDistance;
        _chunkGenerator = new ChunkGenerator(camera, this, Game.CancellationToken);
        Initialize(camera);
    }

    public void Initialize(Camera camera)
    {
        foreach (var point in GetPointsAroundChunkWithinRenderDistance(camera.Position, RenderDistance))
        {
            LoadChunk(camera.Position, point);
        }

        CurrentChunk = Chunks.Values.First();

        DispatchGenerationRequests();
    }

    public bool IsChunkInRenderDistance(Vector3 cameraPosition, Vector3 chunkMidpoint)
    {
        var distance = Vector2.Distance(cameraPosition.Xz, chunkMidpoint.Xz) / Chunk.Dimensions.X;
        return distance < RenderDistance + 1;
    }

    public void UpdateCurrentChunk(Vector3 cameraPosition, Chunk newChunk, bool forceUpdate = false)
    {
        if (CurrentChunk == newChunk && !forceUpdate) return;

        var chunkPositionsAroundPlayer = GetPointsAroundChunkWithinRenderDistance(newChunk.Position, RenderDistance).ToList();

        foreach (var chunkPosition in chunkPositionsAroundPlayer)
        {
            if (!IsChunkInRenderDistance(cameraPosition, chunkPosition))
            {
                continue;
            }
            
            if (Chunks.ContainsKey(chunkPosition.Xz))
            {
                continue;
            }
            
            LoadChunk(cameraPosition, chunkPosition);
        }
        
        CurrentChunk = newChunk;

        foreach (var chunk in Chunks.Values.ToList())
        {
            if (!chunkPositionsAroundPlayer.Contains(chunk.Position))
            {
                UnloadChunk(chunk);
            }
        }
        
        DispatchGenerationRequests();
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

    public IEnumerable<Chunk> GetChunksSortedByDistance(Vector3 position)
    {
        List<(float, Chunk)> sortedChunks = new();

        foreach (var chunk in Chunks.Values)
        {
            var distance = Vector2.Distance(chunk.Midpoint.Xz, position.Xz);
            sortedChunks.Add((distance, chunk));
        }
        
        return sortedChunks.OrderBy(sc => sc.Item1).Select(sc => sc.Item2);
    }

    public void Reset(Camera camera)
    {
        foreach (var chunk in Chunks.Values.ToList())
        {
            UnloadChunk(chunk);
        }

        Chunks.Clear();
        _chunkGenerator.Clear();

        Initialize(camera);
    }

    private void LoadChunk(Vector3 cameraPosition, Vector3 chunkPosition)
    {
        Chunks[chunkPosition.Xz] = new Chunk(chunkPosition);
            
        _chunkGenerationRequests.Add(new ChunkGenerationRequest
        {
            Chunk = Chunks[chunkPosition.Xz],
            DistanceFromCamera = Vector2.Distance(cameraPosition.Xz, Chunks[chunkPosition.Xz].Position.Xz)
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