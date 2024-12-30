using OpenTK.Mathematics;

namespace CSharp3D;

public class Camera
{
    public Vector3 Position { get; set; } = new(0, 115, 0);
    public Vector3 Front { get; set; } = new(0.0f, 0.0f, -1.0f);
    public Vector3 Up { get; set; } = Vector3.UnitY;
    public float Speed { get; } = 15.0f;
    public float SprintMultiplier { get; } = 10f;
    public float Sensitivity { get; } = 0.05f;
    public float Pitch { get; set; } = 0f;
    public float Yaw { get; set; } = 0f;
    public float FieldOfView { get; set; } = 70f;
    public int RenderDistance { get; set; } = 11;
    public Vector3 Velocity { get; set; } = Vector3.Zero;
    public Vector3 Acceleration { get; set; } = Vector3.Zero;

    public AABB BoundingBox { get; set; } = new AABB
    {
        Position = new Vector3(-0.5f, 0.2f, -0.5f),
        Size = new Vector3(1f, 2f, 1f),
    };

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + Front, Up);
    }
}
