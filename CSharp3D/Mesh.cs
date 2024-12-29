using System.Diagnostics;
using OpenTK.Graphics.OpenGLES2;
using OpenTK.Mathematics;

namespace CSharp3D;

public class Mesh : IDisposable
{
    public float[] Vertices { get; private set; }
    
    public int VertexArrayObject { get; private set; }
    
    public int VertexBufferObject { get; private set; }

    private bool _isDirty = false;

    public static Mesh From(Chunk chunk, World world)
    {
        // Calculate a mesh from the chunks block data.
        
        // First we make a simple mesh, we run a plane from top to bottom and once we find
        // where air meets a solid, we create the top triangles for the mesh.

        // A triangle has 3 points, and a point has three floats: x, y, z, texX, texY.
        
        // Y is up.
        
        // These are the points for the top face of a cube.
        // -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
        // 0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        // 0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        // 0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        // -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
        // -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
        
        var stopwatch = Stopwatch.StartNew();
        List<float> vertices = [];

        var midpoint = chunk.Midpoint;
        //
        // var backChunk = world.FindChunkOrNull(midpoint - Vector3.UnitZ * Chunk.Dimensions.Z);
        // var frontChunk = world.FindChunkOrNull(midpoint + Vector3.UnitZ * Chunk.Dimensions.Z);
        // var leftChunk = world.FindChunkOrNull(midpoint - Vector3.UnitX * Chunk.Dimensions.X);
        // var rightChunk = world.FindChunkOrNull(midpoint + Vector3.UnitX * Chunk.Dimensions.X);
        //
        // Block? GetBlockOrNull(int x, int y, int z)
        // {
        //     var current = chunk;
        //
        //     if (x >= 0 && x < Chunk.Dimensions.X 
        //        && y >= 0 && y < Chunk.Dimensions.Y 
        //        && z >= 0 && z < Chunk.Dimensions.Z)
        //     {
        //         return current.Blocks[x, y, z];
        //     }
        //
        //     if (x < 0)
        //     {
        //         current = leftChunk;
        //         x = x + 16;
        //     }
        //
        //     if (x >= 16)
        //     {
        //         current = rightChunk;
        //         x = x - 16;
        //     }
        //
        //     if (z < 0)
        //     {
        //         current = backChunk;
        //         z = z + 16;
        //     }
        //
        //     if (z >= 16)
        //     {
        //         current = frontChunk;
        //         z = z - 16;
        //     }
        //
        //     if (current is null)
        //     {
        //         return null;
        //     }
        //
        //     return current.Blocks[x, y, z];
        // }
        
        // bool IsSolid(int x, int y, int z)
        // {
        //     var block = GetBlockOrNull(x, y, z);
        //
        //     if (block is null) return false;
        //     
        //     return block.Value.Type != BlockType.Air;
        // }

        for (var y = Chunk.Dimensions.Y - 1; y >= 0; y--)
        {
            for (var x = 0; x < Chunk.Dimensions.X; x++)
            {
                for (var z = 0; z < Chunk.Dimensions.Z; z++)
                {
                    if (chunk.Blocks[x, y, z].Type != BlockType.Air)
                    {
                        var worldX = chunk.Position.X + x;
                        var worldY = chunk.Position.Y + y;
                        var worldZ = chunk.Position.Z + z;

                        var position = new Vector3(x, y, z);
                        var block = chunk.Blocks[x, y, z];
                        
                        var topBlock = y + 1 < Chunk.Dimensions.Y ? chunk.Blocks[x, y + 1, z] : Block.LitAir;
                        
                        var bottomBlock = y - 1 > 0 ? chunk.Blocks[x, y - 1, z] : Block.UnlitAir;

                        var frontBlock = z - 1 > 0 ? chunk.Blocks[x, y, z - 1] : Block.LitAir;
                        var backBlock = z + 1 < Chunk.Dimensions.Z ? chunk.Blocks[x, y, z + 1] : Block.LitAir;
                        
                        var leftBlock = x - 1 > 0 ? chunk.Blocks[x - 1, y, z] : Block.LitAir;
                        var rightBlock = x + 1 < Chunk.Dimensions.X ? chunk.Blocks[x + 1, y, z] : Block.LitAir;
                        
                        // Top
                        if (!chunk.IsSolid(x, y + 1, z))
                        {
                            var blockType = (int)OverrideBlockTypeForFace(block.Type, Side.Top);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 0f, 1f, 0f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType), GetLight(topBlock)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 0f, 1f, 0f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType), GetLight(topBlock)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 0f, 1f, 0f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType), GetLight(topBlock)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 0f, 1f, 0f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType), GetLight(topBlock)]);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 0f, 1f, 0f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType), GetLight(topBlock)]);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 0f, 1f, 0f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType), GetLight(topBlock)]);
                        }

                        // Bottom
                        if (!chunk.IsSolid(x, y - 1, z))
                        {
                            var blockType = (int)OverrideBlockTypeForFace(block.Type, Side.Bottom);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 0f, -1f, 0f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType), GetLight(bottomBlock)]);
                            vertices.AddRange([ 0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 0f, -1f, 0f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType), GetLight(bottomBlock)]);
                            vertices.AddRange([ 0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 0f, -1f, 0f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType), GetLight(bottomBlock)]);
                            vertices.AddRange([ 0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 0f, -1f, 0f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType), GetLight(bottomBlock)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 0f, -1f, 0f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType), GetLight(bottomBlock)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 0f, -1f, 0f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType), GetLight(bottomBlock)]);
                        }
                        
                        // Back
                        if (!chunk.IsSolid(x, y, z - 1))
                        {
                            var blockType = (int)OverrideBlockTypeForFace(block.Type, Side.Back);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 0f, 0f, -1f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType), GetLight(frontBlock)]);
                            vertices.AddRange([ 0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 0f, 0f, -1f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType), GetLight(frontBlock)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 0f, 0f, -1f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType), GetLight(frontBlock)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 0f, 0f, -1f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType), GetLight(frontBlock)]);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 0f, 0f, -1f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType), GetLight(frontBlock)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 0f, 0f, -1f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType), GetLight(frontBlock)]);    
                        }
                        
                        // Front
                        if (!chunk.IsSolid(x, y, z + 1))
                        {
                            var blockType = (int)OverrideBlockTypeForFace(block.Type, Side.Front);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 0f, 0f, 1f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType), GetLight(backBlock)]);
                            vertices.AddRange([ 0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 0f, 0f, 1f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType), GetLight(backBlock)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 0f, 0f, 1f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType), GetLight(backBlock)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 0f, 0f, 1f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType), GetLight(backBlock)]);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 0f, 0f, 1f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType), GetLight(backBlock)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 0f, 0f, 1f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType), GetLight(backBlock)]);
                        }
                        
                        // Left
                        if (!chunk.IsSolid(x - 1, y, z))
                        {
                            var blockType = (int)OverrideBlockTypeForFace(block.Type, Side.Left);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, -1f, 0f, 0f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType), GetLight(leftBlock)]);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, -1f, 0f, 0f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType), GetLight(leftBlock)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, -1f, 0f, 0f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType), GetLight(leftBlock)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, -1f, 0f, 0f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType), GetLight(leftBlock)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, -1f, 0f, 0f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType), GetLight(leftBlock)]);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, -1f, 0f, 0f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType), GetLight(leftBlock)]);
                        }
                        
                        // Right
                        if (!chunk.IsSolid(x + 1, y, z))
                        {
                            var blockType = (int)OverrideBlockTypeForFace(block.Type, Side.Right);
                            vertices.AddRange([0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 1f, 0f, 0f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType), GetLight(rightBlock)]);
                            vertices.AddRange([0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 1f, 0f, 0f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType), GetLight(rightBlock)]);
                            vertices.AddRange([0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 1f, 0f, 0f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType), GetLight(rightBlock)]);
                            vertices.AddRange([0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 1f, 0f, 0f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType), GetLight(rightBlock)]);
                            vertices.AddRange([0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 1f, 0f, 0f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType), GetLight(rightBlock)]);
                            vertices.AddRange([0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 1f, 0f, 0f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType), GetLight(rightBlock)]);
                        }
                    }
                }
            }
        }
        
        return new Mesh
        {
            Vertices = vertices.ToArray(),
        };
    }

    public bool IsLoaded = false;

    public const int VertexBufferCount = 9;

    public void Use()
    {
        if (!IsLoaded)
        {
            IsLoaded = true;
            
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
        
            VertexBufferObject = GL.GenBuffer();
        
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexArrayObject);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsage.StaticDraw);
        
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VertexBufferCount * sizeof(float), 0);
        
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, VertexBufferCount * sizeof(float), 3 * sizeof(float));
        
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, VertexBufferCount * sizeof(float), 6 * sizeof(float));
            
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, VertexBufferCount * sizeof(float), 8 * sizeof(float));
            
            // Console.WriteLine($"Loaded mesh VAO: {VertexArrayObject}.");
        }
    }

    private static object glLock = false;
    
    private const int atlasBlockSizeInPixels = 16;
    private const int atlasWidthInBlocks = 16;
    private const int atlasHeightInBlocks = 16;

    private static float TextureX(float x, int blockId)
    {
        // vec2 texSize = vec2(16.0, 16.0);
        // vec2 texOffset = vec2(1.0, (16.0 - 1.0));
        // FragColor = texture(texture0, (texCoord + texOffset) / texSize);
        var offset = blockId % atlasWidthInBlocks;
        return (x + offset) / atlasBlockSizeInPixels;
    }

    private static float TextureY(float y, int blockId)
    {
        var offset = blockId / atlasHeightInBlocks;
        return (y - offset - 1.0f) / atlasBlockSizeInPixels;
    }

    private static float GetLight(Block block)
    {
        return (float)Math.Clamp(block.LightLevel / 15.0, 0.0, 1.0);
    }

    private static BlockType OverrideBlockTypeForFace(BlockType type, Side side)
    {
        if (type == BlockType.Grass)
        {
            return side switch
            {
                Side.Top => BlockType.Grass,
                Side.Bottom => BlockType.Dirt,
                _ => BlockType.GrassSide
            };
        }

        if (type == BlockType.SinisterGrass)
        {
            return side switch
            {
                Side.Top => BlockType.SinisterGrass,
                Side.Bottom => BlockType.Dirt,
                _ => BlockType.SinisterGrassSide
            };
        }

        if (type == BlockType.Tree)
        {
            return side switch
            {
                Side.Top => BlockType.TreeTop,
                Side.Bottom => BlockType.TreeTop,
                _ => BlockType.Tree
            };
        }

        return type;
    }

    enum Side
    {
        Top,
        Bottom,
        Front,
        Back,
        Left,
        Right,
    }

    private bool disposed = false;
    
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        IsLoaded = false;
        
        GL.DeleteVertexArray(VertexArrayObject);
        GL.DeleteBuffer(VertexBufferObject);
        
        // Console.WriteLine($"Disposed mesh VAO: {VertexArrayObject}.");
    }
}