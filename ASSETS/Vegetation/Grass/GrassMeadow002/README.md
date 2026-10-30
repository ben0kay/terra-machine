# Grass Meadow 002

Three authored grass-clump variants A/B/C. `Models/` contains individual LOD1 and LOD2 glTF exports for each, with shared 1K color/alpha texture in `Textures/` and GPU wind in `Materials/`. LOD1 triangles per tuft: **60 / 66 / 54**; LOD2 triangles: **24 / 30 / 24**. Native heights are roughly 0.75–0.9 m. Do not confuse A/B/C appearance variations with LOD levels.

Two independent uses in the test world:

- `WORLD/Vegetation/MeadowPreview/Configuration/SpawnMeadow002Master.tres`: original modest bunches near 12 starting firs, scale 0.88–1.13, original `Materials/MeadowGrass002Wind.tres`, no authored far-LOD switching.
- `WORLD/Vegetation/GrassScatter/Configuration/ForestMeadowMaster.tres`: flat-side forest clumps, scale 0.75–0.95 for 85% of tufts with 15% tall accents at 1.30–1.65, density 0.75 candidates/m² and 1.4 m minimum root clearance within each tile, paired LOD1/2 with a switch around 24 m. Uses dedicated `Materials/ForestMeadow002Wind.tres` with opt-in far-distance fade; original spawn material is unaffected.

Both use the same shader source, with distance fade disabled by default so other materials opt in explicitly. Neither requires grass collision or per-tuft C# updates. The existing GrassMedium001 exports are retained as inactive art sources for optional future use. In-game import/shader compile, appearance and FPS await local verification.
