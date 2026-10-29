// Optional game-owned rendering controls. Collision and streaming remain independent.
public interface ITerrainRenderDebug
{
    void SetDebugRendering(bool visible, bool textured, bool normalMaps);
}
