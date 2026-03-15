using UnityEngine;

public readonly struct WorldClickContext
{
    public WorldClickContext(Camera camera, Vector2 screenPosition, RaycastHit hit)
    {
        Camera = camera;
        ScreenPosition = screenPosition;
        Hit = hit;
    }

    public Camera Camera { get; }
    public Vector2 ScreenPosition { get; }
    public RaycastHit Hit { get; }
}
