# Forest meadow scatter

`ForestMeadowMaster.tres` configures an independent GrassScatter instance for the flat-side pine forest. It selects three matching near/far authored meadow clumps, plus `ASSETS/Vegetation/Grass/GrassMeadow001/Materials/ForestMeadowWind.tres`. The forest area centre and radii are supplied at runtime from the existing `ForestPreview.Settings`, so moving or resizing the forest also moves its meadow layer. No additional world-biome owner is introduced.

World-anchored clumps now use jittered centres roughly 11 m apart, with 2.6 m nominal radii and varied patch size. Samples outside these patches are rejected before terrain queries; denser interiors leave open gaps between bunches. PatchSpacing = 0 disables this mode. Bermuda GrassMaster remains unchanged.

Defaults: 16 m tiles, 1.3 candidate clumps/m² before patch/slope rejection, 0.78–1.15 scale, fade beginning 40 m, total draw ending around 65 m, tile density thinning from 13 to 40 m (to 18%), authored mesh LOD switching around 32 m. Both detailed and simplified meshes remain in memory per tile. The renderer handles switching using tile-native visibility ranges, and shared shader wind remains GPU-side. The 4Hz tile check remains active only while this scatter is visible; physics sleeps between tile placement jobs. The original anchored MeadowPreview and Bermuda GrassMaster settings are independent.

Compare foliage via F1 → OTHER MODELS → `Visible: ForestMeadowScatter`, and inspect Godot renderer draw calls/primitive counts/F2 frame time with it on/off. Godot and C# compilation / visual performance testing are pending locally.
