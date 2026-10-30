# Pine Tree 002

Owner-authored pine, separate from the PolyHaven PineTree001. Models/ and Textures/ keep relative glTF references. LOD1: 23,644 triangles. LOD2: 6,972 triangles. Export height ~22.74 m including roots. Bark has colour/normal/roughness textures; needles use a double-sided cutout atlas with threshold 0.39. Source exports contain no root display translation.

The test scene places four pines alongside twelve firs through WORLD/Vegetation/TreePreview, using the reusable PineTree002.tscn. Its one-time AuthoredTreeLod setup selects the existing LOD1 at 0–70 m, LOD2 at 70–280 m, and culls beyond 280 m via native visibility ranges (±3 m hysteresis, no blended fade). Tune those exported distances in PineTree002.tscn. Both LODs stay loaded, share transforms and use the same union bounds for consistent distance centres. Source geometry and textures are unchanged. Primitive trunk collision is now enabled via the shared profile component; chopping, wind and persistence remain unimplemented. Existing import-generated mesh LODs are preserved. Appearance/performance require local acceptance.

Current LOD pass: source/reference/range checks only; C# compilation and Godot distance switching, silhouette/shadow continuity and FPS need local validation. Test walking past 70/280 m in both directions with F1 tree controls; distances are camera-to-AABB-centre, not ground/trunk distance. No per-frame distance script is used.

## Trunk collision enabled — awaiting local validation

Trees now opt into the shared PHYSICS/StaticCollision construction. PineTree002.tscn carries one StaticCollisionInstance outside both visual LODs; its shared capsule is radius 0.32 m, height 16 m, centred 8 m above the asset origin. The original twelve firs now use A/B/C wrapper scenes with one shared radius 0.14 m / height 3.5 m capsule. Their wrappers cancel the existing B/C display offsets before grounding. These are deliberately simple blocking proxies, not anatomically exact trunk geometry; review their fit locally.

All four original pines and the forest's PineTree002 instances inherit collision from the same asset prefab. No duplicate collider per LOD, full-canopy hull, per-tree ray or tick was added. Layer 4 / mask 0 preserves player contact and terrain-only placement queries. Shape resources are shared; bodies are per tree. Hiding/culling visuals does not disable collision; actual parent unload frees the body. Existing finite resident-world limitation remains.

Local: build, run test.tscn, enable Debug → Visible Collision Shapes, approach pine/fir trunks from several angles, walk through foliage gaps and inspect scaled/rotated instances. Check B/C roots align, F4 doesn't duplicate bodies, changing LOD keeps the same collision and boulders/grass still ground on terrain. Disable an asset profile with Mode None and restart to opt out. Source references/scene layout reviewed; compilation, contact fit and FPS remain unverified here.

## Active visual source

The stable PineTree002.tscn now uses this folder's Models/pine_tree_002_a_LOD1.gltf and LOD2.gltf with pine_needles_photo_detail_2k.png. Removed the _v4 suffix from both model/buffer filenames, imported node/mesh/scene names and import source/cache paths; import UIDs and binary/texture data are preserved. PineTree002_Fix remains untouched for the owner to remove later; the active prefab no longer depends on it. Native 70/280 m ranges and shared trunk collision remain in place.

Source metadata/references were checked; compilation, Godot reimport, appearance and collision fit await local validation.
