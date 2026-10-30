# PineTree003 — taller bare-trunk variant

User-pushed tall pine: about 28.84 m tall, foliage beginning around 10 m. LOD1 has 23,002 triangles; LOD2 6,780. Both have identity node transforms, matching asset origin and external shared bark/needle maps in Textures/. Needles use two-sided MASK at cutoff 0.39. Artwork provenance/licensing remains as supplied by the owner; this integration did not generate or edit its textures/meshes.

Use PineTree003.tscn for placement. It reuses AuthoredTreeLod's native 0–70 / 70–280 m ranges with 3 m hysteresis, with one static capsule outside the visual LODs. TrunkCollision.tres defines an initial radius 0.34 m, height 21 m proxy centred at 10.5 m; tune fit locally. Shared static-world layer 4 / mask 0 preserves terrain-only placement rays.

ForestMaster mixes eight into existing near-entrance sites without increasing the 115-tree target. Scene/physics setup is one-time; no tree distance tick. Bodies stay resident with the current finite test, even when visuals are hidden.

Metadata/resource/loop checks passed. Build/import, alpha/scale/pivot/LOD appearance, trunk fit and frame-time checks remain local. No measured performance claim.

## Stable filenames restored

The updated exports now use the original names pine_tree_003_a_LOD1.gltf/.bin and pine_tree_003_a_LOD2.gltf/.bin, without a revision suffix. The updated dark needle texture is named pine_needles_atlas_1k.png again. Both glTF buffer/image URIs and renamed import metadata are updated; import UIDs and settings are preserved. Mesh/image bytes were moved unchanged. The existing PineTree003.tscn references now resolve again; LOD/collision/forest placement are unchanged.

Checked buffer lengths, relative image references, scene paths and metadata source paths. Close Godot, pull and reopen to regenerate renamed import caches. Godot reimport/visual verification remains local.
