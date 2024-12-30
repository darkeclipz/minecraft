using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace CSharp3D;

public class World
{
    public Chunk CurrentChunk { get; private set; }
    
    public Dictionary<Vector2, Chunk> Chunks { get; private set; } = new ();
    
    private List<ChunkGenerationRequest> _chunkGenerationRequests = [];
    
    private Camera _camera;
    
    private ChunkBackgroundWorker _chunkBackgroundWorker;

    private MeshBackgroundWorker _meshBackgroundWorker;

    private Task _chunkUnloader;

    public const int UpdateDistanceThreshold = 20;

    public World(Camera camera)
    {
        _camera = camera;
        _meshBackgroundWorker = new MeshBackgroundWorker(camera, this, Game.CancellationToken);
        _chunkBackgroundWorker = new ChunkBackgroundWorker(camera, this, _meshBackgroundWorker, Game.CancellationToken);
        _chunkUnloader = Task.Run(ChunkUnloader);
        Initialize();
    }

    public void Initialize()
    {
        var x = ((int)_camera.Position.X / Chunk.Dimensions.X) * Chunk.Dimensions.X;
        var z = ((int)_camera.Position.Z / Chunk.Dimensions.Z) * Chunk.Dimensions.Z;
        var origin = new Vector3(x, 0, z);
        
        foreach (var point in GetPointsAroundChunkWithinRenderDistance(origin, _camera.RenderDistance))
        {
            LoadChunk(_camera.Position, point);
        }

        CurrentChunk = Chunks.Values.First();

        DispatchGenerationRequests();
    }

    public bool IsChunkInRenderDistance(Vector3 cameraPosition, Vector3 chunkMidpoint)
    {
        var maxDistance = (_camera.RenderDistance + 1) * Chunk.Dimensions.X;
        return Vector2.DistanceSquared(cameraPosition.Xz, chunkMidpoint.Xz) < maxDistance * maxDistance;
    }

    public void UpdateCurrentChunk(Vector3 cameraPosition, Chunk newChunk, bool forceUpdate = false)
    {
        if (CurrentChunk == newChunk && !forceUpdate) return;

        var chunkPositionsAroundPlayer =
            GetPointsAroundChunkWithinRenderDistance(newChunk.Position, _camera.RenderDistance).ToList();

        foreach (var chunkPosition in chunkPositionsAroundPlayer)
        {
            if (Chunks.ContainsKey(chunkPosition.Xz))
            {
                continue;
            }
            
            if (!IsChunkInRenderDistance(cameraPosition, chunkPosition))
            {
                continue;
            }

            LoadChunk(cameraPosition, chunkPosition);
        }

        CurrentChunk = newChunk;



        DispatchGenerationRequests();

    }

    private void ChunkUnloader()
    {
        while (true)
        {
            try
            {
                var maxDistance = (_camera.RenderDistance + 3) * Chunk.Dimensions.X;

                foreach (var chunk in Chunks.Values.ToList())
                {
                    if (Vector2.DistanceSquared(_camera.Position.Xz, chunk.Midpoint.Xz) > maxDistance * maxDistance)
                    {
                        UnloadChunk(chunk);
                    }
                }

                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Chunk unloader encoutered {ex}");
            }
        }
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
            var distance = Vector2.DistanceSquared(chunk.Midpoint.Xz, position.Xz);
            sortedChunks.Add((distance, chunk));
        }
        
        return sortedChunks.OrderByDescending(sc => sc.Item1).Select(sc => sc.Item2);
    }

    public void Reset()
    {
        foreach (var chunk in Chunks.Values.ToList())
        {
            UnloadChunk(chunk);
        }

        Chunks.Clear();
        _chunkBackgroundWorker.Clear();
        _meshBackgroundWorker.Clear();

        Initialize();
    }

    public void Reload()
    {
        _chunkBackgroundWorker.Clear();
        _meshBackgroundWorker.Clear();
        
        foreach (var chunk in Chunks.Values)
        {
            _chunkBackgroundWorker.DispatchChunk(chunk);
        }
    }

    public BlockRef? GetBlockOrNull(Vector3 worldPos)
    {
        var chunk = Chunks.Values.FirstOrDefault(c => c.BoundingBox.IntersectsWith(worldPos));

        if (chunk is null)
        {
            Console.WriteLine($"WARNING: Unable to find block with world coordinates {worldPos}.");
            return null;
        }

        var localPosition = worldPos - chunk.Position;

        return chunk.GetBlockRef(localPosition);
    }

    public Chunk? FindChunkOrNull(Vector3 position)
    {
        return Chunks.Values.FirstOrDefault(c => c.BoundingBox.IntersectsWith(position));
    }

    private void LoadChunk(Vector3 cameraPosition, Vector3 chunkPosition)
    {
        var chunk = new Chunk(chunkPosition, this);
        Chunks[chunkPosition.Xz] = chunk;
        LinkNeighbours(chunk);
        var distanceFromCamera = Vector2.DistanceSquared(cameraPosition.Xz, chunk.Position.Xz);
        var chunkGenerationRequest = new ChunkGenerationRequest(chunk, distanceFromCamera);
        _chunkGenerationRequests.Add(chunkGenerationRequest);
    }

    private void LinkNeighbours(Chunk chunk)
    {
        if (Chunks.TryGetValue(chunk.Position.Xz + Vector2.UnitX * Chunk.Dimensions.X, out var right))
        {
            chunk.Right = right;
            right.Left = chunk;
            
            // if (right.HasNeighbours) _meshGenerator.DispatchChunk(right);
        }
        
        if (Chunks.TryGetValue(chunk.Position.Xz - Vector2.UnitX * Chunk.Dimensions.X, out var left))
        {
            chunk.Left = left;
            left.Right = chunk;
            
            // if (left.HasNeighbours) _meshGenerator.DispatchChunk(left);
        }

        if (Chunks.TryGetValue(chunk.Position.Xz + Vector2.UnitY * Chunk.Dimensions.Z, out var front))
        {
            chunk.Front = front;
            front.Back = chunk;
            
            // if (front.HasNeighbours) _meshGenerator.DispatchChunk(front);
        }
        
        if (Chunks.TryGetValue(chunk.Position.Xz - Vector2.UnitY * Chunk.Dimensions.Z, out var back))
        {
            chunk.Back = back;
            back.Front = chunk;
            
            // if (back.HasNeighbours) _meshGenerator.DispatchChunk(back);
        }
    }

    private void UnlinkNeighbours(Chunk chunk)
    {
        if (chunk.Left is not null)
        {
            chunk.Left.Right = null;
            chunk.Left = null;    
        }

        if (chunk.Right is not null)
        {
            chunk.Right.Left = null;
            chunk.Right = null;
        }

        if (chunk.Front is not null)
        {
            chunk.Front.Back = null;
            chunk.Front = null;
        }

        if (chunk.Back is not null)
        {
            chunk.Back.Front = null;
            chunk.Back = null;
        }
    }
    
    private void DispatchGenerationRequests()
    {
        foreach (var requestedChunk in _chunkGenerationRequests
                     .OrderBy(gr => gr.DistanceFromCamera)
                     .Select(gr => gr.Chunk))
        {
            _chunkBackgroundWorker.DispatchChunk(requestedChunk);
        }

        _chunkGenerationRequests.Clear();
    }

    private void UnloadChunk(Chunk chunk)
    {
        UnlinkNeighbours(chunk);
        Chunks.Remove(chunk.Position.Xz);
        Game.ChunkDisposeQueue.Enqueue(chunk);
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
    
    record ChunkGenerationRequest(Chunk Chunk, float DistanceFromCamera);
}