# PineTree002 — fuller near canopy and lightweight distant LOD

The former authored LOD2 is now LOD1: **4,962 triangles** (1,472 bark/wood + 3,490 foliage). Its binary geometry is copied byte-for-byte; only the glTF node/mesh name and buffer URI change to LOD1. Its fuller pine_needles_distance_full_2k.png atlas and alpha cutoff 0.29 are preserved. The previous detailed LOD1 is replaced, and remains recoverable through Git history.

The new authored LOD2 has **1,162 triangles**: 162 for the trunk and 1,000 for foliage. It retains ten nine-sided rings from the source trunk, including its base, bend, taper and bark UVs. Five hundred source foliage cards are selected using deterministic spatial farthest-point sampling (vertical weight 0.62), expanded 1.65× about each card centre and clamped to the source foliage bounds. Cards retain their existing atlas tile, orientation and lighting normals. This trades tiny twigs and wooden branch geometry for broader, separated needle masses distributed throughout the original canopy. It is a distant model; exposed card shapes and reduced branch structure may be noticeable close up.

## Stable scene, materials and collision

PineTree002.tscn and all resource paths remain stable. LOD1 covers 0–70 m, LOD2 70–280 m, with native hysteresis and union bounds; no per-frame tree scripts are added. Existing F1 Auto/forced LOD controls still apply. Both models share the original bark colour/normal/roughness textures and the fuller distance needle atlas. No new materials, textures, draw surfaces, grass settings or forest density changes are introduced.

The shared trunk capsule remains radius 0.32 m, height 16 m, centred 8 m above the origin, outside both visual LODs. It remains a deliberately approximate blocking proxy rather than a canopy hull. StaticCollision construction, player contact layers and collision lifetime are unchanged. PineTree003 and fir assets are unchanged.

## Validation — awaiting Godot acceptance

- Parsed both glTF models; buffer lengths and indices checked.
- All new positions/normals are finite, triangle areas are nonzero, and normals agree with triangle winding.
- Inspected texture-mapped orthographic silhouette previews at 0°, 45°, 90° and 135°. These are software previews, not Godot screenshots or lighting validation.
- Near binary is the exact former LOD2 blob; textures and original import UIDs/settings are retained. Godot should regenerate import caches after pulling.

Godot/.NET are unavailable here. Pull, let Godot reimport, restart test.tscn, then use F1 forced LOD1/LOD2 from the same view and inspect the automatic 70 m transition. Check foliage fullness, alpha edges, branch-tier continuity, roots, shadow transitions and collision. Compare uncapped FPS after placement settles; no measured performance gain is claimed. Fewer triangles do not by themselves remove foliage overdraw or shadow costs.


## Three-stage authored LOD — integrated 2026-10-30

`PineTree002.tscn` now includes the supplied `Models/pine_tree_002_a_LOD3.gltf`, configured through the optional third slot in `AuthoredTreeLod`. Native ranges: **LOD1 0–70 m**, **LOD2 70–280 m**, **LOD3 280–600 m**, then cull. LOD3 is 120 triangles and casts no shadows. The prior 280 m final-cull description above is historical; no geometry, materials, collision, rotation, tree size, placement counts, forest settings or source files changed. F1 provides a second split slider and Force LOD3 for both original and forest pines. See `WORLD/Vegetation/LOD/README.md`; local import/visual performance verification is still required.
