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
