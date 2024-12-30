using System.Collections.Concurrent;
using System.Diagnostics;
using CSharp3D;
using Microsoft.Extensions.Configuration;
using OpenTK.Graphics.OpenGLES2;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Game : GameWindow
{
    private Camera _camera;
    
    private Shader _defaultShader;

    private Shader _wireframeShader;

    private Shader _activeShader;
    
    private World _world;
    
    private Texture _texture;
    
    private readonly System.Timers.Timer _fpsTimer = new();
    
    private int _frameCount = 0;

    private int _width = 1;

    private int _height = 1;

    private Vector2 _lastMousePosition = Vector2.Zero;

    private bool _firstMove = true;

    private static CancellationTokenSource CancellationTokenSource = new();

    public static readonly int WorldSeed = (int)Random.Shared.NextInt64();
    
    public static CancellationToken CancellationToken { get; private set; }

    private IConfiguration _configuration;

    private bool _enableFog = true;

    public static bool EnableChunkLoader = true;

    private bool _enableLight = true;

    public static ConcurrentQueue<Mesh> MeshDisposeQueue = [];
    
    public static ConcurrentQueue<Chunk> ChunkDisposeQueue = [];
    
    public Game(int width, int height, IConfiguration configuration)
        : base(GameWindowSettings.Default, new NativeWindowSettings 
            { ClientSize = (width, height) })
    {
        Console.WriteLine("Initializing game.");
        _width = width;
        _height = height;
        _configuration = configuration;
        
        InitFpsCounter();
        
        CancellationToken = CancellationTokenSource.Token;
    }

    private void InitFpsCounter()
    {
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
        Stopwatch sw = Stopwatch.StartNew();
        
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _activeShader.Use();

        int loadedChunks = 0;
        const int loadMaxChunksPerPass = 16;

        var chunks = _world.GetChunksSortedByDistanceDescending(_camera.Position);
        
        foreach (var chunk in chunks)
        {
            if (!chunk.IsLoaded) continue;
            if (!chunk.Mesh.IsLoaded) continue;
            
            _activeShader.SetFloat("opacity", chunk.GetOpacity());
            
            GL.BindVertexArray(chunk.Mesh.VertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, chunk.Mesh.Vertices.Length / Mesh.VertexBufferCount);
        }
        
        foreach (var chunk in chunks)
        {
            if (!chunk.IsLoaded) continue;
            if (!chunk.Mesh.IsLoaded) continue;
            
            _activeShader.SetFloat("opacity", chunk.GetOpacity());
            
            GL.BindVertexArray(chunk.TransparentMesh.VertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, chunk.TransparentMesh.Vertices.Length / Mesh.VertexBufferCount);
        }
        
        SwapBuffers();
        _frameCount++;

        if (loadedChunks > 0)
        {
            sw.Stop();
            Console.WriteLine($"Rendered frame (with chunks generated) in {sw.ElapsedMilliseconds} ms.");
        }
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
            CancellationTokenSource.Cancel();
            Close();
        }

        var sprint = 1f;

        if (input.IsKeyDown(Keys.LeftShift))
        {
            sprint = _camera.SprintMultiplier;
        }

        if (input.IsKeyReleased(Keys.R))
        {
            _world.Reload();
        }

        if (input.IsKeyReleased(Keys.C))
        {
            ToggleChunkLoader();
        }

        if (input.IsKeyReleased(Keys.L))
        {
            ToggleLight();
        }

        if (input.IsKeyReleased(Keys.F1))
        {
            UseDefaultShader();
        }
        
        if (input.IsKeyReleased(Keys.F2))
        {
            UseWireframeShader();
        }

        if (input.IsKeyReleased(Keys.D0))
        {
            SetRenderDistance(0);
        }
        
        if (input.IsKeyReleased(Keys.D1))
        {
            SetRenderDistance(1);
        }
        
        if (input.IsKeyReleased(Keys.D2))
        {
            SetRenderDistance(3);
        }
        
        if (input.IsKeyReleased(Keys.D3))
        {
            SetRenderDistance(5);
        }
        
        if (input.IsKeyReleased(Keys.D4))
        {
            SetRenderDistance(8);
        }
        
        if (input.IsKeyReleased(Keys.D5))
        {
            SetRenderDistance(11);
        }
        
        if (input.IsKeyReleased(Keys.D6))
        {
            SetRenderDistance(14);
        }
        
        if (input.IsKeyReleased(Keys.D7))
        {
            SetRenderDistance(17);
        }
        
        if (input.IsKeyReleased(Keys.D8))
        {
            SetRenderDistance(20);
        }
        
        if (input.IsKeyReleased(Keys.D9))
        {
            SetRenderDistance(25);
        }

        if (input.IsKeyReleased(Keys.F))
        {
            ToggleFog();
        }

        if (input.IsKeyDown(Keys.W))
        {
            _camera.Position += _camera.Front * _camera.Speed * dt * sprint;
            // _camera.Velocity = _camera.Front * _camera.Speed;
        }
        else
        {
            // _camera.Velocity = Vector3.Zero;
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
            _camera.Velocity += new Vector3(0, 10f, 0);
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

        _activeShader.SetVector3("camPos", _camera.Position);

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
        _activeShader.SetMatrix4("view", ref view);
        
        // Update world
        bool tooFarFromCurrentChunk = Vector2.DistanceSquared(_camera.Position.Xz, _world.CurrentChunk.Position.Xz) >
                                      World.UpdateDistanceThreshold * World.UpdateDistanceThreshold;
        
        if (EnableChunkLoader && tooFarFromCurrentChunk)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var nearestChunk = _world.GetNearestChunk(_camera.Position.Xz);
            _world.UpdateCurrentChunk(_camera.Position, nearestChunk);
            sw.Stop();
            Console.WriteLine($"Current chunk updated to chunk {_world.CurrentChunk.Position.X}, {_world.CurrentChunk.Position.Z} in {sw.ElapsedMilliseconds} milliseconds.");
        }
        
        if (ChunkDisposeQueue.TryDequeue(out var chunkToRemove))
        {
            chunkToRemove.Dispose();
        }

        if (MeshDisposeQueue.TryDequeue(out var meshToRemove))
        {
            meshToRemove.Dispose();
        }

        int loadedChunks = 0;
        const int loadMaxChunksPerPass = 16;
        
        foreach (var chunk in _world.GetChunksSortedByDistance(_camera.Position))
        {
            if (!chunk.IsLoaded) continue;

            if (!chunk.Mesh.IsLoaded && loadedChunks < loadMaxChunksPerPass)
            {
                chunk.Mesh.Use();
                chunk.TransparentMesh.Use();
                loadedChunks++;
            }
        }
        
        // Physics
        // _camera.Acceleration += World.Gravity;

        // _camera.Velocity += World.Gravity;
        // _camera.Position += _camera.Velocity * dt;
        
        
        
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
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.FieldOfView), (float)_width / _height, 0.1f, 1000.0f);
        _defaultShader.SetMatrix4("projection", ref projection);
        Console.WriteLine($"Resized window to ({e.Width}, {e.Height}).");
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        
        GL.ClearColor(135f / 255f, 206f / 255f, 245f / 255f, 1f);
        
        Console.WriteLine("Generating world...");
        
        _texture = new Texture("Textures/tile_atlas.png");
        _texture.Use(TextureUnit.Texture0);

        _camera = new Camera();
        _world = new World(_camera);

        UseDefaultShader();

        CursorState = CursorState.Grabbed;
    }

    private void UseDefaultShader()
    {
        _defaultShader ??= new Shader("Shaders/shader.vert", "Shaders/shader.frag");
        _defaultShader.Use();
        
        Matrix4 model = Matrix4.CreateRotationX(0f);
        Matrix4 view = _camera.GetViewMatrix();
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.FieldOfView), (float)_width / _height, 0.1f, 1000.0f);
        
        _defaultShader.SetMatrix4("model", ref model);
        _defaultShader.SetMatrix4("view", ref view);
        _defaultShader.SetMatrix4("projection", ref projection);
        _defaultShader.SetFloat("fogDensity", 0.008f);
        _defaultShader.SetFloat("fogNear", (_camera.RenderDistance - 3) * Chunk.Dimensions.X);
        _defaultShader.SetFloat("fogFar", (_camera.RenderDistance + 2) * Chunk.Dimensions.X);
        _defaultShader.SetVector4("fogColor", Color.NormalizedRgba(135, 206, 245, 255));
        _defaultShader.SetInt("enableFog", _enableFog ? 1 : 0);
        _defaultShader.SetInt("enableLight", _enableLight ? 1 : 0);

        _activeShader = _defaultShader;
        
        GL.Enable(EnableCap.DepthTest);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.Blend);
    }

    private void UseWireframeShader()
    {
        _wireframeShader ??= new Shader("Shaders/shader.vert", "Shaders/wireframe.frag");
        _wireframeShader.Use();
        
        Matrix4 model = Matrix4.CreateRotationX(0f);
        Matrix4 view = _camera.GetViewMatrix();
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.FieldOfView), (float)_width / _height, 0.1f, 1000.0f);
        
        _wireframeShader.SetMatrix4("model", ref model);
        _wireframeShader.SetMatrix4("view", ref view);
        _wireframeShader.SetMatrix4("projection", ref projection);
        _wireframeShader.SetFloat("lineThickness", 0.0001f);

        _activeShader = _wireframeShader;
        
        GL.Disable(EnableCap.DepthTest);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.Blend);
    }

    private void ToggleFog()
    {
        if (_enableFog)
        {
            _enableFog = false;
            _activeShader.SetInt("enableFog", 0);
        }
        else
        {
            _enableFog = true;
            _activeShader.SetInt("enableFog", 1);
        }
    }

    private void ToggleLight()
    {
        if (_enableLight)
        {
            _enableLight = false;
            _activeShader.SetInt("enableLight", 0);
        }
        else
        {
            _enableLight = true;
            _activeShader.SetInt("enableLight", 1);
        }
    }

    private void ToggleChunkLoader()
    {
        EnableChunkLoader = !EnableChunkLoader;
    }

    private void SetRenderDistance(int dist)
    {
        _camera.RenderDistance = dist;
        _world.Reset();
        _defaultShader.SetFloat("fogNear", (_camera.RenderDistance - 3) * Chunk.Dimensions.X);
        _defaultShader.SetFloat("fogFar", (_camera.RenderDistance + 2) * Chunk.Dimensions.X);
    }

    protected override void OnUnload()
    {
        Console.WriteLine("Unloading OpenGL.");
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        _defaultShader?.Dispose();
        _wireframeShader?.Dispose();
        _texture.Dispose();
        base.OnUnload();
    }
}