using OpenTK.Mathematics;

public static class Color
{
    public static Vector3 NormalizedRgb(int r, int g, int b) => new Vector3(r / 255.0f, g / 255.0f, b / 255.0f);
    public static Vector4 NormalizedRgba(int r, int g, int b, int a) => new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
}