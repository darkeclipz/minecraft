using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using CSharp3D;
using OpenTK.Graphics.OpenGLES2;
using OpenTK.Graphics.Vulkan;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

// Implement ImGuiNet too!

using var game = new Game(1920, 1080, "Hello World!");
game.VSync = VSyncMode.Off;
game.Run();

public class Game : GameWindow
{
    private int _vertexBufferObject;

    private int _vertexArrayObject;

    private int _elementBufferObject;
    
    private Shader _shader;
    
    private Texture _texture;

    private readonly Stopwatch _timer = Stopwatch.StartNew();

    private readonly System.Timers.Timer _fpsTimer = new();
    private int _frameCount = 0;

    private Camera _camera;

    private int _width = 800;

    private int _height = 600;

    private Vector2 _lastMousePosition = Vector2.Zero;

    private bool _firstMove = true;

    private Chunk _chunk;

    private World _world;
    //
    // private float[] _vertices =
    // [
    //     -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
    //     0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
    //     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
    //     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
    //     -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
    //     -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
    //
    //     -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
    //     0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
    //     0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
    //     0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
    //     -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
    //     -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
    //
    //     -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
    //     -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
    //     -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
    //     -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
    //     -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
    //     -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
    //
    //     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
    //     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
    //     0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
    //     0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
    //     0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
    //     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
    //
    //     -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
    //     0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
    //     0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
    //     0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
    //     -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
    //     -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
    //
    //     -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
    //     0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
    //     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
    //     0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
    //     -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
    //     -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
    // ];
    //
    // private readonly uint[] _indices =
    // [ 
    //     0, 1, 3,   // first triangle
    //     1, 2, 3    // second triangle
    // ];
    //
    // private readonly float[] _texCoords =
    // [
    //     0.0f, 0.0f,  // lower-left corner  
    //     1.0f, 0.0f,  // lower-right corner
    //     0.5f, 1.0f   // top-center corner
    // ];
    
    public Game(int width, int height, string title)
        : base(GameWindowSettings.Default, new NativeWindowSettings 
            { ClientSize = (width, height) })
    {
        Console.WriteLine("Initializing game.");
        _width = width;
        _height = height;
        
        _fpsTimer.AutoReset = true;
        _fpsTimer.Interval = 10_000;

        _fpsTimer.Elapsed += (_, _) =>
        {
            var averageFramesPerSecond = _frameCount / 10d;
            Console.WriteLine($"Avg FPS: {averageFramesPerSecond}");
            _frameCount = 0;
        };
        
        _fpsTimer.Start();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _shader.Use();
        
        foreach (var chunk in _world.Chunks)
        {
            if (!chunk.IsLoaded) continue;
            
            chunk.Mesh.Use();
            
            GL.BindVertexArray(chunk.Mesh.VertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, chunk.Mesh.Vertices.Length / 8);    
        }
        
        // GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        
        SwapBuffers();

        _frameCount++;
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

        var sprint = 1f;

        if (input.IsKeyDown(Keys.LeftShift))
        {
            sprint = 2f;
        }

        if (input.IsKeyDown(Keys.W))
        {
            _camera.Position += _camera.Front * _camera.Speed * dt * sprint;
        }

        if (input.IsKeyDown(Keys.S))
        {
            _camera.Position -= _camera.Front * _camera.Speed * dt * sprint;
        }

        if (input.IsKeyDown(Keys.A))
        {
            _camera.Position -= Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * _camera.Speed * dt * sprint;
        }
        
        if (input.IsKeyDown(Keys.D))
        {
            _camera.Position += Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * _camera.Speed * dt * sprint;
        }

        if (input.IsKeyDown(Keys.Space))
        {
            _camera.Position += _camera.Up * _camera.Speed * dt * sprint;
        }

        if (input.IsKeyDown(Keys.LeftControl))
        {
            _camera.Position -= _camera.Up * _camera.Speed * dt * sprint;
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

        _shader.SetVector3("camPos", _camera.Position);

        // Update the model.
        // var time = (float)_timer.Elapsed.TotalSeconds;
        // var rotX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0));
        // var rotZ = Matrix4.CreateRotationZ(0);
        // var rotation = rotX * rotZ;
        //
        // _shader.SetMatrix4("model", ref rotation);
        
        // var blockIdLoc = _shader.GetUniformLocation("blockId");
        // const int numberOfBlocks = 11;
        // GL.Uniform1i(blockIdLoc, 1);
        
        // Update block.
        
        // Update the camera.
        Matrix4 view = _camera.GetViewMatrix();
        _shader.SetMatrix4("view", ref view);
        
        LimitFps(dt);
    }

    void LimitFps(double deltaTime)
    {
        const int maxFramesPerSecond = 60;
        var millisecondsLeftInWindow = (int)(1000d / maxFramesPerSecond - deltaTime);
        
        if (millisecondsLeftInWindow > 0)
        {
            Thread.Sleep(millisecondsLeftInWindow);
        }
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
        
        Console.WriteLine("Generating world...");
        _world = new World(renderDistance: 30);

        // _vertices = _chunk.Mesh.Vertices;

        // _vertexArrayObject = GL.GenVertexArray();
        // GL.BindVertexArray(_vertexArrayObject);
        //
        // _vertexBufferObject = GL.GenBuffer();
        // GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        // GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsage.StaticDraw);
        //
        // _elementBufferObject = GL.GenBuffer();
        // GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        // GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsage.StaticDraw);
        //
        _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
        _shader.Use();
        
        //
        // var vertexLocation = _shader.GetAttributeLocation("aPosition");
        // GL.EnableVertexAttribArray((uint)vertexLocation);
        // GL.VertexAttribPointer((uint)vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        //
        // var texCoordLocation = _shader.GetAttributeLocation("aTexCoord");
        // GL.EnableVertexAttribArray((uint)texCoordLocation);
        // GL.VertexAttribPointer((uint)texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        //
        // var blockIdLoc = _shader.GetUniformLocation("blockId");
        // GL.Uniform1i(blockIdLoc, 0);
        
        _texture = new Texture("Textures/tile_atlas.png");
        _texture.Use(TextureUnit.Texture0);

        _camera = new Camera
        {
            Position = new Vector3(-20, 115, -20),
            Yaw = 45f,
            Pitch = -15f,
        };

        Matrix4 model = Matrix4.CreateRotationX(0f);
        Matrix4 view = _camera.GetViewMatrix();
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)_width / _height, 0.1f, 1000.0f);
        
        _shader.SetMatrix4("model", ref model);
        _shader.SetMatrix4("view", ref view);
        _shader.SetMatrix4("projection", ref projection);

        GL.Enable(EnableCap.DepthTest);
        
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        GL.Enable(EnableCap.Blend);

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