// Optional terrain presentation contract: bind forest-floor colour to the composed forest area.
using Godot;

public interface ITerrainForestFloor
{
    void SetForestFloorArea(Vector2 center, Vector2 radii);
}
