// Game-owned boundary. Third-party types and dynamic method names must stay in Integration.
using Godot;

public interface ITerrainService
{
    bool IsAvailable { get; }
    string Status { get; }
    void BindViewer(Node3D target);
    bool TryFindGround(Vector2 position, float top, float bottom, out Vector3 ground);
}
