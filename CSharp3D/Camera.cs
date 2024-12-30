using OpenTK.Mathematics;

namespace CSharp3D;

public class Camera
{
    public Vector3 Position { get; set; } = new(0, 115, 0);
    public Vector3 Front { get; set; } = new(0.0f, 0.0f, -1.0f);
    public Vector3 Up { get; set; } = Vector3.UnitY;
    public float Speed { get; } = 30.0f;
    public float SprintMultiplier { get; } = 5f;
    public float Sensitivity { get; } = 0.05f;
    public float Pitch { get; set; } = 0f;
    public float Yaw { get; set; } = 0f;
    public float FieldOfView { get; set; } = 70f;
    public int RenderDistance { get; set; } = 11;

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + Front, Up);
    }
}
