using OpenTK.Graphics.OpenGLES2;
using OpenTK.Mathematics;

namespace CSharp3D;

public class Shader : IDisposable
{
    private readonly int _handle;
    private bool _disposed;

    public Shader(string vertexPath, string fragmentPath)
    {
        // Compile vertex shader.
        Console.WriteLine($"Loading {vertexPath}.");
        
        var vertexShaderSource = File.ReadAllText(vertexPath);
        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        
        {
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            GL.GetShaderi(vertexShader, ShaderParameterName.CompileStatus, out var success);
            if (success == 0)
            {
                GL.GetShaderInfoLog(vertexShader, out var info);
                Console.WriteLine(info);
            }
        }
        
        // Compile fragment shader.
        Console.WriteLine($"Loading {fragmentPath}.");
        
        var fragmentShaderSource = File.ReadAllText(fragmentPath);
        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        
        {
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            GL.GetShaderi(fragmentShader, ShaderParameterName.CompileStatus, out var success);
            if (success == 0)
            {
                GL.GetShaderInfoLog(fragmentShader, out var info);
                Console.WriteLine(info);
            }
        }

        // Link shaders.
        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, vertexShader);
        GL.AttachShader(_handle, fragmentShader);

        GL.LinkProgram(_handle);

        {
            GL.GetProgrami(_handle, ProgramProperty.LinkStatus, out var success);
            if (success == 0)
            {
                GL.GetProgramInfoLog(_handle, out var info);
                Console.WriteLine(info);
            }
        }

        // Clean up.
        GL.DetachShader(_handle, vertexShader);
        GL.DetachShader(_handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    public void Use()
    {
        GL.UseProgram(_handle);
    }

    public int GetUniformLocation(string uniform)
    {
        return GL.GetUniformLocation(_handle, uniform);
    }

    public int GetAttributeLocation(string attribute)
    {
        return GL.GetAttribLocation(_handle, attribute);
    }

    public void SetMatrix4(string name, ref Matrix4 matrix)
    {
        var loc = GetUniformLocation(name);
        GL.UniformMatrix4f(loc, 1, true, ref matrix);
    }

    public void SetVector3(string name, Vector3 vector)
    {
        var loc = GetUniformLocation(name);
        GL.Uniform3f(loc, vector.X, vector.Y, vector.Z);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        if (_disposed) return;
        GL.DeleteProgram(_handle);
        _disposed = true;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Shader()
    {
        if (!_disposed)
        {
            Console.WriteLine($"GPU leak: shader is not disposed for shader handle {_handle}.");
        }
    }
}