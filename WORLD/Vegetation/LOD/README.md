# Authored tree distance LOD

`AuthoredTreeLod.cs` is a reusable one-time setup node for a near and far imported tree scene. PineTree002.tscn is the first consumer. It instantiates both PackedScenes, collects mesh handles once, gives them a shared union bound, and assigns native GeometryInstance3D visibility ranges.

- LOD1: 0–70 m by default.
- LOD2: 70–280 m.
- Beyond 280 m: neither model draws.
- 3 m hysteresis prevents repeated boundary switching. Fades are disabled to retain cutout/opaque rendering; a visible silhouette pop is possible.
- Distances use the renderer's world AABB centre, including vertical camera separation. Equal bounds synchronize the near/far reference; margins shift the effective thresholds on approach/departure.
- Both models share the parent's transforms. Source origins must align. Settings are applied at creation; restart/reinstantiate after changing them.

No _Process/_PhysicsProcess implementation, distance timer, player lookup or per-tree material updates. Native renderer culling owns ongoing selection. Both scenes/meshes remain in memory; this is not object streaming, collision LOD, impostors or an automatic triangle reducer. Imported automatic mesh LOD remains independent. Shadows follow range selection; no distant shadow proxy is added.

The parent TreePreview owns grounding and cached debug material/shadow changes. Pine settings live in the reusable asset scene, with no addon edits or Voxel Tools dependency.

Validation: source/API/range review only. Godot/.NET build and local visual/performance acceptance are pending. See ../TreePreview/README.md for the walk-away test.

Reference: https://docs.godotengine.org/en/stable/tutorials/3d/visibility_ranges.html
