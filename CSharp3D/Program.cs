using System.Diagnostics;
using CSharp3D;
using OpenTK.Graphics.OpenGLES2;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

// Implement ImGuiNet too!

using var game = new Game(800, 600, "Hello World!");
game.Run();

public class Game : GameWindow
{
    private int _vertexBufferObject;

    private int _vertexArrayObject;

    private int _elementBufferObject;
    
    private Shader _shader;
    
    private Texture _texture;

    private readonly Stopwatch _timer = Stopwatch.StartNew();
    
    private readonly float[] _vertices =
    [
        //Position          Texture coordinates
        0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
        0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
        -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
        -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
    ];
    
    private readonly uint[] _indices =
    [ 
        0, 1, 3,   // first triangle
        1, 2, 3    // second triangle
    ];
    
    private readonly float[] _texCoords =
    [
        0.0f, 0.0f,  // lower-left corner  
        1.0f, 0.0f,  // lower-right corner
        0.5f, 1.0f   // top-center corner
    ];
    
    public Game(int width, int height, string title)
        : base(GameWindowSettings.Default, new NativeWindowSettings 
            { ClientSize = (width, height) })
    {
        Console.WriteLine("Initializing game.");
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _shader.Use();
        GL.BindVertexArray(_vertexArrayObject);
        // GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        
        // double time = _timer.Elapsed.TotalSeconds;
        // float green = (float)Math.Sin(time) / 2.0f + 0.5f;
        // int vertexColorLocation = _shader.GetUniformLocation("vertexColor");
        // GL.Uniform4f(vertexColorLocation, 0.0f, green, 0.0f, 1.0f);
        
        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
        Console.WriteLine($"Resized window to ({e.Width}, {e.Height}).");
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        Console.WriteLine("Initializing OpenGL.");
        
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsage.StaticDraw);

        _elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsage.StaticDraw);
        
        _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
        _shader.Use();
        
        var vertexLocation = _shader.GetAttributeLocation("aPosition");
        GL.EnableVertexAttribArray((uint)vertexLocation);
        GL.VertexAttribPointer((uint)vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        
        var texCoordLocation = _shader.GetAttributeLocation("aTexCoord");
        GL.EnableVertexAttribArray((uint)texCoordLocation);
        GL.VertexAttribPointer((uint)texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        
        _texture = new Texture("Textures/wall.jpg");
        _texture.Use(TextureUnit.Texture0);
    }

    protected override void OnUnload()
    {
        Console.WriteLine("Unloading OpenGL.");
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);
        _shader.Dispose();
        base.OnUnload();
    }
}