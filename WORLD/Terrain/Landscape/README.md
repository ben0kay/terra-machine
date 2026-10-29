# Authored test landscape

First landscape pass: implemented, awaiting local visual/collision/performance acceptance. Run `WORLD/Scenes/test.tscn`. From spawn, follow the soil route forward and to the right; it curves uphill and then left onto the plateau. The sharp plateau edge is intentionally separate from the walking route. The large ridge is farther behind it.

`Configuration/LandscapeMaster.tres` controls plateau centre/radii/height, cliff width, distant mountain height, fine variation and the route control points (X/Y/Z metres). Route Y specifies walking elevation. Defaults describe a broad starting meadow, a ~34 m plateau and a ~97 m backdrop ridge. The central route is ~7 m wide, with blended shoulders; it is not a navigation mesh or a gameplay restriction. The path's centreline is smooth, with a calculated maximum grade ~26% (~15 degrees). Keep control points spaced generously and review new grades when editing.

`TestLandscape.cs` evaluates deliberate broad shapes and small analytic surface variation, then bakes a 1024-square floating-point height image and a matching soil mask once at startup. Samples are one metre apart, indexed by wrapped world X/Z. Heights are encoded over -32..192 m, matching the integration generator resource. No callbacks run after preparation. The integration feeds the image into native SDF generation; terrain remains volumetric voxels, not a separate heightmap mesh. Later excavation can change those voxels. Natural overhangs, caves, excavation, saving and general procedural world generation are outside this pass. The soil mask describes the original surface, so future excavation will need to account for that.

Native generator types/calls remain in `../Integration/`. Each terrain instance duplicates its native generator and material. No player, vegetation or gameplay API changes are needed. Existing tree-preview placement, streaming, collision, F4 respawn and F1 render comparisons remain in place.

## Surface material

`../Configuration/GroundMaterial.tres` keeps the existing three texture sets. The shader uses fixed world-space projection directions, continuous UV distortion, larger grass patches, macro colour variation and the route mask. Soil follows the same baked curve that shapes the walking bench. This adds one mask sample per pixel; the original 27 texture samples remain when normal mapping is enabled. No new texture sets or per-frame material updates were added. This aims to reduce repetition and projection flips; it does not establish that every previously observed cutoff was caused by UV flipping.

## Validation and handoff

- All project C# compiled with Roslyn against the available Godot 4.4.1 API; not a full Godot.NET.Sdk 4.7.2/source-generator build.
- Managed sampling: 49 route samples, max grade 26.1%, centreline elevation error <=0.025 m; 2 m grid heights -4.65..97.09 m, within encoded/native bounds. Full 1024-square preparation measured ~1.6 seconds here, not a target-machine performance guarantee.
- Godot 4.7.2 with the repo's native Linux extension loaded `VoxelGeneratorImage`, accepted the floating-point image, generated a block and confirmed negative SDF below spawn / positive above. Shader uniforms and material loaded successfully headless.
- Local checks: build in Godot .NET, run test scene, follow the entire route without jumping, inspect plateau edge/shoulders and LOD transitions from both sides, check trees settle, F4 respawn, then F1 five-case comparison. Headless checks do not verify appearance, GPU shader execution or collision along the full route.

Startup preparation temporarily allocates height/mask arrays and images, then stops; no new idle/frame polling or managed per-chunk generation. The persistent CPU image data/mask are a few MB per terrain instance plus the native generator's image copy. First-load pause and native streaming cost should be checked locally.
