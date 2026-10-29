# Authored test landscape

Second landscape pass: implemented, awaiting local visual/collision/performance acceptance. Run `WORLD/Scenes/test.tscn`. Head forward/right onto the wide eastern mountain shoulder and climb toward the plateau. There is no flattened winding track or painted soil ribbon. Several walking lines can cross this broad approach; the western plateau edge remains steep.

## Authoring

`Configuration/LandscapeMaster.tres` controls plateau centre/radii/height, cliff width, distant mountain height, fine variation and the broad approach centre/radii/height. The approach uses overlapping broad landforms and a smooth maximum into the plateau. Meadow transitions extend over ~90 m to avoid creating a steep rim. Default sampled heights are ~0..101 m. Dimensions are metres. No navigation mesh or walking restriction is created.

`TestLandscape.cs` prepares a 1024-square floating-point height image and a matching slope map once at startup. Samples are one metre apart, indexed by wrapped world X/Z. Heights are encoded over -32..192 m, matching the integration generator resource. The slope map contains the upward normal component calculated with central differences of that same surface. Native generation consumes the height image as SDF voxels; this is not a replacement heightmap mesh. No managed callbacks run afterward.

Native types/calls remain in `../Integration/`. Each terrain instance duplicates its generator and material. Player, vegetation, streaming, collision and debug contracts remain unchanged. Later excavation can change the voxels, but its material rendering must also update or replace the original-surface slope map. Caves, overhangs, excavation, saving and general procedural world generation are outside this pass.

## Surface material and square-patch investigation

`../Configuration/GroundMaterial.tres` retains the existing texture sets. Gentle ground uses top-down projection; steeper areas blend toward triplanar projection. Rock thresholds use the shared slope map rather than normals from differently tessellated LOD meshes. Grass patches and UV distortion use an integer hash instead of a floating-point sine hash. These address possible material discontinuities without changing streaming/LOD distances. The screenshot's exact square-patch cause is **not confirmed**, and visual acceptance is still required.

The former route-mask sample is replaced with one slope-map sample per pixel. Full normal-mapped terrain still uses the original 27 texture reads plus that map. There are no per-frame material writes. Fixed world projection directions, continuous UV distortion and macro colour variation remain from the first pass.

## Validation and local handoff

- All project C# compiled with Roslyn against the available Godot 4.4.1 API; not a full Godot.NET.Sdk 4.7.2/source-generator build.
- Full 1024-square managed sampling: finite heights -0.11..101.38 m, within encoded/native bounds. A representative ascent through (0,0), (35,-50), (60,-90), (15,-130), (-42,-150) has a maximum sampled surface incline ~32.3 degrees. These are checks of the authored surface, not collision traversal or proof every ascent is walkable.
- Full height/slope preparation measured ~0.5 seconds in this environment; not a target-machine guarantee.
- Godot 4.7.2: shader uniforms and material loaded headless; native image/SDF check confirmed solid below spawn and air above.
- Local: build in Godot .NET, walk the broad eastern shoulder, inspect steep western cliffs, move across chunk/LOD borders and compare the same screenshot location. Check F4 respawn and F1 render comparisons. If squares remain, compare simple ground, textured ground with normals off, and full material at the same camera; this distinguishes geometry/lighting from material issues. Headless checks do not establish visual correctness or GPU performance.

Preparation allocates temporary arrays/images and stops. Persistent map/image data are a few MB per terrain instance plus the native image copy. No new idle polling or managed per-chunk generation is introduced.
