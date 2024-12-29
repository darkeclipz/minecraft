using System.Diagnostics;
using OpenTK.Graphics.OpenGLES2;

namespace CSharp3D;

public class Mesh : IDisposable
{
    public float[] Vertices { get; private set; }
    
    public int VertexArrayObject { get; private set; }
    
    public int VertexBufferObject { get; private set; }

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

        for (var y = Chunk.Dimensions.Y - 1; y >= 0; y--)
        {
            for (var x = 0; x < Chunk.Dimensions.X; x++)
            {
                for (var z = 0; z < Chunk.Dimensions.Z; z++)
                {
                    if (chunk.Blocks[x, y, z] != BlockType.Air)
                    {
                        var worldX = chunk.Position.X + x;
                        var worldY = chunk.Position.Y + y;
                        var worldZ = chunk.Position.Z + z;
                        
                        // Top
                        if (!chunk.IsSolid(x, y + 1, z))
                        {
                            var blockType = (int)OverrideBlockTypeForFace(chunk.Blocks[x, y, z], Side.Top);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 0f, 1f, 0f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 0f, 1f, 0f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 0f, 1f, 0f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 0f, 1f, 0f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 0f, 1f, 0f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 0f, 1f, 0f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType)]);
                        }

                        // Bottom
                        if (!chunk.IsSolid(x, y - 1, z))
                        {
                            var blockType = (int)OverrideBlockTypeForFace(chunk.Blocks[x, y, z], Side.Bottom);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 0f, -1f, 0f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([ 0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 0f, -1f, 0f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([ 0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 0f, -1f, 0f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([ 0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 0f, -1f, 0f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 0f, -1f, 0f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 0f, -1f, 0f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType)]);
                        }
                        
                        // Back
                        if (!chunk.IsSolid(x, y, z - 1))
                        {
                            var blockType = (int)OverrideBlockTypeForFace(chunk.Blocks[x, y, z], Side.Back);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 0f, 0f, -1f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([ 0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 0f, 0f, -1f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 0f, 0f, -1f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 0f, 0f, -1f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 0f, 0f, -1f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 0f, 0f, -1f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType)]);    
                        }
                        
                        // Front
                        if (!chunk.IsSolid(x, y, z + 1))
                        {
                            var blockType = (int)OverrideBlockTypeForFace(chunk.Blocks[x, y, z], Side.Front);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 0f, 0f, 1f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([ 0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 0f, 0f, 1f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 0f, 0f, 1f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([ 0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 0f, 0f, 1f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 0f, 0f, 1f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 0f, 0f, 1f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType)]);
                        }
                        
                        // Left
                        if (!chunk.IsSolid(x - 1, y, z))
                        {
                            var blockType = (int)OverrideBlockTypeForFace(chunk.Blocks[x, y, z], Side.Left);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, -1f, 0f, 0f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, -1f, 0f, 0f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, -1f, 0f, 0f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, -1f, 0f, 0f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, -1f, 0f, 0f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([-0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, -1f, 0f, 0f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType)]);
                        }
                        
                        // Right
                        if (!chunk.IsSolid(x + 1, y, z))
                        {
                            var blockType = (int)OverrideBlockTypeForFace(chunk.Blocks[x, y, z], Side.Right);
                            vertices.AddRange([0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 1f, 0f, 0f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([0.5f + worldX,  0.5f + worldY, -0.5f + worldZ, 1f, 0f, 0f,  TextureX(0.0f, blockType), TextureY(1.0f, blockType)]);
                            vertices.AddRange([0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 1f, 0f, 0f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([0.5f + worldX, -0.5f + worldY, -0.5f + worldZ, 1f, 0f, 0f,  TextureX(0.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([0.5f + worldX, -0.5f + worldY,  0.5f + worldZ, 1f, 0f, 0f,  TextureX(1.0f, blockType), TextureY(0.0f, blockType)]);
                            vertices.AddRange([0.5f + worldX,  0.5f + worldY,  0.5f + worldZ, 1f, 0f, 0f,  TextureX(1.0f, blockType), TextureY(1.0f, blockType)]);
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
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            
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