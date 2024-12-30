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
        var stopwatch = Stopwatch.StartNew();
        List<float> vertices = [];
        
        for (var y = Chunk.Dimensions.Y - 1; y >= 0; y--)
        {
            for (var x = 0; x < Chunk.Dimensions.X; x++)
            {
                for (var z = 0; z < Chunk.Dimensions.Z; z++)
                {
                    if (chunk.Blocks[x, y, z].Type != BlockType.Air)
                    {
                        var wx = chunk.Position.X + x;
                        var wy = chunk.Position.Y + y;
                        var wz = chunk.Position.Z + z;

                        var position = new Vector3(x, y, z);
                        var block = chunk.Blocks[x, y, z];
                        
                        var top = y + 1 < Chunk.Dimensions.Y ? chunk.Blocks[x, y + 1, z] : Block.LitAir;
                        var bot = y - 1 > 0 ? chunk.Blocks[x, y - 1, z] : Block.UnlitAir;
                        var fro = z - 1 > 0 ? chunk.Blocks[x, y, z - 1] : Block.LitAir;
                        var bac = z + 1 < Chunk.Dimensions.Z ? chunk.Blocks[x, y, z + 1] : Block.LitAir;
                        var lef = x - 1 > 0 ? chunk.Blocks[x - 1, y, z] : Block.LitAir;
                        var rig = x + 1 < Chunk.Dimensions.X ? chunk.Blocks[x + 1, y, z] : Block.LitAir;
                        
                        // Top
                        if (!chunk.IsSolid(x, y + 1, z))
                        {
                            var bt = (int)OverrideBlockTypeForFace(block.Type, Side.Top);
                            vertices.AddRange([-0.5f + wx,  0.5f + wy, -0.5f + wz, 0f, 1f, 0f, 1f, 0f, 0f, Tx(0.0f, bt), Ty(1.0f, bt), Li(top)]);
                            vertices.AddRange([ 0.5f + wx,  0.5f + wy, -0.5f + wz, 0f, 1f, 0f, 0f, 1f, 0f, Tx(1.0f, bt), Ty(1.0f, bt), Li(top)]);
                            vertices.AddRange([ 0.5f + wx,  0.5f + wy,  0.5f + wz, 0f, 1f, 0f, 0f, 0f, 1f, Tx(1.0f, bt), Ty(0.0f, bt), Li(top)]);
                            vertices.AddRange([ 0.5f + wx,  0.5f + wy,  0.5f + wz, 0f, 1f, 0f, 1f, 0f, 0f, Tx(1.0f, bt), Ty(0.0f, bt), Li(top)]);
                            vertices.AddRange([-0.5f + wx,  0.5f + wy,  0.5f + wz, 0f, 1f, 0f, 0f, 1f, 0f, Tx(0.0f, bt), Ty(0.0f, bt), Li(top)]);
                            vertices.AddRange([-0.5f + wx,  0.5f + wy, -0.5f + wz, 0f, 1f, 0f, 0f, 0f, 1f, Tx(0.0f, bt), Ty(1.0f, bt), Li(top)]);
                        }

                        // Bottom
                        if (!chunk.IsSolid(x, y - 1, z))
                        {
                            var bt = (int)OverrideBlockTypeForFace(block.Type, Side.Bottom);
                            vertices.AddRange([-0.5f + wx, -0.5f + wy, -0.5f + wz, 0f, -1f, 0f, 1f, 0f, 0f, Tx(0.0f, bt), Ty(1.0f, bt), Li(bot)]);
                            vertices.AddRange([ 0.5f + wx, -0.5f + wy, -0.5f + wz, 0f, -1f, 0f, 0f, 1f, 0f, Tx(1.0f, bt), Ty(1.0f, bt), Li(bot)]);
                            vertices.AddRange([ 0.5f + wx, -0.5f + wy,  0.5f + wz, 0f, -1f, 0f, 0f, 0f, 1f, Tx(1.0f, bt), Ty(0.0f, bt), Li(bot)]);
                            vertices.AddRange([ 0.5f + wx, -0.5f + wy,  0.5f + wz, 0f, -1f, 0f, 1f, 0f, 0f, Tx(1.0f, bt), Ty(0.0f, bt), Li(bot)]);
                            vertices.AddRange([-0.5f + wx, -0.5f + wy,  0.5f + wz, 0f, -1f, 0f, 0f, 1f, 0f, Tx(0.0f, bt), Ty(0.0f, bt), Li(bot)]);
                            vertices.AddRange([-0.5f + wx, -0.5f + wy, -0.5f + wz, 0f, -1f, 0f, 0f, 0f, 1f, Tx(0.0f, bt), Ty(1.0f, bt), Li(bot)]);
                        }
                        
                        // Back
                        if (!chunk.IsSolid(x, y, z - 1))
                        {
                            var bt = (int)OverrideBlockTypeForFace(block.Type, Side.Back);
                            vertices.AddRange([-0.5f + wx, -0.5f + wy, -0.5f + wz, 0f, 0f, -1f, 1f, 0f, 0f, Tx(0.0f, bt), Ty(0.0f, bt), Li(fro)]);
                            vertices.AddRange([ 0.5f + wx, -0.5f + wy, -0.5f + wz, 0f, 0f, -1f, 0f, 1f, 0f, Tx(1.0f, bt), Ty(0.0f, bt), Li(fro)]);
                            vertices.AddRange([ 0.5f + wx,  0.5f + wy, -0.5f + wz, 0f, 0f, -1f, 0f, 0f, 1f, Tx(1.0f, bt), Ty(1.0f, bt), Li(fro)]);
                            vertices.AddRange([ 0.5f + wx,  0.5f + wy, -0.5f + wz, 0f, 0f, -1f, 1f, 0f, 0f, Tx(1.0f, bt), Ty(1.0f, bt), Li(fro)]);
                            vertices.AddRange([-0.5f + wx,  0.5f + wy, -0.5f + wz, 0f, 0f, -1f, 0f, 1f, 0f, Tx(0.0f, bt), Ty(1.0f, bt), Li(fro)]);
                            vertices.AddRange([-0.5f + wx, -0.5f + wy, -0.5f + wz, 0f, 0f, -1f, 0f, 0f, 1f, Tx(0.0f, bt), Ty(0.0f, bt), Li(fro)]);    
                        }
                        
                        // Front
                        if (!chunk.IsSolid(x, y, z + 1))
                        {
                            var bt = (int)OverrideBlockTypeForFace(block.Type, Side.Front);
                            vertices.AddRange([-0.5f + wx, -0.5f + wy,  0.5f + wz, 0f, 0f, 1f, 1f, 0f, 0f, Tx(0.0f, bt), Ty(0.0f, bt), Li(bac)]);
                            vertices.AddRange([ 0.5f + wx, -0.5f + wy,  0.5f + wz, 0f, 0f, 1f, 0f, 1f, 0f, Tx(1.0f, bt), Ty(0.0f, bt), Li(bac)]);
                            vertices.AddRange([ 0.5f + wx,  0.5f + wy,  0.5f + wz, 0f, 0f, 1f, 0f, 0f, 1f, Tx(1.0f, bt), Ty(1.0f, bt), Li(bac)]);
                            vertices.AddRange([ 0.5f + wx,  0.5f + wy,  0.5f + wz, 0f, 0f, 1f, 1f, 0f, 0f, Tx(1.0f, bt), Ty(1.0f, bt), Li(bac)]);
                            vertices.AddRange([-0.5f + wx,  0.5f + wy,  0.5f + wz, 0f, 0f, 1f, 0f, 1f, 0f, Tx(0.0f, bt), Ty(1.0f, bt), Li(bac)]);
                            vertices.AddRange([-0.5f + wx, -0.5f + wy,  0.5f + wz, 0f, 0f, 1f, 0f, 0f, 1f, Tx(0.0f, bt), Ty(0.0f, bt), Li(bac)]);
                        }
                        
                        // Left
                        if (!chunk.IsSolid(x - 1, y, z))
                        {
                            var bt = (int)OverrideBlockTypeForFace(block.Type, Side.Left);
                            vertices.AddRange([-0.5f + wx,  0.5f + wy,  0.5f + wz, -1f, 0f, 0f, 1f, 0f, 0f, Tx(1.0f, bt), Ty(1.0f, bt), Li(lef)]);
                            vertices.AddRange([-0.5f + wx,  0.5f + wy, -0.5f + wz, -1f, 0f, 0f, 0f, 1f, 0f, Tx(0.0f, bt), Ty(1.0f, bt), Li(lef)]);
                            vertices.AddRange([-0.5f + wx, -0.5f + wy, -0.5f + wz, -1f, 0f, 0f, 0f, 0f, 1f, Tx(0.0f, bt), Ty(0.0f, bt), Li(lef)]);
                            vertices.AddRange([-0.5f + wx, -0.5f + wy, -0.5f + wz, -1f, 0f, 0f, 1f, 0f, 0f, Tx(0.0f, bt), Ty(0.0f, bt), Li(lef)]);
                            vertices.AddRange([-0.5f + wx, -0.5f + wy,  0.5f + wz, -1f, 0f, 0f, 0f, 1f, 0f, Tx(1.0f, bt), Ty(0.0f, bt), Li(lef)]);
                            vertices.AddRange([-0.5f + wx,  0.5f + wy,  0.5f + wz, -1f, 0f, 0f, 0f, 0f, 1f, Tx(1.0f, bt), Ty(1.0f, bt), Li(lef)]);
                        }
                        
                        // Right
                        if (!chunk.IsSolid(x + 1, y, z))
                        {
                            var bt = (int)OverrideBlockTypeForFace(block.Type, Side.Right);
                            vertices.AddRange([0.5f + wx,  0.5f + wy,  0.5f + wz, 1f, 0f, 0f, 1f, 0f, 0f, Tx(1.0f, bt), Ty(1.0f, bt), Li(rig)]);
                            vertices.AddRange([0.5f + wx,  0.5f + wy, -0.5f + wz, 1f, 0f, 0f, 0f, 1f, 0f, Tx(0.0f, bt), Ty(1.0f, bt), Li(rig)]);
                            vertices.AddRange([0.5f + wx, -0.5f + wy, -0.5f + wz, 1f, 0f, 0f, 0f, 0f, 1f, Tx(0.0f, bt), Ty(0.0f, bt), Li(rig)]);
                            vertices.AddRange([0.5f + wx, -0.5f + wy, -0.5f + wz, 1f, 0f, 0f, 1f, 0f, 0f, Tx(0.0f, bt), Ty(0.0f, bt), Li(rig)]);
                            vertices.AddRange([0.5f + wx, -0.5f + wy,  0.5f + wz, 1f, 0f, 0f, 0f, 1f, 0f, Tx(1.0f, bt), Ty(0.0f, bt), Li(rig)]);
                            vertices.AddRange([0.5f + wx,  0.5f + wy,  0.5f + wz, 1f, 0f, 0f, 0f, 0f, 1f, Tx(1.0f, bt), Ty(1.0f, bt), Li(rig)]);
                        }
                    }
                }
            }
        }
        
        stopwatch.Stop();
        Console.WriteLine($"Generated mesh for chunk {chunk.Position.X},{chunk.Position.Z} with {vertices.Count} vertices in {stopwatch.ElapsedMilliseconds} milliseconds.");
        
        return new Mesh
        {
            Vertices = vertices.ToArray(),
        };
    }

    public bool IsLoaded = false;

    public const int VertexBufferCount = 12;

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
        
            // Position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VertexBufferCount * sizeof(float), 0);
        
            // Normal
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, VertexBufferCount * sizeof(float), 3 * sizeof(float));
        
            // Barycentric
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, VertexBufferCount * sizeof(float), 6 * sizeof(float));
            
            // Texture UV
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, VertexBufferCount * sizeof(float), 9 * sizeof(float));
            
            // Light Level
            GL.EnableVertexAttribArray(4);
            GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, VertexBufferCount * sizeof(float), 11 * sizeof(float));
        }
    }

    private static object glLock = false;
    
    private const int atlasBlockSizeInPixels = 16;
    private const int atlasWidthInBlocks = 16;
    private const int atlasHeightInBlocks = 16;

    private static float Tx(float x, int blockId)
    {
        var offset = blockId % atlasWidthInBlocks;
        return (x + offset) / atlasBlockSizeInPixels;
    }

    private static float Ty(float y, int blockId)
    {
        var offset = blockId / atlasHeightInBlocks;
        return (y - offset - 1.0f) / atlasBlockSizeInPixels;
    }

    private static float Li(Block block)
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