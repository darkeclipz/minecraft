using System.Diagnostics;
using CSharp3D;
using OpenTK.Graphics.OpenGLES2;
using OpenTK.Mathematics;
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

    private Camera _camera;

    private int _width = 800;

    private int _height = 600;

    private Vector2 _lastMousePosition = Vector2.Zero;

    private bool _firstMove = true;
    
    private readonly float[] _vertices =
    [
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
        0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
        0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
        0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
        0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
        0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

        -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

        0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
        0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
        0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
        0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
        0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
        0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
        0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
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
        _width = width;
        _height = height;
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _shader.Use();
        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        // GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        
        SwapBuffers();
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        if (IsFocused)
        {
            // MousePosition = (e.X + _width / 2f, e.Y + _height / 2f);
        }
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        var input = KeyboardState;
        var dt = (float)e.Time;

        if (input.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        if (input.IsKeyDown(Keys.W))
        {
            _camera.Position += _camera.Front * _camera.Speed * dt;
        }

        if (input.IsKeyDown(Keys.S))
        {
            _camera.Position -= _camera.Front * _camera.Speed * dt;
        }

        if (input.IsKeyDown(Keys.A))
        {
            _camera.Position -= Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * _camera.Speed * dt;
        }
        
        if (input.IsKeyDown(Keys.D))
        {
            _camera.Position += Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * _camera.Speed * dt;
        }

        if (input.IsKeyDown(Keys.Space))
        {
            _camera.Position += _camera.Up * _camera.Speed * dt;
        }

        if (input.IsKeyDown(Keys.LeftShift))
        {
            _camera.Position -= _camera.Up * _camera.Speed * dt;
        }

        if (input.IsKeyReleased(Keys.LeftAlt))
        {
            CursorState = CursorState == CursorState.Grabbed ? CursorState.Normal : CursorState.Grabbed;
            _lastMousePosition = MousePosition;
        }

        if (_firstMove)
        {
            _firstMove = false;
            _lastMousePosition = MousePosition;
        }
        else if (CursorState == CursorState.Grabbed)
        {
            var deltaX = MousePosition.X - _lastMousePosition.X;
            var deltaY = MousePosition.Y - _lastMousePosition.Y;
            _lastMousePosition = MousePosition;
            _camera.Yaw += deltaX * _camera.Sensitivity;
            _camera.Pitch = Math.Clamp(_camera.Pitch - deltaY * _camera.Sensitivity, -89, 89);
        }
        
        var cameraFrontX = (float)Math.Cos(MathHelper.DegreesToRadians(_camera.Pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(_camera.Yaw));
        var cameraFrontY = (float)Math.Sin(MathHelper.DegreesToRadians(_camera.Pitch));
        var cameraFrontZ = (float)Math.Cos(MathHelper.DegreesToRadians(_camera.Pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(_camera.Yaw));
        _camera.Front = Vector3.Normalize(new Vector3(cameraFrontX, cameraFrontY, cameraFrontZ));

        // Update the model.
        var time = (float)_timer.Elapsed.TotalSeconds;
        var rotX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-55.0f) + 0.5f * time);
        var rotZ = Matrix4.CreateRotationZ(time);
        var rotation = rotX * rotZ;
        
        _shader.SetMatrix4("model", ref rotation);
        
        var blockIdLoc = _shader.GetUniformLocation("blockId");
        GL.Uniform1i(blockIdLoc, (int)(time % 5.0));
        
        // Update block.
        
        // Update the camera.
        Matrix4 view = _camera.GetViewMatrix();
        _shader.SetMatrix4("view", ref view);
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        _width = e.Width;
        _height = e.Height;
        GL.Viewport(0, 0, e.Width, e.Height);
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)_width / _height, 0.1f, 100.0f);
        _shader.SetMatrix4("projection", ref projection);
        Console.WriteLine($"Resized window to ({e.Width}, {e.Height}).");
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        Console.WriteLine("Initializing OpenGL.");
        
        GL.ClearColor(135f / 255f, 206f / 255f, 245f / 255f, 1f);

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
        
        var blockIdLoc = _shader.GetUniformLocation("blockId");
        GL.Uniform1i(blockIdLoc, 0);
        
        _texture = new Texture("Textures/tile_atlas.png");
        _texture.Use(TextureUnit.Texture0);

        _camera = new Camera();

        Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-55.0f));
        Matrix4 view = _camera.GetViewMatrix();
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)_width / _height, 0.1f, 100.0f);
        
        _shader.SetMatrix4("model", ref model);
        _shader.SetMatrix4("view", ref view);
        _shader.SetMatrix4("projection", ref projection);

        GL.Enable(EnableCap.DepthTest);

        CursorState = CursorState.Grabbed;
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
        _texture.Dispose();
        base.OnUnload();
    }
}