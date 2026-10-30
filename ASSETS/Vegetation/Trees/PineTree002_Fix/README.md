# PineTree002 dark-foliage fix

User-pushed DeepFir replacement for PineTree002. LOD1/LOD2 are 23,644/6,972 triangles and about 22.74 m tall, with matching geometry bounds/origins to the prior model. Both reference the shared Textures folder, including pine_needles_dark_fir_atlas_1k.png. Needles use MASK cutoff 0.39 and two-sided cards.

The stable ../PineTree002/PineTree002.tscn now references these exports, so all existing test pines use the fix while retaining LOD and trunk collision. No new asset generation or source texture edits were performed in this integration. Provenance/licensing of the pushed source artwork remains as supplied by the owner.

Metadata/references inspected; Godot build/import/render/contact/performance checks pending locally.
