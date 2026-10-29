// Optional game-owned surface query for vegetation placement; no addon types.
using Godot;
public interface ITerrainSurfaceService
{
    bool TryFindSurface(Vector2 position, float top, float bottom, out Vector3 ground, out Vector3 normal);
}
