# Reusable static collision

## Profiles and explicit adoption

`StaticCollisionProfile` is a shared, inspector-editable Resource. It contains:
- **None (0):** no body/shape created; default for new profiles.
- **AuthoredShape (1):** assign an existing BoxShape3D, CapsuleShape3D, CylinderShape3D, SphereShape3D or prebuilt convex/concave Shape3D.
- **ConvexMesh (2):** create a single convex hull from a caller-supplied mesh; optional simplification, off by default.
- Local ShapeTransform, collision layer and mask. Defaults use existing static-world layer 3 (bitmask 4), mask 0.

Keep an asset's profile beside that asset or in its Configuration folder; rendering/vegetation categories do not own this mechanic. Collision is explicitly adopted by the asset/placement owner, not automatically inferred from every mesh.

## Caller contract

1. Call `StaticCollisionBuilder.Prepare(profile, optionalMesh)` **once per source mesh/profile** at initialization. Hold the returned Shape3D.
2. For each fixed instance call `Attach(parent, profile, preparedShape, sourceTransform)`. sourceTransform is the source mesh frame relative to parent (identity for an asset-origin primitive). The helper creates a native StaticBody3D and one direct-child CollisionShape3D referencing the prepared shared shape.
3. Apply the object's stable parent translation/yaw/uniform scale as usual. Free the parent on actual unload; descendants follow. Keep the profile/shape immutable while shared and restart/rebuild after changes.

No automatic global cache: lifetime belongs to the asset/placement owner. Do not call Prepare separately for every identical object. No frame callback, physics callback, timer, scene scan or distance check exists in this helper.

For a trunk, create an AuthoredShape profile with a capsule/cylinder sized to the **solid trunk**, not branches/needles. Put its centre halfway up the blocking trunk using ShapeTransform. For a crate use a box. For an irregular boulder use one modest convex hull or prebuilt proxy. Never copy the whole pine's visual mesh into a convex trunk shape: that would block the canopy's empty space.

## First consumer: existing boulders

BoulderMaster.tres references Boulder001/BoulderCollision.tres. It explicitly chooses ConvexMesh, layer 4, mask 0, and no simplification; the same hull-generation arguments and body alignment are retained. One hull is cooked per BoulderPreview initialization and shared among up to 14 native bodies. Existing EnableCollision false still avoids both hull cooking and attachment. Trees and grass remain unchanged.

Terrain remains layer 1; the player uses mask 5 (terrain 1 + static scenery 4). Terrain-grounding rays still use mask 1. The helper does not change terrain/player filtering. Visibility ranges and F1 visual switches do **not** unload colliders.

## Performance guidance

One shared shape saves cooking and shape-resource memory, but each placed body still has a physics proxy. Static bodies have no scripted simulation, while the physics engine still maintains spatial data and performs relevant contact/query tests. A single simple primitive is often the cheapest adequate representation; a convex hull is a useful compromise for irregular rocks. Avoid full visual trimeshes, excessive hull vertices, unnecessary shape counts or foliage collision when a simpler proxy suffices. An authored/prebuilt hull avoids cooking on load; the current boulder mode intentionally preserves prior behaviour rather than asserting it is globally optimal.

This finite test keeps placed bodies resident. Native renderer LOD/culling is separate from collision streaming. A large world will need owner/chunk-driven collider activation/unload near relevant actors; no polling manager or distance-based disabling was added here. Do not create one giant body containing an entire forest's shapes, or attach a static collider under a moving/animated actor. Nonuniform/mirrored scaling and future dynamic physics need separate validation/design.

## Validation and review

Reviewed meadow (batched MultiMeshes, shader wind, bounded placement), forest (finite 115-tree target, native LOD, pending nearby placement), and boulder commits. Their static-collision construction was rock-local; extracted here without adding collision to other assets. Meadow currently uses global per-variant batches rather than small culling tiles, and all placed forest/rock instances remain resident. Those are bounded-test limitations, not a complete large-world solution.

Source-level profile/reference/loop/API review and old/new collider transform equivalence checked. **Compilation, Godot runtime contacts and measured performance remain unverified** here (no Godot/.NET executable). Local checks: build; inspect visible collision shapes at different rock scales/yaws; walk/jump around rocks; confirm no grounding on props; test both disabling paths, visual-only hide, F4 and scene reload; verify shared Shape resource identity. Future primitive-profile adoption needs its own scale/contact check. No measured speedup is claimed by this refactor.

Reference: https://docs.godotengine.org/en/stable/tutorials/physics/collision_shapes_3d.html
