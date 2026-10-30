# Fir Sapling Medium — 1K collection

Three user-exported Poly Haven variants: `fir_sapling_medium_a_1k.glb`, `fir_sapling_medium_b_1k.glb`, `fir_sapling_medium_c_1k.glb`. Ben selected the Blender LOD1 objects for all three exports. This is export provenance supplied by the owner, not a measured triangle-count inspection. 1K is the source package label; texture pixel dimensions were not verified here.

## Folder contents and import policy

The three GLBs retain their original binary contents. Their companion `.glb.import` files preserve stable scene UIDs and existing mesh settings, including automatic mesh LOD generation and shadow meshes. Keep these small import recipes in source control; Godot's derived caches belong under ignored `.godot/`.

Embedded image handling is **Embed as Basis Universal** (`gltf/embedded_image_handling=2`), so textures remain compressed inside the imported scenes rather than generating per-model PNGs beside the GLBs. A/B/C still each carry their own embedded images; this is a tidy self-contained workflow, not shared-material or shared-VRAM optimization.

The previous `fir_sapling_medium_1k.blend/` directory was a folder, not a Blender file. The cleanup removed its original loose texture pack and the six Godot-extracted maps per variant, plus their import sidecars. A/B/C extracted copies had identical Git blob hashes for each corresponding map. No external scene/script/material references to the old asset folder were found in the project's non-vendor text resources. Textures needed by these imports are supplied by the GLBs' embedded images; extraction was the previous importer policy.

## Local validation

Close Godot before pulling the rename/deletions, then reopen and let the three models reimport. Inspect each GLB: bark, needles, alpha cutouts, normal shading, scale and silhouette. Confirm PNGs no longer appear next to the models. Reimport may take longer initially because Basis encoding compresses the embedded images. Source/reference/import-setting checks were performed through GitHub; GLB binary parsing, Godot reimport, texture dimensions, compression quality and rendered appearance were not verified in this cleanup environment.

For future exports keep only the selected LOD1 object for the desired A/B/C variant. A/B/C are shape variants, not the three LOD levels. Do not delete LOD1 geometry in an attempt to remove duplicate textures. Leave geometry unchanged until triangle counts and the first-person quality/performance tradeoff are evaluated.

## Later optimization

For a large forest, consider one shared external texture/material set across all three variants, then instancing, distance LOD and foliage/shadow budgets. This takes a deliberate material/import setup or a re-export with shared texture references; merely renaming PNGs does not make embedded GLBs share GPU resources. Keep source Blender/download textures outside the game project if needed for re-export. Runtime optimization and placing trees in the test world are separate tasks.

## Trunk collision enabled — awaiting local validation

Trees now opt into the shared PHYSICS/StaticCollision construction. PineTree002.tscn carries one StaticCollisionInstance outside both visual LODs; its shared capsule is radius 0.32 m, height 16 m, centred 8 m above the asset origin. The original twelve firs now use A/B/C wrapper scenes with one shared radius 0.14 m / height 3.5 m capsule. Their wrappers cancel the existing B/C display offsets before grounding. These are deliberately simple blocking proxies, not anatomically exact trunk geometry; review their fit locally.

All four original pines and the forest's PineTree002 instances inherit collision from the same asset prefab. No duplicate collider per LOD, full-canopy hull, per-tree ray or tick was added. Layer 4 / mask 0 preserves player contact and terrain-only placement queries. Shape resources are shared; bodies are per tree. Hiding/culling visuals does not disable collision; actual parent unload frees the body. Existing finite resident-world limitation remains.

Local: build, run test.tscn, enable Debug → Visible Collision Shapes, approach pine/fir trunks from several angles, walk through foliage gaps and inspect scaled/rotated instances. Check B/C roots align, F4 doesn't duplicate bodies, changing LOD keeps the same collision and boulders/grass still ground on terrain. Disable an asset profile with Mode None and restart to opt out. Source references/scene layout reviewed; compilation, contact fit and FPS remain unverified here.
