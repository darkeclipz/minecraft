using OpenTK.Graphics.OpenGLES2;

namespace CSharp3D;

public class Mesh
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
        
        List<float> vertices = [];

        for (var y = Chunk.Dimensions.Y - 2; y >= 0; y--) // Assuming we cannot do anything with the top layer.
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
                        vertices.AddRange([-0.5f + worldX,  0.5f + worldY, -0.5f + worldZ,  0.0f, 1.0f]);
                        vertices.AddRange([0.5f + worldX,  0.5f + worldY, -0.5f + worldZ,  1.0f, 1.0f]);
                        vertices.AddRange([0.5f + worldX,  0.5f + worldY,  0.5f + worldZ,  1.0f, 0.0f]);
                        vertices.AddRange([0.5f + worldX,  0.5f + worldY,  0.5f + worldZ,  1.0f, 0.0f]);
                        vertices.AddRange([-0.5f + worldX,  0.5f + worldY,  0.5f + worldZ,  0.0f, 0.0f]);
                        vertices.AddRange([-0.5f + worldX,  0.5f + worldY, -0.5f + worldZ,  0.0f, 1.0f]);
                        
                        // Back
                        vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ,  0.0f, 0.0f]);
                        vertices.AddRange([0.5f + worldX, -0.5f + worldY, -0.5f + worldZ,  1.0f, 0.0f]);
                        vertices.AddRange([0.5f + worldX,  0.5f + worldY, -0.5f + worldZ,  1.0f, 1.0f]);
                        vertices.AddRange([0.5f + worldX,  0.5f + worldY, -0.5f + worldZ,  1.0f, 1.0f]);
                        vertices.AddRange([-0.5f + worldX,  0.5f + worldY, -0.5f + worldZ,  0.0f, 1.0f]);
                        vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ,  0.0f, 0.0f]);
                        
                        // Front
                        vertices.AddRange([-0.5f + worldX, -0.5f + worldY,  0.5f + worldZ,  0.0f, 0.0f]);
                        vertices.AddRange([0.5f + worldX, -0.5f + worldY,  0.5f + worldZ,  1.0f, 0.0f]);
                        vertices.AddRange([0.5f + worldX,  0.5f + worldY,  0.5f + worldZ,  1.0f, 1.0f]);
                        vertices.AddRange([0.5f + worldX,  0.5f + worldY,  0.5f + worldZ,  1.0f, 1.0f]);
                        vertices.AddRange([-0.5f + worldX,  0.5f + worldY,  0.5f + worldZ,  0.0f, 1.0f]);
                        vertices.AddRange([-0.5f + worldX, -0.5f + worldY,  0.5f + worldZ,  0.0f, 0.0f]);
                        
                        // Left
                        vertices.AddRange([-0.5f + worldX,  0.5f + worldY,  0.5f + worldZ,  1.0f, 0.0f]);
                        vertices.AddRange([-0.5f + worldX,  0.5f + worldY, -0.5f + worldZ,  1.0f, 1.0f]);
                        vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ,  0.0f, 1.0f]);
                        vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ,  0.0f, 1.0f]);
                        vertices.AddRange([-0.5f + worldX, -0.5f + worldY,  0.5f + worldZ,  0.0f, 0.0f]);
                        vertices.AddRange([-0.5f + worldX,  0.5f + worldY,  0.5f + worldZ,  1.0f, 0.0f]);
                        
                        // Right
                        vertices.AddRange([0.5f + worldX,  0.5f + worldY,  0.5f + worldZ,  1.0f, 0.0f]);
                        vertices.AddRange([0.5f + worldX,  0.5f + worldY, -0.5f + worldZ,  1.0f, 1.0f]);
                        vertices.AddRange([0.5f + worldX, -0.5f + worldY, -0.5f + worldZ,  0.0f, 1.0f]);
                        vertices.AddRange([0.5f + worldX, -0.5f + worldY, -0.5f + worldZ,  0.0f, 1.0f]);
                        vertices.AddRange([0.5f + worldX, -0.5f + worldY,  0.5f + worldZ,  0.0f, 0.0f]);
                        vertices.AddRange([0.5f + worldX,  0.5f + worldY,  0.5f + worldZ,  1.0f, 0.0f]);
                        
                        // Bottom
                        vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ,  0.0f, 1.0f]);
                        vertices.AddRange([0.5f + worldX, -0.5f + worldY, -0.5f + worldZ,  1.0f, 1.0f]);
                        vertices.AddRange([0.5f + worldX, -0.5f + worldY,  0.5f + worldZ,  1.0f, 0.0f]);
                        vertices.AddRange([0.5f + worldX, -0.5f + worldY,  0.5f + worldZ,  1.0f, 0.0f]);
                        vertices.AddRange([-0.5f + worldX, -0.5f + worldY,  0.5f + worldZ,  0.0f, 0.0f]);
                        vertices.AddRange([-0.5f + worldX, -0.5f + worldY, -0.5f + worldZ,  0.0f, 1.0f]);
                    }
                }
            }
        }

        var vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);
        
        var vbo = GL.GenBuffer();

        var mesh = new Mesh
        {
            Vertices = vertices.ToArray(),
            VertexArrayObject = vao,
            VertexBufferObject = vbo,
        };
        
        Console.WriteLine($"Generated mesh with VAO {vao} and VBO {vbo} with a total of {vertices.Count} vertices.");
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, mesh.Vertices.Length * sizeof(float), mesh.Vertices, BufferUsage.StaticDraw);
        
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

        return mesh;
    }
}