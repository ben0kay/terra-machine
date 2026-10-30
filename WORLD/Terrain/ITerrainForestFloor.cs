// Optional terrain presentation contract: bind forest-floor colour to the composed forest area.
using Godot;

public interface ITerrainForestFloor
{
    void SetForestGrassPatches(float spacing, float radius, int seed, float maximumSlopeDegrees);
    void SetForestFloorArea(Vector2 center, Vector2 radii);
}
