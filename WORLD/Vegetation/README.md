# Vegetation

`TreePreview/` owns the bounded twelve-fir/four-pine visual group. `GrassScatter/` owns the player-centred, distance-faded grass trial, batched into nearby tiles. Mesh/texture assets remain under ASSETS/Vegetation; world placement/settings belong here. Both use game-owned terrain interfaces with no Voxel Tools dependencies. Trees sleep after placement; grass runs bounded placement on travel and retains a small crossing timer. See each feature README for lifecycle, tuning and acceptance checks.

`MeadowPreview/` adds an optional, bounded tufted-grass comparison around the existing tree offsets (three imported LOD1 clump variants, shared GPU wind material). It uses at most three MultiMesh batches after placement, sleeps when finished and is separately switchable in F1 → OTHER MODELS. It does not change the player-centred Bermuda scatter; see MeadowPreview/README.md.

`BoulderPreview/` scatters a few seeded, collision-enabled mossy boulders around the flat-side forest, using Boulder001 and the forest's tree/trail exclusion. See BoulderPreview/README.md.

`ForestPreview/` adds a separate, seeded pine-forest test area with sparse edges and a denser interior. It reuses PineTree002's authored distance LOD and uses a bounded collision-query budget as the player approaches; only a single pending-placement timer runs while incomplete. F1 can hide this forest independently. This is deliberately not a streamed, batched large-world forest; see ForestPreview/README.md.

The test scene keeps all visual natural scenery under `NatureRoot/{Trees,Grass,Rocks}`. This is a scene-tree organization only, following the grouped-root idea from War Machine; sources and their ownership remain in their original feature folders.

`ForestMeadowScatter` (under `NatureRoot/Grass`) reuses the same bounded GrassScatter tile system, but with the three MeadowGrass001 near/far authored models, wind and a density profile tied to the flat-side `ForestPreview` ellipse. The original 16-tree meadow preview stays separate; Bermuda grass is slightly less dense. Both can be switched independently using F1.

The forest's interior now has a mildly tighter mature-pine spacing plus a small, seeded A/B/C fir understory using the existing `ForestPreview` placement budget and shared trunk-collision prefabs. Tune counts, spacing, scales, shadows and distance in `ForestPreview/Configuration/ForestMaster.tres`; no new scene root or update loop is introduced.

`SpawnMeadow002` adds a compact GrassMeadow002 A/B/C wind-swaying test in the original 12 near-spawn fir tree locations. It reuses MeadowPreview's finite MultiMesh placement with a configurable fir-site limit and its own F1 toggle/configuration; no changes to the forest or Bermuda grass.
