using OpenTK.Graphics.OpenGLES2;
using StbImageSharp;

namespace CSharp3D;

public class Texture : IDisposable
{
    private int _handle;
    private bool _disposed;

    public Texture(string filePath)
    {
        _handle = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2d, _handle);
        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult image = ImageResult.FromStream(File.OpenRead(filePath), ColorComponents.RedGreenBlueAlpha);
        GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.GenerateMipmap(TextureTarget.Texture2d);
    }
    
    public void Use(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2d, _handle);        
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;
        if (_disposed) return;
        _disposed = true;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        GL.DeleteTexture(_handle);
    }
    
    ~Texture()
    {
        if (!_disposed)
        {
            Console.WriteLine($"GPU leak: texture is not disposed for texture handle {_handle}.");
        }
    }
}