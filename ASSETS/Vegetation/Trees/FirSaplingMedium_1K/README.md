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
