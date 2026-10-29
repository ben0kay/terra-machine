# Pine Tree 002

Owner-authored pine, separate from the PolyHaven PineTree001. Models/ and Textures/ keep relative glTF references. LOD1: 23,644 triangles. LOD2: 6,972 triangles. Export height ~22.74 m including roots. Bark has colour/normal/roughness textures; needles use a double-sided cutout atlas with threshold 0.39. Source exports contain no root display translation.

The test scene places four pines alongside twelve firs through WORLD/Vegetation/TreePreview, using the reusable PineTree002.tscn. Its one-time AuthoredTreeLod setup selects the existing LOD1 at 0–70 m, LOD2 at 70–280 m, and culls beyond 280 m via native visibility ranges (±3 m hysteresis, no blended fade). Tune those exported distances in PineTree002.tscn. Both LODs stay loaded, share transforms and use the same union bounds for consistent distance centres. Source geometry and textures are unchanged. Trunk collision, chopping, wind and persistence are not implemented. Existing import-generated mesh LODs are preserved. Appearance/performance require local acceptance.

Current LOD pass: source/reference/range checks only; C# compilation and Godot distance switching, silhouette/shadow continuity and FPS need local validation. Test walking past 70/280 m in both directions with F1 tree controls; distances are camera-to-AABB-centre, not ground/trunk distance. No per-frame distance script is used.
