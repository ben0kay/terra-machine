# PineTree002 — optional LOD3 for ultra-distant forests

**Asset-only addition.** The existing PineTree002 LOD1/LOD2 files, material settings, Godot scene, collisions, populations, and terrain remain **unchanged**.

## Current reference and design goal

The owner baseline in `ASSETS/Vegetation/VISUAL_AUTHORING_GUIDE.md` is **4,000–6,000 triangles for normal full-tree LOD1** and **1,000–1,500 for LOD2**. The current `PineTree002/README.md` specifies **LOD1 = 4,962** triangles and **LOD2 = 1,162** triangles. This ultra-distance experiment goes well below those budgets to preserve a readable green pine forest from hundreds of metres away.

- **LOD3 = 120 triangles total:** 20 trunk triangles (tapered 5-sided trunk, 2 spans) + **100 foliage triangles from 50 large textured cards**.
- **Height:** 22.64 m (ground skirt start y=-0.19 to crown tip y=22.45); matching approximate height/proportions of the current PineTree002.
- **Width / depth:** ~7.85 m / 8.12 m (roughly the current crown bounds, dependent on view angle).
- **Materials:** 2 surfaces: original bark colour PNG (simplified, no far-distance normal/roughness sampling); one NEW 1K macro-foliage RGBA texture.
- **Canopy:** 11 irregularly rotated tiers, dark cool-green bushy pine-like needle masses with broad alpha-covered cards. Not intended to read as individual needles or branchwork close up.
- **UV orientation, vertex normals and triangle winding:** checked; foliage is two-sided masked alpha (glTF alpha cutoff **0.35**). Bark sampler repeats; leaf sampler clamps to atlas tiles.
- **Model pivot:** original ground/trunk origin, Y up, metre scale.

## Files supplied

```text
PineTree002/
  Models/
    pine_tree_002_a_LOD3.gltf
    pine_tree_002_a_LOD3.bin
  Textures/
    pine_bark_diff_1k.png                 # existing shared bark image, identical copy
    pine_needles_lod3_macro_1k.png       # NEW distance-specific foliage atlas
  README_LOD3.md
  Previews/
    PineTree002_LOD3_Comparison.png
    PineTree002_LOD3_Standalone.png
```

Extract the `PineTree002/` folder **into** `ASSETS/Vegetation/Trees/`, merging it with the existing `PineTree002` directory. It adds the LOD3 files and its new atlas without overwriting LOD1 or LOD2. For a temporary stand-alone preview, the bark PNG is included so the LOD3 glTF opens outside the game as well.

### Godot integration — intentionally NOT changed here

The current `WORLD/Vegetation/LOD/AuthoredTreeLod.cs` has **two slots only** (`NearModel`, `FarModel`) and the current `PineTree002.tscn` uses native 0–70 m / 70–280 m visibility ranges. Merely adding `LOD3` files will **not** activate them.

A later runtime integration pass should support three authored LODs while retaining original LOD1, LOD2, their native hysteresis, union AABB, collision, debug controls and instance transforms. Suggested *starting trial* ranges: LOD1 0–70 m, LOD2 70–260/280 m, LOD3 ~260/280–600 m (tune by apparent screen size, scene density and frame time). Consider **no shadow casting for LOD3** where appropriate, native visibility ranges and sensible fade/cross-transition behaviour to avoid noticeable popping. Do not increase total forest instance count or visibility distance without measuring cost; triangle count is only one part of GPU load.

### Foliage import and lighting validation

1. In Godot, ensure `pine_needles_lod3_macro_1k.png` imports with mipmap generation enabled. Consider **Preserve Alpha Test Coverage** for this masked foliage and inspect alpha behaviour as the model becomes small; do not suppress mipmaps to gain apparent coverage.
2. Inspect the **same tree** with LOD2/LOD3 forced at approximately 150, 250, 300, 400 and 550 m; check crown fullness, tree colour, silhouette switching, texel shimmer, shadow popping and wind/lighting if any custom material override is applied.
3. View with sunlight behind camera, behind tree and to both sides. The model is not unshaded and does not rely on emission. Original near tree light response may not perfectly match ultra-coarse foliage.
4. A 120-triangle tree is intentionally **too crude at close range**. If its silhouette pops too strongly around 260–280 m, adjust the transition distance or author a slightly heavier 200–300-triangle bridge LOD rather than overusing fine, disappearing needle cards.

## Validation / provenance

- Authored using the existing PineTree002 **4,962-triangle** source (the current near LOD is byte-copied from the earlier lush-distance model as documented in the repo), plus the current repository's PineTree002 README, glTF metadata and visual authoring guide.
- Confirmed 120 total index triangles across two glTF primitives, with 50 coarse foliage cards.
- Tested the glTF in a desktop offscreen renderer from four directions; comparison images are **not Godot screenshots** and do **not** establish correct runtime performance or full lighting acceptance.
- Validated local binary buffer lengths and texture paths. Bark PNG reused unchanged; the macro-foliage atlas is newly constructed for this distant-only mesh.
- No changes to gameplay, world population, camera, density, shadow parameters, placement or run-time scripts.
