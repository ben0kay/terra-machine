# Flat-side forest — GrassMeadow002 replacement (awaiting local validation)

`ForestMeadowMaster.tres` retains the existing `NatureRoot/Grass/ForestMeadowScatter` node and F1 toggle, but now selects **GrassMeadow002** instead of the much higher-poly **GrassMedium001**. It uses the already imported A/B/C LOD1 and LOD2 glTFs, paired in order using the existing GrassScatter near/far loader. Three variants × two LODs replace the previous nine variants × three LODs. GrassMedium001 source assets are retained for comparison/reuse and no longer referenced by the active forest-scatter resource.

## Visual settings

- `MinimumScale=1.25`, `MaximumScale=1.65`: 25–65% enlargement with individual deterministic random yaw and uniform scale. Native A/B/C source heights are ~0.75–0.9 m, so placed clumps are roughly 0.94–1.49 m tall (before ground alignment), much taller than the original spawn-only Meadow002 test's 0.88–1.13 scales.
- `TuftsPerSquareMeter=1.4` candidates before patch/slope rejection (was 2.4); `MinimumPatchCoverage=0.92` inside patches.
- Existing world-anchored scattered bunches stay at `PatchSpacing=15` m and `PatchRadius=5.8` m (jittered radii vary by patch); the original seed `9876` and forest ellipse stay unchanged. The terrain's matching grassy-ground/leaf-litter mask still follows these patch settings through `ITerrainForestFloor`.
- 8 m tiles; 16 m full-density range, thinning to 20% by 44 m, cutout wind-fade to 68 m and native tile renderer cutoff beyond it.
- LOD1 applies at near distance; LOD2 starts at approximately 24 m (native tile AABB distance), with shared transforms and bounds. Original model assets, texture and imported automatic LOD settings remain untouched.
- `Materials/ForestMeadow002Wind.tres` is a separate forest-only material using MeadowGrass002's original wind shader plus opt-in distance fade. The spawn-area `MeadowGrass002Wind.tres` remains fade-disabled by default and its appearance/settings are unchanged.

## Performance notes

The previous selected GrassMedium001 near meshes cost **290–6,422 triangles each**. GrassMeadow002 LOD1 has **54–66 triangles** per tuft, LOD2 **24–30**. This should dramatically reduce submitted grass geometry, with fewer scene variants, fewer MultiMesh draw batches per tile, and lower candidate density. The resulting FPS change is unmeasured; mature pine shadows, high alpha-cutout pixel overdraw and terrain rendering may still dominate frame time. Grass remains GPU-wind animated, collision-free, ray-bounded while placing, and timer-limited to 4 Hz after placement; no per-tuft node/update has been added.

## Acceptance

Pull main, build Godot .NET and run `WORLD/Scenes/test.tscn`. Enter the flat-side (+Z) pine forest. Confirm the taller meadow bunches, scale variation, ground alignment, wind motion, texture alpha edges, patch transition and near/far LOD switch at ~24 m. Check that grassy ground aligns with the new patches and that the original Meadow Grass 2 around spawn remains unchanged. F1 → OTHER MODELS → `Visible: ForestMeadowScatter` isolates the replacement; F2 shows frame times. Compare with previous fixed-camera baseline if available, and inspect visually for grass being too tall. No engine compile or local FPS validation was possible in this GitHub-editing environment.


## Lighter meadow patches — October 2026 visual adjustment (pending in-game review)

After examining the forest screenshot, the GrassMeadow002 patches were too tall and had several clumps rooted almost at the same location. The *forest-only* resource now uses:
- `TuftsPerSquareMeter = 0.75` rather than 1.4: about **46% fewer candidate positions** before the existing patch/slope/spacing filters, not a guaranteed 46% final-instance reduction.
- `MinimumTuftSpacing = 1.4` metres: once an instance is placed, any later candidate with a root inside this radius in the **same 8 m tile** is rejected without a terrain ray. This prevents the most obvious five-clumps-in-one-spot look. Neighbouring tile boundaries can still overlap slightly; this remains a bounded, tile-local test rather than an expensive global placement pass.
- `MinimumScale = 0.75`, `MaximumScale = 0.95` for 85% of accepted clumps, keeping them substantially shorter.
- `TallTuftChance = 0.15`, `TallMinimumScale = 1.3`, `TallMaximumScale = 1.65` for occasional full-height accents. The weighted average scale is about **0.944**, vs the previous uniform average **1.45** — about **35% shorter on average**, though visual height depends on the variant.
- Same A/B/C models, GPU wind, near/far LODs, 15 m patch spacing, 5.8 m patch radii, density fade distances and grassy-ground patch mask.

`GrassScatter` exposes the three optional scale and spacing controls via `GrassScatterSettings`; the defaults (`TallTuftChance=0`, `MinimumTuftSpacing=0`) retain the original Bermuda scatter behaviour. `SpawnMeadow002` uses `MeadowPreview` with its own settings and is unchanged. The new list of accepted positions is populated only when minimum spacing is enabled and lives per active tile; no additional per-tuft scripts, collision or permanent update loop.

Test: Pull main, restart the test scene for regenerated positions, enter the flat-side forest (+Z), and compare the same patch with the previous screenshot. Check that leaf litter and short Bermuda grass can be seen between clumps, that a few taller stalks still break up the silhouette, and that occasional tile-boundary overlaps are acceptable. F1 → OTHER MODELS → ForestMeadowScatter and F2 allow same-view performance comparison. Godot import, C# compilation and visual results still require local testing.


## Widely distributed light clumps — follow-up tuning

The forest Meadow Grass 2 now has **more small patches distributed across the forest**, rather than fewer large dense mats. `PatchSpacing` changes from 15 to **12 m**, introducing more potential patch centres; `PatchRadius` drops from 5.8 to **4.2 m** so each one covers less ground. The terrain's existing `ITerrainForestFloor` patch-mask handshake reads these same settings, keeping grassy ground and leaf litter aligned with the altered patch footprints.

Within each patch, `TuftsPerSquareMeter` decreases from 0.75 to **0.65** candidates/m², `MinimumPatchCoverage` decreases from 0.92 to **0.72** (the underlying acceptance formula still reaches 0.98 in some locations), and tile-local `MinimumTuftSpacing` increases from 1.4 to **1.9 m**. This should leave visibly more ground between roots and improve the distribution of individual plants. The minimum spacing remains tile-local, so some cross-tile overlap is still possible.

The **85% short** (scale 0.75–0.95) and **15% tall** (1.30–1.65) height mix, grass models, near/far LODs, forest footprint, GPU wind, seed and render-distance settings are unchanged. The spawn-only Meadow002 and Bermuda configurations are also unchanged.

Restart `test.tscn` after pulling so all relevant 8 m scatter tiles regenerate. Inspect both local separation and how evenly small patches cover the forest; if their footprints still appear too circular, later consider changing the patch-mask shape separately. This was a configuration-only pass; engine rendering and performance were not tested here.
