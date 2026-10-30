// Optional game-owned excavation boundary; no native addon types escape Integration.
using Godot;

public interface ITerrainExcavation
{
    event System.Action<Vector3, float>? Excavated;
    bool TryExcavate(Vector3 origin, Vector3 direction, float reach, float radius, float inset, out string feedback);
}
