using OpenTK.Mathematics;

namespace CSharp3D;

public class AABB
{
    public required Vector3 Position { get; init; }
    
    public required Vector3 Size { get; init; }

    public bool IntersectsWith(Vector3 point)
    {
        if (point.X < Position.X || point.X > Position.X + Size.X) return false;
        if (point.Y < Position.Y || point.Y > Position.Y + Size.Y) return false;
        if (point.Z < Position.Z || point.Z > Position.Z + Size.Z) return false;
        return true;
    }
}