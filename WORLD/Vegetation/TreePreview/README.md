# Current test-world tree selection

The twelve original fir sapling sites (indices 0–11) are disabled through TreePreviewMaster.DisabledIndices. Only the four existing PineTree002 sites instantiate. Disabled sites do not query terrain or create models/colliders. The shared Offsets array stays intact because meadow placement and forest/prop reservations also consume it; this preserves grass placement and each pine's original index, yaw and scale. The fir assets remain available for future reuse. Source checks completed; restart the test scene after pulling and confirm Trees: 4, with no firs near spawn. Earlier sections describe previous configurations.

# Fir and pine visual preview

`TreePreview.tscn` is a reusable, small authored group for `WORLD/Scenes/test.tscn`. `Configuration/TreePreviewMaster.tres` selects twelve fir A/B/C instances plus four PineTree002 distance-LOD scene instances, sixteen fixed XZ offsets relative to spawn, scale range, ground sink, alpha cutoff and loading budget. The default group is 11–55m in front of spawn (negative Z); the path down its centre remains open.

`TestWorld` explicitly calls Initialize after its first successful grounded spawn. The helper uses only ITerrainService. Up to four ground rays per 0.2s retry interval run during initial chunk loading; placement stops completely after all trees are placed or after 40s. F4 does not duplicate existing trees. Remaining unloaded candidates are skipped with one warning. This is a bounded visual preview, not a streamed forest population system or persistent placement.

Original imported mesh pivots include Blender display offsets for B (+5m X) and C (+10m X). The wrapper cancels the first mesh origin without altering GLBs. Deterministic yaw and modest scale variation reuse the actual models. Only source material clones are modified: alpha-blended fir needles become Alpha Scissor at 0.35 cutoff, with anisotropic mip filtering. Already-cutout materials retain their imported threshold (PineTree002: 0.39). Clones are shared across instances of each source material; no per-tree material copies or ticks. PineTree002 has a one-time LOD setup script; it has no active process/physics callbacks. Imported geometry/automatic mesh LOD settings remain unchanged.

These trees now have shared primitive trunk collision (see current pass below), but no chopping, wind or terrain-edit support. They will not re-ground after future excavation. GroundSink accommodates the small root extension below Blender's zero plane. Check root contact, scale and leaf cutouts locally. Disable/remove this scene to compare terrain alone; removing it is supported by TestWorld's optional reference.

GLB inspection confirmed one LOD1 mesh node per variant and six embedded images per file. This corrects the previous cleanup README's unverified-binary limitation for this subsequent job. Normal/roughness/color textures in the GLBs remain embedded and untouched. The inherited source-pack "1K" label is not used to assert all tree image dimensions.

Optional render debugging: SetDebugRendering controls root visibility, cached mesh shadow flags and shared cloned-material unshaded mode. Later-placed meshes honor the active options. These switches do not alter placement simulation. See ../../../DEBUG/Overlay/README.md.

## PineTree002 trial

The existing twelve firs are retained. Four pines use offsets (-19,-12), (22,-17), (-32,-39), (36,-48), leaving the centre open. Variants is an ordered list matching Offsets; repeated scene references reuse imported resources. The pines use the same grounding, scale/yaw variation, timeout and dormant placement helper. No PineTree001 asset is referenced. The F1 visibility/shadows/unshaded controls and comparison now cover all sixteen trees; HUD reports Trees rather than Firs.

PineTree002 export inspection: LOD1 23,644 triangles; LOD2 6,972; original height ~22.74 m including roots, no display translation. Texture paths resolve; needles use MASK 0.39 and double-sided cards. The trial now references the reusable PineTree002.tscn scene, which configures authored LOD1/LOD2 switching once at load. Native visibility ranges choose LOD1 to 70 m, LOD2 to 280 m, then cull; 3 m hysteresis surrounds boundaries. Both LODs receive a shared union bounding box so their distance centres agree. Distances are camera-to-world-AABB-centre, not horizontal distance to the trunk. Both source scenes stay loaded; this is rendering LOD, not asset streaming. Godot's normal import-generated mesh LODs remain available. Earlier LOD-only pass added no collision; the current trunk-collision pass below now enables it. Chopping and wind remain unimplemented.

Validation: C# API compilation and Godot 4.7.2 isolated import/resource checks completed. Full Godot .NET build, in-world placement/appearance, canopy alpha edges and F1 performance acceptance remain local checks. Existing source models/textures are untouched. Startup placement is bounded at four rays per 0.2 seconds and stops after sixteen placements or timeout; no new per-frame tree work.

## Authored pine LOD pass — awaiting local validation

The four existing pines now use `ASSETS/Vegetation/Trees/PineTree002/PineTree002.tscn`. Tune switch/cull/margin there; both imported glTFs and their textures are unchanged. The twelve firs retain their existing behaviour. TreePreview grounds/scales/rotates the complete pine wrapper and applies material/shadow/unshaded debugging recursively to **both** LOD models, preserving active F1 options.

Source checks: confirmed matching glTF node origins/material slots, counts, resource paths, native visibility API names and new loop syntax; shared-bound and range logic reviewed. Compilation, scene import and real renderer switching are **not verified here** (no Godot/.NET executable).

Local: build the project and run test.tscn. Inspect the Remote tree: each placed pine contains LOD1 and LOD2 meshes with 0–70 and 70–280 ranges. Walk away and back through ~70 m, linger near that boundary, and move beyond ~280 m; account for camera-to-AABB-centre distance and ±3 m hysteresis. Check no gap, double tree, pivot/scale jump, unexpected shadow or needle-material change. Test F1 trees/shadows/unshaded and the comparison, then F4. Both LOD buffers remain resident, but only the range-selected model should draw. No measured FPS gain is claimed.

## Trunk collision enabled — awaiting local validation

Trees now opt into the shared PHYSICS/StaticCollision construction. PineTree002.tscn carries one StaticCollisionInstance outside both visual LODs; its shared capsule is radius 0.32 m, height 16 m, centred 8 m above the asset origin. The original twelve firs now use A/B/C wrapper scenes with one shared radius 0.14 m / height 3.5 m capsule. Their wrappers cancel the existing B/C display offsets before grounding. These are deliberately simple blocking proxies, not anatomically exact trunk geometry; review their fit locally.

All four original pines and the forest's PineTree002 instances inherit collision from the same asset prefab. No duplicate collider per LOD, full-canopy hull, per-tree ray or tick was added. Layer 4 / mask 0 preserves player contact and terrain-only placement queries. Shape resources are shared; bodies are per tree. Hiding/culling visuals does not disable collision; actual parent unload frees the body. Existing finite resident-world limitation remains.

Local: build, run test.tscn, enable Debug → Visible Collision Shapes, approach pine/fir trunks from several angles, walk through foliage gaps and inspect scaled/rotated instances. Check B/C roots align, F4 doesn't duplicate bodies, changing LOD keeps the same collision and boulders/grass still ground on terrain. Disable an asset profile with Mode None and restart to opt out. Source references/scene layout reviewed; compilation, contact fit and FPS remain unverified here.
